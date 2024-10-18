using System;
using System.Linq;
using System.Web.Mvc;
using webbannongsan.Models;
using MimeKit;
using MailKit.Net.Smtp;

namespace webbannongsan.Controllers
{
    public class AccountController : Controller
    {
        DB_TadEntities DB = new DB_TadEntities();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string email, string password)
        {
            var user = DB.Accounts.FirstOrDefault(u => u.Email == email && u.Password == password);

            if (user != null)
            {
                Session["Account"] = user;
                if (user.RoleID == "ADMIN")
                {
                    return RedirectToAction("ListProduct", "ProductAdmin", new { area = "Admin" });
                }
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.ErrorMessage = "Thông tin đăng nhập không hợp lệ";
                return View();
            }
        }

        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(Account model)
        {
            if (ModelState.IsValid)
            {
                var existingUser = DB.Accounts.FirstOrDefault(u => u.Email.ToLower() == model.Email.ToLower());
                if (existingUser == null)
                {
                    model.RoleID = "GUEST";
                    DB.Accounts.Add(model);
                    DB.SaveChanges();
                    return RedirectToAction("Login");
                }
                else
                {
                    ViewBag.ErrorMessage = "Email đã được đăng ký.";
                }
            }
            return View(model);
        }

        public ActionResult Profile()
        {
            Account user = Session["Account"] as Account;
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            Account account = DB.Accounts.FirstOrDefault(a => a.AccountID == user.AccountID);
            return View(account);
        }

        public ActionResult ChangePassword()
        {
            if (Session["Account"] == null)
            {
                return RedirectToAction("Login");
            }

            return View();
        }

        [HttpPost]
        public ActionResult ChangePassword(string OldPassword, string NewPassword, string ConfirmPassword)
        {
            Account Ac = (Account)Session["Account"];
            Account account = DB.Accounts.FirstOrDefault(a => a.AccountID == Ac.AccountID);

            if (account == null)
            {
                return RedirectToAction("Login");
            }

            if (account.Password != OldPassword)
            {
                ViewBag.ErrorMessage = "Mật khẩu cũ không đúng.";
                return View();
            }

            if (NewPassword != ConfirmPassword)
            {
                ViewBag.ErrorMessage = "Mật khẩu mới và xác nhận mật khẩu không khớp.";
                return View();
            }

            account.Password = NewPassword;
            DB.SaveChanges();

            ViewBag.SuccessMessage = "Mật khẩu đã được thay đổi thành công.";
            return View();
        }

        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ForgotPassword(string email)
        {
            var account = DB.Accounts.FirstOrDefault(i => i.Email == email);
            if (account != null)
            {
                var otp = new Random().Next(100000, 999999).ToString();
                try
                {
                    SendOTPEmail(email, otp);
                    Session["OTP"] = otp; // Lưu mã OTP vào session
                    return RedirectToAction("ConfirmOTP");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Có lỗi xảy ra khi gửi email. Vui lòng thử lại sau.");
                }
            }
            return View();
        }

        public ActionResult ConfirmOTP()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ConfirmOTP(string otp)
        {
            var otpInSession = Session["OTP"] as string;

            if (otp == otpInSession)
            {
                return RedirectToAction("ResetPassword");
            }
            else
            {
                ViewBag.ErrorMessage = "Mã OTP không chính xác. Vui lòng thử lại.";
                return View();
            }
        }

        public ActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ResetPassword(string newPassword, string confirmPassword)
        {
            if (newPassword == confirmPassword)
            {
                var account = Session["Account"] as Account;

                if (account != null)
                {
                    account.Password = newPassword;
                    DB.SaveChanges();
                    ViewBag.SuccessMessage = "Mật khẩu đã được cập nhật thành công.";
                    return RedirectToAction("Login", "Account");
                }
                else
                {
                    ViewBag.ErrorMessage = "Không thể xác định tài khoản. Vui lòng thử lại.";
                    return View();
                }
            }
            else
            {
                ViewBag.ErrorMessage = "Mật khẩu và xác nhận mật khẩu không khớp.";
                return View();
            }
        }

        private void SendOTPEmail(string toEmail, string otp)
        {
            try
            {
                var fromAddress = new MailboxAddress("Vườn Nông Sản Tươi Ngon", "datbotp123@gmail.com");
                var toAddress = new MailboxAddress("", toEmail);
                string fromPassword = "crgjrvmaykcrhdah";

                if (string.IsNullOrEmpty(fromPassword))
                {
                    throw new Exception("Lỗi: Mật khẩu ứng dụng không được thiết lập.");
                }

                string subject = "Mã OTP Đặt Lại Mật Khẩu";
                string body = $"<p>Xin chào,</p><p>Mã OTP của bạn để đặt lại mật khẩu là: <strong>{otp}</strong></p><p>Mã OTP này có hiệu lực trong 15 phút.</p>";

                var message = new MimeMessage();
                message.From.Add(fromAddress);
                message.To.Add(toAddress);
                message.Subject = subject;
                message.Body = new TextPart("html")
                {
                    Text = body
                };

                using (var smtp = new SmtpClient())
                {
                    smtp.Connect("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                    smtp.Authenticate(fromAddress.Address, fromPassword);
                    smtp.Send(message);
                    smtp.Disconnect(true);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Gửi email không thành công: " + ex.Message);
            }
        }

        public ActionResult ManageMembers()
        {
            var members = DB.Accounts.ToList(); // Lấy toàn bộ danh sách tài khoản
            return View(members); // Trả về view để hiển thị danh sách tài khoản
        }

    }
}
