# 📧 Hướng dẫn cấu hình Email Service

## 🔧 Cấu hình Gmail

### Bước 1: Tạo App Password
1. Đăng nhập vào Gmail
2. Vào **Settings** → **Security** → **2-Step Verification** (bật nếu chưa có)
3. Vào **App passwords** → Tạo password mới cho "Mail"
4. Copy password này (16 ký tự)

### Bước 2: Cập nhật appsettings.json
```json
{
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": "587",
    "Username": "your-email@gmail.com",
    "Password": "your-16-char-app-password",
    "FromEmail": "your-email@gmail.com",
    "FromName": "Cosmetic Store",
    "BaseUrl": "http://localhost:5210"
  }
}
```

### Bước 3: Test Email
1. Chạy ứng dụng: `dotnet run`
2. Đăng ký tài khoản mới
3. Kiểm tra email để xác nhận

## 📋 Các loại email được gửi

### ✅ Email xác nhận đăng ký
- **Khi nào**: Sau khi đăng ký thành công
- **Nội dung**: Link xác nhận tài khoản
- **Hết hạn**: 24 giờ

### 🔒 Email đặt lại mật khẩu
- **Khi nào**: Khi quên mật khẩu
- **Nội dung**: Link đặt lại mật khẩu
- **Hết hạn**: 1 giờ

## 🚀 Cách sử dụng

### Đăng ký tài khoản:
1. Vào `/Auth/Register`
2. Điền thông tin
3. Nhấn "Đăng ký"
4. Kiểm tra email và nhấn link xác nhận
5. Đăng nhập với tài khoản đã xác nhận

### Quên mật khẩu:
1. Vào `/Auth/ForgotPassword`
2. Nhập email
3. Kiểm tra email và nhấn link đặt lại
4. Nhập mật khẩu mới

## ⚠️ Lưu ý

- **App Password**: Không dùng mật khẩu Gmail thường
- **2FA**: Phải bật 2-Step Verification trước
- **Port**: Gmail dùng port 587 (TLS)
- **Security**: Không commit app password vào Git

## 🔍 Troubleshooting

### Lỗi "Authentication failed":
- Kiểm tra App Password đúng chưa
- Đảm bảo đã bật 2FA

### Lỗi "Connection timeout":
- Kiểm tra firewall
- Thử port khác (465 cho SSL)

### Email không đến:
- Kiểm tra Spam folder
- Kiểm tra email address đúng chưa
