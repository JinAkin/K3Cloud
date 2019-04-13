using Hands.K3.SCM.APP.Entity.SynDataObject;
using Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject;
using Kingdee.BOS;
using Kingdee.BOS.JSON;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.APP.Utils.Utils
{
    public class FreightUtils
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="customerNo">客户编码</param>
        /// <param name="customerLevel">客户等级(1-8,默认1)</param>
        /// <param name="countryNo">国家编码</param>
        /// <param name="city">城市</param>
        /// <param name="zipCode">邮编</param>
        /// <param name="currencyNo">币别</param>
        /// <param name="inLogisWay">发货方式</param>
        public static Dictionary<string,object> GetFrei123(Context ctx,AbsSynchroDataInfo info)
        {
            #region 参数
            Dictionary<string, object> dict = null;

            if (info != null)
            {
                K3SalOrderInfo order = info as K3SalOrderInfo;

                if (order != null)
                {
                    long timeStamp = GetTimeStamp(DateTime.Now);//时间戳
                    string phpKey = "ERPSHIPPINGFREE";//签名key
                    string signMsg = MD5Encrypt(order.FCustId + timeStamp + phpKey, Encoding.UTF8).ToUpper();//签名(需大写)

                    JSONObject jObj = new JSONObject();
                    jObj.Add("customer_id", order.FCustId);
                    jObj.Add("whole", !string.IsNullOrWhiteSpace(order.FCustLevel) ? order.FCustLevel.Substring(order.FCustLevel.Length - 1, 1) : "");
                    jObj.Add("country", order.F_HS_RecipientCountry.ToUpper());//国家代号（例如：US）
                    jObj.Add("city", order.F_HS_DeliveryCity);//收货地址城市
                    jObj.Add("code", order.F_HS_PostCode);//收货地址邮编
                    jObj.Add("currency", order.FSettleCurrId);//货币类型（例如：USD）（首字母要大写）
                    jObj.Add("time_stamp", timeStamp);//时间戳
                    jObj.Add("signMsg", signMsg);//签名

                    JSONArray rows = null;
                    JSONObject products = null;
                    JSONObject row = null;
                    string dlcHSName = string.Empty;

                    if (order.OrderEntry != null && order.OrderEntry.Count > 0)
                    {
                        var groups = from o in order.OrderEntry
                                     where !string.IsNullOrWhiteSpace(o.FMaterialId) && !string.IsNullOrWhiteSpace(o.FStockId)
                                     group o by o.FStockId
                                    into g
                                     select g;

                        if (groups != null && groups.Count() > 0)
                        {
                            products = new JSONObject();

                            foreach (var group in groups)
                            {
                                if (group != null && group.Count() > 0)
                                {
                                    rows = new JSONArray();
                                    dlcHSName = GetStockName(ctx, group.FirstOrDefault().FStockId);

                                    foreach (var item in group)
                                    {
                                        if (item != null && !item.FMaterialId.StartsWith("99."))
                                        {
                                            row = new JSONObject();
                                            row.Add("fix_id", item.FMaterialId);
                                            row.Add("quantity", item.FQTY);

                                            rows.Add(row);
                                        }
                                    }

                                    if (!string.IsNullOrWhiteSpace(dlcHSName) && rows.Count > 0)
                                    {
                                        products.Add(dlcHSName, rows);
                                    }
                                }
                            }

                            jObj.Add("products", products);
                        } 
                    }


                    #endregion

                    //获取运费
                    HttpClient http = new HttpClient() { IsProxy = true };
                    //http.Url = "https://test.healthcabin.net/index.php?t_method=shipping";  //测试地址
                    http.Url = "https://www.healthcabin.net/index.php?t_method=shipping"; //正式地址
                    http.Content = "";//清除之前的记录
                    http.Content = string.Concat("&ERP=", jObj.ToString());  //线上那边要求以键值对参数的形式传过去 
                    string result = "";
                    try
                    {
                        result = http.PostData();
                    }
                    catch (Exception)
                    {
                        //服务器在美国，存在连不上远程服务器的情况，此时等待一秒再请求
                        System.Threading.Thread.Sleep(1000);
                        result = http.PostData();
                    }
                    StringBuilder errorMes = new StringBuilder();
                    List<Dictionary<string, string>> lstFreiInfo = AnalysisResult(result, ref errorMes);
                    if (errorMes.Length > 0)
                    {

                    }

                    var freiData = lstFreiInfo.Where(o => o["F_HS_FreTiltle"].Contains(order.F_HS_DropShipDeliveryChannel)).FirstOrDefault();

                    if (freiData != null)
                    {
                        dict = new Dictionary<string, object>();

                        string outLogisWay = freiData["F_HS_FreTiltle"];//线上发货方式
                        string freiAmount = freiData["F_HS_FreAmount"];//运费金额（结算币别）
                        string freiAmountUSD = freiData["F_HS_AmountNoteUSD"];//各发货仓运费(USD)

                       
                        dict.Add("F_HS_FreTiltle", outLogisWay == null ? "": outLogisWay);
                        dict.Add("F_HS_FreAmount", freiAmount);
                        dict.Add("F_HS_AmountNoteUSD", freiAmountUSD == null ?"": freiAmountUSD);
                    }
                }
            }

            return dict;
        }

        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="input"></param>
        /// <param name="encode"></param>
        /// <returns></returns>
        private static string MD5Encrypt(string input, Encoding encode)
        {
            if (string.IsNullOrEmpty(input))
            {
                return null;
            }
            MD5CryptoServiceProvider md5Hasher = new MD5CryptoServiceProvider();
            byte[] data = md5Hasher.ComputeHash(encode.GetBytes(input));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));//16进制
            }
            return sBuilder.ToString();
        }

        /// <summary>
        /// 获取时间戳
        /// </summary>
        /// <returns></returns>
        private static long GetTimeStamp(DateTime date)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            return (long)(date - startTime).TotalMilliseconds; // 相差秒数
        }

        /// <summary>
        /// 解析网上返回结果
        /// </summary>
        /// <param name="result">网上返回的Json串</param>
        /// <param name="errorMes">错误信息</param>
        /// <returns></returns>
        private static List<Dictionary<string, string>> AnalysisResult(string result, ref StringBuilder errorMes)
        {
            List<Dictionary<string, string>> lstAllDic = new List<Dictionary<string, string>>();

            decimal rate = 1;//线上汇率 
            JSONObject YFInfo = null;
            try
            {
                YFInfo = JSONObject.Parse(result);
            }
            catch (Exception ex)
            {
                return lstAllDic;
            }
            if (YFInfo.Keys.Contains("error"))
            {
                errorMes.AppendLine("获取运费失败:" + YFInfo["error"]);
                return lstAllDic;
            }
            if (YFInfo != null && YFInfo.Keys.Contains("currency_value"))
            {
                rate = Convert.ToDecimal(YFInfo["currency_value"]);
            }

            var YFList = YFInfo.GetJSONObject("shipping_list");//线上发货方式集合

            if (YFList != null && YFList.Count() > 0)
            {
                foreach (var list in YFList)//按仓库拆分
                {
                    List<Dictionary<string, string>> lstDic = new List<Dictionary<string, string>>();
                    var lstValue = JSONObject.Parse(list.Value.ToString());
                    if (lstValue.Keys.Contains("state") && !lstValue["state"].IsNullOrEmptyOrWhiteSpace()
                        && lstValue["state"].ToString() == "00001")//没有运费，需查明原因
                    {
                        errorMes.AppendLine(string.Format(@"获取运费失败，当前单据信息无法计算运费，详情请咨询相关人员!详细信息:{0}", lstValue.ToString()));
                        return lstAllDic;
                    }
                    var YFReList = JSONArray.Parse(lstValue["list"].ToString());//发货方式集合
                    for (int newRow = 0; newRow < YFReList.Count; newRow++)
                    {
                        var data = JSONObject.Parse(YFReList.GetJsonString(newRow));
                        if (data != null)
                        {
                            Dictionary<string, string> dic = new Dictionary<string, string>();
                            dic.Add("key", list.Key);
                            dic.Add("F_HS_FreAmount", (Convert.ToDecimal(data["price"]) * rate).ToString());
                            dic.Add("F_HS_AmountNoteUSD", list.Key + ":" + (Convert.ToDecimal(data["price"])).ToString());

                            decimal remote_money = Convert.ToDecimal(data["remote_money"].IsNullOrEmptyOrWhiteSpace() ? 0 : data["remote_money"]);//地区偏远费
                            string remoteNote = "";
                            if (remote_money > 0)
                            {
                                remoteNote = string.Format("(including {0} Service Remote Area)", remote_money);
                            }
                            dic.Add("F_HS_FreTiltle", data["name"].ToString() + remoteNote);
                            lstDic.Add(dic);
                        }
                    }
                    //仓库组合
                    if (lstAllDic.Count == 0)
                    {
                        lstAllDic.AddRange(lstDic);
                    }
                    else//进行接口返回数据的组合,不同仓库的发货方式组合
                    {
                        List<Dictionary<string, string>> lstTemDic = new List<Dictionary<string, string>>(lstAllDic);
                        lstAllDic.Clear();
                        bool isAdd = false;//是否单加当前仓库的发货方式
                        foreach (var temDic in lstTemDic)
                        {
                            if (temDic["key"].Contains("+") && temDic["key"].Contains(lstDic.FirstOrDefault()["key"]))
                            {
                                lstAllDic.Add(temDic);
                                continue;
                            }
                            if (lstDic.FirstOrDefault()["key"].Contains("+") && lstDic.FirstOrDefault()["key"].Contains(temDic["key"]))
                            {
                                lstAllDic.Add(temDic);
                                continue;
                            }
                            if (temDic["key"].Contains("+") && lstDic.FirstOrDefault()["key"].Contains("+"))
                            {
                                lstAllDic.Add(temDic);
                                continue;
                            }
                            foreach (var dic in lstDic)
                            {
                                Dictionary<string, string> newDic = new Dictionary<string, string>();
                                newDic.Add("key", temDic["key"]);
                                newDic.Add("F_HS_FreAmount", (Convert.ToDecimal(temDic["F_HS_FreAmount"]) + Convert.ToDecimal(dic["F_HS_FreAmount"])).ToString());
                                newDic.Add("F_HS_FreTiltle", temDic["F_HS_FreTiltle"] + "、" + dic["F_HS_FreTiltle"]);
                                newDic.Add("F_HS_AmountNoteUSD", temDic["F_HS_AmountNoteUSD"] + "、" + dic["F_HS_AmountNoteUSD"]);
                                lstAllDic.Add(newDic);
                                if (!temDic["key"].Contains("+"))//包含+时，单独加上
                                {
                                    isAdd = true;
                                }
                            }
                        }
                        if (!isAdd)
                        {
                            lstAllDic.AddRange(lstDic);
                            isAdd = true;
                        }
                    }
                }
            }

            return lstAllDic;
        }

        /// <summary>
        /// 根据仓库编码获取仓库HS名称
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="stockNo"></param>
        /// <returns></returns>
        private static string GetStockName(Context ctx,string stockNo)
        {
            string stockName = string.Empty;

            if (!string.IsNullOrWhiteSpace(stockNo))
            {
                string sql = string.Format(@"/*dialect*/ select F_HS_STOCKHSNAME 
											    from T_BD_STOCK a
											    inner join HS_t_Cust_EntryMaterial b on a.FSTOCKID = b.F_HS_DROPSHIPSTOCKID
											    inner join T_BD_CUSTOMER c on c.FCUSTID = b.FCUSTID
											    where a.FNUMBER='{0}'", stockNo);
                stockName = JsonUtils.ConvertObjectToString(SQLUtils.GetObject(ctx,sql, "F_HS_STOCKHSNAME"));
            }

            return stockName;
        }

    }
}
