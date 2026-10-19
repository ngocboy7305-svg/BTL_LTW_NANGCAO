namespace BTL_QLTuyenSinh.ViewModels
{
    public class TieuChiXetTuyenDto
    {
        public int DotTuyenSinhId { get; set; } = 1;
        public string PhuongThucLocAo { get; set; } = "DiemTuCaoXuongThap"; // "DiemTuCaoXuongThap" hoặc "UuTienNV1"
        public string TieuChiPhu { get; set; } = "UuTienMonToan"; // "UuTienMonToan" hoặc "ThuTuNguyenVong"
        public bool TuDongCongDiemUT { get; set; } = true;
        public bool ChiXetHoSoHopLe { get; set; } = true;
    }

    public class KetQuaXetTuyenNganhDto
    {
        public int NganhHocId { get; set; }
        public string MaNganh { get; set; } = string.Empty;
        public string TenNganh { get; set; } = string.Empty;
        public int ChiTieu { get; set; }
        public int SoLuongDaTrungTuyen { get; set; }
        public double DiemChuan { get; set; }
    }
}