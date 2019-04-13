using Hands.K3.SCM.APP.Entity.StructType;
using Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject;
using Hands.K3.SCM.APP.Entity.SynDataObject.SaleOrder;
using Hands.K3.SCM.APP.Utils.Utils;
using HS.K3.Common.Abbott;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Hands.K3.SCM.APP.DynamicFormPlugIn
{
    [Kingdee.BOS.Util.HotUpdate]
    [Description("销售订单--CEO特批已到款插件")]
    public class SalOrderPlugIn : AbstractDynamicFormPlugIn
    {
        public string SaleOrderSource { get; set; }
        public SynchroDataType DataType
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(SaleOrderSource))
                {
                    switch (SaleOrderSource)
                    {
                        case "HCWebProcessingOder":
                        case "HCWebPendingOder":
                            return SynchroDataType.SaleOrder;
                        case "XXBJDD":
                            return SynchroDataType.SaleOrderOffline;
                        case "DropShippingOrder":
                            return SynchroDataType.DropShippingSalOrder;
                        default:
                            return SynchroDataType.None;
                    }
                }

                return SynchroDataType.None;
            }
        }

        /// <summary>
        /// 点击“CEO特批已到款”后设置付款状态
        /// </summary>
        /// <param name="ctx"></param>
        private void SetBillStatus(Context ctx, AbsSynchroDataInfo info)
        {
            if (info != null)
            {
                AbsDataInfo order = info as AbsDataInfo;
                this.View.Model.SetValue("F_HS_BalanceDeducted",order.F_HS_BalanceDeducted);

                if (order.F_HS_CheckBox)
                {
                    this.View.Model.SetValue("F_HS_CheckBox", true);
                    this.View.InvokeFormOperation("Save");

                    if (this.DataType != SynchroDataType.DropShippingSalOrder)
                    {
                        this.View.Model.SetValue("F_HS_PaymentStatus", "3");
                        bool success = this.View.InvokeFormOperation("Save");

                        if (success)
                        {
                            this.View.ShowMessage("付款状态已设置为已到款！", MessageBoxType.Notice);
                        }
                        else
                        {
                            this.View.ShowMessage("付款状态设置失败！", MessageBoxType.Notice);
                        }
                    }
                    else
                    {
                        if (!order.F_HS_BalanceDeducted)
                        {
                            if(order.FDocumentStatus.Equals(BillDocumentStatus.Audit))
                            {
                                if (!order.F_HS_PaymentStatus.Equals("3"))
                                {
                                    if (order.FBillAmount <= order.FCustBalanceAmount)
                                    {
                                        if (!string.IsNullOrWhiteSpace(order.FDocumentStatus))
                                        {
                                            if (order.FDocumentStatus.Equals(BillDocumentStatus.Audit))
                                            {
                                                this.View.InvokeFormOperation("UnAudit");
                                                this.View.InvokeFormOperation("Save");
                                                this.View.InvokeFormOperation("Submit");
                                                this.View.InvokeFormOperation("Audit");
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
                    this.View.Model.SetValue("F_HS_CheckBox", false);
                    this.View.ShowErrMessage("该订单不是CEO特批已到款", "错误提示", MessageBoxType.Error);
                }
            }
        }

        /// <summary>
        /// 5.21.3.2 同步余额扣减数据到redis
        /// 5.21.3.4 更新销售订单.已扣减余额为true、更新客户.余额USD，更新客户.剩余信用额度USD
        /// 5.21.3.5 写客户余额支付日志、信用额度支付日志
        /// </summary>
        /// <param name="ctx"></param>
        private void ExecuteOperate(Context ctx)
        {
            AbsSynchroDataInfo data = GetK3Data(ctx);
            SetBillStatus(ctx, data);
        }

        /// <summary>
        /// 获取销售订单相关信息
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        private AbsSynchroDataInfo GetK3Data(Context ctx)
        {
            AbsDataInfo order = new AbsDataInfo();
            order.F_HS_CheckBox = Convert.ToBoolean(this.View.Model.GetValue("F_HS_CheckBox"));

            DynamicObject source = this.View.Model.GetValue("F_HS_SaleOrderSource") as DynamicObject;
            string sType = SQLUtils.GetFieldValue(source, "FNumber");
            order.F_HS_SaleOrderSource = sType;
            this.SaleOrderSource = sType;

            if (this.DataType.Equals(SynchroDataType.DropShippingSalOrder))
            {
                order.FDocumentStatus = JsonUtils.ConvertObjectToString(this.View.Model.GetValue("FDocumentStatus"));
                order.F_HS_PaymentStatus = JsonUtils.ConvertObjectToString(this.View.Model.GetValue("F_HS_PaymentStatus"));
                order.FCancelStatus = JsonUtils.ConvertObjectToString(this.View.Model.GetValue("FCancelStatus"));
                order.F_HS_BalanceDeducted = Convert.ToBoolean(this.View.Model.GetValue("F_HS_BalanceDeducted"));

                DynamicObject cust = this.View.Model.GetValue("F_HS_B2CCustId") as DynamicObject;
                string custNo = SQLUtils.GetFieldValue(cust, "Number");

                decimal rateToUSA = Convert.ToDecimal(this.View.Model.GetValue("F_HS_RateToUSA"));
                order.FBillAmount = Convert.ToDecimal(this.View.Model.GetValue("FBillAmount")) / rateToUSA;

                decimal useUsdBalance = Math.Round(Convert.ToDecimal(this.View.Model.GetValue("F_HS_USDBalancePayments")), 2) / rateToUSA;
                order.F_HS_BalanceRechargeUSD = Math.Round(Convert.ToDecimal(this.View.Model.GetValue("F_HS_USDBalancePayments")), 2) / rateToUSA;
                decimal useCredit = Math.Round(Convert.ToDecimal(this.View.Model.GetValue("F_HS_USDBalancePayments")), 2) / rateToUSA;
                order.F_HS_CreditLineRechargeUSD = Math.Round(Convert.ToDecimal(this.View.Model.GetValue("F_HS_USDBalancePayments")), 2) / rateToUSA;

                decimal custBalance = Math.Round(LogHelper.GetCustBalance(ctx, custNo, "100.01"));
                decimal custCredit = Math.Round(LogHelper.GetCustCreditLine(ctx, custNo, "F_HS_SurplusCreditUSD"));
                order.FCustBalanceAmount = custBalance + custCredit;
            }
           
            return order;
        }

        /// <summary>
        /// 获取销售订单财务信息
        /// </summary>
        /// <returns></returns>
        private K3SaleOrderFinance GetOrderFinance()
        {
            K3SaleOrderFinance fin = null;
            DynamicObjectCollection coll = this.View.Model.DataObject["SaleOrderFinance"] as DynamicObjectCollection;

            if (coll != null && coll.Count > 0)
            {
                foreach (var item in coll)
                {
                    if (item != null)
                    {
                        fin = new K3SaleOrderFinance();
                        DynamicObject fina = this.View.Model.GetValue("FSettleCurrId") as DynamicObject;
                        fin.FSettleCurrID = SQLUtils.GetFieldValue(fina, "Number");
                        fin.FBillAllAmount = Convert.ToDecimal(this.View.Model.GetValue("FBillAmount"));
                    }
                }
            }

            return fin;
        }
        public override void AfterDoOperation(AfterDoOperationEventArgs e)
        {
            base.AfterDoOperation(e);
            switch (e.Operation.Operation.ToUpper())
            {
                case "CEOTEPI":
                    ExecuteOperate(this.Context);
                    break;
            }

        }

        //public override void ButtonClick(ButtonClickEventArgs e)
        //{
        //    base.ButtonClick(e);

        //    switch (e.Key)
        //    {
        //        case "F_HS_CEOAPPROVAL":
        //            SetPaymentStatus(this.Context);
        //            break;
        //    }
        //}

    }
}
