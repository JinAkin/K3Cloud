using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.APP.Entity.SynDataObject.Shipment
{
    public class Shipment
    {
        /// <summary>
        /// 单据编号
        /// </summary>
        public string FBillNo { get; set; }
        /// <summary>
        /// 单据状态
        /// </summary>
        public string FDocumentStatus { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>
        public string FCreatorId { get; set; }
        /// <summary>
        /// 创建日期
        /// </summary>
        public DateTime FCreateDate { get; set; }
        /// <summary>
        /// 计费重
        /// </summary>
        public decimal FBillingWeight { get; set; }
        /// <summary>
        /// 是否下单
        /// </summary>
        public bool FIsPlaceOrder { get; set; }
        /// <summary>
        /// 出货号
        /// </summary>
        public string FOrderNo { get; set; }
        /// <summary>
        /// 下单日期
        /// </summary>
        public DateTime FPlaceOrderTime { get; set; }
        /// <summary>
        /// 下单结果
        /// </summary>
        public string FPlaceOrderResult { get; set; }
        /// <summary>
        /// 申报金额
        /// </summary>
        public decimal FDecAmount { get; set; }
        /// <summary>
        /// 出货表明细
        /// </summary>
        public List<ShipmentEntry> Entry { get; set; }
    }
}
