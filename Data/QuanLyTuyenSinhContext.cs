using BTL_QLTuyenSinh.Models;
using Microsoft.EntityFrameworkCore;

namespace BTL_QLTuyenSinh.Data
{
    public class QuanLyTuyenSinhContext : DbContext
    {
        public QuanLyTuyenSinhContext(DbContextOptions<QuanLyTuyenSinhContext> options) : base(options) { }

        public DbSet<NguoiDung> NguoiDungs { get; set; } = null!;
        public DbSet<NganhHoc> NganhHocs { get; set; } = null!;
        public DbSet<MonThi> MonThis { get; set; } = null!;
        public DbSet<HoSoTuyenSinh> HoSoTuyenSinhs { get; set; } = null!;
        public DbSet<NguyenVong> NguyenVongs { get; set; } = null!;
        public DbSet<DiemThi> DiemThis { get; set; } = null!;
        public DbSet<MinhChungHoSo> MinhChungHoSos { get; set; } = null!;
        public DbSet<ThongBao> ThongBaos { get; set; } = null!;
        public DbSet<LichSuThaoTac> LichSuThaoTacs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Ràng buộc quan hệ NguoiDung - HoSoTuyenSinh
            modelBuilder.Entity<HoSoTuyenSinh>()
                .HasOne(h => h.ThiSinh)
                .WithMany(u => u.DanhSachHoSoNop)
                .HasForeignKey(h => h.MaThiSinh)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<HoSoTuyenSinh>()
                .HasOne(h => h.CanBoDuyet)
                .WithMany(u => u.DanhSachHoSoDaDuyet)
                .HasForeignKey(h => h.MaCanBoDuyet)
                .OnDelete(DeleteBehavior.Restrict);

            // Ràng buộc Unique Composite
            modelBuilder.Entity<NguyenVong>()
                .HasIndex(nv => new { nv.MaHoSoId, nv.ThuTuNguyenVong })
                .IsUnique();

            modelBuilder.Entity<DiemThi>()
                .HasIndex(dt => new { dt.MaHoSoId, dt.MaMonId })
                .IsUnique();
        }
    }
}