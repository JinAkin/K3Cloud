using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Hands.K3.SCM.App.Synchro.Utils.SynchroService;
using Hands.K3.SCM.APP.Entity.EnumType;
using Hands.K3.SCM.APP.Entity.K3WebApi;
using Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject;
using Hands.K3.SCM.APP.Utils.Utils;
using HS.K3.Common.Abbott;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;

namespace Hands.K3.SCM.App.ServicePlugIn
{
    [Description("收款退款单--生成收款单")]
    public class SynReceiveBillByReFundBillSerPlugIn : AbstractOSPlugIn
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
            e.FieldKeys.Add("FSETTLECUR");
            e.FieldKeys.Add("F_HS_RateToUSA");
            e.FieldKeys.Add("FREALREFUNDAMOUNTFOR");
            e.FieldKeys.Add("FSETTLETYPEID");
            e.FieldKeys.Add("FDATE");
            e.FieldKeys.Add("F_HS_RefundMethod");
            e.FieldKeys.Add("F_HS_BalanceReceivableNo");
            e.FieldKeys.Add("FDOCUMENTSTATUS");
            e.FieldKeys.Add("FDATE");
            e.FieldKeys.Add("FBILLNo");
            e.FieldKeys.Add("FSETTLEORGID");
            e.FieldKeys.Add("FSALEDEPTID");
            e.FieldKeys.Add("FREFUNDBILLENTRY");
            e.FieldKeys.Add("FSALEERID");
            e.FieldKeys.Add("FSETTLECUR");
            e.FieldKeys.Add("FREFUNDBILLENTRY");
        }
        public string GetSettleType(DynamicObjectCollection coll)
        {
            if (coll != null && coll.Count > 0)
            {
                foreach (var item in coll)
                {
                    if (item != null)
                    {
                        DynamicObject sType = item["SETTLETYPEID"] as DynamicObject;
                        return SQLUtils.GetFieldValue(sType, "Number");
                    }
                }
            }
            return string.Empty;
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
                        DynamicObject rMethod = obj["F_HS_RefundMethod"] as DynamicObject;
                        string reFundMethod = SQLUtils.GetFieldValue(rMethod, "FNumber");

                        string document = SQLUtils.GetFieldValue(obj, "DOCUMENTSTATUS");

                        if (!string.IsNullOrWhiteSpace(reFundMethod) && !string.IsNullOrWhiteSpace(document))
                        {
                            if (reFundMethod.CompareTo("TKDKHYE") == 0 && document.CompareTo("C") == 0)
                            {
                                reFund = new AbsDataInfo();

                                reFund.FDate = SQLUtils.GetFieldValue(obj, "DATE");
                                reFund.FBillNo = SQLUtils.GetFieldValue(obj, "BILLNo");

                                DynamicObject cust = obj["F_HS_B2CCustId"] as DynamicObject;
                                reFund.F_HS_B2CCustId = SQLUtils.GetFieldValue(cust, "Number");

                                DynamicObject curr = obj["SETTLECUR"] as DynamicObject;
                                reFund.FSettleCurrId = SQLUtils.GetFieldValue(curr, "Number");

                                reFund.F_HS_RateToUSA = Convert.ToDecimal(SQLUtils.GetFieldValue(obj, "F_HS_RateToUSA"));
                                reFund.F_HS_BalanceRechargeUSD = Convert.ToDecimal(SQLUtils.GetFieldValue(obj, "REALREFUNDAMOUNTFOR"));

                                DynamicObject org = obj["SETTLEORGID"] as DynamicObject;
                                reFund.FSaleOrgId = SQLUtils.GetFieldValue(org, "Number");

                                DynamicObject dept = obj["SALEDEPTID"] as DynamicObject;
                                reFund.FSaleDeptId = SQLUtils.GetFieldValue(dept, "Number");

                                DynamicObject seller = obj["SALEERID"] as DynamicObject;
                                reFund.FSalerId = SQLUtils.GetFieldValue(seller, "Number");
                                reFund.F_HS_BalanceReceivableNo = SQLUtils.GetFieldValue(obj, "F_HS_BalanceReceivableNo");
                                DynamicObjectCollection entry = obj["REFUNDBILLENTRY"] as DynamicObjectCollection;
                                reFund.FSettleTypeId = GetSettleType(entry);

                                reFunds.Add(reFund);
                            }
                        }
                    }
                }
            }

            return reFunds;
        }

        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);
            List<DynamicObject> objs = e.DataEntitys.ToList();
            
            if (objs == null || objs.Count < 0)
                return;

            if (GetK3Datas(this.Context, objs,ref result) != null)
            {
                List<AbsSynchroDataInfo> datas = GetK3Datas(this.Context, objs,ref result).ToList();
                Dictionary<SynOperationType, IEnumerable<AbsSynchroDataInfo>> dict = new Dictionary<SynOperationType, IEnumerable<AbsSynchroDataInfo>>();

                dict.Add(SynOperationType.SAVE, datas);
                result = SynchroDataHelper.SynchroDataToK3(this.Context, this.DataType, true, null, dict);
            }   
        }
    }
}
