using BTL_QLTuyenSinh.Data;
using BTL_QLTuyenSinh.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BTL_QLTuyenSinh.Controllers
{
    public class ThongBaoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ThongBaoController(ApplicationDbContext context)
        {
            _context = context;
        }

        public class CreateThongBaoAjaxDto
        {
            public int MaNguoiNhan { get; set; } // 0 = Gửi tất cả thí sinh
            public string TieuDe { get; set; } = string.Empty;
            public string NoiDung { get; set; } = string.Empty;
        }

        [HttpGet]
        [Route("ThongBao")]
        [Route("ThongBao/Index")]
        [Route("Admin/ThongBao")]
        [Route("Admin/ThongBao/Index")]
        public async Task<IActionResult> Index()
        {
            var danhSachThiSinh = await _context.NguoiDung
                .AsNoTracking()
                .Where(u => u.VaiTro == "ThiSinh" && u.TrangThaiHoatDong)
                .OrderBy(u => u.HoTen)
                .Select(u => new
                {
                    u.MaNguoiDung,
                    HienThi = $"{u.HoTen} ({u.SoCCCD ?? u.TenDangNhap} - {u.Email})"
                })
                .ToListAsync();

            ViewBag.DanhSachThiSinh = danhSachThiSinh;
            return View();
        }

        [HttpGet]
        [Route("ThongBao/GetListJson")]
        [Route("Admin/ThongBao/GetListJson")]
        public async Task<IActionResult> GetListJson(
            string? tuKhoa,
            string? trangThaiXem,
            int? maNguoiNhan,
            string? sortOrder,
            int page = 1,
            int pageSize = 5)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 50) pageSize = 5;

            var query = _context.ThongBao
                .AsNoTracking()
                .Include(t => t.NguoiNhan)
                .AsQueryable();

            bool isThiSinh = User.Identity != null && User.Identity.IsAuthenticated && User.IsInRole("ThiSinh");
            if (isThiSinh && (!maNguoiNhan.HasValue || maNguoiNhan.Value == 0))
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(userIdStr, out int currentUserId))
                {
                    query = query.Where(t => t.MaNguoiNhan == currentUserId);
                }
            }
            else if (maNguoiNhan.HasValue && maNguoiNhan.Value > 0)
            {
                query = query.Where(t => t.MaNguoiNhan == maNguoiNhan.Value);
            }

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                string kw = tuKhoa.Trim().ToLower();
                query = query.Where(t =>
                    t.TieuDe.ToLower().Contains(kw) ||
                    t.NoiDung.ToLower().Contains(kw) ||
                    (t.NguoiNhan != null && (
                        t.NguoiNhan.HoTen.ToLower().Contains(kw) ||
                        t.NguoiNhan.Email.ToLower().Contains(kw) ||
                        (t.NguoiNhan.SoCCCD != null && t.NguoiNhan.SoCCCD.ToLower().Contains(kw))
                    )));
            }

            if (!string.IsNullOrWhiteSpace(trangThaiXem))
            {
                if (trangThaiXem == "chua_xem") query = query.Where(t => !t.DaXem);
                else if (trangThaiXem == "da_xem") query = query.Where(t => t.DaXem);
            }

            query = sortOrder switch
            {
                "oldest" => query.OrderBy(t => t.NgayGui),
                "unread_first" => query.OrderBy(t => t.DaXem).ThenByDescending(t => t.NgayGui),
                _ => query.OrderByDescending(t => t.NgayGui)
            };

            int totalItems = await query.CountAsync();
            int unreadCount = await query.CountAsync(t => !t.DaXem);
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            if (totalPages > 0 && page > totalPages) page = totalPages;

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new
                {
                    maThongBaoId = t.MaThongBaoId,
                    tenNguoiNhan = t.NguoiNhan != null ? t.NguoiNhan.HoTen : "Thí sinh",
                    soCCCD = t.NguoiNhan != null ? (t.NguoiNhan.SoCCCD ?? "—") : "—",
                    tieuDe = t.TieuDe,
                    noiDung = t.NoiDung,
                    daXem = t.DaXem,
                    ngayGui = t.NgayGui.ToLocalTime().ToString("dd/MM/yyyy HH:mm")
                })
                .ToListAsync();

            return Json(new
            {
                success = true,
                data = items,
                currentPage = page,
                pageSize = pageSize,
                totalItems = totalItems,
                unreadCount = unreadCount,
                totalPages = totalPages
            });
        }

        [HttpPost]
        [Route("ThongBao/CreateAjax")]
        [Route("Admin/ThongBao/CreateAjax")]
        public async Task<IActionResult> CreateAjax([FromBody] CreateThongBaoAjaxDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.TieuDe) || string.IsNullOrWhiteSpace(dto.NoiDung))
                return Json(new { success = false, message = "Vui lòng nhập tiêu đề và nội dung!" });

            string tieuDeClean = dto.TieuDe.Trim();
            string noiDungClean = dto.NoiDung.Trim();

            if (dto.MaNguoiNhan == 0)
            {
                var allThiSinhIds = await _context.NguoiDung
                    .Where(u => u.VaiTro == "ThiSinh" && u.TrangThaiHoatDong)
                    .Select(u => u.MaNguoiDung)
                    .ToListAsync();

                if (!allThiSinhIds.Any())
                    return Json(new { success = false, message = "Không tìm thấy thí sinh nào." });

                var listNew = allThiSinhIds.Select(id => new ThongBao
                {
                    MaNguoiNhan = id,
                    TieuDe = tieuDeClean,
                    NoiDung = noiDungClean,
                    DaXem = false,
                    NgayGui = DateTime.UtcNow
                }).ToList();

                _context.ThongBao.AddRange(listNew);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = $"Đã phát hành thông báo tới toàn bộ {listNew.Count} thí sinh!" });
            }
            else
            {
                var tb = new ThongBao
                {
                    MaNguoiNhan = dto.MaNguoiNhan,
                    TieuDe = tieuDeClean,
                    NoiDung = noiDungClean,
                    DaXem = false,
                    NgayGui = DateTime.UtcNow
                };

                _context.ThongBao.Add(tb);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Đã gửi thông báo thành công!" });
            }
        }

        [HttpPost]
        [Route("ThongBao/DeleteAjax")]
        [Route("Admin/ThongBao/DeleteAjax")]
        public async Task<IActionResult> DeleteAjax(int id)
        {
            var tb = await _context.ThongBao.FindAsync(id);
            if (tb == null) return Json(new { success = false, message = "Không tìm thấy thông báo." });

            _context.ThongBao.Remove(tb);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Đã xóa thông báo thành công!" });
        }

        [HttpPost]
        [Route("ThongBao/ToggleDaXemAjax")]
        [Route("Admin/ThongBao/ToggleDaXemAjax")]
        public async Task<IActionResult> ToggleDaXemAjax(int id)
        {
            var tb = await _context.ThongBao.FindAsync(id);
            if (tb == null) return Json(new { success = false, message = "Không tìm thấy thông báo." });

            tb.DaXem = !tb.DaXem;
            await _context.SaveChangesAsync();
            return Json(new { success = true, daXem = tb.DaXem, message = tb.DaXem ? "Đã đánh dấu Đã xem." : "Đã chuyển sang Chưa xem." });
        }
    }
}