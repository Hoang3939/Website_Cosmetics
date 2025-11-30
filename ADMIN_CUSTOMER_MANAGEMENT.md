# 🛒 Admin Customer Management System

## 📋 Tổng quan

Hệ thống quản lý khách hàng trong Admin với đầy đủ tính năng CRUD, tìm kiếm, lọc, phân trang, và quản lý thông tin khách hàng.

**Ngày tạo:** 2025-01-XX  
**Branch:** `feature/admin-customer-management`

---

## ✨ Tính Năng Chính

### 1. Danh Sách Khách Hàng (Index)

#### Route
- **GET:** `/Admin/Customers`
- **GET với filter:** `/Admin/Customers?search={keyword}&status={active/inactive}&role={role}&page={page}&pageSize={size}`

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
  - `status=active` → Chỉ hiển thị khách hàng đang hoạt động
  - `status=inactive` → Chỉ hiển thị khách hàng đã vô hiệu hóa
- **Theo Role:**
  - `role={roleName}` → Lọc theo role cụ thể (ví dụ: `role=Customer`)

**Phân Trang:**
- Mặc định: 10 items/trang
- Có thể tùy chỉnh `pageSize`
- Hiển thị thông tin: CurrentPage, TotalPages, TotalCount

**Sắp Xếp:**
- Mặc định: Sắp xếp theo `CreatedAt` (mới nhất trước)

**Ví Dụ URL:**
```
/Admin/Customers                                    → Tất cả khách hàng
/Admin/Customers?search=nguyen                      → Tìm "nguyen"
/Admin/Customers?status=active                      → Chỉ active
/Admin/Customers?search=john&status=active          → Tìm "john" + active
/Admin/Customers?page=2&pageSize=20                → Trang 2, 20 items/trang
```

#### ViewBag Parameters
- `ViewBag.Search` - Từ khóa tìm kiếm hiện tại
- `ViewBag.Status` - Trạng thái filter hiện tại
- `ViewBag.Role` - Role filter hiện tại
- `ViewBag.CurrentPage` - Trang hiện tại
- `ViewBag.TotalPages` - Tổng số trang
- `ViewBag.TotalCount` - Tổng số khách hàng
- `ViewBag.PageSize` - Số items mỗi trang

---

### 2. Xem Chi Tiết Khách Hàng (Details)

#### Route
- **GET:** `/Admin/Customers/Details/{id}`

#### Chức Năng
- Hiển thị đầy đủ thông tin khách hàng:
  - **Thông tin cá nhân:**
    - Username, Email
    - FirstName, LastName, FullName
    - PhoneNumber
    - CreatedAt, UpdatedAt
  - **Trạng thái:**
    - IsActive (Active/Inactive)
    - IsEmailConfirmed
  - **Roles:**
    - Danh sách tất cả roles của khách hàng
  - **Địa chỉ:**
    - Danh sách địa chỉ giao hàng (UserAddresses)

#### Validation
- Kiểm tra user có tồn tại
- Kiểm tra user có role "Customer"
- Nếu không phải Customer → Return NotFound()

---

### 3. Tạo Khách Hàng Mới (Create)

#### Route
- **GET:** `/Admin/Customers/Create`
- **POST:** `/Admin/Customers/Create`

#### Chức Năng

**GET Action:**
- Hiển thị form tạo khách hàng mới

**POST Action:**
- **Validation:**
  - ModelState validation
  - Kiểm tra Username đã tồn tại → Error: "Username đã tồn tại. Vui lòng chọn username khác."
  - Kiểm tra Email đã tồn tại → Error: "Email đã tồn tại. Vui lòng sử dụng email khác."

- **Tự động:**
  - Tạo password mặc định: `Customer123!` (đã hash bằng BCrypt)
  - Set `IsEmailConfirmed = true` (Admin tạo nên auto-confirm)
  - Set `IsActive = true`
  - Set `CreatedAt = DateTime.UtcNow`
  - Set `UpdatedAt = DateTime.UtcNow`
  - Tự động assign role "Customer"

- **Success:**
  - TempData message: `"Khách hàng '{FullName}' đã được tạo thành công. Mật khẩu mặc định: Customer123!"`
  - Redirect đến Index

- **Error Handling:**
  - Try-catch với logging
  - ModelState error: `"Lỗi khi tạo khách hàng: {ex.Message}"`

#### Form Fields
- Username (required, unique)
- Email (required, unique, email format)
- FirstName (optional)
- LastName (optional)
- PhoneNumber (optional)

---

### 4. Sửa Thông Tin Khách Hàng (Edit)

#### Route
- **GET:** `/Admin/Customers/Edit/{id}`
- **POST:** `/Admin/Customers/Edit/{id}`

#### Chức Năng

**GET Action:**
- Load user với UserRoles
- Kiểm tra user có role "Customer"
- Return View với user data

**POST Action:**
- **Validation:**
  - Kiểm tra `id == model.UserId`
  - ModelState validation
  - Kiểm tra Username đã tồn tại (trừ user hiện tại) → Error
  - Kiểm tra Email đã tồn tại (trừ user hiện tại) → Error
  - Kiểm tra user có role "Customer"

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
  - TempData message: `"Thông tin khách hàng '{FullName}' đã được cập nhật thành công."`
  - Redirect đến Index

- **Error Handling:**
  - DbUpdateConcurrencyException → Kiểm tra CustomerExists
  - Exception → Logging + ModelState error

#### Lưu Ý
- **Không cập nhật password** trong Edit action
- Password được quản lý riêng (có thể thêm action ChangePassword sau)

---

### 5. Xóa Khách Hàng (Delete)

#### Route
- **GET:** `/Admin/Customers/Delete/{id}` (Xác nhận xóa)
- **POST:** `/Admin/Customers/Delete/{id}` (Xóa thực sự)

#### Chức Năng

**GET Action:**
- Hiển thị thông tin khách hàng để xác nhận
- Kiểm tra user có role "Customer"

**POST Action (DeleteConfirmed):**
- **Validation:**
  - Kiểm tra user tồn tại
  - Kiểm tra user có role "Customer"

- **Xóa:**
  - Xóa user → Cascade delete:
    - UserRoles
    - UserAddresses
    - Orders (nếu có)
    - OrderItems (nếu có)
    - Các bảng liên quan khác (theo database schema)

- **Success:**
  - TempData message: `"Khách hàng '{FullName}' đã được xóa thành công."`
  - Redirect đến Index

- **Error Handling:**
  - Exception → Logging + TempData error message
  - Redirect đến Index (không throw exception)

---

### 6. Kích Hoạt / Vô Hiệu Hóa Khách Hàng

#### Route
- **POST:** `/Admin/Customers/Activate/{id}`
- **POST:** `/Admin/Customers/Deactivate/{id}`

#### Chức Năng

**Activate:**
- Set `IsActive = true`
- Set `UpdatedAt = DateTime.UtcNow`
- TempData: `"Khách hàng '{FullName}' đã được kích hoạt thành công."`

**Deactivate:**
- Set `IsActive = false`
- Set `UpdatedAt = DateTime.UtcNow`
- TempData: `"Khách hàng '{FullName}' đã được vô hiệu hóa thành công."`

**Validation:**
- Kiểm tra user tồn tại
- Kiểm tra user có role "Customer"
- Nếu không phải Customer → Return NotFound()

**Error Handling:**
- Try-catch với logging
- TempData error message nếu có lỗi

---

## 🔒 Security & Validation

### Authorization
- **Controller Level:**
  - `[RequirePermission("Admin.Customer.Manage")]` - Tất cả actions đều yêu cầu permission này

### Customer Role Validation
- **Tất cả actions** đều kiểm tra user có role "Customer":
  ```csharp
  var hasCustomerRole = user.UserRoles.Any(ur => ur.Role.RoleName == "Customer");
  if (!hasCustomerRole)
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
- **Logging:** Sử dụng `ILogger<CustomersController>` để log errors
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
  - UserAddresses (ICollection<UserAddress>)
  - Orders (ICollection<Order>) - nếu có
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

### UserAddress Model
```csharp
- AddressId (int, PK)
- UserId (int, FK → User)
- FullName (string)
- PhoneNumber (string, nullable)
- AddressLine1 (string)
- AddressLine2 (string, nullable)
- City (string)
- State (string, nullable)
- PostalCode (string, nullable)
- Country (string)
- IsDefault (bool)
- CreatedAt (DateTime)
- UpdatedAt (DateTime)
```

---

## 🛠️ Helper Methods

### CustomerExists
```csharp
private async Task<bool> CustomerExists(int id)
{
    return await _context.Users
        .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
        .AnyAsync(u => u.UserId == id && u.UserRoles.Any(ur => ur.Role.RoleName == "Customer"));
}
```
- Kiểm tra user có tồn tại và có role "Customer"
- Sử dụng trong Edit action để check concurrency

---

## 📝 API Endpoints Summary

| Method | Route | Action | Mô Tả |
|--------|-------|--------|-------|
| GET | `/Admin/Customers` | Index | Danh sách khách hàng (có filter, search, pagination) |
| GET | `/Admin/Customers/Details/{id}` | Details | Xem chi tiết khách hàng |
| GET | `/Admin/Customers/Create` | Create | Form tạo khách hàng mới |
| POST | `/Admin/Customers/Create` | Create | Tạo khách hàng mới |
| GET | `/Admin/Customers/Edit/{id}` | Edit | Form sửa thông tin |
| POST | `/Admin/Customers/Edit/{id}` | Edit | Cập nhật thông tin |
| GET | `/Admin/Customers/Delete/{id}` | Delete | Xác nhận xóa |
| POST | `/Admin/Customers/Delete/{id}` | DeleteConfirmed | Xóa khách hàng |
| POST | `/Admin/Customers/Activate/{id}` | Activate | Kích hoạt khách hàng |
| POST | `/Admin/Customers/Deactivate/{id}` | Deactivate | Vô hiệu hóa khách hàng |

---

## 🔄 Workflow Sử Dụng

### Tạo Khách Hàng Mới
1. Admin vào **Admin → Customers → Create**
2. Điền thông tin:
   - Username (unique)
   - Email (unique)
   - FirstName, LastName (optional)
   - PhoneNumber (optional)
3. Click **"Tạo Khách Hàng"**
4. **Hệ thống tự động:**
   - Tạo user với password mặc định: `Customer123!`
   - Assign role "Customer"
   - Auto-confirm email
   - Set IsActive = true
5. Redirect đến danh sách khách hàng
6. **Khách hàng mới có thể đăng nhập với:**
   - Username: {username}
   - Password: `Customer123!`
   - **Nên đổi password ngay sau lần đăng nhập đầu tiên**

### Sửa Thông Tin Khách Hàng
1. Từ danh sách, click **"Sửa"** trên khách hàng cần sửa
2. Cập nhật thông tin (Username, Email, Họ tên, SĐT, Trạng thái)
3. Click **"Lưu"**
4. Validation:
   - Username/Email không được trùng với user khác
   - ModelState validation
5. Redirect đến danh sách với success message

### Xóa Khách Hàng
1. Từ danh sách, click **"Xóa"** trên khách hàng cần xóa
2. Xác nhận thông tin khách hàng
3. Click **"Xác Nhận Xóa"**
4. **Hệ thống tự động:**
   - Xóa user
   - Cascade delete: UserRoles, UserAddresses, Orders, OrderItems
5. Redirect đến danh sách với success message

### Tìm Kiếm & Lọc
1. Từ danh sách khách hàng:
   - **Tìm kiếm:** Nhập từ khóa vào ô search (tìm theo tên, username, email)
   - **Lọc trạng thái:** Chọn "Active" hoặc "Inactive"
   - **Lọc role:** Chọn role cụ thể (nếu có nhiều role)
2. Click **"Tìm Kiếm"** hoặc **"Lọc"**
3. Kết quả được hiển thị với phân trang

---

## 📌 Lưu Ý Quan Trọng

### Password Management
- **Mật khẩu mặc định:** `Customer123!` (khi tạo mới)
- **Không có chức năng đổi password trong Edit action**
- **Nên implement ChangePassword action riêng** (có thể thêm sau)
- **Khách hàng nên đổi password ngay sau lần đăng nhập đầu tiên**

### Cascade Delete
- Khi xóa khách hàng, các bảng liên quan sẽ tự động xóa:
  - UserRoles
  - UserAddresses
  - Orders (nếu có)
  - OrderItems (nếu có)
  - (Theo database schema cascade rules)

### Customer Role Requirement
- **Tất cả actions chỉ hoạt động với users có role "Customer"**
- Nếu user không có role "Customer" → Return NotFound()
- Điều này đảm bảo chỉ quản lý khách hàng, không ảnh hưởng đến staff hoặc admin

### Filter & Search Combination
- **Có thể kết hợp nhiều filters cùng lúc:**
  - Search + Status
  - Search + Role
  - Status + Role
  - Search + Status + Role
- **Ví dụ:** `/Admin/Customers?search=nguyen&status=active&role=Customer`

### Pagination
- **Mặc định:** 10 items/trang
- **Có thể tùy chỉnh:** `?pageSize=20`
- **Validation:** Page number được validate (không < 1, không > TotalPages)

### Khác Biệt Với Staff Management
- **Không có quản lý Permissions:** Khách hàng không có permissions như staff
- **Tập trung vào thông tin cá nhân:** Quản lý thông tin đăng nhập, địa chỉ giao hàng
- **Có thể mở rộng:** Thêm quản lý Orders, Order History, Wishlist, etc.

---

## 🚀 Cải Tiến Có Thể Thêm (Future Enhancements)

1. **Change Password Action:**
   - Cho phép admin đổi password cho khách hàng
   - Hoặc cho phép khách hàng tự đổi password

2. **Order Management:**
   - Xem lịch sử đơn hàng của khách hàng
   - Quản lý đơn hàng từ trang khách hàng

3. **Address Management:**
   - Quản lý địa chỉ giao hàng từ admin
   - Thêm/sửa/xóa địa chỉ

4. **Bulk Actions:**
   - Bulk activate/deactivate
   - Bulk delete (với confirmation)
   - Bulk export

5. **Export/Import:**
   - Export danh sách khách hàng ra Excel/CSV
   - Import khách hàng từ file

6. **Advanced Search:**
   - Tìm kiếm theo từng field riêng (Username, Email, FirstName, LastName)
   - Date range filter (CreatedAt, UpdatedAt)
   - Filter theo số lượng đơn hàng

7. **Customer Statistics:**
   - Tổng số đơn hàng
   - Tổng giá trị đơn hàng
   - Đơn hàng gần nhất
   - Sản phẩm yêu thích

8. **Wishlist Management:**
   - Xem danh sách sản phẩm yêu thích của khách hàng

9. **Communication History:**
   - Lịch sử liên hệ với khách hàng
   - Ghi chú về khách hàng

---

## 📄 Files Modified/Created

### Created
- `Areas/Admin/Controllers/CustomersController.cs` - Controller quản lý khách hàng với đầy đủ CRUD operations

### Models Used (Existing)
- `Models/User.cs`
- `Models/Role.cs`
- `Models/UserRole.cs`
- `Models/UserAddress.cs`

### Services Used
- `Data/ApplicationDbContext.cs` - Database context

---

## ✅ Testing Checklist

- [x] Index với search, filter, pagination
- [x] Details hiển thị đầy đủ thông tin
- [x] Create với validation username/email unique
- [x] Edit với validation username/email unique (trừ user hiện tại)
- [x] Delete với cascade delete
- [x] Activate/Deactivate
- [x] Customer role validation trong tất cả actions
- [x] Error handling và logging
- [x] TempData messages

---

**Tài liệu này mô tả đầy đủ hệ thống quản lý khách hàng trong Admin area. Tất cả các tính năng đã được implement trong Controller, sẵn sàng cho việc tạo Views.**

