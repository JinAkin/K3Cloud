using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS;
using Hands.K3.SCM.APP.Entity.SynDataObject.ReFundBillObject;

using Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject;
using System.ComponentModel;
using Hands.K3.SCM.APP.Utils.Utils;
using HS.K3.Common.Abbott;
using Hands.K3.SCM.APP.Entity.K3WebApi;

namespace Hands.K3.SCM.App.ServicePlugIn
{
    [Kingdee.BOS.Util.HotUpdate]
    [Description("收款退款单--同步收款退款单至HC网站插件")]
    public class ReFundBillSerPlugIn : AbstractOSPlugIn
    {
        HttpResponseResult result = null;
        public override SynchroDataType DataType
        {
            get
            {
                return SynchroDataType.ReFundBill;
            }
        }

        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            e.FieldKeys.Add("F_HS_RefundType");
            e.FieldKeys.Add("F_HS_RefundMethod");
            e.FieldKeys.Add("F_HS_B2CCUSTID");
            e.FieldKeys.Add("FREALREFUNDAMOUNTFOR_D");
        }

        public override void OnAddValidators(AddValidatorsEventArgs e)
        {
            base.OnAddValidators(e);

            List<DynamicObject> objects = e.DataEntities.ToList();
            result = OperateAfterAudit(this.Context, objects);

            AuditValidator validator = new AuditValidator(result,this.BusinessInfo.GetForm().Id);
            validator.EntityKey = "FBillHead";
            e.Validators.Add(validator);
        }

        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);

            List<DynamicObject> objects = e.DataEntitys.ToList();
            result = OperateAfterAudit(this.Context, objects);
        }

        public override void AfterExecuteOperationTransaction(AfterExecuteOperationTransaction e)
        {
            base.AfterExecuteOperationTransaction(e);

            if (result != null && !result.Success && !string.IsNullOrWhiteSpace(result.Message))
            {
                throw new Exception(result.Message);
            }
        }
        /// <summary>
        /// 收款退款单明细
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private List<ReFundBillEntry> GetReFundEntry(Context ctx, DynamicObject obj)
        {
            List<ReFundBillEntry> entries = null;
            ReFundBillEntry entry = null;
            if (obj != null)
            {
                DynamicObject cust = obj["F_HS_B2CCUSTID"] as DynamicObject;
                string custNo = SQLUtils.GetFieldValue(cust, "Number");

                DynamicObject curr = obj["CURRENCYID"] as DynamicObject;
                string currNo = SQLUtils.GetFieldValue(curr, "Number");

                decimal custCredit = LogHelper.GetCustCreditLine(ctx, custNo, "F_HS_CreditLineUSD");
                decimal resiCustCredit = LogHelper.GetCustCreditLine(ctx, custNo, "F_HS_SurplusCreditUSD");
                decimal rateToUSA = GetExchangeRate(ctx, currNo);

                DynamicObjectCollection coll = obj["REFUNDBILLENTRY"] as DynamicObjectCollection;

                if (coll != null && coll.Count > 0)
                {
                    entries = new List<ReFundBillEntry>();

                    foreach (var item in coll)
                    {
                        if (item != null)
                        {
                            entry = new ReFundBillEntry();

                            entry.FEntryId = Convert.ToInt32(SQLUtils.GetFieldValue(item, "Id"));
                            entry.FRealReFundAmountFor_D = Math.Round(Convert.ToDecimal(SQLUtils.GetFieldValue(item, "REALREFUNDAMOUNTFOR")), 2);

                            DynamicObject sType = item["SETTLETYPEID"] as DynamicObject;
                            entry.FSettleTypeId = SQLUtils.GetFieldValue(sType, "Number");

                            decimal reFundTurnUSD = Math.Round(entry.FRealReFundAmountFor_D / rateToUSA ,2);

                            if (custCredit > 0)
                            {
                                if (reFundTurnUSD >= (custCredit - resiCustCredit))
                                {
                                    entry.F_HS_CreditLineRechargeUSD = custCredit - resiCustCredit;
                                    entry.F_HS_BalanceRechargeUSD = reFundTurnUSD - entry.F_HS_CreditLineRechargeUSD;
                                }
                                else
                                {
                                    entry.F_HS_CreditLineRechargeUSD = reFundTurnUSD;
                                    entry.F_HS_BalanceRechargeUSD = 0;
                                }    
                            }
                            else if (custCredit == 0)
                            {
                                entry.F_HS_CreditLineRechargeUSD = 0;
                                entry.F_HS_BalanceRechargeUSD = reFundTurnUSD;
                            }

                            entries.Add(entry);
                        }
                    }
                }
            }
            return entries;
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
                            if (data != null)
                            {
                                AbsDataInfo reFund = data as AbsDataInfo;

                                if (reFund != null)
                                {

                                    if (reFund.F_HS_RateToUSA > 0)
                                    {
                                        string billNo = reFund.FBillNo.Contains("_") ? reFund.FBillNo.Split('_')[0] : reFund.FBillNo;


                                        sql += string.Format(@"/*dialect*/ update a set a.F_HS_YNSync ='{0}',a.F_HS_RateToUSA = {1}
                                                                    from T_AR_REFUNDBILL a
                                                                    inner join T_AR_REFUNDBILLENTRY b on a.FID = b.FID
                                                                    inner join T_BD_CUSTOMER c on a.F_HS_B2CCustId = c.FCUSTID
                                                                    inner join T_ORG_ORGANIZATIONS d on a.FSALEORGID = d.FORGID
                                                                    where a.F_HS_YNSync = '{2}'and c.FNumber = '{3}'
                                                                    and a.FBillNo = '{4}'
                                                                    and d.FNUMBER = '{5}'", "1", reFund.F_HS_RateToUSA, "0", reFund.F_HS_B2CCustId, billNo, reFund.FSaleOrgId) + Environment.NewLine;
                                        sql += string.Format(@"/*dialect*/ update a set a.F_HS_RefundTurnUSD = {0},F_HS_BalanceRechargeUSD = {4},F_HS_CreditLineRechargeUSD = {5}
                                                                    from T_AR_REFUNDBILLENTRY a
			                                                        inner join T_AR_REFUNDBILL b on b.FID = a.FID
			                                                        inner join T_ORG_ORGANIZATIONS c on b.FSALEORGID = c.FORGID
			                                                        where b.FBILLNO = '{1}'
			                                                        and c.FNUMBER = '{2}'
			                                                        and a.FENTRYID = {3}", reFund.FRealAmountFor_USD, billNo, reFund.FSaleOrgId, reFund.FEntryId
                                                                    ,reFund.F_HS_BalanceRechargeUSD,reFund.F_HS_CreditLineRechargeUSD) + Environment.NewLine;
                                        sql += string.Format(@"/*dialect*/  update a set a.F_HS_USDBalance = a.F_HS_USDBalance + {0}{3}{4}
                                                                    from T_BD_CUSTOMER a
                                                                    left join T_AR_REFUNDBILL b on a.FCUSTID = b.F_HS_B2CCustId
                                                                    left join T_AR_REFUNDBILLENTRY c on b.FID = c.FID
                                                                    inner join T_ORG_ORGANIZATIONS d on a.FUSEORGID = d.FORGID
                                                                    where a.FNUMBER = '{1}'
                                                                    and d.FNUMBER = '{2}'", reFund.F_HS_BalanceRechargeUSD, reFund.F_HS_B2CCustId, reFund.FSaleOrgId
                                                                    , reFund.FSaleOrgId.CompareTo("100.03") == 0 && reFund.FSettleCurrId.CompareTo("CNY") == 0 ? string.Format(@",a.F_HS_CNYBalance = a.F_HS_CNYBalance + {0}"
                                                                    , reFund.FRealAmountFor) : "", string.Format(@",a.F_HS_SurplusCreditUSD = a.F_HS_SurplusCreditUSD + {0}", reFund.F_HS_CreditLineRechargeUSD) )+ System.Environment.NewLine;
                                        sql = "'" + SQLUtils.DealQuotes(sql) + "'";
                                        execSql += string.Format(@"EXEC(" + sql + ")") + Environment.NewLine;
                                        execSql += string.Format(@"declare @balance{2} decimal(10,2)
                                                                   declare @cnybalance{4} decimal(10,2)
                                                                   declare @creditbalance{11} decimal(10,2)
                                                                       set @balance{3} = ( select F_HS_USDBalance from T_BD_CUSTOMER a 
                                                                                           inner join T_ORG_ORGANIZATIONS b on a.FUSEORGID = b.FORGID
                                                                                           where a.FNUMBER = '{0}' and b.FNUMBER = '{1}'
                                                                                         )
                                                                       set @cnybalance{5} = ( select F_HS_CNYBalance from T_BD_CUSTOMER a 
                                                                                              inner join T_ORG_ORGANIZATIONS b on a.FUSEORGID = b.FORGID
                                                                                              where a.FNUMBER = '{6}' and b.FNUMBER = '{7}'
                                                                                            )
                                                                       set @creditbalance{8} = ( select F_HS_SurplusCreditUSD from T_BD_CUSTOMER a 
                                                                                              inner join T_ORG_ORGANIZATIONS b on a.FUSEORGID = b.FORGID
                                                                                              where a.FNUMBER = '{9}' and b.FNUMBER = '{10}'
                                                                                        )
                                                            ", reFund.F_HS_B2CCustId, reFund.FSaleOrgId, i, i, i, i, reFund.F_HS_B2CCustId, reFund.FSaleOrgId, i, reFund.F_HS_B2CCustId, reFund.FSaleOrgId, i) + System.Environment.NewLine;

                                        
                                        if (reFund.F_HS_BalanceRechargeUSD > 0)
                                        {
                                            execSql += string.Format(@"/*dialect*/ insert into HS_T_customerBalance(F_HS_TradeType,F_HS_UseOrgId,F_HS_B2CCUSTID,changedAmount,changedType,changedCause
                                                               ,balanceAmount,F_HS_RateToUSA,FSETTLECURRID,changedAmountUSA,balanceAmountUSA,F_HS_CNYBalance,updateTime
                                                               ,updateUser,FBillNo,fentryID,needfreezed,remark) values ('{18}','{15}','{0}',{1},'{2}','{3}',@balance{13}*{17},{4},{5}
                                                               ,{6},@balance{14}, @cnybalance{16},'{7}',{8},'{9}',{10},{11},'{12}') ", SQLUtils.GetCustomerId(ctx, reFund.F_HS_B2CCustId, 1), reFund.F_HS_BalanceRechargeUSD * reFund.F_HS_RateToUSA
                                                                   , reFund.ChangedType, reFund.ChangedCause, reFund.F_HS_RateToUSA, SQLUtils.GetSettleCurrId(ctx, reFund.FSettleCurrId)
                                                                   , reFund.F_HS_BalanceRechargeUSD, DateTime.Now, ctx.UserId, billNo, reFund.FEntryId, reFund.NeedFreezed == false ? 0 : 1, reFund.Remark, i, i
                                                                   , SQLUtils.GetOrgId(ctx, reFund.FSaleOrgId), i, reFund.F_HS_RateToUSA, "余额") + Environment.NewLine;
                                        }

                                        if (reFund.F_HS_CreditLineRechargeUSD > 0)
                                        {
                                            execSql += string.Format(@"/*dialect*/ insert into HS_T_customerBalance(F_HS_TradeType,F_HS_UseOrgId,F_HS_B2CCUSTID,changedAmount,changedType,changedCause
                                                               ,balanceAmount,F_HS_RateToUSA,FSETTLECURRID,changedAmountUSA,balanceAmountUSA,F_HS_CNYBalance,updateTime
                                                               ,updateUser,FBillNo,fentryID,needfreezed,remark) values ('{18}','{15}','{0}',{1},'{2}','{3}',@creditbalance{13}*{17},{4},{5}
                                                               ,{6},@creditbalance{14}, @cnybalance{16},'{7}',{8},'{9}',{10},{11},'{12}') ", SQLUtils.GetCustomerId(ctx, reFund.F_HS_B2CCustId, 1), reFund.F_HS_CreditLineRechargeUSD * reFund.F_HS_RateToUSA
                                                                  , reFund.ChangedType, reFund.ChangedCause, reFund.F_HS_RateToUSA, SQLUtils.GetSettleCurrId(ctx, reFund.FSettleCurrId)
                                                                  , reFund.F_HS_CreditLineRechargeUSD , DateTime.Now, ctx.UserId, billNo, reFund.FEntryId, reFund.NeedFreezed == false ? 0 : 1, reFund.Remark, i, i
                                                                  , SQLUtils.GetOrgId(ctx, reFund.FSaleOrgId), i, reFund.F_HS_RateToUSA, "剩余信用额度") + Environment.NewLine;
                                        }
                                        sql = string.Empty;
                                        i++;
                                    }
                                    else
                                    {
                                        LogUtils.WriteSynchroLog(ctx, this.DataType, "获取【" + reFund.FSettleCurrId + "币别】兑美元的实时汇率失败");
                                    }
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

        public override IEnumerable<AbsSynchroDataInfo> GetK3Datas(Context ctx, List<DynamicObject> objects,ref HttpResponseResult result)
        {
            List<AbsDataInfo> reFunds = null;
            AbsDataInfo reFund = null;

            result = new HttpResponseResult();
            result.Success = true;

            if (objects != null && objects.Count > 0)
            {
                reFunds = new List<AbsDataInfo>();

                foreach (var obj in objects)
                {
                    if (obj != null)

                    {
                        string reFundMethod = SQLUtils.GetReFundMethod(ctx, obj, "F_HS_RefundMethod_Id");
                        bool isSyn = Convert.ToBoolean(SQLUtils.GetFieldValue(obj, "F_HS_YNSync"));

                        DynamicObject org = obj["SALEORGID"] as DynamicObject;
                        string orgNo = SQLUtils.GetFieldValue(org, "Number");

                        string custNo = SQLUtils.GetCustomerNo(ctx, obj, "F_HS_B2CCUSTID_Id");

                        if (!isSyn)
                        {
                            if (!string.IsNullOrWhiteSpace(reFundMethod))
                            {
                                List<ReFundBillEntry> entrys = GetReFundEntry(ctx,obj);

                                if (reFundMethod.CompareTo("TKDKHYE") == 0 || reFundMethod.CompareTo("TJF") == 0)
                                {
                                    if (entrys != null && entrys.Count > 0)
                                    {
                                        foreach (var entry in entrys)
                                        {
                                            if (entry != null)
                                            {
                                                reFund = new AbsDataInfo(this.DataType);


                                                reFund.FSaleOrgId = orgNo;

                                                reFund.SrcNo = SQLUtils.GetFieldValue(obj, "BILLNo") + "_" + entry.FEntryId;
                                                reFund.FBillNo = SQLUtils.GetFieldValue(obj, "BILLNo") + "_" + entry.FEntryId;
                                                reFund.FEntryId = entry.FEntryId;

                                                reFund.F_HS_RefundMethod = reFundMethod;
                                                reFund.F_HS_RefundType = SQLUtils.GetReFundType(ctx, obj, "F_HS_RefundType_Id");

                                                reFund.F_HS_B2CCustId = custNo;
                                                reFund.FDate = TimeHelper.GetTimeStamp(Convert.ToDateTime(SQLUtils.GetFieldValue(obj, "DATE")));

                                                reFund.FSettleCurrId = SQLUtils.GetSettleCurrNo(ctx, obj, "CURRENCYID_Id");
                                                reFund.F_HS_RateToUSA = GetExchangeRate(ctx, reFund.FSettleCurrId);

                                                reFund.FSettleTypeId = entry.FSettleTypeId;
                                                reFund.FBusinessTime = SQLUtils.GetFieldValue(obj, "ApproveDate");

                                                if (reFund.F_HS_RefundMethod.CompareTo("TJF") == 0)
                                                {
                                                    reFund.F_HS_BalanceRechargeUSD = 0;
                                                }
                                                else if (reFund.F_HS_RefundMethod.CompareTo("TKDKHYE") == 0)
                                                {
                                                    reFund.F_HS_BalanceRechargeUSD = entry.F_HS_BalanceRechargeUSD;
                                                    reFund.F_HS_CreditLineRechargeUSD = entry.F_HS_CreditLineRechargeUSD;

                                                    if (reFund.F_HS_CreditLineRechargeUSD < 0)
                                                    {
                                                        reFund.F_HS_BalanceRechargeUSD = entry.FRealReFundAmountFor_D / reFund.F_HS_RateToUSA;
                                                    }
                                                }
                                                reFunds.Add(reFund);
                                            }
                                        }
                                    }
                                }

                                if (reFundMethod.CompareTo("JYE01") == 0)
                                {
                                    decimal balance = LogHelper.GetCustBalance(ctx, custNo, orgNo);
                                    decimal amount = 0;
                                    int count = 0;


                                    if (entrys != null && entrys.Count > 0)
                                    {
                                        foreach (var entry in entrys)
                                        {
                                            count++;
                                            amount += entry.FRealReFundAmountFor_D;

                                            if (count < entrys.Count)
                                                continue;
                                            if (balance > 0 && amount <= balance)
                                            {
                                                foreach (var en in entrys)
                                                {
                                                    if (entry != null)
                                                    {
                                                        reFund = new AbsDataInfo(this.DataType);

                                                        reFund.FSaleOrgId = orgNo;

                                                        reFund.SrcNo = SQLUtils.GetFieldValue(obj, "BILLNo") + "_" + en.FEntryId;
                                                        reFund.FBillNo = SQLUtils.GetFieldValue(obj, "BILLNo") + "_" + en.FEntryId;
                                                        reFund.FEntryId = en.FEntryId;

                                                        reFund.F_HS_RefundMethod = reFundMethod;
                                                        reFund.F_HS_RefundType = SQLUtils.GetReFundType(ctx, obj, "F_HS_RefundType_Id");

                                                        reFund.F_HS_B2CCustId = custNo;
                                                        reFund.FDate = TimeHelper.GetTimeStamp(Convert.ToDateTime(SQLUtils.GetFieldValue(obj, "DATE")));

                                                        reFund.FSettleCurrId = SQLUtils.GetSettleCurrNo(ctx, obj, "CURRENCYID_Id");
                                                        reFund.F_HS_RateToUSA = GetExchangeRate(ctx, reFund.FSettleCurrId);

                                                        reFund.FSettleTypeId = en.FSettleTypeId;

                                                        reFund.F_HS_BalanceRechargeUSD = -en.FRealReFundAmountFor_D / GetExchangeRate(ctx,"USD");

                                                        reFunds.Add(reFund);
                                                    }
                                                }

                                            }
                                        }
                                    }
                                }
                            }

                        }
                        else
                        {
                            result.Success = false;
                            result.Message = "收款退款单已同步" + Environment.NewLine;
                            LogUtils.WriteSynchroLog(ctx, SynchroDataType.ReFundBill, result.Message);
                        }
                    }
                }
            }

            return reFunds;
        }
    }
}
