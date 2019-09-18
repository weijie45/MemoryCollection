using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using MemoriesCollection.Function.Extensions;

namespace MemoriesCollection.Function.Common
{
    /// <summary>
    /// Key / Value 資料輔助功能
    /// </summary>
    public class Key
    {
        public static string DataKey = "12345678";

        public static string escapeJq(string selector)
        {
            return Regex.Replace(selector, @"[\W_]+", "", RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// 依 Key/Value 陣列表取出指定 Key 的值
        /// </summary>
        /// <param name="dict">陣列表</param>
        /// <param name="key">Key 名稱</param>
        /// <returns></returns>
        public static object Dict(ref Dictionary<string, object> dict, string key)
        {
            object value = "";
            if (dict != null && key != null && dict.Count > 0 && dict.ContainsKey(key)) {
                value = dict[key];
            }
            return value;
        }

        /// <summary>
        /// 依 Key/Value 陣列表取出指定 Key 的值
        /// </summary>
        /// <param name="dict">陣列表</param>
        /// <param name="key">Key 名稱</param>
        /// <returns></returns>
        public static string Dict(ref Dictionary<string, string> dict, string key)
        {
            string value = "";
            if (dict != null && key != null && dict.Count > 0 && dict.ContainsKey(key)) {
                value = dict[key];
            }
            return value;
        }

        /// <summary>
        /// 依 Key/Value 陣列表取出指定 Key 的值
        /// </summary>
        /// <param name="dict">陣列表</param>
        /// <param name="key">Key 名稱</param>
        /// <returns></returns>
        public static object Dict(ref dynamic dict, string key)
        {
            var d = (Dictionary<string, object>)dict;
            return Dict(ref d, key);
        }

        /// <summary>
        /// 純數值格式化
        /// </summary>
        /// <param name="obj">原數值</param>
        /// <param name="numDigitsAfterDecimal">顯示小數點後幾位數</param>
        /// <param name="unitChar">單位文字</param>
        /// <param name="isKeepZero">非數字顯示0</param>
        /// <param name="isRmZero">移除小數點後不必要的0</param>
        /// <param name="isKeepDash">數字0顯示---</param>
        /// <returns></returns>
        public static string FormatNumber(object obj, int numDigitsAfterDecimal = 0, string unitChar = "", bool isKeepZero = false, bool isRmZero = false, bool isKeepDash = false)
        {
            if (obj != null && obj.IsNumeric()) {
                var numDigits = new string('0', numDigitsAfterDecimal);
                var newNum = string.Format("{0:N" + numDigitsAfterDecimal + "}", double.Parse(obj.ToString().Replace(",", "").Replace(" ", "")));
                if (numDigitsAfterDecimal > 0 && isRmZero) {
                    if (Math.Abs(double.Parse(newNum)) == 0) {
                        // 浮點數判斷0
                        newNum = "0";
                    } else {
                        var dotPos = newNum.IndexOf(".") + 1;
                        if (dotPos > 0) {
                            var dotNum = Int32.Parse(newNum.Substring(dotPos)); // 小數點後數字
                            if (dotNum != 0) {
                                newNum = newNum.Substring(0, dotPos) + dotNum;
                            } else {
                                newNum = newNum.Substring(0, dotPos - 1);
                            }
                        }
                    }
                }
                if (isKeepDash && Math.Abs(double.Parse(newNum)) == 0) {
                    return "---";
                }
                return $"{newNum}{unitChar}";
            } else {
                return isKeepZero ? "0" : "---";
            }
        }

        /// <summary>
        /// 修正 double 數值
        /// </summary>
        /// <param name="num">原始資料</param>
        /// <returns></returns>
        public static double FixNum(object num, int divNum = 1)
        {
            return num.FixNum() / divNum;
        }

        /// <summary>
        /// 去空白
        /// </summary>
        /// <param name="value">原始資料</param>
        /// <returns></returns>
        public static string Trim(object value)
        {
            return value != null ? value.ToString().Trim(new char[] { ' ', '　' }) : "";
        }

        /// <summary>
        /// 判斷是否為負數, 並修改顯示方式
        /// </summary>
        /// <param name="num">原始數字</param>
        /// <param name="cssStyle">加入css class字串</param>
        /// <returns></returns>
        public static string IsNegative(object num, int numDigitsAfterDecimal = 0, string unitChar = "", bool isKeepZero = false, bool cssStyle = false)
        {
            string result = "";
            if (num.IsNumeric() && num.ToString().StartsWith("-")) {
                result = cssStyle ? "text-red" : $"({Key.FormatNumber(num.ToString().Substring(1), numDigitsAfterDecimal, unitChar, isKeepZero)})";
            } else {
                result = cssStyle ? "" : Key.FormatNumber(num, numDigitsAfterDecimal, unitChar, isKeepZero);
            }
            return result;
        }

        /// <summary>
        /// 去除英數字，僅取中文名稱
        /// </summary>
        /// <param name="value">原始資料</param>
        /// <returns></returns>
        public static string ChtName(object value)
        {
            var name = "";
            if (value == null) {
                return name;
            }
            var match = Regex.Match(value.ToString(), "^[a-z0-9０-９Ａ-Ｚ ]*(?<Data>.*)");
            if (match.Success) {
                name = match.Groups["Data"].Value.Trim(new char[] { ' ', '　' });
            }
            return name;
        }

        /// <summary>
        /// 取文字
        /// </summary>
        /// <param name="value">原始資料</param>
        /// <returns></returns>
        public static string FixNull(object value)
        {
            return value != null ? value.ToString() : "";
        }

        /// <summary>
        /// 判斷是否數值
        /// </summary>
        /// <param name="s">原始資料</param>
        /// <returns></returns>
        public static bool IsNumeric(object s)
        {
            return s.IsNumeric();
        }

        /// <summary>
        /// 資料內是否包含指定的檢查字串
        /// </summary>
        /// <param name="obj">原資料</param>
        /// <param name="checkId">檢查字串集</param>
        /// <param name="isUpper">是否轉成大寫</param>
        /// <param name="isEqual">以相同值做判斷</param>
        /// <returns></returns>
        public static bool Contains(object obj, string checkId, bool isUpper = false, bool isEqual = false)
        {
            var value = isUpper ? obj.ToString().ToUpper() : obj.ToString();
            var ids = checkId.Split(',');
            return isEqual ? ids.Any(x => value == x) : ids.Any(x => value.IndexOf(x) != -1);
        }

        ///<summary>
        ///字串轉全形
        ///</summary>
        ///<param name="input">任一字元串</param>
        ///<returns></returns>
        public static string ToFullChar(string input)
        {
            if (string.IsNullOrEmpty(input)) {
                return "";
            }
            //半形轉全形：
            char[] c = input.ToCharArray();
            for (int i = 0; i < c.Length; i++) {
                //全形空格為12288，半形空格為32
                if (c[i] == 32) {
                    c[i] = (char)12288;
                    continue;
                }
                //其他字元半形(33-126)與全形(65281-65374)的對應關係是：均相差65248
                if (c[i] < 127)
                    c[i] = (char)(c[i] + 65248);
            }
            return new string(c);
        }

        ///<summary>
        ///字串轉半形
        ///</summary>
        ///<paramname="input">任一字元串</param>
        ///<returns></returns>
        public static string ToHalfChar(string input)
        {
            if (string.IsNullOrEmpty(input)) {
                return "";
            }
            char[] c = input.ToCharArray();
            for (int i = 0; i < c.Length; i++) {
                if (c[i] == 12288) {
                    c[i] = (char)32;
                    continue;
                }
                if (c[i] > 65280 && c[i] < 65375)
                    c[i] = (char)(c[i] - 65248);
            }
            return new string(c);
        }

        /// <summary>
        /// 格式化民國日期
        /// </summary>
        /// <param name="date">原始日期資料</param>
        /// <param name="useSlash">是否使用 / 分隔字元</param>
        /// <returns></returns>
        public static string ChineseDate(object date, bool useSlash = true)
        {
            return date.ChineseDate(useSlash);
        }

        /// <summary>
        /// 格式化民國日期
        /// </summary>
        /// <param name="date">原始日期資料</param>
        /// <param name="useSlash">是否使用 / 分隔字元</param>
        /// <returns></returns>
        public static string EnglishDate(object date, bool useSlash = true)
        {
            return date.EnglishDate(useSlash);
        }

        /// <summary>
        /// 取得完整年月日時分秒亳秒
        /// </summary>
        /// <param name="date">日期</param>
        /// <returns></returns>
        public static string FullDateTime(object date)
        {
            return DateTime.Parse(date.ToString()).ToString("yyyy/MM/dd HH:mm:ss.fff");
        }

        /// <summary>
        /// Big5 Url 編碼
        /// </summary>
        /// <param name="obj">原資料</param>
        /// <returns></returns>
        public static string Big5UrlEncode(object obj)
        {
            var str = obj.FixNull();
            if (str == "") {
                return "";
            }
            var big5Enc = Encoding.GetEncoding("BIG5");
            return HttpUtility.UrlEncode(str, big5Enc);
        }

        /// <summary>
        /// 使用 AES 解密
        /// </summary>
        /// <param name="encrypted_">AES 加密文字</param>
        /// <returns></returns>
        public static string Decrypt(object encrypted_)
        {
            var encrypted = encrypted_.FixNull().Replace(" ", "+");
            if (encrypted == "") {
                return "";
            }
            try {
                byte[] toEncryptArray = Convert.FromBase64String(encrypted);
                //var key = Encoding.UTF8.GetBytes(DataKey);
                byte[] key = Encoding.ASCII.GetBytes("HeHeHaHaWeiWei00");
                RijndaelManaged rm = new RijndaelManaged();
                rm.Key = key;
                rm.IV = key;
                rm.FeedbackSize = 128;
                rm.Mode = CipherMode.CBC;
                rm.Padding = PaddingMode.PKCS7;

                ICryptoTransform cTransform = rm.CreateDecryptor();
                byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
                return Encoding.UTF8.GetString(resultArray);
            } catch {
                return "";
            }
        }

        /// <summary>
        /// 使用 AES 加密
        /// </summary>
        /// <param name="original_">原始文字</param>
        /// <returns></returns>
        public static string Encrypt(object original_)
        {
            var original = "";
            if (original_ is String) {
                original = original_ + "";
            } else {
                original = JsonConvert.SerializeObject(original_);
            }
            if (original == "") {
                return "";
            }
            try {
                byte[] toEncryptArray = Encoding.UTF8.GetBytes(original);
                //var key = Encoding.UTF8.GetBytes(DataKey);
                byte[] key = Encoding.ASCII.GetBytes("HeHeHaHaWeiWei00");

                RijndaelManaged rm = new RijndaelManaged();
                rm.Key = key;
                rm.IV = key;
                rm.FeedbackSize = 128;
                rm.Mode = CipherMode.CBC;
                rm.Padding = PaddingMode.PKCS7;

                ICryptoTransform cTransform = rm.CreateEncryptor();
                byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

                return Convert.ToBase64String(resultArray, 0, resultArray.Length);
            } catch (Exception e) {
                Log.ErrLog(e);
                return "";
            }
        }

        /// <summary>
        /// Base64 解碼
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string FromBase64(string data)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(data));
        }

        /// <summary>
        /// FTP 檔案資訊編碼
        /// </summary>
        /// <param name="rootFolder">主要根目錄</param>
        /// <param name="folderTitle">次要標題目錄</param>
        /// <param name="key">Key 編號</param>
        /// <param name="fileDescOrExt">檔案描述(含副檔名)</param>
        /// <returns></returns>
        public static string FileKey(object rootFolder, object folderTitle, object key, object fileDescOrExt)
        {
            return Encrypt($"{rootFolder}|{folderTitle}|{key}|{fileDescOrExt}");
            //return $"{rootFolder}|{folderTitle}|{key}|{fileDescOrExt}";
        }
        public static DateTime Now => DateTime.Now;
        public static string Today => DateTime.Now.ToString("yyyyMMdd");
    }
}
