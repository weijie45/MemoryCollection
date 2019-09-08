using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using static BackUp.Components.Common;

namespace BackUp
{
    class Program
    {
        public static StringBuilder Msg = new StringBuilder();
        public static string[] AllowExt = new string[] { "jpg", "jpeg", "png", "bmp", "gif", "tiff" };

        static void Main(string[] args)
        {
            string source = @"D:\Publish\Memory\Upload";
            string destination = @"E:\BackUp_Photo";

            try {
                Directory.CreateDirectory(destination);
            } catch (Exception) {

            }

            DirectoryCopy(source, destination, true);

            if (Msg.Length > 0) BackUp.Components.Common.LineSend(Msg.ToString());
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
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
            foreach (FileInfo file in files) {
                string temppath = Path.Combine(destDirName, file.Name);
                if (File.Exists(temppath)) {
                    if (AllowExt.Contains(file.Extension.Replace(".", "").ToLower())) {
                        // 圖片才需要比對, 若相同則不複製
                        var desImg = new Bitmap(temppath);
                        var sourceImg = new Bitmap(Path.Combine(sourceDirName, file.Name));
                        if (!IsSameImg(sourceImg, desImg)) {
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
            }
            sw.Stop();

            if (files.Count() > 0) {
                Msg.AppendLine(Msg.Length == 0 ? "備份相片" : "");
                Msg.AppendLine($"{Emoji.info}資料夾 /{Path.GetFullPath(sourceDirName).Replace(Path.GetPathRoot(sourceDirName), "").Replace("\\", "/")}");
                if (sw.ElapsedMilliseconds > 60000) {
                    Msg.AppendLine($"耗時{sw.Elapsed.Minutes}分鐘, {Emoji.star} {copyFileCnt}/{files.Count()}(複製/全部)");
                } else {
                    Msg.AppendLine($"{Emoji.star}耗時{sw.Elapsed.Seconds}秒, {copyFileCnt}/{files.Count()}(複製/全部)");
                }
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs) {
                foreach (DirectoryInfo subdir in dirs) {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
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
                    return false;
                } else {
                    //迴圈判斷值 
                    for (int i = 0; i < length_i; i++) {
                        //不一致 
                        if (imgValue_i[i] != imgValue_b[i]) {
                            return false;
                        }
                    }
                    return true;
                }
            } else {
                return false;
            }
        }

    }
}
