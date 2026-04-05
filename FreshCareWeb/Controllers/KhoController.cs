using FreshCareWeb.Data;
using FreshCareWeb.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace FreshCareWeb.Controllers
{
    public class KhoController : Controller
    {
        private readonly DbHelper _db;

        public KhoController(DbHelper db)
        {
            _db = db;
        }

        // ==========================================
        // 1. DASHBOARD CẢNH BÁO HẠN SỬ DỤNG
        // ==========================================
        public IActionResult Dashboard()
        {
            List<LoHang> danhSach = new List<LoHang>();

            using (SqlConnection conn = _db.GetConnection())
            {
                conn.Open();
                string query = @"
                    SELECT l.*, s.TenSP, s.DonViTinh, s.GiaBanGoc, d.PhanTramSale 
                    FROM LoHang l
                    JOIN SanPham s ON l.MaSP = s.MaSP
                    JOIN DanhMuc d ON s.MaDM = d.MaDM
                    WHERE l.TrangThai != N'Đã Hủy' AND l.SoLuongTon > 0";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var lo = new LoHang
                        {
                            MaLo = reader["MaLo"].ToString(),
                            MaSP = reader["MaSP"].ToString(),
                            NgaySanXuat = Convert.ToDateTime(reader["NgaySanXuat"]),
                            HanSuDung = Convert.ToDateTime(reader["HanSuDung"]),
                            SoLuongTon = Convert.ToDouble(reader["SoLuongTon"]),
                            SoLuongBanDau = Convert.ToDouble(reader["SoLuongBanDau"]),
                            TrangThai = reader["TrangThai"].ToString(),
                            TenSP = reader["TenSP"].ToString(),
                            DonViTinh = reader["DonViTinh"].ToString(),
                            GiaBanGoc = Convert.ToDecimal(reader["GiaBanGoc"]),
                            PhanTramSale = Convert.ToDouble(reader["PhanTramSale"])
                        };

                        // Logic cập nhật trạng thái tự động
                        if (lo.SoNgayConLai == 0) lo.TrangThai = "Quá Hạn";
                        else if (lo.SoNgayConLai <= 30) lo.TrangThai = "Cận Date";
                        else lo.TrangThai = "An Toàn";

                        danhSach.Add(lo);
                    }
                }

                // Cập nhật CSDL nếu trạng thái logic khác DB
                foreach (var lo in danhSach)
                {
                    using (SqlCommand updCmd = new SqlCommand("UPDATE LoHang SET TrangThai = @TT WHERE MaLo = @MaLo", conn))
                    {
                        updCmd.Parameters.AddWithValue("@TT", lo.TrangThai);
                        updCmd.Parameters.AddWithValue("@MaLo", lo.MaLo);
                        updCmd.ExecuteNonQuery();
                    }
                }
            }

            return View(danhSach);
        }

        // ==========================================
        // 2. NHẬP KHO
        // ==========================================
        [HttpGet]
        public IActionResult NhapKho()
        {
            List<SanPham> dsSanPham = new List<SanPham>();
            using (SqlConnection conn = _db.GetConnection())
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT MaSP, TenSP FROM SanPham", conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read()) dsSanPham.Add(new SanPham { MaSP = reader["MaSP"].ToString(), TenSP = reader["TenSP"].ToString() });
                }
            }
            ViewBag.SanPhamList = dsSanPham;
            return View();
        }

        [HttpPost]
        public IActionResult NhapKho(string maSP, DateTime ngaysx, DateTime hsd, double soLuong)
        {
            if (hsd <= ngaysx || hsd < DateTime.Now.Date)
            {
                TempData["Message"] = "Ngày hết hạn không hợp lệ!";
                TempData["MessageType"] = "danger";
                return RedirectToAction("NhapKho");
            }

            string maLo = "BATCH-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
            string maPhieu = "PN-" + DateTime.Now.ToString("yyyyMMddHHmmss");

            using (SqlConnection conn = _db.GetConnection())
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();
                try
                {
                    // Tạo Lô (Batch)
                    string qLo = "INSERT INTO LoHang (MaLo, MaSP, NgaySanXuat, HanSuDung, SoLuongBanDau, SoLuongTon) VALUES (@Lo, @SP, @NSX, @HSD, @SL, @SL)";
                    using (SqlCommand cmd = new SqlCommand(qLo, conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@Lo", maLo);
                        cmd.Parameters.AddWithValue("@SP", maSP);
                        cmd.Parameters.AddWithValue("@NSX", ngaysx);
                        cmd.Parameters.AddWithValue("@HSD", hsd);
                        cmd.Parameters.AddWithValue("@SL", soLuong);
                        cmd.ExecuteNonQuery();
                    }

                    // Lưu phiếu nhập
                    string qPhieu = "INSERT INTO PhieuNhapKho (MaPhieuNhap, MaNV, GhiChu) VALUES (@PN, 'NV01', N'Nhập hàng mới')";
                    using (SqlCommand cmd = new SqlCommand(qPhieu, conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@PN", maPhieu);
                        cmd.ExecuteNonQuery();
                    }

                    // Lưu chi tiết nhập
                    string qCT = "INSERT INTO ChiTietNhap (MaPhieuNhap, MaLo, SoLuong) VALUES (@PN, @Lo, @SL)";
                    using (SqlCommand cmd = new SqlCommand(qCT, conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@PN", maPhieu);
                        cmd.Parameters.AddWithValue("@Lo", maLo);
                        cmd.Parameters.AddWithValue("@SL", soLuong);
                        cmd.ExecuteNonQuery();
                    }

                    trans.Commit();
                    TempData["Message"] = "Nhập kho thành công! Mã Lô: " + maLo;
                    TempData["MessageType"] = "success";
                }
                catch (Exception)
                {
                    trans.Rollback();
                    TempData["Message"] = "Có lỗi xảy ra khi nhập kho!";
                    TempData["MessageType"] = "danger";
                }
            }
            return RedirectToAction("Dashboard");
        }

        // ==========================================
        // 3. XUẤT KHO (THUẬT TOÁN FIFO)
        // ==========================================
        [HttpGet]
        public IActionResult XuatKho()
        {
            List<SanPham> dsSanPham = new List<SanPham>();
            using (SqlConnection conn = _db.GetConnection())
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT MaSP, TenSP FROM SanPham", conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read()) dsSanPham.Add(new SanPham { MaSP = reader["MaSP"].ToString(), TenSP = reader["TenSP"].ToString() });
                }
            }
            ViewBag.SanPhamList = dsSanPham;
            return View();
        }

        [HttpPost]
        public IActionResult XuatKho(string maSP, double soLuongXuat)
        {
            using (SqlConnection conn = _db.GetConnection())
            {
                conn.Open();

                // 1. Kiểm tra tồn tổng
                string qCheck = "SELECT ISNULL(SUM(SoLuongTon),0) FROM LoHang WHERE MaSP = @MaSP AND TrangThai != N'Đã Hủy' AND TrangThai != N'Quá Hạn'";
                using (SqlCommand cmdCheck = new SqlCommand(qCheck, conn))
                {
                    cmdCheck.Parameters.AddWithValue("@MaSP", maSP);
                    double tongTonHienCo = Convert.ToDouble(cmdCheck.ExecuteScalar());

                    if (soLuongXuat > tongTonHienCo)
                    {
                        TempData["Message"] = $"Không đủ hàng! Tổng kho an toàn chỉ còn {tongTonHienCo}.";
                        TempData["MessageType"] = "danger";
                        return RedirectToAction("XuatKho");
                    }
                }

                // 2. Thực thi FIFO Transaction
                SqlTransaction trans = conn.BeginTransaction();
                try
                {
                    string maPhieu = "PX-" + DateTime.Now.ToString("yyyyMMddHHmmss");
                    
                    // Tạo phiếu xuất bán
                    string qPhieu = "INSERT INTO PhieuXuat (MaPhieuXuat, LoaiPhieu, MaNV) VALUES (@PX, N'Bán Hàng', 'NV01')";
                    using (SqlCommand cmdPhieu = new SqlCommand(qPhieu, conn, trans))
                    {
                        cmdPhieu.Parameters.AddWithValue("@PX", maPhieu);
                        cmdPhieu.ExecuteNonQuery();
                    }

                    // Lấy danh sách lô ưu tiên FIFO
                    string qLo = @"
                        SELECT l.MaLo, l.SoLuongTon, s.GiaBanGoc, d.PhanTramSale, l.TrangThai 
                        FROM LoHang l
                        JOIN SanPham s ON l.MaSP = s.MaSP
                        JOIN DanhMuc d ON s.MaDM = d.MaDM
                        WHERE l.MaSP = @MaSP AND l.SoLuongTon > 0 AND l.TrangThai != N'Đã Hủy' AND l.TrangThai != N'Quá Hạn'
                        ORDER BY l.HanSuDung ASC"; // Lõi FIFO

                    List<dynamic> dsLo = new List<dynamic>();
                    using (SqlCommand cmdLo = new SqlCommand(qLo, conn, trans))
                    {
                        cmdLo.Parameters.AddWithValue("@MaSP", maSP);
                        using (SqlDataReader reader = cmdLo.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                dsLo.Add(new {
                                    MaLo = reader["MaLo"].ToString(),
                                    SoLuongTon = Convert.ToDouble(reader["SoLuongTon"]),
                                    GiaBanGoc = Convert.ToDecimal(reader["GiaBanGoc"]),
                                    PhanTramSale = Convert.ToDouble(reader["PhanTramSale"]),
                                    TrangThai = reader["TrangThai"].ToString()
                                });
                            }
                        }
                    }

                    double luongTaiCan = soLuongXuat;
                    decimal tongTienPX = 0;

                    foreach (var lo in dsLo)
                    {
                        if (luongTaiCan <= 0) break;

                        double luongTru = (luongTaiCan >= lo.SoLuongTon) ? lo.SoLuongTon : luongTaiCan;
                        luongTaiCan -= luongTru;

                        // Tính giá thực tế theo Sale
                        decimal giaXuat = lo.GiaBanGoc;
                        if (lo.TrangThai == "Cận Date") giaXuat = lo.GiaBanGoc * (decimal)(100 - lo.PhanTramSale) / 100m;
                        tongTienPX += giaXuat * (decimal)luongTru;

                        // Cập nhật CSDL trừ tồn lô
                        string qUpdate = "UPDATE LoHang SET SoLuongTon = SoLuongTon - @SL WHERE MaLo = @MaLo";
                        using (SqlCommand updCmd = new SqlCommand(qUpdate, conn, trans))
                        {
                            updCmd.Parameters.AddWithValue("@SL", luongTru);
                            updCmd.Parameters.AddWithValue("@MaLo", lo.MaLo);
                            updCmd.ExecuteNonQuery();
                        }

                        // Ghi chi tiết xuất
                        string qCT = "INSERT INTO ChiTietXuat (MaPhieuXuat, MaLo, SoLuong, DonGia) VALUES (@PX, @MaLo, @SL, @Gia)";
                        using (SqlCommand ctCmd = new SqlCommand(qCT, conn, trans))
                        {
                            ctCmd.Parameters.AddWithValue("@PX", maPhieu);
                            ctCmd.Parameters.AddWithValue("@MaLo", lo.MaLo);
                            ctCmd.Parameters.AddWithValue("@SL", luongTru);
                            ctCmd.Parameters.AddWithValue("@Gia", giaXuat);
                            ctCmd.ExecuteNonQuery();
                        }
                    }

                    // Cập nhật tổng tiền vào Phiếu Xuất
                    using (SqlCommand updPx = new SqlCommand("UPDATE PhieuXuat SET TongTien = @Tong WHERE MaPhieuXuat = @PX", conn, trans))
                    {
                        updPx.Parameters.AddWithValue("@Tong", tongTienPX);
                        updPx.Parameters.AddWithValue("@PX", maPhieu);
                        updPx.ExecuteNonQuery();
                    }

                    trans.Commit();
                    TempData["Message"] = "Xuất kho FIFO thành công! Hóa đơn: " + maPhieu;
                    TempData["MessageType"] = "success";
                }
                catch (Exception)
                {
                    trans.Rollback();
                    TempData["Message"] = "Lỗi hệ thống khi trừ xuất kho FIFO!";
                    TempData["MessageType"] = "danger";
                }
            }
            return RedirectToAction("Dashboard");
        }

        // ==========================================
        // 4. XUẤT HỦY HÀNG (QUÁ HẠN LỖI)
        // ==========================================
        [HttpPost]
        public IActionResult XuatHuy(string maLo)
        {
            using (SqlConnection conn = _db.GetConnection())
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();
                try
                {
                    // Tạo Phiếu Xuất (Loại: Hủy Hàng)
                    string maPhieu = "PH-" + DateTime.Now.ToString("yyyyMMddHHmmss");
                    using (SqlCommand cmd = new SqlCommand("INSERT INTO PhieuXuat (MaPhieuXuat, LoaiPhieu, MaNV, TongTien) VALUES (@PX, N'Hủy Hàng', 'NV01', 0)", conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@PX", maPhieu);
                        cmd.ExecuteNonQuery();
                    }

                    // Lấy số tồn hiện tại
                    double ton = 0;
                    using (SqlCommand cGet = new SqlCommand("SELECT SoLuongTon FROM LoHang WHERE MaLo = @MaLo", conn, trans))
                    {
                        cGet.Parameters.AddWithValue("@MaLo", maLo);
                        ton = Convert.ToDouble(cGet.ExecuteScalar());
                    }

                    // Ghi chi tiết xuất hủy
                    using (SqlCommand cmdCT = new SqlCommand("INSERT INTO ChiTietXuat (MaPhieuXuat, MaLo, SoLuong, DonGia) VALUES (@PX, @Lo, @SL, 0)", conn, trans))
                    {
                        cmdCT.Parameters.AddWithValue("@PX", maPhieu);
                        cmdCT.Parameters.AddWithValue("@Lo", maLo);
                        cmdCT.Parameters.AddWithValue("@SL", ton);
                        cmdCT.ExecuteNonQuery();
                    }

                    // Đổi trạng thái lô hàng thành Đã Hủy và đưa tồn về 0
                    using (SqlCommand cUpd = new SqlCommand("UPDATE LoHang SET SoLuongTon = 0, TrangThai = N'Đã Hủy' WHERE MaLo = @MaLo", conn, trans))
                    {
                        cUpd.Parameters.AddWithValue("@MaLo", maLo);
                        cUpd.ExecuteNonQuery();
                    }

                    trans.Commit();
                    TempData["Message"] = "Đã xuất hủy lô thành công!";
                    TempData["MessageType"] = "success";
                }
                catch (Exception)
                {
                    trans.Rollback();
                    TempData["Message"] = "Lỗi khi xuất hủy!";
                    TempData["MessageType"] = "danger";
                }
            }
            return RedirectToAction("Dashboard");
        }
    }
}
