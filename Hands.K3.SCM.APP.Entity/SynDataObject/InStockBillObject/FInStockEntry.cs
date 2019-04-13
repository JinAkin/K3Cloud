using Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.APP.Entity.SynDataObject.InStockBillObject
{
    public class FInStockEntry : AbsSynchroDataInfo
    {
        /// <summary>
        /// 物料的ListId
        /// </summary>
        public string F_HS_ListID { get; set; }
        /// <summary>
        /// 物料编码
        /// </summary>
        public string FMaterialId { get; set; }

        /// <summary>
        /// 审核日期
        /// </summary>
        public string FApproveDate { get; set; }

    }
}
