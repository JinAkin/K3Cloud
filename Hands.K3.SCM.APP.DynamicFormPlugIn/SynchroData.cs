using Hands.K3.SCM.App.Synchro.Utils.SynchroService;
using Hands.K3.SCM.APP.Entity.DropShipping;
using Hands.K3.SCM.APP.Entity.EnumType;
using Hands.K3.SCM.APP.Entity.K3WebApi;
using Hands.K3.SCM.APP.Utils.Utils;
using Hands.K3.SCM.APP.WebService.SynchroService;
using HS.K3.Common.Abbott;
using HS.K3.Common.Mike;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Xml;

namespace Hands.K3.SCM.APP.DynamicFormPlugIn
{
    [Kingdee.BOS.Util.HotUpdate]
    [Description("同步数据--动态表单插件")]
    public class SynchroData : AbstractDynamicFormPlugIn
    {

        public void LoadData()
        {
            string k3ServerUrl = JsonUtils.ConvertObjectToString(this.View.Model.GetValue("F_HS_K3Server"));
            if (!string.IsNullOrWhiteSpace(k3ServerUrl) && k3ServerUrl.CompareTo("测试服务器") == 0)
            {
                k3ServerUrl = "http://10.2.0.150/k3cloud/";
            }
            else if (!string.IsNullOrWhiteSpace(k3ServerUrl) && k3ServerUrl.CompareTo("正式服务器") == 0)
            {
                k3ServerUrl = "http://10.2.0.150/k3cloud/";
            }

            string redisServerUrl = JsonUtils.ConvertObjectToString(this.View.Model.GetValue("F_HS_RedisServer"));
            if (!string.IsNullOrWhiteSpace(redisServerUrl) && redisServerUrl.CompareTo("测试服务器") == 0)
            {
                redisServerUrl = "10.2.0.150";
            }
            else if (!string.IsNullOrWhiteSpace(redisServerUrl) && redisServerUrl.CompareTo("正式服务器") == 0)
            {
                redisServerUrl = "221.120.177.22";
            }

            long redsiDb = long.Parse(JsonUtils.ConvertObjectToString(this.View.Model.GetValue("F_HS_RedisDB")));
            string billNo = JsonUtils.ConvertObjectToString(this.View.Model.GetValue("F_HS_BillNo"));
        }
        public void SynchroDataTo(Context ctx, SynchroDataType dataType, SynchroDirection direction, List<string> numbers = null, bool flag = true)
        {
            HttpResponseResult result = null;
            try
            {
                if (direction == SynchroDirection.ToK3)
                {
                    result = SynchroDataHelper.SynchroDataToK3(ctx, dataType);
                }
                else if (direction == SynchroDirection.ToHC)
                {
                    result = SynchroDataHelper.SynchroDataToHC(ctx, dataType, null, numbers, flag);
                }
               
                if (result != null)
                {
                    if (result.Success)
                    {
                        this.View.ShowMessage(string.Format("同步{0}至{1}成功！", LogHelper.GetDataSourceTypeDesc(dataType), direction.ToString()) + result.Message, Kingdee.BOS.Core.DynamicForm.MessageBoxType.Notice);
                    }
                    else
                    {
                        this.View.ShowErrMessage("", string.Format("同步{0}至{1}失败！", LogHelper.GetDataSourceTypeDesc(dataType), direction.ToString()) + result.Message, Kingdee.BOS.Core.DynamicForm.MessageBoxType.Error);
                    }
                }
                else
                {
                    this.View.ShowErrMessage("", string.Format("同步{0}至{1}失败！", LogHelper.GetDataSourceTypeDesc(dataType), direction.ToString()), Kingdee.BOS.Core.DynamicForm.MessageBoxType.Error);
                }
            }
            catch (Exception ex)
            {
                this.View.ShowErrMessage(ex.ToString(), string.Format("同步{0}至{1}失败！", LogHelper.GetDataSourceTypeDesc(dataType), direction.ToString()), Kingdee.BOS.Core.DynamicForm.MessageBoxType.Error);
            }
        }
        private HttpResponseResult SynchroDropShipping(SynchroDataType dataType)
        {
            HttpResponseResult result = null;
            SynchroService cli = new SynchroService();

            DropShippingInfo info = new DropShippingInfo();
            info.Token = "HANDS-test";
            info.FCustId = "107325";
            info.TimeStamp = TimeHelper.GetTimeStamp(DateTime.Now);
            
            info.SignMsg = EncryptUtil.MD5Encrypt(info.FCustId + info.TimeStamp, "DROPSHIPPING");

            switch (dataType)
            {
                case SynchroDataType.DropShippingSalOrder:
                    info.DataType = dataType.ToString();
                    List<DSSaleOrder> orders = null;
                    DSSaleOrder order = null;
                    List<DSSaleOrderEntry> entries = null;
                    DSSaleOrderEntry entry = null;

                    orders = new List<DSSaleOrder>();

                    for (int i = 0; i < 5; i++)
                    {
                        order = new DSSaleOrder();

                        order.FBillNo = i.ToString();
                        order.FDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd hh:mm"));
                        order.F_HS_SaleOrderSource = "DropShippingOrder";
                        order.F_HS_B2CCustId = info.FCustId;
                        order.FSettleCurrId = "USD";
                        order.F_HS_AmountDeclared = 99.9M;
                        order.F_HS_FreightDeclared = 88.8M;
                        order.F_HS_DeclaredCurrId = "USD";
                        order.F_HS_DeclaredCurrId = "USD";
                        order.F_HS_RecipientCountry = "HR";
                        order.F_HS_DeliveryProvinces = "Slavonija";
                        order.F_HS_DeliveryCity = "Djakovo";
                        order.F_HS_DeliveryAddress = "Marina Drzica 19";
                        order.F_HS_PostCode = "31400";
                        order.F_HS_DeliveryName = "Ljiljana Haas";
                        order.F_HS_MobilePhone = "38763520647";
                        order.F_HS_DropShipDeliveryChannel = "SZ DHL(Solid), 2-8 business days";
                        order.F_HS_PlatformCustomerID = "100";
                        order.F_HS_PlatformCustomerEmail = "appfuse@hotmail.com";
                        entries = new List<DSSaleOrderEntry>();

                        for (int j = 0; j < 5; j++)
                        {
                            if (j == 0)
                            {
                                entry = new DSSaleOrderEntry();
                                entry.FMaterialId = "1000100084";
                                entry.FQTY = 5M;
                                entry.F_HS_StockID = "301";
                                entry.F_HS_StockID = "DLC001";
                                entries.Add(entry);
                            }
                            if (j == 1)
                            {
                                entry = new DSSaleOrderEntry();
                                entry.FMaterialId = "1000100085";
                                entry.FQTY = 5M;
                                entry.F_HS_StockID = "302";
                                entry.F_HS_StockID = "DLC003";
                                entries.Add(entry);
                            }

                            if (j == 2)
                            {
                                entry = new DSSaleOrderEntry();
                                entry.FMaterialId = "1000100086";
                                entry.FQTY = 5M;
                                entry.F_HS_StockID = "303";
                                entry.F_HS_StockID = "DLC019";
                                entries.Add(entry);
                            }
                        }

                        order.OrderEntry = entries;
                        orders.Add(order);
                    }

                    info.SaleOrders = orders;
                    break;
                case SynchroDataType.Inventroy:
                case SynchroDataType.ImportLogis:
                    info.DataType = dataType.ToString();
                    break;

            }

            string json = JsonConvert.SerializeObject(info);
            //string ret = cli.SynchroData(json);
            //result = JsonConvert.DeserializeObject<HttpResponseResult>(ret);

            result = RequestServices(json);

            if (result != null)
            {
                if (result.Success)
                {
                    this.View.ShowMessage(string.Format("同步{0}至{1}成功！", "DropShipping订单", "K3") + result.Message, Kingdee.BOS.Core.DynamicForm.MessageBoxType.Notice);
                }
                else
                {
                    this.View.ShowErrMessage("", string.Format("同步{0}至{1}失败！", "DropShipping订单", "K3") + result.Message, Kingdee.BOS.Core.DynamicForm.MessageBoxType.Error);
                }
            }
            else
            {
                this.View.ShowErrMessage("", string.Format("同步{0}至{1}失败！", "DropShipping订单", "K3"), Kingdee.BOS.Core.DynamicForm.MessageBoxType.Error);
            }

            return result;

        }

        private HttpResponseResult RequestServices(string json)
        {

            //if (!string.IsNullOrWhiteSpace(json))
            //{
            //    HttpClient http = new HttpClient() { IsProxy = true };

            //    http.Url = "http://localhost/K3Cloud/Services/SynchroServiceBus.asmx/SynchroData";
            //    http.Content = "&json=" + HttpUtility.UrlEncode(json, Encoding.UTF8);

            //    string ret = string.Empty;
            //    try
            //    {                                        
            //        ret = http.PostData();
            //        return JsonConvert.DeserializeObject<HttpResponseResult>(ret);
            //    }
            //    catch (Exception ex)
            //    {
            //        HttpResponseResult result = new HttpResponseResult();
            //        result.Success = false;
            //        result.Message = ex.Message + Environment.NewLine + ex.StackTrace;

            //        return result;
            //    }
            //}

            try
            {
                string Url = "http://localhost/K3Cloud/Services/SynchroServiceBus.asmx";
                string method = "SynchroData";//The method name
                string postData = "&json=" + HttpUtility.UrlEncode(json, Encoding.UTF8);//required parameter
                ASCIIEncoding encoding = new ASCIIEncoding();
                byte[] dataArray = Encoding.UTF8.GetBytes(postData);//Set coding specifications

                HttpWebRequest request2 = (HttpWebRequest)WebRequest.Create(Url + "/" + method);//Creating a Web service request
                request2.Method = "Post";
                request2.ContentType = "application/x-www-form-urlencoded";
                request2.ContentLength = dataArray.Length;

                Stream Writer = request2.GetRequestStream();//Gets the Stream object used to write to the request data
                Writer.Write(dataArray, 0, dataArray.Length);//Writes parameter data to the request data stream
                Writer.Close();

                HttpWebResponse response2 = (HttpWebResponse)request2.GetResponse();//Get the response
                StreamReader stream2 = new StreamReader(response2.GetResponseStream(), Encoding.UTF8);//Get the response Stream
                string result3 = stream2.ReadToEnd();
                stream2.Close();

                return JsonConvert.DeserializeObject<HttpResponseResult>(result3);

            }
            catch (Exception ex)
            {
                HttpResponseResult result = new HttpResponseResult();
                result.Success = false;
                result.Message = ex.Message + Environment.NewLine + ex.StackTrace;

                return result;
            }

          
        }

  
        public override void ButtonClick(ButtonClickEventArgs e)
        {
            base.ButtonClick(e);

            switch (e.Key)
            {
                case "F_HS_SYNCHROSALORDERSTATUS":
                    SynchroDataTo(this.Context, SynchroDataType.SalesOrderPayStatus, SynchroDirection.ToK3);
                    break;

                case "F_HS_SYNCHROSALORDER":
                    SynchroDataTo(this.Context, SynchroDataType.SaleOrder, SynchroDirection.ToK3);
                    break;

                case "F_HS_SYNCHROINVENTORY":
                    SynchroDataTo(this.Context, SynchroDataType.Inventroy, SynchroDirection.ToHC);
                    break;

                case "F_HS_SYNCHROCUSTOMER":
                    SynchroDataTo(this.Context, SynchroDataType.Customer, SynchroDirection.ToK3);
                    break;

                case "F_HS_SYNCHROCUSTOMERBYEXCEL":
                    SynchroDataTo(this.Context, SynchroDataType.CustomerByExcel, SynchroDirection.ToK3);
                    break;

                case "F_HS_CUSTOMERADDRESS":
                    SynchroDataTo(this.Context, SynchroDataType.CustomerAddress, SynchroDirection.ToK3);
                    break;

                case "F_HS_CUSTADDRBYEXCEL":
                    SynchroDataTo(this.Context, SynchroDataType.CustomerAddressByExcel, SynchroDirection.ToK3);
                    break;

                case "F_HS_SYNOFFLINESALORDER":
                    SynchroDataTo(this.Context, SynchroDataType.SaleOrderOffline, SynchroDirection.ToHC, null, false);
                    break;
                case "F_HS_SYNOFFLINESALORDER1":
                    SynchroDataTo(this.Context, SynchroDataType.SaleOrderOffline, SynchroDirection.ToHC, null, true);
                    break;

                case "F_HS_DELCUSTOMERADDRESS":
                    SynchroDataTo(this.Context, SynchroDataType.DelCustomerAddress, SynchroDirection.ToK3);
                    break;

                case "F_HS_SYNSALORDERSTATUS":
                    SynchroDataTo(this.Context, SynchroDataType.SaleOrderStatus, SynchroDirection.ToHC);
                    break;

                case "F_HS_MATERIAL":
                    SynchroDataTo(this.Context, SynchroDataType.Material, SynchroDirection.ToHC);
                    break;

                case "F_HS_SYNONTHEWAY":
                    SynchroDataTo(this.Context, SynchroDataType.OnTheWay, SynchroDirection.ToHC);
                    break;

                case "F_HS_SYNCHRORECEIVEBILL":
                    SynchroDataTo(this.Context, SynchroDataType.ReceiveBill, SynchroDirection.ToK3);
                    break;
                case "F_HS_SYNOFFLINESALORDER_":
                    SynchroDataTo(this.Context, SynchroDataType.SaleOrderOffline, SynchroDirection.ToHC);
                    break;
                case "F_HS_SYNTRACKINGNUMBER":
                    SynchroDataTo(this.Context, SynchroDataType.ImportLogis, SynchroDirection.ToHC);
                    break;
                case "F_HS_SYNLOGISTIC":
                    SynchroDataTo(this.Context, SynchroDataType.DeliveryNoticeBill, SynchroDirection.ToHC);
                    break;
                case "F_HS_DROPSHIPPING":
                    SynchroDropShipping(SynchroDataType.DropShippingSalOrder);
                    break;
                case "F_HS_DROPSHIPPING1":
                    SynchroDropShipping(SynchroDataType.ImportLogis);
                    break;
                case "F_HS_DROPSHIPPING2":
                    SynchroDropShipping(SynchroDataType.Inventroy);
                    break;
                case "F_HS_DROPSHIPPING3":
                    SynchroDataTo(this.Context,SynchroDataType.DropShippingSalOrder,SynchroDirection.ToK3);
                    break;
                default:
                    break;
            }
        }

    }
}
