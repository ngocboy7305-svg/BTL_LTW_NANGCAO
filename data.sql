-- 1. TẠO CƠ SỞ DỮ LIỆU
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'QuanLyTuyenSinhTrucTuyen')
BEGIN
    CREATE DATABASE QuanLyTuyenSinhTrucTuyen;
END
GO

USE QuanLyTuyenSinhTrucTuyen;
GO

-- 2. TẠO CÁC BẢNG (TABLES)

-- Bảng 1: Người dùng (Thí sinh, Cán bộ, Quản trị viên)
IF OBJECT_ID('NguoiDung', 'U') IS NULL
CREATE TABLE NguoiDung (
    MaNguoiDung INT IDENTITY(1,1) PRIMARY KEY,
    TenDangNhap VARCHAR(50) NOT NULL UNIQUE,
    MatKhauMaHoa VARCHAR(255) NOT NULL,
    HoTen NVARCHAR(100) NOT NULL,
    Email VARCHAR(100) NOT NULL UNIQUE,
    SoDienThoai VARCHAR(15) NULL,
    SoCCCD VARCHAR(20) NULL,
    NgaySinh DATE NULL,
    GioiTinh NVARCHAR(10) NULL,
    DiaChi NVARCHAR(255) NULL,
    VaiTro NVARCHAR(30) NOT NULL DEFAULT N'ThiSinh', -- N'ThiSinh', N'CanBo', N'QuanTri'
    TrangThaiHoatDong BIT NOT NULL DEFAULT 1,
    NgayTao DATETIME NOT NULL DEFAULT GETDATE()
);
GO

-- Bảng 2: Ngành học & Tiêu chí xét tuyển
IF OBJECT_ID('NganhHoc', 'U') IS NULL
CREATE TABLE NganhHoc (
    MaNganhId INT IDENTITY(1,1) PRIMARY KEY,
    MaNganh VARCHAR(20) NOT NULL UNIQUE,
    TenNganh NVARCHAR(150) NOT NULL,
    TenKhoa NVARCHAR(150) NULL,
    ChiTieu INT NOT NULL CHECK (ChiTieu > 0),
    ToHopXetTuyen VARCHAR(50) NOT NULL,
    DiemSan DECIMAL(4,2) NOT NULL DEFAULT 15.00,
    DiemChuan DECIMAL(4,2) NULL,
    DangMoDangKy BIT NOT NULL DEFAULT 1,
    DaXoa BIT NOT NULL DEFAULT 0
);
GO

-- Bảng 3: Môn thi
IF OBJECT_ID('MonThi', 'U') IS NULL
CREATE TABLE MonThi (
    MaMonId INT IDENTITY(1,1) PRIMARY KEY,
    MaMon VARCHAR(20) NOT NULL UNIQUE,
    TenMon NVARCHAR(100) NOT NULL
);
GO

-- Bảng 4: Hồ sơ tuyển sinh
IF OBJECT_ID('HoSoTuyenSinh', 'U') IS NULL
CREATE TABLE HoSoTuyenSinh (
    MaHoSoId INT IDENTITY(1,1) PRIMARY KEY,
    MaHoSo VARCHAR(20) NOT NULL UNIQUE,
    MaThiSinh INT NOT NULL,
    NamTuyenSinh INT NOT NULL DEFAULT 2026,
    KhuVucUuTien NVARCHAR(20) NOT NULL DEFAULT N'KV3',
    DoiTuongUuTien NVARCHAR(20) NOT NULL DEFAULT N'Không',
    DiemCongUuTien DECIMAL(4,2) NOT NULL DEFAULT 0.00,
    TrangThaiHoSo NVARCHAR(50) NOT NULL DEFAULT N'Chờ duyệt',
    GhiChuDuyet NVARCHAR(500) NULL,
    MaCanBoDuyet INT NULL,
    NgayNop DATETIME NOT NULL DEFAULT GETDATE(),
    NgayDuyet DATETIME NULL,
    CONSTRAINT FK_HoSo_ThiSinh FOREIGN KEY (MaThiSinh) REFERENCES NguoiDung(MaNguoiDung),
    CONSTRAINT FK_HoSo_CanBoDuyet FOREIGN KEY (MaCanBoDuyet) REFERENCES NguoiDung(MaNguoiDung)
);
GO

-- Bảng 5: Minh chứng hồ sơ (Ảnh học bạ, CCCD, bằng tốt nghiệp)
IF OBJECT_ID('MinhChungHoSo', 'U') IS NULL
CREATE TABLE MinhChungHoSo (
    MaMinhChungId INT IDENTITY(1,1) PRIMARY KEY,
    MaHoSoId INT NOT NULL,
    LoaiMinhChung NVARCHAR(50) NOT NULL,
    TenTep NVARCHAR(255) NOT NULL,
    DuongDanTep NVARCHAR(500) NOT NULL,
    DungLuongKB INT NOT NULL,
    NgayTaiLen DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_MinhChung_HoSo FOREIGN KEY (MaHoSoId) REFERENCES HoSoTuyenSinh(MaHoSoId) ON DELETE CASCADE
);
GO

-- Bảng 6: Nguyện vọng & Kết quả xét tuyển
IF OBJECT_ID('NguyenVong', 'U') IS NULL
CREATE TABLE NguyenVong (
    MaNguyenVongId INT IDENTITY(1,1) PRIMARY KEY,
    MaHoSoId INT NOT NULL,
    MaNganhId INT NOT NULL,
    ThuTuNguyenVong INT NOT NULL CHECK (ThuTuNguyenVong >= 1),
    KhoiXetTuyen VARCHAR(10) NOT NULL,
    TongDiemBaMon DECIMAL(4,2) NULL,
    TongDiemXetTuyen DECIMAL(4,2) NULL,
    KetQuaXetTuyen NVARCHAR(50) NOT NULL DEFAULT N'Chờ xét tuyển',
    CONSTRAINT FK_NguyenVong_HoSo FOREIGN KEY (MaHoSoId) REFERENCES HoSoTuyenSinh(MaHoSoId) ON DELETE CASCADE,
    CONSTRAINT FK_NguyenVong_Nganh FOREIGN KEY (MaNganhId) REFERENCES NganhHoc(MaNganhId),
    CONSTRAINT UQ_HoSo_ThuTuNguyenVong UNIQUE (MaHoSoId, ThuTuNguyenVong)
);
GO

-- Bảng 7: Điểm thi chi tiết từng môn
IF OBJECT_ID('DiemThi', 'U') IS NULL
CREATE TABLE DiemThi (
    MaDiemId INT IDENTITY(1,1) PRIMARY KEY,
    MaHoSoId INT NOT NULL,
    MaMonId INT NOT NULL,
    DiemSo DECIMAL(4,2) NOT NULL CHECK (DiemSo >= 0.00 AND DiemSo <= 10.00),
    NgayCapNhat DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_DiemThi_HoSo FOREIGN KEY (MaHoSoId) REFERENCES HoSoTuyenSinh(MaHoSoId) ON DELETE CASCADE,
    CONSTRAINT FK_DiemThi_MonThi FOREIGN KEY (MaMonId) REFERENCES MonThi(MaMonId),
    CONSTRAINT UQ_HoSo_MonThi UNIQUE (MaHoSoId, MaMonId)
);
GO

-- Bảng 8: Thông báo gửi người dùng
IF OBJECT_ID('ThongBao', 'U') IS NULL
CREATE TABLE ThongBao (
    MaThongBaoId INT IDENTITY(1,1) PRIMARY KEY,
    MaNguoiNhan INT NOT NULL,
    TieuDe NVARCHAR(200) NOT NULL,
    NoiDung NVARCHAR(1000) NOT NULL,
    DaXem BIT NOT NULL DEFAULT 0,
    NgayGui DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_ThongBao_NguoiDung FOREIGN KEY (MaNguoiNhan) REFERENCES NguoiDung(MaNguoiDung) ON DELETE CASCADE
);
GO

-- Bảng 9: Lịch sử thao tác (Audit log)
IF OBJECT_ID('LichSuThaoTac', 'U') IS NULL
CREATE TABLE LichSuThaoTac (
    MaLichSuId INT IDENTITY(1,1) PRIMARY KEY,
    MaHoSoId INT NULL,
    LoaiThaoTac NVARCHAR(100) NOT NULL,
    NoiDungChiTiet NVARCHAR(1000) NOT NULL,
    NguoiThucHien VARCHAR(50) NOT NULL,
    ThoiGianThucHien DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_LichSu_HoSo FOREIGN KEY (MaHoSoId) REFERENCES HoSoTuyenSinh(MaHoSoId)
);
GO

-- 3. TẠO CÁC CHỈ MỤC (INDEX) TỐI ƯU HIỆU NĂNG
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_NguoiDung_SoCCCD')
    CREATE INDEX IX_NguoiDung_SoCCCD ON NguoiDung(SoCCCD);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_HoSo_MaHoSo')
    CREATE INDEX IX_HoSo_MaHoSo ON HoSoTuyenSinh(MaHoSo);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_NganhHoc_MaNganh')
    CREATE INDEX IX_NganhHoc_MaNganh ON NganhHoc(MaNganh);
GO

-- 4. CHÈN DỮ LIỆU MẪU (SEED DATA ĐỂ TEST THỬ)

-- Dữ liệu Môn thi
INSERT INTO MonThi (MaMon, TenMon) VALUES
('TOAN', N'Toán học'),
('LY', N'Vật lý'),
('HOA', N'Hóa học'),
('ANH', N'Tiếng Anh');

-- Dữ liệu Ngành học
INSERT INTO NganhHoc (MaNganh, TenNganh, TenKhoa, ChiTieu, ToHopXetTuyen, DiemSan, DangMoDangKy) VALUES
('CNTT', N'Công nghệ thông tin', N'Khoa CNTT', 200, 'A00,A01,D01', 16.00, 1),
('KTPM', N'Kỹ thuật phần mềm', N'Khoa CNTT', 120, 'A00,A01', 15.50, 1),
('QTKD', N'Quản trị kinh doanh', N'Khoa Kinh tế', 150, 'A00,D01', 15.00, 1);

-- Dữ liệu Người dùng (1 Cán bộ, 1 Thí sinh)
INSERT INTO NguoiDung (TenDangNhap, MatKhauMaHoa, HoTen, Email, SoDienThoai, SoCCCD, NgaySinh, GioiTinh, DiaChi, VaiTro) VALUES
('canbo_tuyensinh', 'hash_cb_123456', N'Trần Thị Cán Bộ', 'canbo@tuyensinh.edu.vn', '0912345678', NULL, NULL, NULL, NULL, N'CanBo'),
('thisinh2026', 'hash_ts_123456', N'Nguyễn Văn Thí Sinh', 'thisinh@gmail.com', '0987654321', '001202009876', '2006-05-12', N'Nam', N'Hà Nội', N'ThiSinh');

-- Dữ liệu Hồ sơ tuyển sinh mẫu
INSERT INTO HoSoTuyenSinh (MaHoSo, MaThiSinh, NamTuyenSinh, KhuVucUuTien, DoiTuongUuTien, DiemCongUuTien, TrangThaiHoSo) VALUES
('HS20260001', 2, 2026, N'KV2', N'Không', 0.25, N'Chờ duyệt');

-- Dữ liệu Điểm thi (Toán, Lý, Hóa)
INSERT INTO DiemThi (MaHoSoId, MaMonId, DiemSo) VALUES
(1, 1, 8.60),
(1, 2, 8.20),
(1, 3, 7.80);

-- Dữ liệu Nguyện vọng xét tuyển
INSERT INTO NguyenVong (MaHoSoId, MaNganhId, ThuTuNguyenVong, KhoiXetTuyen, TongDiemBaMon, TongDiemXetTuyen, KetQuaXetTuyen) VALUES
(1, 1, 1, 'A00', 24.60, 24.85, N'Chờ xét tuyển'),
(1, 2, 2, 'A00', 24.60, 24.85, N'Chờ xét tuyển');

-- Dữ liệu Minh chứng
INSERT INTO MinhChungHoSo (MaHoSoId, LoaiMinhChung, TenTep, DuongDanTep, DungLuongKB) VALUES
(1, N'CCCD', N'cccd_mat_truoc_sau.pdf', N'/uploads/minhchung/cccd_mat_truoc_sau.pdf', 1024);
GO
-- Hash chuẩn BCrypt của mật khẩu: 123456@Aa
UPDATE NguoiDung 
SET MatKhauMaHoa = '$2a$11$9qY1H5Uu27y5lS.k2w1zCeN9L0sF2zH/rG6XQ1j2g8W0V3k7B0s3K'
WHERE VaiTro IN ('QuanTri', 'CanBo');
USE QuanLyTuyenSinhTrucTuyen;
GO

SELECT MaNguoiDung, TenDangNhap, SoCCCD, HoTen, VaiTro, TrangThaiHoatDong, MatKhauMaHoa 
FROM NguoiDung;
GO

USE QuanLyTuyenSinhTrucTuyen;
GO

-- 1. Nếu đã có tài khoản admin, cập nhật lại mật khẩu là 123456 và bật TrangThaiHoatDong = 1
UPDATE NguoiDung
SET MatKhauMaHoa = '$2a$11$eE61K7Qj4Ww73q9uL8qCSuqj8G3r2H7kO0E8QpT1gN5z9nC9qU8q2',
    TrangThaiHoatDong = 1
WHERE TenDangNhap = 'admin';

-- 2. Nếu chưa hề có tài khoản admin nào thì chèn mới vào luôn
IF NOT EXISTS (SELECT 1 FROM NguoiDung WHERE TenDangNhap = 'admin')
BEGIN
    INSERT INTO NguoiDung (TenDangNhap, MatKhauMaHoa, HoTen, Email, SoDienThoai, SoCCCD, VaiTro, TrangThaiHoatDong, NgayTao)
    VALUES ('admin', '$2a$11$eE61K7Qj4Ww73q9uL8qCSuqj8G3r2H7kO0E8QpT1gN5z9nC9qU8q2', N'Quản Trị Viên', 'admin@tuyensinh.edu.vn', '0988888888', '001205000001', 'QuanTri', 1, GETDATE());
END
GO

USE QuanLyTuyenSinhTrucTuyen;
GO

-- 1. Đảm bảo độ dài cột MatKhauMaHoa đủ chứa mã hash (tối thiểu 100 hoặc 255)
ALTER TABLE NguoiDung ALTER COLUMN MatKhauMaHoa NVARCHAR(255) NOT NULL;
GO

-- 2. Tạm thời đặt mật khẩu trực tiếp thành chuỗi '123456' để kiểm tra cơ chế fallback
UPDATE NguoiDung
SET MatKhauMaHoa = '123456',
    TrangThaiHoatDong = 1
WHERE TenDangNhap IN ('admin', 'canbo_tuyensinh');
GO