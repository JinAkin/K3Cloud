using Hands.K3.SCM.APP.Entity.EnumType;
using Hands.K3.SCM.APP.Entity.K3WebApi;
using Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject;
using HS.K3.Common.Abbott;
using Kingdee.BOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.App.Synchro.Base.Interface
{
    public interface ISynchroData
    {
        /// <summary>
        /// K3Cloud的上下文
        /// </summary>
        Context K3CloudContext { get; set; }
        /// <summary>
        /// 操作的单据类型
        /// </summary>
         SynchroDataType DataType { get; }

        /// <summary>
        /// 数据同步方向
        /// </summary>
        SynchroDirection Direction { get; }

        /// <summary>
        /// Redis数据库的ID
        /// </summary>
        long RedisDbId { get; set; }

        ///// <summary>
        ///// Redis全部的Set类型的key
        ///// </summary>
        //string RedisAllKey { get; }

        ///// <summary>
        ///// Redis未读的Set类型的key
        ///// </summary>
        //string RedisUnreadkey { get; }


        ///// <summary>
        ///// Redis string类型的具体数据
        ///// </summary>
        //string RedisInfoKey { get;}
        void RemoveRedisData(IEnumerable<string> numbers);

    }
}
