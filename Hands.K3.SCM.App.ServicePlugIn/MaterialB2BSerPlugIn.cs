using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hands.K3.SCM.APP.Entity.EnumType;

namespace Hands.K3.SCM.App.ServicePlugIn
{
    public class MaterialB2BSerPlugIn: MaterialSerPlugIn
    {
        public override SynchroDirection Direction
        {
            get
            {
                return SynchroDirection.ToB2B;
            }
        }
    }
}
