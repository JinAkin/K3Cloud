using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.APP.Entity.SynDataObject.DeliveryNotice
{
    public class Dimension
    {
        /// <summary>
        /// 包裹长度
        /// </summary>
        public string Length { get; set; }
        /// <summary>
        ///包裹宽度
        /// </summary>
        public string Width { get; set; }
        /// <summary>
        /// 包裹高度
        /// </summary>
        public string Height { get; set; }
        /// <summary>
        /// 单位
        /// </summary>
        public string Units { get; set; }
    }
}
