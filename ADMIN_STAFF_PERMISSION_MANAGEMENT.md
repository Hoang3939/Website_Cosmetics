# 🔐 Admin Staff Permission Management System

## 📋 Tổng quan

Hệ thống quản lý phân quyền cho nhân viên trong Admin với đầy đủ tính năng CRUD cho permissions, cấp/thu hồi quyền cho nhân viên, tìm kiếm, lọc, và quyền mặc định cho Staff role.

**Ngày tạo:** 2025-01-XX  
**Branch:** `feature/admin-staff-permission-management`

---

## ✨ Tính Năng Chính

### 1. Danh Sách Nhân Viên & Phân Quyền (Index)

#### Route
- **GET:** `/Admin/StaffPermissions`
- **GET với filter:** `/Admin/StaffPermissions?search={keyword}&category={category}&page={page}&pageSize={size}`

#### Chức Năng

**Tìm Kiếm (Search):**
- Tìm kiếm theo:
  - Username (Tên đăng nhập)
  - Email
  - FirstName (Tên)
  - LastName (Họ)
  - FullName (Họ và tên)
- Không phân biệt hoa thường
- Tìm kiếm partial match (Contains)

**Lọc (Filter):**
- **Theo Category:** (Có thể mở rộng trong tương lai)
  - `category={categoryName}` → Lọc theo category của permission

**Phân Trang:**
- Mặc định: 10 items/trang
- Có thể tùy chỉnh `pageSize`
- Hiển thị thông tin: CurrentPage, TotalPages, TotalCount

**Sắp Xếp:**
- Mặc định: Sắp xếp theo `CreatedAt` (mới nhất trước)

**Hiển Thị:**
- Danh sách nhân viên (Staff role)
- Permissions của mỗi nhân viên
- ViewModel: `StaffPermissionViewModel` với User và Permissions

**Ví Dụ URL:**
```
/Admin/StaffPermissions                              → Tất cả nhân viên
/Admin/StaffPermissions?search=nguyen                → Tìm "nguyen"
/Admin/StaffPermissions?page=2&pageSize=20          → Trang 2, 20 items/trang
```

#### ViewBag Parameters
- `ViewBag.Search` - Từ khóa tìm kiếm hiện tại
- `ViewBag.Category` - Category filter hiện tại
- `ViewBag.CurrentPage` - Trang hiện tại
- `ViewBag.TotalPages` - Tổng số trang
- `ViewBag.TotalCount` - Tổng số nhân viên
- `ViewBag.PageSize` - Số items mỗi trang
- `ViewBag.Categories` - Danh sách các categories có sẵn

---

### 2. Xem Chi Tiết Phân Quyền (Details)

#### Route
- **GET:** `/Admin/StaffPermissions/Details/{id}`

#### Chức Năng
- Hiển thị đầy đủ thông tin phân quyền của nhân viên:
  - **Thông tin nhân viên:**
    - UserId, Username, Email
    - FirstName, LastName, FullName
    - Roles (Staff role)
  - **Danh sách permissions hiện tại:**
    - Tất cả permissions của nhân viên (từ role và user-specific)
    - Thông tin chi tiết UserPermissions:
      - PermissionName
      - GrantedBy (Admin nào cấp quyền)
      - GrantedAt (Thời gian cấp)
      - ExpiresAt (Thời gian hết hạn - nếu có)
      - IsActive
  - **Danh sách permissions có thể cấp:**
    - Tất cả permissions thuộc Category "Staff"
    - Cho phép cấp/thu hồi quyền

#### Validation
- Kiểm tra user có tồn tại
- Kiểm tra user có role "Staff"
- Nếu không phải Staff → Return NotFound()

---

### 3. Tạo Permission Mới (Create)

#### Route
- **GET:** `/Admin/StaffPermissions/Create`
- **POST:** `/Admin/StaffPermissions/Create`

#### Chức Năng

**GET Action:**
- Hiển thị form tạo permission mới

**POST Action:**
- **Validation:**
  - ModelState validation
  - Kiểm tra PermissionName đã tồn tại → Error: "Tên quyền đã tồn tại. Vui lòng chọn tên khác."
  - PermissionName format: chỉ chứa chữ cái, số, dấu chấm, và underscore (regex: `^[a-zA-Z0-9._]+$`)
  - Category format: chỉ chứa chữ cái và khoảng trắng (regex: `^[a-zA-Z\s]+$`)

- **Tự động:**
  - Set `IsActive = true`
  - Set `CreatedAt = DateTime.UtcNow`
  - Set `UpdatedAt = DateTime.UtcNow`

- **Success:**
  - TempData message: `"Quyền '{PermissionName}' đã được tạo thành công."`
  - Redirect đến Index

- **Error Handling:**
  - Try-catch với logging
  - ModelState error: `"Lỗi khi tạo quyền: {ex.Message}"`

#### Form Fields
- PermissionName (required, unique, max 100, format: `^[a-zA-Z0-9._]+$`)
- Description (optional, max 255)
- Category (optional, max 50, format: `^[a-zA-Z\s]+$`)
- IsActive (default: true)

---

### 4. Sửa Permission (Edit)

#### Route
- **GET:** `/Admin/StaffPermissions/Edit/{id}`
- **POST:** `/Admin/StaffPermissions/Edit/{id}`

#### Chức Năng

**GET Action:**
- Load permission hiện tại
- Return View với permission data

**POST Action:**
- **Validation:**
  - Kiểm tra `id == permission.PermissionId`
  - ModelState validation
  - Kiểm tra PermissionName đã tồn tại (trừ permission hiện tại) → Error

- **Cập Nhật:**
  - PermissionName
  - Description
  - Category
  - IsActive
  - UpdatedAt = DateTime.UtcNow

- **Success:**
  - TempData message: `"Quyền '{PermissionName}' đã được cập nhật thành công."`
  - Redirect đến Index

- **Error Handling:**
  - DbUpdateConcurrencyException → Kiểm tra PermissionExists
  - Exception → Logging + ModelState error

---

### 5. Xóa Permission (Delete)

#### Route
- **GET:** `/Admin/StaffPermissions/Delete/{id}` (Xác nhận xóa)
- **POST:** `/Admin/StaffPermissions/Delete/{id}` (Xóa thực sự)

#### Chức Năng

**GET Action:**
- Hiển thị thông tin permission để xác nhận
- **Kiểm tra số lượng sử dụng:**
  - Load UserPermissions và RolePermissions
  - Hiển thị số lượng user và role đang sử dụng permission này
  - ViewBag.UserPermissionCount = số lượng
  - ViewBag.RolePermissionCount = số lượng

**POST Action (DeleteConfirmed):**
- **Validation:**
  - Kiểm tra permission tồn tại
  - **Kiểm tra có user hoặc role đang sử dụng:**
    - Nếu có → Không cho phép xóa
    - Error message: `"Không thể xóa quyền '{PermissionName}' vì có {userCount} user và {roleCount} role đang sử dụng quyền này."`
    - Redirect về Delete page

- **Xóa:**
  - Xóa permission
  - **Lưu ý:** Cascade delete sẽ tự động xóa UserPermissions và RolePermissions liên quan

- **Success:**
  - TempData message: `"Quyền '{PermissionName}' đã được xóa thành công."`
  - Redirect đến Index

- **Error Handling:**
  - Exception → Logging + TempData error message
  - Redirect đến Index (không throw exception)

---

### 6. Cấp Quyền Cho Nhân Viên (GrantPermission)

#### Route
- **POST:** `/Admin/StaffPermissions/GrantPermission`

#### Chức Năng
- **Parameters:**
  - `userId` (int) - ID của nhân viên
  - `permissionName` (string) - Tên quyền cần cấp

- **Logic:**
  - Lấy currentUserId từ Claims
  - Gọi `IAuthorizationService.GrantPermissionAsync()`
  - Nếu permission đã tồn tại nhưng inactive → Kích hoạt lại
  - Nếu chưa có → Tạo mới UserPermission

- **Success:**
  - TempData message: `"Quyền đã được cấp thành công!"`
  - Redirect đến Details

- **Error Handling:**
  - Try-catch với logging
  - TempData error message nếu có lỗi

---

### 7. Thu Hồi Quyền (RevokePermission)

#### Route
- **POST:** `/Admin/StaffPermissions/RevokePermission`

#### Chức Năng
- **Parameters:**
  - `userId` (int) - ID của nhân viên
  - `permissionName` (string) - Tên quyền cần thu hồi

- **Logic:**
  - Gọi `IAuthorizationService.RevokePermissionAsync()`
  - Set `IsActive = false` cho UserPermission
  - Không xóa record, chỉ vô hiệu hóa

- **Success:**
  - TempData message: `"Quyền đã được thu hồi thành công!"`
  - Redirect đến Details

- **Error Handling:**
  - Try-catch với logging
  - TempData error message nếu có lỗi

---

### 8. Cấp Quyền Mặc Định (AssignDefaultPermissions)

#### Route
- **POST:** `/Admin/StaffPermissions/AssignDefaultPermissions`

#### Chức Năng
- **Parameters:**
  - `userId` (int) - ID của nhân viên

- **Default Permissions:**
  - `Admin.Order.Manage` - Quản lý đơn hàng
  - `Admin.Customer.Manage` - Quản lý khách hàng

- **Logic:**
  - Lấy currentUserId từ Claims
  - Cấp tất cả default permissions cho nhân viên
  - Sử dụng `IAuthorizationService.GrantPermissionAsync()`

- **Success:**
  - TempData message: `"Đã cấp quyền mặc định cho nhân viên thành công!"`
  - Redirect đến Details

- **Error Handling:**
  - Try-catch với logging
  - TempData error message nếu có lỗi

---

## 🔄 Default Permissions cho Staff

### Tự Động Cấp Khi Tạo Nhân Viên

Khi tạo nhân viên mới trong `StaffController.Create`, hệ thống tự động cấp 2 quyền mặc định:

1. **Admin.Order.Manage** - Quản lý đơn hàng
2. **Admin.Customer.Manage** - Quản lý khách hàng

**Code trong StaffController:**
```csharp
// Assign default permissions: Order.Manage và Customer.Manage
var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
var defaultPermissions = new[] { "Admin.Order.Manage", "Admin.Customer.Manage" };
foreach (var permissionName in defaultPermissions)
{
    await _authService.GrantPermissionAsync(user.UserId, permissionName, currentUserId);
}
```

### Cấp Thủ Công

Admin có thể cấp lại quyền mặc định cho nhân viên bất kỳ thông qua action `AssignDefaultPermissions`.

---

## 🔒 Security & Validation

### Authorization
- **Controller Level:**
  - `[RequirePermission("Admin.Permission.Manage")]` - Tất cả actions đều yêu cầu permission này

### Unique Validation
- **PermissionName:** Không được trùng với permission khác
- Validation được thực hiện trong:
  - Create action
  - Edit action (trừ permission hiện tại)

### Delete Validation
- **Không thể xóa nếu có user/role đang sử dụng:**
  - Kiểm tra `UserPermissions.Count(IsActive) > 0`
  - Kiểm tra `RolePermissions.Count > 0`
  - Nếu có → Hiển thị error và không cho phép xóa

### Permission Name Format
- **Regex:** `^[a-zA-Z0-9._]+$`
- Chỉ cho phép: chữ cái, số, dấu chấm, và underscore
- Ví dụ hợp lệ: `Admin.Order.Manage`, `Staff.Product.View`

### Category Format
- **Regex:** `^[a-zA-Z\s]+$`
- Chỉ cho phép: chữ cái và khoảng trắng
- Ví dụ hợp lệ: `Admin`, `Staff`, `User`

### Error Handling
- **Try-Catch:** Tất cả actions đều có try-catch
- **Logging:** Sử dụng `ILogger<StaffPermissionsController>` để log errors
- **User-Friendly Messages:** TempData messages cho success/error
- **ModelState Errors:** Hiển thị lỗi validation trong form

---

## 📊 Database Models

### Permission Model
```csharp
- PermissionId (int, PK)
- PermissionName (string, required, unique, max 100, regex: ^[a-zA-Z0-9._]+$)
- Description (string, nullable, max 255)
- Category (string, nullable, max 50, regex: ^[a-zA-Z\s]+$)
- IsActive (bool, default: true)
- CreatedAt (DateTime)
- UpdatedAt (DateTime)
- Navigation Properties:
  - RolePermissions (ICollection<RolePermission>)
  - UserPermissions (ICollection<UserPermission>)
```

### UserPermission Model
```csharp
- UserPermissionId (int, PK)
- UserId (int, FK → User)
- PermissionId (int, FK → Permission)
- GrantedBy (int?, FK → User, nullable)
- GrantedAt (DateTime)
- ExpiresAt (DateTime?, nullable)
- IsActive (bool, default: true)
- CreatedAt (DateTime)
- UpdatedAt (DateTime)
- Navigation Properties:
  - User (User, required)
  - Permission (Permission, required)
  - GrantedByUser (User?, nullable)
```

### Database Schema
```sql
CREATE TABLE Permissions (
    PermissionId INT PRIMARY KEY IDENTITY(1,1),
    PermissionName NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(255),
    Category NVARCHAR(50), -- Admin, Staff, User
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2(7) NOT NULL DEFAULT GETDATE()
);

CREATE TABLE UserPermissions (
    UserPermissionId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    PermissionId INT NOT NULL,
    GrantedBy INT, -- Admin nào cấp quyền
    GrantedAt DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    ExpiresAt DATETIME2(7), -- Quyền có hạn sử dụng
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2(7) NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE,
    FOREIGN KEY (PermissionId) REFERENCES Permissions(PermissionId) ON DELETE CASCADE,
    FOREIGN KEY (GrantedBy) REFERENCES Users(UserId),
    UNIQUE(UserId, PermissionId)
);
```

---

## 🛠️ Helper Methods & ViewModels

### PermissionExists
```csharp
private async Task<bool> PermissionExists(int id)
{
    return await _context.Permissions.AnyAsync(e => e.PermissionId == id);
}
```
- Kiểm tra permission có tồn tại
- Sử dụng trong Edit action để check concurrency

### StaffPermissionViewModel
```csharp
public class StaffPermissionViewModel
{
    public User User { get; set; } = null!;
    public List<string> Permissions { get; set; } = new List<string>();
}
```
- ViewModel để truyền dữ liệu nhân viên và permissions của họ
- Sử dụng trong Index action

---

## 📝 API Endpoints Summary

| Method | Route | Action | Mô Tả |
|--------|-------|--------|-------|
| GET | `/Admin/StaffPermissions` | Index | Danh sách nhân viên với permissions (có filter, search, pagination) |
| GET | `/Admin/StaffPermissions/Details/{id}` | Details | Xem chi tiết phân quyền của nhân viên |
| GET | `/Admin/StaffPermissions/Create` | Create | Form tạo permission mới |
| POST | `/Admin/StaffPermissions/Create` | Create | Tạo permission mới |
| GET | `/Admin/StaffPermissions/Edit/{id}` | Edit | Form sửa permission |
| POST | `/Admin/StaffPermissions/Edit/{id}` | Edit | Cập nhật permission |
| GET | `/Admin/StaffPermissions/Delete/{id}` | Delete | Xác nhận xóa permission |
| POST | `/Admin/StaffPermissions/Delete/{id}` | DeleteConfirmed | Xóa permission |
| POST | `/Admin/StaffPermissions/GrantPermission` | GrantPermission | Cấp quyền cho nhân viên |
| POST | `/Admin/StaffPermissions/RevokePermission` | RevokePermission | Thu hồi quyền của nhân viên |
| POST | `/Admin/StaffPermissions/AssignDefaultPermissions` | AssignDefaultPermissions | Cấp quyền mặc định cho nhân viên |

---

## 🔄 Workflow Sử Dụng

### Xem Danh Sách Phân Quyền
1. Admin vào **Admin → StaffPermissions**
2. Có thể:
   - **Tìm kiếm:** Nhập tên, username, email nhân viên
   - **Lọc:** (Có thể mở rộng)
3. Kết quả hiển thị:
   - Danh sách nhân viên
   - Permissions của mỗi nhân viên
   - Phân trang

### Xem Chi Tiết Phân Quyền
1. Từ danh sách, click **"Chi Tiết"** hoặc **"Xem"** trên nhân viên
2. Hiển thị:
   - Thông tin nhân viên
   - Danh sách permissions hiện tại (với thông tin chi tiết)
   - Danh sách permissions có thể cấp
   - Các nút: "Cấp Quyền", "Thu Hồi Quyền", "Cấp Quyền Mặc Định"

### Cấp Quyền Cho Nhân Viên
1. Từ trang Details, chọn permission cần cấp
2. Click **"Cấp Quyền"**
3. **Hệ thống tự động:**
   - Tạo UserPermission mới hoặc kích hoạt lại nếu đã có
   - Ghi lại GrantedBy (Admin cấp quyền)
   - Set GrantedAt = DateTime.UtcNow
4. Redirect về Details với success message

### Thu Hồi Quyền
1. Từ trang Details, chọn permission cần thu hồi
2. Click **"Thu Hồi Quyền"**
3. **Hệ thống tự động:**
   - Set `IsActive = false` cho UserPermission
   - Không xóa record
4. Redirect về Details với success message

### Cấp Quyền Mặc Định
1. Từ trang Details, click **"Cấp Quyền Mặc Định"**
2. **Hệ thống tự động:**
   - Cấp `Admin.Order.Manage`
   - Cấp `Admin.Customer.Manage`
3. Redirect về Details với success message

### Tạo Permission Mới
1. Admin vào **Admin → StaffPermissions → Create**
2. Điền thông tin:
   - PermissionName (required, unique, format: `^[a-zA-Z0-9._]+$`)
   - Description (optional)
   - Category (optional, format: `^[a-zA-Z\s]+$`)
   - IsActive (default: true)
3. Click **"Tạo Quyền"**
4. **Hệ thống tự động:**
   - Validation tên không trùng
   - Set IsActive = true
   - Set CreatedAt, UpdatedAt
5. Redirect đến danh sách với success message

### Sửa Permission
1. Từ danh sách permissions, click **"Sửa"**
2. Cập nhật thông tin (PermissionName, Description, Category, IsActive)
3. Click **"Lưu"**
4. Validation:
   - PermissionName không được trùng với permission khác
   - ModelState validation
5. Redirect đến danh sách với success message

### Xóa Permission
1. Từ danh sách permissions, click **"Xóa"**
2. Xác nhận thông tin permission
3. **Hệ thống kiểm tra:**
   - Số lượng user đang sử dụng permission này
   - Số lượng role đang sử dụng permission này
   - Nếu có → Hiển thị error, không cho phép xóa
4. Nếu không có:
   - Click **"Xác Nhận Xóa"**
   - Xóa permission
   - Cascade delete: UserPermissions và RolePermissions
5. Redirect đến danh sách với success message

---

## 📌 Lưu Ý Quan Trọng

### Default Permissions

**Khi tạo nhân viên mới:**
- Tự động cấp 2 quyền mặc định:
  - `Admin.Order.Manage` - Quản lý đơn hàng
  - `Admin.Customer.Manage` - Quản lý khách hàng
- Được thực hiện trong `StaffController.Create`

**Cấp lại quyền mặc định:**
- Admin có thể cấp lại quyền mặc định cho nhân viên bất kỳ
- Sử dụng action `AssignDefaultPermissions`

### Permission Name Format

- **Chỉ cho phép:** Chữ cái (a-z, A-Z), số (0-9), dấu chấm (.), và underscore (_)
- **Ví dụ hợp lệ:**
  - `Admin.Order.Manage`
  - `Admin.Customer.Manage`
  - `Staff.Product.View`
  - `User.Profile.Edit`
- **Ví dụ không hợp lệ:**
  - `Admin-Order-Manage` (có dấu gạch ngang)
  - `Admin Order Manage` (có khoảng trắng)
  - `Admin@Order#Manage` (có ký tự đặc biệt)

### Category Format

- **Chỉ cho phép:** Chữ cái (a-z, A-Z) và khoảng trắng
- **Ví dụ hợp lệ:**
  - `Admin`
  - `Staff`
  - `User`
  - `Admin Staff` (có khoảng trắng)

### Delete Protection

- **Không thể xóa nếu có user/role đang sử dụng:**
  - Hệ thống kiểm tra số lượng user và role trước khi xóa
  - Nếu có → Hiển thị error message với số lượng
  - Admin phải xóa hoặc chuyển các user/role trước

### Grant/Revoke Logic

- **Grant Permission:**
  - Nếu permission đã tồn tại nhưng inactive → Kích hoạt lại
  - Nếu chưa có → Tạo mới UserPermission
  - Ghi lại GrantedBy và GrantedAt

- **Revoke Permission:**
  - Không xóa record, chỉ set `IsActive = false`
  - Có thể cấp lại sau

### Permission Sources

Nhân viên có thể có permissions từ 2 nguồn:
1. **Role-based permissions:** Từ RolePermissions (thông qua Staff role)
2. **User-specific permissions:** Từ UserPermissions (cấp trực tiếp)

Hệ thống kiểm tra cả 2 nguồn khi verify permission.

---

## 🚀 Cải Tiến Có Thể Thêm (Future Enhancements)

1. **Bulk Grant/Revoke:**
   - Cấp/thu hồi quyền cho nhiều nhân viên cùng lúc
   - Bulk operations

2. **Permission Templates:**
   - Tạo template permissions cho các vị trí khác nhau
   - Áp dụng template cho nhân viên mới

3. **Permission Expiration:**
   - Thêm tính năng hết hạn quyền (ExpiresAt)
   - Tự động thu hồi quyền hết hạn
   - Notification trước khi hết hạn

4. **Permission History:**
   - Lịch sử cấp/thu hồi quyền
   - Audit log cho permissions

5. **Advanced Search:**
   - Tìm kiếm theo permission name
   - Filter theo category
   - Filter theo số lượng permissions

6. **Permission Groups:**
   - Nhóm permissions theo chức năng
   - Cấp cả nhóm permissions cùng lúc

7. **Role-based Permission Management:**
   - Quản lý permissions theo role
   - Cấp permissions cho role thay vì từng user

8. **Permission Inheritance:**
   - Permissions kế thừa từ role
   - Override permissions ở user level

9. **Export/Import:**
   - Export danh sách permissions ra Excel/CSV
   - Import permissions từ file

10. **Permission Statistics:**
    - Thống kê số lượng permissions theo category
    - Dashboard permissions

---

## 📄 Files Modified/Created

### Created
- `Areas/Admin/Controllers/StaffPermissionsController.cs` - Controller quản lý phân quyền với đầy đủ CRUD operations

### Modified
- `Areas/Admin/Controllers/StaffController.cs` - Thêm logic tự động cấp default permissions khi tạo nhân viên mới

### Models Used (Existing)
- `Models/Permission.cs`
- `Models/UserPermission.cs`
- `Models/User.cs`
- `Models/Role.cs`

### Services Used
- `Services/IAuthorizationService.cs` - Quản lý permissions
- `Data/ApplicationDbContext.cs` - Database context

---

## ✅ Testing Checklist

- [x] Index với search, filter, pagination
- [x] Details hiển thị đầy đủ thông tin permissions
- [x] Create với validation PermissionName unique và format
- [x] Edit với validation PermissionName unique (trừ permission hiện tại)
- [x] Delete với validation (không thể xóa nếu có user/role đang sử dụng)
- [x] GrantPermission với logic kích hoạt lại nếu đã có
- [x] RevokePermission với logic set IsActive = false
- [x] AssignDefaultPermissions cấp 2 quyền mặc định
- [x] Default permissions tự động cấp khi tạo nhân viên mới
- [x] Error handling và logging
- [x] TempData messages
- [x] Database schema compliance

---

**Tài liệu này mô tả đầy đủ hệ thống quản lý phân quyền cho nhân viên trong Admin area. Tất cả các tính năng đã được implement trong Controller, sẵn sàng cho việc tạo Views.**

