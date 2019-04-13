using Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.APP.Entity.SynDataObject.SalOutStock
{
    [Serializable]
    public class SalOutStockBillInfo: AbsSynchroDataInfo
    {
        public SalOutStockBillInfo()
        {
            StockEntry = new List<SalOutStockEntry>();
        }
        
        /// <summary>
        /// 销售出库单编码
        /// </summary>
        public string BillNo { get; set; }
        /// <summary>
        /// 销售订单编码
        /// </summary>
        public string OrderBillNo { get; set; }
        /// <summary>
        /// 客户编码
        /// </summary>
        public string CustomerNo { get; set; }
        [JsonIgnore]
        /// <summary>
        /// 结算币别
        /// </summary>
        public string SettleCurrNo { get; set; }
        [JsonIgnore]
        /// <summary>
        /// 收货人
        /// </summary>
        public string DeliveryName { get; set; }
        [JsonIgnore]
        /// <summary>
        /// 收货地址
        /// </summary>
        public string DeliveryAddress { get; set; }
        [JsonIgnore]
        /// <summary>
        /// 付款方式
        /// </summary>
        public string SettleType{get;set;}
        /// <summary>
        ///运输单号
        /// </summary>
        public string CarriageNO { get; set; }
        /// <summary>
        /// 发货日期
        /// </summary>
        public string DeliDate { get; set; }
        /// <summary>
        /// 物流渠道
        /// </summary>
        public string LogisticsChannel { get; set; }
        /// <summary>
        /// 查询网址
        /// </summary>
        public string QueryURL { get; set; }
        [JsonIgnore]
        /// <summary>
        /// 是否已返积分
        /// </summary>
        public bool F_HS_ReturnedIntegral { get; set; }
        /// <summary>
        /// 明细信息
        /// </summary>
        public List<SalOutStockEntry> StockEntry { get; set; }
     
   
    }
}
