using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.APP.Entity.StructType
{
    public struct BillCancelStatus
    {
        /// <summary>
        /// 未作废
        /// </summary>
        public static string NonCancel = "A";
        /// <summary>
        /// 已作废
        /// </summary>
        public static string Cancel = "B";
    }
}
