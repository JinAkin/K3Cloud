
using Hands.K3.SCM.APP.Entity.EnumType;
using Hands.K3.SCM.APP.Entity.K3WebApi;
using Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject;
using Hands.K3.SCM.APP.Utils;
using Hands.K3.SCM.APP.Utils.Utils;
using HS.K3.Common.Abbott;
using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Orm.DataEntity;
using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hands.K3.SCM.App.Synchro.Base.Abstract
{
    public abstract class AbstractSynchroToHC : AbstractSynchro
    {
        private readonly static object oLock = new object();

        public override SynchroDataType DataType { get; }

        public override SynchroDirection Direction
        {
            get
            {
                return SynchroDirection.ToHC;
            }
        }

        public IRedisClient RedisClient
        {
            get
            {
                RedisManager redis = new RedisManager(this.K3CloudContext);
                return redis.GetClientEx(this.K3CloudContext, this.RedisDbId);
            }
        }
        /// <summary>
        /// 同步K3数据至HC网站（用于定时任务）
        /// </summary>
        /// <param name="k3Datas"></param>
        /// <param name="billNos"></param>
        /// <returns></returns>
        public virtual HttpResponseResult SynchroDataToHC(IEnumerable<AbsSynchroDataInfo> k3Datas = null, IEnumerable<string> billNos = null, bool SQLFilter = true, SynchroDirection direction = SynchroDirection.Default)
        {
            return SynK3DataToWebSite(k3Datas, billNos, SQLFilter, direction);
        }

        /// <summary>
        /// 同步K3数据至HC网站
        /// </summary>
        /// <param name="k3Datas"></param>
        /// <param name="billNos"></param>
        /// <returns></returns>
        public virtual HttpResponseResult SynK3DataToWebSite(IEnumerable<AbsSynchroDataInfo> k3Datas = null, IEnumerable<string> billNos = null, bool SQLFilter = true, SynchroDirection direction = SynchroDirection.Default)
        {
            HttpResponseResult result = null;
            Dictionary<string, string> dict = null;
            IEnumerable<AbsSynchroDataInfo> datas = null;
            List<string> SynBillNos = null;
            bool IsSuccess = false;
            IRedisTransaction trans = null;

            lock (oLock)
            {
                if (k3Datas == null)
                {
                    datas = GetK3Datas(billNos, SQLFilter);
                }
                else
                {
                    datas = k3Datas;
                }
                try
                {

                    Dictionary<string, string> redisKeys = RedisKeyUtils.GetRedisSetKey(this.DataType, this.Direction);
                    string allKey = redisKeys["allKey"];
                    string unreadKey = redisKeys["unreadKey"];

                    Dictionary<string, string> synFailDatas = GetSynchroFailureDatas();
                    List<string> failBillNos = null;
                    List<string> failInfoKeys = null;

                    trans = RedisClient.CreateTransaction();


                    if (synFailDatas != null && synFailDatas.Count > 0)
                    {
                        failBillNos = GetSychroFailureBillNos(synFailDatas);
                        failInfoKeys = GetSynchroFailureInfoKeys(synFailDatas);

                        trans.QueueCommand(r => r.AddRangeToSet(allKey, failBillNos));
                        trans.QueueCommand(r => r.AddRangeToSet(unreadKey, failBillNos));
                        trans.QueueCommand(r => r.SetAll(synFailDatas));

                        IsSuccess = trans.Commit();
                        trans.Dispose();

                        if (IsSuccess)
                        {
                            UpdateSynchroDataLog(failInfoKeys);
                        }

                    }
                    if (datas != null && datas.Count() > 0)
                    {
                        dict = new Dictionary<string, string>();
                        SynBillNos = new List<string>();

                        var group = from d in datas
                                    group d by d.SrcNo into g
                                    select g;

                        if (group != null && group.Count() > 0)
                        {
                            foreach (var g in group)
                            {
                                if (g != null && g.ToList().Count > 0)
                                {
                                    SynBillNos.Add(g.Key);
                                    string infoKey = RedisKeyUtils.GetRedisSetKey(this.DataType, this.Direction)["infoKey"] + g.Key;
                                    dict.Add(infoKey, JsonUtils.SerializeObject<IEnumerable<AbsSynchroDataInfo>>(this.K3CloudContext, g.ToList()));
                                }
                            }

                            if (IsConnectSuccess())
                            {
                                if (dict.Count > 0)
                                {
                                    trans = RedisClient.CreateTransaction();
                                    trans.QueueCommand(r => r.AddRangeToSet(allKey, SynBillNos));
                                    trans.QueueCommand(r => r.AddRangeToSet(unreadKey, SynBillNos));
                                    trans.QueueCommand(r => r.SetAll(dict));

                                    IsSuccess = trans.Commit();
                                    trans.Dispose();

                                    if (IsSuccess)
                                    {
                                        UpdateAfterSynchro(datas, true);
                                        LogHelper.WriteSynchroDataLog(this.K3CloudContext, this.DataType, RedisClient, dict, true);

                                        string msg = "【" + this.DataType + "】同步，单据编码" + FormatNumber(datas) + "信息成功同步到Redis";
                                        LogHelper.WriteSynchroLog_Succ(this.K3CloudContext, this.DataType, msg);

                                        result = new HttpResponseResult();
                                        result.Success = true;
                                        result.Message = msg;
                                    }
                                    else
                                    {

                                        LogHelper.WriteSynchroDataLog(this.K3CloudContext, this.DataType, RedisClient, dict, false);

                                        string msg = "【" + this.DataType + "】同步，单据编码" + FormatNumber(datas) + "信息同步到Redis失败";
                                        LogUtils.WriteSynchroLog(this.K3CloudContext, this.DataType, msg);

                                        result = new HttpResponseResult();
                                        result.Success = false;
                                        result.Message = msg;
                                    }
                                }
                            }
                            else
                            {
                                LogHelper.WriteSynchroDataLog(this.K3CloudContext, this.DataType, RedisClient, dict, false);
                            }
                        }
                    }
                    else
                    {
                        result = new HttpResponseResult();
                        result.Success = false;
                        result.Message = "没有需要同步的数据";

                        LogUtils.WriteSynchroLog(this.K3CloudContext, this.DataType, result.Message);
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.WriteSynchroDataLog(this.K3CloudContext, this.DataType, RedisClient, dict, false);
                    string msg = "【" + this.DataType + "】同步过程中出现异常，异常信息：" + ex.Message + System.Environment.NewLine + ex.StackTrace;
                    LogUtils.WriteSynchroLog(this.K3CloudContext, this.DataType, msg);

                    result = new HttpResponseResult();
                    result.Success = false;
                    result.Message = msg;
                    UpdateAfterSynchro(datas, false);
                }
                finally
                {
                    if (trans != null)
                    {
                        trans.Dispose();
                    }
                }
            }

            return result;
        }

        public virtual void UpdateAfterSynchro(IEnumerable<AbsSynchroDataInfo> datas, bool flag)
        {

        }

        /// <summary>
        /// 更新同步日志表中的success标记
        /// </summary>
        /// <param name="infoKeys"></param>
        /// <returns></returns>
        public virtual int UpdateSynchroDataLog(List<string> infoKeys)
        {
            int count = 0;

            if (infoKeys != null && infoKeys.Count > 0)
            {
                string sql = string.Format(@"/*dialect*/ update HS_T_synchroDataLog set success = '1',updateTime = '{3}'
                                                    where SynchroDataType = '{0}'
                                                    and redisKey in('{1}')
                                                    and redisDBID = {2}
                                                    and success = '0'", this.DataType, string.Join("','", infoKeys), this.RedisDbId, DateTime.Now);

                count = DBUtils.Execute(this.K3CloudContext, sql);

            }

            return count;
        }
        public virtual IEnumerable<AbsSynchroDataInfo> GetK3Datas(IEnumerable<string> billNos = null, bool flag = true)
        {
            return null;
        }

        /// <summary>
        /// 获取同步失败的数据
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, string> GetSynchroFailureDatas()
        {
            Dictionary<string, string> datas = null;

            string sql = string.Format(@"/*dialect*/ select redisKey,json from HS_T_synchroDataLog
                                            where SynchroDataType = '{0}'
                                            and success = '{1}'
                                            and redisDBID = '{2}'
                                            order by  createTime desc ", this.DataType, "0", this.RedisDbId);

            DynamicObjectCollection coll = SQLUtils.GetObjects(this.K3CloudContext, sql);

            if (coll != null && coll.Count > 0)
            {
                datas = new Dictionary<string, string>();

                foreach (var item in coll)
                {
                    if (item != null)
                    {
                        string infoKey = SQLUtils.GetFieldValue(item, "redisKey");
                        string json = SQLUtils.GetFieldValue(item, "json");

                        if (!datas.ContainsKey(infoKey))
                        {
                            datas.Add(infoKey, json);
                        }
                    }
                }
            }
            return datas;
        }

        /// <summary>
        /// 获取Redis key中编码的部分key值
        /// </summary>
        /// <param name="datas"></param>
        /// <returns></returns>
        private List<string> GetSychroFailureBillNos(Dictionary<string, string> datas)
        {
            List<string> billNos = null;

            if (datas != null && datas.Count > 0)
            {
                var result = datas.Select(d => d.Key.Split(':')[1]);

                if (result != null && result.Count() > 0)
                {
                    billNos = result.ToList();
                }
            }

            return billNos;
        }

        /// <summary>
        /// 获取Redis中的infoKeys
        /// </summary>
        /// <param name="datas"></param>
        /// <returns></returns>
        private List<string> GetSynchroFailureInfoKeys(Dictionary<string, string> datas)
        {
            List<string> infoKeys = null;

            if (datas != null && datas.Count > 0)
            {
                var result = datas.Select(d => d.Key);

                if (result != null && result.Count() > 0)
                {
                    infoKeys = result.ToList();
                }
            }
            return infoKeys;
        }
        public virtual string FormatNumber(IEnumerable<AbsSynchroDataInfo> datas)
        {
            string info = "";
            if (datas != null && datas.Count() > 0)
            {
                for (int i = 0; i < datas.Count(); i++)
                {
                    if (i < datas.Count() - 1)
                    {
                        info += "[" + datas.ElementAt(i).SrcNo + "],";
                    }
                    else if (i == datas.Count() - 1)
                    {
                        info += "[" + datas.ElementAt(i).SrcNo + "]";
                    }
                }
            }
            return info;
        }
        public virtual bool IsConnectSuccess()
        {
            if (this.RedisClient != null)
            {
                if (RedisClient.HadExceptions == false)
                {
                    return true;
                }
            }

            return false;
        }

    }
}
