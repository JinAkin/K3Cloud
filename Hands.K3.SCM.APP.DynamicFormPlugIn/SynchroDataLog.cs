using Kingdee.BOS.Core.DynamicForm.PlugIn;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Hands.K3.SCM.APP.Entity.CommonObject;
using Kingdee.BOS;
using Kingdee.BOS.Orm.DataEntity;
using Hands.K3.SCM.APP.Utils;

using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Orm.Metadata.DataEntity;
using Kingdee.BOS.Core.Metadata.EntityElement;
using System.Collections;
using Newtonsoft.Json;
using Hands.K3.SCM.APP.Utils.Utils;
using HS.K3.Common.Abbott;

namespace Hands.K3.SCM.APP.DynamicFormPlugIn
{
    [Description("同步日志--动态表单插件")]
    public class SynchroDataLog : AbstractDynamicFormPlugIn
    {
        private StringBuilder sql = new StringBuilder();
        private string initSql = string.Empty;
       
        public void LoadData(Context ctx)
        {
            string dataType = Convert.ToString(this.View.Model.GetValue("F_HS_SynDataType"));
            string beginTime = Convert.ToString(this.View.Model.GetValue("F_HS_SynchroDateTime"));
            string endTime = Convert.ToString(this.View.Model.GetValue("F_HS_SynchroDateTime1"));

            sql.AppendLine(string.Format(@"/*dialect*/ SELECT  TOP 1000 FDataSourceType,FDataSourceId,FBILLNO,FSynchroTime,FIsSuccess,FErrInfor,FDataSourceTypeDesc,b.FNAME as FHSOperateId
                                                       FROM HS_T_SynchroLog a
                                                       INNER JOIN T_SEC_user b on a.FHSOperateId = b.FUSERID  
                                       "));

            if (!string.IsNullOrWhiteSpace(dataType))
            {
                sql.AppendLine(string.Format(@" where FDataSourceType = '{0}'", GetSynchroDataType(dataType)));

                if (!string.IsNullOrWhiteSpace(beginTime))
                {
                    if (!string.IsNullOrWhiteSpace(endTime))
                    {
                        sql.AppendLine(string.Format(@" and FSynchroTime between '{0}' and '{1}'", beginTime, endTime = endTime.Replace("00:00:00", "23:59:59")));
                    }
                    else
                    {
                        sql.AppendLine(string.Format(@" and FSynchroTime between '{0}' and '{1}'", beginTime, beginTime = beginTime.Replace("00:00:00", "23:59:59")));
                    }
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(beginTime))
                {
                    if (!string.IsNullOrWhiteSpace(endTime))
                    {
                        sql.AppendLine(string.Format(@" where FSynchroTime between '{0}' and '{1}'", beginTime, endTime = endTime.Replace("00:00:00", "23:59:59")));
                    }
                    else
                    {
                        sql.AppendLine(string.Format(@" where FSynchroTime between '{0}' and '{1}'", beginTime, beginTime = beginTime.Replace("00:00:00", "23:59:59")));
                    }
                }
            }

            sql.AppendLine(string.Format(@" order by  FSynchroTime desc"));

            try
            {
                BindDataToView(ctx,this.View,sql.ToString(), "F_HS_Entity");
                sql.Clear();
            }
            catch (Exception ex)
            {
                this.View.ShowErrMessage(ex.Message + Environment.NewLine + ex.StackTrace, "操作出错了！", MessageBoxType.Error);
            }
        }

        public void BindDataToView(Context ctx, IDynamicFormView formView,string sql,string entryName)
        {
            List<SynchroLog> logs = GetSynchroLogDatas(ctx,sql);
            if (!string.IsNullOrWhiteSpace(entryName))
            {
                formView.Model.DeleteEntryData(entryName);

                if (logs != null && logs.Count > 0)
                {
                    for (int i = 0; i < logs.Count; i++)
                    {
                        if (logs[i] != null)
                        {
                            formView.Model.InsertEntryRow(entryName, i);
                            formView.Model.SetValue("F_HS_DataSourceType", logs[i].FDataSourceType, i);
                            formView.Model.SetValue("F_HS_DataSourceId", logs[i].sourceId, i);

                            formView.Model.SetValue("F_HS_BILLNO", logs[i].K3BillNo, i);
                            formView.Model.SetValue("F_HS_SynchroTime", logs[i].BeginTime, i);
                            formView.Model.SetValue("F_HS_IsSuccess", logs[i].IsSuccess, i);

                            formView.Model.SetValue("F_HS_ErrInfor", logs[i].ErrInfor, i);
                            formView.Model.SetValue("F_HS_DataSourceTypeDesc", logs[i].FDataSourceTypeDesc, i);
                            formView.Model.SetValue("F_HS_OperateId", logs[i].FOperateId, i);
                        }
                    }

                    formView.UpdateView(entryName);
                }
                else
                {
                    this.View.ShowErrMessage("", "没有需要查询的数据，请检查！", MessageBoxType.Error);
                }
            }
        }
        public DynamicObjectCollection GetObjects(Context ctx,string sql)
        {
            return SQLUtils.GetObjects(ctx, sql);
        }
        public List<SynchroLog> GetSynchroLogDatas(Context ctx,string sql)
        {
            List<SynchroLog> logs = null;
            SynchroLog log = null;

            DynamicObjectCollection coll = GetObjects(ctx,sql);

            if (coll != null && coll.Count > 0)
            {
                logs = new List<SynchroLog>();

                foreach (var item in coll)
                {
                    if (item != null)
                    {
                        log = new SynchroLog();

                        log.FDataSourceType = (SynchroDataType)Enum.Parse(typeof(SynchroDataType), SQLUtils.GetFieldValue(item, "FDataSourceType"));
                        log.sourceId = SQLUtils.GetFieldValue(item, "FDataSourceId");
                        log.K3BillNo = SQLUtils.GetFieldValue(item, "FBILLNO");
                        log.BeginTime = Convert.ToDateTime(SQLUtils.GetFieldValue(item, "FSynchroTime"));
                        log.IsSuccess = Convert.ToInt32(SQLUtils.GetFieldValue(item, "IsSuccess"));
                        log.ErrInfor = SQLUtils.GetFieldValue(item, "FErrInfor");
                        log.FDataSourceTypeDesc = SQLUtils.GetFieldValue(item, "FDataSourceTypeDesc");
                        log.FOperateId = SQLUtils.GetFieldValue(item, "FHSOperateId");

                        logs.Add(log);
                    }
                }
            }

            return logs;   
        }

        private SynchroDataType GetSynchroDataType(string enumValue)
        {
            if (!string.IsNullOrWhiteSpace(enumValue))
            {
                switch (enumValue)
                {
                    case "0":
                        return SynchroDataType.SaleOrder;
                    case "1":
                        return SynchroDataType.Customer;
                    case "2":
                        return SynchroDataType.CustomerAddress;
                    case "3":
                        return SynchroDataType.DelCustomerAddress;
                    case "4":
                        return SynchroDataType.CustomerOrderQty;
                    case "5":
                        return SynchroDataType.DeliveryNoticeBill;
                    case "6":
                        return SynchroDataType.ReceiveBill;
                    case "7":
                        return SynchroDataType.DeductIntegral;
                    case "8":
                        return SynchroDataType.SalesOrderPayStatus;
                    case "9":
                        return SynchroDataType.Inventroy;
                    case "10":
                        return SynchroDataType.SaleOrderOffline;
                    case "11":
                        return SynchroDataType.ReFundBill;
                    case "12":
                        return SynchroDataType.BatchAdjust;
                    case "13":
                        return SynchroDataType.Material;
                }
            }
            return default(SynchroDataType);
        }
        public override void OnInitialize(InitializeEventArgs e)
        {
            base.OnInitialize(e);
            initSql = string.Format(@"/*dialect*/ SELECT  TOP 1000 FDataSourceType,FDataSourceId,FBILLNO,FSynchroTime,FIsSuccess,FErrInfor,FDataSourceTypeDesc,b.FNAME as FHSOperateId
                                                  FROM HS_T_SynchroLog a
                                                  INNER JOIN T_SEC_user b on a.FHSOperateId = b.FUSERID
                                                  order by  FSynchroTime desc");
        }

        public override void CreateNewData(BizDataEventArgs e)
        {
            base.CreateNewData(e);

            DynamicObjectCollection logs = GetObjects(this.Context,initSql);
            DynamicObjectType dtType = this.View.BusinessInfo.GetDynamicObjectType();
            EntryEntity entity = (EntryEntity)this.View.BusinessInfo.GetEntity("F_HS_Entity");
            DynamicObject objData = new DynamicObject(dtType);
            DynamicObject entityObj = null;

            if (logs != null && logs.Count > 0)
            {
                int seq = 1;
                foreach (var log in logs)
                {
                    if (log != null)
                    {
                        entityObj = new DynamicObject(entity.DynamicObjectType);
                        entity.DynamicProperty.GetValue<DynamicObjectCollection>(objData).Add(entityObj);

                        entityObj["seq"] = seq;
                        entityObj["F_HS_DataSourceType"] = log["FDataSourceType"];
                        entityObj["F_HS_DataSourceId"] = log["FDataSourceId"];
                        entityObj["F_HS_BILLNO"] = log["FBILLNO"];
                        entityObj["F_HS_SynchroTime"] = log["FSynchroTime"];
                        entityObj["F_HS_IsSuccess"] = log["FIsSuccess"];
                        entityObj["F_HS_ErrInfor"] = log["FErrInfor"];

                        entityObj["F_HS_DataSourceTypeDesc"] = log["FDataSourceTypeDesc"];
                        entityObj["F_HS_OperateId"] = log["FHSOperateId"];
                        seq++;
                    }
                }

                e.BizDataObject = objData;
            }
        }

        public override void ButtonClick(ButtonClickEventArgs e)
        {
            base.ButtonClick(e);

            switch (e.Key)
            {
                case "F_HS_QUERY":
                    LoadData(this.Context);
                    break;
            }
        }
    }

}
