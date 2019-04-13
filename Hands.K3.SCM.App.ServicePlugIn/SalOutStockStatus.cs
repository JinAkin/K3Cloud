using Hands.K3.SCM.APP.Entity.SynDataObject;
using Kingdee.BOS;
using Kingdee.BOS.Orm.DataEntity;
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
    [Description("销售出库单--上查销售订单状态同步插件")]
    public class SalOutStockStatus : AbstractOSPlugIn
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

        }
        public override void EndOperationTransaction(Kingdee.BOS.Core.DynamicForm.PlugIn.Args.EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);

            //if (e.DataEntitys == null) return;
            //List<DynamicObject> dataEntitys = e.DataEntitys.ToList();

            //if (dataEntitys == null || dataEntitys.Count <= 0)
            //{
            //    return;
            //}

            //this.DyamicObjects = e.DataEntitys.ToList();
            //SynchroK3DataToWebSite(this.Context);
            //IEnumerable<AbsSynchroDataInfo> datas = GetK3Datas(this.Context, this.DyamicObjects);

            //if (datas != null && datas.Count() > 0)
            //{
            //    SynchroDataHelper.WriteSynSalOrderStatus(this.Context, datas.Select(o => (K3SalOrderStatusInfo)o).ToList());
            //}
        }

        public override void AfterExecuteOperationTransaction(AfterExecuteOperationTransaction e)
        {
            base.AfterExecuteOperationTransaction(e);
            HttpResponseResult result = null;
            var task = Task.Factory.StartNew(() =>
            {
                if (e.DataEntitys == null) return;
                List<DynamicObject> dataEntitys = e.DataEntitys.ToList();

                if (dataEntitys == null || dataEntitys.Count <= 0)
                {
                    return;
                }

                this.DyamicObjects = e.DataEntitys.ToList();
                SynchroK3DataToWebSite(this.Context);
                IEnumerable<AbsSynchroDataInfo> datas = GetK3Datas(this.Context, this.DyamicObjects,ref result);

                if (datas != null && datas.Count() > 0)
                {
                    LogHelper.WriteSynSalOrderStatus(this.Context, datas.Select(o => (K3SalOrderStatusInfo)o).ToList());
                }
            }
                                            );

        }

        /// <summary>
        /// 根据审核状态获取销售出库单信息
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="numbers"></param>
        /// <param name="docmentStatus"></param>
        /// <returns></returns>
        public List<K3SalOrderStatusInfo> GetSalOutStockBills(Context ctx, List<string> numbers, string docmentStatus)
        {
            List<K3SalOrderStatusInfo> lstInfo = null;
            HashSet<K3SalOrderStatusInfo>[] infos = null;
            List<string> sqls = GetSql(ctx, numbers, docmentStatus);

            if (sqls != null && sqls.Count > 0)
            {
                infos = new HashSet<K3SalOrderStatusInfo>[sqls.Count];
                for (int i = 0; i < sqls.Count; i++)
                {
                    DynamicObjectCollection coll = SQLUtils.GetObjects(ctx, sqls[i]);
                    infos[i] = BuildSalOrderStatusInfo(coll);
                }
                if (infos != null && infos.Count() > 0)
                {
                    for (int i = 0; i < infos.Count(); i++)
                    {
                        if (infos[i] != null)
                        {
                            if (infos.Count() == 1)
                            {
                                lstInfo = infos[i].ToList<K3SalOrderStatusInfo>();
                            }
                            else if (i + 1 == 1)
                            {
                                if (infos[i + 1] != null)
                                {
                                    lstInfo = infos[i].Concat(infos[i + 1]).ToList<K3SalOrderStatusInfo>();
                                }
                            }
                            else if (i > 1 && i <= infos.Count() - 1)
                            {
                                if (lstInfo != null)
                                {
                                    lstInfo = lstInfo.Concat(infos[i]).ToList<K3SalOrderStatusInfo>();
                                }
                            }
                        }

                    }
                }
            }

            return lstInfo;
        }

        /// <summary>
        /// 查询结果集封装对象
        /// </summary>
        /// <param name="coll"></param>
        /// <returns></returns>
        private HashSet<K3SalOrderStatusInfo> BuildSalOrderStatusInfo(DynamicObjectCollection coll)
        {
            HashSet<K3SalOrderStatusInfo> bills = new HashSet<K3SalOrderStatusInfo>();
            K3SalOrderStatusInfo bill = null;

            if (coll != null && coll.Count > 0)
            {
                foreach (var item in coll)
                {
                    if (item != null)
                    {
                        bill = new K3SalOrderStatusInfo();

                        bill.SrcNo = SQLUtils.GetFieldValue(item, "FBillNo");
                        bill.BillNo = SQLUtils.GetFieldValue(item, "FBillNo");
                        bill.CancelStatus = SQLUtils.GetFieldValue(item, "FCloseStatus");
                        bill.CloseStatus = SQLUtils.GetFieldValue(item, "FCancelStatus");
                        bill.PaymentStatus = SQLUtils.GetFieldValue(item, "F_HS_PaymentStatus");
                        bill.F_HS_PaymentMode = SQLUtils.GetFieldValue(item, "F_HS_PaymentMode");
                        bill.ShipStatus = SQLUtils.GetFieldValue(item, "shipStatus");

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
        private List<string> GetSqlByAudit(List<string> numbers)
        {
            List<string> sqls = new List<string>();

            string sql = string.Format(@"/*dialect*/ select distinct a.FBillNo,a.fDate,  a.FCloseStatus,a.FCancelStatus, F_HS_PaymentStatus,F_HS_PaymentMode  ,'shipped' shipStatus
                                                        from T_SAL_ORDER a
                                                        inner join T_BD_CUSTOMER b on b.FCUSTID= a.FCUSTID    
                                                        inner join T_BAS_ASSISTANTDATAENTRY_L c ON a.F_HS_SaleOrderSource=c.FENTRYID
                                                        inner join T_BAS_ASSISTANTDATAENTRY d ON c.FentryID=d.FentryID
                                                        inner join T_SAL_ORDERentry e on a.FID = e.FID
                                                        inner join T_SAL_ORDERENTRY_R f on a.fid=f.fid and e.fentryID=f.fentryID
                                                        inner join T_BD_MATERIAL g on e.FMaterialID=g.FMaterialID
                                                        inner join
                                                                  (select distinct h.FbillNo From t_sal_outStock a
			                                                        inner join t_sal_outStockentry b on a.fid=b.fid
			                                                        inner join t_sal_outStockentry_R c on b.fid=c.fid and b.fentryid=c.fentryid
			                                                        inner join T_SAL_OUTSTOCKENTRY_LK d on  b.fentryid=d.fentryid
			                                                        inner join T_SAL_DELIVERYNOTICEENTRY e on d.fsbillID=e.fid and d.fsid=e.fentryID
			                                                        inner join T_SAL_DELIVERYNOTICEENTRY_LK f on e.fentryid=f.fentryid
			                                                        inner join T_SAL_OrderEntry g on  f.fsbillID=g.fid and f.fsid=g.fentryID
			                                                        inner join T_SAL_Order h on g.fid=h.fid
			                                                        where a.fbillno in(
		                                                           ");

            string condition = FormatNumber(numbers);

            if (!string.IsNullOrWhiteSpace(condition))
            {
                sql += condition + string.Format(@") t on a.fBillno=t.FbillNo
                                                        where a.FCANCELSTATUS <> 'B'
                                                        and exists(select * From  T_SAL_ORDERentry t1 inner join T_SAL_ORDERENTRY_R  t2 on t1.fid = t2.fid and t1.fentryID = t2.fentryID  where t1.fid = a.fid and t2.FREMAINOUTQTY < t1.FQTY and t2.FREMAINOUTQTY >= 0 )
                                                        and(d.fnumber = 'HCWebProcessingOder' or d.fnumber = 'HCWebPendingOder' or  d.fnumber = 'XXBJDD' and  a.FBillNo not like '%#%')
                                                        and a.FsaleOrgID = 100035 and a.FDate >= '2018-03-15'
                                                        and g.fnumber <> '99.01'");
                sqls.Add(sql);
                return sqls;
            }


            return null;
        }

        /// <summary>
        /// 反审核需要执行的SQL语句
        /// </summary>
        /// <param name="numbers"></param>
        /// <returns></returns>
        private List<string> GetSqlByUnAduit(Context ctx, List<string> numbers)
        {
            List<string> sqls = new List<string>(); ;
            if (numbers != null && numbers.Count > 0)
            {
                foreach (var num in numbers)
                {
                    string sql = string.Format(@"/*dialect*/ select distinct a.FBillNo,a.fDate,  a.FCloseStatus,a.FCancelStatus, F_HS_PaymentStatus,F_HS_PaymentMode  ,'shipped' shipStatus
                                                    from T_SAL_ORDER a
                                                    inner join T_BD_CUSTOMER b on b.FCUSTID= a.FCUSTID    
                                                    inner join T_BAS_ASSISTANTDATAENTRY_L c ON a.F_HS_SaleOrderSource=c.FENTRYID
                                                    inner join T_BAS_ASSISTANTDATAENTRY d ON c.FentryID=d.FentryID
                                                    inner join T_SAL_ORDERentry e on a.FID = e.FID
                                                    inner join T_SAL_ORDERENTRY_R f on a.fid=f.fid and e.fentryID=f.fentryID
                                                    inner join T_BD_MATERIAL g on e.FMaterialID=g.FMaterialID
                                                    inner join
                                                                (select distinct h.FbillNo From t_sal_outStock a
			                                                    inner join t_sal_outStockentry b on a.fid=b.fid
			                                                    inner join t_sal_outStockentry_R c on b.fid=c.fid and b.fentryid=c.fentryid
			                                                    inner join T_SAL_OUTSTOCKENTRY_LK d on  b.fentryid=d.fentryid
			                                                    inner join T_SAL_DELIVERYNOTICEENTRY e on d.fsbillID=e.fid and d.fsid=e.fentryID
			                                                    inner join T_SAL_DELIVERYNOTICEENTRY_LK f on e.fentryid=f.fentryid
			                                                    inner join T_SAL_OrderEntry g on  f.fsbillID=g.fid and f.fsid=g.fentryID
			                                                    inner join T_SAL_Order h on g.fid=h.fid
			                                                    where a.fbillno='{0}'
		                                                        ) t on a.fBillno=t.FbillNo
                                                    where a.FCANCELSTATUS<>'B'  
                                                    and exists(select * From  T_SAL_ORDERentry t1 inner join T_SAL_ORDERENTRY_R  t2 on t1.fid=t2.fid and t1.fentryID=t2.fentryID  where t1.fid=a.fid and t2.FREMAINOUTQTY<t1.FQTY and t2.FREMAINOUTQTY>=0 )
                                                    and (d.fnumber = 'HCWebProcessingOder' or d.fnumber  = 'HCWebPendingOder' or  d.fnumber  = 'XXBJDD' and  a.FBillNo not like '%#%') 
                                                    and a.FsaleOrgID = 100035 and a.FDate >= '2018-03-15' 
                                                    and  g.fnumber<>'99.01'
		                                               
                                            ", num);

                    DynamicObjectCollection coll = SQLUtils.GetObjects(ctx, sql);

                    if (coll != null && coll.Count() > 0)
                    {
                        return null;
                    }
                    else
                    {
                        string sql_ = string.Format(@"/*dialect*/ select distinct a.FBillNo,a.fDate,  a.FCloseStatus,a.FCancelStatus, F_HS_PaymentStatus,F_HS_PaymentMode 
	                                                            from T_SAL_ORDER a
	                                                            inner join T_BD_CUSTOMER b on b.FCUSTID= a.FCUSTID    
	                                                            inner join T_BAS_ASSISTANTDATAENTRY_L c ON a.F_HS_SaleOrderSource=c.FENTRYID
	                                                            inner join T_BAS_ASSISTANTDATAENTRY d ON c.FentryID=d.FentryID
	                                                            inner join T_SAL_ORDERentry e on a.FID = e.FID
	                                                            inner join T_SAL_ORDERENTRY_R f on a.fid=f.fid and e.fentryID=f.fentryID
	                                                            inner join T_BD_MATERIAL g on e.FMaterialID=g.FMaterialID
	                                                            inner join
			                                                              (select distinct h.FbillNo From t_sal_outStock a
				                                                            inner join t_sal_outStockentry b on a.fid=b.fid
				                                                            inner join t_sal_outStockentry_R c on b.fid=c.fid and b.fentryid=c.fentryid
				                                                            inner join T_SAL_OUTSTOCKENTRY_LK d on  b.fentryid=d.fentryid
				                                                            inner join T_SAL_DELIVERYNOTICEENTRY e on d.fsbillID=e.fid and d.fsid=e.fentryID
				                                                            inner join T_SAL_DELIVERYNOTICEENTRY_LK f on e.fentryid=f.fentryid
				                                                            inner join T_SAL_OrderEntry g on  f.fsbillID=g.fid and f.fsid=g.fentryID
				                                                            inner join T_SAL_Order h on g.fid=h.fid
				                                                            where a.fbillno='{0}'
			                                                               ) t on a.fBillno=t.FbillNo
	                                                            where a.FCANCELSTATUS<>'B'  
	                                                            and (d.fnumber = 'HCWebProcessingOder' or d.fnumber  = 'HCWebPendingOder' or  d.fnumber  = 'XXBJDD' and  a.FBillNo not like '%#%') 
	                                                            and a.FsaleOrgID = 100035 and a.FDate >= '2018-03-15' 
	                                                            and  g.fnumber<>'99.01'", num);
                        sqls.Add(sql_);

                    }
                }
                return sqls;
            }

            return null;
        }

        /// <summary>
        /// 根据审核状态选择需要执行的相应SQL语句
        /// </summary>
        /// <param name="numbers"></param>
        /// <param name="docmentStatus"></param>
        /// <returns></returns>
        private List<string> GetSql(Context ctx, List<string> numbers, string docmentStatus)
        {
            if (!string.IsNullOrWhiteSpace(docmentStatus))
            {
                if (docmentStatus.CompareTo("C") == 0)
                {
                    return GetSqlByAudit(numbers);
                }
                if (docmentStatus.CompareTo("D") == 0)
                {
                    return GetSqlByUnAduit(ctx, numbers);
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
            List<string> numbers = new List<string>();
            string docmentStatus = string.Empty;

            if (objects != null && objects.Count > 0)
            {
                numbers = new List<string>();

                foreach (var item in objects)
                {
                    if (item != null)
                    {
                        numbers.Add(SQLUtils.GetFieldValue(item, "BillNo"));//订单号
                        docmentStatus = SQLUtils.GetFieldValue(item, "DocumentStatus");
                    }
                }
            }
            
            return GetSalOutStockBills(ctx,numbers,docmentStatus);
        }
    }
}
