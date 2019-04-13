using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Hands.K3.SCM.App.Synchro.Utils.SynchroService;

using Hands.K3.SCM.APP.Utils.Utils;
using HS.K3.Common.Abbott;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;

namespace Hands.K3.SCM.App.ServicePlugIn
{
    [Description("库存同步服务插件")]
    public class InventorySerPlugIn : AbstractOSPlugIn
    {
        public override SynchroDataType DataType
        {
            get
            {
                return SynchroDataType.Inventroy;
            }
        }

        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            e.FieldKeys.Add("F_HS_SaleOrderSource");
        }
        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            List<DynamicObject> objs = e.DataEntitys.ToList();

            if (objs != null && objs.Count > 0)
            {
                foreach (var item in objs)
                {
                    if (item != null)
                    {
                        DynamicObject oSource = item["F_HS_SaleOrderSource"] as DynamicObject;
                        string orderSource = SQLUtils.GetFieldValue(oSource,"FNumber");

                        string documentStatus = SQLUtils.GetFieldValue(item, "DocumentStatus");

                        if (!string.IsNullOrWhiteSpace(orderSource) && !string.IsNullOrWhiteSpace(documentStatus))
                        {
                            if ((orderSource.CompareTo("HCWebPendingOder") != 0 && orderSource.CompareTo("HCWebProcessingOder") != 0) && (documentStatus.CompareTo("C") != 0 || documentStatus.CompareTo("B") != 0))
                            {
                                SynchroDataHelper.SynchroDataToHC(this.Context, this.DataType);
                                break;
                            }
                        }
                        
                    }
                }
            }
            
        }
    }
}
