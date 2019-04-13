
using Hands.K3.SCM.APP.Entity.SynDataObject;
using Kingdee.BOS.Orm.DataEntity;
using System.Collections.Generic;
using System.Linq;
using Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject;
using Hands.K3.SCM.APP.Utils.Utils;
using Hands.K3.SCM.App.Synchro.Base.Abstract;
using HS.K3.Common.Abbott;

namespace Hands.K3.SCM.App.Core.SynchroService.ToHC
{
    public class SynOrderStatusToHC:AbstractSynchroToHC
    {
        override public SynchroDataType DataType
        {
            get
            {
                return SynchroDataType.SaleOrderStatus;
            }
        }

        /// <summary>
        /// 5a97d35c3e9e06:HCWebProcessingOder
        /// 5a97d3363e9e04：HCWebPendingOder
        /// 5a97d3123e9dff：XXBJDD
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public HashSet<K3SalOrderStatusInfo> GetAllK3SalOrderStatusInfo()
        {

            string sql = string.Format(@"select distinct a.FBillNo,a.FCloseStatus,a.FCancelStatus, F_HS_PaymentStatus,F_HS_PaymentMode  , null shipStatus
                                        from T_SAL_ORDER a
                                        inner join T_BD_CUSTOMER b on b.FCUSTID= a.FCUSTID    
                                        inner join T_BAS_ASSISTANTDATAENTRY_L c ON a.F_HS_SaleOrderSource=c.FENTRYID
                                        inner join T_BAS_ASSISTANTDATAENTRY d ON c.FentryID=d.FentryID
                                        inner join T_SAL_ORDERentry e on a.FID = e.FID
                                        inner join T_SAL_ORDERENTRY_R f on a.fid=f.fid and e.fentryID=f.fentryID
                                        inner join T_BD_MATERIAL g on e.FMaterialID=g.FMaterialID
                                        where (a.FCLOSESTATUS='B' or a.FCANCELSTATUS='B' and  (a.FDOCUMENTSTATUS='D' and  d.fnumber  = 'XXBJDD' and  a.FBillNo not like '%#%' or a.FDOCUMENTSTATUS='A' and  d.fnumber  = 'HCWebPendingOder'  )  )  
	                                        and  exists(select * From  T_SAL_ORDERentry where fid=a.fid and FMRPCLOSESTATUS='A')
	                                        and (d.fnumber = 'HCWebProcessingOder' or d.fnumber  = 'HCWebPendingOder' or  d.fnumber  = 'XXBJDD' and  a.FBillNo not like '%#%') 
	                                        and a.FsaleOrgID = 100035 and a.FDate >= DATEADD(MONTH,-2,GETDATE())
	                                        and g.fnumber<>'99.01'  
                                        union all
                                        select distinct a.FBillNo,a.FCloseStatus,a.FCancelStatus, F_HS_PaymentStatus,F_HS_PaymentMode  ,'shipped' shipStatus
                                        from T_SAL_ORDER a
                                        inner join T_BD_CUSTOMER b on b.FCUSTID= a.FCUSTID    
                                        inner join T_BAS_ASSISTANTDATAENTRY_L c ON a.F_HS_SaleOrderSource=c.FENTRYID
                                        inner join T_BAS_ASSISTANTDATAENTRY d ON c.FentryID=d.FentryID
                                        inner join T_SAL_ORDERentry e on a.FID = e.FID
                                        inner join T_SAL_ORDERENTRY_R f on a.fid=f.fid and e.fentryID=f.fentryID
                                        inner join T_BD_MATERIAL g on e.FMaterialID=g.FMaterialID
                                        where a.FCANCELSTATUS<>'B'  
                                        and exists(select * From  T_SAL_ORDERentry t1 inner join T_SAL_ORDERENTRY_R  t2 on t1.fid=t2.fid and t1.fentryID=t2.fentryID  where t1.fid=a.fid and t2.FREMAINOUTQTY<t1.FQTY and t2.FREMAINOUTQTY>=0 )
                                        and (d.fnumber = 'HCWebProcessingOder' or d.fnumber  = 'HCWebPendingOder' or  d.fnumber  = 'XXBJDD' and  a.FBillNo not like '%#%') 
                                        and a.FsaleOrgID = 100035 and a.FDate >= DATEADD(MONTH,-2,GETDATE()) 
                                        and g.fnumber<>'99.01'

                                   ");
            DynamicObjectCollection coll = SQLUtils.GetObjects(this.K3CloudContext, sql);

            return MergeOrderStauts(BuildK3SalOrderStatusInfos(coll));
        }

        public static HashSet<K3SalOrderStatusInfo> BuildK3SalOrderStatusInfos(DynamicObjectCollection coll)
        {
            HashSet<K3SalOrderStatusInfo> lstStatus = null;
            K3SalOrderStatusInfo status = null;

            if (coll != null && coll.Count > 0)
            {
                lstStatus = new HashSet<K3SalOrderStatusInfo>();

                foreach (var item in coll)
                {
                    if (item != null)
                    {
                        status = new K3SalOrderStatusInfo();

                        status.SrcNo = SQLUtils.GetFieldValue(item, "FBillNo");
                        status.BillNo = SQLUtils.GetFieldValue(item, "FBillNo");
                        status.CloseStatus = SQLUtils.GetFieldValue(item, "FCloseStatus");

                        status.CancelStatus = SQLUtils.GetFieldValue(item, "FCancelStatus");
                        status.PaymentStatus = SQLUtils.GetFieldValue(item, "F_HS_PaymentStatus");
                        status.F_HS_PaymentMode = SQLUtils.GetFieldValue(item, "F_HS_PaymentMode");
                        status.ShipStatus = SQLUtils.GetFieldValue(item, "ShipStatus");

                        lstStatus.Add(status);
                    }
                }
            }
            return lstStatus;
        }

        public static HashSet<K3SalOrderStatusInfo> MergeOrderStauts(HashSet<K3SalOrderStatusInfo> oStatus)
        {
            HashSet<K3SalOrderStatusInfo> merges = null;
          
            if (oStatus != null && oStatus.Count > 0)
            {
                var group = from o in oStatus group o by o.BillNo into g select g;

                if (group != null && group.Count() > 0)
                {
                    merges = new HashSet<K3SalOrderStatusInfo>();
                    
                    foreach (var g in group)
                    {
                        if (g != null && g.Count() == 2)
                        {
                            for (int i = 0; i < g.Count(); i++)
                            {
                                if (!string.IsNullOrWhiteSpace(g.ElementAt(i).ShipStatus))
                                {
                                    if (g.ElementAt(i).ShipStatus.CompareTo("shipped") == 0)
                                    {
                                        merges.Add(g.ElementAt(i));
                                        break;
                                    }   
                                }         
                            }
                        }
                        else if (g != null && g.Count() == 1)
                        {
                            merges.Add(g.ElementAt(0));
                        }

                    }
                }
            }

            return merges;
        }

        public override IEnumerable<AbsSynchroDataInfo> GetK3Datas(IEnumerable<string> billNos = null,bool flag = true)
        {
           return GetAllK3SalOrderStatusInfo();
        }
    }
}
