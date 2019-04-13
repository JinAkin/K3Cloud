
using Hands.K3.SCM.APP.Entity.K3WebApi;
using Hands.K3.SCM.APP.Entity.StructType;
using Hands.K3.SCM.APP.Utils.Utils;
using HS.K3.Common.Abbott;
using Kingdee.BOS;
using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hands.K3.SCM.APP.Utils.Utils
{

    /// <summary>
    /// RedisManager类主要是创建链接池管理对象的
    /// </summary>
    public class RedisManager
    {

        private PooledRedisClientManager _prcm;
        private object lockObj = new object();

        public Context K3CloudContext { get; set; }
        /// <summary>
        /// 创建链接池管理对象
        /// </summary>
        public RedisManager(Context ctx)
        {
            
            List<string> host = new List<string>();
            DataBaseConst.K3CloudContext = ctx;
            host.Add(string.Format(@"{0}@{1}:{2}", DataBaseConst.CurrentRedisServerPwd, DataBaseConst.CurrentRedisServerIp, DataBaseConst.RedisPort));

            //WriteServerList：可写的Redis链接地址。
            //ReadServerList：可读的Redis链接地址。
            //MaxWritePoolSize：最大写链接数。
            //MaxReadPoolSize：最大读链接数。
            //AutoStart：自动重启。
            //LocalCacheTime：本地缓存到期时间，单位:秒。
            //RecordeLog：是否记录日志,该设置仅用于排查redis运行时出现的问题,如redis工作正常,请关闭该项。
            //RedisConfigInfo类是记录redis连接信息，此信息和配置文件中的RedisConfig相呼应
            lock (lockObj)
            {
                if (_prcm == null)
                {
                    _prcm = new PooledRedisClientManager(host, host, new RedisClientManagerConfig
                    {
                        MaxWritePoolSize = 500, // “写”链接池链接数 
                        MaxReadPoolSize = 500, // “读”链接池链接数 
                        AutoStart = true,

                    });

                    _prcm.ConnectTimeout = 100000000;
                    _prcm.SocketReceiveTimeout = 1000000000;
                    _prcm.SocketSendTimeout = 1000000000;
                }
            }


        }

        private Dictionary<string, string> Init(Context ctx)
        {
            
            List<string> host = new List<string>();
            
            host.Add(string.Format(@"{0}@{1}:{2}", DataBaseConst.CurrentRedisServerPwd,DataBaseConst.CurrentRedisServerIp, DataBaseConst.RedisPort));

            //WriteServerList：可写的Redis链接地址。
            //ReadServerList：可读的Redis链接地址。
            //MaxWritePoolSize：最大写链接数。
            //MaxReadPoolSize：最大读链接数。
            //AutoStart：自动重启。
            //LocalCacheTime：本地缓存到期时间，单位:秒。
            //RecordeLog：是否记录日志,该设置仅用于排查redis运行时出现的问题,如redis工作正常,请关闭该项。
            //RedisConfigInfo类是记录redis连接信息，此信息和配置文件中的RedisConfig相呼应
            lock (lockObj)
            {
                if (_prcm == null)
                {
                    _prcm = new PooledRedisClientManager(host, host, new RedisClientManagerConfig
                    {
                        MaxWritePoolSize = 500, // “写”链接池链接数 
                        MaxReadPoolSize = 500, // “读”链接池链接数 
                        AutoStart = true,

                    });

                    _prcm.ConnectTimeout = 100000000;
                    _prcm.SocketReceiveTimeout = 1000000000;
                    _prcm.SocketSendTimeout = 1000000000;
                }
            }

            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add(DataBaseConst.CurrentRedisServerIp,DataBaseConst.CurrentRedisServerPwd);
            return dict;
        }
        ///// <summary>
        ///// 客户端缓存操作对象
        ///// </summary>
        //public  IRedisClient GetClient(Context ctx)
        //{ 
        //    return CreateRedisClient(ctx); ;
        //}

        /// <summary>
        /// 客户端缓存操作对象
        /// </summary>
        public IRedisClient GetClientEx(Context ctx, long dbId)
        {
            

            IRedisClient client = null;

            try
            {
                
                client = client = new RedisClient(DataBaseConst.CurrentRedisServerIp, DataBaseConst.RedisPort, DataBaseConst.CurrentRedisServerPwd);
                client.Db = dbId;
                client.ConnectTimeout = 600000;
                client.RetryTimeout = 600000;
                client.SendTimeout = 600000;
            }
            catch (Exception ex)
            {
                LogUtils.WriteSynchroLog(ctx, SynchroDataType.SaleOrder, "Redis:GetClientEx操作出现异常，异常信息：" + ex.Message + System.Environment.NewLine + ex.StackTrace);
            }

            return client;
        }

        /// <summary>
        /// 测试客户端缓存对象
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="dbId"></param>
        /// <returns></returns>
        public IRedisClient GetClientTest(Context ctx, long dbId = 0)
        {
            
            IRedisClient client = null;

            try
            {
                
                client = client = new RedisClient(DataBaseConst.ALRedisIP, DataBaseConst.RedisPort, DataBaseConst.ALRedisPwd);
                client.Db = dbId;
                client.ConnectTimeout = 600000;
                client.RetryTimeout = 600000;
                client.SendTimeout = 600000;
            }
            catch (Exception ex)
            {
                LogUtils.WriteSynchroLog(ctx, SynchroDataType.SaleOrder, "Redis:GetClientEx操作出现异常，异常信息：" + ex.Message + System.Environment.NewLine + ex.StackTrace);
            }

            return client;
        }
        public IRedisClient GetClient(Context ctx, long dbId)
        {
            IRedisClient client = null;
            
            try
            {
                
                client = client = new RedisClient(DataBaseConst.HKRedisIP, DataBaseConst.RedisPort, DataBaseConst.HKRedisPwd);
                client.Db = dbId;
                client.ConnectTimeout = 600000;
                client.RetryTimeout = 600000;
                client.SendTimeout = 600000;
            }
            catch (Exception ex)
            {
                LogUtils.WriteSynchroLog(ctx, SynchroDataType.SaleOrder, "Redis:GetClientEx操作出现异常，异常信息：" + ex.Message + System.Environment.NewLine + ex.StackTrace);
            }

            return client;
        }

        /// <summary>
        /// 从Redis读取数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="ctx"></param>
        /// <param name="dbId"></param>
        /// <returns></returns>
        public T Get<T>(Context ctx, string key, long dbId)
        {

            if (string.IsNullOrEmpty(key))
            {
                return default(T);
            }
            if (!Exist(ctx, key, dbId))
            {
                return default(T);
            }

            if (_prcm != null)
            {
                using (var client = GetClientEx(ctx, dbId))
                {
                    if (client != null)
                    {
                        return client.Get<T>(key);
                    }
                }
            }

            return default(T);
        }

        /// <summary>
        /// 写数据到Redis
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiry"></param>
        /// <param name="ctx"></param>
        /// <param name="dbId"></param>
        /// <returns></returns>
        public bool Set<T>(string key, T value, DateTime expiry, Context ctx, long dbId)
        {

            if (value == null || string.IsNullOrEmpty(key))
            {
                return false;
            }

            if (_prcm != null)
            {
                using (var client = GetClientEx(ctx, dbId))
                {
                    if (client != null)
                    {
                        return client.Set<T>(key, value, expiry);
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 根据集合keys获取Redis数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ctx"></param>
        /// <param name="keys"></param>
        /// <param name="dbId"></param>
        /// <returns></returns>
        public T GetAll<T>(Context ctx, IEnumerable<string> keys, long dbId)
        {
            if (keys == null || keys.Count() == 0)
            {
                return default(T);
            }

            if (_prcm != null)
            {
                using (var client = GetClientEx(ctx, dbId))
                {
                    if (client != null)
                    {
                        return (T)client.GetAll<T>(keys);
                    }
                }
            }

            return default(T);
        }

        /// <summary>
        /// 从Redis批量读取数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dict"></param>
        /// <param name="expiry"></param>
        /// <param name="ctx"></param>
        /// <param name="dbId"></param>
        public void SetAll<T>(Dictionary<string, T> dict, DateTime expiry, Context ctx, long dbId)
        {
            if (dict == null || dict.Count == 0)
            {
                return;
            }

            if (_prcm != null)
            {
                using (var client = GetClientEx(ctx, dbId))
                {
                    if (client != null)
                    {

                        client.SetAll<T>(dict);
                    }
                }
            }

            return;
        }

        /// <summary>
        /// 把数据写到Redis
        /// </summary>
        /// <param name="key"></param>
        /// <param name="items"></param>
        /// <param name="ctx"></param>
        /// <param name="dbId"></param>
        public void AddRangeToSet(string key, List<string> items, Context ctx, long dbId)
        {
            if (items == null || items.Count == 0 || string.IsNullOrEmpty(key))
            {
                return;
            }

            if (_prcm != null)
            {
                using (var client = GetClientEx(ctx, dbId))
                {
                    if (client != null)
                    {
                        client.AddRangeToSet(key, items);
                    }
                }
            }
        }

        /// <summary>
        /// 根据setId读取Redis数据
        /// </summary>
        /// <param name="setId"></param>
        /// <param name="ctx"></param>
        /// <param name="dbId"></param>
        /// <returns></returns>
        public HashSet<string> GetAllItemsFromSet(string setId, Context ctx, long dbId)
        {
            if (string.IsNullOrEmpty(setId))
            {
                return default(HashSet<string>);
            }

            if (_prcm != null)
            {
                using (var client = GetClientEx(ctx, dbId))
                {
                    if (client != null)
                    {
                        return client.GetAllItemsFromSet(setId);
                    }
                }
            }

            return default(HashSet<string>);
        }

        /// <summary>
        /// 根据setId从Redis读取数据后把数据删除掉
        /// </summary>
        /// <param name="setId"></param>
        /// <param name="ctx"></param>
        /// <param name="dbId"></param>
        public void PopItemFromSet(Context ctx, string setId, long dbId)
        {
            if (string.IsNullOrEmpty(setId))
            {
                return;
            }

            if (_prcm != null)
            {
                using (var client = GetClientEx(ctx, dbId))
                {
                    if (client != null)
                    {
                        client.PopItemFromSet(setId);

                    }
                }
            }
        }

        /// <summary>
        /// 根据keys从Redis删除数据
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="keys"></param>
        /// <param name="dbId"></param>
        public void RemoveAll(Context ctx, IEnumerable<string> keys, long dbId)
        {
            if (keys == null || keys.Count() == 0)
            {
                return;
            }


            if (_prcm != null)
            {
                using (var client = GetClientEx(ctx, dbId))
                {
                    if (client != null)
                    {
                        client.RemoveAll(keys);
                    }
                }
            }
        }

        /// <summary>
        /// 删除指定的Item
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="setId"></param>
        /// <param name="item"></param>
        /// <param name="dbId"></param>
        public void RemoveItemFromSet(Context ctx, string setId, string item, long dbId)
        {
            if (string.IsNullOrWhiteSpace(setId) || string.IsNullOrWhiteSpace(item))
            {
                return;
            }

            if (_prcm != null)
            {
                using (var client = GetClientEx(ctx, dbId))
                {
                    if (client != null)
                    {
                        client.RemoveItemFromSet(setId, item);
                    }
                }
            }
        }

        /// <summary>
        /// 根据key从Redis删除数据
        /// </summary>
        /// <param name="key"></param>
        /// <param name="ctx"></param>
        /// <param name="dbId"></param>
        /// <returns></returns>

        public bool Remove(Context ctx, string key, long dbId)
        {
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }

            if (_prcm != null)
            {
                using (var client = GetClientEx(ctx, dbId))
                {
                    if (client != null)
                    {
                        return client.Remove(key);
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 根据key判断数据在Redis是否存在
        /// </summary>
        /// <param name="key"></param>
        /// <param name="ctx"></param>
        /// <param name="dbId"></param>
        /// <returns></returns>
        public bool Exist(Context ctx, string key, long dbId)
        {
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }

            if (_prcm != null)
            {
                using (var client = GetClientEx(ctx, dbId))
                {
                    if (client != null)
                    {
                        return client.ContainsKey(key);
                    }
                }
            }

            return false;
        }
    }
}
