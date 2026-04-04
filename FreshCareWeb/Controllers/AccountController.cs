using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace FreshCareWeb.Controllers
{
    public class AccountController : Controller
    {
        // 🚨 SỬA LẠI TÊN SERVER CỦA BẠN VÀO ĐÂY
        string connectionString = @"Data Source=Admin;Initial Catalog=FreshCareDB;Integrated Security=True;TrustServerCertificate=True;";

        // ================= ĐĂNG NHẬP =================
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string taikhoan, string matkhau)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT HoTen FROM NguoiDung WHERE TaiKhoan = @TK AND MatKhau = @MK";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@TK", taikhoan);
                    cmd.Parameters.AddWithValue("@MK", matkhau);

                    object result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        TempData["Message"] = $"Đăng nhập thành công! Xin chào {result.ToString()}";
                        TempData["MessageType"] = "success";
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ViewBag.Error = "Tài khoản hoặc mật khẩu không chính xác!";
                        return View();
                    }
                }
            }
        }

        // ================= ĐĂNG KÝ =================
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(string taikhoan, string matkhau, string hoten)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                try
                {
                    string query = "INSERT INTO NguoiDung (TaiKhoan, MatKhau, HoTen, VaiTro) VALUES (@TK, @MK, @HT, 'NhanVien')";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@TK", taikhoan);
                        cmd.Parameters.AddWithValue("@MK", matkhau);
                        cmd.Parameters.AddWithValue("@HT", hoten);
                        cmd.ExecuteNonQuery();
                    }

                    TempData["Message"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                    TempData["MessageType"] = "success";
                    return RedirectToAction("Login");
                }
                catch (SqlException)
                {
                    // Lỗi xảy ra nếu Tài khoản đã tồn tại (Vi phạm Primary Key)
                    ViewBag.Error = "Tên tài khoản này đã có người sử dụng. Vui lòng chọn tên khác!";
                    return View();
                }
            }
        }
    }
}