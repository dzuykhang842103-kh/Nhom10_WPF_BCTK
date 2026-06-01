using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Nhom10_QuanLyLichThiDauBongDa.Helpers;
using Nhom10_QuanLyLichThiDauBongDa.Models;
using Nhom10_QuanLyLichThiDauBongDa.Views;

namespace Nhom10_QuanLyLichThiDauBongDa.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        #region Properties
        private string _username;
        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }
        #endregion

        #region Commands
        public ICommand LoginCommand { get; set; }
        public ICommand CloseCommand { get; set; }
        #endregion

        #region Constructor
        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(ExecuteLogin);
            CloseCommand = new RelayCommand(ExecuteClose);
        }
        #endregion

        #region Methods
        private void ExecuteLogin(object parameter)
        {
            var passwordBox = parameter as PasswordBox;
            if (passwordBox == null) return;

            string password = passwordBox.Password;

            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(password))
            {
                ErrorMessage = "Vui lòng nhập đầy đủ Tên đăng nhập và Mật khẩu!";
                return;
            }

            try
            {
                using (var db = new Nhom10_QuanLyBongDaEntities())
                {
                    string hashPassword = SecurityHelper.ComputeSha256Hash(password);

                    var user = db.NguoiDungs.FirstOrDefault(u => u.TenDangNhap == Username && u.MatKhau == hashPassword);

                    if (user != null)
                    {
                        if (user.TrangThai == false)
                        {
                            ErrorMessage = "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ Admin!";
                            return;
                        }

                        ErrorMessage = string.Empty;
                        CurrentSession.LoggedInUser = user;

                        MainWindow mainWindow = new MainWindow();
                        mainWindow.Show();

                        foreach (Window window in Application.Current.Windows)
                        {
                            if (window is LoginWindow)
                            {
                                window.Close();
                                break;
                            }
                        }
                    }
                    else
                    {
                        ErrorMessage = "Tên đăng nhập hoặc mật khẩu không chính xác!";
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Lỗi kết nối cơ sở dữ liệu: " + ex.Message;
            }
        }

        private void ExecuteClose(object parameter)
        {
            Application.Current.Shutdown();
        }
        #endregion
    }
}