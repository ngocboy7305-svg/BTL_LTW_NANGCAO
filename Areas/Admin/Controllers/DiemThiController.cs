using BTL_QLTuyenSinh.Data;
using BTL_QLTuyenSinh.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BTL_QLTuyenSinh.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "QuanTri,CanBo")]
    public class DiemThiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DiemThiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/DiemThi
        public async Task<IActionResult> Index(string? tuKhoa)
        {
            var query = _context.DiemThi
                .AsNoTracking()
                .Include(d => d.HoSo)
                .Include(d => d.MonThi)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                tuKhoa = tuKhoa.Trim();

                query = query.Where(d =>
                    (d.HoSo != null &&
                     d.HoSo.MaHoSo.Contains(tuKhoa)) ||
                    (d.MonThi != null &&
                     (d.MonThi.MaMon.Contains(tuKhoa) ||
                      d.MonThi.TenMon.Contains(tuKhoa))));
            }

            ViewBag.TuKhoa = tuKhoa ?? string.Empty;

            var danhSachDiem = await query
                .OrderBy(d => d.HoSo!.MaHoSo)
                .ThenBy(d => d.MonThi!.TenMon)
                .ToListAsync();

            return View(danhSachDiem);
        }

        // GET: /Admin/DiemThi/Create
        public async Task<IActionResult> Create()
        {
            await NapDanhSachChonAsync();
            return View(new DiemThi());
        }

        // POST: /Admin/DiemThi/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DiemThi model)
        {
            ModelState.Remove(nameof(model.HoSo));
            ModelState.Remove(nameof(model.MonThi));

            if (!await _context.HoSoTuyenSinh
                    .AnyAsync(h => h.MaHoSoId == model.MaHoSoId))
            {
                ModelState.AddModelError(
                    nameof(model.MaHoSoId),
                    "Hồ sơ tuyển sinh không tồn tại.");
            }

            if (!await _context.MonThi
                    .AnyAsync(m => m.MaMonId == model.MaMonId))
            {
                ModelState.AddModelError(
                    nameof(model.MaMonId),
                    "Môn thi không tồn tại.");
            }

            var daCoDiem = await _context.DiemThi.AnyAsync(d =>
                d.MaHoSoId == model.MaHoSoId &&
                d.MaMonId == model.MaMonId);

            if (daCoDiem)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Hồ sơ này đã có điểm cho môn thi đã chọn.");
            }

            if (!ModelState.IsValid)
            {
                await NapDanhSachChonAsync();
                return View(model);
            }

            model.MaDiemId = 0;
            model.NgayCapNhat = DateTime.UtcNow;

            _context.DiemThi.Add(model);
            await _context.SaveChangesAsync();

            TempData["ThongBao"] = "Thêm điểm thi thành công.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/DiemThi/Edit/1
        public async Task<IActionResult> Edit(int id)
        {
            var diemThi = await _context.DiemThi
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.MaDiemId == id);

            if (diemThi == null)
            {
                return NotFound();
            }

            await NapDanhSachChonAsync();
            return View(diemThi);
        }

        // POST: /Admin/DiemThi/Edit/1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DiemThi model)
        {
            if (id != model.MaDiemId)
            {
                return BadRequest();
            }

            ModelState.Remove(nameof(model.HoSo));
            ModelState.Remove(nameof(model.MonThi));

            var diemTrongDb = await _context.DiemThi
                .FirstOrDefaultAsync(d => d.MaDiemId == id);

            if (diemTrongDb == null)
            {
                return NotFound();
            }

            if (!await _context.HoSoTuyenSinh
                    .AnyAsync(h => h.MaHoSoId == model.MaHoSoId))
            {
                ModelState.AddModelError(
                    nameof(model.MaHoSoId),
                    "Hồ sơ tuyển sinh không tồn tại.");
            }

            if (!await _context.MonThi
                    .AnyAsync(m => m.MaMonId == model.MaMonId))
            {
                ModelState.AddModelError(
                    nameof(model.MaMonId),
                    "Môn thi không tồn tại.");
            }

            var daCoDiem = await _context.DiemThi.AnyAsync(d =>
                d.MaDiemId != id &&
                d.MaHoSoId == model.MaHoSoId &&
                d.MaMonId == model.MaMonId);

            if (daCoDiem)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Hồ sơ này đã có điểm cho môn thi đã chọn.");
            }

            if (!ModelState.IsValid)
            {
                await NapDanhSachChonAsync();
                return View(model);
            }

            diemTrongDb.MaHoSoId = model.MaHoSoId;
            diemTrongDb.MaMonId = model.MaMonId;
            diemTrongDb.DiemSo = model.DiemSo;
            diemTrongDb.NgayCapNhat = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["ThongBao"] = "Cập nhật điểm thi thành công.";
            return RedirectToAction(nameof(Index));
        }

        private async Task NapDanhSachChonAsync()
        {
            var danhSachHoSo = await _context.HoSoTuyenSinh
                .AsNoTracking()
                .OrderBy(h => h.MaHoSo)
                .Select(h => new
                {
                    h.MaHoSoId,
                    h.MaHoSo
                })
                .ToListAsync();

            var danhSachMon = await _context.MonThi
                .AsNoTracking()
                .OrderBy(m => m.TenMon)
                .Select(m => new
                {
                    m.MaMonId,
                    TenHienThi = m.MaMon + " - " + m.TenMon
                })
                .ToListAsync();

            ViewBag.DanhSachHoSo = new SelectList(
                danhSachHoSo,
                "MaHoSoId",
                "MaHoSo");

            ViewBag.DanhSachMon = new SelectList(
                danhSachMon,
                "MaMonId",
                "TenHienThi");
        }
    }
}
