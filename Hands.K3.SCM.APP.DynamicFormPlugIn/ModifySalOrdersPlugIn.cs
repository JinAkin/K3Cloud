
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using System.ComponentModel;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS;
using Hands.K3.SCM.APP.Entity.SynDataObject;
using Hands.K3.SCM.APP.Utils;
using Hands.K3.SCM.APP.Entity.StructType;
using Hands.K3.SCM.APP.Utils.Utils;

namespace Hands.K3.SCM.APP.DynamicFormPlugIn
{
    [Description("订单改单，合单同步插件(列表)")]
    public class ModifySalordersPlugIn : AbstractModifySalOrder
    {
        public List<string> GetSelectedSalOrderNos()
        {
            ListSelectedRowCollection rows = this.ListView.SelectedRowsInfo;
            List<string> billNos = null;
            List<K3SalOrderInfo> orders = null;
            K3SalOrderInfo order = null;

            if (rows != null && rows.Count > 0)
            {
                orders = new List<K3SalOrderInfo>();
                billNos = new List<string>();

                foreach (var row in rows)
                {
                    if (row != null)
                    {
                        order = new K3SalOrderInfo();
                        billNos.Add(row.BillNo);
                    }
                }
            }
            return billNos;  
        }

        public DynamicObjectCollection GetCollection(Context ctx, List<string> billNos)
        {
            if (billNos != null && billNos.Count > 0)
            {
                string sql = string.Format(@"/*dialect*/ select a.FBillNo,a.FDOCUMENTSTATUS,a.FCLOSESTATUS,a.FCANCELSTATUS,
                                                        a.F_HS_PAYMENTSTATUS,a.F_HS_B2CCustId,a.F_HS_RECIPIENTCOUNTRY,c.FSETTLECURRID
                                                        from T_SAL_ORDER a
                                                        inner join T_SAL_ORDERENTRY b on a.FID = b.FID
                                                        inner join T_SAL_ORDERFIN c on c.FID = a.FID
                                                        where FBillNo in ('{0}')
                                           ", string.Join("','", billNos.Select(o => o.ToString()))
                                          );

                DynamicObjectCollection coll = SQLUtils.GetObjects(ctx, sql);
                return coll;
            }
            return null;
        }

        public override List<K3SalOrderInfo> GetSelectedSalOrders(Context ctx)
        {
            List<K3SalOrderInfo> orders = null;
            K3SalOrderInfo order = null;
           
            List<string> billNos = GetSelectedSalOrderNos();
            DynamicObjectCollection coll = GetCollection(ctx, billNos);

            var groups = from o in coll
                         group o by o["FBillNo"] into g
                         select g;

            if (groups != null && groups.Count() > 0)
            {
                orders = new List<K3SalOrderInfo>();

                foreach (var group in groups)
                {
                    if (group != null && group.Count() > 0)
                    {

                        order = new K3SalOrderInfo();
                        order.FBillNo = SQLUtils.GetFieldValue(group.ElementAt(0), "FBillNo");
                        order.FDocumentStatus = SQLUtils.GetFieldValue(group.ElementAt(0), "FDOCUMENTSTATUS");
                        order.FCloseStatus = SQLUtils.GetFieldValue(group.ElementAt(0), "FCLOSESTATUS");
                        order.FCancelStatus = SQLUtils.GetFieldValue(group.ElementAt(0), "FCANCELSTATUS");
                        order.F_HS_PaymentStatus = SQLUtils.GetFieldValue(group.ElementAt(0), "F_HS_PAYMENTSTATUS");
                        order.F_HS_B2CCustId = SQLUtils.GetCustomerNo(ctx,group.ElementAt(0), "F_HS_B2CCustId");
                        order.F_HS_RecipientCountry = SQLUtils.GetCountryNo(ctx,group.ElementAt(0), "F_HS_RECIPIENTCOUNTRY");
                        order.FSettleCurrId = SQLUtils.GetSettleCurrNo(ctx,group.ElementAt(0), "FSETTLECURRID");
                    }

                    orders.Add(order);
                }
            }
            return orders;
        }
        public override DynamicObjectCollection GetAtiveRow()
        {
            return null;
        }
        public override void BarItemClick(BarItemClickEventArgs e)
        {

            base.BarItemClick(e);

            switch (e.BarItemKey)
            {
                case "tbModifySalOrder":
                    ExecutOperate(this.Context, RequestType.MODIFY);
                    break;
                case "tbCombineSalOrder":
                    ExecutOperate(this.Context, RequestType.COMBINE);
                    break;
            }
        }
    }
}
