using Hands.K3.SCM.APP.Entity.CommonObject;
using Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject;
using Hands.K3.SCM.APP.Utils;
using Hands.K3.SCM.APP.Utils.Utils;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Orm.Metadata.DataEntity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.APP.DynamicFormPlugIn
{
    [Description("客户余额支付日志--动态表单插件")]
    public class CustomerBalance : AbstractDynamicFormPlugIn
    {
        private StringBuilder sql = new StringBuilder();
        private string initSql = string.Empty;

        public void LoadData(Context ctx)
        {
            string billNo = Convert.ToString(this.View.Model.GetValue("F_HS_BillNumber"));
            string changeType = Convert.ToString(this.View.Model.GetValue("F_HS_ChangeType"));

            DynamicObject cust = this.View.Model.GetValue("F_HS_CustNo") as DynamicObject;
            string custNo = SQLUtils.GetFieldValue(cust, "Number");

            DynamicObject curr = this.View.Model.GetValue("F_HS_Currency") as DynamicObject;
            string currNo = SQLUtils.GetFieldValue(curr,"Number");

            string createTime = Convert.ToString(this.View.Model.GetValue("F_HS_CreateDate"));
            string endTime = Convert.ToString(this.View.Model.GetValue("F_HS_EndDate"));

            sql.AppendLine(string.Format(@"/*dialect*/  select top 1000 F_HS_UseOrgId,F_HS_B2CCUSTID,changedAmount,changedType,F_HS_TradeType,changedCause,balanceAmount
	                                                    ,F_HS_RateToUSA,FSETTLECURRID,changedAmountUSA,balanceAmountUSA,a.F_HS_CNYBalance,updateTime,updateUser,FBillNo
	                                                    ,fentryID,needfreezed,remark
                                                         from HS_T_customerBalance a
                                                         inner join T_BD_CURRENCY b on b.FCURRENCYID = a.FSETTLECURRID
                                                         inner join T_SEC_user c on c.FUSERID = a.updateUser
                                                         inner join T_BD_CUSTOMER d on d.FCUSTID = a.F_HS_B2CCUSTID
                                                         
                                       "));
            if (!string.IsNullOrWhiteSpace(billNo))
            {
                sql.Append(System.Environment.NewLine + string.Format(@"where a.FBillNo = '{0}'", billNo));

                if (!string.IsNullOrWhiteSpace(changeType))
                {
                    sql.Append(Environment.NewLine + string.Format(@"and changedType = '{0}'", changeType));
                }
                if (!string.IsNullOrWhiteSpace(custNo))
                {
                    sql.Append(Environment.NewLine + string.Format(@"and d.FNumber = '{0}'", custNo));
                }
                if (!string.IsNullOrWhiteSpace(currNo))
                {
                    sql.Append(Environment.NewLine + string.Format(@"and b.FNumber = '{0}'", currNo));
                }
                if (!string.IsNullOrWhiteSpace(createTime))
                {
                    if (!string.IsNullOrWhiteSpace(endTime))
                    {
                        sql.Append(Environment.NewLine + string.Format(@"and a.updateTime between '{0}' and '{1}'", createTime, endTime = endTime.Replace("00:00:00", "23:59:59")));
                    }
                    else
                    {
                        sql.Append(Environment.NewLine + string.Format(@"and a.updateTime between '{0}' and '{1}'", createTime, createTime = createTime.Replace("00:00:00", "23:59:59")));
                    }
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(changeType))
                {
                    sql.Append(Environment.NewLine + string.Format(@"where changedType = '{0}'", changeType));

                    if (!string.IsNullOrWhiteSpace(custNo))
                    {
                        sql.Append(Environment.NewLine + string.Format(@"and d.FNumber = '{0}'", custNo));
                    }
                    if (!string.IsNullOrWhiteSpace(currNo))
                    {
                        sql.Append(Environment.NewLine + string.Format(@"and b.FNumber = '{0}'", currNo));
                    }
                    if (!string.IsNullOrWhiteSpace(createTime))
                    {
                        if (!string.IsNullOrWhiteSpace(endTime))
                        {
                            sql.Append(Environment.NewLine + string.Format(@"and a.updateTime between '{0}' and '{1}'", createTime, endTime = endTime.Replace("00:00:00", "23:59:59")));
                        }
                        else
                        {
                            sql.Append(Environment.NewLine + string.Format(@"and a.updateTime between '{0}' and '{1}'", createTime, createTime = createTime.Replace("00:00:00", "23:59:59")));
                        }
                    }
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(custNo))
                    {
                        sql.Append(Environment.NewLine+string.Format(@"where d.FNumber = '{0}'",custNo));
                        if (!string.IsNullOrWhiteSpace(currNo))
                        {
                            sql.Append(Environment.NewLine + string.Format(@"and b.FNumber = '{0}'", currNo));
                        }
                        if (!string.IsNullOrWhiteSpace(createTime))
                        {
                            if (!string.IsNullOrWhiteSpace(endTime))
                            {
                                sql.Append(Environment.NewLine + string.Format(@"and a.updateTime between '{0}' and '{1}'", createTime, endTime = endTime.Replace("00:00:00", "23:59:59")));
                            }
                            else
                            {
                                sql.Append(Environment.NewLine + string.Format(@"and a.updateTime between '{0}' and '{1}'", createTime, createTime = createTime.Replace("00:00:00", "23:59:59")));
                            }
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(currNo))
                        {
                            sql.Append(Environment.NewLine + string.Format(@"where b.FNumber = '{0}'", currNo));

                            if (!string.IsNullOrWhiteSpace(createTime))
                            {
                                if (!string.IsNullOrWhiteSpace(endTime))
                                {
                                    sql.Append(Environment.NewLine + string.Format(@"and a.updateTime between '{0}' and '{1}'", createTime, endTime = endTime.Replace("00:00:00", "23:59:59")));
                                }
                                else
                                {
                                    sql.Append(Environment.NewLine + string.Format(@"and a.updateTime between '{0}' and '{1}'", createTime, createTime = createTime.Replace("00:00:00", "23:59:59")));
                                }
                            }
                            else
                            {
                                if (!string.IsNullOrWhiteSpace(endTime))
                                {
                                    sql.Append(Environment.NewLine + string.Format(@"and a.updateTime between '{0}' and '{1}'", endTime, endTime = endTime.Replace("00:00:00", "23:59:59")));
                                }
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(createTime))
                            {
                                if (!string.IsNullOrWhiteSpace(endTime))
                                {
                                    sql.Append(Environment.NewLine + string.Format(@"where a.updateTime between '{0}' and '{1}'", createTime, endTime = endTime.Replace("00:00:00", "23:59:59")));
                                }
                                else
                                {
                                    sql.Append(Environment.NewLine + string.Format(@"where a.updateTime between '{0}' and '{1}'", createTime, createTime = createTime.Replace("00:00:00", "23:59:59")));
                                }
                            }
                            else
                            {
                                if (!string.IsNullOrWhiteSpace(endTime))
                                {
                                    sql.Append(Environment.NewLine + string.Format(@"where a.updateTime between '{0}' and '{1}'", endTime, endTime = endTime.Replace("00:00:00", "23:59:59")));
                                }
                            }
                        }
                    }
                }
            }

            sql.Append(Environment.NewLine + string.Format(@"order by updateTime desc"));

            try
            {
                BindDataToView(ctx,this.View,sql.ToString(), "F_HS_Entity");
                sql.Clear();
            }
            catch (Exception ex)
            {
                this.View.ShowErrMessage(ex.ToString(), "操作出错了！"+Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace, MessageBoxType.Error);
            }

        }

        public void BindDataToView(Context ctx, IDynamicFormView formView, string sql, string entryName)
        {
            DynamicObjectCollection coll = GetObjects(ctx, sql);
            int i = 0;

            if (!string.IsNullOrWhiteSpace(entryName))
            {
                formView.Model.DeleteEntryData(entryName);

                if (coll != null && coll.Count > 0)
                {
                    foreach (var item in coll)
                    {
                        if (item != null)
                        {
                            formView.Model.InsertEntryRow(entryName, i);
                            if (!string.IsNullOrWhiteSpace(SQLUtils.GetFieldValue(item, "F_HS_UseOrgId")))
                            {
                                formView.Model.SetItemValueByID("F_HS_UseOrgId", SQLUtils.GetFieldValue(item, "F_HS_UseOrgId"), i);
                            }
                            else
                            {
                                formView.Model.SetItemValueByID("F_HS_UseOrgId", "100035", i);
                            }
                            formView.Model.SetItemValueByID("F_HS_Customer", SQLUtils.GetFieldValue(item, "F_HS_B2CCUSTID"), i);
                            //formView.Model.SetValue("F_HS_Customer", SQLUtils.GetFieldValue(item, "F_HS_B2CCUSTID"), i);
                            formView.Model.SetValue("F_HS_ChangeAmount", SQLUtils.GetFieldValue(item, "changedAmount"), i);

                            formView.Model.SetValue("F_HS_TradeType", SQLUtils.GetFieldValue(item, "F_HS_TradeType"), i);
                            formView.Model.SetValue("F_HS_ChangedType", GetChangeTypeDesc(SQLUtils.GetFieldValue(item, "changedType")), i);
                            formView.Model.SetValue("F_HS_ChangedCause", GetChangeCauseDesc(SQLUtils.GetFieldValue(item, "changedCause")), i);
                            formView.Model.SetValue("F_HS_BalanceAmount", SQLUtils.GetFieldValue(item, "balanceAmount"), i);
                            formView.Model.SetValue("F_HS_RateToUSA", SQLUtils.GetFieldValue(item, "F_HS_RateToUSA"), i);

                            //formView.Model.SetValue("F_HS_SettleCurrId", SQLUtils.GetFieldValue(item, "FSETTLECURRID"), i);
                            formView.Model.SetItemValueByID("F_HS_SettleCurrId", SQLUtils.GetFieldValue(item, "FSETTLECURRID"), i);

                            formView.Model.SetValue("F_HS_ChangedAmountUSD", SQLUtils.GetFieldValue(item, "changedAmountUSA"), i);
                            formView.Model.SetValue("F_HS_BalanceAmountUSD", SQLUtils.GetFieldValue(item, "balanceAmountUSA"), i);
                            formView.Model.SetValue("F_HS_CNYBalance", SQLUtils.GetFieldValue(item, "F_HS_CNYBalance"),i);
                            formView.Model.SetValue("F_HS_UpdateTime", SQLUtils.GetFieldValue(item, "updateTime"), i);

                            //formView.Model.SetValue("F_HS_UpdateUser", SQLUtils.GetFieldValue(item, "updateUser"), i);
                            formView.Model.SetItemValueByID("F_HS_UpdateUser", SQLUtils.GetFieldValue(item, "updateUser"), i);
                            formView.Model.SetValue("F_HS_BillNo", SQLUtils.GetFieldValue(item, "FBillNo"), i);

                            formView.Model.SetValue("F_HS_EntryId", SQLUtils.GetFieldValue(item, "fentryID"), i);
                            formView.Model.SetValue("F_HS_NeedFreezed", SQLUtils.GetFieldValue(item, "needfreezed"), i);
                            formView.Model.SetValue("F_HS_Remark", SQLUtils.GetFieldValue(item, "remark"), i);

                            i++;
                        }
                    }

                    formView.UpdateView(entryName);
                }
                else
                {
                    this.View.ShowErrMessage("", "没有需要查询的数据", MessageBoxType.Error);
                }
            }
        }
        public DynamicObjectCollection GetObjects(Context ctx, string sql)
        {
            return SQLUtils.GetObjects(ctx, sql);
        }

        public override void OnInitialize(InitializeEventArgs e)
        {
            base.OnInitialize(e);

            initSql = string.Format(@"/*dialect*/  select top 1000 F_HS_UseOrgId,F_HS_B2CCUSTID,changedAmount,changedType,F_HS_TradeType,changedCause,balanceAmount
	                                                ,F_HS_RateToUSA,FSETTLECURRID,changedAmountUSA,balanceAmountUSA,a.F_HS_CNYBalance,updateTime,updateUser,FBillNo
	                                                ,fentryID,needfreezed,remark
                                                    from HS_T_customerBalance a
                                                    inner join T_BD_CURRENCY b on b.FCURRENCYID = a.FSETTLECURRID
                                                    inner join T_SEC_user c on c.FUSERID = a.updateUser
                                                    inner join T_BD_CUSTOMER d on d.FCUSTID = a.F_HS_B2CCUSTID
                                                    order by updateTime desc
                                       ");
        }

        public override void CreateNewData(BizDataEventArgs e)
        {
            base.CreateNewData(e);

            DynamicObjectCollection coll = GetObjects(this.Context, initSql);
            DynamicObjectType dtType = this.View.BusinessInfo.GetDynamicObjectType();
            EntryEntity entity = (EntryEntity)this.View.BusinessInfo.GetEntity("F_HS_Entity");
            DynamicObject objData = new DynamicObject(dtType);
            DynamicObject entityObj = null;

            if (coll != null && coll.Count > 0)
            {
                int seq = 1;
                foreach (var item in coll)
                {
                    if (item != null)
                    {
                        if (entity != null)
                        {
                            entityObj = new DynamicObject(entity.DynamicObjectType);
                            entity.DynamicProperty.GetValue<DynamicObjectCollection>(objData).Add(entityObj);

                            entityObj["seq"] = seq;

                            //entityObj["F_HS_Customer_Id"] = Convert.ToInt32(SQLUtils.GetFieldValue(item, "F_HS_B2CCUSTID"));
                            //this.View.Model.SetValue("F_HS_Customer", SQLUtils.GetFieldValue(item, "F_HS_B2CCUSTID"),seq);
                            //this.View.Model.DataObject["F_HS_Customer_Id"] = SQLUtils.GetFieldValue(item, "F_HS_B2CCUSTID");
                            BaseDataField orgId = this.View.BillBusinessInfo.GetField("F_HS_UseOrgId") as BaseDataField;
                            DynamicObject orgObj = Kingdee.BOS.ServiceHelper.BusinessDataServiceHelper.LoadSingle(this.Context, SQLUtils.GetFieldValue(item, "F_HS_UseOrgId"), orgId.RefFormDynamicObjectType);
                            entityObj["F_HS_UseOrgId"] = orgObj;

                            BaseDataField cust = this.View.BillBusinessInfo.GetField("F_HS_Customer") as BaseDataField;
                            DynamicObject bdObj = Kingdee.BOS.ServiceHelper.BusinessDataServiceHelper.LoadSingle(this.Context, SQLUtils.GetFieldValue(item, "F_HS_B2CCUSTID"), cust.RefFormDynamicObjectType);
                            entityObj["F_HS_Customer"] = bdObj;

                            entityObj["F_HS_ChangeAmount"] = SQLUtils.GetFieldValue(item, "changedAmount");
                            entityObj["F_HS_ChangedType"] = GetChangeTypeDesc(SQLUtils.GetFieldValue(item, "changedType"));
                            entityObj["F_HS_TradeType"] = SQLUtils.GetFieldValue(item, "F_HS_TradeType");
                            entityObj["F_HS_ChangedCause"] = GetChangeCauseDesc(SQLUtils.GetFieldValue(item, "changedCause"));
                            entityObj["F_HS_BalanceAmount"] = SQLUtils.GetFieldValue(item, "balanceAmount");
                            entityObj["F_HS_RateToUSA"] = SQLUtils.GetFieldValue(item, "F_HS_RateToUSA");

                            entityObj["F_HS_SettleCurrId_Id"] = SQLUtils.GetFieldValue(item, "FSETTLECURRID");
                            //this.View.Model.SetValue("F_HS_SettleCurrId", SQLUtils.GetFieldValue(item, "FSETTLECURRID"), seq);
                            //BaseDataField curr = this.View.BusinessInfo.GetField("F_HS_SettleCurrId") as BaseDataField;
                            //curr.RefIDDynamicProperty.SetValue(this.View.Model.DataObject, SQLUtils.GetFieldValue(item, "FSETTLECURRID"));
                            //this.View.Model.DataObject["F_HS_SettleCurrId_Id"] = SQLUtils.GetFieldValue(item, "FSETTLECURRID");
                            //this.Model.SetItemValueByID("F_HS_SettleCurrId", 7, seq);
                            //this.View.UpdateView("F_HS_SettleCurrId",seq);

                            BaseDataField curr = this.View.BillBusinessInfo.GetField("F_HS_SettleCurrId") as BaseDataField;
                            DynamicObject cdObj = Kingdee.BOS.ServiceHelper.BusinessDataServiceHelper.LoadSingle(this.Context, SQLUtils.GetFieldValue(item, "FSETTLECURRID"), curr.RefFormDynamicObjectType);
                            entityObj["F_HS_SettleCurrId"] = cdObj;

                            //BaseDataField fldCustomer = formMetadata.BusinessInfo.GetField("FMaterialId") as BaseDataField;
                            //var materialObj = Kingdee.BOS.ServiceHelper.BusinessDataServiceHelper.LoadSingle(this.Context, materialId, fldMaterial.RefFormDynamicObjectType);
                            //fldMaterial.RefIDDynamicProperty.SetValue(dynamicRow, materialId);
                            //fldMaterial.DynamicProperty.SetValue(dynamicRow, materialObj);


                            entityObj["F_HS_ChangedAmountUSD"] = SQLUtils.GetFieldValue(item, "changedAmountUSA");
                            entityObj["F_HS_BalanceAmountUSD"] = SQLUtils.GetFieldValue(item, "balanceAmountUSA"); 
                            entityObj["F_HS_CNYBalance"] = SQLUtils.GetFieldValue(item, "F_HS_CNYBalance");
                            entityObj["F_HS_UpdateTime"] = SQLUtils.GetFieldValue(item, "updateTime");


                            BaseDataField user = this.View.BillBusinessInfo.GetField("F_HS_UpdateUser") as BaseDataField;
                            DynamicObject udObj = Kingdee.BOS.ServiceHelper.BusinessDataServiceHelper.LoadSingle(this.Context, SQLUtils.GetFieldValue(item, "updateUser"), user.RefFormDynamicObjectType);
                            entityObj["F_HS_UpdateUser"] = udObj;

                            entityObj["F_HS_BillNo"] = SQLUtils.GetFieldValue(item, "FBillNo");
                            entityObj["F_HS_EntryId"] = SQLUtils.GetFieldValue(item, "fentryID");
                            entityObj["F_HS_NeedFreezed"] = SQLUtils.GetFieldValue(item, "needfreezed") == "1" ? true : false;
                            entityObj["F_HS_Remark"] = SQLUtils.GetFieldValue(item, "remark");
                            seq++;

                        }
                    }
                }

                e.BizDataObject = objData;
            }

        }

        private string GetChangeTypeDesc(string changeType)
        {
            if (!string.IsNullOrWhiteSpace(changeType))
            {
                switch (changeType)
                {
                    case "TKCZ":
                        return "退款到客户余额账户";
                    case "SKCZ":
                        return "收款单客户充值";
                    case "XSXF":
                        return "线上销售订单消费";
                    case "XXXF":
                        return "线下销售订单消费";
                    case "DSXF":
                        return "DropShipping销售订单消费";
                    case "KHXYEDTZ":
                        return "客户信用额度调整";
                    default:
                        return null;
                }
            }
            return null;
        }

        private string GetChangeCauseDesc(string changeCauseDesc)
        {
            if (!string.IsNullOrWhiteSpace(changeCauseDesc))
            {
                switch (changeCauseDesc)
                {
                    case "DJSH":
                        return "单据审核";
                    case "DSRW":
                        return "定时任务";
                    default:
                        return null;
                }
            }
            return null;
        }
        public override void ButtonClick(ButtonClickEventArgs e)
        {
            switch (e.Key)
            {
                case "F_HS_QUERY":
                    LoadData(this.Context);
                    break;
                default:
                    break;
            }
        }
    }
}
