using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.APP.Entity.SynDataObject.ReFundBillObject
{
    /// <summary>
    /// 退款单明细
    /// </summary>
    public class ReFundBillEntry
    {

        public int FEntryId { get; set; }
        /// <summary>
        /// 实退金额
        /// </summary>
        public decimal FRealReFundAmountFor_D{ get; set; }
        /// <summary>
        /// 结算方式
        /// </summary>
        public string FSettleTypeId { get; set; }
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
