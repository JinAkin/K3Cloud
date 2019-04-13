using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.APP.Entity.SynDataObject.DeliveryNotice
{
    /// <summary>
    /// 发货通知单明细
    /// </summary>
    public class DeliveryNoticeEntry
    {
        /// <summary>
        /// 物料编码
        /// </summary>
        public string FMaterialID { get; set; }
        /// <summary>
        /// 计价单位
        /// </summary>
        public string FUnitID { get; set; }
        /// <summary>
        /// 基本单位
        /// </summary>
        public string FBaseUnitID { get; set; }
        /// <summary>
        /// 超发控制单位类型
        /// </summary>
        public string FOutLmtUnit { get; set; }
    }
}
