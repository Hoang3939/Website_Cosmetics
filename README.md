# Website Cosmetics

Nền tảng thương mại điện tử mỹ phẩm, xây dựng bằng **ASP.NET Core MVC (.NET 8)**, tích hợp:
- Website bán hàng (customer-facing)
- Khu vực quản trị (Admin Area)
- Hệ thống xác thực/phân quyền
- Gợi ý sản phẩm (Recommendation)
- Chat/RAG demo và tích hợp Virtual Try-On (PSGAN API)

---

## 1) Cấu trúc repository

```text
Website_Cosmetics/
├─ Website_Cosmetics/                  # Solution .NET chính
│  ├─ Website_Cosmetics.sln
│  ├─ Database/                        # Script SQL tạo bảng/seed/index
│  └─ Website_Cosmetics/               # ASP.NET Core MVC app
│     ├─ Areas/Admin/
│     ├─ Controllers/
│     ├─ Data/
│     ├─ Migrations/
│     ├─ Models/
│     ├─ Repositories/
│     ├─ Services/
│     ├─ Views/
│     ├─ wwwroot/
│     ├─ Program.cs
│     └─ Website_Cosmetics.csproj
├─ scripts/                            # Python scripts cho RAG/embedding + API phụ trợ
├─ integrations/PSGAN-master/          # Nguồn tích hợp virtual makeup
├─ notebooks/                          # Notebook thử nghiệm
└─ Jenkinsfile                         # Pipeline CI/CD
```

---

## 2) Công nghệ sử dụng

- **Backend:** ASP.NET Core MVC (.NET 8), C#
- **ORM:** Entity Framework Core + SQL Server
- **Authentication:** Cookie auth + service tùy biến
- **Password hash:** BCrypt
- **Email:** MailKit/MimeKit (SMTP)
- **Logging:** Serilog
- **JSON:** Newtonsoft.Json
- **AI/ML tích hợp:** RAG scripts, PSGAN virtual try-on API

---

## 3) Tính năng chính

### Customer
- Trang chủ, danh mục sản phẩm, chi tiết sản phẩm
- Tìm kiếm, wishlist, giỏ hàng, địa chỉ giao hàng
- Đặt hàng, lịch sử đơn hàng
- Hồ sơ người dùng

### Authentication / Authorization
- Đăng ký, đăng nhập, đăng xuất
- Quên mật khẩu, reset mật khẩu, xác thực email
- Phân quyền vai trò (Admin/Staff/User)

### Admin
- Quản lý danh mục, thương hiệu, sản phẩm, biến thể
- Quản lý người dùng, nhân viên, khách hàng
- Quản lý đơn hàng
- Dashboard tổng quan

### Recommendation / AI
- Recommendation service theo độ tương đồng sản phẩm
- Chat/RAG demo qua Python scripts
- Virtual try-on qua tích hợp PSGAN API

---

## 4) Yêu cầu môi trường

- .NET SDK 8.0+
- SQL Server (local hoặc container)
- (Tuỳ chọn) Python 3.10+ cho scripts RAG/PSGAN API

---

## 5) Cấu hình ứng dụng .NET

Tạo file `Website_Cosmetics/Website_Cosmetics/Website_Cosmetics/appsettings.Development.json` và cấu hình:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=CosmeticStoreDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True"
  },
  "Email": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "UseStartTls": true,
    "User": "your_email@gmail.com",
    "Password": "app_password",
    "From": "no-reply@cosmetics.com"
  },
  "Auth": {
    "PasswordHashWorkFactor": 12,
    "ResetTokenExpiryMinutes": 30,
    "EmailConfirmationExpiryHours": 24
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

---

## 6) Chạy dự án .NET

Từ thư mục repo root:

```bash
cd Website_Cosmetics
dotnet restore Website_Cosmetics/Website_Cosmetics/Website_Cosmetics.sln
dotnet build Website_Cosmetics/Website_Cosmetics/Website_Cosmetics.sln -c Debug
```

Apply migration/database:

```bash
dotnet ef database update --project Website_Cosmetics/Website_Cosmetics/Website_Cosmetics/Website_Cosmetics.csproj
```

Run app:

```bash
dotnet run --project Website_Cosmetics/Website_Cosmetics/Website_Cosmetics/Website_Cosmetics.csproj
```

---

## 7) Scripts Python (tuỳ chọn)

Thư mục `scripts/` chứa các tiện ích như:
- `generate_embeddings.py`
- `rag_demo_clean.py`
- `rag_demo_pipeline.py`
- `psgan_api/app.py`

Cài dependency:

```bash
cd scripts
pip install -r requirements.txt
```

PSGAN API riêng:

```bash
cd psgan_api
pip install -r requirements.txt
python app.py
```

---

## 8) Database scripts

- `Website_Cosmetics/Website_Cosmetics/Database/` chứa script SQL phục vụ tạo bảng, seed dữ liệu, index, và script tiện ích kiểm tra/cập nhật.

Khuyến nghị:
- Dùng EF migrations cho luồng chính.
- Dùng SQL scripts khi cần bootstrap nhanh hoặc fix dữ liệu môi trường test.

---

## 9) CI/CD

- Pipeline cấu hình tại `Jenkinsfile` (đang theo nhánh `develop`).

---

## 10) Lưu ý quan trọng

- Không commit secrets vào `appsettings*.json`.
- Chỉ giữ tài liệu tổng hợp tại `README.md` ở root.
- Các tài liệu guide cũ đã được loại bỏ để tránh phân mảnh tài liệu.
