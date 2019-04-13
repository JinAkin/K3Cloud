using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.APP.Entity.SynDataObject.ReceiveBillObject
{
    /// <summary>
    /// 应收票据明细
    /// </summary>
    public class BillReceiveBillEntry
    {
        /// <summary>
        /// 内部账号
        /// </summary>
        public string FInnerAccountID_B { get; set; }
        /// <summary>
        /// 票据流水号
        /// </summary>
        public string FBILLID { get; set; }
        /// <summary>
        /// 票据类型
        /// </summary>
        public string FBPBILLTYPE { get; set; }
        /// <summary>
        /// 票据号
        /// </summary>
        public string FBPBILLNUMBER { get; set; }
        /// <summary>
        /// 票面金额
        /// </summary>
        public string FBPBILLPARAMOUNT { get; set; }
        /// <summary>
        /// 可用余额
        /// </summary>
        public string FPARLEFTAMOUNTFOR { get; set; }
        /// <summary>
        /// 当前占用金额
        /// </summary>
        public string FUSEDAMOUNTFOR { get; set; }
        /// <summary>
        /// 票面金额本位币
        /// </summary>
        public string FBILLPARAMOUNT { get; set; }
        /// <summary>
        /// 可用余额本位币
        /// </summary>
        public string FPARLEFTAMOUNTSTD { get; set; }
        /// <summary>
        /// 当前占用金额本位币
        /// </summary>
        public string FUSEDAMOUNTSTD { get; set; }
        /// <summary>
        /// 到期日
        /// </summary>
        public string FBPBILLDUEDATE { get; set; }
        /// <summary>
        /// 结算状态
        /// </summary>
        public string FBPSETTLESTATUS { get; set; }
        /// <summary>
        /// 票据组织
        /// </summary>
        public string FTempOrgId { get; set; }

    }
}
