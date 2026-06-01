using Nhom10_QuanLyLichThiDauBongDa.Models;

namespace Nhom10_QuanLyLichThiDauBongDa.Helpers
{
    public static class CurrentSession
    {
        public static NguoiDung LoggedInUser { get; set; }

        public static bool IsAdmin => LoggedInUser != null && LoggedInUser.VaiTro == "Admin";
    }
}