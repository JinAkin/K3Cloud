using Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.APP.Entity.SynDataObject.InStockBillObject
{
    [JsonObject(MemberSerialization.OptIn)]
    public class InStock 
    {
        /// <summary>
        /// 单据编号
        /// </summary>
        public string FBillNo { get; set; }
        [JsonProperty]
        

        public List<FInStockEntry> InStockEntry { get; set; }
    }
}
