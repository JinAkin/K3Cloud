using Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject;
using Newtonsoft.Json;
using System;

namespace Hands.K3.SCM.APP.Entity.SynDataObject.Material_
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Material: AbsSynchroDataInfo
    {
        [JsonProperty]
        /// <summary>
        /// 物料LISTID
        /// </summary>
        public string F_HS_ListID { get; set; }
        [JsonProperty]
        /// <summary>
        /// 物料ListName
        /// </summary>
        public string F_HS_ListName { get; set; }
        [JsonProperty]
        /// <summary>
        /// 物料编码
        /// </summary>
        public string FNumber { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime FCreateDate { get; set; }
        /// <summary>
        /// 存货类别
        /// </summary>
        public string FCategoryID { get; set; }
        ///// <summary>
        ///// 物料名称
        ///// </summary>
        //public string FName { get; set; }
        ///// <summary>
        ///// 物料规格型号
        ///// </summary>
        //public string FSpecification { get; set; }
        [JsonProperty]
        /// <summary>
        /// 销售单位
        /// </summary>
        public string FSaleUnitId { get; set; }
        [JsonProperty]
        /// <summary>
        /// 物料毛重
        /// </summary>
        public decimal FGROSSWEIGHT { get; set; }
        /// <summary>
        /// 数据状态
        /// </summary>
        public string FDocumentStatus { get; set; }
        [JsonProperty]
        /// <summary>
        /// 禁用状态
        /// </summary>
        public string FForbidStatus { get; set; }
        [JsonProperty]
        /// <summary>
        /// 产品限制国家
        /// </summary>
        public string F_HS_ProductLimitState { get; set; }
        [JsonProperty]
        /// <summary>
        /// 物料SKU
        /// </summary>
        public string F_HS_ProductSKU { get; set; }
        [JsonProperty]
        /// <summary>
        /// SKU分组码
        /// </summary>
        public string F_HS_SKUGroupCode { get; set; }
        [JsonProperty]
        /// <summary>
        /// 属性1
        /// </summary>
        public string F_HS_Specification1 { get; set; }
        [JsonProperty]
        /// <summary>
        /// 属性2
        /// </summary>
        public string F_HS_Specification2 { get; set; }
        [JsonProperty]
        /// <summary>
        /// 品牌名称
        /// </summary>
        public string F_HS_BrandName { get; set; }
        [JsonProperty]
        /// <summary>
        /// 品牌编码
        /// </summary>
        public string F_HS_BrandNumber { get; set; }
        [JsonProperty]
        /// <summary>
        /// 商品状态
        /// </summary>
        public string F_HS_PRODUCTSTATUS { get; set; }

        /// <summary>
        /// 重量
        /// </summary>
        //public decimal FGROSSWEIGHT { get; set; }
        [JsonProperty]
        /// <summary>
        /// 额外提成比例
        /// </summary>
        public decimal F_HS_ExtraPercentage { get; set; }
        [JsonProperty]
        /// <summary>
        /// 额外提成限定国家
        /// </summary>
        public string F_HS_ExtraCommissionCountry { get; set; }
        [JsonProperty]
        /// <summary>
        /// 底价（USD）
        /// </summary>
        public decimal F_HS_BasePrice { get; set; }
        [JsonProperty]
        /// <summary>
        /// 底价限定国家
        /// </summary>
        public string F_HS_BasePriceCountry { get; set; }
        /// <summary>
        /// 基本
        /// </summary>
        //public MaterialBase MBase { get; set; }
        ///// <summary>
        ///// 销售
        ///// </summary>
        //public MaterialSale MSale { get; set; }

        //public string F_HS_FreeMailMod { get; set; }
        [JsonProperty]
        /// <summary>
        /// 物料免邮模式
        /// </summary>
        public bool F_HS_FreeMailMod { get; set; }
        [JsonProperty]
        /// <summary>
        /// 普货属性
        /// </summary>
        public string F_HS_IsPuHuo { get; set; }
        [JsonProperty]
        /// <summary>
        /// 电池属性
        /// </summary>
        public string F_HS_BatteryMod { get; set; }
        [JsonProperty]
        /// <summary>
        /// 液体属性
        /// </summary>
        public string F_HS_IsOil { get; set; }
        [JsonProperty]
        /// <summary>
        /// 是否已上架
        /// </summary>
        public bool F_HS_IsOnSale { get; set; }
        [JsonProperty]
        /// <summary>
        /// 最小销售数量
        /// </summary>
        public int F_HS_MinSalesNum { get; set; }
        [JsonProperty]
        /// <summary>
        /// 最大销售数量
        /// </summary>
        public int F_HS_MaxSalesNum { get; set; }
        [JsonProperty]
        /// <summary>
        /// 游客价
        /// </summary>
        public decimal F_HS_TouristPrice { get; set; }
        [JsonProperty]
        /// <summary>
        /// 烟油是否0mg
        /// </summary>
        public bool F_HS_YNZeroMgSmokeOil { get; set; }
        [JsonProperty]
        /// <summary>
        /// 澳洲积分返点率
        /// </summary>
        public decimal F_HS_IntegralReturnRate
        {
            get
            {
                if (F_HS_PRODUCTSTATUS.CompareTo("SPQC") == 0)
                {
                    return 0;
                }
                if (F_HS_IsOil.CompareTo("3") == 0 || F_HS_IsPuHuo.CompareTo("4") == 0)
                {
                    return 5;
                }
                else
                {
                    return 1;
                }
            }
            set
            {
            }
        }
        public bool F_HS_NotCoverMaterialName { get; set; }
        [JsonProperty]
        /// <summary>
        /// 是否可夹纯电池
        /// </summary>
        public bool F_HS_ISCanTogetherPureBT { get; set; }
        /// <summary>
        /// 客户DropShipOrder订单前缀
        /// </summary>
        public string F_HS_DropShipOrderPrefix { get; set; }
        /// <summary>
        /// 物料归属
        /// </summary>
        public string F_HS_MaterialOwner { get; set; }
        [JsonProperty]
        /// <summary>
        /// 首页标记
        /// </summary>
        public bool F_HS_YNHomePageMarkup { get; set; }
    }
}
