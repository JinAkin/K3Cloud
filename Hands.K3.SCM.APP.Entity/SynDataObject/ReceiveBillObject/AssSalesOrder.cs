using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.APP.Entity.SynDataObject.ReceiveBillObject
{
    /// <summary>
    /// 关联销售订单
    /// </summary>
    public class AssSalesOrder
    {
        /// <summary>
        /// 关联单据ID
        /// </summary>
        public string FASSBILLID { get; set; }
        /// <summary>
        /// 关联单据编号
        /// </summary>
        public string FASSBILLNO { get; set; }
        /// <summary>
        /// 关联金额
        /// </summary>
        public decimal FASSAMOUNTFOR { get; set; }
        /// <summary>
        /// 预付预收已核销金额
        /// </summary>
        public decimal FPREMATCHAMOUNTFOR { get; set; }
    }
}
