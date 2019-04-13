using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hands.K3.SCM.APP.Entity.EnumType;

namespace Hands.K3.SCM.App.ServicePlugIn
{
    public class BatchAdjustB2BSerPlugIn: BatchAdjustSerPlugIn
    {
        [Description("批量调价服务插件-同步至B2B网站")]
        public override SynchroDirection Direction
        {
            get
            {
                return SynchroDirection.ToB2B;
            }
        }
    }
}
