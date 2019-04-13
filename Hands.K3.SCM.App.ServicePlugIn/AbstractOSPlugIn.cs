using Hands.K3.SCM.App.Synchro.Utils.SynchroService;
using Hands.K3.SCM.APP.Entity.CommonObject;
using Hands.K3.SCM.APP.Entity.EnumType;
using Hands.K3.SCM.APP.Entity.K3WebApi;
using Hands.K3.SCM.APP.Entity.StructType;
using Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject;
using Hands.K3.SCM.APP.Utils;
using Hands.K3.SCM.APP.Utils.Utils;
using HS.K3.Common.Abbott;
using HS.K3.Common.Jenna;
using Kingdee.BOS;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.Interaction;
using Kingdee.BOS.Core.Validation;
using Kingdee.BOS.Orm.DataEntity;
using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;

namespace Hands.K3.SCM.App.ServicePlugIn
{
    [Description("抽象类服务插件--更新单据和同步数据到HC网站")]
    public abstract class AbstractOSPlugIn : AbstractOperationServicePlugIn
    {
        public abstract SynchroDataType DataType { get; }

        public virtual SynchroDirection Direction
        {
            get
            {
                if (this.DataType == SynchroDataType.SaleOrderOffline)
                {
                    return SynchroDirection.Default;
                }
                else
                {
                    return SynchroDirection.ToHC;
                }
            }
        }

        public virtual List<DynamicObject> DyamicObjects { get; set; }
        public long GetDBID(Context ctx)
        {
            if (ctx.DBId.CompareTo("5a52cfa2b6f201") == 0)
            {
                if (this.Direction == SynchroDirection.ToB2B)
                {
                    return 5;
                }
                return 3;
            }
            else
            {
                if (this.Direction == SynchroDirection.ToB2B)
                {
                    return 5;
                }
                return 9;
            }
        }

        /// <summary>
        /// 获取K3系统的数据
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<AbsSynchroDataInfo> GetK3Datas(Context ctx, List<DynamicObject> objects, ref HttpResponseResult result)
        {
            return null;
        }

        public virtual HttpResponseResult SynchroK3DataToWebSite(Context ctx, IEnumerable<AbsSynchroDataInfo> datas = null)
        {
            HttpResponseResult result = null;
            if (datas == null)
            {
                return SynchroDataHelper.SynchroDataToHC(ctx, this.DataType, GetK3Datas(ctx, this.DyamicObjects, ref result), null, true, this.Direction);
            }
            else
            {
                return SynchroDataHelper.SynchroDataToHC(ctx, this.DataType, datas, null, true, this.Direction);
            }
        }

        /// <summary>
        /// 单据审核成功后同步数据到HC网站
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="objects"></param>
        /// <returns></returns>
        public virtual HttpResponseResult OperateAfterAudit(Context ctx, List<DynamicObject> objects = null, IEnumerable<AbsSynchroDataInfo> datas = null, SynchroDataType dataType = default(SynchroDataType))
        {
            IEnumerable<AbsSynchroDataInfo> oDatas = null;
            IRedisClient client = null;
            IRedisTransaction trans = null;
            HttpResponseResult result = null;
            Dictionary<List<string>, Dictionary<string, string>> dicts = null;

            if (objects != null && objects.Count > 0)
            {
                oDatas = GetK3Datas(ctx, objects, ref result);
            }
            if (datas != null && datas.Count() > 0)
            {
                oDatas = datas;
            }

            if (result == null)
            {
                result = new HttpResponseResult();
                result.Success = true;
            }

            SqlCommand comm = null;
            bool flag = true;

            try
            {
                if (oDatas != null && oDatas.Count() > 0)
                {
                    int count = 0;
                    string sql = GetExecuteUpdateSql(ctx, oDatas.ToList());

                    comm = UpdateBillInfos(ctx, oDatas.ToList(), sql, ref result);

                    if (comm != null)
                    {
                        count = comm.ExecuteNonQuery();
                    }

                    dicts = GetSynchroData(ctx, oDatas, dataType);

                    if (dicts != null && dicts.Count > 0)
                    {
                        RedisManager manager = new RedisManager(ctx);
                        client = manager.GetClientEx(ctx, GetDBID(ctx));
                        trans = client.CreateTransaction();

                        foreach (var kv in dicts)
                        {
                            trans.QueueCommand(r => r.SetAll(kv.Value));

                            if (dataType == default(SynchroDataType))
                            {
                                trans.QueueCommand(r => r.AddRangeToSet(RedisKeyUtils.GetRedisSetKey(this.DataType, this.Direction)["allKey"], kv.Key));
                                trans.QueueCommand(r => r.AddRangeToSet(RedisKeyUtils.GetRedisSetKey(this.DataType, this.Direction)["unreadKey"], kv.Key));
                            }
                            else
                            {
                                trans.QueueCommand(r => r.AddRangeToSet(RedisKeyUtils.GetRedisSetKey(dataType, this.Direction)["allKey"], kv.Key));
                                trans.QueueCommand(r => r.AddRangeToSet(RedisKeyUtils.GetRedisSetKey(dataType, this.Direction)["unreadKey"], kv.Key));
                            }
                        }

                    }
                    else
                    {
                        result.Success = result.Success && true;
                    }


                    if (count > 0)
                    {
                        if (comm != null)
                        {
                            try
                            {
                                comm.Transaction.Commit();
                                flag = true;
                            }
                            catch (Exception ex)
                            {
                                flag = false;
                                result.Success = result.Success && false;
                                result.Message += ex.Message + Environment.NewLine + ex.StackTrace;
                                LogUtils.WriteSynchroLog(ctx, this.DataType, result.Message);
                            }

                        }
                    }
                    if (trans != null)
                    {
                        flag = trans.Commit();
                    }


                    if (!flag)
                    {
                        if (trans != null)
                        {
                            trans.Rollback();
                        }
                        if (comm != null)
                        {
                            if (comm.Transaction != null)
                            {
                                comm.Transaction.Rollback();
                            }
                        }
                        result.Success = result.Success && false;
                        result.Message += string.Format("【{0}】单据编码【{1}】数据更新和同步到HC网站失败！", dataType == SynchroDataType.None ? DataType : dataType, string.Join(",", oDatas.Select(o => o.SrcNo).ToList()));
                        LogUtils.WriteSynchroLog(ctx, dataType == SynchroDataType.None ? DataType : dataType, result.Message);
                    }
                    else
                    {
                        foreach (var kv in dicts)
                        {
                            if (kv.Value != null && kv.Value.Count > 0)
                            {
                                LogHelper.WriteSynchroDataLog(ctx, this.DataType, client, kv.Value, true);
                            }
                        }

                        result.Success = result.Success && true;
                        result.Message += string.Format("【{0}】单据编码【{1}】数据更新和同步到HC网站成功！", dataType == SynchroDataType.None ? DataType : dataType, string.Join(",", oDatas.Select(o => o.SrcNo).ToList()));
                        LogUtils.WriteSynchroLog(ctx, dataType == SynchroDataType.None ? DataType : dataType, result.Message);
                    }
                }
                else
                {
                    result.Success = result.Success && true;
                }
            }
            catch (Exception ex)
            {
                if (result != null)
                {
                    result.Success = result.Success && false;
                    result.Message += ex.Message + Environment.NewLine + ex.StackTrace;
                    LogUtils.WriteSynchroLog(ctx, this.DataType, result.Message);
                }
                if (trans != null)
                {
                    trans.Rollback();
                }
                if (client != null)
                {
                    client.Dispose();
                }
                if (comm != null)
                {
                    if (comm.Transaction != null)
                    {
                        comm.Transaction.Rollback();
                    }
                }
            }

            finally
            {
                if (trans != null)
                {
                    trans.Dispose();
                }
                if (client != null)
                {
                    client.Dispose();
                }
                if (comm != null)
                {
                    if (comm.Connection != null)
                    {
                        comm.Connection.Close();
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 执行更新"是否已同步客户余额"标记和客户余额操作
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public virtual SqlCommand UpdateBillInfos(Context ctx, List<AbsSynchroDataInfo> datas, string sql, ref HttpResponseResult result)
        {
            SqlCommand comm = null;
            DataBaseInfo info = GetDataBaseInfo(ctx);

            if (info != null)
            {
                string str = string.Format(@"Data Source={3};Initial Catalog={0};Persist Security Info=True;User ID={1};Password={2}", info.DBName, info.DBUser, info.DBPwd, info.DBIPAddr);

                if (!string.IsNullOrWhiteSpace(sql))
                {
                    SqlConnection conn = new SqlConnection(str);
                    conn.Open();

                    SqlTransaction trans = conn.BeginTransaction();
                    comm = new SqlCommand();

                    comm.Connection = conn;
                    comm.CommandText = sql;

                    comm.Transaction = trans;
                    comm.CommandTimeout = 60;
                }

                result.Success = true;
            }
            else
            {
                result.Success = result.Success && false;
                result.Message += "对应账套的数据库信息缺失，请检查.." + Environment.NewLine;
                LogUtils.WriteSynchroLog(ctx, this.DataType, result.Message);
            }
            return comm;
        }

        /// <summary>
        /// 更新"是否已同步客户余额"标记和客户余额的SQL
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public virtual string GetExecuteUpdateSql(Context ctx, List<AbsSynchroDataInfo> datas)
        {
            return null;
        }

        /// <summary>
        /// 获取需要同步的数据（过滤掉100.03组织的单据数据）
        /// </summary>
        /// <param name="datas"></param>
        /// <returns></returns>
        public Dictionary<List<string>, Dictionary<string, string>> GetSynchroData(Context ctx, IEnumerable<AbsSynchroDataInfo> datas, SynchroDataType dataType = default(SynchroDataType))
        {
            Dictionary<List<string>, Dictionary<string, string>> dicts = null;
            Dictionary<string, string> dict = null;
            List<string> keys = null;

            if (datas != null && datas.Count() > 0)
            {
                dicts = new Dictionary<List<string>, Dictionary<string, string>>();
                dict = new Dictionary<string, string>();
                keys = new List<string>();

                var group = default(IEnumerable<IGrouping<string, AbsSynchroDataInfo>>);
                List<AbsDataInfo> infos = null;

                if (this.DataType == SynchroDataType.SaleOrder || this.DataType == SynchroDataType.SaleOrderOffline
                    || this.DataType == SynchroDataType.ReceiveBill || this.DataType == SynchroDataType.ReFundBill
                    || this.DataType == SynchroDataType.DropShippingSalOrder)
                {
                    infos = datas.Select(d => (AbsDataInfo)d).ToList();

                    group = from g in infos
                            where g != null && !string.IsNullOrWhiteSpace(g.SrcNo)
                            && !string.IsNullOrWhiteSpace(g.FSaleOrgId)
                            && g.FSaleOrgId.CompareTo("100.03") != 0
                            && ((g.DataType.CompareTo(SynchroDataType.SaleOrder) == 0
                            && (!string.IsNullOrWhiteSpace(g.F_HS_SaleOrderSource) && g.F_HS_SaleOrderSource.CompareTo("HCWebPendingOder") == 0
                            && !string.IsNullOrWhiteSpace(g.FCancelStatus) && g.FCancelStatus.CompareTo("B") == 0)
                            )
                            || g.DataType.CompareTo(SynchroDataType.SaleOrderOffline) == 0
                            || g.DataType.CompareTo(SynchroDataType.ReceiveBill) == 0
                            || g.DataType.CompareTo(SynchroDataType.ReFundBill) == 0
                            || g.DataType.CompareTo(SynchroDataType.DropShippingSalOrder) == 0
                            )
                            group g by g.SrcNo into g
                            select g;
                }
                else
                {
                    group = from g in datas
                            group g by g.SrcNo into g
                            select g;
                }

                if (group != null && group.Count() > 0)
                {
                    foreach (var g in group)
                    {
                        if (g != null && g.ToList().Count > 0)
                        {
                            keys.Add(g.Key);
                            if (dataType == default(SynchroDataType))
                            {
                                dict.Add(RedisKeyUtils.GetRedisSetKey(this.DataType, this.Direction)["infoKey"] + g.Key, JsonUtils.SerializeObject<IEnumerable<AbsSynchroDataInfo>>(ctx, g.ToList()));
                            }
                            else
                            {
                                dict.Add(RedisKeyUtils.GetRedisSetKey(dataType, this.Direction)["infoKey"] + g.Key, JsonUtils.SerializeObject<IEnumerable<AbsSynchroDataInfo>>(ctx, g.ToList()));
                            }
                        }
                    }

                    dicts.Add(keys, dict);
                }
            }

            return dicts;
        }

        /// <summary>
        /// 连接是否成功
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public virtual bool IsConnectSuccess(Context ctx)
        {
            RedisManager manager = new RedisManager(ctx);

            if (manager != null)
            {
                IRedisClient redis = manager.GetClientEx(ctx, GetDBID(ctx));

                if (redis.HadExceptions == false)
                {
                    return true;
                }
            }
            return false;
        }

        public DataBaseInfo GetDataBaseInfo(Context ctx)
        {
            DataBaseInfo dbInfo = null;

            switch (ctx.DBId)
            {
                case "5a52cfa2b6f201":
                case "5c9db8a1e7be6a":
                    dbInfo = new DataBaseInfo();
                    dbInfo.DBName = "AIS20180108094504";
                    dbInfo.DBUser = "sa";
                    dbInfo.DBPwd = "HandsGroup@!";
                    dbInfo.DBIPAddr = "120.79.41.184";
                    return dbInfo;
                case "5ca16bfd49bae5":
                case "5cac036e65303a":
                    dbInfo = new DataBaseInfo();
                    dbInfo.DBName = "AIS20190401093048";
                    dbInfo.DBUser = "sa";
                    dbInfo.DBPwd = "HandsGroup@!";
                    dbInfo.DBIPAddr = "120.79.41.184";
                    return dbInfo;
                case "5c9841aa6418c3":
                    dbInfo = new DataBaseInfo();
                    dbInfo.DBName = "AIS20190325103937";
                    dbInfo.DBUser = "sa";
                    dbInfo.DBPwd = "1";
                    dbInfo.DBIPAddr = "10.2.0.166";
                    return dbInfo;
                case "5b6901a69345c3":
                case "5c24a16b1d67fe":
                case "5c92f23849bf40":
                    dbInfo = new DataBaseInfo();
                    dbInfo.DBName = "AIS20180807101523";
                    dbInfo.DBUser = "sa";
                    dbInfo.DBPwd = "Hands666";
                    dbInfo.DBIPAddr = "10.2.0.150";
                    return dbInfo;
                case "5c24a9b8ac22a9":
                case "5c7f9b127d7fcf":
                    dbInfo = new DataBaseInfo();
                    dbInfo.DBName = "AIS20181220110342";
                    dbInfo.DBUser = "sa";
                    dbInfo.DBPwd = "HandsGroup@!";
                    return dbInfo;
                default:
                    return dbInfo;
            }
        }

        /// <summary>
        /// 获取实时汇率
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="FSettleCurrId"></param>
        /// <returns></returns>
        public virtual decimal GetExchangeRate(Context ctx, string FSettleCurrId)
        {
            if (!string.IsNullOrWhiteSpace(FSettleCurrId))
            {
                return JennaCommonFile.GetRateToUSAFromTable(ctx, FSettleCurrId);
            }
            return 0;
        }
        public virtual void ShowK3DisplayerMsg(List<string> msgs)
        {
            if (msgs != null && msgs.Count > 0)
            {
                string spensorKey = "JDSample.ServicePlugIn.Operation.S160425ShowInteractionOpPlug.ShowK3DisplayMessage";
                K3DisplayerModel model = K3DisplayerModel.Create(Context, "操作结果提示信息");

                foreach (var msg in msgs)
                {
                    // 消息内容：可以添加多行
                    if (!string.IsNullOrWhiteSpace(msg))
                    {
                        model.AddMessage(msg);
                    }
                }

                // 消息抬头
                model.Option.SetVariableValue(K3DisplayerModel.CST_FormTitle, "有多条信息需要您确认，是否继续？");
                // 是否继续按钮
                model.OKButton.Visible = true;
                model.OKButton.Caption = new LocaleValue("是", Context.UserLocale.LCID);
                model.CancelButton.Visible = true;
                model.CancelButton.Caption = new LocaleValue("否", Context.UserLocale.LCID);
                // 创建一个交互提示错误对象KDInteractionException：
                // 通过throw new KDInteractionException()的方式，向操作调用者，输出交互信息
                KDInteractionException ie = new KDInteractionException(this.Option, spensorKey);
                // 提示信息显示界面
                ie.InteractionContext.InteractionFormId = FormIdConst.BOS_K3Displayer;
                // 提示内容
                ie.InteractionContext.K3DisplayerModel = model;
                // 是否需要交互
                ie.InteractionContext.IsInteractive = true;
                // 抛出错误，终止流程

                throw ie;
            }
        }

    }
}
