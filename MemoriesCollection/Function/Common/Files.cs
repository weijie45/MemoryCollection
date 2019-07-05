using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Hosting;

namespace MemoriesCollection.Function.Common
{
    public class Files
    {
        public static byte[] GetFtpFile(string rootFolder, string folderTitle, string fileName, string fileDescOrExt)
        {
            byte[] aBuffer = new byte[] { 0 };
            using (var webClient = new WebClient()) {
                webClient.Credentials = new NetworkCredential(AppConfig.FtpUser, AppConfig.FtpPassword);

                var fileExt = Path.GetExtension(fileDescOrExt);
                try {
                    aBuffer = webClient.DownloadData($"ftp://{AppConfig.FtpIp}{rootFolder}{folderTitle}{fileName}{fileExt}");
                } catch (Exception e) {
                    string msg = e.Message;
                }
            }
            return aBuffer;
        }

        public static bool PutFtpFile(string rootFolder, string folderTitle, object key, string fileDescOrExt, byte[] fileData)
        {
            var isPass = true;

            var fileExt = Path.GetExtension(fileDescOrExt.ToString());
            var ftpFolder = $"ftp://{AppConfig.FtpIp}{rootFolder}{folderTitle}";
            if (rootFolder != "" || folderTitle != "") {
                try {
                    // 直接建立目錄, 有目錄會error但沒差, 可以減少多跑一次連線
                    WebRequest req = WebRequest.Create(ftpFolder);
                    req.Method = WebRequestMethods.Ftp.MakeDirectory;
                    req.Credentials = new NetworkCredential(AppConfig.FtpUser, AppConfig.FtpPassword);
                    using (var resp = (FtpWebResponse)req.GetResponse()) { }
                } catch {
                }
            }
            try {
                WebClient wc = new WebClient();
                wc.Credentials = new NetworkCredential(AppConfig.FtpUser, AppConfig.FtpPassword);
                wc.UploadData($"{ftpFolder}/{key}{fileExt}", fileData);
            } catch {
                isPass = false;
            }
            return isPass;
        }

        public static bool DelFtpFile(string rootFolder, string folderTitle, object key, object fileDescOrExt)
        {
            var isPass = true;

            var fileExt = Path.GetExtension(fileDescOrExt.ToString());
            try {
                FtpWebRequest req = (FtpWebRequest)WebRequest.Create($"ftp://{AppConfig.FtpIp}{rootFolder}{folderTitle}{key}{fileExt}");
                req.Credentials = new NetworkCredential(AppConfig.FtpUser, AppConfig.FtpPassword);
                req.Method = WebRequestMethods.Ftp.DeleteFile;
                using (var resp = (FtpWebResponse)req.GetResponse()) { }
            } catch {
                isPass = false;
            }
            return isPass;
        }

        public static byte[] GetFile(string rootFolder, string folderTitle, string fileName, string fileDescOrExt)
        {
            byte[] aBuffer = new byte[] { 0 };
            var fileExt = Path.GetExtension(fileDescOrExt);
            try {
                string path = HostingEnvironment.MapPath($"{rootFolder}{folderTitle}{fileName}{fileExt}");
                aBuffer = File.ReadAllBytes(path);
            } catch (Exception e) {
                Log.ErrLog(e);
            }

            return aBuffer;
        }

        public static bool PutFile(byte[] file, string path, object fileName, string fileExt)
        {
            //string filePath = HostingEnvironment.MapPath(path + fileName + fileExt);
            string filePath = (path + fileName + fileExt);
            try {
                //Directory.CreateDirectory(HostingEnvironment.MapPath(path));
                Directory.CreateDirectory(path);
            } catch {

            }
            File.WriteAllBytes(filePath, file);
            return File.Exists(filePath);
        }


        public static bool DelFile(string path, object fileName, string fileExt)
        {
            if (fileName.Equals(string.Empty)) return false;

            string filePath = (path + fileName + fileExt);
            File.Delete(filePath);

            return !File.Exists(filePath);
        }

    }
}