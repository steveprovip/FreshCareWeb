using FreshCareWeb.Data;
using FreshCareWeb.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace FreshCareWeb.Controllers
{
    public class ProductController : Controller
    {
        private readonly DbHelper _db;

        public ProductController(DbHelper db)
        {
            _db = db;
        }

        // GET: Product
        public IActionResult Index()
        {
            List<SanPham> danhSach = new List<SanPham>();

            using (SqlConnection conn = _db.GetConnection())
            {
                conn.Open();
                string query = @"
                    SELECT s.*, d.TenDM, n.TenNCC 
                    FROM SanPham s
                    JOIN DanhMuc d ON s.MaDM = d.MaDM
                    JOIN NhaCungCap n ON s.MaNCC = n.MaNCC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            danhSach.Add(new SanPham
                            {
                                MaSP = reader["MaSP"].ToString(),
                                TenSP = reader["TenSP"].ToString(),
                                MaDM = Convert.ToInt32(reader["MaDM"]),
                                MaNCC = Convert.ToInt32(reader["MaNCC"]),
                                DonViTinh = reader["DonViTinh"].ToString(),
                                GiaBanGoc = Convert.ToDecimal(reader["GiaBanGoc"]),
                                MoTa = reader["MoTa"]?.ToString(),
                                TenDM = reader["TenDM"].ToString(),
                                TenNCC = reader["TenNCC"].ToString()
                            });
                        }
                    }
                }
            }
            return View(danhSach);
        }

        // GET: Product/AddProduct
        [HttpGet]
        public IActionResult AddProduct()
        {
            LoadDropDownData();
            return View();
        }

        // POST: Product/AddProduct
        [HttpPost]
        public IActionResult AddProduct(SanPham model)
        {
            using (SqlConnection conn = _db.GetConnection())
            {
                conn.Open();
                try
                {
                    string query = "INSERT INTO SanPham (MaSP, MaDM, MaNCC, TenSP, DonViTinh, GiaBanGoc, MoTa) VALUES (@MaSP, @MaDM, @MaNCC, @TenSP, @DonViTinh, @GiaBanGoc, @MoTa)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@MaSP", model.MaSP);
                        cmd.Parameters.AddWithValue("@MaDM", model.MaDM);
                        cmd.Parameters.AddWithValue("@MaNCC", model.MaNCC);
                        cmd.Parameters.AddWithValue("@TenSP", model.TenSP);
                        cmd.Parameters.AddWithValue("@DonViTinh", model.DonViTinh);
                        cmd.Parameters.AddWithValue("@GiaBanGoc", model.GiaBanGoc);
                        cmd.Parameters.AddWithValue("@MoTa", model.MoTa ?? (object)DBNull.Value);

                        cmd.ExecuteNonQuery();
                    }
                    TempData["MessageType"] = "success";
                    TempData["Message"] = "Đã thêm sản phẩm thành công!";
                }
                catch (SqlException)
                {
                    TempData["MessageType"] = "danger";
                    TempData["Message"] = "Lỗi: Mã sản phẩm này đã tồn tại trong hệ thống!";
                }
            }
            return RedirectToAction("AddProduct");
        }

        [HttpPost]
        public IActionResult Delete(string id)
        {
            using (SqlConnection conn = _db.GetConnection())
            {
                conn.Open();
                try
                {
                    string query = "DELETE FROM SanPham WHERE MaSP = @MaSP";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@MaSP", id);
                        cmd.ExecuteNonQuery();
                    }
                    TempData["Message"] = "Xóa sản phẩm thành công!";
                    TempData["MessageType"] = "success";
                }
                catch (SqlException)
                {
                    TempData["Message"] = "Không thể xóa do sản phẩm đã có lịch sử trong Lô / Phiếu!";
                    TempData["MessageType"] = "danger";
                }
            }
            return RedirectToAction(nameof(Index));
        }

        private void LoadDropDownData()
        {
            List<DanhMuc> dsDanhMuc = new List<DanhMuc>();
            List<NhaCungCap> dsNhaCungCap = new List<NhaCungCap>();

            using (SqlConnection conn = _db.GetConnection())
            {
                conn.Open();
                // Load DanhMuc
                using (SqlCommand cmd = new SqlCommand("SELECT MaDM, TenDM FROM DanhMuc", conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        dsDanhMuc.Add(new DanhMuc { MaDM = Convert.ToInt32(reader["MaDM"]), TenDM = reader["TenDM"].ToString() });
                    }
                }
                // Load NhaCungCap
                using (SqlCommand cmd = new SqlCommand("SELECT MaNCC, TenNCC FROM NhaCungCap", conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        dsNhaCungCap.Add(new NhaCungCap { MaNCC = Convert.ToInt32(reader["MaNCC"]), TenNCC = reader["TenNCC"].ToString() });
                    }
                }
            }
            ViewBag.DanhMucList = dsDanhMuc;
            ViewBag.NhaCungCapList = dsNhaCungCap;
        }
    }
}