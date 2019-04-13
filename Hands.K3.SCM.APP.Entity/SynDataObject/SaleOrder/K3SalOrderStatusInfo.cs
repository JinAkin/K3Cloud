using Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.APP.Entity.SynDataObject
{
    public class K3SalOrderStatusInfo: AbsSynchroDataInfo
    {
        /// <summary>
        /// 销售订单编码
        /// </summary>
        public string BillNo { get; set; }
        /// <summary>
        /// 销售订单关闭状态
        /// </summary>
        public string CloseStatus { get; set; }
        /// <summary>
        /// 销售订单作废状态
        /// </summary>
        public string CancelStatus { get; set; }
        /// <summary>
        /// 系统自动关闭
        /// </summary>
        public string ShipStatus { get; set; }
        /// <summary>
        /// 销售订单付款状态
        /// </summary>
        public string PaymentStatus { get; set; }
        /// <summary>
        /// 结算方式
        /// </summary>
        public string F_HS_PaymentMode { get; set; }

        public K3SalOrderStatusInfo() { }      
        
    }
}
