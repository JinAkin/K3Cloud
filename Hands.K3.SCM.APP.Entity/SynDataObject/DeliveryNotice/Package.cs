using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.APP.Entity.SynDataObject.DeliveryNotice
{
    public class Package
    {
        /// <summary>
        /// 包裹序列号
        /// </summary>
        public string SequenceNumber { get; set; }
        /// <summary>
        /// 包裹重量
        /// </summary>
        public decimal Weight { get; set; }
        /// <summary>
        /// 包裹重量单位
        /// </summary>
        public string Units { get; set; }
        /// <summary>
        /// 包裹规格
        /// </summary>
        public Dimension Dimension { get; set; }
    }
}
