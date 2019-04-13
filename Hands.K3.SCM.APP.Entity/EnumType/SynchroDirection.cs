using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.APP.Entity.EnumType
{
    /// <summary>
    /// 数据同步方向（读，写）
    /// </summary>
    public enum SynchroDirection
    {
        /// <summary>
        /// 默认选项
        /// </summary>
        Default,
        /// <summary>
        /// ToHC向Redis写入数据更新HC网站
        /// </summary>
        ToHC,
        /// <summary>
        /// ToK3向Redis读取数据同步到K3
        /// </summary>
        ToK3,
        /// <summary>
        /// ToB2B向Redis写数据同步到B2B网站
        /// </summary>
        ToB2B
    }
}
