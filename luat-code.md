1\. QUY TẮC CÔNG NGHỆ CHỦ ĐẠO (TECH STACK)

Ngôn ngữ: C# (.NET Core MVC).



Database: SQL Server 2022.



Truy cập dữ liệu: BẮT BUỘC dùng ADO.NET (SqlConnection, SqlCommand, SqlDataReader). KHÔNG dùng Entity Framework Core để đảm bảo tính minh bạch theo yêu cầu báo cáo.



Giao diện: Bootstrap 5, FontAwesome 6, jQuery AJAX (để quét mã vạch không load trang).



2\. QUY TẮC CƠ SỞ DỮ LIỆU (DATABASE RULES)

Nguyên tắc "Dữ liệu vĩnh viễn": Tuyệt đối không dùng lệnh DELETE. Mọi thao tác xóa hàng lỗi/hỏng phải chuyển TrangThai thành 'Đã Hủy'.



Phân cấp Danh mục: Bảng DanhMuc phải chứa PhanTramSale để hỗ trợ tính giá tự động.



Đơn vị tính (UoM): Mọi sản phẩm phải có trường DonViTinh.



Tính toàn vẹn: MaSP và MaLo phải là khóa chính dạng chuỗi (String) để hỗ trợ quét mã vạch thực tế.



3\. QUY TẮC LOGIC NGHIỆP VỤ (BUSINESS LOGIC)

3.1. Quản lý Hạn sử dụng (HSD)

Chặn ngày âm: Logic tính toán SoNgayConLai = HanSuDung - Today. Nếu kết quả < 0, phải gán cứng bằng 0. Không được hiển thị ngày âm trên giao diện.



Phân loại màu sắc:



ConLai == 0: Trạng thái "Quá Hạn" -> Màu Đỏ -> Khóa nút Bán, chỉ hiện nút Hủy.



0 < ConLai <= 30: Trạng thái "Cận Date" -> Màu Cam -> Kích hoạt cơ chế Auto-Sale.



ConLai > 30: Trạng thái "An Toàn" -> Màu Xanh.



3.2. Thuật toán Xuất kho (FIFO/FEFO)

Ưu tiên xuất lô có HanSuDung gần nhất trước (ORDER BY HanSuDung ASC).



Chỉ được phép xuất những lô có TrangThai = N'An Toàn' hoặc N'Cận Date'.



3.3. Cơ chế Giá \& Sale

Giá thực tế khi bán = GiaBanGoc \* (100 - PhanTramSale) / 100.



Chỉ áp dụng Sale cho hàng "Cận Date".



4\. QUY TẮC GIAO DIỆN \& TRẢI NGHIỆM (UI/UX)

Nghiệp vụ Bán hàng: Ô nhập Mã sản phẩm phải sử dụng sự kiện onchange (AJAX) để máy quét mã vạch bắn vào là hiện thông tin ngay lập tức.



Ràng buộc nhập liệu theo Đơn vị tính:



Nếu Đơn vị là "Bó", "Hộp", "Cái": Chỉ cho phép nhập số nguyên (Integer).



Nếu Đơn vị là "Kg": Cho phép nhập số thập phân (Decimal/Float).



Thanh Menu: Phải cố định hàng ngang (Navbar), phân định rõ: Trang Chủ, Quản Lý Sản Phẩm, Báo Cáo Doanh Thu, Tài Khoản.



5\. QUY TẮC ĐẶT TÊN \& CẤU TRÚC CODE (CLEAN CODE)

Controller: Luôn bọc các thao tác SQL trong khối using (SqlConnection...) để tránh rò rỉ bộ nhớ.



ViewBag/TempData: Dùng TempData cho các thông báo Toast (Thành công/Lỗi). Dùng ViewBag để truyền danh sách hiển thị.



SqlParameters: Tuyệt đối không cộng chuỗi SQL. Phải dùng @Parameters để chống tấn công SQL Injection.



6\. QUY TẮC BÁO CÁO (REPORTING)

Doanh thu: Chỉ tính tổng tiền từ các PhieuXuat có loại là 'Bán Hàng'.



Thất thoát: Tính tổng tiền (giá gốc) của các PhieuXuat có loại là 'Hủy Hàng'.

