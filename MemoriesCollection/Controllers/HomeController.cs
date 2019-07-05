using Dapper;
using MemoriesCollection;
using MemoriesCollection.Function.Common.Base;
using MemoriesCollection.Function.Common;
using MemoriesCollection.Function.Extensions;
using MemoriesCollection.Models;
using MemoriesCollection.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MemoriesCollection.Controllers
{
    public class HomeController : BaseController
    {
        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult Index()
        {
            return View();
        }


        [HttpPost]
        /// <summary>
        /// 批次取得圖片(預設:50張)
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
            var pv = new PageTableViewModel();
            var sPic = Key.Dict(ref tags, "SPic").FixInt();
            var cond = Key.Dict(ref tags, "Cond"); // 時間軸的時間

            Sql = " SELECT ";
            Sql += "  *  ";
            Sql += " FROM  ";
            Sql += "   ( ";
            Sql += "     SELECT *, ROW_NUMBER() OVER(ORDER BY ModifyDateTime Desc) as row  FROM Photo ";
            Sql += "        WHERE 1= 1 ";
            if (cond != "") {
                Sql += Key.Decrypt(cond);
            }
            Sql += "   ) a ";
            Sql += " WHERE ";
            Sql += $"    a.row > {sPic} ";
            Sql += $"    and a.row <= {sPic + PhotoLimit} ";
            Sql += " Order By a.ModifyDateTime Desc ";

            pv.PhotoList = db.Query<Photo>(Sql).ToList();
            pv.IsData = pv.PhotoList.Count > 0;

            ViewBag.IsEnd = pv.PhotoList.Count < PhotoLimit;

            pv.ViewBag = ViewBag;
            rtn[1] = page.View("Photo", pv);
            rtn[2] = ViewBag.IsEnd ? "Y" : "";

            return this.ToJsonNet(rtn);
        }


        /// <summary>
        /// 取得相片及影片數量
        /// </summary>
        /// <param name="vt"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetSum(VtTags vt)
        {
            string[] rtn = new string[] { "", "" };
            if (vt.Error) {
                return PageSettion.VarTagsError(vt.ErrorMsg);
            }
            var tags = vt.Tags;
            var date = Key.Dict(ref tags, "Date"); // 時間軸的時間

            Sql = " SELECT  COUNT(ImgNo) photos, (SELECT COUNT(VideoNo) FROM VIDEO ) videos, (SELECT COUNT(AlbumNo) FROM Album) albums FROM PHOTO ";
            var data = db.Query(Sql).FirstOrDefault();

            var obj = new { Photos = data.photos, Videos = data.videos, Albums = data.albums };

            rtn[1] = obj.ToJson();

            return this.ToJsonNet(rtn);
        }


    }
}