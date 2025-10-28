# Hướng dẫn triển khai hệ thống Authentication

## 1. Tạo Database và Tables

### Chạy script SQL:
```sql
-- Chạy file Database/DB_Tables.sql để tạo các bảng
-- Chạy file Database/DB_SampleData.sql để thêm dữ liệu mẫu
```

### Hoặc sử dụng Entity Framework Migrations:

```bash
# Tạo migration đầu tiên
dotnet ef migrations add InitialCreate

# Cập nhật database
dotnet ef database update
```

## 2. Cấu hình Connection String

Cập nhật `appsettings.json` với connection string phù hợp:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=WebsiteCosmetic;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

## 3. Cài đặt Packages

Các packages đã được thêm vào `Website_Cosmetics.csproj`:
- Microsoft.EntityFrameworkCore.SqlServer
- Microsoft.EntityFrameworkCore.Tools
- Microsoft.AspNetCore.Identity.EntityFrameworkCore
- BCrypt.Net-Next

## 4. Chạy ứng dụng

```bash
# Restore packages
dotnet restore

# Build project
dotnet build

# Chạy ứng dụng
dotnet run
```

## 5. Truy cập các trang Authentication

- **Đăng nhập**: `/Auth/Login`
- **Đăng ký**: `/Auth/Register`
- **Quên mật khẩu**: `/Auth/ForgotPassword`
- **Đặt lại mật khẩu**: `/Auth/ResetPassword?token={token}&email={email}`

## 6. Tài khoản Admin mặc định

- **Username**: admin
- **Email**: admin@cosmetics.com
- **Password**: Admin123!

## 7. Các tính năng đã được triển khai

### Models:
- `User` - Thông tin người dùng
- `Role` - Vai trò người dùng
- `UserRole` - Liên kết user-role
- `PasswordResetToken` - Token đặt lại mật khẩu
- `EmailConfirmationToken` - Token xác nhận email

### ViewModels:
- `LoginViewModel` - Form đăng nhập
- `RegisterViewModel` - Form đăng ký
- `ForgotPasswordViewModel` - Form quên mật khẩu
- `ResetPasswordViewModel` - Form đặt lại mật khẩu

### Services:
- `IAuthService` - Interface cho authentication
- `AuthService` - Service xử lý authentication

### Controllers:
- `AuthController` - Controller xử lý các action authentication

### Views:
- `Login.cshtml` - Trang đăng nhập
- `Register.cshtml` - Trang đăng ký
- `ForgotPassword.cshtml` - Trang quên mật khẩu
- `ResetPassword.cshtml` - Trang đặt lại mật khẩu

### CSS:
- `auth.css` - Styles cho các trang authentication

## 8. Lưu ý quan trọng

1. **Email Service**: Hiện tại token được log ra console. Trong production cần tích hợp email service thực tế.

2. **Password Hashing**: Sử dụng BCrypt để hash mật khẩu.

3. **Session Management**: Sử dụng Cookie Authentication với sliding expiration.

4. **Validation**: Có validation đầy đủ cho tất cả forms.

5. **Security**: Có CSRF protection và proper authentication flow.

## 9. Các bước tiếp theo có thể làm

1. Tích hợp email service để gửi email xác nhận và đặt lại mật khẩu
2. Thêm tính năng đổi mật khẩu
3. Thêm tính năng quản lý profile
4. Thêm tính năng đăng nhập bằng Google/Facebook
5. Thêm tính năng 2FA
6. Thêm audit log cho các hoạt động authentication
