#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Flask API Server for PSGAN Makeup Transfer
==========================================

Tích hợp với Website Cosmetics để cung cấp tính năng Virtual Try-On.

API Endpoints:
- POST /api/makeup/transfer - Chuyển đổi makeup từ ảnh reference sang ảnh khách hàng
- GET /api/health - Kiểm tra trạng thái server

Author: AI Assistant
Date: 2025-11-07
"""

import os
import io
import sys
import traceback
from pathlib import Path
from typing import Optional, Tuple

from flask import Flask, request, jsonify, send_file
from flask_cors import CORS
from PIL import Image
import numpy as np
import torch

# Import PSGAN modules
from psgan import Inference, PostProcess, get_config

# Setup paths
SCRIPT_DIR = Path(__file__).parent.resolve()
sys.path.insert(0, str(SCRIPT_DIR))

# Initialize Flask app
app = Flask(__name__)
CORS(app)  # Enable CORS for cross-origin requests from ASP.NET

# Global variables for model
inference_model: Optional[Inference] = None
postprocess_model: Optional[PostProcess] = None
config = None
device = "cpu"  # Default to CPU, can be changed to "cuda" if GPU available


def initialize_models():
    """
    Khởi tạo các model PSGAN khi server start.
    Model sẽ được load vào RAM/GPU và tái sử dụng cho các request.
    """
    global inference_model, postprocess_model, config, device
    
    try:
        print("=" * 60)
        print("🚀 Đang khởi tạo PSGAN Models...")
        print("=" * 60)
        
        # Check if CUDA is available
        if torch.cuda.is_available():
            device = "cuda:0"
            print(f"✅ GPU phát hiện: {torch.cuda.get_device_name(0)}")
        else:
            device = "cpu"
            print("ℹ️  Sử dụng CPU (có thể chậm hơn)")
        
        # Load configuration
        config = get_config()
        config_file = SCRIPT_DIR / "configs" / "base.yaml"
        
        if config_file.exists():
            config.merge_from_file(str(config_file))
            print(f"✅ Đã load config từ: {config_file}")
        else:
            print(f"⚠️  Không tìm thấy config file, sử dụng config mặc định")
        
        config.freeze()
        
        # Model path
        model_path = SCRIPT_DIR / "assets" / "models" / "G.pth"
        
        if not model_path.exists():
            raise FileNotFoundError(
                f"❌ Không tìm thấy model file: {model_path}\n"
                f"   Vui lòng tải model và đặt vào thư mục assets/models/"
            )
        
        print(f"✅ Model file: {model_path}")
        
        # Initialize Inference model
        print("⏳ Đang load Inference model...")
        inference_model = Inference(
            config=config,
            device=device,
            model_path=str(model_path)
        )
        print("✅ Inference model đã sẵn sàng")
        
        # Initialize PostProcess model
        print("⏳ Đang load PostProcess model...")
        postprocess_model = PostProcess(config)
        print("✅ PostProcess model đã sẵn sàng")
        
        print("=" * 60)
        print("🎉 PSGAN Models khởi tạo thành công!")
        print(f"🖥️  Device: {device}")
        print(f"📦 Image size: {config.DATA.IMG_SIZE}x{config.DATA.IMG_SIZE}")
        print("=" * 60)
        
    except Exception as e:
        print("=" * 60)
        print("❌ LỖI khi khởi tạo models:")
        print(str(e))
        print(traceback.format_exc())
        print("=" * 60)
        raise


def validate_image(file_data, field_name: str) -> Tuple[bool, str, Optional[Image.Image]]:
    """
    Validate và load image từ request.
    
    Args:
        file_data: File data từ request.files
        field_name: Tên field (để hiển thị lỗi)
    
    Returns:
        Tuple (success, message, image)
    """
    if not file_data:
        return False, f"Thiếu field '{field_name}'", None
    
    try:
        # Read image
        image_bytes = file_data.read()
        
        if len(image_bytes) == 0:
            return False, f"File '{field_name}' trống", None
        
        # Convert to PIL Image
        image = Image.open(io.BytesIO(image_bytes)).convert("RGB")
        
        # Validate image size (minimum 256x256)
        min_size = 256
        if image.width < min_size or image.height < min_size:
            return False, f"Ảnh '{field_name}' quá nhỏ (tối thiểu {min_size}x{min_size}px)", None
        
        return True, "OK", image
        
    except Exception as e:
        return False, f"Lỗi khi đọc ảnh '{field_name}': {str(e)}", None


def process_makeup_transfer(customer_image: Image.Image, makeup_reference: Image.Image, lip_only: bool = False) -> Tuple[bool, str, Optional[Image.Image]]:
    """
    Thực hiện chuyển đổi makeup từ reference sang customer image.
    
    Args:
        customer_image: Ảnh khuôn mặt khách hàng
        makeup_reference: Ảnh mẫu makeup (từ sản phẩm)
        lip_only: CHỈ apply makeup lên vùng môi (mặc định: False)
    
    Returns:
        Tuple (success, message, result_image)
    """
    global inference_model, postprocess_model
    
    if inference_model is None or postprocess_model is None:
        return False, "Model chưa được khởi tạo", None
    
    try:
        print(f"🔄 Đang xử lý makeup transfer...")
        print(f"   - Customer image: {customer_image.size}")
        print(f"   - Reference image: {makeup_reference.size}")
        print(f"   - Lip only: {lip_only}")
        
        # Perform makeup transfer với lip_only parameter
        print(f"   - Đang gọi inference_model.transfer()...")
        transfer_result = inference_model.transfer(
            source=customer_image,
            reference=makeup_reference,
            with_face=True,
            lip_only=lip_only
        )
        
        # Handle return value - có thể là None, (None, None), hoặc (tensor, crop_face)
        if transfer_result is None:
            result_tensor, crop_face = None, None
        elif isinstance(transfer_result, tuple):
            result_tensor, crop_face = transfer_result
        else:
            # Single return value (không có with_face)
            result_tensor = transfer_result
            crop_face = None
        
        print(f"   - Transfer result type: {type(transfer_result)}")
        print(f"   - result_tensor type: {type(result_tensor)}")
        print(f"   - result_tensor is None: {result_tensor is None}")
        print(f"   - crop_face type: {type(crop_face)}")
        print(f"   - crop_face is None: {crop_face is None}")
        
        # Check if face detected - check None và các falsy values
        if result_tensor is None:
            # Check which image failed by preprocessing separately
            print(f"   ⚠️  Không phát hiện khuôn mặt - kiểm tra từng ảnh...")
            
            # Reuse preprocess instance from inference_model instead of creating new ones
            # This is more efficient and uses the same configuration
            try:
                customer_input, customer_face, customer_crop = inference_model.preprocess(customer_image)
                reference_input, reference_face, reference_crop = inference_model.preprocess(makeup_reference)
                
                print(f"   - Customer image preprocessing: input={customer_input is not None}, face={customer_face is not None}")
                print(f"   - Reference image preprocessing: input={reference_input is not None}, face={reference_face is not None}")
                
                if customer_input is None:
                    return False, "Không phát hiện khuôn mặt trong ảnh của bạn. Vui lòng sử dụng ảnh có khuôn mặt rõ ràng, nhìn thẳng vào camera, không bị che khuất.", None
                elif reference_input is None:
                    return False, "Không phát hiện khuôn mặt trong ảnh mẫu makeup. Ảnh mẫu phải là ảnh có khuôn mặt người mẫu, không phải ảnh sản phẩm. Vui lòng liên hệ admin.", None
                else:
                    return False, "Không thể xử lý makeup transfer. Có thể do chất lượng ảnh hoặc vấn đề kỹ thuật. Vui lòng thử lại với ảnh khác.", None
            except Exception as e:
                print(f"   ❌ Error during face detection check: {str(e)}")
                return False, f"Lỗi khi kiểm tra khuôn mặt: {str(e)}", None
        
        # Check crop_face separately (may be None even if result_tensor is not None)
        if crop_face is None:
            print(f"   ⚠️  crop_face is None - bỏ qua postprocess")
            # Skip postprocess, use result directly
        else:
            print(f"   - Crop face: left={crop_face.left()}, top={crop_face.top()}, right={crop_face.right()}, bottom={crop_face.bottom()}")
        
        # Handle result - solver.test() returns PIL Image, not tensor
        # Check if result_tensor is already a PIL Image
        if isinstance(result_tensor, Image.Image):
            print(f"   ✅ Result is already a PIL Image: {result_tensor.size}, mode={result_tensor.mode}")
            result_image = result_tensor
            # Ensure RGB mode
            if result_image.mode != 'RGB':
                print(f"   - Converting from {result_image.mode} to RGB")
                result_image = result_image.convert('RGB')
        elif hasattr(result_tensor, 'cpu') and hasattr(result_tensor, 'numpy'):
            # It's a tensor - convert to PIL Image
            print(f"   - Converting tensor to image...")
            try:
                result_numpy = result_tensor.squeeze().cpu().numpy()
                
                # Handle different tensor shapes
                if len(result_numpy.shape) == 2:
                    # Grayscale - convert to RGB
                    result_numpy = np.stack([result_numpy] * 3, axis=2)
                elif len(result_numpy.shape) == 3:
                    if result_numpy.shape[0] == 3:
                        # [C, H, W] - transpose to [H, W, C]
                        result_numpy = np.transpose(result_numpy, (1, 2, 0))
                    elif result_numpy.shape[2] == 3:
                        # Already [H, W, C]
                        pass
                    else:
                        # Unexpected shape
                        raise ValueError(f"Unexpected tensor shape: {result_numpy.shape}")
                
                result_numpy = (result_numpy + 1) / 2.0  # Denormalize from [-1, 1] to [0, 1]
                result_numpy = (result_numpy * 255).clip(0, 255).astype(np.uint8)
                result_image = Image.fromarray(result_numpy, mode='RGB')
                print(f"   - Result image shape: {result_numpy.shape}, size: {result_image.size}")
            except Exception as e:
                error_msg = f"Lỗi khi convert tensor sang image: {str(e)}"
                print(f"   ❌ {error_msg}")
                if hasattr(result_tensor, 'shape'):
                    print(f"   Tensor shape: {result_tensor.shape}")
                raise Exception(error_msg)
        else:
            error_msg = f"Unexpected result type: {type(result_tensor)}"
            print(f"   ❌ {error_msg}")
            raise Exception(error_msg)
        
        # Post-process to enhance quality (only if crop_face is available)
        if crop_face is not None:
            try:
                print(f"   - Post-processing với crop_face...")
                # Validate crop_face coordinates
                left = max(0, crop_face.left())
                top = max(0, crop_face.top())
                right = min(customer_image.width, crop_face.right())
                bottom = min(customer_image.height, crop_face.bottom())
                
                if right > left and bottom > top:
                    source_crop = customer_image.crop((left, top, right, bottom))
                    result_image = postprocess_model(source_crop, result_image)
                    print(f"   - Post-process completed")
                else:
                    print(f"   ⚠️  Invalid crop_face coordinates, skip postprocess")
            except Exception as e:
                print(f"   ⚠️  Lỗi khi post-process (bỏ qua): {str(e)}")
                # Continue without postprocess - result_image is already usable
        
        print(f"✅ Makeup transfer hoàn tất")
        print(f"   - Result image: {result_image.size}")
        
        return True, "Thành công", result_image
        
    except Exception as e:
        error_msg = f"Lỗi khi xử lý makeup transfer: {str(e)}"
        print(f"❌ {error_msg}")
        print(traceback.format_exc())
        return False, error_msg, None


# ==================== API ENDPOINTS ====================

@app.route('/api/health', methods=['GET'])
def health_check():
    """
    Kiểm tra trạng thái server và models.
    """
    global inference_model, postprocess_model, device
    
    models_ready = (inference_model is not None and postprocess_model is not None)
    
    return jsonify({
        'status': 'ok' if models_ready else 'initializing',
        'message': 'PSGAN Makeup Transfer API đang hoạt động',
        'models_ready': models_ready,
        'device': device,
        'version': '1.0',
        'endpoints': {
            'transfer': '/api/makeup/transfer (POST)',
            'health': '/api/health (GET)'
        }
    })


@app.route('/api/makeup/transfer', methods=['POST'])
def makeup_transfer():
    """
    API endpoint chính để thực hiện makeup transfer.
    
    Request:
        - Method: POST
        - Content-Type: multipart/form-data
        - Fields:
            * customer_photo: File (required) - Ảnh khuôn mặt khách hàng
            * makeup_reference: File (required) - Ảnh mẫu makeup từ sản phẩm
    
    Response:
        - Success: image/png (binary data của ảnh kết quả)
        - Error: JSON với thông tin lỗi
    """
    try:
        print("\n" + "=" * 60)
        print("📥 Nhận request makeup transfer")
        
        # Check if models are ready
        if inference_model is None or postprocess_model is None:
            return jsonify({
                'success': False,
                'message': 'Models chưa được khởi tạo. Vui lòng đợi hoặc khởi động lại server.'
            }), 503
        
        # Validate request
        if 'customer_photo' not in request.files:
            return jsonify({
                'success': False,
                'message': "Thiếu field 'customer_photo' trong request"
            }), 400
        
        if 'makeup_reference' not in request.files:
            return jsonify({
                'success': False,
                'message': "Thiếu field 'makeup_reference' trong request"
            }), 400
        
        # Đọc parameter lip_only từ request (optional, default: False)
        lip_only = request.form.get('lip_only', 'false').lower() == 'true'
        print(f"   - Lip only mode: {lip_only}")
        
        # Validate and load customer photo
        success, msg, customer_image = validate_image(
            request.files['customer_photo'],
            'customer_photo'
        )
        
        if not success:
            return jsonify({
                'success': False,
                'message': msg
            }), 400
        
        # Validate and load makeup reference
        success, msg, makeup_image = validate_image(
            request.files['makeup_reference'],
            'makeup_reference'
        )
        
        if not success:
            return jsonify({
                'success': False,
                'message': msg
            }), 400
        
        # Process makeup transfer với lip_only parameter
        success, msg, result_image = process_makeup_transfer(
            customer_image,
            makeup_image,
            lip_only=lip_only
        )
        
        if not success:
            return jsonify({
                'success': False,
                'message': msg
            }), 500
        
        # Convert result image to bytes
        img_byte_arr = io.BytesIO()
        result_image.save(img_byte_arr, format='PNG', quality=95)
        img_byte_arr.seek(0)
        
        print("✅ Gửi kết quả về client")
        print("=" * 60 + "\n")
        
        # Return image as binary
        return send_file(
            img_byte_arr,
            mimetype='image/png',
            as_attachment=False,
            download_name='makeup_result.png'
        )
        
    except Exception as e:
        error_msg = f"Lỗi server: {str(e)}"
        print(f"❌ {error_msg}")
        print(traceback.format_exc())
        print("=" * 60 + "\n")
        
        return jsonify({
            'success': False,
            'message': error_msg
        }), 500


@app.route('/', methods=['GET'])
def index():
    """
    Trang chủ API với hướng dẫn sử dụng.
    """
    return jsonify({
        'name': 'PSGAN Makeup Transfer API',
        'version': '1.0',
        'description': 'API để chuyển đổi makeup cho Website Cosmetics',
        'endpoints': {
            'health': {
                'url': '/api/health',
                'method': 'GET',
                'description': 'Kiểm tra trạng thái server'
            },
            'transfer': {
                'url': '/api/makeup/transfer',
                'method': 'POST',
                'description': 'Thực hiện makeup transfer',
                'parameters': {
                    'customer_photo': 'File - Ảnh khuôn mặt khách hàng',
                    'makeup_reference': 'File - Ảnh mẫu makeup'
                },
                'response': 'image/png hoặc JSON error'
            }
        },
        'usage': {
            'curl_example': '''
curl -X POST http://localhost:5000/api/makeup/transfer \\
  -F "customer_photo=@customer.jpg" \\
  -F "makeup_reference=@makeup_style.jpg" \\
  -o result.png
            '''
        }
    })


# ==================== MAIN ====================

def main():
    """
    Entry point cho Flask server.
    """
    import argparse
    
    parser = argparse.ArgumentParser(
        description='PSGAN Flask API Server for Virtual Try-On'
    )
    parser.add_argument(
        '--host',
        default='0.0.0.0',
        help='Host để bind server (default: 0.0.0.0)'
    )
    parser.add_argument(
        '--port',
        type=int,
        default=5000,
        help='Port để chạy server (default: 5000)'
    )
    parser.add_argument(
        '--device',
        default='cpu',
        choices=['cpu', 'cuda', 'cuda:0', 'cuda:1'],
        help='Device để chạy model (default: cpu)'
    )
    parser.add_argument(
        '--debug',
        action='store_true',
        help='Bật debug mode'
    )
    
    args = parser.parse_args()
    
    # Set device
    global device
    if args.device.startswith('cuda') and not torch.cuda.is_available():
        print("⚠️  CUDA không khả dụng, fallback về CPU")
        device = 'cpu'
    else:
        device = args.device
    
    # Initialize models trước khi start server
    print("\n")
    print("🚀 PSGAN Flask API Server")
    print("=" * 60)
    print(f"📍 Host: {args.host}")
    print(f"📍 Port: {args.port}")
    print(f"🖥️  Device: {device}")
    print(f"🐛 Debug: {args.debug}")
    print("=" * 60)
    print("\n")
    
    try:
        initialize_models()
    except Exception as e:
        print(f"\n❌ Không thể khởi tạo models. Server sẽ không hoạt động đúng.")
        print(f"   Lỗi: {str(e)}")
        if not args.debug:
            print(f"   Khuyến nghị: Kiểm tra lại model file và dependencies")
            return
    
    # Start Flask server
    print("\n")
    print("=" * 60)
    print(f"🌐 Server đang chạy tại: http://{args.host}:{args.port}")
    print(f"📡 API endpoint: http://{args.host}:{args.port}/api/makeup/transfer")
    print(f"🏥 Health check: http://{args.host}:{args.port}/api/health")
    print("=" * 60)
    print("\n💡 Nhấn Ctrl+C để dừng server\n")
    
    # Run Flask app
    app.run(
        host=args.host,
        port=args.port,
        debug=args.debug,
        threaded=True  # Enable threading for concurrent requests
    )


if __name__ == '__main__':
    main()

