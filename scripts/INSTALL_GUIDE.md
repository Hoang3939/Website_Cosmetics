# Hướng dẫn cài đặt Python Dependencies

## Cách 1: Sử dụng Virtual Environment hiện có

Nếu bạn đã có virtual environment (ví dụ: `c:\Users\Hoang\VirtualMakeup\.venv`):

### Bước 1: Activate virtual environment

**Windows PowerShell:**
```powershell
cd c:\Users\Hoang\VirtualMakeup
.\.venv\Scripts\Activate.ps1
```

**Windows CMD:**
```cmd
cd c:\Users\Hoang\VirtualMakeup
.venv\Scripts\activate.bat
```

### Bước 2: Cài đặt dependencies

```bash
cd d:\Projects\Website_Cosmetics\scripts
pip install -r requirements.txt
```

## Cách 2: Tạo Virtual Environment mới cho project

### Bước 1: Tạo virtual environment

```bash
cd d:\Projects\Website_Cosmetics\scripts
python -m venv .venv
```

### Bước 2: Activate virtual environment

**Windows PowerShell:**
```powershell
.\.venv\Scripts\Activate.ps1
```

**Windows CMD:**
```cmd
.venv\Scripts\activate.bat
```

### Bước 3: Cài đặt dependencies

```bash
pip install -r requirements.txt
```

## Kiểm tra cài đặt

Sau khi cài đặt, kiểm tra:

```bash
python -c "import pyodbc; import google.genai; import numpy; print('✅ All packages installed successfully!')"
```

## Lưu ý về pyodbc

Nếu gặp lỗi khi cài `pyodbc`, bạn cần cài **ODBC Driver for SQL Server**:

1. Tải từ: https://learn.microsoft.com/en-us/sql/connect/odbc/download-odbc-driver-for-sql-server
2. Chọn: **ODBC Driver 17 for SQL Server** (hoặc version mới hơn)
3. Cài đặt và chạy lại `pip install pyodbc`

## Chạy script

### Script SQL Server (hệ thống chính)

```bash
python generate_embeddings.py
```

### Script RAG demo PostgreSQL (chuyển từ notebook)

Script mới: `rag_demo_pipeline.py`

1. Thiết lập biến môi trường (PowerShell):

```powershell
$env:GEMINI_API_KEY="your_gemini_api_key"
$env:PGHOST="localhost"
$env:PGDATABASE="rag_demo"
$env:PGUSER="postgres"
$env:PGPASSWORD="your_password"
$env:PGPORT="5432"
```

2. Chạy script:

```bash
python rag_demo_pipeline.py
```

Tùy chọn:

- `--dry-run`: tạo embedding nhưng không ghi DB
- `--all`: xử lý toàn bộ sản phẩm có mô tả (mặc định chỉ xử lý bản ghi thiếu embedding)


