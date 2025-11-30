# 👥 Admin Staff Management System

## 📋 Tổng quan

Hệ thống quản lý nhân viên trong Admin với đầy đủ tính năng CRUD, tìm kiếm, lọc, phân trang, và quản lý quyền truy cập.

**Ngày tạo:** 2025-01-XX  
**Branch:** `feature/admin-staff-management`

---

## ✨ Tính Năng Chính

### 1. Danh Sách Nhân Viên (Index)

#### Route
- **GET:** `/Admin/Staff`
- **GET với filter:** `/Admin/Staff?search={keyword}&status={active/inactive}&role={role}&page={page}&pageSize={size}`

#### Chức Năng

**Tìm Kiếm (Search):**
- Tìm kiếm theo:
  - Username
  - Email
  - FirstName
  - LastName
  - FullName (FirstName + LastName)
- Không phân biệt hoa thường
- Tìm kiếm partial match (Contains)

**Lọc (Filter):**
- **Theo trạng thái:**
  - `status=active` → Chỉ hiển thị nhân viên đang hoạt động
  - `status=inactive` → Chỉ hiển thị nhân viên đã vô hiệu hóa
- **Theo Role:**
  - `role={roleName}` → Lọc theo role cụ thể (ví dụ: `role=Staff`)

**Phân Trang:**
- Mặc định: 10 items/trang
- Có thể tùy chỉnh `pageSize`
- Hiển thị thông tin: CurrentPage, TotalPages, TotalCount

**Sắp Xếp:**
- Mặc định: Sắp xếp theo `CreatedAt` (mới nhất trước)

**Ví Dụ URL:**
```
/Admin/Staff                                    → Tất cả nhân viên
/Admin/Staff?search=nguyen                      → Tìm "nguyen"
/Admin/Staff?status=active                      → Chỉ active
/Admin/Staff?search=admin&status=active         → Tìm "admin" + active
/Admin/Staff?page=2&pageSize=20                 → Trang 2, 20 items/trang
```

#### ViewBag Parameters
- `ViewBag.Search` - Từ khóa tìm kiếm hiện tại
- `ViewBag.Status` - Trạng thái filter hiện tại
- `ViewBag.Role` - Role filter hiện tại
- `ViewBag.CurrentPage` - Trang hiện tại
- `ViewBag.TotalPages` - Tổng số trang
- `ViewBag.TotalCount` - Tổng số nhân viên
- `ViewBag.PageSize` - Số items mỗi trang

---

### 2. Xem Chi Tiết Nhân Viên (Details)

#### Route
- **GET:** `/Admin/Staff/Details/{id}`

#### Chức Năng
- Hiển thị đầy đủ thông tin nhân viên:
  - **Thông tin cá nhân:**
    - Username, Email
    - FirstName, LastName, FullName
    - PhoneNumber
    - CreatedAt, UpdatedAt
  - **Trạng thái:**
    - IsActive (Active/Inactive)
    - IsEmailConfirmed
  - **Roles:**
    - Danh sách tất cả roles của nhân viên
  - **Permissions:**
    - Danh sách tất cả permissions được cấp
  - **Địa chỉ:**
    - Danh sách địa chỉ (nếu có)

#### Validation
- Kiểm tra user có tồn tại
- Kiểm tra user có role "Staff"
- Nếu không phải Staff → Return NotFound()

---

### 3. Tạo Nhân Viên Mới (Create)

#### Route
- **GET:** `/Admin/Staff/Create`
- **POST:** `/Admin/Staff/Create`

#### Chức Năng

**GET Action:**
- Hiển thị form tạo nhân viên mới

**POST Action:**
- **Validation:**
  - ModelState validation
  - Kiểm tra Username đã tồn tại → Error: "Username đã tồn tại. Vui lòng chọn username khác."
  - Kiểm tra Email đã tồn tại → Error: "Email đã tồn tại. Vui lòng sử dụng email khác."

- **Tự động:**
  - Tạo password mặc định: `Staff123!` (đã hash bằng BCrypt)
  - Set `IsEmailConfirmed = true` (Admin tạo nên auto-confirm)
  - Set `IsActive = true`
  - Set `CreatedAt = DateTime.UtcNow`
  - Set `UpdatedAt = DateTime.UtcNow`
  - Tự động assign role "Staff"

- **Success:**
  - TempData message: `"Nhân viên '{FullName}' đã được tạo thành công. Mật khẩu mặc định: Staff123!"`
  - Redirect đến Index

- **Error Handling:**
  - Try-catch với logging
  - ModelState error: `"Lỗi khi tạo nhân viên: {ex.Message}"`

#### Form Fields
- Username (required, unique)
- Email (required, unique, email format)
- FirstName (optional)
- LastName (optional)
- PhoneNumber (optional)

---

### 4. Sửa Thông Tin Nhân Viên (Edit)

#### Route
- **GET:** `/Admin/Staff/Edit/{id}`
- **POST:** `/Admin/Staff/Edit/{id}`

#### Chức Năng

**GET Action:**
- Load user với UserRoles
- Kiểm tra user có role "Staff"
- Return View với user data

**POST Action:**
- **Validation:**
  - Kiểm tra `id == model.UserId`
  - ModelState validation
  - Kiểm tra Username đã tồn tại (trừ user hiện tại) → Error
  - Kiểm tra Email đã tồn tại (trừ user hiện tại) → Error
  - Kiểm tra user có role "Staff"

- **Cập Nhật:**
  - Username
  - Email
  - FirstName
  - LastName
  - PhoneNumber
  - IsActive
  - IsEmailConfirmed
  - UpdatedAt = DateTime.UtcNow

- **Success:**
  - TempData message: `"Thông tin nhân viên '{FullName}' đã được cập nhật thành công."`
  - Redirect đến Index

- **Error Handling:**
  - DbUpdateConcurrencyException → Kiểm tra StaffExists
  - Exception → Logging + ModelState error

#### Lưu Ý
- **Không cập nhật password** trong Edit action
- Password được quản lý riêng (có thể thêm action ChangePassword sau)

---

### 5. Xóa Nhân Viên (Delete)

#### Route
- **GET:** `/Admin/Staff/Delete/{id}` (Xác nhận xóa)
- **POST:** `/Admin/Staff/Delete/{id}` (Xóa thực sự)

#### Chức Năng

**GET Action:**
- Hiển thị thông tin nhân viên để xác nhận
- Kiểm tra user có role "Staff"

**POST Action (DeleteConfirmed):**
- **Validation:**
  - Kiểm tra user tồn tại
  - Kiểm tra user có role "Staff"

- **Xóa:**
  - Xóa user → Cascade delete:
    - UserRoles
    - UserPermissions
    - UserAddresses
    - Các bảng liên quan khác (theo database schema)

- **Success:**
  - TempData message: `"Nhân viên '{FullName}' đã được xóa thành công."`
  - Redirect đến Index

- **Error Handling:**
  - Exception → Logging + TempData error message
  - Redirect đến Index (không throw exception)

---

### 6. Kích Hoạt / Vô Hiệu Hóa Nhân Viên

#### Route
- **POST:** `/Admin/Staff/Activate/{id}`
- **POST:** `/Admin/Staff/Deactivate/{id}`

#### Chức Năng

**Activate:**
- Set `IsActive = true`
- Set `UpdatedAt = DateTime.UtcNow`
- TempData: `"Nhân viên '{FullName}' đã được kích hoạt thành công."`

**Deactivate:**
- Set `IsActive = false`
- Set `UpdatedAt = DateTime.UtcNow`
- TempData: `"Nhân viên '{FullName}' đã được vô hiệu hóa thành công."`

**Validation:**
- Kiểm tra user tồn tại
- Kiểm tra user có role "Staff"
- Nếu không phải Staff → Return NotFound()

**Error Handling:**
- Try-catch với logging
- TempData error message nếu có lỗi

---

### 7. Quản Lý Quyền (Permissions)

#### Route
- **GET:** `/Admin/Staff/Permissions/{id}`
- **POST:** `/Admin/Staff/GrantPermission`
- **POST:** `/Admin/Staff/RevokePermission`

#### Chức Năng

**Permissions (GET):**
- Hiển thị danh sách permissions hiện tại của nhân viên
- Hiển thị danh sách tất cả permissions có thể cấp (Category = "Staff")
- ViewBag:
  - `ViewBag.User` - User object
  - `ViewBag.UserPermissions` - List permissions của user
  - `ViewBag.AllPermissions` - List tất cả permissions

**GrantPermission (POST):**
- Cấp quyền cho nhân viên
- Sử dụng `IAuthorizationService.GrantPermissionAsync()`
- TempData success/error message

**RevokePermission (POST):**
- Thu hồi quyền của nhân viên
- Sử dụng `IAuthorizationService.RevokePermissionAsync()`
- TempData success/error message

---

## 🔒 Security & Validation

### Authorization
- **Controller Level:**
  - `[RequirePermission("Admin.Staff.Manage")]` - Tất cả actions đều yêu cầu permission này

### Staff Role Validation
- **Tất cả actions** đều kiểm tra user có role "Staff":
  ```csharp
  var hasStaffRole = user.UserRoles.Any(ur => ur.Role.RoleName == "Staff");
  if (!hasStaffRole)
  {
      return NotFound();
  }
  ```

### Unique Validation
- **Username:** Không được trùng với user khác
- **Email:** Không được trùng với user khác
- Validation được thực hiện trong:
  - Create action
  - Edit action (trừ user hiện tại)

### Error Handling
- **Try-Catch:** Tất cả actions đều có try-catch
- **Logging:** Sử dụng `ILogger<StaffController>` để log errors
- **User-Friendly Messages:** TempData messages cho success/error
- **ModelState Errors:** Hiển thị lỗi validation trong form

---

## 📊 Database Models

### User Model
```csharp
- UserId (int, PK)
- Username (string, unique)
- Email (string, unique)
- PasswordHash (string)
- FirstName (string, nullable)
- LastName (string, nullable)
- PhoneNumber (string, nullable)
- IsActive (bool)
- IsEmailConfirmed (bool)
- CreatedAt (DateTime)
- UpdatedAt (DateTime)
- Navigation Properties:
  - UserRoles (ICollection<UserRole>)
  - UserPermissions (ICollection<UserPermission>)
  - UserAddresses (ICollection<UserAddress>)
```

### Role Model
```csharp
- RoleId (int, PK)
- RoleName (string, unique)
- Description (string, nullable)
- IsActive (bool)
```

### UserRole Model
```csharp
- UserId (int, FK → User)
- RoleId (int, FK → Role)
- CreatedAt (DateTime)
- UpdatedAt (DateTime)
```

### Permission Model
```csharp
- PermissionId (int, PK)
- PermissionName (string, unique)
- Category (string)
- Description (string, nullable)
- IsActive (bool)
```

---

## 🛠️ Helper Methods

### StaffExists
```csharp
private async Task<bool> StaffExists(int id)
{
    return await _context.Users
        .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
        .AnyAsync(u => u.UserId == id && u.UserRoles.Any(ur => ur.Role.RoleName == "Staff"));
}
```
- Kiểm tra user có tồn tại và có role "Staff"
- Sử dụng trong Edit action để check concurrency

---

## 📝 API Endpoints Summary

| Method | Route | Action | Mô Tả |
|--------|-------|--------|-------|
| GET | `/Admin/Staff` | Index | Danh sách nhân viên (có filter, search, pagination) |
| GET | `/Admin/Staff/Details/{id}` | Details | Xem chi tiết nhân viên |
| GET | `/Admin/Staff/Create` | Create | Form tạo nhân viên mới |
| POST | `/Admin/Staff/Create` | Create | Tạo nhân viên mới |
| GET | `/Admin/Staff/Edit/{id}` | Edit | Form sửa thông tin |
| POST | `/Admin/Staff/Edit/{id}` | Edit | Cập nhật thông tin |
| GET | `/Admin/Staff/Delete/{id}` | Delete | Xác nhận xóa |
| POST | `/Admin/Staff/Delete/{id}` | DeleteConfirmed | Xóa nhân viên |
| POST | `/Admin/Staff/Activate/{id}` | Activate | Kích hoạt nhân viên |
| POST | `/Admin/Staff/Deactivate/{id}` | Deactivate | Vô hiệu hóa nhân viên |
| GET | `/Admin/Staff/Permissions/{id}` | Permissions | Quản lý quyền |
| POST | `/Admin/Staff/GrantPermission` | GrantPermission | Cấp quyền |
| POST | `/Admin/Staff/RevokePermission` | RevokePermission | Thu hồi quyền |

---

## 🔄 Workflow Sử Dụng

### Tạo Nhân Viên Mới
1. Admin vào **Admin → Staff → Create**
2. Điền thông tin:
   - Username (unique)
   - Email (unique)
   - FirstName, LastName (optional)
   - PhoneNumber (optional)
3. Click **"Tạo Nhân Viên"**
4. **Hệ thống tự động:**
   - Tạo user với password mặc định: `Staff123!`
   - Assign role "Staff"
   - Auto-confirm email
   - Set IsActive = true
5. Redirect đến danh sách nhân viên
6. **Nhân viên mới có thể đăng nhập với:**
   - Username: {username}
   - Password: `Staff123!`
   - **Nên đổi password ngay sau lần đăng nhập đầu tiên**

### Sửa Thông Tin Nhân Viên
1. Từ danh sách, click **"Sửa"** trên nhân viên cần sửa
2. Cập nhật thông tin (Username, Email, Họ tên, SĐT, Trạng thái)
3. Click **"Lưu"**
4. Validation:
   - Username/Email không được trùng với user khác
   - ModelState validation
5. Redirect đến danh sách với success message

### Xóa Nhân Viên
1. Từ danh sách, click **"Xóa"** trên nhân viên cần xóa
2. Xác nhận thông tin nhân viên
3. Click **"Xác Nhận Xóa"**
4. **Hệ thống tự động:**
   - Xóa user
   - Cascade delete: UserRoles, UserPermissions, UserAddresses
5. Redirect đến danh sách với success message

### Tìm Kiếm & Lọc
1. Từ danh sách nhân viên:
   - **Tìm kiếm:** Nhập từ khóa vào ô search (tìm theo tên, username, email)
   - **Lọc trạng thái:** Chọn "Active" hoặc "Inactive"
   - **Lọc role:** Chọn role cụ thể (nếu có nhiều role)
2. Click **"Tìm Kiếm"** hoặc **"Lọc"**
3. Kết quả được hiển thị với phân trang

### Quản Lý Quyền
1. Từ danh sách hoặc chi tiết, click **"Quản Lý Quyền"**
2. Xem danh sách permissions hiện tại
3. **Cấp quyền:** Click "Grant" trên permission chưa có
4. **Thu hồi quyền:** Click "Revoke" trên permission đã có
5. Redirect về trang permissions với success/error message

---

## 📌 Lưu Ý Quan Trọng

### Password Management
- **Mật khẩu mặc định:** `Staff123!` (khi tạo mới)
- **Không có chức năng đổi password trong Edit action**
- **Nên implement ChangePassword action riêng** (có thể thêm sau)
- **Nhân viên nên đổi password ngay sau lần đăng nhập đầu tiên**

### Cascade Delete
- Khi xóa nhân viên, các bảng liên quan sẽ tự động xóa:
  - UserRoles
  - UserPermissions
  - UserAddresses
  - (Theo database schema cascade rules)

### Staff Role Requirement
- **Tất cả actions chỉ hoạt động với users có role "Staff"**
- Nếu user không có role "Staff" → Return NotFound()
- Điều này đảm bảo chỉ quản lý nhân viên, không ảnh hưởng đến customers

### Filter & Search Combination
- **Có thể kết hợp nhiều filters cùng lúc:**
  - Search + Status
  - Search + Role
  - Status + Role
  - Search + Status + Role
- **Ví dụ:** `/Admin/Staff?search=nguyen&status=active&role=Staff`

### Pagination
- **Mặc định:** 10 items/trang
- **Có thể tùy chỉnh:** `?pageSize=20`
- **Validation:** Page number được validate (không < 1, không > TotalPages)

---

## 🚀 Cải Tiến Có Thể Thêm (Future Enhancements)

1. **Change Password Action:**
   - Cho phép admin đổi password cho nhân viên
   - Hoặc cho phép nhân viên tự đổi password

2. **Bulk Actions:**
   - Bulk activate/deactivate
   - Bulk delete (với confirmation)
   - Bulk assign roles/permissions

3. **Export/Import:**
   - Export danh sách nhân viên ra Excel/CSV
   - Import nhân viên từ file

4. **Advanced Search:**
   - Tìm kiếm theo từng field riêng (Username, Email, FirstName, LastName)
   - Date range filter (CreatedAt, UpdatedAt)
   - Multiple role filter

5. **Activity Log:**
   - Log các thay đổi của nhân viên
   - Xem lịch sử hoạt động

6. **Avatar Upload:**
   - Cho phép upload avatar cho nhân viên
   - Hiển thị avatar trong danh sách và chi tiết

---

## 📄 Files Modified/Created

### Created
- `Areas/Admin/Controllers/StaffController.cs` - Controller đã được cập nhật với đầy đủ CRUD operations

### Models Used (Existing)
- `Models/User.cs`
- `Models/Role.cs`
- `Models/UserRole.cs`
- `Models/Permission.cs`

### Services Used
- `Services/IAuthorizationService.cs` - Quản lý permissions
- `Data/ApplicationDbContext.cs` - Database context

---

## ✅ Testing Checklist

- [x] Index với search, filter, pagination
- [x] Details hiển thị đầy đủ thông tin
- [x] Create với validation username/email unique
- [x] Edit với validation username/email unique (trừ user hiện tại)
- [x] Delete với cascade delete
- [x] Activate/Deactivate
- [x] Permissions management
- [x] Staff role validation trong tất cả actions
- [x] Error handling và logging
- [x] TempData messages

---

**Tài liệu này mô tả đầy đủ hệ thống quản lý nhân viên trong Admin area. Tất cả các tính năng đã được implement trong Controller, sẵn sàng cho việc tạo Views.**

