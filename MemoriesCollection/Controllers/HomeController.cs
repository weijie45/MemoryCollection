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

        /// <summary>
        /// 時間軸
        /// </summary>
        /// <returns></returns>
        public ActionResult TimeLine()
        {
            Sql = " SELECT ";
            Sql += "   p.ImgNo, ";
            Sql += "   p.FileExt, ";
            Sql += "   p.Location, ";
            Sql += "   p.Person, ";
            Sql += "   CONVERT( VARCHAR(7), ";
            Sql += "            CASE p.OrgCreateDateTime ";
            Sql += "            WHEN '9999-12-31 00:00:00.000' ";
            Sql += "            THEN p.CreateDateTime ";
            Sql += "            ELSE p.OrgCreateDateTime END, 126 ) YearMon,";
            Sql += "   p.FileDesc, ";
            Sql += "   a.CNT ";
            Sql += " FROM ";
            Sql += "   Photo p , ";
            Sql += "   ( ";
            Sql += "   SELECT ";
            Sql += "   CONVERT( VARCHAR(7), ";
            Sql += "            CASE OrgCreateDateTime ";
            Sql += "            WHEN '9999-12-31 00:00:00.000' ";
            Sql += "            THEN CreateDateTime ";
            Sql += "            ELSE OrgCreateDateTime END, 126 ) YearMon,";
            Sql += "       COUNT(1) CNT ";
            Sql += "     FROM ";
            Sql += "       Photo ";
            Sql += "     GROUP BY ";
            Sql += "       CONVERT( VARCHAR(7), ";
            Sql += "                CASE OrgCreateDateTime ";
            Sql += "                WHEN '9999-12-31 00:00:00.000' ";
            Sql += "                THEN CreateDateTime ";
            Sql += "                ELSE OrgCreateDateTime END, 126 ) ";
            Sql += "   ) AS a ";
            Sql += " WHERE ";
            Sql += "   p.CreateDateTime IN( ";
            Sql += "     SELECT ";
            Sql += "       MAx(CreateDateTime) CrtTime ";
            Sql += "     FROM ";
            Sql += "       Photo ";
            Sql += "     GROUP BY ";
            Sql += "       CONVERT( VARCHAR(7), ";
            Sql += "                CASE OrgCreateDateTime ";
            Sql += "                WHEN '9999-12-31 00:00:00.000' ";
            Sql += "                THEN CreateDateTime ";
            Sql += "                ELSE OrgCreateDateTime END, 126 ) ";
            Sql += "   ) ";
            Sql += "  AND a.YearMon = ";
            Sql += "       CONVERT( VARCHAR(7), ";
            Sql += "                CASE OrgCreateDateTime ";
            Sql += "                WHEN '9999-12-31 00:00:00.000' ";
            Sql += "                THEN CreateDateTime ";
            Sql += "                ELSE OrgCreateDateTime END, 126 ) ";
            Sql += " ORDER BY YearMon DESC ";
            ViewBag.Data = db.Query(Sql).ToList();
            return View("TimeLine");
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
            Sql += "     SELECT *, ROW_NUMBER() OVER(ORDER BY ModifyDateTime Desc) as row  FROM Photo ";
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