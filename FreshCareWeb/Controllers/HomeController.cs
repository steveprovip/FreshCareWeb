using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Collections.Generic;
using System;

namespace FreshCareWeb.Controllers
{
    // Lớp Model dùng để chứa dữ liệu truyền ra giao diện
    public class LoHangViewModel
    {
        public string MaLo { get; set; }
        public string MaSP { get; set; }
        public string TenSP { get; set; }
        public string DonViTinh { get; set; } // Kg, Bó, Hộp...
        public string TenDM { get; set; }
        public DateTime NgaySanXuat { get; set; }
        public DateTime HanSuDung { get; set; }
        public double SoLuongTon { get; set; }
        public decimal GiaBanGoc { get; set; }
        public int PhanTramSale { get; set; }
        public string TrangThai { get; set; }
        public int SoNgayConLai { get; set; }

        // Tự động tính giá Sale: Nếu cận date có % sale thì giảm giá, không thì giữ nguyên
        public decimal GiaThucTe => TrangThai == "Cận Date" ? GiaBanGoc * (100 - PhanTramSale) / 100 : GiaBanGoc;
    }

    public class HomeController : Controller
    {
        // 🚨 NHỚ ĐỔI TÊN SERVER CỦA BẠN Ở ĐÂY
        string connectionString = @"Data Source=Admin;Initial Catalog=FreshCareDB;Integrated Security=True;TrustServerCertificate=True;";

        // =========================================================
        // 1. HÀM HIỂN THỊ TRANG CHỦ & CẬP NHẬT TRẠNG THÁI TỰ ĐỘNG
        // =========================================================
        public IActionResult Index()
        {
            List<LoHangViewModel> hangQuaHan = new List<LoHangViewModel>();
            List<LoHangViewModel> hangCanDate = new List<LoHangViewModel>();
            List<LoHangViewModel> hangAnToan = new List<LoHangViewModel>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                // Lấy các lô hàng chưa bị hủy và còn tồn kho
                string query = @"
                    SELECT l.MaLo, l.MaSP, s.TenSP, s.DonViTinh, s.GiaBan, d.TenDM, d.PhanTramSale, 
                           l.NgaySanXuat, l.HanSuDung, l.SoLuongTon, l.TrangThai
                    FROM LoHang l
                    JOIN SanPham s ON l.MaSP = s.MaSP
                    JOIN DanhMuc d ON s.MaDM = d.MaDM
                    WHERE l.TrangThai != N'Đã Hủy' AND l.SoLuongTon > 0";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var lo = new LoHangViewModel
                            {
                                MaLo = reader["MaLo"].ToString(),
                                MaSP = reader["MaSP"].ToString(),
                                TenSP = reader["TenSP"].ToString(),
                                DonViTinh = reader["DonViTinh"].ToString(),
                                TenDM = reader["TenDM"].ToString(),
                                NgaySanXuat = Convert.ToDateTime(reader["NgaySanXuat"]),
                                HanSuDung = Convert.ToDateTime(reader["HanSuDung"]),
                                SoLuongTon = Convert.ToDouble(reader["SoLuongTon"]),
                                GiaBanGoc = Convert.ToDecimal(reader["GiaBan"]),
                                PhanTramSale = Convert.ToInt32(reader["PhanTramSale"]),
                                TrangThai = reader["TrangThai"].ToString()
                            };

                            // TÍNH TOÁN VÀ CẬP NHẬT TRẠNG THÁI THEO THỜI GIAN THỰC
                            TimeSpan thoiGian = lo.HanSuDung.Date - DateTime.Now.Date;
                            lo.SoNgayConLai = thoiGian.Days;

                            if (lo.SoNgayConLai < 0)
                            {
                                lo.TrangThai = "Quá Hạn";
                                lo.SoNgayConLai = 0; // Giảng viên yêu cầu: Không để ngày âm
                                hangQuaHan.Add(lo);
                            }
                            else if (lo.SoNgayConLai <= 30)
                            {
                                lo.TrangThai = "Cận Date";
                                hangCanDate.Add(lo);
                            }
                            else
                            {
                                lo.TrangThai = "An Toàn";
                                hangAnToan.Add(lo);
                            }
                        }
                    }
                }

                // Cập nhật lại trạng thái "Quá Hạn" vào CSDL để khóa bán hàng
                foreach (var item in hangQuaHan)
                {
                    SqlCommand updateCmd = new SqlCommand("UPDATE LoHang SET TrangThai = N'Quá Hạn' WHERE MaLo = @MaLo", conn);
                    updateCmd.Parameters.AddWithValue("@MaLo", item.MaLo);
                    updateCmd.ExecuteNonQuery();
                }
            }

            ViewBag.HangQuaHan = hangQuaHan;
            ViewBag.HangCanDate = hangCanDate;
            ViewBag.HangAnToan = hangAnToan;
            return View();
        }

        // =========================================================
        // 2. HÀM HỦY HÀNG (KHÔNG XÓA CSDL - CHỈ ĐỔI TRẠNG THÁI)
        // =========================================================
        [HttpPost]
        public IActionResult HuyHang(string maLo)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                // B1: Ghi vào bảng Phiếu Xuất (Loại: Hủy Hàng)
                string insertPhieu = "INSERT INTO PhieuXuat (LoaiPhieu, GhiChu) OUTPUT INSERTED.MaPX VALUES (N'Hủy Hàng', N'Hủy lô hàng quá hạn/hỏng');";
                SqlCommand cmdPhieu = new SqlCommand(insertPhieu, conn);
                int maPX = (int)cmdPhieu.ExecuteScalar();

                // B2: Ghi chi tiết hủy và đổi trạng thái lô hàng
                string huyQuery = @"
                    INSERT INTO ChiTietXuat (MaPX, MaLo, SoLuong, GiaXuat, ThanhTien) 
                    SELECT @MaPX, MaLo, SoLuongTon, 0, 0 FROM LoHang WHERE MaLo = @MaLo;

                    UPDATE LoHang SET TrangThai = N'Đã Hủy', SoLuongTon = 0 WHERE MaLo = @MaLo;";

                SqlCommand cmdHuy = new SqlCommand(huyQuery, conn);
                cmdHuy.Parameters.AddWithValue("@MaPX", maPX);
                cmdHuy.Parameters.AddWithValue("@MaLo", maLo); // <--- ĐÃ SỬA CHỮ cmd THÀNH cmdHuy
                cmdHuy.ExecuteNonQuery();
            }
            TempData["MessageType"] = "success";
            TempData["Message"] = "Đã hủy lô hàng và lưu vào lịch sử, không xóa dữ liệu gốc!";
            return RedirectToAction("Index");
        }
        // =========================================================
        // 3. HÀM BÁO CÁO THỐNG KÊ (DOANH THU & HÀNG HỦY)
        // =========================================================
        public IActionResult BaoCao()
        {
            decimal tongDoanhThu = 0;
            double tongHangHuy = 0;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                // Lấy tổng doanh thu (Chỉ cộng những phiếu Bán Hàng)
                string queryDoanhThu = "SELECT SUM(ThanhTien) FROM ChiTietXuat ctx JOIN PhieuXuat px ON ctx.MaPX = px.MaPX WHERE px.LoaiPhieu = N'Bán Hàng'";
                object resultDT = new SqlCommand(queryDoanhThu, conn).ExecuteScalar();
                tongDoanhThu = resultDT != DBNull.Value ? Convert.ToDecimal(resultDT) : 0;

                // Lấy tổng lượng hàng hủy
                string queryHuy = "SELECT SUM(SoLuong) FROM ChiTietXuat ctx JOIN PhieuXuat px ON ctx.MaPX = px.MaPX WHERE px.LoaiPhieu = N'Hủy Hàng'";
                object resultHuy = new SqlCommand(queryHuy, conn).ExecuteScalar();
                tongHangHuy = resultHuy != DBNull.Value ? Convert.ToDouble(resultHuy) : 0;
            }

            ViewBag.TongDoanhThu = tongDoanhThu;
            ViewBag.TongHangHuy = tongHangHuy;
            return View();
        }
        // =========================================================
        // 4. API DÀNH CHO MÁY QUÉT MÃ VẠCH (AJAX)
        // =========================================================
        [HttpGet]
        public IActionResult GetProductInfo(string maSP)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                // Tìm thông tin sản phẩm và tính tổng tồn kho an toàn
                string query = @"
                    SELECT s.TenSP, s.DonViTinh, ISNULL(SUM(l.SoLuongTon), 0) AS TongTon
                    FROM SanPham s
                    LEFT JOIN LoHang l ON s.MaSP = l.MaSP AND l.TrangThai = N'An Toàn'
                    WHERE s.MaSP = @MaSP
                    GROUP BY s.TenSP, s.DonViTinh";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@MaSP", maSP);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return Json(new
                            {
                                success = true,
                                tenSP = reader["TenSP"].ToString(),
                                donViTinh = reader["DonViTinh"].ToString(),
                                tongTon = Convert.ToDouble(reader["TongTon"])
                            });
                        }
                    }
                }
            }
            return Json(new { success = false, message = "Không tìm thấy mã sản phẩm này!" });
        }
    }
}
