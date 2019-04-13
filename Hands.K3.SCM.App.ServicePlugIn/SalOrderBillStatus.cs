using Hands.K3.SCM.APP.Entity.SynDataObject;
using Kingdee.BOS.Orm.DataEntity;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject;

using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Hands.K3.SCM.APP.Utils.Utils;
using HS.K3.Common.Abbott;
using Hands.K3.SCM.APP.Entity.K3WebApi;

namespace Hands.K3.SCM.App.ServicePlugIn
{
    [Description("销售订单--销售订单状态同步")]
    public class SalOrderBillStatus : AbstractOSPlugIn
    {
        public override SynchroDataType DataType
        {
            get
            {
                return SynchroDataType.SaleOrderStatus;
            }
        }

        public override void OnPreparePropertys(Kingdee.BOS.Core.DynamicForm.PlugIn.Args.PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            e.FieldKeys.Add("F_HS_PaymentStatus");
            e.FieldKeys.Add("F_HS_SaleOrderSource");
            e.FieldKeys.Add("F_HS_PaymentMode");
            e.FieldKeys.Add("F_HS_B2CCustId");
            e.FieldKeys.Add("F_HS_PaymentModeNew");

        }
        public override void EndOperationTransaction(Kingdee.BOS.Core.DynamicForm.PlugIn.Args.EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);

            List<AbsSynchroDataInfo> temp = new List<AbsSynchroDataInfo>();
            if (e.DataEntitys == null) return;
            List<DynamicObject> dataEntitys = e.DataEntitys.ToList();
            HttpResponseResult result = new HttpResponseResult();
            if (dataEntitys == null || dataEntitys.Count <= 0)
            {
                return;
            }

            List<AbsSynchroDataInfo> datas = GetK3Datas(this.Context, dataEntitys,ref result).ToList();

            if (datas != null && datas.Count > 0)
            {
                if (temp.All(a => datas.Any(b => a.Equals(b))))
                {
                    temp = temp.Concat(datas).ToList();

                    if (IsConnectSuccess(this.Context))
                    {
                        this.DyamicObjects = e.DataEntitys.ToList();
                        SynchroK3DataToWebSite(this.Context);
                        LogHelper.WriteSynSalOrderStatus(this.Context, temp.Select(o => (K3SalOrderStatusInfo)o).ToList());

                    }
                }

            }
        }

        public override void AfterExecuteOperationTransaction(AfterExecuteOperationTransaction e)
        {
            //base.AfterExecuteOperationTransaction(e);

            //var task = Task.Factory.StartNew(() => {
            //                                            List<AbsSynchroDataInfo> temp = new List<AbsSynchroDataInfo>();
            //                                            if (e.DataEntitys == null) return;
            //                                            List<DynamicObject> dataEntitys = e.DataEntitys.ToList();

            //                                            if (dataEntitys == null || dataEntitys.Count <= 0)
            //                                            {
            //                                                return;
            //                                            }

            //                                            List<AbsSynchroDataInfo> datas = GetK3Datas(this.Context, dataEntitys).ToList();

            //                                            if (datas != null && datas.Count > 0)
            //                                            {
            //                                                if (temp.All(a => datas.Any(b => a.Equals(b))))
            //                                                {
            //                                                    temp = temp.Concat(datas).ToList();

            //                                                    if (IsConnectSuccess(this.Context))
            //                                                    {
            //                                                        this.DyamicObjects = e.DataEntitys.ToList();
            //                                                        SynchroK3DataToWebSite(this.Context);
            //                                                        SynchroDataHelper.WriteSynSalOrderStatus(this.Context, temp.Select(o => (K3SalOrderStatusInfo)o).ToList());

            //                                                    }
            //                                                }

            //                                            }
            //                                        }
            //                                );
        }
        public override IEnumerable<AbsSynchroDataInfo> GetK3Datas(Context ctx, List<DynamicObject> objects,ref HttpResponseResult result)
        {
            HashSet<K3SalOrderStatusInfo> orders = new HashSet<K3SalOrderStatusInfo>();
            K3SalOrderStatusInfo order = null;

            result = new HttpResponseResult();
            result.Success = true;

            if (objects != null && objects.Count > 0)
            {
                foreach (var item in objects)
                {
                    if (item != null)
                    {
                        if ((SQLUtils.GetSaleOrderSourceNo(this.Context, item, "F_HS_SaleOrderSource_Id").CompareTo("XXBJDD") == 0
                        && SQLUtils.GetFieldValue(item, "BillNo").StartsWith("SO"))
                        || SQLUtils.GetSaleOrderSourceNo(this.Context, item, "F_HS_SaleOrderSource_Id").CompareTo("HCWebPendingOder") == 0
                        || SQLUtils.GetSaleOrderSourceNo(this.Context, item, "F_HS_SaleOrderSource_Id").CompareTo("HCWebProcessingOder") == 0)//订单类型
                        {
                            order = new K3SalOrderStatusInfo();

                            order.SrcNo = SQLUtils.GetFieldValue(item, "BillNo");
                            order.BillNo = SQLUtils.GetFieldValue(item, "BillNo");//订单号
                            order.CloseStatus = SQLUtils.GetFieldValue(item, "CloseStatus");//关闭状态
                            order.CancelStatus = SQLUtils.GetFieldValue(item, "CancelStatus");//作废状态
                            order.PaymentStatus = SQLUtils.GetFieldValue(item, "F_HS_PaymentStatus");//付款状态
                            order.F_HS_PaymentMode = SQLUtils.GetFieldValue(item, "F_HS_PaymentMode");//结算方式

                            if (string.IsNullOrWhiteSpace(SQLUtils.GetFieldValue(item, "F_HS_PaymentMode")))
                            {
                                order.F_HS_PaymentMode = SQLUtils.GetPaymentNo(this.Context, item, "F_HS_PaymentModeNew_Id");//结算方式
                            }

                            orders.Add(order);

                        }

                    }

                }
            }
            return orders;
        }
    }
}
