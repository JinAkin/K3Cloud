using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.BOS.Core.Metadata;
using Newtonsoft.Json.Linq;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS;
using Kingdee.BOS.App.Data;

using Hands.K3.SCM.APP.Entity.StructType;
using Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject;
using Hands.K3.SCM.APP.Entity.SynDataObject;
using Hands.K3.SCM.APP.Entity.CommonObject;
using Hands.K3.SCM.APP.Entity.K3WebApi;
using Hands.K3.SCM.APP.Utils.Utils;
using Kingdee.BOS.Orm.DataEntity;
using Hands.K3.SCM.App.Synchro.Base.Abstract;
using HS.K3.Common.Abbott;
using Hands.K3.SCM.APP.Entity.EnumType;

namespace Hands.K3.SCM.App.Core.SynchroService.ToK3
{
    public class SynReceiveBillToK3 : AbstractSynchroToK3
    {

        public override SynchroDataType DataType
        {
            get
            {
                return SynchroDataType.ReceiveBill;
            }
        }

        public override string FormKey
        {
            get
            {
                return HSFormIdConst.AR_RECEIVEBILL;
            }
        }

        public override FormMetadata Metadata
        {
            get
            {
                return MetaDataServiceHelper.Load(this.K3CloudContext, this.FormKey) as FormMetadata;
            }
        }
        public override Dictionary<SynOperationType, IEnumerable<AbsSynchroDataInfo>> EntityDataSource(Context ctx, IEnumerable<AbsSynchroDataInfo> srcDatas)
        {
            Dictionary<SynOperationType, IEnumerable<AbsSynchroDataInfo>> dict = null;

            if (srcDatas != null && srcDatas.Count() > 0)
            {
                dict = new Dictionary<SynOperationType, IEnumerable<AbsSynchroDataInfo>>();
                dict.Add(SynOperationType.SAVE, srcDatas);
            }

            return dict;
        }
        public override JObject BuildSynchroDataJson(AbsSynchroDataInfo sourceData, SynchroLog log, SynOperationType operationType)
        {
            return null;
        }

        public override JObject BuildSynchroDataJsons(IEnumerable<AbsSynchroDataInfo> sourceDatas, SynOperationType operationType)
        {
            JObject root = null;
            JArray model = null;
            JObject baseData = null;

            if (sourceDatas != null && sourceDatas.Count() > 0)
            {
                root = new JObject();
                model = new JArray();

                root.Add("NeedUpDateFields", new JArray(""));
                root.Add("NeedReturnFields", new JArray("FBillNo"));
                root.Add("IsDeleteEntry", "false");
                root.Add("SubSystemId", "");
                root.Add("IsVerifyBaseDataField", "true");

                if (sourceDatas.GetType() == typeof(List<K3SalOrderInfo>) || sourceDatas.ElementAt(0) as K3SalOrderInfo is K3SalOrderInfo)
                {
                    List<K3SalOrderInfo> orders = sourceDatas.Select(o => (K3SalOrderInfo)o).ToList();

                    foreach (var order in orders)
                    {
                        if (order != null)
                        {
                            if (IsExistSaleOrderNo(order.FBillNo))
                            {
                                baseData = new JObject();

                                JObject FBillTypeID = new JObject();//单据类型
                                FBillTypeID.Add("FNumber", "SKDLX01_SYS");
                                baseData.Add("FBillTypeID", FBillTypeID);

                                baseData.Add("FDATE", DateTime.Now);
                                baseData.Add("FPAYUNITTYPE", "BD_Customer");//付款单位类型

                                baseData.Add("FCONTACTUNITTYPE", "BD_Customer");//往来单位类型

                                JObject FCONTACTUNIT = new JObject();//往来单位
                                FCONTACTUNIT.Add("FNumber", order.FCustId);
                                baseData.Add("FCONTACTUNIT", FCONTACTUNIT);

                                JObject FPAYUNIT = new JObject();//付款单位
                                FPAYUNIT.Add("FNumber", order.FCustId);
                                baseData.Add("FPAYUNIT", FPAYUNIT);

                                JObject F_HS_B2CCUSTID = new JObject();//客户真实ID
                                F_HS_B2CCUSTID.Add("FNumber", order.F_HS_B2CCustId);
                                baseData.Add("F_HS_B2CCUSTID", F_HS_B2CCUSTID);

                                if (Convert.ToDecimal(order.F_HS_PayTotal) > 0)
                                {
                                    if (!string.IsNullOrWhiteSpace(order.FSettleCurrId))
                                    {
                                        if (order.FSettleCurrId.CompareTo("JPY") == 0)
                                        {
                                            baseData.Add("F_HS_FinancialReceived", Math.Truncate(Convert.ToDecimal(order.F_HS_PayTotal) * order.F_HS_RateToUSA));//财务实收金额
                                        }
                                        else
                                        {
                                            baseData.Add("F_HS_FinancialReceived", Convert.ToDecimal(order.F_HS_PayTotal) * order.F_HS_RateToUSA);//财务实收金额
                                        }
                                    }
                                }
                                if (string.IsNullOrWhiteSpace(order.F_HS_PayTotal))
                                {
                                    if (order.FSettleCurrId.CompareTo("JPY") == 0)
                                    {
                                        baseData.Add("F_HS_FinancialReceived", Math.Truncate(order.F_HS_Total));//财务实收金额
                                    }
                                    else
                                    {
                                        baseData.Add("F_HS_FinancialReceived", order.F_HS_Total);//财务实收金额
                                    }
                                }

                                JObject FSALEERID = new JObject();//销售员
                                FSALEERID.Add("FNumber", order.FSalerId);
                                baseData.Add("FSALEERID", FSALEERID);

                                JObject FPAYORGID = new JObject();//收款组织
                                FPAYORGID.Add("FNumber", order.FSaleOrgId);
                                baseData.Add("FPAYORGID", FPAYORGID);

                                JObject FSETTLEORGID = new JObject();//结算组织
                                FSETTLEORGID.Add("FNumber", order.FSaleOrgId);
                                baseData.Add("FSETTLEORGID", FSETTLEORGID);

                                JObject FSALEORGID = new JObject();//销售组织
                                FSALEORGID.Add("FNumber", order.FSaleOrgId);
                                baseData.Add("FSALEORGID", FSALEORGID);

                                JObject FSALEDEPTID = new JObject();//销售部门
                                FSALEDEPTID.Add("FNumber", order.FSaleDeptId);
                                baseData.Add("FSALEDEPTID", FSALEDEPTID);

                                JObject FCURRENCYID = new JObject();//币别
                                FCURRENCYID.Add("FNumber", order.FSettleCurrId);
                                baseData.Add("FCURRENCYID", FCURRENCYID);

                                baseData.Add("FRECEIVEBILLENTRY", BuildReceiveBillEntry(K3CloudContext, order));//收款明细

                                model.Add(baseData);
                            }
                        }
                        else
                        {
                            LogUtils.WriteSynchroLog(K3CloudContext, this.DataType, "【" + order.FBillNo + "】对应的收款单已经生成！");
                        }
                    }
                }

                else if (sourceDatas.GetType() == typeof(List<AbsDataInfo>) || sourceDatas.ElementAt(0) as AbsDataInfo is AbsDataInfo)
                {
                    List<AbsDataInfo> infos = sourceDatas.Select(s => (AbsDataInfo)s).ToList();
                    string msg = string.Empty;

                    if (infos != null && infos.Count > 0)
                    {
                        foreach (var info in infos)
                        {
                            if (info != null)
                            {
                                if (!IsGenerateReceiveBill(info.FBillNo))
                                {
                                    baseData = new JObject();

                                    JObject FBillTypeID = new JObject();//单据类型
                                    FBillTypeID.Add("FNumber", "SKDLX01_SYS");
                                    baseData.Add("FBillTypeID", FBillTypeID);

                                    baseData.Add("FDATE", DateTime.Now);
                                    baseData.Add("FPAYUNITTYPE", "BD_Customer");//付款单位类型

                                    baseData.Add("FCONTACTUNITTYPE", "BD_Customer");//往来单位类型

                                    JObject FCONTACTUNIT = new JObject();//往来单位
                                    FCONTACTUNIT.Add("FNumber", info.F_HS_B2CCustId);
                                    baseData.Add("FCONTACTUNIT", FCONTACTUNIT);

                                    JObject FPAYUNIT = new JObject();//付款单位
                                    FPAYUNIT.Add("FNumber", info.F_HS_B2CCustId);
                                    baseData.Add("FPAYUNIT", FPAYUNIT);

                                    JObject F_HS_B2CCUSTID = new JObject();//客户真实ID
                                    F_HS_B2CCUSTID.Add("FNumber", info.F_HS_B2CCustId);
                                    baseData.Add("F_HS_B2CCUSTID", F_HS_B2CCUSTID);
                                    baseData.Add("F_HS_LinkReceiptRefundBillNo", info.FBillNo);

                                    if (Convert.ToDecimal(info.FRealAmountFor) > 0)
                                    {
                                        baseData.Add("F_HS_FinancialReceived", info.FRealAmountFor);//财务实收金额
                                    }

                                    JObject FSALEERID = new JObject();//销售员
                                    FSALEERID.Add("FNumber", info.FSalerId);
                                    baseData.Add("FSALEERID", FSALEERID);

                                    JObject FPAYORGID = new JObject();//收款组织
                                    FPAYORGID.Add("FNumber", info.FSaleOrgId);
                                    baseData.Add("FPAYORGID", FPAYORGID);

                                    JObject FSETTLEORGID = new JObject();//结算组织
                                    FSETTLEORGID.Add("FNumber", info.FSaleOrgId);
                                    baseData.Add("FSETTLEORGID", FSETTLEORGID);

                                    JObject FSALEORGID = new JObject();//销售组织
                                    FSALEORGID.Add("FNumber", info.FSaleOrgId);
                                    baseData.Add("FSALEORGID", FSALEORGID);

                                    JObject FSALEDEPTID = new JObject();//销售部门
                                    FSALEDEPTID.Add("FNumber", info.FSaleDeptId);
                                    baseData.Add("FSALEDEPTID", FSALEDEPTID);

                                    JObject FCURRENCYID = new JObject();//币别
                                    FCURRENCYID.Add("FNumber", info.FSettleCurrId);
                                    baseData.Add("FCURRENCYID", FCURRENCYID);

                                    baseData.Add("FRECEIVEBILLENTRY", BuildReceiveBillEntry(K3CloudContext, info));//收款明细

                                    model.Add(baseData);
                                }
                                else
                                {
                                    LogUtils.WriteSynchroLog(K3CloudContext, this.DataType, "【" + info.FBillNo + "】已经生成收款单【" + info.F_HS_BalanceReceivableNo + "】，不能再次生成！");
                                    msg += string.Format("收款退款单【{0}】已经生成收款单【{1}】，不能再次生成！", info.FBillNo, info.F_HS_BalanceReceivableNo) + Environment.NewLine;
                                }

                            } 
                        }
                    }
                }
                root.Add("Model", model);
            }

            return root;
        }

        private JArray BuildBillSkdRecEntry(K3SalOrderInfo order)
        {
            JArray FBILLSKDRECENTRY = null;

            if (order != null)
            {

            }

            return FBILLSKDRECENTRY;
        }

        /// <summary>
        /// 收款明细
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private JArray BuildReceiveBillEntry(Context ctx, AbsSynchroDataInfo data)
        {
            JArray FRECEIVEBILLENTRY = null;
            JObject baseData = null;

            if (data != null)
            {
                if (data.GetType() == typeof(K3SalOrderInfo))
                {
                    K3SalOrderInfo order = data as K3SalOrderInfo;

                    FRECEIVEBILLENTRY = new JArray();
                    baseData = new JObject();

                    JObject FSETTLETYPEID = new JObject();//结算方式"JSFS01_SYS"
                    FSETTLETYPEID.Add("FNumber", SQLUtils.GetSettleTypeNo(K3CloudContext, SQLUtils.GetPaymentMethodId(K3CloudContext, order.F_HS_PaymentModeNew)));
                    baseData.Add("FSETTLETYPEID", FSETTLETYPEID);

                    JObject FPURPOSEID = new JObject();//收款用途
                    FPURPOSEID.Add("FNumber", "SFKYT02_SYS");
                    baseData.Add("FPURPOSEID", FPURPOSEID);

                    baseData.Add("FRECEIVEITEMTYPE", "1");//预收项目类型
                    baseData.Add("FRECEIVEITEM", order.FBillNo);//销售订单
                    baseData.Add("FSaleOrderID", SQLUtils.GetSaleOrderId(K3CloudContext, order.FBillNo));//销售订单内码


                    if (Convert.ToDecimal(order.F_HS_PayTotal) > 0)
                    {
                        if (order.FSettleCurrId.CompareTo("JPY") == 0)
                        {
                            baseData.Add("FRECTOTALAMOUNTFOR", Math.Truncate(Convert.ToDecimal(order.F_HS_PayTotal) * order.F_HS_RateToUSA));//财务实收金额
                        }
                        else
                        {
                            baseData.Add("FRECTOTALAMOUNTFOR", Convert.ToDecimal(order.F_HS_PayTotal) * order.F_HS_RateToUSA);//财务实收金额
                        }
                    }
                    else if (string.IsNullOrWhiteSpace(order.F_HS_PayTotal))
                    {
                        if (order.FSettleCurrId.CompareTo("JPY") == 0)
                        {
                            baseData.Add("FRECTOTALAMOUNTFOR", Math.Truncate(order.F_HS_Total));//财务实收金额
                        }
                        else
                        {
                            baseData.Add("FRECTOTALAMOUNTFOR", order.F_HS_Total);//财务实收金额
                        }
                    }

                    JObject FACCOUNTID = new JObject();//我方银行账号
                    FACCOUNTID.Add("FNumber", SQLUtils.GetBankAccountNo(K3CloudContext, SQLUtils.GetBankAccountId(K3CloudContext, order.F_HS_PaymentModeNew)));
                    baseData.Add("FACCOUNTID", FACCOUNTID);

                    baseData.Add("FSETTLENO", order.F_HS_TransactionID.Trim());//结算号

                    FRECEIVEBILLENTRY.Add(baseData);
                }
                else if (data.GetType() == typeof(AbsDataInfo))
                {
                    AbsDataInfo info = data as AbsDataInfo;

                    FRECEIVEBILLENTRY = new JArray();
                    baseData = new JObject();

                    JObject FSETTLETYPEID = new JObject();//结算方式"JSFS01_SYS"
                    FSETTLETYPEID.Add("FNumber", info.FSettleTypeId);
                    baseData.Add("FSETTLETYPEID", FSETTLETYPEID);

                    JObject FPURPOSEID = new JObject();//收款用途
                    FPURPOSEID.Add("FNumber", "SFKYT02_SYS");
                    baseData.Add("FPURPOSEID", FPURPOSEID);

                    if (Convert.ToDecimal(info.FRealAmountFor) > 0)
                    {
                        baseData.Add("FRECTOTALAMOUNTFOR", info.FRealAmountFor);//财务实收金额
                    }

                    JObject FACCOUNTID = new JObject();//我方银行账号
                    FACCOUNTID.Add("FNumber", SQLUtils.GetBankAccountNo(K3CloudContext, SQLUtils.GetBankAccountId(K3CloudContext, info.FSettleTypeId)));
                    baseData.Add("FACCOUNTID", FACCOUNTID);

                    baseData.Add("FSETTLENO", "");//结算号
                    baseData.Add("F_HS_YNRecharge", false);
                    baseData.Add("F_HS_SynchronizedRecharge", false);

                    FRECEIVEBILLENTRY.Add(baseData);
                }
            }

            return FRECEIVEBILLENTRY;
        }

        /// <summary>
        /// 执行同步操作
        /// </summary>
        /// <param name="sourceDatas"></param>
        /// <param name="logs"></param>
        /// <param name="operationType"></param>
        /// <returns></returns>
        public override HttpResponseResult ExecuteSynchro(IEnumerable<AbsSynchroDataInfo> sourceDatas, List<SynchroLog> logs, SynOperationType operationType)
        {

            HttpResponseResult result = null;
            JObject bizData = BuildSynchroDataJsons(sourceDatas, operationType);

            KDTransactionScope trans = null;
            HttpResponseResult saveRet = null;
            HttpResponseResult sumbitRet = null;
            HttpResponseResult auditRet = null;

            if (sourceDatas == null || sourceDatas.Count() == 0)
            {
                result = new HttpResponseResult();
                result.Success = false;
                result.Message = "没有需要同步的数据！";
            }

            try
            {
                if (operationType == SynOperationType.SAVE)
                {
                    #region

                    try
                    {
                        #region
                        using (trans = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
                        {
                            //单据保存 
                            saveRet = ExecuteOperate(SynOperationType.SAVE, null, null, bizData.ToString());
                            result = saveRet;

                            if (saveRet != null && saveRet.SuccessEntityNos != null && saveRet.SuccessEntityNos.Count > 0)
                            {
                                result.Message = "收款单【" + string.Join(",", saveRet.SuccessEntityNos.Select(o => o.ToString())) + "】" + SynOperationType.SAVE + "成功！";
                                //单据提交
                                sumbitRet = ExecuteOperate(SynOperationType.SUBMIT, saveRet.SuccessEntityNos);
                                result = sumbitRet;

                                if (sumbitRet != null && sumbitRet.SuccessEntityNos != null && sumbitRet.SuccessEntityNos.Count > 0)
                                {
                                    auditRet = ExecuteOperate(SynOperationType.AUDIT, sumbitRet.SuccessEntityNos);
                                    result = auditRet;
                                    result.Message = "收款单【" + string.Join(",", sumbitRet.SuccessEntityNos.Select(o => o.ToString())) + "】" + SynOperationType.SUBMIT + "成功！";
                                    //单据审核后

                                    if (auditRet != null && auditRet.SuccessEntityNos != null && auditRet.SuccessEntityNos.Count > 0)
                                    {
                                        result.Message = "收款单【" + string.Join(",", auditRet.SuccessEntityNos.Select(o => o.ToString())) + "】" + SynOperationType.AUDIT + "成功！";
                                    }
                                }
                            }
                            trans.Complete();
                        }
                        //更新收款单对应单据字段的信息
                        UpdateAfterOperate(K3CloudContext, saveRet, sourceDatas, SynOperationType.SAVE);
                        //更新收款单对应单据字段的信息
                        UpdateAfterOperate(K3CloudContext, sumbitRet, sourceDatas, SynOperationType.SUBMIT);
                        //更新收款单对应单据字段的信息
                        UpdateAfterOperate(K3CloudContext, auditRet, sourceDatas, SynOperationType.AUDIT);

                        #endregion
                    }
                    catch (Exception ex)
                    {
                        LogUtils.WriteSynchroLog(K3CloudContext, this.DataType, "同步收款单，异常信息：" + ex.Message + System.Environment.NewLine + ex.StackTrace);
                    }
                    finally
                    {
                        if (trans != null)
                        {
                            trans.Dispose();
                        }
                    }
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

            }

            return null;
        }

        /// <summary>
        ///  收款单保存成功后，更新对应单据字段信息
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="result"></param>
        /// <param name="sourceDatas"></param>
        /// <param name="operType"></param>
        /// <returns></returns>
        private int UpdateAfterOperate(Context ctx, HttpResponseResult result, IEnumerable<AbsSynchroDataInfo> sourceDatas, SynOperationType operType)
        {
            int count = 0;
            string sql = string.Empty;

            if (result != null)
            {
                try
                {
                    if (result.SuccessEntityNos != null && result.SuccessEntityNos.Count > 0)
                    {
                        if (sourceDatas.GetType() == typeof(List<K3SalOrderInfo>) || sourceDatas.ElementAt(0) as K3SalOrderInfo is K3SalOrderInfo)
                        {
                            string filterNos = string.Join("','", result.SuccessEntityNos.Select(o => o.ToString()));

                            sql += string.Format(@"/*dialect*/ update a set F_HS_YNSyncCollection = '{0}'
                                                            from T_SAL_ORDER a
                                                            inner join T_AR_RECEIVEBILLENTRY b on a.FBILLNO = b.FRECEIVEITEM
                                                            inner join T_AR_RECEIVEBILL c on b.FID = c.FID
                                                            where c.FBILLNO in ('{1}')
                                                            and a.FDOCUMENTSTATUS != 'D' and a.FDOCUMENTSTATUS != 'B'", "1", filterNos) + Environment.NewLine;

                        }
                        else if (sourceDatas.GetType() == typeof(List<AbsDataInfo>) || sourceDatas.ElementAt(0) as AbsDataInfo is AbsDataInfo)
                        {
                            List<AbsDataInfo> infos = sourceDatas.Select(o => (AbsDataInfo)o).ToList();

                            foreach (var number in result.SuccessEntityNos)
                            {
                                if (number != null)
                                {
                                    if (operType == SynOperationType.SAVE)
                                    {
                                        sql += string.Format(@"/*dialect*/ update a set F_HS_BalanceReceivableNo = '{0}'
                                                                      from T_AR_REFUNDBILL a
                                                                      inner join T_AR_RECEIVEBILL b on b.F_HS_LinkReceiptRefundBillNo = a.FBillNo
                                                                      where b.FBILLNO = '{1}' ", number, number) + Environment.NewLine;

                                        sql += string.Format(@"/*dialect*/update  a set a.FCREATORID = {0}
                                                                    from T_AR_RECEIVEBILL a
                                                                    where a.FBILLNO = '{1}'", K3CloudContext.UserId, number) + Environment.NewLine;
                                    }
                                    else if (operType == SynOperationType.SUBMIT)
                                    {
                                        sql += string.Format(@"/*dialect*/update  a set a.FMODIFIERID = {0}
                                                                    from T_AR_RECEIVEBILL a
                                                                    where a.FBILLNO = '{1}'", K3CloudContext.UserId, number) + Environment.NewLine;
                                    }
                                    else if (operType == SynOperationType.AUDIT)
                                    {
                                        sql += string.Format(@"/*dialect*/update  a set a.FAPPROVERID = {0}
                                                                    from T_AR_RECEIVEBILL a
                                                                    where a.FBILLNO = '{1}'", K3CloudContext.UserId, number) + Environment.NewLine;

                                    }
                                }
                            }
                        }

                        count = DBUtils.Execute(K3CloudContext, sql);
                    }

                }
                catch (Exception ex)
                {
                    LogUtils.WriteSynchroLog(K3CloudContext, SynchroDataType.SaleOrder, "收款单【" + string.Join(",", result.SuccessEntityNos.Select(o => o.ToString())) + "】对应的销售订单更新已同步收款单状态出现异常：" + System.Environment.NewLine + ex.Message + System.Environment.NewLine + ex.Message);
                }
            }

            return count;
        }
        /// <summary>
        /// 判断收款单是否已经关联了了销售订单
        /// </summary>
        /// <param name="saleOrderNo"></param>
        /// <returns></returns>
        private bool IsExistSaleOrderNo(string saleOrderNo)
        {
            if (!string.IsNullOrWhiteSpace(saleOrderNo))
            {
                string sql = string.Format(@"/*dialect*/ select FRECEIVEITEM from T_AR_RECEIVEBILLENTRY a 
                                                        inner join T_AR_RECEIVEBILL b on a.FID = b.FID
                                                        inner join T_SAL_ORDER c on c.FBILLNO = a.FRECEIVEITEM
                                                        where FRECEIVEITEM = '{0}'", saleOrderNo);
                return string.IsNullOrWhiteSpace(JsonUtils.ConvertObjectToString(SQLUtils.GetObject(K3CloudContext, sql, "FRECEIVEITEM")));
            }
            return false;
        }

        /// <summary>
        /// 判断收款退款单是否已经生成了收款单
        /// </summary>
        /// <param name="billNo"></param>
        /// <returns></returns>
        private bool IsGenerateReceiveBill(string billNo)
        {
            if (!string.IsNullOrWhiteSpace(billNo))
            {
                string sql = string.Format(@"/*dialect*/ select F_HS_BalanceReceivableNo from T_AR_REFUNDBILL where F_HS_BalanceReceivableNo = '{0}'",billNo);
                DynamicObjectCollection coll = SQLUtils.GetObjects(this.K3CloudContext,sql);

                if (coll == null || coll.Count == 0)
                {
                    return false;
                }
            }
            return true;
        }
        /// <summary>
        /// 获取未生成收款单的销售订单(定时任务)
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        private List<K3SalOrderInfo> GetNonSynRevSalOrder(Context ctx)
        {
            List<K3SalOrderInfo> orders = null;
            K3SalOrderInfo order = null;

            string sql = string.Format(@"/*dialect*/ select distinct FBillNo,FDate,j.FNUMBER  as FCUSTID,j.FNUMBER  as F_HS_B2CCustId,m.FNUMBER as FSalerId,n.FNUMBER as FSALEDEPTID,z.FNUMBER as FSaleOrgId,o.FNumber as FSettleCurrId,F_HS_RateToUSA,a.F_HS_TransactionID,F_HS_PayTotal,q.FNUMBER as F_HS_PaymentModeNew
                                                    from T_SAL_ORDER a 
                                                    inner join T_SAL_ORDERENTRY b on b.FID = a.FID
                                                    inner join T_SAL_ORDERENTRY_F c on c.FENTRYID = b.FENTRYID and c.FID = b.FID
                                                    inner join T_SAL_ORDERFIN d on d.FID = a.FID
                                                    inner join T_BD_CUSTOMER e on e.FCUSTID= a.FCUSTID
                                                    inner join T_BAS_ASSISTANTDATAENTRY_L f ON a.F_HS_SaleOrderSource=f.FENTRYID
                                                    inner join T_BAS_ASSISTANTDATAENTRY g ON f.FentryID=g.FentryID
                                                    left join T_BAS_BILLTYPE h on a.FBILLTypeID=h.FBILLTypeID
													inner join T_ORG_ORGANIZATIONS z on a.FSALEORGID=z.FORGID
													inner join T_BD_CUSTOMER j on j.FCUSTID = a.F_HS_B2CCUSTID
													inner join T_BD_CURRENCY o on o.FCURRENCYID = d.FSettleCurrId
													inner join T_BAS_ASSISTANTDATAENTRY_L p ON a.F_HS_PaymentModeNew=p.FENTRYID
                                                    inner join T_BAS_ASSISTANTDATAENTRY q ON q.FentryID=p.FentryID
													inner join V_BD_SALESMAN m on m.FID = a.FSALERID
													inner join T_BD_DEPARTMENT n on n.FDEPTID = m.FDEPTID
                                                    where a.FDOCUMENTSTATUS = 'C'and a.FsaleOrgID = 100035 and a.FCANCELSTATUS<>'B' and h.FNUMBER='XSDD01_SYS' and z.FNUMBER='100.01' and g.FNUMBER = 'HCWebProcessingOder'
													and a.FCREATEDATE >= DATEADD(MONTH,-1,GETDATE())
                                                    and a.fbillno not in 
													(select l.FRECEIVEITEM from T_AR_RECEIVEBILL k
													inner join  T_AR_RECEIVEBILLENTRY l on l.FID = K.FID
													where k.FCREATEDATE >= DATEADD(MONTH,-1,GETDATE())
													and l.FRECEIVEITEM not like 'SO%' and l.FRECEIVEITEM <> '')");

            DynamicObjectCollection coll = SQLUtils.GetObjects(K3CloudContext, sql);

            if (coll != null && coll.Count > 0)
            {
                orders = new List<K3SalOrderInfo>();

                foreach (var item in coll)
                {
                    if (item != null)
                    {
                        order = new K3SalOrderInfo();

                        order.FBillNo = SQLUtils.GetFieldValue(item, "FBillNo");
                        order.FDate = Convert.ToDateTime(SQLUtils.GetFieldValue(item, "FDate"));
                        order.FCustId = SQLUtils.GetFieldValue(item, "FCUSTID");
                        order.F_HS_B2CCustId = SQLUtils.GetFieldValue(item, "FCUSTID");
                        order.FSalerId = SQLUtils.GetFieldValue(item, "FSalerId");
                        order.FSaleDeptId = SQLUtils.GetFieldValue(item, "FSALEDEPTID");
                        order.FSaleOrgId = SQLUtils.GetFieldValue(item, "FSaleOrgId");
                        order.FSettleCurrId = SQLUtils.GetFieldValue(item, "FSettleCurrId");
                        order.F_HS_RateToUSA = Convert.ToDecimal(SQLUtils.GetFieldValue(item, "F_HS_RateToUSA"));
                        order.F_HS_TransactionID = SQLUtils.GetFieldValue(item, "F_HS_TransactionID");
                        order.F_HS_PayTotal = SQLUtils.GetFieldValue(item, "F_HS_PayTotal");
                        order.F_HS_PaymentModeNew = SQLUtils.GetFieldValue(item, "F_HS_PaymentModeNew");

                        orders.Add(order);
                    }
                }
            }

            return orders;
        }

        public HttpResponseResult SynReceiveBill(Context ctx)
        {
            IEnumerable<AbsSynchroDataInfo> datas = GetNonSynRevSalOrder(ctx);

            if (datas != null)
            {
                return ExecuteSynchro(datas.ToList(), null, SynOperationType.SAVE);
            }
            return null;
        }

        public override IEnumerable<AbsSynchroDataInfo> GetSynchroData(IEnumerable<string> numbers = null)
        {
            return GetNonSynRevSalOrder(this.K3CloudContext);
        }
        public override AbsSynchroDataInfo BuildSynchroData(Context ctx, string json, AbsSynchroDataInfo data = null)
        {
            return null;
        }
    }
}
