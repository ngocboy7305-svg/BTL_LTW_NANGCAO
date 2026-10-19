using BTL_QLTuyenSinh.Data;
using BTL_QLTuyenSinh.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BTL_QLTuyenSinh.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "QuanTri,CanBo")]
    public class NganhHocController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NganhHocController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/NganhHoc
        public async Task<IActionResult> Index(string? tuKhoa)
        {
            var query = _context.NganhHoc
                .Where(n => !n.DaXoa)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                tuKhoa = tuKhoa.Trim();
                query = query.Where(n => n.TenNganh.Contains(tuKhoa) || n.MaNganh.Contains(tuKhoa));
            }

            ViewBag.TuKhoa = tuKhoa ?? "";
            var danhSach = await query.OrderBy(n => n.MaNganh).ToListAsync();
            return View(danhSach);
        }

        // GET: /Admin/NganhHoc/Create
        [Authorize(Roles = "QuanTri")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Admin/NganhHoc/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "QuanTri")]
        public async Task<IActionResult> Create(NganhHoc model)
        {
            if (ModelState.IsValid)
            {
                if (await _context.NganhHoc.AnyAsync(n => n.MaNganh == model.MaNganh))
                {
                    ModelState.AddModelError("MaNganh", "Mã ngành này đã tồn tại trong hệ thống.");
                    return View(model);
                }

                model.DaXoa = false;
                _context.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }
    }
}