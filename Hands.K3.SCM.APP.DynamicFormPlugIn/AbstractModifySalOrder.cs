using System;
using System.Collections.Generic;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Core.List.PlugIn;
using Newtonsoft.Json.Linq;
using System.Linq;
using Kingdee.BOS.App.Data;
using Hands.K3.SCM.APP.Entity.SynDataObject;
using Hands.K3.SCM.APP.Entity.K3WebApi;
using Hands.K3.SCM.APP.Entity.StructType;

using Hands.K3.SCM.APP.Utils.Utils;
using HS.K3.Common.Abbott;

namespace Hands.K3.SCM.APP.DynamicFormPlugIn
{
    public abstract class AbstractModifySalOrder : AbstractListPlugIn
    {
       
        /// <summary>
        /// 获取选中要操作的销售订单
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public abstract List<K3SalOrderInfo> GetSelectedSalOrders(Context ctx);
        /// <summary>
        /// 判断单据是否有下游单据
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        public virtual bool IsPush(Context ctx, K3SalOrderInfo order)
        {
            DynamicObjectCollection coll = null;

            if (order != null)
            {
                string sql = string.Format(@" /*dialect*/select h.FBILLNO, b.F_HS_UnUSATruePrice, a.F_HS_RateToUSA 
                                                        from T_SAL_DELIVERYNOTICE a 
                                                        inner join T_SAL_DELIVERYNOTICEENTRY b on a.fid=b.fid
                                                        inner join T_SAL_DELIVERYNOTICEENTRY_LK f on b.fentryid=f.fentryid
                                                        inner join T_SAL_OrderEntry g on  f.fsbillID=g.fid and f.fsid=g.fentryID
                                                        inner join T_SAL_Order h on g.fid=h.fid
                                                        where h.FBILLNO='{0}'", order.FBillNo
                                            );
                coll = SQLUtils.GetObjects(ctx, sql);
            }

            return (coll != null && coll.Count > 0) ? true : false;
        }

        /// <summary>
        /// 创建请求参数
        /// </summary>
        /// <param name="orders"></param>
        /// <param name="requestType"></param>
        /// <returns></returns>
        public virtual JArray BuildRequestParams(Context ctx, List<K3SalOrderInfo> orders,string requestType)
        {
            JArray paras = null;
            JObject para = null;

            if (orders != null && orders.Count > 0)
            {
                paras = new JArray();

                foreach (var order in orders)
                {
                    if (order != null)
                    {
                        para = new JObject();

                        para.Add("customers_id", order.F_HS_B2CCustId);
                        para.Add("orders_id", order.FBillNo);
                        para.Add("orders_operate_status", requestType.ToString());

                        paras.Add(para);
                    }
                }
            }

            return paras;
        }

        /// <summary>
        /// 获取元数据
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="formId"></param>
        /// <returns></returns> 
        public virtual BusinessInfo GetBusinessInfo(Context ctx)
        {
            return this.View.BusinessInfo;
        }

        /// <summary>
        /// 获取EntityKey
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public string GetEntityKey(BusinessInfo info)
        {

            if (info != null)
            {
                if (info.Entrys != null && info.Entrys.Count > 0)
                {
                    foreach (var entry in info.Entrys)
                    {
                        if (entry != null)
                        {
                            if (entry.EntryName.CompareTo("SaleOrderEntry") == 0)
                            {
                                return entry.Key;
                            }
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 获取源单单据体数据行
        /// </summary>
        /// <param name="formId"></param>
        /// <returns></returns>
        public abstract DynamicObjectCollection GetAtiveRow();

        /// <summary>
        /// 获取请求操作类型描述
        /// </summary>
        /// <param name="operateType"></param>
        /// <returns></returns>
        public virtual string GetOperateTypeDesc(string requestType)
        {
            if (requestType.Equals(RequestType.COMBINE))
            {
                return "合单请求";
            }
            else if (requestType.Equals(RequestType.MODIFY))
            {
                return "改单请求";
            }
            else if (requestType.Equals(RequestType.LOCK))
            {
                return "锁单请求";
            }

            return null;
        }
        /// <summary>
        /// 获取服务器响应请求操作的状态
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public virtual string GetOperateStatusDesc(HttpResponseResult result)
        {
            if (result != null)
            {
                return result.Success ? "成功" : "失败";
            }

            return null;
        }
        /// <summary>
        /// 获取销售订单发货状态
        /// </summary>
        /// <returns></returns>
        //public abstract string GetShipStatus(Context ctx);
        /// <summary>
        /// 向HC网站发送请求
        /// </summary>
        public virtual List<HttpResponseResult> SendRequest(Context ctx, List<K3SalOrderInfo> orders, string requestType)
        {
            List<HttpResponseResult> results = null;
            HttpResponseResult result = null;
            try
            {
                HttpClient_ client = new HttpClient_();
                
                if (ctx.DBId.CompareTo(DataBaseConst.K3CloudDbId) == 0)
                {
                    client.Url = "https://admin2.healthcabin.net/Staff_AdminIT/response.php?t_method=updateOrderStatus_api";
                }
                else
                {
                    client.Url = "https://admin2.healthcabin.net/Staff_AdminIT/response.php?t_method=updateOrderStatus_api";
                }

                client.Content = string.Concat("&ERP=", BuildRequestParams(ctx, orders, requestType).ToString());
                string response = client.PostData();
                return GetResponse(ctx, response);
            }
            catch (Exception ex)
            {
                results = new List<HttpResponseResult>();
                result = new HttpResponseResult();
                result.Success = false;
                result.Message = "请求服务器发生异常：" + System.Environment.NewLine + ex.Message + System.Environment.NewLine + ex.StackTrace;

                results.Add(result);
                LogUtils.WriteSynchroLog(ctx, SynchroDataType.SaleOrder, "请求服务器发生异常：" + System.Environment.NewLine + ex.Message + System.Environment.NewLine + ex.StackTrace);

                return results;
            }
        }

        /// <summary>
        /// 获取HC网站响应结果
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public virtual List<HttpResponseResult> GetResponse(Context ctx, string result)
        {
            List<HttpResponseResult> responses = null;
            HttpResponseResult response = null;

            if (!string.IsNullOrWhiteSpace(result))
            {
                try
                {
                    JObject obj = JObject.Parse(result);

                    if (obj != null)
                    {
                        JArray jArr = JArray.Parse(JsonUtils.GetFieldValue(obj, "data"));

                        if (jArr != null && jArr.Count > 0)
                        {
                            responses = new List<HttpResponseResult>();

                            foreach (var jObj in jArr)
                            {
                                if (jObj != null)
                                {
                                    response = new HttpResponseResult();
                                    response.Source = JsonUtils.GetFieldValue(jObj, "orders_id");
                                    response.Success = JsonUtils.GetFieldValue(jObj, "status").CompareTo("success") == 0 ? true : false;
                                    response.Message = JsonUtils.GetFieldValue(jObj, "remark");

                                    responses.Add(response);
                                }
                            }
                        }
                    }

                    return responses;
                }
                catch (Exception ex)
                {
                    LogUtils.WriteSynchroLog(ctx, SynchroDataType.SaleOrder, "解析响应信息JSON出现异常：【" + result + "】" + System.Environment.NewLine + ex.Message + System.Environment.NewLine + ex.StackTrace);
                }
            }

            return responses;
        }

        /// <summary>
        /// 执行改单或合单操作请求
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="operateType"></param>
        public virtual void ExecutOperate(Context ctx, string requestType)
        {
            List<K3SalOrderInfo> orders = GetSelectedSalOrders(ctx);
            List<K3SalOrderInfo> needSendorders = IsCanSendRequest(ctx, orders,requestType) == null ? new List<K3SalOrderInfo>(): IsCanSendRequest(ctx, orders, requestType);
            List<K3SalOrderInfo> failOrders = null;

            if (orders != null && needSendorders != null)
            {
                failOrders = orders.Except(needSendorders).ToList();
            }
            
            if (needSendorders != null && needSendorders.Count > 0)
            {
                if (requestType.CompareTo(RequestType.MODIFY) == 0)
                {
                    List<HttpResponseResult> results = SendRequest(ctx, needSendorders, requestType);
                    this.View.ShowMessage(ResponseMessage(results, requestType));
                    SuspendedSalOrder(ctx, results);
                }
                else if (requestType.CompareTo(RequestType.COMBINE) == 0)
                {
                    List<K3SalOrderInfo> combineOrders = IsPropertiesSame(needSendorders);

                    if (combineOrders != null && combineOrders.Count > 0)
                    {
                        List<HttpResponseResult> results = SendRequest(ctx, combineOrders, requestType);
                        this.View.ShowMessage(ResponseMessage(results, requestType));
                        SuspendedSalOrder(ctx, results);     
                    }
                    else
                    {
                        this.View.ShowErrMessage("销售订单国家，客户，币别不一致，请检查");   
                    }
                }
            }
            if(failOrders != null && failOrders.Count > 0)
            {
                this.View.ShowErrMessage("", JudgeSalOrderStatus(ctx, failOrders), MessageBoxType.Error);
            }
            
        }

        /// <summary>
        /// 改单或合单请求成功挂起对应的销售订单
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="results"></param>
        /// <returns></returns>
        private int SuspendedSalOrder(Context ctx, List<HttpResponseResult> results)
        {
            int count = 0;
            string filter = string.Empty;

            if (results != null && results.Count > 0)
            {
                var group = from r in results
                            where r.Success == true
                            select r;
                if (group != null && group.Count() > 0)
                {
                    try
                    {
                        filter = string.Join("','", group.Select(o => o.Source));
                        string sql = string.Format(@" /*dialect*/ update T_SAL_ORDER set F_HS_HANG = '{0}' where FBillNo in ('{1}')","1", filter);

                        count = DBUtils.Execute(ctx, sql);
                    }
                    catch (Exception ex)
                    {
                        LogUtils.WriteSynchroLog(ctx, SynchroDataType.SaleOrder, "更新销售订单挂起状态发生异常：单号【" + filter + "】" + System.Environment.NewLine + ex.Message + System.Environment.NewLine + ex.StackTrace);
                    }
                }
            }

            return count;
        }

        private string ResponseMessage(List<HttpResponseResult> results, string requestType)
        {
            string message = string.Empty;

            if (results != null && results.Count > 0)
            {
                var succ = from r in results
                           where r.Success == true
                           select r;

                List<HttpResponseResult> success = succ.ToList();
                List<HttpResponseResult> fail = results.Except(success).ToList();

                if (success != null && success.Count > 0)
                {
                    message = "销售订单【" + string.Join(",", success.Select(o => o.Source)) + "】" + GetOperateTypeDesc(requestType) + "成功！";
                    
                    message += System.Environment.NewLine;
                }
                if (fail != null && fail.Count > 0)
                {
                    foreach (var m in fail)
                    {
                        if (m != null)
                        {
                            message += m.Message;
                        }
                       
                    }
                    message += "销售订单【" + string.Join(",", fail.Select(o => o.Source)) + "】" + GetOperateTypeDesc(requestType) + "失败！";
                }
                
            }

            return message;
        }

        /// <summary>
        /// 判断是否可以向服务器发送请求
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="orders"></param>
        /// <returns></returns>
        private List<K3SalOrderInfo> IsCanSendRequest(Context ctx, List<K3SalOrderInfo> orders, string requestType)
        {
            HashSet<bool> flags = new HashSet<bool>();
            List<K3SalOrderInfo> canOrders = null;

            if (orders != null && orders.Count > 0)
            {
                canOrders = new List<K3SalOrderInfo>();

                foreach (var order in orders)
                {
                    if (order != null)
                    {
                        bool flag = order.F_HS_PaymentStatus.CompareTo("2") == 0 && IsPush(ctx, order) == false
                             && (order.FDocumentStatus.CompareTo(BillDocumentStatus.Create) == 0 || order.FDocumentStatus.CompareTo(BillDocumentStatus.ReAudit) == 0 
                             ) && order.FCancelStatus.CompareTo(BillCancelStatus.Cancel) != 0 && order.FCancelStatus.CompareTo(BillCloseStatus.Close) != 0;

                        if (flag == true)
                        {
                            if (requestType.CompareTo(RequestType.MODIFY) == 0 || requestType.CompareTo(RequestType.LOCK) == 0)
                            {
                                canOrders.Add(order);
                            }
                        }
                        else
                        {
                            if (requestType.CompareTo(RequestType.COMBINE) == 0)
                            {
                                return null;
                            }
                        }

                        flags.Add(flag);

                    }
                }

                if (flags != null)
                {
                    if (flags.Count == 1 && flags.ElementAt(0))
                    {
                        if (requestType.CompareTo(RequestType.COMBINE) == 0)
                        {
                            return orders;
                        }
                    }    
                }
            }

            return canOrders;
        }
        private List<K3SalOrderInfo> IsPropertiesSame(List<K3SalOrderInfo> orders)
        {
            if (orders != null && orders.Count >= 2)
            {
                var group = from o in orders
                            group o by new { o.F_HS_B2CCustId, o.F_HS_RecipientCountry, o.FSettleCurrId } into g
                            select g;

                if (group.Count() == 1)
                {
                    return orders;
                }
            }

            return null;
        }

        /// <summary>
        /// 判断销售订单状态
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="orders"></param>
        /// <returns></returns>
        private string JudgeSalOrderStatus(Context ctx,List<K3SalOrderInfo> orders)
        {
            string message = string.Empty;

            if (orders != null && orders.Count > 0)
            {
                for (int i = 0; i < orders.Count; i++)
                {
                    K3SalOrderInfo order = orders[i];

                    if (order != null)
                    {
                        if (i < orders.Count - 1)
                        {
                            if (order.FDocumentStatus.CompareTo(BillDocumentStatus.Auditing) == 0)
                            {
                                message += "销售订单【" + order.FBillNo + "】状态为审核中，请先撤销" + System.Environment.NewLine;
                            }
                            if (order.FDocumentStatus.CompareTo(BillDocumentStatus.Audit) == 0)
                            {
                                if (IsPush(ctx, order))
                                {
                                    message += "销售订单【" + order.FBillNo + "】有下游单据，请先删除下游单据" + System.Environment.NewLine;
                                }
                                else
                                {
                                   message += "销售订单【" + order.FBillNo + "】状态为已审核，请先反审核" + System.Environment.NewLine;
                                }
                            }
                            if (order.FCloseStatus.CompareTo(BillCloseStatus.Close) == 0)
                            {
                                message += "销售订单【" + order.FBillNo + "】状态为已关闭，不能参与改单或合单" + System.Environment.NewLine;
                            }
                            if (order.FCancelStatus.CompareTo(BillCancelStatus.Cancel) == 0)
                            {
                                message += "销售订单【" + order.FBillNo + "】状态为已作废，不能参与改单或合单" + System.Environment.NewLine;
                            }
                            if (order.F_HS_PaymentStatus.CompareTo("3") == 0 || order.F_HS_PaymentStatus.CompareTo("1") == 0)
                            {
                                message += "销售订单【" + order.FBillNo + "】付款状态为已付款！" + System.Environment.NewLine;
                            }
                        }
                        else if (i == orders.Count - 1)
                        {
                            if (order.FDocumentStatus.CompareTo(BillDocumentStatus.Auditing) == 0)
                            {
                                message += "销售订单【" + order.FBillNo + "】状态为审核中，请先撤销";
                            }
                            if (order.FDocumentStatus.CompareTo(BillDocumentStatus.Audit) == 0)
                            {
                                if (IsPush(ctx, order))
                                {
                                    message += "销售订单【" + order.FBillNo + "】有下游单据，请先删除下游单据";
                                }
                                else
                                {
                                    message += "销售订单【" + order.FBillNo + "】状态为已审核，请先反审核";
                                }
                            }
                            if (order.FCloseStatus.CompareTo(BillCloseStatus.Close) == 0)
                            {
                                message += "销售订单【" + order.FBillNo + "】状态为已关闭，不能参与改单或合单" ;
                            }
                            if (order.FCancelStatus.CompareTo(BillCancelStatus.Cancel) == 0)
                            {
                                message += "销售订单【" + order.FBillNo + "】状态为已作废，不能参与改单或合单" ;
                            }
                            if (order.F_HS_PaymentStatus.CompareTo("3") == 0 || order.F_HS_PaymentStatus.CompareTo("1") == 0)
                            {
                                message += "销售订单【" + order.FBillNo + "】付款状态为已付款！" + System.Environment.NewLine;
                            }
                        }  
                    }
                    
                }
            }
            return message;
        }
    }
}
