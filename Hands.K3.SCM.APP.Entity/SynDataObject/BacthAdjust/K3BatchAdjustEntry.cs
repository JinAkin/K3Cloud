using Hands.K3.SCM.APP.Entity.SynDataObject.Material_;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.APP.Entity.SynDataObject.BacthAdjust
{
    [JsonObject(MemberSerialization.OptIn)]
    public class K3BatchAdjustEntry
    {
        public int FEntryId { get; set; }
        [JsonProperty]
        /// <summary>
        /// 物料编码
        /// </summary>
        public string FMaterialId { get;set; }

        public string F_HS_IsOil { get; set; }
        public string F_HS_PRODUCTSTATUS { get; set; }

        public string F_HS_DropShipOrderPrefix { get; set; }
        [JsonProperty]
        /// <summary>
        /// 调价类型
        /// </summary>
        public string FAdjustType { get; set; }
        [JsonProperty]
        /// <summary>
        /// 价格等级
        /// </summary>
        public string FPriceLevel { get; set; }
        /// <summary>
        /// 价目表
        /// </summary>
        public string FPriceListId { get; set; }
        ///// <summary>
        ///// 适用国家
        ///// </summary>
        //public string F_HS_ApplicableState { get; set; }
        /// <summary>
        /// 调前单价
        /// </summary>
        public string FBeforePrice { get; set; }
        [JsonProperty]
        /// <summary>
        /// 调后单价
        /// </summary>
        public string FAfterPrice { get; set; }
        [JsonProperty]
        /// <summary>
        /// 币别
        /// </summary>
        public string FCurrencyId { get; set; }
        /// <summary>
        /// 调前美国包邮单价
        /// </summary>
        public string F_HS_BeforeUSPrice { get; set; }
        /// <summary>
        /// 调前美国不包邮单价
        /// </summary>
        public string F_HS_BeforeUSNoPostagePrice { get; set; }
        [JsonProperty]
        /// <summary>
        /// 调后美国包邮单价
        /// </summary>
        public string F_HS_AfterUSPrice { get; set; }
        [JsonProperty]
        /// <summary>
        /// 调后美国不包邮单价
        /// </summary>
        public string F_HS_AfterUSNoPostagePrice { get; set; }
        /// <summary>
        /// 调前澳洲包邮单价
        /// </summary>
        public string F_HS_BeforeAUPrice { get; set; }
        /// <summary>
        /// 调前澳洲不包邮单价
        /// </summary>
        public string F_HS_BeforeAUNoPostagePrice { get; set; }
        [JsonProperty]
        /// <summary>
        /// 调后澳洲包邮单价
        /// </summary>
        public string F_HS_AfterAUPrice { get; set; }
        [JsonProperty]
        /// <summary>
        /// 调后澳洲不包邮单价
        /// </summary>
        public string F_HS_AfterAUNoPostagePrice { get; set; }
        
        /// <summary>
        /// 欧洲调后单价
        /// </summary>
        public string F_HS_AfterEUPrice { get; set; }
        /// <summary>
        /// 调前欧洲不包邮单
        /// </summary>
        public string F_HS_BeforeEUNoPostagePrice { get; set; }
        [JsonProperty]
        /// <summary>
        /// 调后欧洲不包邮单价
        /// </summary>
        public string F_HS_AfterEUNoPostagePrice { get; set; }
        /// <summary>
        /// 调前韩国不包邮单价
        /// </summary>
        public string F_HS_BeforeKRNoPostagePrice { get; set; }
        [JsonProperty]
        /// <summary>
        /// 调后韩国不包邮单价
        /// </summary>
        public string F_HS_AfterKRNoPostagePrice { get; set; }
        /// <summary>
        /// 调前日本不包邮单价
        /// </summary>
        public string F_HS_BeforeJPNoPostagePrice { get; set; }
        [JsonProperty]
        /// <summary>
        /// 调后日本不包邮单价
        /// </summary>
        public string F_HS_AfterJPNoPostagePrice { get; set; }
        /// <summary>
        /// 调前英国不包邮单价
        /// </summary>
        public string F_HS_BeforeUKNoPostagePrice { get; set; }
        
        /// <summary>
        /// 调后英国不包邮单价
        /// </summary>
        public string F_HS_AfterUKNoPostagePrice { get; set; }
        /// <summary>
        /// 调前德国不包邮单价
        /// </summary>
        public string F_HS_BeforeDENoPostagePrice { get; set; }
        
        /// <summary>
        /// 调后德国不包邮单价
        /// </summary>
        public string F_HS_AfterDENoPostagePrice { get; set; }
        /// <summary>
        /// 调前法国不包邮单价
        /// </summary>
        public string F_HS_BeforeFRNoPostagePrice { get; set; }
        
        /// <summary>
        /// 调后法国不包邮单价
        /// </summary>
        public string F_HS_AfterFRNoPostagePrice { get; set; }
        /// <summary>
        /// 调前生效日
        /// </summary>
        public string FBeforeEffDate { get; set; }
        /// <summary>
        /// 调前失效日
        /// </summary>
        public string FBeforeUnEffDate { get; set; }
        [JsonProperty]
        /// <summary>
        /// 调后生效日
        /// </summary>
        public string FAfterEffDate { get; set; }
        [JsonProperty]
        /// <summary>
        /// 调后失效日
        /// </summary>
        public string FAfterUnEffDate { get; set; }
        [JsonProperty]
        /// <summary>
        /// 是否为特价价目表
        /// </summary>
        public bool F_HS_YNSpecialPrice { get; set; }
        /// <summary>
        /// 是否已同步
        /// </summary>
        public bool F_HS_YNEntryInSync { get; set; }
        /// <summary>
        /// 计价单位
        /// </summary>
       public string FMatUnitId { get; set; }
    }
}
