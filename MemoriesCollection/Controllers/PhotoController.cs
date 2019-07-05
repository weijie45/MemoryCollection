using Dapper;
using Dapper.Contrib.Extensions;
using MemoriesCollection.DAL;
using MemoriesCollection.Function.Common;
using MemoriesCollection.Function.Extensions;
using MemoriesCollection.Hubs;
using MemoriesCollection.Models;
using MemoriesCollection.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using Westwind.Web.Mvc;

namespace MemoriesCollection.Controllers
{
    public class PhotoController : BaseController
    {

        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 新增相片
        /// </summary>
        /// <returns></returns>
        public ActionResult AddPhoto()
        {
            // 透過相簿上傳相片
            string k = Key.Decrypt(Request.QueryString["k"].FixReq());
            var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(k);

            ViewBag.AlbumNo = Key.Dict(ref dict, "AlbumNo");
            ViewBag.AlbumName = Key.Dict(ref dict, "AlbumName");

            return View();
        }

        [HttpPost]
        /// <summary>
        /// 批次讀取相片
        /// </summary>
        /// <param name="vt"></param>
        /// <returns></returns>
        public ActionResult Photos(VtTags vt)
        {
            string[] rtn = new string[] { "", "", "" };
            if (vt.Error) {
                return PageSettion.VarTagsError(vt.ErrorMsg);
            }
            var tags = vt.Tags;
            var sPic = Key.Dict(ref tags, "SPic").FixInt();
            string cond = "";


            PageTableViewModel pv = new PageTableViewModel();

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

            ViewBag.IsData = pv.PhotoList.Count() > 0;
            ViewBag.TargetID = Key.Dict(ref tags, "TargetID");
            ViewBag.IsEnd = pv.PhotoList.Count < PhotoLimit;

            pv.ViewBag = ViewBag;

            //rtn[0] = ViewBag.IsData ? "" : "請先上傳圖片 !";
            rtn[1] = page.View("Photo", pv);
            //var obj = new { End = ViewBag.IsEnd ? "Y" : "", Cond = Key.Encrypt(cond) };
            //rtn[2] = obj.ToJson();
            rtn[2] = ViewBag.IsEnd ? "Y" : "";
            return new JsonNetResult(rtn);
        }

        /// <summary>
        /// 刪除相片, 連同相簿也要更新
        /// </summary>
        /// <param name="vt"></param>
        /// <returns></returns>
        public ActionResult Delete(VtTags vt)
        {
            string[] rtn = new string[] { "", "" };
            if (vt.Error) {
                return PageSettion.VarTagsError(vt.ErrorMsg);
            }
            var tags = vt.Tags;
            var imgNoList = Key.Dict(ref tags, "ImgList");

            if (imgNoList != "") {

                foreach (var imgNo in imgNoList.Split(',')) {

                    Sql = $"SELECT * FROM Album WHERE ImgNo like '%{imgNo}%' ";
                    var album = db.Query<Album>(Sql).ToList();

                    using (var scope = new TransactionScope()) {
                        try {
                            Sql = $" SELECT * FROM Photo WHERE imgNo = '{imgNo}' ";
                            var photo = db.Query<Photo>(Sql).FirstOrDefault();
                            string ext = photo.FileExt;
                            db.Delete(photo);
                            foreach (var a in album) {
                                var bgImg = a.BgImg;
                                var imgList = a.ImgNo.Split(',').ToList();

                                if (imgList.Contains(imgNo.ToString())) {
                                    imgList.Remove(imgNo.ToString());
                                    a.ImgNo = imgList.ToArray().Join(",");
                                }

                                if (imgNo == bgImg.Substring(0, bgImg.IndexOf('.'))) {
                                    a.BgImg = "";
                                }

                                a.ModifyDateTime = DateTime.Now;
                                db.Update(a);
                            }

                            if (Files.DelFile(ImgPath, imgNo, ext)) {
                                scope.Complete();
                                rtn[1] = "刪除成功 !";
                            } else {
                                rtn[0] = "刪除失敗 !";
                            }

                        } catch (Exception e) {
                            //rtn[0] = $"刪除失敗 !";
                            Log.ErrLog(e);
                        }
                    }

                }

            } else {
                rtn[0] = AppConfig.NoData;
            }

            return new JsonNetResult(rtn);
        }

        /// <summary>
        /// 上傳圖片
        /// </summary>
        /// <returns></returns>
        public ActionResult Upload()
        {
            string[] rtn = new string[] { "", "" };

            if (Request.HttpMethod == "POST") {

                var files = Request.Files;
                string failUpload = files.AllKeys.Count() == 0 ? "無任何檔案上傳 !" : "";
                var totalSize = Request["fileTsize"].ToInt();
                var albumNo = Request["album"].FixNull();
                var addList = new List<int>(); // 要加入相簿的相片    
                int doneSize = 0;
                int successCnt = 0;
                int uploadCnt = files.AllKeys.Count();
                for (var i = 0; i < uploadCnt; i++) {
                    var file = files[i];
                    var fileExt = Path.GetExtension(file.FileName);
                    var isPass = true;
                    //https://www.aspsnippets.com/Articles/Convert-HttpPostedFile-to-Byte-Array-in-ASPNet-using-C-and-VBNet.aspx

                    if (AllowExt.Contains(fileExt.Replace(".", "").ToLower())) {
                        var modDate = Key.FullDateTime(Request[$"fileModDate_{i}"]);
                        var fileName = Path.GetFileNameWithoutExtension(file.FileName);
                        var size = file.ContentLength;
                        doneSize += size;

                        Stream fs = file.InputStream;
                        Photo img = new Photo();
                        img.FileName = fileName;
                        img.FileExt = fileExt;
                        img.Width = 0;
                        img.Height = 0;
                        img.OrgCreateDateTime = Key.Now;
                        img.OrgModifyDateTime = Key.Now;
                        img.CreateDateTime = Key.Now;
                        img.ModifyDateTime = Key.Now;

                        if (db.Insert(img) > 0) {
                            var imgInfo = GetExIf(fs, img.ImgNo.ToString(), fileExt);
                            if (imgInfo["Error"] == "") {
                                Sql = $"SELECT * FROM Photo WHERE ImgNo = '{img.ImgNo}' ";

                                var res = db.Query<Photo>(Sql).FirstOrDefault();
                                res.Width = imgInfo["Width"].ToInt();
                                res.Height = imgInfo["Height"].ToInt();
                                // 無拍攝日期, 就用檔案最後更新日期
                                res.OrgCreateDateTime = DateTime.Parse((imgInfo["OrgCreatDateTime"] == "") ? modDate : imgInfo["OrgCreatDateTime"]);
                                res.OrgModifyDateTime = DateTime.Parse((imgInfo["OrgModifyDateTime"] == "") ? modDate : imgInfo["OrgModifyDateTime"]);

                                if (db.Update(res)) {
                                    addList.Add(img.ImgNo);
                                    successCnt++;
                                    isPass = true;
                                }
                            } else {
                                isPass = false;
                                failUpload += fileName;
                            }

                            if (!isPass) {
                                Files.DelFile(ImgPath, img.ImgNo, fileExt);
                                Files.DelFile(ImgThbPath, img.ImgNo, fileExt);
                                db.Delete(new Photo { ImgNo = img.ImgNo });
                            }

                        }

                        var percent = ((double)doneSize / (double)totalSize * 100 / 2 + 50);
                        ProgressHub.SendMessage(percent > 100 ? 100 : percent);
                    }
                }

                rtn[0] = failUpload;
                var obj = new { Files = files.Count, Success = successCnt, Failed = uploadCnt - successCnt };

                rtn[1] = obj.ToJson();

                if (rtn[0] == "" && albumNo != "" && addList.Count() > 0) {
                    // 加入相簿
                    Album a = db.Query<Album>($" SELECT * FROM Album WHERE AlbumNo= '{albumNo.FixSQL()}' ").FirstOrDefault();
                    a.ImgNo = $"{a.ImgNo},{addList.ToArray().Join(",")}".Trim(',');
                    // 設定背景
                    if (string.IsNullOrEmpty(a.BgImg)) {
                        var bgImgNo = a.ImgNo.Split(',')[0].Replace("'", "").Trim(',');
                        Sql = $"SELECT CAST(ImgNo AS varchar) + FileExt FileName FROM Photo  WHERE ImgNo = '{bgImgNo}' ";
                        a.BgImg = db.Query<string>(Sql).FirstOrDefault();
                    }
                    a.ModifyDateTime = now;
                    db.Update(a);
                }
            }

            return new JsonNetResult(rtn);
        }

        /// <summary>
        /// 取得Exif, 檢查ios圖片長寬問題
        /// </summary>
        /// <param name="fs"></param>
        /// <param name="fileName"></param>
        /// <param name="fileExt"></param>
        /// <returns></returns>
        public Dictionary<string, string> GetExIf(Stream fs, string fileName, string fileExt)
        {
            Encoding ascii = Encoding.ASCII;
            string picDate;
            var dt = new Dictionary<string, string>();
            Image image = Image.FromStream(fs, true, false);
            short orientation = 0;
            dt["Error"] = "";
            dt["OrgModifyDateTime"] = "";
            dt["OrgCreatDateTime"] = "";
            int[] idList = new int[] { 36867, 306, 274, 5029 };
            foreach (PropertyItem p in image.PropertyItems) {
                if (idList.Contains(p.Id)) {
                    switch (p.Id) {
                        case 36867:  // 拍摄建立日期
                            picDate = ascii.GetString(p.Value);
                            if ((!"".Equals(picDate)) && picDate.Length >= 10) {
                                picDate = Regex.Replace(picDate.Substring(0, 10), "[:,-]", "/");
                                dt["OrgCreatDateTime"] = picDate;
                                dt["OrgModifyDateTime"] = picDate;
                            }
                            break;
                        case 306: // 拍摄更新日期
                            picDate = ascii.GetString(p.Value);
                            if ((!"".Equals(picDate)) && picDate.Length >= 10) {
                                picDate = Regex.Replace(picDate.Substring(0, 10), "[:,-]", "/");
                                dt["OrgModifyDateTime"] = picDate;
                            }
                            break;
                        case 274:
                        case 5029:
                            orientation = BitConverter.ToInt16(p.Value, 0);
                            switch (orientation) {
                                case 1:
                                    image.RotateFlip(RotateFlipType.RotateNoneFlipNone);
                                    //rft = RotateFlipType.RotateNoneFlipNone;
                                    break;
                                case 2:
                                    image.RotateFlip(RotateFlipType.RotateNoneFlipX);//horizontal flip
                                    break;
                                case 3:
                                    image.RotateFlip(RotateFlipType.Rotate180FlipNone);//right-top
                                    break;
                                case 4:
                                    image.RotateFlip(RotateFlipType.RotateNoneFlipY);//vertical flip
                                    break;
                                case 5:
                                    image.RotateFlip(RotateFlipType.Rotate90FlipX);
                                    break;
                                case 6:
                                    image.RotateFlip(RotateFlipType.Rotate90FlipNone);//right-top
                                    break;
                                case 7:
                                    image.RotateFlip(RotateFlipType.Rotate270FlipX);
                                    break;
                                case 8:
                                    image.RotateFlip(RotateFlipType.Rotate270FlipNone);//left-bottom
                                    break;

                            }
                            break;
                    }
                }
            }

            try {

                if (orientation == 0) {
                    SaveThb(fs, fileName, fileExt);

                    fs.Seek(0, SeekOrigin.Begin);
                    // Save Original Photo
                    using (Stream ftpStream = System.IO.File.Create($"{ImgPath}{fileName}{fileExt}")) {
                        byte[] buffer = new byte[1024000];
                        int read;
                        while ((read = fs.Read(buffer, 0, buffer.Length)) > 0) {
                            ftpStream.Write(buffer, 0, read);
                        }
                    }

                } else {
                    byte[] data = null;
                    using (MemoryStream oMemoryStream = new MemoryStream()) {
                        //建立副本
                        using (Bitmap oBitmap = new Bitmap(image)) {
                            //儲存圖片到 MemoryStream 物件，並且指定儲存影像之格式
                            oBitmap.Save(oMemoryStream, ImageFormat.Jpeg);
                            oMemoryStream.Position = 0;
                            data = new byte[oMemoryStream.Length];
                            oMemoryStream.Read(data, 0, Convert.ToInt32(oMemoryStream.Length));
                            // 縮圖
                            SaveThb(oMemoryStream, fileName, fileExt);
                            oMemoryStream.Flush();
                        }
                    }
                    // Save Original Photo
                    Files.PutFile(data, ImgPath, fileName, fileExt);
                }
            } catch (Exception e) {
                Log.ErrLog(e);
                dt["Error"] = "製作縮圖失敗 !";
            }

            dt["Orientation"] = orientation.FixNull();
            dt["Width"] = image.Width.FixNull();
            dt["Height"] = image.Height.FixNull();

            fs.Close();
            return dt;
        }

        /// <summary>
        /// 建立儲存縮圖
        /// </summary>
        /// <param name="fs"></param>
        /// <param name="fileName"></param>
        /// <param name="fileExt"></param>
        public void SaveThb(Stream fs, object fileName, string fileExt)
        {
            Image image = Image.FromStream(fs);
            //必須使用絕對路徑
            ImageFormat thisFormat = image.RawFormat;
            //取得影像的格式
            int fixWidth = 0;
            int fixHeight = 0;
            //第一種縮圖用
            int maxPx = AppConfig.MaxPixel.ToInt();
            //宣告一個最大值，demo是把該值寫到web.config裡
            if (image.Width > maxPx || image.Height > maxPx)
            //如果圖片的寬大於最大值或高大於最大值就往下執行
            {
                if (image.Width >= image.Height)
                //圖片的寬大於圖片的高
                {
                    fixHeight = maxPx;
                    //設定修改後的圖高
                    fixWidth = Convert.ToInt32((Convert.ToDouble(fixHeight) / Convert.ToDouble(image.Height)) * Convert.ToDouble(image.Width));
                    //設定修改後的圖寬
                } else {

                    fixWidth = maxPx;
                    //設定修改後的圖寬
                    fixHeight = Convert.ToInt32((Convert.ToDouble(fixWidth) / Convert.ToDouble(image.Width)) * Convert.ToDouble(image.Height));
                    //設定修改後的圖高
                }
            } else
              //圖片沒有超過設定值，不執行縮圖
              {
                fixHeight = image.Height;
                fixWidth = image.Width;
            }

            Bitmap imageOutput = new Bitmap(image, fixWidth, fixHeight);

            try {
                //using (var stream = new MemoryStream()) {
                //    imageOutput.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                //    Ftp.PutLocalFile(stream.ToArray(), ImgThbPath, fileName, fileExt);
                //}
                //using (Stream ftpStream = System.IO.File.Create(Server.MapPath($"{ImgPath}{fileExt}"))) {
                //    byte[] buffer = new byte[1024000];

                //    int read;
                //    while ((read = fs.Read(buffer, 0, buffer.Length)) > 0) {
                //        ftpStream.Write(buffer, 0, read);
                //    }
                //}

                imageOutput.Save(string.Concat(ImgThbPath, fileName + fileExt), thisFormat);


            } catch (Exception e) {
                Log.ErrLog(e);
                throw e;
            } finally {

                //將修改過的圖存於設定的位子
                imageOutput.Dispose();
                //釋放記憶體
                image.Dispose();
                //釋放掉圖檔 
            }
        }

    }
}