using System;
using System.Collections.Generic;
using System.Linq;
using Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Hands.K3.SCM.APP.Entity.SynDataObject.SaleOrder;
using System.ComponentModel;
using Hands.K3.SCM.APP.Utils.Utils;
using HS.K3.Common.Abbott;
using Hands.K3.SCM.APP.Entity.K3WebApi;
using Hands.K3.SCM.APP.Entity.StructType;

namespace Hands.K3.SCM.App.ServicePlugIn
{
    [Kingdee.BOS.Util.HotUpdate]
    [Description("销售订单--销售订单客户余额的维护")]
    public class SalOrderBalanceSerPlugIn : AbstractOSPlugIn
    {
        HttpResponseResult result = null;

        public string SalOrderType { get; set; }
        public override SynchroDataType DataType
        {
            get
            {
                switch (SalOrderType)
                {
                    case "online":
                        return SynchroDataType.SaleOrder;
                    case "offline":
                        return SynchroDataType.SaleOrderOffline;
                    case "dropshipping":
                        return SynchroDataType.DropShippingSalOrder;
                    default:
                        return SynchroDataType.None;
                }
            }
        }
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);

            e.FieldKeys.Add("F_HS_B2CCustId");
            e.FieldKeys.Add("F_HS_RateToUSA");
            e.FieldKeys.Add("F_HS_BalanceDeducted");
            e.FieldKeys.Add("F_HS_BalancePayments");
            e.FieldKeys.Add("F_HS_USDBalancePayments");
            e.FieldKeys.Add("F_HS_SaleOrderSource");
            e.FieldKeys.Add("F_HS_CNYBalancePayments");
            e.FieldKeys.Add("F_HS_RateToUSA");
            e.FieldKeys.Add("F_HS_CheckBox");
            e.FieldKeys.Add("F_HS_CreditLineUSDPayments");
            e.FieldKeys.Add("FBillAmount");
        }

        public override void OnAddValidators(AddValidatorsEventArgs e)
        {
            base.OnAddValidators(e);

            //if (this.BusinessInfo.GetForm().Id.CompareTo("SAL_SaleOrder") == 0)
            //{
            //    List<DynamicObject> offline = null;
            //    List<DynamicObject> online = null;
            //    List<DynamicObject> dropshipping = null;
            //    this.DyamicObjects = e.DataEntities.ToList();

            //    offline = this.DyamicObjects.Where(o =>
            //    {
            //        DynamicObject oType = o["F_HS_SaleOrderSource"] as DynamicObject;
            //        return SQLUtils.GetFieldValue(oType, "FNumber").CompareTo("XXBJDD") == 0;
            //    }).ToList();

            //    online = this.DyamicObjects.Where(o =>
            //    {
            //        DynamicObject oType = o["F_HS_SaleOrderSource"] as DynamicObject;
            //        string orderSrc = SQLUtils.GetFieldValue(oType, "FNumber");
            //        return orderSrc.CompareTo("HCWebPendingOder") == 0 || orderSrc.CompareTo("HCWebProcessingOder") == 0;
            //    }).ToList();

            //    dropshipping = this.DyamicObjects.Where(o =>
            //    {
            //        DynamicObject oType = o["F_HS_SaleOrderSource"] as DynamicObject;
            //        string orderSrc = SQLUtils.GetFieldValue(oType, "FNumber");
            //        return orderSrc.CompareTo("DropShippingOrder") == 0;
            //    }).ToList();

            //    if (offline != null && offline.Count > 0)
            //    {
            //        SalOrderType = "offline";
            //        result = OperateAfterAudit(this.Context, offline);
            //    }
            //    if (online != null && online.Count > 0)
            //    {
            //        SalOrderType = "online";
            //        result = OperateAfterAudit(this.Context, online);
            //    }
            //    if (dropshipping != null && dropshipping.Count > 0)
            //    {
            //        SalOrderType = "dropshipping";
            //        result = OperateAfterAudit(this.Context, dropshipping);
            //    }

            //}

            //AuditValidator validator = new AuditValidator(result, this.BusinessInfo.GetForm().Id);
            //validator.EntityKey = "FBillHead";
            //e.Validators.Add(validator);
        }

        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);

            if (this.BusinessInfo.GetForm().Id.CompareTo("SAL_SaleOrder") == 0)
            {
                List<DynamicObject> offline = null;
                List<DynamicObject> online = null;
                List<DynamicObject> dropshipping = null;
                this.DyamicObjects = e.DataEntitys.ToList();

                offline = this.DyamicObjects.Where(o =>
                {
                    DynamicObject oType = o["F_HS_SaleOrderSource"] as DynamicObject;
                    return SQLUtils.GetFieldValue(oType, "FNumber").CompareTo("XXBJDD") == 0;
                }).ToList();

                online = this.DyamicObjects.Where(o =>
                {
                    DynamicObject oType = o["F_HS_SaleOrderSource"] as DynamicObject;
                    string orderSrc = SQLUtils.GetFieldValue(oType, "FNumber");
                    return orderSrc.CompareTo("HCWebPendingOder") == 0 || orderSrc.CompareTo("HCWebProcessingOder") == 0;
                }).ToList();

                dropshipping = this.DyamicObjects.Where(o =>
                {
                    DynamicObject oType = o["F_HS_SaleOrderSource"] as DynamicObject;
                    string orderSrc = SQLUtils.GetFieldValue(oType, "FNumber");
                    return orderSrc.CompareTo("DropShippingOrder") == 0;
                }).ToList();

                if (offline != null && offline.Count > 0)
                {
                    SalOrderType = "offline";
                    result = OperateAfterAudit(this.Context, offline);
                }
                if (online != null && online.Count > 0)
                {
                    SalOrderType = "online";
                    result = OperateAfterAudit(this.Context, online);
                }
                if (dropshipping != null && dropshipping.Count > 0)
                {
                    SalOrderType = "dropshipping";
                    result = OperateAfterAudit(this.Context, dropshipping);
                }
            }
        }
        public override void AfterExecuteOperationTransaction(AfterExecuteOperationTransaction e)
        {
            base.AfterExecuteOperationTransaction(e);

            if (result != null && !result.Success && !string.IsNullOrWhiteSpace(result.Message))
            {
                throw new Exception(result.Message);
            }
        }
        public override IEnumerable<AbsSynchroDataInfo> GetK3Datas(Context ctx, List<DynamicObject> objects, ref HttpResponseResult result)
        {
            List<AbsDataInfo> orders = null;
            AbsDataInfo order = null;

            result = new HttpResponseResult();
            result.Success = true;

            if (objects != null && objects.Count > 0)
            {
                orders = new List<AbsDataInfo>();

                foreach (var obj in objects)
                {
                    if (obj != null)
                    {
                        bool deducted = Convert.ToBoolean(SQLUtils.GetFieldValue(obj, "F_HS_BalanceDeducted"));
                        bool isApproval = Convert.ToBoolean(SQLUtils.GetFieldValue(obj, "F_HS_CheckBox"));

                        if (!deducted)
                        {
                            DynamicObject cust = obj["F_HS_B2CCustId"] as DynamicObject;
                            string custNo = SQLUtils.GetFieldValue(cust, "Number");
                            string currNo = GetOrderFinance(obj).FSettleCurrID;

                            string billNo = SQLUtils.GetFieldValue(obj, "BillNo");

                            DynamicObject sOrg = obj["SALEORGID"] as DynamicObject;
                            string orgNo = SQLUtils.GetFieldValue(sOrg, "Number");

                            DynamicObject src = obj["F_HS_SaleOrderSource"] as DynamicObject;
                            string sType = SQLUtils.GetFieldValue(src, "FNumber");

                            decimal rateToUSA = Convert.ToDecimal(SQLUtils.GetFieldValue(obj, "F_HS_RateToUSA"));
                            string documentStatus = SQLUtils.GetFieldValue(obj, "DocumentStatus");
                            decimal orderAmount = Math.Round(GetOrderFinance(obj).FBillAllAmount / rateToUSA, 2);
                            decimal useCnyBalance = Math.Round(Convert.ToDecimal(SQLUtils.GetFieldValue(obj, "F_HS_CNYBalancePayments")), 2);
                            decimal useBalance = Math.Round(Convert.ToDecimal(SQLUtils.GetFieldValue(obj, "F_HS_BalancePayments")), 2);
                            decimal useUsdBalance = Math.Round(Convert.ToDecimal(SQLUtils.GetFieldValue(obj, "F_HS_USDBalancePayments")), 2);
                            decimal useCredit = Math.Round(Convert.ToDecimal(SQLUtils.GetFieldValue(obj, "F_HS_CreditLineUSDPayments")), 2);
                            decimal useAmount = 0;

                            if (this.DataType == SynchroDataType.SaleOrder || this.DataType == SynchroDataType.SaleOrderOffline)
                            {
                                if (orgNo.CompareTo("100.03") != 0)
                                {
                                    useAmount = useUsdBalance + useCredit;
                                }
                                else
                                {
                                    useAmount = useCnyBalance + useCredit * rateToUSA;
                                }
                            }
                            if (sType.CompareTo("DropShippingOrder") == 0)
                            {
                                useAmount = orderAmount;
                            }

                            if (useAmount > 0)
                            {
                                decimal custBalance = 0;
                                decimal custSurplus = LogHelper.GetCustBalance(ctx, custNo, orgNo);
                                decimal custCredit = LogHelper.GetCustCreditLine(ctx, custNo, "F_HS_SurplusCreditUSD");

                                if (this.DataType == SynchroDataType.SaleOrder || this.DataType == SynchroDataType.SaleOrderOffline)
                                {
                                    custBalance = custSurplus;
                                }
                                else
                                {
                                    custBalance = custSurplus + custCredit;
                                }

                                if (useAmount <= custBalance)
                                {
                                    order = new AbsDataInfo(this.DataType);
                                    order.FSettleCurrId = currNo;

                                    if (order != null)
                                    {
                                        order.F_HS_RateToUSA = Convert.ToDecimal(SQLUtils.GetFieldValue(obj, "F_HS_RateToUSA"));
                                        order.F_HS_SaleOrderSource = sType;

                                        DynamicObject org = obj["SALEORGID"] as DynamicObject;
                                        order.FSaleOrgId = SQLUtils.GetFieldValue(org, "Number");

                                        order.FBillNo = billNo;
                                        order.SrcNo = order.FBillNo;

                                        order.FDate = TimeHelper.GetTimeStamp(Convert.ToDateTime(SQLUtils.GetFieldValue(obj, "Date")));
                                        order.F_HS_B2CCustId = custNo;
                                        order.FCancelStatus = SQLUtils.GetFieldValue(obj, "CancelStatus");
                                        order.FBusinessTime = SQLUtils.GetFieldValue(obj, "ApproveDate");
                                        order.FCustBalance = custSurplus;
                                        order.FCustBalanceAmount = custBalance;

                                        order.F_HS_BalanceRecharge = -useBalance;
                                        order.F_HS_BalanceRechargeUSD = -useUsdBalance;
                                        order.F_HS_CreditLineRechargeUSD = -useCredit;

                                        if (sType.CompareTo("DropShippingOrder") == 0)
                                        {
                                            order.FBillAmount = orderAmount;
                                            SetPayment(order);
                                        }

                                        orders.Add(order);

                                    }
                                }
                                else
                                {
                                    if (!string.IsNullOrWhiteSpace(sType))
                                    {
                                        if ((this.DataType.CompareTo(SynchroDataType.DropShippingSalOrder) == 0 && !isApproval)
                                            || this.DataType.CompareTo(SynchroDataType.SaleOrder) == 0 || this.DataType.CompareTo(SynchroDataType.SaleOrderOffline) == 0)
                                        {
                                            result.Success = false;
                                            result.Message = "单据编码【" + billNo + "】客户【" + custNo + "】余额支付大于客户结余！" + Environment.NewLine;
                                            LogUtils.WriteSynchroLog(ctx, this.DataType, result.Message);
                                        }

                                    }
                                }
                            }
                            
                        }
                        else
                        {
                            result.Success = false;
                            result.Message = "客户余额已扣减！" + Environment.NewLine;
                            LogUtils.WriteSynchroLog(ctx, this.DataType, result.Message);
                        }
                    }
                }
            }

            return orders;
        }
        private void SetPayment(AbsDataInfo info)
        {
            if (info != null)
            {

                if (info.FBillAmount > info.FCustBalance)
                {
                    info.F_HS_BalanceRechargeUSD = -info.FCustBalance;
                    info.F_HS_CreditLineRechargeUSD = -(info.FBillAmount + info.F_HS_BalanceRechargeUSD);
                }
                else
                {
                    info.F_HS_BalanceRechargeUSD = -info.FBillAmount;
                    info.F_HS_CreditLineRechargeUSD = 0;
                }
            }
        }
        private K3SaleOrderFinance GetOrderFinance(DynamicObject obj)
        {
            K3SaleOrderFinance fin = null;

            if (obj != null)
            {
                DynamicObjectCollection coll = obj["SaleOrderFinance"] as DynamicObjectCollection;

                if (coll != null && coll.Count > 0)
                {
                    foreach (var item in coll)
                    {
                        if (item != null)
                        {
                            fin = new K3SaleOrderFinance();
                            DynamicObject curr = item["SettleCurrId"] as DynamicObject;
                            fin.FSettleCurrID = SQLUtils.GetFieldValue(curr, "Number");
                            fin.FBillAllAmount = Convert.ToDecimal(SQLUtils.GetFieldValue(item, "BillAmount"));
                        }
                    }
                }
            }
            return fin;
        }

        public override string GetExecuteUpdateSql(Context ctx, List<AbsSynchroDataInfo> datas)
        {
            string sql = string.Empty;

            string execSql = string.Empty;
            int i = 0;

            try
            {
                if (datas != null && datas.Count > 0)
                {
                    foreach (var data in datas)
                    {
                        if (data != null)
                        {
                            AbsDataInfo order = data as AbsDataInfo;

                            if (order != null)
                            {
                                if (order.F_HS_RateToUSA > 0)
                                {
                                    //pending单作废成功
                                    if (order.F_HS_SaleOrderSource.CompareTo("HCWebPendingOder") == 0 && order.FCancelStatus.CompareTo("B") == 0
                                       && order.FRealAmountFor > 0 && !order.F_HS_BalanceDeducted)
                                    {
                                        execSql += string.Format(@"/*dialect*/ update a set a.F_HS_BalanceDeducted = '{0}' 
                                                                        from T_SAL_ORDER a 
                                                                        inner join T_BD_CUSTOMER b on a.FCUSTID = b.FCUSTID
                                                                        inner join T_ORG_ORGANIZATIONS c on a.FSALEORGID = c.FORGID
                                                                        where a.FBillNo = '{1}'
                                                                        and c.FNUMBER = '{2}'", "1", order.FBillNo, order.FSaleOrgId) + System.Environment.NewLine;
                                    }
                                    else
                                    {
                                        if (order.F_HS_SaleOrderSource.CompareTo("DropShippingOrder") == 0)
                                        {
                                            sql += string.Format(@"/*dialect*/ update a set a.F_HS_BalanceDeducted = '{0}' ,a.F_HS_BalancePayments = {1},a.F_HS_USDBalancePayments = {2}
                                                                ,a.F_HS_CreditLineUSDPayments = {3},a.F_HS_NeedPayAmount = 0,a.F_HS_PaymentStatus = '3'
                                                                from T_SAL_ORDER a 
                                                                inner join T_BD_CUSTOMER b on a.FCUSTID = b.FCUSTID
                                                                inner join T_ORG_ORGANIZATIONS c on a.FSALEORGID = c.FORGID
                                                                where a.FBillNo = '{4}'
                                                                and b.FNUMBER = '{5}'
                                                                and c.FNUMBER = '{6}'", "1", -order.F_HS_BalanceRecharge, -order.F_HS_BalanceRechargeUSD, -order.F_HS_CreditLineRechargeUSD, order.FBillNo, order.F_HS_B2CCustId, "100.01");

                                            sql += string.Format(@"/*dialect*/  update a set a.F_HS_USDBalance = a.F_HS_USDBalance + {0},a.F_HS_SurplusCreditUSD = F_HS_SurplusCreditUSD + {1}
                                                                from T_BD_CUSTOMER a
                                                                left join T_SAL_ORDER b on a.FCUSTID = b.FCUSTID
                                                                inner join T_ORG_ORGANIZATIONS c on a.FUSEORGID = c.FORGID
                                                                where a.FNUMBER = '{2}'
                                                                and c.FNUMBER = '{3}'", order.F_HS_BalanceRechargeUSD, order.F_HS_CreditLineRechargeUSD, order.F_HS_B2CCustId, "100.01") + System.Environment.NewLine;
                                        }
                                        else
                                        {
                                            sql += string.Format(@"/*dialect*/ update a set a.F_HS_USDBalance = a.F_HS_USDBalance + {0}{3}
                                                                from T_BD_CUSTOMER a
                                                                left join T_SAL_ORDER b on a.FCUSTID = b.FCUSTID
                                                                inner join T_ORG_ORGANIZATIONS c on a.FUSEORGID = c.FORGID
                                                                where a.FNUMBER = '{1}'
                                                                and c.FNUMBER = '{2}'", order.FRealAmountFor_USD, order.F_HS_B2CCustId, order.FSaleOrgId, order.FSaleOrgId.CompareTo("100.03") == 0 && order.FSettleCurrId.CompareTo("CNY") == 0 ? string.Format(@",a.F_HS_CNYBalance = a.F_HS_CNYBalance + {0}", order.FRealAmountFor) : "") + System.Environment.NewLine;


                                            sql += string.Format(@"/*dialect*/ update a set a.F_HS_BalanceDeducted = '{0}' 
                                                                    from T_SAL_ORDER a 
                                                                    inner join T_ORG_ORGANIZATIONS c on a.FSALEORGID = c.FORGID
                                                                    where a.FBillNo = '{1}'
                                                                    and c.FNUMBER = '{2}'", "1", order.FBillNo, order.FSaleOrgId) + System.Environment.NewLine;
                                        }


                                        sql = "'" + SQLUtils.DealQuotes(sql) + "'";
                                        execSql += string.Format(@"EXEC(" + sql + ")") + System.Environment.NewLine;

                                        if (order.F_HS_SaleOrderSource.CompareTo("DropShippingOrder") == 0)
                                        {
                                            execSql += string.Format(@"declare @balance{0} decimal(10,2)
                                                                       declare @cnybalance{1} decimal(10,2)
                                                                       declare @creditbalance{2} decimal(10,2)
                                                                       set @balance{3} = ( select F_HS_USDBalance from T_BD_CUSTOMER a 
                                                                                           inner join T_ORG_ORGANIZATIONS b on a.FUSEORGID = b.FORGID
                                                                                           where a.FNUMBER = '{4}' and b.FNUMBER = '{5}'
                                                                                         )
                                                                       set @cnybalance{6} = ( select F_HS_CNYBalance from T_BD_CUSTOMER a 
                                                                                              inner join T_ORG_ORGANIZATIONS b on a.FUSEORGID = b.FORGID
                                                                                              where a.FNUMBER = '{7}' and b.FNUMBER = '{8}'
                                                                                            )
                                                                       set @creditbalance{9} = ( select F_HS_SurplusCreditUSD from T_BD_CUSTOMER a 
                                                                                              inner join T_ORG_ORGANIZATIONS b on a.FUSEORGID = b.FORGID
                                                                                              where a.FNUMBER = '{10}' and b.FNUMBER = '{11}'
                                                                                        )
                                                            ", i, i, i, i, order.F_HS_B2CCustId, order.FSaleOrgId, i, order.F_HS_B2CCustId, order.FSaleOrgId, i, order.F_HS_B2CCustId, order.FSaleOrgId) + System.Environment.NewLine;

                                            if (order.F_HS_BalanceRechargeUSD < 0)
                                            {
                                                execSql += string.Format(@"/*dialect*/ insert into HS_T_customerBalance(F_HS_TradeType,F_HS_UseOrgId,F_HS_B2CCUSTID,changedAmount,changedType,changedCause
                                                                        ,balanceAmount,F_HS_RateToUSA,FSETTLECURRID,changedAmountUSA,balanceAmountUSA,F_HS_CNYBalance,updateTime
                                                                        ,updateUser,FBillNo,fentryID,needfreezed,remark) values ('{0}',{1},{2},{3},'{4}','{5}',@balance{6}*{7},{8},{9}
                                                                        ,{10},@balance{11}, @cnybalance{12},'{13}',{14},'{15}',{16},{17},'{18}')", "余额", SQLUtils.GetOrgId(ctx, order.FSaleOrgId), SQLUtils.GetCustomerId(ctx, order.F_HS_B2CCustId, 1)
                                                                        , order.F_HS_BalanceRechargeUSD * order.F_HS_RateToUSA, order.ChangedType, order.ChangedCause, i, order.F_HS_RateToUSA, order.F_HS_RateToUSA, SQLUtils.GetSettleCurrId(ctx, order.FSettleCurrId)
                                                                        , order.F_HS_BalanceRechargeUSD, i, i, DateTime.Now, ctx.UserId, order.FBillNo, order.FEntryId, order.NeedFreezed == false ? 0 : 1, order.Remark
                                                                    );
                                            }
                                            if (order.F_HS_CreditLineRechargeUSD < 0)
                                            {
                                                execSql += string.Format(@"/*dialect*/ insert into HS_T_customerBalance(F_HS_TradeType,F_HS_UseOrgId,F_HS_B2CCUSTID,changedAmount,changedType,changedCause
                                                                        ,balanceAmount,F_HS_RateToUSA,FSETTLECURRID,changedAmountUSA,balanceAmountUSA,F_HS_CNYBalance,updateTime
                                                                        ,updateUser,FBillNo,fentryID,needfreezed,remark) values ('{0}',{1},{2},{3},'{4}','{5}',@creditbalance{6}*{7},{8},{9}
                                                                        ,{10},@creditbalance{11}, @cnybalance{12},'{13}',{14},'{15}',{16},{17},'{18}') "
                                                                        , "信用额度", SQLUtils.GetOrgId(ctx, order.FSaleOrgId), SQLUtils.GetCustomerId(ctx, order.F_HS_B2CCustId, 1)
                                                                        , order.F_HS_CreditLineRechargeUSD * order.F_HS_RateToUSA, order.ChangedType, order.ChangedCause, i, order.F_HS_RateToUSA, order.F_HS_RateToUSA, SQLUtils.GetSettleCurrId(ctx, order.FSettleCurrId)
                                                                        , order.F_HS_CreditLineRechargeUSD, i, i, DateTime.Now, ctx.UserId, order.FBillNo, order.FEntryId, order.NeedFreezed == false ? 0 : 1, order.Remark
                                                                        ) + System.Environment.NewLine;
                                            }
                                        }

                                        else
                                        {
                                            execSql += string.Format(@"declare @balance{2} decimal(10,2)
                                                                   declare @cnybalance{4} decimal(10,2)
                                                                   set @balance{3} = ( select F_HS_USDBalance from T_BD_CUSTOMER a 
                                                                                       inner join T_ORG_ORGANIZATIONS b on a.FUSEORGID = b.FORGID
                                                                                       where a.FNUMBER = '{0}' and b.FNUMBER = '{1}'
                                                                                     )
                                                                   set @cnybalance{5} = ( select F_HS_CNYBalance from T_BD_CUSTOMER a 
                                                                                          inner join T_ORG_ORGANIZATIONS b on a.FUSEORGID = b.FORGID
                                                                                          where a.FNUMBER = '{6}' and b.FNUMBER = '{7}'
                                                                                        )
                                                            ", order.F_HS_B2CCustId, order.FSaleOrgId, i, i, i, i, order.F_HS_B2CCustId, order.FSaleOrgId) + System.Environment.NewLine;

                                            execSql += string.Format(@"/*dialect*/ insert into HS_T_customerBalance(F_HS_TradeType,F_HS_UseOrgId,F_HS_B2CCUSTID,changedAmount,changedType,changedCause
                                                                    ,balanceAmount,F_HS_RateToUSA,FSETTLECURRID,changedAmountUSA,balanceAmountUSA,F_HS_CNYBalance,updateTime
                                                                    ,updateUser,FBillNo,fentryID,needfreezed,remark) values ('{18}','{15}','{0}',{1},'{2}','{3}',@balance{13}*{17},{4},{5}
                                                                    ,{6},@balance{14}, @cnybalance{16},'{7}',{8},'{9}',{10},{11},'{12}') ", SQLUtils.GetCustomerId(ctx, order.F_HS_B2CCustId, 1), order.FRealAmountFor
                                                                    , order.ChangedType, order.ChangedCause, order.F_HS_RateToUSA, SQLUtils.GetSettleCurrId(ctx, order.FSettleCurrId)
                                                                    , order.FRealAmountFor_USD, DateTime.Now, ctx.UserId, order.FBillNo, order.FEntryId, order.NeedFreezed == false ? 0 : 1, order.Remark, i, i
                                                                    , SQLUtils.GetOrgId(ctx, order.FSaleOrgId), i, order.F_HS_RateToUSA, "余额") + System.Environment.NewLine;
                                        }


                                        sql = string.Empty;
                                        i++;
                                    }
                                }
                                else
                                {
                                    LogUtils.WriteSynchroLog(ctx, this.DataType, "获取【" + order.FSettleCurrId + "币别】兑美元的实时汇率失败");
                                    throw new Exception("获取【" + order.FSettleCurrId + "币别】兑美元的实时汇率失败！");
                                }
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogUtils.WriteSynchroLog(ctx, this.DataType, ex.Message + System.Environment.NewLine + ex.StackTrace);
            }
            return execSql;
        }
    }
}
