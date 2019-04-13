using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.APP.Entity.SynDataObject
{
    public class K3CustLocationInfo
    {
        /// <summary>
        /// 联系人编码
        /// </summary>
        public string FContactId { get; set; }
        /// <summary>
        /// 默认收货地址
        /// </summary>
        public bool FIsDefaultConsigneeCT { get; set; }

        public string FContactEmail { get; set; }

    }
}
