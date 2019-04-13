
using Hands.K3.SCM.APP.Entity.SynDataObject;
using Hands.K3.SCM.APP.Entity.SynDataObject.SaleOrder;
using Kingdee.BOS.Orm.DataEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject;
using Hands.K3.SCM.APP.Utils.Utils;
using Hands.K3.SCM.App.Synchro.Base.Abstract;
using HS.K3.Common.Abbott;

namespace Hands.K3.SCM.App.Core.SynchroService.ToHC
{
    public class SynOfflineSalOrderToHC : AbstractSynchroToHC
    {

        override public SynchroDataType DataType
        {
            get
            {
                return SynchroDataType.SaleOrderOffline;
            }
        }

        /// <summary>
        /// 获取全部线下订单
        /// </summary>
        /// <param name="flag"></param>
        /// <param name="saleOrderNo"></param>
        /// <returns></returns>
        public List<K3SalOrderInfo> GetAllSalOrders( bool flag = true, string saleOrderNo = null)
        {
            List<K3SalOrderInfo> orders = null;
            K3SalOrderInfo order = null;

            List<K3SalOrderEntryInfo> entries = null;
            string sql = string.Empty;

            if (flag)
            {
                sql = string.Format(@"/*dialect*/select FBillNo,F_HS_OriginOnlineOrderNo,FDate,a.FNOTE,r.FNUMBER  as FCUSTID,o.FNUMBER as F_HS_B2CCustId,FSalerId,g.FNUMBER as F_HS_SaleOrderSource,w.FNUMBER as FSettleCurrId,F_HS_RateToUSA,x.FNUMBER as F_HS_RecipientCountry,F_HS_DeliveryProvinces
                                        ,F_HS_DeliveryCity,F_HS_DeliveryAddress,F_HS_PostCode,F_HS_DeliveryName,F_HS_BillAddress,F_HS_MobilePhone,F_HS_ShippingMethod,n.FNUMBER as F_HS_PaymentModeNew          
                                        ,F_HS_PaymentStatus,F_HS_ShippingMethodRemark,F_HS_OnlineOrderWay,F_HS_CouponAmount,F_HS_IntegralDeduction,v.FNUMBER as FMaterialId,c.FTAXPRICE,FAmount,FQty,k.FNUMBER as F_HS_StockID
                                        ,F_HS_IsVirtualEntry,F_HS_IsEmptyStock,c.FIsFree,a.FApproveDate,F_HS_CollectionTime 
                                        from T_SAL_ORDER a 
                                        inner join T_SAL_ORDERENTRY b on b.FID = a.FID
                                        inner join T_SAL_ORDERENTRY_F c on c.FENTRYID = b.FENTRYID and c.FID = b.FID
                                        inner join T_SAL_ORDERFIN d on d.FID = a.FID
                                        inner join T_BD_CUSTOMER e on e.FCUSTID= a.FCUSTID
                                        inner join T_BAS_ASSISTANTDATAENTRY_L f ON a.F_HS_SaleOrderSource=f.FENTRYID and f.FLOCALEID = 2052
                                        inner join T_BAS_ASSISTANTDATAENTRY g ON f.FENTRYID=g.FENTRYID
                                        inner join T_BAS_ASSISTANTDATAENTRY_L m ON a.F_HS_PaymentModeNew=m.FENTRYID and m.FLOCALEID = 2052
                                        inner join T_BAS_ASSISTANTDATAENTRY n ON m.FENTRYID=n.FENTRYID
                                        left join T_BAS_BILLTYPE h on a.FBILLTypeID=h.FBILLTypeID
                                        inner join T_ORG_ORGANIZATIONS z on a.FSALEORGID=z.FORGID
                                        inner join VW_BAS_ASSISTANTDATA_CountryName x on x.FCountry = a.F_HS_RecipientCountry
                                        inner join T_BD_MATERIAL v on v.FMATERIALID = b.FMATERIALID
                                        inner join T_BD_CURRENCY w on w.FCURRENCYID = d.FSETTLECURRID
                                        inner join T_BD_CUSTOMER r on r.FCUSTID = a.FCUSTID
										inner join T_BD_CUSTOMER o on o.FCUSTID = a.F_HS_B2CCustId
                                        left join T_BD_STOCK i on i.FSTOCKID=b.F_HS_StockID
										left join T_BAS_ASSISTANTDATAENTRY_L j ON i.F_HS_DLC=j.FENTRYID and j.FLOCALEID = 2052
                                        left join T_BAS_ASSISTANTDATAENTRY k ON j.FentryID=k.FentryID
                                        where (a.FDate >= DATEADD(day,-180,GETDATE()) or a.F_HS_CollectionTime >= DATEADD(day,-180,GETDATE()))and a.FDOCUMENTSTATUS = 'C' and g.fnumber = 'XXBJDD' and FBillNo like 'SO%' and a.FsaleOrgID = 100035 and a.FCANCELSTATUS<>'B' and h.FNUMBER='XSDD01_SYS' and z.FNUMBER='100.01'
                                        and not exists(select * From  T_SAL_ORDERentry where fid=a.fid and  a.FCLOSESTATUS='B' and FMRPCLOSESTATUS='A')
                                        and  a.fchangeDate>= DATEADD(day,-180,GETDATE())
                                        ");
            }
            else
            {
                sql = string.Format(@"/*dialect*/select FBillNo,F_HS_OriginOnlineOrderNo,FDate,a.FNOTE,r.FNUMBER  as FCUSTID,o.FNUMBER as F_HS_B2CCustId,FSalerId,g.FNUMBER as F_HS_SaleOrderSource,w.FNUMBER as FSettleCurrId,F_HS_RateToUSA,x.FNUMBER as F_HS_RecipientCountry,F_HS_DeliveryProvinces
                                        ,F_HS_DeliveryCity,F_HS_DeliveryAddress,F_HS_PostCode,F_HS_DeliveryName,F_HS_BillAddress,F_HS_MobilePhone,F_HS_ShippingMethod,n.FNUMBER as F_HS_PaymentModeNew          
                                        ,F_HS_PaymentStatus,F_HS_ShippingMethodRemark,F_HS_OnlineOrderWay,F_HS_CouponAmount,F_HS_IntegralDeduction,v.FNUMBER as FMaterialId,c.FTAXPRICE,FAmount,FQty,k.FNUMBER as F_HS_StockID
                                        ,F_HS_IsVirtualEntry,F_HS_IsEmptyStock,c.FIsFree,a.FApproveDate,F_HS_CollectionTime 
                                        from T_SAL_ORDER a 
                                        inner join T_SAL_ORDERENTRY b on b.FID = a.FID
                                        inner join T_SAL_ORDERENTRY_F c on c.FENTRYID = b.FENTRYID and c.FID = b.FID
                                        inner join T_SAL_ORDERFIN d on d.FID = a.FID
                                        inner join T_BD_CUSTOMER e on e.FCUSTID= a.FCUSTID
                                        inner join T_BAS_ASSISTANTDATAENTRY_L f ON a.F_HS_SaleOrderSource=f.FENTRYID and f.FLOCALEID = 2052
                                        inner join T_BAS_ASSISTANTDATAENTRY g ON f.FENTRYID=g.FENTRYID
                                        inner join T_BAS_ASSISTANTDATAENTRY_L m ON a.F_HS_PaymentModeNew=m.FENTRYID and m.FLOCALEID = 2052
                                        inner join T_BAS_ASSISTANTDATAENTRY n ON m.FENTRYID=n.FENTRYID
                                        left join T_BAS_BILLTYPE h on a.FBILLTypeID=h.FBILLTypeID
                                        inner join T_ORG_ORGANIZATIONS z on a.FSALEORGID=z.FORGID
                                        inner join VW_BAS_ASSISTANTDATA_CountryName x on x.FCountry = a.F_HS_RecipientCountry
                                        inner join T_BD_MATERIAL v on v.FMATERIALID = b.FMATERIALID
                                        inner join T_BD_CURRENCY w on w.FCURRENCYID = d.FSETTLECURRID
                                        inner join T_BD_CUSTOMER r on r.FCUSTID = a.FCUSTID
										inner join T_BD_CUSTOMER o on o.FCUSTID = a.F_HS_B2CCustId
                                        left join T_BD_STOCK i on i.FSTOCKID=b.F_HS_StockID
										left join T_BAS_ASSISTANTDATAENTRY_L j ON i.F_HS_DLC=j.FENTRYID and j.FLOCALEID = 2052
                                        left join T_BAS_ASSISTANTDATAENTRY k ON j.FentryID=k.FentryID
                                        where (a.FDate >= DATEADD(day,-45,GETDATE()) or a.F_HS_CollectionTime >= DATEADD(day,-45,GETDATE()))and a.FDOCUMENTSTATUS = 'C' and g.fnumber = 'XXBJDD' and FBillNo like 'SO%' and a.FsaleOrgID = 100035 and a.FCANCELSTATUS<>'B' and h.FNUMBER='XSDD01_SYS' and z.FNUMBER='100.01'
                                        and not exists(select * From  T_SAL_ORDERentry where fid=a.fid and  a.FCLOSESTATUS='B' and FMRPCLOSESTATUS='A')
                                        ");
            }

            DynamicObjectCollection coll = SQLUtils.GetObjects(this.K3CloudContext, sql);
            orders = GetK3SalOrderInfo(coll);

            if (orders != null && orders.Count > 0)
            {
                var group = from o in orders group o by o.FBillNo into g select g;

                if (group != null && group.Count() > 0)
                {
                    orders = new List<K3SalOrderInfo>();
                    entries = new List<K3SalOrderEntryInfo>();

                    foreach (var g in group)
                    {
                        if (g != null && g.Count() > 0)
                        {
                            order = g.ElementAt(0) as K3SalOrderInfo; ;

                            entries = GetK3SalOrderEntryInfo(g.ToList<K3SalOrderInfo>());
                            order.OrderEntry = null;
                            order.OrderEntry = entries;

                            order.F_HS_Total = order.F_HS_Subtotal + order.F_HS_Shipping;//优惠后金额
                            orders.Add(order);
                        }
                    }
                }
            }

            return orders;
        }

        /// <summary>
        /// 获取销售订单
        /// </summary>
        /// <param name="coll"></param>
        /// <returns></returns>
        private List<K3SalOrderInfo> GetK3SalOrderInfo( DynamicObjectCollection coll)
        {
            K3SalOrderInfo order = null;
            List<K3SalOrderInfo> orders = null;

            if (coll != null && coll.Count > 0)
            {
                orders = new List<K3SalOrderInfo>();

                foreach (var obj in coll)
                {
                    if (obj != null)
                    {
                        if (SQLUtils.GetFieldValue(obj, "FBillNo").StartsWith("SO"))
                        {
                            order = new K3SalOrderInfo();

                            order.SrcNo = SQLUtils.GetFieldValue(obj, "FBillNo");
                            order.FBillNo = SQLUtils.GetFieldValue(obj, "FBillNo");//订单号
                            order.F_HS_OriginOnlineOrderNo = SQLUtils.GetFieldValue(obj, "F_HS_OriginOnlineOrderNo");//原线下订单单号
                            order.FDate = Convert.ToDateTime(SQLUtils.GetFieldValue(obj, "FDate"));//订单日期

                            order.PurseDate = TimeHelper.GetTimeStamp(order.FDate);
                            order.FCustId = SQLUtils.GetFieldValue(obj, "FCUSTID");//客户编码
                            order.F_HS_B2CCustId = SQLUtils.GetFieldValue(obj, "F_HS_B2CCustId");//客户真是编码
                            order.FSalerId = SQLUtils.GetSellerNo(this.K3CloudContext, obj, "FSalerId");//销售员编码

                            order.OrderSource = SQLUtils.GetFieldValue(obj, "F_HS_SaleOrderSource");//订单来源

                            order.FSettleCurrId = SQLUtils.GetFieldValue(obj, "FSettleCurrId");//结算币别
                            order.F_HS_RateToUSA = Convert.ToDecimal(SQLUtils.GetFieldValue(obj, "F_HS_RateToUSA"));//汇率

                            order.F_HS_RecipientCountry = SQLUtils.GetFieldValue(obj, "F_HS_RecipientCountry");//国家
                            order.F_HS_DeliveryProvinces = SQLUtils.GetFieldValue(obj, "F_HS_DeliveryProvinces");//省份/州

                            order.F_HS_DeliveryCity = SQLUtils.GetFieldValue(obj, "F_HS_DeliveryCity");//城市
                            order.F_HS_DeliveryAddress = SQLUtils.GetFieldValue(obj, "F_HS_DeliveryAddress");//具体地址
                            
                            order.F_HS_PostCode = SQLUtils.GetFieldValue(obj, "F_HS_PostCode");//邮编
                            order.F_HS_DeliveryName = SQLUtils.GetFieldValue(obj, "F_HS_DeliveryName");//收货人姓名

                            order.F_HS_BillAddress = SQLUtils.GetFieldValue(obj, "F_HS_BillAddress");//账单地址
                            order.F_HS_MobilePhone = SQLUtils.GetFieldValue(obj, "F_HS_MobilePhone");//收货人联系电话

                            order.F_HS_ShippingMethod = SQLUtils.GetFieldValue(obj, "F_HS_ShippingMethod");//发货方式
                            order.F_HS_PaymentModeNew = SQLUtils.GetFieldValue(obj, "F_HS_PaymentModeNew");//付款方式

                            order.F_HS_PaymentStatus = SQLUtils.GetFieldValue(obj, "F_HS_PaymentStatus");//付款状态
                            order.F_HS_Shipping = Convert.ToDecimal(SQLUtils.GetFieldValue(obj, "F_HS_Shipping"));//运费

                            order.FCustLevel = SQLUtils.GetFieldValue(obj, "F_HS_FGroup");//分组
                            order.FNote = SQLUtils.GetFieldValue(obj, "FNOTE");//备注

                            order.F_HS_Channel = SQLUtils.GetFieldValue(obj, "F_HS_OnlineOrderWay");//订单渠道
                            order.F_HS_CouponAmount = Convert.ToDecimal(SQLUtils.GetFieldValue(obj, "F_HS_CouponAmount"));//优惠金额

                            order.F_HS_IntegralDeduction = Convert.ToDecimal(SQLUtils.GetFieldValue(obj, "F_HS_IntegralDeduction"));//积分金额
                            order.F_HS_DiscountedAmount = Convert.ToDecimal(SQLUtils.GetFieldValue(obj, "F_HS_DiscountedAmount"));//已优惠金额
                            order.FApproveDate = TimeHelper.GetTimeStamp(Convert.ToDateTime(SQLUtils.GetFieldValue(obj, "FApproveDate")));//订单审核时间

                            order.F_HS_CollectionTime = Convert.ToDateTime(SQLUtils.GetFieldValue(obj, "F_HS_CollectionTime"));//CEO特批已到款时间
                            order.PayedTime = TimeHelper.GetTimeStamp(order.F_HS_CollectionTime);//CEO特批已到款时间(时间戳)

                            K3SalOrderEntryInfo entry = new K3SalOrderEntryInfo();
                            
                            entry.FMaterialId = SQLUtils.GetFieldValue(obj, "FMaterialId");//物料编码
                            entry.FTAXPRICE = Convert.ToDecimal(SQLUtils.GetFieldValue(obj, "FTAXPRICE"));//单价
                            entry.FTAXAmt = Convert.ToDecimal(SQLUtils.GetFieldValue(obj, "FAmount"));//商品总价
                            entry.FQTY = Convert.ToDecimal(SQLUtils.GetFieldValue(obj, "FQTY"));//数量
                            entry.FStockId = SQLUtils.GetFieldValue(obj, "F_HS_StockID");//仓库地理编码
                            entry.F_HS_IsVirtualEntry = SQLUtils.GetFieldValue(obj, "F_HS_IsVirtualEntry").CompareTo("1") == 1 ? true : false;//是否虚拟
                            entry.F_HS_IsEmptyStock = SQLUtils.GetFieldValue(obj, "F_HS_IsEmptyStock").CompareTo("1") == 1 ? true : false;//是否清仓
                            entry.FIsFree = SQLUtils.GetFieldValue(obj, "FIsFree").CompareTo("1") == 1 ? true : false;//是否为赠品

                            order.OrderEntry.Add(entry);
                            orders.Add(order);
                        }
                    }
                }
            }
            return orders;
        }

        /// <summary>
        /// 获取销售订单明细
        /// </summary>
        /// <param name="orders"></param>
        /// <returns></returns>
        private static List<K3SalOrderEntryInfo> GetK3SalOrderEntryInfo( List<K3SalOrderInfo> orders)
        {
            List<K3SalOrderEntryInfo> entries = null;
            K3SalOrderEntryInfo entry = null;

            if (orders != null && orders.Count > 0)
            {
                entries = new List<K3SalOrderEntryInfo>();

                for (int i = 0; i < orders.Count; i++)
                {
                    if (orders[i] != null)
                    {
                        if (orders[i].OrderEntry != null && orders[i].OrderEntry.Count > 0)
                        {
                            foreach (var en in orders[i].OrderEntry)
                            {
                                if (en != null)
                                {
                                    if (en.FMaterialId.CompareTo("99.01") != 0)
                                    {
                                        entry = new K3SalOrderEntryInfo();

                                        entry.FMaterialId = en.FMaterialId;//物料编码
                                        entry.FTAXPRICE = en.FTAXPRICE;//单价
                                        entry.FQTY = en.FQTY;//数量
                                        entry.FTAXAmt = en.FTAXAmt;
                                        entry.FStockId = en.FStockId;//仓库编码
                                        entry.F_HS_IsVirtualEntry = en.F_HS_IsVirtualEntry;//是否虚拟
                                        entry.F_HS_IsEmptyStock = en.F_HS_IsEmptyStock;//是否清仓
                                        entry.FIsFree = en.FIsFree;//是否为赠品

                                        orders[0].F_HS_Subtotal += en.FTAXAmt;
                                        entries.Add(entry);
                                    }

                                    else
                                    {
                                        orders[0].F_HS_Shipping = en.FTAXAmt;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return entries;
        }

        public override IEnumerable<AbsSynchroDataInfo> GetK3Datas(IEnumerable<string> billNos = null,bool flag = true)
        {
            List<K3SalOrderInfo> orders = GetAllSalOrders(flag);
            if (orders != null && orders.Count > 0)
                return orders.ToList<AbsSynchroDataInfo>();
            return null;
        }
    }
}
