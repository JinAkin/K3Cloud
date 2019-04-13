
using HS.K3.Common.Abbott;
using Kingdee.BOS;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hands.K3.SCM.APP.Entity.K3WebApi
{
    [Serializable]
    public class HttpResponseResult
    {
        [JsonIgnore]
        private Exception _innerException;
        /// <summary>
        /// 单据操作成功与否
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// 单据操作后的结果
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 单据操作后的结果
        /// </summary>
        public object Result { get; set; }

        /// <summary>
        /// 单据操作后失败的结果
        /// </summary>
        public object FailedResult { get; set; }

        /// <summary>
        /// 异常代码
        /// </summary>
        public string ExceptionCode { get; set; }
        /// <summary>
        /// 堆栈跟踪信息
        /// </summary>

        public string StackTrace { get; set; }


        public string Source { get; set; }
        /// <summary>
        /// 请求服务成功后返回的信息
        /// </summary>
        public JArray NeedReturnDatas { get; set; }
        /// <summary>
        /// 请求服务成功后返回的信息
        /// </summary>
        public JArray SuccessEntitys { get; set; }
        public string FieldName { get; set; }
        /// <summary>
        /// 单据操作类型
        /// </summary>
        public SynchroDataType DataType { get; set; }

        //public string JSon { get; set; }

        //public Dictionary<string, string> BillNo2SalerIds
        //{
        //    get
        //    {
        //        if (!string.IsNullOrWhiteSpace(JSon))
        //        {
        //            JObject jObject = JObject.Parse(JSon);
        //            Dictionary<string, string> dict = null;

        //            if (jObject != null && jObject.Property("Model") != null)
        //            {
        //                JArray jArray = JArray.Parse(jObject["Model"].ToString());

        //                if (jArray != null && jArray.Count > 0)
        //                {
        //                    dict = new Dictionary<string, string>();

        //                    foreach (var jToken in jArray)
        //                    {
        //                        if (jToken != null)
        //                        {
        //                            JObject jObj = jToken as JObject;

        //                            string billNo = GetBillNo(typeof(string));

        //                            if (!string.IsNullOrWhiteSpace(billNo))
        //                            {
        //                                if (jObj.Property(billNo) != null)
        //                                {
        //                                    string sellerId = GetBillNo(typeof(string), "SELLERID");

        //                                    if (!string.IsNullOrWhiteSpace(sellerId))
        //                                    {
        //                                        if (jObj.Property(sellerId) != null)
        //                                        {
        //                                            JObject seller = jObj[sellerId] as JObject;
        //                                            dict.Add(jObj[billNo].ToString(), seller["FNumber"].ToString());
        //                                        }
        //                                    }

        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //            return dict;
        //        }

        //        return null;
        //    }

        //}

        //public List<K3Response> SpecifiedMsgs
        //{
        //    get
        //    {
        //        List<K3Response> responses = null;
        //        string[] strError = null;
        //        int count = 0;

        //        if (!string.IsNullOrWhiteSpace(Message) && !Success)
        //        {   
        //            responses = new List<K3Response>();

        //            if (Message.Contains(Environment.NewLine))
        //            {
        //                strError = Message.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

        //                if (strError != null && strError.Count() > 0)
        //                {
        //                    foreach (var str in strError)
        //                    {
        //                        if (!string.IsNullOrWhiteSpace(str) && str.Contains("[") && str.Contains("]"))
        //                        {
        //                            int begin = str.IndexOf("[");
        //                            int end = str.IndexOf("]");
        //                            string num = str.Substring(begin + 1, end - begin - 1);
        //                            K3Response res = new K3Response(num, str);
        //                            List<string> salerIds = BillNo2SalerIds.Where(s => s.Equals(num)).Select(s => s.Value).ToList();

        //                            if (salerIds != null && salerIds.Count > 0)
        //                            {
        //                                res.SalerId = salerIds.ElementAt(0);
        //                            }
        //                            res.K3CloudContext = K3CloudContext;
        //                            responses.Add(res);
        //                        }
        //                    }
        //                    if (responses != null && responses.Count > 0)
        //                    {
        //                        StringBuilder sb = null;
        //                        var group = responses.GroupBy(m => m.BillNo);

        //                        if (group != null && group.Count() > 0)
        //                        {
        //                            foreach (var gro in group)
        //                            {
        //                                if (gro != null && gro.Count() > 0)
        //                                {
        //                                    sb = new StringBuilder();

        //                                    foreach (var g in gro)
        //                                    {
        //                                        if (!string.IsNullOrWhiteSpace(g.Msg))
        //                                        {
        //                                            if (count < responses.Count() - 1)
        //                                            {
        //                                                sb.AppendLine(g.Msg + Environment.NewLine);
        //                                            }
        //                                            else
        //                                            {
        //                                                sb.AppendLine(g.Msg);
        //                                            }
        //                                        }
        //                                        count++;
        //                                    }
                                            
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }

        //        return responses;
        //    }
        //}
       
        public string ResultJson { get; set; }
        /// <summary>
        /// 单据操作成功返回的单据编码
        /// </summary>
        public List<string> SuccessEntityNos
        {
            get
            {
                if (GetSuccessEntityFieldValues("Number") != null)
                {
                    if (GetSuccessEntityFieldValues("Number").ConvertAll(obj => string.Format("{0}", obj)) != null)
                    {
                        return GetSuccessEntityFieldValues("Number").ConvertAll(obj => string.Format("{0}", obj));
                    }
                }

                else
                {
                    if (GetNeedReturnData(typeof(string)) != null)
                    {
                        if (GetNeedReturnData(typeof(string)).ConvertAll(obj => string.Format("{0}", obj)) != null)
                        {
                            return GetNeedReturnData(typeof(string)).ConvertAll(obj => string.Format("{0}", obj));
                        }
                    }
                }

                return null;
            }
            set
            { }

        }
        /// <summary>
        /// 单据操作成功返回的单据内码
        /// </summary>
        public List<int> SuccessEntityIds
        {
            get
            {
                if (GetSuccessEntityFieldValues("Id") != null)
                {
                    if (GetSuccessEntityFieldValues("Id").ConvertAll(obj => Convert.ToInt32(obj)) != null)
                    {
                        return GetSuccessEntityFieldValues("Id").ConvertAll(obj => Convert.ToInt32(obj));
                    }
                }

                else
                {
                    if (GetNeedReturnData(typeof(int)) != null)
                    {
                        if (GetNeedReturnData(typeof(int)).ConvertAll(obj => string.Format("{0}", obj)) != null)
                        {
                            return GetNeedReturnData(typeof(int)).ConvertAll(obj => Convert.ToInt32(obj));
                        }
                    }
                }

                return null;
            }
            set
            { }

        }

        /// <summary>
        /// 根据字段名获取请求服务后返回的字段值
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public List<object> GetNeedReturnValues(string fieldName)
        {
            List<object> retVal = new List<object>();

            if (NeedReturnDatas != null)
            {
                if (NeedReturnDatas.Count > 0)
                {
                    for (int i = 0; i < NeedReturnDatas.Count; i++)
                    {
                        if (!string.IsNullOrEmpty(fieldName))
                        {
                            retVal.Add(NeedReturnDatas[i][fieldName].ToString());
                        }

                    }
                }
            }
            return retVal;
        }

        /// <summary>
        /// 根据类型获取单据的编码或内码值
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public List<object> GetNeedReturnData(Type type)
        {
            List<object> retVal = new List<object>();

            if (NeedReturnDatas != null)
            {
                if (NeedReturnDatas.Count > 0)
                {
                    for (int i = 0; i < NeedReturnDatas.Count; i++)
                    {
                        if (type != null)
                        {
                            retVal.Add(NeedReturnDatas[i][GetBillNo(type)].ToString());
                        }

                    }
                }
            }
            return retVal;
        }
        /// <summary>
        /// 根据类型获取单据的编码或内码名称
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public string GetBillNo(Type type, string fieldName = "BILLNO")
        {
            if (type == typeof(string))
            {
                switch (DataType)
                {
                    case SynchroDataType.SaleOrder:
                    case SynchroDataType.ReceiveBill:
                        switch (fieldName)
                        {
                            case "BILLNO":
                                return "FBillNo";
                            case "SELLERID":
                                return "FSalerId";
                            default:
                                return null;
                        }

                    case SynchroDataType.Customer:
                        switch (fieldName)
                        {
                            case "BILLNO":
                                return "FNumber";
                            case "SELLERID":
                                return "FSELLER";
                            default:
                                return null;
                        }
                }
            }
            if (type == typeof(int))
            {
                switch (DataType)
                {
                    case SynchroDataType.SaleOrder:
                    case SynchroDataType.ReceiveBill:
                        return "FID";
                    case SynchroDataType.Customer:
                        return "FCUSTID";
                }
            }
            return null;
        }
        /// <summary>
        /// 根据字段名获取请求服务后返回的字段值
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public List<object> GetSuccessEntityFieldValues(string fieldName)
        {
            List<object> succNos = new List<object>();

            if (SuccessEntitys != null)
            {
                if (SuccessEntitys.Count > 0)
                {
                    for (int i = 0; i < SuccessEntitys.Count; i++)
                    {
                        succNos.Add(SuccessEntitys[i][fieldName].ToString());
                    }
                }
            }
            return succNos;
        }
        [JsonIgnore]
        public Exception InnerException
        {
            get
            {
                return this._innerException;
            }
            set
            {
                this._innerException = value;
                if (value.InnerException != null)
                {
                    this._innerException = value.InnerException;
                }
                this.Message = this._innerException.Message;
                this.StackTrace = this._innerException.StackTrace;
                this.Source = this._innerException.Source;
            }
        }

        public HttpResponseResult()
        {
        }

        public HttpResponseResult(SynchroDataType dataType)
        {
            this.DataType = dataType;
        }
        public HttpResponseResult(bool success, string message, string result)
        {
            this.Success = success;
            this.Message = message;
            this.Result = result;
        }

        //public override string ToString()
        //{
        //    return JNObjectConverter.SerializeObject(this, new JsonConverter[]
        //    {
        //        new DynamicJObjectConverter()
        //    });
        //}
    }
}
