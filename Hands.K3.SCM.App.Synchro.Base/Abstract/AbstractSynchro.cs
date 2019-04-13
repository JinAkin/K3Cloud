
using Hands.K3.SCM.APP.Entity.EnumType;
using Hands.K3.SCM.APP.Entity.K3WebApi;
using Hands.K3.SCM.APP.Entity.StructType;
using Hands.K3.SCM.APP.Utils;
using Hands.K3.SCM.APP.Utils.Utils;
using HS.K3.Common.Abbott;
using Kingdee.BOS;
using System.Collections.Generic;
using System.Linq;


namespace Hands.K3.SCM.App.Synchro.Base.Abstract
{
    public abstract class AbstractSynchro
    {
        /// <summary>
        /// K3Cloud的上下文
        /// </summary>
        public Context K3CloudContext { get; set; }

        /// <summary>
        /// 操作的单据类型
        /// </summary>
        public abstract SynchroDataType DataType { get; }

        /// <summary>
        /// 数据同步方向
        /// </summary>
        public abstract SynchroDirection Direction { get; }
        /// <summary>
        /// Redis数据库的ID
        /// </summary>
        public long RedisDbId { get; set; }

        /// <summary>
        /// Redis全部的Set类型的key
        /// </summary>
        public string RedisAllKey
        {
            get
            {
                return RedisKeyUtils.GetRedisSetKey(this.DataType, this.Direction)["allKey"];
            }
        }

        /// <summary>
        /// Redis未读的Set类型的key
        /// </summary>
        public string RedisUnreadkey
        {
            get
            {
                return RedisKeyUtils.GetRedisSetKey(this.DataType, this.Direction)["unreadKey"];
            }
        }

        /// <summary>
        /// Redis string类型的具体数据
        /// </summary>
        public string RedisInfoKey
        {
            get
            {
                return RedisKeyUtils.GetRedisSetKey(this.DataType, this.Direction)["infoKey"];
            }
        }

        /// <summary>
        /// 根据K3Cloud上下文获取Redis数据库的ID
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public long GetRedisDbId(Context ctx)
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

        /// <summary>
        /// 删除Redis数据库中的数据
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="numbers"></param>
        /// <param name="dataType"></param>
        public virtual void RemoveRedisData(Context ctx, IEnumerable<string> numbers, SynchroDataType dataType = SynchroDataType.None)
        {

            long dbId = 0;

            if (this.K3CloudContext == null)
            {
                dbId = GetRedisDbId(ctx);
            }
            else
            {
                dbId = this.RedisDbId;
            }

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
                                infoKeys.Add(this.RedisInfoKey + num);
                                manager.RemoveItemFromSet(ctx, this.RedisUnreadkey, num, dbId);
                            }
                            else
                            {
                                infoKeys.Add(RedisKeyUtils.GetRedisSetKey(dataType, this.Direction)["infoKey"] + num);
                                manager.RemoveItemFromSet(ctx, RedisKeyUtils.GetRedisSetKey(dataType, this.Direction)["unreadKey"], num, dbId);
                            }
                        }
                    }
                    
                    if (DataBaseConst.CurrentRedisServerIp.CompareTo(DataBaseConst.HKRedisIP) == 0)
                    {
                        manager.RemoveAll(ctx, infoKeys, this.RedisDbId);
                    }

                    manager = null;
                }
            }
        }
    }
}
