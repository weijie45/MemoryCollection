using Newtonsoft.Json;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MemoriesCollection.Function.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// 將 json 轉換為指定類型的資料
        /// </summary>
        /// <typeparam name="T">指定類型</typeparam>
        /// <param name="obj">json 資料</param>
        /// <returns></returns>
        public static T Json<T>(this string obj)
        {
            return JsonConvert.DeserializeObject<T>(obj);
        }

        /// <summary>
        /// 資料內是否包含指定的檢查字串
        /// </summary>
        /// <param name="obj">原資料</param>
        /// <param name="checkId">檢查字串集</param>
        /// <param name="isUpper">是否轉成大寫</param>
        /// <returns></returns>
        public static bool Contains(this string obj, string checkId, bool isUpper = false)
        {
            var value = isUpper ? obj.ToUpper() : obj;
            return checkId.Split(',').Any(x => value.IndexOf(x) != -1);
        }

        /// <summary>
        /// 補齊英數文字
        /// </summary>
        /// <param name="data">原資料</param>
        /// <param name="maxLen">最大長度</param>
        /// <param name="isPadRight">是否往右補齊空白</param>
        /// <returns></returns>
        public static string PadText(this string data, int maxLen, bool isPadRight = false)
        {
            if (data == "") {
                return new string(' ', maxLen);
            }
            var encBIG5 = Encoding.GetEncoding("BIG5");
            var org = encBIG5.GetBytes(data).Take(maxLen).ToArray();
            var append = new string(' ', maxLen - org.Length);
            return isPadRight ? encBIG5.GetString(org) + append : append + encBIG5.GetString(org);
        }

        /// <summary>
        /// 指定替換字串
        /// </summary>
        /// <typeparam name="string">字串</typeparam>
        /// <param name="value">原字串</param>
        /// <param name="start">起點</param>
        /// <param name="length">長度</param>
        /// <param name="criteria">替換字串</param>
        /// <returns></returns>
        public static string Mask(this string value, int start, int length, string criteria)
        {
            value = value.FixNull();
            string result = "";
            int end = start + length;
            if (value.Length < end) end = value.Length - 1;

            for (int i = 0; i < value.Length; i++) {
                if (i >= start && i < end) {
                    result = result + criteria;
                } else {
                    result = result + value.Substring(i, 1);
                }
            }

            return result;
        }

        public static int ToInt(this string value)
        {
            if (value.IsNumeric()) {
                return Convert.ToInt32(value);
            }
            return 0;
        }

        public static string Mid(this string s, int start, int length)
        {
            return s.Substring(start, length);
        }
    }
}
