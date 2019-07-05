using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Mvc;
using MemoriesCollection.DAL;
using MemoriesCollection.Function.Common;
using MemoriesCollection.Function.Common.Base;
using MemoriesCollection.Function.Extensions;

namespace MemoriesCollection
{
    public class BaseController : Controller
    {
        public PageSettion page = new PageSettion();
        public DateTime now = DateTime.Now;
        public string ErrMsg = "";
        public string ImgPath = AppConfig.ImgPath;
        public string ImgThbPath = AppConfig.ImgThbPath;
        public string ZipPath = AppConfig.ZipPath;
        public string VideoPath = AppConfig.VideoPath;
        public string VideoThbPath = AppConfig.VideoThbPath;
        public string Sql = "";
        public string[] AllowExt = new string[] { "jpg", "jpeg", "png", "bmp", "gif", "tiff" };
        public int PhotoLimit = AppConfig.PhotoLimit.FixInt();

        public BaseController()
        {
            //if (!AppConfig.FtpPass) {
            //    throw new Exception("Ftp 連線失敗");
            //}
            ImgPath = System.Web.HttpContext.Current.Server.MapPath(ImgPath);
            ImgThbPath = System.Web.HttpContext.Current.Server.MapPath(ImgThbPath);
            ZipPath = System.Web.HttpContext.Current.Server.MapPath(ZipPath);
            VideoPath = System.Web.HttpContext.Current.Server.MapPath(VideoPath);
            VideoThbPath = System.Web.HttpContext.Current.Server.MapPath(VideoThbPath);
        }

        public static SqlConnection db
        {
            get
            {
                return DbHelper.db;
            }
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            bool isAjaxCall = string.Equals("XMLHttpRequest", filterContext.RequestContext.HttpContext.Request.Headers["x-requested-with"], StringComparison.OrdinalIgnoreCase);
            var ex = filterContext.Exception;

            //Debugger.Break();

            List<string> errors = new List<string>();
            if (ex is HttpAntiForgeryException) {
                var errorMsg = new ContentResult();
                errorMsg.Content = "無效的網站識別碼 !";
                filterContext.Result = errorMsg;
            } else {
                if (isAjaxCall) {


                    if (ex.InnerException != null) {
                        if (ex.InnerException.InnerException == null) {
                            ErrMsg = ex.InnerException.Message;
                        } else {
                            ErrMsg = ex.InnerException.InnerException.Message;
                        }
                    } else {
                        ErrMsg = ex.Message;
                    }
                    if (ErrMsg.StartsWith("Violation of PRIMARY KEY constraint")) {
                        ErrMsg = AppConfig.DataExist;
                    }
                    errors.Add(ErrMsg);

                    filterContext.Result = new JsonNetResult()
                    {
                        Data = new JsonResponseModel { Errors = new List<string>() { "發生例外狀況，請重整頁面後再嘗試，或通知服務人員。" }, Status = "500" }
                    };
                } else {
                    if (ex.InnerException != null) {
                        if (ex.InnerException.InnerException == null) {
                            ErrMsg = ex.InnerException.Message;
                        } else {
                            ErrMsg = ex.InnerException.InnerException.Message;
                        }
                    } else {
                        ErrMsg = ex.Message;
                    }
                    errors.Add(ErrMsg);

                    //filterContext.Result = new JsonResult()
                    //{
                    //    Data = new JsonResponse { Errors = errors }
                    //};
                    filterContext.Result = RedirectToAction("Index", "ErrorHandle", new { ErrMsg = ErrMsg });
                }

                Log.AddLog(ErrMsg, ex.StackTrace);
            }

            //Make sure that we mark the exception as handled
            filterContext.ExceptionHandled = true;
        }

        /// <summary>
        /// 404找不到資源
        /// </summary>
        /// <param name="actionName"></param>
        protected override void HandleUnknownAction(string actionName)

        {
            Response.Redirect("/", true);

        }
    }

}