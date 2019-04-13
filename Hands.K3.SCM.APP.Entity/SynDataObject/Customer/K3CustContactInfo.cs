using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.APP.Entity.SynDataObject
{
    public class K3CustContactInfo:K3CustomerInfo
    {
        public string FCustNo { get; set; }
        /// <summary>
        /// 单据明细ID
        /// </summary>
        public int FENTRYID { get; set; }
        /// <summary>
        /// 地点编码
        /// </summary>
        public string FNUMBER1 { get; set; }
        /// <summary>
        /// 地点名称
        /// </summary>
        public string FNAME1 { get; set; }
        /// <summary>
        /// 详细地址
        /// </summary>
        public string FADDRESS1 { get; set; }
        /// <summary>
        /// 运输提前期
        /// </summary>
        public DateTime FTRANSLEADTIME1 { get; set; }
        /// <summary>
        /// 固定电话
        /// </summary>
        public string FTTel { get; set; }
        /// <summary>
        /// 移动电话
        /// </summary>
        public string FMOBILE { get; set; }
        /// <summary>
        /// 电子邮箱
        /// </summary>
        public string FEMail { get; set; }
        /// <summary>
        /// 默认收货地址
        /// </summary>
        public bool FIsDefaultConsignee { get; set; }
        /// <summary>
        /// 默认开票地址
        /// </summary>
        public bool FIsDefaultSettle { get; set; }
        /// <summary>
        /// 默认付款地址
        /// </summary>
        public bool FIsDefaultPayer { get; set; }
        /// <summary>
        /// 启用
        /// </summary>
        public bool FIsUsed { get; set; }
        /// <summary>
        /// 交货联系人
        /// </summary>
        public string F_HS_DeliveryName { get; set; }
        /// <summary>
        /// 交货邮编
        /// </summary>
        public string F_HS_PostCode { get; set; }
        /// <summary>
        /// 交货城市
        /// </summary>
        public string F_HS_DeliveryCity { get; set; }
        /// <summary>
        /// 交货省份/州
        /// </summary>
        public string F_HS_DeliveryProvinces { get; set; }
        /// <summary>
        /// 国家
        /// </summary>
        public string F_HS_RecipientCountry { get; set; }
    }
}
