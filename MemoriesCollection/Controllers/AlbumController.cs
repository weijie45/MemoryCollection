using Dapper;
using Dapper.Contrib.Extensions;
using System.Linq;
using System.Web.Mvc;
using MemoriesCollection.Function.Common;
using MemoriesCollection.Function.Extensions;
using MemoriesCollection.Models;
using MemoriesCollection.ViewModels;
using Westwind.Web.Mvc;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;
using System.IO;
using System.IO.Compression;

namespace MemoriesCollection.Controllers
{
    public class AlbumController : BaseController
    {
        public string RtnMsg = "";
        // GET: Album
        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult Index()
        {
            PageTableViewModel pv = new PageTableViewModel();
            Sql = "SELECT * FROM Album ORDER BY ModifyDateTime Desc ";
            pv.AlbumList = db.Query<Album>(Sql).ToList();

            return View(pv);
        }

        /// <summary>
        /// 儲存相簿
        /// </summary>
        /// <param name="vt"></param>
        /// <returns></returns>
        public ActionResult Save(VtTags vt)
        {
            string[] rtn = new string[] { "", "" };
            if (vt.Error) {
                return PageSettion.VarTagsError(vt.ErrorMsg);
            }
            var tags = vt.Tags;
            PageTableViewModel pv = new PageTableViewModel();
            var name = Key.Dict(ref tags, "albumName");
            var desc = Key.Dict(ref tags, "albumDesc");
            var passwd = Key.Dict(ref tags, "passwd");

            Album a = new Album();
            if (name != "") {

                a.AlbumName = name;
                a.AlbumDesc = desc;
                a.PassWord = passwd;
                a.ImgNo = "";
                a.CreateDateTime = now;
                a.ModifyDateTime = now;

                db.Insert(a);
                rtn[1] = "新增成功 !";
            } else {
                rtn[0] = AppConfig.ParamError;
            }

            return new JsonNetResult(rtn);
        }

        /// <summary>
        /// 刪除相簿
        /// </summary>
        /// <param name="vt"></param>
        /// <returns></returns>
        public ActionResult DelAlbum(VtTags vt)
        {
            string[] rtn = new string[] { "", "" };
            if (vt.Error) {
                return PageSettion.VarTagsError(vt.ErrorMsg);
            }
            var tags = vt.Tags;
            var albumNo = Key.Dict(ref tags, "AlbumNo");
            if (albumNo != "") {
                db.Delete(new Album { AlbumNo = albumNo.FixInt() });
                rtn[1] = "刪除成功 !";
            } else {
                rtn[0] = AppConfig.NoData;
            }

            return new JsonNetResult(rtn);
        }

        /// <summary>
        /// 相簿明細
        /// </summary>
        /// <param name="vt"></param>
        /// <returns></returns>
        public ActionResult Detail(VtTags vt)
        {
            PageTableViewModel pv = new PageTableViewModel();
            var albumNo = Key.Decrypt(Request.QueryString["k"].FixReq());
            Sql = $" SELECT * FROM Album WHERE AlbumNo = {albumNo}  ";
            var albumInfo = db.Query<Album>(Sql).FirstOrDefault();
            pv.Album = albumInfo;

            return View("Detail", pv);
        }

        /// <summary>
        /// 批次讀取相片
        /// </summary>
        /// <param name="vt"></param>
        /// <returns></returns>
        public ActionResult Photo(VtTags vt)
        {
            string[] rtn = new string[] { "", "", "" };
            if (vt.Error) {
                return PageSettion.VarTagsError(vt.ErrorMsg);
            }
            var tags = vt.Tags;
            var type = Key.Dict(ref tags, "Type");
            var albumNo = Key.Dict(ref tags, "AlbumNo");
            var sPic = Key.Dict(ref tags, "SPic").FixInt();
            string cond = "";

            if (albumNo == "") {
                rtn[0] = "相簿名稱錯誤 !";
                return new JsonNetResult(rtn);
            }

            PageTableViewModel pv = new PageTableViewModel();
            List<Photo> img = new List<Photo>();
            Sql = $" SELECT TOP {PhotoLimit} *  FROM Photo ORDER BY ModifyDateTime Desc";
            ViewBag.IsData = db.Query<Photo>(Sql).ToList().Count() > 0;

            Sql = $" SELECT AlbumNo, AlbumName, AlbumDesc, ImgNo FROM Album WHERE AlbumNo = '{albumNo}'  ";
            var albumInfo = db.Query<Album>(Sql).FirstOrDefault();
            if (albumInfo != null) {
                switch (type) {
                    case "tab_1":
                        cond = "";
                        break;
                    case "tab_2": // 相簿照片
                        cond = $" And imgNo IN ({albumInfo.ImgNo}) ";
                        break;
                    case "tab_3": // 非相簿照片
                        cond = $" And imgNo NOT IN ({albumInfo.ImgNo}) ";
                        break;
                    case "tab_4": // 未分類的相片
                        var imgNoList = db.Query<string>("SELECT ImgNo FROM Album ").ToArray().Join(",").Trim(',');
                        if (imgNoList != "") {
                            cond = $" and imgNo NOT IN ({Regex.Replace(imgNoList, ",,", ",")}) ";
                        }
                        break;
                    default:
                        cond = "AND 1=2 ";
                        break;
                }

                Sql = " SELECT ";
                Sql += "  *  ";
                Sql += " FROM  ";
                Sql += "   ( ";
                Sql += "     SELECT *, ROW_NUMBER() OVER(ORDER BY ModifyDateTime Desc) as row  FROM Photo ";
                Sql += "    WHERE 1=1 ";
                Sql += cond == "" ? "" : cond;
                Sql += "   ) a ";
                Sql += " WHERE ";
                Sql += $"    a.row > {sPic} ";
                Sql += $"    and a.row <= {sPic + PhotoLimit} ";
                Sql += " Order By a.ModifyDateTime Desc ";

                pv.PhotoList = db.Query<Photo>(Sql).ToList();
            }

            ViewBag.Type = type;
            ViewBag.TargetID = Key.Dict(ref tags, "TargetID");
            ViewBag.IsEnd = pv.PhotoList.Count < PhotoLimit;

            pv.Album = albumInfo;
            pv.ViewBag = ViewBag;

            rtn[0] = ViewBag.IsData ? "" : "請先上傳圖片 !";
            rtn[1] = page.View("Photo", pv);
            var obj = new { End = ViewBag.IsEnd ? "Y" : "", Cond = Key.Encrypt(cond) };
            rtn[2] = obj.ToJson();
            return new JsonNetResult(rtn);
        }

        /// <summary>
        /// 加入相簿, 編修相簿名稱
        /// </summary>
        /// <param name="vt"></param>
        /// <returns></returns>
        public ActionResult AddImg(VtTags vt)
        {
            string[] rtn = new string[] { "", "" };
            if (vt.Error) {
                return PageSettion.VarTagsError(vt.ErrorMsg);
            }
            var tags = vt.Tags;
            PageTableViewModel pv = new PageTableViewModel();
            var imgNo = Key.Dict(ref tags, "ImgNo");
            var albumNo = Key.Dict(ref tags, "AlbumNo");
            var albumDesc = Key.Dict(ref tags, "AlbumDesc");
            var albumName = Key.Dict(ref tags, "AlbumName");

            Album a = db.Query<Album>($" SELECT * FROM Album WHERE AlbumNo={albumNo} ").FirstOrDefault();
            if (a != null) {
                if (imgNo != "") {
                    a.ImgNo = $"{a.ImgNo},{imgNo}".Trim(',');
                    if (string.IsNullOrEmpty(a.BgImg)) {
                        var bgImgNo = imgNo.Split(',')[0].Replace("'", "").Trim(',');
                        Sql = $"SELECT CAST(ImgNo AS varchar) + FileExt FileName FROM Photo  WHERE ImgNo = {bgImgNo} ";
                        a.BgImg = db.Query<string>(Sql).FirstOrDefault();
                    }
                }

                a.AlbumDesc = albumDesc;
                a.AlbumName = albumName;
                a.ModifyDateTime = now;

                db.Update(a);
                rtn[1] = "新增成功 !";
            } else {
                rtn[0] = AppConfig.NoData;
            }

            return new JsonNetResult(rtn);
        }

        /// <summary>
        /// 移除相片
        /// </summary>
        /// <param name="vt"></param>
        /// <returns></returns>
        public ActionResult RmImg(VtTags vt)
        {
            string[] rtn = new string[] { "", "" };
            if (vt.Error) {
                return PageSettion.VarTagsError(vt.ErrorMsg);
            }
            var tags = vt.Tags;
            var albumNo = Key.Dict(ref tags, "AlbumNo");
            var imgNo = Key.Dict(ref tags, "ImgNo");

            if (albumNo != "") {
                Sql = $"Select * From Album WHERE AlbumNo = {albumNo} ";
                var album = db.Query<Album>(Sql).FirstOrDefault();
                var bgImg = album.BgImg;
                var img = album.ImgNo.Split(',').ToList();

                if (img.Contains(imgNo)) {
                    img.Remove(imgNo);
                    album.ImgNo = img.ToArray().Join(",");
                }

                if (bgImg != "" && (imgNo == bgImg.Substring(0, bgImg.IndexOf('.')))) {
                    album.BgImg = "";
                }

                rtn[1] = "移除成功 !";
            } else {
                rtn[0] = AppConfig.NoData;
            }

            return new JsonNetResult(rtn);
        }

        /// <summary>
        /// 設定背景
        /// </summary>
        /// <param name="vt"></param>
        /// <returns></returns>
        public ActionResult SetBg(VtTags vt)
        {
            string[] rtn = new string[] { "", "" };
            if (vt.Error) {
                return PageSettion.VarTagsError(vt.ErrorMsg);
            }
            var tags = vt.Tags;
            var albumNo = Key.Dict(ref tags, "AlbumNo").ToInt();
            var fileName = Key.Dict(ref tags, "FileName");
            if (albumNo > 0) {
                Sql = $"SELECT * FROM Album WHERE AlbumNo = {albumNo} ";
                var photo = db.Query<Album>(Sql).FirstOrDefault();
                rtn[0] = (photo != null) ? "" : AppConfig.NoData;
                if (photo != null) {
                    photo.BgImg = fileName;
                    photo.ModifyDateTime = DateTime.Now;
                    db.Update(photo);
                    rtn[1] = "設定成功 !";
                }

            } else {
                rtn[0] = AppConfig.NoData;
            }

            return new JsonNetResult(rtn);
        }

        [HttpPost]
        /// <summary>
        /// 將相簿資料, 複製到實體檔案到資料夾並改名為fileName
        /// </summary>
        /// <param name="vt"></param>
        /// <returns></returns>        
        public ActionResult AlbumExport(VtTags vt)
        {
            string[] rtn = new string[] { "", "" };
            if (vt.Error) {
                return PageSettion.VarTagsError(vt.ErrorMsg);
            }
            var tags = vt.Tags;
            var albumNo = Key.Dict(ref tags, "AlbumNo");
            Sql = "SELECT * FROM Album";
            Sql += (albumNo == "") ? "" : $" WHERE AlbumNo = '{albumNo}' ";
            var album = db.Query<Album>(Sql).ToList();

            try {
                foreach (var a in album) {
                    Sql = $"SELECT * FROM Photo WHERE ImgNo IN ({a.ImgNo}) ";
                    var photo = db.Query<Photo>(Sql).ToList();
                    var albumFolder = $"{ImgPath}Tmp\\{a.AlbumName}\\";
                    var zipFileName = $"{a.AlbumNo}{a.AlbumName}.zip";
                    var zipPath = $"{ZipPath}{zipFileName}";

                    // 刪除已存在檔案
                    if (Directory.Exists(albumFolder)) {
                        DirectoryInfo di = new DirectoryInfo(albumFolder);
                        foreach (FileInfo file in di.GetFiles()) {
                            file.Delete();
                        }
                    } else {
                        Directory.CreateDirectory(albumFolder);
                    }

                    // 複製
                    foreach (var p in photo) {
                        var source = $"{ImgPath}{p.ImgNo}{p.FileExt}";
                        var dest = $"{albumFolder}{p.FileName}{p.FileExt}";
                        // 檔名重複
                        if (System.IO.File.Exists(dest)) {
                            dest = $"{albumFolder}{p.FileName}_{p.ImgNo}{p.FileExt}";
                        }
                        System.IO.File.Copy(source, dest, true);
                    }

                    if (System.IO.File.Exists(zipPath)) {
                        System.IO.File.Delete(zipPath);
                    }

                    ZipFile.CreateFromDirectory($"{albumFolder}", zipPath);

                    // 刪除原始檔, 保留zip
                    if (Directory.Exists(albumFolder)) {
                        DirectoryInfo di = new DirectoryInfo(albumFolder);
                        foreach (FileInfo file in di.GetFiles()) {
                            file.Delete();
                        }
                        Directory.Delete(albumFolder);
                    }
                }
            } catch (Exception e) {
                Log.ErrLog(e);
                //rtn[0] = e.Message;
            }
            return new JsonNetResult(rtn);
        }

        public ActionResult InitDetail(VtTags vt)
        {
            string[] rtn = new string[] { "", "" };
            if (vt.Error) {
                return PageSettion.VarTagsError(vt.ErrorMsg);
            }
            var tags = vt.Tags;

            Sql = $" SELECT ImgNo FROM Album WHERE AlbumNo = '{Key.Dict(ref tags, "AlbumNo")}'";
            var imgNoList = db.Query<string>(Sql).ToArray().Join(",").Trim(',');
            var imgNoListCnt = (imgNoList == "") ? 0 : imgNoList.Split(',').Length;

            Sql = $"SELECT COUNT(ImgNo) FROM Photo ";
            Sql += (imgNoListCnt == 0) ? "" : $" WHERE imgNo NOT IN ({imgNoList}) ";
            var photoCnt = db.Query<int>(Sql).FirstOrDefault();

            var obj = new { ImgAmout = photoCnt, AlbumImgCnt = imgNoListCnt, Diff = photoCnt - imgNoListCnt };
            rtn[1] = obj.ToJson();

            return new JsonNetResult(rtn);
        }

    }
}