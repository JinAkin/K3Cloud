
using Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Hands.K3.SCM.APP.Entity.SynDataObject
{
    [JsonObject(MemberSerialization.OptIn)]
    /// <summary>
    /// k3客户信息
    /// </summary>
    public class K3CustomerInfo : AbsSynchroDataInfo
    {
        /// <summary>
        /// 客户内码
        /// </summary>
        public int FCUSTID { get; set; }
        /// <summary>
        /// 创建组织
        /// </summary>
        public string FCreateOrgId { get; set; }
        /// <summary>
        /// 使用组织
        /// </summary>
        public string FUseOrgId { get;set;}
        [JsonProperty]
        /// <summary>
        /// 客户编码
        /// </summary>
        public string FNumber { get; set; }

        /// <summary>
        /// 简称
        /// </summary>
        public string FShortName { get; set; }
        /// <summary>
        /// 客户名称
        /// </summary>
        public string FName { get; set; }
        /// <summary>
        /// 国家
        /// </summary>
        public string FFCOUNTRY { get; set; }
        /// <summary>
        /// 地区
        /// </summary>
        public string FPROVINCIAL { get; set; }

        /// <summary>
        /// 通讯地址
        /// </summary>
        public string FAddress { get; set; }
        /// <summary>
        /// 邮政编码
        /// </summary>
        public string FZIP { get; set; }
        /// <summary>
        /// 公司网址
        /// </summary>
        public string FWEBSITE { get; set; }
        /// <summary>
        /// 联系电话
        /// </summary>
        public string FTEL { get; set; }
        /// <summary>
        /// 传真
        /// </summary>
        public string FFAX { get; set; }
        /// <summary>
        /// 纳税登记号
        /// </summary>
        public string FTAXREGISTERCODE { get; set; }
        /// <summary>
        /// 纳税登记号
        /// </summary>
        public string FCompanyClassify { get; set; }
        /// <summary>
        /// 公司性质
        /// </summary>
        public string FCompanyNature { get; set; }
        /// <summary>
        /// 公司规模 
        /// </summary>
        public string FCompanyScale { get; set; }
        /// <summary>
        /// 对应供应商
        /// </summary>
        public string FSUPPLIERID { get; set; }
        /// <summary>
        /// 对应集团客户
        /// </summary>
        public string FGROUPCUSTID { get; set; }
        /// <summary>
        /// 集团客户
        /// </summary>
        public bool FIsGroup { get; set; }
        /// <summary>
        /// 默认付款方
        /// </summary>
        public bool FIsDefPayer { get; set; }
        /// <summary>
        /// 客户类别
        /// </summary>
        public string FCustTypeId { get; set; }
        /// <summary>
        /// 客户分组
        /// </summary>
        public string FGroup { get; set; }
        /// <summary>
        /// 对应组织
        /// </summary>
        public string FCorrespondOrgId { get; set; }
        /// <summary>
        /// 结算币别
        /// </summary>
        public string FTRADINGCURRID { get; set; }
        /// <summary>
        /// 销售部门
        /// </summary>
        public string FSALDEPTID { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string FDescription { get; set; }
        /// <summary>
        /// 销售员
        /// </summary>
        public string FSELLER { get; set; }
        /// <summary>
        /// 结算方式
        /// </summary>
        public string FSETTLETYPEID { get; set; }
        /// <summary>
        /// 收款条件
        /// </summary>
        public string FRECCONDITIONID { get; set; }
        /// <summary>
        /// 价目表
        /// </summary>
        public string FPRICELISTID { get; set; }
        /// <summary>
        /// 折扣表
        /// </summary>
        public string FDISCOUNTLISTID { get; set; }
        /// <summary>
        /// 运输提前期
        /// </summary>
        public DateTime FTRANSLEADTIME { get; set; }
        /// <summary>
        /// 税分类
        /// </summary>
        public string FTaxType { get; set; }
        /// <summary>
        /// 发票类型
        /// </summary>
        public string FInvoiceType { get; set; }
        /// <summary>
        /// 默认税率
        /// </summary>
        public string FTaxRate { get; set; }
        /// <summary>
        /// 客户优先级
        /// </summary>
        public string FPriority { get; set; }
        /// <summary>
        /// 收款币别
        /// </summary>
        public string FRECEIVECURRID { get; set; }
        /// <summary>
        /// 启用信用管理
        /// </summary>
        public bool FISCREDITCHECK { get; set; }
        /// <summary>
        /// 单据状态
        /// </summary>
        public string FDOCUMENTSTATUS { get; set; }
        /// <summary>
        /// 销售价目表
        /// </summary>
        public string PRICELISTID { get; set; }
        /// <summary>
        /// 客户特殊要求
        /// </summary>
        public string F_HS_SpecialDemand { get; set; }
        /// <summary>
        /// 是否交易客户
        /// </summary>
        public bool FIsTrade { get; set; }
        /// <summary>
        /// 子单据头 商务信息
        /// </summary>
        public K3CustExtInfo custExtInfo { get; set; }
        /// <summary>
        /// 单据体 联系人
        /// </summary>
        public List<K3CustLocationInfo> lstCustLocInfo { get; set; }
        /// <summary>
        /// 单据体 银行信息
        /// </summary>
        public List<K3CustBankInfo> lstCustBakInfo { get; set; }
        /// <summary>
        /// 单据体 地址信息
        /// </summary>
        public List<K3CustContactInfo> lstCustCtaInfo { get; set; }
        /// <summary>
        /// 单据体 订货组织
        /// </summary>
        public List<K3CustOrderOrgInfo> lstCustOrgInfo { get; set; }
        /// <summary>
        /// 客户电子邮箱
        /// </summary>
        public string F_HS_CustomerRegisteredMail { get; set; }
        /// <summary>
        /// 客户税号
        /// </summary>
        public string F_HS_TaxNum { get; set; }
        /// <summary>
        /// 客户会员等级
        /// </summary>
        public string F_HS_Grade { get; set; }
        /// <summary>
        /// 客户下单次数
        /// </summary>
        public string F_HS_OrderQty { get; set; }

        /// <summary>
        /// 是否免运费
        /// </summary>
        public bool F_HS_UseLocalPriceList { get; set; }
        /// <summary>
        /// 客户采购邮箱
        /// </summary>
        public string F_HS_CustomerPurchaseMail { get; set; }

        public decimal F_HS_Balance { get; set;}

        /// <summary>
        /// 固定申报要求
        /// </summary>
        public string F_HS_ReportingRequirements { get; set; }
        /// <summary>
        /// 固定随货资料
        /// </summary>
        public string F_HS_ShippingInformation { get; set; }
        /// <summary>
        /// 固定发票备注
        /// </summary>
        public string F_HS_InvoiceComments { get; set; }
        /// <summary>
        /// 积分或折扣
        /// </summary>
        public string F_HS_DiscountOrPoint { get; set; }
        /// <summary>
        /// 客户公司名
        /// </summary>
        public string F_HS_CompanyName { get; set; }
        [JsonProperty]
        /// <summary>
        /// 线上订单折扣率
        /// </summary>
        public decimal F_HS_OnlineDiscount { get; set; }
        [JsonProperty]
        /// <summary>
        /// TT折扣率
        /// </summary>
        public decimal F_HS_TTDiscount { get; set; }
        [JsonProperty]
        /// <summary>
        /// 固定运费折扣率
        /// </summary>
        public decimal F_HS_FixedFreightDiscount { get; set; }
        [JsonProperty]
        /// <summary>
        /// 积分返点率
        /// </summary>
        public decimal F_HS_IntegralReturnRate { get; set; }
        /// <summary>
        /// 固定_率_信用额度变更备注
        /// </summary>
        public string F_HS_DiscountChangeRemark { get; set; }
        /// <summary>
        /// 原线上订单折扣率
        /// </summary>
        public decimal F_HS_OldOnlineDiscount { get; set; }
        /// <summary>
        /// 原固定TT折扣率
        /// </summary>
        public decimal F_HS_OldTTDISCOUNT { get; set; }
        /// <summary>
        /// 原固定运费折扣率
        /// </summary>
        public decimal F_HS_OldFixedFreightDiscount { get; set; }
        /// <summary>
        /// 原固定积分返点率
        /// </summary>
        public decimal F_HS_OldIntegralReturnRate { get; set; }
        /// <summary>
        /// 单据状态
        /// </summary>
        public string FDocumentStatus { get; set; }
        /// <summary>
        /// 原信用额度
        /// </summary>
        public decimal F_HS_OldCreditLineUSD { get; set; }
        [JsonProperty]
        /// <summary>
        /// 信用额度USD
        /// </summary>
        public decimal F_HS_CreditLineUSD { get; set; }
        /// <summary>
        /// 剩余信用额度USD
        /// </summary>
        public decimal F_HS_SurplusCreditUSD { get; set; }
        /// <summary>
        /// 原固定_率_信用额度变更备注
        /// </summary>
        public string F_HS_OldDiscountChangeRemark { get; set; }
    }

}

