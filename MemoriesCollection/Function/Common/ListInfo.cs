using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MemoriesCollection.Function.Common
{

    public class VtTags
    {
        public bool Error = false; //判斷參數正確性
        public string ErrorMsg = "Error !";
        public Dictionary<string, string> Tags;


        /// <summary>
        /// 欄位資料集
        /// </summary>
        public string _tags
        {
            get
            {
                return null;
            }
            set
            {
                //value = PutInfo.DecryptAES(JsHelper.unescape(value));
                if (value == "") {
                    Error = true;
                }
                Tags = Dict(value);
            }
        }

        /// <summary>
        /// 欄位資料集
        /// </summary>
        public string _models
        {
            get
            {
                return null;
            }
            set
            {
                //value = PutInfo.DecryptAES(JsHelper.unescape(value));
                if (value == "") {
                    Error = true;
                }
                try {
                    if (Tags.ContainsKey("Model")) {
                        var model = Tags["Model"].Split(',');
                        foreach (var m in model) {
                            switch (m) {
                                case "Income":
                                    //Income = JsonConvert.DeserializeObject<Income>(value);
                                    break;
                            }
                        }
                    }
                } catch (Exception ex) {
                    Error = true;
                    if (ex.InnerException != null) {
                        if (ex.InnerException.InnerException == null) {
                            ErrorMsg = ex.InnerException.Message;
                        } else {
                            ErrorMsg = ex.InnerException.InnerException.Message;
                        }
                    } else {
                        ErrorMsg = ex.Message;
                    }
                }
            }
        }

        /// <summary>
        /// 轉換為 Key / Value 資料
        /// </summary>
        /// <param name="json">json 資料</param>
        /// <returns></returns>
        public Dictionary<string, string> Dict(string json)
        {
            var rtn = new Dictionary<string, string>();
            if (json == null || json == "") {
                return rtn;
            }
            rtn = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            return rtn;
        }

    }

}
