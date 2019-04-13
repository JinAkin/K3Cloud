using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Hands.K3.SCM.APP.Entity.SynDataObject;
using Hands.K3.SCM.APP.Entity.K3WebApi;
using Hands.K3.SCM.APP.Entity.StructType;

using System.ComponentModel;
using Hands.K3.SCM.APP.Utils.Utils;
using HS.K3.Common.Abbott;

namespace Hands.K3.SCM.App.ServicePlugIn
{
    [Description("销售锁单--销售订单锁单插件")]
    public class ModifySalOrder:AbstractOSPlugIn
    {
        public static Type type = null;

        public override SynchroDataType DataType
        {
            get
            {
                return SynchroDataType.SaleOrder;
            }
        }

        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            e.FieldKeys.Add("F_HS_B2CCustId");     
        }
        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);

            if (e.DataEntitys == null || e.DataEntitys.Count() < 0)
            {
                return;
            }

            //AbstractModifySalOrder modify = new ModifySalOrderPlugIn();
            List<DynamicObject> dataEntitys = e.DataEntitys.ToList();

            List<K3SalOrderInfo> orders = GetOrders(dataEntitys);
            List<HttpResponseResult> results = null/*modify.SendRequest(this.Context, orders, RequestType.LOCK)*/;

            if (results != null)
            {
                if (results.GroupBy(r => r.Success == true).ToList().Count != 1)
                {
                    throw new Exception("不符合合单条件");
                }
            }
            else
            {
                throw new Exception("Redis服务器没有反应！");
            }
        }

        public override void AfterExecuteOperationTransaction(AfterExecuteOperationTransaction e)
        {
            //base.AfterExecuteOperationTransaction(e);

            //var task = Task.Factory.StartNew(() => {
            //                                            if (e.DataEntitys == null || e.DataEntitys.Count() < 0)
            //                                            {
            //                                                return;
            //                                            }

            //                                            AbstractModifySalOrder modify = new ModifySalOrderPlugIn();
            //                                            List<DynamicObject> dataEntitys = e.DataEntitys.ToList();

            //                                            List<K3SalOrderInfo> orders = GetOrders(dataEntitys);
            //                                            List<HttpResponseResult> results = modify.SendRequest(this.Context, orders, RequestType.LOCK);

            //                                            if (results != null)
            //                                            {
            //                                                if (results.GroupBy(r => r.Success == true).ToList().Count != 1)
            //                                                {
            //                                                    throw new Exception("不符合合单条件");
            //                                                }
            //                                            }
            //                                            else
            //                                            {
            //                                                throw new Exception("Redis服务器没有反应！");
            //                                            }
            //                                        }
            //                                );
        }

        public List<K3SalOrderInfo> GetOrders(List<DynamicObject> dynObjects)
        {
            List<K3SalOrderInfo> orders = null;
            K3SalOrderInfo order = null;

            if (dynObjects != null && dynObjects.Count > 0)
            {
                orders = new List<K3SalOrderInfo>();

                foreach (var obj in dynObjects)
                {
                    if (obj != null)
                    {
                        order = new K3SalOrderInfo();

                        order.F_HS_B2CCustId = SQLUtils.GetCustomerNo(this.Context,obj, "F_HS_B2CCustId_Id");
                        order.FBillNo = SQLUtils.GetFieldValue(obj, "BillNo");

                        orders.Add(order);
                    }
                }
            }
            return orders;
        }
    }
}
