using MemoriesCollection.Function.Common;
using MemoriesCollection.Function.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace MemoriesCollection.Controllers
{
    public class FilesController : Controller
    {

        public ActionResult GetLocal()
        {
            string t = Key.Decrypt(Request.QueryString["t"].FixReq());
            var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(t); ;
            string fileName = Key.Decrypt(Key.Dict(ref dict, "FileName"));
            //string ext = Key.Decrypt(Key.Dict(ref dict, "Ext")).ToLower();
            string root = Key.Dict(ref dict, "Root");
            string folder = Key.Dict(ref dict, "Folder");
            string dwName = Key.Dict(ref dict, "DwName");

            if (fileName == "") {
                return new EmptyResult();
                //fileName = DateTime.UtcNow.Ticks.ToString();
            }

            // 檔名不含副檔名處理方式
            //if (Path.GetExtension(fileName) == "") {
            //    fileName += ext;
            //    ext = "";
            //}

            var isPdf = fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);
            var isZip = fileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase);

            if (isPdf) {
                Response.Clear();
                Response.ContentType = "application/pdf";
                Response.AddHeader("Pragma", "no-cache");
                Response.AddHeader("content-disposition", $"inline;filename=\"{fileName}\"");
                Response.AddHeader("Expires", "0");
                Response.BinaryWrite(Files.GetFile("", "", "", fileName));
                return new EmptyResult();
            } else if (isZip) {
                string zipPath = $"{Server.MapPath(root)}{fileName}";

                System.IO.FileInfo TargetFile = new System.IO.FileInfo(zipPath);//讀進檔案
                Response.Clear();
                Response.BufferOutput = false; //若下載的檔案太大, 需要將Response.BufferOutput 設為false, 不然由於IIS的限制,可能會讓我們遇到 Overflow or underflow in the arithmetic operation的錯訊息
                Response.AddHeader("Content-Disposition", "attachment; filename=" + TargetFile.Name);
                Response.AddHeader("Content-Length", TargetFile.Length.ToString());
                Response.ContentType = "application/octet-stream";
                
                return File( new FileStream(zipPath, FileMode.Open, FileAccess.Read), System.Net.Mime.MediaTypeNames.Application.Octet, dwName);
            } else {

                //return File(System.IO.File.OpenRead(Server.MapPath(root + fileName)), System.Net.Mime.MediaTypeNames.Application.Octet, dwName);
                return File(Files.GetFile(root, folder, fileName, ""), System.Net.Mime.MediaTypeNames.Application.Octet, dwName);
                //return File(Files.GetFile(root, folder, fileName, ""), GetMimeTypeByWindowsRegistry(Path.GetExtension(fileName)), dwName);
            }
        }


        /// <summary>
        /// Retrieves the MimeType bound to the given filename or extension by looking into the Windows Registry entries.
        /// NOTE: This method supports only the MimeTypes registered in the server OS / Windows installation.
        /// </summary>
        /// <param name="fileNameOrExtension">a valid filename (file.txt) or extension (.txt or txt)</param>
        /// <returns>A valid Mime Type (es. text/plain)</returns>
        public string GetMimeTypeByWindowsRegistry(string fileNameOrExtension)
        {
            string mimeType = "application/unknown";
            string ext = (fileNameOrExtension.Contains(".")) ? System.IO.Path.GetExtension(fileNameOrExtension).ToLower() : "." + fileNameOrExtension;
            Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
            if (regKey != null && regKey.GetValue("Content Type") != null) mimeType = regKey.GetValue("Content Type").ToString();
            return mimeType;
        }
    }
}