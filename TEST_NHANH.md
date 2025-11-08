# TEST NHANH - 5 PHÚT

## Bước 1: Chuẩn bị ảnh (30 giây)

Copy ảnh từ folder Maybelline vào:
```
D:\Website_Cosmetics\Website_Cosmetics\Website_Cosmetics\wwwroot\images\products\
```

Ví dụ:
- `maybelline-model-full.jpg` (ảnh người mẫu full mặt)
- `maybelline-lips.webp` (ảnh môi)

---

## Bước 2: Cập nhật Database (2 phút)

1. Mở **SSMS**
2. Mở file: `D:\Website_Cosmetics\Database\01_Add_IsMakeupReference_Column.sql`
3. **Execute** (F5)
4. Mở file: `D:\Website_Cosmetics\Database\QUICK_TEST_INSERT.sql`
5. Làm theo hướng dẫn trong file (tìm ProductId, update/insert)
6. **Execute** (F5)

---

## Bước 3: Chạy API (30 giây)

Mở PowerShell, chạy:
```powershell
cd D:\PSGAN-master
C:\Users\Hoang\VirtualMakeup\.venv\Scripts\python.exe api_server_v2.py
```

Đợi thấy: `✅ Model đã sẵn sàng!`

**GIỮ CỬA SỔ NÀY MỞ!**

---

## Bước 4: Chạy Website (30 giây)

1. Mở **Visual Studio/Rider**
2. Mở solution: `D:\Website_Cosmetics\Website_Cosmetics.sln`
3. Nhấn **Run** (F5)
4. Website mở tại: `http://localhost:5210`

---

## Bước 5: Test (1 phút)

1. Vào `/Products`
2. Chọn sản phẩm đã setup makeup reference
3. Click **"Thử son ảo"**
4. Upload ảnh của bạn
5. Click **"Apply Makeup"**
6. Đợi 10-30 giây
7. Xem kết quả!
8. Có thể download ảnh về

---

## Xong! 🎉

**So sánh:**
- Test với ảnh full mặt vs ảnh chỉ có môi
- Xem loại nào cho kết quả tốt hơn

**Lưu ý:**
- Ảnh kết quả KHÔNG được lưu
- F5 trang web → mất hết
- Xử lý hoàn toàn trong RAM

