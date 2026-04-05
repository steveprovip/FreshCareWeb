-- Task 1: Cập nhật schema bảng LoHang theo quy tắc luat-code.md
USE FreshCareDB;
GO

-- Xóa bảng cũ nếu cần để đồng bộ (Lưu ý: Chỉ dùng trong môi trường dev/test)
IF OBJECT_ID('dbo.LoHang', 'U') IS NOT NULL
DROP TABLE dbo.LoHang;
GO

CREATE TABLE dbo.LoHang(
    MaLo varchar(50) NOT NULL PRIMARY KEY, -- MaLo là chuỗi theo quy tắc 2.3
    MaSP varchar(20) NOT NULL,              -- MaSP là chuỗi theo quy tắc 2.3
    NgaySanXuat date NOT NULL,
    HanSuDung date NOT NULL,
    SoLuongBanDau float NOT NULL,           -- float để hỗ trợ đơn vị 'Kg' theo quy tắc 4.2
    SoLuongTon float NOT NULL,
    NgayNhap datetime DEFAULT GETDATE(),
    
    CONSTRAINT FK_LoHang_SanPham FOREIGN KEY (MaSP) REFERENCES dbo.SanPham(MaSP)
);
GO

-- Thêm dữ liệu mẫu kiểm thử
INSERT INTO dbo.LoHang (MaLo, MaSP, NgaySanXuat, HanSuDung, SoLuongBanDau, SoLuongTon)
VALUES 
('L001', 'SP01', '2026-01-01', '2026-03-01', 100, 50), -- Đã hết hạn (SoNgayConLai = 0)
('L002', 'SP02', '2026-03-20', '2026-04-15', 50, 50),  -- Cận date (Khoảng 10 ngày)
('L003', 'SP01', '2026-04-01', '2026-12-31', 200, 200); -- An toàn
GO
