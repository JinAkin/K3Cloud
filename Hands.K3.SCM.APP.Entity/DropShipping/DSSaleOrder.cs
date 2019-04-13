using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.APP.Entity.DropShipping
{
    [Serializable]
    public class DSSaleOrder
    {
        public string FBillNo { get; set; }

        public DateTime FDate { get; set; }
        [JsonIgnore]
        public string F_HS_SaleOrderSource { get; set; }
        [JsonIgnore]
        public string F_HS_B2CCustId{get;set;}

        public string FSettleCurrId { get; set; }

        public decimal F_HS_AmountDeclared { get; set; }

        public decimal F_HS_FreightDeclared { get; set; }

        public string F_HS_DeclaredCurrId { get; set; }
        public string F_HS_RecipientCountry { get; set; }

        public string F_HS_DeliveryProvinces { get; set; }

        public string F_HS_DeliveryCity { get; set; }

        public string F_HS_DeliveryAddress { get; set; }

        public string F_HS_PostCode { get; set; }

        public string F_HS_DeliveryName { get; set; }

        public string F_HS_MobilePhone { get; set; }

        public string F_HS_DropShipDeliveryChannel { get; set; }

        /// <summary>
        /// 平台客户ID
        /// </summary>
        public string F_HS_PlatformCustomerID { get; set; }
        /// <summary>
        /// 平台客户邮箱
        /// </summary>
        public string F_HS_PlatformCustomerEmail { get; set; }

        public List<DSSaleOrderEntry> OrderEntry { get; set; }

    }
}
