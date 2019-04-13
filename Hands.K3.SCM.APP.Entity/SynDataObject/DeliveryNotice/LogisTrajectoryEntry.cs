using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.APP.Entity.SynDataObject.DeliveryNotice
{
    /// <summary>
    /// 物流轨迹明细
    /// </summary>
    public class LogisTrajectoryEntry
    {
        public string FEntryID { get; set; }
        /// <summary>
        /// 签收时间
        /// </summary>
        public string F_HS_Signtime { get; set; }
        /// <summary>
        /// 轨迹信息
        /// </summary>
        public string F_HS_TrackInfo { get; set; }
        /// <summary>
        /// 地区编码
        /// </summary>
        public string F_HS_AreaCode { get; set; }
        /// <summary>
        /// 所在地
        /// </summary>
        public string F_HS_AreaName { get; set; }
        /// <summary>
        /// 轨迹状态
        /// </summary>
        public string F_HS_TarckStatus { get; set; }


    }
}
