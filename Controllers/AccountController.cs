using BTL_QLTuyenSinh.Data;
using BTL_QLTuyenSinh.Models;
using BTL_QLTuyenSinh.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace BTL_QLTuyenSinh.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        #region ĐĂNG KÝ
        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            string cccd = model.CCCD.Trim();
            string email = model.Email.Trim();

            bool tonTai = await _context.NguoiDungs
                .AnyAsync(u =>
                    u.TenDangNhap == cccd ||
                    u.SoCCCD == cccd ||
                    u.Email == email);

            if (tonTai)
            {
                ModelState.AddModelError("", "CCCD hoặc Email đã tồn tại.");
                return View(model);
            }

            var thiSinh = new NguoiDung
            {
                TenDangNhap = cccd,
                SoCCCD = cccd,
                HoTen = model.HoTen.Trim(),
                Email = email,
                SoDienThoai = model.SoDienThoai.Trim(),
                MatKhauMaHoa = BCrypt.Net.BCrypt.HashPassword(model.MatKhau),
                VaiTro = "ThiSinh",
                TrangThaiHoatDong = true,
                NgayTao = DateTime.UtcNow
            };

            _context.NguoiDungs.Add(thiSinh);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Không thể lưu tài khoản. Hãy kiểm tra lại.");
                return View(model);
            }

            TempData["ThanhCong"] = "Đăng ký thành công!";
            return RedirectToAction("Login");
        }
        #endregion

        #region ĐĂNG NHẬP
        [HttpGet]
        public async Task<IActionResult> Login(string? returnUrl = null)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Admin") || User.IsInRole("QuanTri") || User.IsInRole("CanBo") || User.IsInRole("GiaoVien"))
                {
                    return RedirectToAction("Index", "NganhHoc", new { area = "Admin" });
                }
                return RedirectToAction("Index", "Home");
            }

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

            // 1. Tìm tài khoản theo TenDangNhap hoặc SoCCCD
            var user = await _context.NguoiDungs
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

            // 2. Xác thực mật khẩu
            bool isValid = false;

            if (user.MatKhauMaHoa == passInput)
            {
                isValid = true;
            }
            else if (!string.IsNullOrEmpty(user.MatKhauMaHoa) && user.MatKhauMaHoa == HashPasswordSHA256(passInput))
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

            // 3. Xóa session và sign out phiên cũ
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            string role = user.VaiTro ?? "ThiSinh";

            // Lưu Session
            HttpContext.Session.SetString("SessionUser", user.TenDangNhap);
            HttpContext.Session.SetString("SessionRole", role);
            HttpContext.Session.SetString("LoginTime", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));

            // 4. Lưu Cookie theo dõi
            int visitCount = 0;
            if (Request.Cookies["VisitCount"] != null)
            {
                int.TryParse(Request.Cookies["VisitCount"], out visitCount);
            }
            visitCount++;

            var persistentOptions = new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(30),
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax
            };

            Response.Cookies.Append("VisitCount", visitCount.ToString(), persistentOptions);
            Response.Cookies.Append("LastLoginTime", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), persistentOptions);
            Response.Cookies.Append("LastUserRole", role, persistentOptions);

            // 5. Tạo Claims xác thực
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.MaNguoiDung.ToString()),
                new Claim(ClaimTypes.Name, user.HoTen ?? user.TenDangNhap),
                new Claim(ClaimTypes.Role, role),
                new Claim("TenDangNhap", user.TenDangNhap),
                new Claim("CCCD", user.SoCCCD ?? "")
            };

            if (role.Equals("Admin", StringComparison.OrdinalIgnoreCase) || role.Equals("QuanTri", StringComparison.OrdinalIgnoreCase))
            {
                claims.Add(new Claim(ClaimTypes.Role, "Admin"));
                claims.Add(new Claim(ClaimTypes.Role, "QuanTri"));
            }
            else if (role.Equals("CanBo", StringComparison.OrdinalIgnoreCase) || role.Equals("GiaoVien", StringComparison.OrdinalIgnoreCase))
            {
                claims.Add(new Claim(ClaimTypes.Role, "CanBo"));
                claims.Add(new Claim(ClaimTypes.Role, "GiaoVien"));
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties
                {
                    IsPersistent = model.GhiNho,
                    ExpiresUtc = model.GhiNho ? DateTimeOffset.UtcNow.AddHours(8) : null
                });

            // 6. Điều hướng
            bool isAdminOrStaff = role.Equals("Admin", StringComparison.OrdinalIgnoreCase)
                               || role.Equals("QuanTri", StringComparison.OrdinalIgnoreCase)
                               || role.Equals("CanBo", StringComparison.OrdinalIgnoreCase)
                               || role.Equals("GiaoVien", StringComparison.OrdinalIgnoreCase);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                if (!isAdminOrStaff && returnUrl.StartsWith("/Admin", StringComparison.OrdinalIgnoreCase))
                {
                    return RedirectToAction("Index", "Home");
                }
                return Redirect(returnUrl);
            }

            if (isAdminOrStaff)
            {
                return RedirectToAction("Index", "NganhHoc", new { area = "Admin" });
            }

            return RedirectToAction("Index", "Home");
        }
        #endregion

        #region ĐĂNG XUẤT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }
        #endregion

        #region HÀM TIỆN ÍCH BĂM MẬT KHẨU SHA-256
        private static string HashPasswordSHA256(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }
        #endregion
    }
}