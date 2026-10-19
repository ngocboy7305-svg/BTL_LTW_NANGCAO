using BTL_QLTuyenSinh.Models;
using Microsoft.EntityFrameworkCore;

namespace BTL_QLTuyenSinh.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // ================= CÁC DBSET CHÍNH (SỐ ÍT THEO CODE CỦA NHÓM) =================
        public DbSet<NguoiDung> NguoiDung { get; set; } = null!;
        public DbSet<NganhHoc> NganhHoc { get; set; } = null!;
        public DbSet<MonThi> MonThi { get; set; } = null!;
        public DbSet<HoSoTuyenSinh> HoSoTuyenSinh { get; set; } = null!;
        public DbSet<MinhChungHoSo> MinhChungHoSo { get; set; } = null!;
        public DbSet<NguyenVong> NguyenVong { get; set; } = null!;
        public DbSet<DiemThi> DiemThi { get; set; } = null!;
        public DbSet<ThongBao> ThongBao { get; set; } = null!;
        public DbSet<LichSuThaoTac> LichSuThaoTac { get; set; } = null!;

        // ================= ALIAS SỐ NHIỀU (ĐỂ CHẠY CHỨC NĂNG CỦA BẠN MÀ KHÔNG GÂY LỖI) =================
        public DbSet<NguoiDung> NguoiDungs => NguoiDung;
        public DbSet<NganhHoc> NganhHocs => NganhHoc;
        public DbSet<MonThi> MonThis => MonThi;
        public DbSet<HoSoTuyenSinh> HoSoTuyenSinhs => HoSoTuyenSinh;
        public DbSet<MinhChungHoSo> MinhChungHoSos => MinhChungHoSo;
        public DbSet<NguyenVong> NguyenVongs => NguyenVong;
        public DbSet<DiemThi> DiemThis => DiemThi;
        public DbSet<ThongBao> ThongBaos => ThongBao;
        public DbSet<LichSuThaoTac> LichSuThaoTacs => LichSuThaoTac;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình Index và Unique
            modelBuilder.Entity<NguoiDung>().HasIndex(u => u.TenDangNhap).IsUnique();
            modelBuilder.Entity<NguoiDung>().HasIndex(u => u.Email).IsUnique();
            modelBuilder.Entity<NguoiDung>().HasIndex(u => u.SoCCCD);

            modelBuilder.Entity<NganhHoc>().HasIndex(m => m.MaNganh).IsUnique();
            modelBuilder.Entity<MonThi>().HasIndex(m => m.MaMon).IsUnique();
            modelBuilder.Entity<HoSoTuyenSinh>().HasIndex(h => h.MaHoSo).IsUnique();

            modelBuilder.Entity<NguyenVong>()
                .HasIndex(nv => new { nv.MaHoSoId, nv.ThuTuNguyenVong }).IsUnique();

            modelBuilder.Entity<DiemThi>()
                .HasIndex(d => new { d.MaHoSoId, d.MaMonId }).IsUnique();

            // Cấu hình ràng buộc khóa ngoại chống xung đột Cascade
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

            modelBuilder.Entity<MinhChungHoSo>()
                .HasOne(m => m.HoSo)
                .WithMany(h => h.DanhSachMinhChung)
                .HasForeignKey(m => m.MaHoSoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<NguyenVong>()
                .HasOne(nv => nv.HoSo)
                .WithMany(h => h.DanhSachNguyenVong)
                .HasForeignKey(nv => nv.MaHoSoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DiemThi>()
                .HasOne(d => d.HoSo)
                .WithMany(h => h.DanhSachDiem)
                .HasForeignKey(d => d.MaHoSoId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}