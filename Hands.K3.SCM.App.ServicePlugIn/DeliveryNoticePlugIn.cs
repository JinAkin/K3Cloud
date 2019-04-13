using Hands.K3.SCM.APP.Entity.SynDataObject.DeliveryNotice;
using Hands.K3.SCM.APP.Utils;
using Kingdee.BOS;
using Kingdee.BOS.App;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Orm.DataEntity;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject;
using System;

using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using System.Threading.Tasks;
using Hands.K3.SCM.APP.Utils.Utils;
using HS.K3.Common.Abbott;
using Hands.K3.SCM.APP.Entity.K3WebApi;

namespace Hands.K3.SCM.App.ServicePlugIn
{
    [Description("发货通知单--获取物流信息插件")]
    public class DeliveryNoticePlugIn : AbstractOSPlugIn
    {
        static object objLock = new object();

        public override SynchroDataType DataType
        {
            get
            {
                return SynchroDataType.DeliveryNoticeBill;
            }
        }

        public override void OnPreparePropertys(Kingdee.BOS.Core.DynamicForm.PlugIn.Args.PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            e.FieldKeys.Add("F_HS_DeliveryAddress");
            e.FieldKeys.Add("F_HS_DeliveryCity");
            e.FieldKeys.Add("F_HS_DeliveryProvinces");
            e.FieldKeys.Add("F_HS_RecipientCountry");
            e.FieldKeys.Add("F_HS_PostCode");
        }
        public override void EndOperationTransaction(Kingdee.BOS.Core.DynamicForm.PlugIn.Args.EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);


            if (e.DataEntitys == null || e.DataEntitys.Count() < 0)
            {
                return;
            }

            List<DynamicObject> dataEntitys = e.DataEntitys.ToList();
            List<DeliveryNotice> notices = GetDeliveryNotices(dataEntitys);
        }

        public override void AfterExecuteOperationTransaction(AfterExecuteOperationTransaction e)
        {
            //base.AfterExecuteOperationTransaction(e);

            //var task = Task.Factory.StartNew(() => {
            //                                            if (e.DataEntitys == null || e.DataEntitys.Count() < 0)
            //                                            {
            //                                                return;
            //                                            }

            //                                            List<DynamicObject> dataEntitys = e.DataEntitys.ToList();
            //                                            List<DeliveryNotice> notices = GetDeliveryNotices(dataEntitys);
            //                                        }
            //                                );
        }

        private List<DeliveryNotice> GetDeliveryNotices(List<DynamicObject> dynObjects)
        {
            DeliveryNotice notice = null;
            List<DeliveryNotice> notices = null;

            if (dynObjects == null || dynObjects.Count() == 0)
            {
                return null;
            }

            notices = new List<DeliveryNotice>();

            foreach (var item in dynObjects)
            {
                if (item != null)
                {
                    notice = new DeliveryNotice();

                    notice.FBillNo = SQLUtils.GetFieldValue(item,"FBillNo");
                    notice.DeliveryStreetAddress = SQLUtils.GetFieldValue(item, "F_HS_DeliveryAddress");
                    notice.F_HS_DeliveryCity = SQLUtils.GetFieldValue(item, "F_HS_DeliveryCity");
                    notice.F_HS_DeliveryProvinces = SQLUtils.GetFieldValue(item, "F_HS_DeliveryProvinces");
                    notice.F_HS_RecipientCountry = SQLUtils.GetCountryNo(this.Context, item, "F_HS_RecipientCountry_Id");
                    notice.F_HS_PostCode = SQLUtils.GetFieldValue(item, "F_HS_PostCode");

                    notices.Add(notice);

                }
            }

            return notices;
        }
        private int SaveLogisticsInfo(Context ctx, List<DeliveryNotice> notices)
        {
            int count = 0;
            lock (objLock)
            {
                if (notices != null && notices.Count > 0)
                {
                    foreach (var notice in notices)
                    {
                        if (notice != null)
                        {
                            if (notice.TraceEntry != null && notice.TraceEntry.Count > 0)
                            {
                                foreach (var trace in notice.TraceEntry)
                                {
                                    string sql = string.Format(@"if not exists(select F_HS_SHIPPINGMETHODS,F_HS_DELIDATE,F_HS_CARRYBILLNO,F_HS_QUERYURL,F_HS_Channel,F_HS_LASTTRACKTIME,F_HS_IsTrack from {0})
                                             insert into {1} (F_HS_SHIPPINGMETHODS,F_HS_DELIDATE,F_HS_CARRYBILLNO,F_HS_QUERYURL,F_HS_Channel,F_HS_LASTTRACKTIME,F_HS_IsTrack)
                                             
                                             select @F_HS_SHIPPINGMETHODS as F_HS_SHIPPINGMETHODS,@F_HS_DELIDATE as F_HS_DELIDATE,@F_HS_CARRYBILLNO as F_HS_CARRYBILLNO,
                                             @F_HS_QUERYURL as F_HS_QUERYURL,@F_HS_Channel as F_HS_Channel,@F_HS_LASTTRACKTIME as F_HS_LASTTRACKTIME,@F_HS_IsTrack as F_HS_IsTrack

                                           ");
                                    var para = new List<SqlParam>();
                                    para.Add(new SqlParam("@F_HS_SHIPPINGMETHODS", KDDbType.String,"CN_FEDEX"));
                                    para.Add(new SqlParam("@F_HS_DELIDATE", KDDbType.Date, trace.F_HS_DeliDate));
                                    para.Add(new SqlParam("@F_HS_CARRYBILLNO", KDDbType.String, trace.F_HS_CarryBillNO));
                                    para.Add(new SqlParam("@F_HS_QUERYURL", KDDbType.String,"https://www.fedex.com/hk/index/index.html"));
                                    para.Add(new SqlParam("@F_HS_Channel", KDDbType.String,"FedEx"));
                                    para.Add(new SqlParam("@F_HS_LASTTRACKTIME", KDDbType.DateTime,trace.F_HS_LastTrackTime));
                                    para.Add(new SqlParam("@F_HS_IsTrack", KDDbType.Single,trace.F_HS_IsTrack));

                                   count += ServiceHelper.GetService<IDBService>().Execute(ctx,sql,para);
                                }
                                
                            }
                            
                        }
                    }
                }
                
            }

            return count;
        }

        public override IEnumerable<AbsSynchroDataInfo> GetK3Datas(Context ctx,List<DynamicObject> objects,ref HttpResponseResult result)
        {
            throw new NotImplementedException();
        }
    }
}


