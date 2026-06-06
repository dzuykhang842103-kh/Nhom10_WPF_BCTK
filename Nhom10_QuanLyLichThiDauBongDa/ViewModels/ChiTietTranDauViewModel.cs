using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Nhom10_QuanLyLichThiDauBongDa.Models;

namespace Nhom10_QuanLyLichThiDauBongDa.ViewModels
{
    public class ChiTietTranDauViewModel : BaseViewModel
    {
        #region Properties & Collections
        private TranDau _tranDauHienTai;
        public TranDau TranDauHienTai
        {
            get => _tranDauHienTai;
            set { _tranDauHienTai = value; OnPropertyChanged(); }
        }

        public ObservableCollection<CauThu> DanhSachCauThu { get; set; }
        private ObservableCollection<SuKienTranDau> _danhSachSuKien;
        public ObservableCollection<SuKienTranDau> DanhSachSuKien
        {
            get => _danhSachSuKien;
            set { _danhSachSuKien = value; OnPropertyChanged(); } 
        }

        public List<string> DanhSachLoaiSuKien { get; set; } = new List<string>
        {
            "Ghi bàn", "Thẻ vàng", "Thẻ đỏ", "Phản lưới nhà"
        };
        #endregion

        #region Form Binding
        private CauThu _selectedCauThu;
        public CauThu SelectedCauThu { get => _selectedCauThu; set { _selectedCauThu = value; OnPropertyChanged(); } }

        private string _selectedLoaiSuKien;
        public string SelectedLoaiSuKien { get => _selectedLoaiSuKien; set { _selectedLoaiSuKien = value; OnPropertyChanged(); } }

        private string _phutThu;
        public string PhutThu { get => _phutThu; set { _phutThu = value; OnPropertyChanged(); } }

        private SuKienTranDau _selectedSuKien;
        public SuKienTranDau SelectedSuKien { get => _selectedSuKien; set { _selectedSuKien = value; OnPropertyChanged(); } }
        #endregion

        #region Commands
        public ICommand ThemSuKienCommand { get; set; }
        public ICommand XoaSuKienCommand { get; set; }
        #endregion

        #region Constructor
        public ChiTietTranDauViewModel(TranDau tranDau)
        {
            TranDauHienTai = tranDau;
            ThemSuKienCommand = new RelayCommand(ExecuteThemSuKien);
            XoaSuKienCommand = new RelayCommand(ExecuteXoaSuKien);
            LoadData();
        }
        #endregion

        #region Methods
        private void LoadData()
        {
            try
            {
                using (var db = new Nhom10_QuanLyBongDaEntities())
                {
                    var listCauThu = db.CauThus.Include("DoiBong")
                        .Where(c => c.MaDoiBong == TranDauHienTai.MaDoiNha || c.MaDoiBong == TranDauHienTai.MaDoiKhach)
                        .ToList();
                    DanhSachCauThu = new ObservableCollection<CauThu>(listCauThu);

                    var listSuKien = db.SuKienTranDaus.Include("CauThu")
                        .Where(sk => sk.MaTranDau == TranDauHienTai.MaTranDau)
                        .OrderBy(sk => sk.PhutThu).ToList();
                    DanhSachSuKien = new ObservableCollection<SuKienTranDau>(listSuKien);

                    TranDauHienTai = db.TranDaus.Include("DoiBong").Include("DoiBong1").FirstOrDefault(t => t.MaTranDau == TranDauHienTai.MaTranDau);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message);
            }
        }

        private void ExecuteThemSuKien(object obj)
        {
            if (SelectedCauThu == null || string.IsNullOrEmpty(SelectedLoaiSuKien) || !int.TryParse(PhutThu, out int phut) || phut <= 0 || phut > 120)
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin. Phút phải từ 1 đến 120!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var db = new Nhom10_QuanLyBongDaEntities())
                {
                    var suKien = new SuKienTranDau
                    {
                        MaTranDau = TranDauHienTai.MaTranDau,
                        MaCauThu = SelectedCauThu.MaCauThu,
                        LoaiSuKien = SelectedLoaiSuKien,
                        PhutThu = phut
                    };
                    db.SuKienTranDaus.Add(suKien);

                    var tranDauDb = db.TranDaus.Find(TranDauHienTai.MaTranDau);
                    var cauThuDb = db.CauThus.Find(SelectedCauThu.MaCauThu);

                    if (SelectedLoaiSuKien == "Ghi bàn")
                    {
                        if (cauThuDb.MaDoiBong == tranDauDb.MaDoiNha) tranDauDb.BanThangNha += 1;
                        else tranDauDb.BanThangKhach += 1;

                        cauThuDb.SoBanThang += 1;
                    }
                    else if (SelectedLoaiSuKien == "Phản lưới nhà")
                    {
                        if (cauThuDb.MaDoiBong == tranDauDb.MaDoiNha) tranDauDb.BanThangKhach += 1;
                        else tranDauDb.BanThangNha += 1;
                    }

                    if (tranDauDb.TrangThai == "Chưa diễn ra") tranDauDb.TrangThai = "Đang thi đấu";

                    db.SaveChanges();
                }
                MessageBox.Show("Cập nhật sự kiện thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                PhutThu = "";
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi thêm sự kiện: " + ex.Message);
            }
        }

        private void ExecuteXoaSuKien(object obj)
        {
            if (SelectedSuKien == null) return;

            var result = MessageBox.Show("Bạn có chắc chắn muốn xóa sự kiện này? Tỉ số và thành tích cầu thủ sẽ được hệ thống tính toán lại.", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                using (var db = new Nhom10_QuanLyBongDaEntities())
                {
                    // Tìm sự kiện cần xóa trong DB
                    var skDb = db.SuKienTranDaus.Find(SelectedSuKien.MaSuKien);
                    if (skDb != null)
                    {
                        // Lấy thông tin trận đấu và cầu thủ liên quan để trừ điểm
                        var tranDauDb = db.TranDaus.Find(skDb.MaTranDau);
                        var cauThuDb = db.CauThus.Find(skDb.MaCauThu);

                        if (skDb.LoaiSuKien == "Ghi bàn")
                        {
                            // Trừ điểm của đội
                            if (cauThuDb.MaDoiBong == tranDauDb.MaDoiNha) tranDauDb.BanThangNha -= 1;
                            else tranDauDb.BanThangKhach -= 1;

                            // Trừ thành tích cá nhân (chỉ trừ nếu > 0 để tránh số âm)
                            if (cauThuDb.SoBanThang > 0) cauThuDb.SoBanThang -= 1;
                        }
                        else if (skDb.LoaiSuKien == "Phản lưới nhà")
                        {
                            // Trừ điểm của đội đối phương đã được hưởng lợi
                            if (cauThuDb.MaDoiBong == tranDauDb.MaDoiNha) tranDauDb.BanThangKhach -= 1;
                            else tranDauDb.BanThangNha -= 1;
                        }

                        db.SuKienTranDaus.Remove(skDb);

                       
                        db.SaveChanges();
                    }
                }
                MessageBox.Show("Đã xóa sự kiện và cập nhật lại tỉ số thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadData(); 
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xóa sự kiện: " + ex.Message, "Lỗi Hệ Thống", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion
    }
}