AromaCafe Management System - Backend API
Hệ thống Backend quản lý quán cà phê tích hợp quản lý kho tự động, tích điểm thành viên và báo cáo doanh thu. Dự án được xây dựng trên nền tảng ASP.NET Core Web API sử dụng Dapper để tối ưu hóa hiệu suất truy vấn MySQL.

🛠 Công nghệ sử dụng
Framework: .NET Core API.

Database: MySQL.

ORM: Dapper (Micro-ORM).

Authentication: SHA256 Password Hashing.

📋 Hướng dẫn cài đặt
1. Cấu hình Cơ sở dữ liệu
Mở công cụ quản lý MySQL (Workbench, HeidiSQL, ...).

Tạo database mới tên là AromaCafeDB.

Chạy toàn bộ nội dung trong tệp database.sql để tạo các bảng và ràng buộc.

Lưu ý: Chạy thêm lệnh bổ sung cột email cho khách hàng:

SQL

ALTER TABLE customers ADD COLUMN email VARCHAR(100) AFTER phone_number;
2. Cấu hình Ứng dụng
Mở tệp appsettings.json và cập nhật chuỗi kết nối phù hợp với môi trường của bạn:

JSON

"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Port=3306;Database=aromacafedb;Uid=ROOT_USER;Pwd=YOUR_PASSWORD;AllowUserVariables=True"
}
3. Khởi chạy
Mở Solution bằng Visual Studio hoặc VS Code.

Chạy lệnh dotnet run hoặc nhấn F5.

Truy cập https://localhost:PORT/swagger để xem tài liệu API đầy đủ.

🚀 Các tính năng chính & API tiêu biểu
🔐 Xác thực & Người dùng (/api/Auth)
Đăng nhập/Đăng ký nhân viên: Hỗ trợ phân quyền Admin/Staff.

Khách hàng thành viên: Đăng ký tài khoản dành riêng cho khách hàng để tích điểm.

📦 Quản lý Kho (/api/Inventory)
Giao dịch Kho: Tự động ghi nhật ký nhập/xuất kèm theo giá và nhân viên thực hiện.

Cảnh báo tồn kho: Tự động tính toán trạng thái IsLowStock khi hàng xuống dưới mức tối thiểu.

📝 Đơn hàng & Thanh toán (/api/Orders)
Xử lý Đơn hàng: Tạo đơn hàng kèm danh sách nhiều món (Details).

Tự động hóa:

Tự động trừ nguyên liệu trong kho dựa trên công thức (Recipes) khi đơn hàng hoàn tất.

Tự động tích điểm cho khách hàng (10.000 VNĐ = 1 điểm).

Tự động cập nhật trạng thái bàn (Trống/Có người).

📊 Báo cáo & Thống kê
Báo cáo kho: Thống kê tồn đầu kỳ, nhập, xuất và tồn cuối kỳ theo tháng.

Báo cáo chi phí: Thống kê số tiền đã chi trả cho từng nhà cung cấp.

💡 Lưu ý cho Front-end
Trạng thái bàn: Sử dụng các giá trị Available, Occupied, Reserved.

Định dạng tiền tệ: Toàn bộ giá trị tiền tệ sử dụng kiểu decimal để đảm bảo độ chính xác.

Search: Hầu hết các Controller đều hỗ trợ endpoint /search?q=keyword để tìm kiếm nhanh.
