using FreshCareWeb.Data;
using FreshCareWeb.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace FreshCareWeb.Controllers
{
    public class NhaCungCapController : Controller
    {
        private readonly DbHelper _db;

        public NhaCungCapController(DbHelper db)
        {
            _db = db;
        }

        // GET: NhaCungCap
        public IActionResult Index()
        {
            List<NhaCungCap> danhSach = new List<NhaCungCap>();

            using (SqlConnection conn = _db.GetConnection())
            {
                conn.Open();
                string query = "SELECT * FROM NhaCungCap";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            danhSach.Add(new NhaCungCap
                            {
                                MaNCC = Convert.ToInt32(reader["MaNCC"]),
                                TenNCC = reader["TenNCC"].ToString(),
                                SoDienThoai = reader["SoDienThoai"]?.ToString(),
                                Email = reader["Email"]?.ToString(),
                                DiaChi = reader["DiaChi"]?.ToString()
                            });
                        }
                    }
                }
            }
            return View(danhSach);
        }

        // GET: NhaCungCap/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: NhaCungCap/Create
        [HttpPost]
        public IActionResult Create(NhaCungCap model)
        {
            using (SqlConnection conn = _db.GetConnection())
            {
                conn.Open();
                string query = "INSERT INTO NhaCungCap (TenNCC, SoDienThoai, Email, DiaChi) VALUES (@TenNCC, @SoDienThoai, @Email, @DiaChi)";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@TenNCC", model.TenNCC ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@SoDienThoai", model.SoDienThoai ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Email", model.Email ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@DiaChi", model.DiaChi ?? (object)DBNull.Value);
                    
                    cmd.ExecuteNonQuery();
                }
                TempData["Message"] = "Thêm nhà cung cấp thành công!";
                TempData["MessageType"] = "success";
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: NhaCungCap/Delete/5
        [HttpPost]
        public IActionResult Delete(int id)
        {
            using (SqlConnection conn = _db.GetConnection())
            {
                conn.Open();
                try
                {
                    string query = "DELETE FROM NhaCungCap WHERE MaNCC = @MaNCC";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@MaNCC", id);
                        cmd.ExecuteNonQuery();
                    }
                    TempData["Message"] = "Xóa nhà cung cấp thành công!";
                    TempData["MessageType"] = "success";
                }
                catch (SqlException)
                {
                    TempData["Message"] = "Không thể xóa nhà cung cấp vì đang có sản phẩm liên kết!";
                    TempData["MessageType"] = "danger";
                }
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
