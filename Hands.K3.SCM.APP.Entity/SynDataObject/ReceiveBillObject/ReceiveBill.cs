using Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.APP.Entity.SynDataObject.ReceiveBillObject
{
    public class ReceiveBill: AbsSynchroDataInfo
    {
        public string FBillNo { get; set; }

        public string F_HS_B2CCustId { get; set; }

        public DateTime FDate { get; set; }

        public string FSettleCurrId { get; set; }
        public decimal F_HS_FinancialReceived { get; set; }
        
        /// <summary>
        /// 实收金额
        /// </summary>
        public decimal FREALRECAMOUNTFOR { get; set; }

        /// <summary>
        /// 是否充值
        /// </summary>
        public bool F_HS_YNRecharge { get; set; }
        /// <summary>
        /// 是否已同步充值
        /// </summary>
        public bool F_HS_SynchronizedRecharge { get; set; }

        public List<ReceiveBillEntry> BillEntry { get; set; }


    }
}
