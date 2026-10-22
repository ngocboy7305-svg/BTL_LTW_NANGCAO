using System.Security.Claims;
using BTL_QLTuyenSinh.Data;
using BTL_QLTuyenSinh.Models;
using BTL_QLTuyenSinh.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BTL_QLTuyenSinh.Controllers
{
    [Authorize(Roles = "ThiSinh")]
    public class ThiSinhController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string ChoDuyet = "Chờ duyệt";

        public ThiSinhController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Lấy ID thí sinh đang đăng nhập
        private int? MaNguoiDung()
        {
            return int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out var id
            ) ? id : null;
        }

        // =======================================
        // 1. XEM HỒ SƠ CỦA THÍ SINH
        // =======================================

        [HttpGet]
        public async Task<IActionResult> HoSo()
        {
            var id = MaNguoiDung();

            if (id == null)
                return Challenge();

            var hoSo = await _context.HoSoTuyenSinh
                .AsNoTracking()
                .Include(h => h.DanhSachNguyenVong)
                    .ThenInclude(nv => nv.NganhHoc)
                .Where(h => h.MaThiSinh == id)
                .OrderByDescending(h => h.MaHoSoId)
                .FirstOrDefaultAsync();

            return View(hoSo);
        }

        [HttpGet]
        public IActionResult NguyenVong()
        {
            return RedirectToAction(nameof(DangKy));
        }

        // =======================================
        // 2. FR-03: HIỂN THỊ FORM ĐĂNG KÝ
        // =======================================

        [HttpGet]
        public async Task<IActionResult> DangKy()
        {
            var id = MaNguoiDung();

            if (id == null)
                return Challenge();

            bool daCoHoSo = await _context.HoSoTuyenSinh
                .AnyAsync(h =>
                    h.MaThiSinh == id &&
                    h.TrangThaiHoSo != "Đã hủy");

            if (daCoHoSo)
            {
                TempData["ThongBao"] =
                    "Bạn đã có hồ sơ. Hãy xem hồ sơ của mình.";

                return RedirectToAction(nameof(HoSo));
            }

            await TaiDanhSachNganh();

            return View(new HoSoDangKyViewModel());
        }

        // =======================================
        // 3. FR-03: XỬ LÝ NỘP HỒ SƠ
        // =======================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangKy(
            HoSoDangKyViewModel model)
        {
            var id = MaNguoiDung();

            if (id == null)
                return Challenge(); bool daCoHoSo = await _context.HoSoTuyenSinh
                .AnyAsync(h =>
                    h.MaThiSinh == id &&
                    h.TrangThaiHoSo != "Đã hủy");

            if (daCoHoSo)
            {
                return BadRequest(
                    "Bạn đã có hồ sơ đăng ký chưa hủy.");
            }

            if (!await KiemTraDuLieu(model))
            {
                await TaiDanhSachNganh();
                return View(model);
            }

            var hoSo = new HoSoTuyenSinh
            {
                MaHoSo = "HS"
                    + DateTime.UtcNow.ToString("yyMMdd")
                    + Guid.NewGuid().ToString("N")[..12],

                MaThiSinh = id.Value,

                NamTuyenSinh = DateTime.Now.Year,

                KhuVucUuTien = model.KhuVucUuTien,

                DoiTuongUuTien = model.DoiTuongUuTien,

                DiemCongUuTien = 0m,

                TrangThaiHoSo = ChoDuyet,

                NgayNop = DateTime.UtcNow,

                DanhSachNguyenVong =
                    TaoDanhSachNguyenVong(model)
            };

            _context.HoSoTuyenSinh.Add(hoSo);

            await _context.SaveChangesAsync();

            TempData["ThongBao"] =
                "Đăng ký hồ sơ và nguyện vọng thành công!";

            return RedirectToAction(nameof(HoSo));
        }

        // =======================================
        // 4. FR-04: MỞ FORM SỬA HỒ SƠ
        // =======================================

        [HttpGet]
        public async Task<IActionResult> Sua(int id)
        {
            var userId = MaNguoiDung();

            if (userId == null)
                return Challenge();

            var hoSo = await _context.HoSoTuyenSinh
                .Include(h => h.DanhSachNguyenVong)
                .FirstOrDefaultAsync(h =>
                    h.MaHoSoId == id &&
                    h.MaThiSinh == userId);

            if (hoSo == null)
                return NotFound();

            if (hoSo.TrangThaiHoSo != ChoDuyet)
            {
                return BadRequest(
                    "Hồ sơ không còn được phép sửa.");
            }

            var model = new HoSoDangKyViewModel
            {
                MaHoSoId = hoSo.MaHoSoId,

                KhuVucUuTien = hoSo.KhuVucUuTien,

                DoiTuongUuTien = hoSo.DoiTuongUuTien,

                NguyenVongs = hoSo.DanhSachNguyenVong
                    .OrderBy(nv => nv.ThuTuNguyenVong)
                    .Select(nv =>
                        new NguyenVongNhapViewModel
                        {
                            MaNganhId = nv.MaNganhId,
                            KhoiXetTuyen = nv.KhoiXetTuyen
                        })
                    .ToList()
            };

            while (model.NguyenVongs.Count < 3)
            {
                model.NguyenVongs.Add(
                    new NguyenVongNhapViewModel());
            }

            await TaiDanhSachNganh();

            return View("DangKy", model);
        }

        // =======================================
        // 5. FR-04: LƯU HỒ SƠ ĐÃ SỬA
        // =======================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sua(
            HoSoDangKyViewModel model)
        {
            var userId = MaNguoiDung();

            if (userId == null)
                return Challenge();

            if (!model.MaHoSoId.HasValue)
                return BadRequest("Thiếu mã hồ sơ.");

            var hoSo = await _context.HoSoTuyenSinh
                .Include(h => h.DanhSachNguyenVong)
                .FirstOrDefaultAsync(h =>
                    h.MaHoSoId == model.MaHoSoId.Value &&
                    h.MaThiSinh == userId.Value);

            if (hoSo == null)
                return NotFound();

            if (hoSo.TrangThaiHoSo != ChoDuyet)
            {
                return BadRequest(
                    "Hồ sơ đã được xử lý, không thể sửa.");
            }

            if (!await KiemTraDuLieu(model))
            {
                await TaiDanhSachNganh();
                return View("DangKy", model);
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                // Cập nhật thông tin hồ sơ
                hoSo.KhuVucUuTien = model.KhuVucUuTien;
                hoSo.DoiTuongUuTien = model.DoiTuongUuTien;

                // Xóa nguyện vọng cũ
                _context.NguyenVong.RemoveRange(
                    hoSo.DanhSachNguyenVong);

                await _context.SaveChangesAsync();

                // Tạo danh sách nguyện vọng mới
                var danhSachMoi =
                    TaoDanhSachNguyenVong(model);

                foreach (var nv in danhSachMoi)
                {
                    nv.MaHoSoId = hoSo.MaHoSoId;
                }

                _context.NguyenVong.AddRange(danhSachMoi);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                TempData["ThongBao"] =
                    "Cập nhật hồ sơ thành công!";

                return RedirectToAction(nameof(HoSo));
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync();

                ModelState.AddModelError("",
                    "Không thể lưu hồ sơ. Vui lòng thử lại.");

                await TaiDanhSachNganh();

                return View("DangKy", model);
            }
        }

        // =======================================
        // 6. FR-04: HỦY ĐĂNG KÝ HỒ SƠ
        // =======================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Huy(int id)
        {
            var userId = MaNguoiDung();

            if (userId == null)
                return Challenge();

            var hoSo = await _context.HoSoTuyenSinh
                .FirstOrDefaultAsync(h =>
                    h.MaHoSoId == id &&
                    h.MaThiSinh == userId);

            if (hoSo == null)
                return NotFound();

            if (hoSo.TrangThaiHoSo != ChoDuyet)
            {
                return BadRequest(
                    "Không thể hủy hồ sơ đã xử lý.");
            }

            hoSo.TrangThaiHoSo = "Đã hủy";

            await _context.SaveChangesAsync();

            TempData["ThongBao"] =
                "Bạn đã hủy đăng ký thành công.";

            return RedirectToAction(nameof(HoSo));
        }

        // =======================================
        // 7. TẢI DANH SÁCH NGÀNH HỌC
        // =======================================

        private async Task TaiDanhSachNganh()
        {
            ViewBag.Nganhs = await _context.NganhHoc
                .AsNoTracking()
                .Where(n => n.DangMoDangKy && !n.DaXoa)
                .OrderBy(n => n.TenNganh)
                .ToListAsync();
        }

        // =======================================
        // 8. KIỂM TRA DỮ LIỆU HỒ SƠ
        // =======================================

        private async Task<bool> KiemTraDuLieu(
            HoSoDangKyViewModel model)
        {
            var khuVucHopLe = new[]
            {
                "KV1", "KV2-NT", "KV2", "KV3"
            };

            if (!khuVucHopLe.Contains(model.KhuVucUuTien))
            {
                ModelState.AddModelError(
                    nameof(model.KhuVucUuTien),
                    "Khu vực không hợp lệ.");
            }

            var doiTuongHopLe = new[]
            {
                "Không", "ƯT1", "ƯT2"
            };

            if (!doiTuongHopLe.Contains(model.DoiTuongUuTien))
            {
                ModelState.AddModelError(
                    nameof(model.DoiTuongUuTien),
                    "Đối tượng không hợp lệ.");
            }

            model.NguyenVongs ??= new();

            if (model.NguyenVongs.Count != 3)
            {
                ModelState.AddModelError("",
                    "Biểu mẫu phải có 3 dòng nguyện vọng.");
            }

            if (model.NguyenVongs.Count == 0 ||
                !model.NguyenVongs[0].MaNganhId.HasValue)
            {
                ModelState.AddModelError("",
                    "Phải đăng ký ít nhất nguyện vọng 1.");
            }

            var daChon = model.NguyenVongs
                .Where(nv => nv.MaNganhId.HasValue)
                .ToList();

            if (daChon.Select(nv => nv.MaNganhId)
                .Distinct().Count() != daChon.Count)
            {
                ModelState.AddModelError("",
                    "Không được chọn trùng ngành.");
            }
            var ids = daChon
                .Select(nv => nv.MaNganhId!.Value)
                .ToList();

            var nganhs = await _context.NganhHoc
                .AsNoTracking()
                .Where(n =>
                    ids.Contains(n.MaNganhId) &&
                    n.DangMoDangKy &&
                    !n.DaXoa)
                .ToDictionaryAsync(n => n.MaNganhId);

            if (nganhs.Count != daChon
                .Select(nv => nv.MaNganhId)
                .Distinct().Count())
            {
                ModelState.AddModelError("",
                    "Ngành không tồn tại hoặc đã đóng đăng ký.");
            }

            foreach (var nv in model.NguyenVongs)
            {
                if (nv.MaNganhId == null)
                {
                    if (!string.IsNullOrWhiteSpace(
                        nv.KhoiXetTuyen))
                    {
                        ModelState.AddModelError("",
                            "Đã chọn tổ hợp nhưng chưa chọn ngành.");
                    }

                    continue;
                }

                if (string.IsNullOrWhiteSpace(
                    nv.KhoiXetTuyen))
                {
                    ModelState.AddModelError("",
                        "Bạn phải chọn tổ hợp xét tuyển.");
                    continue;
                }

                if (nganhs.TryGetValue(
                    nv.MaNganhId.Value, out var nganh))
                {
                    var toHops = nganh.ToHopXetTuyen
                        .Split(',')
                        .Select(s => s.Trim())
                        .ToList();

                    if (!toHops.Contains(
                        nv.KhoiXetTuyen.Trim(),
                        StringComparer.OrdinalIgnoreCase))
                    {
                        ModelState.AddModelError("",
                            $"Ngành {nganh.TenNganh} " +
                            $"không hỗ trợ tổ hợp {nv.KhoiXetTuyen}.");
                    }
                }
            }

            return ModelState.IsValid;
        }

        // =======================================
        // 9. TẠO DANH SÁCH NGUYỆN VỌNG
        // =======================================

        private static List<NguyenVong> TaoDanhSachNguyenVong(
            HoSoDangKyViewModel model)
        {
            return model.NguyenVongs
                .Select((nv, i) => (nv, i))
                .Where(x => x.nv.MaNganhId.HasValue)
                .Select(x => new NguyenVong
                {
                    MaNganhId = x.nv.MaNganhId!.Value,

                    ThuTuNguyenVong = x.i + 1,

                    KhoiXetTuyen =
                        x.nv.KhoiXetTuyen!.Trim(),

                    KetQuaXetTuyen = "Chờ xét tuyển"
                })
                .ToList();
        }
    }
}