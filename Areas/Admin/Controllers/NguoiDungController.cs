using System.Security.Cryptography;
using System.Text;
using BTL_QLTuyenSinh.Data;
using BTL_QLTuyenSinh.Models;
using BTL_QLTuyenSinh.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BTL_QLTuyenSinh.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "QuanTri")] // Chỉ Quản trị viên theo DB
    public class NguoiDungController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NguoiDungController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? keyword, string? vaiTro, int page = 1)
        {
            int pageSize = 10;
            var query = _context.NguoiDungs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();
                query = query.Where(u => u.TenDangNhap.Contains(keyword) ||
                                         u.HoTen.Contains(keyword) ||
                                         u.Email.Contains(keyword) ||
                                         (u.SoCCCD != null && u.SoCCCD.Contains(keyword)));
            }

            if (!string.IsNullOrWhiteSpace(vaiTro))
            {
                query = query.Where(u => u.VaiTro == vaiTro);
            }

            int totalRecords = await query.CountAsync();
            var users = await query
                .OrderByDescending(u => u.NgayTao)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            ViewBag.Keyword = keyword;
            ViewBag.SelectedVaiTro = vaiTro;

            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var user = await _context.NguoiDungs.FindAsync(id);
            if (user == null) return Json(new { success = false, message = "Không tìm thấy người dùng." });

            user.TrangThaiHoatDong = !user.TrangThaiHoatDong;
            await _context.SaveChangesAsync();

            return Json(new { success = true, newStatus = user.TrangThaiHoatDong });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateRole(int id, string newRole)
        {
            var user = await _context.NguoiDungs.FindAsync(id);
            if (user == null) return NotFound();

            user.VaiTro = newRole; // "QuanTri", "CanBo", "ThiSinh"
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromBody] TaoNguoiDungViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Json(new { success = false, message = string.Join("<br/>", errors) });
            }

            // 1. Kiểm tra trùng Tên đăng nhập hoặc Email
            bool exists = await _context.NguoiDungs.AnyAsync(u =>
                u.TenDangNhap.ToLower() == model.TenDangNhap.Trim().ToLower() ||
                u.Email.ToLower() == model.Email.Trim().ToLower());

            if (exists)
            {
                return Json(new { success = false, message = "Tên đăng nhập hoặc Email đã tồn tại trong hệ thống." });
            }

            // 2. Mã hóa mật khẩu
            string hashedPassword = HashPassword(model.MatKhau);

            // 3. Khởi tạo đối tượng NguoiDung
            var newUser = new NguoiDung
            {
                TenDangNhap = model.TenDangNhap.Trim(),
                HoTen = model.HoTen.Trim(),
                Email = model.Email.Trim(),
                SoDienThoai = model.SoDienThoai?.Trim(),
                MatKhauMaHoa = hashedPassword,
                VaiTro = model.VaiTro,
                TrangThaiHoatDong = true,
                NgayTao = DateTime.Now
            };

            _context.NguoiDungs.Add(newUser);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Tạo tài khoản thành công!" });
        }

        // Hàm băm mật khẩu chuẩn SHA256
        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}