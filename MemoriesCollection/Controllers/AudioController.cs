using MemoriesCollection.Function.Common;
using MemoriesCollection.Function.Extensions;
using MemoriesCollection.Hubs;
using MemoriesCollection.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Westwind.Web.Mvc;
using Dapper;
using Dapper.Contrib;
using Dapper.Contrib.Extensions;
using MetadataExtractor;
using System.Globalization;
using MemoriesCollection.ViewModels;
using System.Transactions;

namespace MemoriesCollection.Controllers
{
    public class AudioController : BaseController
    {
        public string[] AllowExt = new string[] { "m4a", "mp3", "wav", "ogg", };
        public int AudioLimit = AppConfig.AudioLimit.FixInt();

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        /// <summary>
        /// 每次查詢50筆
        /// </summary>
        /// <param name="vt"></param>
        /// <returns></returns>
        public ActionResult AudioList(VtTags vt)
        {
            string[] rtn = new string[] { "", "", "" };
            if (vt.Error) {
                return PageSettion.VarTagsError(vt.ErrorMsg);
            }
            var tags = vt.Tags;
            var pv = new PageTableViewModel();
            var sPic = Key.Dict(ref tags, "SPic").FixInt();

            Sql = " SELECT ";
            Sql += "  *  ";
            Sql += " FROM  ";
            Sql += "   ( ";
            Sql += "     SELECT *, ROW_NUMBER() OVER(ORDER BY ModifyDateTime Desc) as row  FROM Audio ";
            Sql += "        WHERE 1= 1 ";
            Sql += "   ) a ";
            Sql += " WHERE ";
            Sql += $"    a.row > {sPic} ";
            Sql += $"    and a.row <= {sPic + AudioLimit} ";
            Sql += " Order By a.ModifyDateTime Desc ";

            pv.AudioList = db.Query<Audio>(Sql).ToList();
            pv.IsData = pv.AudioList.Count > 0;

            ViewBag.IsEnd = pv.AudioList.Count < AudioLimit;

            pv.ViewBag = ViewBag;
            rtn[1] = page.View("Audio", pv);
            rtn[2] = ViewBag.IsEnd ? "Y" : "";
            return new JsonNetResult(rtn);
        }

        public ActionResult AddAudio()
        {
            return View();
        }

        public ActionResult Upload()
        {
            string[] rtn = new string[] { "", "" };

            if (Request.HttpMethod == "POST") {

                var files = Request.Files;
                string failUpload = files.AllKeys.Count() == 0 ? "無任何檔案上傳 !" : "";
                var totalSize = Request["fileTsize"].ToInt();
                int doneSize = 0;
                int successCnt = 0;
                int uploadCnt = files.AllKeys.Count();
                for (var i = 0; i < uploadCnt; i++) {
                    var file = files[i];
                    var fileExt = Path.GetExtension(file.FileName);
                    //https://www.aspsnippets.com/Articles/Convert-HttpPostedFile-to-Byte-Array-in-ASPNet-using-C-and-VBNet.aspx

                    if (AllowExt.Contains(fileExt.Replace(".", "").ToLower())) {
                        using (var scope = new TransactionScope()) {
                            var modDate = Key.FullDateTime(Request[$"fileModDate_{i}"]);
                            var fileName = Path.GetFileNameWithoutExtension(file.FileName);
                            var size = file.ContentLength;
                            doneSize += size;

                            Stream fs = file.InputStream;
                            Audio audio = new Audio();
                            audio.FileName = fileName;
                            audio.FileExt = fileExt;
                            audio.Size = file.ContentLength;
                            audio.OrgCreateDateTime = Key.Now;
                            audio.OrgModifyDateTime = Key.Now;
                            audio.CreateDateTime = Key.Now;
                            audio.ModifyDateTime = Key.Now;
                            db.Insert(audio);

                            var fileStream = file.InputStream;
                            // 分批寫入
                            try {
                                using (Stream ftpStream = System.IO.File.Create($"{AudioPath}{audio.AudioNo}{fileExt}")) {
                                    byte[] buffer = new byte[1024000];

                                    int read;
                                    while ((read = fileStream.Read(buffer, 0, buffer.Length)) > 0) {
                                        doneSize += 1024000;
                                        ftpStream.Write(buffer, 0, read);
                                    }
                                }
                            } catch (Exception e) {

                            }
                            fileStream.Seek(0, SeekOrigin.Begin);

                            ////取得 Metadata , 影片解析                               
                            string crtDateTime = "", modDateTime = "";
                            var directories = ImageMetadataReader.ReadMetadata(fileStream);
                            foreach (var directory in directories) {
                                foreach (var tag in directory.Tags) {
                                    var name = tag.Name.ToLower();
                                    var val = tag.Description.Trim();
                                    switch (name) {
                                        case "created":
                                            crtDateTime = (crtDateTime != "") ? crtDateTime : val;
                                            break;
                                        case "modified":
                                            modDateTime = (modDateTime != "") ? modDateTime : val;
                                            break;
                                    }
                                }
                            }

                            Sql = $"SELECT * FROM Audio WHERE AudioNo = '{audio.AudioNo}' ";
                            var res = db.Query<VideoInfo>(Sql).FirstOrDefault();
                            res.OrgCreateDateTime = DateTimeFormat(crtDateTime);
                            res.OrgModifyDateTime = DateTimeFormat(modDateTime);

                            var percent = ((double)doneSize / (double)totalSize * 100 / 2 + 50);
                            ProgressHub.SendMessage(percent > 100 ? 100 : percent);

                            scope.Complete();
                        }
                    }
                }

                rtn[0] = failUpload;
                var obj = new { Files = files.Count, Success = successCnt, Failed = uploadCnt - successCnt };

                rtn[1] = obj.ToJson();

            }

            return new JsonNetResult(rtn);
        }

        public ActionResult Save(VtTags vt)
        {
            string[] rtn = new string[] { "", "", "" };
            if (vt.Error) {
                return PageSettion.VarTagsError(vt.ErrorMsg);
            }
            var tags = vt.Tags;

            Sql = $"SELECT * FROM Audio WHERE AudioNo = '{Key.Dict(ref tags, "AudioNo")}' ";
            var audio = db.Query<Audio>(Sql).FirstOrDefault();

            if (audio != null) {
                audio.FileName = Key.Dict(ref tags, "FileName");
                audio.ModifyDateTime = Key.Now;
                db.Update(audio);
            } else {
                rtn[0] = AppConfig.NoData;
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
    }
}