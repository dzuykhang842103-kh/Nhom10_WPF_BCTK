using System.Windows;
using System.Windows.Input;
using Nhom10_QuanLyLichThiDauBongDa.Helpers;
using Nhom10_QuanLyLichThiDauBongDa.Models;
using Nhom10_QuanLyLichThiDauBongDa.Views;
using Nhom10_QuanLyLichThiDauBongDa.Views.Controls;

namespace Nhom10_QuanLyLichThiDauBongDa.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        #region Properties
        private object _currentView;
        public object CurrentView
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(); }
        }

        private string _currentUserInfo;
        public string CurrentUserInfo
        {
            get => _currentUserInfo;
            set { _currentUserInfo = value; OnPropertyChanged(); }
        }
        #endregion

        #region Commands
        public ICommand ShowDoiBongCommand { get; set; }
        public ICommand ShowCauThuCommand { get; set; }
        public ICommand ShowLichThiDauCommand { get; set; }
        public ICommand ShowThongKeCommand { get; set; }
        public ICommand LogoutCommand { get; set; }
        #endregion

        #region Constructor
        public MainViewModel()
        {
            // Tự động nhận diện người dùng từ Session
            if (CurrentSession.LoggedInUser != null)
            {
                CurrentUserInfo = "Xin chào, " + CurrentSession.LoggedInUser.HoTen;
            }
            else
            {
                CurrentUserInfo = "Xin chào, Admin";
            }

            // Khởi tạo các lệnh điều hướng
            ShowDoiBongCommand = new RelayCommand(ExecuteShowDoiBong);
            ShowCauThuCommand = new RelayCommand(ExecuteShowCauThu);
            ShowLichThiDauCommand = new RelayCommand(ExecuteShowLichThiDau);
            ShowThongKeCommand = new RelayCommand(ExecuteShowThongKe);
            LogoutCommand = new RelayCommand(ExecuteLogout);
        }
        #endregion

        #region Navigation Methods
        private void ExecuteShowDoiBong(object obj)
        {
            CurrentView = new DoiBongUserControl();
        }

        private void ExecuteShowCauThu(object obj)
        {
            CurrentView = new CauThuUserControl();
        }

        private void ExecuteShowLichThiDau(object obj)
        {
            CurrentView = new TranDauUserControl();
        }

        private void ExecuteShowThongKe(object obj)
        {
            CurrentView = new ThongKeUserControl();
        }
        #endregion

        #region System Methods
        private void ExecuteLogout(object obj)
        {
            var result = MessageBox.Show("Bạn có chắc chắn muốn đăng xuất?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                LoginWindow loginWindow = new LoginWindow();
                loginWindow.Show();

                foreach (Window window in Application.Current.Windows)
                {
                    if (window is MainWindow)
                    {
                        window.Close();
                        break;
                    }
                }
            }
        }
        #endregion
    }
}