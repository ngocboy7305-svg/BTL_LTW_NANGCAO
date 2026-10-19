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
                // 1. Thêm tài khoản Quản trị viên (Admin) và Cán bộ tuyển sinh mẫu
                if (!context.NguoiDung.Any(u => u.VaiTro == "QuanTri"))
                {
                    context.NguoiDung.Add(new NguoiDung
                    {
                        TenDangNhap = "admin",
                        MatKhauMaHoa = "123456@Aa", // Nếu controller dùng hash/plaintext, bạn đổi theo logic AccountController
                        HoTen = "Quản Trị Viên Hệ Thống",
                        Email = "admin@tuyensinh.edu.vn",
                        SoDienThoai = "0988888888",
                        SoCCCD = "001205000001",
                        VaiTro = "QuanTri",
                        TrangThaiHoatDong = true,
                        NgayTao = DateTime.Now
                    });
                }

                if (!context.NguoiDung.Any(u => u.VaiTro == "CanBo"))
                {
                    context.NguoiDung.Add(new NguoiDung
                    {
                        TenDangNhap = "canbo01",
                        MatKhauMaHoa = "123456@Aa",
                        HoTen = "Cán Bộ Tuyển Sinh",
                        Email = "canbo01@tuyensinh.edu.vn",
                        SoDienThoai = "0977777777",
                        SoCCCD = "001205000002",
                        VaiTro = "CanBo",
                        TrangThaiHoatDong = true,
                        NgayTao = DateTime.Now
                    });
                }

                // Thêm một tài khoản Thí sinh mẫu để test quy trình nộp hồ sơ
                if (!context.NguoiDung.Any(u => u.VaiTro == "ThiSinh"))
                {
                    context.NguoiDung.Add(new NguoiDung
                    {
                        TenDangNhap = "thisinh01",
                        MatKhauMaHoa = "123456",
                        HoTen = "Nguyễn Văn Thí Sinh",
                        Email = "thisinh01@gmail.com",
                        SoDienThoai = "0912345678",
                        SoCCCD = "001205000003",
                        VaiTro = "ThiSinh",
                        TrangThaiHoatDong = true,
                        NgayTao = DateTime.Now
                    });
                }

                context.SaveChanges();

                // 2. Thêm danh mục Môn thi mẫu nếu chưa có
                if (!context.MonThi.Any())
                {
                    context.MonThi.AddRange(
                        new MonThi { MaMon = "TOAN", TenMon = "Toán học" },
                        new MonThi { MaMon = "LY", TenMon = "Vật lý" },
                        new MonThi { MaMon = "HOA", TenMon = "Hóa học" },
                        new MonThi { MaMon = "ANH", TenMon = "Tiếng Anh" },
                        new MonThi { MaMon = "VAN", TenMon = "Ngữ văn" },
                        new MonThi { MaMon = "SU", TenMon = "Lịch sử" },
                        new MonThi { MaMon = "DIA", TenMon = "Địa lý" }
                    );
                    context.SaveChanges();
                }

                // 3. Thêm danh mục Ngành học mẫu theo đúng các mockup
                if (!context.NganhHoc.Any())
                {
                    context.NganhHoc.AddRange(
                        new NganhHoc
                        {
                            MaNganh = "7480201",
                            TenNganh = "Công nghệ thông tin",
                            TenKhoa = "Khoa CNTT",
                            ChiTieu = 200,
                            ToHopXetTuyen = "A00, A01, D01, D07",
                            DiemSan = 16.00m,
                            DangMoDangKy = true,
                            DaXoa = false
                        },
                        new NganhHoc
                        {
                            MaNganh = "7480103",
                            TenNganh = "Kỹ thuật phần mềm",
                            TenKhoa = "Khoa CNTT",
                            ChiTieu = 120,
                            ToHopXetTuyen = "A00, A01, D01",
                            DiemSan = 15.50m,
                            DangMoDangKy = true,
                            DaXoa = false
                        },
                        new NganhHoc
                        {
                            MaNganh = "7340101",
                            TenNganh = "Quản trị kinh doanh",
                            TenKhoa = "Khoa Kinh tế",
                            ChiTieu = 150,
                            ToHopXetTuyen = "A00, A01, D01, C00",
                            DiemSan = 15.00m,
                            DangMoDangKy = true,
                            DaXoa = false
                        },
                        new NganhHoc
                        {
                            MaNganh = "7510605",
                            TenNganh = "Logistics & Chuỗi cung ứng",
                            TenKhoa = "Khoa Kinh tế",
                            ChiTieu = 100,
                            ToHopXetTuyen = "A00, A01, D01",
                            DiemSan = 15.00m,
                            DangMoDangKy = true,
                            DaXoa = false
                        },
                        new NganhHoc
                        {
                            MaNganh = "7320101",
                            TenNganh = "Truyền thông đa phương tiện",
                            TenKhoa = "Khoa CNTT",
                            ChiTieu = 80,
                            ToHopXetTuyen = "A01, D01, D14, D15",
                            DiemSan = 14.50m,
                            DangMoDangKy = true,
                            DaXoa = false
                        }
                    );

                    context.SaveChanges();
                }
            }
        }
    }
}