using Hands.K3.SCM.App.Synchro.Base.Abstract;
using Hands.K3.SCM.App.Synchro.Utils.SynchroService;
using Hands.K3.SCM.APP.Entity.CommonObject;
using Hands.K3.SCM.APP.Entity.EnumType;
using Hands.K3.SCM.APP.Entity.K3WebApi;
using Hands.K3.SCM.APP.Entity.StructType;
using Hands.K3.SCM.APP.Entity.SynDataObject;
using Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject;
using Hands.K3.SCM.APP.Entity.SynDataObject.SaleOrder;
using Hands.K3.SCM.APP.Utils;
using Hands.K3.SCM.APP.Utils.Utils;
using HS.K3.Common.Abbott;
using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;


namespace Hands.K3.SCM.App.Core.SynchroService.ToK3
{
    [Kingdee.BOS.Util.HotUpdate]
    public class SynSalOrderToK3 : AbstractSynchroToK3
    {
        private const int ORGID = 100035;//使用组织
        private const decimal ALLOWDIFF = 0;
        public static HashSet<K3SalOrderInfo> first = new HashSet<K3SalOrderInfo>();//保存第一次同步过来的订单
        /// <summary>
        /// 默认销售员编码
        /// </summary>
        public string DefaultSalerNumber
        {
            get;
            set;
        }

        /// <summary>
        /// 默认客户编码
        /// </summary>
        public string DefaultCustNumber
        {
            get;
            set;
        }

        /// <summary>
        /// 同步数据类型
        /// </summary>
        override public SynchroDataType DataType
        {
            get
            {
                return SynchroDataType.SaleOrder;
            }
        }

        /// <summary>
        /// 数据类型对应的k3cloud的formkey，参照 HSFormIdConst 去
        /// </summary>
        override public string FormKey
        {
            get
            {
                return HSFormIdConst.SAL_SaleOrder;
            }
        }
        override public FormMetadata Metadata
        {
            get
            {
                return MetaDataServiceHelper.Load(this.K3CloudContext, this.FormKey) as FormMetadata;
            }
        }
        public override string BillNoKey
        {
            get
            {
                return "FBillNo";
            }
        }

        public bool BySelect
        {
            get;
            set;
        }
        public List<string> OrderRedisKeys
        {
            get;
            set;
        }

        List<string> beUpdateOrder = new List<string>();


        public override void BeforeDoSynchroData()
        {
            base.BeforeDoSynchroData();
            GetDefualtOrgNumber();
            GetDefualtSalerNumber();
            GetDefualtCustNumber();
            //GetFusionOrderSettingInfo();
            beUpdateOrder = new List<string>();
        }

        /// <summary>
        /// 获取Redis销售订单信息
        /// </summary>
        /// <param name="numbers"></param>
        /// <returns></returns>
        public override IEnumerable<AbsSynchroDataInfo> GetSynchroData(IEnumerable<string> numbers = null)
        {
            List<K3SalOrderInfo> lstSalOrder = null;

            var datas = ServiceHelper.GetSynchroDatas(this.K3CloudContext, this.DataType, this.RedisDbId, numbers,this.Direction);
            if (datas != null && datas.Count > 0)
            {
                lstSalOrder = datas.Select(o => (K3SalOrderInfo)o).ToList();
            }

            List<K3SalOrderInfo> unpaidOrders = GetOrdersByPaymentStatus(lstSalOrder, "2");

            if (unpaidOrders != null && unpaidOrders.Count > 0)
            {
                foreach (var un in unpaidOrders)
                {
                    if (un != null)
                    {
                        first.Add(un);
                    }
                }
            }

            if (lstSalOrder != null && lstSalOrder.Count > 0)
            {
                List<string> cancelNos = GetCanceledNos(lstSalOrder);

                if (cancelNos != null && cancelNos.Count > 0)
                {
                    List<AbsSynchroDataInfo> orders = lstSalOrder.Where(o => !cancelNos.Contains(o.FBillNo)).ToList<AbsSynchroDataInfo>();
                    RemoveRedisData(this.K3CloudContext, cancelNos);
                    return orders;
                }

                return lstSalOrder.ToList<AbsSynchroDataInfo>();
            }
            return null;
        }

        /// <summary>
        /// 将需要同步的数据转为JSON格式（单个）
        /// </summary>
        /// <param name="sourceData"></param>
        /// <param name="log"></param>
        /// <param name="operationType"></param>
        /// <returns></returns>
        public override JObject BuildSynchroDataJson(AbsSynchroDataInfo sourceData, SynchroLog log, SynOperationType operationType)
        {

            JObject root = new JObject();
            K3SalOrderInfo order = sourceData as K3SalOrderInfo;

            root.Add("NeedUpDateFields", new JArray(""));
            root.Add("NeedReturnFields", new JArray("FBillNo"));
            root.Add("IsDeleteEntry", "false");
            root.Add("SubSystemId", "");
            root.Add("IsVerifyBaseDataField", "true");

            JArray model = new JArray();

            if (sourceData != null)
            {

                JObject baseData = new JObject();

                JObject FBILLTYPEID = new JObject();
                FBILLTYPEID.Add("FNumber", "XSDD01_SYS");
                baseData.Add("FBILLTYPEID", FBILLTYPEID);

                JObject FBusinessType = new JObject();
                FBusinessType.Add("FNumber", "NORMAL");
                baseData.Add("FBusinessType", FBusinessType);

                baseData.Add("FBillNo", order.FBillNo);

                JObject FSaleOrgId = new JObject();
                FSaleOrgId.Add("FNumber", "100.01");
                baseData.Add("FSaleOrgId", FSaleOrgId);

                JObject FSaleDeptId = new JObject();
                FSaleDeptId.Add("FNumber", order.FSalerId);
                baseData.Add("FSaleDeptId", FSaleDeptId);

                JObject FCustId = new JObject();
                FCustId.Add("FNumber", order.FCustId);
                baseData.Add("FCustId", FCustId);

                baseData.Add("FDATE", order.FDate);

                JObject FSettleCurrId = new JObject();
                FSettleCurrId.Add("FNumber", order.FSettleCurrId);
                baseData.Add("FSettleCurrId", FSettleCurrId);

                JObject FSalerId = new JObject();
                FSalerId.Add("FNumber", order.FSalerId);
                baseData.Add("FSalerId", FSalerId);
                baseData.Add("FBusinessType", order.FBillTypeId);
                baseData.Add("F_HS_TransactionID", order.F_HS_TransactionID);//交易流水号
                baseData.Add("FSaleOrderFinance", BuildSaleOrderFinanceJsons(order));//销售订单财务信息
                baseData.Add("FSaleOrderEntry", BuildSaleOrderEntryJsons(order));//销售订单明细
                model.Add(baseData);
            }
            root.Add("Model", model);
            return root;
        }

        /// <summary>
        /// 将同步的数据转换为JSON格式（集合）
        /// </summary>
        /// <param name="sourceDatas"></param>
        /// <param name="operationType"></param>
        /// <returns></returns>
        public override JObject BuildSynchroDataJsons(IEnumerable<AbsSynchroDataInfo> sourceDatas, SynOperationType operationType)
        {
            JObject root = new JObject()
;
            JArray model = new JArray();
            List<K3SalOrderInfo> orders = sourceDatas.Select(c => (K3SalOrderInfo)c).ToList();
            int batchCount = BatchCount(sourceDatas);

            root.Add("NeedUpDateFields", new JArray(""));
            root.Add("NeedReturnFields", new JArray("FBillNo"));
            root.Add("IsDeleteEntry", "false");
            root.Add("SubSystemId", "");
            root.Add("IsVerifyBaseDataField", "true");
            root.Add("BatchCount", batchCount);


            if (orders != null)
            {
                if (orders.Count > 0)
                {
                    foreach (var order in orders)
                    {
                        if (order != null)
                        {
                            JObject baseData = new JObject();
                            //基本信息(单据头)
                            JObject FBILLTYPEID = new JObject();//单据类型
                            FBILLTYPEID.Add("FNumber", order.FBillTypeId);
                            baseData.Add("FBILLTYPEID", FBILLTYPEID);

                            baseData.Add("FBusinessType", "NORMAL");//业务类型

                            if (operationType != SynOperationType.UPDATE)
                            {
                                baseData.Add("FBillNo", order.FBillNo);

                                JObject FSaleOrgId = new JObject();//销售组织
                                FSaleOrgId.Add("FNumber", order.FSaleOrgId);
                                baseData.Add("FSaleOrgId", FSaleOrgId);
                            }

                            JObject FSaleDeptId = new JObject();//销售部门
                            FSaleDeptId.Add("FNumber", order.FSaleDeptId);
                            baseData.Add("FSaleDeptId", FSaleDeptId);

                            JObject F_HS_SALEORDERSOURCE = new JObject();//订单来源
                            F_HS_SALEORDERSOURCE.Add("FNumber", order.F_HS_SaleOrderSource);
                            baseData.Add("F_HS_SaleOrderSource", F_HS_SALEORDERSOURCE);

                            JObject FCustId = new JObject();//客户
                            FCustId.Add("FNumber", order.FCustId);
                            baseData.Add("FCustId", FCustId);

                            JObject F_HS_B2CCUSTID = new JObject();//B2C客户（零售客户）
                            F_HS_B2CCUSTID.Add("FNumber", order.F_HS_B2CCustId);
                            baseData.Add("F_HS_B2CCUSTID", F_HS_B2CCUSTID);

                            baseData.Add("FDATE", order.FDate);//单据日期

                            JObject FPriceListId = new JObject();//销售价目表
                            FPriceListId.Add("FNumber", "");
                            baseData.Add("FPriceListId", FPriceListId);

                            JObject FSettleCurrId = new JObject();//结算币别
                            FSettleCurrId.Add("FNumber", order.FSettleCurrId);
                            baseData.Add("FSettleCurrId", FSettleCurrId);

                            JObject FSalerId = new JObject();//销售员
                            FSalerId.Add("FNumber", order.FSalerId);
                            baseData.Add("FSalerId", FSalerId);

                            baseData.Add("FNOTE", order.FNote);//备注
                            baseData.Add("F_HS_OnlineOrderWay", order.F_HS_Channel);//订单渠道

                            baseData.Add("F_HS_CouponAmount", order.F_HS_CouponAmount);//使用优惠券金额
                            baseData.Add("F_HS_UsedPointsNum", order.F_HS_Points);//使用积分数
                            baseData.Add("F_HS_UsedPoints", order.F_HS_Points);//已扣减积分数
                            baseData.Add("F_HS_UsePoint", order.F_HS_Points);//使用积分数
                            baseData.Add("F_HS_IntegralDeduction", order.F_HS_IntegralDeduction);//使用积分金额
                            baseData.Add("F_HS_BrandDiscountAmount", order.F_HS_BrandDiscountAmount);//品牌折扣抵扣额
                            baseData.Add("F_HS_CombinedDiscountAmount", order.F_HS_CombinedDiscountAmount);//组合产品折扣抵扣额
                            baseData.Add("F_HS_OrderDiscountAmountNew", order.F_HS_OrderDiscountAmountNew);//订单折扣额
                            baseData.Add("F_HS_IsVirtual", order.F_HS_IsVirtual);//是否为虚拟
                            baseData.Add("F_HS_PaymentStatus", order.F_HS_PaymentStatus);//是否已付款

                            baseData.Add("F_HS_DeliveryName", order.F_HS_DeliveryName);//交货联系人
                            baseData.Add("F_HS_DeliveryCity", order.F_HS_DeliveryCity);//交货城市
                            baseData.Add("F_HS_DeliveryProvinces", order.F_HS_DeliveryProvinces);//交货省份，州
                            baseData.Add("F_HS_PostCode", order.F_HS_PostCode);//交货邮编

                            JObject F_HS_RecipientCountry = new JObject();
                            F_HS_RecipientCountry.Add("FNumber", order.F_HS_RecipientCountry);//国家
                            baseData.Add("F_HS_RecipientCountry", F_HS_RecipientCountry);

                            baseData.Add("F_HS_MobilePhone", order.F_HS_MobilePhone);//移动电话
                            baseData.Add("F_HS_DeliveryAddress", order.F_HS_DeliveryAddress);//交货详细地址
                            baseData.Add("F_HS_ShippingMethod", order.F_HS_ShippingMethod);//发货方式
                            baseData.Add("F_HS_EachShipmentFreight", order.F_HS_EachShipmentFreight);
                            baseData.Add("F_HS_PaymentMode", order.FSettleModeId);//付款方式

                            JObject F_HS_PaymentModeNew = new JObject();//付款方式
                            F_HS_PaymentModeNew.Add("FNumber", order.F_HS_PaymentModeNew);
                            baseData.Add("F_HS_PaymentModeNew", F_HS_PaymentModeNew);

                            baseData.Add("FSaleOrderFinance", BuildSaleOrderFinanceJsons(order));//销售订单财务信息
                            baseData.Add("FSaleOrderEntry", BuildSaleOrderEntryJsons(order));//销售订单明细
                            baseData.Add("FSaleOrderPlan", BuildOrderPlanJsons());//收款条件

                            baseData.Add("F_HS_USADatetime", order.F_HS_USADatetime);//订单美国时间
                            baseData.Add("FCreateDate", order.FCreateDate);//订单创建时间
                            baseData.Add("F_HS_RateToUSA", order.F_HS_RateToUSA);//订单美国汇率
                            baseData.Add("F_HS_BillAddress", order.F_HS_BillAddress);//账单地址

                            baseData.Add("F_HS_TransactionID", order.F_HS_TransactionID);//交易流水号
                            baseData.Add("F_HS_DeclareRqstd", order.F_HS_DeclareRqstd);//客户申报要求
                            baseData.Add("F_HS_IsSameAdress", order.F_HS_IsSameAdress);//收货地址与账单地址是否一致
                            baseData.Add("F_HS_UseLocalPriceList", order.F_HS_UseLocalPriceList);//是否使用收货地价目表

                            baseData.Add("F_HS_MergeSrcBillNo", order.F_HS_MergeSrcBillNo);//是否为合并单据
                            baseData.Add("F_HS_IsMergeBill", order.F_HS_IsMergeBill);//合并单据源单编码
                            baseData.Add("F_HS_SZHKMergeNote", order.F_HS_SZHKMergeNote);//深圳香港仓合并备注
                            baseData.Add("F_HS_BalancePayments", order.F_HS_BalancePayments);//支付余额
                            baseData.Add("F_HS_USDBalancePayments", order.F_HS_USDBalancePayments);//支付余额（USD）
                            baseData.Add("F_HS_CreditLineUSDPayments", order.F_HS_CreditLineUSDPayments);//信用余额支付

                            baseData.Add("F_HS_PayTotal", order.F_HS_PayTotal);
                            //baseData.Add("F_HS_ProductDiscountAmount", order.F_HS_ProductDiscountAmount);//整单折扣额(内网创建单才有)
                            baseData.Add("F_HS_FreightAmount", order.F_HS_FreightAmount);//运费金额
                            baseData.Add("F_HS_FreightDiscountAmount", order.F_HS_FreightDiscountAmount);//运费折扣额(内网创建单才有)
                            baseData.Add("F_HS_ProductDiscountRate", order.F_HS_ProductDiscountRate * 100);//整单产品折扣率%
                            //baseData.Add("F_HS_FreightDiscountRate", order.F_HS_FreightDiscountRate * 100);//运费折扣率%
                            baseData.Add("F_HS_DiscountRemarks", order.F_HS_DiscountRemarks);//整单产品折扣备注原因
                            baseData.Add("F_HS_PlatformFreightDiscount", order.F_HS_ShippingDiscount);//平台运费折扣额
                            //使用信用卡付款（延迟付款），将 销售订单.需支付金额结算币别字段清0
                            if (order.F_HS_PaymentModeNew.CompareTo("VISA") == 0 || order.F_HS_PaymentModeNew.CompareTo("MasterCard") == 0)
                            {
                                baseData.Add("F_HS_NeedPayAmount", 0);//需支付金额_结算币
                            }
                            else
                            {
                                baseData.Add("F_HS_NeedPayAmount", order.F_HS_NeedPayAmount);//需支付金额_结算币
                            }
                            baseData.Add("F_HS_PlatformFreightRate", order.F_HS_WebFreightDiscountRate);//平台运费折扣率
                            if (order.F_HS_Subtotal > 0)
                            {
                                baseData.Add("F_HS_OnlineDiscount", order.F_HS_OrderDiscountAmountNew / order.F_HS_Subtotal * 100);//固定产品折扣率%
                            }
                            baseData.Add("F_HS_DropShipDeliveryChannel", order.F_HS_DropShipDeliveryChannel);//DropShipping发货渠道
                            baseData.Add("F_HS_PlatformCustomerID",order.F_HS_PlatformCustomerID);//平台客户ID
                            baseData.Add("F_HS_PlatformCustomerEmail",order.F_HS_PlatformCustomerEmail);//平台客户邮箱
                            baseData.Add("F_HS_AmountDeclared",order.F_HS_AmountDeclared); //产品申报金额
                            baseData.Add("F_HS_FreightDeclared",order.F_HS_FreightDeclared);//运费申报金额

                            JObject F_HS_DeclaredCurrId = new JObject();//申报币别
                            F_HS_DeclaredCurrId.Add("Number",order.F_HS_DeclaredCurrId);
                            baseData.Add("F_HS_DeclaredCurrId", F_HS_DeclaredCurrId);

                            string platform = SQLUtils.GetDropshipPlatform(this.K3CloudContext,order.F_HS_B2CCustId);

                            if (!string.IsNullOrWhiteSpace(platform))
                            {
                                JObject F_HS_Platform = new JObject();//平台
                                F_HS_Platform.Add("FNumber", platform);
                                baseData.Add("F_HS_Platform", F_HS_Platform);
                            }
                           
                            model.Add(baseData);
                        }
                    }
                }
            }
            root.Add("Model", model);
            return root;
        }

        /// <summary>
        /// 销售订单财务信息
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        private JObject BuildSaleOrderFinanceJsons(K3SalOrderInfo order)
        {

            JObject FSaleOrderFinance = new JObject();
            JObject FEXCHANGETYPE = new JObject();
            JObject FExchangeTypeId = new JObject();//汇率类型
            JObject FLocalCurrId = new JObject();//本位币

            FEXCHANGETYPE.Add("FNumber", order.FSettleCurrId);
            FExchangeTypeId.Add("FNumber", "HLTX01_SYS");
            FLocalCurrId.Add("FNumber", "CNY");

            FSaleOrderFinance.Add("FEXCHANGETYPE", FEXCHANGETYPE);
            FSaleOrderFinance.Add("FExchangeTypeId", FExchangeTypeId);//汇率类型
            FSaleOrderFinance.Add("FLocalCurrId", FLocalCurrId);
            //FSaleOrderFinance.Add("FExchangeRate", order.F_HS_RateToUSA);//汇率

            return FSaleOrderFinance;
        }

        /// <summary>
        /// 销售订单明细
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        private JArray BuildSaleOrderEntryJsons(K3SalOrderInfo order)
        {
            JArray FSaleOrderEntry = null;

            if (order != null)
            {
                FSaleOrderEntry = new JArray();

                for (int i = 0; i < order.OrderEntry.Count; i++)
                {
                    K3SalOrderEntryInfo orderEntry = order.OrderEntry[i];
                    JObject oEntry = new JObject();

                    JObject FMATERIALID = new JObject
                    {
                        { "FNumber", orderEntry.FMaterialId }
                    };
                    oEntry.Add("FMATERIALID", FMATERIALID);

                    JObject FUNITID = new JObject
                    {
                        { "FNumber", string.IsNullOrEmpty(orderEntry.FUnitId) ? "Pcs" : orderEntry.FUnitId }
                    };
                    oEntry.Add("FUNITID", FUNITID);

                    JObject FStockId = new JObject
                    {
                        { "FNumber", orderEntry.FStockId }
                    };
                    oEntry.Add("F_HS_StockID", FStockId);


                    oEntry.Add("FQty", orderEntry.FQTY);
                    oEntry.Add("FTaxPrice", orderEntry.FTAXPRICE);
                    oEntry.Add("F_HS_OrgTaxPrice", orderEntry.F_HS_OrgTaxPrice);

                    JObject F_HS_CUSTOMERGROUP = new JObject
                    {
                        { "FNumber", orderEntry.F_HS_FGroup }
                    };
                    oEntry.Add("F_HS_CUSTOMERGROUP", F_HS_CUSTOMERGROUP);

                    if (BuildOrderEntryPlanJsons(orderEntry) != null)
                    {
                        oEntry.Add("FOrderEntryPlan", BuildOrderEntryPlanJsons(orderEntry));
                    }
                   
                    oEntry.Add("F_HS_IsVirtualEntry", orderEntry.F_HS_IsVirtualEntry);
                    oEntry.Add("FIsFree", orderEntry.FIsFree);
                    oEntry.Add("F_HS_IsEmptyStock", orderEntry.F_HS_IsEmptyStock);
                    oEntry.Add("FEntryTaxRate", 0);//税率
                    oEntry.Add("F_HS_CreateDateEntry", orderEntry.F_HS_CreateDateEntry);

                    JObject F_HS_ProductStatus_GD = new JObject();//商品状态
                    F_HS_ProductStatus_GD.Add("F_HS_ProductStatus_GD", orderEntry.F_HS_ProductStatus_GD);
                    oEntry.Add("F_HS_ProductStatus_GD", F_HS_ProductStatus_GD);//是否清仓

                    oEntry.Add("F_HS_YNOnLine", orderEntry.F_HS_YNOnLine);

                    if (order.F_HS_Subtotal > 0 && orderEntry.FMaterialId.CompareTo("99.01") != 0)
                    {
                        oEntry.Add("F_HS_PointsDiscountRate", orderEntry.F_HS_PointsDiscountRate);//积分抵扣折扣率
                        oEntry.Add("F_HS_CouponDiscountRate", orderEntry.F_HS_CouponDiscountRate);//优惠券折扣率
                        oEntry.Add("F_HS_CombineDiscountRate", orderEntry.F_HS_CombineDiscountRate);//组合产品折扣率
                        oEntry.Add("F_HS_BrandDiscountRate", orderEntry.F_HS_BrandDiscountRate);//品牌折扣率
                        oEntry.Add("F_HS_OnlineOrderDiscountRate", orderEntry.F_HS_OnlineOrderDiscountRate);//线上订单折扣率      
                    }
                    if (orderEntry.FMaterialId.CompareTo("99.01") == 0)
                    {
                        if (order.F_HS_Shipping > 0)
                        {
                            oEntry.Add("F_HS_WebFreightDiscountRate", order.F_HS_WebFreightDiscountRate);//平台运费折扣率 
                        }
                    }

                    FSaleOrderEntry.Add(oEntry);
                }

                return FSaleOrderEntry;
            }

            return null;
        }

        /// <summary>
        /// 销售订单交货明细
        /// </summary>
        /// <param name="orderEntry"></param>
        /// <returns></returns>
        private JArray BuildOrderEntryPlanJsons(K3SalOrderEntryInfo orderEntry)
        {
            JArray FOrderEntryPlan = null;
            JObject entryPlan = null;

            if (orderEntry != null)
            {
                if (orderEntry.EntryPlans != null && orderEntry.EntryPlans.Count > 0)
                {
                    foreach (var item in orderEntry.EntryPlans)
                    {
                        FOrderEntryPlan = new JArray();
                        entryPlan = new JObject();

                        entryPlan.Add("FPlanDeliveryDate", item.FPlanDeliveryDate);
                        entryPlan.Add("FPlanDate", item.FPlanDate);
                        entryPlan.Add("FPlanQty", item.FPlanQty);

                        FOrderEntryPlan.Add(entryPlan);
                    }
                } 
            }

            return FOrderEntryPlan;
        }

        /// <summary>
        /// 收款条件
        /// </summary>
        /// <returns></returns>
        private JArray BuildOrderPlanJsons()
        {
            JArray FSaleOrderPlan = new JArray();
            JObject baseDate = new JObject();

            baseDate.Add("FNeedRecAdvance", true);
            FSaleOrderPlan.Add(baseDate);

            return FSaleOrderPlan;
        }

        /// <summary>
        /// 判断单据编码是否存在
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="srcData"></param>
        /// <returns></returns>
        public bool IsExist(Context ctx, AbsSynchroDataInfo srcData)
        {
            QueryBuilderParemeter para = new QueryBuilderParemeter();
            para.FormId = this.FormKey;
            para.SelectItems = SelectorItemInfo.CreateItems("FBillNo");

            if (srcData != default(AbsSynchroDataInfo))
            {
                K3SalOrderInfo order = srcData as K3SalOrderInfo;
                if (order != default(K3SalOrderInfo))
                {
                    para.FilterClauseWihtKey = " FBillNo='" + order.FBillNo + "'";
                }

                var k3Data = Kingdee.BOS.App.ServiceHelper.GetService<IQueryService>().GetDynamicObjectCollection(ctx, para);

                if (k3Data != null && k3Data.Count > 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 筛选更新或同步的订单
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="srcDatas"></param>
        /// <returns></returns>
        public override Dictionary<SynOperationType, IEnumerable<AbsSynchroDataInfo>> EntityDataSource(Context ctx, IEnumerable<AbsSynchroDataInfo> srcDatas)
        {
            Dictionary<SynOperationType, IEnumerable<AbsSynchroDataInfo>> dict = new Dictionary<SynOperationType, IEnumerable<AbsSynchroDataInfo>>();

            List<AbsSynchroDataInfo> lstDiff = null;

            List<string> K3SalNos = null;
            List<string> synSalNos = null;
            List<string> existNos = null;

            string sql = string.Empty;
            DynamicObjectCollection coll = null;

            #region

            List<K3SalOrderInfo> orders = srcDatas.Select(o => (K3SalOrderInfo)o).ToList();
            synSalNos = orders.Select(o => o.FBillNo).ToList<string>();

            sql = string.Format(@"/*dialect*/ select FBillNo from T_SAL_ORDER");
            coll = SQLUtils.GetObjects(ctx, sql);

            if (coll != null)
            {
                K3SalNos = coll.Select(o => SQLUtils.GetFieldValue(o, "FBillNo")).ToList<string>();

                if (K3SalNos != null && K3SalNos.Count > 0)
                {
                    existNos = K3SalNos.Intersect(synSalNos).ToList();

                    var diff = from c in orders
                               where !existNos.ToArray().Contains(c.FBillNo)
                               select c;

                    var same = from c in orders
                               where existNos.ToArray().Contains(c.FBillNo)
                               select c;

                    if (diff != null && diff.Count() > 0)
                    {
                        var combine = diff.Where(o => o.OriginalOrderNos != null);

                        if (combine != null)
                        {
                            if (diff.Except(combine) != null)
                            {
                                if (diff.Except(combine).ToList<AbsSynchroDataInfo>() != null)
                                {
                                    lstDiff = diff.Except(combine).ToList<AbsSynchroDataInfo>();
                                }
                            }
                        }
                        else
                        {
                            lstDiff = diff.ToList<AbsSynchroDataInfo>();
                        }

                        if (lstDiff != null && lstDiff.Count > 0)
                        {
                            dict.Add(SynOperationType.SAVE, lstDiff);
                        }
                        if (combine != null && combine.Count() > 0)
                        {
                            dict.Add(SynOperationType.CANCEL_AFTER_SAVE, combine.ToList<AbsSynchroDataInfo>());
                        }
                    }
                    if (same != null && same.Count() > 0)
                    {
                        sql = string.Format(@"/*dialect*/ select FBillNo from T_SAL_ORDER a
		                                    inner join T_BAS_ASSISTANTDATAENTRY_L b on a.F_HS_SaleOrderSource=b.FENTRYID
                                            inner join T_BAS_ASSISTANTDATAENTRY c on b.FentryID=c.FentryID
			                                where a.FCancelStatus != '{0}' and a.FCloseStatus != '{1}'
			                                and a.FDocumentStatus = '{2}'
			                                and c.FNumber = '{3}'", BillCancelStatus.Cancel, BillCloseStatus.Close, BillDocumentStatus.Create, "HCWebPendingOder");

                        coll = SQLUtils.GetObjects(ctx, sql);

                        if (coll != null && coll.Count > 0)
                        {
                            List<string> updateNos = coll.Select(o => SQLUtils.GetFieldValue(o, "FBillNo")).ToList();

                            var update = from o in same
                                         where updateNos.ToArray().Contains(o.FBillNo)
                                         select o;
                            if (update != null && update.Count() > 0)
                            {
                                dict.Add(SynOperationType.SAVE_AFTER_DELETE_WITHOUTREQ, update.ToList<AbsSynchroDataInfo>());
                            }
                        }

                        sql = string.Format(@"/*dialect*/ select FBillNo from T_SAL_ORDER a
		                                    inner join T_BAS_ASSISTANTDATAENTRY_L b on a.F_HS_SaleOrderSource=b.FENTRYID
                                            inner join T_BAS_ASSISTANTDATAENTRY c on b.FentryID=c.FentryID
			                                where a.F_HS_HANG = '{0}' 
			                                and a.FCancelStatus != '{1}' 
			                                and (a.FDocumentStatus = '{2}' or a.FDocumentStatus = '{3}' or a.FDocumentStatus = '{4}')
			                                and c.FNumber = '{5}'", "1", BillCancelStatus.Cancel, BillDocumentStatus.Create, BillDocumentStatus.Auditing, BillDocumentStatus.ReAudit, "HCWebPendingOder");
                        coll = SQLUtils.GetObjects(ctx, sql);

                        if (coll != null)
                        {
                            List<string> modifyNos = coll.Select(o => SQLUtils.GetFieldValue(o, "FBillNo")).ToList<string>();

                            var modify = from c in orders
                                         where modifyNos.Contains(c.FBillNo) && (c.OriginalOrderNos == null || c.OriginalOrderNos.Count == 0)
                                         select c;

                            if (modify != null && modify.Count() > 0)
                            {
                                //dict.Add(SynOperationType.SAVE_AFTER_DELETE, modify.ToList<AbsSynchroDataInfo>());
                            }
                        }

                    }
                }
                else
                {
                    dict.Add(SynOperationType.SAVE, srcDatas);
                    return dict;
                }
            }


            return dict;

            #endregion
        }

        /// <summary>
        /// 取默认的客户编码
        /// </summary>
        public void GetDefualtCustNumber()
        {
            QueryBuilderParemeter para = new QueryBuilderParemeter();
            para.FormId = "BD_Customer";
            para.SelectItems = SelectorItemInfo.CreateItems("FNumber,FName");
            var k3Data = Kingdee.BOS.App.ServiceHelper.GetService<IQueryService>().GetDynamicObjectCollection(this.K3CloudContext, para);

            if (k3Data == null || k3Data.Count == 0)
            {
                DefaultCustNumber = "Bazaar";
                return;
            }

            var existBazaaer = k3Data.FirstOrDefault(f => f["FNumber"].ToString().EqualsIgnoreCase("Bazaar"));
            if (existBazaaer != null)
            {
                DefaultCustNumber = "Bazaar";
                return;
            }

            DefaultCustNumber = k3Data[0]["FNumber"].ToString();
        }


        /// <summary>
        /// 取默认的销售员编码
        /// </summary>
        public void GetDefualtSalerNumber()
        {
            QueryBuilderParemeter para = new QueryBuilderParemeter();
            para.FormId = "BD_OPERATOR";
            para.SelectItems = SelectorItemInfo.CreateItems("FStaffId.FNumber as FNumber,FStaffId.FName as FName");
            para.OrderByClauseWihtKey = "";
            para.FilterClauseWihtKey = " FBIZORGID=100017 And FOperatorType='XSY' ";
            var k3Data = Kingdee.BOS.App.ServiceHelper.GetService<IQueryService>().GetDynamicObjectCollection(this.K3CloudContext, para);

            if (k3Data == null || k3Data.Count == 0)
            {
                DefaultOrgNumber = "TTYG_02_SYS";
                return;
            }

            var ex = k3Data.FirstOrDefault(f => f["FNumber"].ToString() == "TTYG_02_SYS");
            if (ex != null)
            {
                DefaultOrgNumber = "TTYG_02_SYS";
                return;
            }

            DefaultSalerNumber = k3Data[0]["FNumber"].ToString();
        }

        /// <summary>
        /// 检查订单的情况：如果系统里面订单已经关闭或作废，就不再重新下载了
        /// </summary>
        /// <param name="srcData"></param>
        /// <returns></returns>
        public override bool IsCancelSynchro(AbsSynchroDataInfo srcData)
        {
            K3SalOrderInfo soData = srcData as K3SalOrderInfo;

            if (SynSuccList != null)
            {
                if (SynSuccList.Count > 0)
                {
                    foreach (var item in SynSuccList)
                    {
                        if (item.CompareTo(soData.FBillNo) == 0)
                        {
                            return false;
                        }
                    }
                }
            }

            QueryBuilderParemeter para = new QueryBuilderParemeter();
            para.FormId = this.FormKey;
            para.SelectItems = SelectorItemInfo.CreateItems("FID,FCancelStatus,FCloseStatus");
            para.IsSingleForQuery = false;

            if (soData != null)
            {
                para.FilterClauseWihtKey = string.Format(" {0} = '{1}' ", this.BillNoKey, soData.FBillNo);
            }

            var k3Data = Kingdee.BOS.App.ServiceHelper.GetService<IQueryService>().GetDynamicObjectCollection(this.K3CloudContext, para);
            if (k3Data == null || k3Data.Count == 0)
            {
                return false;
            }

            if (k3Data[0]["FCancelStatus"] != null && k3Data[0]["FCancelStatus"].ToString().EqualsIgnoreCase("B"))
            {
                LogUtils.WriteSynchroLog(this.K3CloudContext, this.DataType, "该订单在K3里面已经作废，不再更新", false, soData.FBillNo, soData.FBillNo);
                return true;
            }

            return false;
        }

        public override void AfterDoSynchroData(HttpResponseResult result, SynchroLog log)
        {
            base.AfterDoSynchroData(result, log);

            if (log.K3CloudId.IsNullOrEmptyOrWhiteSpace() == false)
            {
                beUpdateOrder.Add(log.sourceId);
            }
            if (beUpdateOrder.Count > 50)
            {
                ServiceHelper.UpdateOrderEnd(this.K3CloudContext, beUpdateOrder);
                beUpdateOrder.Clear();
            }
        }

        public override void FinishDoSynchroData()
        {
            base.FinishDoSynchroData();
            if (beUpdateOrder.Count > 0)
            {
                ServiceHelper.UpdateOrderEnd(this.K3CloudContext, beUpdateOrder);
            }

            //订单同步完成后，同步一次即时库存
            //var svc = new SynchroDataService();
            //var datas = svc.GetMatStockQtyInfo(this.K3CloudContext, false);
            //if (datas != null && datas.Count > 0)
            //{
            //    svc.SynchroStockQty(K3CloudContext, datas.ToList());
            //}
        }

        /// <summary>
        /// 执行数据同步
        /// </summary>
        /// <param name="sourceDatas"></param>
        /// <param name="logs"></param>
        /// <param name="operationType"></param>
        /// <returns></returns>
        public override HttpResponseResult ExecuteSynchro(IEnumerable<AbsSynchroDataInfo> sourceDatas, List<SynchroLog> logs, SynOperationType operationType)
        {
            HttpResponseResult result = null;
            JObject bizData = BuildSynchroDataJsons(sourceDatas, operationType);

            List<K3SalOrderInfo> orders = sourceDatas.Select(o => (K3SalOrderInfo)o).ToList();
            KDTransactionScope trans = null;

            try
            {
                if (operationType == SynOperationType.SAVE_AFTER_DELETE || operationType == SynOperationType.SAVE_AFTER_DELETE_WITHOUTREQ)
                {
                    List<string> srcOrderNos = orders.Select(o => o.FBillNo).ToList<string>();
                    //using (trans = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
                    //{
                    //    //改单成功后删除原有的销售订单
                    //    List<string> scrOrderNos = orders.Select(o => o.FBillNo).ToList<string>();
                    //    //改单请求成功后删除原有的销售订单
                    //    result = ExecuteOperate(SynOperationType.DELETE, scrOrderNos);
                    //    //改单请求成功后同步新的销售订单
                    //    result = ExecuteOperate(SynOperationType.SAVE, null, null, bizData.ToString());
                    //    if (result != null && result.SuccessEntityNos != null && result.SuccessEntityNos.Count > 0)
                    //    {
                    //        WriteSynchroFailLog(orders, result.SuccessEntityNos);
                    //        //同步成功后删除Redis中的数据
                    //        RemoveRedisData(this.K3CloudContext, result.SuccessEntityNos);
                    //        //同步成功后删除之前未成功的记录（数据库）
                    //        LogUtils.DelSynSucessOrderInfo(this.K3CloudContext, result.SuccessEntityNos);
                    //    }

                    //    result = ExecuteOperateAfterSave(this.K3CloudContext, result, logs, orders);

                    //    trans.Complete();
                    //}
                    result = new HttpResponseResult();
                    result.Success = false;
                    result.Message = string.Format("销售订单【{0}】在系统已经存在！", string.Join(",", srcOrderNos));
                }
                else if (operationType == SynOperationType.SAVE)
                {

                    using (trans = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
                    {
                        result = ExecuteOperate(SynOperationType.SAVE, null, null, bizData.ToString());
                        if (result != null)
                        {
                            WriteSynchroFailLog(orders, result.SuccessEntityNos);

                            //同步成功后删除之前未成功的记录（数据库）
                            LogHelper.DelSynSucessOrderInfo(this.K3CloudContext, result.SuccessEntityNos);
                            //销售订单保存成功后删除Redis中的数据
                            RemoveRedisData(this.K3CloudContext, result.SuccessEntityNos);
                        }
                        result = ExecuteOperateAfterSave(this.K3CloudContext, result, logs, orders);
                        //提交事务
                        trans.Complete();
                       
                    }
                    return result;
                }
                else if (operationType == SynOperationType.UPDATE)
                {
                    using (trans = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
                    {
                        result = new HttpResponseResult();
                        result.Message += "销售订单【" + string.Join(",", orders.Select(o => o.FBillNo).ToList<string>()) + "】在系统已经存在！";

                        trans.Complete();
                    }
                }
                else if (operationType == SynOperationType.CANCEL_AFTER_SAVE)
                {
                    using (trans = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
                    {
                        HttpResponseResult saveResult = ExecuteOperate(SynOperationType.SAVE, null, null, bizData.ToString());
                        WriteSynchroFailLog(orders, saveResult.SuccessEntityNos);
                        if (saveResult != null)
                        {
                            //同步成功后删除Redis中的数据
                            RemoveRedisData(this.K3CloudContext, saveResult.SuccessEntityNos);
                        }

                        List<string> originalNos = GetOriginalSalOrderNos(saveResult, orders);
                        int count = UpdateCancelSalOrder(this.K3CloudContext, saveResult, orders);

                        //result = ExecuteOperate(SynOperationType.CANCEL, null, null);
                        result = ExecuteOperateAfterSave(this.K3CloudContext, saveResult, logs, orders);

                        trans.Complete();
                    }
                }
            }
            catch (Exception ex)
            {
                LogUtils.WriteSynchroLog(this.K3CloudContext, SynchroDataType.SaleOrder, "数据批量更新过程中出现异常，异常信息：" + ex.Message + System.Environment.NewLine + ex.StackTrace);
                result = new HttpResponseResult();
                result.Success = false;
                result.Message = ex.Message;

                if (logs != null && logs.Count > 0)
                {
                    foreach (var log in logs)
                    {
                        log.IsSuccess = 0;
                        log.ErrInfor = ex.Message + System.Environment.NewLine + ex.StackTrace;
                    }
                }
            }
            finally
            {
                if (trans != null)
                {
                    trans.Dispose();
                }
            }

            if (result == null)
            {
                return null;
            }

            if (result.Success == false && result.FailedResult == null && result.Result == null)
            {
                //同步出现错误之类：如令牌错误，url错误之类的
                //log.IsSuccess = 0;
                //log.ErrInfor = "数据同步失败：" + result.Message == null ? "" : result.Message;

                return result;
            }
            return result;
        }

        /// <summary>
        /// 更新客户下单次数
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="orderNos"></param>
        /// <returns></returns>
        public int StatisticsOrderCount(Context ctx, List<string> orderNos)
        {
            int uCount = 0;
            try
            {
                if (orderNos != null && orderNos.Count > 0)
                {
                    foreach (var orderNo in orderNos)
                    {
                        if (!string.IsNullOrWhiteSpace(orderNo))
                        {
                            string sCustId = string.Format(@"/*dialect*/ select F_HS_B2CCUSTID from T_SAL_ORDER where FBILLNO = '{0}'", orderNo);
                            int custId = Convert.ToInt32(SQLUtils.GetObject(ctx, sCustId, "F_HS_B2CCUSTID"));
                            string custNo = SQLUtils.GetCustomerNo(ctx, custId);

                            string uSql = string.Format(@"/*dialect*/ update T_BD_CUSTOMER set F_HS_OrderQty = F_HS_OrderQty + 1 where FNumber = '{0}'", custNo);
                            uCount = DBUtils.Execute(ctx, uSql);

                            if (uCount > 0)
                            {
                                LogUtils.WriteSynchroLog(ctx, SynchroDataType.CustomerOrderQty, "客户【" + custNo + "】下单成功,单号【" + orderNo + "】");
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                LogUtils.WriteSynchroLog(ctx, SynchroDataType.CustomerOrderQty, ex.Message + System.Environment.NewLine + ex.StackTrace + System.Environment.NewLine);
            }

            return uCount;
        }

        /// <summary>
        /// 条件查询订单
        /// </summary>
        /// <param name="orders"></param>
        /// <param name="orderNos"></param>
        /// <returns></returns>
        public List<K3SalOrderInfo> GetSelectedSalOrders(IEnumerable<K3SalOrderInfo> orders, List<string> orderNos)
        {
            if (orders != null && orders.Count() > 0)
            {
                if (orderNos != null && orderNos.Count > 0)
                {
                    var result = from o in orders
                                 where (orderNos.ToArray().Contains(o.FBillNo))
                                 select o;

                    if (result != null && result.Count() > 0)
                    {
                        return result.ToList<K3SalOrderInfo>();
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 根据销售订单付款状态搜索销售订单
        /// </summary>
        /// <param name="orders"></param>
        /// <param name="paymentStatus"></param>
        /// <returns></returns>
        private List<K3SalOrderInfo> GetOrdersByPaymentStatus(List<K3SalOrderInfo> orders, string paymentStatus)
        {
            if (!string.IsNullOrWhiteSpace(paymentStatus))
            {
                if (orders != null && orders.Count > 0)
                {
                    var result = from o in orders
                                 where o.F_HS_PaymentStatus != null && o.F_HS_PaymentStatus.Equals(paymentStatus)
                                 select o;
                    if (result != null && result.Count() > 0)
                    {
                        return result.ToList<K3SalOrderInfo>();
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 销售订单同步保存成功后执行提交，审核，收款单同步的操作
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="result"></param>
        /// <param name="logs"></param>
        /// <param name="orders"></param>
        /// <returns></returns>
        private HttpResponseResult ExecuteOperateAfterSave(Context ctx, HttpResponseResult result, List<SynchroLog> logs, List<K3SalOrderInfo> orders)
        {
            List<K3SalOrderInfo> auditOrders = null;

            List<string> successNos = GetSavedAndPayedSalOrderNos(result, orders);

            if (successNos != null && successNos.Count > 0)
            {
                result = ExecuteOperate(SynOperationType.SUBMIT, successNos);

                if (result != null && result.SuccessEntityNos != null && result.SuccessEntityNos.Count > 0)
                {
                    result = ExecuteOperate(SynOperationType.AUDIT, result.SuccessEntityNos);

                    if (result != null)
                    {
                        if (result.SuccessEntityNos != null && result.SuccessEntityNos.Count > 0)
                        {
                            if (ctx == null)
                            {
                                LogUtils.WriteSynchroLog(this.K3CloudContext, this.DataType, "ctx为空");
                            }
                            //统计客户下单次数
                            StatisticsOrderCount(ctx, result.SuccessEntityNos);
                            //销售订单审核成功后同步收款单
                            auditOrders = GetSelectedSalOrders(orders, result.SuccessEntityNos);
                        }
                        else
                        {
                            if (!result.Success)
                            {
                                auditOrders = GetSelectedSalOrders(orders, GetAuditedSalOrderNos(ctx, orders));
                            }
                        }
                    }
                }
            }

            //纯余额支付不生成收款单
            if (auditOrders != null && auditOrders.Count > 0)
            {
                //收款单同步条件：1.PayedTotal为空 2.PayedTotal>0
                var g = from o in auditOrders
                        where string.IsNullOrWhiteSpace(o.F_HS_PayTotal) || Convert.ToDecimal(o.F_HS_PayTotal) > 0
                        select o;
                auditOrders = g.ToList();

                if (auditOrders != null)
                {
                    Dictionary<SynOperationType, IEnumerable<AbsSynchroDataInfo>> dict = new Dictionary<SynOperationType, IEnumerable<AbsSynchroDataInfo>>();
                    dict.Add(SynOperationType.SAVE, auditOrders);

                    SynchroDataHelper.SynchroDataToK3(this.K3CloudContext, SynchroDataType.ReceiveBill, true, null, dict);
                }
            }

            return result;
        }

        /// <summary>
        /// 当销售订单审核过程出现异常，销售订单已审核成功但WebApi不能返回审核成功的销售订单编码，需要查询验证已审核成功的销售订单编码
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="orders"></param>
        /// <returns></returns>
        public List<string> GetAuditedSalOrderNos(Context ctx, List<K3SalOrderInfo> orders)
        {
            List<string> orderNos = null;
            if (orders != null && orders.Count > 0)
            {
                orderNos = orders.Select(o => o.FBillNo).ToList();

                if (orderNos != null && orderNos.Count > 0)
                {
                    string sql = string.Format(@"/*dialect*/ select FBILLNO from T_SAL_ORDER where FDOCUMENTSTATUS = 'C' and FBILLNO in ('{0}')", string.Join("','", orderNos));
                    DynamicObjectCollection coll = SQLUtils.GetObjects(ctx, sql);

                    return coll.Select(o => SQLUtils.GetFieldValue(o, "FBILLNO")).ToList();
                }
            }

            return null;
        }

        /// <summary>
        /// 获取合单后原销售订单编码
        /// </summary>
        /// <param name="result"></param>
        /// <param name="orders"></param>
        /// <returns></returns>
        private List<string> GetOriginalSalOrderNos(HttpResponseResult result, List<K3SalOrderInfo> orders)
        {
            List<string> numbers = null;

            if (result != null)
            {
                List<string> successNos = result.SuccessEntityNos;

                List<K3SalOrderInfo> selectedOrders = GetSelectedSalOrders(orders, successNos);

                if (selectedOrders != null && selectedOrders.Count > 0)
                {
                    numbers = new List<string>();

                    foreach (var order in selectedOrders)
                    {
                        if (order != null)
                        {
                            if (order.OriginalOrderNos != null && order.OriginalOrderNos.Count > 0)
                            {
                                numbers.AddRange(order.OriginalOrderNos);
                            }
                        }
                    }
                }
            }

            return numbers;
        }

        /// <summary>
        /// 记录同步失败的销售订单
        /// </summary>
        /// <param name="orders"></param>
        /// <param name="successNos"></param>
        /// <returns></returns>
        private int WriteSynchroFailLog(List<K3SalOrderInfo> orders, List<string> successNos)
        {
            int count = 0;

            if (orders != null && orders.Count > 0)
            {
                List<string> allNos = orders.Select(o => o.FBillNo).ToList<string>();

                if (allNos != null && successNos != null)
                {
                    List<string> failNos = allNos.Except(successNos).ToList<string>();

                    if (failNos != null && failNos.Count > 0)
                    {
                        var failOrders = from o in orders
                                         where failNos.Contains(o.FBillNo)
                                         select o;
                        if (failOrders != null && failOrders.Count() > 0)
                        {
                            List<K3SalOrderInfo> synFailorders = failOrders.ToList<K3SalOrderInfo>();
                            count = LogHelper.WriteSynFailOrdersInfo(this.K3CloudContext, synFailorders);
                        }
                    }
                }
            }
            return count;
        }

        /// <summary>
        /// 获取保存成功并已经付款的销售订单
        /// </summary>
        /// <param name="orders"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private List<string> GetSavedAndPayedSalOrderNos(HttpResponseResult result, List<K3SalOrderInfo> orders)
        {
            if (result != null)
            {
                if (orders != null && orders.Count > 0)
                {
                    List<string> payedNos = null;//已付款订单号编码列表
                    if (GetOrdersByPaymentStatus(orders, "3") != null && GetOrdersByPaymentStatus(orders, "3").Count > 0)
                    {
                        payedNos = GetOrdersByPaymentStatus(orders, "3").Select(o => o.FBillNo).ToList<string>();
                    }

                    if (result.SuccessEntityNos != null && result.SuccessEntityNos.Count > 0)
                    {
                        if (payedNos != null)
                        {
                            return result.SuccessEntityNos.Intersect(payedNos).ToList<string>();
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 更新作废的销售订单【合并或修改新单编码】字段
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="result"></param>
        /// <param name="orders"></param>
        /// <returns></returns>
        private int UpdateCancelSalOrder(Context ctx, HttpResponseResult result, List<K3SalOrderInfo> orders)
        {
            int count = 0;

            if (result != null)
            {
                if (result.SuccessEntityNos != null && result.SuccessEntityNos.Count > 0)
                {
                    List<K3SalOrderInfo> newOrders = GetSelectedSalOrders(orders, result.SuccessEntityNos);

                    if (newOrders != null && newOrders.Count > 0)
                    {
                        foreach (var order in newOrders)
                        {
                            if (order != null)
                            {
                                try
                                {
                                    string sql = string.Format(@" /*dialect*/ update T_SAL_ORDER set F_HS_NEWMERORMODBILLNO = '{0}' where FBillNo in ('{1}')", order.FBillNo, string.Join("','", order.OriginalOrderNos));
                                    count = DBUtils.Execute(ctx, sql);
                                }
                                catch (Exception ex)
                                {
                                    LogUtils.WriteSynchroLog(ctx, SynchroDataType.SaleOrder, "更新作废销售订单【合并或修改新单编码】字段发生异常：单号【" + string.Join(",", order.OriginalOrderNos) + "】" + System.Environment.NewLine + ex.Message + System.Environment.NewLine + ex.StackTrace);
                                }
                            }
                        }

                    }
                }
            }
            return count;
        }

        public override AbsSynchroDataInfo BuildSynchroData(Context ctx, string json, AbsSynchroDataInfo data = null)
        {
            JObject jobj = JsonUtils.ParseJson2JObj(ctx, SynchroDataType.SaleOrder, json);
            K3SalOrderInfo so = null;
            decimal amount = 0;

            if (jobj != null)
            {
                if (JsonUtils.GetFieldValue(jobj, "products_info") != null)
                {
                    string orderSource = JsonUtils.GetFieldValue(jobj, "orders_status");//订单来源

                    if ("1".Equals(orderSource) || "2".Equals(orderSource))
                    {
                        #region
                        so = new K3SalOrderInfo();
                        so.SrcNo = JsonUtils.GetFieldValue(jobj, "orders_id");
                        so.FBillNo = JsonUtils.GetFieldValue(jobj, "orders_id");


                        so.FSaleOrgId = SQLUtils.GetOrgNo(ctx, 100035);

                        string sellerNo = JsonUtils.GetFieldValue(jobj, "account_manager_id");
                        so.FSalerId = JsonUtils.HtmlESC(sellerNo);
                        so.FSalerId = SQLUtils.GetSellerNo(ctx, so.FSalerId);

                        so.FSaleDeptId = SQLUtils.GetDeptNo(ctx, so.FSalerId);//部门编码

                        string custNo = JsonUtils.GetFieldValue(jobj, "customers_id");//客户编码

                        if (string.IsNullOrWhiteSpace(custNo))
                        {
                            LogUtils.WriteSynchroLog(ctx, SynchroDataType.SaleOrder, "销售订单订单号[" + so.FBillNo + "] 的客户编码为空！");
                        }

                        so.FCustAddress = JsonUtils.GetFieldValue(jobj, "customer_address");//客户地址
                        so.FCustCompany = JsonUtils.GetFieldValue(jobj, "customers_company");//客户公司
                        so.FCustEmail = JsonUtils.GetFieldValue(jobj, "customers_email_address");//客户邮箱
                        so.FCustName = JsonUtils.GetFieldValue(jobj, "customers_name");//客户名称
                        so.FCustPhone = JsonUtils.GetFieldValue(jobj, "customers_telephone");//客户固话

                        so.OrderStatus = JsonUtils.GetFieldValue(jobj, "orders_status");//订单状态
                        so.FBillTypeId = "XSDD01_SYS";//单据类型
                        so.F_HS_RateToUSA = Convert.ToDecimal(JsonUtils.GetFieldValue(jobj, "currency_value"));//兑美元汇率
                        so.F_HS_RateToUSA = so.F_HS_RateToUSA;

                        if ("0".Equals(JsonUtils.GetFieldValue(jobj, "is_mobile_orders"))) //订单下单渠道
                        {
                            so.F_HS_Channel = "DND";
                        }
                        else if ("2".Equals(JsonUtils.GetFieldValue(jobj, "is_mobile_orders")))
                        {
                            so.F_HS_Channel = "YDD";
                        }


                        //只同步状态1和2的
                        if ("1".Equals(orderSource))
                        {
                            so.F_HS_SaleOrderSource = "HCWebPendingOder";//订单来源
                            so.F_HS_PaymentStatus = "2";//未付款
                        }
                        else if ("2".Equals(orderSource))
                        {
                            so.F_HS_SaleOrderSource = "HCWebProcessingOder";//订单来源
                            so.F_HS_PaymentStatus = "3";//已到款
                        }

                        so.FNote = JsonUtils.GetFieldValue(jobj, "comments");//订单用户备注
                        string auShipMethod = JsonUtils.GetFieldValue(jobj, "shipping_method_cheapest_au");//澳洲免邮时最便宜渠道

                        if (!string.IsNullOrWhiteSpace(auShipMethod))
                        {
                            so.FNote += Environment.NewLine + string.Format("【澳洲免邮时最便宜渠道{0}】", auShipMethod);
                        }

                        DateTime purchaseDate = Convert.ToDateTime(JsonUtils.GetFieldValue(jobj, "date_purchased")); //原始订单日期

                        so.FDate = purchaseDate.AddHours(13);//订单北京时间
                        so.F_HS_USADatetime = purchaseDate;//订单美国时间
                        so.FCreateDate = so.FDate;//订单创建时间
                        so.FSettleCurrId = JsonUtils.GetFieldValue(jobj, "currency");//结算币别
                        so.FSettleModeId = JsonUtils.GetFieldValue(jobj, "payment_method");//结算方式
                        so.F_HS_PaymentModeNew = JsonUtils.GetFieldValue(jobj, "payment_method");

                        so.F_HS_MobilePhone = JsonUtils.GetFieldValue(jobj, "customers_telephone");//移动电话
                        so.F_HS_CouponAmount = Math.Round(Convert.ToDecimal(JsonUtils.GetFieldValue(jobj, "ot_coupon")) * so.F_HS_RateToUSA, 2);//使用优惠券金额
                        so.F_HS_Points = Convert.ToDecimal(JsonUtils.GetFieldValue(jobj, "ot_reward_points")) * 100;//使用积分数
                        so.F_HS_IntegralDeduction = Math.Round(Convert.ToDecimal(JsonUtils.GetFieldValue(jobj, "ot_reward_points")) * so.F_HS_RateToUSA, 2);//使用积分金额
                        so.F_HS_Shipping = Math.Round(Convert.ToDecimal(JsonUtils.GetFieldValue(jobj, "ot_shipping")) * so.F_HS_RateToUSA, 2);//运费
                        so.F_HS_ShippingDiscount = Math.Round((Convert.ToDecimal(JsonUtils.GetFieldValue(jobj, "ot_shipping_discount_hs")) + Convert.ToDecimal(JsonUtils.GetFieldValue(jobj, "ot_shipping_discount"))) * so.F_HS_RateToUSA, 2);//运费折扣
                        so.F_HS_Subtotal = Math.Round(Convert.ToDecimal(JsonUtils.GetFieldValue(jobj, "ot_subtotal")) * so.F_HS_RateToUSA, 2);//订单总金额

                        decimal total = Convert.ToDecimal(JsonUtils.GetFieldValue(jobj, "ot_total")) * so.F_HS_RateToUSA;//优惠后订单金额（包括运费）

                        if (so.FSettleCurrId.CompareTo("JPY") == 0)
                        {
                            so.F_HS_Total = Math.Truncate(total);//优惠后订单金额（包括运费）
                        }
                        else
                        {
                            so.F_HS_Total = total;
                        }

                        string shippingType = JsonUtils.GetFieldValue(jobj, "shipping_method");
                        so.F_HS_ShippingMethod = JsonUtils.HtmlESC(shippingType);//发货方式
                        so.F_HS_DeclareRqstd = JsonUtils.HtmlESC(JsonUtils.GetFieldValue(jobj, "declare_value"));//客户申报要求

                        so.F_HS_IsSameAdress = IsSameAddress(ctx, so, jobj);//客户地址信息和账单地址对比
                        so.F_HS_TransactionID = JsonUtils.GetFieldValue(jobj, "transaction_id");//交易流水号
                        so.F_HS_EachShipmentFreight = JsonUtils.GetFieldValue(jobj, "shipping_cost");//各发货仓运费(USD)

                        so.F_HS_BrandDiscountAmount = Math.Round(Convert.ToDecimal(string.IsNullOrWhiteSpace(JsonUtils.GetFieldValue(jobj, "ot_brand_discount")) ? "0" : JsonUtils.GetFieldValue(jobj, "ot_brand_discount")) * so.F_HS_RateToUSA, 2);//品牌折扣抵扣额
                        so.F_HS_CombinedDiscountAmount = Math.Round(Convert.ToDecimal(string.IsNullOrWhiteSpace(JsonUtils.GetFieldValue(jobj, "ot_combination_products_discount")) ? "0" : JsonUtils.GetFieldValue(jobj, "ot_combination_products_discount")) * so.F_HS_RateToUSA, 2);//组合产品折扣抵扣额
                        so.F_HS_OrderDiscountAmountNew = Math.Round(Convert.ToDecimal(string.IsNullOrWhiteSpace(JsonUtils.GetFieldValue(jobj, "ot_order_discount")) ? "0" : JsonUtils.GetFieldValue(jobj, "ot_order_discount")) * so.F_HS_RateToUSA, 2);//订单折扣额
                        so.OriginalOrderNos = ServiceHelper.GetItems(JsonUtils.GetFieldValue(jobj, "coalescent_order"));//订单合单前的原单
                        so.F_HS_MergeSrcBillNo = JsonUtils.GetFieldValue(jobj, "coalescent_order");//合并单据源单编码
                        so.F_HS_PlatformCustomerEmail = JsonUtils.GetFieldValue(jobj,"customers_email_address");//平台客户邮箱

                        so.F_HS_SZHKMergeNote = string.Empty;//深圳香港仓合并备注
                        string mergeMode = JsonUtils.GetFieldValue(jobj, "merge_mode");//SZHK合并备注
                        string finalWeight = JsonUtils.GetFieldValue(jobj, "final_weight");//
                        string freeMergeDhl = JsonUtils.GetFieldValue(jobj, "free_merge_dhl");//
                        string weight_range_start = JsonUtils.GetFieldValue(jobj, "weight_range_start");
                        string weight_range_end = JsonUtils.GetFieldValue(jobj, "weight_range_end");
                        string merge_weight = JsonUtils.GetFieldValue(jobj, "merge_weight");

                        if (!string.IsNullOrWhiteSpace(mergeMode))
                        {
                            if (mergeMode.CompareTo("1") == 0)
                            {
                                if (!string.IsNullOrWhiteSpace(so.F_HS_SZHKMergeNote))
                                {
                                    so.F_HS_SZHKMergeNote += Environment.NewLine + "SZLiquid两仓合并";
                                }
                                else
                                {
                                    so.F_HS_SZHKMergeNote += "SZLiquid两仓合并";
                                }
                            }
                            else if (mergeMode.CompareTo("2") == 0)
                            {
                                if (!string.IsNullOrWhiteSpace(so.F_HS_SZHKMergeNote))
                                {
                                    so.F_HS_SZHKMergeNote += Environment.NewLine + "SZHK两仓合并";
                                }
                                else
                                {
                                    so.F_HS_SZHKMergeNote += "SZHK两仓合并";
                                }
                            }
                        }
                        if (!string.IsNullOrWhiteSpace(freeMergeDhl))
                        {
                            if (freeMergeDhl.CompareTo("1") == 0)
                            {
                                if (!string.IsNullOrWhiteSpace(so.FNote))
                                {
                                    so.F_HS_SZHKMergeNote += Environment.NewLine + "-免邮发DHL";
                                }
                                else
                                {
                                    so.F_HS_SZHKMergeNote += "-免邮发DHL";
                                }
                            }
                        }
                        if (!string.IsNullOrWhiteSpace(so.F_HS_SZHKMergeNote))
                        {
                            if (!string.IsNullOrWhiteSpace(finalWeight))
                            {
                                if (Convert.ToDecimal(finalWeight) > 0)
                                {
                                    string rang = string.Format("【{0}-{1}】,", weight_range_start, weight_range_end);
                                    string merge_weight_msg = string.Format("两仓合并重量:{0}", merge_weight);

                                    if (!string.IsNullOrWhiteSpace(so.F_HS_SZHKMergeNote))
                                    {
                                        so.F_HS_SZHKMergeNote += Environment.NewLine + ",增重到" + finalWeight + "g," + rang + merge_weight_msg;
                                    }
                                    else
                                    {
                                        so.F_HS_SZHKMergeNote += ",增重到" + finalWeight + "g," + rang + merge_weight_msg;
                                    }
                                }
                            }
                        }

                        so.F_HS_PayTotal = JsonUtils.GetFieldValue(jobj, "pay_total");

                        if (!string.IsNullOrWhiteSpace(so.F_HS_PayTotal))
                        {
                            so.F_HS_BalancePayments = so.F_HS_Total - Convert.ToDecimal(so.F_HS_PayTotal) * so.F_HS_RateToUSA;
                            so.F_HS_USDBalancePayments = Convert.ToDecimal(JsonUtils.GetFieldValue(jobj, "ot_total")) - Convert.ToDecimal(so.F_HS_PayTotal);
                        }
                        else
                        {
                            so.F_HS_BalancePayments = 0;
                            so.F_HS_USDBalancePayments = 0;
                        }

                        so.F_HS_ProductDiscountAmount = Convert.ToDecimal(JsonUtils.GetFieldValue(jobj, "ot_single_discount")) * so.F_HS_RateToUSA;//整单折扣额(内网创建单才有)
                        so.F_HS_FreightDiscountAmount = Convert.ToDecimal(JsonUtils.GetFieldValue(jobj, "ot_freight_discount")) * so.F_HS_RateToUSA;//运费折扣额(内网创建单才有)
                        so.F_HS_DiscountRemarks = JsonUtils.GetFieldValue(jobj, "ot_shipping_discount_hs");//整单产品折扣备注原因


                        if (!string.IsNullOrWhiteSpace(so.F_HS_MergeSrcBillNo))//是否为合并单据
                        {
                            so.F_HS_IsMergeBill = true;
                        }
                        else
                        {
                            so.F_HS_IsMergeBill = false;
                        }

                        if (!string.IsNullOrWhiteSpace(JsonUtils.GetFieldValue(jobj, "use_area_prices")))
                        {
                            so.F_HS_UseLocalPriceList = JsonUtils.GetFieldValue(jobj, "use_area_prices").CompareTo("1") == 0 ? true : false;//是否使用收货地
                        }

                        so.OrderEntry = GetOrderEntrys(ctx, jobj);//订单明细

                        so.F_HS_NeedPayAmount = Convert.ToDecimal(so.F_HS_PayTotal) * so.F_HS_RateToUSA;//需支付金额_结算币别的赋值放在订单明细赋值之后（触发值更新用）

                        SetMaterialPrice(ctx, so, Math.Round(so.F_HS_Total, 2));//消除K3与HC订单总金额的差异
                        amount = GetSalOrderAmount(so, Math.Round(so.F_HS_Total, 2));//销售订单金额
                        

                        so.OrderEntryPlan = GetOrderEntryPlans(so.OrderEntry, jobj);//收货明细

                        if (so.OrderEntry != null && so.OrderEntry.Count > 0)
                        {

                            for (int i = 0; i < so.OrderEntry.Count(); i++)
                            {
                                if ("99.01".CompareTo(so.OrderEntry[i].FMaterialId) != 0)
                                {
                                    so.OrderEntry[i].F_HS_OrgTaxPrice = so.OrderEntry[i].FTAXPRICE * so.F_HS_RateToUSA;//商品原含税单价
                                                                                                                       //so.OrderEntry[i].FTAXPRICE = so.OrderEntry[i].FTAXPRICE * so.Discount * so.F_HS_RateToUSA;//商品折扣后单价
                                    so.OrderEntry[i].FTAXPRICE = so.OrderEntry[i].FTAXPRICE * so.F_HS_RateToUSA;//商品折扣后单价      
                                }
                                else
                                {
                                    so.OrderEntry[i].F_HS_OrgTaxPrice = so.OrderEntry[i].FTAXPRICE * so.F_HS_RateToUSA;//运费原含税单价
                                    so.OrderEntry[i].FTAXPRICE = so.OrderEntry[i].FTAXPRICE * so.F_HS_RateToUSA;//运费不参与折扣运 
                                }

                                //交货明细
                                List<K3SalOrderEntryPlan> entryPlans = new List<K3SalOrderEntryPlan>();
                                K3SalOrderEntryPlan oPlan = new K3SalOrderEntryPlan();

                                if (so.FDate != null)
                                {
                                    oPlan.FPlanDeliveryDate = so.FDate;//计划发货日期（与订单日期一致）
                                    oPlan.FPlanDate = so.FDate;//要货日期（与订单日期一致）
                                    oPlan.FPlanQty = so.OrderEntry[i].FQTY;//订单数量
                                    entryPlans.Add(oPlan);
                                    so.OrderEntry[i].EntryPlans = entryPlans;
                                }

                                if ("Level0".CompareTo(so.OrderEntry[i].F_HS_FGroup) == 0)//零售客户
                                {
                                    so.F_HS_B2CCustId = custNo;//零售客户编码（具体名称）
                                    so.FCustId = "RC";//零售客户统一标记为retail customer
                                }
                                else
                                {
                                    so.F_HS_B2CCustId = custNo;
                                    so.FCustId = custNo;//批发客户
                                }
                            }

                            for (int i = 0; i < so.OrderEntry.Count(); i++)
                            {
                                if (so.OrderEntry[i].F_HS_IsVirtualEntry == true)
                                {
                                    //表头标记是否为虚拟
                                    so.F_HS_IsVirtual = true;
                                    break;
                                }
                                else
                                {
                                    so.F_HS_IsVirtual = false;
                                }
                            }
                        }
                        IsOwegoods(ctx, so);
                        #endregion
                    }
                }
            }

            if (so != null)
            {
                if (Math.Abs(amount - so.F_HS_Total) / so.F_HS_RateToUSA <= 1M)
                {
                    return so;
                }
                else
                {
                    LogUtils.WriteSynchroLog(ctx, SynchroDataType.SaleOrder, "【" + so.FBillNo + "】订单金额与实际严重不符！");
                }
            }

            return null;
            //return so;
        }

        /// <summary>
        /// 获取订单明细列表
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="jobj"></param>
        /// <returns></returns>
        private List<K3SalOrderEntryInfo> GetOrderEntrys(Context ctx, JObject jobj)
        {
            List<K3SalOrderEntryInfo> dataEntrys = new List<K3SalOrderEntryInfo>();
            JArray rows = JArray.Parse(JsonUtils.GetFieldValue(jobj, "products_info"));

            if (rows != null && rows.Count > 0)
            {
                foreach (var item in rows)
                {
                    JObject jObj = item as JObject;
                    List<K3SalOrderEntryInfo> oEntrys = GetOrderEntry(ctx, jObj);

                    if (oEntrys != null && oEntrys.Count > 0)
                    {
                        foreach (var oEntry in oEntrys)
                        {
                            if (oEntry != null)
                            {
                                oEntry.F_HS_FGroup = SetCustomerLevel(JsonUtils.GetFieldValue(jobj, "customers_whole"));
                                oEntry.F_HS_CreateDateEntry = Convert.ToDateTime(JsonUtils.GetFieldValue(jobj, "date_purchased")).AddHours(13);

                                dataEntrys.Add(oEntry);
                            }
                        }
                    }
                }
            }

            //运费当作物料计算
            if (JsonUtils.GetFieldValue(jobj, "ot_shipping") != null && Convert.ToDecimal(JsonUtils.GetFieldValue(jobj, "ot_shipping")) > 0)
            {

                K3SalOrderEntryInfo soEntry = new K3SalOrderEntryInfo();
                soEntry.FMaterialId = "99.01";
                soEntry.FTAXPRICE = Convert.ToDecimal(JsonUtils.GetFieldValue(jobj, "ot_shipping"));//运费信息在JSON的第一层
                soEntry.FQTY = 1;
                soEntry.FStockId = "501";
                soEntry.F_HS_FGroup = SetCustomerLevel(JsonUtils.GetFieldValue(jobj, "customers_whole"));
                soEntry.F_HS_CreateDateEntry = Convert.ToDateTime(JsonUtils.GetFieldValue(jobj, "date_purchased")).AddHours(13);
                dataEntrys.Add(soEntry);
            }

            return dataEntrys;
        }

        /// <summary>
        /// 获取订单明细
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="jObj"></param>
        /// <returns></returns>
        private List<K3SalOrderEntryInfo> GetOrderEntry(Context ctx, JObject jObj)
        {
            Dictionary<string, string> dict = RedisKeyUtils.GetStockNo(ctx, jObj);

            List<K3SalOrderEntryInfo> soEntrys = null;
            K3SalOrderEntryInfo soEntry = null;

            if (dict != null && dict.Count > 0)
            {
                soEntrys = new List<K3SalOrderEntryInfo>();

                foreach (KeyValuePair<string, string> kv in dict)
                {
                    if (!string.IsNullOrEmpty(kv.Key) && !string.IsNullOrWhiteSpace(kv.Value))
                    {
                        if (Convert.ToInt32(JsonUtils.GetFieldValue(jObj, kv.Key)) > 0)
                        {
                            soEntry = new K3SalOrderEntryInfo();

                            if (kv.Key.CompareTo("virtual_quantity") == 0)
                            {
                                soEntry.F_HS_IsVirtualEntry = true;
                            }
                            soEntry.FStockId = kv.Value;
                            soEntry.FQTY = Convert.ToInt32(JsonUtils.GetFieldValue(jObj, kv.Key));

                            SetK3SalOrderEntryInfo(ctx, soEntry, jObj);
                            soEntrys.Add(soEntry);
                        }
                    }
                }
            }

            return soEntrys;
        }

        /// <summary>
        /// 设置订单明细基本信息
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="soEntry"></param>
        /// <param name="jTok"></param>
        private void SetK3SalOrderEntryInfo(Context ctx, K3SalOrderEntryInfo soEntry, JToken jTok)
        {

            soEntry.FMaterialId = JsonUtils.GetFieldValue(jTok, "order_fix_id");
            soEntry.FTAXPRICE = Convert.ToDecimal(JsonUtils.GetFieldValue(jTok, "products_price"));

            soEntry.FUnitId = SQLUtils.GetUnitNo(ctx, soEntry.FMaterialId);

            string giftflag = JsonUtils.GetFieldValue(jTok, "giftflag");

            if (!string.IsNullOrWhiteSpace(giftflag))
            {
                if ("0".Equals(JsonUtils.GetFieldValue(jTok, "giftflag")))//是否为赠品
                {
                    soEntry.FIsFree = false;
                }
                else
                {
                    soEntry.FIsFree = true;
                }
            }
            else
            {
                soEntry.FIsFree = false;
            }

            string clearance = JsonUtils.GetFieldValue(jTok, "is_clearance");//是否清仓

            if (!string.IsNullOrWhiteSpace(clearance))
            {
                if ("0".Equals(clearance))
                {
                    soEntry.F_HS_IsEmptyStock = false;
                    soEntry.F_HS_ProductStatus_GD = "";
                }
                else if ("1".Equals(clearance))
                {
                    soEntry.F_HS_IsEmptyStock = true;
                    soEntry.F_HS_ProductStatus_GD = "SPQC";
                }
            }
            else
            {
                soEntry.F_HS_IsEmptyStock = false;
            }

            soEntry.F_HS_FGroup = SetCustomerLevel(JsonUtils.GetFieldValue(jTok, "customers_whole"));
            soEntry.F_HS_YNOnLine = true;
            SetProductDiscount(ctx, soEntry, jTok);
        }

        /// <summary>
        /// 销售订单产组合产品折扣额和品牌折扣额
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="soEntry"></param>
        /// <param name="jTok"></param>
        public void SetProductDiscount(Context ctx, K3SalOrderEntryInfo soEntry, JToken jTok)
        {
            string remark = JsonUtils.GetFieldValue(jTok, "discount_remark");
            JObject dis = null;

            if (!string.IsNullOrWhiteSpace(remark) && !remark.Contains("[]"))
            {
                dis = JObject.Parse(JsonUtils.GetFieldValue(jTok, "discount_remark"));
            }

            if (dis != null)
            {
                decimal braProDis = Convert.ToDecimal(JsonUtils.GetFieldValue(dis, "brand_discount"));
                decimal comProDis = Convert.ToDecimal(JsonUtils.GetFieldValue(dis, "combination_products_discount"));

                if (soEntry != null && soEntry.FTAXPRICE > 0)
                {
                    if (braProDis > 0)
                    {
                        soEntry.F_HS_BrandDiscountRate = braProDis / soEntry.FTAXPRICE * 100;
                    }
                    if (comProDis > 0)
                    {
                        soEntry.F_HS_CombineDiscountRate = comProDis / soEntry.FTAXPRICE * 100;
                    }
                }
            }
        }

        /// <summary>
        /// 收货明细
        /// </summary>
        /// <param name="oEntries"></param>
        /// <param name="jObj"></param>
        /// <returns></returns>
        public static List<K3SalOrderEntryPlan> GetOrderEntryPlans(List<K3SalOrderEntryInfo> oEntries, JObject jObj)
        {
            List<K3SalOrderEntryPlan> oPlans = null;
            if (oEntries != null && oEntries.Count > 0)
            {
                oPlans = new List<K3SalOrderEntryPlan>();

                foreach (var oEntry in oEntries)
                {
                    K3SalOrderEntryPlan oPlan = new K3SalOrderEntryPlan();
                    oPlan.FPlanDeliveryDate = Convert.ToDateTime(JsonUtils.GetFieldValue(jObj, "date_purchased"));
                    oPlans.Add(oPlan);
                }

            }
            return oPlans;
        }

        /// <summary>
        /// 判断是否有欠货历史记录
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="order"></param>
        private static void IsOwegoods(Context ctx, K3SalOrderInfo order)
        {

            string sql = string.Format(@"/*dialect*/ select d.FNUMBER,g.Fnumber FStockNo,h.FNUMBER CustomerNo,sum(c.FREMAINOUTQTY) FQty from T_SAL_ORDER a 
		                                            inner join T_SAL_ORDERENTRY b on a.FID=b.FID 
		                                            inner join T_SAL_ORDERENTRY_R c on a.fid=c.fid and b.fentryID=c.fentryID
		                                            inner join T_BD_MATERIAL d on b.FMATERIALID=d.FMATERIALID
		                                            inner join T_BD_STOCK e on b.F_HS_STOCKID=e.FSTOCKID
                                                    inner join T_BAS_ASSISTANTDATAENTRY_L f on e.F_HS_DLC=f.FENTRYID
		                                            inner join T_BAS_ASSISTANTDATAENTRY g on f.FentryID=g.FentryID
		                                            inner join T_BD_CUSTOMER h on a.FCUSTID=h.FCUSTID
		                                            where a.FCLOSESTATUS<>'B' AND a.FCANCELSTATUS<>'B' AND b.FMRPCLOSESTATUS<>'B' and d.fnumber<>'99.01' and  h.FNUMBER = '{0}'
													and a.F_HS_DeliveryAddress = '{1}' and a.F_HS_RecipientCountry = '{2}' and a.F_HS_DeliveryProvinces = '{3}' and a.F_HS_DeliveryCity = '{4}'
		                                            group by d.FNUMBER,g.Fnumber,h.FNUMBER", SQLUtils.DealQuotes(order.F_HS_B2CCustId), SQLUtils.DealQuotes(order.F_HS_DeliveryAddress), SQLUtils.DealQuotes(order.F_HS_RecipientCountry)
                                                                                           , SQLUtils.DealQuotes(order.F_HS_DeliveryProvinces), SQLUtils.DealQuotes(order.F_HS_DeliveryProvinces), SQLUtils.DealQuotes(order.F_HS_DeliveryCity));

            DynamicObjectCollection coll = SQLUtils.GetObjects(ctx, sql);

            if (coll != null && coll.Count() > 0)
            {
                order.FNote += System.Environment.NewLine + "【有欠货历史记录】";
            }
        }

        /// <summary>
        /// 销售订单保存前模拟K3Cloud计算订单总金额
        /// </summary>
        /// <param name="order"></param>
        /// <param name="hcAmount"></param>
        /// <returns></returns>
        private static decimal GetSalOrderAmount(K3SalOrderInfo order, decimal hcAmount)
        {
            decimal amount = 0;

            if (order != null)
            {
                if (order.OrderEntry != null && order.OrderEntry.Count > 0)
                {
                    foreach (var entry in order.OrderEntry)
                    {
                        if (entry != null)
                        {
                            decimal eachAmount = Math.Round(entry.FTAXPRICE * entry.FQTY * order.F_HS_RateToUSA, 2);

                            if (entry.FMaterialId.CompareTo("99.01") != 0)
                            {
                                decimal afterDisAmount = eachAmount - (Math.Round(eachAmount * Math.Round(entry.FDiscountRate / 100, 6), 2));
                                amount += afterDisAmount;
                            }
                            else
                            {
                                decimal afterDisAmount = eachAmount - (Math.Round(eachAmount * Math.Round(order.F_HS_WebFreightDiscountRate / 100, 6), 2));
                                amount += afterDisAmount;
                            }
                        }
                    }
                }
            }
            if (order.FSettleCurrId.CompareTo("JPY") == 0)
            {
                if (Math.Round(amount) == hcAmount)
                {
                    return Math.Round(amount);
                }
                else
                {
                    return Math.Truncate(amount);
                }
            }
            return amount;
        }

        /// <summary>
        /// 比较销售订单在HC网站和K3Cloud间总金额的差异
        /// </summary>
        /// <param name="order"></param>
        /// <param name="hcAmount"></param>
        /// <returns></returns>
        private decimal GetDiffAmount(K3SalOrderInfo order, decimal hcAmount)
        {
            decimal k3Amount = GetSalOrderAmount(order, hcAmount);
            decimal diffAount = k3Amount - Math.Round(hcAmount, 2);
            return diffAount / order.F_HS_RateToUSA;
        }
        private void GetDiffRate(K3SalOrderInfo order, decimal hcAmount)
        {
            decimal diff = GetDiffAmount(order, hcAmount);

            if (diff > 0)
            {
                order.F_HS_CouponAmount -= diff;
            }
            else
            {
                order.F_HS_CouponAmount += diff;
            }
        }

        /// <summary>
        /// 将HC网站和K3Cloud间的总金额差异分摊在订单明细首个商品的单价中
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="order"></param>
        /// <param name="hcAmount"></param>
        private void SetMaterialPrice(Context ctx, K3SalOrderInfo order, decimal hcAmount)
        {
            int count = 0;
            decimal allowDiff = 0.3M;

            if (order == null)
            {
                return;
            }
            else
            {
                if (order.OrderEntry == null || order.OrderEntry.Count <= 0)
                {
                    LogUtils.WriteSynchroLog(ctx, this.DataType, "订单号【" + order.FBillNo + "】的商品个数为零！");
                    return;
                }
                if (order.OrderEntry.Count == 1 && order.OrderEntry.ElementAt(0).FMaterialId.CompareTo("99.01") == 0)
                {
                    LogUtils.WriteSynchroLog(ctx, this.DataType, "订单号【" + order.FBillNo + "】的商品个数为零！");
                    return;
                }
            }

            decimal diffAmount = GetDiffAmount(order, hcAmount);

            if (diffAmount == 0 || (diffAmount <= allowDiff && diffAmount > 0))
            {
                return;
            }

            if (Math.Abs(diffAmount) < allowDiff)
            {
                while (true)
                {
                    for (int i = 0; i < order.OrderEntry.Count; i++)
                    {
                        if (order.OrderEntry[i] != null)
                        {
                            if (order.OrderEntry[i].FMaterialId.CompareTo("99.01") != 0)
                            {
                                order.OrderEntry[i].FTAXPRICE += -diffAmount * order.F_HS_RateToUSA / (1 - order.OrderEntry[i].FDiscountRate / 100) / order.OrderEntry[i].FQTY / order.F_HS_RateToUSA;
                                break;
                            }
                        }
                    }
                    diffAmount = GetDiffAmount(order, hcAmount);

                    if (diffAmount == 0 || (diffAmount <= allowDiff && diffAmount > 0))
                    {
                        break;
                    }
                    count++;

                    if (count == 5)
                    {
                        break;
                    }

                }
            }
            else
            {
                LogUtils.WriteSynchroLog(ctx, this.DataType, "订单号【" + order.FBillNo + "】的金额与实际严重不符！");
                return;
            }
        }

        /// <summary>
        /// 设置客户等级
        /// </summary>
        /// <param name="custLevel"></param>
        /// <returns></returns>
        public string SetCustomerLevel(string custLevel)
        {
            if (!string.IsNullOrEmpty(custLevel))
            {
                switch (custLevel)
                {
                    case "1":
                        return "Level1";
                    case "2":
                        return "Level2";
                    case "3":
                        return "Level3";
                    case "4":
                        return "Level4";
                    case "5":
                        return "Level5";
                    case "6":
                        return "Level6";
                    case "7":
                        return "Level7";
                    case "8":
                        return "Level8";
                    case "0":
                        return "Level0";
                }

            }
            return null;
        }

        /// <summary>
        /// 比较收货地址与账单地址是否相同
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="so"></param>
        /// <param name="jObj"></param>
        /// <returns></returns>
        public bool IsSameAddress(Context ctx, K3SalOrderInfo so, JObject jObj)
        {
            so.F_HS_DeliveryProvinces = JsonUtils.GetFieldValue(jObj, "delivery_state");//交货省份/州
            so.F_HS_DeliveryName = JsonUtils.GetFieldValue(jObj, "delivery_name");//交货联系人
            so.F_HS_DeliveryCity = JsonUtils.GetFieldValue(jObj, "delivery_city");//交货城市
            so.F_HS_PostCode = JsonUtils.GetFieldValue(jObj, "delivery_postcode");//交货邮编
            so.F_HS_RecipientCountry = JsonUtils.GetFieldValue(jObj, "delivery_country");//国家

            string dsAddr = JsonUtils.GetFieldValue(jObj, "delivery_street_address");
            string dSuburb = JsonUtils.GetFieldValue(jObj, "delivery_suburb");

            so.F_HS_DeliveryAddress = dsAddr + System.Environment.NewLine
                                    + dSuburb; //交货详细地址


            string billName = JsonUtils.GetFieldValue(jObj, "billing_name");
            string billAddress = JsonUtils.GetFieldValue(jObj, "billing_street_address");
            string billSburb = JsonUtils.GetFieldValue(jObj, "billing_suburb");
            string billCity = JsonUtils.GetFieldValue(jObj, "billing_city");
            string billPostCode = JsonUtils.GetFieldValue(jObj, "billing_postcode");
            string billState = JsonUtils.GetFieldValue(jObj, "billing_state");
            string billCountry = JsonUtils.GetFieldValue(jObj, "billing_country");

            //账单地址
            string billDetailAddress = billName + System.Environment.NewLine
                                + billAddress + System.Environment.NewLine
                                + billSburb + System.Environment.NewLine
                                + billCity + " "
                                + billPostCode + System.Environment.NewLine
                                + billState + System.Environment.NewLine
                                + billCountry + System.Environment.NewLine;

            so.F_HS_BillAddress = billDetailAddress;

            return JsonUtils.CompareString(so.F_HS_DeliveryName, billName)
                    && JsonUtils.CompareString(so.F_HS_PostCode, billPostCode)
                    && JsonUtils.CompareString(dsAddr, billAddress)
                    && JsonUtils.CompareString(dSuburb, billSburb)
                    && JsonUtils.CompareString(so.F_HS_DeliveryCity, billCity)
                    && JsonUtils.CompareString(so.F_HS_DeliveryProvinces, billState)
                    && JsonUtils.CompareString(so.F_HS_RecipientCountry, billCountry);


        }

        /// <summary>
        /// 验证销售订单明细的物料编码在K3Cloud是否存在
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="datas"></param>
        /// <returns></returns>
        private bool MaterialNoIsExistInK3(Context ctx, List<AbsSynchroDataInfo> datas)
        {
            List<string> noExistNos = new List<string>();
            string errorInfo = "";
            int len = 0;

            bool flag = true;
            List<K3SalOrderInfo> orders = datas.Select(o => (K3SalOrderInfo)o).ToList();
            if (orders != null && orders.Count > 0)
            {
                foreach (var order in orders)
                {
                    if (order.OrderEntry != null && order.OrderEntry.Count > 0)
                    {
                        for (int i = 0; i < order.OrderEntry.Count; i++)
                        {
                            if (!string.IsNullOrWhiteSpace(order.OrderEntry[i].FMaterialId))
                            {
                                string sql = string.Format(@"/*dialect*/ select FNUMBER From T_BD_MATERIAL where FNUMBER = Convert(nvarchar(14),'{0}') and FUSEORGID={1} and FDOCUMENTSTATUS = 'C'", order.OrderEntry[i].FMaterialId, ORGID);
                                string materialNo = JsonUtils.ConvertObjectToString(SQLUtils.GetObject(ctx, sql, "FNUMBER"));

                                if (string.IsNullOrEmpty(materialNo))
                                {
                                    errorInfo = "销售订单订单号[" + order.FBillNo + "] 的明细物料";
                                    len = errorInfo.Length;

                                    noExistNos.Add(order.OrderEntry[i].FMaterialId);
                                    flag = false;
                                }
                            }
                            else
                            {
                                LogUtils.WriteSynchroLog(ctx, SynchroDataType.SaleOrder, "销售订单订单号[" + orders[i].FBillNo + "] 存在空的物料编码！");
                            }

                        }

                        if (noExistNos != null && noExistNos.Count > 0)
                        {
                            for (int i = 0; i < noExistNos.Count; i++)
                            {
                                if (i < noExistNos.Count - 1)
                                {
                                    errorInfo += "[" + noExistNos[i] + "],";
                                }
                                else if (i == noExistNos.Count - 1)
                                {
                                    errorInfo += "[" + noExistNos[i] + "] 在K3Cloud 不存在！";
                                }
                            }
                        }

                    }

                    if (!string.IsNullOrWhiteSpace(errorInfo) && errorInfo.Length > len)
                    {
                        LogUtils.WriteSynchroLog(ctx, SynchroDataType.SaleOrder, errorInfo);
                        errorInfo = "";

                        if (noExistNos != null || noExistNos.Count > 0)
                        {
                            noExistNos.Clear();
                        }
                    }
                }

            }
            return flag;
        }

        /// <summary>
        /// 验证销售订单的国家编码在K3Cloud是否存在
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="orders"></param>
        /// <returns></returns>
        private bool CountryNoIsExistInK3(Context ctx, List<K3SalOrderInfo> orders)
        {
            bool flag = true;
            string errorInfo = "";

            if (orders != null && orders.Count > 0)
            {
                for (int i = 0; i < orders.Count; i++)
                {
                    if (!string.IsNullOrEmpty(orders[i].F_HS_RecipientCountry))
                    {
                        string sql = string.Format(@"/*dialect*/ select FNUMBER from VW_BAS_ASSISTANTDATA_CountryName where FNUMBER = '{0}'", orders[i].F_HS_RecipientCountry);
                        string countryNo = JsonUtils.ConvertObjectToString(SQLUtils.GetObject(ctx, sql, "FNUMBER"));

                        if (string.IsNullOrWhiteSpace(countryNo))
                        {
                            flag = false;
                            errorInfo = "销售订单订单号:[" + orders[i].FBillNo + "]的国家编码[" + orders[i].F_HS_RecipientCountry + "] 在K3Cloud 不存在！";
                            LogUtils.WriteSynchroLog(ctx, SynchroDataType.SaleOrder, errorInfo);

                        }
                    }
                }
            }
            return flag;
        }

        /// <summary>
        /// 合并后生成新的销售订单获取源单编码
        /// </summary>
        /// <param name="orders"></param>
        /// <returns></returns>
        private List<string> GetCanceledNos(List<K3SalOrderInfo> orders)
        {
            List<string> cancelNos = null;
            List<string> cancelSalNos = new List<string>();

            if (orders != null && orders.Count > 0)
            {
                if (orders.Where(o => !string.IsNullOrWhiteSpace(o.F_HS_MergeSrcBillNo)) != null)
                {
                    cancelNos = orders.Select(o => o.F_HS_MergeSrcBillNo).ToList();

                    if (cancelNos != null && cancelNos.Count > 0)
                    {
                        foreach (var no in cancelNos)
                        {
                            if (!string.IsNullOrWhiteSpace(no))
                            {
                                List<string> cNos = ServiceHelper.GetItems(no);

                                if (cNos != null && cNos.Count > 0)
                                {
                                    cancelSalNos = cancelSalNos.Concat(cNos).ToList();
                                }
                            }
                        }
                    }
                }

            }
            return cancelSalNos;
        }

        /// <summary>
        /// 删除Redis已作废的销售订单数据
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="dataType"></param>
        private void RemoveCanceledOrdersInRedis(Context ctx, SynchroDataType dataType)
        {
            string now = DateTime.Now.ToString("yyyy-MM-dd") + " 00:00:00";
            string sql = string.Empty;

            if (first != null && first.Count > 0)
            {
                sql = string.Format(@"/*dialect*/ select FBillNo from T_SAL_ORDER
			                                      and FBILLNO in('{0}')", string.Join("','", first.Select(o => o.FBillNo).ToList()));

            }
            else
            {
                sql = string.Format(@"/*dialect*/ select a.FBillNo from T_SAL_ORDER a 
                                                    inner join T_BAS_ASSISTANTDATAENTRY_L f ON a.F_HS_SaleOrderSource=f.FENTRYID
                                                    inner join T_BAS_ASSISTANTDATAENTRY g ON f.FentryID=g.FentryID
                                                    where (g.FNUMBER = 'HCWebProcessingOder' or g.FNUMBER = 'HCWebPendingOder')
                                                    
                                                    and a.FCREATEDATE between '{0}' and GETDATE()", "2018-11-01 00:00:00");
            }
            DynamicObjectCollection coll = SQLUtils.GetObjects(ctx, sql);
            List<string> orderNos = coll.Select(o => SQLUtils.GetFieldValue(o, "FBillNo")).ToList();
            RemoveRedisData(ctx, orderNos, dataType);
        }

        /// <summary>
        /// 指定截取小数点后几位
        /// </summary>
        /// <param name="d"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        private static decimal CutDecimalWithN(decimal d, int n)
        {
            string strDecimal = d.ToString();
            int index = strDecimal.IndexOf(".");
            if (index == -1 || strDecimal.Length < index + n + 1)
            {
                strDecimal = string.Format("{0:F" + n + "}", d);
            }
            else
            {
                int length = index;
                if (n != 0)
                {
                    length = index + n + 1;
                }
                strDecimal = strDecimal.Substring(0, length);
            }
            return Decimal.Parse(strDecimal);
        }
    }
}


