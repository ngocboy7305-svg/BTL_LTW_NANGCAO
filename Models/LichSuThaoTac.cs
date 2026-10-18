using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL_QLTuyenSinh.Models
{
    [Table("LichSuThaoTac")]
    public class LichSuThaoTac
    {
        [Key]
        public int MaLichSuId { get; set; }

        public int? MaHoSoId { get; set; }
        [ForeignKey("MaHoSoId")]
        public HoSoTuyenSinh? HoSo { get; set; }

        [Required, MaxLength(100)]
        public string LoaiThaoTac { get; set; } = string.Empty;

        [Required, MaxLength(1000)]
        public string NoiDungChiTiet { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string NguoiThucHien { get; set; } = string.Empty;

        public DateTime ThoiGianThucHien { get; set; } = DateTime.UtcNow;
    }
}