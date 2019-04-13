
using Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Hands.K3.SCM.APP.Entity.SynDataObject.SaleOrder
{
    [JsonObject(MemberSerialization.OptIn)]
    public class K3SalOrderEntryInfo
    {
        #region
        /// <summary>
        /// 明细主键
        /// </summary>
        public int FENTRYID { get; set; }
        /// <summary>
        /// 明细行
        /// </summary>
        public int FSEQ { get; set; }
        [JsonProperty]
        /// <summary>
        /// 仓库编码
        /// </summary>
        public string FStockId { get; set; }
        /// <summary>
        /// 地理仓ID
        /// </summary>
        public string F_HS_DLCID { get; set; }
        [JsonProperty]
        /// <summary>
        /// 物料编码
        /// </summary>
        public string FMaterialId { get; set; }
        /// <summary>
        /// 物料名称
        /// </summary>
        public string FMaterialName { get; set; }
        /// <summary>
        /// 销售单位
        /// </summary>
        public string FUnitId { get; set; }
        [JsonProperty]
        /// <summary>
        /// 商品数量
        /// </summary>
        public decimal FQTY { get; set; }

        /// <summary>
        /// 单价
        /// </summary>
        public decimal FTAXPRICE { get; set; }

        /// <summary>
        /// 订单原始单价
        /// </summary>
        public decimal F_HS_OrgTaxPrice { get; set; }
        [JsonProperty]
        /// <summary>
        /// 金额
        /// </summary>
        public decimal FTAXAmt { get; set; }
        /// <summary>
        /// 金额
        /// </summary>
        public decimal FAMOUNT { get; set; }
        /// <summary>
        /// 价税合计
        /// </summary>
        public decimal FAllAmount { get; set; }
        /// <summary>
        /// 折扣率
        /// </summary>
        public decimal FDiscRate { get; set; }

        /// <summary>
        /// 折扣额
        /// </summary>
        public decimal FDiscAmt { get; set; }

        /// <summary>
        /// 客户分组
        /// </summary>
        public string F_HS_FGroup { get; set; }
        [JsonProperty]
        /// <summary>
        /// 是否虚拟
        /// </summary>
        public bool F_HS_IsVirtualEntry { get; set; }
        [JsonProperty]
        /// <summary>
        /// 是否为赠品
        /// </summary>
        public bool FIsFree { get; set; }
        [JsonProperty]
        /// <summary>
        /// 是否为清仓
        /// </summary>
        public bool F_HS_IsEmptyStock { get; set; }
        /// <summary>
        /// 电池属性
        /// </summary>
        public string F_HS_BatteryMod { get; set; }
        /// <summary>
        /// 液体属性
        /// </summary>
        public string F_HS_IsOil { get; set; }
        /// <summary>
        /// 普货属性
        /// </summary>
        public string F_HS_IsPuHuo { get; set; }
        /// <summary>
        /// 重量小计
        /// </summary>
        public decimal F_HS_TotalWeight { get; set; }
        /// <summary>
        /// 订单明细创建时间
        /// </summary>
        public DateTime F_HS_CreateDateEntry { get; set; }
        /// <summary>
        /// 是否清仓
        /// </summary>
        public string F_HS_ProductStatus_GD { get; set; }
        /// <summary>
        /// 是否线上订单明细
        /// </summary>
        public bool F_HS_YNOnLine { get; set; }
        /// <summary>
        /// 折扣率%
        /// </summary>
        public decimal FDiscountRate
        {
            get
            {
                return F_HS_PointsDiscountRate + F_HS_CouponDiscountRate + F_HS_OnlineOrderDiscountRate + F_HS_CombineDiscountRate + F_HS_BrandDiscountRate;
            }
        }

        //private decimal _F_HS_PointsDiscountRate;

        /// <summary>
        /// 积分抵扣折扣率
        /// </summary>
        public decimal F_HS_PointsDiscountRate
        {
            get;set;
        }

        //private decimal _F_HS_CouponDiscountRate = 0;
        /// <summary>
        /// 优惠券折扣率
        /// </summary>
        public decimal F_HS_CouponDiscountRate
        {
            get;set;
        }
        //private decimal _F_HS_OnlineOrderDiscountRate = 0;
        /// <summary>
        /// 线上订单折扣率 
        /// </summary>
        public decimal F_HS_OnlineOrderDiscountRate
        {
            get;set;
        }
        /// <summary>
        /// 组合产品折扣率
        /// </summary>
        public decimal F_HS_CombineDiscountRate { get; set; }
        /// <summary>
        /// 品牌折扣率
        /// </summary>
        public decimal F_HS_BrandDiscountRate { get; set; }

        /// <summary>
        /// 子单据体 交货明细
        /// </summary>
        public List<K3SalOrderEntryPlan> EntryPlans { get; set; }
        #endregion
    }
}
