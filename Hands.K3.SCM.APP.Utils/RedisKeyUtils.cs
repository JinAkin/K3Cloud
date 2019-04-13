

using Hands.K3.SCM.APP.Entity.EnumType;
using Hands.K3.SCM.APP.Utils.Utils;
using HS.K3.Common.Abbott;
using Kingdee.BOS;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Hands.K3.SCM.APP.Utils
{
    public class RedisKeyUtils
    {
        public const string UnreadKey = "unreadKey";
        public const string AllKey = "allKey";
        public const string InfoKey = "infoKey";
        public static Dictionary<string, string> GetRedisSetKey(SynchroDataType dataType, SynchroDirection direct = default(SynchroDirection))
        {    
            Dictionary<string, string> dict = new Dictionary<string, string>();

            switch (dataType)
            {
                case SynchroDataType.Customer:
                    switch (direct)
                    {
                        case SynchroDirection.ToK3:
                            dict.Add(UnreadKey, "customers_unread");
                            dict.Add(InfoKey, "customers_info:");
                            dict.Add(AllKey, "customers_all");
                            return dict;
                        case SynchroDirection.ToHC:
                            dict.Add(UnreadKey, "K3Customers_unread");
                            dict.Add(InfoKey, "K3Customers_info:");
                            dict.Add(AllKey, "K3Customers_all");
                            return dict;
                        default:
                            return null;
                    }
                    
                case SynchroDataType.CustomerAddress:
                    dict.Add(UnreadKey, "b2b_address_unread");
                    dict.Add(InfoKey, "b2b_address_info:");
                    dict.Add(AllKey, "b2b_address_all");
                    return dict;
                case SynchroDataType.DelCustomerAddress:
                    dict.Add(UnreadKey, "b2b_address_delete_unread");
                    dict.Add(InfoKey, "b2b_address_delete_info:");
                    dict.Add(AllKey, "b2b_address_delete_all");
                    return dict;
                case SynchroDataType.SaleOrder:
                case SynchroDataType.DropShippingSalOrder:
                    switch (direct)
                    {
                        case SynchroDirection.ToK3:
                        case SynchroDirection.ToB2B:
                            dict.Add(UnreadKey, "unread_orders");
                            dict.Add(InfoKey, "orders_info:");
                            dict.Add(AllKey, "all_orders");
                            return dict;
                        case SynchroDirection.ToHC:
                            dict.Add(UnreadKey, "Credit_unread");
                            dict.Add(InfoKey, "Credit_info:");
                            dict.Add(AllKey, "Credit_all");
                            return dict;
                        default:
                            return null;
                    }
                    
                case SynchroDataType.SalesOrderPayStatus:
                    dict.Add(UnreadKey, "orders_status_unread");
                    dict.Add(InfoKey, "orders_status_info:");
                    dict.Add(AllKey, "orders_status_all");
                    return dict;
                case SynchroDataType.DownLoadListInfo:
                    dict.Add(UnreadKey, "unread_material_listid");
                    dict.Add(InfoKey, "material_info:");
                    dict.Add(AllKey, "all_material_listid");
                    return dict;
                case SynchroDataType.SynchroListInfo:
                    dict.Add(UnreadKey, "MaterialListInfo_unread");
                    dict.Add(InfoKey, "MaterialListInfo_info:");
                    dict.Add(AllKey, "MaterialListInfo_all");
                    return dict;
                case SynchroDataType.ReceiveBill:
                    dict.Add(UnreadKey, "Credit_unread");
                    dict.Add(InfoKey, "Credit_info:");
                    dict.Add(AllKey, "Credit_all");
                    return dict;
                case SynchroDataType.ReFundBill:
                    dict.Add(UnreadKey, "Credit_unread");
                    dict.Add(InfoKey, "Credit_info:");
                    dict.Add(AllKey, "Credit_all");
                    return dict;
                case SynchroDataType.ImportLogis:
                    dict.Add(UnreadKey, "CarriageNO_unread");
                    dict.Add(InfoKey, "CarriageNO_info:");
                    dict.Add(AllKey, "CarriageNO_all");
                    return dict;
                case SynchroDataType.Material:
                    dict.Add(UnreadKey, "material_unread");
                    dict.Add(InfoKey, "material_info:");
                    dict.Add(AllKey, "material_all");
                    return dict;
                case SynchroDataType.Inventroy:
                    dict.Add(UnreadKey, "inventories_Unread");
                    dict.Add(InfoKey, "inventories_info:");
                    dict.Add(AllKey, "inventories_All");
                    return dict;
                case SynchroDataType.SaleOrderOffline:
                    switch (direct)
                    {
                        case SynchroDirection.Default:
                            dict.Add(UnreadKey, "Credit_unread");
                            dict.Add(InfoKey, "Credit_info:");
                            dict.Add(AllKey, "Credit_all");
                            return dict;
                        case SynchroDirection.ToHC:
                            dict.Add(UnreadKey, "offlineOrder_unread");
                            dict.Add(InfoKey, "offlineOrder_info:");
                            dict.Add(AllKey, "offlineOrder_all");
                            return dict;
                        default:
                            return null;
                    }
                    
                case SynchroDataType.SaleOrderStatus:
                    dict.Add(UnreadKey, "orderStatus_unread");
                    dict.Add(InfoKey, "orderStatus_info:");
                    dict.Add(AllKey, "orderStatus_all");
                    return dict;
                case SynchroDataType.BatchAdjust:
                    dict.Add(UnreadKey, "BatchAdjust_unread");
                    dict.Add(InfoKey, "BatchAdjust_info:");
                    dict.Add(AllKey, "BatchAdjust_all");
                    return dict;
                case SynchroDataType.OnTheWay:
                    switch (direct)
                    {
                        case SynchroDirection.ToHC:
                        case SynchroDirection.ToB2B:
                            dict.Add(UnreadKey, "TransportationInventory_unread");
                            dict.Add(InfoKey, "TransportationInventory_info:");
                            dict.Add(AllKey, "TransportationInventory_all");
                            return dict;
                        default:
                            return null;
                    }
                case SynchroDataType.DeliveryNoticeBill:
                    dict.Add(UnreadKey, "LogisticsTrajectory_unread");
                    dict.Add(InfoKey, "LogisticsTrajectory_info:");
                    dict.Add(AllKey, "LogisticsTrajectory_all");
                    return dict;
                case SynchroDataType.InStock:
                    dict.Add(UnreadKey, "HomemarkReached_unread");
                    dict.Add(InfoKey, "HomemarkReached_info:");
                    dict.Add(AllKey, "HomemarkReached_all");
                    return dict;
            }
            return null;
        }

        public static Dictionary<string,string> GetStockNo(Context ctx,JObject jObj)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();

            dict.Add("us_quantity", "202");
            dict.Add("us_liquid_quantity", "202A");

            dict.Add("us_east_quantity", "202B");
            dict.Add("xm_quantity", "102");

            dict.Add("sz_quantity", "101");
            dict.Add("hk_quantity", "203");

            dict.Add("eur_quantity", "205");
            dict.Add("au_quantity", "207");

            if (Convert.ToInt32(JsonUtils.GetFieldValue(jObj, "virtual_quantity")) > 0)
            {
                string materialNo = JsonUtils.GetFieldValue(jObj, "order_fix_id");
                string stockNo = SQLUtils.GetStockNo(ctx, materialNo);

                if (!string.IsNullOrWhiteSpace(stockNo))
                {
                    dict.Add("virtual_quantity", stockNo);
                }
                else
                {
                    //如果是固体，则销售订单明细仓库默认为深圳硬件仓  ，如果是液体，则销售订单明细仓库默认为深圳液体仓
                    if (materialNo.StartsWith("2"))
                    {
                        dict.Add("virtual_quantity", "102");
                    }
                    else
                    {
                        dict.Add("virtual_quantity", "101");
                    }
                }
            }
            
            return dict;
           
        }
    }
}
