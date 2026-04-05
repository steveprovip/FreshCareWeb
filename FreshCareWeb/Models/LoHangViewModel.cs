using System;

namespace FreshCareWeb.Models
{
    public class LoHangViewModel
    {
        public LoHang LoHang { get; set; }

        // Logic tính Số ngày còn lại theo quy tắc 3.1
        public int SoNgayConLai
        {
            get
            {
                var diff = (LoHang.HanSuDung - DateTime.Today).Days;
                return diff < 0 ? 0 : diff; // Chặn ngày âm
            }
        }

        // Logic phân loại màu sắc theo quy tắc 3.1
        public string StatusColor
        {
            get
            {
                int conLai = SoNgayConLai;
                if (conLai == 0) return "danger";  // Đỏ
                if (conLai <= 30) return "warning"; // Cam
                return "success";                 // Xanh
            }
        }

        public string StatusText
        {
            get
            {
                int conLai = SoNgayConLai;
                if (conLai == 0) return "Quá Hạn";
                if (conLai <= 30) return "Cận Date";
                return "An Toàn";
            }
        }
    }
}
