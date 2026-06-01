using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Nhom10_QuanLyLichThiDauBongDa.Models;

namespace Nhom10_QuanLyLichThiDauBongDa.ViewModels
{
    public class DoiBongViewModel : BaseViewModel
    {
        #region Collections & Data
        private ObservableCollection<DoiBong> _danhSachDoiBong;
        public ObservableCollection<DoiBong> DanhSachDoiBong
        {
            get => _danhSachDoiBong;
            set { _danhSachDoiBong = value; OnPropertyChanged(); }
        }
        #endregion

        #region Form Binding Properties
        private string _tenDoi;
        public string TenDoi { get => _tenDoi; set { _tenDoi = value; OnPropertyChanged(); } }

        private string _thanhPho;
        public string ThanhPho { get => _thanhPho; set { _thanhPho = value; OnPropertyChanged(); } }

        private string _sanVanDong;
        public string SanVanDong { get => _sanVanDong; set { _sanVanDong = value; OnPropertyChanged(); } }

        private string _huanLuyenVien;
        public string HuanLuyenVien { get => _huanLuyenVien; set { _huanLuyenVien = value; OnPropertyChanged(); } }

        private DoiBong _selectedDoiBong;
        public DoiBong SelectedDoiBong
        {
            get => _selectedDoiBong;
            set
            {
                _selectedDoiBong = value;
                OnPropertyChanged();
                if (_selectedDoiBong != null)
                {
                    TenDoi = _selectedDoiBong.TenDoi;
                    ThanhPho = _selectedDoiBong.ThanhPho;
                    SanVanDong = _selectedDoiBong.SanVanDong;
                    HuanLuyenVien = _selectedDoiBong.HuanLuyenVien;
                }
            }
        }
        #endregion

        #region Commands
        public ICommand ThemCommand { get; set; }
        public ICommand SuaCommand { get; set; }
        public ICommand XoaCommand { get; set; }
        public ICommand LamMoiCommand { get; set; }
        #endregion

        #region Constructor
        public DoiBongViewModel()
        {
            LoadData();
            ThemCommand = new RelayCommand(ExecuteThem);
            SuaCommand = new RelayCommand(ExecuteSua);
            XoaCommand = new RelayCommand(ExecuteXoa);
            LamMoiCommand = new RelayCommand(ExecuteLamMoi);
        }
        #endregion

        #region Methods
        private void LoadData()
        {
            try
            {
                using (var db = new Nhom10_QuanLyBongDaEntities())
                {
                    DanhSachDoiBong = new ObservableCollection<DoiBong>(db.DoiBongs.ToList());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu đội bóng: " + ex.Message);
            }
        }

        private void ExecuteThem(object obj)
        {
            if (!ValidateInput()) return;

            try
            {
                using (var db = new Nhom10_QuanLyBongDaEntities())
                {
                    if (db.DoiBongs.Any(d => d.TenDoi.ToLower() == TenDoi.ToLower()))
                    {
                        MessageBox.Show("Tên đội bóng này đã tồn tại trong hệ thống!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var doiBongMoi = new DoiBong
                    {
                        TenDoi = TenDoi.Trim(),
                        ThanhPho = ThanhPho.Trim(),
                        SanVanDong = SanVanDong?.Trim(),
                        HuanLuyenVien = HuanLuyenVien?.Trim()
                    };
                    db.DoiBongs.Add(doiBongMoi);
                    db.SaveChanges();
                }
                MessageBox.Show("Thêm đội bóng thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadData();
                ExecuteLamMoi(null);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi thêm đội bóng: " + ex.Message);
            }
        }

        private void ExecuteSua(object obj)
        {
            if (SelectedDoiBong == null)
            {
                MessageBox.Show("Vui lòng chọn đội bóng cần sửa!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!ValidateInput()) return;

            try
            {
                using (var db = new Nhom10_QuanLyBongDaEntities())
                {
                    if (db.DoiBongs.Any(d => d.TenDoi.ToLower() == TenDoi.ToLower() && d.MaDoiBong != SelectedDoiBong.MaDoiBong))
                    {
                        MessageBox.Show("Tên đội bóng này đã bị trùng với một đội khác!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var doiBongSua = db.DoiBongs.Find(SelectedDoiBong.MaDoiBong);
                    if (doiBongSua != null)
                    {
                        doiBongSua.TenDoi = TenDoi.Trim();
                        doiBongSua.ThanhPho = ThanhPho.Trim();
                        doiBongSua.SanVanDong = SanVanDong?.Trim();
                        doiBongSua.HuanLuyenVien = HuanLuyenVien?.Trim();
                        db.SaveChanges();
                    }
                }
                MessageBox.Show("Cập nhật thông tin thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi cập nhật: " + ex.Message);
            }
        }

        private void ExecuteXoa(object obj)
        {
            if (SelectedDoiBong == null)
            {
                MessageBox.Show("Vui lòng chọn đội bóng cần xóa!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa đội bóng {SelectedDoiBong.TenDoi}? Cảnh báo: Việc này sẽ ảnh hưởng đến các cầu thủ thuộc đội bóng này.", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var db = new Nhom10_QuanLyBongDaEntities())
                    {
                        var doiBongXoa = db.DoiBongs.Find(SelectedDoiBong.MaDoiBong);
                        if (doiBongXoa != null)
                        {
                            db.DoiBongs.Remove(doiBongXoa);
                            db.SaveChanges();
                        }
                    }
                    MessageBox.Show("Đã xóa đội bóng!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadData();
                    ExecuteLamMoi(null);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi xóa đội bóng (Có thể đội này đã tham gia trận đấu hoặc đang có cầu thủ): " + ex.Message, "Lỗi Ràng Buộc", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExecuteLamMoi(object obj)
        {
            TenDoi = string.Empty;
            ThanhPho = string.Empty;
            SanVanDong = string.Empty;
            HuanLuyenVien = string.Empty;
            SelectedDoiBong = null;
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(TenDoi) || !TenDoi.All(c => char.IsLetter(c) || char.IsWhiteSpace(c)))
            {
                MessageBox.Show("Tên đội bóng không được để trống và chỉ được chứa chữ cái và khoảng trắng!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (string.IsNullOrWhiteSpace(ThanhPho))
            {
                MessageBox.Show("Thành phố không được để trống!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (string.IsNullOrWhiteSpace(SanVanDong))
            {
                MessageBox.Show("Sân vận động không được để trống!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (!string.IsNullOrWhiteSpace(HuanLuyenVien) && !HuanLuyenVien.All(c => char.IsLetter(c) || char.IsWhiteSpace(c)))
            {
                MessageBox.Show("Tên huấn luyện viên chỉ được chứa chữ cái và khoảng trắng!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }
        #endregion
    }
}