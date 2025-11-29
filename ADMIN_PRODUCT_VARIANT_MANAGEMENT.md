# 📦 Admin Product & Variant Management System

## 📋 Tổng quan

Hệ thống quản lý sản phẩm và variants trong Admin với workflow 2 bước rõ ràng, tổ chức ảnh theo cấu trúc folder, và các tính năng quản lý nâng cao.

**Ngày tạo:** 2025-01-XX  
**Branch:** `feature/admin-product-variant-management`

---

## 🎯 Workflow Quản Lý Sản Phẩm

### Bước 1: Tạo Sản Phẩm Chính
- Admin vào **Admin → Products → Create New Product**
- Điền thông tin sản phẩm chính:
  - Tên sản phẩm, Brand, Category
  - Base Price, Description, Ingredients
  - SPF, Size, Finish
- Click **"Tạo Sản Phẩm"**
- **Hệ thống tự động:**
  - Tạo sản phẩm trong database
  - Tạo folder: `/wwwroot/public/images/products/{ProductName}/`
  - Tạo variant mặc định (tên = tên sản phẩm)
  - Redirect đến trang quản lý variants

### Bước 2: Quản Lý Variants
- Từ trang **Products Index**, click **"Quản Lý Variants"**
- Hoặc tự động redirect sau khi tạo sản phẩm
- Trang **Variants Index** hiển thị:
  - Thông tin sản phẩm
  - Danh sách tất cả variants
  - Nút **"+ Thêm Variant Mới"**
- Thêm/Sửa/Xóa variants với đầy đủ thông tin:
  - Màu sắc (ColorName, ColorCode, ColorFamily)
  - Kích thước (Size, SizeValue, SizeUnit)
  - Giá, Tồn kho, SKU, Barcode
  - Upload nhiều ảnh cho mỗi variant

---

## 📁 Cấu Trúc Folder Ảnh

### Quy Tắc Tổ Chức

```
wwwroot/public/images/products/
├── {ProductName}/                    ← Folder sản phẩm (tự động tạo khi tạo sản phẩm)
│   ├── {VariantName}/                ← Folder variant (tự động tạo khi upload ảnh)
│   │   ├── image1.jpg
│   │   ├── image2.jpg
│   │   └── model.jpg
│   └── {VariantName2}/
│       └── image1.jpg
└── {ProductName2}/
    └── {VariantName}/
        └── image1.jpg
```

### Tên Folder

**Folder Sản Phẩm:**
- Tạo từ `Product.Name` → Slug hóa
- Ví dụ: "3CE Cashmere Hug Lipstick" → `3ce-cashmere-hug-lipstick`

**Folder Variant:**
- **Ưu tiên 1:** `ColorName` (nếu có) → "Midnight Lily" → `midnight-lily`
- **Ưu tiên 2:** `VariantName` (nếu không có màu) → "100ml" → `100ml`
- **Fallback:** `variant-{VariantId}` → `variant-5`

### Tự Động Xóa Folder

- **Khi xóa variant:** Tự động xóa folder variant
- **Khi xóa variant cuối cùng:** Tự động xóa folder sản phẩm
- **Khi bulk delete:** Xóa tất cả folders của variants bị xóa

---

## ✨ Tính Năng Chính

### 1. Quản Lý Sản Phẩm (ProductsController)

#### Tạo Sản Phẩm
- **Route:** `GET/POST /Admin/Products/Create`
- **Chức năng:**
  - Form chỉ có thông tin sản phẩm chính
  - Tự động tạo variant mặc định
  - Tự động tạo folder sản phẩm
  - Redirect đến quản lý variants

#### Danh Sách Sản Phẩm
- **Route:** `GET /Admin/Products`
- **Hiển thị:**
  - Thông tin sản phẩm (tên, brand, category, giá)
  - Số lượng variants
  - Nút "Quản Lý Variants" cho mỗi sản phẩm

### 2. Quản Lý Variants (VariantsController)

#### Danh Sách Variants
- **Route:** `GET /Admin/Variants/Index?productId={id}`
- **Chức năng:**
  - Hiển thị thông tin sản phẩm
  - Danh sách variants với checkbox
  - Nút bulk delete
  - Link đến Create/Edit/Delete

#### Tạo Variant Mới
- **Route:** `GET/POST /Admin/Variants/Create?productId={id}`
- **Form fields:**
  - Tên variant, Màu sắc (Name, Code, Family)
  - Kích thước (Size, Value, Unit)
  - Loại/Type (cho skincare)
  - Thông tin bán hàng (SKU, Barcode, Price, Stock)
  - Upload nhiều ảnh
  - Đánh dấu Primary và Makeup Reference

#### Sửa Variant
- **Route:** `GET/POST /Admin/Variants/Edit/{id}`
- **Tính năng:**
  - Sửa tất cả thông tin variant
  - Quản lý ảnh hiện có (xóa, đánh dấu primary/makeup)
  - **Drag & Drop** để sắp xếp thứ tự ảnh
  - Upload thêm ảnh mới

#### Xóa Variant
- **Route:** `GET/POST /Admin/Variants/Delete/{id}`
- **Chức năng:**
  - Xóa variant và tất cả ảnh
  - Tự động xóa folder variant
  - Tự động set default variant nếu cần

#### Bulk Delete
- **Route:** `POST /Admin/Variants/BulkDelete`
- **Chức năng:**
  - Xóa nhiều variants cùng lúc
  - Xóa tất cả ảnh và folders
  - Tự động xóa folder sản phẩm nếu không còn variant

---

## 🎨 Tính Năng Nâng Cao

### 1. Drag & Drop Sắp Xếp Ảnh

**Vị trí:** Trang Edit Variant

**Cách sử dụng:**
1. Kéo ảnh đến vị trí mong muốn
2. Thả ảnh
3. Thứ tự tự động cập nhật
4. Hiển thị số thứ tự trên mỗi ảnh

**Technical:**
- Sử dụng HTML5 Drag & Drop API
- Tự động cập nhật `DisplayOrder` trong database
- Visual feedback khi kéo (border, background)

### 2. Bulk Delete Variants

**Vị trí:** Trang Variants Index

**Cách sử dụng:**
1. Chọn variants cần xóa (checkbox)
2. Hoặc dùng "Select All"
3. Click "Xóa đã chọn (X)"
4. Xác nhận xóa
5. Tất cả variants và folders được xóa

**Features:**
- Hiển thị số lượng đã chọn
- Nút disabled khi chưa chọn
- Xác nhận trước khi xóa
- Tự động reload sau khi xóa

### 3. Validation Primary Image

**Yêu cầu:**
- Variant **PHẢI** có ít nhất 1 ảnh
- Variant **PHẢI** có ít nhất 1 ảnh primary

**Tự động xử lý:**
- Nếu không có primary → Tự động set ảnh đầu tiên làm primary
- Nếu xóa ảnh primary → Tự động set ảnh đầu tiên còn lại làm primary
- Nếu không có ảnh → Báo lỗi và yêu cầu upload

---

## 📊 Database Schema

### Product Table
```sql
ProductId INT PRIMARY KEY
Name NVARCHAR(200)
BasePrice DECIMAL(12,2)
BrandId INT
CategoryId INT
...
```

### ProductVariant Table
```sql
VariantId INT PRIMARY KEY
ProductId INT FOREIGN KEY
VariantName NVARCHAR(200)
ColorName NVARCHAR(100)
ColorCode NVARCHAR(50)
Size NVARCHAR(100)
Price DECIMAL(12,2)
Stock INT
IsDefault BIT
IsActive BIT
...
```

### ProductVariantImage Table
```sql
ImageId INT PRIMARY KEY
VariantId INT FOREIGN KEY
Url NVARCHAR(500)
IsPrimary BIT
IsMakeupReference BIT
DisplayOrder INT
...
```

---

## 🔧 Technical Details

### Controllers

#### ProductsController.cs
- **Modified:** `Create()` - Chỉ tạo sản phẩm + variant mặc định
- **Added:** `CreateProductFolder()` - Tạo folder sản phẩm

#### VariantsController.cs (NEW)
- **Index()** - Danh sách variants
- **Create()** - Tạo variant mới
- **Edit()** - Sửa variant
- **Delete()** - Xóa variant
- **BulkDelete()** - Xóa nhiều variants
- **DeleteImage()** - Xóa ảnh (AJAX)
- **UploadVariantImages()** - Upload và tổ chức ảnh
- **DeleteVariantFolder()** - Xóa folder variant
- **DeleteProductFolder()** - Xóa folder sản phẩm
- **GenerateSlug()** - Tạo slug cho tên folder

### Views

#### Products/
- **Create.cshtml** - Form tạo sản phẩm (chỉ thông tin chính)
- **Index.cshtml** - Danh sách sản phẩm + link quản lý variants

#### Variants/ (NEW)
- **Index.cshtml** - Danh sách variants + bulk delete
- **Create.cshtml** - Form tạo variant + upload ảnh
- **Edit.cshtml** - Form sửa variant + drag & drop ảnh
- **Delete.cshtml** - Xác nhận xóa variant

---

## 🚀 Cách Sử Dụng

### 1. Tạo Sản Phẩm Mới

```
1. Vào Admin → Products → Create New Product
2. Điền thông tin sản phẩm:
   - Tên: "3CE Cashmere Hug Lipstick"
   - Brand: 3CE
   - Category: Lipstick
   - Base Price: 350000
   - Description: ...
3. Click "Tạo Sản Phẩm"
4. Tự động redirect đến /Admin/Variants/Index?productId=5
```

### 2. Thêm Variant Mới

```
1. Từ Variants Index, click "+ Thêm Variant Mới"
2. Điền thông tin variant:
   - Variant Name: "Midnight Lily"
   - Color Name: "Midnight Lily"
   - Color Code: #8B4F5E
   - Price: 350000
   - Stock: 50
3. Upload ảnh (ít nhất 1 ảnh)
4. Đánh dấu ảnh Primary và Makeup Reference
5. Click "Tạo Variant"
```

### 3. Sắp Xếp Ảnh (Drag & Drop)

```
1. Vào Edit Variant
2. Kéo ảnh đến vị trí mong muốn
3. Thả ảnh
4. Thứ tự tự động cập nhật
5. Click "Lưu Thay đổi"
```

### 4. Bulk Delete Variants

```
1. Vào Variants Index
2. Chọn variants cần xóa (checkbox)
3. Click "Xóa đã chọn (X)"
4. Xác nhận xóa
5. Tất cả variants và folders được xóa
```

---

## 📝 Files Đã Tạo/Sửa

### Controllers
- ✅ `Areas/Admin/Controllers/ProductsController.cs` (Modified)
- ✅ `Areas/Admin/Controllers/VariantsController.cs` (NEW)

### Views
- ✅ `Areas/Admin/Views/Products/Create.cshtml` (Modified)
- ✅ `Areas/Admin/Views/Products/Index.cshtml` (Modified)
- ✅ `Areas/Admin/Views/Variants/Index.cshtml` (NEW)
- ✅ `Areas/Admin/Views/Variants/Create.cshtml` (NEW)
- ✅ `Areas/Admin/Views/Variants/Edit.cshtml` (NEW)
- ✅ `Areas/Admin/Views/Variants/Delete.cshtml` (NEW)

---

## ✅ Checklist Tính Năng

- [x] Workflow 2 bước: Tạo sản phẩm → Quản lý variants
- [x] Tự động tạo variant mặc định khi tạo sản phẩm
- [x] Tổ chức ảnh theo folder: `products/{ProductName}/{VariantName}/`
- [x] Tự động tạo folder khi tạo sản phẩm/variant
- [x] Tự động xóa folder khi xóa variant/sản phẩm
- [x] Drag & Drop sắp xếp ảnh
- [x] Bulk delete variants
- [x] Validation: Variant phải có ít nhất 1 ảnh primary
- [x] Upload nhiều ảnh cho mỗi variant
- [x] Đánh dấu Primary và Makeup Reference
- [x] Xóa ảnh bằng AJAX
- [x] Tự động set primary image nếu chưa có

---

## 🔍 API Endpoints

### Products
- `GET /Admin/Products` - Danh sách sản phẩm
- `GET /Admin/Products/Create` - Form tạo sản phẩm
- `POST /Admin/Products/Create` - Tạo sản phẩm
- `GET /Admin/Products/Edit/{id}` - Form sửa sản phẩm
- `POST /Admin/Products/Edit/{id}` - Sửa sản phẩm
- `GET /Admin/Products/Delete/{id}` - Xác nhận xóa
- `POST /Admin/Products/Delete/{id}` - Xóa sản phẩm

### Variants
- `GET /Admin/Variants/Index?productId={id}` - Danh sách variants
- `GET /Admin/Variants/Create?productId={id}` - Form tạo variant
- `POST /Admin/Variants/Create` - Tạo variant
- `GET /Admin/Variants/Edit/{id}` - Form sửa variant
- `POST /Admin/Variants/Edit/{id}` - Sửa variant
- `GET /Admin/Variants/Delete/{id}` - Xác nhận xóa
- `POST /Admin/Variants/Delete/{id}` - Xóa variant
- `POST /Admin/Variants/BulkDelete` - Xóa nhiều variants (JSON)
- `POST /Admin/Variants/DeleteImage` - Xóa ảnh (JSON)

---

## 🎯 Lợi Ích

1. **Tổ chức rõ ràng:** Ảnh được sắp xếp theo sản phẩm và variant
2. **Dễ quản lý:** Tách biệt quản lý sản phẩm và variants
3. **Tự động hóa:** Tự động tạo/xóa folders
4. **User-friendly:** Drag & drop, bulk actions
5. **Validation:** Đảm bảo dữ liệu đầy đủ (phải có ảnh primary)

---

## 📌 Lưu Ý Quan Trọng

1. **Folder Structure:**
   - Folder sản phẩm được tạo khi tạo sản phẩm
   - Folder variant được tạo khi upload ảnh variant đầu tiên
   - Folder tự động xóa khi xóa variant/sản phẩm

2. **Validation:**
   - Variant **PHẢI** có ít nhất 1 ảnh
   - Variant **PHẢI** có ít nhất 1 ảnh primary
   - Hệ thống tự động set primary nếu chưa có

3. **Default Variant:**
   - Variant đầu tiên được tạo tự động là default
   - Nếu xóa default variant, variant đầu tiên còn lại sẽ được set làm default

4. **Image Organization:**
   - Tên folder được slug hóa (chữ thường, không dấu, dùng dấu gạch ngang)
   - Ảnh được lưu với tên unique (GUID)
   - URL format: `/public/images/products/{ProductName}/{VariantName}/{FileName}`

---

## 🐛 Troubleshooting

### Lỗi: "Variant phải có ít nhất 1 ảnh"
**Nguyên nhân:** Chưa upload ảnh hoặc đã xóa hết ảnh  
**Giải pháp:** Upload ít nhất 1 ảnh và đánh dấu làm primary

### Lỗi: "Folder không được tạo"
**Nguyên nhân:** Quyền ghi file không đủ  
**Giải pháp:** Kiểm tra quyền thư mục `wwwroot/public/images/products/`

### Lỗi: "Không tìm thấy variant"
**Nguyên nhân:** VariantId không hợp lệ  
**Giải pháp:** Kiểm tra variant có tồn tại trong database không

---

## 📚 Tham Khảo

- Database Schema: `Database/01_CreateTables.sql`
- Code Structure: `CODE_STRUCTURE.md`
- Integration: `VIRTUAL_TRYON_INTEGRATION.md`

---

**Happy Coding! 🚀💄**

