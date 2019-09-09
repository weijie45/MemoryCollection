using BackUp.Components;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Web;
using static BackUp.Components.Common;

namespace BackUp
{
    class Program
    {
        public static StringBuilder Msg = new StringBuilder();
        public static string[] AllowExt = new string[] { "jpg", "jpeg", "png", "bmp", "gif", "tiff" };
        public static string _Source = AppSettings("Source_Path");// @"D:\Publish\Memory\Upload";
        public static string _Destination = AppSettings("Destination_Path");// @"E:\BackUp_Photo";
        public static string _BackUp = AppSettings("BackUp_Path");// @"E:\BackUp_Photo";

        static void Main(string[] args)
        {

            try {
                if (!File.Exists($"{Directory.GetCurrentDirectory()}//Log.txt")) File.Create($"{Directory.GetCurrentDirectory()}//Log.txt");
                Directory.CreateDirectory(_BackUp);
                Directory.CreateDirectory(_Destination);
            } catch (Exception) {

            }

            if (!File.Exists(Path.Combine(_Destination, "Init.txt"))) {
                Log($"{_Source} 同步至 {_Destination}");
                Log("檔案同步中...");
                Stopwatch sw = new Stopwatch();
                sw.Start();
                DirectoryCopy(_Source, _Destination, true);
                sw.Stop();
                Log("檔案同步完成...");

                var ts = sw.Elapsed;
                Msg.AppendLine($"{Emoji.shinyStar}總耗時{string.Format("{0}:{1}", Math.Floor(ts.TotalMinutes), ts.ToString("ss\\.ff"))}");
                LineSend(Msg.ToString());
                Log($"總耗時{string.Format("{0}:{1}", Math.Floor(ts.TotalMinutes), ts.ToString("ss\\.ff"))}");

                File.CreateText(Path.Combine(_Destination, "Init.txt"));
            }

            WatcherStrat(_Source, "");
            Console.ReadKey();
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            try {
                Log("同步" + sourceDirName + "中...");
                Stopwatch sw = new Stopwatch();
                sw.Start();
                // Get the subdirectories for the specified directory.
                DirectoryInfo dir = new DirectoryInfo(sourceDirName);

                if (!dir.Exists) {
                    throw new DirectoryNotFoundException(
                        "Source directory does not exist or could not be found: "
                        + sourceDirName);
                }

                DirectoryInfo[] dirs = dir.GetDirectories();
                // If the destination directory doesn't exist, create it.
                if (!Directory.Exists(destDirName)) {
                    Directory.CreateDirectory(destDirName);
                }

                // Get the files in the directory and copy them to the new location.
                double copyFileCnt = 0;
                FileInfo[] files = dir.GetFiles();
                for (var i = 0; i < files.Count(); i++) {
                    FileInfo file = files[i];
                    //foreach (FileInfo file in files) {
                    string temppath = Path.Combine(destDirName, file.Name);
                    if (File.Exists(temppath)) {
                        if (AllowExt.Contains(file.Extension.Replace(".", "").ToLower())) {
                            // 圖片才需要比對, 若相同則不複製
                            var desImg = new Bitmap(temppath);
                            var sourceImg = new Bitmap(Path.Combine(sourceDirName, file.Name));
                            if (!IsSameImg(sourceImg, desImg)) {
                                sourceImg.Dispose();
                                sourceImg = null;
                                desImg.Dispose();
                                desImg = null;
                                file.CopyTo(temppath, true);
                                copyFileCnt++;
                            }
                        }
                    } else {
                        file.CopyTo(temppath);
                        copyFileCnt++;
                    }

                    if ((i > 1 && i % 50 == 0 && i != files.Count() - 1) || i == files.Count() - 1) {
                        Log($"已複製{copyFileCnt}/{files.Count()}張... 耗時: {string.Format("{0}:{1}", Math.Floor(sw.Elapsed.TotalMinutes), sw.Elapsed.ToString("ss\\.ff"))}");
                    }
                }
                sw.Stop();
                var ts = sw.Elapsed;

                if (copyFileCnt > 0) {
                    Msg.AppendLine(Msg.Length == 0 ? $"{DateTime.Today}備份相片" : "");
                    Msg.AppendLine($"{Emoji.info} /{sourceDirName.Substring(sourceDirName.IndexOf("\\Upload")).Replace("\\", "/")}");
                    Msg.AppendLine($"{Emoji.star} {copyFileCnt}/{files.Count()}張, 耗時 {string.Format("{0}:{1}", Math.Floor(ts.TotalMinutes), ts.ToString("ss\\.ff"))}");
                }

                //Log($"完成... 結果: {copyFileCnt}/{files.Count()}張");
                Log($"耗時{string.Format("{0}:{1}", Math.Floor(ts.TotalMinutes), ts.ToString("ss\\.ff"))}");

                // If copying subdirectories, copy them and their contents to new location.
                if (copySubDirs) {
                    foreach (DirectoryInfo subdir in dirs) {
                        string temppath = Path.Combine(destDirName, subdir.Name);
                        DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                    }
                }
            } catch (Exception e) {
                Log(e.Message);
            }
        }

        /// <summary> 
        /// 判斷圖片是否一致 
        /// </summary> 
        /// <param name="img">圖片一</param> 
        /// <param name="bmp">圖片二</param> 
        /// <returns>是否一致</returns> 
        public static bool IsSameImg(Bitmap img, Bitmap bmp)
        {
            //大小一致 
            if (img.Width == bmp.Width && img.Height == bmp.Height) {

                //將圖片一鎖定到記憶體 
                System.Drawing.Imaging.BitmapData imgData_i = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                IntPtr ipr_i = imgData_i.Scan0;
                int length_i = imgData_i.Width * imgData_i.Height * 3;
                byte[] imgValue_i = new byte[length_i];
                Marshal.Copy(ipr_i, imgValue_i, 0, length_i);
                img.UnlockBits(imgData_i);

                //將圖片二鎖定到記憶體 
                BitmapData imgData_b = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                IntPtr ipr_b = imgData_b.Scan0;
                int length_b = imgData_b.Width * imgData_b.Height * 3;
                byte[] imgValue_b = new byte[length_b];
                Marshal.Copy(ipr_b, imgValue_b, 0, length_b);
                img.UnlockBits(imgData_b);
                //長度不相同 
                if (length_i != length_b) {
                    img.Dispose();
                    img = null;
                    bmp.Dispose();
                    bmp = null;
                    return false;
                } else {
                    //迴圈判斷值 
                    for (int i = 0; i < length_i; i++) {
                        //不一致 
                        if (imgValue_i[i] != imgValue_b[i]) {
                            img.Dispose();
                            img = null;
                            bmp.Dispose();
                            bmp = null;
                            return false;
                        }
                    }
                    img.Dispose();
                    img = null;
                    bmp.Dispose();
                    bmp = null;
                    return true;
                }
            } else {
                return false;
            }
        }

        private static void WatcherStrat(string path, string filter)
        {
            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = path;
            watcher.IncludeSubdirectories = true;
            //watcher.Filter = filter;

            watcher.Changed += new FileSystemEventHandler(OnProcess);
            watcher.Created += new FileSystemEventHandler(OnProcess);
            watcher.Deleted += new FileSystemEventHandler(OnProcess);
            watcher.Renamed += new RenamedEventHandler(OnRenamed);

            watcher.EnableRaisingEvents = true;
        }

        private static void OnProcess(object source, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Created) {
                OnCreated(source, e);

            } else if (e.ChangeType == WatcherChangeTypes.Changed) {
                OnChanged(source, e);

            } else if (e.ChangeType == WatcherChangeTypes.Deleted) {
                OnDeleted(source, e);

            }
        }

        private static void OnCreated(object source, FileSystemEventArgs e)
        {
            Log($"--- 新增檔案 {DateTime.Now.ToString("yyyy/MM/dd")} ---");
            var filePath = e.FullPath.Substring(e.FullPath.IndexOf("\\Upload")).Substring(7);
            var destPath = $"{_Destination}{filePath}";

            try {
                Directory.CreateDirectory(Path.GetDirectoryName(destPath));
            } catch (Exception) {

            }

            File.Copy(e.FullPath, destPath);

            if (File.Exists(destPath)) {
                Log($"新增成功並同步 ! {destPath}");
            } else {
                Log($"新增失敗 ! {e.FullPath}");
            }

        }

        private static void OnChanged(object source, FileSystemEventArgs e)
        {


        }

        private static void OnDeleted(object source, FileSystemEventArgs e)
        {
            var filePath = e.FullPath.Substring(e.FullPath.IndexOf("\\Upload")).Substring(7);
            var destPath = $"{_Destination}{filePath}";
            var backupPath = $"{_BackUp}{filePath}";

            try {
                Directory.CreateDirectory(Path.GetDirectoryName(backupPath));
                Directory.CreateDirectory(Path.GetDirectoryName(destPath));
            } catch (Exception) {

            }

            Log($"--- 刪除檔案 {DateTime.Now.ToString("yyyy/MM/dd")} ---");

            Log($"備份刪除檔案...至 {backupPath}");
            File.Copy(destPath, backupPath);

            File.Delete(destPath);

            Log(File.Exists(destPath) ? "" : $"刪除檔案並同步 !");
        }

        private static void OnRenamed(object source, RenamedEventArgs e)
        {
            var filePath = e.FullPath.Substring(e.FullPath.IndexOf("\\Upload")).Substring(7);
            var oldFilePath = e.OldFullPath.Substring(e.OldFullPath.IndexOf("\\Upload")).Substring(7);
            var sourcePath = e.FullPath;
            var destPath = $"{_Destination}{oldFilePath}";
            Log($"--- 檔名變動 {DateTime.Now.ToString("yyyy/MM/dd")} ---");

            File.Delete(destPath);

            Log(File.Exists(destPath) ? "" : $"目的檔案已刪除 !");

            destPath = $"{_Destination}{filePath}";

            Log($"{sourcePath} 複製到 {destPath}...");

            File.Copy(sourcePath, destPath, true);

            Log(File.Exists(destPath) ? "複製檔案並同步  !" : "");
        }


    }
}
