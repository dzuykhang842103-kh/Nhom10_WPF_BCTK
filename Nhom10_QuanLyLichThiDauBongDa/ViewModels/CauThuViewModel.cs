using Microsoft.Win32;
using Nhom10_QuanLyLichThiDauBongDa.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace Nhom10_QuanLyLichThiDauBongDa.ViewModels
{
    public class CauThuViewModel : BaseViewModel
    {
        #region Collections & Data
        private ObservableCollection<CauThu> _danhSachCauThu;
        public ObservableCollection<CauThu> DanhSachCauThu
        {
            get => _danhSachCauThu;
            set { _danhSachCauThu = value; OnPropertyChanged(); }
        }

        private ObservableCollection<DoiBong> _danhSachDoiBong;
        public ObservableCollection<DoiBong> DanhSachDoiBong
        {
            get => _danhSachDoiBong;
            set { _danhSachDoiBong = value; OnPropertyChanged(); }
        }

        private ObservableCollection<DoiBong> _danhSachDoiBongFilter;
        public ObservableCollection<DoiBong> DanhSachDoiBongFilter
        {
            get => _danhSachDoiBongFilter;
            set { _danhSachDoiBongFilter = value; OnPropertyChanged(); }
        }

        public List<string> DanhSachViTri { get; set; } = new List<string>
        {
            "Thủ môn", "Hậu vệ", "Tiền vệ", "Tiền đạo"
        };
        #endregion

        #region Form Binding Properties
        private string _hoTen;
        public string HoTen { get => _hoTen; set { _hoTen = value; OnPropertyChanged(); } }

        private string _tuoi;
        public string Tuoi { get => _tuoi; set { _tuoi = value; OnPropertyChanged(); } }

        private string _viTri;
        public string ViTri { get => _viTri; set { _viTri = value; OnPropertyChanged(); } }

        private string _soAo;
        public string SoAo { get => _soAo; set { _soAo = value; OnPropertyChanged(); } }

        private string _hinhAnh;
        public string HinhAnh { get => _hinhAnh; set { _hinhAnh = value; OnPropertyChanged(); } }

        private DoiBong _selectedDoiBong;
        public DoiBong SelectedDoiBong { get => _selectedDoiBong; set { _selectedDoiBong = value; OnPropertyChanged(); } }

        private CauThu _selectedCauThu;
        public CauThu SelectedCauThu
        {
            get => _selectedCauThu;
            set
            {
                _selectedCauThu = value;
                OnPropertyChanged();
                if (_selectedCauThu != null)
                {
                    HoTen = _selectedCauThu.HoTen;
                    Tuoi = _selectedCauThu.Tuoi?.ToString();
                    ViTri = _selectedCauThu.ViTri;
                    SoAo = _selectedCauThu.SoAo?.ToString();
                    SelectedDoiBong = DanhSachDoiBong.FirstOrDefault(d => d.MaDoiBong == _selectedCauThu.MaDoiBong);
                    HinhAnh = _selectedCauThu.HinhAnh;
                }
            }
        }
        #endregion

        #region Search & Filter Properties
        private string _searchKeyword;
        public string SearchKeyword
        {
            get => _searchKeyword;
            set
            {
                _searchKeyword = value;
                OnPropertyChanged();
                CollectionViewSource.GetDefaultView(DanhSachCauThu)?.Refresh();
            }
        }

        private DoiBong _selectedFilterDoiBong;
        public DoiBong SelectedFilterDoiBong
        {
            get => _selectedFilterDoiBong;
            set
            {
                _selectedFilterDoiBong = value;
                OnPropertyChanged();
                CollectionViewSource.GetDefaultView(DanhSachCauThu)?.Refresh();
            }
        }
        #endregion

        #region Commands
        public ICommand ThemCommand { get; set; }
        public ICommand SuaCommand { get; set; }
        public ICommand XoaCommand { get; set; }
        public ICommand LamMoiCommand { get; set; }
        public ICommand ChonAnhCommand { get; set; }
        public ICommand InBaoCaoCommand { get; set; }
        #endregion

        public CauThuViewModel()
        {
            LoadData();
            ThemCommand = new RelayCommand(ExecuteThem);
            SuaCommand = new RelayCommand(ExecuteSua);
            XoaCommand = new RelayCommand(ExecuteXoa);
            LamMoiCommand = new RelayCommand(ExecuteLamMoi);
            ChonAnhCommand = new RelayCommand(ExecuteChonAnh);
            //InBaoCaoCommand = new RelayCommand(ExecuteInBaoCao);
        }

        #region Methods
        private void LoadData()
        {
            try
            {
                using (var db = new Nhom10_QuanLyBongDaEntities())
                {
                    DanhSachDoiBong = new ObservableCollection<DoiBong>(db.DoiBongs.ToList());
                    var listCT = db.CauThus.Include("DoiBong").ToList();
                    DanhSachCauThu = new ObservableCollection<CauThu>(listCT);

                    ICollectionView view = CollectionViewSource.GetDefaultView(DanhSachCauThu);
                    view.Filter = FilterCauThu;

                    var listFilter = db.DoiBongs.ToList();
                    listFilter.Insert(0, new DoiBong { MaDoiBong = 0, TenDoi = "--- Tất cả đội bóng ---" });
                    DanhSachDoiBongFilter = new ObservableCollection<DoiBong>(listFilter);
                    SelectedFilterDoiBong = DanhSachDoiBongFilter.First();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message);
            }
        }

        private bool FilterCauThu(object item)
        {
            if (item is CauThu ct)
            {
                bool matchName = string.IsNullOrWhiteSpace(SearchKeyword) ||
                                 ct.HoTen.ToLower().Contains(SearchKeyword.ToLower());

                bool matchTeam = SelectedFilterDoiBong == null ||
                                 SelectedFilterDoiBong.MaDoiBong == 0 ||
                                 ct.MaDoiBong == SelectedFilterDoiBong.MaDoiBong;

                return matchName && matchTeam;
            }
            return false;
        }

        private void ExecuteChonAnh(object obj)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg|All files (*.*)|*.*";
            dialog.Title = "Chọn ảnh đại diện cầu thủ";

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    string destFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
                    if (!Directory.Exists(destFolder))
                    {
                        Directory.CreateDirectory(destFolder);
                    }

                    string fileName = Path.GetFileName(dialog.FileName);
                    string newFileName = DateTime.Now.ToString("yyyyMMdd_HHmmss_") + fileName;
                    string destFilePath = Path.Combine(destFolder, newFileName);

                    File.Copy(dialog.FileName, destFilePath, true);
                    HinhAnh = destFilePath;
                    if (SelectedCauThu != null)
                    {
                        SelectedCauThu.HinhAnh = destFilePath;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi tải ảnh: " + ex.Message);
                }
            }
        }

        private void ExecuteThem(object obj)
        {
            if (!ValidateInput(out int tuoiInt, out int soAoInt)) return;

            try
            {
                using (var db = new Nhom10_QuanLyBongDaEntities())
                {
                    bool isDuplicate = db.CauThus.Any(c => c.HoTen.ToLower() == HoTen.ToLower() && c.MaDoiBong == SelectedDoiBong.MaDoiBong);

                    if (isDuplicate)
                    {
                        MessageBox.Show("Cầu thủ này đã tồn tại trong đội bóng!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var cauThuMoi = new CauThu
                    {
                        HoTen = HoTen.Trim(),
                        Tuoi = tuoiInt,
                        ViTri = ViTri,
                        SoAo = soAoInt,
                        SoBanThang = 0,
                        MaDoiBong = SelectedDoiBong?.MaDoiBong,
                        HinhAnh = this.HinhAnh
                    };
                    db.CauThus.Add(cauThuMoi);
                    db.SaveChanges();
                }
                MessageBox.Show("Thêm cầu thủ thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadData();
                ExecuteLamMoi(null);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi thêm cầu thủ: " + ex.Message);
            }
        }

        private void ExecuteSua(object obj)
        {
            if (SelectedCauThu == null)
            {
                MessageBox.Show("Vui lòng chọn cầu thủ cần sửa!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!ValidateInput(out int tuoiInt, out int soAoInt)) return;

            try
            {
                using (var db = new Nhom10_QuanLyBongDaEntities())
                {
                    bool isDuplicate = db.CauThus.Any(c => c.HoTen.ToLower() == HoTen.ToLower()
                                                        && c.MaDoiBong == SelectedDoiBong.MaDoiBong
                                                        && c.MaCauThu != SelectedCauThu.MaCauThu);

                    if (isDuplicate)
                    {
                        MessageBox.Show("Cầu thủ này đã tồn tại trong đội bóng!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var cauThuSua = db.CauThus.Find(SelectedCauThu.MaCauThu);
                    if (cauThuSua != null)
                    {
                        cauThuSua.HoTen = HoTen.Trim();
                        cauThuSua.Tuoi = tuoiInt;
                        cauThuSua.ViTri = ViTri;
                        cauThuSua.SoAo = soAoInt;
                        cauThuSua.MaDoiBong = SelectedDoiBong?.MaDoiBong;
                        cauThuSua.HinhAnh = this.HinhAnh;
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
            if (SelectedCauThu == null)
            {
                MessageBox.Show("Vui lòng chọn cầu thủ cần xóa!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var db = new Nhom10_QuanLyBongDaEntities())
                {
                    bool isTeamPlaying = db.TranDaus.Any(t =>
                        (t.MaDoiNha == SelectedCauThu.MaDoiBong || t.MaDoiKhach == SelectedCauThu.MaDoiBong)
                        && t.TrangThai == "Đang thi đấu");

                    if (isTeamPlaying)
                    {
                        MessageBox.Show($"Không thể xóa cầu thủ {SelectedCauThu.HoTen} lúc này!\n\nLý do: Đội bóng chủ quản đang có trận đấu diễn ra trên sân.\nHướng xử lý: Vui lòng đợi trận đấu kết thúc hoặc cập nhật lại trạng thái trận đấu.",
                                        "Thông báo từ hệ thống",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Warning);
                        return; 
                    }

                    var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa cầu thủ {SelectedCauThu.HoTen}?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        var cauThuXoa = db.CauThus.Find(SelectedCauThu.MaCauThu);
                        if (cauThuXoa != null)
                        {
                            db.CauThus.Remove(cauThuXoa);
                            db.SaveChanges();
                        }
                        MessageBox.Show("Đã xóa cầu thủ thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadData();
                        ExecuteLamMoi(null);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Rất tiếc, đã xảy ra sự cố khi thực hiện thao tác: " + ex.Message, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ExecuteLamMoi(object obj)
        {
            HoTen = string.Empty;
            Tuoi = string.Empty;
            ViTri = string.Empty;
            SoAo = string.Empty;
            SelectedDoiBong = null;
            SelectedCauThu = null;
            HinhAnh = null;
        }

        //private void ExecuteInBaoCao(object obj)
        //{
        //    // Lấy thông tin từ Combobox Filter hiện tại
        //    int maDoi = SelectedFilterDoiBong != null ? SelectedFilterDoiBong.MaDoiBong : 0;
        //    string tenDoi = SelectedFilterDoiBong != null ? SelectedFilterDoiBong.TenDoi : "Tất cả các đội";

        //    if (maDoi == 0) tenDoi = "TẤT CẢ CÁC ĐỘI BÓNG"; // Format lại chữ cho đẹp

        //    // Mở form Báo cáo và truyền tham số sang
        //    var reportWin = new Views.BaoCaoWindow(maDoi, tenDoi);
        //    reportWin.ShowDialog();
        //}

        private bool ValidateInput(out int tuoiInt, out int soAoInt)
        {
            tuoiInt = 0;
            soAoInt = 0;

            if (string.IsNullOrWhiteSpace(HoTen))
            {
                MessageBox.Show("Tên cầu thủ không được để trống!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!HoTen.All(c => char.IsLetter(c) || char.IsWhiteSpace(c)))
            {
                MessageBox.Show("Tên cầu thủ chỉ được chứa chữ cái và khoảng trắng!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!int.TryParse(Tuoi, out tuoiInt) || tuoiInt < 15 || tuoiInt > 50)
            {
                MessageBox.Show("Tuổi phải là số nguyên từ 15 đến 50!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!int.TryParse(SoAo, out soAoInt) || soAoInt <= 0 || soAoInt > 99)
            {
                MessageBox.Show("Số áo phải là số nguyên từ 1 đến 99!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (SelectedDoiBong == null)
            {
                MessageBox.Show("Vui lòng chọn Đội bóng chủ quản!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(ViTri))
            {
                MessageBox.Show("Vui lòng chọn Vị trí thi đấu!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }
        #endregion
    }
}