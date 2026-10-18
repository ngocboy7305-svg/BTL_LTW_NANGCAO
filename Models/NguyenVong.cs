using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL_QLTuyenSinh.Models
{
    [Table("NguyenVong")]
    public class NguyenVong
    {
        [Key]
        public int MaNguyenVongId { get; set; }

        public int MaHoSoId { get; set; }
        [ForeignKey("MaHoSoId")]
        public HoSoTuyenSinh? HoSo { get; set; }

        public int MaNganhId { get; set; }
        [ForeignKey("MaNganhId")]
        public NganhHoc? NganhHoc { get; set; }

        [Range(1, 20)]
        public int ThuTuNguyenVong { get; set; }

        [Required, MaxLength(10)]
        public string KhoiXetTuyen { get; set; } = string.Empty;

        [Column(TypeName = "decimal(4,2)")]
        public decimal? TongDiemBaMon { get; set; }

        [Column(TypeName = "decimal(4,2)")]
        public decimal? TongDiemXetTuyen { get; set; }

        [Required, MaxLength(50)]
        public string KetQuaXetTuyen { get; set; } = "Chờ xét tuyển";
    }
}