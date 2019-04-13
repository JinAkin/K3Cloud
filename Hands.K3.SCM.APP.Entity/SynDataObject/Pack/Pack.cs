using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.APP.Entity.SynDataObject.Pack
{
    public class Pack
    {
        /// <summary>
        /// 单据编号
        /// </summary>
        public string FBillNo { get; set; }
        /// <summary>
        /// 单据状态
        /// </summary>
        public string FDocumentStatus { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>
        public string FCreatorId { get; set; }
        /// <summary>
        /// 创建日期
        /// </summary>
        public DateTime FCreateDatew { get; set; }
        /// <summary>
        /// 最后修改人
        /// </summary>
        public decimal FModifierId { get; set; }
        /// <summary>
        /// 最后修改日期
        /// </summary>
        public DateTime FModifyDate { get; set; }
        /// <summary>
        /// 审核人
        /// </summary>
        public string FApproverID { get; set; }
        /// <summary>
        /// 审核日期
        /// </summary>
        public string FApproveDate { get; set; }
        /// <summary>
        /// 实际重量
        /// </summary>
        public decimal FWeight { get; set; }
        /// <summary>
        /// 箱号
        /// </summary>
        public string FCartonNO { get; set; }
        /// <summary>
        /// 装箱时间
        /// </summary>
        public DateTime FBoxTime { get; set; }
        /// <summary>
        /// 仓库箱子
        /// </summary>
        public string FStockBox { get; set; }
        /// <summary>
        /// 是否是最后一箱
        /// </summary>
        public bool F_HS_IsLast { get; set; }
        /// <summary>
        /// 是否拆箱
        /// </summary>
        public bool F_HS_IsCancel { get; set; }
        /// <summary>
        /// 箱长
        /// </summary>
        public decimal FLength { get; set; }
        /// <summary>
        /// 箱宽
        /// </summary>
        public decimal FWidthw { get; set; }
        /// <summary>
        /// 箱高
        /// </summary>
        public decimal FHigh { get; set; }
        /// <summary>
        /// 箱体积
        /// </summary>
        public decimal FVolumeWeight { get; set; }


    }
}
