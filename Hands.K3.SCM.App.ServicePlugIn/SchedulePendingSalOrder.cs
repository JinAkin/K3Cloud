using System;
using System.Collections.Generic;
using Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject;
using Hands.K3.SCM.APP.Utils.Utils;
using HS.K3.Common.Abbott;
using Kingdee.BOS;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core;
using Kingdee.BOS.Orm.DataEntity;

namespace Hands.K3.SCM.App.ServicePlugIn
{
    class SchedulePendingSalOrder : IScheduleService
    {
        public void Run(Context ctx, Schedule schedule)
        {
            if (schedule != null)
            {
                SynK3Data2Website(ctx);
            }
        }

        private DynamicObjectCollection GetDynamicObjects(Context ctx)
        {
            string sql = string.Format(@"/*dialect*/ select distinct FBillNo,FDate,e.FNUMBER as F_HS_B2CCUSTID,a.F_HS_BalancePayments,a.F_HS_USDBalancePayments
                                                    ,o.FNUMBER as FSettleCurrId,q.FNUMBER as F_HS_PaymentModeNew,a.F_HS_BalanceDeducted
                                                    ,a.F_HS_RateToUSA,d.FBillAmount,z.FNUMBER as UseOrgId,g.FNUMBER as F_HS_SaleOrderSource
                                                    ,a.FCancelStatus,a.FCancelDate
                                                    from T_SAL_ORDER a 
                                                    inner join T_SAL_ORDERENTRY b on b.FID = a.FID
                                                    inner join T_SAL_ORDERENTRY_F c on c.FENTRYID = b.FENTRYID and c.FID = b.FID
                                                    inner join T_SAL_ORDERFIN d on d.FID = a.FID
                                                    inner join T_BD_CUSTOMER e on e.FCUSTID= a.F_HS_B2CCustId
                                                    inner join T_BAS_ASSISTANTDATAENTRY_L f ON a.F_HS_SaleOrderSource=f.FENTRYID
                                                    inner join T_BAS_ASSISTANTDATAENTRY g ON f.FentryID=g.FentryID
                                                    left join T_BAS_BILLTYPE h on a.FBILLTypeID=h.FBILLTypeID
													inner join T_ORG_ORGANIZATIONS z on a.FSALEORGID=z.FORGID
													inner join T_BD_CUSTOMER j on j.FCUSTID = a.F_HS_B2CCUSTID
													inner join T_BD_CURRENCY o on o.FCURRENCYID = d.FSettleCurrId
													inner join T_BAS_ASSISTANTDATAENTRY_L p ON a.F_HS_PaymentModeNew=p.FENTRYID
                                                    inner join T_BAS_ASSISTANTDATAENTRY q ON q.FentryID=p.FentryID
                                                    where g.FNUMBER = 'HCWebPendingOder'
													and a.FCANCELSTATUS = 'B'
													and a.F_HS_USDBALANCEPAYMENTS > 0
													and a.F_HS_BalanceDeducted <>'1'
													and a.FDATE > '2018-11-01'				");
            return SQLUtils.GetObjects(ctx,sql);
        }
        private IEnumerable<AbsSynchroDataInfo> GetK3Datas(Context ctx,DynamicObjectCollection coll)
        {
            AbsDataInfo data = null;
            List<AbsDataInfo> datas = null;

            if (coll != null && coll.Count > 0)
            {
                datas = new List<AbsDataInfo>();

                foreach (var item in coll)
                {
                    if (item != null)
                    {
                        data = new AbsDataInfo(SynchroDataType.SaleOrder);
                        data.SrcNo = SQLUtils.GetFieldValue(item, "FBillNo");
                        data.FBillNo = data.SrcNo;
                        data.FDate = TimeHelper.GetTimeStamp(Convert.ToDateTime(SQLUtils.GetFieldValue(item, "FDate")));
                        data.F_HS_B2CCustId = SQLUtils.GetFieldValue(item, "F_HS_B2CCUSTID");
                        data.F_HS_BalanceRechargeUSD = Convert.ToDecimal(SQLUtils.GetFieldValue(item, "F_HS_USDBalancePayments"));
              
                        data.FSettleCurrId = SQLUtils.GetFieldValue(item, "FSettleCurrId");
                        data.FSettleTypeId = SQLUtils.GetFieldValue(item, "F_HS_PaymentModeNew");
                        data.F_HS_RateToUSA = Convert.ToDecimal(SQLUtils.GetFieldValue(item, "F_HS_RateToUSA"));
                        data.FSaleOrgId = SQLUtils.GetFieldValue(item, "UseOrgId");
                        data.F_HS_SaleOrderSource = SQLUtils.GetFieldValue(item, "F_HS_SaleOrderSource");
                        data.FCancelStatus = SQLUtils.GetFieldValue(item, "FCancelStatus");
                        data.FBusinessTime = SQLUtils.GetFieldValue(item, "FCancelDate");
                        data.F_HS_BalanceDeducted = SQLUtils.GetFieldValue(item, "F_HS_BalanceDeducted").Equals("1") ? true : false; 
                        datas.Add(data);
                    }
                }
            }
            return datas;
        }

        private void SynK3Data2Website(Context ctx)
        {
            DynamicObjectCollection coll = GetDynamicObjects(ctx);
            IEnumerable<AbsSynchroDataInfo> datas = GetK3Datas(ctx,coll);

            if (coll != null && coll.Count > 0)
            {
                SalOrderBalanceSerPlugIn sal = new SalOrderBalanceSerPlugIn();
                sal.OperateAfterAudit(ctx,null,datas,SynchroDataType.SaleOrder);
            }
           
        }
    }
}
