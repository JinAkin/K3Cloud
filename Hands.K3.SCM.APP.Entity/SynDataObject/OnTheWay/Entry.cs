using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.APP.Entity.SynDataObject.OnTheWay
{
    public class OnTheWayEntry
    {
        /// <summary>
        /// 地理仓编码
        /// </summary>
        public string FStockId { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public decimal FQty { get; set; }
        /// <summary>
        /// 交货日期
        /// </summary>
        public string FDeliveryDate { get; set; }
    }
}
