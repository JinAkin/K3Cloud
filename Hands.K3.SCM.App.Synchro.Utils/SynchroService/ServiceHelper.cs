using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS.Util;
using Kingdee.BOS;
using System.Threading;

using Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject;
using Hands.K3.SCM.APP.Utils;
using Hands.K3.SCM.APP.Entity.K3WebApi;
using Hands.K3.SCM.APP.Entity.StructType;
using Hands.K3.SCM.APP.Utils.Utils;
using HS.K3.Common.Abbott;
using Hands.K3.SCM.APP.Entity.EnumType;

namespace Hands.K3.SCM.App.Synchro.Utils.SynchroService
{
    /// <summary>
    /// 与系统交互的工具类
    /// </summary>
    public static partial class ServiceHelper
    {
        private const int ORGID = 100035;//使用组织

        /// <summary>
        /// 从Redis获取同步数据
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="dataType"></param>
        /// <param name="dbId"></param>
        /// <param name="numbers"></param>
        /// <returns></returns>
        public static List<AbsSynchroDataInfo> GetSynchroDatas(Context ctx, SynchroDataType dataType, long dbId, IEnumerable<string> numbers = null,SynchroDirection direction = SynchroDirection.ToK3)
        {
            List<AbsSynchroDataInfo> datas = null;
            RedisManager manager = new RedisManager(ctx);
            IRedisClient redis = manager.GetClientEx(ctx, dbId);

            IEnumerable<string> keys = null;
            HashSet<string> infos = null;
            Dictionary<string, string> dict = null;

            if (redis != null)
            {
                dict = RedisKeyUtils.GetRedisSetKey(dataType, SynchroDirection.ToK3);

                if (numbers != null && numbers.Count() > 0)
                {
                    keys = numbers;
                }
                else
                {
                    if (dict != null && dict.Count > 0)
                    {
                        if (!string.IsNullOrEmpty(dict["unreadKey"]))
                        {
                            keys = manager.GetAllItemsFromSet(dict["unreadKey"], ctx, dbId);
                        }
                    }
                }

                if (keys != default(HashSet<string>) && keys.Count() > 0)
                {
                    infos = new HashSet<string>();

                    if (!string.IsNullOrEmpty(dict["infoKey"]))
                    {
                        foreach (var item in keys)
                        {
                            if (!string.IsNullOrWhiteSpace(item))
                            {
                                string info = dict["infoKey"] + item;

                                if (!string.IsNullOrEmpty(info))
                                {
                                    infos.Add(info.Trim());
                                }
                            }
                        }
                    }
                }

                datas = GetSynchroObjects(ctx, dataType, redis, infos,direction);

                redis.Dispose();
                redis = null;
            }

            return datas;
        }

        /// <summary>
        /// 从Redis获取同步数据
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="dataType"></param>
        /// <param name="redis"></param>
        /// <param name="redisKeys"></param>
        /// <returns></returns>
        private static List<AbsSynchroDataInfo> GetSynchroObjects(Context ctx, SynchroDataType dataType, IRedisClient redis, IEnumerable<string> redisKeys,SynchroDirection direction = SynchroDirection.ToK3)
        {
            List<AbsSynchroDataInfo> datas = null;
            List<string> jsons = null;

            Dictionary<string, string> dict = null;

            if (redis != null)
            {
                if (redisKeys != null && redisKeys.Count() > 0)
                {
                    dict = new Dictionary<string, string>();
                    jsons = new List<string>();

                    foreach (var item in redisKeys)
                    {
                        if (!string.IsNullOrWhiteSpace(item))
                        {
                            string json = redis.Get<string>(item.ToString());

                            if (!string.IsNullOrWhiteSpace(json))
                            {
                                BackupJson.WriteJsonToLocal(ctx, dataType, json);
                                jsons.Add(json);
                                dict.Add(item.ToString(), JsonUtils.ReplaceDoubleQuotes(json));
                            }
                        }
                    }

                    BackupDataToRedis(ctx, dict, redisKeys, dataType, direction);
                }
            }

            if (jsons != null && jsons.Count > 0)
            {
                datas = new List<AbsSynchroDataInfo>();

                foreach (var json in jsons)
                {
                    if (json.IsNullOrEmptyOrWhiteSpace() || json.EqualsIgnoreCase("None"))
                    {
                        continue;
                    }
                    try
                    {
                        AbsSynchroDataInfo data = SynchroDataHelper.BuildSynchroData(ctx, dataType, json);

                        if (data != null)
                        {
                            datas.Add(data);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogUtils.WriteSynchroLog(ctx, dataType,
                                       "下载" + dataType + "出现异常" +
                                       ex.Message + System.Environment.NewLine + ex.StackTrace);
                    }
                }
            }

            return datas;
        }

        /// <summary>
        ///  k3 订单同步成功后，把对应key加入到已成功列表
        /// </summary> 
        /// <returns></returns>
        public static void UpdateOrderEnd(Context ctx, List<string> keys)
        {
            //try
            //{
            //    IRedisClient redis = RedisManager.GetClientEx(ctx, 2);
            //    redis.AddRangeToSet(Order_Done_Set_Key, keys);

            //    redis.Dispose();
            //    redis = null;

            //}
            //catch (Exception ex)
            //{
            //}
        }

        /// <summary>
        /// 设置客户等级
        /// </summary>
        /// <param name="custLevel"></param>
        /// <returns></returns>
        public static string SetCustomerLevel(string custLevel)
        {
            if (!string.IsNullOrEmpty(custLevel))
            {
                switch (custLevel)
                {
                    case "1":
                        return "Level1";
                    case "2":
                        return "Level2";
                    case "3":
                        return "Level3";
                    case "4":
                        return "Level4";
                    case "5":
                        return "Level5";
                    case "6":
                        return "Level6";
                    case "7":
                        return "Level7";
                    case "8":
                        return "Level8";
                    case "0":
                        return "Level0";
                }

            }
            return null;
        }

        /// <summary>
        /// 获取单据编码列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        public static List<string> GetItems<T>(T t)
        {
            List<string> items = null;

            if (t != null)
            {
                Type ty = t.GetType();
                Type ty2 = typeof(IEnumerable<string>);
                bool flag = t.GetType().IsSubclassOf(typeof(IEnumerable<string>));

                if (t.GetType() == typeof(IEnumerable<string>) || t.GetType().IsSubclassOf(typeof(IEnumerable<string>)))
                {
                    IEnumerable<string> keys = t as IEnumerable<string>;
                    if (keys != null && keys.Count() > 0)
                    {
                        items = new List<string>();

                        foreach (var key in keys)
                        {
                            if (key != null)
                            {
                                string[] infos = key.Split(':');
                                if (!string.IsNullOrWhiteSpace(infos[1]))
                                {
                                    items.Add(infos[1].Trim());
                                }
                            }
                        }
                    }
                }
                else if (t.GetType() == typeof(string))
                {
                    string str = t as string;

                    if (!string.IsNullOrWhiteSpace(str))
                    {
                        items = new List<string>();

                        if (str.Contains(","))
                        {
                            string[] strs = str.Split(',');

                            foreach (var item in strs)
                            {
                                if (!string.IsNullOrWhiteSpace(item))
                                {
                                    items.Add(item.Trim());
                                }
                            }
                        }
                        else
                        {
                            items.Add(str);
                        }
                    }
                }
            }

            return items;
        }

        /// <summary>
        /// 复制Redis正式库数据至测试库
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="dict"></param>
        /// <param name="keys"></param>
        /// <param name="dataType"></param>
        public static void BackupDataToRedis(Context ctx, Dictionary<string, string> dict, IEnumerable<string> keys, SynchroDataType dataType,SynchroDirection direction)
        {
            long backupRedisId = 0;
            
            try
            {
                Dictionary<string, string> redisKey = RedisKeyUtils.GetRedisSetKey(dataType, SynchroDirection.ToK3);

                switch (direction)
                {
                    case SynchroDirection.ToK3:
                        backupRedisId = 1;
                        break;
                    case SynchroDirection.ToB2B:
                        backupRedisId = 2;
                        break;
                    default:
                        break;
                }

                if (DataBaseConst.CurrentRedisServerIp.CompareTo(DataBaseConst.HKRedisIP) == 0)
                {
                    RedisManager manager = new RedisManager(ctx);
                    Thread subTh = new Thread(new ThreadStart(() =>
                    {
                        using (IRedisClient redis = manager.GetClientTest(ctx, backupRedisId))
                        {
                            if (redis != null)
                            {
                                if (dict != null && dict.Count > 0)
                                {
                                    redis.SetAll(dict);

                                    if (keys != null && keys.Count() > 0)
                                    {
                                        if (!string.IsNullOrWhiteSpace(redisKey["unreadKey"]) && !string.IsNullOrWhiteSpace(redis["allKey"]))
                                        {
                                            if (GetItems(keys) != null && GetItems(keys).Count > 0)
                                            {
                                                redis.AddRangeToSet(redisKey["unreadKey"], GetItems(keys));
                                                redis.AddRangeToSet(redis["allKey"], GetItems(keys));
                                            }

                                        }
                                    }

                                }

                                redis.Dispose();
                            }

                        }

                    }

                    ));
                    subTh.IsBackground = true;
                    subTh.Start();
                }
            }
            catch (Exception ex)
            {
                LogUtils.WriteSynchroLog(ctx, SynchroDataType.Redis, ex.Message + System.Environment.NewLine + ex.StackTrace);
            }

        }

    }
}
