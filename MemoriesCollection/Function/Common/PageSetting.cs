using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MemoriesCollection.Function.Extensions;
using Westwind.Web.Mvc;
using MemoriesCollection.Models;
using MemoriesCollection.Function.Common.Base;

namespace MemoriesCollection.Function.Common
{
    public class PageSettion
    {
        /// <summary>
        /// 返回參數錯誤訊息
        /// </summary>
        /// <returns></returns>
        public static ActionResult VarTagsError(string errMsg)
        {
            return new JsonResult()
            {
                Data = new JsonResponseModel { Errors = new List<string>() { errMsg } }
            };
        }

        /// <summary>
        /// 取得版面
        /// </summary>
        /// <param name="viewName">View 名稱</param>
        /// <param name="model">Model 資料源</param>
        /// <returns></returns>
        public string View(string viewName, object model = null)
        {
            if (model.IsAnonymousType()) {
                model = model.ToModel();
            }
            return ViewRenderer.RenderPartialView(ViewPath(viewName), model);
        }

        /// <summary>
        /// 取得 View 路徑
        /// </summary>
        /// <param name="viewName"></param>
        /// <returns></returns>
        private string ViewPath(string viewName)
        {
            return viewName.IndexOf("/") == -1 ? $"~/Views/{System.Web.HttpContext.Current.Request.RequestContext.RouteData.Values["controller"]}/{viewName}.cshtml" : viewName;
        }
    }
}