using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;

namespace MemoriesCollection.Function.Common.Base
{
    public class JsonNetResult : JsonResult
    {
        public JsonNetResult()
        {
            this.SerializerSettings = new JsonSerializerSettings
            {
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = DateTimeZoneHandling.Local
            };
        }

        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null) {
                throw new ArgumentNullException("context");
            }
            HttpResponseBase response = context.HttpContext.Response;
            response.ContentType = !string.IsNullOrEmpty(base.ContentType) ? base.ContentType : "application/json";
            if (base.ContentEncoding != null) {
                response.ContentEncoding = base.ContentEncoding;
            }
            if (base.Data != null) {
                string s = JsonConvert.SerializeObject(base.Data, Formatting.Indented, this.SerializerSettings);
                response.Write(s);
            }
        }

        public JsonSerializerSettings SerializerSettings { get; set; }
    }

    [Serializable]
    public class JsonResponseModel
    {
        public object Data { get; set; }

        public List<string> Errors { get; set; }

        public string Status { get; set; }
    }

    public static class JsonExtension
    {
        public static string ToJsonNet(this object obj) =>
            JsonConvert.SerializeObject(obj, Formatting.Indented, new JsonSerializerSettings
            {
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = DateTimeZoneHandling.Local
            });
    }

    public static class ControllerExtensions
    {
        public static JsonNetResult ToJsonNet(this Controller controller, object data) =>
            new JsonNetResult { Data = data };
    }
}

