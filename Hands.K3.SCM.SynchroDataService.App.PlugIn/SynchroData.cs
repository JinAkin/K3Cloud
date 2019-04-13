using Hands.K3.SCM.APP.Entity.EnumType;
using Hands.K3.SCM.APP.Entity.K3WebApi;
using Hands.K3.SCM.APP.Utils.Utils;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using System;

namespace Hands.K3.SCM.APP.DynamicFormPlugIn
{
    [Description("数据同步--动态表单插件")]
    public class SynchroData : AbstractDynamicFormPlugIn
    {

        public void LoadData()
        {
            string k3ServerUrl = JsonUtils.ConvertObjectToString(this.View.Model.GetValue("F_HS_K3Server"));
            if (!string.IsNullOrWhiteSpace(k3ServerUrl) && k3ServerUrl.CompareTo("测试服务器") == 0)
            {
                k3ServerUrl = "http://10.2.0.150/k3cloud/";
            }
            else if (!string.IsNullOrWhiteSpace(k3ServerUrl) && k3ServerUrl.CompareTo("正式服务器") == 0)
            {
                k3ServerUrl = "http://10.2.0.150/k3cloud/";
            }

            string redisServerUrl = JsonUtils.ConvertObjectToString(this.View.Model.GetValue("F_HS_RedisServer"));
            if (!string.IsNullOrWhiteSpace(redisServerUrl) && redisServerUrl.CompareTo("测试服务器") == 0)
            {
                redisServerUrl = "10.2.0.150";
            }
            else if (!string.IsNullOrWhiteSpace(redisServerUrl) && redisServerUrl.CompareTo("正式服务器") == 0)
            {
                redisServerUrl = "221.120.177.22";
            }

            long redsiDb = long.Parse(JsonUtils.ConvertObjectToString(this.View.Model.GetValue("F_HS_RedisDB")));
            string billNo = JsonUtils.ConvertObjectToString(this.View.Model.GetValue("F_HS_BillNo"));
        }
        public void SynchroDataTo(Context ctx, SynchroDataType dataType,SynchroDirection direction)
        {
            HttpResponseResult result = null;
            try
            {
                if (direction == SynchroDirection.ToK3)
                {
                    result = SynchroDataHelper.SynchroDataToK3(ctx, dataType);
                }
                if(direction == SynchroDirection.ToHC)
                {
                    result = SynchroDataHelper.SynchroDataToHC(ctx,dataType);
                }
                if (result != null)
                {
                    if (result.Success)
                    {
                        this.View.ShowErrMessage("", string.Format("同步{0}至{1}成功！",LogUtils.GetDataSourceTypeDesc(dataType),direction.ToString()) + result.Message, Kingdee.BOS.Core.DynamicForm.MessageBoxType.Error);
                    }
                    else
                    {
                        this.View.ShowErrMessage("", string.Format("同步{0}至{1}失败！",LogUtils.GetDataSourceTypeDesc(dataType), direction.ToString()) + result.Message, Kingdee.BOS.Core.DynamicForm.MessageBoxType.Error);
                    }
                }
                else
                {
                    this.View.ShowErrMessage("", string.Format("同步{0}至{1}失败！",LogUtils.GetDataSourceTypeDesc(dataType), direction.ToString()), Kingdee.BOS.Core.DynamicForm.MessageBoxType.Error);
                }
            }
            catch (Exception ex)
            {
                this.View.ShowErrMessage(ex.ToString(), string.Format("同步{0}至{1}失败！",LogUtils.GetDataSourceTypeDesc(dataType), direction.ToString()), Kingdee.BOS.Core.DynamicForm.MessageBoxType.Error);
            }
        }

        public override void ButtonClick(ButtonClickEventArgs e)
        {
            base.ButtonClick(e);

            switch (e.Key)
            {
                case "F_HS_SYNCHROSALORDERSTATUS":
                    SynchroDataTo(this.Context, SynchroDataType.SalesOrderPayStatus, SynchroDirection.ToK3);
                    break;
               
                case "F_HS_SYNCHROSALORDER":
                    SynchroDataTo(this.Context, SynchroDataType.SaleOrder, SynchroDirection.ToK3);
                    break;

                case "F_HS_SYNCHROINVENTORY":
                    SynchroDataTo(this.Context, SynchroDataType.Inventroy, SynchroDirection.ToHC);
                    break;
                
                case "F_HS_SYNCHROCUSTOMER":
                    SynchroDataTo(this.Context, SynchroDataType.Customer, SynchroDirection.ToK3);
                    break;

                case "F_HS_SYNCHROCUSTOMERBYEXCEL":
                    SynchroDataTo(this.Context, SynchroDataType.CustomerByExcel, SynchroDirection.ToK3);
                    break;

                case "F_HS_CUSTOMERADDRESS":
                    SynchroDataTo(this.Context, SynchroDataType.CustomerAddress, SynchroDirection.ToK3);
                    break;

                case "F_HS_CUSTADDRBYEXCEL":
                    SynchroDataTo(this.Context, SynchroDataType.CustomerAddressByExcel, SynchroDirection.ToK3);
                    break;

                case "F_HS_SYNOFFLINESALORDER":
                    SynchroDataTo(this.Context, SynchroDataType.SaleOrderOffline, SynchroDirection.ToHC);
                    break;

                case "F_HS_DELCUSTOMERADDRESS":
                    SynchroDataTo(this.Context, SynchroDataType.DelCustomerAddress, SynchroDirection.ToK3);
                    break;

                case "F_HS_SYNSALORDERSTATUS":
                    SynchroDataTo(this.Context, SynchroDataType.SaleOrderStatus, SynchroDirection.ToHC);
                    break;

                case "F_HS_MATERIAL":
                    SynchroDataTo(this.Context, SynchroDataType.Material, SynchroDirection.ToHC);
                    break;

                case "F_HS_SYNONTHEWAY":
                    SynchroDataTo(this.Context,SynchroDataType.OnTheWay,SynchroDirection.ToHC);
                    break;

                case "F_HS_SYNCHRORECEIVEBILL":
                    SynchroDataTo(this.Context, SynchroDataType.ReceiveBill, SynchroDirection.ToK3);
                    break;

                default:
                    break;
            }
        }

    }

}
