# 🏷️ Admin Brand Management System

## 📋 Tổng quan

Hệ thống quản lý thương hiệu sản phẩm trong Admin với đầy đủ tính năng CRUD, tìm kiếm, lọc, phân trang, và quản lý thương hiệu sản phẩm.

**Ngày tạo:** 2025-01-XX  
**Branch:** `feature/admin-brand-management`

---

## ✨ Tính Năng Chính

### 1. Danh Sách Thương Hiệu (Index)

#### Route
- **GET:** `/Admin/Brands`
- **GET với filter:** `/Admin/Brands?search={keyword}&status={active/inactive}&page={page}&pageSize={size}`

#### Chức Năng

**Tìm Kiếm (Search):**
- Tìm kiếm theo:
  - Name (Tên thương hiệu)
  - Country (Quốc gia)
- Không phân biệt hoa thường
- Tìm kiếm partial match (Contains)

**Lọc (Filter):**
- **Theo trạng thái:**
  - `status=active` → Chỉ hiển thị thương hiệu đang hoạt động
  - `status=inactive` → Chỉ hiển thị thương hiệu đã vô hiệu hóa

**Phân Trang:**
- Mặc định: 10 items/trang
- Có thể tùy chỉnh `pageSize`
- Hiển thị thông tin: CurrentPage, TotalPages, TotalCount

**Sắp Xếp:**
- Mặc định: Sắp xếp theo `CreatedAt` (mới nhất trước)

**Ví Dụ URL:**
```
/Admin/Brands                                        → Tất cả thương hiệu
/Admin/Brands?search=HERA                            → Tìm "HERA"
/Admin/Brands?status=active                          → Chỉ active
/Admin/Brands?search=Korea&status=active            → Tìm "Korea" + active
/Admin/Brands?page=2&pageSize=20                    → Trang 2, 20 items/trang
```

#### ViewBag Parameters
- `ViewBag.Search` - Từ khóa tìm kiếm hiện tại
- `ViewBag.Status` - Trạng thái filter hiện tại
- `ViewBag.CurrentPage` - Trang hiện tại
- `ViewBag.TotalPages` - Tổng số trang
- `ViewBag.TotalCount` - Tổng số thương hiệu
- `ViewBag.PageSize` - Số items mỗi trang

---

### 2. Xem Chi Tiết Thương Hiệu (Details)

#### Route
- **GET:** `/Admin/Brands/Details/{id}`

#### Chức Năng
- Hiển thị đầy đủ thông tin thương hiệu:
  - **Thông tin thương hiệu:**
    - BrandId
    - Name (Tên thương hiệu)
    - Country (Quốc gia)
    - IsActive (Trạng thái hoạt động)
    - CreatedAt, UpdatedAt
  - **Danh sách sản phẩm:**
    - Products (Tất cả sản phẩm thuộc thương hiệu này)
    - Số lượng sản phẩm

#### Validation
- Kiểm tra brand có tồn tại
- Nếu không tồn tại → Return NotFound()

---

### 3. Tạo Thương Hiệu Mới (Create)

#### Route
- **GET:** `/Admin/Brands/Create`
- **POST:** `/Admin/Brands/Create`

#### Chức Năng

**GET Action:**
- Hiển thị form tạo thương hiệu mới

**POST Action:**
- **Validation:**
  - ModelState validation
  - Kiểm tra Name đã tồn tại → Error: "Tên thương hiệu đã tồn tại. Vui lòng chọn tên khác."

- **Tự động:**
  - Set `IsActive = true` nếu null
  - Set `CreatedAt = DateTime.UtcNow`
  - Set `UpdatedAt = DateTime.UtcNow`

- **Success:**
  - TempData message: `"Thương hiệu '{Name}' đã được tạo thành công."`
  - Redirect đến Index

- **Error Handling:**
  - Try-catch với logging
  - ModelState error: `"Lỗi khi tạo thương hiệu: {ex.Message}"`

#### Form Fields
- Name (required, unique, max 200 characters)
- Country (optional, max 100 characters)
- IsActive (optional, default: true)

---

### 4. Sửa Thương Hiệu (Edit)

#### Route
- **GET:** `/Admin/Brands/Edit/{id}`
- **POST:** `/Admin/Brands/Edit/{id}`

#### Chức Năng

**GET Action:**
- Load brand hiện tại
- Return View với brand data

**POST Action:**
- **Validation:**
  - Kiểm tra `id == brand.BrandId`
  - ModelState validation
  - Kiểm tra Name đã tồn tại (trừ brand hiện tại) → Error

- **Cập Nhật:**
  - Name
  - Country
  - IsActive (set = true nếu null)
  - UpdatedAt = DateTime.UtcNow

- **Success:**
  - TempData message: `"Thương hiệu '{Name}' đã được cập nhật thành công."`
  - Redirect đến Index

- **Error Handling:**
  - DbUpdateConcurrencyException → Kiểm tra BrandExists
  - Exception → Logging + ModelState error

---

### 5. Xóa Thương Hiệu (Delete)

#### Route
- **GET:** `/Admin/Brands/Delete/{id}` (Xác nhận xóa)
- **POST:** `/Admin/Brands/Delete/{id}` (Xóa thực sự)

#### Chức Năng

**GET Action:**
- Hiển thị thông tin thương hiệu để xác nhận
- **Kiểm tra số lượng sản phẩm:**
  - Load Products của brand
  - Hiển thị số lượng sản phẩm đang sử dụng thương hiệu này
  - ViewBag.ProductCount = số lượng sản phẩm

**POST Action (DeleteConfirmed):**
- **Validation:**
  - Kiểm tra brand tồn tại
  - **Kiểm tra có sản phẩm đang sử dụng:**
    - Nếu có sản phẩm → Không cho phép xóa
    - Error message: `"Không thể xóa thương hiệu '{Name}' vì có {count} sản phẩm đang sử dụng thương hiệu này. Vui lòng xóa hoặc chuyển các sản phẩm trước."`
    - Redirect về Delete page

- **Xóa:**
  - Xóa brand
  - **Lưu ý:** Theo database schema, `Product.BrandId` có `ON DELETE SET NULL`, nên khi xóa brand, các sản phẩm sẽ tự động set `BrandId = NULL` (không bị xóa)

- **Success:**
  - TempData message: `"Thương hiệu '{Name}' đã được xóa thành công."`
  - Redirect đến Index

- **Error Handling:**
  - Exception → Logging + TempData error message
  - Redirect đến Index (không throw exception)

---

## 🔒 Security & Validation

### Authorization
- **Controller Level:**
  - `[RequirePermission("Admin.Brand.Manage")]` - Tất cả actions đều yêu cầu permission này

### Unique Validation
- **Name:** Không được trùng với brand khác
- Validation được thực hiện trong:
  - Create action
  - Edit action (trừ brand hiện tại)

### Delete Validation
- **Không thể xóa nếu có sản phẩm:** Kiểm tra `Products.Count > 0`
- Nếu có sản phẩm → Hiển thị error và không cho phép xóa
- Admin phải xóa hoặc chuyển các sản phẩm trước khi xóa thương hiệu

### Error Handling
- **Try-Catch:** Tất cả actions đều có try-catch
- **Logging:** Sử dụng `ILogger<BrandsController>` để log errors
- **User-Friendly Messages:** TempData messages cho success/error
- **ModelState Errors:** Hiển thị lỗi validation trong form

---

## 📊 Database Models

### Brand Model
```csharp
- BrandId (int, PK)
- Name (string, required, max 200)
- Country (string, nullable, max 100)
- IsActive (bool, nullable, default: true)
- CreatedAt (DateTime)
- UpdatedAt (DateTime)
- Navigation Properties:
  - Products (ICollection<Product>)
```

### Database Schema
```sql
CREATE TABLE Brand (
    BrandId INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(200) NOT NULL,
    Country NVARCHAR(100),
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2 DEFAULT GETDATE()
);
```

### Relationship với Product
```sql
-- Product table có foreign key:
BrandId INT,
CONSTRAINT FK_Product_Brand FOREIGN KEY (BrandId)
    REFERENCES Brand(BrandId)
    ON DELETE SET NULL
```

**Lưu ý:** Khi xóa Brand, các Product liên quan sẽ tự động set `BrandId = NULL` (không bị xóa).

---

## 🛠️ Helper Methods

### BrandExists
```csharp
private async Task<bool> BrandExists(int id)
{
    return await _context.Brands.AnyAsync(e => e.BrandId == id);
}
```
- Kiểm tra brand có tồn tại
- Sử dụng trong Edit action để check concurrency

---

## 📝 API Endpoints Summary

| Method | Route | Action | Mô Tả |
|--------|-------|--------|-------|
| GET | `/Admin/Brands` | Index | Danh sách thương hiệu (có filter, search, pagination) |
| GET | `/Admin/Brands/Details/{id}` | Details | Xem chi tiết thương hiệu |
| GET | `/Admin/Brands/Create` | Create | Form tạo thương hiệu mới |
| POST | `/Admin/Brands/Create` | Create | Tạo thương hiệu mới |
| GET | `/Admin/Brands/Edit/{id}` | Edit | Form sửa thương hiệu |
| POST | `/Admin/Brands/Edit/{id}` | Edit | Cập nhật thương hiệu |
| GET | `/Admin/Brands/Delete/{id}` | Delete | Xác nhận xóa |
| POST | `/Admin/Brands/Delete/{id}` | DeleteConfirmed | Xóa thương hiệu |

---

## 🔄 Workflow Sử Dụng

### Tạo Thương Hiệu Mới
1. Admin vào **Admin → Brands → Create**
2. Điền thông tin:
   - Name (required, unique)
   - Country (optional)
   - IsActive (optional, default: true)
3. Click **"Tạo Thương Hiệu"**
4. **Hệ thống tự động:**
   - Validation tên không trùng
   - Set IsActive = true nếu null
   - Set CreatedAt, UpdatedAt
5. Redirect đến danh sách với success message

### Sửa Thương Hiệu
1. Từ danh sách, click **"Sửa"** trên thương hiệu cần sửa
2. Cập nhật thông tin (Name, Country, IsActive)
3. Click **"Lưu"**
4. Validation:
   - Name không được trùng với brand khác
   - ModelState validation
5. Redirect đến danh sách với success message

### Xóa Thương Hiệu
1. Từ danh sách, click **"Xóa"** trên thương hiệu cần xóa
2. Xác nhận thông tin thương hiệu
3. **Hệ thống kiểm tra:**
   - Số lượng sản phẩm đang sử dụng thương hiệu này
   - Nếu có sản phẩm → Hiển thị error, không cho phép xóa
4. Nếu không có sản phẩm:
   - Click **"Xác Nhận Xóa"**
   - Xóa brand
   - Các sản phẩm liên quan tự động set BrandId = NULL
5. Redirect đến danh sách với success message

### Tìm Kiếm & Lọc
1. Từ danh sách thương hiệu:
   - **Tìm kiếm:** Nhập từ khóa vào ô search (tìm theo tên, quốc gia)
   - **Lọc trạng thái:** Chọn "Active" hoặc "Inactive"
2. Click **"Tìm Kiếm"** hoặc **"Lọc"**
3. Kết quả được hiển thị với phân trang

---

## 📌 Lưu Ý Quan Trọng

### Unique Name Validation
- **Tên thương hiệu phải unique:**
  - Không được trùng với brand khác
  - Validation được thực hiện trong Create và Edit

### Delete Protection
- **Không thể xóa nếu có sản phẩm:**
  - Hệ thống kiểm tra số lượng sản phẩm trước khi xóa
  - Nếu có sản phẩm → Hiển thị error message với số lượng
  - Admin phải xóa hoặc chuyển các sản phẩm trước

### Database Cascade Behavior
- **Khi xóa Brand:**
  - Các Product liên quan **KHÔNG bị xóa**
  - `Product.BrandId` tự động set thành `NULL` (ON DELETE SET NULL)
  - Điều này đảm bảo dữ liệu sản phẩm không bị mất

### IsActive Default Value
- **Khi tạo mới:**
  - Nếu `IsActive` là null → Tự động set = true
  - Đảm bảo thương hiệu mới luôn active

### Filter & Search Combination
- **Có thể kết hợp:**
  - Search + Status
  - Ví dụ: `/Admin/Brands?search=Korea&status=active`

### Pagination
- **Mặc định:** 10 items/trang
- **Có thể tùy chỉnh:** `?pageSize=20`
- **Validation:** Page number được validate (không < 1, không > TotalPages)

---

## 🚀 Cải Tiến Có Thể Thêm (Future Enhancements)

1. **Brand Logo:**
   - Thêm logo cho thương hiệu
   - Upload và quản lý logo

2. **Brand Description:**
   - Thêm mô tả chi tiết về thương hiệu
   - Rich text editor

3. **Brand Website:**
   - Thêm link website chính thức
   - Social media links

4. **Bulk Actions:**
   - Bulk activate/deactivate
   - Bulk delete (với validation)
   - Bulk export

5. **Export/Import:**
   - Export danh sách thương hiệu ra Excel/CSV
   - Import thương hiệu từ file

6. **Advanced Search:**
   - Tìm kiếm theo số lượng sản phẩm
   - Filter theo quốc gia
   - Filter theo ngày tạo (date range)

7. **Brand Statistics:**
   - Số lượng sản phẩm trong mỗi thương hiệu
   - Tổng doanh thu theo thương hiệu
   - Dashboard thống kê

8. **Brand Slug:**
   - Tự động tạo slug từ tên thương hiệu
   - URL-friendly cho SEO

9. **Brand Order:**
   - Thêm DisplayOrder để sắp xếp thứ tự hiển thị
   - Drag & drop để sắp xếp

10. **Brand SEO:**
    - Meta title, meta description
    - SEO keywords

---

## 📄 Files Modified/Created

### Created
- `Areas/Admin/Controllers/BrandsController.cs` - Controller quản lý thương hiệu với đầy đủ CRUD operations

### Models Used (Existing)
- `Models/Brand.cs`
- `Models/Product.cs` (navigation property)

### Services Used
- `Data/ApplicationDbContext.cs` - Database context

---

## ✅ Testing Checklist

- [x] Index với search, filter (status), pagination
- [x] Details hiển thị đầy đủ thông tin và danh sách sản phẩm
- [x] Create với validation name unique
- [x] Edit với validation name unique (trừ brand hiện tại)
- [x] Delete với validation (không thể xóa nếu có sản phẩm)
- [x] Delete hiển thị số lượng sản phẩm đang sử dụng
- [x] Error handling và logging
- [x] TempData messages
- [x] Database schema compliance

---

**Tài liệu này mô tả đầy đủ hệ thống quản lý thương hiệu trong Admin area. Tất cả các tính năng đã được implement trong Controller, sẵn sàng cho việc tạo Views.**

