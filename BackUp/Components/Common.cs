using RestSharp;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BackUp.Components
{
    static class Common
    {
        public static NameValueCollection ApplicationSettings;

        /// <summary>
        /// 取得AppConfig
        /// </summary>
        /// <param name="sKey"></param>
        /// <returns></returns>
        public static string AppSettings(string sKey)
        {
            string sAppSettings = null;

            if (ApplicationSettings != null) {
                sAppSettings = ApplicationSettings[sKey];
            }
            if (sAppSettings == null) {
                sAppSettings = ConfigurationManager.AppSettings[sKey];
            }

            //switch (sKey) {
            //    case "Destination_Path":
            //        sAppSettings = @"D:\Sync_Photo";
            //        break;
            //    case "BackUp_Path":
            //        sAppSettings = @"D:\BackUp_Photo";
            //        break;
            //    case "Source_Path":
            //        //sAppSettings = @"C:\inetpub\wwwroot\Memory\Upload\";
            //        break;
            //}

            return FixNull(sAppSettings);
        }

        /// <summary>
        /// Line Notify
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static string LineSend(string msg)
        {
            var rtn = "";
            var client = new RestClient("https://notify-api.line.me/");
            var req = new RestRequest("api/notify", Method.POST);

            try {
                req.AddHeader("Authorization", "Bearer ZtKnpXpLMsySlr7Sc59TIFAJ4ac86Fevvb2ROgzn4gd");//測試
                //req.AddHeader("Authorization", "Bearer zvc7Vxi7Ilw4wK8dXDyCLuG3iSzvgF63O3lXD4m5WcZ"); //正式
                req.AddHeader("Content-Type", "application/x-www-form-urlencoded");
                req.AddParameter("message", Regex.Replace(msg, @"^\s+$[\r\n]*", string.Empty, RegexOptions.Multiline));
                var resp = client.Execute<Dictionary<string, string>>(req);
                rtn = resp.Data["message"];
            } catch (Exception ex) {
                rtn = ex.Message;
            }
            return rtn;
        }

        public static void Log(string msg, bool whiteLine = false)
        {
            try {
                Console.WriteLine(msg);
                if (whiteLine) Console.WriteLine("");

                using (StreamWriter sw = File.AppendText($"{Directory.GetCurrentDirectory()}//Log.txt")) {
                    sw.WriteLine(msg);
                    if (whiteLine) sw.WriteLine("");
                }
            } catch (Exception e) {
                //LineSend(e.Message);
            }
        }

        public static string FixNull(this object obj)
        {
            if (obj == null || obj == DBNull.Value) {
                return "";
            } else {
                return obj.ToString().Trim();
            }
        }

        /// <summary>
        /// LINE 專屬的Emoji 
        /// </summary>
        public static class Emoji
        {
            public static string star = char.ConvertFromUtf32(0x1000B2);
            public static string heart = char.ConvertFromUtf32(0x100037);
            public static string cake = char.ConvertFromUtf32(0x100076);
            public static string shinyStar = char.ConvertFromUtf32(0x10002D);
            public static string car = char.ConvertFromUtf32(0x100049);
            public static string bed = char.ConvertFromUtf32(0x1000B5);
            public static string airplane = char.ConvertFromUtf32(0x10004A);
            public static string angry = char.ConvertFromUtf32(0x10001D);
            public static string kiss = char.ConvertFromUtf32(0x100007);
            public static string shy = char.ConvertFromUtf32(0x10000A);
            public static string happy = char.ConvertFromUtf32(0x100001);
            public static string bank = char.ConvertFromUtf32(0x100050);
            public static string info = char.ConvertFromUtf32(0x100035);
        }

    }
}
