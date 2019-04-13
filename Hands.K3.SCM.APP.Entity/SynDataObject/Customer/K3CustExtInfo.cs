using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.APP.Entity.SynDataObject
{
    public class K3CustExtInfo
    {
        /// <summary>
        /// 启用商联在线
        /// </summary>
        public bool FEnableSL { get; set; }
        /// <summary>
        /// 冻结范围
        /// </summary>
        public string FFreezeLimit { get; set; }
        /// <summary>
        /// 冻结人
        /// </summary>
        public string FFreezeOperator { get; set; }
        /// <summary>
        /// 冻结日期
        /// </summary>
        public DateTime FFreezeDate { get; set; }
        /// <summary>
        /// 省份
        /// </summary>
        public string FPROVINCE { get; set; }
        /// <summary>
        /// 城市
        /// </summary>
        public string FCITY { get; set; }
        /// <summary>
        /// 默认收货地点
        /// </summary>
        public string FDefaultConsiLoc { get; set; }
        /// <summary>
        /// 默认开票地点
        /// </summary>
        public string FDefaultSettleLoc { get; set; }
        /// <summary>
        /// 默认付款地点
        /// </summary>
        public string FDefaultPayerLoc { get; set; }
        /// <summary>
        /// 默认联系人
        /// </summary>
        public string FDefaultContact { get; set; }

    }
}
