using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.APP.Entity.DropShipping
{
    public class DropShippingInfo
    {
        public string Token { get; set; }

        public string DataType { get; set; }

        public string FCustId { get; set; }

        public string TimeStamp { get; set; }

        public string SignMsg { get; set; }

        public List<DSSaleOrder> SaleOrders { get; set; }
    }
}
