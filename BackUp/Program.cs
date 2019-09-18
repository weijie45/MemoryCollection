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
using System.Security.Cryptography;
using System.Text;
using System.Threading;
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

            Log($"{_Source} 同步至 {_Destination}");
            Log("檔案同步中...");

            Stopwatch sw = new Stopwatch();
            sw.Start();
            DirectoryCopy(_Source, _Destination, true);
            sw.Stop();

            var ts = sw.Elapsed;
            Msg.AppendLine($"{Emoji.shinyStar}總耗時{string.Format("{0}:{1}", Math.Floor(ts.TotalMinutes), ts.ToString("ss\\.ff"))}");
            LineSend(Msg.ToString());

            Log("檔案同步完成...");
            Log($"  總耗時{string.Format("{0}:{1}", Math.Floor(ts.TotalMinutes), ts.ToString("ss\\.ff"))}");


            Console.WriteLine("資料夾監測中...");
            try {
                WatcherStrat(_Source, "");
            } catch (Exception e) {
                LineSend("WatcherStrat\t" + e.Message);
            }

            bool isContinue = true;
            while (isContinue) {
                Console.WriteLine("離開(999), 重新同步檔案(000)：");
                string input = Console.ReadLine();
                if (input.Equals("999", StringComparison.CurrentCultureIgnoreCase))
                    isContinue = false;
                if (input.Equals("000", StringComparison.CurrentCultureIgnoreCase)) {
                    DirectoryCopy(_Source, _Destination, true);
                }
            }

            SetConsoleCtrlHandler(t =>
            {
                // 這邊簡單的顯示導致視窗關閉的來源是什麼
                LineSend("Console Close\t" + t.ToString());
                Log(t.ToString());
                // 在這裡處理視窗關閉前想要執行的程式碼

                // 返回 false 將事件交回原處理函式執行正常關閉
                return false;
            },
           true);

            Console.WriteLine("按下任意鍵結束程式");

            Console.ReadKey();

            Console.WriteLine("程式正常結束");
        }

        [DllImport("Kernel32")]
        public static extern bool SetConsoleCtrlHandler(ConsoleCtrlDelegate handler, bool add);

        public delegate bool ConsoleCtrlDelegate(CtrlTypes ctrlType);

        public enum CtrlTypes
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT,
            CTRL_CLOSE_EVENT,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT
        }


        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            try {
                StringBuilder sb = new StringBuilder();
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
                double totalRec = files.Count();
                for (int i = 0; i < files.Count(); i++) {
                    FileInfo file = files[i];
                    string fileName = file.Name;
                    //foreach (FileInfo file in files) {
                    string desPath = Path.Combine(destDirName, fileName);
                    string sourcePath = Path.Combine(sourceDirName, fileName);

                    if (File.Exists(desPath)) {
                        if (AllowExt.Contains(file.Extension.Replace(".", "").ToLower())) {

                            //圖片才需要比對, 若相同則不複製
                            if (!IsSameImg(sourcePath, desPath)) {
                                copyFileCnt++;
                                Copy(sourcePath, desPath, true);
                                // 備份原始檔案
                                var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                                var backupPath = $"{_BackUp}{desPath.Replace(Path.GetDirectoryName(destDirName), "")}".Replace(fileNameWithoutExt, $"{fileNameWithoutExt}_{DateTime.Now.ToString("yyyyMMddHHmmssfff")}");
                                Copy(desPath, backupPath, true);
                                Log($"\t{fileName}與原檔案不相同, 備份原檔案,並更新檔案 ...");
                            }

                            Console.SetCursorPosition(0, Console.CursorTop);   // - Move cursor
                            Console.Write($"\t比對進度... {(((double)i / (double)totalRec) * 100).ToString("0.000")}%");
                        }
                    } else {
                        Copy(sourcePath, desPath, true);
                        copyFileCnt++;
                    }

                    if (i == totalRec - 1) {
                        if (copyFileCnt > 0) {
                            Log($"\t已複製{copyFileCnt}張...");
                        }
                        //Log($"共耗時: {string.Format("{0}:{1}", Math.Floor(sw.Elapsed.TotalMinutes), sw.Elapsed.ToString("ss\\.ff"))}");
                    }

                }
                sw.Stop();
                var ts = sw.Elapsed;

                if (copyFileCnt > 0) {
                    Msg.AppendLine(Msg.Length == 0 ? $"{DateTime.Today}備份相片" : "");
                    Msg.AppendLine($"{Emoji.info} /{sourceDirName.Substring(sourceDirName.IndexOf("\\Upload")).Replace("\\", "/")}");
                    Msg.AppendLine($"{Emoji.star} {copyFileCnt}/{totalRec}張, 耗時 {string.Format("{0}:{1}", Math.Floor(ts.TotalMinutes), ts.ToString("ss\\.ff"))}");
                }

                //Log($"完成... 結果: {copyFileCnt}/{files.Count()}張");
                Log($"\t耗時{string.Format("{0}:{1}", Math.Floor(ts.TotalMinutes), ts.ToString("ss\\.ff"))}");

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

        public static void ReleaseMem(Bitmap bmp)
        {
            bmp.Dispose();
            bmp = null;
        }

        public static bool IsSameImg(string sourcePath, string destPath)
        {
            var bmp1 = new Bitmap(destPath);
            var bmp2 = new Bitmap(sourcePath);
            if (bmp1.Size != bmp2.Size) {
                ReleaseMem(bmp1);
                ReleaseMem(bmp2);
                return false;
            } else {
                using (MD5 md5Hash = MD5.Create()) {

                    //create instance or System.Drawing.ImageConverter to convert
                    //each image to a byte array
                    ImageConverter converter = new ImageConverter();
                    //create 2 byte arrays, one for each image
                    byte[] imgBytes1 = new byte[1];
                    byte[] imgBytes2 = new byte[1];

                    //convert images to byte array
                    imgBytes1 = (byte[])converter.ConvertTo(bmp1, imgBytes2.GetType());
                    imgBytes2 = (byte[])converter.ConvertTo(bmp2, imgBytes1.GetType());


                    byte[] hash1 = md5Hash.ComputeHash(imgBytes1);
                    byte[] hash2 = md5Hash.ComputeHash(imgBytes2);

                    if (hash1.SequenceEqual(hash2) == false) {
                        ReleaseMem(bmp1);
                        ReleaseMem(bmp2);
                        return false;
                    } else {
                        ReleaseMem(bmp1);
                        ReleaseMem(bmp2);
                        return true;
                    }

                    //Compare the hash values
                    //for (int i = 0; i < hash1.Length && i < hash2.Length; i++) {
                    //    if (hash1[i] != hash2[i]) {
                    //        ReleaseMem(bmp1);
                    //        ReleaseMem(bmp2);
                    //        return false;
                    //    }
                    //}
                }
            }
        }

        public static bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try {
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            } catch (IOException) {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            } finally {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
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

            var filePath = e.FullPath.Substring(e.FullPath.IndexOf("\\Upload")).Substring(7);
            var destPath = $"{_Destination}{filePath}";

            try {
                Directory.CreateDirectory(Path.GetDirectoryName(destPath));
            } catch (Exception) {

            }
            try {
                while (IsFileLocked(new FileInfo(e.FullPath))) {
                    Thread.Sleep(500);
                }

                Copy(e.FullPath, destPath);
                Log(File.Exists(destPath) ? $"\tCreate Successful  ! {destPath}" : $"\t[ERROR] tCreate Failed ! {e.FullPath}");

            } catch (Exception e1) {
                Log("OnCreated: " + e1.Message);
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

            Copy(destPath, backupPath);
            Log((File.Exists(destPath) ? $"\t BackUp Successfule !" : "\t[ERROR] BackUp Failed !") + backupPath);

            File.Delete(destPath);
            Log((!File.Exists(destPath) ? "\t Delete Successful  !" : "\t[ERROR] Delete Failed !") + destPath);
        }

        private static void OnRenamed(object source, RenamedEventArgs e)
        {

            var filePath = e.FullPath.Substring(e.FullPath.IndexOf("\\Upload")).Substring(7);
            var oldFilePath = e.OldFullPath.Substring(e.OldFullPath.IndexOf("\\Upload")).Substring(7);
            var sourcePath = e.FullPath;
            var destPath = $"{_Destination}{oldFilePath}";


            File.Delete(destPath);
            Log(File.Exists(destPath) ? "\t[ERROR] 目的檔案尚未刪除 !" : "");

            destPath = $"{_Destination}{filePath}";

            Copy(sourcePath, destPath, true);
            Log((File.Exists(destPath) ? "\tReName Successful  !" : "\t[ERROR] ReName Failed !") + $"{e.OldName} -> {e.Name}");
        }

        private static void Copy(string sourcePath, string destPath, bool overwrite = true)
        {
            var sourceDirPath = Path.GetDirectoryName(sourcePath);
            var destDirPath = Path.GetDirectoryName(destPath);
            if (!Directory.Exists(sourceDirPath)) Directory.CreateDirectory(sourceDirPath);
            if (!Directory.Exists(destDirPath)) Directory.CreateDirectory(destDirPath);
            File.Copy(sourcePath, destPath, overwrite);
        }


    }
}
