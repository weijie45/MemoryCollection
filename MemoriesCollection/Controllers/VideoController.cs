using Dapper;
using Dapper.Contrib.Extensions;
using MetadataExtractor;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using MemoriesCollection.Function.Common;
using MemoriesCollection.Function.Extensions;
using MemoriesCollection.Models;
using MemoriesCollection.ViewModels;
using Westwind.Web.Mvc;
using MemoriesCollection.Hubs;
using System.Transactions;
using Newtonsoft.Json;
using System.Drawing;

namespace MemoriesCollection.Controllers
{
    public class VideoController : BaseController
    {
        public int VideoLimit = AppConfig.VideoLimit.FixInt();

        // GET: Video
        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 新增相片
        /// </summary>
        /// <returns></returns>
        public ActionResult AddVideo()
        {
            return View();
        }

        [HttpPost]
        /// <summary>
        /// 每次查詢50筆
        /// </summary>
        /// <param name="vt"></param>
        /// <returns></returns>
        public ActionResult VideoList(VtTags vt)
        {
            string[] rtn = new string[] { "", "", "" };
            if (vt.Error) {
                return PageSettion.VarTagsError(vt.ErrorMsg);
            }
            var tags = vt.Tags;
            var pv = new PageTableViewModel();
            var sPic = Key.Dict(ref tags, "SPic").FixInt();
            var fmDate = Key.Dict(ref tags, "FmDate");
            var toDate = Key.Dict(ref tags, "ToDate");
            var keyWord = Key.Dict(ref tags, "KeyWord");
            var timeLineDate = Key.Dict(ref tags, "Date");
            string cond = "";

            cond += fmDate == "" ? "" : $"AND OrgCreateDateTime >= '{fmDate.Replace("-", "")}' ";
            cond += toDate == "" ? "" : $"AND OrgCreateDateTime <= '{toDate.Replace("-", "")}' ";
            cond += keyWord == "" ? "" : $"AND FileName like'%{keyWord}%'  ";

            Sql = " SELECT ";
            Sql += "  *  ";
            Sql += " FROM  ";
            Sql += "   ( ";
            Sql += "     SELECT *, ROW_NUMBER() OVER(ORDER BY ModifyDateTime Desc) as row  FROM Video ";
            Sql += "        WHERE 1= 1 ";
            Sql += cond == "" ? "" : cond;

            if (timeLineDate != "") {
                Sql += " AND CONVERT( VARCHAR(7), ";
                Sql += " CASE OrgCreateDateTime ";
                Sql += " WHEN '9999-12-31 00:00:00.000' ";
                Sql += " THEN CreateDateTime ";
                Sql += " ELSE OrgCreateDateTime END, 126 ) ";
                Sql += $" = '{timeLineDate}' ";
            }

            Sql += "   ) a ";
            Sql += " WHERE ";
            Sql += $"    a.row > {sPic} ";
            Sql += $"    and a.row <= {sPic + VideoLimit} ";
            Sql += " Order By a.ModifyDateTime Desc ";

            pv.VideoList = db.Query<VideoInfo>(Sql).ToList();
            pv.IsData = pv.VideoList.Count > 0;

            ViewBag.IsEnd = pv.VideoList.Count < VideoLimit;

            pv.ViewBag = ViewBag;
            rtn[1] = page.View("Video", pv);
            rtn[2] = ViewBag.IsEnd ? "Y" : "";
            return new JsonNetResult(rtn);
        }

        /// <summary>
        /// 編輯相片
        /// </summary>
        /// <param name="vt"></param>
        /// <returns></returns>
        public ActionResult Edit(VtTags vt)
        {
            string[] rtn = new string[] { "", "" };
            if (vt.Error) {
                return PageSettion.VarTagsError(vt.ErrorMsg);
            }
            var tags = vt.Tags;
            PageTableViewModel pv = new PageTableViewModel();
            //var k = Key.Decrypt(Request.QueryString["k"].FixReq());
            //var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(k);
            //var videoNo = Key.Dict(ref dict, "VideoNo");
            //var backUrl = Key.Dict(ref dict, "BackUrl");
            var videoNo = Key.Dict(ref tags, "VideoNo");
            var backUrl = Key.Dict(ref tags, "BackUrl");
            ViewBag.IsData = false;
            if (videoNo != "") {

                Sql = " SELECT ";
                Sql += "    * ";
                Sql += " FROM ";
                Sql += "   Video i ";
                Sql += " WHERE ";
                Sql += $"   VideoNo = {videoNo} ";
                Sql += " Order by i.ModifyDateTime Desc";

                pv.VideoList = db.Query<VideoInfo>(Sql).ToList();

                ViewBag.IsData = pv.VideoList.Count() > 0;

                if (ViewBag.IsData) {
                    Sql = " SELECT Distinct Person FROM Video WHERE Person != '' ";
                    Sql += " UNION ";
                    Sql += " SELECT Distinct Person FROM Photo WHERE Person != '' ";
                    ViewBag.PersonList = db.Query<string>(Sql).ToArray().Join(",");
                }
            }

            pv.ViewBag = ViewBag;
            rtn[1] = page.View("Edit", pv);

            return new JsonNetResult(rtn);
        }

        /// <summary>
        /// 新增上傳
        /// </summary>
        /// <returns></returns>
        public ActionResult ConfirmUpload()
        {
            string[] rtn = new string[] { "", "" };
            List<long> time = new List<long>();
            if (Request.HttpMethod == "POST") {
                var files = Request.Files;
                double interval = (double)100 / (double)files.AllKeys.Count();
                interval = interval <= 1 ? 1.8 : interval;
                var totalSize = Request["fileTsize"].ToInt();
                int doneSize = 0;
                string videoNo = "";
                for (var i = 0; i < files.AllKeys.Count(); i++) {
                    var file = files[i];
                    var size = file.ContentLength;
                    string fileName = Path.GetFileNameWithoutExtension(file.FileName);
                    var fileExt = Path.GetExtension(file.FileName);

                    try {
                        if (fileExt == ".jpg") {
                            // 上傳影片縮圖
                            Stream fs = file.InputStream;
                            using (Stream ftpStream = System.IO.File.Create($"{VideoThbPath}{videoNo}{fileExt}")) {
                                byte[] buffer = new byte[1024000];
                                int read;
                                while ((read = fs.Read(buffer, 0, buffer.Length)) > 0) {
                                    ftpStream.Write(buffer, 0, read);
                                }
                            }

                            fs.Seek(0, SeekOrigin.Begin);
                            Image image = Image.FromStream(fs, true, false);
                            Sql = $"SELECT * FROM Video WHERE VideoNo = '{videoNo}' ";
                            var res = db.Query<VideoInfo>(Sql).FirstOrDefault();
                            var vdW = res.Width;
                            var vdH = res.Height;

                            if (image.Width != res.Width && image.Height != res.Height) {
                                res.IsRotate = "Y";
                                db.Update(res);
                            }
                        } else {
                            using (var scope = new TransactionScope()) {
                                VideoInfo vd = new VideoInfo();
                                vd.FileName = fileName;
                                vd.FileExt = fileExt;
                                vd.FileDesc = Request["videoDesc"];
                                vd.Size = file.ContentLength;
                                vd.Width = 0;
                                vd.Height = 0;
                                vd.OrgCreateDateTime = Key.Now;
                                vd.OrgModifyDateTime = Key.Now;
                                vd.CreateDateTime = Key.Now;
                                vd.ModifyDateTime = Key.Now;
                                db.Insert(vd);

                                videoNo = vd.VideoNo.ToString();

                                var fileStream = file.InputStream;
                                // 分批寫入影片
                                using (Stream ftpStream = System.IO.File.Create($"{VideoPath}{videoNo}{fileExt}")) {
                                    byte[] buffer = new byte[1024000];

                                    int read;
                                    while ((read = fileStream.Read(buffer, 0, buffer.Length)) > 0) {
                                        doneSize += 1024000;
                                        ftpStream.Write(buffer, 0, read);
                                    }
                                }

                                fileStream.Seek(0, SeekOrigin.Begin);

                                ////取得 Metadata , 影片解析         

                                string crtDateTime = "", modDateTime = "", videoW = "", videoH = "";
                                var directories = ImageMetadataReader.ReadMetadata(fileStream);
                                foreach (var directory in directories) {
                                    foreach (var tag in directory.Tags) {
                                        var name = tag.Name.ToLower();
                                        var val = tag.Description.Trim();
                                        switch (name) {
                                            case "modified":
                                                modDateTime = (modDateTime != "") ? modDateTime : val;
                                                break;
                                            case "created":
                                                crtDateTime = (crtDateTime != "") ? crtDateTime : val;
                                                break;
                                            case "width":
                                                videoW = val == "0" ? videoW : tag.Description.Trim();
                                                break;
                                            case "height":
                                                videoH = val == "0" ? videoH : tag.Description.Trim();
                                                break;
                                        }
                                    }
                                }
                                var keyName = files.AllKeys[i];
                                var fileModDate = Request[$"fileModDate_{keyName.Substring(keyName.LastIndexOf('_') + 1)}"].FixNull();
                                var modDate = fileModDate == "" ? Key.Now : DateTime.Parse(Key.FullDateTime(fileModDate));
                                var orgCrt = DateTimeFormat(crtDateTime);
                                var orgMod = DateTimeFormat(modDateTime);

                                Sql = $"SELECT * FROM Video WHERE VideoNo = '{vd.VideoNo}' ";
                                var res = db.Query<VideoInfo>(Sql).FirstOrDefault();
                                res.Width = videoW.FixInt();
                                res.Height = videoH.FixInt();
                                res.OrgCreateDateTime = DateTime.Compare(modDate, orgCrt) == -1 ? modDate : orgCrt; // 選日期小的
                                res.OrgModifyDateTime = DateTime.Compare(modDate, orgMod) == -1 ? modDate : orgCrt;

                                if (db.Update(res)) {
                                    scope.Complete();
                                } else {
                                    Files.DelFile(VideoPath, fileName, fileExt);
                                    Files.DelFile(VideoThbPath, fileName, ".jpg");
                                    rtn[0] = "新增失敗 !";
                                }

                            } // transaction       
                        }

                        ProgressHub.SendMessage(100);
                    } catch (Exception e) {
                        Files.DelFile(VideoPath, fileName, fileExt);
                        Files.DelFile(VideoThbPath, fileName, ".jpg");
                        Log.ErrLog(e, $"[Error] {fileName}");
                        rtn[0] = e.Message;
                    }

                }
            } else {
                rtn[0] = AppConfig.ParamError;
            }

            return new JsonNetResult(rtn);
        }

        public DateTime DateTimeFormat(string dateTime)
        {
            CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");
            if (dateTime.Length == 22) {
                culture = CultureInfo.CreateSpecificCulture("zh-TW");
                dateTime = dateTime == "" ? "" : dateTime.Substring(0, 9) + "," + dateTime.Substring(17) + dateTime.Substring(8, 9);
            } else if (dateTime.Length == 24) {
                dateTime = dateTime == "" ? "" : dateTime.Substring(0, 10) + "," + dateTime.Substring(19) + dateTime.Substring(10, 9);
            } else {
                dateTime = Key.Now.ToString("yyyy/MM/dd");
            }
            return DateTime.Parse(dateTime, culture, DateTimeStyles.None);
        }

        /// <summary>
        /// 影片尺寸
        /// </summary>
        /// <returns></returns>
        public ActionResult ChkSize()
        {
            string[] rtn = new string[] { "", "" };
            string w = "0", h = "0";
            if (Request.HttpMethod == "POST") {
                var files = Request.Files;
                for (var i = 0; i < files.AllKeys.Count(); i++) {
                    var file = files[i];
                    var fileStream = file.InputStream;
                    var directories = ImageMetadataReader.ReadMetadata(fileStream);
                    foreach (var directory in directories) {
                        foreach (var tag in directory.Tags) {
                            var val = tag.Description.Trim();
                            switch (tag.Name.ToLower()) {
                                case "width":
                                    w = val == "0" ? w : tag.Description.Trim();
                                    break;
                                case "height":
                                    h = val == "0" ? h : tag.Description.Trim();
                                    break;
                            }
                        }
                    }
                    //Log.AddLog(file.FileName, $"實際寬高:{w} * {h}/寬高:{Request["w"]} * {Request["h"]}");
                }
            }
            rtn[1] = new { w = w, h = h }.ToJson();
            return new JsonNetResult(rtn);
        }

        /// <summary>
        /// 儲存編輯
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
            var name = Key.Dict(ref tags, "name");
            var desc = Key.Dict(ref tags, "desc");
            var location = Key.Dict(ref tags, "location");
            var person = Key.Dict(ref tags, "person");
            var videoNo = Key.Dict(ref tags, "videoNo").ToInt();
            var base64Img = Key.Dict(ref tags, "Img");
            var isSuccess = false;

            Sql = $"SELECT * FROM Video WHERE VideoNo = '{videoNo}' ";
            var videoInfo = db.Query<VideoInfo>(Sql).FirstOrDefault();

            if (videoInfo != null) {
                videoInfo.FileName = name;
                videoInfo.FileDesc = desc;
                videoInfo.Location = location;
                videoInfo.Person = person;
                videoInfo.ModifyDateTime = Key.Now;
                isSuccess = db.Update(videoInfo);
            } else {
                rtn[0] = AppConfig.NoData;
            }

            return new JsonNetResult(rtn);
        }

        /// <summary>
        /// 刪除影片
        /// </summary>
        /// <param name="vt"></param>
        /// <returns></returns>
        public ActionResult Del(VtTags vt)
        {
            string[] rtn = new string[] { "", "" };
            if (vt.Error) {
                return PageSettion.VarTagsError(vt.ErrorMsg);
            }
            var tags = vt.Tags;
            var videoNo = Key.Dict(ref tags, "VideoNo");

            Sql = $" SELECT  CAST(VideoNo AS varchar) VideoNo, FileExt  FROM Video WHERE VideoNo = {videoNo} ";
            var info = db.Query(Sql).FirstOrDefault();
            if (info != null) {
                var ftpDel = Files.DelFile(VideoThbPath, info.VideoNo, info.Ext);
                if (!ftpDel) {
                    rtn[0] = "刪除影片失敗 !";
                } else {
                    Sql = $"Delete Video WHERE VideoNo = {videoNo} ";
                    db.Execute(Sql);
                }

            } else {
                rtn[0] = AppConfig.NoData;
            }
            return new JsonNetResult(rtn);
        }

    }
}