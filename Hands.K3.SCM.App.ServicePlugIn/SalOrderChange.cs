using Kingdee.BOS.Orm.DataEntity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Hands.K3.SCM.APP.Entity.SynDataObject;
using Hands.K3.SCM.APP.Entity.SynDataObject.SaleOrder;

using Hands.K3.SCM.APP.Utils.Utils;
using HS.K3.Common.Abbott;

namespace Hands.K3.SCM.App.ServicePlugIn
{
    [Description("销售变更单--销售订单状态同步插件")]
    public class SalOrderChange : AbstractOSPlugIn
    {
        public override SynchroDataType DataType
        {
            get
            {
                return SynchroDataType.SaleOrderStatus;
            }
        }

        public override void OnPreparePropertys(Kingdee.BOS.Core.DynamicForm.PlugIn.Args.PreparePropertysEventArgs e)
        {
            //base.OnPreparePropertys(e);
            //e.FieldKeys.Add("F_HS_SaleOrderSource");
            //e.FieldKeys.Add("F_HS_RecipientCountry");
            //e.FieldKeys.Add("F_HS_DeliveryProvinces");
            //e.FieldKeys.Add("F_HS_DeliveryCity");
            //e.FieldKeys.Add("F_HS_DeliveryAddress");
            //e.FieldKeys.Add("F_HS_PostCode");
            //e.FieldKeys.Add("F_HS_DeliveryName");
            //e.FieldKeys.Add("F_HS_BillAddress");
            //e.FieldKeys.Add("F_HS_MobilePhone");
            //e.FieldKeys.Add("F_HS_ShippingMethod");
            //e.FieldKeys.Add("F_HS_PaymentMode");
            //e.FieldKeys.Add("F_HS_PaymentStatus");
            //e.FieldKeys.Add("F_HS_FGroup");
            //e.FieldKeys.Add("Note");
            //e.FieldKeys.Add("F_HS_OnlineOrderWay");
            //e.FieldKeys.Add("F_HS_COUPONAMOUNT");
            //e.FieldKeys.Add("F_HS_Shipping");
            //e.FieldKeys.Add("F_HS_Points");
            //e.FieldKeys.Add("F_HS_StockID");
            //e.FieldKeys.Add("F_HS_IsEmptyStock");
            //e.FieldKeys.Add("Note");
            //e.FieldKeys.Add("F_HS_DiscountedAmount");
        }
        //public override void EndOperationTransaction(Kingdee.BOS.Core.DynamicForm.PlugIn.Args.EndOperationTransactionArgs e)
        //{
        //    base.EndOperationTransaction(e);

           
        //    //List<K3SalOrderInfo> temp = new List<K3SalOrderInfo>();

        //    //if (e.DataEntitys == null) return;
        //    //List<DynamicObject> dataEntitys = e.DataEntitys.ToList();

        //    //if (dataEntitys == null || dataEntitys.Count <= 0)
        //    //{
        //    //    return;
        //    //}
            

        //    //foreach (var item in dataEntitys)
        //    //{
        //    //    if (SQLUtils.GetSaleOrderSourceNo(this.Context, item, "F_HS_SaleOrderSource_Id").CompareTo("XXBJDD") == 0
        //    //        && SQLUtils.GetFieldValue(item, "DocumentStatus").CompareTo("C") == 0 && SQLUtils.GetFieldValue(item, "BillNo").StartsWith("SO")
        //    //        && SQLUtils.GetFieldValue(item, "BillNo").Contains("_"))
        //    //    {
        //    //        K3SaleOrderFinance finance = GetOrderFinance(item, "SaleOrderFinance");//财务信息

        //    //        string FBillNo = SQLUtils.GetFieldValue(item, "BillNo");//订单号

        //    //        if (!string.IsNullOrWhiteSpace(FBillNo))
        //    //        {
        //    //            if (FBillNo.Contains("_"))
        //    //            {
        //    //                FBillNo = FBillNo.Substring(0, FBillNo.IndexOf("_"));
        //    //                SynOfflineSalOrderToHC.SynAllSalOrderToPHP(this.Context, false, FBillNo);
        //    //            }
                        
        //    //        }
        //    //    }

        //    //}
    
        //}


        public override void AfterExecuteOperationTransaction(AfterExecuteOperationTransaction e)
        {
            base.AfterExecuteOperationTransaction(e);

           
            List<K3SalOrderInfo> orders = null;
            List<K3SalOrderInfo> temp = new List<K3SalOrderInfo>();

            if (e.DataEntitys == null) return;
            List<DynamicObject> dataEntitys = e.DataEntitys.ToList();

            if (dataEntitys == null || dataEntitys.Count <= 0)
            {
                return;
            }
            orders = new List<K3SalOrderInfo>();

            foreach (var item in dataEntitys)
            {
                if (SQLUtils.GetSaleOrderSourceNo(this.Context, item, "F_HS_SaleOrderSource_Id").CompareTo("XXBJDD") == 0
                    && SQLUtils.GetFieldValue(item, "DocumentStatus").CompareTo("C") == 0 && SQLUtils.GetFieldValue(item, "BillNo").StartsWith("SO")
                    && SQLUtils.GetFieldValue(item, "BillNo").Contains("_"))
                {
                    K3SaleOrderFinance finance = GetOrderFinance(item, "SaleOrderFinance");//财务信息

                    string FBillNo = SQLUtils.GetFieldValue(item, "BillNo");//订单号

                    if (!string.IsNullOrWhiteSpace(FBillNo))
                    {
                        if (FBillNo.Contains("_"))
                        {
                            FBillNo = FBillNo.Substring(0, FBillNo.IndexOf("_"));
                            //SynOfflineSalOrderToHC.SynAllSalOrderToPHP(false, FBillNo);
                        }

                    }

                    //order = new K3SalOrderInfo();

                    //K3SaleOrderFinance finance = GetOrderFinance(item, "SaleOrderFinance");//财务信息

                    //order.FBillNo = SQLUtils.GetFieldValue(item, "BillNo");//订单号
                    //order.F_HS_OriginOnlineOrderNo = SQLUtils.GetFieldValue(item, "F_HS_OriginOnlineOrderNo");//原线上订单单号
                    //order.FDate = Convert.ToDateTime(SQLUtils.GetFieldValue(item, "Date"));//订单日期
                    //order.PurseDate = TimeHelper.GetTimeStamp(order.FDate);//订单日期（时间戳）
                    //order.FNote = SQLUtils.GetFieldValue(item, "FNote");//备注
                    //order.FCustId = SQLUtils.GetCustomerNo(this.Context, item, "CustId_Id");//客户
                    //order.FSalerId = SQLUtils.GetSellerNo(this.Context, item, "SalerId_Id");//销售员
                    //order.OrderSource = SQLUtils.GetSaleOrderSourceNo(this.Context, item, "F_HS_SaleOrderSource_Id");//订单来源

                    //order.F_HS_PaymentStatus = SQLUtils.GetFieldValue(item, "F_HS_PaymentStatus");//付款状态
                    //order.FSettleCurrId = finance.FSettleCurrID;//结算币别

                    //order.F_HS_RateToUSA = Convert.ToDecimal(SQLUtils.GetFieldValue(item, "ExchangeRate"));//汇率
                    //order.F_HS_RecipientCountry = SQLUtils.GetCountryNo(this.Context, item, "F_HS_RecipientCountry_Id");//国家

                    //order.F_HS_DeliveryProvinces = SQLUtils.GetFieldValue(item, "F_HS_DeliveryProvinces");//省份
                    //order.F_HS_DeliveryCity = SQLUtils.GetFieldValue(item, "F_HS_DeliveryCity");//城市

                    //order.F_HS_DeliveryAddress = SQLUtils.GetFieldValue(item, "F_HS_DeliveryAddress");//具体地址
                    //order.F_HS_PostCode = SQLUtils.GetFieldValue(item, "F_HS_PostCode");//邮编

                    //order.F_HS_DeliveryName = SQLUtils.GetFieldValue(item, "F_HS_DeliveryName");//收货人
                    //order.F_HS_BillAddress = SQLUtils.GetFieldValue(item, "F_HS_BillAddress");//账单地址

                    //order.F_HS_MobilePhone = SQLUtils.GetFieldValue(item, "F_HS_MobilePhone");//联系人手机
                    //order.F_HS_ShippingMethod = SQLUtils.GetFieldValue(item, "F_HS_ShippingMethod");//发货方式

                    //order.FSETTLEMODEID = SQLUtils.GetFieldValue(item, "F_HS_PaymentMode");//结算方式

                    //order.FCustLevel = SQLUtils.GetCustGroupNo(this.Context, item, "F_HS_FGroup_Id");//客户分组

                    //order.FNote = SQLUtils.GetFieldValue(item, "Note");//备注
                    //order.F_HS_Channel = SQLUtils.GetFieldValue(item, "F_HS_OnlineOrderWay");//下单方式

                    //order.F_HS_CouponAmount = Convert.ToDecimal(SQLUtils.GetFieldValue(item, "F_HS_CouponAmount"));//优惠券金额
                    ////order.F_HS_Shipping = Convert.ToDecimal(SQLUtils.GetFieldValue(item, "F_HS_Shipping"));//运费

                    //order.F_HS_Points = Convert.ToDecimal(SQLUtils.GetFieldValue(item, "F_HS_Points"));//积分

                    //order.OrderEntry = GetOrderEntry(item, "SaleOrderEntry", order);//订单明细
                    //order.F_HS_Total = order.F_HS_Subtotal - order.F_HS_CouponAmount - order.F_HS_IntegralDeduction - order.F_HS_DiscountedAmount + order.F_HS_Shipping;//优惠后金额
                    //order.FApproveDate = TimeHelper.GetTimeStamp(Convert.ToDateTime(SQLUtils.GetFieldValue(item, "ApproveDate")));
                    //orders.Add(order);
                }
                //if (orders != null && orders.Count > 0)
                //{
                //    if (temp.All(a => orders.Any(b => a.Equals(b))))
                //    {
                //        temp = temp.Concat(orders).ToList();

                //        if (IsConnectSuccess(this.Context))
                //        {
                //            SynOfflineSalOrderToHC.SynSalOrderToPHP(this.Context, temp);
                //            temp.Clear();
                //        }
                //    }

                //}
            }
        }
        public override void OnPrepareOperationServiceOption(OnPrepareOperationServiceEventArgs e)
        {
            base.OnPrepareOperationServiceOption(e);
        }
        public override void InitializeOperationResult(IOperationResult result)
        {
            base.InitializeOperationResult(result);
        }
        public override void BeforeExecuteOperationTransaction(BeforeExecuteOperationTransaction e)
        {
            base.BeforeExecuteOperationTransaction(e);
        }
        public override void BeginOperationTransaction(BeginOperationTransactionArgs e)
        {
            base.BeginOperationTransaction(e);
        }
        public override void OnAddValidators(AddValidatorsEventArgs e)
        {
            base.OnAddValidators(e);
        }
        public override void RollbackData(OperationRollbackDataArgs e)
        {
            base.RollbackData(e);
        }
        
        /// <summary>
        /// 获取销售订单明细
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        private List<K3SalOrderEntryInfo> GetOrderEntry(DynamicObject obj, string fieldName, K3SalOrderInfo order)
        {
            List<K3SalOrderEntryInfo> entries = null;
            K3SalOrderEntryInfo entry = null;

            if (obj != null && !string.IsNullOrWhiteSpace(fieldName))
            {
                DynamicObjectCollection coll = obj[fieldName] as DynamicObjectCollection;

                if (coll != null && coll.Count > 0)
                {
                    entries = new List<K3SalOrderEntryInfo>();
                    foreach (var item in coll)
                    {
                        if (SQLUtils.GetMaterialNo(this.Context, item, "MaterialID_Id").CompareTo("99.01") != 0)
                        {
                            entry = new K3SalOrderEntryInfo();
                            entry.FMaterialId = SQLUtils.GetMaterialNo(this.Context, item, "MaterialID_Id");//物料编码
                            entry.FTAXPRICE = Convert.ToDecimal(SQLUtils.GetFieldValue(item, "TaxPrice"));//单价
                            entry.FTAXAmt = Convert.ToDecimal(SQLUtils.GetFieldValue(item, "AllAmount"));
                            entry.FQTY = Convert.ToDecimal(SQLUtils.GetFieldValue(item, "Qty"));//数量
                            entry.FStockId = SQLUtils.GetStockNo(this.Context, item, "F_HS_StockID_Id");//仓库编码

                            entry.F_HS_IsVirtualEntry = Convert.ToBoolean(SQLUtils.GetFieldValue(item, "F_HS_IsVirtualEntry"));//是否为虚拟
                            entry.FIsFree = Convert.ToBoolean(SQLUtils.GetFieldValue(item, "IsFree"));//是否为赠品
                            entry.F_HS_IsEmptyStock = Convert.ToBoolean(SQLUtils.GetFieldValue(item, "F_HS_IsEmptyStock"));//是否为清仓

                            entries.Add(entry);
                        }
                        else
                        {
                            order.F_HS_Shipping = Convert.ToDecimal(SQLUtils.GetFieldValue(item, "AllAmount"));//运费
                        }

                        order.F_HS_Subtotal += Convert.ToDecimal(SQLUtils.GetFieldValue(item, "AllAmount"));//产品总金额

                    }
                }
            }

            return entries;
        }

        /// <summary>
        /// 获取财务信息
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="fielName"></param>
        /// <returns></returns>
        private K3SaleOrderFinance GetOrderFinance(DynamicObject obj, string fielName)
        {
            K3SaleOrderFinance finance = null;

            if (obj != null && !string.IsNullOrWhiteSpace(fielName))
            {
                DynamicObjectCollection coll = obj[fielName] as DynamicObjectCollection;

                if (coll != null && coll.Count > 0)
                {
                    finance = new K3SaleOrderFinance();

                    foreach (var item in coll)
                    {
                        finance.FSettleCurrID = SQLUtils.GetSettleCurrNo(this.Context, item, "SettleCurrId_Id");//结算币别
                    }

                }

            }
            return finance;
        }

    }
}
