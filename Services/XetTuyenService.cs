using BTL_QLTuyenSinh.Data;
using BTL_QLTuyenSinh.Models;
using BTL_QLTuyenSinh.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BTL_QLTuyenSinh.Services
{
    public class XetTuyenService
    {
        private readonly ApplicationDbContext _context;

        public XetTuyenService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<KetQuaXetTuyenNganhDto>> ChayThuatToanXetTuyenAsync(TieuChiXetTuyenDto tieuChi)
        {
            // 1. Lấy danh sách ngành còn mở đăng ký và chưa xóa
            var danhSachNganh = await _context.NganhHocs
                .Where(n => !n.DaXoa && n.DangMoDangKy)
                .ToListAsync();

            // 2. Lấy danh sách hồ sơ kèm điểm và nguyện vọng
            var queryHoSo = _context.HoSoTuyenSinhs
                .Include(h => h.DanhSachNguyenVong)
                .Include(h => h.DanhSachDiem)
                    .ThenInclude(dt => dt.MonThi)
                .AsQueryable();

            if (tieuChi.ChiXetHoSoHopLe)
            {
                queryHoSo = queryHoSo.Where(h => h.TrangThaiHoSo == "Hợp lệ" || h.TrangThaiHoSo == "HỢP LỆ");
            }

            var danhSachHoSo = await queryHoSo.ToListAsync();

            // 3. Tính điểm từng nguyện vọng dựa trên bảng DiemThi và MonThi
            var candidateWishes = new List<WishScoreItem>();

            foreach (var hs in danhSachHoSo)
            {
                decimal diemUT = tieuChi.TuDongCongDiemUT ? hs.DiemCongUuTien : 0.00m;

                var diemMap = hs.DanhSachDiem
                    .Where(d => d.MonThi != null)
                    .ToDictionary(d => d.MonThi.MaMon.Trim().ToUpper(), d => d.DiemSo);

                foreach (var nv in hs.DanhSachNguyenVong.OrderBy(n => n.ThuTuNguyenVong))
                {
                    decimal tongDiemBaMon = TinhDiemToHop(diemMap, nv.KhoiXetTuyen);
                    decimal tongDiemXetTuyen = tongDiemBaMon + diemUT;

                    nv.TongDiemBaMon = tongDiemBaMon;
                    nv.TongDiemXetTuyen = tongDiemXetTuyen;

                    decimal diemToan = diemMap.ContainsKey("TOAN") ? diemMap["TOAN"] : 0m;

                    candidateWishes.Add(new WishScoreItem
                    {
                        HoSoId = hs.MaHoSoId,
                        NguyenVongEntity = nv,
                        MaNganhId = nv.MaNganhId,
                        ThuTuNV = nv.ThuTuNguyenVong,
                        TongDiemXetTuyen = tongDiemXetTuyen,
                        DiemMonToan = diemToan
                    });
                }
            }

            // 4. Thuật toán lọc ảo toàn hệ thống theo điểm từ cao xuống thấp
            var ketQuaTheoNganh = new Dictionary<int, List<WishScoreItem>>();
            foreach (var ng in danhSachNganh)
            {
                ketQuaTheoNganh[ng.MaNganhId] = new List<WishScoreItem>();
            }

            var hoSoDaTrungTuyen = new HashSet<int>();
            bool conThayDoi = true;

            while (conThayDoi)
            {
                conThayDoi = false;

                foreach (var ng in danhSachNganh)
                {
                    var danhSachXepHang = candidateWishes
                        .Where(w => w.MaNganhId == ng.MaNganhId &&
                                    w.TongDiemXetTuyen >= ng.DiemSan &&
                                    (!hoSoDaTrungTuyen.Contains(w.HoSoId) || ketQuaTheoNganh[ng.MaNganhId].Any(x => x.HoSoId == w.HoSoId)))
                        .OrderByDescending(w => w.TongDiemXetTuyen)
                        .ThenByDescending(w => tieuChi.TieuChiPhu == "UuTienMonToan" ? w.DiemMonToan : 0)
                        .ThenBy(w => w.ThuTuNV)
                        .Take(ng.ChiTieu)
                        .ToList();

                    if (!SoSanhHaiDanhSach(ketQuaTheoNganh[ng.MaNganhId], danhSachXepHang))
                    {
                        ketQuaTheoNganh[ng.MaNganhId] = danhSachXepHang;
                        conThayDoi = true;
                    }
                }

                hoSoDaTrungTuyen.Clear();
                foreach (var list in ketQuaTheoNganh.Values)
                {
                    foreach (var item in list)
                    {
                        hoSoDaTrungTuyen.Add(item.HoSoId);
                    }
                }
            }

            // 5. Tổng hợp kết quả trả về giao diện xem trước (chưa cập nhật trực tiếp DB cho đến khi bấm Phê duyệt)
            var result = new List<KetQuaXetTuyenNganhDto>();

            foreach (var ng in danhSachNganh)
            {
                var dsTrungTuyen = ketQuaTheoNganh[ng.MaNganhId];
                decimal diemChuan = dsTrungTuyen.Any() ? dsTrungTuyen.Last().TongDiemXetTuyen : ng.DiemSan;

                result.Add(new KetQuaXetTuyenNganhDto
                {
                    NganhHocId = ng.MaNganhId,
                    MaNganh = ng.MaNganh,
                    TenNganh = ng.TenNganh,
                    ChiTieu = ng.ChiTieu,
                    SoLuongDaTrungTuyen = dsTrungTuyen.Count,
                    DiemChuan = (double)diemChuan,
                    DanhSachMaNguyenVongTrungTuyen = dsTrungTuyen.Select(x => x.NguyenVongEntity.MaNguyenVongId).ToList()
                });
            }

            return result;
        }

        private decimal TinhDiemToHop(Dictionary<string, decimal> diemMap, string khoi)
        {
            decimal d1 = 0, d2 = 0, d3 = 0;
            switch (khoi.Trim().ToUpper())
            {
                case "A00":
                    diemMap.TryGetValue("TOAN", out d1);
                    diemMap.TryGetValue("LY", out d2);
                    diemMap.TryGetValue("HOA", out d3);
                    break;
                case "A01":
                    diemMap.TryGetValue("TOAN", out d1);
                    diemMap.TryGetValue("LY", out d2);
                    diemMap.TryGetValue("ANH", out d3);
                    break;
                case "D01":
                    diemMap.TryGetValue("TOAN", out d1);
                    diemMap.TryGetValue("VAN", out d2);
                    diemMap.TryGetValue("ANH", out d3);
                    break;
                case "B00":
                    diemMap.TryGetValue("TOAN", out d1);
                    diemMap.TryGetValue("HOA", out d2);
                    diemMap.TryGetValue("SINH", out d3);
                    break;
                default:
                    diemMap.TryGetValue("TOAN", out d1);
                    diemMap.TryGetValue("LY", out d2);
                    diemMap.TryGetValue("HOA", out d3);
                    break;
            }
            return d1 + d2 + d3;
        }

        private bool SoSanhHaiDanhSach(List<WishScoreItem> a, List<WishScoreItem> b)
        {
            if (a.Count != b.Count) return false;
            for (int i = 0; i < a.Count; i++)
            {
                if (a[i].NguyenVongEntity.MaNguyenVongId != b[i].NguyenVongEntity.MaNguyenVongId)
                    return false;
            }
            return true;
        }

        private class WishScoreItem
        {
            public int HoSoId { get; set; }
            public NguyenVong NguyenVongEntity { get; set; } = null!;
            public int MaNganhId { get; set; }
            public int ThuTuNV { get; set; }
            public decimal TongDiemXetTuyen { get; set; }
            public decimal DiemMonToan { get; set; }
        }
    }
}