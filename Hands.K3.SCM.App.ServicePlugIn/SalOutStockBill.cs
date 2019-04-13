using Hands.K3.SCM.APP.Entity.SynDataObject.SalOutStock;
using Kingdee.BOS;
using Kingdee.BOS.Orm.DataEntity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject;

using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using System.Threading.Tasks;
using Hands.K3.SCM.APP.Utils.Utils;
using HS.K3.Common.Abbott;
using Hands.K3.SCM.APP.Entity.K3WebApi;

namespace Hands.K3.SCM.App.ServicePlugIn
{
    [Description("销售出库单--销售出库单同步插件")]
    public class SalOutStockBill : AbstractOSPlugIn
    {
        public override SynchroDataType DataType
        {
            get
            {
                return SynchroDataType.SaleOutStockBill;
            }
        }

        public override void OnPreparePropertys(Kingdee.BOS.Core.DynamicForm.PlugIn.Args.PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
        }
        public override void EndOperationTransaction(Kingdee.BOS.Core.DynamicForm.PlugIn.Args.EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);

            //string docmentStatus = null;
            //List<string> numbers = null;

            //if (e.DataEntitys == null)
            //{
            //    return;
            //}

            //List<DynamicObject> dataEntitys = e.DataEntitys.ToList();

            //if (dataEntitys == null || dataEntitys.Count <= 0)
            //{
            //    return;
            //}

            //if (dataEntitys != null && dataEntitys.Count > 0)
            //{
            //    numbers = new List<string>();

            //    foreach (var item in dataEntitys)
            //    {
            //        docmentStatus = SQLUtils.GetFieldValue(item, "DocumentStatus");
            //        numbers.Add(SQLUtils.GetFieldValue(item, "BillNo"));
            //    }
            //}

            //this.DyamicObjects = e.DataEntitys.ToList();
            //SynchroK3DataToWebSite(this.Context);
        }

        public override void AfterExecuteOperationTransaction(AfterExecuteOperationTransaction e)
        {
            base.AfterExecuteOperationTransaction(e);

            var task = Task.Factory.StartNew(() =>
            {
                string docmentStatus = null;
                List<string> numbers = null;

                if (e.DataEntitys == null)
                {
                    return;
                }

                List<DynamicObject> dataEntitys = e.DataEntitys.ToList();

                if (dataEntitys == null || dataEntitys.Count <= 0)
                {
                    return;
                }

                if (dataEntitys != null && dataEntitys.Count > 0)
                {
                    numbers = new List<string>();

                    foreach (var item in dataEntitys)
                    {
                        docmentStatus = SQLUtils.GetFieldValue(item, "DocumentStatus");
                        numbers.Add(SQLUtils.GetFieldValue(item, "BillNo"));
                    }
                }

                this.DyamicObjects = e.DataEntitys.ToList();
                SynchroK3DataToWebSite(this.Context);
            }
                                            );
        }

        //财务信息
        private void SetSalOutStockFin(DynamicObject item, SalOutStockBillInfo oStock)
        {
            if (item.DynamicObjectType.Properties.Contains("SAL_OUTSTOCKFIN"))
            {
                DynamicObjectCollection oFin = item["SAL_OUTSTOCKFIN"] as DynamicObjectCollection;
                if (oFin != null && oFin.Count > 0)
                {
                    foreach (var fin in oFin)
                    {
                        if (fin.DynamicObjectType.Properties.Contains("SettleCurrID_Id"))
                        {
                            oStock.SettleCurrNo = SQLUtils.GetSettleCurrNo(this.Context, fin, "SettleCurrID_Id");
                        }
                        if (fin.DynamicObjectType.Properties.Contains("SettleTypeID_Id"))
                        {
                            oStock.SettleType = SQLUtils.GetSettleTypeNo(this.Context, fin, "SettleTypeID_Id");
                        }
                    }
                }
            }
        }
        public HashSet<SalOutStockBillInfo> GetSalOutStockBills(Context ctx, List<DynamicObject> objects)
        {
            DynamicObjectCollection coll = SQLUtils.GetObjects(ctx, GetSql(ctx, objects));

            return BuildSalOutStockBillInfo(coll);
        }

        /// <summary>
        /// 查询结果集封装对象
        /// </summary>
        /// <param name="coll"></param>
        /// <returns></returns>
        private HashSet<SalOutStockBillInfo> BuildSalOutStockBillInfo(DynamicObjectCollection coll)
        {
            HashSet<SalOutStockBillInfo> bills = new HashSet<SalOutStockBillInfo>();
            SalOutStockBillInfo bill = null;

            List<SalOutStockEntry> entries = null;
            SalOutStockEntry entry = null;

            if (coll != null && coll.Count > 0)
            {
                var groups = from l in coll group l by l.DynamicObjectType.Properties.Contains("FBillNo") into g select g;

                foreach (var group in groups)
                {
                    if (group != null && group.Count() > 0)
                    {
                        bill = new SalOutStockBillInfo();
                        entries = new List<SalOutStockEntry>();

                        bill.SrcNo = SQLUtils.GetFieldValue(group.ElementAt(0), "FBillNo");
                        bill.OrderBillNo = SQLUtils.GetFieldValue(group.ElementAt(0), "FBillNo");
                        bill.CustomerNo = SQLUtils.GetFieldValue(group.ElementAt(0), "customerID");
                        bill.CarriageNO = SQLUtils.GetFieldValue(group.ElementAt(0), "FCARRYBILLNO");
                        bill.DeliDate = SQLUtils.GetFieldValue(group.ElementAt(0), "F_HS_DeliDate");
                        bill.LogisticsChannel = SQLUtils.GetLogisticsChannelNo(this.Context, SQLUtils.GetFieldValue(group.ElementAt(0), "F_HS_Channel"));
                        bill.QueryURL = SQLUtils.GetFieldValue(group.ElementAt(0), "F_HS_QueryURL");

                        foreach (var item in group)
                        {
                            if (item != null)
                            {
                                entry = new SalOutStockEntry();
                                entry.MaterialNo = SQLUtils.GetFieldValue(item, "materialNO");
                                entry.Quantity = Convert.ToDecimal(SQLUtils.GetFieldValue(item, "QTY"));
                                entries.Add(entry);
                            }

                        }

                        bill.StockEntry = entries;
                        bills.Add(bill);
                    }
                }
            }

            return bills;
        }

        /// <summary>
        /// 审核需要执行的SQL语句
        /// </summary>
        /// <param name="numbers"></param>
        /// <returns></returns>
        private string GetSqlByAudit(List<string> numbers)
        {

            string sql = string.Format(@"/*dialect*/ select  h.FBILLNO ,k.FNUMBER customerID, i.FCARRYBILLNO,i.F_HS_DeliDate ,j.F_HS_Channel,i.F_HS_QueryUrl , m.FNUMBER materialNO,  b.FREALQTY  QTY        
                                                            From t_sal_outStock a
                                                            inner join t_sal_outStockentry b on a.fid=b.fid
                                                            inner join T_SAL_OUTSTOCKENTRY_LK d on  b.fentryid=d.fentryid
                                                            inner join T_SAL_DELIVERYNOTICEENTRY e on d.fsbillID=e.fid and d.fsid=e.fentryID
                                                            inner join T_SAL_DELIVERYNOTICEENTRY_LK f on e.fentryid=f.fentryid
                                                            inner join T_SAL_OrderEntry g on  f.fsbillID=g.fid and f.fsid=g.fentryID
                                                            inner join T_SAL_Order h on g.fid=h.fid
                                                            inner join T_SAL_DELIVERYNOTICETRACE i on e.FID=i.FID
                                                            inner join T_HS_ShipMethods j on i.F_HS_ShipMethods=j.FID
                                                            inner join T_BD_CUSTOMER k on h.F_HS_B2CCUSTID=k.FCUSTID
                                                            inner join T_BD_MATERIAL m  on b.FMATERIALID=m.FMATERIALID
                                                            where a.fbillno
                                                             in(
		                                                           ");

            string condition = FormatNumber(numbers);

            if (!string.IsNullOrWhiteSpace(condition))
            {
                sql += condition;

                return sql;
            }

            return null;
        }

        /// <summary>
        /// 根据审核状态选择需要执行的相应SQL语句
        /// </summary>
        /// <param name="numbers"></param>
        /// <param name="docmentStatus"></param>
        /// <returns></returns>
        private string GetSql(Context ctx, List<DynamicObject> objects)
        {
            List<string> numbers = null;
            string documentStatus = string.Empty;

            if (objects != null && objects.Count > 0)
            {
                numbers = new List<string>();

                foreach (var item in objects)
                {
                    documentStatus = SQLUtils.GetFieldValue(item, "DocumentStatus");
                    numbers.Add(SQLUtils.GetFieldValue(item, "BillNo"));
                }
            }
            if (!string.IsNullOrWhiteSpace(documentStatus))
            {
                if (documentStatus.CompareTo("C") == 0)
                {
                    return GetSqlByAudit(numbers);
                }
            }
            return null;
        }

        /// <summary>
        /// 格式化销售出库单编码
        /// </summary>
        /// <param name="numbers"></param>
        /// <returns></returns>
        private string FormatNumber(List<string> numbers)
        {
            string[] nums = null;
            string condition = "";

            if (numbers != null && numbers.Count > 0)
            {
                nums = new string[numbers.Count];
                for (int i = 0; i < numbers.Count; i++)
                {
                    if (i < numbers.Count - 1)
                    {
                        nums[i] = "'" + numbers[i] + "'" + ", ";
                    }
                    else if (i == numbers.Count - 1)
                    {
                        nums[i] = "'" + numbers[i] + "'" + ")";
                    }
                    condition += nums[i];
                }
            }

            return condition;
        }

        public override IEnumerable<AbsSynchroDataInfo> GetK3Datas(Context ctx, List<DynamicObject> objects,ref HttpResponseResult result)
        {
            result = new HttpResponseResult();
            result.Success = true;

            DynamicObjectCollection coll = SQLUtils.GetObjects(ctx, GetSql(ctx, objects));

            return BuildSalOutStockBillInfo(coll);
        }
    }

}
