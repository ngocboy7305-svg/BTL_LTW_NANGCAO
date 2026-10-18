using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL_QLTuyenSinh.Models
{
    [Table("DiemThi")]
    public class DiemThi
    {
        [Key]
        public int MaDiemId { get; set; }

        public int MaHoSoId { get; set; }
        [ForeignKey("MaHoSoId")]
        public HoSoTuyenSinh? HoSo { get; set; }

        public int MaMonId { get; set; }
        [ForeignKey("MaMonId")]
        public MonThi? MonThi { get; set; }

        [Column(TypeName = "decimal(4,2)")]
        [Range(0.00, 10.00)]
        public decimal DiemSo { get; set; }

        public DateTime NgayCapNhat { get; set; } = DateTime.UtcNow;
    }
}