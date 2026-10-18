using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL_QLTuyenSinh.Models
{
    [Table("MonThi")]
    public class MonThi
    {
        [Key]
        public int MaMonId { get; set; }

        [Required, MaxLength(20)]
        public string MaMon { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string TenMon { get; set; } = string.Empty;

        public ICollection<DiemThi> DanhSachDiemThi { get; set; } = new List<DiemThi>();
    }
}