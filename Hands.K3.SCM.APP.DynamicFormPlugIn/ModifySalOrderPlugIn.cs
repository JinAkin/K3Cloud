using System.Collections.Generic;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS;
using Kingdee.BOS.Orm.DataEntity;
using System.ComponentModel;
using Hands.K3.SCM.APP.Utils;
using Hands.K3.SCM.APP.Entity.SynDataObject;
using Hands.K3.SCM.APP.Entity.StructType;
using Hands.K3.SCM.APP.Utils.Utils;

namespace Hands.K3.SCM.APP.DynamicFormPlugIn
{
    [Description("订单改单，合单同步插件(单个改单)")]
    public class ModifySalOrderPlugIn : AbstractModifySalOrder
    {
        public override List<K3SalOrderInfo> GetSelectedSalOrders(Context ctx)
        {
            K3SalOrderInfo order = new K3SalOrderInfo();
            DynamicObject obj = null;

            obj = this.View.Model.GetValue("FBillTypeId") as DynamicObject;
            order.FBillTypeId = SQLUtils.GetFieldValue(obj,"Number");

            order.FBillNo = GetValue("FBillNo");
            order.FDocumentStatus = GetValue("FDocumentStatus");

            order.FCloseStatus = GetValue("FCloseStatus");
            order.FCancelStatus = GetValue("FCancelStatus");
            order.F_HS_PaymentStatus = GetValue("F_HS_PaymentStatus");

            obj = this.View.Model.GetValue("F_HS_B2CCustId") as DynamicObject;
            order.F_HS_B2CCustId = SQLUtils.GetFieldValue(obj, "Number");

            List<K3SalOrderInfo> orders = new List<K3SalOrderInfo>() { order };

            return orders;
        }

        /// <summary>
        /// 根据字段名获取字段值
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        private string GetValue(string fieldName)
        {
            if (!string.IsNullOrWhiteSpace(fieldName))
            {
                return JsonUtils.ConvertObjectToString(this.View.Model.GetValue(fieldName));
            }

            return null;
        }
       
        /// <summary>
        /// 获取源单单据体数据行
        /// </summary>
        /// <param name="formId"></param>
        /// <returns></returns>
        public override DynamicObjectCollection GetAtiveRow()
        {
            DynamicObjectCollection rows = null;
            rows = this.View.Model.DataObject["SaleOrderEntry"] as DynamicObjectCollection;

            return rows;
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
