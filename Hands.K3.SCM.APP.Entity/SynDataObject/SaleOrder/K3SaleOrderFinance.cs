using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.APP.Entity.SynDataObject.SaleOrder
{
    /// <summary>
    /// 销售订单财务信息
    /// </summary>
    public class K3SaleOrderFinance
    {
        /// <summary>
        /// 结算币别
        /// </summary>
        public string FSettleCurrID { get; set; }
        /// <summary>
        /// 收款条件
        /// </summary>
        public string FRecConditionId { get; set; }
        /// <summary>
        /// 是否含税
        /// </summary>
        public bool FIsIncludedTax { get; set; }
        /// <summary>
        /// 结算方式
        /// </summary>
        public string FSettleModeId { get; set; }
        /// <summary>
        /// 价外税
        /// </summary>
        public string FIsPriceExcludeTax { get; set; }
        /// <summary>
        /// 价目表
        /// </summary>
        public string FPriceListId { get;set; }
        /// <summary>
        /// 收款单号
        /// </summary>
        public string FRecBillId { get; set; }
        /// <summary>
        /// 折扣表
        /// </summary>
        public decimal FDiscountListId { get; set; }
        /// <summary>
        /// 税额
        /// </summary>
        public decimal FBillTaxAmount { get; set; }
        /// <summary>
        /// 金额
        /// </summary>
        public decimal FBillAmount { get; set; }
        /// <summary>
        /// 价税合计
        /// </summary>
        public decimal FBillAllAmount { get; set; }
        /// <summary>
        /// 关联应收金额（订单）
        /// </summary>
        public decimal FJoinOrderAmount { get; set; }
        /// <summary>
        /// 关联应收金额（出库
        /// </summary>
        public decimal FJoinStockAmount { get; set; }
        /// <summary>
        /// 本位币
        /// </summary>
        public string FLocalCurrId { get; set; }
        /// <summary>
        /// 需要预收
        /// </summary>
        public string FExchangeTypeId { get; set; }
        /// <summary>
        /// 汇率
        /// </summary>
        public decimal F_HS_RateToUSA { get; set; }
        /// <summary>
        /// 预收金额
        /// </summary>
        public decimal FPayAdvanceAmount {get;set;}
        /// <summary>
        /// 预收比例%
        /// </summary>
        public decimal FPayAdvanceRate { get; set; }
        /// <summary>
        /// 税额（本位币）
        /// </summary>
        public decimal FBillTaxAmount_LC { get; set; }
        /// <summary>
        /// 价税合计（本位币）
        /// </summary>
        public decimal FBillAllAmount_LC { get; set; }
        /// <summary>
        /// 金额（本位币）
        /// </summary>
        public decimal FBillAmount_LC { get; set; }
        /// <summary>
        /// 工作流信用检查状态
        /// </summary>
        public string FCreChkStatus { get; set; }
        /// <summary>
        /// 审批流信用压批月结检查
        /// </summary>
        public string FCrePreBatAndMonStatus { get; set; }
        /// <summary>
        /// 工作流信用超标天数
        /// </summary>
        public int FCreChkDays { get; set; }
        /// <summary>
        /// 工作流信用超标金额
        /// </summary>
        public decimal FCreChkAmount { get; set; }
        /// <summary>
        /// 信用压批超标
        /// </summary>
        public string FCrePreBatchOver { get; set; }
        /// <summary>
        /// 信用月结超标
        /// </summary>
        public string FCreMonControlOver { get; set; }

    }
}
