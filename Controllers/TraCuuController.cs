using BTL_QLTuyenSinh.Data;
using BTL_QLTuyenSinh.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BTL_QLTuyenSinh.Controllers
{
    public class TraCuuController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TraCuuController(ApplicationDbContext context)
        {
            _context = context;
        }

        public class CreateGhiChuTraCuuDto
        {
            public int MaHoSoId { get; set; }
            public string LoaiThaoTac { get; set; } = string.Empty;
            public string NoiDungChiTiet { get; set; } = string.Empty;
            public string NguoiThucHien { get; set; } = string.Empty;
        }

        [HttpGet]
        [Route("TraCuu")]
        [Route("TraCuu/Index")]
        [Route("ThiSinh/TraCuu")]
        public async Task<IActionResult> Index()
        {
            var danhSachHoSo = await _context.HoSoTuyenSinh
                .AsNoTracking()
                .Include(h => h.ThiSinh)
                .OrderBy(h => h.MaHoSo)
                .Select(h => new
                {
                    h.MaHoSoId,
                    HienThi = $"{h.MaHoSo} - {(h.ThiSinh != null ? h.ThiSinh.HoTen : "Thí sinh")} (CCCD: {(h.ThiSinh != null ? h.ThiSinh.SoCCCD : "—")})"
                })
                .ToListAsync();

            ViewBag.DanhSachHoSo = danhSachHoSo;
            return View("~/Views/TraCuu/Index.cshtml");
        }

        [HttpGet]
        [Route("TraCuu/GetListJson")]
        public async Task<IActionResult> GetListJson(
            string? tuKhoa,
            string? trangThaiHoSo,
            string? ketQuaXetTuyen,
            int page = 1,
            int pageSize = 5)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 50) pageSize = 5;

            var query = _context.HoSoTuyenSinh
                .AsNoTracking()
                .Include(h => h.ThiSinh)
                .Include(h => h.DanhSachNguyenVong)
                    .ThenInclude(nv => nv.NganhHoc)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                string kw = tuKhoa.Trim().ToLower();
                query = query.Where(h =>
                    h.MaHoSo.ToLower().Contains(kw) ||
                    (h.ThiSinh != null && (
                        h.ThiSinh.HoTen.ToLower().Contains(kw) ||
                        (h.ThiSinh.SoCCCD != null && h.ThiSinh.SoCCCD.ToLower().Contains(kw)) ||
                        h.ThiSinh.Email.ToLower().Contains(kw)
                    )));
            }

            if (!string.IsNullOrWhiteSpace(trangThaiHoSo))
            {
                query = query.Where(h => h.TrangThaiHoSo == trangThaiHoSo);
            }

            if (!string.IsNullOrWhiteSpace(ketQuaXetTuyen))
            {
                if (ketQuaXetTuyen == "TrungTuyen")
                    query = query.Where(h => h.DanhSachNguyenVong.Any(nv => nv.KetQuaXetTuyen == "Trúng tuyển"));
                else if (ketQuaXetTuyen == "ChoXetTuyen")
                    query = query.Where(h => h.DanhSachNguyenVong.All(nv => nv.KetQuaXetTuyen == "Chờ xét tuyển"));
                else if (ketQuaXetTuyen == "Truot")
                    query = query.Where(h => h.DanhSachNguyenVong.Any() && !h.DanhSachNguyenVong.Any(nv => nv.KetQuaXetTuyen == "Trúng tuyển"));
            }

            query = query.OrderBy(h => h.MaHoSo);

            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            if (totalPages > 0 && page > totalPages) page = totalPages;

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(h => new
                {
                    maHoSoId = h.MaHoSoId,
                    maHoSo = h.MaHoSo,
                    hoTen = h.ThiSinh != null ? h.ThiSinh.HoTen : "—",
                    soCCCD = h.ThiSinh != null ? (h.ThiSinh.SoCCCD ?? "—") : "—",
                    khuVucUuTien = h.KhuVucUuTien,
                    doiTuongUuTien = h.DoiTuongUuTien,
                    diemCongUuTien = h.DiemCongUuTien,
                    trangThaiHoSo = h.TrangThaiHoSo,
                    ghiChuDuyet = h.GhiChuDuyet ?? "Chưa có ghi chú",
                    ngayNop = h.NgayNop.ToLocalTime().ToString("dd/MM/yyyy HH:mm"),
                    soNguyenVong = h.DanhSachNguyenVong.Count,
                    daTrungTuyen = h.DanhSachNguyenVong.Any(nv => nv.KetQuaXetTuyen == "Trúng tuyển"),
                    nganhTrungTuyen = h.DanhSachNguyenVong
                        .Where(nv => nv.KetQuaXetTuyen == "Trúng tuyển")
                        .Select(nv => nv.NganhHoc != null ? $"{nv.NganhHoc.TenNganh} (NV{nv.ThuTuNguyenVong})" : "Trúng tuyển")
                        .FirstOrDefault() ?? "Chưa trúng tuyển",
                    danhSachNguyenVong = h.DanhSachNguyenVong
                        .OrderBy(nv => nv.ThuTuNguyenVong)
                        .Select(nv => new
                        {
                            thuTu = nv.ThuTuNguyenVong,
                            tenNganh = nv.NganhHoc != null ? nv.NganhHoc.TenNganh : "",
                            khoiXetTuyen = nv.KhoiXetTuyen,
                            tongDiemXetTuyen = nv.TongDiemXetTuyen ?? 0m,
                            diemChuan = nv.NganhHoc != null ? nv.NganhHoc.DiemChuan : null,
                            ketQua = nv.KetQuaXetTuyen
                        }).ToList()
                })
                .ToListAsync();

            var lichSuItems = await _context.LichSuThaoTac
                .AsNoTracking()
                .OrderByDescending(l => l.ThoiGianThucHien)
                .Take(8)
                .Select(l => new
                {
                    maLichSuId = l.MaLichSuId,
                    loaiThaoTac = l.LoaiThaoTac,
                    noiDungChiTiet = l.NoiDungChiTiet,
                    nguoiThucHien = l.NguoiThucHien,
                    thoiGian = l.ThoiGianThucHien.ToLocalTime().ToString("dd/MM/yyyy HH:mm")
                })
                .ToListAsync();

            return Json(new
            {
                success = true,
                data = items,
                lichSu = lichSuItems,
                currentPage = page,
                pageSize = pageSize,
                totalItems = totalItems,
                totalPages = totalPages
            });
        }

        [HttpPost]
        [Route("TraCuu/CreateGhiChuAjax")]
        public async Task<IActionResult> CreateGhiChuAjax([FromBody] CreateGhiChuTraCuuDto dto)
        {
            if (dto == null || dto.MaHoSoId <= 0 || string.IsNullOrWhiteSpace(dto.NoiDungChiTiet))
                return Json(new { success = false, message = "Vui lòng nhập nội dung theo dõi / tra cứu!" });

            var hoSo = await _context.HoSoTuyenSinh.FindAsync(dto.MaHoSoId);
            if (hoSo == null) return Json(new { success = false, message = "Không tìm thấy hồ sơ." });

            var lichSu = new LichSuThaoTac
            {
                MaHoSoId = hoSo.MaHoSoId,
                LoaiThaoTac = !string.IsNullOrWhiteSpace(dto.LoaiThaoTac) ? dto.LoaiThaoTac.Trim() : "Theo dõi hồ sơ (FR-05/06)",
                NoiDungChiTiet = dto.NoiDungChiTiet.Trim(),
                NguoiThucHien = !string.IsNullOrWhiteSpace(dto.NguoiThucHien) ? dto.NguoiThucHien.Trim() : (User.Identity?.Name ?? "Đào Việt Đức"),
                ThoiGianThucHien = DateTime.UtcNow
            };

            _context.LichSuThaoTac.Add(lichSu);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = $"Đã ghi nhận nhật ký theo dõi cho hồ sơ {hoSo.MaHoSo}!" });
        }

        [HttpPost]
        [Route("TraCuu/DeleteLichSuAjax")]
        public async Task<IActionResult> DeleteLichSuAjax(int id)
        {
            var ls = await _context.LichSuThaoTac.FindAsync(id);
            if (ls == null) return Json(new { success = false, message = "Không tìm thấy bản ghi cần xóa." });

            _context.LichSuThaoTac.Remove(ls);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã xóa bản ghi nhật ký thành công!" });
        }
    }
}