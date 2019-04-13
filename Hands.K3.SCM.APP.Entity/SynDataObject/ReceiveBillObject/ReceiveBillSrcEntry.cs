using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.APP.Entity.SynDataObject.ReceiveBillObject
{
    /// <summary>
    /// 收款单源单明细
    /// </summary>
    public class ReceiveBillSrcEntry
    {
        /// <summary>
        /// 源单类型
        /// </summary>
        public string FSRCBILLTYPEID { get; set; }
        /// <summary>
        /// 源单编号
        /// </summary>
        public string FSRCBILLNO { get;set;}
        /// <summary>
        /// 源单币别
        /// </summary>
        public string FSRCCURRENCYID { get;set;}
        /// <summary>
        /// 到期日
        /// </summary>
        public DateTime FEXPIRY { get; set; }
        /// <summary>
        /// 应收金额
        /// </summary>
        public decimal FAFTTAXTOTALAMOUNT { get; set; }
        /// <summary>
        /// 计划收款金额
        /// </summary>
        public string FPLANRECAMOUNT { get; set; }
        /// <summary>
        /// 结算方式
        /// </summary>
        public string FSRCSETTLETYPEID { get; set; }
        /// <summary>
        /// 本次收款金额
        /// </summary>
        public string FREALRECAMOUNT { get; set; }
        /// <summary>
        /// 源单内码
        /// </summary>
        public string FSRCBILLID { get; set; }
        /// <summary>
        /// 源单行号
        /// </summary>
        public string FSRCSEQ { get; set; }
        /// <summary>
        /// 源单行内码
        /// </summary>
        public string FSRCROWID { get; set; }
        /// <summary>
        /// 销售订单号
        /// </summary>
        public string FORDERBILLNO { get; set; }
        /// <summary>
        /// 物料编码
        /// </summary>
        public string FSRCMATERIALID { get; set; }
        /// <summary>
        /// 物料名称
        /// </summary>
        public string FSRCMATERIALNAME { get; set; }
        /// <summary>
        /// 订单行号
        /// </summary>
        public string FSRCMATERIALSEQ { get; set; }
        /// <summary>
        /// 销售订单明细内码
        /// </summary>
        public string FSRCORDERENTRYID { get; set; }


    }
}
