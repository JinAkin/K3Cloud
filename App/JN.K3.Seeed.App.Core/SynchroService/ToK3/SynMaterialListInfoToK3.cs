using System;
using System.Collections.Generic;
using System.Linq;
using Hands.K3.SCM.APP.Entity.CommonObject;

using Hands.K3.SCM.APP.Entity.K3WebApi;
using Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject;
using Kingdee.BOS;
using Kingdee.BOS.Core.Metadata;
using Newtonsoft.Json.Linq;
using Kingdee.BOS.App.Data;
using Hands.K3.SCM.APP.Entity.StructType;
using Kingdee.BOS.ServiceHelper;
using Hands.K3.SCM.App.Synchro.Base.Abstract;
using Hands.K3.SCM.App.Synchro.Utils.SynchroService;
using Hands.K3.SCM.APP.Entity.EnumType;
using HS.K3.Common.Abbott;
using Hands.K3.SCM.APP.Entity.SynDataObject.Material_;

namespace Hands.K3.SCM.App.Core.SynchroService.ToK3
{
    public class SynMaterialListInfoToK3 : AbstractSynchroToK3
    {
        public override SynchroDataType DataType
        {
            get
            {
                return SynchroDataType.DownLoadListInfo;
            }
        }

        public override string FormKey
        {
            get
            {
                return HSFormIdConst.Material;
            }
        }

        public override FormMetadata Metadata
        {
            get
            {
                return MetaDataServiceHelper.Load(this.K3CloudContext, this.FormKey) as FormMetadata;
            }
        }

        public override IEnumerable<AbsSynchroDataInfo> GetSynchroData(IEnumerable<string> numbers = null)
        {
            return ServiceHelper.GetSynchroDatas(this.K3CloudContext, this.DataType, RedisDbId, null,this.Direction);
        }
        public override Dictionary<SynOperationType, IEnumerable<AbsSynchroDataInfo>> EntityDataSource(Context ctx, IEnumerable<AbsSynchroDataInfo> srcDatas)
        {
            Dictionary<SynOperationType, IEnumerable<AbsSynchroDataInfo>> dict = new Dictionary<SynOperationType, IEnumerable<AbsSynchroDataInfo>>();

            if (srcDatas != null && srcDatas.Count() > 0)
            {
                dict.Add(SynOperationType.UPDATE, srcDatas);
            }
            return dict;
        }
        public override JObject BuildSynchroDataJson(AbsSynchroDataInfo sourceData, SynchroLog log, SynOperationType operationType)
        {
            return null;
        }
        public override JObject BuildSynchroDataJsons(IEnumerable<AbsSynchroDataInfo> sourceDatas, SynOperationType operationType)
        {
            return null;
        }

        public override AbsSynchroDataInfo BuildSynchroData(Context ctx, string json, AbsSynchroDataInfo data = null)
        {
            return null;
        }
        public override HttpResponseResult ExecuteSynchro(IEnumerable<AbsSynchroDataInfo> sourceDatas, List<SynchroLog> logs, SynOperationType operationType)
        {
            HttpResponseResult result = null;
            List<Material> materials = null/*sourceDatas.Select(m => (Material)m).ToList()*/;

            KDTransactionScope trans = null;
            List<string> numbers = null;
            try
            {
                if (operationType == SynOperationType.SAVE)
                {

                }
                else if (operationType == SynOperationType.UPDATE)
                {
                    using (trans = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
                    {
                        int count = 0;
                        string messages = "";

                        if (materials != null && materials.Count > 0)
                        {
                            numbers = new List<string>();

                            foreach (var data in materials)
                            {
                                //if (data != null)
                                //{
                                   
                                //    string sql = string.Format(@"/*dialect*/ update T_BD_MATERIAL 
                                //                                set F_HS_LISTID = '{0}' ,F_HS_LISTNAME= '{1}'
                                //                                where FNUMBER = '{2}'
                                //                                and  len(F_HS_LISTID) = 0", info.F_HS_ListID, info.F_HS_ListName, info.FNumber);
                                //    try
                                //    {
                                //        count += DBUtils.Execute(this.K3CloudContext, sql);

                                //        if (count > 0)
                                //        {
                                //            numbers.Add(info.FNumber);
                                //        }
                                //        else
                                //        {
                                //            string message = "物料【" + info.FNumber + "】LISTID【" + info.F_HS_ListID + "】已更新，不再更新！";
                                //            messages += message;
                                //            (this.K3CloudContext, SynchroDataType.DownLoadListInfo, message);
                                //        }
                                //    }
                                //    catch (Exception ex)
                                //    {
                                //        (this.K3CloudContext, SynchroDataType.DownLoadListInfo, "数据批量更新过程中出现异常，异常信息：" + ex.Message + System.Environment.NewLine + ex.StackTrace);
                                //        messages += ex.Message + System.Environment.NewLine + ex.StackTrace;
                                //    }
                                //}


                            }
                            if (numbers != null && numbers.Count > 0)
                            {
                                RemoveRedisData(this.K3CloudContext, numbers);
                            }

                            if (!string.IsNullOrWhiteSpace(messages))
                            {
                                result.Success = false;
                                result.Message = messages;
                            }
                            else
                            {
                                if (count == numbers.Count && count > 0)
                                {
                                    result.Success = true;
                                    result.Message = "物料ListId下载成功！";
                                }
                            }
                        }
                    }
                    return result;

                }
            }
            catch (Exception ex)
            {
                if (logs != null && logs.Count > 0)
                {
                    foreach (var log in logs)
                    {
                        log.IsSuccess = 0;
                        log.ErrInfor = ex.Message + System.Environment.NewLine + ex.StackTrace;
                    }
                }

            }

            if (result == null)
            {
                return null;
            }

            if (result.Success == false && result.FailedResult == null && result.Result == null)
            {
                //同步出现错误之类：如令牌错误，url错误之类的
                if (logs != null && logs.Count > 0)
                {
                    foreach (var log in logs)
                    {
                        log.IsSuccess = 0;
                        log.ErrInfor = "数据同步失败：" + result.Message == null ? "" : result.Message;
                    }
                }

                return result;
            }

            return result;
        }
    }
}
