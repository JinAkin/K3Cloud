using System;
using System.Collections.Generic;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.App.Data;

using Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject;
using Hands.K3.SCM.APP.Entity.StructType;
using Hands.K3.SCM.APP.Entity.SynDataObject.DeliveryNotice;
using Hands.K3.SCM.APP.Entity.CommonObject;
using Hands.K3.SCM.APP.Entity.K3WebApi;
using Kingdee.BOS;
using System.Linq;
using Hands.K3.SCM.App.Synchro.Base.Abstract;
using HS.K3.Common.Abbott;
using Hands.K3.SCM.APP.Entity.EnumType;
using Newtonsoft.Json.Linq;

namespace Hands.K3.SCM.App.Core.SynchroService.ToHC
{
    public class SynDeliveryNotice : AbstractSynchroToK3
    {
        public override SynchroDataType DataType
        {
            get
            {
                return SynchroDataType.DeliveryNoticeBill;
            }
        }

        public override string FormKey
        {
            get
            {
                return HSFormIdConst.SAL_DELIVERYNOTICE;
            }
        }

        public override FormMetadata Metadata
        {
            get
            {
                return MetaDataServiceHelper.Load(this.K3CloudContext, this.FormKey) as FormMetadata;
            }
        }

        public override JObject BuildSynchroDataJson(AbsSynchroDataInfo sourceData, SynchroLog log, SynOperationType operationType)
        {
            return null;
        }

        /// <summary>
        /// 将同步的数据转换为JSON格式
        /// </summary>
        /// <param name="sourceDatas"></param>
        /// <param name="operationType"></param>
        /// <returns></returns>
        public override JObject BuildSynchroDataJsons(IEnumerable<AbsSynchroDataInfo> sourceDatas, SynOperationType operationType)
        {
            JObject root = default(JObject);
            JArray model = default(JArray);
            JObject baseData = null;

            List<DeliveryNotice> notices = ConvertAbsSynchroObject(sourceDatas);
            int batchCount = BatchCount(sourceDatas);

            root.Add("NeedUpDateFields", new JArray(""));
            root.Add("NeedReturnFields", new JArray("FBillNo"));
            root.Add("IsDeleteEntry", "false");
            root.Add("SubSystemId", "");
            root.Add("IsVerifyBaseDataField", "true");
            root.Add("BatchCount", batchCount);

            if (notices != null && notices.Count > 0)
            {
                root = new JObject();
                model = new JArray();

                foreach (var item in notices)
                {
                    if (item != null)
                    {
                        baseData = ConvertSynDataToJObj(item,SynOperationType.SAVE);
                        model.Add(baseData);
                    }
                }

                root.Add("Model",model);

            }
            return root;
        }

        /// <summary>
        /// 同步数据
        /// </summary>
        /// <param name="sourceDatas"></param>
        /// <param name="logs"></param>
        /// <param name="operationType"></param>
        /// <returns></returns>
        public override HttpResponseResult ExecuteSynchro(IEnumerable<AbsSynchroDataInfo> sourceDatas, List<SynchroLog> logs, SynOperationType operationType)
        {
            HttpResponseResult result = null;
            JObject bizData = BuildSynchroDataJsons(sourceDatas, operationType);

            List<DeliveryNotice> notices = ConvertAbsSynchroObject(sourceDatas);
            KDTransactionScope trans = null;

            try
            {
                if (operationType == SynOperationType.SAVE)
                {
                    using (trans = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
                    {
                        //单据保存
                        //result = InvokeWebApi.InvokeBatchSave(this.K3CloudContext, this.DataType, this.FormKey, bizData.ToString());
                        //提交事务
                        trans.Complete();
                    }
                    return result;
                }
                else if (operationType == SynOperationType.UPDATE)
                {
                    result = new HttpResponseResult();
                    result.Message = "订单已经存在！";
                }

            }
            catch (Exception ex)
            {
                LogUtils.WriteSynchroLog(this.K3CloudContext, SynchroDataType.DeliveryNoticeBill, "数据批量更新过程中出现异常，异常信息：" + ex.Message + System.Environment.NewLine + ex.StackTrace);
                result = new HttpResponseResult();
                result.Success = false;
                result.Message = ex.Message;

                if (logs != null && logs.Count > 0)
                {
                    foreach (var log in logs)
                    {
                        log.IsSuccess = 0;
                        log.ErrInfor = ex.Message + System.Environment.NewLine + ex.StackTrace;
                    }
                }
            }
            finally
            {
                if (trans != null)
                {
                    trans.Dispose();
                }
            }

            if (result == null)
            {
                return null;
            }

            if (result.Success == false && result.FailedResult == null && result.Result == null)
            {
                return result;
            }
            return result;
        }

        /// <summary>
        /// 将抽象父类转换为子类
        /// </summary>
        /// <param name="sourceDatas"></param>
        /// <returns></returns>
        public List<DeliveryNotice> ConvertAbsSynchroObject(IEnumerable<AbsSynchroDataInfo> sourceDatas)
        {
            if (sourceDatas != null)
            {
                if (sourceDatas.Count() > 0)
                {
                    List<DeliveryNotice> notices = new List<DeliveryNotice>();

                    foreach (var item in sourceDatas)
                    {
                        if (item != null)
                        {
                            DeliveryNotice notice = item as DeliveryNotice;
                            notices.Add(notice);
                        }
                    }

                    return notices;
                }
            }
            return null;
        }

        /// <summary>
        /// 将同步的数据转换为JSON格式
        /// </summary>
        /// <param name="sourceData"></param>
        /// <param name="operationType"></param>
        /// <returns></returns>
        private JObject ConvertSynDataToJObj(AbsSynchroDataInfo sourceData, SynOperationType operationType)
        {
            DeliveryNotice notice = sourceData as DeliveryNotice;
            JObject baseData = default(JObject);

            if (notice != null)
            {
                baseData = new JObject();

                JObject FBillTypeID = new JObject();//单据类型
                FBillTypeID.Add("FNumber","");
                baseData.Add("FBillTypeID", FBillTypeID);

                baseData.Add("FBillNo",notice.FBillNo);//单据编号
                baseData.Add("FDate",notice.FDate);

                JObject FSaleOrgId = new JObject();//销售组织
                baseData.Add("FNumber",notice.FSaleOrgId);
                FSaleOrgId.Add("FSaleOrgId", FSaleOrgId);

                JObject FDeliveryOrgID = new JObject();//发货组织
                FDeliveryOrgID.Add("FNumber","");
                baseData.Add("FDeliveryOrgID", FDeliveryOrgID);

                baseData.Add("FEntity",BuildDeliveryNoitceEntryJson(notice));//明细信息
                baseData.Add("SubHeadEntity", BuildDeliveryNoticeFinJson(notice));//财务信息

                baseData.Add("F_HS_LocusEntity", DeliveryNoticeLocusEntryJson(notice));//轨迹明细信息
                baseData.Add("FDeliNoticeTrace", BuildDeliveryNoticeTraceEntryJson(notice));//物流跟踪明细信息

            }
            return baseData;
        }

        /// <summary>
        /// 发货通知单明细信息
        /// </summary>
        /// <param name="notice"></param>
        /// <returns></returns>
        private JArray BuildDeliveryNoitceEntryJson(DeliveryNotice notice)
        {
            JArray FEntity = default(JArray);
            JObject data = default(JObject);

            if (notice.Entry != null && notice.Entry.Count > 0)
            {
                FEntity = new JArray();

                foreach (var item in notice.Entry)
                {
                    data = new JObject();

                    JObject FMaterialID = new JObject();//物料编码
                    FMaterialID.Add("FNumber",item.FMaterialID);
                    data.Add("FMaterialID", FMaterialID);

                    JObject FUnitID = new JObject();//计价单位
                    FUnitID.Add("FNumber", item.FUnitID);
                    data.Add("FUnitID", FUnitID);

                    JObject FBaseUnitID = new JObject();//基本单位
                    FBaseUnitID.Add("FNumber", item.FBaseUnitID);
                    data.Add("FBaseUnitID", FBaseUnitID);

                    JObject FOutLmtUnit = new JObject();//超发控制单位类型
                    FOutLmtUnit.Add("FNumber", item.FOutLmtUnit);
                    data.Add("FOutLmtUnit", FOutLmtUnit);

                    FEntity.Add(data);
                }
            }

            return FEntity;
        }

        /// <summary>
        /// 发货通知单财务信息
        /// </summary>
        /// <param name="notice"></param>
        /// <returns></returns>
        private JObject BuildDeliveryNoticeFinJson(DeliveryNotice notice)
        {
            JObject SubHeadEntity = default(JObject);
            JObject data = default(JObject);

            if (notice != null && notice.DelNotFin != null)
            {
                SubHeadEntity = new JObject();
                data = new JObject();

                JObject FSettleOrgID = new JObject();//结算组织
                FSettleOrgID.Add("FNumber","");
                data.Add("FSettleOrgID", FSettleOrgID);

                JObject FSettleTypeID = new JObject();//结算方式
                FSettleTypeID.Add("FNumber", "");
                data.Add("FSettleTypeID", FSettleTypeID);

                SubHeadEntity.Add("SubHeadEntity", data);

            }

            return SubHeadEntity;
        }

        /// <summary>
        /// 发货通知单轨迹明细信息
        /// </summary>
        /// <param name="notice"></param>
        /// <returns></returns>
        private JArray DeliveryNoticeLocusEntryJson(DeliveryNotice notice)
        {
            JArray F_HS_LocusEntity = default(JArray);
            JObject data = default(JObject);

            if (notice != null && notice.LocusEntry != null && notice.LocusEntry.Count > 0)
            {
                F_HS_LocusEntity = new JArray();

                foreach (var item in notice.LocusEntry)
                {
                    data = new JObject();

                    data.Add("F_HS_Signtime",item.F_HS_Signtime);//签收时间
                    data.Add("F_HS_TrackInfo",item.F_HS_TrackInfo);//轨迹信息

                    data.Add("F_HS_AreaCode",item.F_HS_AreaCode);//地区编码
                    data.Add("F_HS_AreaName",item.F_HS_AreaName);//所在地
                    data.Add("F_HS_TarckStatus",item.F_HS_TarckStatus);//轨迹状态

                    F_HS_LocusEntity.Add(data);
                }
            }

            return F_HS_LocusEntity;
        }

        /// <summary>
        /// 发货通知单物流跟踪明细信息
        /// </summary>
        /// <param name="notice"></param>
        /// <returns></returns>
        private JArray BuildDeliveryNoticeTraceEntryJson(DeliveryNotice notice)
        {
            JArray FDeliNoticeTrace = default(JArray);
            JObject data = default(JObject);

            if (notice != null && notice.TraceEntry != null && notice.TraceEntry.Count > 0)
            {
                FDeliNoticeTrace = new JArray();

                foreach (var item in notice.TraceEntry)
                {
                    data = new JObject();

                    JObject FLogComId = new JObject();//物流公司
                    FLogComId.Add("FCode",item.FLogComId);
                    data.Add("FLogComId", FLogComId);

                    JObject F_HS_ShipMethods = new JObject();//物流方式
                    F_HS_ShipMethods.Add("FNumber",item.F_HS_ShipMethods);
                    data.Add("F_HS_ShipMethods", F_HS_ShipMethods);

                    data.Add("FCarryBillNo",item.F_HS_CarryBillNO);//物流单号
                    data.Add("FTraceStatus",item.FTraceStatus);

                    data.Add("F_HS_DeliDate",item.F_HS_DeliDate);//发货日期
                    data.Add("F_HS_QueryURL",item.F_HS_QueryURL);//查询网址
                    data.Add("F_HS_Channel",item.F_HS_Channel);//物流渠道

                    FDeliNoticeTrace.Add(data);
                }
            }

            return FDeliNoticeTrace;
        }

        public override AbsSynchroDataInfo BuildSynchroData(Context ctx, string json, AbsSynchroDataInfo data = null)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<AbsSynchroDataInfo> GetSynchroData(IEnumerable<string> numbers = null)
        {
            return null;
        }
    }
}
