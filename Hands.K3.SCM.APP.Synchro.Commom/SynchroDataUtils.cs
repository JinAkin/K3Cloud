using Hands.K3.SCM.APP.Entity.EnumType;
using Hands.K3.SCM.APP.Entity.K3WebApi;
using Hands.K3.SCM.APP.Entity.StructType;
using Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject;
using Hands.K3.SCM.APP.Utils;
using Hands.K3.SCM.APP.Utils.Utils;
using HS.K3.Common.Abbott;
using Kingdee.BOS;
using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.APP.Synchro.Commom
{
    public class SynchroDataUtils
    {
        private readonly static object oLock = new object();


        /// <summary>
        /// Redis数据库的ID
        /// </summary>
        public long RedisDbId { get; set; }

        /// <summary>
        /// Redis全部的Set类型的key
        /// </summary>
        public static string GetRedisAllKey(SynchroDataType DataType, SynchroDirection Direction)
        {
            return RedisKeyUtils.GetRedisSetKey(DataType, Direction)["allKey"];
        }

        /// <summary>
        /// Redis未读的Set类型的key
        /// </summary>taType, SynchroDirection 
        public static string RedisUnreadkey(SynchroDataType DataType, SynchroDirection Direction)
        {
            return RedisKeyUtils.GetRedisSetKey(DataType, Direction)["unreadKey"];
        }

        /// <summary>
        /// Redis string类型的具体数据
        /// </summary>
        public static string RedisInfoKey(SynchroDataType DataType, SynchroDirection Direction)
        {
            return RedisKeyUtils.GetRedisSetKey(DataType, Direction)["infoKey"];

        }
        public static HttpResponseResult SynK3DataToWebSite(Context ctx, SynchroDataType DataType, SynchroDirection Direction, long RedisDbId, IEnumerable<AbsSynchroDataInfo> k3Datas = null, IEnumerable<string> billNos = null, bool SQLFilter = true, bool IsSynchroB2B = false)
        {
            HttpResponseResult result = null;
            Dictionary<string, string> dict = null;
            IEnumerable<AbsSynchroDataInfo> datas = null;
            List<string> keys = null;
            bool IsSuccess = false;
            IRedisTransaction trans = null;

            lock (oLock)
            {
                RedisManager redis = new RedisManager(ctx);

                try
                {
                    if (datas != null && datas.Count() > 0)
                    {
                        dict = new Dictionary<string, string>();
                        keys = new List<string>();

                        var group = from d in datas
                                    group d by d.SrcNo into g
                                    select g;

                        if (group != null && group.Count() > 0)
                        {
                            foreach (var g in group)
                            {
                                if (g != null && g.ToList().Count > 0)
                                {
                                    keys.Add(g.Key);
                                    string infoKey = RedisUnreadkey(DataType, Direction) + g.Key;
                                    dict.Add(infoKey, JsonUtils.SerializeObject<IEnumerable<AbsSynchroDataInfo>>(ctx, g.ToList()));
                                }
                            }

                            if (dict.Count > 0)
                            {
                                IRedisClient client = redis.GetClientEx(ctx, RedisDbId);

                                if (IsConnectSuccess(client))
                                {
                                    using (trans = redis.GetClientEx(ctx, RedisDbId).CreateTransaction())
                                    {
                                        trans.QueueCommand(r => r.AddRangeToSet(GetRedisAllKey(DataType, Direction), keys));
                                        trans.QueueCommand(r => r.AddRangeToSet(RedisUnreadkey(DataType, Direction), keys));
                                        trans.QueueCommand(r => r.SetAll(dict));

                                        IsSuccess = trans.Commit();
                                    }
                                    if (IsSuccess)
                                    {
                                        

                                        foreach (var d in dict)
                                        {
                                            LogHelper.WriteSynchroDataLog(ctx, DataType, redis.GetClient(ctx, RedisDbId), d.Key, d.Value);
                                        }

                                        LogHelper.WriteSynchroLog_Succ(ctx,DataType, "【" + DataType + "】同步，单据编码" + FormatNumber(datas) + "信息成功同步到Redis");
                                        result = new HttpResponseResult();
                                        result.Success = true;
                                        result.Message = "【" + DataType + "】同步成功！";
                                    }
                                }
                            }
                        }

                    }
                    else
                    {
                        result = new HttpResponseResult();
                        result.Success = false;
                        result.Message = "没有需要同步的数据";

                        LogUtils.WriteSynchroLog(ctx, DataType, result.Message);
                    }
                }
                catch (Exception ex)
                {
                    LogUtils.WriteSynchroLog(ctx, DataType, "【" + DataType + "】同步过程中出现异常，异常信息：" + ex.Message + System.Environment.NewLine + ex.StackTrace);

                    result = new HttpResponseResult();
                    result.Success = false;
                    result.Message = "【" + DataType + "】出现异常，异常信息：" + System.Environment.NewLine + ex.Message;
                }
            }

            return result;
        }

        /// <summary>
        /// 删除Redis数据库中的数据
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="numbers"></param>
        /// <param name="dataType"></param>
        public  static void RemoveRedisData(Context ctx, IEnumerable<string> numbers, SynchroDirection direction,long RedisDbId,SynchroDataType dataType = SynchroDataType.None)
        {
            if (true/*K3LoginInfo.GetRedisServerIp(ctx).CompareTo(DataBaseConst.RedisServerIP) == 0*/)
            {
                List<string> infoKeys = null;
                RedisManager manager = new RedisManager(ctx);

                if (numbers != null && numbers.Count() > 0)
                {
                    infoKeys = new List<string>();

                    foreach (var num in numbers)
                    {
                        if (!string.IsNullOrWhiteSpace(num))
                        {
                            if (dataType.CompareTo(SynchroDataType.None) == 0)
                            {
                                infoKeys.Add(RedisInfoKey(dataType,direction) + num);
                                manager.RemoveItemFromSet(ctx, RedisUnreadkey(dataType,direction), num, RedisDbId);
                            }
                            else
                            {
                                infoKeys.Add(RedisInfoKey(dataType, direction) + num);
                                manager.RemoveItemFromSet(ctx, RedisUnreadkey(dataType, direction), num, RedisDbId);
                            }
                        }
                    }

                    if (DataBaseConst.CurrentRedisServerIp.CompareTo(DataBaseConst.HKRedisIP) == 0)
                    {
                        manager.RemoveAll(ctx, infoKeys, RedisDbId);
                    }

                    manager = null;
                }
            }
        }

        /// <summary>
        /// 测试Redis是否连接成功
        /// </summary>
        /// <param name="redis"></param>
        /// <returns></returns>
        private static bool IsConnectSuccess(IRedisClient redis)
        {
            if (redis != null)
            {
                if (redis.HadExceptions == false)
                {
                    return true;
                }
            }
            return false;
        }

        private static long GetRedisDbId(Context ctx)
        {

            if (ctx.DBId.CompareTo(DataBaseConst.K3CloudDbId) == 0)
            {
                return DataBaseConst.HKRedisDbId;
            }
            else
            {
                return DataBaseConst.ALRedisDbId;
            }
        }

        private static string FormatNumber(IEnumerable<AbsSynchroDataInfo> datas)
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
    }
}
