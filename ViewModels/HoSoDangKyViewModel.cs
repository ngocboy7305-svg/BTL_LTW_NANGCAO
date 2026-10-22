using System.ComponentModel.DataAnnotations;

namespace BTL_QLTuyenSinh.ViewModels
{
    public class NguyenVongNhapViewModel
    {
        public int? MaNganhId { get; set; }

        public string? KhoiXetTuyen { get; set; }
    }

    public class HoSoDangKyViewModel
    {
        public int? MaHoSoId { get; set; }

        [Required]
        public string KhuVucUuTien { get; set; } = "KV3";

        [Required]
        public string DoiTuongUuTien { get; set; } = "Không";

        public List<NguyenVongNhapViewModel> NguyenVongs
        { get; set; } = new()
            {
                new(), new(), new()
            };
    }
}