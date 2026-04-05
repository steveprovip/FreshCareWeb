using System;

namespace FreshCareWeb.Models
{
    public class LoHang
    {
        public string MaLo { get; set; }
        public string MaSP { get; set; }
        public DateTime NgaySanXuat { get; set; }
        public DateTime HanSuDung { get; set; }
        public double SoLuongBanDau { get; set; }
        public double SoLuongTon { get; set; }
        public DateTime NgayNhapKho { get; set; }
        public string TrangThai { get; set; }

        // Field mở rộng cho hiển thị
        public string TenSP { get; set; }
        public string DonViTinh { get; set; }
        public double PhanTramSale { get; set; }
        public decimal GiaBanGoc { get; set; }
        
        // Property tính toán HSD
        public int SoNgayConLai 
        {
            get 
            {
                var days = (int)(HanSuDung.Date - DateTime.Now.Date).TotalDays;
                return days < 0 ? 0 : days;
            }
        }
        
        // Property tính Giá Thực Tế dự kiến
        public decimal GiaThucTe 
        {
            get 
            {
                if (TrangThai == "Cận Date")
                {
                    return GiaBanGoc * (decimal)(100 - PhanTramSale) / 100m;
                }
                return GiaBanGoc;
            }
        }
    }
}