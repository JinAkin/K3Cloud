using Hands.K3.SCM.App.Synchro.Base.Abstract;
using HS.K3.Common.Abbott;

namespace Hands.K3.SCM.App.Core.SynchroService.ToHC
{
    public class SynBatchAdjust : AbstractSynchroToHC
    {
        public override SynchroDataType DataType
        {
            get
            {
                return SynchroDataType.BatchAdjust;
            }
        }
    }
}
