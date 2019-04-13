using Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.APP.Entity.SynDataObject.BacthAdjust
{
    [JsonObject(MemberSerialization.OptIn)]
    public class K3BatchAdjust: AbsSynchroDataInfo
    {
        [JsonProperty]
        /// <summary>
        /// 调价单编码
        /// </summary>
        public string FBillNo { get; set; }
        /// <summary>
        /// 批调名称
        /// </summary>
        public string FName { get; set; }
        [JsonProperty]
        /// <summary>
        /// 调价单日期
        /// </summary>
        public string FDate { get; set; }
        /// <summary>
        /// 是否已同步
        /// </summary>
        public bool F_HS_YNInSync { get; set; }
        [JsonProperty]
        /// <summary>
        /// 调价单明细
        /// </summary>
        public List<K3BatchAdjustEntry> Entry { get; set; }
    }
}
