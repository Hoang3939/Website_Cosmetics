#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
PSGAN Flask API server for Website_Cosmetics integration.

Environment variables:
- PSGAN_ROOT: absolute path to PSGAN source root (contains psgan/, configs/, assets/)
- PSGAN_MODEL_PATH: optional absolute path to G.pth
- PSGAN_DEVICE: cpu | cuda | cuda:0 ... (default: cpu)
- PSGAN_HOST: default 0.0.0.0
- PSGAN_PORT: default 5000
"""

from __future__ import annotations

import io
import os
import sys
import traceback
from pathlib import Path
from typing import Optional, Tuple

import numpy as np
import torch
from flask import Flask, jsonify, request, send_file
from flask_cors import CORS
from PIL import Image


PSGAN_ROOT = Path(os.getenv("PSGAN_ROOT", "")).resolve() if os.getenv("PSGAN_ROOT") else None
if PSGAN_ROOT and PSGAN_ROOT.exists():
    sys.path.insert(0, str(PSGAN_ROOT))

try:
    from psgan import Inference, PostProcess, get_config
except Exception as import_error:
    Inference = None
    PostProcess = None
    get_config = None
    _import_error_msg = str(import_error)
else:
    _import_error_msg = ""


app = Flask(__name__)
CORS(app)

inference_model: Optional[Inference] = None
postprocess_model: Optional[PostProcess] = None
config = None
device = os.getenv("PSGAN_DEVICE", "cpu")


def resolve_model_path() -> Path:
    env_model = os.getenv("PSGAN_MODEL_PATH", "").strip()
    if env_model:
        return Path(env_model).resolve()

    if PSGAN_ROOT:
        return PSGAN_ROOT / "assets" / "models" / "G.pth"

    return Path("assets/models/G.pth").resolve()


def resolve_config_path() -> Path:
    if PSGAN_ROOT:
        return PSGAN_ROOT / "configs" / "base.yaml"
    return Path("configs/base.yaml").resolve()


def initialize_models() -> None:
    global inference_model, postprocess_model, config, device

    if Inference is None or PostProcess is None or get_config is None:
        raise RuntimeError(
            "Failed to import PSGAN modules. "
            f"Set PSGAN_ROOT correctly. Import error: {_import_error_msg}"
        )

    if device.startswith("cuda") and not torch.cuda.is_available():
        device = "cpu"

    config = get_config()
    config_file = resolve_config_path()
    if config_file.exists():
        config.merge_from_file(str(config_file))
    config.freeze()

    model_path = resolve_model_path()
    if not model_path.exists():
        raise FileNotFoundError(f"PSGAN model not found: {model_path}")

    inference_model = Inference(config=config, device=device, model_path=str(model_path))
    postprocess_model = PostProcess(config)


def validate_image(file_data, field_name: str) -> Tuple[bool, str, Optional[Image.Image]]:
    if not file_data:
        return False, f"Missing field '{field_name}'", None

    try:
        image_bytes = file_data.read()
        if len(image_bytes) == 0:
            return False, f"File '{field_name}' is empty", None

        image = Image.open(io.BytesIO(image_bytes)).convert("RGB")
        if image.width < 256 or image.height < 256:
            return False, f"Image '{field_name}' must be at least 256x256", None
        return True, "OK", image
    except Exception as exc:
        return False, f"Invalid image in '{field_name}': {exc}", None


def process_makeup_transfer(
    customer_image: Image.Image,
    makeup_reference: Image.Image,
    lip_only: bool = False,
) -> Tuple[bool, str, Optional[Image.Image]]:
    if inference_model is None or postprocess_model is None:
        return False, "Model not initialized", None

    try:
        transfer_result = inference_model.transfer(
            source=customer_image,
            reference=makeup_reference,
            with_face=True,
            lip_only=lip_only,
        )

        if transfer_result is None:
            return False, "No face detected in one or both images", None

        if isinstance(transfer_result, tuple):
            result_tensor, crop_face = transfer_result
        else:
            result_tensor, crop_face = transfer_result, None

        if result_tensor is None:
            return False, "No face detected in one or both images", None

        if isinstance(result_tensor, Image.Image):
            result_image = result_tensor.convert("RGB")
        else:
            result_numpy = result_tensor.squeeze().cpu().numpy()
            if len(result_numpy.shape) == 2:
                result_numpy = np.stack([result_numpy] * 3, axis=2)
            elif len(result_numpy.shape) == 3 and result_numpy.shape[0] == 3:
                result_numpy = np.transpose(result_numpy, (1, 2, 0))

            result_numpy = (result_numpy + 1) / 2.0
            result_numpy = (result_numpy * 255).clip(0, 255).astype(np.uint8)
            result_image = Image.fromarray(result_numpy, mode="RGB")

        if crop_face is not None:
            try:
                left = max(0, crop_face.left())
                top = max(0, crop_face.top())
                right = min(customer_image.width, crop_face.right())
                bottom = min(customer_image.height, crop_face.bottom())
                if right > left and bottom > top:
                    source_crop = customer_image.crop((left, top, right, bottom))
                    result_image = postprocess_model(source_crop, result_image)
            except Exception:
                pass

        return True, "Success", result_image

    except Exception as exc:
        return False, f"Transfer failed: {exc}", None


@app.get("/api/health")
def health_check():
    ready = inference_model is not None and postprocess_model is not None
    return jsonify(
        {
            "status": "ok" if ready else "initializing",
            "models_ready": ready,
            "device": device,
            "psgan_root": str(PSGAN_ROOT) if PSGAN_ROOT else None,
            "import_error": _import_error_msg if not ready else "",
        }
    )


@app.post("/api/makeup/transfer")
def makeup_transfer():
    try:
        if inference_model is None or postprocess_model is None:
            return jsonify({"success": False, "message": "Models are not initialized"}), 503

        if "customer_photo" not in request.files:
            return jsonify({"success": False, "message": "Missing customer_photo"}), 400
        if "makeup_reference" not in request.files:
            return jsonify({"success": False, "message": "Missing makeup_reference"}), 400

        lip_only = request.form.get("lip_only", "false").lower() == "true"

        ok, msg, customer = validate_image(request.files["customer_photo"], "customer_photo")
        if not ok:
            return jsonify({"success": False, "message": msg}), 400

        ok, msg, reference = validate_image(request.files["makeup_reference"], "makeup_reference")
        if not ok:
            return jsonify({"success": False, "message": msg}), 400

        ok, msg, result_image = process_makeup_transfer(customer, reference, lip_only=lip_only)
        if not ok:
            return jsonify({"success": False, "message": msg}), 500

        img_byte_arr = io.BytesIO()
        result_image.save(img_byte_arr, format="PNG", quality=95)
        img_byte_arr.seek(0)

        return send_file(img_byte_arr, mimetype="image/png", as_attachment=False, download_name="makeup_result.png")

    except Exception as exc:
        return jsonify({"success": False, "message": f"Server error: {exc}"}), 500


@app.get("/")
def index():
    return jsonify(
        {
            "name": "PSGAN Makeup Transfer API",
            "endpoints": {
                "health": "/api/health",
                "transfer": "/api/makeup/transfer",
            },
            "required_form_fields": ["customer_photo", "makeup_reference"],
            "optional_form_fields": ["lip_only"],
        }
    )


def main() -> None:
    host = os.getenv("PSGAN_HOST", "0.0.0.0")
    port = int(os.getenv("PSGAN_PORT", "5000"))
    debug = os.getenv("PSGAN_DEBUG", "false").lower() == "true"

    print(f"Starting PSGAN API on {host}:{port} (device={device})")
    try:
        initialize_models()
        print("Models initialized.")
    except Exception as exc:
        print(f"Model init failed: {exc}")
        print(traceback.format_exc())

    app.run(host=host, port=port, debug=debug, threaded=True)


if __name__ == "__main__":
    main()
