using System;

namespace FreshCareWeb.Models
{
    public class SanPham
    {
        public string MaSP { get; set; }
        public string TenSP { get; set; }
        public int MaDM { get; set; }
        public int MaNCC { get; set; }
        public string DonViTinh { get; set; }
        public decimal GiaBanGoc { get; set; }
        public string MoTa { get; set; }

        // Mở rộng hiển thị tên (Dùng cho giao diện)
        public string TenDM { get; set; }
        public string TenNCC { get; set; }
    }
}
