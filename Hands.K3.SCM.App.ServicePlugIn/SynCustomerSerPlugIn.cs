using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Hands.K3.SCM.APP.Entity.EnumType;
using Hands.K3.SCM.APP.Entity.K3WebApi;
using Hands.K3.SCM.APP.Entity.SynDataObject;
using Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject;
using Hands.K3.SCM.APP.Utils.Utils;
using HS.K3.Common.Abbott;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;

namespace Hands.K3.SCM.App.ServicePlugIn
{
    [Kingdee.BOS.Util.HotUpdate]
    [Description("同步客户信息至HC网站的服务插件")]
    public class SynCustomerSerPlugIn : AbstractOSPlugIn
    {
        public override SynchroDataType DataType
        {
            get
            {
                return SynchroDataType.Customer;
            }
        }

        public override SynchroDirection Direction
        {
            get
            {
                return SynchroDirection.ToHC;
            }
        }

        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            e.FieldKeys.Add("F_HS_OnlineDiscount");
            e.FieldKeys.Add("F_HS_TTDiscount");
            e.FieldKeys.Add("F_HS_FixedFreightDiscount");
            e.FieldKeys.Add("F_HS_IntegralReturnRate");
            e.FieldKeys.Add("F_HS_DiscountChangeRemark");
            e.FieldKeys.Add("F_HS_OldCreditLineUSD");
            e.FieldKeys.Add("F_HS_CreditLineUSD");
            e.FieldKeys.Add("F_HS_OldDiscountChangeRemark");
            e.FieldKeys.Add("F_HS_SurplusCreditUSD");
        }

        public override void OnAddValidators(AddValidatorsEventArgs e)
        {
            base.OnAddValidators(e);

            if (e.DataEntities == null || e.DataEntities.Count() == 0)
            {
                return;
            }

            List<DynamicObject> objs = e.DataEntities.ToList();
            HttpResponseResult result = OperateAfterAudit(this.Context, objs);

            AuditValidator validator = new AuditValidator(result, this.BusinessInfo.GetForm().Id);
            validator.EntityKey = "FBillHead";
            e.Validators.Add(validator);
        }

        /// <summary>
        /// 5、客户审核时：
        ///5.1  校验信用额度的调整：若客户.原信用额度 - 客户.信用额度>客户.剩余信用额度，校验不通过，审核失败
        ///5.2  若客户.信用额度<> 客户.原信用额度，更新客户.剩余信用额度:
        ///客户.剩余信用额度 +=客户.信用额度 - 客户.原信用额度
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="objs"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public override IEnumerable<AbsSynchroDataInfo> GetK3Datas(Context ctx, List<DynamicObject> objs, ref HttpResponseResult result)
        {
            List<K3CustomerInfo> custs = null;
            K3CustomerInfo cust = null;
            decimal oldCreditLineUSD = 0;
            decimal creditLineUSD = 0;
            decimal surplusCreditUSD = 0;

            result = new HttpResponseResult();
            result.Success = true;

            if (objs != null && objs.Count > 0)
            {
                custs = new List<K3CustomerInfo>();

                foreach (var item in objs)
                {
                    if (item != null)
                    {
                        oldCreditLineUSD = Convert.ToDecimal(SQLUtils.GetFieldValue(item, "F_HS_OldCreditLineUSD"));
                        creditLineUSD = Convert.ToDecimal(SQLUtils.GetFieldValue(item, "F_HS_CreditLineUSD"));
                        surplusCreditUSD = Convert.ToDecimal(SQLUtils.GetFieldValue(item, "F_HS_SurplusCreditUSD"));

                        if (oldCreditLineUSD - creditLineUSD <= surplusCreditUSD)
                        {
                            DynamicObject useOrgId = item["UseOrgId"] as DynamicObject;
                            string useOrgNo = SQLUtils.GetFieldValue(useOrgId, "Number");

                            if (useOrgNo.CompareTo("100.01") == 0)
                            {
                                cust = new K3CustomerInfo();

                                cust.FUseOrgId = useOrgNo;
                                cust.SrcNo = SQLUtils.GetFieldValue(item, "Number");
                                cust.FNumber = cust.SrcNo;

                                cust.FDocumentStatus = SQLUtils.GetFieldValue(item, "DocumentStatus");
                                cust.F_HS_IntegralReturnRate = Convert.ToDecimal(SQLUtils.GetFieldValue(item, "F_HS_IntegralReturnRate"));
                                cust.F_HS_FixedFreightDiscount = Convert.ToDecimal(SQLUtils.GetFieldValue(item, "F_HS_FixedFreightDiscount"));
                                cust.F_HS_IntegralReturnRate = Convert.ToDecimal(SQLUtils.GetFieldValue(item, "F_HS_IntegralReturnRate"));
                                cust.F_HS_OnlineDiscount = Convert.ToDecimal(SQLUtils.GetFieldValue(item, "F_HS_OnlineDiscount"));
                                cust.F_HS_TTDiscount = Convert.ToDecimal(SQLUtils.GetFieldValue(item, "F_HS_TTDiscount"));
                                cust.F_HS_DiscountChangeRemark = SQLUtils.GetFieldValue(item, "F_HS_DiscountChangeRemark");
                                cust.F_HS_OldOnlineDiscount = Convert.ToDecimal(SQLUtils.GetFieldValue(item, "F_HS_OldOnlineDiscount"));
                                cust.F_HS_OldTTDISCOUNT = Convert.ToDecimal(SQLUtils.GetFieldValue(item, "F_HS_OldTTDISCOUNT"));
                                cust.F_HS_OldFixedFreightDiscount = Convert.ToDecimal(SQLUtils.GetFieldValue(item, "F_HS_OldFixedFreightDiscount"));
                                cust.F_HS_OldIntegralReturnRate = Convert.ToDecimal(SQLUtils.GetFieldValue(item, "F_HS_OldIntegralReturnRate"));
                                cust.F_HS_OldCreditLineUSD = Convert.ToDecimal(SQLUtils.GetFieldValue(item, "F_HS_OldCreditLineUSD"));
                                cust.F_HS_CreditLineUSD = Convert.ToDecimal(SQLUtils.GetFieldValue(item, "F_HS_CreditLineUSD"));
                                cust.F_HS_OldDiscountChangeRemark = SQLUtils.GetFieldValue(item, "F_HS_OldDiscountChangeRemark");
                                cust.F_HS_SurplusCreditUSD = surplusCreditUSD;

                                custs.Add(cust);
                            }
                        }
                        else
                        {
                            result = new HttpResponseResult();
                            result.Success = false;
                            result.Message = "客户原信用额度不能大于客户信用额度！" + Environment.NewLine;
                        }

                    }
                }
            }

            return custs.Where(c =>
            {
                return c.FUseOrgId.CompareTo("100.01") == 0
                        && (c.F_HS_FixedFreightDiscount > 0
                        || c.F_HS_IntegralReturnRate > 0
                        || c.F_HS_OnlineDiscount > 0
                        || c.F_HS_TTDiscount > 0
                        );
            }).ToList<AbsSynchroDataInfo>(); ;
        }

        /// <summary>
        /// 1、仅当固定产品折扣率<>原固定产品折扣率 or 固定TT折扣率<>原固定TT折扣率 or 固定运费折扣率<>原固定运费折扣率 
        /// or 积分返点率<>原积分返点率 or 信用额度<>原信用额度 时写日志
        /// 2、写日志时，仅记录以上有变化的的字段，格式如： 
        ///“客户【XXX】，  固定产品折扣率由【XX】变更为【XX】，变更原因：【XXXXXX】 ”
        /// 2、当客户审核成功后将当前固定产品折扣率、固定TT折扣率、固定运费折扣率、积分返点率、信用额度、
        /// 固定_率_信用额度变更原因分别写入到客户.原固定产品折扣率、客户.原固定TT折扣率、客户.原固定运费折扣率、
        /// 客户.原积分返点率、原信用额度、客户.原固定_率_信用额度变更原因。注意以上字段的反写，仅反写当前组织
        /// 5.2  若客户.信用额度<>客户.原信用额度，更新客户.剩余信用额度:
        ///客户.剩余信用额度 +=客户.信用额度 - 客户.原信用额度
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="datas"></param>
        /// <returns></returns>
        public override string GetExecuteUpdateSql(Context ctx, List<AbsSynchroDataInfo> datas)
        {
            List<K3CustomerInfo> custs = null;
            string sql = string.Empty;
            string msg = string.Empty;
            decimal rateToUSA = GetExchangeRate(ctx, "USD");

            if (datas != null && datas.Count() > 0)
            {
                custs = datas.Select(c => (K3CustomerInfo)c).ToList();

                if (custs != null && custs.Count > 0)
                {
                    foreach (var cust in custs)
                    {
                        if (cust != null)
                        {
                            if (cust.F_HS_CreditLineUSD != cust.F_HS_OldCreditLineUSD)
                            {
                                decimal chaAmount = cust.F_HS_CreditLineUSD - cust.F_HS_OldCreditLineUSD;

                                sql += string.Format(@"/*dialect*/ update a SET a.F_HS_SurplusCreditUSD = a.F_HS_SurplusCreditUSD + {0}
			                                            from T_BD_CUSTOMER a
			                                            inner join T_ORG_ORGANIZATIONS b on a.FUseOrgId = b.FORGID
                                                        where a.FNUMBER = '{1}'
			                                            and b.FNUMBER = '{2}'", cust.F_HS_CreditLineUSD - cust.F_HS_OldCreditLineUSD, cust.FNumber, cust.FUseOrgId);

                                sql += string.Format(@"/*dialect*/ insert into HS_T_customerBalance(F_HS_TradeType,F_HS_UseOrgId,F_HS_B2CCUSTID,changedAmount,changedType,changedCause
                                                                    ,balanceAmount,F_HS_RateToUSA,FSETTLECURRID,changedAmountUSA,balanceAmountUSA,F_HS_CNYBalance,updateTime
                                                                    ,updateUser,FBillNo,fentryID,needfreezed,remark) values ('{0}',{1},{2},{3},'{4}','{5}',{6}*{7},{8},{9}
                                                                    ,{10},{11}, {12},'{13}',{14},'{15}',{16},{17},'{18}') "
                                                                    , "剩余信用额度", SQLUtils.GetOrgId(ctx, cust.FUseOrgId), SQLUtils.GetCustomerId(ctx, cust.FNumber, 1)
                                                                    , chaAmount, "KHXYEDTZ", "DJSH", cust.F_HS_SurplusCreditUSD + chaAmount, rateToUSA, rateToUSA, SQLUtils.GetSettleCurrId(ctx, "USD")
                                                                    , chaAmount, cust.F_HS_SurplusCreditUSD + chaAmount, 0, DateTime.Now, ctx.UserId, cust.FNumber, 0, true ? 0 : 1, ""
                                                                    ) + Environment.NewLine;
                            }
                            sql += string.Format(@"/*dialect*/ update a SET a.F_HS_OldOnlineDiscount = {0},a.F_HS_OldTTDISCOUNT = {1}
                                                    ,a.F_HS_OldFixedFreightDiscount = {2},a.F_HS_OldIntegralReturnRate = {3}
                                                    ,a.F_HS_OldDiscountChangeRemark = '{4}',a.F_HS_DiscountChangeRemark = '{5}'
                                                    ,a.F_HS_OldCreditLineUSD = {8}
			                                        from T_BD_CUSTOMER a
			                                        inner join T_ORG_ORGANIZATIONS b on a.FUseOrgId = b.FORGID
                                                    where a.FNUMBER = '{6}'
			                                        and b.FNUMBER = '{7}'", cust.F_HS_OnlineDiscount, cust.F_HS_TTDiscount, cust.F_HS_FixedFreightDiscount
                                                    , cust.F_HS_IntegralReturnRate, cust.F_HS_DiscountChangeRemark, string.Empty, cust.FNumber, cust.FUseOrgId, cust.F_HS_CreditLineUSD) + Environment.NewLine;


                            if (cust.F_HS_FixedFreightDiscount != cust.F_HS_OldFixedFreightDiscount)
                            {
                                msg = string.Format("客户编码【{0}】:固定运费折扣率由{1}变更为{2},变更原因：{3}", cust.FNumber, cust.F_HS_OldFixedFreightDiscount, cust.F_HS_FixedFreightDiscount, cust.F_HS_DiscountChangeRemark);
                            }
                            if (cust.F_HS_IntegralReturnRate != cust.F_HS_OldIntegralReturnRate)
                            {
                                msg = string.Format("客户编码【{0}】:固定积分返点率由{1}变更为{2},变更原因：{3}", cust.FNumber, cust.F_HS_OldIntegralReturnRate, cust.F_HS_IntegralReturnRate, cust.F_HS_DiscountChangeRemark);
                            }
                            if (cust.F_HS_TTDiscount != cust.F_HS_OldTTDISCOUNT)
                            {
                                msg = string.Format("客户编码【{0}】:固定TT折扣率由{1}变更为{2},变更原因：{3}", cust.FNumber, cust.F_HS_OldTTDISCOUNT, cust.F_HS_TTDiscount, cust.F_HS_DiscountChangeRemark);
                            }
                            if (cust.F_HS_OnlineDiscount != cust.F_HS_OldOnlineDiscount)
                            {
                                msg = string.Format("客户编码【{0}】:固定产品折扣率由{1}变更为{2},变更原因：{3}", cust.FNumber, cust.F_HS_OldOnlineDiscount, cust.F_HS_OnlineDiscount, cust.F_HS_DiscountChangeRemark);
                            }
                            if (cust.F_HS_CreditLineUSD != cust.F_HS_OldCreditLineUSD)
                            {
                                msg = string.Format("客户编码【{0}】:信用额度USD由{1}变更为{2},变更原因：{3}", cust.FNumber, cust.F_HS_OldCreditLineUSD, cust.F_HS_CreditLineUSD, cust.F_HS_DiscountChangeRemark);
                            }

                            sql += string.Format(@"/*dialect*/ Insert Into HS_T_SynchroLog_Succ(FDataSourceType,FDataSourceId,FBILLNO,
                                                        FSynchroTime,FIsSuccess,FErrInfor,FDataSourceTypeDesc,FHSOperateId) values( '{0}','{1}','{2}','{3}','{4}','{5}','{6}',{7} )",
                                                    this.DataType.ToString(), "", cust.FNumber, DateTime.Now, true, msg, "客户", ctx.UserId);

                        }
                        
                    }

                }
            }

            return sql;
        }
    }
}
