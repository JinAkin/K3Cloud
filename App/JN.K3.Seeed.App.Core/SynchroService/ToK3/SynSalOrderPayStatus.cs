using System;
using System.Collections.Generic;
using System.Linq;
using Hands.K3.SCM.App.Synchro.Utils.SynchroService;
using Hands.K3.SCM.APP.Entity.CommonObject;
using Hands.K3.SCM.APP.Entity.EnumType;
using Hands.K3.SCM.APP.Entity.K3WebApi;
using Hands.K3.SCM.APP.Entity.SynDataObject;
using Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject;
using Hands.K3.SCM.APP.Utils.Utils;
using HS.K3.Common.Abbott;
using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Orm.DataEntity;
using Newtonsoft.Json.Linq;

namespace Hands.K3.SCM.App.Core.SynchroService.ToK3
{
    public class SynSalOrderPayStatus : SynSalOrderToK3
    {
        override public SynchroDataType DataType
        {
            get
            {
                return SynchroDataType.SalesOrderPayStatus;
            }
        }

        public override IEnumerable<AbsSynchroDataInfo> GetSynchroData(IEnumerable<string> numbers = null)
        {
            var datas = ServiceHelper.GetSynchroDatas(this.K3CloudContext, this.DataType, this.RedisDbId, numbers,this.Direction);
            return datas;
        }
        public override AbsSynchroDataInfo BuildSynchroData(Context ctx, string json, AbsSynchroDataInfo data = null)
        {
            K3SalOrderInfo so = null;
            JObject jobj = JsonUtils.ParseJson2JObj(ctx, this.DataType, json);

            if (jobj != null)
            {
                so = new K3SalOrderInfo();
                string payStatus = JsonUtils.GetFieldValue(jobj, "order_status");

                if ("1".Equals(payStatus))
                {
                    so.F_HS_SaleOrderSource = "HCWebPendingOder";//订单来源
                    so.F_HS_PaymentStatus = "2";//未付款
                }
                else if ("2".Equals(payStatus))
                {
                    so.F_HS_SaleOrderSource = "HCWebProcessingOder";//订单来源
                    so.F_HS_PaymentStatus = "3";//已到款
                }
                so.FBillNo = JsonUtils.GetFieldValue(jobj, "orders_id");
                so.F_HS_PaymentModeNew = JsonUtils.GetFieldValue(jobj, "payment");

                so.F_HS_IsSameAdress = IsSameAddress(ctx, so, jobj);
            }

            return so;
        }

        public override Dictionary<SynOperationType, IEnumerable<AbsSynchroDataInfo>> EntityDataSource(Context ctx, IEnumerable<AbsSynchroDataInfo> srcDatas)
        {
            Dictionary<SynOperationType, IEnumerable<AbsSynchroDataInfo>> dict = new Dictionary<SynOperationType, IEnumerable<AbsSynchroDataInfo>>();
            dict.Add(SynOperationType.UPDATE, srcDatas);

            return dict;
        }
        public override HttpResponseResult ExecuteSynchro(IEnumerable<AbsSynchroDataInfo> sourceDatas, List<SynchroLog> logs, SynOperationType operationType)
        {
            HttpResponseResult result = new HttpResponseResult();
            List<K3SalOrderInfo> second = null;
            List<string> numbers = null;
            List<string> auditSuccNos = null;

            if (sourceDatas == null || sourceDatas.Count() == 0)
            {
                result = new HttpResponseResult();
                result.Success = false;
                result.Message = "没有需要同步的数据！";
            }

            if (operationType == SynOperationType.UPDATE)
            {
                try
                {
                    second = sourceDatas.Select(o => (K3SalOrderInfo)o).ToList();

                    if (second != default(List<K3SalOrderInfo>))
                    {
                        if (second.Count > 0)
                        {
                            numbers = new List<string>();
                            auditSuccNos = new List<string>();

                            foreach (var item in second)
                            {
                                if (IsExist(this.K3CloudContext, item))
                                {
                                    char isSameAdress = default(char);

                                    if (item.F_HS_IsSameAdress)
                                    {
                                        isSameAdress = '1';
                                    }
                                    else
                                    {
                                        isSameAdress = '0';
                                    }
                                    string uSql = string.Format(@"/*dialect*/ update T_SAL_ORDER set F_HS_PAYMENTSTATUS = '{0}',F_HS_SALEORDERSOURCE = '5a97d3123e9dff',F_HS_BillAddress = '{1}',F_HS_IsSameAdress = '{2}',F_HS_PaymentModeNew = '{4}' where FBILLNO = '{3}' and FDOCUMENTSTATUS != 'C' and FDOCUMENTSTATUS != 'D' and FDOCUMENTSTATUS != 'B' and FBILLNO <>''", item.F_HS_PaymentStatus, SQLUtils.DealQuotes(item.F_HS_BillAddress), isSameAdress, item.FBillNo, SQLUtils.GetPaymentMethodId(this.K3CloudContext, item.F_HS_PaymentModeNew));
                                    int count = DBUtils.Execute(this.K3CloudContext, uSql);

                                    if (count > 0 && item.F_HS_PaymentStatus != null && item.F_HS_PaymentStatus.CompareTo("3") == 0)
                                    {
                                        numbers.Add(item.FBillNo);
                                    }
                                }
                                else
                                {
                                    auditSuccNos.Add(item.FBillNo);

                                    result.Message += "编码为【" + item.FBillNo + "】的订单已经审核" + System.Environment.NewLine;
                                    result.Success = false;
                                }
                            }

                            if (numbers != null && numbers.Count > 0)
                            {
                                //单据提交
                                result = ExecuteOperate(SynOperationType.SUBMIT, numbers, null, null);

                                if (result != null && result.SuccessEntityNos != null && result.SuccessEntityNos.Count > 0)
                                {
                                    //单据审核
                                    result = ExecuteOperate(SynOperationType.AUDIT, result.SuccessEntityNos, null, null);

                                    if (result != null && result.SuccessEntityNos != null && result.SuccessEntityNos.Count > 0)
                                    {
                                        List<K3SalOrderInfo> auditOrders = null;
                                        //审核成功后的销售订单
                                        if (result.Success)
                                        {
                                            auditOrders = GetSelectedSalOrders(first, result.SuccessEntityNos);
                                        }
                                        else
                                        {
                                            auditOrders = GetSelectedSalOrders(first, GetAuditedSalOrderNos(this.K3CloudContext, second));
                                        }

                                        //第二次同步后的销售订单信息和第一次同步的销售订单信息合成新的销售订单列表信息（获取完整的销售订单信息）
                                        auditOrders = CombineSalOrder(second, auditOrders);
                                        //从redis再次获取数据
                                        if (auditOrders == null || auditOrders.Count == 0)
                                        {
                                            //auditOrders = GetAuditedSalOrderDatas(ctx, second);
                                            if (second != null)
                                            {
                                                auditOrders = GetSalOrdersByDb(this.K3CloudContext, second.Select(o => o.FBillNo).ToList());
                                            }
                                        }
                                        //同步收款单
                                        if (auditOrders != null)
                                        {
                                            Dictionary<SynOperationType, IEnumerable<AbsSynchroDataInfo>> dict = new Dictionary<SynOperationType, IEnumerable<AbsSynchroDataInfo>>();
                                            dict.Add(SynOperationType.SAVE, auditOrders);

                                            HttpResponseResult respone = SynchroDataHelper.SynchroDataToK3(this.K3CloudContext, SynchroDataType.ReceiveBill, true, null, dict);
                                            //收款单同步成功后删除销售订单记录
                                            if (respone != null)
                                            {
                                                first.RemoveWhere(o => respone.SuccessEntityNos.Contains(o.FBillNo));
                                            }
                                        }

                                        //更新客户下单次数
                                        StatisticsOrderCount(this.K3CloudContext, result.SuccessEntityNos);
                                        //审核成功后删除Redis中的数据(销售订单第二次同步)
                                        RemoveRedisData(this.K3CloudContext, result.SuccessEntityNos);

                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        LogUtils.WriteSynchroLog(this.K3CloudContext, this.DataType, "未找到需要同步的数据！");
                    }
                }
                catch (Exception ex)
                {
                    LogUtils.WriteSynchroLog(this.K3CloudContext, this.DataType, "数据批量更新过程中出现异常，异常信息：" + ex.Message + System.Environment.NewLine + ex.StackTrace);
                }
            }

            return result;
        }

        /// <summary>
        /// 销售订单二次同步成功后，更新订单相应的字段信息
        /// </summary>
        /// <param name="second">销售订单第二次同步的数据</param>
        /// <param name="first">销售订单第一次同步的数据</param>
        /// <returns></returns>
        private List<K3SalOrderInfo> CombineSalOrder(List<K3SalOrderInfo> second, List<K3SalOrderInfo> first)
        {
            List<K3SalOrderInfo> orders = null;

            if (second != null && second.Count > 0)
            {
                if (first != null && first.Count > 0)
                {
                    orders = new List<K3SalOrderInfo>();

                    foreach (var order in second)
                    {
                        if (order != null)
                        {
                            foreach (var order_ in first)
                            {
                                if (order_ != null)
                                {
                                    if (order.FBillNo.CompareTo(order_.FBillNo) == 0)
                                    {
                                        order_.F_HS_PaymentModeNew = order.F_HS_PaymentModeNew;
                                        order_.F_HS_PaymentStatus = order.F_HS_PaymentStatus;
                                        order_.F_HS_SaleOrderSource = order.F_HS_SaleOrderSource;
                                        order_.F_HS_BillAddress = order.F_HS_BillAddress;

                                        orders.Add(order_);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return orders;
        }

        /// <summary>
        /// 当服务器重启，存储在内存的第一次同步过来的订单数据消失后，从Redis再次读取数据和第二次同步过来的订单数据合成新的销售订单列表
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="second">第二次同步过来的订单数据</param>
        /// <returns></returns>
        private List<K3SalOrderInfo> GetAuditedSalOrderDatas(Context ctx, List<K3SalOrderInfo> second)
        {
            List<K3SalOrderInfo> first = null;
            List<string> orderNos = null;

            if (second != null && second.Count > 0)
            {
                orderNos = second.Select(o => o.FBillNo).ToList();

                if (orderNos != null && orderNos.Count > 0)
                {
                    var datas = ServiceHelper.GetSynchroDatas(ctx, SynchroDataType.SaleOrder, this.RedisDbId, new HashSet<string>(orderNos),this.Direction);

                    if (datas != null && datas.Count() > 0)
                    {
                        first = datas.Select(o => (K3SalOrderInfo)o).ToList();
                    }
                }
            }
            return CombineSalOrder(second, first);
        }

        /// <summary>
        /// 数据库查询需要生成收款单对应销售订单的信息
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="orderNos"></param>
        /// <returns></returns>
        private List<K3SalOrderInfo> GetSalOrdersByDb(Context ctx, List<string> orderNos)
        {
            List<K3SalOrderInfo> orders = null; ;
            K3SalOrderInfo order = null;

            if (orderNos != null && orderNos.Count > 0)
            {
                string sql = string.Format(@"/*dialect*/ select distinct FBillNo,FDate,j.FNUMBER  as FCUSTID,j.FNUMBER  as F_HS_B2CCustId,m.FNUMBER as FSalerId,n.FNUMBER as FSALEDEPTID,z.FNUMBER as FSaleOrgId,o.FNumber as FSettleCurrId,F_HS_RateToUSA,a.F_HS_TransactionID,F_HS_PayTotal,q.FNUMBER as F_HS_PaymentModeNew
			                                    from T_SAL_ORDER a 
			                                    inner join T_SAL_ORDERENTRY b on b.FID = a.FID
			                                    inner join T_SAL_ORDERENTRY_F c on c.FENTRYID = b.FENTRYID and c.FID = b.FID
			                                    inner join T_SAL_ORDERFIN d on d.FID = a.FID
			                                    inner join T_BD_CUSTOMER e on e.FCUSTID= a.FCUSTID
			                                    inner join T_BAS_ASSISTANTDATAENTRY_L f ON a.F_HS_SaleOrderSource=f.FENTRYID
			                                    inner join T_BAS_ASSISTANTDATAENTRY g ON f.FentryID=g.FentryID
			                                    left join T_BAS_BILLTYPE h on a.FBILLTypeID=h.FBILLTypeID
			                                    inner join T_ORG_ORGANIZATIONS z on a.FSALEORGID=z.FORGID
			                                    inner join T_BD_CUSTOMER j on j.FCUSTID = a.F_HS_B2CCUSTID
			                                    inner join T_BD_CURRENCY o on o.FCURRENCYID = d.FSettleCurrId
			                                    inner join T_BAS_ASSISTANTDATAENTRY_L p ON a.F_HS_PaymentModeNew=p.FENTRYID
			                                    inner join T_BAS_ASSISTANTDATAENTRY q ON q.FentryID=p.FentryID
												inner join V_BD_SALESMAN m on m.FID = a.FSALERID
												inner join T_BD_DEPARTMENT n on n.FDEPTID = m.FDEPTID
			                                    where a.FCANCELSTATUS<>'B' 
			                                    and h.FNUMBER='XSDD01_SYS' 
			                                    and z.FNUMBER='100.01'
			                                    and a.FBILLNO in('{0}')", string.Join("','", orderNos));

                DynamicObjectCollection coll = SQLUtils.GetObjects(ctx, sql);

                if (coll != null && coll.Count > 0)
                {
                    orders = new List<K3SalOrderInfo>();

                    foreach (var item in coll)
                    {
                        if (item != null)
                        {
                            order = new K3SalOrderInfo();

                            order.FBillNo = SQLUtils.GetFieldValue(item, "FBILLNO");
                            order.FDate = Convert.ToDateTime(SQLUtils.GetFieldValue(item, "FDATE"));
                            order.FCustId = SQLUtils.GetFieldValue(item, "FCUSTID");
                            order.F_HS_B2CCustId = SQLUtils.GetFieldValue(item, "F_HS_B2CCustId");
                            order.FSalerId = SQLUtils.GetFieldValue(item, "FSalerId");
                            order.FSaleDeptId = SQLUtils.GetFieldValue(item, "FSALEDEPTID");
                            order.FSaleOrgId = SQLUtils.GetFieldValue(item, "FSaleOrgId");
                            order.FSettleCurrId = SQLUtils.GetFieldValue(item, "FSettleCurrId");
                            order.F_HS_PaymentModeNew = SQLUtils.GetFieldValue(item, "F_HS_PaymentModeNew");
                            order.F_HS_TransactionID = SQLUtils.GetFieldValue(item, "F_HS_TransactionID");
                            order.F_HS_PayTotal = SQLUtils.GetFieldValue(item, "F_HS_PayTotal");
                            order.F_HS_RateToUSA = Convert.ToDecimal(SQLUtils.GetFieldValue(item, "F_HS_RateToUSA"));

                            orders.Add(order);
                        }
                    }
                }
            }
            return orders;
        }
    }
}
