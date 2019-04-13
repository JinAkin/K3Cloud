using Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.APP.Entity.SynDataObject.DeliveryNotice
{
    /// <summary>
    /// 发货通知单
    /// </summary>
    public class DeliveryNotice: K3SalOrderInfo
    {
        /// <summary>
        /// 出货表编码
        /// </summary>
        public HashSet<string> FShipmentBillNo { get; set; }
        /// <summary>
        /// 单据类型
        /// </summary>
        public string FBillTypeID { get; set; }     
        /// <summary>
        /// 发货组织
        /// </summary>
        public string FDeliveryOrgID { get; set;}
        /// <summary>
        /// 发货日期
        /// </summary>
        public DateTime F_HS_DELIDATE { get; set; }
        /// <summary>
        /// 重量合计
        /// </summary>
        public decimal F_HS_AllTotalWeight { get; set; }
        /// <summary>
        /// 申报金额
        /// </summary>
        public decimal FDecAmount { get; set; }
        /// <summary>
        /// 明细信息
        /// </summary>
        public List<DeliveryNoticeEntry> Entry { get; set; }
        /// <summary>
        /// 财务信息
        /// </summary>
        public DeliveryNoticeFin DelNotFin { get; set; }
        /// <summary>
        /// 轨迹明细信息
        /// </summary>
        public List<DeliveryNoticeLocusEntry> LocusEntry { get; set; }
        /// <summary>
        /// 物流跟踪明细
        /// </summary>
        public List<DeliveryNoticeTraceEntry> TraceEntry { get; set; }
        /// <summary>
        /// 包裹
        /// </summary>
        public List<Package> Packages { get; set; }

        public DeliveryNotice()
        {
            FShipmentBillNo = new HashSet<string>();
            LocusEntry = new List<DeliveryNoticeLocusEntry>();
            TraceEntry = new List<DeliveryNoticeTraceEntry>();
            Packages = new List<Package>();
        }

    }
}
