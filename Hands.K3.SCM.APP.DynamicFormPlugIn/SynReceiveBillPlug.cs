
using Hands.K3.SCM.APP.Entity.SynDataObject;
using Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject;
using Hands.K3.SCM.APP.Utils;
using HS.K3.Common.Abbott;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.FormElement;
using Kingdee.BOS.Orm.DataEntity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.APP.DynamicFormPlugIn
{
    [Description("收款退款单界面生成收款单插件")]
    public class SynReceiveBillPlug: AbstractDynPlugIn
    {
        public override SynchroDataType DataType
        {
            get
            {
                return SynchroDataType.ReceiveBill;
            }
        }
        private string GetSql(Context ctx)
        {
            string sql = string.Empty;

            if (SelectedNos != null && SelectedNos.Count() > 0)
            {
                sql = string.Format(@"/*dialect*/ ");
            }
            return sql;
        }

        private IEnumerable<DynamicObject> GetDynamicObjects(Context ctx)
        {
            return null;
        }
        public override IEnumerable<AbsSynchroDataInfo> GetK3Datas(Context ctx, FormOperation oper = null)
        {
            return base.GetK3Datas(ctx, oper);
        }
        public override void BeforeDoAction(BeforeDoActionEventArgs e)
        {
            base.BeforeDoAction(e);
        }
        public override void AfterDoOperation(AfterDoOperationEventArgs e)
        {

        }
    }
}
