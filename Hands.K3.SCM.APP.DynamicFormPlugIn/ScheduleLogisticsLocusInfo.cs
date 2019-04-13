using Kingdee.BOS;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core;

namespace Hands.K3.SCM.APP.DynamicFormPlugIn
{
    public class ScheduleLogisticsLocusInfo : IScheduleService
    {
        public void Run(Context ctx, Schedule schedule)
        {
            if (schedule == null)
            {
                return;
            }
            new DeliveryNoticePlugIn().SaveLogisticsLocus(ctx);
        }
    }
}
