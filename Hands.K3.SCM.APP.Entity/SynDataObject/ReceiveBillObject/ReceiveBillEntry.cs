using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.APP.Entity.SynDataObject.ReceiveBillObject
{
    /// <summary>
    /// 收款单明细
    /// </summary>
    public class ReceiveBillEntry
    {

        public int FEntryID { get; set; }
        /// <summary>
        /// 结算方式
        /// </summary>
        public string FSETTLETYPEID { get; set; }
        ///// <summary>
        ///// 收款用途
        ///// </summary>
        //public string FPURPOSEID { get; set; }
        ///// <summary>
        ///// 预收项目类型
        ///// </summary>
        //public string FRECEIVEITEMTYPE { get; set; }
        ///// <summary>
        ///// 销售订单
        ///// </summary>
        //public string FRECEIVEITEM { get; set; }
        ///// <summary>
        ///// 销售订单号
        ///// </summary>
        //public string FSaleOrderID { get; set; }
        ///// <summary>
        ///// 销售单号（查款）
        ///// </summary>
        //public string F_HS_OrderNumCheck { get; set; }
        ///// <summary>
        ///// 应收金额
        ///// </summary>
        //public decimal FRECTOTALAMOUNTFOR { get; set; }
        ///// <summary>
        ///// 收款金额
        ///// </summary>
        //public decimal FRECAMOUNTFOR_E { get; set; }
        ///// <summary>
        ///// 现金折扣
        ///// </summary>
        //public decimal FSETTLEDISTAMOUNTFOR { get; set; }
        ///// <summary>
        ///// 折后金额
        ///// </summary>
        //public decimal FSETTLERECAMOUNTFOR { get; set; }
        ///// <summary>
        ///// 手续费
        ///// </summary>
        //public decimal FHANDLINGCHARGEFOR { get; set; }
        ///// <summary>
        ///// 长短款
        ///// </summary>
        //public decimal FOVERUNDERAMOUNTFOR{get;set;}
        ///// <summary>
        ///// 我方银行账号
        ///// </summary>
        //public string FACCOUNTID { get; set; }
        ///// <summary>
        ///// 我方账户名称
        ///// </summary>
        //public string FRECACCOUNTNAME { get; set; }

        //public string FOPPOSITEBANKACCOUNT { get; set; }
        ///// <summary>
        ///// 对方账户名称
        ///// </summary>
        //public string FOPPOSITECCOUNTNAME { get; set; }
        ///// <summary>
        ///// 内部账号
        ///// </summary>
        //public string FINNERACCOUNTID { get; set; }
        ///// <summary>
        ///// 内部账户名称
        ///// </summary>
        //public string FINNERACCOUNTNAME { get; set; }
        ///// <summary>
        ///// 现金账号
        ///// </summary>
        //public string FCashAccount { get; set; }
        ///// <summary>
        ///// 结算号
        ///// </summary>
        //public string FSETTLENO { get; set; }
        ///// <summary>
        ///// 我方开户行
        ///// </summary>
        //public string FRECBANKID { get; set; }
        ///// <summary>
        ///// 对方开户行
        ///// </summary>
        //public string FOPPOSITEBANKNAME { get; set; }
        ///// <summary>
        ///// 备注
        ///// </summary>
        //public string FCOMMENT { get; set; }
        ///// <summary>
        ///// 表体明细-核销状态
        ///// </summary>
        //public string FWRITTENOFFSTATUS_D { get; set; }
        ///// <summary>
        ///// 表体明细-已核销金额
        ///// </summary>
        //public decimal FWRITTENOFFAMOUNTFOR_D { get; set; }
        ///// <summary>
        ///// 勾对
        ///// </summary>
        //public bool FBLEND { get; set; }
        ///// <summary>
        ///// 表体-应收金额本位币
        ///// </summary>
        //public decimal FRECTOTALAMOUNT { get; set; }
        ///// <summary>
        ///// 长短款本位币
        ///// </summary>
        //public decimal FOVERUNDERAMOUNT { get; set; }
        ///// <summary>
        ///// 手续费本位币
        ///// </summary>
        //public decimal FHANDLINGCHARGE { get; set; }
        ///// <summary>
        ///// 关联总金额
        ///// </summary>
        //public string FASSTOTALAMOUNTFOR { get; set; }
        /// <summary>
        /// 表体-实收金额
        /// </summary>
        public decimal FREALRECAMOUNTFOR_D { get; set; }
        /// <summary>
        /// 表体-实收金额本位币
        /// </summary>
        //public decimal FREALRECAMOUNT_D { get; set; }
        ///// <summary>
        ///// 退款关联金额
        ///// </summary>
        //public decimal FReFundAmount { get; set; }
        ///// <summary>
        ///// 折后金额本位币
        ///// </summary>
        //public decimal FSETTLERECAMOUNT { get; set; }
        ///// <summary>
        ///// 现金折扣本位币
        ///// </summary>
        //public decimal FSETTLEDISTAMOUNT { get; set; }
        ///// <summary>
        ///// 收款金额本位币
        ///// </summary>
        //public decimal FRECAMOUNT_E { get; set; }
        ///// <summary>
        ///// 登账日期
        ///// </summary>
        //public DateTime FPOSTDATE { get; set; }
        ///// <summary>
        ///// 是否登账
        ///// </summary>
        //public bool FISPOST { get; set; }
        ///// <summary>
        ///// 物料编码
        ///// </summary>
        //public string FMATERIALID { get; set; }
        ///// <summary>
        ///// 物料名称
        ///// </summary>
        //public string FMATERIALNAME { get; set; }
        ///// <summary>
        ///// 销售订单号
        ///// </summary>
        //public string FSALEORDERNO { get; set; }
        ///// <summary>
        ///// 订单行号
        ///// </summary>
        //public string FMATERIALSEQ { get; set; }
        ///// <summary>
        ///// 销售订单明细内码
        ///// </summary>
        //public string FORDERENTRYID { get; set; }
        /// <summary>
        /// 是否充值
        /// </summary>
        public bool F_HS_YNRecharge { get; set; }

        public bool F_HS_SynchronizedRecharge { get; set; }
        /// <summary>
        /// 信用额度充值金额USD
        /// </summary>
        public decimal F_HS_CreditLineRechargeUSD { get; set; }
        /// <summary>
        /// 余额充值金额USD
        /// </summary>
        public decimal F_HS_BalanceRechargeUSD { get; set; }
    }
}
