using BTL_QLTuyenSinh.Data;
using BTL_QLTuyenSinh.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace BTL_QLTuyenSinh.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "QuanTri,CanBo")]
    public class NganhHocController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;

        private const string DanhSachNganhCacheKey = "DanhSachNganhChuaXoa";

        public NganhHocController(
            ApplicationDbContext context,
            IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // GET: /Admin/NganhHoc
        public async Task<IActionResult> Index(string? tuKhoa)
        {
            // Lấy danh sách từ cache; nếu chưa có thì truy vấn SQL Server.
            var danhSachNganh = await _cache.GetOrCreateAsync(
                DanhSachNganhCacheKey,
                async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);

                    return await _context.NganhHoc
                        .AsNoTracking()
                        .Where(n => !n.DaXoa)
                        .OrderBy(n => n.MaNganh)
                        .ToListAsync();
                });

            danhSachNganh ??= new List<NganhHoc>();

            // Tìm kiếm trên danh sách đã cache.
            var danhSachHienThi = danhSachNganh.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                tuKhoa = tuKhoa.Trim();

                danhSachHienThi = danhSachHienThi.Where(n =>
                    (!string.IsNullOrEmpty(n.TenNganh) &&
                    n.TenNganh.Contains(tuKhoa, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(n.MaNganh) &&
                    n.MaNganh.Contains(tuKhoa, StringComparison.OrdinalIgnoreCase)));
            }

            ViewBag.TuKhoa = tuKhoa ?? string.Empty;

            return View(danhSachHienThi.ToList());
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
                var maNganh = model.MaNganh?.Trim();

                if (string.IsNullOrWhiteSpace(maNganh))
                {
                    ModelState.AddModelError(
                        nameof(model.MaNganh),
                        "Mã ngành không được để trống.");

                    return View(model);
                }

                // Kiểm tra mã ngành đã tồn tại, kể cả ngành đã bị đánh dấu xóa.
                var daTonTai = await _context.NganhHoc
                    .AnyAsync(n => n.MaNganh == maNganh);

                if (daTonTai)
                {
                    ModelState.AddModelError(
                        nameof(model.MaNganh),
                        "Mã ngành này đã tồn tại trong hệ thống.");

                    return View(model);
                }

                model.MaNganh = maNganh;
                model.DaXoa = false;

                _context.NganhHoc.Add(model);
                await _context.SaveChangesAsync();

                // Xóa cache để lần truy cập tiếp theo nạp lại dữ liệu mới.
                _cache.Remove(DanhSachNganhCacheKey);

                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }
    }
}