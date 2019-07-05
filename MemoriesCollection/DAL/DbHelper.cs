using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace MemoriesCollection.DAL
{
    public static class DbHelper
    {

        /// <summary>
        /// 修正 SQL Injection
        /// </summary>
        /// <param name="value">欄位值</param>
        /// <returns></returns>
        public static string FixSQL(this string value)
        {
            if (value == null) {
                return "";
            } else {
                return value.Replace("'", "''");
            }
        }

        public static SqlConnection db
        {
            get
            {
                return new SqlConnection(ConfigurationManager.ConnectionStrings["TESTDB"].ToString());
            }
        }

        public static SqlConnection GetDb(string db)
        {
            return new SqlConnection(ConfigurationManager.ConnectionStrings[db].ToString());
        }
    }
}