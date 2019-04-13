
using Kingdee.BOS;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.SqlBuilder;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Kingdee.BOS.App.Data;
using Hands.K3.SCM.APP.Entity.K3WebApi;
using Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject;
using Hands.K3.SCM.APP.Entity.CommonObject;
using Newtonsoft.Json;
using Hands.K3.SCM.APP.Utils.Utils;
using Hands.K3.SCM.APP.Entity.EnumType;
using HS.K3.Common.Abbott;
using System.Text;
using HS.K3.Common.Mike;
using System.Web;

namespace Hands.K3.SCM.App.Synchro.Base.Abstract
{

    /// <summary>
    /// 数据同步到K3系统的抽象类
    /// </summary>
    public abstract class AbstractSynchroToK3 : AbstractSynchro
    {
        private const int batch = 9;
        public static List<AbsSynchroDataInfo> synCustomersLog = new List<AbsSynchroDataInfo>();
        public AbstractSynchroToK3()
        {

        }

        #region Var

        private string svc = "http://localhost/k3cloud/services/K3EDIService.asmx";

        protected string groupId = Guid.NewGuid().ToString();

        protected FormMetadata metaInfo = null;
        protected BusinessInfo bizInfor = null;

        /// <summary>
        /// K3Cloudd 的同步用户令牌
        /// </summary>
        public string UserToken { get; set; }
        public string WS_URL_Service
        {
            get
            {
                return svc;
            }
            set
            {
                svc = value;
            }
        }

        /// <summary>
        /// 数据类型
        /// </summary>
        public override SynchroDataType DataType { get; }
        /// <summary>
        /// 数据同步方向
        /// </summary>
        public override SynchroDirection Direction
        {
            get
            {
                return SynchroDirection.ToK3;
            }
        }
        /// <summary>
        /// 数据类型对应的k3cloud的formkey，参照 JNFormIdConst
        /// </summary>
        public abstract string FormKey { get; }
        public abstract FormMetadata Metadata { get; }

        /// <summary>
        /// 单据的编号字段Key（或者基础资料的编码字段Key）
        /// </summary>
        public virtual string BillNoKey
        {
            get
            {
                return "FNumber";
            }
        }

        /// <summary>
        /// 是否自动审核
        /// </summary>
        protected virtual bool AutoAudit
        {
            get
            {
                return true;
            }
        }

        #endregion Var

        /// <summary>
        /// 单个文件时判断是否成功执行-Daniel
        /// </summary>
        public bool IsSuccess { get; set; }
        /// <summary>
        /// 记录同步成功的单据编码
        /// </summary>
        public virtual List<string> SynSuccList
        {
            get
            {
                return new List<string>();
            }
        }
        /// <summary>
        /// 默认组织编码
        /// </summary>
        public string DefaultOrgNumber { get; set; }
        /// <summary>
        /// 总入口：进行同步数据
        /// </summary> 
        public virtual HttpResponseResult DoSynchroData(IEnumerable<string> numbers = null, Dictionary<SynOperationType, IEnumerable<AbsSynchroDataInfo>> datas = null)
        {
            HttpResponseResult result = null;
            try
            {
                this.IsSuccess = true;
                IEnumerable<AbsSynchroDataInfo> sourceDatas = GetSynchroData(numbers);
                Dictionary<SynOperationType, IEnumerable<AbsSynchroDataInfo>> dict = null;

                if (datas == null)
                {
                    if (sourceDatas != null)
                    {
                        dict = EntityDataSource(this.K3CloudContext, sourceDatas);
                    }
                }
                else
                {
                    dict = datas;
                }
                //拆分数据，批次进行操作
                List<AbsSynchroDataInfo> lstBacth = new List<AbsSynchroDataInfo>();
                List<SynchroLog> logs = null;

                if (dict != null && dict.Count > 0)
                {
                    foreach (var item in dict)
                    {
                        if (item.Value != null && item.Value.Count() > 0)
                        {
                            #region

                            if (item.Value.Count() >= batch)
                            {
                                for (int j = 0; j < item.Value.Count(); j++)
                                {
                                    lstBacth.Add(item.Value.ElementAt(j));

                                    if (j > 0 && (j + 1) % batch == 0)
                                    {
                                        if (lstBacth != null && lstBacth.Count > 0)
                                        {
                                            logs = CreateSynchroLog(lstBacth);
                                            result = ExecuteSynchro(lstBacth, logs, item.Key);
                                            lstBacth.Clear();
                                        }
                                    }
                                    else
                                    {
                                        if (j == item.Value.Count() - 1)
                                        {
                                            if (lstBacth != null && lstBacth.Count > 0)
                                            {
                                                logs = CreateSynchroLog(lstBacth);
                                                result = ExecuteSynchro(lstBacth, null, item.Key);
                                                lstBacth.Clear();
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (item.Value != null && item.Value.Count() > 0)
                                {
                                    logs = CreateSynchroLog(item.Value);
                                    result = ExecuteSynchro(item.Value, logs, item.Key);
                                }
                            }

                            #endregion
                        }
                        else
                        {
                            result = new HttpResponseResult();
                            result.Success = false;
                            result.Message = string.Format("{0}同步操作，未找到需要同步的数据！", this.DataType);

                            LogUtils.WriteSynchroLog(this.K3CloudContext, this.DataType, result.Message, true);
                            return result;
                        }
                    }
                }
                else
                {

                    result = new HttpResponseResult();
                    result.Success = false;
                    result.Message = string.Format("{0}同步操作，未找到需要同步的数据！", this.DataType);

                    LogUtils.WriteSynchroLog(this.K3CloudContext, this.DataType, result.Message, true);
                    return result;
                }
                DeleteStopFlag();
            }

            catch (Exception ex)
            {
                DeleteStopFlag();
                this.IsSuccess = false;

                LogUtils.WriteSynchroLog(this.K3CloudContext, this.DataType,
                        "数据同步过程中出现异常，异常信息：" + ex.Message + System.Environment.NewLine + ex.StackTrace);

            }

            return result;
        }

        /// <summary>
        /// 是否取消同步: true 取消同步 ，false 不取消同步
        /// </summary>
        /// <param name="srcData"></param>
        /// <returns></returns>
        public virtual bool IsCancelSynchro(AbsSynchroDataInfo srcData)
        {
            return false;
        }
        /// <summary>
        /// 筛选同步或需要更新的数据
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="srcDatas"></param>
        /// <returns></returns>
        public virtual Dictionary<SynOperationType, IEnumerable<AbsSynchroDataInfo>> EntityDataSource(Context ctx, IEnumerable<AbsSynchroDataInfo> srcDatas)
        {
            return null;
        }
        /// <summary>
        /// 是否终止后续的所有同步（具体作法：外部在数据库插入一条记录标准要终止同步，参照DeleteStopFlag()）
        /// </summary>
        /// <returns></returns>
        public virtual bool IsStopSynchro()
        {
            return false;
        }

        /// <summary>
        /// 删除终止同步标记
        /// </summary>
        public virtual void DeleteStopFlag()
        {
            var sql = string.Format(@" Delete From  t_bas_systemprofile 
                                        where fcategory='synchro' and fkey='{0}' ", this.DataType.ToString());
            DBUtils.Execute(this.K3CloudContext, sql);
        }

        /// <summary>
        /// 获取数据之前及所有数据同步之前的相关操作：如建相关表，如同步物料时先同步物料分组
        /// </summary> 
        public virtual void BeforeDoSynchroData()
        {
            //SynchroDataHelper.CreateNetCtrlTable(this.K3CloudContext, GetNetCtrlTableName);
        }

        /// <summary>
        /// 单条数据同步之前的相关操作：如判断数据是否存在，存在则先反审核等等之类的动作
        /// </summary> 
        public virtual void BeforeDoSynchroDataSign(AbsSynchroDataInfo row, SynchroLog log, out bool cancel)
        {
            cancel = false;
        }

        /// <summary>
        /// 获取需要同步的数据 
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerable<AbsSynchroDataInfo> GetSynchroData(IEnumerable<string> numbers = null);


        /// <summary>
        /// 获取到需要同步的数据之后的相关操作：如重新组织数据，格式转换、前置数据同步之类的
        /// </summary>
        /// <param name="sourceData"></param>
        public virtual void AfterGetSynchroData(List<AbsSynchroDataInfo> sourceData)
        {

        }
        /// <summary>
        /// 数据同步操作完成后（单张数据）
        /// </summary>
        public virtual void AfterDoSynchroData(HttpResponseResult result, SynchroLog log)
        {
            //更新同步日志的提示信息
            //UpdateSynchroLogInfo(result, log);

            //反写中间库（原系统）的数据的同步状态
            WriteBackSrcStatus(log);

            if (log.IsSuccess == 1 && AutoAudit)
            {
                //将数据提交审核
                //AuditData(result.Result, log);
            }

            //记录同步日志
            LogHelper.WriteSynchroLog(this.K3CloudContext, log);
        }

        /// <summary>
        /// 所有数据同步操作完成后：如关闭连接等
        /// </summary> 
        public virtual void FinishDoSynchroData()
        {

        }
        /// <summary>
        /// 将需要同步的数据转换为JSON格式（单个）
        /// </summary>
        /// <param name="sourceData"></param>
        /// <param name="log"></param>
        /// <param name="operationType"></param>
        /// <returns></returns>
        public abstract JObject BuildSynchroDataJson(AbsSynchroDataInfo sourceData, SynchroLog log, SynOperationType operationType);

        /// <summary>
        /// 将需要同步的数据转换为JSON格式（集合）
        /// </summary>
        /// <param name="sourceDatas"></param>
        /// <param name="operationType"></param>
        /// <returns></returns>
        public abstract JObject BuildSynchroDataJsons(IEnumerable<AbsSynchroDataInfo> sourceDatas, SynOperationType operationType);
        /// <summary>
        /// 构建同步至K3的对象
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        public abstract AbsSynchroDataInfo BuildSynchroData(Context ctx, string json, AbsSynchroDataInfo data = null);
        /// <summary>
        /// 执行同步操作
        /// </summary>
        /// <param name="sourceDatas"></param>
        /// <param name="logs"></param>
        /// <param name="operationType"></param>
        /// <returns></returns>
        public abstract HttpResponseResult ExecuteSynchro(IEnumerable<AbsSynchroDataInfo> sourceDatas, List<SynchroLog> logs, SynOperationType operationType);

        /// <summary>
        /// 反写中间库的数据的同步状态
        /// </summary>
        /// <param name="log"></param>
        public virtual void WriteBackSrcStatus(SynchroLog log)
        {

        }
        /// <summary>
        /// 设置同步数据同步的批次
        /// </summary>
        /// <param name="sourceDatas"></param>
        /// <returns></returns>
        public virtual int BatchCount(IEnumerable<AbsSynchroDataInfo> sourceDatas)
        {
            if (sourceDatas != null)
            {
                if (sourceDatas.Count() > 1000)
                {
                    if (sourceDatas.Count() % 1000 == 0)
                    {
                        return sourceDatas.Count() % 1000;
                    }
                    else
                    {
                        return sourceDatas.Count() % 1000 + 1;
                    }
                }
                else
                {
                    return 1;
                }

            }
            return 0;
        }

        /// <summary>
        /// 更新同步后的操作（提交、审核）日志的提示信息
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="result"></param>
        /// <param name="dataType"></param>
        /// <param name="operateType"></param>
        public virtual void SynchroDataLog(Context ctx, HttpResponseResult result, SynchroDataType dataType, SynOperationType operateType)
        {

            if (result != null)
            {
                if (result.SuccessEntityNos != null && result.SuccessEntityNos.Count > 0)
                {
                    string msg = string.Format("单据编码【{0}】{1}成功！", string.Join(",", result.SuccessEntityNos.Select(o => o.ToString())), operateType.ToString());
                    result.Message = msg;
                    LogUtils.WriteSynchroLog(ctx, dataType, msg);
                }
            }
            else
            {
                LogUtils.WriteSynchroLog(ctx, dataType, "" + dataType + "" + operateType + " 失败！");
            }
        }
        #region  Log
        /// <summary>
        /// 创建同步日志
        /// </summary>
        /// <param name="datas"></param>
        /// <returns></returns>
        protected List<SynchroLog> CreateSynchroLog(IEnumerable<AbsSynchroDataInfo> datas)
        {
            List<SynchroLog> logs = null;
            if (datas != null && datas.Count() > 0)
            {
                logs = new List<SynchroLog>();

                foreach (var data in datas)
                {
                    SynchroLog log = new SynchroLog();
                    //log.FileName = data.FileName;
                    log.GroupId = groupId;
                    log.BeginTime = DateTime.Now;
                    log.FDataSourceType = DataType;
                    log.FDataSourceTypeDesc = LogUtils.GetDataSourceTypeDesc(this.DataType);
                    //log.sourceId = data.srcPKId;
                    log.sourceNo = data.SrcNo;
                    log.K3BillNo = "";
                }
            }

            return logs;
        }

        #endregion  Log

        #region 获取默认值的方法，最好是在继承类里面的BeforeDoSynchroData里面调用赋值


        public void GetDefualtOrgNumber()
        {
            QueryBuilderParemeter para = new QueryBuilderParemeter();
            para.FormId = "ORG_Organizations";
            para.SelectItems = SelectorItemInfo.CreateItems("FNumber,FName");
            para.OrderByClauseWihtKey = "FOrgID";

            var k3Data = Kingdee.BOS.App.ServiceHelper.GetService<IQueryService>().GetDynamicObjectCollection(this.K3CloudContext, para);

            if (k3Data == null || k3Data.Count == 0)
            {
                DefaultOrgNumber = "100";
                return;
            }

            DefaultOrgNumber = k3Data[0]["FNumber"].ToString();
        }


        #endregion  defaul value

        public virtual HttpResponseResult ExecuteOperate(SynOperationType operateType, List<string> numbers = null, List<int> pkIds = null, string json = null)
        {
            HttpResponseResult result = null;
           
            if ((numbers != null && numbers.Count > 0) || (pkIds != null && pkIds.Count > 0) || !string.IsNullOrWhiteSpace(json))
            {
                HttpClient httpClient = new HttpClient();
                httpClient.Url = "http://localhost/K3Cloud/Services/SynchroServiceBus.asmx/ExecuteOperate";
                Dictionary<string, string> dict = new Dictionary<string, string>();

                dict.Add("ctx", HttpUtility.UrlEncode(JsonConvert.SerializeObject(this.K3CloudContext), Encoding.UTF8));
                dict.Add("dataType", HttpUtility.UrlEncode(this.DataType.ToString(), Encoding.UTF8));
                dict.Add("operateType", HttpUtility.UrlEncode(operateType.ToString(), Encoding.UTF8));
                dict.Add("formId", HttpUtility.UrlEncode(this.FormKey, Encoding.UTF8));
                dict.Add("numbers", numbers == null ? null : HttpUtility.UrlEncode(JsonConvert.SerializeObject(numbers.ToArray()), Encoding.UTF8));
                dict.Add("pkIds", pkIds == null ? null : HttpUtility.UrlEncode(JsonConvert.SerializeObject(pkIds.ToArray()), Encoding.UTF8));
                dict.Add("json", HttpUtility.UrlEncode(json, Encoding.UTF8));

                StringBuilder sb = new StringBuilder();

                foreach (var item in dict)
                {
                    sb.AppendFormat("&{0}={1}", item.Key, item.Value);
                }

                httpClient.Content = sb.ToString();
                string ret = httpClient.PostData();
               
                result = JsonConvert.DeserializeObject<HttpResponseResult>(ret);
               
                if (result != null)
                {
                    SynchroDataLog(this.K3CloudContext, result, this.DataType, operateType);
                }
            }

            return result;
        }
    }

}