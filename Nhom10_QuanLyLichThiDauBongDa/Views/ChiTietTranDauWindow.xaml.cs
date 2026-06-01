using System.Windows;
using Nhom10_QuanLyLichThiDauBongDa.Models;
using Nhom10_QuanLyLichThiDauBongDa.ViewModels;

namespace Nhom10_QuanLyLichThiDauBongDa.Views
{
    public partial class ChiTietTranDauWindow : Window
    {
        public ChiTietTranDauWindow(TranDau tranDau)
        {
            InitializeComponent();
            this.DataContext = new ChiTietTranDauViewModel(tranDau);
        }
    }
}