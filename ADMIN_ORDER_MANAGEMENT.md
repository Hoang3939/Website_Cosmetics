# 📦 Admin Order Management System

## 📋 Tổng quan

Hệ thống quản lý đơn hàng trong Admin với đầy đủ tính năng xem danh sách, chi tiết, cập nhật trạng thái, hủy đơn hàng, tìm kiếm, lọc và phân trang.

**Ngày tạo:** 2025-01-XX  
**Branch:** `feature/admin-order-management`

---

## ✨ Tính Năng Chính

### 1. Danh Sách Đơn Hàng (Index)

#### Route
- **GET:** `/Admin/Orders`
- **GET với filter:** `/Admin/Orders?search={keyword}&status={status}&paymentStatus={paymentStatus}&page={page}&pageSize={size}`

#### Chức Năng

**Tìm Kiếm (Search):**
- Tìm kiếm theo:
  - OrderNumber (Mã đơn hàng)
  - Username (Tên đăng nhập khách hàng)
  - Email (Email khách hàng)
  - FirstName (Tên khách hàng)
  - LastName (Họ khách hàng)
  - FullName (Họ và tên khách hàng)
- Không phân biệt hoa thường
- Tìm kiếm partial match (Contains)

**Lọc (Filter):**
- **Theo trạng thái đơn hàng:**
  - `status=Pending` → Đơn hàng chờ xử lý
  - `status=Processing` → Đơn hàng đang xử lý
  - `status=Shipped` → Đơn hàng đã gửi
  - `status=Delivered` → Đơn hàng đã giao
  - `status=Cancelled` → Đơn hàng đã hủy
- **Theo trạng thái thanh toán:**
  - `paymentStatus=Pending` → Chờ thanh toán
  - `paymentStatus=Paid` → Đã thanh toán
  - `paymentStatus=Failed` → Thanh toán thất bại
  - `paymentStatus=Refunded` → Đã hoàn tiền

**Phân Trang:**
- Mặc định: 10 items/trang
- Có thể tùy chỉnh `pageSize`
- Hiển thị thông tin: CurrentPage, TotalPages, TotalCount

**Sắp Xếp:**
- Mặc định: Sắp xếp theo `CreatedAt` (mới nhất trước)

**Ví Dụ URL:**
```
/Admin/Orders                                          → Tất cả đơn hàng
/Admin/Orders?search=ORD-2025                          → Tìm "ORD-2025"
/Admin/Orders?status=Pending                         → Chỉ đơn chờ xử lý
/Admin/Orders?paymentStatus=Paid                       → Chỉ đơn đã thanh toán
/Admin/Orders?status=Processing&paymentStatus=Paid    → Đơn đang xử lý + đã thanh toán
/Admin/Orders?page=2&pageSize=20                      → Trang 2, 20 items/trang
```

#### ViewBag Parameters
- `ViewBag.Search` - Từ khóa tìm kiếm hiện tại
- `ViewBag.Status` - Trạng thái đơn hàng filter hiện tại
- `ViewBag.PaymentStatus` - Trạng thái thanh toán filter hiện tại
- `ViewBag.CurrentPage` - Trang hiện tại
- `ViewBag.TotalPages` - Tổng số trang
- `ViewBag.TotalCount` - Tổng số đơn hàng
- `ViewBag.PageSize` - Số items mỗi trang
- `ViewBag.StatusList` - Danh sách các trạng thái có thể chọn
- `ViewBag.PaymentStatusList` - Danh sách các trạng thái thanh toán có thể chọn

---

### 2. Xem Chi Tiết Đơn Hàng (Details)

#### Route
- **GET:** `/Admin/Orders/Details/{id}`

#### Chức Năng
- Hiển thị đầy đủ thông tin đơn hàng:
  - **Thông tin đơn hàng:**
    - OrderId, OrderNumber
    - Status (Trạng thái đơn hàng)
    - PaymentStatus (Trạng thái thanh toán)
    - CreatedAt, UpdatedAt
  - **Thông tin giá:**
    - SubTotal (Tổng tiền sản phẩm)
    - ShippingFee (Phí vận chuyển)
    - Discount (Giảm giá)
    - Total (Tổng cộng)
  - **Thông tin giao hàng:**
    - ShippingAddress (Địa chỉ giao hàng)
    - ShippingMethod (Phương thức vận chuyển)
  - **Thông tin thanh toán:**
    - PaymentMethod (Phương thức thanh toán)
  - **Ghi chú:**
    - Notes (Ghi chú đơn hàng)
  - **Thông tin khách hàng:**
    - User (Thông tin khách hàng - có thể null nếu là guest)
  - **Danh sách sản phẩm:**
    - OrderItems (Chi tiết từng sản phẩm trong đơn)
      - ProductName (Tên sản phẩm)
      - ColorName (Tên màu)
      - Quantity (Số lượng)
      - UnitPrice (Giá đơn vị)
      - Subtotal (Thành tiền)
      - ProductVariant (Link đến variant)
      - Product (Link đến sản phẩm)

#### Validation
- Kiểm tra order có tồn tại
- Nếu không tồn tại → Return NotFound()

---

### 3. Cập Nhật Trạng Thái Đơn Hàng (UpdateStatus)

#### Route
- **GET:** `/Admin/Orders/UpdateStatus/{id}`
- **POST:** `/Admin/Orders/UpdateStatus/{id}`

#### Chức Năng

**GET Action:**
- Hiển thị form cập nhật trạng thái
- Load order hiện tại
- Hiển thị danh sách các trạng thái có thể chọn

**POST Action:**
- **Validation:**
  - Kiểm tra order tồn tại
  - Validate status phải là một trong: "Pending", "Processing", "Shipped", "Delivered", "Cancelled"
  - Validate paymentStatus (nếu có) phải là một trong: "Pending", "Paid", "Failed", "Refunded"

- **Logic tự động:**
  - **Nếu hủy đơn hàng (status = "Cancelled") và đã thanh toán (PaymentStatus = "Paid"):**
    - Tự động set `PaymentStatus = "Refunded"`

- **Cập Nhật:**
  - Status (Trạng thái đơn hàng)
  - PaymentStatus (Trạng thái thanh toán - optional)
  - UpdatedAt = DateTime.UtcNow

- **Success:**
  - TempData message: `"Trạng thái đơn hàng '{OrderNumber}' đã được cập nhật thành công."`
  - Redirect đến Details

- **Error Handling:**
  - Try-catch với logging
  - TempData error message nếu có lỗi
  - Redirect đến Details

#### Trạng Thái Đơn Hàng (Status)

| Trạng Thái | Mô Tả |
|------------|-------|
| **Pending** | Đơn hàng mới, chờ xử lý |
| **Processing** | Đơn hàng đang được xử lý (đóng gói, chuẩn bị) |
| **Shipped** | Đơn hàng đã được gửi đi |
| **Delivered** | Đơn hàng đã được giao đến khách hàng |
| **Cancelled** | Đơn hàng đã bị hủy |

#### Trạng Thái Thanh Toán (PaymentStatus)

| Trạng Thái | Mô Tả |
|------------|-------|
| **Pending** | Chờ thanh toán |
| **Paid** | Đã thanh toán |
| **Failed** | Thanh toán thất bại |
| **Refunded** | Đã hoàn tiền |

---

### 4. Hủy Đơn Hàng (Cancel)

#### Route
- **GET:** `/Admin/Orders/Cancel/{id}` (Xác nhận hủy)
- **POST:** `/Admin/Orders/Cancel/{id}` (Hủy thực sự)

#### Chức Năng

**GET Action:**
- Hiển thị thông tin đơn hàng để xác nhận hủy
- **Validation:**
  - Kiểm tra đơn hàng đã bị hủy chưa → Nếu đã hủy, hiển thị error và redirect
  - Kiểm tra đơn hàng đã được giao chưa → Nếu đã giao, không cho phép hủy

**POST Action (CancelConfirmed):**
- **Validation:**
  - Kiểm tra order tồn tại
  - Kiểm tra đơn hàng đã bị hủy chưa → Nếu đã hủy, hiển thị error
  - Kiểm tra đơn hàng đã được giao chưa → Nếu đã giao, không cho phép hủy

- **Hủy đơn hàng:**
  - Set `Status = "Cancelled"`
  - **Nếu đã thanh toán (PaymentStatus = "Paid"):**
    - Tự động set `PaymentStatus = "Refunded"`
  - **Ghi lý do hủy vào Notes:**
    - Nếu có `cancelReason` → Ghi vào Notes với timestamp
    - Nếu không có → Ghi mặc định: "Đơn hàng đã được hủy bởi admin."
  - Set `UpdatedAt = DateTime.UtcNow`

- **Success:**
  - TempData message: `"Đơn hàng '{OrderNumber}' đã được hủy thành công."`
  - Redirect đến Details

- **Error Handling:**
  - Try-catch với logging
  - TempData error message nếu có lỗi
  - Redirect đến Details

#### Quy Tắc Hủy Đơn Hàng

1. **Không thể hủy đơn đã giao:**
   - Nếu `Status = "Delivered"` → Không cho phép hủy
   - Error message: "Không thể hủy đơn hàng đã được giao."

2. **Không thể hủy đơn đã hủy:**
   - Nếu `Status = "Cancelled"` → Không cho phép hủy lại
   - Error message: "Đơn hàng này đã bị hủy trước đó."

3. **Tự động hoàn tiền:**
   - Nếu đơn hàng đã thanh toán (`PaymentStatus = "Paid"`) → Tự động set `PaymentStatus = "Refunded"`

4. **Ghi lý do hủy:**
   - Lý do hủy được ghi vào `Notes` với format: `[Hủy đơn hàng - {timestamp}]: {reason}`

---

## 🔒 Security & Validation

### Authorization
- **Controller Level:**
  - `[RequirePermission("Admin.Order.Manage")]` - Tất cả actions đều yêu cầu permission này

### Status Validation
- **Status:** Chỉ cho phép các giá trị: "Pending", "Processing", "Shipped", "Delivered", "Cancelled"
- **PaymentStatus:** Chỉ cho phép các giá trị: "Pending", "Paid", "Failed", "Refunded"

### Cancel Validation
- **Không thể hủy đơn đã giao:** `Status != "Delivered"`
- **Không thể hủy đơn đã hủy:** `Status != "Cancelled"`

### Error Handling
- **Try-Catch:** Tất cả actions đều có try-catch
- **Logging:** Sử dụng `ILogger<OrdersController>` để log errors
- **User-Friendly Messages:** TempData messages cho success/error
- **ModelState Errors:** Hiển thị lỗi validation trong form

---

## 📊 Database Models

### Order Model
```csharp
- OrderId (int, PK)
- UserId (int?, FK → User, nullable - có thể là guest)
- OrderNumber (string, unique, required)
- Status (string, required, default: "Pending")
- SubTotal (decimal)
- ShippingFee (decimal, default: 0)
- Discount (decimal, default: 0)
- Total (decimal, required)
- ShippingAddress (string, nullable)
- ShippingMethod (string, nullable)
- PaymentMethod (string, nullable)
- PaymentStatus (string, default: "Pending")
- Notes (string, nullable)
- CreatedAt (DateTime)
- UpdatedAt (DateTime)
- Navigation Properties:
  - User (User?, nullable)
  - OrderItems (ICollection<OrderItem>)
```

### OrderItem Model
```csharp
- OrderItemId (int, PK)
- OrderId (int, FK → Order)
- VariantId (int, FK → ProductVariant)
- ProductName (string, required) - Lưu tên tại thời điểm đặt
- ColorName (string, required) - Lưu tên màu tại thời điểm đặt
- Quantity (int, required)
- UnitPrice (decimal, required)
- Subtotal (decimal, required)
- CreatedAt (DateTime)
- Navigation Properties:
  - Order (Order, required)
  - ProductVariant (ProductVariant, required)
```

---

## 🛠️ Helper Methods

### OrderExists
```csharp
private async Task<bool> OrderExists(int id)
{
    return await _context.Orders.AnyAsync(e => e.OrderId == id);
}
```
- Kiểm tra order có tồn tại
- Sử dụng trong các actions để validate

---

## 📝 API Endpoints Summary

| Method | Route | Action | Mô Tả |
|--------|-------|--------|-------|
| GET | `/Admin/Orders` | Index | Danh sách đơn hàng (có filter, search, pagination) |
| GET | `/Admin/Orders/Details/{id}` | Details | Xem chi tiết đơn hàng |
| GET | `/Admin/Orders/UpdateStatus/{id}` | UpdateStatus | Form cập nhật trạng thái |
| POST | `/Admin/Orders/UpdateStatus/{id}` | UpdateStatus | Cập nhật trạng thái đơn hàng |
| GET | `/Admin/Orders/Cancel/{id}` | Cancel | Form hủy đơn hàng |
| POST | `/Admin/Orders/Cancel/{id}` | CancelConfirmed | Hủy đơn hàng |

---

## 🔄 Workflow Sử Dụng

### Xem Danh Sách Đơn Hàng
1. Admin vào **Admin → Orders**
2. Có thể:
   - **Tìm kiếm:** Nhập OrderNumber, tên khách hàng, email
   - **Lọc theo trạng thái:** Chọn Pending, Processing, Shipped, Delivered, Cancelled
   - **Lọc theo thanh toán:** Chọn Pending, Paid, Failed, Refunded
   - **Kết hợp nhiều filter:** Search + Status + PaymentStatus
3. Kết quả được hiển thị với phân trang

### Xem Chi Tiết Đơn Hàng
1. Từ danh sách, click **"Chi Tiết"** hoặc **"Xem"** trên đơn hàng
2. Hiển thị:
   - Thông tin đơn hàng đầy đủ
   - Thông tin khách hàng
   - Danh sách sản phẩm trong đơn
   - Các nút: "Cập Nhật Trạng Thái", "Hủy Đơn Hàng"

### Cập Nhật Trạng Thái Đơn Hàng
1. Từ trang chi tiết, click **"Cập Nhật Trạng Thái"**
2. Chọn trạng thái mới:
   - Status: Pending, Processing, Shipped, Delivered, Cancelled
   - PaymentStatus (optional): Pending, Paid, Failed, Refunded
3. Click **"Cập Nhật"**
4. **Logic tự động:**
   - Nếu hủy đơn và đã thanh toán → Tự động refund
5. Redirect về trang chi tiết với success message

### Hủy Đơn Hàng
1. Từ trang chi tiết, click **"Hủy Đơn Hàng"**
2. **Validation:**
   - Kiểm tra đơn chưa được giao
   - Kiểm tra đơn chưa bị hủy
3. Nhập lý do hủy (optional)
4. Click **"Xác Nhận Hủy"**
5. **Hệ thống tự động:**
   - Set Status = "Cancelled"
   - Nếu đã thanh toán → Set PaymentStatus = "Refunded"
   - Ghi lý do hủy vào Notes
6. Redirect về trang chi tiết với success message

---

## 📌 Lưu Ý Quan Trọng

### Trạng Thái Đơn Hàng

**Workflow chuẩn:**
```
Pending → Processing → Shipped → Delivered
   ↓
Cancelled (có thể hủy ở bất kỳ giai đoạn nào trước Delivered)
```

**Quy tắc:**
- **Pending:** Đơn hàng mới, chờ xử lý
- **Processing:** Đơn hàng đang được xử lý (đóng gói, chuẩn bị)
- **Shipped:** Đơn hàng đã được gửi đi
- **Delivered:** Đơn hàng đã được giao → **Không thể hủy**
- **Cancelled:** Đơn hàng đã bị hủy → **Không thể hủy lại**

### Trạng Thái Thanh Toán

**Workflow chuẩn:**
```
Pending → Paid
   ↓
Failed (nếu thanh toán thất bại)
   ↓
Refunded (nếu hủy đơn hoặc hoàn tiền)
```

**Quy tắc:**
- **Pending:** Chờ thanh toán
- **Paid:** Đã thanh toán
- **Failed:** Thanh toán thất bại
- **Refunded:** Đã hoàn tiền (tự động khi hủy đơn đã thanh toán)

### Tự Động Hoàn Tiền

- **Khi hủy đơn đã thanh toán:**
  - Tự động set `PaymentStatus = "Refunded"`
  - Không cần thao tác thủ công

### Ghi Chú Đơn Hàng

- **Khi hủy đơn:**
  - Lý do hủy được ghi vào `Notes` với format: `[Hủy đơn hàng - {timestamp}]: {reason}`
  - Nếu không có lý do → Ghi mặc định: "Đơn hàng đã được hủy bởi admin."

### Guest Orders

- **Đơn hàng có thể không có User:**
  - `UserId` có thể là `null` (guest checkout)
  - Trong Details, cần kiểm tra `User != null` trước khi hiển thị thông tin khách hàng

### Filter & Search Combination

- **Có thể kết hợp nhiều filters cùng lúc:**
  - Search + Status
  - Search + PaymentStatus
  - Status + PaymentStatus
  - Search + Status + PaymentStatus
- **Ví dụ:** `/Admin/Orders?search=ORD&status=Pending&paymentStatus=Paid`

### Pagination

- **Mặc định:** 10 items/trang
- **Có thể tùy chỉnh:** `?pageSize=20`
- **Validation:** Page number được validate (không < 1, không > TotalPages)

---

## 🚀 Cải Tiến Có Thể Thêm (Future Enhancements)

1. **Print Invoice:**
   - In hóa đơn đơn hàng
   - Export PDF

2. **Shipping Tracking:**
   - Thêm mã vận đơn (Tracking Number)
   - Cập nhật trạng thái vận chuyển tự động

3. **Email Notifications:**
   - Gửi email khi cập nhật trạng thái
   - Gửi email khi hủy đơn hàng

4. **Order History:**
   - Lịch sử thay đổi trạng thái
   - Audit log cho đơn hàng

5. **Bulk Actions:**
   - Cập nhật trạng thái nhiều đơn cùng lúc
   - Export danh sách đơn hàng

6. **Advanced Filters:**
   - Filter theo ngày tạo (date range)
   - Filter theo giá trị đơn hàng
   - Filter theo phương thức thanh toán

7. **Order Statistics:**
   - Thống kê đơn hàng theo trạng thái
   - Thống kê doanh thu
   - Dashboard đơn hàng

8. **Refund Management:**
   - Quản lý hoàn tiền riêng
   - Lịch sử hoàn tiền

9. **Order Notes:**
   - Thêm/sửa/xóa ghi chú đơn hàng
   - Lịch sử ghi chú

10. **Export Orders:**
    - Export ra Excel/CSV
    - Export theo filter hiện tại

---

## 📄 Files Modified/Created

### Created
- `Areas/Admin/Controllers/OrdersController.cs` - Controller quản lý đơn hàng với đầy đủ tính năng

### Models Used (Existing)
- `Models/Order.cs`
- `Models/OrderItem.cs`
- `Models/User.cs`
- `Models/ProductVariant.cs`
- `Models/Product.cs`

### Services Used
- `Data/ApplicationDbContext.cs` - Database context

---

## ✅ Testing Checklist

- [x] Index với search, filter (status, paymentStatus), pagination
- [x] Details hiển thị đầy đủ thông tin đơn hàng và OrderItems
- [x] UpdateStatus với validation status và paymentStatus
- [x] UpdateStatus tự động refund khi hủy đơn đã thanh toán
- [x] Cancel với validation (không thể hủy đơn đã giao)
- [x] Cancel tự động refund khi hủy đơn đã thanh toán
- [x] Cancel ghi lý do hủy vào Notes
- [x] Error handling và logging
- [x] TempData messages

---

**Tài liệu này mô tả đầy đủ hệ thống quản lý đơn hàng trong Admin area. Tất cả các tính năng đã được implement trong Controller, sẵn sàng cho việc tạo Views.**

