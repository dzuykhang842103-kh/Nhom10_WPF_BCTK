using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Nhom10_QuanLyLichThiDauBongDa.Models;

namespace Nhom10_QuanLyLichThiDauBongDa.ViewModels
{
    public class TranDauViewModel : BaseViewModel
    {
        #region Collections & Data
        private ObservableCollection<TranDau> _danhSachTranDau;
        public ObservableCollection<TranDau> DanhSachTranDau
        {
            get => _danhSachTranDau;
            set { _danhSachTranDau = value; OnPropertyChanged(); }
        }

        private ObservableCollection<DoiBong> _danhSachDoiBong;
        public ObservableCollection<DoiBong> DanhSachDoiBong
        {
            get => _danhSachDoiBong;
            set { _danhSachDoiBong = value; OnPropertyChanged(); }
        }

        public List<string> DanhSachTrangThai { get; set; } = new List<string>
        {
            "Chưa diễn ra", "Đang thi đấu", "Kết thúc"
        };
        #endregion

        #region Form Binding Properties
        private DoiBong _selectedDoiNha;
        public DoiBong SelectedDoiNha { get => _selectedDoiNha; set { _selectedDoiNha = value; OnPropertyChanged(); } }

        private DoiBong _selectedDoiKhach;
        public DoiBong SelectedDoiKhach { get => _selectedDoiKhach; set { _selectedDoiKhach = value; OnPropertyChanged(); } }

        private DateTime _ngayThiDau = DateTime.Now;
        public DateTime NgayThiDau
        {
            get => _ngayThiDau;
            set
            {
                _ngayThiDau = value; OnPropertyChanged();
                if (_ngayThiDau.Date < DateTime.Now.Date)
                {
                    SelectedTrangThai = "Kết thúc";
                }
            }
        }

        private string _vongDau;
        public string VongDau { get => _vongDau; set { _vongDau = value; OnPropertyChanged(); } }

        private string _banThangNha = "0";
        public string BanThangNha { get => _banThangNha; set { _banThangNha = value; OnPropertyChanged(); } }

        private string _banThangKhach = "0";
        public string BanThangKhach { get => _banThangKhach; set { _banThangKhach = value; OnPropertyChanged(); } }

        private string _selectedTrangThai = "Chưa diễn ra";
        public string SelectedTrangThai { get => _selectedTrangThai; set { _selectedTrangThai = value; OnPropertyChanged(); } }

        private TranDau _selectedTranDau;
        public TranDau SelectedTranDau
        {
            get => _selectedTranDau;
            set
            {
                _selectedTranDau = value;
                OnPropertyChanged();
                if (_selectedTranDau != null)
                {
                    SelectedDoiNha = DanhSachDoiBong.FirstOrDefault(d => d.MaDoiBong == _selectedTranDau.MaDoiNha);
                    SelectedDoiKhach = DanhSachDoiBong.FirstOrDefault(d => d.MaDoiBong == _selectedTranDau.MaDoiKhach);
                    NgayThiDau = _selectedTranDau.NgayThiDau;
                    VongDau = _selectedTranDau.VongDau.ToString();
                    BanThangNha = _selectedTranDau.BanThangNha?.ToString();
                    BanThangKhach = _selectedTranDau.BanThangKhach?.ToString();
                    SelectedTrangThai = _selectedTranDau.TrangThai;
                }
            }
        }
        #endregion

        #region Commands
        public ICommand ThemCommand { get; set; }
        public ICommand SuaCommand { get; set; }
        public ICommand XoaCommand { get; set; }
        public ICommand LamMoiCommand { get; set; }
        public ICommand SinhLichTuDongCommand { get; set; }
        public ICommand ChiTietCommand { get; set; }
        #endregion

        #region Constructor
        public TranDauViewModel()
        {
            LoadData();
            ThemCommand = new RelayCommand(ExecuteThem);
            SuaCommand = new RelayCommand(ExecuteSua);
            XoaCommand = new RelayCommand(ExecuteXoa);
            LamMoiCommand = new RelayCommand(ExecuteLamMoi);
            SinhLichTuDongCommand = new RelayCommand(ExecuteSinhLichTuDong);
            ChiTietCommand = new RelayCommand(ExecuteChiTiet);
        }
        #endregion

        #region Core Methods
        private void LoadData()
        {
            try
            {
                using (var db = new Nhom10_QuanLyBongDaEntities())
                {
                    DanhSachDoiBong = new ObservableCollection<DoiBong>(db.DoiBongs.ToList());
                    DanhSachTranDau = new ObservableCollection<TranDau>(db.TranDaus.ToList());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu trận đấu: " + ex.Message);
            }
        }

        private void ExecuteChiTiet(object obj)
        {
            if (SelectedTranDau == null)
            {
                MessageBox.Show("Vui lòng chọn 1 trận đấu để xem chi tiết!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var window = new Nhom10_QuanLyLichThiDauBongDa.Views.ChiTietTranDauWindow(SelectedTranDau);
            window.ShowDialog();
            LoadData();
        }

        private void ExecuteSinhLichTuDong(object obj)
        {
            var result = MessageBox.Show("Hệ thống sẽ TẠO MỚI một lịch thi đấu vòng tròn 1 lượt cho toàn bộ các đội trong hệ thống. \n\nLưu ý: Bạn nên xóa hết các lịch thi đấu cũ (nếu có) để tránh trùng lặp. Bạn có muốn tiếp tục?", "Xác nhận tạo lịch tự động", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                using (var db = new Nhom10_QuanLyBongDaEntities())
                {
                    var danhSachDoi = db.DoiBongs.ToList();
                    if (danhSachDoi.Count < 2)
                    {
                        MessageBox.Show("Cần ít nhất 2 đội bóng để tạo lịch thi đấu!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    bool isOdd = danhSachDoi.Count % 2 != 0;
                    if (isOdd)
                    {
                        danhSachDoi.Add(new DoiBong { MaDoiBong = -1, TenDoi = "BYE" });
                    }

                    int n = danhSachDoi.Count;
                    int soVong = n - 1;
                    int soTranMoiVong = n / 2;
                    DateTime ngayBatDau = DateTime.Now.Date.AddDays(1);

                    List<int> currentTeams = new List<int>();
                    for (int i = 0; i < n; i++) currentTeams.Add(i);

                    for (int vong = 0; vong < soVong; vong++)
                    {
                        for (int tran = 0; tran < soTranMoiVong; tran++)
                        {
                            int idNha = currentTeams[tran];
                            int idKhach = currentTeams[n - 1 - tran];

                            if (danhSachDoi[idNha].MaDoiBong != -1 && danhSachDoi[idKhach].MaDoiBong != -1)
                            {
                                var tranMoi = new TranDau
                                {
                                    MaDoiNha = danhSachDoi[idNha].MaDoiBong,
                                    MaDoiKhach = danhSachDoi[idKhach].MaDoiBong,
                                    VongDau = vong + 1,
                                    NgayThiDau = ngayBatDau.AddDays(vong * 7).AddHours(14 + (tran * 2)),
                                    BanThangNha = 0,
                                    BanThangKhach = 0,
                                    TrangThai = "Chưa diễn ra"
                                };
                                db.TranDaus.Add(tranMoi);
                            }
                        }

                        int lastTeam = currentTeams[n - 1];
                        currentTeams.RemoveAt(n - 1);
                        currentTeams.Insert(1, lastTeam);
                    }
                    db.SaveChanges();
                }

                MessageBox.Show("Tuyệt vời! Thuật toán đã tự động sinh lịch thi đấu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi thuật toán tạo lịch: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteThem(object obj)
        {
            if (!ValidateInput(out int vongDauInt, out int btNhaInt, out int btKhachInt)) return;

            try
            {
                using (var db = new Nhom10_QuanLyBongDaEntities())
                {
                    var tranDauMoi = new TranDau
                    {
                        MaDoiNha = SelectedDoiNha.MaDoiBong,
                        MaDoiKhach = SelectedDoiKhach.MaDoiBong,
                        NgayThiDau = NgayThiDau,
                        VongDau = vongDauInt,
                        BanThangNha = btNhaInt,
                        BanThangKhach = btKhachInt,
                        TrangThai = SelectedTrangThai
                    };
                    db.TranDaus.Add(tranDauMoi);
                    db.SaveChanges();
                }
                MessageBox.Show("Tạo lịch thi đấu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadData();
                ExecuteLamMoi(null);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi thêm trận đấu: " + ex.Message);
            }
        }

        private void ExecuteSua(object obj)
        {
            if (SelectedTranDau == null)
            {
                MessageBox.Show("Vui lòng chọn trận đấu cần cập nhật!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!ValidateInput(out int vongDauInt, out int btNhaInt, out int btKhachInt)) return;

            try
            {
                using (var db = new Nhom10_QuanLyBongDaEntities())
                {
                    var tranSua = db.TranDaus.Find(SelectedTranDau.MaTranDau);
                    if (tranSua != null)
                    {
                        tranSua.MaDoiNha = SelectedDoiNha.MaDoiBong;
                        tranSua.MaDoiKhach = SelectedDoiKhach.MaDoiBong;
                        tranSua.NgayThiDau = NgayThiDau;
                        tranSua.VongDau = vongDauInt;
                        tranSua.BanThangNha = btNhaInt;
                        tranSua.BanThangKhach = btKhachInt;
                        tranSua.TrangThai = SelectedTrangThai;
                        db.SaveChanges();
                    }
                }
                MessageBox.Show("Cập nhật kết quả thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi cập nhật: " + ex.Message);
            }
        }

        private void ExecuteXoa(object obj)
        {
            if (SelectedTranDau == null)
            {
                MessageBox.Show("Vui lòng chọn trận đấu cần xóa!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa trận đấu này?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var db = new Nhom10_QuanLyBongDaEntities())
                    {
                        var tranXoa = db.TranDaus.Find(SelectedTranDau.MaTranDau);
                        if (tranXoa != null)
                        {
                            db.TranDaus.Remove(tranXoa);
                            db.SaveChanges();
                        }
                    }
                    MessageBox.Show("Đã xóa trận đấu!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadData();
                    ExecuteLamMoi(null);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi xóa: " + ex.Message);
                }
            }
        }
        
        private void ExecuteLamMoi(object obj)
        {
            SelectedDoiNha = null;
            SelectedDoiKhach = null;
            NgayThiDau = DateTime.Now;
            VongDau = string.Empty;
            BanThangNha = "0";
            BanThangKhach = "0";
            SelectedTrangThai = "Chưa diễn ra";
            SelectedTranDau = null;
        }
        #endregion

        #region Validation
        private bool ValidateInput(out int vongDauInt, out int btNhaInt, out int btKhachInt)
        {
            vongDauInt = 0; btNhaInt = 0; btKhachInt = 0;

            if (SelectedDoiNha == null || SelectedDoiKhach == null)
            {
                MessageBox.Show("Vui lòng chọn đầy đủ Đội Nhà và Đội Khách!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (SelectedDoiNha.MaDoiBong == SelectedDoiKhach.MaDoiBong)
            {
                MessageBox.Show("Đội nhà và Đội khách không được trùng nhau!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!int.TryParse(VongDau, out vongDauInt) || vongDauInt <= 0)
            {
                MessageBox.Show("Vòng đấu phải là số nguyên dương (VD: 1, 2, 3...)!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!int.TryParse(BanThangNha, out btNhaInt) || btNhaInt < 0 ||
                !int.TryParse(BanThangKhach, out btKhachInt) || btKhachInt < 0)
            {
                MessageBox.Show("Bàn thắng phải là số tự nhiên (>= 0)!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (NgayThiDau.Date < DateTime.Now.Date && SelectedTrangThai != "Kết thúc")
            {
                MessageBox.Show("Ngày thi đấu đã qua trong quá khứ. Trạng thái trận đấu bắt buộc phải là 'Kết thúc'!", "Lỗi Logic Nghiệp Vụ", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (NgayThiDau.Date > DateTime.Now.Date && SelectedTrangThai == "Kết thúc")
            {
                MessageBox.Show("Ngày thi đấu ở tương lai, trận đấu chưa diễn ra nên không thể chọn trạng thái 'Kết thúc'!", "Lỗi Logic Nghiệp Vụ", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (SelectedTrangThai == "Chưa diễn ra" && (btNhaInt > 0 || btKhachInt > 0))
            {
                MessageBox.Show("Trận đấu 'Chưa diễn ra' thì số bàn thắng bắt buộc phải bằng 0!", "Nghịch Lý Dữ Liệu", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if ((btNhaInt > 0 || btKhachInt > 0) && SelectedTrangThai == "Chưa diễn ra")
            {
                MessageBox.Show("Trận đấu đã phát sinh bàn thắng thì không thể để trạng thái 'Chưa diễn ra'!", "Nghịch Lý Dữ Liệu", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }
        #endregion
    }
}