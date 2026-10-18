using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL_QLTuyenSinh.Models
{
    [Table("ThongBao")]
    public class ThongBao
    {
        [Key]
        public int MaThongBaoId { get; set; }

        public int MaNguoiNhan { get; set; }
        [ForeignKey("MaNguoiNhan")]
        public NguoiDung? NguoiNhan { get; set; }

        [Required, MaxLength(200)]
        public string TieuDe { get; set; } = string.Empty;

        [Required, MaxLength(1000)]
        public string NoiDung { get; set; } = string.Empty;

        public bool DaXem { get; set; } = false;
        public DateTime NgayGui { get; set; } = DateTime.UtcNow;
    }
}