using BTL_QLTuyenSinh.Data;
using BTL_QLTuyenSinh.Models;
using BTL_QLTuyenSinh.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
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

            // 2. Xác thực mật khẩu: Hỗ trợ Plaintext, SHA-256 (tạo qua AJAX) và BCrypt (đăng ký)
            bool isValid = false;

            if (user.MatKhauMaHoa == passInput)
            {
                // TH 1: Mật khẩu dạng text thường (như admin, canbo_tuyensinh ban đầu)
                isValid = true;
            }
            else if (!string.IsNullOrEmpty(user.MatKhauMaHoa) && user.MatKhauMaHoa == HashPasswordSHA256(passInput))
            {
                // TH 2: Mật khẩu băm SHA-256 base64 (các tài khoản tạo bằng AJAX: canbo01, admin3, gv01...)
                isValid = true;
            }
            else
            {
                // TH 3: Mật khẩu băm BCrypt (tài khoản thí sinh)
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

            if (user.VaiTro == "QuanTri" || user.VaiTro == "CanBo")
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