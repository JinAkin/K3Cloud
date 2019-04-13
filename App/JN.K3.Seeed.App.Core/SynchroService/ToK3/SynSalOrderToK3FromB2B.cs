using Hands.K3.SCM.App.Core.SynchroService.ToK3;
using Hands.K3.SCM.APP.Entity.EnumType;
using HS.K3.Common.Abbott;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.App.Core.SynchroService.ToK3
{
    public class SynSalOrderToK3FromB2B: SynSalOrderToK3
    {
        public override SynchroDataType DataType
        {
            get
            {
                return SynchroDataType.DropShippingSalOrder;
            }
        }
        public override SynchroDirection Direction
        {
            get
            {
                return SynchroDirection.ToB2B;
            }
        }
    }
}
