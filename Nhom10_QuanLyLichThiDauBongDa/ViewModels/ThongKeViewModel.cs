using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Nhom10_QuanLyLichThiDauBongDa.Models;

namespace Nhom10_QuanLyLichThiDauBongDa.ViewModels
{
    #region Helper Classes
    public class BangXepHangItem
    {
        public int STT { get; set; }
        public string TenDoi { get; set; }
        public int SoTran { get; set; }
        public int Thang { get; set; }
        public int Hoa { get; set; }
        public int Thua { get; set; }
        public int BanThang { get; set; }
        public int BanThua { get; set; }
        public int HieuSo => BanThang - BanThua; 
        public int DiemSo { get; set; }
    }

    // Class mới để chứa dữ liệu Vua Phá Lưới
    public class VuaPhaLuoiItem
    {
        public int STT { get; set; }
        public string TenCauThu { get; set; }
        public string TenDoiBong { get; set; }
        public int SoBanThang { get; set; }
    }
    #endregion

    public class ThongKeViewModel : BaseViewModel
    {
        #region Collections & Data
        private ObservableCollection<BangXepHangItem> _danhSachXepHang;
        public ObservableCollection<BangXepHangItem> DanhSachXepHang
        {
            get => _danhSachXepHang;
            set { _danhSachXepHang = value; OnPropertyChanged(); }
        }

        private ObservableCollection<VuaPhaLuoiItem> _danhSachVuaPhaLuoi;
        public ObservableCollection<VuaPhaLuoiItem> DanhSachVuaPhaLuoi
        {
            get => _danhSachVuaPhaLuoi;
            set { _danhSachVuaPhaLuoi = value; OnPropertyChanged(); }
        }
        #endregion

        #region Constructor
        public ThongKeViewModel()
        {
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
                    var listDoiBong = db.DoiBongs.ToList();
                    var listTranDau = db.TranDaus.Where(t => t.TrangThai == "Kết thúc").ToList();

                    var bangXepHang = new List<BangXepHangItem>();

                    foreach (var doi in listDoiBong)
                    {
                        var item = new BangXepHangItem
                        {
                            TenDoi = doi.TenDoi,
                            SoTran = 0, Thang = 0, Hoa = 0, Thua = 0,
                            BanThang = 0, BanThua = 0, DiemSo = 0
                        };

                        foreach (var tran in listTranDau)
                        {
                            if (tran.MaDoiNha == doi.MaDoiBong)
                            {
                                item.SoTran++;
                                item.BanThang += tran.BanThangNha ?? 0;
                                item.BanThua += tran.BanThangKhach ?? 0;

                                if (tran.BanThangNha > tran.BanThangKhach) { item.Thang++; item.DiemSo += 3; }
                                else if (tran.BanThangNha == tran.BanThangKhach) { item.Hoa++; item.DiemSo += 1; }
                                else { item.Thua++; }
                            }
                            else if (tran.MaDoiKhach == doi.MaDoiBong)
                            {
                                item.SoTran++;
                                item.BanThang += tran.BanThangKhach ?? 0;
                                item.BanThua += tran.BanThangNha ?? 0;

                                if (tran.BanThangKhach > tran.BanThangNha) { item.Thang++; item.DiemSo += 3; }
                                else if (tran.BanThangKhach == tran.BanThangNha) { item.Hoa++; item.DiemSo += 1; }
                                else { item.Thua++; }
                            }
                        }
                        bangXepHang.Add(item);
                    }

                    var sortedList = bangXepHang
                        .OrderByDescending(x => x.DiemSo)
                        .ThenByDescending(x => x.HieuSo)
                        .ThenByDescending(x => x.BanThang)
                        .ToList();

                    for (int i = 0; i < sortedList.Count; i++)
                    {
                        sortedList[i].STT = i + 1;
                    }

                    DanhSachXepHang = new ObservableCollection<BangXepHangItem>(sortedList);

                    var topGhiBanDb = db.CauThus.Include("DoiBong")
                                                .Where(c => c.SoBanThang > 0)
                                                .OrderByDescending(c => c.SoBanThang)
                                                .Take(10)
                                                .ToList();

                    var listVuaPhaLuoi = new List<VuaPhaLuoiItem>();
                    for (int i = 0; i < topGhiBanDb.Count; i++)
                    {
                        listVuaPhaLuoi.Add(new VuaPhaLuoiItem
                        {
                            STT = i + 1,
                            TenCauThu = topGhiBanDb[i].HoTen,
                            TenDoiBong = topGhiBanDb[i].DoiBong?.TenDoi,
                            SoBanThang = topGhiBanDb[i].SoBanThang ?? 0
                        });
                    }

                    DanhSachVuaPhaLuoi = new ObservableCollection<VuaPhaLuoiItem>(listVuaPhaLuoi);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tính toán thống kê: " + ex.Message);
            }
        }
        #endregion
    }
}