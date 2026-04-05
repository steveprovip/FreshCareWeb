-- Tạo DB mới để thiết kế cấu trúc hoàn chỉnh
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'FreshCareDB_V2')
BEGIN
    ALTER DATABASE FreshCareDB_V2 SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE FreshCareDB_V2;
END
GO

CREATE DATABASE FreshCareDB_V2;
GO
USE FreshCareDB_V2;
GO

-- 1. Bảng Nhân Viên (UC-01, UC-08)
CREATE TABLE dbo.NhanVien(
    MaNV VARCHAR(20) PRIMARY KEY,
    HoTen NVARCHAR(100) NOT NULL,
    TenDangNhap VARCHAR(50) UNIQUE NOT NULL,
    MatKhau VARCHAR(255) NOT NULL,
    VaiTro NVARCHAR(50) NOT NULL -- Quản lý / Nhân viên
);
GO

-- 2. Bảng Danh Mục
CREATE TABLE dbo.DanhMuc(
    MaDM INT IDENTITY(1,1) PRIMARY KEY,
    TenDM NVARCHAR(100) NOT NULL,
    PhanTramSale FLOAT DEFAULT 0 -- Quy tắc 2.2 luật code
);
GO

-- 3. Bảng Nhà Cung Cấp (Theo yêu cầu update biểu đồ)
CREATE TABLE dbo.NhaCungCap(
    MaNCC INT IDENTITY(1,1) PRIMARY KEY,
    TenNCC NVARCHAR(100) NOT NULL,
    SoDienThoai VARCHAR(20),
    Email VARCHAR(50),
    DiaChi NVARCHAR(255)
);
GO

-- 4. Bảng Sản Phẩm
CREATE TABLE dbo.SanPham(
    MaSP VARCHAR(20) PRIMARY KEY,
    TenSP NVARCHAR(100) NOT NULL,
    MaDM INT FOREIGN KEY REFERENCES dbo.DanhMuc(MaDM),
    MaNCC INT FOREIGN KEY REFERENCES dbo.NhaCungCap(MaNCC),
    DonViTinh NVARCHAR(20) NOT NULL, -- Kg, Bó, Hộp
    GiaBanGoc DECIMAL(18,2) NOT NULL,
    MoTa NVARCHAR(500)
);
GO

-- 5. Bảng Lô Hàng (Trung tâm của hệ thống - Batch)
-- Trạng thái: 'An Toàn', 'Cận Date', 'Quá Hạn', 'Đã Hủy' (Tuyệt đối không có lệnh DELETE)
CREATE TABLE dbo.LoHang(
    MaLo VARCHAR(50) PRIMARY KEY, -- Sinh tự động từ C# ứng dụng (Guid/Prefix)
    MaSP VARCHAR(20) FOREIGN KEY REFERENCES dbo.SanPham(MaSP),
    NgaySanXuat DATE NOT NULL,
    HanSuDung DATE NOT NULL,
    SoLuongBanDau FLOAT NOT NULL,
    SoLuongTon FLOAT NOT NULL,
    NgayNhapKho DATETIME DEFAULT GETDATE(),
    TrangThai NVARCHAR(50) DEFAULT N'An Toàn',
    
    CONSTRAINT CHK_HanSuDung CHECK (HanSuDung >= NgaySanXuat)
);
GO

-- 6. Bảng Phiếu Nhập Kho (Quản lý nhập hàng)
CREATE TABLE dbo.PhieuNhapKho(
    MaPhieuNhap VARCHAR(50) PRIMARY KEY,
    NgayNhap DATETIME DEFAULT GETDATE(),
    MaNV VARCHAR(20) FOREIGN KEY REFERENCES dbo.NhanVien(MaNV),
    GhiChu NVARCHAR(255)
);
GO

-- 7. Bảng Chi Tiết Nhập
CREATE TABLE dbo.ChiTietNhap(
    MaPhieuNhap VARCHAR(50) FOREIGN KEY REFERENCES dbo.PhieuNhapKho(MaPhieuNhap),
    MaLo VARCHAR(50) FOREIGN KEY REFERENCES dbo.LoHang(MaLo),
    SoLuong FLOAT NOT NULL,
    PRIMARY KEY(MaPhieuNhap, MaLo)
);
GO

-- 8. Bảng Phiếu Xuất (Bán hàng hoặc Hủy - Định nghĩa rõ Doanh Thu và Thất Thoát)
CREATE TABLE dbo.PhieuXuat(
    MaPhieuXuat VARCHAR(50) PRIMARY KEY,
    NgayXuat DATETIME DEFAULT GETDATE(),
    LoaiPhieu NVARCHAR(50) NOT NULL, -- 'Bán Hàng', 'Hủy Hàng'
    MaNV VARCHAR(20) FOREIGN KEY REFERENCES dbo.NhanVien(MaNV),
    TongTien DECIMAL(18,2) DEFAULT 0
);
GO

-- 9. Bảng Chi Tiết Xuất (Từng mảnh xuất của lô nào theo quy tắc FIFO)
CREATE TABLE dbo.ChiTietXuat(
    MaPhieuXuat VARCHAR(50) FOREIGN KEY REFERENCES dbo.PhieuXuat(MaPhieuXuat),
    MaLo VARCHAR(50) FOREIGN KEY REFERENCES dbo.LoHang(MaLo),
    SoLuong FLOAT NOT NULL,
    DonGia DECIMAL(18,2) DEFAULT 0, -- Giá xuất lúc đó sau khi tính toán giảm giá
    PRIMARY KEY (MaPhieuXuat, MaLo)
);
GO

-- Nạp Data Mẫu
INSERT INTO dbo.NhanVien (MaNV, HoTen, TenDangNhap, MatKhau, VaiTro) VALUES 
('NV01', N'Nguyễn Văn Quản Lý', 'admin', '123456', N'Quản lý'),
('NV02', N'Trần Thị Bán Hàng', 'staff1', '123456', N'Nhân viên');
GO

INSERT INTO dbo.DanhMuc (TenDM, PhanTramSale) VALUES 
(N'Rau Củ Quả Sạch', 50),
(N'Thịt Cá Tươi Sống', 30),
(N'Hạt Hữu Cơ', 10);
GO

INSERT INTO dbo.NhaCungCap (TenNCC, SoDienThoai) VALUES 
(N'Trang Trại Rau Sạch Thái Nguyên', '0987654321'),
(N'Hợp Tác Xã Chăn Nuôi Sạch', '0912345678');
GO

INSERT INTO dbo.SanPham (MaSP, TenSP, MaDM, MaNCC, DonViTinh, GiaBanGoc) VALUES 
('SP01', N'Rau muống hữu cơ', 1, 1, N'Bó', 15000),
('SP02', N'Thịt lợn sạch', 2, 2, N'Kg', 150000),
('SP03', N'Gạo ST25', 3, 1, N'Kg', 35000);
GO
