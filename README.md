# 💄 Cosmetic Store – ASP.NET Core MVC (develop)

> **Branch:** `develop`  
> **Framework:** .NET 8 • ASP.NET Core MVC • EF Core • SQL Server  
> **Goal:** Stable integration branch that aggregates features, passes tests, and is deployable to test/staging.

---

## 📦 Project Overview

An e-commerce web application for cosmetics with:
- User Authentication (Login/Logout, Register, Password Recovery, Email Confirmation)
- Role-based Authorization (Admin / Staff / User)
- Admin module: Users, Products, Orders, Permissions, Reports
- Customer module: Home, Product browsing/search, Cart (in scope of FE)
- Clean MVC layering and EF Core with SQL Server

---

## 🏗 Architecture

Website_Cosmetics/ # Solution root
├─ Controllers/ # MVC controllers (Auth, Products, Orders, Admin/*)
├─ Areas/
│ └─ Admin/
│ ├─ Controllers/
│ └─ Views/
├─ Models/ # Domain models + ViewModels
├─ Data/
│ ├─ ApplicationDbContext.cs # EF Core DbContext
│ └─ Migrations/ # EF Core migrations
├─ Services/ # AuthService, EmailService, AuthorizationService, etc.
├─ Views/ # Razor Views (.cshtml)
│ └─ Shared/ (_Layout.cshtml, _Alerts.cshtml, ...)
├─ wwwroot/ # Static assets (css, js, images)
├─ Database/ # Optional SQL scripts (schema, seed, permissions)
├─ appsettings.json # Local settings (DO NOT commit secrets)
├─ Program.cs # App bootstrap (DI, middleware, routing)
└─ Website_Cosmetics.csproj


---

## 🛠 Tech Stack

- **Backend:** ASP.NET Core MVC (.NET 8), C#
- **ORM:** Entity Framework Core (SQL Server)
- **Auth:** Cookies / Identity-like flow (custom models), BCrypt
- **Mail:** MailKit / SMTP
- **Build/CI:** dotnet CLI, (Jenkins/GitHub Actions supported)
- **Tests:** Manual test packs + (room for xUnit/MSTest)

---

## ⚙️ Configuration

Create `appsettings.Development.json` (local) and set environment variables in production.

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

▶️ Build & Run
# Restore dependencies
dotnet restore

# Build project
dotnet build -c Debug   # or Release

# Apply database migrations
dotnet ef database update

# Run the application
dotnet run --project Website_Cosmetics
# App runs at http://localhost:5000 (or as configured in Kestrel/appsettings)

🧩 EF Core CLI Commands
# Install EF tool (if not installed)
dotnet tool install --global dotnet-ef

# Create a new migration
dotnet ef migrations add InitAuthAndCore

# Apply latest migration
dotnet ef database update
