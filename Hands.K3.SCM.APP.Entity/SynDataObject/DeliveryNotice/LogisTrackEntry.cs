using Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.APP.Entity.SynDataObject.DeliveryNotice
{
    /// <summary>
    ///物流跟踪
    /// </summary>
    public class LogisTrackEntry: AbsSynchroDataInfo
    {
        public string FEntryID { get; set; }
        /// <summary>
        /// 发货通知单号
        /// </summary>
        public string FBillNo { get; set; }
        /// <summary>
        /// 销售订单号
        /// </summary>
        public string F_HS_SaleOrder { get; set; }
        /// <summary>
        /// 发货日期
        /// </summary>
        public string F_HS_DELIDATE { get; set; }
        /// <summary>
        /// 运单号
        /// </summary>
        public string F_HS_CARRYBILLNO { get; set; }
        /// <summary>
        /// 最新轨迹状态
        /// </summary>
        public string F_HS_LatestTrajectory { get; set; }
        /// <summary>
        /// 是否已完成轨迹同步
        /// </summary>
        public bool F_HS_YNCompleteTrajectory { get; set; }
        /// <summary>
        /// 物流轨迹明细
        /// </summary>
        public List<LogisTrajectoryEntry> TrajectoryEntry { get; set; }
    }
}
