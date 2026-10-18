using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BTL_QLTuyenSinh.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MonThi",
                columns: table => new
                {
                    MaMonId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaMon = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TenMon = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonThi", x => x.MaMonId);
                });

            migrationBuilder.CreateTable(
                name: "NganhHoc",
                columns: table => new
                {
                    MaNganhId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaNganh = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TenNganh = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    TenKhoa = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    ChiTieu = table.Column<int>(type: "int", nullable: false),
                    ToHopXetTuyen = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DiemSan = table.Column<decimal>(type: "decimal(4,2)", nullable: false),
                    DiemChuan = table.Column<decimal>(type: "decimal(4,2)", nullable: true),
                    DangMoDangKy = table.Column<bool>(type: "bit", nullable: false),
                    DaXoa = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NganhHoc", x => x.MaNganhId);
                });

            migrationBuilder.CreateTable(
                name: "NguoiDung",
                columns: table => new
                {
                    MaNguoiDung = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenDangNhap = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MatKhauMaHoa = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SoDienThoai = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    SoCCCD = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    NgaySinh = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GioiTinh = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    DiaChi = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    VaiTro = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    TrangThaiHoatDong = table.Column<bool>(type: "bit", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NguoiDung", x => x.MaNguoiDung);
                });

            migrationBuilder.CreateTable(
                name: "HoSoTuyenSinh",
                columns: table => new
                {
                    MaHoSoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaHoSo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MaThiSinh = table.Column<int>(type: "int", nullable: false),
                    NamTuyenSinh = table.Column<int>(type: "int", nullable: false),
                    KhuVucUuTien = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DoiTuongUuTien = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DiemCongUuTien = table.Column<decimal>(type: "decimal(4,2)", nullable: false),
                    TrangThaiHoSo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    GhiChuDuyet = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MaCanBoDuyet = table.Column<int>(type: "int", nullable: true),
                    NgayNop = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayDuyet = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoSoTuyenSinh", x => x.MaHoSoId);
                    table.ForeignKey(
                        name: "FK_HoSoTuyenSinh_NguoiDung_MaCanBoDuyet",
                        column: x => x.MaCanBoDuyet,
                        principalTable: "NguoiDung",
                        principalColumn: "MaNguoiDung",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HoSoTuyenSinh_NguoiDung_MaThiSinh",
                        column: x => x.MaThiSinh,
                        principalTable: "NguoiDung",
                        principalColumn: "MaNguoiDung",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ThongBao",
                columns: table => new
                {
                    MaThongBaoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaNguoiNhan = table.Column<int>(type: "int", nullable: false),
                    TieuDe = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    DaXem = table.Column<bool>(type: "bit", nullable: false),
                    NgayGui = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThongBao", x => x.MaThongBaoId);
                    table.ForeignKey(
                        name: "FK_ThongBao_NguoiDung_MaNguoiNhan",
                        column: x => x.MaNguoiNhan,
                        principalTable: "NguoiDung",
                        principalColumn: "MaNguoiDung",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DiemThi",
                columns: table => new
                {
                    MaDiemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaHoSoId = table.Column<int>(type: "int", nullable: false),
                    MaMonId = table.Column<int>(type: "int", nullable: false),
                    DiemSo = table.Column<decimal>(type: "decimal(4,2)", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiemThi", x => x.MaDiemId);
                    table.ForeignKey(
                        name: "FK_DiemThi_HoSoTuyenSinh_MaHoSoId",
                        column: x => x.MaHoSoId,
                        principalTable: "HoSoTuyenSinh",
                        principalColumn: "MaHoSoId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DiemThi_MonThi_MaMonId",
                        column: x => x.MaMonId,
                        principalTable: "MonThi",
                        principalColumn: "MaMonId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LichSuThaoTac",
                columns: table => new
                {
                    MaLichSuId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaHoSoId = table.Column<int>(type: "int", nullable: true),
                    LoaiThaoTac = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NoiDungChiTiet = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    NguoiThucHien = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ThoiGianThucHien = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichSuThaoTac", x => x.MaLichSuId);
                    table.ForeignKey(
                        name: "FK_LichSuThaoTac_HoSoTuyenSinh_MaHoSoId",
                        column: x => x.MaHoSoId,
                        principalTable: "HoSoTuyenSinh",
                        principalColumn: "MaHoSoId");
                });

            migrationBuilder.CreateTable(
                name: "MinhChungHoSo",
                columns: table => new
                {
                    MaMinhChungId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaHoSoId = table.Column<int>(type: "int", nullable: false),
                    LoaiMinhChung = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TenTep = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    DuongDanTep = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DungLuongKB = table.Column<int>(type: "int", nullable: false),
                    NgayTaiLen = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MinhChungHoSo", x => x.MaMinhChungId);
                    table.ForeignKey(
                        name: "FK_MinhChungHoSo_HoSoTuyenSinh_MaHoSoId",
                        column: x => x.MaHoSoId,
                        principalTable: "HoSoTuyenSinh",
                        principalColumn: "MaHoSoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NguyenVong",
                columns: table => new
                {
                    MaNguyenVongId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaHoSoId = table.Column<int>(type: "int", nullable: false),
                    MaNganhId = table.Column<int>(type: "int", nullable: false),
                    ThuTuNguyenVong = table.Column<int>(type: "int", nullable: false),
                    KhoiXetTuyen = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TongDiemBaMon = table.Column<decimal>(type: "decimal(4,2)", nullable: true),
                    TongDiemXetTuyen = table.Column<decimal>(type: "decimal(4,2)", nullable: true),
                    KetQuaXetTuyen = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NguyenVong", x => x.MaNguyenVongId);
                    table.ForeignKey(
                        name: "FK_NguyenVong_HoSoTuyenSinh_MaHoSoId",
                        column: x => x.MaHoSoId,
                        principalTable: "HoSoTuyenSinh",
                        principalColumn: "MaHoSoId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NguyenVong_NganhHoc_MaNganhId",
                        column: x => x.MaNganhId,
                        principalTable: "NganhHoc",
                        principalColumn: "MaNganhId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DiemThi_MaHoSoId_MaMonId",
                table: "DiemThi",
                columns: new[] { "MaHoSoId", "MaMonId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DiemThi_MaMonId",
                table: "DiemThi",
                column: "MaMonId");

            migrationBuilder.CreateIndex(
                name: "IX_HoSoTuyenSinh_MaCanBoDuyet",
                table: "HoSoTuyenSinh",
                column: "MaCanBoDuyet");

            migrationBuilder.CreateIndex(
                name: "IX_HoSoTuyenSinh_MaHoSo",
                table: "HoSoTuyenSinh",
                column: "MaHoSo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HoSoTuyenSinh_MaThiSinh",
                table: "HoSoTuyenSinh",
                column: "MaThiSinh");

            migrationBuilder.CreateIndex(
                name: "IX_LichSuThaoTac_MaHoSoId",
                table: "LichSuThaoTac",
                column: "MaHoSoId");

            migrationBuilder.CreateIndex(
                name: "IX_MinhChungHoSo_MaHoSoId",
                table: "MinhChungHoSo",
                column: "MaHoSoId");

            migrationBuilder.CreateIndex(
                name: "IX_MonThi_MaMon",
                table: "MonThi",
                column: "MaMon",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NganhHoc_MaNganh",
                table: "NganhHoc",
                column: "MaNganh",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NguoiDung_Email",
                table: "NguoiDung",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NguoiDung_SoCCCD",
                table: "NguoiDung",
                column: "SoCCCD");

            migrationBuilder.CreateIndex(
                name: "IX_NguoiDung_TenDangNhap",
                table: "NguoiDung",
                column: "TenDangNhap",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NguyenVong_MaHoSoId_ThuTuNguyenVong",
                table: "NguyenVong",
                columns: new[] { "MaHoSoId", "ThuTuNguyenVong" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NguyenVong_MaNganhId",
                table: "NguyenVong",
                column: "MaNganhId");

            migrationBuilder.CreateIndex(
                name: "IX_ThongBao_MaNguoiNhan",
                table: "ThongBao",
                column: "MaNguoiNhan");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiemThi");

            migrationBuilder.DropTable(
                name: "LichSuThaoTac");

            migrationBuilder.DropTable(
                name: "MinhChungHoSo");

            migrationBuilder.DropTable(
                name: "NguyenVong");

            migrationBuilder.DropTable(
                name: "ThongBao");

            migrationBuilder.DropTable(
                name: "MonThi");

            migrationBuilder.DropTable(
                name: "HoSoTuyenSinh");

            migrationBuilder.DropTable(
                name: "NganhHoc");

            migrationBuilder.DropTable(
                name: "NguoiDung");
        }
    }
}
