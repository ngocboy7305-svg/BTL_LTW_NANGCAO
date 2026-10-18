using BTL_QLTuyenSinh.Models;
using Microsoft.EntityFrameworkCore;

namespace BTL_QLTuyenSinh.Data
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                // Kiểm tra nếu đã có dữ liệu ngành học thì không chèn nữa
                if (context.NganhHoc.Any())
                {
                    return; // Đã có dữ liệu
                }

                // 1. Thêm danh mục Môn thi
                if (!context.MonThi.Any())
                {
                    context.MonThi.AddRange(
                        new MonThi { MaMon = "TOAN", TenMon = "Toán học" },
                        new MonThi { MaMon = "LY", TenMon = "Vật lý" },
                        new MonThi { MaMon = "HOA", TenMon = "Hóa học" },
                        new MonThi { MaMon = "ANH", TenMon = "Tiếng Anh" }
                    );
                    context.SaveChanges();
                }

                // 2. Thêm danh mục Ngành học mẫu
                context.NganhHoc.AddRange(
                    new NganhHoc
                    {
                        MaNganh = "CNTT",
                        TenNganh = "Công nghệ thông tin",
                        TenKhoa = "Khoa Công nghệ thông tin",
                        ChiTieu = 200,
                        ToHopXetTuyen = "A00, A01, D01",
                        DiemSan = 16.00m,
                        DangMoDangKy = true,
                        DaXoa = false
                    },
                    new NganhHoc
                    {
                        MaNganh = "KTPM",
                        TenNganh = "Kỹ thuật phần mềm",
                        TenKhoa = "Khoa Công nghệ thông tin",
                        ChiTieu = 120,
                        ToHopXetTuyen = "A00, A01",
                        DiemSan = 15.50m,
                        DangMoDangKy = true,
                        DaXoa = false
                    },
                    new NganhHoc
                    {
                        MaNganh = "QTKD",
                        TenNganh = "Quản trị kinh doanh",
                        TenKhoa = "Khoa Kinh tế",
                        ChiTieu = 150,
                        ToHopXetTuyen = "A00, D01",
                        DiemSan = 15.00m,
                        DangMoDangKy = true,
                        DaXoa = false
                    }
                );

                context.SaveChanges();
            }
        }
    }
}