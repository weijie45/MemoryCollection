using MemoriesCollection.Function.Common;
using MemoriesCollection.Function.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
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
            string fileName = !dict.ContainsKey("FileName") ? "" : dict["FileName"].ToString();
            string root = !dict.ContainsKey("Root") ? "" : dict["Root"].ToString();
            string folder = !dict.ContainsKey("Folder") ? "" : dict["Folder"].ToString();
            string dwName = !dict.ContainsKey("DwName") ? "" : dict["DwName"].ToString();

            if (fileName == "") {
                fileName = DateTime.UtcNow.Ticks.ToString();
            }

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
                Response.AddHeader("Content-Disposition", "attachment; filename=" + TargetFile.Name);
                Response.AddHeader("Content-Length", TargetFile.Length.ToString());
                Response.ContentType = "application/octet-stream";
                Response.WriteFile(TargetFile.FullName);
                Response.End();
                return new EmptyResult();
            } else {
                return File(Files.GetFile(root, folder, fileName, ""), System.Net.Mime.MediaTypeNames.Application.Octet, dwName);
            }
        }

    }
}