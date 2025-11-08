# Database Migration Summary - INT ID & Product Variants

## Ngày thực hiện: November 6, 2025

## 📋 Tổng quan thay đổi

Dự án đã được cập nhật từ GUID sang INT IDENTITY cho tất cả primary keys và thêm hệ thống Product Variants để hỗ trợ:
- ✅ Sản phẩm có nhiều màu sắc (VD: son môi)
- ✅ Sản phẩm có nhiều kích thước (VD: chai xịt, kem)
- ✅ Sản phẩm có cả màu và size (VD: phấn má)
- ✅ Hình ảnh riêng cho từng variant
- ✅ Giỏ hàng, đơn hàng, đánh giá, wishlist

---

## 🗄️ Database Changes

### 1. **Cấu trúc Database Mới**

#### Primary Key Changes:
- **Brand**: `BrandId` GUID → INT IDENTITY(1,1)
- **Category**: `CategoryId` GUID → INT IDENTITY(1,1)
- **Product**: `ProductId` GUID → INT IDENTITY(1,1)
- **ProductImage**: `ProductImageId` GUID → INT IDENTITY(1,1)

#### Product Table Updates:
```sql
-- Renamed: Price → BasePrice
-- Added:
- Rating (decimal(3,2))
- ReviewCount (int)
- LikeCount (int)
- SPF (nvarchar(50))
- Size (nvarchar(50))
- Finish (nvarchar(50))
```

#### New Tables:
1. **ProductVariant** - Các biến thể sản phẩm
   - VariantId (INT PK)
   - ColorName, ColorCode, ColorFamily (cho màu sắc)
   - Size, SizeValue, SizeUnit (cho kích thước)
   - VariantType (cho loại: oily skin, dry skin, etc.)
   - Price, CompareAtPrice, Stock, SKU

2. **ProductVariantImage** - Ảnh cho từng variant
   - ImageId (INT PK)
   - VariantId (FK)
   - Url, AltText, IsPrimary, DisplayOrder

3. **ShoppingCart** & **CartItem**
4. **Orders** & **OrderItem**
5. **ProductReview**
6. **ProductLike**

---

## 📁 Code Changes

### Models Updated/Created:

#### ✅ Updated Models:
- `Brand.cs` - INT ID
- `Category.cs` - INT ID
- `Product.cs` - INT ID + new properties + relationships
- `ProductImage.cs` - INT ID + ImageType + DisplayOrder
- `User.cs` - Added shopping/order/review relationships

#### ✅ New Models Created:
- `ProductVariant.cs` - Flexible variant system
- `ProductVariantImage.cs`
- `ShoppingCart.cs` + `CartItem.cs`
- `Order.cs` + `OrderItem.cs`
- `ProductReview.cs`
- `ProductLike.cs`

### Repositories:

#### `IProductRepository.cs` - New Methods:
```csharp
Task<Product?> GetByIdWithVariantsAsync(int id);
Task<ProductVariant?> GetVariantByIdAsync(int variantId);
Task<IEnumerable<ProductVariant>> GetVariantsByProductIdAsync(int productId);
Task<ProductVariant?> GetDefaultVariantAsync(int productId);
Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId);
Task<IEnumerable<Product>> SearchAsync(string keyword);
```

#### `ProductRepository.cs`:
- All methods updated to use INT
- Added eager loading for variants and images
- Improved queries for performance

### Controllers:

#### `ProductsController.cs`:
```csharp
// Updated:
- Index(int page, int? categoryId, string? search)
- Details(int id) - now loads variants
- VirtualTryOn(int variantId, ...) - uses variant instead of product

// New APIs:
- GetVariant(int variantId) - Returns variant data as JSON
```

### Views:

#### `Products/Details.cshtml`:
- ✅ Variant selection UI (colors & sizes)
- ✅ Dynamic image gallery updates when variant changes
- ✅ Real-time price/stock updates
- ✅ Color swatches with hex codes
- ✅ Stock indicators (In Stock, Low Stock, Out of Stock)
- ✅ Responsive design

### CSS:

#### New File: `product-variants.css`
- Variant option buttons with hover effects
- Color swatch styling
- Size selection UI
- Badge indicators
- Stock status styling
- Responsive breakpoints

### Database Context:

#### `ApplicationDbContext.cs`:
- Added all new DbSets
- Configured relationships and constraints
- Set default values for properties
- Added indexes for performance

---

## 🎨 UI Features

### Product Details Page:

1. **Variant Selection**:
   - Color picker with visual swatches
   - Size dropdown/buttons
   - Real-time stock updates
   - Disabled state for out-of-stock items

2. **Image Gallery**:
   - Automatically updates when user selects a variant
   - Smooth transitions
   - Thumbnail navigation
   - Prev/Next buttons

3. **Price Display**:
   - Current price
   - Compare at price (strikethrough)
   - Discount percentage badge

4. **Stock Information**:
   - "X items in stock"
   - Low stock warning (yellow badge)
   - Out of stock indicator (red badge)

5. **Virtual Try-On**:
   - Works with selected variant
   - Uses variant's primary image as makeup reference

---

## 📊 Data Structure Examples

### Example 1: Lipstick (Color Only)
```
Product: "The Kissu Lip Tint SPF 25"
├─ Variant: "Midnight Lily" (ColorName, ColorCode)
│  ├─ Image 1 (Primary)
│  ├─ Image 2
│  └─ Image 3
├─ Variant: "Camellia"
│  └─ Images...
└─ Variant: "Plum Blossom"
   └─ Images...
```

### Example 2: Spray (Size Only)
```
Product: "Facial Spray"
├─ Variant: "59ml" (Size, SizeValue, SizeUnit)
├─ Variant: "118ml"
└─ Variant: "236ml"
```

### Example 3: Blush (Color + Size)
```
Product: "NARS Blush"
├─ Variant: "Orgasm - 3.5g" (ColorName + Size)
├─ Variant: "Orgasm - 4.8g"
├─ Variant: "Deep Throat - 3.5g"
└─ Variant: "Deep Throat - 4.8g"
```

---

## 🚀 Next Steps

### Bước 1: Chạy Database Script
```sql
-- Chạy file này để tạo database mới
Website_Cosmetics/Database/01_CreateTables.sql
```

### Bước 2: Migration (Nếu dùng EF Core)
```bash
# Xóa migrations cũ nếu có
Remove-Migration

# Tạo migration mới
Add-Migration IntIdWithProductVariants

# Update database
Update-Database
```

### Bước 3: Test Data
Sử dụng file example để insert data:
```sql
Website_Cosmetics/Database/ProductVariant_Examples.sql
```

### Bước 4: Test Features
- [ ] Create product with variants
- [ ] Select different colors → images change
- [ ] Check stock updates
- [ ] Add to cart functionality
- [ ] Virtual try-on with variant
- [ ] Mobile responsiveness

---

## ⚠️ Breaking Changes

### API Changes:
- All `Guid` parameters → `int` parameters
- `productId` is now `int` everywhere
- Virtual Try-On API now requires `variantId` instead of `productId`

### Data Model:
- `Product.Price` → `Product.BasePrice`
- `ProductImage.IsCover` → `ProductImage.ImageType`
- `ProductImage.IsMakeupReference` → removed

### Frontend:
- Product details page requires variant selection
- Cannot add to cart without selecting variant
- Image gallery is now variant-specific

---

## 📝 Files Modified

### Models (10 files):
- `Brand.cs`
- `Category.cs`
- `Product.cs`
- `ProductImage.cs`
- `ProductVariant.cs` ✨ NEW
- `ProductVariantImage.cs` ✨ NEW
- `ShoppingCart.cs` ✨ NEW
- `CartItem.cs` ✨ NEW
- `Order.cs` ✨ NEW
- `OrderItem.cs` ✨ NEW
- `ProductReview.cs` ✨ NEW
- `ProductLike.cs` ✨ NEW
- `User.cs`

### Data Layer (2 files):
- `ApplicationDbContext.cs`

### Repositories (2 files):
- `IProductRepository.cs`
- `ProductRepository.cs`

### Controllers (1 file):
- `ProductsController.cs`

### Views (1 file):
- `Views/Products/Details.cshtml`

### CSS (1 file):
- `wwwroot/public/css/components/product-variants.css` ✨ NEW

### Database (2 files):
- `Database/01_CreateTables.sql`
- `Database/ProductVariant_Examples.sql` ✨ NEW

---

## 🎯 Benefits

1. **Stable IDs**: INT không thay đổi, dễ debug
2. **Better Performance**: INT indexes faster than GUID
3. **Flexible Variants**: Hỗ trợ mọi loại biến thể sản phẩm
4. **Better UX**: User thấy ảnh thật của màu họ chọn
5. **Scalable**: Dễ mở rộng cho shopping cart, orders
6. **SEO Friendly**: Clean URLs với INT IDs

---

## 🐛 Known Issues / TODO

- [ ] Implement actual shopping cart functionality
- [ ] Add admin panel for managing variants
- [ ] Bulk upload variants from CSV/Excel
- [ ] Image optimization and lazy loading
- [ ] Add filters by color/size on product listing page
- [ ] Implement wishlist functionality
- [ ] Add product comparison feature

---

## 📞 Support

Nếu có vấn đề gì, check:
1. Database script đã chạy thành công chưa?
2. Connection string đúng chưa?
3. All packages restored? (`dotnet restore`)
4. Build successful? (`dotnet build`)

Happy coding! 🚀

