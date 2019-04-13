using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Hands.K3.SCM.APP.Entity.SynDataObject;
using Kingdee.BOS;
using Hands.K3.SCM.APP.Entity.SynDataObject.SaleOrder;

using Hands.K3.SCM.APP.Entity.K3WebApi;
using System.ComponentModel;
using Hands.K3.SCM.APP.Utils.Utils;
using Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject;
using Hands.K3.SCM.App.Synchro.Utils.SynchroService;
using HS.K3.Common.Abbott;
using Hands.K3.SCM.APP.Entity.EnumType;

namespace Hands.K3.SCM.App.ServicePlugIn
{
    [Description("销售订单界面生成收款单的服务插件")]
    public class SynReceiveBillSerPlugIn : AbstractOSPlugIn
    {
        HttpResponseResult result = null;
        HttpResponseResult _result = null;

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

            e.FieldKeys.Add("FDate");
            e.FieldKeys.Add("FDocumentStatus");
            e.FieldKeys.Add("F_HS_YNSyncCollection");
            e.FieldKeys.Add("F_HS_SaleOrderSource");
            e.FieldKeys.Add("FCustId");
            e.FieldKeys.Add("F_HS_B2CCustId");
            e.FieldKeys.Add("FSalerId");
            e.FieldKeys.Add("FSaleOrgId");
            e.FieldKeys.Add("FSaleDeptId");
            e.FieldKeys.Add("FSettleCurrId");
            e.FieldKeys.Add("FSaleOrderEntry");
            e.FieldKeys.Add("F_HS_PaymentModeNew");
            e.FieldKeys.Add("F_HS_TransactionID");
            e.FieldKeys.Add("FSaleOrderFinance");
            e.FieldKeys.Add("F_HS_PayTotal");
            e.FieldKeys.Add("F_HS_RateToUSA");
            e.FieldKeys.Add("F_HS_RateToUSA");

        }

        public override void OnPrepareOperationServiceOption(OnPrepareOperationServiceEventArgs e)
        {
            base.OnPrepareOperationServiceOption(e);

        }

        public override void OnAddValidators(AddValidatorsEventArgs e)
        {
            base.OnAddValidators(e);
            this.OperationResult.IsShowMessage = true;
            
            
        }
        public override void BeforeExecuteOperationTransaction(BeforeExecuteOperationTransaction e)
        {
            base.BeforeExecuteOperationTransaction(e);
        }

        public override void AfterExecuteOperationTransaction(AfterExecuteOperationTransaction e)
        {
            base.AfterExecuteOperationTransaction(e);

            if (result != null && !string.IsNullOrWhiteSpace(result.Message))
            {
                throw new Exception(result.Message);
            }
            if (_result != null && !string.IsNullOrWhiteSpace(_result.Message))
            {
                throw new Exception(_result.Message);
            }
        }

        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);

            List<DynamicObject> objs = e.DataEntitys.ToList();
            if (objs == null || objs.Count < 0)
                return;

            List<K3SalOrderInfo> orders = GetK3SalOrderInfos(objs);
            if (orders != null)
            {
                Dictionary<SynOperationType, IEnumerable<AbsSynchroDataInfo>> dict = new Dictionary<SynOperationType, IEnumerable<AbsSynchroDataInfo>>();
                dict.Add(SynOperationType.SAVE,orders);

                result = SynchroDataHelper.SynchroDataToK3(this.Context,this.DataType,true,null,dict);
            }
        }

        private List<K3SalOrderInfo> GetK3SalOrderInfos(List<DynamicObject> objs)
        {
            List<K3SalOrderInfo> orders = null;
            K3SalOrderInfo order = null;
            string message = string.Empty;

            if (objs != null && objs.Count > 0)
            {
               
                orders = new List<K3SalOrderInfo>();

                foreach (var obj in objs)
                {
                    if (obj != null)
                    {
                        string documentstatus = SQLUtils.GetFieldValue(obj, "DocumentStatus");
                        bool isSyn = Convert.ToBoolean(SQLUtils.GetFieldValue(obj, "F_HS_YNSyncCollection"));

                        DynamicObject oSrc = obj["F_HS_SaleOrderSource"] as DynamicObject;
                        string oSource = SQLUtils.GetFieldValue(oSrc, "FNumber");

                        if (documentstatus.CompareTo("C") == 0 && !isSyn && oSource.CompareTo("HCWebProcessingOder") == 0)
                        {
                            if (Convert.ToDecimal(SQLUtils.GetFieldValue(obj, "F_HS_PayTotal")) > 0)
                            {
                                order = new K3SalOrderInfo();

                                order.FDate = Convert.ToDateTime(SQLUtils.GetFieldValue(obj, "Date"));
                                order.FBillNo = SQLUtils.GetFieldValue(obj, "BillNo");

                                DynamicObject cust = obj["CustId"] as DynamicObject;
                                order.FCustId = SQLUtils.GetFieldValue(cust, "Number");

                                DynamicObject b2cCust = obj["F_HS_B2CCustId"] as DynamicObject;
                                order.F_HS_B2CCustId = SQLUtils.GetFieldValue(b2cCust, "Number");

                                DynamicObject saler = obj["SalerId"] as DynamicObject;
                                order.FSalerId = SQLUtils.GetFieldValue(saler, "Number");

                                DynamicObject saleOrg = obj["SaleOrgId"] as DynamicObject;
                                order.FSaleOrgId = SQLUtils.GetFieldValue(saleOrg, "Number");

                                DynamicObject saleDept = obj["SaleDeptId"] as DynamicObject;
                                order.FSaleDeptId = SQLUtils.GetFieldValue(saleDept, "Number");

                                DynamicObject pay = obj["F_HS_PaymentModeNew"] as DynamicObject;
                                order.F_HS_PaymentModeNew = SQLUtils.GetFieldValue(pay, "FNumber");

                                order.F_HS_TransactionID = SQLUtils.GetFieldValue(obj, "F_HS_TransactionID");
                                order.F_HS_PayTotal = SQLUtils.GetFieldValue(obj, "F_HS_PayTotal");
                                order.F_HS_RateToUSA = Convert.ToDecimal(SQLUtils.GetFieldValue(obj, "F_HS_RateToUSA"));

                                K3SaleOrderFinance fin = GetK3SaleOrderFinance(obj);
                                if (fin != null)
                                {
                                    order.FSettleCurrId = fin.FSettleCurrID;
                                }

                                orders.Add(order);
                            }
                            else
                            {
                                message += "销售订单：【" + SQLUtils.GetFieldValue(obj, "BillNo") + "】的Pay_Total的金额小于零，不允许生成收款单！" + Environment.NewLine;
                            }
                            
                        }
                        else
                        {
                            message += "销售订单：【"+ SQLUtils.GetFieldValue(obj, "BillNo") + "】收款单生成只能是processing和未生成收款单的销售订单" + Environment.NewLine; 
                        }
                    }
                } 
            }
            if (!string.IsNullOrWhiteSpace(message))
            {
                _result = new HttpResponseResult();
                _result.Message = message;
                _result.Success = false; 
            }
            return orders;
        }

        private K3SaleOrderFinance GetK3SaleOrderFinance(DynamicObject obj)
        {
            K3SaleOrderFinance fin = null;

            if (obj != null)
            {
                DynamicObjectCollection objs = obj["SaleOrderFinance"] as DynamicObjectCollection;

                if (objs != null && objs.Count > 0)
                {
                    fin = new K3SaleOrderFinance();
                    foreach (var item in objs)
                    {
                        if (item != null)
                        {
                            DynamicObject curr = item["SettleCurrId"] as DynamicObject;
                            fin.FSettleCurrID = SQLUtils.GetFieldValue(curr, "Number");
                            fin.F_HS_RateToUSA = Convert.ToDecimal(SQLUtils.GetFieldValue(item, "ExchangeRate"));
                        }
                    }
                }
            }

            return fin;
        }

        private void GetNonSychroRevSalOrder(Context ctx,DateTime begin,DateTime end)
        {
            DateTime firstDay = new DateTime(DateTime.Now.Year,DateTime.Now.Month,1);
            string sql = string.Format(@"/*dialect*/ select distinct FBillNo,FDate,j.FNUMBER as custNo,o.FNumber,F_HS_RateToUSA,a.F_HS_TransactionID,d.FBillAmount,q.FNUMBER as paymentMehtod
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
                                                    where a.FDOCUMENTSTATUS = 'C'and a.FsaleOrgID = 100035 and a.FCANCELSTATUS<>'B' and h.FNUMBER='XSDD01_SYS' and z.FNUMBER='100.01' and g.FNUMBER = 'HCWebProcessingOder'
													and a.FCREATEDATE between '{0} 00:00:00' and getdate()
                                                    and a.fbillno not in 
													(select l.FRECEIVEITEM from T_AR_RECEIVEBILL k
													inner join  T_AR_RECEIVEBILLENTRY l on l.FID = K.FID
													where k.FCREATEDATE between '{1} 00:00:00' and getdate()
													and l.FRECEIVEITEM not like 'SO%' and l.FRECEIVEITEM <> '')",firstDay,firstDay

                                                    );
            DynamicObjectCollection coll = SQLUtils.GetObjects(ctx,sql);
            if (coll != null && coll.Count() > 0)
            {
                foreach (var item in coll)
                {
                    if (item != null)
                    {

                    }
                }
            }
        }
    }
}
