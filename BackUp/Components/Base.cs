using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BackUp.Components
{
    public static class Base
    {
        static SqlConnection ConnSQL;
        static string SqlConnectionString = Common.AppSettings("Memory");
        static string DataBase_Name;


        public static bool Init()
        {
            bool isPass = false;
            if (SqlConnectionString != "") {
                string sSQL = "SELECT db_name() AS DataBaseName  ";
                DataBase_Name = QueryString(sSQL);
                isPass = DataBase_Name != "";
            }

            return isPass;
        }

        public static void Open()
        {
            ConnSQL.Open();
        }

        public static string QueryString(string sql)
        {
            ConnSQL = new SqlConnection(SqlConnectionString);
            Open();
            SqlCommand command = new SqlCommand(sql, ConnSQL);
            command.CommandTimeout = 6000;
            SqlDataReader dr = command.ExecuteReader();

            string sReturn = "";
            try {
                if (dr.Read() && dr.HasRows) {
                    sReturn = dr[0].ToString();
                }
            } catch (Exception e) {
                ErrHandle("QueryString", e);
            }
            Close();
            return sReturn;
        }

        public static DataSet QueryDataSet(string sql, string tableName)
        {
            ConnSQL = new SqlConnection(SqlConnectionString);
            Open();
            SqlDataAdapter adapter = new SqlDataAdapter(sql, ConnSQL);
            DataSet ds = new DataSet();
            adapter.Fill(ds, tableName);

            Close();
            return ds;
        }

        public static DataTable GatDataTable(string sql)
        {
            SqlConnection connSQL = new SqlConnection(SqlConnectionString);
            SqlDataAdapter adpData = new SqlDataAdapter(sql, connSQL);
            adpData.SelectCommand.CommandTimeout = 6000;
            DataTable dtReturn = new DataTable();

            try {
                adpData.Fill(dtReturn);
            } catch (Exception e) {
                ErrHandle("GatDataTable", e);
            }

            connSQL.Close();
            connSQL.Dispose();

            return dtReturn;
        }

        public static int UpdData(string sql)
        {
            SqlConnection connSQL = new SqlConnection(SqlConnectionString);
            connSQL.Open();
            SqlCommand cmd = new SqlCommand(sql, connSQL);

            int res = 0;
            try {
                res = cmd.ExecuteNonQuery();
            } catch (Exception e) {
                ErrHandle("UpdData", e);
            }

            connSQL.Close();
            connSQL.Dispose();
            return res;
        }

        public static void Close()
        {
            ConnSQL.Close();
            ConnSQL.Dispose();
        }

        public class DBTable
        {
            public DataTable dt = null;
            public DataRow dr = null;
            public string Error = "";
            private SqlConnection connSQL = null;
            private SqlDataAdapter adapt = null;
            public bool IsAutoNewRow = true;
            public bool KeepLive = false;
            public bool IsNewRow = false;
            private Dictionary<string, object> _dict = null;

            public DBTable()
            {
                KeepLive = true;
            }

            public DBTable(string sSQL)
            {
                Init(sSQL, SqlConnectionString);
            }

            public DBTable(string sSQL, bool bIsAutoNewRow)
            {
                IsAutoNewRow = bIsAutoNewRow;
                Init(sSQL, SqlConnectionString);
            }

            public DBTable(string sSQL, string sConnectionString)
            {
                Init(sSQL, sConnectionString);
            }

            public DBTable(string sSQL, string sConnectionString, bool bIsAutoNewRow)
            {
                IsAutoNewRow = bIsAutoNewRow;
                Init(sSQL, sConnectionString);
            }

            private void Init(string sSQL, string sConnectionString)
            {
                if (connSQL == null) {
                    connSQL = new SqlConnection(sConnectionString);
                    connSQL.Open();
                }
                adapt = new SqlDataAdapter(sSQL, connSQL);
                new SqlCommandBuilder(adapt);
                adapt.MissingSchemaAction = MissingSchemaAction.AddWithKey;

                dt = new DataTable();
                adapt.Fill(dt);

                object val;
                _dict = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns) {
                    if (col.AllowDBNull || (dt.PrimaryKey.Length > 0 && dt.PrimaryKey.Contains(col) && col.AutoIncrement)) {
                        continue;
                    } else if (!col.AllowDBNull) {
                        switch (col.DataType.Name) {
                            case "String":
                                val = "";
                                break;
                            case "Char":
                                val = ' ';
                                break;
                            case "Boolean":
                                val = false;
                                break;
                            case "DateTime":
                                val = DateTime.Today;
                                break;
                            default:
                                val = 0;
                                break;
                        }
                        _dict[col.ColumnName] = val;
                    }
                }

                IsNewRow = false;
                dr = null;
                if (dt.Rows.Count == 1) {
                    dr = dt.Rows[0];
                } else if (dt.Rows.Count == 0 && IsAutoNewRow) {
                    NewRow();
                    IsNewRow = true;
                }
            }

            public string SQL
            {
                set
                {
                    if (value != null && value != "") {
                        Init(value, SqlConnectionString);
                    }
                }
            }

            public string PrimaryKey
            {
                set
                {
                    if (value != null && value != "") {
                        var aColumn = Regex.Split(value, "[ ]*,[ ]*");
                        var dc = new DataColumn[aColumn.Length];
                        for (int i = 0; i < aColumn.Length; i++) {
                            dc[i] = dt.Columns[aColumn[i]];
                        }
                        dt.PrimaryKey = dc;
                    }
                }
            }

            public DataRow NewRow(string sColumnName = null, object value = null)
            {
                dr = dt.NewRow();
                if (_dict.Count > 0) {
                    foreach (var key in _dict.Keys) {
                        dr[key] = _dict[key];
                    }
                }
                if (sColumnName != null) {
                    dr[sColumnName] = value;
                }
                dt.Rows.Add(dr);
                return dr;
            }

            public DataRow Find(object key)
            {
                return key is Array ? dt.Rows.Find((object[])key) : dt.Rows.Find(key);
            }

            public bool Update()
            {
                var bIsSuccess = false;
                if (dt.Rows.Count > 0) {
                    try {
                        bIsSuccess = adapt.Update(dt) > 0;
                    } catch (Exception e) {
                        ErrHandle("Update", e);
                    }
                }
                if (!KeepLive) {
                    Close();
                }
                return bIsSuccess;
            }

            public void Close()
            {
                if (connSQL != null) {
                    connSQL.Close();
                    connSQL.Dispose();
                    connSQL = null;
                }
            }
        }

        public static void ErrHandle(string funcName, Exception e)
        {
            if (e.InnerException != null) {
                Console.WriteLine($"{funcName} err: {e.InnerException.InnerException.Message}");
            } else {
                Console.WriteLine($"{funcName} err: {e.Message}");
            }
        }
    }


}
