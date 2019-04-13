
using Hands.K3.SCM.APP.Entity.EnumType;
using Hands.K3.SCM.APP.Entity.K3WebApi;
using Hands.K3.SCM.APP.Entity.StructType;
using Hands.K3.SCM.APP.Utils.Utils;
using HS.K3.Common.Abbott;
using Kingdee.BOS;
using Kingdee.BOS.WebApi.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Hands.K3.SCM.App.Synchro.Utils.K3WebApi
{
    public class InvokeWebApi
    {

        private const int ORGID = 100035;
        private static Type type = null;
        private static readonly object SaveLock = new object();

        public InvokeWebApi() { }
        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        public static bool Login(Context ctx, K3CloudApiClient client)
        {
            
            K3LoginInfo.DbId = ctx.DBId;

            DataBaseConst.K3CloudContext = ctx;
            K3LoginInfo.UserName = DataBaseConst.K3CloudUserName;
            K3LoginInfo.Password = DataBaseConst.K3CloudPwd;
            K3LoginInfo.LanguageType =DataBaseConst.K3CloudLanType;

            try
            {
                if (client != null)
                {
                    if (client.Login(K3LoginInfo.DbId, K3LoginInfo.UserName, K3LoginInfo.Password, K3LoginInfo.LanguageType))
                    {
                        return true;
                    }
                    else
                    {
                        LogUtils.WriteSynchroLog(ctx, SynchroDataType.UserLoin, "系统用户没有登录成功！！！");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                LogUtils.WriteSynchroLog(ctx, SynchroDataType.UserLoin, "系统用户出现异常，异常信息：" + ex.Message + System.Environment.NewLine + ex.StackTrace);
            }

            return false;
        }

        /// <summary>
        /// K3WebApi接口
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="dataType"></param>
        /// <param name="operateType"></param>
        /// <param name="formId"></param>
        /// <param name="numbers"></param>
        /// <param name="pkIds"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        public static HttpResponseResult InvokeBatchOperate(Context ctx, SynchroDataType dataType, SynOperationType operateType, string formId, IEnumerable<string> numbers = null, IEnumerable<int> pkIds = null, string json = null)
        {
            HttpResponseResult response = default(HttpResponseResult);

            try
            {
                type = DynamicInvoke.GetType(DynamicInvoke.GetAssembly(Assembly.GetExecutingAssembly().Location), "Hands.K3.SCM.App.Synchro.Utils.K3WebApi.InvokeWebApi");

                if (type != null)
                {
                    response = DynamicInvoke.InvokeMethod<HttpResponseResult>(ctx, type, GetMethodName(type, operateType), GetAgrs(ctx, operateType, dataType, formId, json, numbers, pkIds));

                    if (response != null)
                    {
                        if (!string.IsNullOrEmpty(response.Message) && response.Message.Contains("Timeout 时间已到。在操作完成之前超时时间已过或服务器未响应。"))
                        {
                            LogUtils.WriteSynchroLog(ctx, dataType, "数据批量" + operateType + "过程中出现异常，异常信息：服务器没有响应！！！");
                        }
                    }
                }
                else
                {
                    response = new HttpResponseResult();
                    response.Success = false;
                    response.Message = "反射获取程序集失败！";

                    LogUtils.WriteSynchroLog(ctx, dataType, "数据批量" + operateType + "过程中出现异常，异常信息：" + response.Message);
                    return response;
                }
            }

            catch (Exception ex)
            {
                LogUtils.WriteSynchroLog(ctx, dataType, "数据批量" + operateType + "过程中出现异常，异常信息：" + ex.Message + System.Environment.NewLine + ex.StackTrace);
            }

            return response;
        }

        /// <summary>
        /// 单据批量保存
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="dataType"></param>
        /// <param name="formId"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        public static HttpResponseResult InvokeBatchSave(Context ctx, SynchroDataType dataType, string formId, string json)
        {
            HttpResponseResult response = default(HttpResponseResult);
            string ret = default(string);
            K3CloudApiClient client = null;
            DataBaseConst.K3CloudContext = ctx;

            lock (SaveLock)
            {
                
                client = new K3CloudApiClient(DataBaseConst.CurrentK3CloudURL);

                if (Login(ctx, client))
                {
                    ret = client.BatchSave(formId, json);
                }
                else
                {
                    response = new HttpResponseResult();
                    response.Success = false;
                    response.Message = "登陆账号或密码错误！";
                }
                if (!string.IsNullOrEmpty(ret))
                {
                    response = Response(ctx, dataType, SynOperationType.SAVE, ret, json);
                }
            }
            
            return response;
        }

        /// <summary>
        /// 单据批量分配
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="dataType"></param>
        /// <param name="formId"></param>
        /// <param name="pkIds"></param>
        /// <returns></returns>
        public static HttpResponseResult InvokeBatchAllot(Context ctx, SynchroDataType dataType, string formId, IEnumerable<int> pkIds)
        {
            HttpResponseResult response = default(HttpResponseResult);
            string ret = default(string);
            K3CloudApiClient client = null;
            DataBaseConst.K3CloudContext = ctx;


            client = new K3CloudApiClient(DataBaseConst.CurrentK3CloudURL);

            if (Login(ctx, client))
            {
                ret = client.Allocate(formId, "{\"PkIds\":\"" + FormatFNumber(pkIds) + "\",\"TOrgIds\":" + ORGID + ",\"IsAutoSubmitAndAudit\":\"true\"}");
            }

            if (!string.IsNullOrEmpty(ret))
            {
                response = Response(ctx, dataType, SynOperationType.ALLOT, ret);
            }

            return response;
        }

        /// <summary>
        /// 单据批量审核
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="dataType"></param>
        /// <param name="formId"></param>
        /// <param name="numbers"></param>
        /// <param name="pkIds"></param>
        /// <returns></returns>
        public static HttpResponseResult InvokeBatchAudit(Context ctx, SynchroDataType dataType, string formId, IEnumerable<string> numbers = null, IEnumerable<int> pkIds = null)
        {
            HttpResponseResult response = default(HttpResponseResult);
            string ret = default(string);

            K3CloudApiClient client = null;
            DataBaseConst.K3CloudContext = ctx;

            client = new K3CloudApiClient(DataBaseConst.CurrentK3CloudURL);

            // 登陆成功
            if (Login(ctx, client))
            {

                if (numbers != null && numbers.Count() > 0)
                {
                    ret = client.Audit(formId, "{\"CreateOrgId\":\"0\",\"Numbers\":[" + FormatFNumber(numbers) + "],\"Ids\":\"\"}");
                }
                if (pkIds != null && pkIds.Count() > 0)
                {
                    ret = client.Audit(formId, "{\"CreateOrgId\":\"0\",\"Numbers\":[],\"Ids\":\"" + FormatFNumber(pkIds) + "\"}");
                }

                if (!string.IsNullOrEmpty(ret))
                {
                    response = Response(ctx, dataType, SynOperationType.AUDIT, ret);
                }

            }
            return response;
        }

        /// <summary>
        /// 单据反审核
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="dataType"></param>
        /// <param name="formId"></param>
        /// <param name="numbers"></param>
        /// <param name="pkIds"></param>
        /// <returns></returns>
        public static HttpResponseResult InvokeBatchUnAudit(Context ctx, SynchroDataType dataType, string formId, IEnumerable<string> numbers = null, IEnumerable<int> pkIds = null)
        {
            HttpResponseResult response = default(HttpResponseResult);
            string ret = default(string);

            K3CloudApiClient client = null;
            DataBaseConst.K3CloudContext = ctx;

            client = new K3CloudApiClient(DataBaseConst.CurrentK3CloudURL);

            if (Login(ctx, client))
            {

                if (numbers != null && numbers.Count() > 0)
                {
                    ret = client.UnAudit(formId, "{\"CreateOrgId\":\"0\",\"Numbers\":[" + FormatFNumber(numbers) + "],\"Ids\":\"\"}");
                }
                if (pkIds != null && pkIds.Count() > 0)
                {
                    ret = client.UnAudit(formId, "{\"CreateOrgId\":\"0\",\"Numbers\":[],\"Ids\":\"" + FormatFNumber(pkIds) + "\"}");
                }

                if (!string.IsNullOrEmpty(ret))
                {
                    response = Response(ctx, dataType, SynOperationType.UNAUDIT, ret);

                }

            }
            return response;
        }

        /// <summary>
        /// 单据批量提交
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="dataType"></param>
        /// <param name="formId"></param>
        /// <param name="numbers"></param>
        /// <param name="pkIds"></param>
        /// <returns></returns>
        public static HttpResponseResult InvokeBatchSubmit(Context ctx, SynchroDataType dataType, string formId, IEnumerable<string> numbers = null, IEnumerable<int> pkIds = null)
        {
            HttpResponseResult response = default(HttpResponseResult);
            string ret = default(string);

            K3CloudApiClient client = null;
            DataBaseConst.K3CloudContext = ctx;

            client = new K3CloudApiClient(DataBaseConst.CurrentK3CloudURL);

            // 登陆成功
            if (Login(ctx, client))
            {

                if (numbers != null && numbers.Count() > 0)
                {
                    ret = client.Submit(formId, "{\"CreateOrgId\":\"0\",\"Numbers\":[" + FormatFNumber(numbers) + "],\"Ids\":\"\"}");
                }
                if (pkIds != null && pkIds.Count() > 0)
                {
                    ret = client.Submit(formId, "{\"CreateOrgId\":\"0\",\"Numbers\":[],\"Ids\":\"" + FormatFNumber(pkIds) + "\"}");
                }
                if (!string.IsNullOrEmpty(ret))
                {
                    response = Response(ctx, dataType, SynOperationType.SUBMIT, ret);
                }

            }
            return response;
        }

        /// <summary>
        /// 单据批量删除
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="dataType"></param>
        /// <param name="formId"></param>
        /// <param name="numbers"></param>
        /// <param name="pkIds"></param>
        /// <returns></returns>
        public static HttpResponseResult InvokeBatchDelete(Context ctx, SynchroDataType dataType, string formId, IEnumerable<string> numbers = null, IEnumerable<int> pkIds = null)
        {
            HttpResponseResult response = default(HttpResponseResult);
            string ret = default(string);

            K3CloudApiClient client = null;
            DataBaseConst.K3CloudContext = ctx;

            client = new K3CloudApiClient(DataBaseConst.CurrentK3CloudURL);

            // 登陆成功
            if (Login(ctx, client))
            {

                if (numbers != null && numbers.Count() > 0)
                {
                    ret = client.Delete(formId, "{\"CreateOrgId\":\"0\",\"Numbers\":[" + FormatFNumber(numbers) + "],\"Ids\":\"\"}");
                }
                if (pkIds != null && pkIds.Count() > 0)
                {
                    ret = client.Delete(formId, "{\"CreateOrgId\":\"0\",\"Numbers\":[],\"Ids\":\"" + FormatFNumber(pkIds) + "\"}");
                }
                if (!string.IsNullOrEmpty(ret))
                {
                    response = Response(ctx, dataType, SynOperationType.DELETE, ret);

                }

            }
            return response;
        }

        /// <summary>
        /// 单据批量作废
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="dataType"></param>
        /// <param name="formId"></param>
        /// <param name="numbers"></param>
        /// <param name="pkIds"></param>
        /// <returns></returns>
        public static HttpResponseResult InvokeBatchCancel(Context ctx, SynchroDataType dataType, string formId, IEnumerable<string> numbers = null, IEnumerable<int> pkIds = null)
        {
            HttpResponseResult response = default(HttpResponseResult);
            string ret = default(string);

            K3CloudApiClient client = null;
            DataBaseConst.K3CloudContext = ctx;

            client = new K3CloudApiClient(DataBaseConst.CurrentK3CloudURL);

            // 登陆成功
            if (Login(ctx, client))
            {

                if (numbers != null && numbers.Count() > 0)
                {
                    ret = client.ExcuteOperation(formId, "Cancel", "{\"CreateOrgId\":\"0\",\"Numbers\":[" + FormatFNumber(numbers) + "],\"Ids\":\"\"}");
                }
                if (pkIds != null && pkIds.Count() > 0)
                {
                    ret = client.ExcuteOperation(formId, "Cancel", "{\"CreateOrgId\":\"0\",\"Numbers\":[],\"Ids\":\"" + FormatFNumber(pkIds) + "\"}");
                }
                if (!string.IsNullOrEmpty(ret))
                {
                    response = Response(ctx, dataType, SynOperationType.DELETE, ret);
                }
            }

            return response;
        }
        /// <summary>
        /// 批量下推
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="dataType"></param>
        /// <param name="formId"></param>
        /// <param name="numbers"></param>
        /// <param name="pkIds"></param>
        /// <returns></returns>
        public static HttpResponseResult InvokeBatchPush(Context ctx, SynchroDataType dataType, string formId, IEnumerable<string> numbers = null, IEnumerable<int> pkIds = null)
        {
            HttpResponseResult response = default(HttpResponseResult);
            string ret = default(string);

            K3CloudApiClient client = null;
            DataBaseConst.K3CloudContext = ctx;

            client = new K3CloudApiClient(DataBaseConst.CurrentK3CloudURL);

            // 登陆成功
            if (Login(ctx, client))
            {

                if (numbers != null && numbers.Count() > 0)
                {
                    ret = client.Push("SAL_BATCHADJUSTPRICE", "{\"Ids\":\"\",\"Numbers\":[],\"EntryIds\":\"\",\"RuleId\":\"\",\"TargetBillTypeId\":\"\",\"TargetOrgId\":0,\"TargetFormId\":\"\",\"IsEnableDefaultRule\":\"false\",\"IsDraftWhenSaveFail\":\"false\",\"CustomParams\":{}}");
                }
                if (pkIds != null && pkIds.Count() > 0)
                {
                    ret = client.ExcuteOperation(formId, "Cancel", "{\"CreateOrgId\":\"0\",\"Numbers\":[],\"Ids\":\"" + FormatFNumber(pkIds) + "\"}");
                }
                if (!string.IsNullOrEmpty(ret))
                {
                    response = Response(ctx, dataType, SynOperationType.DELETE, ret);
                }
            }

            return response;
        }
        /// <summary>
        /// 提交请求后，服务端响应客户端信息
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="dataType"></param>
        /// <param name="operationType"></param>
        /// <param name="ret"></param>
        /// <returns></returns>
        public static HttpResponseResult Response(Context ctx, SynchroDataType dataType, SynOperationType operationType, string ret, string json = null)
        {
            HttpResponseResult response = default(HttpResponseResult);
            Dictionary<string, string> dict = new Dictionary<string, string>();

            string msg = string.Empty;
            string last = string.Empty;

            if (!string.IsNullOrEmpty(ret))
            {
                response = new HttpResponseResult(dataType);
                
                JObject result = JObject.Parse(ret);

                if (result.Property("Result") != null)
                {
                    if (JObject.Parse(JsonUtils.ConvertObjectToString(result["Result"])).Property("ResponseStatus") != null)
                    {
                        response.Success = Convert.ToBoolean(JsonUtils.ConvertObjectToString(result["Result"]["ResponseStatus"]["IsSuccess"]));
                    }

                }

                if (response.Success == false)
                {
                    if (result.Property("Result") != null)
                    {
                        if (JObject.Parse(JsonUtils.ConvertObjectToString(result["Result"])).Property("ResponseStatus") != null)
                        {
                            JArray jArr = JArray.Parse(JsonUtils.ConvertObjectToString(result["Result"]["ResponseStatus"]["Errors"]));

                            if (jArr != null && jArr.Count() > 0)
                            {
                                string number = null;

                                for (int i = 0; i < jArr.Count(); i++)
                                {
                                    if (jArr[i]["DIndex"] != null)
                                    {
                                        int index = Convert.ToInt32(JsonUtils.ConvertObjectToString(jArr[i]["DIndex"]));
                                        number = GetSynFailBillNo(ctx, json, dataType, index);
                                        

                                        if (index > 0)
                                        {
                                            last = GetSynFailBillNo(ctx, json, dataType, index - 1);
                                        }
                                    }
                                    if (jArr[i]["FieldName"] != null)
                                    {
                                        if (!string.IsNullOrWhiteSpace(number))
                                        {
                                            if (i < jArr.Count() - 1)
                                            {
                                                response.Message += "编码为[" + number + "]:" + JsonUtils.ConvertObjectToString(jArr[i]["FieldName"]) + System.Environment.NewLine;
                                            }
                                            else if (i == jArr.Count() - 1)
                                            {
                                                response.Message += "编码为[" + number + "]:" + JsonUtils.ConvertObjectToString(jArr[i]["FieldName"]);
                                            }
                                        }
                                        else
                                        {
                                            if (i < jArr.Count() - 1)
                                            {
                                                response.Message += JsonUtils.ConvertObjectToString(jArr[i]["FieldName"]) + System.Environment.NewLine;
                                            }
                                            else if (i == jArr.Count() - 1)
                                            {
                                                response.Message += JsonUtils.ConvertObjectToString(jArr[i]["FieldName"]);
                                            }
                                        }
                                    }
                                    if (jArr[i]["Message"] != null)
                                    {
                                        if (!string.IsNullOrWhiteSpace(number))
                                        {
                                            if (i < jArr.Count() - 1)
                                            {
                                                response.Message += "编码为[" + number + "]:" + JsonUtils.ConvertObjectToString(jArr[i]["Message"]) + System.Environment.NewLine;
                                            }
                                            else if (i == jArr.Count() - 1)
                                            {
                                                response.Message += "编码为[" + number + "]:" + JsonUtils.ConvertObjectToString(jArr[i]["Message"]);
                                            }
                                        }
                                        else
                                        {
                                            if (i < jArr.Count() - 1)
                                            {
                                                response.Message += JsonUtils.ConvertObjectToString(jArr[i]["Message"]) + System.Environment.NewLine;
                                            }
                                            else if (i == jArr.Count() - 1)
                                            {
                                                response.Message += JsonUtils.ConvertObjectToString(jArr[i]["Message"]);
                                            }
                                        }
                                    }
                                    
                                }
                                
                                if (!string.IsNullOrWhiteSpace(response.Message))
                                {
                                    LogUtils.WriteSynchroLog(ctx, dataType, operationType + "操作, Response返回的Json信息异常，异常信息：" + System.Environment.NewLine + response.Message);
                                }
                            }

                            response.ExceptionCode = JsonUtils.ConvertObjectToString(result["Result"]["ResponseStatus"]["ErrorCode"]);
                        }
                    }
                    
                }

                if (result.Property("Result") != null)
                {
                    if (JObject.Parse(JsonUtils.ConvertObjectToString(result["Result"])).Property("Number") != null)
                    {
                        response.Source = JsonUtils.ConvertObjectToString(result["Result"]["Number"]);
                    }

                    if (JObject.Parse(JsonUtils.ConvertObjectToString(result["Result"])).Property("NeedReturnData") != null)
                    {
                        response.NeedReturnDatas = JArray.Parse(JsonUtils.ConvertObjectToString(result["Result"]["NeedReturnData"]));
                        response.ResultJson = JsonUtils.ConvertObjectToString(result["Result"]["NeedReturnData"]);
                    }

                    if (JObject.Parse(JsonUtils.ConvertObjectToString(result["Result"])).Property("ResponseStatus") != null)
                    {
                        response.SuccessEntitys = JArray.Parse(JsonUtils.ConvertObjectToString(result["Result"]["ResponseStatus"]["SuccessEntitys"]));
                    }
                }

            }

            if (response != null)
            {
                if (response.Success == false || ret.StartsWith("Code"))
                {
                    LogUtils.WriteSynchroLog(ctx, dataType, "" + operationType + "操作, Response返回的Json信息异常，异常信息：" + ret);
                }
            }

            return response;
        }

        /// <summary>
        /// 解析json
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static JArray GetResponseByJson(string json)
        {
            JArray jArr = null;

            if (!string.IsNullOrWhiteSpace(json))
            {
                JObject jObj = JObject.Parse(json);

                if (jObj.Property("Model") != null)
                {
                    jArr = JArray.Parse(JsonUtils.ConvertObjectToString(jObj["Model"]));
                }
            }
            return jArr;
        }

        /// <summary>
        /// 根据返回结果获取不能保存成功的单据编号
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="json"></param>
        /// <param name="dataType"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static string GetSynFailBillNo(Context ctx, string json, SynchroDataType dataType, int index)
        {
            JArray jArr = GetResponseByJson(json);
            string number = null;

            if (jArr != null && jArr.Count() > 0)
            {
                if (dataType == SynchroDataType.Customer && index < jArr.Count())
                {
                    if (JObject.Parse(JsonUtils.ConvertObjectToString(jArr[index])).Property("FNumber") != null)
                    {
                        number = JsonUtils.ConvertObjectToString(jArr[index]["FNumber"]);
                    }
                    else if (JObject.Parse(JsonUtils.ConvertObjectToString(jArr[index])).Property("FCUSTID") != null)
                    {
                        int custId = Convert.ToInt32(JsonUtils.ConvertObjectToString(jArr[index]["FCUSTID"]));
                        number = SQLUtils.GetCustomerNo(ctx, custId);
                    }
                }
                else if (dataType == SynchroDataType.ReceiveBill && index < jArr.Count())
                {
                    if (JObject.Parse(JsonUtils.ConvertObjectToString(jArr[index])).Property("FRECEIVEBILLENTRY") != null)
                    {
                        number = JsonUtils.ConvertObjectToString(jArr[index]["FRECEIVEBILLENTRY"][0]["FRECEIVEITEM"]);
                    }
                }
                else
                {
                    if (JObject.Parse(JsonUtils.ConvertObjectToString(jArr[index])).Property("FBillNo") != null)
                    {
                        number = JsonUtils.ConvertObjectToString(jArr[index]["FBillNo"]);
                    }
                }
            }
            return number;
        }

        /// <summary>
        /// 将单据编码或内码转换为json格式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="numbers"></param>
        /// <returns></returns>
        public static string FormatFNumber<T>(IEnumerable<T> numbers)
        {
            string strNumbers = "";
            Type t = numbers.GetType();
            Type it = typeof(IEnumerable<int>);

            if (numbers != null)
            {
                if (numbers.Count() > 0)
                {
                    for (int i = 0; i < numbers.Count(); i++)
                    {
                        if (i < numbers.Count() - 1)
                        {

                            if (t == it || t.IsSubclassOf(it) || it.IsAssignableFrom(t))
                            {
                                strNumbers += numbers.ElementAt(i) + ",";
                            }
                            else
                            {
                                strNumbers += "\"" + numbers.ElementAt(i) + "\"" + ",";
                            }

                        }
                        else if (i == numbers.Count() - 1)
                        {
                            if (t == it || t.IsSubclassOf(it) || it.IsAssignableFrom(t))
                            {
                                strNumbers += numbers.ElementAt(i);
                            }
                            else
                            {
                                strNumbers += "\"" + numbers.ElementAt(i) + "\"";
                            }
                        }
                    }
                }
            }
            return strNumbers;
        }

        /// <summary>
        /// 根据操作类型确定动态调用方法的参数类型和个数
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="operateType"></param>
        /// <param name="dataType"></param>
        /// <param name="formId"></param>
        /// <param name="json"></param>
        /// <param name="numbers"></param>
        /// <param name="pkIds"></param>
        /// <returns></returns>
        public static object[] GetAgrs(Context ctx, SynOperationType operateType, SynchroDataType dataType, string formId, string json, IEnumerable<string> numbers = null, IEnumerable<int> pkIds = null)
        {
            if (!string.IsNullOrWhiteSpace(operateType.ToString()))
            {
                switch (operateType)
                {
                    case SynOperationType.SAVE:
                        return new object[] { ctx, dataType, formId, json };
                    case SynOperationType.DELETE:
                    case SynOperationType.AUDIT:
                    case SynOperationType.UNAUDIT:
                    case SynOperationType.SUBMIT:
                        return new object[] { ctx, dataType, formId, numbers, pkIds };
                    case SynOperationType.ALLOT:
                        return new object[] { ctx, dataType, formId, pkIds };
                }
            }
            return null;
        }

        /// <summary>
        /// 根据操作类型确定调用动态方法的名称
        /// </summary>
        /// <param name="type"></param>
        /// <param name="operateType"></param>
        /// <returns></returns>
        public static string GetMethodName(Type type, SynOperationType operateType)
        {
            MethodInfo[] methods = DynamicInvoke.GetMethods(type);

            if (methods != null && methods.Length > 0)
            {
                foreach (var method in methods)
                {
                    if (method != null)
                    {
                        if (method.Name.ToUpper().Contains(operateType.ToString().ToUpper()))
                        {
                            return method.Name;
                        }
                    }
                }
            }

            return null;
        }
    }
}
