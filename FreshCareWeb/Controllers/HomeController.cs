using Microsoft.AspNetCore.Mvc;
using FreshCareWeb.Models;
using System.Data.SqlClient;
using System.Collections.Generic;
using System;

namespace FreshCareWeb.Controllers
{
    public class HomeController : Controller
    {
        // CHUỖI KẾT NỐI TỚI DATABASE CỦA BẠN
        string connectionString = @"Data Source=.\SQLEXPRESS01;Initial Catalog=FreshCareDB;Integrated Security=True;TrustServerCertificate=True;";

        // ------------------------------------------------------------------
        // 1. HÀM CHẠY KHI MỞ TRANG CHỦ -> TẢI DỮ LIỆU 3 MÀU ĐỎ, CAM, XANH
        // ------------------------------------------------------------------
        public IActionResult Index()
        {
            List<LoHang> hangCanDate = new List<LoHang>();
            List<LoHang> hangQuaHan = new List<LoHang>();
            List<LoHang> hangAnToan = new List<LoHang>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT * FROM dbo.LoHang WHERE SoLuongTon > 0";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            LoHang lo = new LoHang
                            {
                                MaLo = reader["MaLo"].ToString(),
                                MaSP = reader["MaSP"].ToString(),
                                HanSuDung = Convert.ToDateTime(reader["HanSuDung"]),
                                SoLuongTon = Convert.ToDouble(reader["SoLuongTon"])
                            };

                            TimeSpan thoiGianConLai = lo.HanSuDung.Date - DateTime.Now.Date;

                            if (thoiGianConLai.Days < 0)
                            {
                                hangQuaHan.Add(lo); // Đỏ
                            }
                            else if (thoiGianConLai.Days <= 30)
                            {
                                hangCanDate.Add(lo); // Cam
                            }
                            else
                            {
                                hangAnToan.Add(lo); // Xanh
                            }
                        }
                    }
                }
            }

            ViewBag.HangQuaHan = hangQuaHan;
            ViewBag.HangCanDate = hangCanDate;
            ViewBag.HangAnToan = hangAnToan;
            return View();
        }

        // ------------------------------------------------------------------
        // 2. HÀM CHẠY KHI BẤM NÚT "XUẤT KHO" -> THỰC THI THUẬT TOÁN FIFO
        // ------------------------------------------------------------------
        [HttpPost]
        public IActionResult XuatKhoFIFO(string maSP, double soLuongCanXuat)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // BƯỚC 1: KIỂM TRA TỔNG TỒN KHO HỢP LỆ
                string checkQuery = "SELECT SUM(SoLuongTon) FROM dbo.LoHang WHERE MaSP = @MaSP AND SoLuongTon > 0 AND HanSuDung >= @HomNay";
                SqlCommand checkCmd = new SqlCommand(checkQuery, conn);
                checkCmd.Parameters.AddWithValue("@MaSP", maSP);
                checkCmd.Parameters.AddWithValue("@HomNay", DateTime.Now.Date);

                object result = checkCmd.ExecuteScalar();
                double tongTonKho = result != DBNull.Value ? Convert.ToDouble(result) : 0;

                if (soLuongCanXuat > tongTonKho)
                {
                    TempData["MessageType"] = "danger";
                    TempData["Message"] = $"🚨 LỖI: Bạn muốn xuất {soLuongCanXuat}kg nhưng kho chỉ còn {tongTonKho}kg hàng an toàn. Giao dịch bị hủy!";
                    return RedirectToAction("Index");
                }

                // BƯỚC 2: TIẾN HÀNH THUẬT TOÁN FIFO
                string query = "SELECT * FROM dbo.LoHang WHERE MaSP = @MaSP AND SoLuongTon > 0 AND HanSuDung >= @HomNay ORDER BY HanSuDung ASC";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@MaSP", maSP);
                cmd.Parameters.AddWithValue("@HomNay", DateTime.Now.Date);

                SqlDataReader reader = cmd.ExecuteReader();
                List<LoHang> danhSachLo = new List<LoHang>();
                while (reader.Read())
                {
                    danhSachLo.Add(new LoHang
                    {
                        MaLo = reader["MaLo"].ToString(),
                        SoLuongTon = Convert.ToDouble(reader["SoLuongTon"])
                    });
                }
                reader.Close();

                double soLuongConLai = soLuongCanXuat;
                foreach (var lo in danhSachLo)
                {
                    if (soLuongConLai <= 0) break;

                    double tonKhoMoi = 0;
                    if (lo.SoLuongTon >= soLuongConLai)
                    {
                        tonKhoMoi = lo.SoLuongTon - soLuongConLai;
                        soLuongConLai = 0;
                    }
                    else
                    {
                        soLuongConLai -= lo.SoLuongTon;
                        tonKhoMoi = 0;
                    }

                    string updateQuery = "UPDATE dbo.LoHang SET SoLuongTon = @TonMoi WHERE MaLo = @MaLo";
                    using (SqlCommand cmdUpdate = new SqlCommand(updateQuery, conn))
                    {
                        cmdUpdate.Parameters.AddWithValue("@TonMoi", tonKhoMoi);
                        cmdUpdate.Parameters.AddWithValue("@MaLo", lo.MaLo);
                        cmdUpdate.ExecuteNonQuery();
                    }
                }

                TempData["MessageType"] = "success";
                TempData["Message"] = $"✅ THÀNH CÔNG: Đã xuất {soLuongCanXuat}kg hàng (Mã: {maSP}) theo đúng quy tắc FIFO!";
            }
            return RedirectToAction("Index");
        }

        // ------------------------------------------------------------------
        // 3. HÀM CHẠY KHI BẤM NÚT "LƯU PHIẾU NHẬP KHO"
        // ------------------------------------------------------------------
        [HttpPost]
        public IActionResult NhapKho(string MaLo, string MaSP, DateTime NgaySanXuat, DateTime HanSuDung, double SoLuongBanDau)
        {
            if (HanSuDung < NgaySanXuat)
            {
                TempData["MessageType"] = "danger";
                TempData["Message"] = "🚨 LỖI: Hạn sử dụng không thể nhỏ hơn Ngày sản xuất!";
                return RedirectToAction("Index");
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "INSERT INTO dbo.LoHang (MaLo, MaSP, NgaySanXuat, HanSuDung, SoLuongBanDau, SoLuongTon) VALUES (@MaLo, @MaSP, @NgaySanXuat, @HanSuDung, @SoLuongBanDau, @SoLuongTon)";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@MaLo", MaLo);
                    cmd.Parameters.AddWithValue("@MaSP", MaSP);
                    cmd.Parameters.AddWithValue("@NgaySanXuat", NgaySanXuat);
                    cmd.Parameters.AddWithValue("@HanSuDung", HanSuDung);
                    cmd.Parameters.AddWithValue("@SoLuongBanDau", SoLuongBanDau);
                    cmd.Parameters.AddWithValue("@SoLuongTon", SoLuongBanDau);

                    try
                    {
                        cmd.ExecuteNonQuery();
                        TempData["MessageType"] = "success";
                        TempData["Message"] = $"✅ NHẬP KHO THÀNH CÔNG: Đã tạo mã lô {MaLo} với số lượng {SoLuongBanDau}kg!";
                    }
                    catch (SqlException ex)
                    {
                        // In ra chính xác lỗi từ SQL Server để dễ bắt bệnh
                        TempData["MessageType"] = "danger";
                        TempData["Message"] = $"🚨 LỖI CSDL: {ex.Message}";
                    }
                }
            }
            return RedirectToAction("Index");
        }
    }
}