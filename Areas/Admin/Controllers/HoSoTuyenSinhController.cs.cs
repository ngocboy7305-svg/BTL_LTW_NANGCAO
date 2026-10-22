using System.Security.Claims;
using BTL_QLTuyenSinh.Data;
using BTL_QLTuyenSinh.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BTL_QLTuyenSinh.Areas.Admin.Controllers
{
    // Chỉ Cán bộ và Quản trị viên được truy cập
    [Area("Admin")]
    [Authorize(Roles = "CanBo,QuanTri")]
    public class HoSoTuyenSinhController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Khởi tạo kết nối database
        public HoSoTuyenSinhController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // ========================================
        // FR-09: HIỂN THỊ DANH SÁCH HỒ SƠ
        // ========================================

        [HttpGet]
        public async Task<IActionResult> Index(
            string? trangThai)
        {
            // Lấy danh sách hồ sơ và thông tin thí sinh
            var query = _context.HoSoTuyenSinh
                .AsNoTracking()
                .Include(h => h.ThiSinh)
                .AsQueryable();

            // Lọc hồ sơ theo trạng thái
            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                query = query.Where(h =>
                    h.TrangThaiHoSo == trangThai);
            }

            // Lưu trạng thái lọc để hiển thị trên View
            ViewBag.TrangThai = trangThai;

            // Sắp xếp hồ sơ mới nhất lên đầu
            var danhSach = await query
                .OrderByDescending(h => h.NgayNop)
                .ToListAsync();

            // Trả về giao diện Index.cshtml
            return View(danhSach);
        }

        // ========================================
        // FR-09: XEM CHI TIẾT HỒ SƠ
        // ========================================

        [HttpGet]
        public async Task<IActionResult> ChiTiet(int id)
        {
            // Lấy hồ sơ theo mã ID
            var hoSo = await _context.HoSoTuyenSinh
                .AsNoTracking()
                .Include(h => h.ThiSinh)
                .Include(h => h.DanhSachNguyenVong)
                    .ThenInclude(nv => nv.NganhHoc)
                .FirstOrDefaultAsync(h =>
                    h.MaHoSoId == id);

            // Nếu không tìm thấy hồ sơ
            if (hoSo == null)
            {
                return NotFound();
            }

            // Hiển thị thông tin hồ sơ
            return View(hoSo);
        }

        // ========================================
        // FR-09: DUYỆT HOẶC TỪ CHỐI HỒ SƠ
        // ========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Duyet(
            int id,
            string ketQua,
            string? ghiChu)
        {// 1. Kiểm tra độ dài ghi chú
            if (ghiChu?.Length > 500)
            {
                return BadRequest(
                    "Ghi chú không được vượt quá 500 ký tự.");
            }

            // 2. Kiểm tra kết quả duyệt hợp lệ
            if (ketQua != "Hợp lệ" &&
                ketQua != "Không hợp lệ")
            {
                return BadRequest(
                    "Trạng thái duyệt không hợp lệ.");
            }

            // 3. Bắt buộc có lý do khi từ chối
            if (ketQua == "Không hợp lệ" &&
                string.IsNullOrWhiteSpace(ghiChu))
            {
                TempData["Loi"] =
                    "Hồ sơ không hợp lệ cần nêu lý do.";

                return RedirectToAction(
                    nameof(ChiTiet), new { id });
            }

            // 4. Tìm hồ sơ cần duyệt
            var hoSo = await _context.HoSoTuyenSinh
                .FirstOrDefaultAsync(h =>
                    h.MaHoSoId == id);

            if (hoSo == null)
            {
                return NotFound();
            }

            // 5. Chỉ được duyệt hồ sơ Chờ duyệt
            if (hoSo.TrangThaiHoSo != "Chờ duyệt")
            {
                TempData["Loi"] =
                    "Hồ sơ đã được xử lý hoặc đã hủy.";

                return RedirectToAction(
                    nameof(ChiTiet), new { id });
            }

            // 6. Lấy ID của cán bộ đang đăng nhập
            if (!int.TryParse(
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier),
                out var userId))
            {
                return Forbid();
            }

            // 7. Cập nhật kết quả duyệt
            hoSo.TrangThaiHoSo = ketQua;

            // Lưu ghi chú duyệt
            hoSo.GhiChuDuyet = ghiChu?.Trim();

            // Lưu mã cán bộ duyệt
            hoSo.MaCanBoDuyet = userId;

            // Lưu thời gian duyệt
            hoSo.NgayDuyet = DateTime.UtcNow;

            // ====================================
            // 8. GHI LỊCH SỬ THAO TÁC
            // ====================================

            var lichSu = new LichSuThaoTac
            {
                MaHoSoId = id,

                LoaiThaoTac = "Duyệt hồ sơ",

                NoiDungChiTiet =
                    $"Kết quả: {ketQua}. " +
                    $"Ghi chú: {ghiChu?.Trim()}",

                NguoiThucHien = userId.ToString(),

                ThoiGianThucHien = DateTime.UtcNow
            };

            // Thêm lịch sử vào database
            _context.LichSuThaoTac.Add(lichSu);

            // ====================================
            // 9. LƯU THAY ĐỔI VÀO SQL SERVER
            // ====================================

            await _context.SaveChangesAsync();// Thông báo thành công
            TempData["ThanhCong"] =
                "Đã cập nhật trạng thái hồ sơ.";

            // Quay về trang chi tiết hồ sơ
            return RedirectToAction(
                nameof(ChiTiet), new { id });
        }
    }
}