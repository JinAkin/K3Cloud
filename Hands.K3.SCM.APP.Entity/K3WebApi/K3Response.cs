using Kingdee.BOS;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.APP.Entity.K3WebApi
{
    public class K3Response
    {
        public Context K3CloudContext { get; set; }
        public string BillNo { get; set; }

        public string SalerId { get; set; }

        public string SalerEmail
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(SalerId))
                {
                    string sql = string.Format(@"/*dialect*/ select FEmail from T_HR_EMPINFO where FNUMBER = '{0}'", SalerId);

                    DynamicObjectCollection coll = DBServiceHelper.ExecuteDynamicObject(K3CloudContext, sql);

                    if (coll.Count > 0)
                    {
                        foreach (var item in coll)
                        {
                            if (item != null)
                            {
                                return item["FEmail"].ToString();
                            }
                        }
                    }

                }
                return "stella.ou@healthcabin.net";
            }
        }

        public string Msg { get; set; }

        public K3Response()
        {

        }

        public K3Response(string billNo, string msg)
        {
            this.BillNo = billNo;
            this.Msg = msg;
        }
    }
}