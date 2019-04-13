using System;
using System.Collections.Generic;
using System.Linq;
using Hands.K3.SCM.APP.Entity.CommonObject;

using Hands.K3.SCM.APP.Entity.SynDataObject;
using Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject;
using HS.K3.Common.Abbott;
using Kingdee.BOS;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using ServiceStack.Redis;

namespace Hands.K3.SCM.APP.Utils.Utils
{
    public class LogHelper
    {
        static object objLock = new object();

        public static string GetDataSourceTypeDesc(SynchroDataType dataType)
        {
            switch (dataType)
            {
                case SynchroDataType.Inventroy:
                    return "库存";
                case SynchroDataType.Customer:
                    return "客户";
                case SynchroDataType.CustomerByExcel:
                    return "客户（EXCEL）";
                case SynchroDataType.CustomerOrderQty:
                    return "客户下单次数";
                case SynchroDataType.CustomerAddress:
                    return "客户地址";
                case SynchroDataType.CustomerAddressByExcel:
                    return "客户地址（EXCEL）";
                case SynchroDataType.SaleOrder:
                    return "销售订单";
                case SynchroDataType.SaleOrderOffline:
                    return "线下销售订单";
                case SynchroDataType.SaleOrderStatus:
                    return "销售订单状态";
                case SynchroDataType.SalesOrderPayStatus:
                    return "销售订单支付状态";
                case SynchroDataType.SaleOutStockBill:
                    return "销售出库单";
                case SynchroDataType.UserLoin:
                    return "用户登录";
                case SynchroDataType.DelCustomerAddress:
                    return "删除客户地址";
                case SynchroDataType.Material:
                    return "物料";
                case SynchroDataType.DownLoadListInfo:
                    return "物料List信息";
                case SynchroDataType.DeliveryNoticeBill:
                    return "发货通知单";
                case SynchroDataType.K3ToKuaiDi100:
                    return "订阅快递100";
                case SynchroDataType.ImportLogis:
                    return "上传运单号";
                case SynchroDataType.DeductIntegral:
                    return "扣减积分";
                case SynchroDataType.UnDeductIntegral:
                    return "反扣减积分";
                case SynchroDataType.ReceiveBill:
                    return "收款单";
                case SynchroDataType.BatchAdjust:
                    return "批量调价单";
                case SynchroDataType.OnTheWay:
                    return "在途明细";
                default:
                    return dataType.ToString();
            }

        }

        #region WriteSynchroLog


        /// <summary>
        /// 记录同步日志
        /// </summary>
        /// <param name="ctx">k3cloud登录上下文</param>
        /// <param name="result">同步结果</param>
        public static void WriteSynchroLog(Context ctx, SynchroLog log, bool notMerge = false)
        {
            lock (objLock)
            {
                string insertSql = "";
                if (notMerge)
                {
                    insertSql = @"
                                 Insert Into HS_T_SynchroLog(FDataSourceType,FDataSourceId,FBILLNO,
                                        FSynchroTime,FIsSuccess,FErrInfor,FDataSourceTypeDesc,FHSOperateId)
                                 Select @FDataSourceType as FDataSourceType,@FDataSourceId as FDataSourceId,
                                        @FBILLNO as FBILLNO,@FSynchroTime as FSynchroTime,
                                        @FIsSuccess as FIsSuccess,@FErrInfor as FErrInfor,
                                        @FDataSourceTypeDesc as FDataSourceTypeDesc,
                                        @FHSOperateId as FHSOperateId ";
                }
                else
                {
                    insertSql = @"if not exists(select 1 from HS_T_SynchroLog where FDataSourceType =@FDataSourceType and FDataSourceId=@FDataSourceId and FBILLNO=@FBILLNO)
                                 Insert Into HS_T_SynchroLog(FDataSourceType,FDataSourceId,FBILLNO,
                                        FSynchroTime,FIsSuccess,FErrInfor,FDataSourceTypeDesc,FHSOperateId)
                                 Select @FDataSourceType as FDataSourceType,@FDataSourceId as FDataSourceId,
                                        @FBILLNO as FBILLNO,@FSynchroTime as FSynchroTime,
                                        @FIsSuccess as FIsSuccess,@FErrInfor as FErrInfor,
                                        @FDataSourceTypeDesc as FDataSourceTypeDesc,
                                        @FHSOperateId as FHSOperateId 
                                Else
                                    Update HS_T_SynchroLog Set FSynchroTime=@FSynchroTime,
                                            FIsSuccess=@FIsSuccess,
                                            FErrInfor=@FErrInfor,
                                            FDataSourceTypeDesc=@FDataSourceTypeDesc,
                                            FHSOperateId=@FHSOperateId
                                    Where FDataSourceType =@FDataSourceType and FDataSourceId=@FDataSourceId and FBILLNO=@FBILLNO
                                ";
                }
                List<SqlParam> paramList = new List<SqlParam>();
                paramList.Add(new SqlParam("@FDataSourceType", KDDbType.String, log.FDataSourceType.ToString()));
                paramList.Add(new SqlParam("@FDataSourceId", KDDbType.String, string.IsNullOrWhiteSpace(log.sourceId) ? string.IsNullOrWhiteSpace(log.sourceNo) ? "0" : log.sourceNo : log.sourceId));
                paramList.Add(new SqlParam("@FBILLNO", KDDbType.String, string.IsNullOrWhiteSpace(log.sourceNo) ? "" : log.sourceNo));
                paramList.Add(new SqlParam("@FSynchroTime", KDDbType.DateTime, DateTime.Now));
                paramList.Add(new SqlParam("@FIsSuccess", KDDbType.String, log.IsSuccess));
                paramList.Add(new SqlParam("@FErrInfor", KDDbType.String, log.ErrInfor));
                paramList.Add(new SqlParam("@FDataSourceTypeDesc", KDDbType.String, GetDataSourceTypeDesc(log.FDataSourceType)));
                paramList.Add(new SqlParam("@FHSOperateId", KDDbType.Int64, ctx.UserId));

                Kingdee.BOS.App.ServiceHelper.GetService<IDBService>().Execute(ctx, insertSql, paramList);
            }
        }

        /// <summary>
        /// 订单同步失败记录
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="orders"></param>
        public static int WriteSynFailOrdersInfo(Context ctx, List<K3SalOrderInfo> orders)
        {
            int count = 0;

            lock (objLock)
            {
                if (orders != null && orders.Count > 0)
                {
                    foreach (var order in orders)
                    {
                        if (order != null)
                        {
                            if (order.OrderEntry != null && order.OrderEntry.Count > 0)
                            {
                                foreach (var entry in order.OrderEntry)
                                {
                                    if (entry.FMaterialId.CompareTo("99.01") != 0)
                                    {
                                        string exist = string.Format(@"/*dialect*/ select FBillNo,FDate,FNumber,FStockID from HS_T_SynchroFailOrder where FBillNo = '{0}'
                                                                                   and FDate = '{1}' and FNumber = '{2}' and FStockID = '{3}'"
                                                                    , order.FBillNo, order.F_HS_USADatetime, entry.FMaterialId, entry.FStockId);

                                        DynamicObjectCollection coll = SQLUtils.GetObjects(ctx, exist);

                                        if (coll == null || coll.Count <= 0)
                                        {
                                            string iSql = string.Format(@"
                                                   insert into HS_T_SynchroFailOrder (FBillNo,FDate,FNumber,FStockID,quantity)
                                                   select @FBillNo as FBillNo,@FDate as FDate, @FNumber as FNumber,@FStockID as FStockID,@quantity as quantity
        
                                               ");

                                            var para = new List<SqlParam>();
                                            para.Add(new SqlParam("@FBillNo", KDDbType.String, order.FBillNo));
                                            para.Add(new SqlParam("@FDate", KDDbType.String, order.F_HS_USADatetime));
                                            para.Add(new SqlParam("@FNumber", KDDbType.String, entry.FMaterialId));
                                            para.Add(new SqlParam("@FStockID", KDDbType.String, entry.FStockId));
                                            para.Add(new SqlParam("@quantity", KDDbType.Int32, entry.FQTY));

                                            count += Kingdee.BOS.App.ServiceHelper.GetService<IDBService>().Execute(ctx, iSql, para);
                                        }

                                    }
                                }
                            }

                        }

                    }

                }
            }
            return count;
        }

        /// <summary>
        /// 销售订单状态同步记录
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="oStatus"></param>
        /// <returns></returns>
        public static int WriteSynSalOrderStatus(Context ctx, List<K3SalOrderStatusInfo> oStatus)
        {
            int count = 0;

            lock (objLock)
            {
                if (oStatus != null && oStatus.Count > 0)
                {
                    string iSql = string.Format(@"/*dialect*/ 
                                                   insert into HS_T_SynchroOrderStatusLog(FBillNo,FCloseStatus,FCancelStatus,shipStatus,paymentStatus,F_HS_PaymentMode,updateTime,updateUser)
                                                   select @FBillNo as FBillNo,@FCloseStatus as FCloseStatus, @FCancelStatus as FCancelStatus,@shipStatus as shipStatus,@paymentStatus as paymentStatus
                                                          ,@F_HS_PaymentMode as F_HS_PaymentMode,@updateTime as updateTime,@updateUser as updateUser
                                                   ");

                    foreach (var os in oStatus)
                    {
                        if (os != null)
                        {
                            var para = new List<SqlParam>();
                            para.Add(new SqlParam("@FBillNo", KDDbType.String, os.BillNo));
                            para.Add(new SqlParam("@FCloseStatus", KDDbType.String, os.CloseStatus));
                            para.Add(new SqlParam("@FCancelStatus", KDDbType.String, os.CancelStatus));
                            para.Add(new SqlParam("@shipStatus", KDDbType.String, os.ShipStatus));
                            para.Add(new SqlParam("@paymentStatus", KDDbType.String, os.PaymentStatus));
                            para.Add(new SqlParam("@F_HS_PaymentMode", KDDbType.String, os.F_HS_PaymentMode));
                            para.Add(new SqlParam("@updateTime", KDDbType.DateTime, DateTime.Now));
                            para.Add(new SqlParam("@updateUser", KDDbType.String, ctx.UserName));

                            count += Kingdee.BOS.App.ServiceHelper.GetService<IDBService>().Execute(ctx, iSql, para);
                        }
                    }
                }
            }
            return count;
        }
        /// <summary>
        /// 同步失败的订单在同步成功后将同步失败记录删除
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="orders"></param>
        public static void DelSynSucessOrderInfo(Context ctx, List<string> successNos)
        {
            string[] numbers = null;
            string condition = "";
            lock (objLock)
            {
                if (successNos != null && successNos.Count > 0)
                {
                    numbers = new string[successNos.Count];

                    string dSql = string.Format(@"delete from HS_T_SynchroFailOrder where FBillNo in(");
                    for (int i = 0; i < successNos.Count; i++)
                    {
                        if (!string.IsNullOrWhiteSpace(successNos[i]))
                        {
                            if (i < successNos.Count - 1)
                            {
                                numbers[i] = "'" + successNos[i] + "'" + ",";
                            }
                            else if (i == successNos.Count - 1)
                            {
                                numbers[i] = "'" + successNos[i] + "'" + ")";
                            }
                            condition += numbers[i];
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(condition))
                    {
                        Kingdee.BOS.App.ServiceHelper.GetService<IDBService>().Execute(ctx, dSql + condition);
                    }

                }
            }
        }
        /// <summary>
        /// 同步成功日志
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="dataType"></param>
        /// <param name="errMessage"></param>
        /// <param name="isOk"></param>
        /// <param name="srcPKId"></param>
        /// <param name="billNo"></param>
        public static void WriteSynchroLog_Succ(Context ctx, SynchroDataType dataType, string errMessage, bool isOk = true, string srcPKId = "", string billNo = "")
        {
            lock (objLock)
            {
                if (errMessage.Length > 4000)
                {
                    errMessage = errMessage.Substring(0, 3999);
                }

                string insertSql = @" Insert Into HS_T_SynchroLog_Succ(FDataSourceType,FDataSourceId,FBILLNO,
                                        FSynchroTime,FIsSuccess,FErrInfor,FDataSourceTypeDesc,FHSOperateId) 
                                  Select @FDataSourceType as FDataSourceType,@FDataSourceId as FDataSourceId,@FBILLNO as FBILLNO,
                                        @FSynchroTime as FSynchroTime,@FIsSuccess as FIsSuccess,@FErrInfor as FErrInfor,
                                        @FDataSourceTypeDesc as FDataSourceTypeDesc,@FHSOperateId as FHSOperateId ";

                var para = new List<SqlParam>();
                para.Add(new SqlParam("@FDataSourceType", KDDbType.String, dataType.ToString()));
                para.Add(new SqlParam("@FDataSourceId", KDDbType.String, srcPKId));
                para.Add(new SqlParam("@FBILLNO", KDDbType.String, billNo));
                para.Add(new SqlParam("@FSynchroTime", KDDbType.DateTime, DateTime.Now));
                para.Add(new SqlParam("@FIsSuccess", KDDbType.String, isOk == true ? 1 : 0));
                para.Add(new SqlParam("@FErrInfor", KDDbType.String, errMessage));
                para.Add(new SqlParam("@FDataSourceTypeDesc", KDDbType.String, GetDataSourceTypeDesc(dataType)));
                para.Add(new SqlParam("@FHSOperateId", KDDbType.Int64, ctx.UserId));

                Kingdee.BOS.App.ServiceHelper.GetService<IDBService>().Execute(ctx, insertSql, para);
            }
        }

        /// <summary>
        /// 写客户余额支付日志
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="dataType"></param>
        /// <param name="adatas"></param>
        public static void WriteCustBalanceLog(Context ctx, SynchroDataType dataType, IEnumerable<AbsSynchroDataInfo> adatas)
        {
            lock (objLock)
            {
                List<SqlParam> para = null;

                string insertSql = @"Insert Into HS_T_customerBalance(F_HS_B2CCUSTID,changedAmount,changedType,changedCause,balanceAmount,F_HS_RateToUSA
                                        ,FSETTLECURRID,changedAmountUSA,balanceAmountUSA,updateTime,updateUser,FBillNo,fentryID,needfreezed,remark) 
                                    Select  @F_HS_B2CCUSTID as F_HS_B2CCUSTID,@changedAmount as changedAmount,@changedType as changedType,@changedCause as changedCause,
                                            @balanceAmount as balanceAmount, @F_HS_RateToUSA as F_HS_RateToUSA, @FSETTLECURRID as FSETTLECURRID,@changedAmountUSA as changedAmountUSA,
                                            @balanceAmountUSA as balanceAmountUSA,@updateTime as updateTime,@updateUser as updateUser,@FBillNo as FBillNo,@fentryID as fentryID
                                           ,@needfreezed as needfreezed,@remark as remark
                                    ";


                if (adatas != null && adatas.Count() > 0)
                {
                    List<AbsDataInfo> datas = adatas.Select(d => (AbsDataInfo)d).ToList();

                    foreach (var data in datas)
                    {
                        if (data != null)
                        {

                            para = new List<SqlParam>();

                            para.Add(new SqlParam("@F_HS_B2CCUSTID", KDDbType.Int32, SQLUtils.GetCustomerId(ctx, data.F_HS_B2CCustId, 1)));
                            para.Add(new SqlParam("@changedAmount", KDDbType.Decimal, data.FRealAmountFor));
                            para.Add(new SqlParam("@changedType", KDDbType.String, data.ChangedType));
                            para.Add(new SqlParam("@changedCause", KDDbType.String, data.ChangedCause));
                            para.Add(new SqlParam("@balanceAmount", KDDbType.Decimal, GetCustBalance(ctx, data.F_HS_B2CCustId, data.FSaleOrgId) * data.F_HS_RateToUSA));

                            para.Add(new SqlParam("@F_HS_RateToUSA", KDDbType.Decimal, data.F_HS_RateToUSA));
                            para.Add(new SqlParam("@FSETTLECURRID", KDDbType.Int32, SQLUtils.GetSettleCurrId(ctx, data.FSettleCurrId)));


                            if (data.F_HS_RateToUSA > 0)
                            {
                                para.Add(new SqlParam("@changedAmountUSA", KDDbType.Decimal, data.FRealAmountFor / data.F_HS_RateToUSA));
                                para.Add(new SqlParam("@balanceAmountUSA", KDDbType.Decimal, GetCustBalance(ctx, data.F_HS_B2CCustId, data.FSaleOrgId)));
                            }

                            para.Add(new SqlParam("@updateTime", KDDbType.DateTime, DateTime.Now));
                            para.Add(new SqlParam("@updateUser", KDDbType.Int64, ctx.UserId));
                            para.Add(new SqlParam("@FBillNo", KDDbType.String, data.FBillNo.Contains("_") ? data.FBillNo.Split('_')[0] : data.FBillNo));
                            para.Add(new SqlParam("@fentryID", KDDbType.Int64, data.FEntryId));
                            para.Add(new SqlParam("@needfreezed", KDDbType.Int16, data.NeedFreezed ? 1 : 0));
                            para.Add(new SqlParam("@remark", KDDbType.String, data.Remark));

                            Kingdee.BOS.App.ServiceHelper.GetService<IDBService>().Execute(ctx, insertSql, para);
                        }
                    }

                }
            }
        }

        /// <summary>
        /// 获取客户结余
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="custNo"></param>
        /// <param name="orgNo"></param>
        /// <returns></returns>
        public static decimal GetCustBalance(Context ctx, string custNo, string orgNo)
        {
            if (!string.IsNullOrWhiteSpace(custNo) && !string.IsNullOrWhiteSpace(orgNo))
            {
                string sql = string.Format(@"/*dialect*/ select {0} from T_BD_CUSTOMER a 
                                                        inner join T_ORG_ORGANIZATIONS b on a.FUSEORGID = b.FORGID
                                                        where a.FNUMBER = '{1}' and b.FNUMBER = '{2}'", orgNo.CompareTo("100.03") == 0 ? "F_HS_CNYBalance" : "F_HS_USDBalance", custNo, orgNo);
                decimal balance = Convert.ToDecimal(JsonUtils.ConvertObjectToString(SQLUtils.GetObject(ctx, sql, string.Format("{0}", orgNo.CompareTo("100.03") == 0 ? "F_HS_CNYBalance" : "F_HS_USDBalance"))));
                return Math.Round(balance, 2);
            }

            return 0;
        }

        /// <summary>
        /// 获取信用余额
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="custNo"></param>
        /// <returns></returns>
        public static decimal GetCustCreditLine(Context ctx, string custNo, string fieldName)
        {
            if (!string.IsNullOrWhiteSpace(custNo) && !string.IsNullOrWhiteSpace(fieldName))
            {
                string sql = string.Format(@"/*dialect*/ select {0} from T_BD_CUSTOMER a 
                                                        inner join T_ORG_ORGANIZATIONS b on a.FUSEORGID = b.FORGID
                                                        where a.FNUMBER = '{1}' and b.FNUMBER = '100.01'", fieldName, custNo);
                decimal credit = Convert.ToDecimal(JsonUtils.ConvertObjectToString(SQLUtils.GetObject(ctx, sql, fieldName)));
                return Math.Round(credit, 2);
            }

            return 0;
        }

        /// <summary>
        /// 记录同步至HC网站redis数据
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="dataType"></param>
        /// <param name="redis"></param>
        /// <param name="dict"></param>
        /// <param name="isSuccess"></param>
        public static void WriteSynchroDataLog(Context ctx, SynchroDataType dataType, IRedisClient redis, Dictionary<string, string> dict, bool isSuccess)
        {
            string sql = string.Empty;

            lock (objLock)
            {
                string insertSql = @"/*dialect*/insert into HS_T_synchroDataLog(SynchroDataType,redisKey,json,redisServerIP,redisDBID,K3CloudDbId
                                    ,createTime,success) values
                        
                                    ";

                if (dict != null && dict.Count > 0)
                {
                    for (int i = 0; i < dict.Count; i++)
                    {
                        if (i < dict.Count - 1)
                        {
                            sql += insertSql + string.Format(@"('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}')", dataType, dict.ElementAt(i).Key, SQLUtils.DealQuotes(dict.ElementAt(i).Value), redis.Host, redis.Db, ctx.DBId, DateTime.Now, isSuccess ? 1 : 0) + Environment.NewLine;
                        }
                        else
                        {
                            sql += insertSql + string.Format(@"('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}')", dataType, dict.ElementAt(i).Key, SQLUtils.DealQuotes(dict.ElementAt(i).Value), redis.Host, redis.Db, ctx.DBId, DateTime.Now, isSuccess ? 1 : 0);
                        }
                    }

                    Kingdee.BOS.App.ServiceHelper.GetService<IDBService>().Execute(ctx, sql);
                }
            }
        }

        #endregion 
    }
}
