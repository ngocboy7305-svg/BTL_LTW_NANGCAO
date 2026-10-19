using BTL_QLTuyenSinh.Data;
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
    }
}