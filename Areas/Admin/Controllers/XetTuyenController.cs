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

        public XetTuyenController(XetTuyenService xetTuyenService, ApplicationDbContext context) // Đã đổi ở đây
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

            foreach (var item in results)
            {
                var nganh = await _context.NganhHocs.FindAsync(item.NganhHocId);
                if (nganh != null)
                {
                    nganh.DiemChuan = (decimal)item.DiemChuan; // Cập nhật cột DiemChuan trong bảng NganhHoc
                }
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Đã phê duyệt và công bố bảng điểm chuẩn tuyển sinh!" });
        }
    }
}