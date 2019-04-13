using System;
using System.Collections.Generic;
using System.Linq;
using Hands.K3.SCM.App.Synchro.Base.Abstract;

using Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject;
using Hands.K3.SCM.APP.Entity.SynDataObject.OnTheWay;
using Hands.K3.SCM.APP.Utils.Utils;
using HS.K3.Common.Abbott;
using Kingdee.BOS.Orm.DataEntity;

namespace Hands.K3.SCM.App.Core.SynchroService.ToHC
{
    public class SynOnTheWayInfo : AbstractSynchroToHC
    {
        public override SynchroDataType DataType
        {
            get
            {
                return SynchroDataType.OnTheWay;
            }
        }
        public Dictionary<DateTime, IEnumerable<string>> SynchroDataLog = new Dictionary<DateTime, IEnumerable<string>>();

        /// <summary>
        /// 同步在途明细前将Redis的历史数据清空
        /// </summary>
        /// <param name="now"></param>
        public void BeforeSynchroOperate(DateTime now)
        {
            if (SynchroDataLog != null && SynchroDataLog.Count > 0)
            {
                foreach (var item in SynchroDataLog)
                {
                    if (item.Key != now)
                    {
                        RemoveRedisData(this.K3CloudContext,item.Value);
                    }
                }
            }
        }
        public virtual string GetSQL()
        {
            return string.Format(@"/*dialect*/  select t1.FNUMBER as FMaterialId,m1.FDeliveryDate,m1.FQTY,m1.FStockId
				                    from T_BD_MATERIAL t1
				                    inner join (
					                    select t5.FNUMBER,Convert(nvarchar(100),ISNULL(t1.F_HS_DUEDATE,''),23) as FDeliveryDate,
					                    (t2.FQTY-t3.FRECEIVEQTY-t4.FJOINPATHLOSSQTY) as FQTY,
					                    ISNULL(t8.FNUMBER,'') as FStockId
					                    from T_STK_STKTRANSFEROUT t1
					                    inner join T_STK_STKTRANSFEROUTENTRY t2 on t1.FID=t2.FID
					                    inner join T_STK_STKTRANSFEROUTENTRY_R t3 on t2.FENTRYID=t3.FENTRYID
					                    inner join T_STK_STKTRANSFEROUTENTRY_T t4 on t2.FENTRYID=t4.FENTRYID
					                    inner join T_BD_MATERIAL t5 on t2.FMATERIALID=t5.FMATERIALID
					                    left join T_BD_STOCK t6 on t2.FDESTSTOCKID=t6.FSTOCKID
										inner join T_BAS_ASSISTANTDATAENTRY_L t7 ON t6.F_HS_DLC=t7.FENTRYID
                                        inner join T_BAS_ASSISTANTDATAENTRY t8 ON t8.FENTRYID=t7.FENTRYID
		                                where t1.FDOCUMENTSTATUS='C' and t1.FCANCELSTATUS<>'B'
					                    and t2.FQTY > t3.FRECEIVEQTY+t4.FJOINPATHLOSSQTY and t1.FVESTONWAY='B'
										and t1.F_HS_ConfirmDeliveryDate = '1'
										and t5.FNUMBER not like '99.%'
                                        and t6.F_HS_TJ = '1'
	                                )m1 on m1.FNUMBER=t1.FNUMBER and t1.FMASTERID=t1.FMATERIALID
				                   
				                union all

					            select t5.FNUMBER as FMaterialId,Convert(nvarchar(100),t2.F_HS_EXPECTEDARRIVALDATE,23)as FDeliveryDate,t2.FQTY
                                ,t7.FNUMBER as FStockId
			                    from T_PUR_POOrder t1
			                    inner join T_PUR_POOrderEntry t2 on t1.FID=t2.FID
			                    inner join HS_T_FinalStock t3 on t2.F_HS_OutStockID=t3.FID 
		                        left join T_BD_Stock t4 on t3.F_HS_STOCKID=t4.FSTOCKID 
		                        inner join T_BD_MATERIAL t5 on t2.FMATERIALID=t5.FMATERIALID
								inner join T_BAS_ASSISTANTDATAENTRY_L t6 ON t4.F_HS_DLC=t6.FENTRYID
                                inner join T_BAS_ASSISTANTDATAENTRY t7 ON t7.FENTRYID=t6.FENTRYID
			                    where (t1.FDOCUMENTSTATUS='C' or t1.FDOCUMENTSTATUS='B')
			                    and t1.FCLOSESTATUS<>'B' and t1.FCANCELSTATUS<>'B'
			                    and t2.FMRPCLOSESTATUS<>'B' and ISNULL(t2.F_HS_OutStockID,0)<>0
								and t2.F_HS_ConfirmDeliveryDate = '1'
								and t5.FNUMBER not like '99.%'
                                and t4.F_HS_TJ = '1'
                                order by FStockId,FDeliveryDate asc");
        }
        public override IEnumerable<AbsSynchroDataInfo> GetK3Datas(IEnumerable<string> billNos = null,bool flag = true)
        {
            List<OnTheWay> ways = null;
            OnTheWay way = null;
            List<OnTheWayEntry> entrys = null;
            OnTheWayEntry entry = null;

            DynamicObjectCollection coll = SQLUtils.GetObjects(this.K3CloudContext,GetSQL());
            var group = from g in coll
                        orderby SQLUtils.GetFieldValue(g, "FMaterialId"),Convert.ToDateTime(SQLUtils.GetFieldValue(g, "FDeliveryDate"))
                        group g by SQLUtils.GetFieldValue(g, "FMaterialId")
                        into c
                        select c;

            if (group != null && group.Count() > 0)
            {
                ways = new List<OnTheWay>();

                foreach (var item in group)
                {
                    if (item != null)
                    {
                        way = new OnTheWay();
                        entrys = new List<OnTheWayEntry>();

                        way.FMaterialId = SQLUtils.GetFieldValue(item.ElementAt(0), "FMaterialId");
                        way.SrcNo = way.FMaterialId;

                        if (item != null && item.Count() > 0)
                        {
                            foreach (var obj in item)
                            {
                                if (obj != null)
                                {
                                    entry = new OnTheWayEntry();

                                    entry.FStockId = SQLUtils.GetFieldValue(obj, "FStockId");
                                    entry.FDeliveryDate = Convert.ToDateTime(SQLUtils.GetFieldValue(obj, "FDeliveryDate")).ToString();
                                    entry.FQty = Convert.ToDecimal(SQLUtils.GetFieldValue(obj, "FQTY"));

                                    entrys.Add(entry);
                                }
                            }

                            way.Entry = entrys;
                            ways.Add(way);
                        }  
                    }
                }
                DateTime now = DateTime.Now;
                SynchroDataLog.Add(now,ways.Select(w => w.SrcNo));
                BeforeSynchroOperate(now);
            }

            return ways;
        }
    }
}
