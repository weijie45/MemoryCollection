using Dapper;
using MemoriesCollection.Function.Common;
using MemoriesCollection.Models;
using MemoriesCollection.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Westwind.Web.Mvc;

namespace MemoriesCollection.Controllers
{
    public class LogInfoController : BaseController
    {

        // GET: LogInfo
        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 查詢
        /// </summary>
        /// <param name="vt"></param>
        /// <returns></returns>
        public ActionResult FuncInfo(VtTags vt)
        {
            PageTableViewModel pv = new PageTableViewModel();
            string[] rtn = new string[] { "", "", "" };
            if (vt.Error) { return PageSettion.VarTagsError(vt.ErrorMsg); }
            var tags = vt.Tags;
            var func = Key.Dict(ref tags, "Func");
            var action = Key.Dict(ref tags, "Action");
            var controller = Key.Dict(ref tags, "Controller");
            var errMsg = Key.Dict(ref tags, "ErrMsg");
            var fmDate = Key.Dict(ref tags, "SearchFmDate").Replace("/", "");
            var toDate = Key.Dict(ref tags, "SearchToDate").Replace("/", "");

            switch (func) {
                case "Query":
                    Sql = " SELECT ";
                    Sql += "   * ";
                    Sql += " FROM ";
                    Sql += "   ErrorLog  ";
                    Sql += " WHERE 1=1";
                    Sql += $" AND   convert(char, LogDate, 112)  BETWEEN '{fmDate}' AND '{toDate}' ";
                    Sql += errMsg == "" ? "" : $" AND  Msg Liek '%{errMsg}%' ";
                    Sql += action == "" ? "" : $" AND  Action = '{action}' ";
                    Sql += controller == "" ? "" : $"   AND Controller like '%{controller}%' ";
                    Sql += " ORDER BY ";
                    Sql += "   LogDate DESC ";

                    var errLog = db.Query<ErrorLog>(Sql).ToList();
                    pv.ErrLogList = errLog;
                    ViewBag.IsData = errLog.Count() > 0;

                    break;
            }

            ViewBag.TargetID = Key.Dict(ref tags, "TargetID");
            pv.ViewBag = ViewBag;
            rtn[1] = page.View(func, pv);
            return new JsonNetResult(rtn);
        }

    }
}