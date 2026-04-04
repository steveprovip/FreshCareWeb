-- 1. Xóa DB cũ nếu đang tồn tại để làm sạch
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'FreshCareDB')
BEGIN
    ALTER DATABASE FreshCareDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE FreshCareDB;
END
GO

-- 2. Tạo DB mới
CREATE DATABASE FreshCareDB;
GO
USE FreshCareDB;
GO

-- 3. Tạo bảng Sản Phẩm
CREATE TABLE dbo.SanPham(
	MaSP varchar(20) NOT NULL PRIMARY KEY,
	TenSP nvarchar(100) NOT NULL,
	DonViTinh nvarchar(20) NULL
);
GO

-- 4. Tạo bảng Lô Hàng
CREATE TABLE dbo.LoHang(
	MaLo varchar(50) NOT NULL PRIMARY KEY,
	MaSP varchar(20) NULL FOREIGN KEY REFERENCES dbo.SanPham(MaSP),
	NgaySanXuat date NOT NULL,
	HanSuDung date NOT NULL,
	SoLuongBanDau float NOT NULL,
	SoLuongTon float NOT NULL
);
GO

-- 5. Bơm dữ liệu mẫu
INSERT INTO dbo.SanPham (MaSP, TenSP, DonViTinh) VALUES 
('SP01', N'Cà chua Hữu cơ', N'Kg'),
('SP02', N'Rau muống', N'Bó');
GO

INSERT INTO dbo.LoHang (MaLo, MaSP, NgaySanXuat, HanSuDung, SoLuongBanDau, SoLuongTon) VALUES 
('LO_001', 'SP01', '2026-03-01', '2026-03-25', 100, 100), -- Hàng hết hạn (Đỏ)
('LO_002', 'SP02', '2026-03-20', '2026-04-10', 50, 50),   -- Hàng cận hạn (Cam)
('LO_003', 'SP01', '2026-03-25', '2026-08-01', 200, 200); -- Hàng an toàn (Xanh)
GO