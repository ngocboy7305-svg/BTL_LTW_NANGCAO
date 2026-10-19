using BTL_QLTuyenSinh.Data;
using BTL_QLTuyenSinh.Models;
using BTL_QLTuyenSinh.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BTL_QLTuyenSinh.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string input = (model.TenDangNhap ?? "").Trim();
            string passInput = (model.MatKhau ?? "").Trim();

            // 1. Tìm tài khoản không phân biệt chữ hoa/thường theo TenDangNhap hoặc SoCCCD
            var user = await _context.NguoiDung
                .FirstOrDefaultAsync(u => u.TenDangNhap.ToLower() == input.ToLower()
                                       || (u.SoCCCD != null && u.SoCCCD == input));

            if (user == null)
            {
                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không chính xác.");
                return View(model);
            }

            if (!user.TrangThaiHoatDong)
            {
                ModelState.AddModelError("", "Tài khoản của bạn hiện đang bị khóa.");
                return View(model);
            }

            // 2. Xác thực mật khẩu: kiểm tra chuỗi thường (Plaintext) trước, sau đó kiểm tra băm BCrypt
            bool isValid = false;

            if (user.MatKhauMaHoa == passInput)
            {
                isValid = true;
            }
            else
            {
                try
                {
                    if (!string.IsNullOrEmpty(user.MatKhauMaHoa) &&
                        user.MatKhauMaHoa.StartsWith("$2") &&
                        BCrypt.Net.BCrypt.Verify(passInput, user.MatKhauMaHoa))
                    {
                        isValid = true;
                    }
                }
                catch
                {
                    isValid = false;
                }
            }

            if (!isValid)
            {
                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không chính xác.");
                return View(model);
            }

            // 3. Thiết lập thông tin Cookie đăng nhập (Claims)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.MaNguoiDung.ToString()),
                new Claim(ClaimTypes.Name, user.HoTen ?? user.TenDangNhap),
                new Claim(ClaimTypes.Role, user.VaiTro ?? "ThiSinh"),
                new Claim("TenDangNhap", user.TenDangNhap),
                new Claim("CCCD", user.SoCCCD ?? "")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties
                {
                    IsPersistent = model.GhiNho,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                });

            // 4. Điều hướng theo quyền
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            // Cả Admin (mở Ngành học) và Giáo viên / Cán bộ (mở Danh mục Ngành) đều trỏ vào NganhHocController
            if (user.VaiTro == "QuanTri" || user.VaiTro == "CanBo")
            {
                return RedirectToAction("Index", "NganhHoc", new { area = "Admin" });
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }
    }
}