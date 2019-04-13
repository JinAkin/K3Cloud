using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web.Services;
using System.Web.Script.Services;
using Newtonsoft.Json;
using Kingdee.BOS;
using Kingdee.BOS.Authentication;
using Kingdee.BOS.DataCenterInfo;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.BOS.Performance.Common;
using System.IO;
using Newtonsoft.Json.Linq;
using Hands.K3.SCM.APP.Entity.K3WebApi;
using Hands.K3.SCM.APP.Utils.Utils;
using Hands.K3.SCM.App.Synchro.Utils.SynchroService;
using Hands.K3.SCM.App.Synchro.Utils.K3WebApi;
using Hands.K3.SCM.APP.WebService.SynchroService.Entity;
using Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject;
using Kingdee.BOS.Orm.DataEntity;
using Hands.K3.SCM.APP.Entity.SynDataObject;
using Hands.K3.SCM.APP.Entity.SynDataObject.SalOutStock;
using Hands.K3.SCM.APP.Entity.SynDataObject.SaleOrder;
using HS.K3.Common.Abbott;
using Hands.K3.SCM.APP.Entity.EnumType;
using HS.K3.Common.Mike;
using HS.K3.Common.Jenna;
using Hands.K3.SCM.APP.Entity.DropShipping;
using Hands.K3.SCM.APP.Entity.DropShipping.DeliveryNotice;
using HS.K3.Common.Mike.Model;
using Kingdee.BOS.App.Data;
using System.Data;
using System.Web;
using System.Runtime.Serialization.Json;

namespace Hands.K3.SCM.APP.WebService.SynchroService
{
    [ToolboxItem(false)]
    [WebService(Namespace = "http://schema.K3cloud.com/HSServiceBus/",
        Name = "HSServiceClient",
        Description = "HC同步服务")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ScriptService]
    public partial class SynchroService : System.Web.Services.WebService
    {
        decimal lastOrdAmo = 0;//记录当前订单前几个订单的总金额
        /// <summary>
        /// 登录系统
        /// </summary>
        /// <param name="dataCenterNumber">待登录账套</param>
        /// <param name="userName">账户</param>
        /// <param name="passWord">密码</param>
        /// <returns></returns>
        private Context AuthByUserToken(string usertoken)
        {
            string path = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "\\WebEDI.config";
            if (!File.Exists(path))
            {
                throw new KDException("400", string.Format("访问令牌[{0}]被拒绝，错误的请求！", usertoken));
            }
            WebToken webToken = JsonConvert.DeserializeObject<List<WebToken>>(File.ReadAllText(path, Encoding.Default))
                .Where(o => o.token == usertoken).FirstOrDefault();
            if (webToken == null)
            {
                throw new KDException("401", string.Format("访问令牌[{0}]被拒绝，访问令牌无效！", usertoken));
            }

            return AuthByUser(webToken.dcNumber, webToken.userName, webToken.passWord);
        }


        private Context AuthByUser(string DataCenter, string userName, string Password)
        {
            if (DataCenter.IsNullOrEmptyOrWhiteSpace()
                || userName.IsNullOrEmptyOrWhiteSpace()
                || Password.IsNullOrEmptyOrWhiteSpace())
            {
                throw new KDException("400", string.Format("参数无效！"));
            }
            List<DataCenter> dataCentersFromMC = DataCenterService
                .GetDataCentersFromMC("", Kingdee.BOS.Context.DataBaseCategory.Normal, null);

            DataCenter dataCenter = (
                        from p in dataCentersFromMC
                        where p.Number.EqualsIgnoreCase(DataCenter)
                        select p).FirstOrDefault<DataCenter>();

            LoginInfo loginInfo = new LoginInfo();
            loginInfo.AcctID = dataCenter.Id;
            loginInfo.AuthenticateType = AuthenticationType.PwdAuthentication;
            loginInfo.Username = userName;
            loginInfo.Password = ConfidentialDataSecurityUtil.CipherText(Password);
            loginInfo.Lcid = 2052;
            loginInfo.LoginType = LoginType.NormalERPLogin;

            LoginResult loginResult = LoginServiceHelper.Login(PerformanceContext.Create(PerfArgsCollectionType.CallDirectly)
                , "", loginInfo);
            if (!loginResult.IsSuccessByAPI && loginResult.Context == null)
            {
                throw new KDException(loginResult.MessageCode, string.Format(loginResult.Message));
            }
            return loginResult.Context;
        }

        [WebMethod(Description = @"
        <table>
            <tr>
                <td>Summary:</td><td>用户登陆</td>
            </tr>
            <tr>
                <td>Parameters:</td><td>&nbsp;</td>
            </tr>
            <tr>
                <td>UserName:</td><td>用户名</td>
            </tr>
            <tr>
                <td>Pwd:</td><td>密码</td>
            </tr>
            <tr>
                <td>Result:</td><td>返回登录信息</td>
            </tr>
        </table>", EnableSession = true)]
        public string Login(string UserName, string Pwd)
        {
            //ResponseResult result = new ResponseResult();

            //try
            //{
            //    Kingdee.BOS.Context oContext = AuthByUser(UserSession._DataCenterNumber, UserName, Pwd);
            //    result.Success = "true";
            //    result.Result = oContext.UserToken;
            //}
            //catch (Exception ex)
            //{
            //    result.Message = ex.Message;
            //}
            //
            return null;
        }
        [WebMethod(Description = @"
        <table>
            <tr>
                <td>Summary:</td><td>获取销售订单编码</td>
            </tr>
            <tr>
                <td>Parameters:</td><td>&nbsp;</td>
            </tr>
            <tr>
                <td>token:</td><td>销售订单编码</td>
            </tr>
            <tr>
                <td>orderNos:</td><td>销售订单编码</td>
            </tr>
            <tr>
                <td>Result:</td><td>返回获取销售订单编码信息</td>
            </tr>
        </table>", EnableSession = true)]
        public void GetOrderNos(string token, string orderNos)
        {
            ResponseResult result = new ResponseResult();
            Context ctx = null;

            string response = string.Empty;

            try
            {
                if (!string.IsNullOrWhiteSpace(token))
                {
                    ctx = AuthByUserToken(token);
                    HashSet<string> numbers = null;

                    if (!string.IsNullOrWhiteSpace(orderNos))
                    {
                        numbers = new HashSet<string>();

                        JArray jArr = JArray.Parse(orderNos);

                        if (jArr != null && jArr.Count > 0)
                        {
                            for (int i = 0; i < jArr.Count; i++)
                            {
                                if (jArr[i] != null)
                                {
                                    numbers.Add(JsonUtils.GetFieldValue(jArr[i], "orders_id_" + i));
                                }
                            }
                        }
                        if (numbers != null && numbers.Count > 0)
                        {
                            result.Success = true;
                            result.Message = "销售订单【" + string.Join(",", numbers.Select(o => o)) + "】在" + DateTime.Now + "写入Redis";
                            SynchroDataHelper.SynchroDataToK3(ctx, SynchroDataType.SaleOrder, true, numbers);

                        }

                        result.Success = true;
                        result.Message = "销售订单编码读取成功！";
                    }
                    else
                    {
                        result.Success = false;
                        result.Message = "无销售订单编码！";
                    }
                }
                else
                {
                    result.Success = false;
                    result.Message = "登陆令牌不能为空！";
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message + System.Environment.NewLine + ex.StackTrace;
            }

            LogUtils.WriteSynchroLog(ctx, SynchroDataType.SaleOrder, result.Message);
            Context.Response.Write(JsonConvert.SerializeObject(response));

        }

        [WebMethod(Description = @"
        <table>
            <tr>
                <td>Summary:</td><td>调用WebApi进行单据操作</td>
            </tr>
            <tr>
                <td>Parameters:</td><td>&nbsp;</td>
            </tr>
            <tr>
                <td>ctx:</td><td>K3Cloud上下文</td>
            </tr>
            <tr>
                <td>dataType:</td><td>操作类型</td>
            </tr>
            <tr>
                <td>formId:</td><td>单据ID</td>
            </tr>
            <tr>
                <td>numbers:</td><td>单据编码集合</td>
            </tr>
            <tr>
                <td>pkIds:</td><td>单据内码集合</td>
            </tr>
            <tr>
                <td>json:</td><td>json文本数据</td>
            </tr>
        </table>
        ", EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public void ExecuteOperate(string ctx, string dataType, string operateType, string formId, string numbers = null, string pkIds = null, string json = null)
        {
            HttpResponseResult result = null;
            result = InvokeWebApi.InvokeBatchOperate(JsonConvert.DeserializeObject<Context>(ctx)
                                                    , (SynchroDataType)Enum.Parse(typeof(SynchroDataType), dataType)
                                                    , (SynOperationType)Enum.Parse(typeof(SynOperationType), operateType)
                                                    , formId
                                                    , string.IsNullOrWhiteSpace(numbers) ? null : JsonConvert.DeserializeObject<List<string>>(numbers)
                                                    , string.IsNullOrWhiteSpace(pkIds) ? null : JsonConvert.DeserializeObject<List<int>>(pkIds)
                                                    , json);
            Context.Response.Write(JsonConvert.SerializeObject(result));
        }
        [WebMethod(Description = @"
        <table>
            <tr>
                <td>Summary:</td><td>The user requests the K3 service to return the specified data</td>
            </tr>
            <tr>
                <td>Parameters:</td><td>&nbsp;</td>
            </tr>
            <tr>
                <td>json:</td><td>required parameter</td>
            </tr>
        </table>
        ", EnableSession = true)]
        public void SynchroData(string json)
        {
            ResponseResult result = null;
            DropShippingInfo info = null;
            Context ctx = null;
            string response = string.Empty;

            try
            {
                info = GetDropShippingInfo(json);

                if (info != null)
                {
                    if (!string.IsNullOrWhiteSpace(info.Token))
                    {
                        if (info.Token.Contains("-test"))
                        {
                            ctx = AuthByUserToken("TestHandsToken");
                        }
                        else
                        {
                            ctx = AuthByUserToken("HandsToken");
                        }

                        if (ctx != null)
                        {
                            if (CryptCheck(info))
                            {
                                SynchroDataType sDataType = (SynchroDataType)Enum.Parse(typeof(SynchroDataType), info.DataType);
                                IEnumerable<AbsSynchroDataInfo> datas = GetK3Datas<IEnumerable<AbsSynchroDataInfo>>(ctx, (SynchroDataType)Enum.Parse(typeof(SynchroDataType), info.DataType), info.FCustId);

                                if (datas != null && datas.Count() > 0)
                                {
                                    if (AuthByUserToken(datas.ElementAt(0), info.Token))
                                    {
                                        if (sDataType != SynchroDataType.DropShippingSalOrder)
                                        {
                                            result = new ResponseResult();
                                            result.Success = true;
                                            result.Message = string.Format("DataType:{0},User:{1},Request service Success!", info.DataType, info.FCustId);
                                            result.Result = datas;

                                            if (sDataType == SynchroDataType.ImportLogis)
                                            {
                                                UpdateAfterSychro(ctx, datas);
                                            }

                                        }
                                        else
                                        {
                                            if (!string.IsNullOrWhiteSpace(json))
                                            {
                                                JObject jObject = JObject.Parse(json);
                                                result = SynchroDataToK3(ctx, jObject, datas.ElementAt(0));

                                                if (result == null)
                                                {
                                                    result = new ResponseResult();
                                                    result.Success = false;
                                                    result.Message = string.Format("DataType:{0},User:{1},Request service failed", info.DataType, info.FCustId);
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        result = new ResponseResult();
                                        result.Success = false;
                                        result.Message = string.Format("DataType:{0},User:{1},Wrong token! Please check token!", info.DataType, info.FCustId);
                                    }
                                }
                                else
                                {
                                    result = new ResponseResult();
                                    result.Success = false;
                                    result.Message = string.Format("DataType:{0},User:{1},The requested data was not found!", info.DataType, info.FCustId);
                                }
                            }
                            else
                            {
                                result = new ResponseResult();
                                result.Success = false;
                                result.Message = string.Format("DataType:{0},User:{1},Failure Of Signature", info.DataType, info.FCustId);
                            }
                        }
                        else
                        {
                            result = new ResponseResult();
                            result.Success = false;
                            result.Message = string.Format("DataType:{0},User:{1},Failed to get the context of K3Cloud, please check whether token is correct!", info.DataType, info.FCustId);

                            ctx = AuthByUserToken("TestHandsToken");
                        }

                    }
                    else
                    {
                        result = new ResponseResult();
                        result.Success = false;
                        result.Message = string.Format("DataType:{0},User:{1},The token cannot be empty!", info.DataType, info.FCustId);

                        ctx = AuthByUserToken("TestHandsToken");
                    }
                }
                else
                {
                    result = new ResponseResult();
                    result.Success = false;
                    result.Message = string.Format("Wrong parameter passed!");

                    ctx = AuthByUserToken("TestHandsToken");
                }

            }
            catch (Exception ex)
            {
                result = new ResponseResult();
                result.Success = false;
                result.Message = string.Format("Synchronous exception:") + ex.Message + ex.StackTrace;

                ctx = AuthByUserToken("TestHandsToken");
            }

            LogUtils.WriteSynchroLog(ctx, SynchroDataType.DropShippingSalOrder, result.Message);
            response = JsonConvert.SerializeObject(result);

            Context.Response.Write(response);
        }

        /// <summary>
        /// 获取DropShipping客户token和签名等信息
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        private DropShippingInfo GetDropShippingInfo(string json)
        {
            DropShippingInfo info = null;

            if (!string.IsNullOrWhiteSpace(json))
            {
                JObject jObject = JObject.Parse(json);

                if (jObject != null)
                {
                    info = new DropShippingInfo();
                    info.Token = JsonUtils.GetFieldValue(jObject, "Token");
                    info.DataType = JsonUtils.GetFieldValue(jObject, "DataType");
                    info.FCustId = JsonUtils.GetFieldValue(jObject, "FCustId");
                    info.SignMsg = JsonUtils.GetFieldValue(jObject, "SignMsg");
                    info.TimeStamp = JsonUtils.GetFieldValue(jObject, "TimeStamp");
                }
            }

            return info;
        }

        /// <summary>
        /// DropShipping签名校验
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        private bool CryptCheck(DropShippingInfo info)
        {
            if (info != null)
            {
                string signMsg = EncryptUtil.MD5Encrypt(info.FCustId + info.TimeStamp, "DROPSHIPPING");
                if (signMsg.CompareTo(info.SignMsg) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 验证token
        /// </summary>
        /// <param name="dataInfo"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private bool AuthByUserToken(AbsSynchroDataInfo dataInfo, string token)
        {
            if (dataInfo != null && !string.IsNullOrWhiteSpace(dataInfo.UserToken)
                && !string.IsNullOrWhiteSpace(token))
            {
                if (dataInfo.UserToken.CompareTo(token) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 响应客户请求，返回K3数据
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="DataType"></param>
        /// <param name="numbers"></param>
        /// <returns></returns>
        public T GetK3Datas<T>(Context ctx, SynchroDataType DataType, string custNo, List<string> materialIds = null)
        {
            List<K3SalOrderInfo> orders = null;
            List<K3SalOrderEntryInfo> entries = null;
            K3SalOrderInfo order = null;
            K3SalOrderEntryInfo entry = null;
            List<InventoryInfo> inventories = null;
            InventoryInfo inventory = null;
            List<DeliveryNotice> tracks = null;
            DeliveryNotice track = null;

            DynamicObjectCollection coll = GetDynamicObjects(ctx, DataType, custNo, materialIds);

            if (coll != null && coll.Count > 0)
            {
                switch (DataType)
                {
                    case SynchroDataType.DropShippingSalOrder:
                        if (materialIds == null)
                        {
                            orders = new List<K3SalOrderInfo>();
                            order = new K3SalOrderInfo();

                            foreach (var item in coll)
                            {
                                order.FCustId = SQLUtils.GetFieldValue(item, "FCustId");
                                order.FSalerId = SQLUtils.GetFieldValue(item, "FSELLER");
                                order.UserToken = SQLUtils.GetFieldValue(item, "F_HS_DropShipRequestToken");
                                order.FCustLevel = SQLUtils.GetFieldValue(item, "FCustTypeId");
                                order.F_HS_DropShipOrderPrefix = SQLUtils.GetFieldValue(item, "F_HS_DropShipOrderPrefix");
                                order.F_HS_YNFixedDropShipStock = SQLUtils.GetFieldValue(item, "F_HS_YNFixedDropShipStock").Equals("1") ? true : false;
                                orders.Add(order);
                            }

                            return (T)(object)orders;
                        }
                        else
                        {
                            entries = new List<K3SalOrderEntryInfo>();

                            foreach (var item in coll)
                            {
                                if (item != null)
                                {
                                    entry = new K3SalOrderEntryInfo();
                                    entry.FMaterialId = SQLUtils.GetFieldValue(item, "FMATERIALID");
                                    entry.FStockId = SQLUtils.GetFieldValue(item, "FSTOCKID");
                                    entry.FTAXPRICE = Convert.ToDecimal(SQLUtils.GetFieldValue(item, "price"));
                                    entry.F_HS_FGroup = SQLUtils.GetFieldValue(item, "FCustTypeId");
                                    entries.Add(entry);
                                }
                            }
                            return (T)(object)entries;
                        }

                    case SynchroDataType.Inventroy:
                        inventories = new List<InventoryInfo>();
                        var iGro = coll.GroupBy(c => c["FNUMBER"]);

                        if (iGro != null && iGro.Count() > 0)
                        {
                            foreach (var ig in iGro)
                            {
                                if (ig != null && ig.Count() > 0)
                                {
                                    foreach (var i in ig)
                                    {
                                        inventory = new InventoryInfo();
                                        inventory.UserToken = SQLUtils.GetFieldValue(i, "F_HS_DropShipRequestToken");
                                        inventory.FixId = SQLUtils.GetFieldValue(i, "FNUMBER");
                                        inventory.SrcNo = inventory.FixId;
                                        inventory.StockId = SQLUtils.GetFieldValue(i, "FSTOCKID");
                                        inventory.Quantity = Convert.ToDouble(SQLUtils.GetFieldValue(i, "availableQOH"));
                                        inventories.Add(inventory);
                                    }
                                }
                            }
                        }
                        return (T)(object)inventories;
                    case SynchroDataType.ImportLogis:
                        var tGro = coll.GroupBy(c => c["BillNo"]);
                        tracks = new List<DeliveryNotice>();

                        if (tGro != null && tGro.Count() > 0)
                        {
                            foreach (var item in tGro)
                            {

                                track = new DeliveryNotice();

                                track.UserToken = SQLUtils.GetFieldValue(item.ElementAt(0), "F_HS_DropShipRequestToken");
                                track.OrderBillNo = SQLUtils.GetFieldValue(item.ElementAt(0), "OrderBillNo");

                                if (!string.IsNullOrWhiteSpace(track.OrderBillNo) && track.OrderBillNo.Contains(track.UserToken))
                                {
                                    track.OrderBillNo = track.OrderBillNo.Replace(track.UserToken, "");
                                }
                                track.BillNo = SQLUtils.GetFieldValue(item.ElementAt(0), "BillNo");

                                track.DeliDate = SQLUtils.GetFieldValue(item.ElementAt(0), "DeliDate");
                                track.CarriageNO = SQLUtils.GetFieldValue(item.ElementAt(0), "CarriageNO");
                                track.QueryURL = SQLUtils.GetFieldValue(item.ElementAt(0), "QueryURL");
                                track.LogisticsChannel = SQLUtils.GetFieldValue(item.ElementAt(0), "LogisticsChannel");

                                tracks.Add(track);
                            }
                        }
                        return (T)(object)tracks;
                    default:
                        return default(T);
                }
            }
            return default(T);
        }

        /// <summary>
        /// 返回指定数据类型的SQL
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="numbers"></param>
        /// <returns></returns>
        public string GetSQL(Context ctx, SynchroDataType dataType, string custNo, List<string> materialIds = null)
        {
            if (!string.IsNullOrWhiteSpace(custNo))
            {
                switch (dataType)
                {
                    case SynchroDataType.DropShippingSalOrder:
                        string sql = string.Empty;
                        if (materialIds == null)
                        {
                            sql = string.Format(@" /*dialect*/  select a.FNUMBER as FCustId,a.F_HS_DropShipRequestToken,a.F_HS_DropShipOrderPrefix
														         ,a.F_HS_YNFixedDropShipStock, d.FNUMBER as FCustTypeId,b.FNUMBER as FSELLER
														         from T_BD_CUSTOMER a
														         inner join V_BD_SALESMAN b on b.FID = a.FSELLER
														         inner join T_BAS_ASSISTANTDATAENTRY_L c on a.FCustTypeId = c.FENTRYID and c.FLOCALEID = 2052
														         inner join T_BAS_ASSISTANTDATAENTRY d on d.FentryID = c.FentryID
														         where a.FNUMBER = '{0}' 
														         and a.FUSEORGID = 100035", custNo);
                            return sql;
                        }
                        else
                        {
                            List<SqlParam> sqlParams = new List<SqlParam>();
                            SqlParam sqlParam0 = new SqlParam("@custNo", KDDbType.String, custNo);
                            SqlParam sqlParam1 = new SqlParam("@materialIds", KDDbType.String, string.Join("','", materialIds));
                            SqlParam parOutput = new SqlParam("@sql", KDDbType.AnsiString, ParameterDirection.Output);
                            parOutput.Direction = ParameterDirection.Output;
                            parOutput.Size = 2000;

                            sqlParams.Add(sqlParam0);
                            sqlParams.Add(sqlParam1);
                            sqlParams.Add(parOutput);
                            var retValue = DBUtils.ExecuteStoreProcedure(ctx, "HS_SP_GetPrice", sqlParams);
                            return retValue.FirstOrDefault().Value.ToString();
                        }

                    case SynchroDataType.Inventroy:

                        if (IsFirstSynchro(ctx, custNo))
                        {
                            return string.Format(@"/*dialect*/ declare @guidString nvarchar(50)
                                                    select @guidString=NewID()
                                                    select  e.FNAME productGroupName,k.Fnumber FSTOCKID,
                                                           b.FNUMBER,d.FNAME,d.FSPECIFICATION ,f.FNAME unitName	
	                                                       ,sum(a.FBASEQTY/h.fConvertnumerator) quantity,o.customerNumber as FCUSTID,o.F_HS_DropShipRequestToken
                                                    into #jskc	  
                                                    From  T_STK_INVENTORY a
                                                    inner join T_BD_MATERIAL b on a.FMATERIALID=b.FMATERIALID
                                                    inner join T_BD_MATERIAL_L d on b.FMATERIALID=d.FMATERIALID and d.FLOCALEID=2052
                                                    left join T_BD_MATERIALGROUP_L e on b.Fmaterialgroup=e.fid and e.FLOCALEID=2052
                                                    left join T_BD_unit_L f on a.FSTOCKUNITID=f.FUNITID and f.FLOCALEID=2052
                                                    left join T_BD_LOTMASTER g on a.fLot=g.flotID
                                                    left join T_BD_UNITCONVERTRATE h on a.FSTOCKUNITID=h.fcurrentUnitID and a.fbaseunitID=h.FdestUnitID
                                                    inner join T_BD_STOCK i on a.FSTOCKID=i.FSTOCKID
                                                    inner join T_BAS_ASSISTANTDATAENTRY_L j ON i.F_HS_DLC=j.FENTRYID and j.FLOCALEID=2052
                                                    inner join T_BAS_ASSISTANTDATAENTRY k ON j.FentryID=k.FentryID
													inner join (
													            select distinct h.FNUMBER customerNumber,h.F_HS_DropShipRequestToken,g.FNUMBER DLCNumber 
															    from HS_t_DropShipStockEntity a
		                                                        inner join T_BD_STOCK c on a.F_HS_DropShipStock=c.FSTOCKID
                                                                inner join T_BAS_ASSISTANTDATAENTRY_L f ON c.F_HS_DLC=f.FENTRYID and f.FLOCALEID=2052
		                                                        inner join T_BAS_ASSISTANTDATAENTRY g ON f.FentryID=g.FentryID
															    inner join T_BD_CUSTOMER h on a.FCUSTID=h.FCUSTID
		                                                        Where h.FNUMBER = '{0}'
		                                                       ) o on k.fnumber=o.DLCNumber
                                                    where   a.FBASEQTY>0 and  a.FSTOCKSTATUSID=10000 and i.F_HS_TJ='1'
                                                    group by  e.FNAME,k.Fnumber,
                                                           b.FNUMBER,d.FNAME,d.FSPECIFICATION,f.FNAME,o.customerNumber,o.F_HS_DropShipRequestToken
                                                    order by e.FNAME desc

                                                    insert into tbl_store_availableQOH_ds(FCUSTID,F_HS_DropShipRequestToken,FSTOCKID,FNUMBER,FNAME,FSPECIFICATION ,unitName,availableQOH, updateTag , updateTime, guidString) 
                                                    select a.FCUSTID,a.F_HS_DropShipRequestToken,a.FSTOCKID,a.FNUMBER,a.FNAME,a.FSPECIFICATION , a.unitName, a.quantity-isnull(b.FQty ,0) availableQOH,'original',getDate(), @guidString
                                                    from #jskc a
                                                    left join 
		                                                    (SELECT d.FNUMBER,g.Fnumber fstockID,sum(c.FREMAINOUTQTY) FQty 	FROM T_SAL_ORDER a 
		                                                    INNER JOIN T_SAL_ORDERENTRY b ON a.FID=b.FID 
		                                                    inner join T_SAL_ORDERENTRY_R c on a.fid=c.fid and b.fentryID=c.fentryID
		                                                    INNER JOIN T_BD_MATERIAL d on b.FMATERIALID=d.FMATERIALID
		                                                    inner join T_BD_STOCK e on b.F_HS_STOCKID=e.FSTOCKID
                                                            inner join T_BAS_ASSISTANTDATAENTRY_L f ON e.F_HS_DLC=f.FENTRYID
		                                                    inner join T_BAS_ASSISTANTDATAENTRY g ON f.FentryID=g.FentryID
		                                                    Where a.FCLOSESTATUS<>'B' AND a.FCANCELSTATUS<>'B' AND b.FMRPCLOSESTATUS<>'B' and a.FDOCUMENTSTATUS<>'Z'
		                                                    group by d.FNUMBER,g.Fnumber
		                                                    ) b on a.fnumber=b.fnumber and a.fstockID=b.fstockID

                                                    where a.quantity-isnull(b.FQty ,0)>0 
                                                    insert into tbl_store_availableQOH_Log_ds(FCUSTID,F_HS_DropShipRequestToken,FSTOCKID,FNUMBER,FNAME,FSPECIFICATION ,unitName,availableQOH,updateTag,updateTime,guidString)
                                                    select FCUSTID,F_HS_DropShipRequestToken,FSTOCKID,FNUMBER,FNAME,FSPECIFICATION ,unitName,availableQOH,'original',getDate(),@guidString from tbl_store_availableQOH_ds where guidString=@guidString

                                                    select FCUSTID,F_HS_DropShipRequestToken,FSTOCKID,FNUMBER,FNAME,FSPECIFICATION ,unitName,availableQOH,guidString from tbl_store_availableQOH_ds where guidString=@guidString

                                                    delete from tbl_store_availableQOH_ds where guidString<>@guidString and updateTag='original'

                                                    drop table #jskc

                                    ", custNo);
                        }
                        return string.Format(@"/*dialect*/	declare @guidString nvarchar(50)
                                                    select @guidString=NewID()
                                                    select  e.FNAME productGroupName,k.Fnumber FSTOCKID,
                                                           b.FNUMBER,d.FNAME,d.FSPECIFICATION ,f.FNAME unitName	
	                                                       ,sum(a.FBASEQTY/h.fConvertnumerator) quantity,o.customerNumber as FCUSTID,o.F_HS_DropShipRequestToken
                                                    into #jskc	  
                                                    From  T_STK_INVENTORY a
                                                    inner join T_BD_MATERIAL b on a.FMATERIALID=b.FMATERIALID
                                                    inner join T_BD_MATERIAL_L d on b.FMATERIALID=d.FMATERIALID and d.FLOCALEID=2052
                                                    left join T_BD_MATERIALGROUP_L e on b.Fmaterialgroup=e.fid and e.FLOCALEID=2052
                                                    left join T_BD_unit_L f on a.FSTOCKUNITID=f.FUNITID and f.FLOCALEID=2052
                                                    left join T_BD_LOTMASTER g on a.fLot=g.flotID
                                                    left join T_BD_UNITCONVERTRATE h on a.FSTOCKUNITID=h.fcurrentUnitID and a.fbaseunitID=h.FdestUnitID
                                                    inner join T_BD_STOCK i on a.FSTOCKID=i.FSTOCKID
                                                    inner join T_BAS_ASSISTANTDATAENTRY_L j ON i.F_HS_DLC=j.FENTRYID and j.FLOCALEID=2052
                                                    inner join T_BAS_ASSISTANTDATAENTRY k ON j.FentryID=k.FentryID
													inner join (
													                select distinct h.FNUMBER customerNumber,h.F_HS_DropShipRequestToken,g.FNUMBER DLCNumber 
															        from HS_t_DropShipStockEntity a
		                                                            inner join T_BD_STOCK c on a.F_HS_DropShipStock=c.FSTOCKID
                                                                    inner join T_BAS_ASSISTANTDATAENTRY_L f ON c.F_HS_DLC=f.FENTRYID and f.FLOCALEID=2052
		                                                            inner join T_BAS_ASSISTANTDATAENTRY g ON f.FentryID=g.FentryID
															        inner join T_BD_CUSTOMER h on a.FCUSTID=h.FCUSTID
		                                                            Where h.FNUMBER = '{0}'
		                                                        ) o on k.fnumber=o.DLCNumber
                                                    where   a.FBASEQTY>0 and  a.FSTOCKSTATUSID=10000 and i.F_HS_TJ='1'
                                                    group by  e.FNAME,k.Fnumber,
                                                           b.FNUMBER,d.FNAME,d.FSPECIFICATION,f.FNAME,o.customerNumber,o.F_HS_DropShipRequestToken
                                                    order by e.FNAME desc

                                                    select a.FCUSTID,a.F_HS_DropShipRequestToken,a.FSTOCKID, a.FNUMBER, a.FNAME, a.FSPECIFICATION, a.unitName, (case when a.quantity - isnull(b.FQty, 0) < 0 then 0 else a.quantity - isnull(b.FQty, 0) end) availableQOH
                                                            into #tbl_store_availableQOH_ds
                                                    from #jskc a
                                                    left join
                                                            (SELECT d.FNUMBER,g.Fnumber fstockID, sum(c.FREMAINOUTQTY) FQty FROM T_SAL_ORDER a
                                                            INNER JOIN T_SAL_ORDERENTRY b ON a.FID = b.FID
                                                            inner join T_SAL_ORDERENTRY_R c on a.fid = c.fid and b.fentryID = c.fentryID
                                                            INNER JOIN T_BD_MATERIAL d on b.FMATERIALID = d.FMATERIALID
                                                            inner join T_BD_STOCK e on b.F_HS_STOCKID = e.FSTOCKID
                                                            inner join T_BAS_ASSISTANTDATAENTRY_L f ON e.F_HS_DLC = f.FENTRYID
                                                            inner join T_BAS_ASSISTANTDATAENTRY g ON f.FentryID = g.FentryID
                                                            Where a.FCLOSESTATUS <> 'B' AND a.FCANCELSTATUS <> 'B' AND b.FMRPCLOSESTATUS <> 'B' and a.FDOCUMENTSTATUS <> 'Z'
                                                            group by d.FNUMBER,g.Fnumber
		                                                    ) b on a.fnumber = b.fnumber and a.fstockID = b.fstockID


                                                            update tbl_store_availableQOH_ds
                                                            set availableQOH = b.availableQOH, updateTag = 'update', updateTime = getDate(), guidString = @guidString, unitName = b.unitName
                                                            from tbl_store_availableQOH_ds a
                                                            inner join #tbl_store_availableQOH_ds b on a.fnumber=b.fnumber and a.fstockID=b.fstockID
                                                            where a.availableQOH <> b.availableQOH

                                                            insert into tbl_store_availableQOH_ds(FCUSTID,F_HS_DropShipRequestToken,FSTOCKID, FNUMBER, FNAME, FSPECIFICATION, unitName, availableQOH, updateTag, updateTime, guidString)
                                                            select t.* ,'insert',getDate(),@guidString From #tbl_store_availableQOH_ds  t
                                                            where not exists(select * From tbl_store_availableQOH_ds where fnumber = t.fnumber and fstockID = t.fstockID)


                                                            select FCUSTID,F_HS_DropShipRequestToken,FSTOCKID, FNUMBER, FNAME, FSPECIFICATION, unitName, availableQOH from tbl_store_availableQOH_ds
                                                                where updateTag in ('update', 'insert')  and guidString = @guidString

                                                            insert into tbl_store_availableQOH_Log_ds(FCUSTID,F_HS_DropShipRequestToken,FSTOCKID, FNUMBER, FNAME, FSPECIFICATION, unitName, availableQOH, updateTag, updateTime, guidString)
                                                            select FCUSTID,F_HS_DropShipRequestToken,FSTOCKID, FNUMBER, FNAME, FSPECIFICATION, unitName, availableQOH, updateTag, updateTime, guidString from tbl_store_availableQOH_ds
                                                                   where updateTag in ('update', 'insert')  and guidString = @guidString

                                                            update tbl_store_availableQOH_ds set updateTag = null where updateTag in ('update', 'insert') and guidString = @guidString

                                                            drop table #jskc
                                                            drop table #tbl_store_availableQOH_ds", custNo);
                    case SynchroDataType.ImportLogis:
                        return string.Format(@"/*dialect*/ select  k.F_HS_DropShipRequestToken,h.FBILLNO as OrderBillNo ,a.FBILLNO as BillNo, i.F_HS_CARRYBILLNO as CarriageNO, i.F_HS_DeliDate as DeliDate
                                                            , q.FNUMBER as LogisticsChannel, i.F_HS_QueryUrl as QueryURL
                                                            from T_SAL_DELIVERYNOTICE a
                                                            inner join T_SAL_DELIVERYNOTICEENTRY b on a.fid = b.fid
                                                            inner join T_SAL_DELIVERYNOTICEENTRY_LK f on b.fentryid = f.fentryid

                                                            inner join T_SAL_OrderEntry g on f.fsbillID = g.fid and f.fsid = g.fentryID
                                                            inner join T_SAL_Order h on g.fid = h.fid

                                                            inner join T_SAL_ORDERENTRY_F o on o.FID = g.FID and g.FENTRYID = o.FENTRYID
                                                            inner join(select a.fid, max(F_HS_ShipMethods) F_HS_ShipMethods, max(F_HS_DeliDate) F_HS_DeliDate, max(F_HS_QueryUrl) F_HS_QueryUrl
                                                                                        , F_HS_CARRYBILLNO = STUFF((select ',' + t.F_HS_CARRYBILLNO

                                                                                                                from HS_T_LogisTrack t

                                                                                                                where len(F_HS_CARRYBILLNO) > 0 and t.fid = a.fid

                                                                                                                for xml path('')

                                                                                                                ) , 1 , 1 , '')  
		                                                                from HS_T_LogisTrack a

                                                                        where len(a.F_HS_CARRYBILLNO) > 0
																		and a.F_HS_YNDropShipWaybillSync = '0' 
																		and (a.F_HS_CARRYBILLNO <>'' or a.F_HS_CARRYBILLNO is not null)
																		and a.F_HS_IsLogistics = '0'
                                                                        group by a.fid
		                                                                ) i on a.FID = i.FID
                                                            inner join T_HS_ShipMethods j on i.F_HS_ShipMethods = j.FID
															inner join T_BAS_ASSISTANTDATAENTRY_L w ON j.F_HS_Channel = w.FENTRYID
                                                            inner join T_BAS_ASSISTANTDATAENTRY q ON q.FENTRYID = w.FENTRYID
                                                            inner join T_BD_CUSTOMER k on h.F_HS_B2CCUSTID = k.FCUSTID
															inner join HS_t_DropShipStockEntity r on r.FCUSTID = k.FCUSTID
                                                            inner join T_BD_MATERIAL m  on b.FMATERIALID = m.FMATERIALID
                                                            left join T_BAS_ASSISTANTDATAENTRY_L x ON m.F_HS_PRODUCTSTATUS = x.FENTRYID
                                                            left join T_BAS_ASSISTANTDATAENTRY z ON x.FentryID = z.FentryID
                                                            left join T_BD_STOCK n on b.FSHIPMENTSTOCKID = n.FSTOCKID and n.FSTOCKID = r.F_HS_DROPSHIPSTOCK
                                                            left join  T_BAS_BILLTYPE t on h.FBILLTypeID = t.FBILLTypeID
                                                            left join T_BAS_ASSISTANTDATAENTRY s ON h.F_HS_SaleOrderSource = s.FentryID
                                                            where s.fnumber = 'DropShippingOrder'
                                                            and h.FsaleOrgID = 100035 and h.FCANCELSTATUS <> 'B'
                                                            and t.FNUMBER = 'XSDD01_SYS'
                                                            and k.FNUMBER = '{0}'
													", custNo);
                    default:
                        return string.Empty;
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// 返回K3数据包
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="dataType"></param>
        /// <param name="numbers"></param>
        /// <returns></returns>
        public DynamicObjectCollection GetDynamicObjects(Context ctx, SynchroDataType dataType, string custNo, List<string> materialIds)
        {
            string sql = GetSQL(ctx, dataType, custNo, materialIds);
            return SQLUtils.GetObjects(ctx, sql);
        }

        /// <summary>
        /// 将JSON转换为销售订单对象集合
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="jArray"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private Dictionary<ResponseResult, AbsSynchroDataInfo> BuildSynchroData(Context ctx, JObject jObject, AbsSynchroDataInfo data)
        {
            ResponseResult result = null;
            K3SalOrderInfo order = null;
            List<K3SalOrderInfo> orders = null;

            K3SalOrderEntryInfo entry = null;
            K3SalOrderInfo info = (K3SalOrderInfo)data;

            if (info != null && jObject != null)
            {
                orders = new List<K3SalOrderInfo>();
                JArray jArray = JArray.Parse(JsonUtils.GetFieldValue(jObject, "SaleOrders"));

                if (jArray != null && jArray.Count > 0)
                {
                    foreach (var jObj in jArray)
                    {
                        if (jObj != null)
                        {
                            order = new K3SalOrderInfo();
                            order.FBillTypeId = "XSDD01_SYS";
                            order.FBillNo = info.F_HS_DropShipOrderPrefix + JsonUtils.GetFieldValue(jObj, "FBillNo");
                            order.SrcNo = order.FBillNo;
                            order.FDate = Convert.ToDateTime(JsonUtils.GetFieldValue(jObj, "FDate"));
                            order.FCreateDate = order.FDate;
                            order.F_HS_USADatetime = order.FDate;
                            order.FSaleOrgId = "100.01";

                            order.F_HS_SaleOrderSource = "DropShippingOrder";
                            order.FCustId = info.FCustId;
                            order.F_HS_B2CCustId = info.FCustId;
                            order.FCustLevel = info.FCustLevel;
                            order.FSettleCurrId = "USD";
                            order.FSalerId = string.IsNullOrWhiteSpace(info.FSalerId) ? "NA" : info.FSalerId;
                            order.FSaleDeptId = string.IsNullOrWhiteSpace(SQLUtils.GetDeptNo(ctx, order.FSalerId)) ? "BM000001" : SQLUtils.GetDeptNo(ctx, order.FSalerId);
                            order.F_HS_PaymentModeNew = "Deposit";
                            order.F_HS_RateToUSA = JennaCommonFile.GetRateToUSAFromTable(ctx, order.FSettleCurrId);
                            order.F_HS_DropShipDeliveryChannel = JsonUtils.GetFieldValue(jObj, "F_HS_DropShipDeliveryChannel");

                            order.F_HS_RecipientCountry = JsonUtils.GetFieldValue(jObj, "F_HS_RecipientCountry");
                            order.F_HS_DeliveryProvinces = JsonUtils.GetFieldValue(jObj, "F_HS_DeliveryProvinces");
                            order.F_HS_DeliveryCity = JsonUtils.GetFieldValue(jObj, "F_HS_DeliveryCity");
                            order.F_HS_DeliveryAddress = JsonUtils.GetFieldValue(jObj, "F_HS_DeliveryAddress");
                            order.F_HS_PostCode = JsonUtils.GetFieldValue(jObj, "F_HS_PostCode");
                            order.F_HS_DeliveryName = JsonUtils.GetFieldValue(jObj, "F_HS_DeliveryName");
                            order.F_HS_MobilePhone = JsonUtils.GetFieldValue(jObj, "F_HS_MobilePhone");
                            order.F_HS_PlatformCustomerID = JsonUtils.GetFieldValue(jObj, "F_HS_PlatformCustomerID");
                            order.F_HS_PlatformCustomerEmail = JsonUtils.GetFieldValue(jObj, "F_HS_PlatformCustomerEmail");
                            order.F_HS_YNFixedDropShipStock = info.F_HS_YNFixedDropShipStock;
                            order.F_HS_AmountDeclared = Convert.ToDecimal(JsonUtils.GetFieldValue(jObj, "F_HS_AmountDeclared"));
                            order.F_HS_FreightDeclared = Convert.ToDecimal(JsonUtils.GetFieldValue(jObj, "F_HS_FreightDeclared"));
                            order.F_HS_DeclaredCurrId = JsonUtils.GetFieldValue(jObj, "F_HS_DeclaredCurrId");
                            order.F_HS_Platform = SQLUtils.GetDropshipPlatform(ctx, order.FCustId);

                            JArray oEntries = JArray.Parse(JsonUtils.GetFieldValue(jObj, "OrderEntry").ToString());
                            order.OrderEntry = new List<K3SalOrderEntryInfo>();

                            List<string> materialIds = oEntries.Select(o => JsonUtils.GetFieldValue(o, "FMaterialId")).ToList();
                            List<K3SalOrderEntryInfo> queryEntries = GetK3Datas<List<K3SalOrderEntryInfo>>(ctx, SynchroDataType.DropShippingSalOrder, order.F_HS_B2CCustId, materialIds);

                            if (oEntries != null && oEntries.Count > 0)
                            {
                                foreach (var oEntry in oEntries)
                                {
                                    if (oEntry != null)
                                    {
                                        entry = new K3SalOrderEntryInfo();

                                        entry.FMaterialId = JsonUtils.GetFieldValue(oEntry, "FMaterialId");
                                        entry.FQTY = Convert.ToDecimal(JsonUtils.GetFieldValue(oEntry, "FQTY"));
                                        entry.FUnitId = SQLUtils.GetUnitNo(ctx, entry.FMaterialId);

                                        order.OrderEntry.Add(entry);
                                    }
                                }

                                if (queryEntries != null && queryEntries.Count > 0)
                                {
                                    if (order.OrderEntry != null && order.OrderEntry.Count > 0)
                                    {
                                        foreach (var item in order.OrderEntry)
                                        {
                                            var group = from o in queryEntries
                                                        where o.FMaterialId.Equals(item.FMaterialId) && o.FStockId.Equals(item.FStockId)
                                                        select o;
                                            if (group != null && group.Count() > 0)
                                            {
                                                item.FTAXPRICE = group.FirstOrDefault().FTAXPRICE;
                                                item.F_HS_FGroup = group.FirstOrDefault().F_HS_FGroup;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    result = new ResponseResult();
                                    result.Success = false;
                                    result.Message = string.Format("No corresponding sales price list was found for the customer's[{0}] sales order[{1}]", order.F_HS_B2CCustId, order.FBillNo);
                                    LogUtils.WriteSynchroLog(ctx, SynchroDataType.SaleOrder, result.Message);
                                }
                                order.orderFin = new K3SaleOrderFinance();
                                order.orderFin.FBillAllAmount = GetSalOrderAmount(order);
                                lastOrdAmo += order.orderFin.FBillAllAmount;
                              
                                order.F_HS_PaymentStatus = IsEnoughToPay(ctx,order) ? "3" : "2";

                                BuildOrderEntryPlans(ref order);

                                orders.Add(order);
                            }
                            else
                            {
                                result = new ResponseResult();
                                result.Success = false;
                                result.Message = string.Format("The sales order[{0}] details of the customer[{1}] cannot be empty", order.FBillNo, order.F_HS_B2CCustId);
                                LogUtils.WriteSynchroLog(ctx, SynchroDataType.SaleOrder, result.Message);
                            }
                        }
                    }
                }
            }
            
            return SplitStocks(ctx, orders);
        }

        private void BuildOrderEntryPlans(ref K3SalOrderInfo order)
        {
            if (order != null && order.OrderEntry != null && order.OrderEntry.Count > 0)
            {
                foreach (var oEntry in order.OrderEntry)
                {
                    K3SalOrderEntryPlan oPlan = new K3SalOrderEntryPlan();
                    oPlan.FPlanDeliveryDate = order.FDate;
                    oPlan.FPlanDate = order.FDate;
                    oPlan.FPlanQty = oEntry.FQTY;
                    oEntry.EntryPlans = new List<K3SalOrderEntryPlan>();
                    oEntry.EntryPlans.Add(oPlan);
                }
            }
        }

        /// <summary>
        /// 同步数据至K3
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="jArray"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private ResponseResult SynchroDataToK3(Context ctx, JObject jObject, AbsSynchroDataInfo data)
        {
            Dictionary<SynOperationType, IEnumerable<AbsSynchroDataInfo>> dict = null;
            Dictionary<ResponseResult, AbsSynchroDataInfo> splDict = BuildSynchroData(ctx, jObject, data);

            ResponseResult result = new ResponseResult();
            IEnumerable<AbsSynchroDataInfo> datas = MathSynchroData(splDict, ref result);

            if (datas != null && datas.Count() > 0)
            {
                dict = new Dictionary<SynOperationType, IEnumerable<AbsSynchroDataInfo>>();
                dict.Add(SynOperationType.SAVE, datas);

                HttpResponseResult k3Result = SynchroDataHelper.SynchroDataToK3(ctx, SynchroDataType.DropShippingSalOrder, true, null, dict);

                if (k3Result != null)
                {
                    result.Success = k3Result.Success;
                    result.SuccessNos = k3Result.SuccessEntityNos;
                    result.Message += k3Result.Message;
                }
            }

            return result;
        }

        /// <summary>
        /// 计算销售订单在K3Cloud的金额
        /// </summary>
        /// <param name="order"></param>
        /// <param name="hcAmount"></param>
        /// <returns></returns>
        private decimal GetSalOrderAmount(K3SalOrderInfo order)
        {
            decimal amount = 0;

            if (order != null)
            {
                if (order.OrderEntry != null && order.OrderEntry.Count > 0)
                {
                    foreach (var entry in order.OrderEntry)
                    {
                        if (entry != null)
                        {
                            decimal eachAmount = Math.Round(entry.FTAXPRICE * entry.FQTY * order.F_HS_RateToUSA, 2);

                            if (entry.FMaterialId.CompareTo("99.01") != 0)
                            {
                                decimal afterDisAmount = eachAmount - (Math.Round(eachAmount * Math.Round(entry.FDiscountRate / 100, 6), 2));
                                amount += afterDisAmount;
                            }
                            else
                            {
                                decimal afterDisAmount = eachAmount - (Math.Round(eachAmount * Math.Round(order.F_HS_WebFreightDiscountRate / 100, 6), 2));
                                amount += afterDisAmount;
                            }
                        }
                    }
                }
            }

            return amount;
        }

        /// <summary>
        /// 判断客户余额是否足以支付销售订单的金额
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        private bool IsEnoughToPay(Context ctx, K3SalOrderInfo order)
        {
            decimal surplus = LogHelper.GetCustBalance(ctx, order.FCustId, "100.01");
            decimal credit = LogHelper.GetCustCreditLine(ctx, order.FCustId, "F_HS_SurplusCreditUSD");
            decimal residue = (surplus + credit) - lastOrdAmo;

            if (order.orderFin.FBillAllAmount / order.F_HS_RateToUSA <= residue)
            {
                return true;
            }
            return false;
        }

        private bool IsFirstSynchro(Context ctx, string custNo)
        {
            if (!string.IsNullOrWhiteSpace(custNo))
            {
                string sql = string.Format(@"/*dialect*/ select * From  tbl_store_availableQOH_ds where FCUSTID = '{0}'", custNo);
                DynamicObjectCollection coll = SQLUtils.GetObjects(ctx, sql);

                if (coll == null || coll.Count == 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 库存校验
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="datas"></param>
        /// <returns></returns>
        private Dictionary<ResponseResult, AbsSynchroDataInfo> InventoryCheck(Context ctx, IEnumerable<AbsSynchroDataInfo> datas)
        {
            AvailInvDLCInfo info = null;
            List<AvailInvDLCInfo> infos = null;
            Dictionary<ResponseResult, AbsSynchroDataInfo> dict = null;
            K3SalOrderEntryInfo invCom = null;
            List<K3SalOrderEntryInfo> invComs = null;
            ResponseResult result = null;

            decimal avaQty = 0;

            if (datas != null && datas.Count() > 0)
            {
                List<K3SalOrderInfo> orders = datas.Select(o => (K3SalOrderInfo)o).ToList();

                if (orders != null && orders.Count > 0)
                {
                    dict = new Dictionary<ResponseResult, AbsSynchroDataInfo>();

                    foreach (var order in orders)
                    {
                        if (order != null && order.OrderEntry != null && order.OrderEntry.Count > 0)
                        {
                            infos = new List<AvailInvDLCInfo>();

                            foreach (var item in order.OrderEntry)
                            {
                                if (item != null && !item.FMaterialId.StartsWith("99.01"))
                                {
                                    info = new AvailInvDLCInfo();

                                    info.matNo = item.FMaterialId;
                                    info.saleQty = item.FQTY;
                                    info.dlcNo = item.F_HS_DLCID;

                                    infos.Add(info);
                                }
                            }
                        }
                    }

                    DateTime entCreTime = DateTime.Now;
                    AvailInventory.QueryAvailInvtory(ctx, ref infos);

                    if (infos != null && infos.Count > 0)
                    {
                        var groups = infos.Where(i => !string.IsNullOrWhiteSpace(i.matNo) && !string.IsNullOrWhiteSpace(i.dlcNo))
                                          .GroupBy(i => i.matNo)
                                          .SelectMany(i => i.GroupBy(e => e.dlcNo))
                                          .Select(i => new { i.FirstOrDefault().matNo, DLCID = i.Key, invQty = i.Sum(item => item.invQty), ocuQty = i.Sum(item => item.ocuQty), avaQty = i.Sum(item => item.invQty) - i.Sum(item => item.ocuQty) });

                        if (groups != null && groups.Count() > 0)
                        {
                            invComs = new List<K3SalOrderEntryInfo>();

                            for (int i = 0; i < orders.Count; i++)
                            {
                                if (orders[i] != null && orders[i].OrderEntry != null && orders[i].OrderEntry.Count > 0)
                                {
                                    result = new ResponseResult();
                                    result.Success = true;
                                    result.Message = string.Empty;

                                    var enGroup = orders[i].OrderEntry.Where(e => !string.IsNullOrWhiteSpace(e.FMaterialId) && !string.IsNullOrWhiteSpace(e.F_HS_DLCID))
                                                                  .GroupBy(e => e.FMaterialId)
                                                                  .SelectMany(e => e.GroupBy(d => d.F_HS_DLCID))
                                                                  .Select(e => new { e.FirstOrDefault().FMaterialId, DLCID = e.FirstOrDefault().F_HS_DLCID, FQTY = e.Sum(item => item.FQTY) });


                                    if (enGroup != null && enGroup.Count() > 0)
                                    {
                                        foreach (var en in enGroup)
                                        {
                                            if (en != null)
                                            {
                                                var gro = groups.Where(g => g.matNo.Equals(en.FMaterialId) && g.DLCID.Equals(en.DLCID));

                                                if (gro != null)
                                                {
                                                    if (i == 0)
                                                    {
                                                        invCom = new K3SalOrderEntryInfo();
                                                        invCom.FMaterialId = gro.FirstOrDefault().matNo;
                                                        invCom.FQTY = gro.FirstOrDefault().avaQty;
                                                        invCom.FQTY -= en.FQTY;
                                                        avaQty = invCom.FQTY;
                                                        invComs.Add(invCom);
                                                    }
                                                    else
                                                    {
                                                        if (invComs != null && invComs.Count > 0)
                                                        {
                                                            for (int j = 0; j < invComs.Count; j++)
                                                            {
                                                                if (en.FMaterialId.CompareTo(invComs[j].FMaterialId) == 0)
                                                                {
                                                                    invComs[j].FQTY -= en.FQTY;
                                                                    avaQty = invComs[j].FQTY;
                                                                }
                                                                else
                                                                {
                                                                    invCom = new K3SalOrderEntryInfo();
                                                                    invCom.FMaterialId = gro.FirstOrDefault().matNo;
                                                                    invCom.FQTY = gro.FirstOrDefault().avaQty;
                                                                    invCom.FQTY -= en.FQTY;
                                                                    avaQty = invCom.FQTY;
                                                                    invComs.Add(invCom);
                                                                }
                                                            }
                                                        }
                                                    }

                                                    if (avaQty >= 0)
                                                    {
                                                        result.Success = result.Success && true;
                                                    }
                                                    else
                                                    {
                                                        result.Success = result.Success && false;
                                                        result.Message += string.Format("销售订单[{0}]物料[{1}]+地理仓[{2}]缺少数量[{3}]！", orders[i].FBillNo, en.FMaterialId, en.DLCID, -avaQty);
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    // 5.23 销售订单.明细.明细创建日期：用可用库存校验那一刻的时间
                                    if (result.Success)
                                    {
                                        orders[i].OrderEntry.ForEach(en => en.F_HS_CreateDateEntry = entCreTime);
                                    }

                                    dict.Add(result, orders[i]);
                                }
                            }
                        }

                    }
                }

            }

            return dict;
        }

        /// <summary>
        /// 销售订单明细分仓信息
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="datas"></param>
        /// <returns></returns>
        private Dictionary<ResponseResult, AbsSynchroDataInfo> SplitStocks(Context ctx, List<K3SalOrderInfo> orders)
        {
            SplitStock_MatInfo info = null;
            List<SplitStock_MatInfo> infos = null;

            if (orders != null && orders.Count > 0)
            {
                var result = orders.SelectMany(o => o.OrderEntry.Select(e => new { e.FMaterialId, e.FStockId, e.FQTY, OrederId = o.FBillNo })).ToList();

                if (result != null && result.Count() > 0)
                {
                    infos = new List<SplitStock_MatInfo>();

                    foreach (var item in result)
                    {
                        if (item != null)
                        {
                            info = new SplitStock_MatInfo();
                            info.orderNo = item.OrederId;
                            info.matNo = item.FMaterialId;
                            info.saleQty = item.FQTY;

                            infos.Add(info);
                        }
                    }
                }

                DateTime entCreTime = DateTime.Now;
                List<string> failMsgs = new List<string>();
                CommonMethod.SplitStock(ctx, ref infos, orders.FirstOrDefault().FCustId, out failMsgs);

                return RecombineOrderEntry(ctx, infos, orders, failMsgs);
            }

            return null;
        }

        /// <summary>
        /// 销售订单分仓后的明细信息
        /// </summary>
        /// <param name="infos"></param>
        /// <returns></returns>
        private Dictionary<ResponseResult, AbsSynchroDataInfo> RecombineOrderEntry(Context ctx, List<SplitStock_MatInfo> infos, List<K3SalOrderInfo> orders, List<string> messages)
        {
            Dictionary<ResponseResult, AbsSynchroDataInfo> dict = new Dictionary<ResponseResult, AbsSynchroDataInfo>();
            List<K3SalOrderInfo> failOrders = null;
            List<K3SalOrderEntryInfo> entries = null;
            K3SalOrderEntryInfo oEntry = null;
            List<string> orderNos = null;
            ResponseResult response = null;

            if (infos != null && infos.Count > 0)
            {
                orderNos = infos.Select(i => i.orderNo).Distinct().ToList();
                failOrders = orders.Where(o => !orderNos.Contains(o.FBillNo)).ToList();

                var group = from e in infos
                            where !string.IsNullOrWhiteSpace(e.orderNo)
                            group e by e.orderNo;

                if (group != null && group.Count() > 0)
                {
                    foreach (var items in group)
                    {
                        var order = orders.Where(o => !string.IsNullOrWhiteSpace(o.FBillNo) && o.FBillNo.Equals(items.FirstOrDefault().orderNo)).FirstOrDefault();
                        List<K3SalOrderEntryInfo> queryEntries = GetK3Datas<List<K3SalOrderEntryInfo>>(ctx, SynchroDataType.DropShippingSalOrder, order.F_HS_B2CCustId, items.Select(i => i.matNo).ToList());

                        if (items != null && items.Count() > 0)
                        {
                            entries = new List<K3SalOrderEntryInfo>();

                            foreach (var item in items)
                            {
                                if (item != null)
                                {
                                    oEntry = new K3SalOrderEntryInfo();

                                    oEntry.FMaterialId = item.matNo;
                                    oEntry.FStockId = item.stockNo;
                                    oEntry.FQTY = item.saleQty;

                                    var result = order.OrderEntry.Where(i => !string.IsNullOrWhiteSpace(i.FMaterialId) && i.FMaterialId.Equals(oEntry.FMaterialId));
                                    var result_ = from o in queryEntries
                                                  where o.FMaterialId.Equals(oEntry.FMaterialId) && o.FStockId.Equals(oEntry.FStockId)
                                                  select o;

                                    oEntry.FTAXPRICE = result_.FirstOrDefault().FTAXPRICE;
                                    oEntry.F_HS_FGroup = result_.FirstOrDefault().F_HS_FGroup;
                                    oEntry.EntryPlans = result.FirstOrDefault().EntryPlans;
                                    oEntry.FUnitId = result.FirstOrDefault().FUnitId;
                                    oEntry.F_HS_CreateDateEntry = DateTime.Now;

                                    entries.Add(oEntry);
                                }
                            }
                        }

                        response = new ResponseResult();
                        response.Success = true;

                        order.OrderEntry = entries;
                        SetOrderFreightInfo(ctx, ref order);
                        dict.Add(response, order);
                    }
                }
            }
            else
            {
                failOrders = orders;
            }

            if (messages != null && messages.Count > 0)
            {
                if (failOrders != null && failOrders.Count() > 0)
                {
                    foreach (var failOrder in failOrders)
                    {
                        var msgs = messages.Where(m => m.Contains(failOrder.FBillNo));

                        if (msgs != null && msgs.Count() > 0)
                        {
                            response = new ResponseResult();

                            foreach (var msg in msgs)
                            {
                                if (!string.IsNullOrEmpty(msg))
                                {
                                    response.Success = false;
                                    response.Message += msg;
                                }
                            }

                            dict.Add(response, failOrder);
                        }
                    }
                }
            }

            return dict;
        }

        /// <summary>
        ///设置销售订单运费信息
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="order"></param>
        private void SetOrderFreightInfo(Context ctx, ref K3SalOrderInfo order)
        {
            Dictionary<string, string> dict = FreightUtils.GetFreight(ctx, order);
            order.F_HS_ShippingMethod = dict != null ? dict["F_HS_FreTiltle"].ToString() : "";
            order.F_HS_EachShipmentFreight = dict != null ? dict["F_HS_AmountNoteUSD"].ToString() : "";

            K3SalOrderEntryInfo entry = new K3SalOrderEntryInfo();
            entry.FMaterialId = "99.01";
            entry.FTAXPRICE = dict == null ? 0 : Convert.ToDecimal(dict["F_HS_FreAmount"]);
            entry.FQTY = 1;
            entry.FUnitId = SQLUtils.GetUnitNo(ctx, entry.FMaterialId);
            entry.FStockId = "501";
            entry.F_HS_FGroup = order.OrderEntry.FirstOrDefault().F_HS_FGroup;
            entry.F_HS_CreateDateEntry = DateTime.Now;

            order.OrderEntry.Add(entry);
            BuildOrderEntryPlans(ref order);
            order.F_HS_Shipping = order.OrderEntry.Where(e => e.FMaterialId.Equals("99.01")).ToList().FirstOrDefault().FTAXPRICE;
        }
        /// <summary>
        /// 库存校验后筛选符合同步条件的销售订单
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private IEnumerable<AbsSynchroDataInfo> MathSynchroData(Dictionary<ResponseResult, AbsSynchroDataInfo> dict, ref ResponseResult result)
        {
            List<AbsSynchroDataInfo> infos = null;

            if (dict != null && dict.Count > 0)
            {
                var math = from o in dict
                           where o.Key != null && o.Key.Success == true
                           select o;

                if (math != null && math.Count() > 0)
                {
                    infos = new List<AbsSynchroDataInfo>();

                    foreach (var item in math)
                    {
                        if (item.Value != null)
                        {
                            infos.Add(item.Value);
                        }
                    }
                }

                var nonMath = from o in dict
                              where o.Key != null && o.Key.Success == false
                              select o;

                if (nonMath != null && nonMath.Count() > 0)
                {
                    result = new ResponseResult();

                    foreach (var item in nonMath)
                    {
                        result.Success = item.Key.Success;
                        result.Message += item.Key.Message + Environment.NewLine;
                    }
                }
            }

            return infos;
        }

        /// <summary>
        /// 9.3 返回成功后反写发货通知单.物流跟踪.dropshipping运单同步为true
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="datas"></param>
        /// <returns></returns>
        private int UpdateAfterSychro(Context ctx, IEnumerable<AbsSynchroDataInfo> datas)
        {
            if (datas != null && datas.Count() > 0)
            {
                List<DeliveryNotice> deli = datas.Select(d => (DeliveryNotice)d).ToList();

                if (deli != null && deli.Count > 0)
                {
                    string sql = string.Format(@"/*dialect*/update a set  a.F_HS_YNDropShipWaybillSync = 1
                                                    from HS_T_LogisTrack a
                                                    inner join T_SAL_DELIVERYNOTICE b on b.FID = a.FID
			                                        where a.F_HS_YNDropShipWaybillSync = 0
													and b.FBILLNO in ('{0}') ", string.Join("','", deli.Select(d => d.SrcNo)));

                    return SQLUtils.Execute(ctx, sql);
                }
            }
            return 0;
        }
    }
}