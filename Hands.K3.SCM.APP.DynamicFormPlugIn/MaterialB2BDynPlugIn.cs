using Hands.K3.SCM.APP.Entity.EnumType;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.APP.DynamicFormPlugIn
{
    public class MaterialB2BDynPlugIn: MaterialDynPlugIn
    {
        [Description("物料同步到B2B网站表单插件")]
        public override SynchroDirection Direction
        {
            get
            {
                return SynchroDirection.ToB2B;
            }
        }
    }
}
