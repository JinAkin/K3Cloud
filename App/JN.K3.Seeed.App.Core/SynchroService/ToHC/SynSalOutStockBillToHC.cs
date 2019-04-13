
using Hands.K3.SCM.APP.Entity.StructType;
using Hands.K3.SCM.APP.Entity.SynDataObject.SalOutStock;
using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Orm.DataEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject;
using Hands.K3.SCM.APP.Utils.Utils;
using Hands.K3.SCM.App.Synchro.Base.Abstract;
using HS.K3.Common.Abbott;

namespace Hands.K3.SCM.App.Core.SynchroService.ToHC
{
    public class SynSalOutStockBillToHC : AbstractSynchroToHC
    {
        public override SynchroDataType DataType
        {
            get
            {
                return SynchroDataType.ImportLogis;
            }
        }

        /// <summary>
        /// 获取运单号的相关信息
        /// </summary>
        /// <param name="numbers"></param>
        /// <returns></returns>
        public List<SalOutStockBillInfo> GetSalOutStockBills(IEnumerable<string> numbers)
        {
            DynamicObjectCollection coll = SQLUtils.GetObjects(this.K3CloudContext, GetSql(numbers));

            return BuildSalOutStockBillInfo(coll);
        }

        /// <summary>
        /// 查询结果集封装对象
        /// </summary>
        /// <param name="coll"></param>
        /// <returns></returns>
        private List<SalOutStockBillInfo> BuildSalOutStockBillInfo(DynamicObjectCollection coll)
        {
            List<SalOutStockBillInfo> bills = new List<SalOutStockBillInfo>();
            SalOutStockBillInfo bill = null;
            SalOutStockEntry entry = null;

            if (coll != null && coll.Count > 0)
            {
                foreach (var item in coll)
                {
                    if (item != null)
                    {
                        bill = new SalOutStockBillInfo();

                        bill.SrcNo = SQLUtils.GetFieldValue(item, "FDeliBillNo");
                        bill.BillNo = SQLUtils.GetFieldValue(item, "FDeliBillNo");
                        bill.OrderBillNo = SQLUtils.GetFieldValue(item, "FBillNo");
                        bill.CustomerNo = SQLUtils.GetFieldValue(item, "customerID");
                        bill.CarriageNO = SQLUtils.GetFieldValue(item, "F_HS_CARRYBILLNO");
                        bill.DeliDate = SQLUtils.GetFieldValue(item, "F_HS_DeliDate");
                        bill.LogisticsChannel = SQLUtils.GetLogisticsChannelNo(this.K3CloudContext, SQLUtils.GetFieldValue(item, "F_HS_Channel"));
                        bill.QueryURL = SQLUtils.GetFieldValue(item, "F_HS_QueryURL");

                        entry = new SalOutStockEntry();
                        entry.StockName = SQLUtils.GetFieldValue(item, "F_HS_STOCKHSNAME");
                        entry.MaterialNo = SQLUtils.GetFieldValue(item, "materialNO");
                        entry.Quantity = Convert.ToDecimal(SQLUtils.GetFieldValue(item, "QTY"));
                        entry.Price = Math.Round(Convert.ToDecimal(SQLUtils.GetFieldValue(item, "price")), 4);
                        entry.IntegralReturnRate = Convert.ToDecimal(SQLUtils.GetFieldValue(item, "F_HS_IntegralReturnRate"));
                        bill.StockEntry.Add(entry);
                        bills.Add(bill);
                    }
                }
            }

            return bills;
        }

        /// <summary>
        /// 合并单据
        /// </summary>
        /// <param name="bills"></param>
        /// <returns></returns>
        private static List<SalOutStockBillInfo> CombineBills(List<SalOutStockBillInfo> bills)
        {
            List<SalOutStockBillInfo> oStocks = null;
            SalOutStockBillInfo oStock = null;

            if (bills != null && bills.Count > 0)
            {
                var groups = from s in bills group s by s.BillNo into gr select gr;

                if (groups != null && groups.Count() > 0)
                {
                    oStocks = new List<SalOutStockBillInfo>();

                    foreach (var group in groups)
                    {
                        List<SalOutStockBillInfo> lst = group.ToList<SalOutStockBillInfo>();
                        var gro = from a in lst group a by a.OrderBillNo into g select g;

                        if (gro != null && gro.Count() > 0)
                        {
                            foreach (var g in gro)
                            {
                                if (g != null)
                                {
                                    oStock = g.ElementAt(0) as SalOutStockBillInfo;
                                    oStock.StockEntry = GetSalOutStockEntry(g.ToList<SalOutStockBillInfo>());

                                    oStocks.Add(oStock);
                                }
                            }
                        }
                    }
                }
            }

            return oStocks;
        }

        /// <summary>
        /// 获取明细信息
        /// </summary>
        /// <param name="bills"></param>
        /// <returns></returns>
        private static List<SalOutStockEntry> GetSalOutStockEntry(List<SalOutStockBillInfo> bills)
        {
            List<SalOutStockEntry> entries = null;
            SalOutStockEntry entry = null;

            if (bills != null && bills.Count > 0)
            {
                entries = new List<SalOutStockEntry>();

                foreach (var bill in bills)
                {
                    if (bill != null)
                    {
                        if (bill.StockEntry != null && bill.StockEntry.Count > 0)
                        {
                            foreach (var en in bill.StockEntry)
                            {
                                if (en != null)
                                {
                                    entry = new SalOutStockEntry();

                                    entry.StockName = en.StockName;
                                    entry.MaterialNo = en.MaterialNo;
                                    entry.Quantity = en.Quantity;
                                    entry.Price = en.Price;
                                    entry.IntegralReturnRate = en.IntegralReturnRate;
                                    entries.Add(entry);

                                }

                            }
                        }
                    }
                }
            }

            return entries;
        }

        /// <summary>
        /// 审核需要执行的SQL语句
        /// </summary>
        /// <param name="numbers"></param>
        /// <returns></returns>
        private string GetSql(IEnumerable<string> numbers = null)
        {

            if (numbers != null && numbers.Count() > 0)
            {
                //运单号上传调用的sql
                string sql = string.Format(@"/*dialect*/ select  h.FBILLNO ,k.FNUMBER customerID, i.F_HS_CARRYBILLNO, i.F_HS_DeliDate ,j.F_HS_Channel,i.F_HS_QueryUrl , 
                                                        m.FNUMBER materialNO, b.FQTY  QTY ,n.F_HS_STOCKHSNAME  ,a.FBILLNO FDeliBillNo,case when z.FNUMBER='SPQC' then 0 else o.FPRICE*(1-o.FDiscountRate/100)/h.F_HS_RATETOUSA end price ,m.F_HS_IntegralReturnRate
                                                        ,case when z.FNUMBER='SPQC' then 0 else (case when m.F_HS_IsOil='3' or m.F_HS_IsPuHuo='4' then 5 else 1 end) end F_HS_IntegralReturnRate
                                                        from T_SAL_DELIVERYNOTICE a 
                                                        inner join T_SAL_DELIVERYNOTICEENTRY b on a.fid=b.fid
                                                        inner join T_SAL_DELIVERYNOTICEENTRY_LK f on b.fentryid=f.fentryid
                                                        inner join T_SAL_OrderEntry g on  f.fsbillID=g.fid and f.fsid=g.fentryID
                                                        inner join T_SAL_Order h on g.fid=h.fid
														inner join T_SAL_ORDERENTRY_F o on o.FID = g.FID and g.FENTRYID = o.FENTRYID
                                                        inner join (select a.fid , max(F_HS_ShipMethods) F_HS_ShipMethods, max(F_HS_DeliDate) F_HS_DeliDate,max(F_HS_QueryUrl) F_HS_QueryUrl 
                                                                                  ,F_HS_CARRYBILLNO=STUFF((select ',' + t.F_HS_CARRYBILLNO
													                                                        from HS_T_LogisTrack t
													                                                        where  len(F_HS_CARRYBILLNO)>0 and t.fid = a.fid   
													                                                        for xml path('')
													                                                        ) , 1 , 1 , '')  
		                                                           from HS_T_LogisTrack a
		                                                           where len(a.F_HS_CARRYBILLNO)>0
                                                                   and a.F_HS_ISLOGISTICS=0
		                                                           group by a.fid
		                                                          ) i on a.FID=i.FID
                                                        inner join T_HS_ShipMethods j on i.F_HS_ShipMethods=j.FID
                                                        inner join T_BD_CUSTOMER k on h.F_HS_B2CCUSTID=k.FCUSTID
                                                        inner join T_BD_MATERIAL m  on b.FMATERIALID=m.FMATERIALID
														left join T_BAS_ASSISTANTDATAENTRY_L x ON m.F_HS_PRODUCTSTATUS=x.FENTRYID
														left join T_BAS_ASSISTANTDATAENTRY z ON x.FentryID=z.FentryID
                                                        left join T_BD_STOCK n on b.FSHIPMENTSTOCKID=n.FSTOCKID
                                                        left join  T_BAS_BILLTYPE t on h.FBILLTypeID=t.FBILLTypeID
                                                        left join T_BAS_ASSISTANTDATAENTRY s ON h.F_HS_SaleOrderSource=s.FentryID
                                                        where a.FBILLNO in ('{0}')
                                                        and a.F_HS_RETURNEDINTEGRAL =0 
											            and m.FNUMBER not like '99%'
											            and s.fnumber in ('HCWebProcessingOder','HCWebPendingOder', 'XXBJDD')  
											            and h.FsaleOrgID = 100035 and h.FCANCELSTATUS<>'B' 
											            and t.FNUMBER='XSDD01_SYS'", string.Join("','", numbers));






                return sql;

            }
            //定时任务调用的sql
            return string.Format(@"/*dialect*/ select  h.FBILLNO ,k.FNUMBER customerID, i.F_HS_CARRYBILLNO, i.F_HS_DeliDate ,j.F_HS_Channel,i.F_HS_QueryUrl , 
                                                m.FNUMBER materialNO, b.FQTY  QTY ,n.F_HS_STOCKHSNAME  ,a.FBILLNO FDeliBillNo,case when z.FNUMBER='SPQC' then 0 else o.FPRICE*(1-o.FDiscountRate/100)/h.F_HS_RATETOUSA end price ,m.F_HS_IntegralReturnRate
                                                ,case when z.FNUMBER='SPQC' then 0 else (case when m.F_HS_IsOil='3' or m.F_HS_IsPuHuo='4' then 5 else 1 end) end F_HS_IntegralReturnRate
                                                from T_SAL_DELIVERYNOTICE a 
                                                inner join T_SAL_DELIVERYNOTICEENTRY b on a.fid=b.fid
                                                inner join T_SAL_DELIVERYNOTICEENTRY_LK f on b.fentryid=f.fentryid
                                                inner join T_SAL_OrderEntry g on  f.fsbillID=g.fid and f.fsid=g.fentryID
                                                inner join T_SAL_Order h on g.fid=h.fid
												inner join T_SAL_ORDERENTRY_F o on o.FID = g.FID and g.FENTRYID = o.FENTRYID
                                                inner join (select a.fid , max(F_HS_ShipMethods) F_HS_ShipMethods, max(F_HS_DeliDate) F_HS_DeliDate,max(F_HS_QueryUrl) F_HS_QueryUrl 
                                                                            ,F_HS_CARRYBILLNO=STUFF((select ',' + t.F_HS_CARRYBILLNO
													                                                from HS_T_LogisTrack t
													                                                where  len(F_HS_CARRYBILLNO)>0 and t.fid = a.fid   
													                                                for xml path('')
													                                                ) , 1 , 1 , '')  
		                                                    from HS_T_LogisTrack a
		                                                    where len(a.F_HS_CARRYBILLNO)>0
		                                                    group by a.fid
		                                                    ) i on a.FID=i.FID
                                                inner join T_HS_ShipMethods j on i.F_HS_ShipMethods=j.FID
                                                inner join T_BD_CUSTOMER k on h.F_HS_B2CCUSTID=k.FCUSTID
                                                inner join T_BD_MATERIAL m  on b.FMATERIALID=m.FMATERIALID
												left join T_BAS_ASSISTANTDATAENTRY_L x ON m.F_HS_PRODUCTSTATUS=x.FENTRYID
												left join T_BAS_ASSISTANTDATAENTRY z ON x.FentryID=z.FentryID
                                                left join T_BD_STOCK n on b.FSHIPMENTSTOCKID=n.FSTOCKID
                                                left join  T_BAS_BILLTYPE t on h.FBILLTypeID=t.FBILLTypeID
                                                left join T_BAS_ASSISTANTDATAENTRY s ON h.F_HS_SaleOrderSource=s.FentryID 
												where a.F_HS_RETURNEDINTEGRAL =0 
                                                and m.FNUMBER not like '99%'
												and s.fnumber in ('HCWebProcessingOder','HCWebPendingOder', 'XXBJDD')  
												and h.FsaleOrgID = 100035 and h.FCANCELSTATUS<>'B' 
												and t.FNUMBER='XSDD01_SYS'
                                                and a.FDate >= DATEADD(MONTH,-1,GETDATE())"


                        );
        }


        /// <summary>
        /// 根据运单号同步状态标记发货通知单的字段
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="oStocks"></param>
        /// <param name="flag"></param>
        public void MarkSynSuccessOrNo(Context ctx, List<SalOutStockBillInfo> oStocks, bool flag)
        {
            string mark = flag ? "1" : "0";
            int count = 0;
            List<string> billNos = null;

            if (oStocks != null && oStocks.Count > 0)
            {
                billNos = oStocks.Select(o => o.BillNo).ToList();
                string sql = string.Format(@"/*dialect*/ update {0} set F_HS_RETURNEDINTEGRAL = '{1}' 
                                                            where FBILLNO in ('{2}') ",
                                                        HSTableConst.T_SAL_DELIVERYNOTICE, mark, string.Join("','", billNos)
                                           );
                try
                {
                    count = DBUtils.Execute(ctx, sql);

                    if (count > 0)
                    {
                        LogUtils.WriteSynchroLog(ctx, SynchroDataType.DeliveryNoticeBill, "发货通知单号【" + string.Join(",", billNos) + "】更新同步标记成功");
                    }
                    else
                    {
                        LogUtils.WriteSynchroLog(ctx, SynchroDataType.DeliveryNoticeBill, "发货通知单号【" + string.Join(",", billNos) + "】更新同步标记失败");
                    }
                }
                catch (Exception ex)
                {
                    LogUtils.WriteSynchroLog(ctx, SynchroDataType.DeliveryNoticeBill, "发货通知单号【" + string.Join(",", billNos )+ "】更新同步标记失败" + System.Environment.NewLine + ex.Message + System.Environment.NewLine + ex.StackTrace);
                }  
            }
        }

        /// <summary>
        /// 获取同步运单号失败后发货通知单所对应的编码
        /// </summary>
        /// <param name="datas"></param>
        /// <param name="flag"></param>

        public override void UpdateAfterSynchro(IEnumerable<AbsSynchroDataInfo> datas, bool flag)
        {
            MarkSynSuccessOrNo(this.K3CloudContext, datas.Select(o => (SalOutStockBillInfo)o).ToList(), flag);
        }
        public override IEnumerable<AbsSynchroDataInfo> GetK3Datas(IEnumerable<string> billNos, bool flag = true)
        {
            List<SalOutStockBillInfo> oStocks = GetSalOutStockBills(billNos);
            oStocks = CombineBills(oStocks);

            return oStocks;
        }
    }
}
