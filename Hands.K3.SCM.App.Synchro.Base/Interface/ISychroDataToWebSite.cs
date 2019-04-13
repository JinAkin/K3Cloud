using Hands.K3.SCM.APP.Entity.K3WebApi;
using Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.App.Synchro.Base.Interface
{
    public interface ISychroDataToWebSite: ISynchroData
    {
        HttpResponseResult SynchroDataToWebSite(IEnumerable<AbsSynchroDataInfo> k3Datas = null, IEnumerable<string> billNos = null, bool SQLFilter = true);
    }
}
