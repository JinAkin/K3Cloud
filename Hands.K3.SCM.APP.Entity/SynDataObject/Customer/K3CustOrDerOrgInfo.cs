using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.APP.Entity.SynDataObject
{
    /// <summary>
    /// 订货组织
    /// </summary>
    public class K3CustOrderOrgInfo
    {
        /// <summary>
        /// 单据体ID
        /// </summary>
        public int FEntryID { get; set; }
        /// <summary>
        /// 订货组织
        /// </summary>
        public string FOrderOrgId { get; set; }
        /// <summary>
        /// 默认
        /// </summary>
        public bool FIsDefaultOrderOrg { get; set; }
    }
}
