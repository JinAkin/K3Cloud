using Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.APP.Entity.SynDataObject
{
    public class InventoryInfo : AbsSynchroDataInfo
    {
        /// <summary>
        /// 地理仓编码
        /// </summary>
        public string StockId { get; set; }
        /// <summary>
        /// 物料编码
        /// </summary>
        public string FixId { get; set; }
        /// <summary>
        /// 可用库存数量
        /// </summary>
        public double Quantity { get; set; }

        public InventoryInfo() { }
        public InventoryInfo(string fixId, string stockId, double quantity)
        {
            this.FixId = fixId;
            this.StockId = stockId;
            this.Quantity = quantity;
        }
    }
}
