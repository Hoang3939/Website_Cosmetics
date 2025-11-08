# HƯỚNG DẪN ĐỔI TÊN FILE ẢNH

## Vị trí file:
```
D:\Website_Cosmetics\Website_Cosmetics\Website_Cosmetics\wwwroot\public\images\products\
```

## Đổi tên như sau:

### 1. Ảnh cây son (Cover)
**Tên cũ:**
```
cây son Super Stay Matte Ink Lipstick #135 - Globetrotter.jpg
```
**→ Tên mới:**
```
cay-son-superstay-135.jpg
```

---

### 2. Ảnh người mẫu full mặt (Makeup Reference)
**Tên cũ:**
```
ảnh người mẫu dùng son Super Stay Matte Ink Lipstick #135 - Globetrotter.jpg
```
**→ Tên mới:**
```
nguoi-mau-superstay-135.jpg
```

---

### 3. Ảnh môi mẫu (Makeup Reference)
**Tên cũ:**
```
ảnh môi mẫu Super Stay Matte Ink Lipstick #135 - Globetrotter.jpg
```
**→ Tên mới:**
```
moi-mau-superstay-135.jpg
```

---

## Hoặc dùng PowerShell để đổi tên tự động:

```powershell
cd "D:\Website_Cosmetics\Website_Cosmetics\Website_Cosmetics\wwwroot\public\images\products\"

# Đổi tên 3 file
Rename-Item "cây son Super Stay Matte Ink Lipstick #135 - Globetrotter.jpg" "cay-son-superstay-135.jpg"
Rename-Item "ảnh người mẫu dùng son Super Stay Matte Ink Lipstick #135 - Globetrotter.jpg" "nguoi-mau-superstay-135.jpg"
Rename-Item "ảnh môi mẫu Super Stay Matte Ink Lipstick #135 - Globetrotter.jpg" "moi-mau-superstay-135.jpg"
```

---

## Sau khi đổi tên xong:

1. Mở **SSMS**
2. Mở file: `D:\Website_Cosmetics\Database\03_Insert_Maybelline_Product10_Images.sql`
3. **Execute** (F5)
4. Kiểm tra kết quả trong bảng ProductImage

---

## Lưu ý:
- Không dùng dấu tiếng Việt trong tên file
- Không có khoảng trắng (dùng dấu `-`)
- Chữ thường hết

