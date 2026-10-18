using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL_QLTuyenSinh.Models
{
    [Table("MinhChungHoSo")]
    public class MinhChungHoSo
    {
        [Key]
        public int MaMinhChungId { get; set; }

        public int MaHoSoId { get; set; }
        [ForeignKey("MaHoSoId")]
        public HoSoTuyenSinh? HoSo { get; set; }

        [Required, MaxLength(50)]
        public string LoaiMinhChung { get; set; } = string.Empty;

        [Required, MaxLength(255)]
        public string TenTep { get; set; } = string.Empty;

        [Required, MaxLength(500)]
        public string DuongDanTep { get; set; } = string.Empty;

        public int DungLuongKB { get; set; }
        public DateTime NgayTaiLen { get; set; } = DateTime.UtcNow;
    }
}