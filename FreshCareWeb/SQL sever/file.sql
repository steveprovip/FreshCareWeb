-- ========================================================
-- BƯỚC 1: ĐẢM BẢO DATABASE TỒN TẠI VÀ CHỌN ĐÚNG DATABASE
-- ========================================================
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'FreshCareDB')
BEGIN
    CREATE DATABASE FreshCareDB;
END
GO

USE FreshCareDB;
GO

-- ========================================================
-- BƯỚC 2: XÓA SẠCH DỮ LIỆU VÀ BẢNG CŨ (NẾU CÓ)
-- (Lưu ý: Phải xóa bảng con trước, bảng cha sau để không lỗi khóa ngoại)
-- ========================================================
IF OBJECT_ID('dbo.ChiTietXuat', 'U') IS NOT NULL DROP TABLE dbo.ChiTietXuat;
IF OBJECT_ID('dbo.PhieuXuat', 'U') IS NOT NULL DROP TABLE dbo.PhieuXuat;
IF OBJECT_ID('dbo.LoHang', 'U') IS NOT NULL DROP TABLE dbo.LoHang;
IF OBJECT_ID('dbo.SanPham', 'U') IS NOT NULL DROP TABLE dbo.SanPham;
IF OBJECT_ID('dbo.DanhMuc', 'U') IS NOT NULL DROP TABLE dbo.DanhMuc;
GO

-- ========================================================
-- BƯỚC 3: TẠO LẠI CÁC BẢNG MỚI TINH TỪ ĐẦU
-- ========================================================

-- 3.1. Bảng Danh Mục
CREATE TABLE DanhMuc (
    MaDM INT IDENTITY(1,1) PRIMARY KEY,
    TenDM NVARCHAR(100) NOT NULL,
    PhanTramSale INT DEFAULT 0 
);

-- 3.2. Bảng Sản Phẩm
CREATE TABLE SanPham (
    MaSP VARCHAR(20) PRIMARY KEY,
    MaDM INT FOREIGN KEY REFERENCES DanhMuc(MaDM),
    TenSP NVARCHAR(150) NOT NULL,
    DonViTinh NVARCHAR(50) NOT NULL,
    GiaBan DECIMAL(18,0) NOT NULL
);

-- 3.3. Bảng Lô Hàng
CREATE TABLE LoHang (
    MaLo VARCHAR(50) PRIMARY KEY,
    MaSP VARCHAR(20) FOREIGN KEY REFERENCES SanPham(MaSP),
    NgaySanXuat DATE NOT NULL,
    HanSuDung DATE NOT NULL,
    SoLuongBanDau FLOAT NOT NULL,
    SoLuongTon FLOAT NOT NULL,
    TrangThai NVARCHAR(50) DEFAULT N'An Toàn'
);

-- 3.4. Bảng Lịch Sử Xuất/Hủy
CREATE TABLE PhieuXuat (
    MaPX INT IDENTITY(1,1) PRIMARY KEY,
    LoaiPhieu NVARCHAR(50) NOT NULL, 
    NgayXuat DATETIME DEFAULT GETDATE(),
    GhiChu NVARCHAR(255)
);

CREATE TABLE ChiTietXuat (
    MaPX INT FOREIGN KEY REFERENCES PhieuXuat(MaPX),
    MaLo VARCHAR(50) FOREIGN KEY REFERENCES LoHang(MaLo),
    SoLuong FLOAT NOT NULL,
    GiaXuat DECIMAL(18,0) NOT NULL, 
    ThanhTien DECIMAL(18,0) NOT NULL,
    PRIMARY KEY (MaPX, MaLo)
);
GO

-- ========================================================
-- BƯỚC 4: BƠM DỮ LIỆU MẪU ĐỂ CHẠY WEB
-- ========================================================
INSERT INTO DanhMuc (TenDM, PhanTramSale) VALUES 
(N'Rau củ', 50), 
(N'Thịt cá', 30), 
(N'Đồ khô', 10);

INSERT INTO SanPham (MaSP, MaDM, TenSP, DonViTinh, GiaBan) VALUES 
('SP01', 1, N'Rau muống VietGAP', N'Bó', 10000),
('SP02', 2, N'Thịt ba chỉ heo', N'Kg', 120000);

INSERT INTO LoHang (MaLo, MaSP, NgaySanXuat, HanSuDung, SoLuongBanDau, SoLuongTon, TrangThai) VALUES 
('LO_001', 'SP01', '2026-03-01', '2026-03-25', 100, 100, N'Quá Hạn'),
('LO_002', 'SP02', '2026-03-25', '2026-04-10', 50, 50, N'Cận Date'),
('LO_003', 'SP01', '2026-04-01', '2026-04-20', 200, 200, N'An Toàn');
GO
USE FreshCareDB;
GO
CREATE TABLE NguoiDung (
    TaiKhoan VARCHAR(50) PRIMARY KEY,
    MatKhau VARCHAR(255) NOT NULL,
    HoTen NVARCHAR(100) NOT NULL,
    VaiTro NVARCHAR(50) DEFAULT 'Admin'
);
INSERT INTO NguoiDung (TaiKhoan, MatKhau, HoTen) VALUES ('admin', '123', N'Quản Trị Viên');
GO