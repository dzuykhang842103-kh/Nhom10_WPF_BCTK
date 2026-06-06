using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Nhom10_QuanLyLichThiDauBongDa.Helpers;

namespace Nhom10_QuanLyLichThiDauBongDa.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public Visibility AdminVisibility => CurrentSession.IsAdmin ? Visibility.Visible : Visibility.Collapsed;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}