using Kingdee.BOS.Orm.DataEntity;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using System.ComponentModel;

using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Hands.K3.SCM.APP.Utils.Utils;
using HS.K3.Common.Abbott;

namespace Hands.K3.SCM.App.ServicePlugIn
{
    [Description("销售订单--统计客户下单次数插件")]
    public class StatisticsSaleOrder : AbstractOSPlugIn
    {
        private readonly object updateObj = new object();
        public override SynchroDataType DataType
        {
            get
            {
                return SynchroDataType.SaleOrder;
            }
        }

        public override void OnPreparePropertys(Kingdee.BOS.Core.DynamicForm.PlugIn.Args.PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);

            e.FieldKeys.Add("F_HS_B2CCustId");
        }
        public override void EndOperationTransaction(Kingdee.BOS.Core.DynamicForm.PlugIn.Args.EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);

            if (e.DataEntitys == null) return;
            List<DynamicObject> dataEntitys = e.DataEntitys.ToList();

            if (dataEntitys == null || dataEntitys.Count <= 0)
            {
                return;
            }

            int count = StatisticsOrderCount(this.Context, dataEntitys);
        }

        public override void AfterExecuteOperationTransaction(AfterExecuteOperationTransaction e)
        {
            //base.AfterExecuteOperationTransaction(e);

            //var task = Task.Factory.StartNew(() => {
            //                                            if (e.DataEntitys == null) return;
            //                                            List<DynamicObject> dataEntitys = e.DataEntitys.ToList();

            //                                            if (dataEntitys == null || dataEntitys.Count <= 0)
            //                                            {
            //                                                return;
            //                                            }

            //                                            foreach (var item in dataEntitys)
            //                                            {
            //                                                if (item != null)
            //                                                {
            //                                                    int count = StatisticsOrderCount(this.Context, item);
            //                                                }
            //                                            }
            //                                        }
            //                                );

        }

        /// <summary>
        /// 根据订单状态统计下单次数
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private int GetOrderCount(DynamicObject item)
        {
            int count = 0;
            string documentStatus = SQLUtils.GetFieldValue(item, "DocumentStatus");
            string closeStatus = SQLUtils.GetFieldValue(item, "CloseStatus");

            if (item != null)
            {
                if (documentStatus.CompareTo("C") == 0 && closeStatus.CompareTo("B") != 0)//审核状态
                {
                    count += 1;
                }
                else if (documentStatus.CompareTo("C") == 0 && closeStatus.CompareTo("B") == 0)
                {
                    count = -1;
                }
                else if (SQLUtils.GetFieldValue(item, "DocumentStatus").CompareTo("D") == 0)//反审核状态
                {
                    return -1;
                }

            }

            return count;
        }

        /// <summary>
        /// 统计客户下单次数
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="custNo"></param>
        /// <param name="orderCount"></param>
        /// <returns></returns>
        public int StatisticsOrderCount(Context ctx, List<DynamicObject> objs)
        {
            int cnt = 0;

            if (objs != null && objs.Count > 0)
            {
                foreach (var obj in objs)
                {
                    if (obj != null)
                    {
                        DynamicObject cust = obj["F_HS_B2CCustId"] as DynamicObject;
                        string custNo = SQLUtils.GetFieldValue(cust, "Number");
                        int count = GetOrderCount(obj);

                        string sql = string.Format(@"/*dialect*/ update T_BD_CUSTOMER set F_HS_OrderQty = F_HS_OrderQty + {0} where FNumber = '{1}'", count, custNo);

                        lock (updateObj)
                        {
                            cnt += DBUtils.Execute(ctx, sql);
                        }
                            
                    }
                }
            }

            return cnt;
        }

    }
}
