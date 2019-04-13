using Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.APP.Entity.DropShipping.DeliveryNotice
{
    [Serializable]
    public class DeliveryNotice: AbsSynchroDataInfo
    {
        public string BillNo { get; set; }
        public string OrderBillNo { get; set; }

        public string CarriageNO { get; set; }

        public string DeliDate { get; set; }

        public string LogisticsChannel { get; set; }

        public string QueryURL { get; set; }
    }

}
