using Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.APP.Entity.SynDataObject.OnTheWay
{
    public class OnTheWay: AbsSynchroDataInfo
    {
        /// <summary>
        /// 物料编码
        /// </summary>
        public string FMaterialId { get; set; }
        /// <summary>
        /// 采购，调拨在途明细
        /// </summary>
        public List<OnTheWayEntry> Entry { get; set; }
    }
}
