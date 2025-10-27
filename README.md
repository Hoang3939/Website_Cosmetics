# 🔐 User Authentication Module – Cosmetic Store Website

> **Branch:** `feature/user-authentication`  
> **Commit:** `ca96e8f`  
> **Description:** Implements full authentication system (Login, Register, Password Recovery) with role-based access, email confirmation, and admin/staff dashboards.

---

## 🧩 Overview

This feature adds a **complete authentication and authorization system** for the Cosmetic Store web application, including:

- Secure **Login / Logout** flow with session handling  
- **User Registration** with email confirmation  
- **Password Recovery** via reset token  
- Role-based **Access Control (Admin / Staff / User)**  
- Integrated **database migrations**, **validation rules**, and **test documentation**

---

## 📁 Affected Files & Modules

### 🔐 1. User Login/Logout

**Core Models & Services**
Website_Cosmetics/Models/User.cs
Website_Cosmetics/Models/LoginViewModel.cs
Website_Cosmetics/Models/Role.cs
Website_Cosmetics/Models/UserRole.cs
Website_Cosmetics/Services/AuthService.cs
Website_Cosmetics/Services/AuthorizationService.cs

**Controllers & Views**
Website_Cosmetics/Controllers/AuthController.cs
Website_Cosmetics/Views/Auth/Login.cshtml
Website_Cosmetics/Views/Auth/Logout.cshtml

**Layout & UI**
Website_Cosmetics/Views/Shared/_Layout.cshtml
Website_Cosmetics/Areas/Admin/Views/Shared/_LayoutAdmin.cshtml
Website_Cosmetics/wwwroot/public/css/authorize/auth.css

**Configuration**
Website_Cosmetics/Program.cs
Website_Cosmetics/Data/ApplicationDbContext.cs

---

### 📝 2. User Registration

**Models & ViewModels**
Website_Cosmetics/Models/User.cs
Website_Cosmetics/Models/RegisterViewModel.cs
Website_Cosmetics/Models/EmailConfirmationToken.cs

**Services**
Website_Cosmetics/Services/AuthService.cs
Website_Cosmetics/Services/EmailService.cs
Website_Cosmetics/Services/IEmailService.cs

**Controllers & Views**
Website_Cosmetics/Controllers/AuthController.cs
Website_Cosmetics/Views/Auth/Register.cshtml

**Configuration**
Website_Cosmetics/appsettings.json
Website_Cosmetics/Program.cs
Website_Cosmetics/Data/ApplicationDbContext.cs

---

### 🔑 3. Password Recovery

**Models & ViewModels**
Website_Cosmetics/Models/User.cs
Website_Cosmetics/Models/PasswordResetToken.cs
Website_Cosmetics/Models/ForgotPasswordViewModel.cs
Website_Cosmetics/Models/ResetPasswordViewModel.cs

**Services**
Website_Cosmetics/Services/AuthService.cs
Website_Cosmetics/Services/EmailService.cs

**Controllers & Views**
Website_Cosmetics/Controllers/AuthController.cs
Website_Cosmetics/Views/Auth/ForgotPassword.cshtml
Website_Cosmetics/Views/Auth/ResetPassword.cshtml

**Configuration**
Website_Cosmetics/appsettings.json
Website_Cosmetics/Data/ApplicationDbContext.cs

---

### 🗄️ 4. Database & Migration Files

**SQL Scripts**
Website_Cosmetics/Database/DB_Tables.sql
Website_Cosmetics/Database/DB_SampleData.sql
Website_Cosmetics/Database/DB_PermissionTables.sql
Website_Cosmetics/Database/DB_PermissionData.sql

**Entity Framework**
Website_Cosmetics/Migrations/
Website_Cosmetics/Data/ApplicationDbContext.cs

---

### 📚 5. Documentation Files

Website_Cosmetics/TEST_RESULTS.md
Website_Cosmetics/ADMIN_TEST_CASES.md
Website_Cosmetics/VALIDATION_RULES.md
Website_Cosmetics/EMAIL_SETUP.md

---

### 🎨 6. UI Components

**Shared Components**
Website_Cosmetics/Views/Shared/_Alerts.cshtml
Website_Cosmetics/wwwroot/public/css/authorize/auth.css

**Admin Components**
Website_Cosmetics/Areas/Admin/Controllers/StaffDashboardController.cs
Website_Cosmetics/Areas/Admin/Views/StaffDashboard/Index.cshtml
Website_Cosmetics/Attributes/AuthorizationAttributes.cs

---

### ⚙️ 7. Configuration Files

Website_Cosmetics/Website_Cosmetics.csproj
Website_Cosmetics/Program.cs
Website_Cosmetics/appsettings.json

---

## 📊 Summary

| Function Area | Description | Files |
|----------------|--------------|--------|
| **Login/Logout** | Core authentication, role validation, session handling | 15 |
| **Registration** | User creation, validation, email confirmation | 12 |
| **Password Recovery** | Forgot/reset password flow with tokens | 10 |
| **Database** | Schema + EF Migrations | 8 |
| **Documentation** | Test cases, results, validation rules | 4 |

**Total Affected Files:** `49`  
**Commit ID:** `ca96e8f`  
**Status:** ✅ Successfully committed and tested  

---

## 🧪 Testing & Validation

- **Manual Test Reports:**  
  `TEST_RESULTS.md` – All test cases passed (Login, Register, Recovery)

- **Unit/Integration Tests:**  
  Basic test coverage for `AuthService`, `EmailService`, `AuthorizationService`

- **Validation Rules:**  
  Defined in `VALIDATION_RULES.md` and enforced via DataAnnotations + server-side checks.

---

## 🚀 Deployment Notes

1. Update database:
   ```bash
   dotnet ef database update

2. Configure environment variables:
  SMTP_SERVER, SMTP_PORT, SMTP_USER, SMTP_PASS
  JWT_SECRET, ASPNETCORE_ENVIRONMENT

3. Rebuild and restart:
  ```bash
  dotnet build
  dotnet run

4. Access URLs:
/Auth/Login
/Auth/Register
/Auth/ForgotPassword
