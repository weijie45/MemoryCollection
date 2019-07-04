using System;
using System.Linq;

namespace HeHeMemory.Function.Extensions
{
    public static class ArrayExtensions
    {
        /// <summary>
        /// 串接資料
        /// </summary>
        /// <param name="array">陣列</param>
        /// <param name="joinString">串接字串</param>
        /// <returns></returns>
        public static string Join(this int[] array, string joinString)
        {
            return string.Join(joinString, array.Select(x => x.ToString()).ToArray());
        }

        /// <summary>
        /// 串接資料
        /// </summary>
        /// <param name="array">陣列</param>
        /// <param name="joinString">串接字串</param>
        /// <returns></returns>
        public static string Join(this object[] array, string joinString)
        {
            return string.Join(joinString, array);
        }

        /// <summary>
        /// 串接資料
        /// </summary>
        /// <param name="array">陣列</param>
        /// <param name="joinString">串接字串</param>
        /// <returns></returns>
        public static string Join(this string[] array, string joinString)
        {
            return string.Join(joinString, array);
        }

        /// <summary>
        /// 是否陣列中包含指定的字元
        /// </summary>
        /// <param name="array">陣列</param>
        /// <param name="chkString">指定的字元</param>
        /// <returns></returns>
        public static bool Contains(this char[] array, string chkString)
        {
            var chkList = chkString.ToCharArray();
            return array.Any(x => Array.IndexOf<char>(chkList, x) != -1);
        }
    }
}
