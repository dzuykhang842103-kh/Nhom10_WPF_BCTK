-- ==============================================================
-- BƯỚC 0: TỰ ĐỘNG TẠO DATABASE NẾU CHƯA TỒN TẠI
-- ==============================================================
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'Nhom10_QuanLyBongDa')
BEGIN
    CREATE DATABASE Nhom10_QuanLyBongDa;
END
GO

-- ==============================================================
-- PROJECT: QUẢN LÝ LỊCH THI ĐẤU BÓNG ĐÁ (NHÓM 10)
-- MÔ TẢ: Xóa dữ liệu cũ, Khởi tạo CSDL, ràng buộc dữ liệu bảo mật
-- ==============================================================
USE Nhom10_QuanLyBongDa;
GO

-- ==============================================================
-- BƯỚC 2: KHỞI TẠO LẠI CÁC BẢNG (CREATE TABLES)
-- ==============================================================

-- 1. Bảng Phân Quyền & Đăng Nhập
CREATE TABLE NguoiDung (
    MaNguoiDung INT IDENTITY(1,1) PRIMARY KEY,
    TenDangNhap VARCHAR(50) UNIQUE NOT NULL,
    MatKhau VARCHAR(255) NOT NULL, -- Sẽ lưu mã băm SHA256
    HoTen NVARCHAR(100) NOT NULL,
    VaiTro VARCHAR(20) CHECK (VaiTro IN ('Admin', 'User')) NOT NULL,
    TrangThai BIT DEFAULT 1 -- 1: Hoạt động, 0: Khóa
);

-- 2. Bảng Danh Mục Đội Bóng
CREATE TABLE DoiBong (
    MaDoiBong INT IDENTITY(1,1) PRIMARY KEY,
    TenDoi NVARCHAR(100) UNIQUE NOT NULL,
    ThanhPho NVARCHAR(100) NOT NULL,
    SanVanDong NVARCHAR(100),
    HuanLuyenVien NVARCHAR(100),
    DuongDanLogo NVARCHAR(500)
);

-- 3. Bảng Danh Mục Cầu Thủ
CREATE TABLE CauThu (
    MaCauThu INT IDENTITY(1,1) PRIMARY KEY,
    HoTen NVARCHAR(100) NOT NULL,
    Tuoi INT CHECK (Tuoi >= 15 AND Tuoi <= 50),
    ViTri NVARCHAR(50),
    SoAo INT CHECK (SoAo > 0 AND SoAo <= 99),
    SoBanThang INT DEFAULT 0,
    HinhAnh NVARCHAR(500),
    MaDoiBong INT,
    CONSTRAINT FK_CauThu_DoiBong FOREIGN KEY (MaDoiBong) 
        REFERENCES DoiBong(MaDoiBong) ON DELETE SET NULL
);

-- 4. Bảng Nghiệp Vụ Trận Đấu (Tương đương Master / Hóa Đơn)
CREATE TABLE TranDau (
    MaTranDau INT IDENTITY(1,1) PRIMARY KEY,
    MaDoiNha INT NOT NULL,
    MaDoiKhach INT NOT NULL,
    NgayThiDau DATETIME NOT NULL,
    VongDau INT NOT NULL,
    BanThangNha INT DEFAULT 0,
    BanThangKhach INT DEFAULT 0,
    TrangThai NVARCHAR(50) DEFAULT N'Chưa diễn ra' CHECK (TrangThai IN (N'Chưa diễn ra', N'Đang thi đấu', N'Kết thúc')),
    CONSTRAINT FK_TranDau_DoiNha FOREIGN KEY (MaDoiNha) REFERENCES DoiBong(MaDoiBong),
    CONSTRAINT FK_TranDau_DoiKhach FOREIGN KEY (MaDoiKhach) REFERENCES DoiBong(MaDoiBong),
    CONSTRAINT CHK_HaiDoiKhacNhau CHECK (MaDoiNha <> MaDoiKhach)
);

-- 5. Bảng Chi Tiết Sự Kiện (Tương đương Detail / Chi Tiết Hóa Đơn)
CREATE TABLE SuKienTranDau (
    MaSuKien INT IDENTITY(1,1) PRIMARY KEY,
    MaTranDau INT NOT NULL,
    MaCauThu INT NOT NULL,
    LoaiSuKien NVARCHAR(50) CHECK (LoaiSuKien IN (N'Ghi bàn', N'Thẻ vàng', N'Thẻ đỏ', N'Phản lưới nhà')),
    PhutThu INT CHECK (PhutThu > 0 AND PhutThu <= 120),
    CONSTRAINT FK_SuKien_TranDau FOREIGN KEY (MaTranDau) 
        REFERENCES TranDau(MaTranDau) ON DELETE CASCADE,
    CONSTRAINT FK_SuKien_CauThu FOREIGN KEY (MaCauThu) 
        REFERENCES CauThu(MaCauThu)
);

-- ==============================================================
-- BƯỚC 3: DỮ LIỆU MẪU ĐỂ TEST PHÂN QUYỀN VÀ HIỂN THỊ
-- ==============================================================

-- Tạo tài khoản Admin (Mật khẩu: 123)
INSERT INTO NguoiDung (TenDangNhap, MatKhau, HoTen, VaiTro, TrangThai) 
VALUES ('admin', 'a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3', N'Quản trị viên', 'Admin', 1); 

-- Tạo tài khoản User (Mật khẩu: 123456)
INSERT INTO NguoiDung (TenDangNhap, MatKhau, HoTen, VaiTro, TrangThai) 
VALUES ('user', '8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92', N'Nguyễn Văn A', 'User', 1);

-- Khởi tạo danh sách Đội Bóng (7 Đội để test thuật toán xoay vòng với số lẻ)
INSERT INTO DoiBong (TenDoi, ThanhPho, SanVanDong, HuanLuyenVien) VALUES (N'Tuyển Anh', N'London', N'Wembley', N'Gareth Southgate');
INSERT INTO DoiBong (TenDoi, ThanhPho, SanVanDong, HuanLuyenVien) VALUES (N'Tuyển Pháp', N'Paris', N'Stade de France', N'Didier Deschamps');
INSERT INTO DoiBong (TenDoi, ThanhPho, SanVanDong, HuanLuyenVien) VALUES (N'Tuyển Đức', N'Berlin', N'Allianz Arena', N'Julian Nagelsmann');
INSERT INTO DoiBong (TenDoi, ThanhPho, SanVanDong, HuanLuyenVien) VALUES (N'Tuyển Tây Ban Nha', N'Madrid', N'Santiago Bernabéu', N'Luis de la Fuente');
INSERT INTO DoiBong (TenDoi, ThanhPho, SanVanDong, HuanLuyenVien) VALUES (N'Tuyển Bồ Đào Nha', N'Lisbon', N'Estádio da Luz', N'Roberto Martínez');
INSERT INTO DoiBong (TenDoi, ThanhPho, SanVanDong, HuanLuyenVien) VALUES (N'Tuyển Argentina', N'Buenos Aires', N'Monumental', N'Lionel Scaloni');
INSERT INTO DoiBong (TenDoi, ThanhPho, SanVanDong, HuanLuyenVien) VALUES (N'Tuyển Brazil', N'Rio de Janeiro', N'Maracanã', N'Dorival Júnior');
GO

-- ==============================================================
-- BƯỚC 4: DỮ LIỆU CẦU THỦ MẪU ĐỂ TEST "VUA PHÁ LƯỚI" VÀ SỰ KIỆN
-- (Mã đội bóng được ngầm định từ 1 đến 7 theo thứ tự Insert ở trên)
-- ==============================================================

-- Đội 1: Tuyển Anh
INSERT INTO CauThu (HoTen, Tuoi, ViTri, SoAo, MaDoiBong) VALUES (N'Harry Kane', 30, N'Tiền đạo', 9, 1);
INSERT INTO CauThu (HoTen, Tuoi, ViTri, SoAo, MaDoiBong) VALUES (N'Jude Bellingham', 20, N'Tiền vệ', 10, 1);
INSERT INTO CauThu (HoTen, Tuoi, ViTri, SoAo, MaDoiBong) VALUES (N'Jordan Pickford', 30, N'Thủ môn', 1, 1);

-- Đội 2: Tuyển Pháp
INSERT INTO CauThu (HoTen, Tuoi, ViTri, SoAo, MaDoiBong) VALUES (N'Kylian Mbappé', 25, N'Tiền đạo', 10, 2);
INSERT INTO CauThu (HoTen, Tuoi, ViTri, SoAo, MaDoiBong) VALUES (N'Antoine Griezmann', 33, N'Tiền vệ', 7, 2);
INSERT INTO CauThu (HoTen, Tuoi, ViTri, SoAo, MaDoiBong) VALUES (N'William Saliba', 23, N'Hậu vệ', 4, 2);

-- Đội 3: Tuyển Đức
INSERT INTO CauThu (HoTen, Tuoi, ViTri, SoAo, MaDoiBong) VALUES (N'Jamal Musiala', 21, N'Tiền vệ', 10, 3);
INSERT INTO CauThu (HoTen, Tuoi, ViTri, SoAo, MaDoiBong) VALUES (N'Toni Kroos', 34, N'Tiền vệ', 8, 3);
INSERT INTO CauThu (HoTen, Tuoi, ViTri, SoAo, MaDoiBong) VALUES (N'Antonio Rüdiger', 31, N'Hậu vệ', 2, 3);

-- Đội 4: Tuyển Tây Ban Nha
INSERT INTO CauThu (HoTen, Tuoi, ViTri, SoAo, MaDoiBong) VALUES (N'Lamine Yamal', 16, N'Tiền đạo', 19, 4);
INSERT INTO CauThu (HoTen, Tuoi, ViTri, SoAo, MaDoiBong) VALUES (N'Rodri', 27, N'Tiền vệ', 16, 4);
INSERT INTO CauThu (HoTen, Tuoi, ViTri, SoAo, MaDoiBong) VALUES (N'Dani Carvajal', 32, N'Hậu vệ', 2, 4);

-- Đội 5: Tuyển Bồ Đào Nha
INSERT INTO CauThu (HoTen, Tuoi, ViTri, SoAo, MaDoiBong) VALUES (N'Cristiano Ronaldo', 39, N'Tiền đạo', 7, 5);
INSERT INTO CauThu (HoTen, Tuoi, ViTri, SoAo, MaDoiBong) VALUES (N'Bruno Fernandes', 29, N'Tiền vệ', 8, 5);
INSERT INTO CauThu (HoTen, Tuoi, ViTri, SoAo, MaDoiBong) VALUES (N'Rúben Dias', 27, N'Hậu vệ', 4, 5);

-- Đội 6: Tuyển Argentina
INSERT INTO CauThu (HoTen, Tuoi, ViTri, SoAo, MaDoiBong) VALUES (N'Lionel Messi', 36, N'Tiền đạo', 10, 6);
INSERT INTO CauThu (HoTen, Tuoi, ViTri, SoAo, MaDoiBong) VALUES (N'Enzo Fernández', 23, N'Tiền vệ', 8, 6);
INSERT INTO CauThu (HoTen, Tuoi, ViTri, SoAo, MaDoiBong) VALUES (N'Emiliano Martínez', 31, N'Thủ môn', 23, 6);

-- Đội 7: Tuyển Brazil
INSERT INTO CauThu (HoTen, Tuoi, ViTri, SoAo, MaDoiBong) VALUES (N'Vinícius Júnior', 23, N'Tiền đạo', 7, 7);
INSERT INTO CauThu (HoTen, Tuoi, ViTri, SoAo, MaDoiBong) VALUES (N'Rodrygo', 23, N'Tiền đạo', 10, 7);
INSERT INTO CauThu (HoTen, Tuoi, ViTri, SoAo, MaDoiBong) VALUES (N'Alisson Becker', 31, N'Thủ môn', 1, 7);
GO