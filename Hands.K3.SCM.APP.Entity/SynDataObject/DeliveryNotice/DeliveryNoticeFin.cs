using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.APP.Entity.SynDataObject.DeliveryNotice
{
    /// <summary>
    /// 财务信息
    /// </summary>
    public class DeliveryNoticeFin
    {
        /// <summary>
        /// 结算组织
        /// </summary>
        public string FSettleOrgID { get; set; }
        /// <summary>
        /// 结算方式
        /// </summary>
        public string FSettleTypeID { get; set; }
    }
}
