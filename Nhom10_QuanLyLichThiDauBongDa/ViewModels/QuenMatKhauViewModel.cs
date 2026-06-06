using Nhom10_QuanLyLichThiDauBongDa.Models;
using Nhom10_QuanLyLichThiDauBongDa.Helpers;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Nhom10_QuanLyLichThiDauBongDa.ViewModels
{
    public class QuenMatKhauViewModel : BaseViewModel
    {
        private string _tenDangNhap;
        public string TenDangNhap
        {
            get => _tenDangNhap;
            set { _tenDangNhap = value; OnPropertyChanged(); }
        }

        public ICommand ThoatCommand { get; set; }
        public ICommand XacNhanCommand { get; set; }

        public QuenMatKhauViewModel()
        {
            ThoatCommand = new RelayCommand<Window>((p) => { return true; }, (p) => { p?.Close(); });
            XacNhanCommand = new RelayCommand<Window>((p) => { return true; }, (p) => { XacNhanReset(p); });
        }

        private void XacNhanReset(Window window)
        {
            if (window == null) return;

            var view = window as Views.QuenMatKhauWindow;
            if (view == null) return;

            string secretKey = view.txtSecretKey.Password;
            string newPass = view.txtNewPass.Password;
            string confirmPass = view.txtConfirmPass.Password;

            if (string.IsNullOrWhiteSpace(TenDangNhap) || string.IsNullOrWhiteSpace(secretKey) ||
                string.IsNullOrWhiteSpace(newPass) || string.IsNullOrWhiteSpace(confirmPass))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (secretKey != "Nhom10_QLBD")
            {
                MessageBox.Show("Mã bảo mật không chính xác. Hành vi đã bị từ chối!", "Lỗi Bảo Mật", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (newPass != confirmPass)
            {
                MessageBox.Show("Mật khẩu xác nhận không khớp!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                using (var db = new Nhom10_QuanLyBongDaEntities())
                {
                    var user = db.NguoiDungs.FirstOrDefault(u => u.TenDangNhap == TenDangNhap);
                    if (user == null)
                    {
                        MessageBox.Show("Tên đăng nhập không tồn tại trong hệ thống!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    user.MatKhau = SecurityHelper.ComputeSha256Hash(newPass);
                    db.SaveChanges();

                    MessageBox.Show("Đã cấp lại mật khẩu thành công! Vui lòng đăng nhập lại.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    window.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối cơ sở dữ liệu: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}