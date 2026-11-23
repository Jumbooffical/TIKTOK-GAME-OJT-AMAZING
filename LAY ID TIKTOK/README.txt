====================================================
  TIKTOK LIVE DASHBOARD — HƯỚNG DẪN CÀI ĐẶT
====================================================

YÊU CẦU:
  - Máy tính Windows
  - Python 3.9 trở lên (xem bên dưới để tải)
  - Kết nối Internet

----------------------------------------------------
BƯỚC 1: CÀI PYTHON (nếu chưa có)
----------------------------------------------------
1. Vào trang: https://www.python.org/downloads/
2. Nhấn "Download Python 3.x.x" (bản mới nhất)
3. Chạy file tải về
4. ⚠️ QUAN TRỌNG: Tích vào ô "Add Python to PATH" trước khi nhấn Install

Kiểm tra cài thành công: mở CMD, gõ lệnh:
  py --version
  → Phải hiện "Python 3.x.x"

----------------------------------------------------
BƯỚC 2: CHẠY CHƯƠNG TRÌNH
----------------------------------------------------
1. Mở thư mục này
2. Double-click vào file: start.bat
3. Lần đầu chạy sẽ tự tải thư viện (~1-2 phút)
4. Trình duyệt tự mở tại: http://127.0.0.1:8000

----------------------------------------------------
CÁCH SỬ DỤNG
----------------------------------------------------
1. Nhập username TikTok vào ô tìm kiếm (vd: abc_streamer)
   - Người đó phải đang LIVESTREAM thì mới kết nối được
2. Nhấn nút "Kết nối"
3. Dữ liệu live sẽ hiển thị theo thời gian thực:
   - Tab Real-time : tất cả sự kiện
   - Tab Bình luận : danh sách bình luận
   - Tab Quà tặng  : danh sách quà + diamonds
   - Tab Vào phòng : ai vào lúc mấy giờ
   - Tab Lịch sử   : xem lại dữ liệu các phiên cũ
4. Nhấn "Dừng" để ngừng theo dõi

----------------------------------------------------
LƯU Ý
----------------------------------------------------
- Dữ liệu được lưu tự động vào file tiktok_live.db
- Mỗi lần kết nối live mới thì dữ liệu trên màn hình
  sẽ được xóa, nhưng vẫn còn trong Tab Lịch sử
- Nhấn Ctrl+C trong cửa sổ CMD để tắt server

====================================================
