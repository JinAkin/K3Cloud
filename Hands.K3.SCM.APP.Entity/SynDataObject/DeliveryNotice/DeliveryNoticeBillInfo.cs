using Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.APP.Entity.SynDataObject.DeliveryNotice
{
    public class DeliveryNoticeBillInfo: AbsSynchroDataInfo
    {
        /// <summary>
        /// 发货通知单号
        /// </summary>
        public string FBillNo { get; set; }
        /// <summary>
        /// 销售订单号
        /// </summary>
        public string F_HS_SaleOrder { get; set; }
        /// <summary>
        /// 物流跟踪明细
        /// </summary>
     
      
    }
}
