using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Hosting;

namespace MemoriesCollection.Function.Common
{
    public class AppConfig
    {
        public static string FtpIp;           //FTP IP
        public static string FtpUser;         //FTP Login ID
        public static string FtpPassword;     //FTP Login Password
        public static bool FtpPass = false;
        public static string ImgPath;
        public static string VideoPath;
        public static string VideoThbPath;
        public static string ImgThbPath;
        public static string ParamError;
        public static string NoData;
        public static string DataExist;
        public static string NoFolder;
        public static string PhotoLimit;
        public static string VideoLimit;
        public static string MaxPixel;
        public static string ZipPath;


        public static void Init()
        {
            var urls = Dict(File.ReadAllText(HttpContext.Current.Server.MapPath($"~/App_Data/Urls.dll"), Encoding.GetEncoding("BIG5")));

            FtpIp = urls["FtpIp"];
            FtpUser = urls["FtpUser"];
            FtpPassword = urls["FtpPassword"];
            //FtpPass = Ftp.PutFile("", "", "", "ftp.check", new byte[] { 0x20 });
            ImgPath = urls["ImagePath"];
            VideoPath = urls["VideoPath"];
            VideoThbPath = urls["VideoThumbnailPath"];
            ImgThbPath = urls["ImageThumbnailPath"];
            ParamError = urls["ParamError"];
            NoData = urls["NoData"];
            DataExist = urls["DataExist"];
            NoFolder = urls["NoFolder"];
            PhotoLimit = urls["PhotoLimit"];
            VideoLimit = urls["VideoLimit"];
            MaxPixel = urls["MaxPixel"];
            ZipPath = urls["ZipPath"];
            if (!Directory.Exists(HostingEnvironment.MapPath(VideoThbPath))) {
                Directory.CreateDirectory(HostingEnvironment.MapPath(VideoThbPath));
            }
            if (!Directory.Exists(HostingEnvironment.MapPath(ImgThbPath))) {
                Directory.CreateDirectory(HostingEnvironment.MapPath(ImgThbPath));
            }
            if (!Directory.Exists(HostingEnvironment.MapPath(VideoPath))) {
                Directory.CreateDirectory(HostingEnvironment.MapPath(VideoPath));
            }
            if (!Directory.Exists(HostingEnvironment.MapPath(ImgPath))) {
                Directory.CreateDirectory(HostingEnvironment.MapPath(ImgPath));
            }
            if (!Directory.Exists(HostingEnvironment.MapPath(ZipPath))) {
                Directory.CreateDirectory(HostingEnvironment.MapPath(ZipPath));
            }
        }

        // <summary>
        /// 轉換為 Key / Value 資料
        /// </summary>
        /// <param name="json">json 資料</param>
        /// <returns></returns>
        public static Dictionary<string, string> Dict(string json)
        {
            var rtn = new Dictionary<string, string>();
            if (json == null || json == "") {
                return rtn;
            }
            rtn = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            return rtn;
        }
    }
}