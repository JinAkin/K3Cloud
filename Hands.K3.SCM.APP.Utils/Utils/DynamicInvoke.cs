
using Hands.K3.SCM.APP.Entity.EnumType;
using Hands.K3.SCM.APP.Entity.K3WebApi;
using Kingdee.BOS;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Hands.K3.SCM.APP.Utils.Utils
{
    public class DynamicInvoke
    {
        static object objLock = new object();
        static object plmLock = new object();

        static List<Type> clsSynchroType = new List<Type>();

        /// <summary>
        /// 动态调用方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ctx"></param>
        /// <param name="type"></param>
        /// <param name="methodName"></param>
        /// <param name="agrs"></param>
        /// <returns></returns>
        public static T InvokeMethod<T>(Context ctx, Type type, string methodName, object[] agrs)
        {
            if (type != null)
            {
                return (T)type.InvokeMember(methodName, BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.InvokeMethod|BindingFlags.Static|BindingFlags.Instance, null, null, agrs);
                //MethodInfo method = type.GetMethod(methodName);
                //return (T)method.Invoke(type, agrs);
            }

            return default(T);
        }

        /// <summary>
        /// 获取程序集
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public static Assembly GetAssembly(string location)
        {
            if (!string.IsNullOrWhiteSpace(location))
            {
                return Assembly.LoadFile(location);
            }
            
            return null;
        }

        /// <summary>
        /// 获取类型
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="className"></param>
        /// <returns></returns>
        public static Type GetType(Assembly assembly, string className)
        {
            if (assembly != null)
            {
                if (!string.IsNullOrWhiteSpace(className))
                {
                    return assembly.GetType(className);
                }
            }
            return null;
        }

        /// <summary>
        /// 获取方法
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static MethodInfo[] GetMethods(Type type)
        {
            if (type != null)
            {
                return type.GetMethods();
            }
            return null;
        }

        /// <summary>
        /// 动态创建同步的实现类
        /// </summary>
        private static List<T> GetSynchroClsInstance<T>(Assembly assembly, Type type, SynOperationType operType)
        {

            List<T> types = null;

            if (assembly != null)
            {
                CreateSynchroClsInstance<T>(assembly, type);

                types = new List<T>();

                if (clsSynchroType != null && clsSynchroType.Count > 0)
                {
                    foreach (var item in clsSynchroType)
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
                                            if (x.GetType().GetProperty("OperType") != null)
                                            {
                                                if (x.GetType().GetProperty("OperType").GetValue(x, null) != null)
                                                {
                                                    if (operType.ToString() == x.GetType().GetProperty("OperType").GetValue(x, null).ToString())
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
            }

            return types;
        }

        private static void CreateSynchroClsInstance<T>(Assembly assembly, Type type)
        {
            lock (objLock)
            {
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
                                    clsSynchroType.Add(item);
                                }
                            }
                        }
                    }
                }
            }
        }

       
    }
}
