using Dapper;
using Dapper.Contrib;
using Dapper.Contrib.Extensions;
using MemoriesCollection.DAL;
using MemoriesCollection.Function.Extensions;
using MemoriesCollection.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;

namespace MemoriesCollection.Function.Common
{
    public static class Log
    {
        public static void AddLog(string msg, string xml)
        {
            var routeData = HttpContext.Current.Request.RequestContext.RouteData;

            var log = new ErrorLog();
            log.Action = routeData.Values["action"].FixNull();
            log.Controller = routeData.Values["controller"].FixNull();
            log.LogDate = DateTime.Now;
            log.Message = msg;
            log.StackTrace = xml;

            var db = DbHelper.db;
            db.Insert(log);
        }

        public static void ErrLog(Exception ex)
        {
            var er = new ErrorLog();
            er.Controller = HttpContext.Current.Request.RequestContext.RouteData.Values["controller"].ToString();
            er.Action = HttpContext.Current.Request.RequestContext.RouteData.Values["action"].ToString();
            er.LogDate = DateTime.Now;
            er.Message = ex.Message;
            er.StackTrace = ex.StackTrace;
            DbHelper.db.Insert(er);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="msg"></param>
        /// <param name="appendErrMsg">顯示錯誤訊息</param>
        public static void ErrLog(Exception ex, string msg, bool appendErrMsg = true)
        {
            var er = new ErrorLog();
            er.Controller = HttpContext.Current.Request.RequestContext.RouteData.Values["controller"].ToString();
            er.Action = HttpContext.Current.Request.RequestContext.RouteData.Values["action"].ToString();
            er.LogDate = DateTime.Now;
            er.Message = appendErrMsg ? $"{msg} {ex.Message}" : ex.Message;
            er.StackTrace = ex.StackTrace;
            DbHelper.db.Insert(er);

        }

    }
}