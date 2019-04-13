
using HS.K3.Common.Abbott;
using Kingdee.BOS;
using System.Diagnostics;

namespace Hands.K3.SCM.APP.Utils.Utils
{
    public class BackupJson
    {
        public static void WriteJsonToLocal(Context ctx,SynchroDataType dataType, string json)
        {
            Trace.Listeners.Clear();

            if (!string.IsNullOrWhiteSpace(GetBillNo(ctx, dataType, json)))
            {
                Trace.Listeners.Add(LogerTraceListener.CreateInstance(dataType, GetBillNo(ctx, dataType, json)));
            }
            Trace.WriteLine(json);
        }

        public static string GetBillNo(Context ctx,SynchroDataType dataType, string json)
        {
            switch (dataType)
            {
                case SynchroDataType.SaleOrder:
                    return JsonUtils.GetFieldValue(JsonUtils.ParseJson2JObj(ctx,SynchroDataType.SaleOrder,json),"orders_id");
                case SynchroDataType.SaleOrderStatus:
                    return JsonUtils.GetFieldValue(JsonUtils.ParseJson2JObj(ctx, SynchroDataType.SaleOrderStatus, json),"orders_id");
                case SynchroDataType.Customer:
                    return JsonUtils.GetFieldValue(JsonUtils.ParseJson2JObj(ctx, SynchroDataType.Customer,json),"customers_id");
                case SynchroDataType.CustomerAddress:
                    return JsonUtils.GetFieldValue(JsonUtils.ParseJson2JObj(ctx, SynchroDataType.CustomerAddress, json),"address_book_id");
                case SynchroDataType.DelCustomerAddress:
                    return JsonUtils.GetFieldValue(JsonUtils.ParseJson2JObj(ctx, SynchroDataType.DelCustomerAddress, json),"address_book_id");
            }

            return null;
        }
    }
}
