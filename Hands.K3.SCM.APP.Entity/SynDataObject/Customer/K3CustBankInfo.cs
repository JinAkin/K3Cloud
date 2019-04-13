using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.APP.Entity.SynDataObject
{
    public class K3CustBankInfo
    {
        /// <summary>
        /// 单据明细ID
        /// </summary>
        public int FENTRYID { get; set; }
        /// <summary>
        /// 开户国家
        /// </summary>
        public string FCOUNTRY1 { get; set; }
        /// <summary>
        /// 开户银行
        /// </summary>
        public string FOPENBANKNAME { get; set; }
        /// <summary>
        /// 银行账号
        /// </summary>
        public string FBANKCODE { get; set; }
        /// <summary>
        /// 账户名称
        /// </summary>
        public string FACCOUNTNAME { get; set; }
        /// <summary>
        /// 收款银行
        /// </summary>
        public string FBankTypeRec { get; set; }
        /// <summary>
        /// 开户行地址
        /// </summary>
        public string FOpenAddressRec { get; set; }
        /// <summary>
        /// 默认
        /// </summary>
        public bool FISDEFAULT1 { get; set; }
        /// <summary>
        /// 联行号
        /// </summary>
        public string FCNAPS { get; set; }
        /// <summary>
        /// 币别
        /// </summary>
        public string FCURRENCYID { get; set; }
 
    }
}
