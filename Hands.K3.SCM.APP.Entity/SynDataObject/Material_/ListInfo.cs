using Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.APP.Entity.SynDataObject.Material_
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ListInfo: AbsSynchroDataInfo
    {
        [JsonProperty]
        /// <summary>
        /// 物料的ListId
        /// </summary>
        public string F_HS_ListID { get; set; }
        [JsonProperty]
        /// <summary>
        /// 物料的ListName
        /// </summary>
        public string F_HS_ListName { get; set; }
        ///// <summary>
        ///// 毛重
        ///// </summary>
        //public decimal F_HS_GROSSWEIGHT { get; set; }
        /// <summary>
        /// 不覆盖物料名称
        /// </summary>
        public bool F_HS_NotCoverMaterialName { get; set; }
        [JsonProperty]
        /// <summary>
        /// 物料明细
        /// </summary>
        public List<Material> Materials { get; set; }
    }
}
