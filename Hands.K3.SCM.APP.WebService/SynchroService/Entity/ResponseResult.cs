using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject;
using Newtonsoft.Json;

namespace Hands.K3.SCM.APP.WebService.SynchroService.Entity
{
   public class ResponseResult
    {
       public bool Success { get; set; }
       public IEnumerable<AbsSynchroDataInfo> Result{ get; set; }
       public IEnumerable<string> SuccessNos { get; set; }
       public string Message { get; set; }
    }
   public class Kuaidi100ResponseResult
   {
       public bool result { get; set; }
       public string returnCode { get; set; }
       public string message { get; set; }
   }

   public class PropogateResponseResult : ResponseResult
   {
       public int pagesize { get; set; }
       public int pageindex { get; set; }
       public int pagecount { get; set; }
       public int totalrecord { get; set; }
   }
}
