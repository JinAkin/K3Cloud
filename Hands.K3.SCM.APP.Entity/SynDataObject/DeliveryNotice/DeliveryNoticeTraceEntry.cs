using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.APP.Entity.SynDataObject.DeliveryNotice
{
    /// <summary>
    /// 物流跟踪明细
    /// </summary>
    public class DeliveryNoticeTraceEntry
    {
        /// <summary>
        /// 物流公司
        /// </summary>
        public string FLogComId { get; set; }
        /// <summary>
        /// 物流方式
        /// </summary>
        public string F_HS_ShipMethods { get; set; }
        /// <summary>
        /// 物流单号
        /// </summary>
        public string F_HS_CarryBillNO { get; set; }
        /// <summary>
        /// 物流状态
        /// </summary>
        public string FTraceStatus { get; set; }
        /// <summary>
        /// 发货日期
        /// </summary>
        public DateTime F_HS_DeliDate { get; set; }
        /// <summary>
        /// 查询网址
        /// </summary>
        public string F_HS_QueryURL { get; set; }
        /// <summary>
        /// 物流渠道
        /// </summary>
        public string F_HS_Channel { get; set; }
        /// <summary>
        /// 最新轨迹查询日期
        /// </summary>
        public DateTime F_HS_LastTrackTime { get; set; }
        /// <summary>
        /// 是否轨迹查询
        /// </summary>
        public string F_HS_IsTrack { get; set; }

    }
}
