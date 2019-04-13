using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hands.K3.SCM.App.Synchro.Base.Abstract;
using HS.K3.Common.Abbott;

namespace Hands.K3.SCM.App.Core.SynchroService.ToHC
{
    public class SynCustomersInfo2HC : AbstractSynchroToHC
    {
        public override SynchroDataType DataType
        {
            get
            {
                return SynchroDataType.Customer;
            }
        }
    }
}
