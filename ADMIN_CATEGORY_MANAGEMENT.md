# 📁 Admin Category Management System

## 📋 Tổng quan

Hệ thống quản lý danh mục sản phẩm trong Admin với đầy đủ tính năng CRUD, tìm kiếm, lọc, phân trang, và quản lý danh mục sản phẩm.

**Ngày tạo:** 2025-01-XX  
**Branch:** `feature/admin-category-management`

---

## ✨ Tính Năng Chính

### 1. Danh Sách Danh Mục (Index)

#### Route
- **GET:** `/Admin/Categories`
- **GET với filter:** `/Admin/Categories?search={keyword}&status={active/inactive}&page={page}&pageSize={size}`

#### Chức Năng

**Tìm Kiếm (Search):**
- Tìm kiếm theo:
  - Name (Tên danh mục)
  - Description (Mô tả danh mục)
- Không phân biệt hoa thường
- Tìm kiếm partial match (Contains)

**Lọc (Filter):**
- **Theo trạng thái:**
  - `status=active` → Chỉ hiển thị danh mục đang hoạt động
  - `status=inactive` → Chỉ hiển thị danh mục đã vô hiệu hóa

**Phân Trang:**
- Mặc định: 10 items/trang
- Có thể tùy chỉnh `pageSize`
- Hiển thị thông tin: CurrentPage, TotalPages, TotalCount

**Sắp Xếp:**
- Mặc định: Sắp xếp theo `CreatedAt` (mới nhất trước)

**Ví Dụ URL:**
```
/Admin/Categories                                    → Tất cả danh mục
/Admin/Categories?search=son                        → Tìm "son"
/Admin/Categories?status=active                      → Chỉ active
/Admin/Categories?search=makeup&status=active       → Tìm "makeup" + active
/Admin/Categories?page=2&pageSize=20               → Trang 2, 20 items/trang
```

#### ViewBag Parameters
- `ViewBag.Search` - Từ khóa tìm kiếm hiện tại
- `ViewBag.Status` - Trạng thái filter hiện tại
- `ViewBag.CurrentPage` - Trang hiện tại
- `ViewBag.TotalPages` - Tổng số trang
- `ViewBag.TotalCount` - Tổng số danh mục
- `ViewBag.PageSize` - Số items mỗi trang

---

### 2. Xem Chi Tiết Danh Mục (Details)

#### Route
- **GET:** `/Admin/Categories/Details/{id}`

#### Chức Năng
- Hiển thị đầy đủ thông tin danh mục:
  - **Thông tin danh mục:**
    - CategoryId
    - Name (Tên danh mục)
    - Description (Mô tả)
    - IsActive (Trạng thái hoạt động)
    - CreatedAt, UpdatedAt
  - **Danh sách sản phẩm:**
    - Products (Tất cả sản phẩm thuộc danh mục này)
    - Số lượng sản phẩm

#### Validation
- Kiểm tra category có tồn tại
- Nếu không tồn tại → Return NotFound()

---

### 3. Tạo Danh Mục Mới (Create)

#### Route
- **GET:** `/Admin/Categories/Create`
- **POST:** `/Admin/Categories/Create`

#### Chức Năng

**GET Action:**
- Hiển thị form tạo danh mục mới

**POST Action:**
- **Validation:**
  - ModelState validation
  - Kiểm tra Name đã tồn tại → Error: "Tên danh mục đã tồn tại. Vui lòng chọn tên khác."

- **Tự động:**
  - Set `IsActive = true` nếu null
  - Set `CreatedAt = DateTime.UtcNow`
  - Set `UpdatedAt = DateTime.UtcNow`

- **Success:**
  - TempData message: `"Danh mục '{Name}' đã được tạo thành công."`
  - Redirect đến Index

- **Error Handling:**
  - Try-catch với logging
  - ModelState error: `"Lỗi khi tạo danh mục: {ex.Message}"`

#### Form Fields
- Name (required, unique, max 100 characters)
- Description (optional, max 255 characters)
- IsActive (optional, default: true)

---

### 4. Sửa Danh Mục (Edit)

#### Route
- **GET:** `/Admin/Categories/Edit/{id}`
- **POST:** `/Admin/Categories/Edit/{id}`

#### Chức Năng

**GET Action:**
- Load category hiện tại
- Return View với category data

**POST Action:**
- **Validation:**
  - Kiểm tra `id == category.CategoryId`
  - ModelState validation
  - Kiểm tra Name đã tồn tại (trừ category hiện tại) → Error

- **Cập Nhật:**
  - Name
  - Description
  - IsActive (set = true nếu null)
  - UpdatedAt = DateTime.UtcNow

- **Success:**
  - TempData message: `"Danh mục '{Name}' đã được cập nhật thành công."`
  - Redirect đến Index

- **Error Handling:**
  - DbUpdateConcurrencyException → Kiểm tra CategoryExists
  - Exception → Logging + ModelState error

---

### 5. Xóa Danh Mục (Delete)

#### Route
- **GET:** `/Admin/Categories/Delete/{id}` (Xác nhận xóa)
- **POST:** `/Admin/Categories/Delete/{id}` (Xóa thực sự)

#### Chức Năng

**GET Action:**
- Hiển thị thông tin danh mục để xác nhận
- **Kiểm tra số lượng sản phẩm:**
  - Load Products của category
  - Hiển thị số lượng sản phẩm đang sử dụng danh mục này
  - ViewBag.ProductCount = số lượng sản phẩm

**POST Action (DeleteConfirmed):**
- **Validation:**
  - Kiểm tra category tồn tại
  - **Kiểm tra có sản phẩm đang sử dụng:**
    - Nếu có sản phẩm → Không cho phép xóa
    - Error message: `"Không thể xóa danh mục '{Name}' vì có {count} sản phẩm đang sử dụng danh mục này. Vui lòng xóa hoặc chuyển các sản phẩm trước."`
    - Redirect về Delete page

- **Xóa:**
  - Xóa category
  - **Lưu ý:** Theo database schema, `Product.CategoryId` có `ON DELETE SET NULL`, nên khi xóa category, các sản phẩm sẽ tự động set `CategoryId = NULL` (không bị xóa)

- **Success:**
  - TempData message: `"Danh mục '{Name}' đã được xóa thành công."`
  - Redirect đến Index

- **Error Handling:**
  - Exception → Logging + TempData error message
  - Redirect đến Index (không throw exception)

---

## 🔒 Security & Validation

### Authorization
- **Controller Level:**
  - `[RequirePermission("Admin.Category.Manage")]` - Tất cả actions đều yêu cầu permission này

### Unique Validation
- **Name:** Không được trùng với category khác
- Validation được thực hiện trong:
  - Create action
  - Edit action (trừ category hiện tại)

### Delete Validation
- **Không thể xóa nếu có sản phẩm:** Kiểm tra `Products.Count > 0`
- Nếu có sản phẩm → Hiển thị error và không cho phép xóa
- Admin phải xóa hoặc chuyển các sản phẩm trước khi xóa danh mục

### Error Handling
- **Try-Catch:** Tất cả actions đều có try-catch
- **Logging:** Sử dụng `ILogger<CategoriesController>` để log errors
- **User-Friendly Messages:** TempData messages cho success/error
- **ModelState Errors:** Hiển thị lỗi validation trong form

---

## 📊 Database Models

### Category Model
```csharp
- CategoryId (int, PK)
- Name (string, required, max 100)
- Description (string, nullable, max 255)
- IsActive (bool, nullable, default: true)
- CreatedAt (DateTime)
- UpdatedAt (DateTime)
- Navigation Properties:
  - Products (ICollection<Product>)
```

### Database Schema
```sql
CREATE TABLE Category (
    CategoryId INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(255) NULL,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2 DEFAULT GETDATE()
);
```

### Relationship với Product
```sql
-- Product table có foreign key:
CategoryId INT,
CONSTRAINT FK_Product_Category FOREIGN KEY (CategoryId)
    REFERENCES Category(CategoryId)
    ON DELETE SET NULL
```

**Lưu ý:** Khi xóa Category, các Product liên quan sẽ tự động set `CategoryId = NULL` (không bị xóa).

---

## 🛠️ Helper Methods

### CategoryExists
```csharp
private async Task<bool> CategoryExists(int id)
{
    return await _context.Categories.AnyAsync(e => e.CategoryId == id);
}
```
- Kiểm tra category có tồn tại
- Sử dụng trong Edit action để check concurrency

---

## 📝 API Endpoints Summary

| Method | Route | Action | Mô Tả |
|--------|-------|--------|-------|
| GET | `/Admin/Categories` | Index | Danh sách danh mục (có filter, search, pagination) |
| GET | `/Admin/Categories/Details/{id}` | Details | Xem chi tiết danh mục |
| GET | `/Admin/Categories/Create` | Create | Form tạo danh mục mới |
| POST | `/Admin/Categories/Create` | Create | Tạo danh mục mới |
| GET | `/Admin/Categories/Edit/{id}` | Edit | Form sửa danh mục |
| POST | `/Admin/Categories/Edit/{id}` | Edit | Cập nhật danh mục |
| GET | `/Admin/Categories/Delete/{id}` | Delete | Xác nhận xóa |
| POST | `/Admin/Categories/Delete/{id}` | DeleteConfirmed | Xóa danh mục |

---

## 🔄 Workflow Sử Dụng

### Tạo Danh Mục Mới
1. Admin vào **Admin → Categories → Create**
2. Điền thông tin:
   - Name (required, unique)
   - Description (optional)
   - IsActive (optional, default: true)
3. Click **"Tạo Danh Mục"**
4. **Hệ thống tự động:**
   - Validation tên không trùng
   - Set IsActive = true nếu null
   - Set CreatedAt, UpdatedAt
5. Redirect đến danh sách với success message

### Sửa Danh Mục
1. Từ danh sách, click **"Sửa"** trên danh mục cần sửa
2. Cập nhật thông tin (Name, Description, IsActive)
3. Click **"Lưu"**
4. Validation:
   - Name không được trùng với category khác
   - ModelState validation
5. Redirect đến danh sách với success message

### Xóa Danh Mục
1. Từ danh sách, click **"Xóa"** trên danh mục cần xóa
2. Xác nhận thông tin danh mục
3. **Hệ thống kiểm tra:**
   - Số lượng sản phẩm đang sử dụng danh mục này
   - Nếu có sản phẩm → Hiển thị error, không cho phép xóa
4. Nếu không có sản phẩm:
   - Click **"Xác Nhận Xóa"**
   - Xóa category
   - Các sản phẩm liên quan tự động set CategoryId = NULL
5. Redirect đến danh sách với success message

### Tìm Kiếm & Lọc
1. Từ danh sách danh mục:
   - **Tìm kiếm:** Nhập từ khóa vào ô search (tìm theo tên, mô tả)
   - **Lọc trạng thái:** Chọn "Active" hoặc "Inactive"
2. Click **"Tìm Kiếm"** hoặc **"Lọc"**
3. Kết quả được hiển thị với phân trang

---

## 📌 Lưu Ý Quan Trọng

### Unique Name Validation
- **Tên danh mục phải unique:**
  - Không được trùng với category khác
  - Validation được thực hiện trong Create và Edit

### Delete Protection
- **Không thể xóa nếu có sản phẩm:**
  - Hệ thống kiểm tra số lượng sản phẩm trước khi xóa
  - Nếu có sản phẩm → Hiển thị error message với số lượng
  - Admin phải xóa hoặc chuyển các sản phẩm trước

### Database Cascade Behavior
- **Khi xóa Category:**
  - Các Product liên quan **KHÔNG bị xóa**
  - `Product.CategoryId` tự động set thành `NULL` (ON DELETE SET NULL)
  - Điều này đảm bảo dữ liệu sản phẩm không bị mất

### IsActive Default Value
- **Khi tạo mới:**
  - Nếu `IsActive` là null → Tự động set = true
  - Đảm bảo danh mục mới luôn active

### Filter & Search Combination
- **Có thể kết hợp:**
  - Search + Status
  - Ví dụ: `/Admin/Categories?search=son&status=active`

### Pagination
- **Mặc định:** 10 items/trang
- **Có thể tùy chỉnh:** `?pageSize=20`
- **Validation:** Page number được validate (không < 1, không > TotalPages)

---

## 🚀 Cải Tiến Có Thể Thêm (Future Enhancements)

1. **Category Hierarchy:**
   - Thêm ParentCategoryId để tạo danh mục con
   - Hiển thị cây danh mục (tree view)

2. **Category Image:**
   - Thêm ảnh đại diện cho danh mục
   - Upload và quản lý ảnh

3. **Bulk Actions:**
   - Bulk activate/deactivate
   - Bulk delete (với validation)
   - Bulk export

4. **Export/Import:**
   - Export danh sách danh mục ra Excel/CSV
   - Import danh mục từ file

5. **Advanced Search:**
   - Tìm kiếm theo số lượng sản phẩm
   - Filter theo ngày tạo (date range)

6. **Category Statistics:**
   - Số lượng sản phẩm trong mỗi danh mục
   - Tổng doanh thu theo danh mục
   - Dashboard thống kê

7. **Category Slug:**
   - Tự động tạo slug từ tên danh mục
   - URL-friendly cho SEO

8. **Category Order:**
   - Thêm DisplayOrder để sắp xếp thứ tự hiển thị
   - Drag & drop để sắp xếp

9. **Category SEO:**
   - Meta title, meta description
   - SEO keywords

10. **Category Translation:**
    - Hỗ trợ đa ngôn ngữ
    - Tên và mô tả theo ngôn ngữ

---

## 📄 Files Modified/Created

### Created
- `Areas/Admin/Controllers/CategoriesController.cs` - Controller quản lý danh mục với đầy đủ CRUD operations

### Models Used (Existing)
- `Models/Category.cs`
- `Models/Product.cs` (navigation property)

### Services Used
- `Data/ApplicationDbContext.cs` - Database context

---

## ✅ Testing Checklist

- [x] Index với search, filter (status), pagination
- [x] Details hiển thị đầy đủ thông tin và danh sách sản phẩm
- [x] Create với validation name unique
- [x] Edit với validation name unique (trừ category hiện tại)
- [x] Delete với validation (không thể xóa nếu có sản phẩm)
- [x] Delete hiển thị số lượng sản phẩm đang sử dụng
- [x] Error handling và logging
- [x] TempData messages
- [x] Database schema compliance

---

**Tài liệu này mô tả đầy đủ hệ thống quản lý danh mục trong Admin area. Tất cả các tính năng đã được implement trong Controller, sẵn sàng cho việc tạo Views.**


