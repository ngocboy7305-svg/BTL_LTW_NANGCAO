using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL_QLTuyenSinh.Models
{
    [Table("HoSoTuyenSinh")]
    public class HoSoTuyenSinh
    {
        [Key]
        public int MaHoSoId { get; set; }

        [Required, MaxLength(20)]
        public string MaHoSo { get; set; } = string.Empty;

        public int MaThiSinh { get; set; }
        [ForeignKey("MaThiSinh")]
        public NguoiDung? ThiSinh { get; set; }

        public int NamTuyenSinh { get; set; } = DateTime.UtcNow.Year;

        [Required, MaxLength(20)]
        public string KhuVucUuTien { get; set; } = "KV3";

        [Required, MaxLength(20)]
        public string DoiTuongUuTien { get; set; } = "Không";

        [Column(TypeName = "decimal(4,2)")]
        public decimal DiemCongUuTien { get; set; } = 0.00m;

        [Required, MaxLength(50)]
        public string TrangThaiHoSo { get; set; } = "Chờ duyệt";

        [MaxLength(500)]
        public string? GhiChuDuyet { get; set; }

        public int? MaCanBoDuyet { get; set; }
        [ForeignKey("MaCanBoDuyet")]
        public NguoiDung? CanBoDuyet { get; set; }

        public DateTime NgayNop { get; set; } = DateTime.UtcNow;
        public DateTime? NgayDuyet { get; set; }

        public ICollection<MinhChungHoSo> DanhSachMinhChung { get; set; } = new List<MinhChungHoSo>();
        public ICollection<NguyenVong> DanhSachNguyenVong { get; set; } = new List<NguyenVong>();
        public ICollection<DiemThi> DanhSachDiem { get; set; } = new List<DiemThi>();
        public ICollection<LichSuThaoTac> DanhSachLichSu { get; set; } = new List<LichSuThaoTac>();
    }
}