using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.APP.WebService.SynchroService.Entity
{
    public class WebToken
    {
        public string token { get; set; }
        /// <summary>
        /// 账套代码
        /// </summary>
        public string dcNumber { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string userName { get; set; }
        /// <summary>
        /// 密码
        /// </summary>
        public string passWord { get; set; }
    }
}
