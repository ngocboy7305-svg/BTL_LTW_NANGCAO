using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL_QLTuyenSinh.Models
{
    [Table("NganhHoc")]
    public class NganhHoc
    {
        [Key]
        public int MaNganhId { get; set; }

        [Required, MaxLength(20)]
        public string MaNganh { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string TenNganh { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? TenKhoa { get; set; }

        [Range(1, 100000)]
        public int ChiTieu { get; set; }

        [Required, MaxLength(50)]
        public string ToHopXetTuyen { get; set; } = string.Empty;

        [Column(TypeName = "decimal(4,2)")]
        public decimal DiemSan { get; set; } = 15.00m;

        [Column(TypeName = "decimal(4,2)")]
        public decimal? DiemChuan { get; set; }

        public bool DangMoDangKy { get; set; } = true;
        public bool DaXoa { get; set; } = false;

        public ICollection<NguyenVong> DanhSachNguyenVong { get; set; } = new List<NguyenVong>();
    }
}