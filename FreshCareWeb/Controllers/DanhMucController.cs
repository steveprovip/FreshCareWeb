using FreshCareWeb.Data;
using FreshCareWeb.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace FreshCareWeb.Controllers
{
    public class DanhMucController : Controller
    {
        private readonly DbHelper _db;

        public DanhMucController(DbHelper db)
        {
            _db = db;
        }

        // GET: DanhMuc
        public IActionResult Index()
        {
            List<DanhMuc> danhSach = new List<DanhMuc>();

            using (SqlConnection conn = _db.GetConnection())
            {
                conn.Open();
                string query = "SELECT * FROM DanhMuc";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            danhSach.Add(new DanhMuc
                            {
                                MaDM = Convert.ToInt32(reader["MaDM"]),
                                TenDM = reader["TenDM"].ToString(),
                                PhanTramSale = Convert.ToDouble(reader["PhanTramSale"])
                            });
                        }
                    }
                }
            }
            return View(danhSach);
        }

        // GET: DanhMuc/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: DanhMuc/Create
        [HttpPost]
        public IActionResult Create(DanhMuc model)
        {
            using (SqlConnection conn = _db.GetConnection())
            {
                conn.Open();
                string query = "INSERT INTO DanhMuc (TenDM, PhanTramSale) VALUES (@TenDM, @PhanTramSale)";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@TenDM", model.TenDM);
                    cmd.Parameters.AddWithValue("@PhanTramSale", model.PhanTramSale);
                    
                    cmd.ExecuteNonQuery();
                }
                TempData["Message"] = "Thêm danh mục thành công!";
                TempData["MessageType"] = "success";
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: DanhMuc/Delete/5
        [HttpPost]
        public IActionResult Delete(int id)
        {
            using (SqlConnection conn = _db.GetConnection())
            {
                conn.Open();
                try
                {
                    string query = "DELETE FROM DanhMuc WHERE MaDM = @MaDM";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@MaDM", id);
                        cmd.ExecuteNonQuery();
                    }
                    TempData["Message"] = "Xóa danh mục thành công!";
                    TempData["MessageType"] = "success";
                }
                catch (SqlException)
                {
                    TempData["Message"] = "Không thể xóa danh mục vì đang có sản phẩm liên kết!";
                    TempData["MessageType"] = "danger";
                }
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
