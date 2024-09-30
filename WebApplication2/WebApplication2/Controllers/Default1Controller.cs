using System;
using System.Linq;
using System.Web.Mvc;
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
    public class Default1Controller : Controller
    {
        // GET: Default1 (Trang đăng nhập)
        public ActionResult Index()
        {
            return View();
        }

        // POST: Default1/Login (Xử lý khi người dùng nhấn nút đăng nhập)
        
        public ActionResult Login(string email, string password)
        {
            using (DB_TadEntities dB = new DB_TadEntities())
            {
                var user = dB.Accounts.FirstOrDefault(u => u.Email == email && u.Password == password);
                ViewBag.user=user;
                if (user != null)
                {
                    // Đăng nhập thành công, chuyển hướng tới TrangSauKhiDangNhap
                    ViewBag.ErrorMessage = "Thông tin đăng nhập hợp lệ";
                    return View("~/Views/Trangsaukhidangnhap/Index.cshtml");

                }
                else
                {
                    ViewBag.ErrorMessage = "Thông tin đăng nhập không hợp lệ";
                    return View("~/Views/Trangsaukhidangnhap/Index.cshtml");

                }
            }
        }

    }
}
        