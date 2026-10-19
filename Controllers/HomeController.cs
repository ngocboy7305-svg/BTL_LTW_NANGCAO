using BTL_QLTuyenSinh.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BTL_QLTuyenSinh.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? tuKhoa, string? toHop)
        {
            var query = _context.NganhHocs
                .Where(n => !n.DaXoa && n.DangMoDangKy)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                tuKhoa = tuKhoa.Trim();
                query = query.Where(n => n.TenNganh.Contains(tuKhoa) || n.MaNganh.Contains(tuKhoa));
            }

            if (!string.IsNullOrWhiteSpace(toHop))
            {
                query = query.Where(n => n.ToHopXetTuyen.Contains(toHop));
            }

            var danhSachNganh = await query
                .OrderBy(n => n.MaNganh)
                .ToListAsync();

            ViewBag.TuKhoa = tuKhoa;
            ViewBag.ToHop = toHop;

            // Thống kê nhanh hiển thị lên dashboard trang chủ
            ViewBag.TongChiTieu = await _context.NganhHocs.Where(n => !n.DaXoa).SumAsync(n => (int?)n.ChiTieu) ?? 0;
            ViewBag.SoNganhDaoTao = await _context.NganhHocs.Where(n => !n.DaXoa).CountAsync();
            ViewBag.SoHoSoDaNop = await _context.HoSoTuyenSinhs.CountAsync();

            return View(danhSachNganh);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
        [Route("DeAnTuyenSinh")]
        [Route("Home/DeAnTuyenSinh")]
        public async Task<IActionResult> DeAnTuyenSinh(string? tuKhoa, string? khoa, string? toHop)
        {
            var query = _context.NganhHocs
                .Where(n => !n.DaXoa)
                .AsQueryable();

            // 1. Lọc theo từ khóa (Mã ngành hoặc Tên ngành)
            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                tuKhoa = tuKhoa.Trim();
                query = query.Where(n => n.TenNganh.Contains(tuKhoa) || n.MaNganh.Contains(tuKhoa));
            }

            // 2. Lọc theo Khoa quản lý
            if (!string.IsNullOrWhiteSpace(khoa) && khoa != "Tất cả")
            {
                query = query.Where(n => n.TenKhoa == khoa);
            }

            // 3. Lọc theo Tổ hợp môn
            if (!string.IsNullOrWhiteSpace(toHop) && toHop != "Tất cả")
            {
                query = query.Where(n => n.ToHopXetTuyen.Contains(toHop));
            }

            var danhSachNganh = await query.OrderBy(n => n.MaNganh).ToListAsync();

            // Danh sách các Khoa duy nhất để đổ vào Dropdown
            ViewBag.DanhSachKhoa = await _context.NganhHocs
                .Where(n => !n.DaXoa && !string.IsNullOrEmpty(n.TenKhoa))
                .Select(n => n.TenKhoa!)
                .Distinct()
                .ToListAsync();

            ViewBag.TuKhoa = tuKhoa ?? "";
            ViewBag.Khoa = khoa ?? "Tất cả";
            ViewBag.ToHop = toHop ?? "Tất cả";

            return View(danhSachNganh);
        }
    }
}