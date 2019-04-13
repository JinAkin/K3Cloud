using Hands.K3.SCM.APP.Entity.StructType;
using Kingdee.BOS;

namespace Hands.K3.SCM.APP.Entity.K3WebApi
{
    public static class K3LoginInfo
    {
        /// <summary>
        /// 接口所在服务器的地址
        /// </summary>
        public static string K3CloudURL { get; set; }
        
        /// <summary>
        /// 账套ID
        /// </summary>
        public static string DbId { get; set; }
       
        /// <summary>
        /// 登录用户名
        /// </summary>
        public static string UserName { get; set; }
        
        /// <summary>
        /// 登录密码
        /// </summary>
        public static string Password { get; set; }
       
        /// <summary>
        /// 账套言语类型
        /// </summary>
        public static int LanguageType { get; set; }

        ///// <summary>
        ///// 根据K3Cloud的账套选择对应的K3Cloud服务器地址
        ///// </summary>
        ///// <param name="ctx"></param>
        ///// <returns></returns>
        //public static string GetK3CloudURL(Context ctx)
        //{
        //    
           
        //    if (dbc.K3CloudServerURL.Contains(ctx.IpAddress))
        //    {
        //        return dbc.K3CloudServerURL;
        //    }
        //    else
        //    {
        //        return dbc.K3CloudServerURL_T;
        //    }
        //}

        ///// <summary>
        ///// 根据K3Cloud服务网址获取Redis服务的IP地址（正式/测试）
        ///// </summary>
        ///// <param name="ctx"></param>
        ///// <returns></returns>
        //public static string GetRedisServerIp(Context ctx)
        //{
        //    
            

        //    if (GetK3CloudURL(ctx).CompareTo(dbc.K3CloudServerURL) == 0)
        //    {
        //        return DataBaseConst.HKRedisIP;
        //    }
        //    return DataBaseConst.ALRedisIP;
        //}

        ///// <summary>
        ///// 根据Redis服务器的IP获取相对应的Redis的密码
        ///// </summary>
        ///// <param name="ctx"></param>
        ///// <returns></returns>
        //public static string GetRedisServerPwd(Context ctx)
        //{
        //    

        //    if (GetRedisServerIp(ctx).CompareTo(DataBaseConst.HKRedisIP) == 0 )
        //    {
        //        return DataBaseConst.HKRedisPwd;
        //    }
        //    return DataBaseConst.ALRedisPwd;
        //}
    }
}
