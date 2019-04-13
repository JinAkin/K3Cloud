using System;
using System.Collections.Generic;
using System.Linq;

using Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Hands.K3.SCM.APP.Entity.SynDataObject.ReceiveBillObject;
using System.ComponentModel;
using Hands.K3.SCM.APP.Utils.Utils;
using HS.K3.Common.Abbott;
using Hands.K3.SCM.APP.Entity.K3WebApi;

namespace Hands.K3.SCM.App.ServicePlugIn
{
    [Kingdee.BOS.Util.HotUpdate]
    [Description("收款单--收款单收款用途为客户充值")]
    public class ReceiveBillSerPlugIn : AbstractOSPlugIn
    {
        HttpResponseResult result = null;
        public override SynchroDataType DataType
        {
            get
            {
                return SynchroDataType.ReceiveBill;
            }
        }

        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            e.FieldKeys.Add("F_HS_B2CCUSTID");
            e.FieldKeys.Add("F_HS_YNRecharge");
            e.FieldKeys.Add("F_HS_SynchronizedRecharge");
            e.FieldKeys.Add("FREALRECAMOUNTFOR_D");
        }

        public override void OnAddValidators(AddValidatorsEventArgs e)
        {
            //base.OnAddValidators(e);

            //List<DynamicObject> objects = e.DataEntities.ToList();
            //result = OperateAfterAudit(this.Context, objects);

            //AuditValidator validator = new AuditValidator(result, this.BusinessInfo.GetForm().Id);
            //e.Validators.Add(validator);
        }
        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);

            List<DynamicObject> objects = e.DataEntitys.ToList();
            result = OperateAfterAudit(this.Context, objects);
        }

        public override void AfterExecuteOperationTransaction(AfterExecuteOperationTransaction e)
        {
            if (result != null && !result.Success && !string.IsNullOrWhiteSpace(result.Message))
            {
                throw new Exception(result.Message);
            }
        }

        public override IEnumerable<AbsSynchroDataInfo> GetK3Datas(Context ctx, List<DynamicObject> objects, ref HttpResponseResult result)
        {
            List<AbsDataInfo> receives = null;
            AbsDataInfo receive = null;

            result = new HttpResponseResult();
            result.Success = true;

            if (objects != null && objects.Count > 0)
            {
                receives = new List<AbsDataInfo>();

                foreach (var obj in objects)
                {
                    if (obj != null)
                    {
                        DynamicObject cust = obj["F_HS_B2CCUSTID"] as DynamicObject;
                        string custNo = SQLUtils.GetFieldValue(cust, "Number");

                        List<ReceiveBillEntry> entrys = GetReceiveBillEntry(ctx, obj);

                        if (entrys != null && entrys.Count > 0)
                        {
                            foreach (var entry in entrys)
                            {
                                if (entry != null)
                                {
                                    if (entry.F_HS_YNRecharge)
                                    {
                                        if (!entry.F_HS_SynchronizedRecharge)
                                        {
                                            receive = new AbsDataInfo(this.DataType);

                                            DynamicObject org = obj["SALEORGID"] as DynamicObject;
                                            receive.FSaleOrgId = SQLUtils.GetFieldValue(org, "Number");

                                            receive.SrcNo = SQLUtils.GetFieldValue(obj, "BILLNo") + "_" + entry.FEntryID;
                                            receive.FBillNo = SQLUtils.GetFieldValue(obj, "BILLNo") + "_" + entry.FEntryID;
                                            receive.FEntryId = entry.FEntryID;
                                            receive.F_HS_B2CCustId = custNo;
                                            receive.FDate = TimeHelper.GetTimeStamp(Convert.ToDateTime(SQLUtils.GetFieldValue(obj, "DATE")));

                                            DynamicObject curr = obj["CURRENCYID"] as DynamicObject;
                                            receive.FSettleCurrId = SQLUtils.GetFieldValue(curr, "Number");

                                            receive.F_HS_RateToUSA = GetExchangeRate(ctx, receive.FSettleCurrId);
                                            receive.FSettleTypeId = entry.FSETTLETYPEID;
                                            receive.FBusinessTime = SQLUtils.GetFieldValue(obj, "ApproveDate");

                                            receive.F_HS_BalanceRechargeUSD = entry.F_HS_BalanceRechargeUSD;
                                            receive.F_HS_CreditLineRechargeUSD = entry.F_HS_CreditLineRechargeUSD;

                                            if (receive.F_HS_CreditLineRechargeUSD < 0)
                                            {
                                                entry.F_HS_BalanceRechargeUSD = entry.FREALRECAMOUNTFOR_D / receive.F_HS_RateToUSA;
                                            }

                                            receives.Add(receive);
                                        }
                                        else
                                        {
                                            result.Success = false;
                                            result.Message = string.Format(@"收款单【{0}】已同步", receive.FBillNo);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return receives;
        }

        /// <summary>
        /// 3.3  计算明细余额充值金额USD、信用额度充值金额USD
        ///3.3.1 若客户.信用额度USD>0，则：
        ///3.3.1.1 若明细.实收金额USD>=客户.信用额度USD-客户.剩余信用额度USD,则：
        ///明细.信用额度充值金额USD=客户.信用额度USD-客户.剩余信用额度USD
        ///明细.余额充值金额USD=明细.实收金额USD-明细.信用额度充值金额USD
        ///3.3.1.2 若明细.实收金额USD<客户.信用额度USD-客户.剩余信用额度USD, 则：
        ///明细.信用额度充值金额USD=明细.实收金额USD
        ///明细.余额充值金额USD=0
        ///3.3.2  若客户.信用额度USD==0，则：
        ///明细.信用额度充值金额USD=0
        ///明细.余额充值金额USD=明细.实收金额USD
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        private List<ReceiveBillEntry> GetReceiveBillEntry(Context ctx, DynamicObject obj)
        {
            List<ReceiveBillEntry> entrys = null;
            ReceiveBillEntry entry = null;

            if (obj != null)
            {
                DynamicObjectCollection coll = obj["RECEIVEBILLENTRY"] as DynamicObjectCollection;

                DynamicObject cust = obj["F_HS_B2CCUSTID"] as DynamicObject;
                string custNo = SQLUtils.GetFieldValue(cust, "Number");

                DynamicObject curr = obj["CURRENCYID"] as DynamicObject;
                string currNo = SQLUtils.GetFieldValue(curr, "Number");

                decimal custCredit = LogHelper.GetCustCreditLine(ctx, custNo, "F_HS_CreditLineUSD");
                decimal resiCustCredit = LogHelper.GetCustCreditLine(ctx, custNo, "F_HS_SurplusCreditUSD");
                decimal rateToUSA = GetExchangeRate(ctx, currNo);

                if (coll != null && coll.Count > 0)
                {
                    entrys = new List<ReceiveBillEntry>();

                    foreach (var item in coll)
                    {
                        if (item != null)
                        {
                            entry = new ReceiveBillEntry();

                            entry.FEntryID = Convert.ToInt32(SQLUtils.GetFieldValue(item, "Id"));

                            entry.FREALRECAMOUNTFOR_D = Math.Round(Convert.ToDecimal(SQLUtils.GetFieldValue(item, "REALRECAMOUNTFOR")), 2);

                            DynamicObject stype = item["SETTLETYPEID"] as DynamicObject;
                            entry.FSETTLETYPEID = SQLUtils.GetFieldValue(stype, "Number");

                            entry.F_HS_YNRecharge = Convert.ToBoolean(SQLUtils.GetFieldValue(item, "F_HS_YNRecharge"));
                            entry.F_HS_SynchronizedRecharge = Convert.ToBoolean(SQLUtils.GetFieldValue(obj, "F_HS_SynchronizedRecharge"));

                            decimal receivedTurnUSD = Math.Round((entry.FREALRECAMOUNTFOR_D / rateToUSA), 2);


                            if (custCredit > 0)
                            {
                                if (receivedTurnUSD >= (custCredit - resiCustCredit))
                                {
                                    entry.F_HS_CreditLineRechargeUSD = custCredit - resiCustCredit;
                                    entry.F_HS_BalanceRechargeUSD = receivedTurnUSD - entry.F_HS_CreditLineRechargeUSD;
                                }
                                else
                                {
                                    entry.F_HS_CreditLineRechargeUSD = receivedTurnUSD;
                                    entry.F_HS_BalanceRechargeUSD = 0;
                                }
                            }
                            else if (custCredit == 0)
                            {
                                entry.F_HS_CreditLineRechargeUSD = 0;
                                entry.F_HS_BalanceRechargeUSD = receivedTurnUSD;
                            }

                            entrys.Add(entry);
                        }
                    }
                }
            }

            return entrys;
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
                            AbsDataInfo receive = data as AbsDataInfo;

                            if (receive != null)
                            {
                                decimal balance = LogHelper.GetCustBalance(ctx, receive.F_HS_B2CCustId, receive.FSaleOrgId);

                                if (receive.F_HS_RateToUSA > 0)
                                {

                                    string billNo = receive.FBillNo.Contains("_") ? receive.FBillNo.Split('_')[0] : receive.FBillNo;


                                    sql += string.Format(@"/*dialect*/ update a set a.F_HS_SynchronizedRecharge ='{0}',a.F_HS_ReceivedTurnUSD = {1}
                                                            ,a.F_HS_CreditLineRechargeUSD = '{7}',a.F_HS_BalanceRechargeUSD = '{8}'
                                                            from T_AR_RECEIVEBILLENTRY a
                                                            inner join T_AR_RECEIVEBILL b on a.FID = b.FID
                                                            inner join T_BD_CUSTOMER c on b.F_HS_B2CCustId = c.FCUSTID
                                                            inner join T_ORG_ORGANIZATIONS d on b.FSALEORGID = d.FORGID
                                                            where a.F_HS_YNRecharge = '{2}'
														    and c.FNumber = '{3}'
                                                            and b.FBillNo = '{4}'
                                                            and d.FNUMBER = '{5}'
														    and a.FENTRYID = {6}", "1", receive.FRealAmountFor_USD, "1", receive.F_HS_B2CCustId
                                                            , billNo, receive.FSaleOrgId, receive.FEntryId, receive.F_HS_CreditLineRechargeUSD, receive.F_HS_BalanceRechargeUSD) + System.Environment.NewLine;
                                    sql += string.Format(@"/*dialect*/ update a set a.F_HS_RateToUSA ={0}
                                                                from T_AR_RECEIVEBILL a
                                                                inner join T_AR_RECEIVEBILLENTRY b on a.FID = b.FID
                                                                inner join T_BD_CUSTOMER c on a.F_HS_B2CCustId = c.FCUSTID
                                                                inner join T_ORG_ORGANIZATIONS d on a.FSALEORGID = d.FORGID
                                                                where b.F_HS_YNRecharge = '{1}'and c.FNumber = '{2}'
                                                                and a.FBillNo = '{3}'
                                                                and d.FNUMBER = '{4}'", receive.F_HS_RateToUSA, "1", receive.F_HS_B2CCustId, billNo, receive.FSaleOrgId) + System.Environment.NewLine;
                                    sql += string.Format(@"/*dialect*/  update a set a.F_HS_USDBalance = a.F_HS_USDBalance + {0}{3}{4}
                                                            from T_BD_CUSTOMER a
                                                            left join T_AR_RECEIVEBILL b on a.FCUSTID = b.F_HS_B2CCustId
                                                            left join T_AR_RECEIVEBILLENTRY c on b.FID = c.FID
                                                            inner join T_ORG_ORGANIZATIONS d on a.FUSEORGID = d.FORGID
                                                            where a.FNUMBER = '{1}'
                                                            and d.FNUMBER = '{2}'", receive.F_HS_BalanceRechargeUSD, receive.F_HS_B2CCustId, receive.FSaleOrgId
                                                            , receive.FSaleOrgId.CompareTo("100.03") == 0 && receive.FSettleCurrId.CompareTo("CNY") == 0 ? string.Format(@",a.F_HS_CNYBalance = a.F_HS_CNYBalance + {0}", receive.FRealAmountFor) : ""
                                                            , string.Format(@",a.F_HS_SurplusCreditUSD = a.F_HS_SurplusCreditUSD + {0}", receive.F_HS_CreditLineRechargeUSD)) + Environment.NewLine;

                                    sql = "'" + SQLUtils.DealQuotes(sql) + "'";
                                    execSql += string.Format(@"EXEC(" + sql + ")") + System.Environment.NewLine;
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

                                                            ", receive.F_HS_B2CCustId, receive.FSaleOrgId, i, i, i, i, receive.F_HS_B2CCustId
                                                             , receive.FSaleOrgId, i, receive.F_HS_B2CCustId, receive.FSaleOrgId, i) + System.Environment.NewLine;

                                    if (receive.F_HS_BalanceRechargeUSD > 0)
                                    {
                                        execSql += string.Format(@"/*dialect*/ insert into HS_T_customerBalance(F_HS_TradeType,F_HS_UseOrgId,F_HS_B2CCUSTID,changedAmount,changedType,changedCause
                                                               ,balanceAmount,F_HS_RateToUSA,FSETTLECURRID,changedAmountUSA,balanceAmountUSA,F_HS_CNYBalance,updateTime
                                                               ,updateUser,FBillNo,fentryID,needfreezed,remark) values ('{18}','{15}','{0}',{1},'{2}','{3}',@balance{13}*{17},{4},{5}
                                                               ,{6},@balance{14}, @cnybalance{16},'{7}',{8},'{9}',{10},{11},'{12}') ", SQLUtils.GetCustomerId(ctx, receive.F_HS_B2CCustId, 1), receive.F_HS_BalanceRechargeUSD / receive.F_HS_RateToUSA
                                                               , receive.ChangedType, receive.ChangedCause, receive.F_HS_RateToUSA, SQLUtils.GetSettleCurrId(ctx, receive.FSettleCurrId)
                                                               , receive.F_HS_BalanceRechargeUSD, DateTime.Now, ctx.UserId, billNo, receive.FEntryId, receive.NeedFreezed == false ? 0 : 1, receive.Remark, i, i
                                                               , SQLUtils.GetOrgId(ctx, receive.FSaleOrgId), i, receive.F_HS_RateToUSA, "余额") + System.Environment.NewLine;
                                    }

                                    if (receive.F_HS_CreditLineRechargeUSD > 0)
                                    {
                                        execSql += string.Format(@"/*dialect*/ insert into HS_T_customerBalance(F_HS_TradeType,F_HS_UseOrgId,F_HS_B2CCUSTID,changedAmount,changedType,changedCause
                                                               ,balanceAmount,F_HS_RateToUSA,FSETTLECURRID,changedAmountUSA,balanceAmountUSA,F_HS_CNYBalance,updateTime
                                                               ,updateUser,FBillNo,fentryID,needfreezed,remark) values ('{18}','{15}','{0}',{1},'{2}','{3}',@creditbalance{13}*{17},{4},{5}
                                                               ,{6},@creditbalance{14}, @cnybalance{16},'{7}',{8},'{9}',{10},{11},'{12}') ", SQLUtils.GetCustomerId(ctx, receive.F_HS_B2CCustId, 1), receive.F_HS_CreditLineRechargeUSD
                                                              , receive.ChangedType, receive.ChangedCause, receive.F_HS_RateToUSA, SQLUtils.GetSettleCurrId(ctx, receive.FSettleCurrId)
                                                              , receive.F_HS_CreditLineRechargeUSD / receive.F_HS_RateToUSA, DateTime.Now, ctx.UserId, billNo, receive.FEntryId, receive.NeedFreezed == false ? 0 : 1, receive.Remark, i, i
                                                              , SQLUtils.GetOrgId(ctx, receive.FSaleOrgId), i, receive.F_HS_RateToUSA, "剩余信用额度") + System.Environment.NewLine;
                                    }

                                    sql = string.Empty;
                                    i++;

                                }

                            }
                            else
                            {
                                LogUtils.WriteSynchroLog(ctx, this.DataType, "获取【" + receive.FSettleCurrId + "币别】兑美元的实时汇率失败");
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
