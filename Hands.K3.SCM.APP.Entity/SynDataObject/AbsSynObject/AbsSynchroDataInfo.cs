using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject
{
    [Serializable]
    public abstract class AbsSynchroDataInfo
    {
        [JsonIgnore]
        /// <summary>
        /// 在原系统里面的编码（编号）
        /// </summary>
        public string SrcNo{get;set;}

        [JsonIgnore]
        public string UserToken { get; set; }
        public override bool Equals(object obj)
        {
            if (obj != null)
            {
                if (obj.GetType().IsSubclassOf(typeof(AbsSynchroDataInfo)))
                {
                    AbsSynchroDataInfo info = obj as AbsSynchroDataInfo;
                    return this.SrcNo == info.SrcNo;
                }
            }
            return false;
        }

        public override int GetHashCode()
        {
            return this.SrcNo.GetHashCode();
        }
    }
}
