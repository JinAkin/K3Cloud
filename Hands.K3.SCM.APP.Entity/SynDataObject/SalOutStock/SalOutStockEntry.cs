using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.APP.Entity.SynDataObject.SalOutStock
{
    [Serializable]
    public class SalOutStockEntry
    {
      
        /// <summary>
        /// 物料编码
        /// </summary>
        public string MaterialNo { get; set; }
        [JsonIgnore]
        /// <summary>
        /// 仓库
        /// </summary>
        public string StockNo { get; set; }
        /// <summary>
        /// 仓库名称
        /// </summary>
        public string StockName { get; set; }
        /// <summary>
        /// 物料数量
        /// </summary>
        public decimal Quantity { get; set; }
        /// <summary>
        /// 商品单价
        /// </summary>
        public decimal Price { get; set; }
        [JsonIgnore]
        /// <summary>
        /// 积分返点率
        /// </summary>
        public decimal IntegralReturnRate { get; set; }


    }
}
