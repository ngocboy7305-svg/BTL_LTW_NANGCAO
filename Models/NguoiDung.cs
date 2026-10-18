using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL_QLTuyenSinh.Models
{
    [Table("NguoiDung")]
    public class NguoiDung
    {
        [Key]
        public int MaNguoiDung { get; set; }

        [Required, MaxLength(50)]
        public string TenDangNhap { get; set; } = string.Empty;

        [Required, MaxLength(255)]
        public string MatKhauMaHoa { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string HoTen { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(15)]
        public string? SoDienThoai { get; set; }

        [MaxLength(20)]
        public string? SoCCCD { get; set; }

        [DataType(DataType.Date)]
        public DateTime? NgaySinh { get; set; }

        [MaxLength(10)]
        public string? GioiTinh { get; set; }

        [MaxLength(255)]
        public string? DiaChi { get; set; }

        [Required, MaxLength(30)]
        public string VaiTro { get; set; } = "ThiSinh";

        public bool TrangThaiHoatDong { get; set; } = true;

        public DateTime NgayTao { get; set; } = DateTime.UtcNow;

        [InverseProperty("ThiSinh")]
        public ICollection<HoSoTuyenSinh> DanhSachHoSoNop { get; set; } = new List<HoSoTuyenSinh>();

        [InverseProperty("CanBoDuyet")]
        public ICollection<HoSoTuyenSinh> DanhSachHoSoDaDuyet { get; set; } = new List<HoSoTuyenSinh>();

        public ICollection<ThongBao> DanhSachThongBao { get; set; } = new List<ThongBao>();
    }
}