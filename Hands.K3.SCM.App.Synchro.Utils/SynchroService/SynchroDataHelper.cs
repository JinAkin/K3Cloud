using Kingdee.BOS;
using System;
using System.Collections.Generic;
using System.Reflection;
using Kingdee.BOS.Util;

using Hands.K3.SCM.APP.Entity.K3WebApi;

using Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject;
using Hands.K3.SCM.APP.Entity.StructType;
using Kingdee.BOS.ServiceHelper;
using Hands.K3.SCM.App.Synchro.Base.Abstract;
using HS.K3.Common.Abbott;
using Hands.K3.SCM.APP.Entity.EnumType;
using System.Linq;

namespace Hands.K3.SCM.App.Synchro.Utils.SynchroService
{

    /// <summary>
    /// 数据同步工具类
    /// </summary>
    public partial class SynchroDataHelper
    {
        static List<Type> clsSynchroToK3Type = new List<Type>();
        static List<Type> clsSynchroToHCType = new List<Type>();

        public delegate HttpResponseResult DoSchroData(AbstractSynchro absSyn, SynchroDirection direciton, Dictionary<SynOperationType, IEnumerable<AbsSynchroDataInfo>> dict = null, IEnumerable<AbsSynchroDataInfo> datas = null, IEnumerable<string> numbers = null, bool flag = true);


        static object K3Lock = new object();
        static object HCLock = new object();

        private static readonly object synchroLock = new object();

        static Assembly assembly = Assembly.LoadFrom(string.Format(@"{0}\bin\Hands.K3.SCM.App.Core.dll", AppDomain.CurrentDomain.BaseDirectory));

        /// <summary>
        /// 同步所有待同步数据
        /// </summary>
        /// <param name="ctx"></param>
        public static void SynchroAllDataToK3(Context ctx)
        {
            foreach (string name in Enum.GetNames(typeof(SynchroDataType)))
            {
                SynchroDataType dataType = (SynchroDataType)Enum.Parse(typeof(SynchroDataType), name);
                SynchroDataToK3(ctx, dataType);
            }
        }

        /// <summary>
        /// 同步数据至K3
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="dataType"></param>
        /// <param name="IsAsync"></param>
        /// <param name="numbers"></param>
        /// <param name="dict"></param>
        /// <returns></returns>
        public static HttpResponseResult SynchroDataToK3(Context ctx, SynchroDataType dataType, bool IsAsync = true, IEnumerable<string> numbers = null, Dictionary<SynOperationType, IEnumerable<AbsSynchroDataInfo>> dict = null)
        {

            List<AbstractSynchroToK3> svc = new List<AbstractSynchroToK3>();

            HttpResponseResult result = new HttpResponseResult();
            result.Success = true;

            lock (K3Lock)
            {
                svc = GetSynchroToK3ClsInstance(dataType);
            }
            if (svc == null || svc.Count == 0)
            {
                result = new HttpResponseResult();
                result.Success = false;
                result.Message = "没有找到【" + dataType + "】同步类的实例！！！";
                return result;
            }

            var token = GetSeeedSystemProfile(ctx, "");
            if (token.IsNullOrEmptyOrWhiteSpace())
            {
                token = "";
            }

            string url = GetWebAppRootUrl(ctx);

            foreach (var item in svc)
            {
                item.UserToken = token;
                item.WS_URL_Service = url;
                item.K3CloudContext = ctx;
                DataBaseConst.K3CloudContext = ctx;


                if (item.K3CloudContext != null)
                {
                    if (item.K3CloudContext.DBId.CompareTo(DataBaseConst.K3CloudDbId) == 0)
                    {
                        if (item.Direction == SynchroDirection.ToB2B)
                        {
                            item.RedisDbId = DataBaseConst.HKRedisDbId_B2B;
                        }
                        else
                        {
                            item.RedisDbId = DataBaseConst.HKRedisDbId;
                        }
                    }
                    else
                    {
                        if (item.Direction == SynchroDirection.ToB2B)
                        {
                            item.RedisDbId = DataBaseConst.ALRedisDbId_B2B;
                        }
                        else
                        {
                            item.RedisDbId = DataBaseConst.ALRedisDbId;
                        }
                    }

                    HttpResponseResult ret = item.DoSynchroData(numbers, dict);

                    if (ret != null)
                    {
                        if (item.RedisDbId != 0)
                        {
                            result.Message += "RedisDbId【" + item.RedisDbId + "】" + ret.Message + Environment.NewLine;
                            result.Success = result.Success && ret.Success;
                        }
                        else
                        {
                            result.Message += item.RedisDbId  + ret.Message + Environment.NewLine;
                            result.Success = result.Success && ret.Success;
                        }
                    }
                   
                    return result;
                }
            }

            return result;

            //List<AbstractSynchroToK3> types = GetSynchroClsInstance_1<AbstractSynchroToK3>(assembly, typeof(AbstractSynchroToK3), dataType, SynchroDirection.ToK3);

            //if (types != null && types.Count > 0)
            //{
            //    DoSchroData doSchro = new DoSchroData(DoSychro);
            //    return SynchroDataTo(ctx, types.FirstOrDefault(), dataType, SynchroDirection.ToK3, doSchro, dict, null, numbers, true);
            //}
            //return null;
        }

        private static HttpResponseResult DoSychro(AbstractSynchro absSyn, SynchroDirection direciton, Dictionary<SynOperationType, IEnumerable<AbsSynchroDataInfo>> dict = null, IEnumerable<AbsSynchroDataInfo> datas = null, IEnumerable<string> numbers = null, bool flag = true)
        {
            if (direciton == SynchroDirection.ToK3)
            {
                AbstractSynchroToK3 k3 = absSyn as AbstractSynchroToK3;
                return k3.DoSynchroData(numbers, dict);
            }
            else
            {
                AbstractSynchroToHC hc = absSyn as AbstractSynchroToHC;
                return hc.SynK3DataToWebSite(datas, numbers, flag);
            }
        }
        /// <summary>
        /// 同步数据至HC网站
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="dataType"></param>
        /// <param name="datas"></param>
        /// <param name="billNos"></param>
        /// <returns></returns>
        public static HttpResponseResult SynchroDataToHC(Context ctx, SynchroDataType dataType, IEnumerable<AbsSynchroDataInfo> datas = null, IEnumerable<string> billNos = null, bool SQLFilter = true, SynchroDirection direction = SynchroDirection.ToHC)
        {

            List<AbstractSynchroToHC> svc = new List<AbstractSynchroToHC>();
            HttpResponseResult result = null;

            lock (HCLock)
            {
                svc = GetSynchroClsInstance<AbstractSynchroToHC>(assembly, typeof(AbstractSynchroToHC), dataType);
            }

            if (svc == null || svc.Count == 0)
            {
                result = new HttpResponseResult();
                result.Success = false;
                result.Message = "没有找到【" + dataType + "】同步类的实例！！！";
                return result;
            }

            if (svc != null && svc.Count > 0)
            {
                foreach (var item in svc)
                {
                    item.K3CloudContext = ctx;
                    DataBaseConst.K3CloudContext = ctx;

                    if (item.K3CloudContext != null)
                    {
                        if (item.K3CloudContext.DBId.CompareTo(DataBaseConst.K3CloudDbId) == 0)
                        {
                            if (direction != SynchroDirection.ToHC)
                            {
                                if (direction == SynchroDirection.ToB2B)
                                {
                                    item.RedisDbId = DataBaseConst.HKRedisDbId_B2B;
                                }
                                else
                                {
                                    item.RedisDbId = DataBaseConst.HKRedisDbId;
                                }
                            }
                            else
                            {
                                if (item.Direction == SynchroDirection.ToB2B)
                                {
                                    item.RedisDbId = DataBaseConst.HKRedisDbId_B2B;
                                }
                                else
                                {
                                    item.RedisDbId = DataBaseConst.HKRedisDbId;
                                }
                            }

                        }
                        else
                        {
                            if (direction != SynchroDirection.ToHC)
                            {
                                if (direction == SynchroDirection.ToB2B)
                                {
                                    item.RedisDbId = DataBaseConst.ALRedisDbId_B2B;
                                }
                                else
                                {
                                    item.RedisDbId = DataBaseConst.ALRedisDbId;
                                }
                            }
                            else
                            {
                                if (item.Direction == SynchroDirection.ToB2B)
                                {
                                    item.RedisDbId = DataBaseConst.ALRedisDbId_B2B;
                                }
                                else
                                {
                                    item.RedisDbId = DataBaseConst.ALRedisDbId;
                                }
                            }
                        }
                    }

                    result = item.SynchroDataToHC(datas, billNos, SQLFilter, direction);
                }
            }

            return result;

            //List<AbstractSynchroToHC> types = GetSynchroClsInstance_1<AbstractSynchroToHC>(assembly, typeof(AbstractSynchroToHC), dataType, SynchroDirection.ToK3);

            //if (types != null && types.Count > 0)
            //{
            //    DoSchroData doSchro = new DoSchroData(DoSychro);
            //    return SynchroDataTo(ctx, types.FirstOrDefault(), dataType, SynchroDirection.ToHC, doSchro, null, datas, null, true);
            //}
            //return null;
        }

        private static HttpResponseResult SynchroDataTo(Context ctx, AbstractSynchro absSyn, SynchroDataType dataType, SynchroDirection direction, DoSchroData doSchroData, Dictionary<SynOperationType, IEnumerable<AbsSynchroDataInfo>> dict = null, IEnumerable<AbsSynchroDataInfo> datas = null, IEnumerable<string> numbers = null, bool flag = true)
        {
            HttpResponseResult result = null;

            if (absSyn == null)
            {
                result = new HttpResponseResult();
                result.Success = false;
                result.Message = "没有找到【" + dataType + "】同步类的实例！！！";

                return result;
            }

            var token = GetSeeedSystemProfile(ctx, "");
            if (token.IsNullOrEmptyOrWhiteSpace())
            {
                token = "";
            }

            string url = GetWebAppRootUrl(ctx);

            absSyn.K3CloudContext = ctx;


            if (absSyn.K3CloudContext != null)
            {
                if (absSyn.K3CloudContext.DBId.CompareTo(DataBaseConst.K3CloudDbId) == 0)
                {
                    absSyn.RedisDbId = DataBaseConst.HKRedisDbId;
                }
                else
                {
                    absSyn.RedisDbId = DataBaseConst.ALRedisDbId;
                }
            }

            return doSchroData(absSyn, direction, dict, datas, numbers, flag);
        }

        /// <summary>
        /// 创建同步至K3的数据
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="dataType"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        public static AbsSynchroDataInfo BuildSynchroData(Context ctx, SynchroDataType dataType, string json)
        {
            List<AbstractSynchroToK3> svc = new List<AbstractSynchroToK3>();

            lock (K3Lock)
            {
                svc = GetSynchroToK3ClsInstance(dataType);
            }

            if (svc != null && svc.Count > 0)
            {
                foreach (var item in svc)
                {
                    return item.BuildSynchroData(ctx, json);
                }
            }

            return null;
        }

        /// <summary>
        /// 获取K/3 Cloud站点地址
        /// </summary>
        /// <returns></returns>
        public static string GetWebAppRootUrl(Context ctx)
        {
            var siteuri = GetSeeedSystemProfile(ctx, "F_JN_K3EDIAddress");
            if (siteuri.IsNullOrEmptyOrWhiteSpace() || !siteuri.StartsWith(@"http://"))
            {
                return @"http://localhost/k3cloud/services/K3EDIService.asmx";
            }

            return siteuri;
        }

        /// <summary>
        /// 动态创建同步至K3的实现类
        /// </summary>
        private static void CreateSynchroToK3ClsInstance()
        {
            lock (K3Lock)
            {
                if (clsSynchroToK3Type.Count > 0)
                {
                    return;
                }

                var clsTypes = assembly.GetTypes();

                if (clsTypes != null && clsTypes.Length > 0)
                {
                    foreach (var item in clsTypes)
                    {
                        if (item != null)
                        {
                            if (item.BaseType != null)
                            {
                                if (item.BaseType == typeof(AbstractSynchroToK3) || item.BaseType.BaseType == typeof(AbstractSynchroToK3))
                                {
                                    clsSynchroToK3Type.Add(item);
                                }
                            }
                        }
                    }
                }
            }

        }

        /// <summary>
        /// 动态创建同步至K3的实现类
        /// </summary>
        private static List<AbstractSynchroToK3> GetSynchroToK3ClsInstance(SynchroDataType dataType)
        {
            if (clsSynchroToK3Type.Count == 0)
            {
                CreateSynchroToK3ClsInstance();
            }
            List<AbstractSynchroToK3> lst = new List<AbstractSynchroToK3>();
            foreach (var item in clsSynchroToK3Type)
            {
                AbstractSynchroToK3 x = Activator.CreateInstance(item) as AbstractSynchroToK3;
                if (x.DataType == dataType)
                {
                    lst.Add(x);
                }
            }

            return lst;
        }

        public static string GetSeeedSystemProfile(Context ctx, string key)
        {
            var sql = string.Format(@"select FKEY,FVALUE from T_BAS_SYSTEMPROFILE 
                                        Where FCATEGORY='Hands' And FKEY = '{0}' ", key);
            var data = DBServiceHelper.ExecuteDynamicObject(ctx, sql);
            if (data == null || data.Count == 0)
            {
                return "";
            }

            return Convert.ToString(data[0]["FVALUE"]);
        }

        /// <summary>
        /// Token连接测试
        /// </summary>
        /// <param name="ctx"></param> 
        public static bool TokenTest(Context ctx, ref string message)
        {
            var token = GetSeeedSystemProfile(ctx, "");
            if (token.IsNullOrEmptyOrWhiteSpace())
            {
                token = "";
            }

            ////HttpRequestParam paras = HttpRequestParam.Create(
            //   new KeyValuePair<string, string>[] { 
            //        new KeyValuePair<string,string>("userToken",token) 
            //    }
            //   );

            HttpResponseResult result = null;
            try
            {
                var url = GetWebAppRootUrl(ctx);
                //result = WSUtil.InvokeYQTWebService(url, "TestWebConnection", paras);
            }
            catch (Exception ex)
            {
                message = ex.Message + Environment.NewLine + ex.StackTrace;
                return false;
            }

            if (result != null && result.Success)
            {
                message = "Token连接正确";
                return true;
            }

            message = result.Message;
            return false;
        }


        /// <summary>
        /// 获取同步HC类的实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assembly"></param>
        /// <param name="type"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        private static List<T> GetSynchroClsInstance<T>(Assembly assembly, Type type, SynchroDataType dataType)
        {

            List<T> types = null;

            if (clsSynchroToHCType == null || clsSynchroToHCType.Count == 0)
            {
                CreateSynchroHCClsInstance<T>(assembly, type);
            }

            types = new List<T>();

            if (clsSynchroToHCType != null && clsSynchroToHCType.Count > 0)
            {
                foreach (var item in clsSynchroToHCType)
                {
                    if (item != null)
                    {
                        if (item.BaseType != null)
                        {
                            if (item.BaseType == type || item.BaseType.BaseType == type)
                            {
                                T x = (T)Activator.CreateInstance(item);

                                if (x != null)
                                {
                                    if (x.GetType() != null)
                                    {
                                        if (x.GetType().GetProperty("DataType") != null)
                                        {
                                            if (x.GetType().GetProperty("DataType").GetValue(x, null) != null)
                                            {
                                                if (dataType.ToString() == x.GetType().GetProperty("DataType").GetValue(x, null).ToString())
                                                {
                                                    types.Add(x);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                    }
                }
            }


            return types;
        }


        /// <summary>
        /// 获取同步类的实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assembly"></param>
        /// <param name="type"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        private static List<T> GetSynchroClsInstance_<T>(Assembly assembly, Type type, SynchroDataType dataType)
        {

            List<T> types = null;

            if (clsSynchroToHCType == null || clsSynchroToHCType.Count == 0)
            {
                CreateSynchroHCClsInstance<T>(assembly, type);
            }

            types = new List<T>();

            if (clsSynchroToHCType != null && clsSynchroToHCType.Count > 0)
            {
                foreach (var item in clsSynchroToHCType)
                {
                    if (item != null)
                    {
                        if (item.BaseType != null)
                        {
                            if (item.BaseType == type || item.BaseType.BaseType == type)
                            {
                                T x = (T)Activator.CreateInstance(item);

                                if (x != null)
                                {
                                    if (x.GetType() != null)
                                    {
                                        if (x.GetType().GetProperty("DataType") != null)
                                        {
                                            if (x.GetType().GetProperty("DataType").GetValue(x, null) != null)
                                            {
                                                if (dataType.ToString() == x.GetType().GetProperty("DataType").GetValue(x, null).ToString())
                                                {
                                                    types.Add(x);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                    }
                }
            }


            return types;
        }

        /// <summary>
        /// 动态创建同步至HC的实现类
        /// </summary>
        private static void CreateSynchroHCClsInstance<T>(Assembly assembly, Type type)
        {
            lock (HCLock)
            {
                if (clsSynchroToHCType.Count > 0)
                {
                    return;
                }
                var clsTypes = assembly.GetTypes();

                if (clsTypes != null && clsTypes.Length > 0)
                {
                    foreach (var item in clsTypes)
                    {
                        if (item != null)
                        {
                            if (item.BaseType != null)
                            {
                                if (item.BaseType == type || item.BaseType.BaseType == type)
                                {
                                    clsSynchroToHCType.Add(item);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 动态创建同步至K3或HC的实现类
        /// </summary>
        private static List<T> GetSynchroClsInstance_1<T>(Assembly assembly, Type type, SynchroDataType dataType, SynchroDirection direction)
        {
            lock (synchroLock)
            {
                List<Type> types = CreateSynchroInstance<Type>(assembly, type, dataType);

                if (direction.ToString().Equals(SynchroDirection.ToHC))
                {
                    if (clsSynchroToHCType.Count > 0)
                    {
                        return clsSynchroToHCType as List<T>;
                    }
                    else
                    {
                        return GetInstances<T>(types, SynchroDirection.ToHC);
                    }
                }
                else
                {
                    if (clsSynchroToK3Type.Count > 0)
                    {
                        return clsSynchroToK3Type as List<T>;
                    }
                    else
                    {
                        return GetInstances<T>(types, SynchroDirection.ToK3);
                    }
                }
            }
        }

        private static List<T> CreateSynchroInstance<T>(Assembly assembly, Type type, SynchroDataType dataType)
        {
            List<T> types = null;

            if (assembly != null)
            {
                var clsTypes = assembly.GetTypes();

                if (clsTypes != null && clsTypes.Length > 0)
                {
                    types = new List<T>();

                    foreach (var item in clsTypes)
                    {
                        if (item != null)
                        {
                            if (item.BaseType != null)
                            {
                                if (item.BaseType == type || item.BaseType.BaseType == type)
                                {
                                    object proValue = GetPropertyValue<object>(item, "DataType");

                                    if (proValue != null)
                                    {
                                        if (proValue.ToString().CompareTo(dataType) == 0)
                                        {
                                            types.Add((T)(object)item);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return types;
        }

        private static List<T> GetInstances<T>(List<Type> types, SynchroDirection direction)
        {
            if (types != null && types.Count > 0)
            {
                object proVal = null;

                foreach (var type in types)
                {
                    if (type != null)
                    {
                        proVal = GetPropertyValue<object>(type, "Direction");

                        if (proVal != null)
                        {
                            if (proVal.ToString().CompareTo(SynchroDirection.ToHC) == 0)
                            {
                                clsSynchroToHCType.Add(type);
                            }
                            else
                            {
                                clsSynchroToK3Type.Add(type);
                            }
                        }
                    }

                    if (proVal.ToString().CompareTo(SynchroDirection.ToHC) == 0)
                    {
                        return clsSynchroToHCType as List<T>;
                    }
                    else
                    {
                        return clsSynchroToK3Type as List<T>;
                    }
                }
            }

            return null;
        }
        private static object GetPropertyValue<T>(Type type, string property)
        {
            if (type != null && !string.IsNullOrWhiteSpace(property))
            {
                T x = (T)Activator.CreateInstance(type);

                if (x != null)
                {
                    return type.GetProperty(property).GetValue(x, null);
                }
            }
            return null;
        }
    }
}
