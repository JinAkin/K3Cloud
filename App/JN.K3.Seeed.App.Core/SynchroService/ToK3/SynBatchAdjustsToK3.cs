using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hands.K3.SCM.App.Synchro.Base.Abstract;
using Hands.K3.SCM.APP.Entity.CommonObject;
using Hands.K3.SCM.APP.Entity.EnumType;
using Hands.K3.SCM.APP.Entity.K3WebApi;
using Hands.K3.SCM.APP.Entity.StructType;
using Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject;
using Hands.K3.SCM.APP.Entity.SynDataObject.BacthAdjust;
using Hands.K3.SCM.APP.Utils.Utils;
using HS.K3.Common.Abbott;
using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.ServiceHelper;
using Newtonsoft.Json.Linq;

namespace Hands.K3.SCM.App.Core.SynchroService.ToK3
{
    public class SynBatchAdjustsToK3 : AbstractSynchroToK3
    {
        public override SynchroDataType DataType
        {
            get
            {
                return SynchroDataType.BatchAdjust;
            }
        }

        public override string FormKey
        {
            get
            {
                return HSFormIdConst.Sal_BatchAdjust;
            }
        }

        public override FormMetadata Metadata
        {
            get
            {
                return MetaDataServiceHelper.Load(this.K3CloudContext, this.FormKey) as FormMetadata;
            }
        }

        public override AbsSynchroDataInfo BuildSynchroData(Context ctx, string json, AbsSynchroDataInfo data = null)
        {
            if (data != null)
            {
                return data;
            }
            return null;
        }

        public override JObject BuildSynchroDataJson(AbsSynchroDataInfo sourceData, SynchroLog log, SynOperationType operationType)
        {
            JObject baseData = null;

            if (sourceData != null)
            {
                K3BatchAdjust just = sourceData as K3BatchAdjust;

                if (just != null)
                {
                    baseData = new JObject();

                    baseData.Add("FName",just.FName);
                    baseData.Add("FDate", DateTime.Now);

                    JObject FSaleOrgId = new JObject();
                    FSaleOrgId.Add("FNumber", "100.01");
                    baseData.Add("FSaleOrgId", FSaleOrgId);

                    baseData.Add("FBATCHADJUSTENTRY", BuildBatchAdjustEntry(sourceData));
                }
            }
            return baseData;
        }

        public override JObject BuildSynchroDataJsons(IEnumerable<AbsSynchroDataInfo> sourceDatas, SynOperationType operationType)
        {
            JObject root = null;
            JArray model = null;
           

            if (sourceDatas != null && sourceDatas.Count() > 0)
            {
                root = new JObject();
                model = new JArray();

                root.Add("NeedUpDateFields", new JArray(""));
                root.Add("IsDeleteEntry", "false");
                root.Add("SubSystemId", "");
                root.Add("IsVerifyBaseDataField", "true");

                foreach (var data in sourceDatas)
                {
                    if (data != null)
                    {
                        model.Add(BuildSynchroDataJson(data,null,SynOperationType.SAVE));
                    }
                }

                root.Add("Model", model);
            }

            return root;
        }
        private JArray BuildBatchAdjustEntry(AbsSynchroDataInfo data)
        {
            JArray model = null;
            JObject baseData = null;

            if (data != null)
            {
                K3BatchAdjust just = data as K3BatchAdjust;

                if (just != null)
                {
                    model = new JArray();

                    if (just.Entry != null && just.Entry.Count > 0)
                    {
                        foreach (var item in just.Entry)
                        {
                            if (item != null)
                            {
                                baseData = new JObject();

                                baseData.Add("FAdjustType", item.FAdjustType);

                                JObject FPriceListId = new JObject();//价目表
                                FPriceListId.Add("FNUMBER", item.FPriceListId);
                                baseData.Add("FPriceListId", FPriceListId);

                                JObject FCurrencyId = new JObject();//币别
                                FCurrencyId.Add("FNUMBER", "USD");
                                baseData.Add("FCurrencyId", FCurrencyId);

                                baseData.Add("FPriceObject", "A");
                                baseData.Add("FIsIncludedTax", "true");

                                JObject FMaterialId = new JObject();
                                FMaterialId.Add("FNUMBER", item.FMaterialId);
                                baseData.Add("FMaterialId", FMaterialId);

                                JObject FMatUnitId = new JObject();
                                FMatUnitId.Add("FNUMBER", item.FMatUnitId);
                                baseData.Add("FMatUnitId", FMatUnitId);

                                baseData.Add("FBeforePrice", item.FBeforePrice);
                                baseData.Add("FAfterPrice", item.FAfterPrice);
                                baseData.Add("F_HS_BeforeUSPrice", item.F_HS_BeforeUSPrice);
                                baseData.Add("F_HS_BeforeUSNoPostagePrice",item.F_HS_BeforeUSNoPostagePrice);

                                baseData.Add("F_HS_AfterUSPrice", item.F_HS_AfterUSPrice);
                                baseData.Add("F_HS_AfterUSNoPostagePrice",item.F_HS_AfterUSNoPostagePrice);
                                baseData.Add("F_HS_BeforeEUPrice", 0);
                                baseData.Add("F_HS_AfterEUPrice", item.F_HS_AfterEUPrice);

                                baseData.Add("F_HS_BeforeAUPrice", item.F_HS_BeforeAUPrice);
                                baseData.Add("F_HS_BeforeAUNoPostagePrice",item.F_HS_BeforeAUNoPostagePrice);
                                baseData.Add("F_HS_AfterAUPrice", item.F_HS_AfterAUPrice);
                                baseData.Add("F_HS_AfterAUNoPostagePrice",item.F_HS_AfterAUNoPostagePrice);

                                baseData.Add("F_HS_BeforeJPNoPostagePrice", item.F_HS_BeforeJPNoPostagePrice);
                                baseData.Add("F_HS_AfterJPNoPostagePrice", item.F_HS_AfterJPNoPostagePrice);
                                baseData.Add("F_HS_BeforeKRNoPostagePrice", item.F_HS_BeforeKRNoPostagePrice);
                                baseData.Add("F_HS_AfterKRNoPostagePrice", item.F_HS_AfterKRNoPostagePrice);

                                baseData.Add("F_HS_BeforeUKNoPostagePrice", item.F_HS_BeforeUKNoPostagePrice);
                                baseData.Add("F_HS_AfterUKNoPostagePrice", item.F_HS_AfterUKNoPostagePrice);
                                baseData.Add("F_HS_BeforeDENoPostagePrice", item.F_HS_BeforeDENoPostagePrice);
                                baseData.Add("F_HS_AfterDENoPostagePrice", item.F_HS_AfterDENoPostagePrice);
                                baseData.Add("F_HS_BeforeFRNoPostagePrice", item.F_HS_BeforeFRNoPostagePrice);
                                baseData.Add("F_HS_AfterFRNoPostagePrice", item.F_HS_AfterFRNoPostagePrice);

                                baseData.Add("F_HS_BeforeEUNoPostagePrice", item.F_HS_BeforeEUNoPostagePrice);
                                baseData.Add("F_HS_AfterEUNoPostagePrice", item.F_HS_AfterEUNoPostagePrice);

                                baseData.Add("FBeforeEffDate", item.FBeforeEffDate);
                                baseData.Add("FAfterEffDate", item.FAfterEffDate);
                                baseData.Add("FBeforeUnEffDate", item.FBeforeUnEffDate);
                                baseData.Add("FAfterUnEffDate", item.FAfterUnEffDate);
                 
                                baseData.Add("FUnEffective", "");

                                model.Add(baseData);
                            }
                        }
                    }
                }
            }

            return model;
        }

        private int UpdateAfterOperate(Context ctx, HttpResponseResult result, SynOperationType operType)
        {
            int count = 0;
            string sql = string.Empty;

            if (result != null)
            {
                try
                {

                    if (result.SuccessEntityNos != null && result.SuccessEntityNos.Count > 0)
                    {
                        foreach (var number in result.SuccessEntityNos)
                        {
                            if (number != null)
                            {
                                if (operType == SynOperationType.SAVE)
                                {
                                    sql += string.Format(@"/*dialect*/ update T_SAL_BATCHADJUST set FCREATORID = '{0}'
                                                                   where FBILLNO = '{1}' ", ctx.UserId, number) + Environment.NewLine;

                                }
                                else if (operType == SynOperationType.SUBMIT)
                                {
                                    sql += string.Format(@"/*dialect*/update T_SAL_BATCHADJUST set FMODIFIERID = '{0}'
                                                                  where FBILLNO = '{1}'", ctx.UserId, number) + Environment.NewLine;
                                }
                                else if (operType == SynOperationType.AUDIT)
                                {
                                    sql += string.Format(@"/*dialect*/update T_SAL_BATCHADJUST set FAPPROVERID = '{0}'
                                                                  where FBILLNO = '{1}'", ctx.UserId, number) + Environment.NewLine;

                                }
                            }

                            count = DBUtils.Execute(ctx, sql);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogUtils.WriteSynchroLog(ctx, SynchroDataType.SaleOrder, "收款单【" + string.Join(",", result.SuccessEntityNos.Select(o => o.ToString())) + "】对应的销售订单更新已同步收款单状态出现异常：" + System.Environment.NewLine + ex.Message + System.Environment.NewLine + ex.Message);
                }
            }

            return count;
        }
        public override HttpResponseResult ExecuteSynchro(IEnumerable<AbsSynchroDataInfo> sourceDatas, List<SynchroLog> logs, SynOperationType operationType)
        {
            HttpResponseResult result = null;
            HttpResponseResult saveR = null;
            HttpResponseResult submitR = null;
            HttpResponseResult auditR = null;
            KDTransactionScope trans = null;
            JObject bizData = null;


            if (sourceDatas != null && sourceDatas.Count() > 0)
            {
                bizData = BuildSynchroDataJsons(sourceDatas, operationType);
            }
            else
            {
                result = new HttpResponseResult();
                result.Success = false;
                result.Message = "没有需要保存的数据！";
                return result;
            }
            try
            {
                if (operationType == SynOperationType.SAVE)
                {
                    #region
                    using (trans = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
                    {
                        #region
                        //单据保存 
                        result = ExecuteOperate(SynOperationType.SAVE, null, null, bizData.ToString());
                        saveR = result;

                        if (result != null && result.SuccessEntityNos != null && result.SuccessEntityNos.Count > 0)
                        {
                            result.Message = this.DataType + "【" + string.Join(",", result.SuccessEntityNos.Select(o => o.ToString())) + "】" + SynOperationType.SAVE + " 成功！";
                            
                            //单据提交
                            result = ExecuteOperate(SynOperationType.SUBMIT, result.SuccessEntityNos, null, null);
                            submitR = result;

                            if (result != null && result.SuccessEntityNos != null && result.SuccessEntityNos.Count > 0)
                            {
                                result.Message = this.DataType + "【" + string.Join(",", result.SuccessEntityNos.Select(o => o.ToString())) + "】" + SynOperationType.SUBMIT + " 成功！";
                                //单据审核
                                result = ExecuteOperate(SynOperationType.AUDIT, result.SuccessEntityNos, null, null);
                                result.Message = this.DataType + "【" + string.Join(",", result.SuccessEntityNos.Select(o => o.ToString())) + "】" + SynOperationType.AUDIT + " 成功！";
                                auditR = result;
                            }
                        }

                        //提交事务
                        trans.Complete();

                        #endregion
                    }

                    UpdateAfterOperate(this.K3CloudContext, saveR, SynOperationType.SAVE);
                    UpdateAfterOperate(this.K3CloudContext, submitR, SynOperationType.SUBMIT);
                    UpdateAfterOperate(this.K3CloudContext, auditR, SynOperationType.AUDIT);

                    #endregion

                }
                return result;
            }
            catch (Exception ex)
            {
                if (logs != null && logs.Count > 0)
                {
                    foreach (var log in logs)
                    {
                        log.IsSuccess = 0;
                        log.ErrInfor = ex.Message + System.Environment.NewLine + ex.StackTrace;
                    }
                }

                LogUtils.WriteSynchroLog(this.K3CloudContext, this.DataType, ex.Message + System.Environment.NewLine + ex.StackTrace);
            }
            finally
            {
                if (trans != null)
                {
                    trans.Dispose();
                }
            }
            if (result == null)
            {
                return null;
            }

            if (result.Success == false && result.FailedResult == null && result.Result == null)
            {
                //同步出现错误之类：如令牌错误，url错误之类的
                if (logs != null && logs.Count > 0)
                {
                    foreach (var log in logs)
                    {
                        log.IsSuccess = 0;
                        log.ErrInfor = "数据同步失败：" + result.Message == null ? "" : result.Message;
                    }
                }

                return result;
            }

            return result;
        }

        public override IEnumerable<AbsSynchroDataInfo> GetSynchroData(IEnumerable<string> numbers = null)
        {
            return null;
        }
    }
}
