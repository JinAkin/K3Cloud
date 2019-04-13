using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.APP.Entity.SynDataObject.ReceiveBillObject
{
    /// <summary>
    /// 应收票据背书
    /// </summary>
    public class SkdRecEntry
    {
        /// <summary>
        /// 内部账户
        /// </summary>
        public string FInnerActId { get; set; }
        /// <summary>
        /// 票据流水号
        /// </summary>
        public string FReceivebleBillId { get; set; }
        /// <summary>
        /// 付款用途
        /// </summary>
        public string FPayPurse { get; set; }
        /// <summary>
        /// 背书退回金额
        /// </summary>
        public decimal FReturnAmount { get; set; }
        /// <summary>
        /// 背书退回金额本位币
        /// </summary>
        public string FReturnAmountStd { get; set; }
        /// <summary>
        /// 票据类型
        /// </summary>
        public string FKDBPARBILLTYPE { get; set; }
        /// <summary>
        /// 票据号
        /// </summary>
        public string FKDBPARBILLNO { get; set; }
        /// <summary>
        /// 客户
        /// </summary>
        public string FKDBPCUSTOMER { get; set; }
        /// <summary>
        /// 票面金
        /// </summary>
        public decimal FParAmount { get; set; }
        /// <summary>
        /// 票面金额本位币
        /// </summary>
        public string FPARAMOUNTSTD { get; set; }
        /// <summary>
        /// 往来单位类型
        /// </summary>
        public string FBCONTACTUNITTYPE { get; set; }
        /// <summary>
        /// 往来单位
        /// </summary>
        public string FBCONTACTUNIT { get; set; }
    }
}
