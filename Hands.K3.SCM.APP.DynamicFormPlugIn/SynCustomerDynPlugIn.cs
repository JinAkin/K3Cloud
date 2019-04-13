using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Hands.K3.SCM.APP.Entity.EnumType;
using Hands.K3.SCM.APP.Entity.SynDataObject;
using Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject;
using Hands.K3.SCM.APP.Utils.Utils;
using HS.K3.Common.Abbott;
using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.FormElement;
using Kingdee.BOS.Orm.DataEntity;

namespace Hands.K3.SCM.APP.DynamicFormPlugIn
{
    [Description("客户同步至HC网站表单插件")]
    public class SynCustomerDynPlugIn : AbstractDynPlugIn
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
        public override IEnumerable<AbsSynchroDataInfo> GetK3Datas(Context ctx, FormOperation oper = null)
        {
            List<K3CustomerInfo> custs = null;
            K3CustomerInfo cust = null;

            if (SelectedNos != null && SelectedNos.Count() > 0)
            {
                string sql = string.Format(@"/*dialect*/ select b.FNUMBER as FUseOrgId,a.FNUMBER,a.F_HS_FixedFreightDiscount,a.F_HS_IntegralReturnRate ,a.F_HS_OnlineDiscount,a.F_HS_TTDiscount
                                                ,a.F_HS_DiscountChangeRemark,a.F_HS_OldOnlineDiscount,a.F_HS_OldTTDISCOUNT,a.F_HS_OldFixedFreightDiscount
                                                ,a.F_HS_OldIntegralReturnRate
                                                from T_BD_CUSTOMER a
                                                inner join T_ORG_ORGANIZATIONS b on a.FUSEORGID = b.FORGID
                                                where a.FNUMBER in ('{0}')
                                                and b.FNUMBER = '{1}'", string.Join("','",SelectedNos),"100.01");

                DynamicObjectCollection coll = SQLUtils.GetObjects(ctx, sql);

                if (coll != null && coll.Count > 0)
                {
                    custs = new List<K3CustomerInfo>();

                    foreach (var item in coll)
                    {
                        if (item != null)
                        {
                            cust = new K3CustomerInfo();

                            cust.FUseOrgId = SQLUtils.GetFieldValue(item, "FUseOrgId");
                            cust.FNumber = SQLUtils.GetFieldValue(item, "FNUMBER");
                            cust.SrcNo = cust.FNumber;
                            cust.F_HS_FixedFreightDiscount = Convert.ToDecimal(SQLUtils.GetFieldValue(item, "F_HS_FixedFreightDiscount"));
                            cust.F_HS_IntegralReturnRate = Convert.ToDecimal(SQLUtils.GetFieldValue(item, "F_HS_IntegralReturnRate"));
                            cust.F_HS_OnlineDiscount = Convert.ToDecimal(SQLUtils.GetFieldValue(item, "F_HS_OnlineDiscount"));
                            cust.F_HS_TTDiscount = Convert.ToDecimal(SQLUtils.GetFieldValue(item, "F_HS_TTDiscount"));
                            cust.F_HS_DiscountChangeRemark = SQLUtils.GetFieldValue(item, "F_HS_DiscountChangeRemark");

                            cust.F_HS_OldOnlineDiscount = Convert.ToDecimal(SQLUtils.GetFieldValue(item, "F_HS_OldOnlineDiscount"));
                            cust.F_HS_OldTTDISCOUNT = Convert.ToDecimal(SQLUtils.GetFieldValue(item, "F_HS_OldTTDISCOUNT"));

                            cust.F_HS_OldFixedFreightDiscount = Convert.ToDecimal(SQLUtils.GetFieldValue(item, "F_HS_OldFixedFreightDiscount"));
                            cust.F_HS_OldIntegralReturnRate = Convert.ToDecimal(SQLUtils.GetFieldValue(item, "F_HS_OldIntegralReturnRate"));

                            custs.Add(cust);
                        }
                    }
                }
                
            }

            return custs;
        }

        public IEnumerable<AbsSynchroDataInfo> NeedSynchro2HC(IEnumerable<AbsSynchroDataInfo> datas)
        {
            if (datas != null && datas.Count() > 0)
            {
                List<K3CustomerInfo> custs = datas.Select(c => (K3CustomerInfo)c).ToList();
                List<AbsSynchroDataInfo> synDatas = null;
                if (custs != null && custs.Count > 0)
                {
                    synDatas = custs.Where(c =>
                    {
                        return c.FUseOrgId.CompareTo("100.01") == 0
                                && (c.F_HS_FixedFreightDiscount > 0
                                || c.F_HS_IntegralReturnRate > 0
                                || c.F_HS_OnlineDiscount > 0
                                || c.F_HS_TTDiscount > 0
                                );
                    }).ToList<AbsSynchroDataInfo>();
                }


                return synDatas;
            }

            return null;
        }
        private int UpdateDiscountRate(Context ctx, IEnumerable<AbsSynchroDataInfo> datas)
        {
            List<K3CustomerInfo> custs = null;
            int count = 0;

            if (datas != null && datas.Count() > 0)
            {
                custs = datas.Select(c => (K3CustomerInfo)c).ToList();

                if (custs != null && custs.Count > 0)
                {
                    foreach (var cust in custs)
                    {
                        if (cust != null)
                        {
                            try
                            {
                                string sql = string.Format(@"/*dialect*/ update a SET a.F_HS_OldOnlineDiscount = {0},a.F_HS_OldTTDISCOUNT = {1},a.F_HS_OldFixedFreightDiscount = {2},a.F_HS_OldIntegralReturnRate = {3},a.F_HS_OldDiscountChangeRemark = '{4}'
			                                                                from T_BD_CUSTOMER a
			                                                                inner join T_ORG_ORGANIZATIONS b on a.FUseOrgId = b.FORGID
                                                                            where a.FNUMBER = '{5}'
			                                                                and b.FNUMBER = '{6}'", cust.F_HS_OnlineDiscount, cust.F_HS_TTDiscount, cust.F_HS_FixedFreightDiscount, cust.F_HS_IntegralReturnRate, cust.F_HS_DiscountChangeRemark, cust.FNumber, cust.FUseOrgId);
                                count += DBUtils.Execute(ctx, sql);

                            }
                            catch (Exception ex)
                            {

                                LogUtils.WriteSynchroLog(ctx, this.DataType, "客户折扣率更新出现异常：" + ex.Message + Environment.NewLine + ex.StackTrace);
                            }

                        }
                    }
                }
            }

            return count;
        }

        private void WriteDiscountRateLog(Context ctx, IEnumerable<AbsSynchroDataInfo> datas)
        {
            if (datas != null && datas.Count() > 0)
            {
                foreach (var data in datas)
                {
                    if (data != null)
                    {
                        K3CustomerInfo cust = data as K3CustomerInfo;

                        if (cust.F_HS_FixedFreightDiscount != cust.F_HS_OldFixedFreightDiscount)
                        {
                            LogHelper.WriteSynchroLog_Succ(ctx, this.DataType, string.Format("客户编码【{0}】:固定运费折扣率由{1}变更为{2},变更原因：{3}", cust.FNumber, cust.F_HS_OldFixedFreightDiscount, cust.F_HS_FixedFreightDiscount, cust.F_HS_DiscountChangeRemark));
                        }
                        if (cust.F_HS_IntegralReturnRate != cust.F_HS_OldIntegralReturnRate)
                        {
                            LogHelper.WriteSynchroLog_Succ(ctx, this.DataType, string.Format("客户编码【{0}】:固定积分返点率由{1}变更为{2},变更原因：{3}", cust.FNumber, cust.F_HS_OldIntegralReturnRate, cust.F_HS_IntegralReturnRate, cust.F_HS_DiscountChangeRemark));
                        }
                        if (cust.F_HS_TTDiscount != cust.F_HS_OldTTDISCOUNT)
                        {
                            LogHelper.WriteSynchroLog_Succ(ctx, this.DataType, string.Format("客户编码【{0}】:固定TT折扣率由{1}变更为{2},变更原因：{3}", cust.FNumber, cust.F_HS_OldTTDISCOUNT, cust.F_HS_TTDiscount, cust.F_HS_DiscountChangeRemark));
                        }
                        if (cust.F_HS_OnlineDiscount != cust.F_HS_OldOnlineDiscount)
                        {
                            LogHelper.WriteSynchroLog_Succ(ctx, this.DataType, string.Format("客户编码【{0}】:固定产品折扣率由{1}变更为{2},变更原因：{3}", cust.FNumber, cust.F_HS_OldOnlineDiscount, cust.F_HS_OnlineDiscount, cust.F_HS_DiscountChangeRemark));
                        }
                    }

                }
            }
        }

        public override void AfterDoOperation(AfterDoOperationEventArgs e)
        {
            base.AfterDoOperation(e);

            if (e.Operation.Operation.CompareTo(SynOperationType.AUDIT.ToString()) == 0 && e.ExecuteResult)
            {
                IEnumerable<AbsSynchroDataInfo> custs = GetK3Datas(this.Context);
                IEnumerable<AbsSynchroDataInfo> synCusts = NeedSynchro2HC(custs);
                SynK3Datas2HC(this.Context, synCusts);
                UpdateDiscountRate(this.Context,custs);
            }
        }
    }
}
