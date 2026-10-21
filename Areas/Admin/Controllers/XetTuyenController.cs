using BTL_QLTuyenSinh.Data;
using BTL_QLTuyenSinh.Services;
using BTL_QLTuyenSinh.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BTL_QLTuyenSinh.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "QuanTri,CanBo")]
    public class XetTuyenController : Controller
    {
        private readonly XetTuyenService _xetTuyenService;
        private readonly ApplicationDbContext _context;

        public XetTuyenController(XetTuyenService xetTuyenService, ApplicationDbContext context)
        {
            _xetTuyenService = xetTuyenService;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RunAlgorithm([FromBody] TieuChiXetTuyenDto tieuChi)
        {
            var ketQua = await _xetTuyenService.ChayThuatToanXetTuyenAsync(tieuChi);
            return PartialView("_BangKetQuaPartial", ketQua);
        }

        [HttpPost]
        public async Task<IActionResult> PublishResult([FromBody] List<KetQuaXetTuyenNganhDto> results)
        {
            if (results == null || !results.Any())
                return Json(new { success = false, message = "Chưa có kết quả xét tuyển." });

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Gom tất cả ID các nguyện vọng trúng tuyển
                var allTrungTuyenIds = results
                    .Where(r => r.DanhSachMaNguyenVongTrungTuyen != null)
                    .SelectMany(r => r.DanhSachMaNguyenVongTrungTuyen)
                    .ToHashSet();

                // 2. Cập nhật Điểm chuẩn cho bảng Ngành học
                foreach (var item in results)
                {
                    var nganh = await _context.NganhHocs.FindAsync(item.NganhHocId);
                    if (nganh != null)
                    {
                        nganh.DiemChuan = (decimal)item.DiemChuan;
                    }
                }

                // 3. Cập nhật Trạng thái trúng tuyển cho bảng Nguyện vọng
                var danhSachNganhIds = results.Select(r => r.NganhHocId).ToList();
                var allNguyenVongs = await _context.NguyenVongs
                    .Where(nv => danhSachNganhIds.Contains(nv.MaNganhId))
                    .ToListAsync();

                foreach (var nv in allNguyenVongs)
                {
                    if (allTrungTuyenIds.Contains(nv.MaNguyenVongId))
                    {
                        nv.KetQuaXetTuyen = "Trúng tuyển";
                    }
                    else
                    {
                        nv.KetQuaXetTuyen = "Không trúng tuyển";
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { success = true, message = "Đã phê duyệt và công bố bảng điểm chuẩn cùng danh sách trúng tuyển thành công!" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "Lỗi khi lưu dữ liệu: " + ex.Message });
            }
        }
    }
}