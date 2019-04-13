using Kingdee.BOS.Contracts;
using System.ComponentModel;
using Kingdee.BOS;
using Kingdee.BOS.Core;
using Hands.K3.SCM.APP.Entity.StructType;
using Hands.K3.SCM.App.Synchro.Utils.SynchroService;
using HS.K3.Common.Abbott;

namespace Hands.K3.SCM.App.Synchro.Task
{
    [Kingdee.BOS.Util.HotUpdate]
    [Description("顺序同步客户，订单，库存，订单状态")]
    public class ScheduleSequenceSynchro : IScheduleService
    {
        public void Run(Context ctx, Schedule schedule)
        {
            
            if (ctx.DBId.CompareTo(DataBaseConst.K3CloudDbId) == 0)
            {
                if (schedule == null)
                {
                    return;
                }

                SynchroDataHelper.SynchroDataToK3(ctx, SynchroDataType.Customer);
                SynchroDataHelper.SynchroDataToK3(ctx, SynchroDataType.SaleOrder);
                SynchroDataHelper.SynchroDataToHC(ctx, SynchroDataType.Inventroy);
            }

        }
    }

    [Description("同步全部或即时库存(modify)至HC网站")]
    public class ScheduleInventory : IScheduleService
    {
        public void Run(Context ctx, Schedule schedule)
        {
            if (ctx.DBId.CompareTo(DataBaseConst.K3CloudDbId) == 0)
            {
                if (schedule == null)
                {
                    return;
                }
                SynchroDataHelper.SynchroDataToHC(ctx, SynchroDataType.Inventroy);
            }
        }
    }

    [Description("同步客户至K3")]
    public class ScheduleSynchroCustomer : IScheduleService
    {
        public void Run(Context ctx, Schedule schedule)
        {
            

            if (ctx.DBId.CompareTo(DataBaseConst.K3CloudDbId) == 0)
            {
                if (schedule == null)
                {
                    return;
                }

                SynchroDataHelper.SynchroDataToK3(ctx, SynchroDataType.Customer);
            }
        }
    }

    [Description("同步客户(数据源Excel)至K3")]
    public class ScheduleSynchroCustomerByExcel : IScheduleService
    {
        public void Run(Context ctx, Schedule schedule)
        {
            

            if (ctx.DBId.CompareTo(DataBaseConst.K3CloudDbId) == 0)
            {
                if (schedule == null)
                {
                    return;
                }

                SynchroDataHelper.SynchroDataToK3(ctx, SynchroDataType.CustomerByExcel);
            }

        }
    }

    [Description("同步客户地址(数据源Excel)至K3")]
    public class ScheduleSynchroCustomerAddressByExcel : IScheduleService
    {
        public void Run(Context ctx, Schedule schedule)
        {

            
            if (ctx.DBId.CompareTo(DataBaseConst.K3CloudDbId) == 0)
            {
                if (schedule == null)
                {
                    return;
                }

                SynchroDataHelper.SynchroDataToK3(ctx, SynchroDataType.CustomerAddressByExcel);
            }
        }
    }

    [Description("同步客户地址(数据源Redis)至K3")]
    public class ScheduleSynchroCustomerAddress : IScheduleService
    {
        public void Run(Context ctx, Schedule schedule)
        {
            
            if (ctx.DBId.CompareTo(DataBaseConst.K3CloudDbId) == 0)
            {
                if (schedule == null)
                {
                    return;
                }

                SynchroDataHelper.SynchroDataToK3(ctx, SynchroDataType.CustomerAddress);
            }
        }
    }

    [Description("删除客户地址")]
    public class ScheduleDelCustomerAddress : IScheduleService
    {
        public void Run(Context ctx, Schedule schedule)
        {
            
            if (ctx.DBId.CompareTo(DataBaseConst.K3CloudDbId) == 0)
            {
                if (schedule == null)
                {
                    return;
                }

                SynchroDataHelper.SynchroDataToK3(ctx, SynchroDataType.DelCustomerAddress);
            }
        }
    }

    [Description("同步销售订单至K3")]
    public class ScheduleSynchroSalOrder : IScheduleService
    {
        public void Run(Context ctx, Schedule schedule)
        {
            
            if (ctx.DBId.CompareTo(DataBaseConst.K3CloudDbId) == 0)
            {
                if (schedule == null)
                {
                    return;
                }

                SynchroDataHelper.SynchroDataToK3(ctx, SynchroDataType.SaleOrder);
            }
        }
    }

    [Description("同步Dropshipping销售订单至K3")]
    public class ScheduleSynchroDropshippingSalOrder : IScheduleService
    {
        public void Run(Context ctx, Schedule schedule)
        {

            if (ctx.DBId.CompareTo(DataBaseConst.K3CloudDbId) == 0)
            {
                if (schedule == null)
                {
                    return;
                }

                SynchroDataHelper.SynchroDataToK3(ctx, SynchroDataType.DropShippingSalOrder);
            }
        }
    }

    [Description("同步销售订单状态--付款状态至K3")]
    public class ScheduleSynchroSalOrderStatus : IScheduleService
    {
        public void Run(Context ctx, Schedule schedule)
        {
            
            if (ctx.DBId.CompareTo(DataBaseConst.K3CloudDbId) == 0)
            {
                if (schedule == null)
                {
                    return;
                }

                SynchroDataHelper.SynchroDataToK3(ctx, SynchroDataType.SalesOrderPayStatus);
            }

        }
    }
    [Description("同步销售订单状态至HC网站")]
    public class ScheduleSynSalOrderStatus : IScheduleService
    {
        public void Run(Context ctx, Schedule schedule)
        {
            
            if (ctx.DBId.CompareTo(DataBaseConst.K3CloudDbId) == 0)
            {
                if (schedule == null)
                {
                    return;
                }

                SynchroDataHelper.SynchroDataToHC(ctx, SynchroDataType.SaleOrderStatus);
            }

        }
    }
    [Description("同步物料至HC网站")]
    public class ScheduleSynchroMaterial : IScheduleService
    {
        public void Run(Context ctx, Schedule schedule)
        {
            
            if (ctx.DBId.CompareTo(DataBaseConst.K3CloudDbId) == 0)
            {
                if (schedule == null)
                {
                    return;
                }
            }
            SynchroDataHelper.SynchroDataToHC(ctx, SynchroDataType.Material);
        }
    }

    [Kingdee.BOS.Util.HotUpdate]
    [Description("同步物流跟踪明细信息至HC网站")]
    public class ScheduleSynLogisticsTraceInfo : IScheduleService
    {
        public void Run(Context ctx, Schedule schedule)
        {

            if (ctx.DBId.CompareTo(DataBaseConst.K3CloudDbId) == 0)
            {
                if (schedule == null)
                {
                    return;
                }
                SynchroDataHelper.SynchroDataToHC(ctx, SynchroDataType.ImportLogis);
            }
        }
    }

    [Description("同步线下销售订单至HC网站")]
    public class ScheduleSynOfflineSalOrder : IScheduleService
    {
        public void Run(Context ctx, Schedule schedule)
        {
            
            if (ctx.DBId.CompareTo(DataBaseConst.K3CloudDbId) == 0)
            {
                if (schedule == null)
                {
                    return;
                }
                SynchroDataHelper.SynchroDataToHC(ctx, SynchroDataType.SaleOrderOffline);
            }
        }
    }

    [Description("定时任务生成历史生成失败的收款单")]
    public class ScheduleSynReceiveBill : IScheduleService
    {
        public void Run(Context ctx, Schedule schedule)
        {
            
            if (ctx.DBId.CompareTo(DataBaseConst.K3CloudDbId) == 0)
            {
                if (schedule == null)
                {
                    return;
                }
                SynchroDataHelper.SynchroDataToK3(ctx, SynchroDataType.ReceiveBill);
            }
        }
    }

    [Description("采购，调拨在途明细同步至HC网站")]
    public class ScheduleSynOnTheWay : IScheduleService
    {
        public void Run(Context ctx, Schedule schedule)
        {
            
            //if (ctx.DBId.CompareTo(DataBaseConst.K3CloudDbId) == 0)
            //{
                if (schedule == null)
                {
                    return;
                }
                SynchroDataHelper.SynchroDataToHC(ctx, SynchroDataType.OnTheWay);
            //}
        }
    }

    [Description("物流轨迹同步至HC网站")]
    public class ScheduleSynLogisTrack : IScheduleService
    {
        public void Run(Context ctx, Schedule schedule)
        {
            
            if (ctx.DBId.CompareTo(DataBaseConst.K3CloudDbId) == 0)
            {
                if (schedule == null)
                {
                    return;
                }
                SynchroDataHelper.SynchroDataToHC(ctx, SynchroDataType.DeliveryNoticeBill);
            }
        }
    }
}
