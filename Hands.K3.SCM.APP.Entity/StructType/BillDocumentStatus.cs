using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.APP.Entity.StructType
{
    public struct BillDocumentStatus
    {
        /// <summary>
        /// 创建
        /// </summary>
        public static string Create = "A";
        /// <summary>
        /// 审核中
        /// </summary>
        public static string Auditing = "B";
        /// <summary>
        /// 已审核
        /// </summary>
        public static string Audit = "C";
        /// <summary>
        /// 重新审核
        /// </summary>
        public static string ReAudit = "D";
        
    }
}
