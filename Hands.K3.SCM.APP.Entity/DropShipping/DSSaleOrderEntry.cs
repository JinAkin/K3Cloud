using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.APP.Entity.DropShipping
{
    public class DSSaleOrderEntry
    {
        public string FMaterialId { get; set; }

        public decimal FQTY { get; set; }

        public string F_HS_StockID { get; set; }
    }
}
