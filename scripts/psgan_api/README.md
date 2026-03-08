# PSGAN API Integration (minimal copy)

This folder contains only the integration layer for running PSGAN as a separate API service.

## Included files
- `app.py`: Flask API for virtual try-on (makeup transfer)
- `requirements.txt`: Python dependencies for this API

## Not included
- Full `PSGAN-master` source/tree
- Model weights file (`G.pth`)

## Required external assets
Set environment variables to point to your existing PSGAN source and model:

- `PSGAN_ROOT`: absolute path to PSGAN root (contains `psgan/`, `configs/`, etc.)
- `PSGAN_MODEL_PATH`: absolute path to `G.pth` (optional if under `${PSGAN_ROOT}/assets/models/G.pth`)

## Run
```bash
pip install -r scripts/psgan_api/requirements.txt
python scripts/psgan_api/app.py
```

## API
- `GET /api/health`
- `POST /api/makeup/transfer`
  - form-data:
    - `customer_photo` (file)
    - `makeup_reference` (file)
    - `lip_only` (optional bool string, e.g. `true`)
