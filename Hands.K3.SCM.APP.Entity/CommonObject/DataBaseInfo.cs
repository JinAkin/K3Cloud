using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.APP.Entity.CommonObject
{
    public class DataBaseInfo
    {
        /// <summary>
        /// 数据库名称
        /// </summary>
        public string DBName { get; set; }
        /// <summary>
        /// 数据库登陆用户名
        /// </summary>
        public string DBUser { get; set; }
        /// <summary>
        /// 数据库登陆密码
        /// </summary>
        public string DBPwd { get; set; }
        /// <summary>
        /// 数据库的IP地址
        /// </summary>
        public string DBIPAddr { get; set; }
    }
}
