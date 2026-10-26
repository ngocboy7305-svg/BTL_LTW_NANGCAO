using BTL_QLTuyenSinh.Data;
using BTL_QLTuyenSinh.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace BTL_QLTuyenSinh.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ThongKeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ThongKeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public class CreateBaoCaoLogDto
        {
            public string LoaiThaoTac { get; set; } = string.Empty;
            public string NoiDungChiTiet { get; set; } = string.Empty;
            public string NguoiThucHien { get; set; } = string.Empty;
        }

        [HttpGet]
        [Route("Admin/ThongKe")]
        [Route("Admin/ThongKe/Index")]
        [Route("ThongKe")]
        [Route("ThongKe/Index")]
        public async Task<IActionResult> Index()
        {
            var danhSachKhoa = await _context.NganhHoc
                .AsNoTracking()
                .Where(n => !n.DaXoa && n.TenKhoa != null && n.TenKhoa != "")
                .Select(n => n.TenKhoa!)
                .Distinct()
                .OrderBy(k => k)
                .ToListAsync();

            ViewBag.DanhSachKhoa = danhSachKhoa;
            return View("~/Areas/Admin/Views/ThongKe/Index.cshtml");
        }

        [HttpGet]
        [Route("Admin/ThongKe/GetListJson")]
        [Route("ThongKe/GetListJson")]
        public async Task<IActionResult> GetListJson(
            string? tuKhoa,
            string? khoa,
            string? sortOrder,
            int page = 1,
            int pageSize = 5)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 50) pageSize = 5;

            int tongThiSinh = await _context.NguoiDung.CountAsync(u => u.VaiTro == "ThiSinh");
            int tongHoSo = await _context.HoSoTuyenSinh.CountAsync();
            int hoSoHopLe = await _context.HoSoTuyenSinh.CountAsync(h => h.TrangThaiHoSo == "Hợp lệ");
            int hoSoChoDuyet = await _context.HoSoTuyenSinh.CountAsync(h => h.TrangThaiHoSo == "Chờ duyệt");
            int hoSoKhongHopLe = await _context.HoSoTuyenSinh.CountAsync(h => h.TrangThaiHoSo == "Không hợp lệ");
            int tongNguyenVong = await _context.NguyenVong.CountAsync();
            int tongTrungTuyen = await _context.NguyenVong.CountAsync(nv => nv.KetQuaXetTuyen == "Trúng tuyển");
            int tongThongBao = await _context.ThongBao.CountAsync();

            var query = _context.NganhHoc
                .AsNoTracking()
                .Include(n => n.DanhSachNguyenVong)
                .Where(n => !n.DaXoa)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                string kw = tuKhoa.Trim().ToLower();
                query = query.Where(n =>
                    n.MaNganh.ToLower().Contains(kw) ||
                    n.TenNganh.ToLower().Contains(kw) ||
                    (n.TenKhoa != null && n.TenKhoa.ToLower().Contains(kw)));
            }

            if (!string.IsNullOrWhiteSpace(khoa))
            {
                query = query.Where(n => n.TenKhoa == khoa);
            }

            query = sortOrder switch
            {
                "nv_desc" => query.OrderByDescending(n => n.DanhSachNguyenVong.Count),
                "chitieu_desc" => query.OrderByDescending(n => n.ChiTieu),
                "trungtuyen_desc" => query.OrderByDescending(n => n.DanhSachNguyenVong.Count(nv => nv.KetQuaXetTuyen == "Trúng tuyển")),
                _ => query.OrderBy(n => n.MaNganh)
            };

            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            if (totalPages > 0 && page > totalPages) page = totalPages;

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(n => new
                {
                    maNganhId = n.MaNganhId,
                    maNganh = n.MaNganh,
                    tenNganh = n.TenNganh,
                    tenKhoa = n.TenKhoa ?? "Chưa phân khoa",
                    chiTieu = n.ChiTieu,
                    diemSan = n.DiemSan,
                    diemChuan = n.DiemChuan,
                    tongNguyenVong = n.DanhSachNguyenVong.Count,
                    soNV1 = n.DanhSachNguyenVong.Count(nv => nv.ThuTuNguyenVong == 1),
                    soTrungTuyen = n.DanhSachNguyenVong.Count(nv => nv.KetQuaXetTuyen == "Trúng tuyển"),
                    tyLeChoi = n.ChiTieu > 0
                        ? Math.Round((double)n.DanhSachNguyenVong.Count / n.ChiTieu, 2)
                        : 0.0,
                    tyLeLapDay = n.ChiTieu > 0
                        ? Math.Round((double)n.DanhSachNguyenVong.Count(nv => nv.KetQuaXetTuyen == "Trúng tuyển") * 100.0 / n.ChiTieu, 1)
                        : 0.0
                })
                .ToListAsync();

            var baoCaoLogs = await _context.LichSuThaoTac
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
                kpi = new
                {
                    tongThiSinh,
                    tongHoSo,
                    hoSoHopLe,
                    hoSoChoDuyet,
                    hoSoKhongHopLe,
                    tongNguyenVong,
                    tongTrungTuyen,
                    tongThongBao
                },
                data = items,
                baoCaoLogs = baoCaoLogs,
                currentPage = page,
                pageSize = pageSize,
                totalItems = totalItems,
                totalPages = totalPages
            });
        }

        [HttpPost]
        [Route("Admin/ThongKe/CreateBaoCaoAjax")]
        [Route("ThongKe/CreateBaoCaoAjax")]
        public async Task<IActionResult> CreateBaoCaoAjax([FromBody] CreateBaoCaoLogDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.NoiDungChiTiet))
            {
                return Json(new { success = false, message = "Vui lòng nhập nội dung báo cáo!" });
            }

            var log = new LichSuThaoTac
            {
                MaHoSoId = null,
                LoaiThaoTac = !string.IsNullOrWhiteSpace(dto.LoaiThaoTac) ? dto.LoaiThaoTac.Trim() : "Báo cáo thống kê (FR-14, FR-15)",
                NoiDungChiTiet = dto.NoiDungChiTiet.Trim(),
                NguoiThucHien = !string.IsNullOrWhiteSpace(dto.NguoiThucHien) ? dto.NguoiThucHien.Trim() : "Đào Việt Đức",
                ThoiGianThucHien = DateTime.UtcNow
            };

            _context.LichSuThaoTac.Add(log);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã lưu biên bản chốt số liệu thành công!" });
        }

        [HttpPost]
        [Route("Admin/ThongKe/DeleteBaoCaoAjax")]
        [Route("ThongKe/DeleteBaoCaoAjax")]
        public async Task<IActionResult> DeleteBaoCaoAjax(int id)
        {
            var log = await _context.LichSuThaoTac.FindAsync(id);
            if (log == null) return Json(new { success = false, message = "Không tìm thấy biên bản." });

            _context.LichSuThaoTac.Remove(log);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã xóa biên bản báo cáo thành công!" });
        }

        [HttpGet]
        [Route("Admin/ThongKe/ExportCsv")]
        [Route("ThongKe/ExportCsv")]
        public async Task<IActionResult> ExportCsv()
        {
            var nganhs = await _context.NganhHoc
                .AsNoTracking()
                .Include(n => n.DanhSachNguyenVong)
                .Where(n => !n.DaXoa)
                .OrderBy(n => n.MaNganh)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("MaNganh,TenNganh,TenKhoa,ChiTieu,TongNguyenVong,SoNguyenVong1,DiemSan,DiemChuan,SoTrungTuyen,TyLeChoi");

            foreach (var n in nganhs)
            {
                int tongNV = n.DanhSachNguyenVong.Count;
                int soNV1 = n.DanhSachNguyenVong.Count(nv => nv.ThuTuNguyenVong == 1);
                int soTT = n.DanhSachNguyenVong.Count(nv => nv.KetQuaXetTuyen == "Trúng tuyển");
                double tyLeChoi = n.ChiTieu > 0 ? Math.Round((double)tongNV / n.ChiTieu, 2) : 0;
                string diemChuanStr = n.DiemChuan.HasValue ? n.DiemChuan.Value.ToString("0.00") : "Chua cong bo";

                sb.AppendLine($"\"{n.MaNganh}\",\"{n.TenNganh}\",\"{n.TenKhoa ?? "Khoa Dao tao"}\",{n.ChiTieu},{tongNV},{soNV1},{n.DiemSan:0.00},\"{diemChuanStr}\",{soTT},{tyLeChoi}");
            }

            byte[] buffer = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
            return File(buffer, "text/csv; charset=utf-8", $"BaoCao_ThongKe_TuyenSinh_{DateTime.Now:yyyyMMdd}.csv");
        }
    }
}