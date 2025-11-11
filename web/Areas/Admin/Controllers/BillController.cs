using Nhom3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using System.Data.Entity;

namespace Nhom3.Areas.Admin.Controllers
{
    public class BillController : BaseController
    {
        // GET: Admin/Bill
        Nhom3DB db = new Nhom3DB();
        [HttpGet]
        public ActionResult Index(DateTime? searchString, int? status, int page = 1, int pageSize = 10)
        {
            List<HoaDon> hoaDons = db.HoaDons.Include("TaiKhoanNguoiDung").Select(p => p).ToList();
            if (status != null)
            {
                hoaDons = hoaDons.Where(x => x.TrangThai == status).ToList();
                ViewBag.Status = status;
            }
            if (searchString != null)
            {
                ViewBag.searchString = searchString.Value.ToString("yyyy-MM-dd");
                string search = searchString.Value.ToString("dd/MM/yyyy");
                hoaDons = hoaDons.Where(hd => hd.NgayDat.ToString().Contains(search)).ToList();
            }
            return View(hoaDons.OrderBy(hd => hd.NgayDat).ToPagedList(page, pageSize));
        }

        [HttpPost]
        public JsonResult Index(int id)
        {
            try
            {
                var hoaDon = db.HoaDons
                    .Include("TaiKhoanNguoiDung")
                    .FirstOrDefault(h => h.MaHD == id);

                if (hoaDon == null)
                {
                    return Json(new { error = "Không tìm thấy hóa đơn" }, JsonRequestBehavior.AllowGet);
                }

                var chiTietHD = db.ChiTietHoaDons
                    .Include("SanPhamChiTiet")
                    .Include("SanPhamChiTiet.KichCo")
                    .Include("SanPhamChiTiet.SanPham")
                    .Where(c => c.MaHD == id)
                    .ToList();

                var result = new
                {
                    hoadon = new
                    {
                        hoaDon.MaHD,
                        hoaDon.HoTenNguoiNhan,
                        hoaDon.SoDienThoaiNhan,
                        hoaDon.DiaChiNhan,
                        hoaDon.TrangThai,
                        hoaDon.NgayDat,
                        hoaDon.NgaySua,
                        hoaDon.NguoiSua,
                        hoaDon.GhiChu,
                        TaiKhoanNguoiDung = new { hoaDon.TaiKhoanNguoiDung.HoTen }
                    },
                    cthd = chiTietHD.Select(c => new
                    {
                        c.GiaMua,
                        c.SoLuongMua,
                        SanPhamChiTiet = new
                        {
                            KichCo = new { c.SanPhamChiTiet.KichCo.TenKichCo }
                        }
                    }),
                    sp = chiTietHD.Select(c => new
                    {
                        c.SanPhamChiTiet.SanPham.TenSP,
                        c.SanPhamChiTiet.SanPham.HinhAnh
                    })
                };

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = "Lỗi: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult ChangeStatus(int mahd, int stt)
        {
            try
            {
                TaiKhoanQuanTri tk = (TaiKhoanQuanTri)Session[Nhom3.Session.ConstaintUser.ADMIN_SESSION];
                HoaDon hd = db.HoaDons.FirstOrDefault(x => x.MaHD == mahd);

                if (hd == null)
                {
                    return Json(new { status = false, message = "Không tìm thấy hóa đơn" }, JsonRequestBehavior.AllowGet);
                }
                
                if (stt == 0 && (hd.TrangThai == 3 || hd.TrangThai == 2))
                {
                    var chiTietHDs = db.ChiTietHoaDons
                        .Include("SanPhamChiTiet")
                        .Where(x => x.MaHD == mahd)
                        .ToList();

                    foreach (var cthd in chiTietHDs)
                    {
                        var spct = db.SanPhamChiTiets.FirstOrDefault(x => x.IDCTSP == cthd.IDCTSP);
                        if (spct != null)
                        {
                            
                            spct.SoLuong += cthd.SoLuongMua;
                        }
                    }
                }
                
                if (stt == 3 && hd.TrangThai != 3)
                {
                    var chiTietHDs = db.ChiTietHoaDons
                        .Include("SanPhamChiTiet")
                        .Where(x => x.MaHD == mahd)
                        .ToList();

                    foreach (var cthd in chiTietHDs)
                    {
                        var spct = db.SanPhamChiTiets.FirstOrDefault(x => x.IDCTSP == cthd.IDCTSP);
                        if (spct != null)
                        {
                            
                            if (spct.SoLuong >= cthd.SoLuongMua)
                            {
                                spct.SoLuong -= cthd.SoLuongMua;
                            }
                            else
                            {
                                
                                spct.SoLuong = 0;
                                
                            }
                        }
                    }
                }

                // Cập nhật trạng thái hóa đơn
                hd.TrangThai = stt;
                hd.NguoiSua = tk.HoTen;
                hd.NgaySua = DateTime.Now;
                db.SaveChanges();
                return Json(new { status = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = "Lỗi: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}