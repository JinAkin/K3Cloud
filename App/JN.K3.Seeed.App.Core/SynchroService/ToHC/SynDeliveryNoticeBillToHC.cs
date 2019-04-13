using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Hands.K3.SCM.App.Synchro.Base.Abstract;

using Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject;
using Hands.K3.SCM.APP.Entity.SynDataObject.DeliveryNotice;
using Hands.K3.SCM.APP.Utils.Utils;
using HS.K3.Common.Abbott;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Orm.DataEntity;

namespace Hands.K3.SCM.App.Core.SynchroService.ToHC
{

    [Description("同步物流轨迹至HC网站")]
    public class SynDeliveryNoticeBillToHC : AbstractSynchroToHC
    {
        public override SynchroDataType DataType
        {
            get
            {
                return SynchroDataType.DeliveryNoticeBill;
            }
        }

        private string GetSQL()
        {
           
            return string.Format(@"/*dialect*/ select d.FID,d.FEntryID,d.F_HS_CARRYBILLNO,a.FBillNo,d.F_HS_DELIDATE,a.F_HS_SaleOrder
			                                ,k.FNUMBER as F_HS_Channel,e.F_HS_Signtime,e.F_HS_TrackInfo,e.F_HS_AreaCode,e.F_HS_AreaName
                                            ,e.F_HS_TarckStatus,d.F_HS_LatestTrajectory,d.F_HS_YNCompleteTrajectory
                                            from T_SAL_DELIVERYNOTICE a
                                            inner join T_SAL_DELIVERYNOTICEFIN c on a.FID = c.FID
                                            inner join HS_T_LogisTrack d on d.FID = a.FID
                                            inner join HS_SAL_NOTICELocus e on e.FEntryID = d.FEntryID
			                                inner join T_HS_ShipMethods f on f.FID = d.F_HS_SHIPMETHODS
			                                inner join T_BAS_ASSISTANTDATAENTRY_L j ON f.F_HS_Channel=j.FENTRYID
                                            inner join T_BAS_ASSISTANTDATAENTRY k ON j.FentryID=k.FentryID
			                                where d.F_HS_YNCompleteTrajectory = 0
										    and d.F_HS_IsLogistics=0
										    and d.F_HS_CARRYBILLNO<>''
										    and a.FDOCUMENTSTATUS='C' and F_HS_DELIDATE>'2019-01-01' 
										    order by a.FBillNo,d.FEntryID, e.F_HS_Signtime asc
                                        ");
        }

        public override IEnumerable<AbsSynchroDataInfo> GetK3Datas(IEnumerable<string> billNos = null,bool flag = true)
        {
           
            List<LogisTrackEntry> trackEntries = null;
            LogisTrackEntry trackEntry = null;
            List<LogisTrajectoryEntry> trajectoryEntries = null;
            LogisTrajectoryEntry logisTrajectoryEntry = null;

            string sql = GetSQL();
            DynamicObjectCollection coll = SQLUtils.GetObjects(this.K3CloudContext, sql);

            var group = coll.GroupBy(c => c["FEntryID"]);

            if (group != null && group.Count() > 0)
            {
                trackEntries = new List<LogisTrackEntry>();

                foreach (var gro in group)
                {
                    if (gro != null)
                    {
                        trackEntry = new LogisTrackEntry();
                        trajectoryEntries = new List<LogisTrajectoryEntry>();

                        trackEntry.FBillNo = SQLUtils.GetFieldValue(gro.ElementAt(0), "FBillNo");
                        trackEntry.FEntryID = SQLUtils.GetFieldValue(gro.ElementAt(0), "FEntryID");
                        trackEntry.SrcNo = trackEntry.FBillNo + "_" + trackEntry.FEntryID;
                        trackEntry.F_HS_CARRYBILLNO = SQLUtils.GetFieldValue(gro.ElementAt(0), "F_HS_CARRYBILLNO");
                        trackEntry.F_HS_SaleOrder = SQLUtils.GetFieldValue(gro.ElementAt(0), "F_HS_SaleOrder");
                        trackEntry.F_HS_LatestTrajectory = SQLUtils.GetFieldValue(gro.ElementAt(0), "F_HS_LatestTrajectory").Trim();
                        trackEntry.F_HS_YNCompleteTrajectory = SQLUtils.GetFieldValue(gro.ElementAt(0), "F_HS_YNCompleteTrajectory").Equals("1")?true:false;

                        foreach (var g in gro)
                        {
                            if (g != null)
                            {
                                logisTrajectoryEntry = new LogisTrajectoryEntry();
                                logisTrajectoryEntry.FEntryID = SQLUtils.GetFieldValue(g, "FEntryID");
                                logisTrajectoryEntry.F_HS_AreaCode = SQLUtils.GetFieldValue(g, "F_HS_AreaCode");
                                logisTrajectoryEntry.F_HS_AreaName = SQLUtils.GetFieldValue(g, "F_HS_AreaName");
                                logisTrajectoryEntry.F_HS_Signtime = SQLUtils.GetFieldValue(g, "F_HS_Signtime");
                                logisTrajectoryEntry.F_HS_TarckStatus = SQLUtils.GetFieldValue(g, "F_HS_TarckStatus");
                                logisTrajectoryEntry.F_HS_TrackInfo = SQLUtils.GetFieldValue(g, "F_HS_TrackInfo");

                                trajectoryEntries.Add(logisTrajectoryEntry);
                            }
                            
                        }

                        trackEntry.TrajectoryEntry = trajectoryEntries;
                        trackEntries.Add(trackEntry);
                    }
                }
            }
            if (trajectoryEntries != null && trajectoryEntries.Count > 0)
            {
                return trackEntries.Where(t => !t.F_HS_YNCompleteTrajectory);
            }
            return null;
        }

        public override void UpdateAfterSynchro(IEnumerable<AbsSynchroDataInfo> datas, bool flag)
        {
            if (datas != null && datas.Count() > 0)
            {
                List<LogisTrackEntry> trackEntries = datas.Select(d =>(LogisTrackEntry)d).ToList();

                trackEntries = trackEntries.Where(t => t.F_HS_LatestTrajectory.Equals("签收")
                                               || t.F_HS_LatestTrajectory.Equals("退签")
                                               || t.F_HS_LatestTrajectory.Equals("退回")
                                               || t.F_HS_LatestTrajectory.Equals("销毁")
                                               ).ToList();

                if (trackEntries != null && trackEntries.Count > 0)
                {
                    List<string> billNos = trackEntries.Select(t => t.FBillNo).ToList();
                    List<string> entryIds = trackEntries.Select(t => t.FEntryID).ToList();

                    if (entryIds != null && entryIds.Count > 0)
                    {
                        string sql = string.Format(@"/*dialect*/update a set  a.F_HS_YNCompleteTrajectory = 1
                                                                from HS_T_LogisTrack a
                                                                inner join T_SAL_DELIVERYNOTICE b on b.FID = a.FID
			                                                    where a.F_HS_YNCompleteTrajectory = 0
			                                                    and (a.F_HS_LatestTrajectory ='签收'or a.F_HS_LatestTrajectory ='退签'
                                                                or a.F_HS_LatestTrajectory ='退回'or a.F_HS_LatestTrajectory ='销毁' )
			                                                    and a.FEntryID in({0})
                                                                and b.FBILLNO in('{1}')", string.Join(",", entryIds),string.Join("','", billNos));

                        try
                        {
                            int count = DBUtils.Execute(this.K3CloudContext, sql);
                        }
                        catch (Exception ex)
                        {
                            LogUtils.WriteSynchroLog(this.K3CloudContext,this.DataType, "更新发货通知单【是否已完成轨迹同步】字段出现异常："+ex.Message+Environment.NewLine+ex.StackTrace);
                        }   
                    }
                   
                }
            }      
        }
    }
}
