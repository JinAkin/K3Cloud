using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS;
using Kingdee.BOS.Contracts;

namespace Hands.K3.SCM.APP.WebService.SynchroService.Entity
{
   public class UserSession
    {
       /// <summary>
       /// 登录时数据中心代码
       /// </summary>
       public static string _DataCenterNumber = "0625";

       public static long GetID(Context Context, string TableName, int Count = 1)
       {
           IDBService oDBService = Kingdee.BOS.Contracts.ServiceFactory.GetService<IDBService>(Context);
           long[] oReturnIDS = oDBService.GetSequenceInt64(Context, TableName, Count).ToArray();
           return oReturnIDS[0];
       }
    }
}
