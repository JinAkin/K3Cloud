using Hands.K3.SCM.APP.Entity.EnumType;
using Hands.K3.SCM.APP.Entity.K3WebApi;
using Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.App.Synchro.Base.Interface
{
    public interface ISynchroDataToK3: ISynchroData
    {
        HttpResponseResult SynchroDataToK3(IEnumerable<string> numbers = null, Dictionary<SynOperationType, IEnumerable<AbsSynchroDataInfo>> datas = null);
        
    }
}
