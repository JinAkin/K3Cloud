using Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject;
using Hands.K3.SCM.APP.Entity.SynDataObject.ReceiveBillObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.APP.Entity.SynDataObject.ReFundBillObject
{
    public class ReFundBill: AbsSynchroDataInfo
    {
        public ReFundBill()
        {
            this.Entry = new List<ReFundBillEntry>();
        }
        /// <summary>
        /// 单据编号
        /// </summary>
        public string FBillNo { get; set; }
        /// <summary>
        /// 客户真是ID
        /// </summary>
        public string F_HS_B2CCustId { get; set; }
        /// <summary>
        /// 单据日期
        /// </summary>
        public DateTime FDate { get; set; }
        /// <summary>
        /// 结算币别
        /// </summary>
        public string FSettleCurrId { get; set; }
        /// <summary>
        /// 退款方式
        /// </summary>
        public string F_HS_RefundMethod { get; set; }
        /// <summary>
        /// 退款类型
        /// </summary>
        public string F_HS_RefundType { get; set; }
        /// <summary>
        /// 退款金额
        /// </summary>
        public decimal FReFundAmountFor_H{ get; set; }
        /// <summary>
        /// 实退金额
        /// </summary>
        public decimal FRealReFundAmountFor { get; set; }
        /// <summary>
        /// 是否已同步
        /// </summary>
        public bool F_HS_YNSync { get; set; }
        /// <summary>
        /// 退款单明细
        /// </summary>
        public List<ReFundBillEntry> Entry { get; set; }
    }
}
