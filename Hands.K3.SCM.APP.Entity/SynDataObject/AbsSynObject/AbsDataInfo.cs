
using HS.K3.Common.Abbott;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject
{
    [JsonObject(MemberSerialization.OptIn)]
    public class AbsDataInfo : AbsSynchroDataInfo
    {
        /// <summary>
        /// 销售组织
        /// </summary>
        public string FSaleOrgId { get; set; }
        /// <summary>
        /// 销售部门
        /// </summary>
        public string FSaleDeptId { get; set; }
        /// <summary>
        /// 销售员
        /// </summary>
        public string FSalerId { get; set; }
        [JsonProperty]

        /// <summary>
        /// 同步数据类型
        /// </summary>
        public SynchroDataType DataType { get; set; }
        [JsonProperty]
        /// <summary>
        /// 单据来源
        /// </summary>
        public string F_HS_SaleOrderSource { get; set; }
        [JsonProperty]

        /// <summary>
        /// 单据编码
        /// </summary>
        public string FBillNo { get; set; }
        [JsonProperty]

        /// <summary>
        /// 单据日期
        /// </summary>
        public string FDate { get; set; }
        /// <summary>
        /// 单据状态
        /// </summary>
        public string FDocumentStatus { get; set; }
        /// <summary>
        /// 销售订单付款状态
        /// </summary>
        public string F_HS_PaymentStatus { get; set; }
        /// <summary>
        /// 单据作废状态
        /// </summary>
        public string FCancelStatus { get; set; }
        [JsonProperty]

        /// <summary>
        /// 客户真实ID
        /// </summary>
        public string F_HS_B2CCustId { get; set; }
        [JsonProperty]
        /// <summary>
        /// 变动金额
        /// </summary>
        public decimal FRealAmountFor
        {
            get
            {
                if (F_HS_RateToUSA > 0)
                {
                    return Math.Round((F_HS_BalanceRechargeUSD + F_HS_CreditLineRechargeUSD) * F_HS_RateToUSA,2);
                }
                return 0;
            }
        }
        [JsonProperty]

        /// <summary>
        /// 币别
        /// </summary>
        public string FSettleCurrId { get; set; }
        [JsonProperty]
        /// <summary>
        /// 结算方式
        /// </summary>
        public string FSettleTypeId { get; set; }
        /// <summary>
        /// 单据明细ID
        /// </summary>
        public int FEntryId { get; set; }

        /// <summary>
        /// 是否已扣减余额 
        /// </summary>
        public bool F_HS_BalanceDeducted { get; set; }
        /// <summary>
        /// CEO特批发货
        /// </summary>
        public bool F_HS_CheckBox { get; set; }
        /// <summary>
        /// 是否需要冻结
        /// </summary>
        public bool NeedFreezed { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
        [JsonProperty]
        /// <summary>
        /// 退款方式
        /// </summary>
        public string F_HS_RefundMethod { get; set; }
        [JsonProperty]
        /// <summary>
        /// 退款类型
        /// </summary>
        public string F_HS_RefundType { get; set; }
        /// <summary>
        /// 兑美元汇率
        /// </summary>
        public decimal F_HS_RateToUSA { get; set; }

        public string F_HS_BalanceReceivableNo { get; set; }
        [JsonProperty]

        /// <summary>
        /// 变动金额（USD）
        /// </summary>
        public decimal FRealAmountFor_USD
        {
            get
            {
                if (F_HS_RateToUSA > 0)
                {
                    return Math.Round(FRealAmountFor / F_HS_RateToUSA, 2);
                }

                return 0;
            } 
        }
        [JsonProperty]
        /// <summary>
        /// 信用额度支付/充值金额USD
        /// </summary>
        public decimal F_HS_CreditLineRechargeUSD { get; set; }
        /// <summary>
        /// 余额充值金额(本币别)
        /// </summary>
        public decimal F_HS_BalanceRecharge { get; set; }
        [JsonProperty]
        /// <summary>
        /// 余额充值金额USD
        /// </summary>
        public decimal F_HS_BalanceRechargeUSD { get; set; }
        /// <summary>
        /// 订单金额
        /// </summary>
        public decimal FBillAmount { get; set; }
        public K3CustomerInfo Customer { get; set; }
        /// <summary>
        /// 客户余额
        /// </summary>
        public decimal FCustBalance { get; set; }
        /// <summary>
        /// 客户总结余
        /// </summary>
        public decimal FCustBalanceAmount { get; set; }
        [JsonProperty]
        /// <summary>
        /// 业务发生时间
        /// </summary>
        public string FBusinessTime { get; set; }
        /// <summary>
        /// 变动类型
        /// </summary>
        public string ChangedType
        {
            get
            {
                switch (this.DataType)
                {
                    case SynchroDataType.SaleOrder:
                        return "XSXF";
                    case SynchroDataType.SaleOrderOffline:
                        return "XXXF";
                    case SynchroDataType.DropShippingSalOrder:
                        return "DSXF";
                    case SynchroDataType.ReceiveBill:
                        return "SKCZ";
                    case SynchroDataType.ReFundBill:
                        return "TKCZ";
                    case SynchroDataType.Customer:
                        return "KHXYEDTZ";
                }

                return string.Empty;
            }
            set { }
        }
        /// <summary>
        /// 变动原因
        /// </summary>
        public string ChangedCause
        {
            get
            {
                switch (this.DataType)
                {
                    case SynchroDataType.SaleOrder:
                    case SynchroDataType.SaleOrderOffline:
                    case SynchroDataType.DropShippingSalOrder:
                    case SynchroDataType.ReceiveBill:
                    case SynchroDataType.ReFundBill:
                        return "DJSH";
                }
                return "DSRW";
            }
            set { }
        }

        public AbsDataInfo() { }
        public AbsDataInfo(SynchroDataType dataType)
        {
            this.DataType = dataType;
        }

    }
}
