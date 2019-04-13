
using HS.K3.Common.Abbott;
using HS.K3.Common.Mike;
using Kingdee.BOS;
using Kingdee.BOS.Util;
using System.Collections.Generic;
using System.Linq;

namespace Hands.K3.SCM.APP.Entity.StructType
{
    [HotUpdate]
    public static class DataBaseConst
    {
        public static Context K3CloudContext { get; set; }

        /// <summary>
        /// 阿里云K3Cloud服务器地址
        /// </summary>
        public static string K3CloudServerURL
        {
            get
            {
                if (K3CloudContext != null)
                {
                    return CommonMethod.GetSystemParam(K3CloudContext, "K3CloudServerURL");
                }
                else
                {
                    return "";
                }
            }
        }
        /// <summary>
        /// K3Cloud测试服务器地址
        /// </summary>
        public static string K3CloudServerURL_T
        {
            get
            {
                return CommonMethod.GetSystemParam(K3CloudContext, "K3CloudServerURL_T");
            }
        }
        /// <summary>
        /// K3Cloud登陆用户名
        /// </summary>
        public static string K3CloudUserName
        {
            get
            {
                return CommonMethod.GetSystemParam(K3CloudContext, "K3CloudUserName");
            }
        }
        /// <summary>
        /// K3Cloud登陆密码
        /// </summary>
        public static string K3CloudPwd
        {
            get
            {
                return CommonMethod.GetSystemParam(K3CloudContext, "K3CloudPwd");
            }

        }
        /// <summary>
        /// 阿里云K3Cloud账套ID
        /// </summary>
        public static string K3CloudDbId
        {
            get
            {
                return CommonMethod.GetSystemParam(K3CloudContext, "K3CloudDbId");
            }

        }
        /// <summary>
        /// 本地测试K3Cloud账套ID
        /// </summary>
        public static string K3CloudDbId_Local
        {
            get
            {
                return CommonMethod.GetSystemParam(K3CloudContext, "K3CloudDbId_Local");
            }
        }
        /// <summary>
        /// 150测试服务器K3Cloud账套ID
        /// </summary>
        public static string K3CloudDbId_150Test
        {
            get
            {
                return CommonMethod.GetSystemParam(K3CloudContext, "K3CloudDbId_150Test");
            }
        }
        /// <summary>
        /// 多语言
        /// </summary>
        public static int K3CloudLanType
        {
            get
            {
                return int.Parse(CommonMethod.GetSystemParam(K3CloudContext, "K3CloudLanType"));
            }
        }
        /// <summary>
        /// 香港服务器Redis正式数据库ID(HC)
        /// </summary>
        public static long HKRedisDbId
        {
            get
            {
                return long.Parse(CommonMethod.GetSystemParam(K3CloudContext, "HKRedisDbId"));
            }
        }
        /// <summary>
        /// 香港服务器Redis正式数据库ID(B2B)
        /// </summary>
        public static long HKRedisDbId_B2B
        {
            get
            {
                return 5/*long.Parse(CommonMethod.GetSystemParam(K3CloudContext, "HKRedisDbId_B2B"))*/;
            }
        }
        /// <summary>
        /// 阿里云服务器Redis测试数据库ID(HC)
        /// </summary>
        public static long ALRedisDbId
        {
            get
            {
                return long.Parse(CommonMethod.GetSystemParam(K3CloudContext, "ALRedisDbId"));
            }
        }

        /// <summary>
        /// 阿里云服务器Redis测试数据库ID(B2B)
        /// </summary>
        public static long ALRedisDbId_B2B
        {
            get
            {
                return 5/*long.Parse(CommonMethod.GetSystemParam(K3CloudContext, "ALRedisDbId_B2B"))*/;
            }
        }
        /// <summary>
        ///香港服务器RedisIP地址
        /// </summary>
        public static string HKRedisIP
        {
            get
            {
                return CommonMethod.GetSystemParam(K3CloudContext, "HKRedisIP");
            }
        }
        /// <summary>
        /// 阿里云服务器RedisIP地址
        /// </summary>
        public static string ALRedisIP
        {
            get
            {
                return CommonMethod.GetSystemParam(K3CloudContext, "ALRedisIP");
            }
        }
        /// <summary>
        ///香港服务器Redis密码
        /// </summary>
        public static string HKRedisPwd
        {
            get
            {
                return CommonMethod.GetSystemParam(K3CloudContext, "HKRedisPwd");
            }
        }
        /// <summary>
        /// 阿里云服务器Redis密码
        /// </summary>
        public static string ALRedisPwd
        {
            get
            {
                return CommonMethod.GetSystemParam(K3CloudContext, "ALRedisPwd");
            }
        }
        /// <summary>
        /// redis服务端口
        /// </summary>
        public static int RedisPort
        {
            get
            {
                return int.Parse(CommonMethod.GetSystemParam(K3CloudContext, "RedisPort"));
            }
        }

        public static string CurrentK3CloudURL
        {
            get
            {
                if (K3CloudContext.DBId.CompareTo("5a52cfa2b6f201") == 0)
                {
                    return K3CloudServerURL;
                }
                else
                {
                    return K3CloudServerURL_T;
                }
            }
        }

        /// <summary>
        /// 根据K3Cloud服务网址获取Redis服务的IP地址（正式/测试）
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static string CurrentRedisServerIp
        {
            get
            {
                if (CurrentK3CloudURL.CompareTo(K3CloudServerURL) == 0)
                {
                    return HKRedisIP;
                }

                return ALRedisIP;
            }
        }

        /// <summary>
        /// 根据Redis服务器的IP获取相对应的Redis的密码
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static string CurrentRedisServerPwd
        {
            get
            {
                if (CurrentRedisServerIp.CompareTo(HKRedisIP) == 0)
                {
                    return HKRedisPwd;
                }
                return ALRedisPwd;
            }
        }

        /// <summary>
        /// 销售员邮箱
        /// </summary>
        public static string SalerEmail
        {
            get
            {
                return CommonMethod.GetSystemParam(K3CloudContext, "InsteadSaler");
            }
        }

        public static string Param_AUB2B_customerID
        {
            get
            {
                return CommonMethod.GetSystemParam(K3CloudContext, "Param_AUB2B_customerID");
            }
        }
    }
}
