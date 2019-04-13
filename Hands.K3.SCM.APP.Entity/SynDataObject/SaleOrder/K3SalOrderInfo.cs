
using System;
using System.Collections.Generic;
using Hands.K3.SCM.APP.Entity.SynDataObject.SaleOrder;
using Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject;
using Newtonsoft.Json;

namespace Hands.K3.SCM.APP.Entity.SynDataObject
{
    [JsonObject(MemberSerialization.OptIn)]
    /// <summary>
    /// K3平台的订单信息
    /// </summary>
    public class K3SalOrderInfo : AbsSynchroDataInfo
    {

        public K3SalOrderInfo()
        {
            OrderEntry = new List<K3SalOrderEntryInfo>();
        }
        /// <summary>
        /// 订单来源
        /// </summary>
        public string OrderSource { get; set; }
        /// <summary>
        /// 单据类型
        /// </summary>
        public string FBillTypeId { get; set; }

        /// <summary>
        /// 业务类型
        /// </summary>
        public string FBusinessType { get; set; }
        [JsonProperty]
        /// <summary>
        /// 销售订单编码
        /// </summary>
        public string FBillNo { get; set; }
        /// <summary>
        /// 单据状态
        /// </summary>
        public string FDocumentStatus { get; set; }
        /// <summary>
        /// 关闭状态
        /// </summary>
        public string FCloseStatus { get; set; }
        /// <summary>
        /// 作废状态
        /// </summary>
        public string FCancelStatus { get; set; }
        [JsonProperty]
        /// <summary>
        /// 订单用户备注
        /// </summary>
        public string FNote { get; set; }
        [JsonProperty]
        /// <summary>
        /// 订单日期
        /// </summary>
        public DateTime FDate { get; set; }
        [JsonProperty]
        /// <summary>
        /// 订单的创建时间
        /// </summary>
        public DateTime FCreateDate { get; set; }
        [JsonProperty]
        /// <summary>
        /// 订单审核时间(时间戳)
        /// </summary>
        public string FApproveDate { get; set; }
        [JsonProperty]
        /// <summary>
        /// 订单日期（时间戳）
        /// </summary>
        public string PurseDate { get; set; }

        /// <summary>
        /// 销售组织编码
        /// </summary>
        public string FSaleOrgId { get; set; }

        /// <summary>
        /// 销售部门编码
        /// </summary>
        public string FSaleDeptId { get; set; }
        [JsonProperty]
        /// <summary>
        /// 客户编码,用于标记客户
        /// </summary>
        public string FCustId { get; set; }


        /// <summary>
        /// 客户邮箱
        /// </summary>
        public string FCustEmail { get; set; }

        /// <summary>
        /// 客户名称
        /// </summary>
        public string FCustName { get; set; }

        /// <summary>
        /// 客户所属公司
        /// </summary>
        public string FCustCompany { get; set; }

        /// <summary>
        /// 客户地址
        /// </summary>
        public string FCustAddress { get; set; }

        /// <summary>
        /// 客户手机号码
        /// </summary>
        public string FCustPhone { get; set; }

        /// <summary>
        /// 客户等级
        /// </summary>
        public string FCustLevel { get; set; }

        /// <summary>
        /// 销售员编码
        /// </summary>
        public string FSalerId { get; set; }
        [JsonProperty]
        /// <summary>
        /// 结算币别
        /// </summary>
        public string FSettleCurrId { get; set; }

        /// <summary>
        /// 结算方式
        /// </summary>
        public string FSettleModeId { get; set; }
        [JsonProperty]
        /// <summary>
        /// 付款方式
        /// </summary>
        public string F_HS_PaymentModeNew { get; set; }

        /// <summary>
        /// 销售订单状态
        /// </summary>
        public string OrderStatus { get; set; }

        /// <summary>
        /// 汇率
        /// </summary>
        public decimal FExchangeRate { get; set; }
        /// <summary>
        /// dropship订单前缀
        /// </summary>
        public string F_HS_DropShipOrderPrefix { get; set; }

        private List<K3SalOrderEntryInfo> _OrderEntry;
        [JsonProperty]
        /// <summary>
        /// 订单明细
        /// </summary>
        public List<K3SalOrderEntryInfo> OrderEntry
        {
            get
            {
                if (_OrderEntry != null)
                {
                    foreach (var item in _OrderEntry)
                    {
                        if (item != null && this.F_HS_Subtotal > 0)
                        {
                            if (this.F_HS_IntegralDeduction > 0)
                            {
                                item.F_HS_PointsDiscountRate = this.F_HS_IntegralDeduction / this.F_HS_Subtotal * 100;
                            }
                            if (this.F_HS_CouponAmount > 0)
                            {
                                item.F_HS_CouponDiscountRate = this.F_HS_CouponAmount / this.F_HS_Subtotal * 100;
                            }
                            if (this.F_HS_OrderDiscountAmountNew > 0)
                            {
                                item.F_HS_OnlineOrderDiscountRate = this.F_HS_OrderDiscountAmountNew / this.F_HS_Subtotal * 100;
                            }
                        }

                    }
                    return _OrderEntry;
                }

                return null;
            }
            set
            {
                _OrderEntry = value;
            }

        }
        /// <summary>
        /// 财务信息
        /// </summary>
        public K3SaleOrderFinance orderFin { get; set; }
        /// <summary>
        /// 收货明细
        /// </summary>
        public List<K3SalOrderEntryPlan> OrderEntryPlan { get; set; }
        /// <summary>
        /// 收款条件
        /// </summary>
        public K3SalOrderPlan OrderPlan { get; set; }

        public string FShipTypeId { get; set; }
        [JsonProperty]
        /// <summary>
        /// 客户真实ID
        /// </summary>
        public string F_HS_B2CCustId { get; set; }
        [JsonProperty]
        /// <summary>
        /// 订单渠道
        /// </summary>
        public string F_HS_Channel { get; set; }
        /// <summary>
        /// 订单来源
        /// </summary>
        public string F_HS_SaleOrderSource { get; set; }
        [JsonProperty]
        /// <summary>
        ///优惠券金额
        /// </summary>
        public decimal F_HS_CouponAmount { get; set; }
        [JsonProperty]
        /// <summary>
        /// 运费
        /// </summary>
        public decimal F_HS_Shipping { get; set; }
        /// <summary>
        /// 运费折扣
        /// </summary>
        public decimal F_HS_ShippingDiscount { get; set; }
        [JsonProperty]
        /// <summary>
        /// 订单总金额
        /// </summary>
        public decimal F_HS_Subtotal { get; set; }
        [JsonProperty]
        /// <summary>
        /// 优惠后订单金额（包括运费）
        /// </summary>
        public decimal F_HS_Total { get; set; }
        [JsonProperty]
        /// <summary>
        /// 已优惠金额
        /// </summary>
        public decimal F_HS_DiscountedAmount { get; set; }
        [JsonProperty]
        /// <summary>
        /// 产品积分
        /// </summary>
        public decimal F_HS_Points { get; set; }
        [JsonProperty]
        /// <summary>
        /// 使用积分数（金额）
        /// </summary>
        public decimal F_HS_IntegralDeduction { get; set; }
        /// <summary>
        /// 是否为虚拟
        /// </summary>
        public bool F_HS_IsVirtual { get; set; }
        [JsonProperty]
        /// <summary>
        /// 订单付款状态
        /// </summary>
        public string F_HS_PaymentStatus { get; set; }
        [JsonProperty]
        /// <summary>
        /// 交货联系人
        /// </summary>
        public string F_HS_DeliveryName { get; set; }
        [JsonProperty]
        /// <summary>
        /// 交货城市
        /// </summary>
        public string F_HS_DeliveryCity { get; set; }
        [JsonProperty]
        /// <summary>
        /// 交货省份/州
        /// </summary>
        public string F_HS_DeliveryProvinces { get; set; }
        [JsonProperty]
        /// <summary>
        /// 交货邮编
        /// </summary>
        public string F_HS_PostCode { get; set; }
        [JsonProperty]
        /// <summary>
        /// 国家
        /// </summary>
        public string F_HS_RecipientCountry { get; set; }
        /// <summary>
        /// 交货详细地址1
        /// </summary>
        public string DeliveryStreetAddress { get; set; }
        /// <summary>
        /// 交货详细地址2
        /// </summary>
        public string DeliverySuburbAddress { get; set; }
        [JsonProperty]
        /// <summary>
        /// 交货详细地址
        /// </summary>
        public string F_HS_DeliveryAddress { get; set; }
        [JsonProperty]
        /// <summary>
        /// 移动电话
        /// </summary>
        public string F_HS_MobilePhone { get; set; }
        [JsonProperty]
        /// <summary>
        /// 出货方式
        /// </summary>
        public string F_HS_ShippingMethod { get; set; }
        /// <summary>
        /// 订单美国时间
        /// </summary>
        public DateTime F_HS_USADatetime { get; set; }
        [JsonProperty]
        /// <summary>
        /// 订单兑美元汇率
        /// </summary>
        public decimal F_HS_RateToUSA { get; set; }
        /// <summary>
        /// 账单地址
        /// </summary>
        public string F_HS_BillAddress { get; set; }
        /// <summary>
        /// 交易流水号
        /// </summary>
        public string F_HS_TransactionID { get; set; }
        /// <summary>
        /// 客户申报要求
        /// </summary>
        public string F_HS_DeclareRqstd { get; set; }
        /// <summary>
        /// 账单地址&发货地址是否相同
        /// </summary>
        public bool F_HS_IsSameAdress { get; set; }
        /// <summary>
        /// 各发货仓运费(USD)
        /// </summary>
        public string F_HS_EachShipmentFreight { get; set; }
        [JsonProperty]
        /// <summary>
        /// 原线上单号
        /// </summary>
        public string F_HS_OriginOnlineOrderNo { get; set; }
        /// <summary>
        /// 品牌折扣抵扣额
        /// </summary>
        public decimal F_HS_BrandDiscountAmount { get; set; }
        /// <summary>
        /// 组合产品折扣抵扣额
        /// </summary>
        public decimal F_HS_CombinedDiscountAmount { get; set; }
        /// <summary>
        /// 订单折扣额
        /// </summary>
        public decimal F_HS_OrderDiscountAmountNew { get; set; }
        /// <summary>
        /// 是否使用收货地价目表
        /// </summary>
        public bool F_HS_UseLocalPriceList { get; set; }
       
        /// <summary>
        /// 抵扣的积分
        /// </summary>
        public decimal F_HS_UsePoint { get; set; }
        /// <summary>
        /// 订单合单前的原单
        /// </summary>
        public List<string> OriginalOrderNos { get; set; }

        public string F_HS_Hand { get; set; }
        /// <summary>
        /// 是否为合并单据
        /// </summary>
        public bool F_HS_IsMergeBill { get; set; }
        /// <summary>
        /// 合并单据源单编码
        /// </summary>
        public string F_HS_MergeSrcBillNo { get; set; }
        /// <summary>
        /// 深圳香港仓合并备注
        /// </summary>
        public string F_HS_SZHKMergeNote { get; set; }
        /// <summary>
        /// 订单已支付金额
        /// </summary>
        public string F_HS_PayTotal { get; set; }
        /// <summary>
        /// 余额支付金额
        /// </summary>
        public decimal F_HS_BalancePayments { get; set; }
        /// <summary>
        /// 余额支付金额（USD）
        /// </summary>
        public decimal F_HS_USDBalancePayments { get; set; }
        /// <summary>
        /// 余额支付金额（CNY）
        /// </summary>
        public decimal F_HS_CNYBalancePayments { get; set; }
        /// <summary>
        /// 整单折扣额(内网创建单才有)
        /// </summary>
        public decimal F_HS_ProductDiscountAmount { get; set; }
        /// <summary>
        /// 运费折扣额(内网创建单才有)
        /// </summary>
        public decimal F_HS_FreightDiscountAmount { get; set; }
        [JsonProperty]
        /// <summary>
        /// CEO特批已到款时间
        /// </summary>
        public DateTime F_HS_CollectionTime { get; set; }
        [JsonProperty]
        /// <summary>
        /// CEO特批已到款时间(时间戳)
        /// </summary>
        public string PayedTime { get; set; }
        /// <summary>
        /// 运费金额
        /// </summary>
        public decimal F_HS_FreightAmount
        {
            get
            {
                return F_HS_Shipping;
            }
        }
        /// <summary>
        /// 整单产品折扣率%
        /// </summary>
        public decimal F_HS_ProductDiscountRate
        {
            //get
            //{
            //    decimal ProductDiscount = F_HS_Total - (F_HS_Shipping - F_HS_ShippingDiscount - F_HS_FreightDiscountAmount) + F_HS_ProductDiscountAmount;

            //    if (ProductDiscount > 0)
            //    {
            //        return Math.Round(F_HS_ProductDiscountAmount / ProductDiscount,6);  
            //    }
            //    return 0;
            //}
            get
            {
                if (F_HS_Subtotal > 0)
                {
                    return Math.Round(F_HS_ProductDiscountAmount / F_HS_Subtotal,6);
                }

                return 0;
            }
        }
        /// <summary>
        /// 运费折扣率%(线下订单)
        /// </summary>
        public decimal F_HS_FreightDiscountRate
        {
            get
            {
                if (F_HS_Shipping > 0)
                {
                    return Math.Round(F_HS_ShippingDiscount / F_HS_Shipping, 6) * 100;
                }
                return 0;
            }
        }

        public decimal F_HS_WebFreightDiscountRate
        {
            get
            {
                if (F_HS_Shipping > 0)
                {
                    return Math.Round(F_HS_ShippingDiscount / F_HS_Shipping, 6) * 100;
                }
                return 0;
            }
        }

        /// <summary>
        /// 整单折扣备注原因 
        /// </summary>
        public string F_HS_DiscountRemarks { get; set; }
        [JsonProperty]
        /// <summary>
        /// 折扣
        /// </summary>
        public decimal Discount
        {
            get
            {
                if (F_HS_Total > 0)
                {
                    if (F_HS_Subtotal != 0)
                    {

                        return (F_HS_Total - F_HS_Shipping + F_HS_ShippingDiscount + F_HS_FreightDiscountAmount + F_HS_ProductDiscountAmount) / F_HS_Subtotal;
                    }
                }

                return 1;
            }
        }

        public decimal F_HS_NeedPayAmount { get; set; }

        public decimal F_HS_PlatformFreightRate { get; set; }

        /// <summary>
        /// DropShip发货渠道
        /// </summary>
        public string F_HS_DropShipDeliveryChannel { get; set; }
        /// <summary>
        /// dropship是否固定仓库
        /// </summary>
        public bool F_HS_YNFixedDropShipStock { get; set; }

        /// <summary>
        /// 信用额度支付金额USD
        /// </summary>
        public decimal F_HS_CreditLineUSDPayments { get; set; }
        /// <summary>
        /// 平台客户ID
        /// </summary>
        public string F_HS_PlatformCustomerID { get; set; }
        /// <summary>
        /// 平台客户邮箱
        /// </summary>
        public string F_HS_PlatformCustomerEmail { get; set; }
        /// <summary>
        /// 产品申报金额
        /// </summary>
        public decimal F_HS_AmountDeclared { get; set; }
        /// <summary>
        /// 运费申报金额
        /// </summary>
        public decimal F_HS_FreightDeclared { get; set; }
        /// <summary>
        /// 申报币别
        /// </summary>
        public string F_HS_DeclaredCurrId { get; set; }
        /// <summary>
        /// 平台
        /// </summary>
        public string F_HS_Platform { get; set; }

    }

}
