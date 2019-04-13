using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.APP.Entity.SynDataObject.Pack
{
    public class PackEntry
    {
        /// <summary>
        /// 发货通知单号
        /// </summary>
        public string FNotcieNo { get; set; }
        /// <summary>
        /// 物料编码
        /// </summary>
        public string FMaterialId { get; set; }
        /// <summary>
        /// 物料名称
        /// </summary>
        public string FSKUName { get; set; }
        /// <summary>
        /// 规格型号
        /// </summary>
        public string FSpecification { get; set; }
        /// <summary>
        /// 属性1
        /// </summary>
        public string FAttr1 { get; set; }
        /// <summary>
        /// 属性2
        /// </summary>
        public string FAttr2 { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public string FQTY { get; set; }
        /// <summary>
        /// 拣货仓位
        /// </summary>
        public string F_HS_LocationId { get; set; }
        /// <summary>
        /// 发货通知单明细序号
        /// </summary>
        public int FNoticeEntityNum{ get; set; }
    }
}
