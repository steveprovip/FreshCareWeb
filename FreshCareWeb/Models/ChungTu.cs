using System;
using System.Collections.Generic;

namespace FreshCareWeb.Models
{
    public class PhieuNhapKho
    {
        public string MaPhieuNhap { get; set; }
        public DateTime NgayNhap { get; set; }
        public string MaNV { get; set; }
        public string GhiChu { get; set; }
        
        public string TenNV { get; set; } // Hiển thị
        public List<ChiTietNhap> ChiTiets { get; set; } = new List<ChiTietNhap>();
    }

    public class ChiTietNhap
    {
        public string MaPhieuNhap { get; set; }
        public string MaLo { get; set; }
        public double SoLuong { get; set; }
    }

    public class PhieuXuat
    {
        public string MaPhieuXuat { get; set; }
        public DateTime NgayXuat { get; set; }
        public string LoaiPhieu { get; set; } // Bán Hàng, Hủy Hàng
        public string MaNV { get; set; }
        public decimal TongTien { get; set; }
        
        public string TenNV { get; set; } // Hiển thị
        public List<ChiTietXuat> ChiTiets { get; set; } = new List<ChiTietXuat>();
    }

    public class ChiTietXuat
    {
        public string MaPhieuXuat { get; set; }
        public string MaLo { get; set; }
        public double SoLuong { get; set; }
        public decimal DonGia { get; set; }
    }
}
