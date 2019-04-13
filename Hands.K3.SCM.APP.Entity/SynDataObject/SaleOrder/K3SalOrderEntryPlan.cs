using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.APP.Entity.SynDataObject
{
    public class K3SalOrderEntryPlan
    {
        /// <summary>
        /// 交货地点
        /// </summary>
        public string FDetailLocId { get; set; }
        /// <summary>
        /// 要货日期
        /// </summary>
        public DateTime FPlanDate { get; set; }
        /// <summary>
        /// 交货地址
        /// </summary>
        public string FDetailLocAddress { get; set; }
        /// <summary>
        /// 仓库
        /// </summary>
        public string FStockId { get; set; }
        /// <summary>
        /// 运输提前期
        /// </summary>
        public DateTime FTransportLeadTime { get; set; }
        /// <summary>
        /// 销售单位
        /// </summary>
        public string FPlanUnitId { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public decimal FPlanQty { get; set; }
        /// <summary>
        /// 已出货数量
        /// </summary>
        public decimal FDeliCommitQty { get; set; }
        /// <summary>
        /// 剩余未出数量
        /// </summary>
        public int FDeliRemainQty { get; set; }
        /// <summary>
        /// 数量（基本单位）
        /// </summary>
        public decimal FBasePlanQty { get; set; }
        /// <summary>
        /// 已出货数量计划（基本单位）
        /// </summary>
        public decimal FBaseDeliCommitQty { get; set; }
        /// <summary>
        /// 剩余未出数量计划（基本单位）
        /// </summary>
        public decimal FBaseDeliRemainQty { get; set; }
        /// <summary>
        /// 基本计量单位
        /// </summary>
        public string FPlanBaseUnitId { get; set; }
        /// <summary>
        /// 计划发货日期
        /// </summary>
        public DateTime FPlanDeliveryDate { get; set; }
    }
}
