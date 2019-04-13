using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.APP.Entity.StructType
{
    public struct RequestType
    {
        /// <summary>
        /// 改单请求
        /// </summary>
        public const string MODIFY = "1";
        /// <summary>
        /// 合单请求
        /// </summary>
        public const string COMBINE = "2";
        /// <summary>
        /// 锁单请求
        /// </summary>
        public const string LOCK = "0";
    }
}
