using Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.APP.Entity.SynDataObject.SaleOrder
{
    public class K3OrderStatusInfo
    {
        public string SrcPKId { get; set; }
        public string SrcNo { get; set; }
         /// <summary>
        /// 订单号
        /// </summary>
        public string OrderNo{get;set;}
        /// <summary>
        /// 订单状态的原始名称
        /// </summary>
        public string SrcOrderStatus{get;set;}
        /// <summary>
        /// 状态设置时间（UNIX时间戳）
        /// </summary>
        public string Createtime{get;set;}
        /// <summary>
        /// 订单状态设置备注
        /// </summary>
        public string Comments{get;set;}
        /// <summary>
        /// 订单状态变更是否通知用户(0：不邮件通知用户，1:邮件通知用户)
        /// </summary>
        public int IsNotified{get;set;}
        /// <summary>
        /// 订单状态 
        /// </summary>
        public BazzarOrderStatus BazzarSOStatus{get;set;}
        /// <summary>
        /// 运输方式
        /// </summary>
        public string ShippingMethod{get;set;}
        /// <summary>
        /// 运单号
        /// </summary>
        public string ShippingNumber{get;set;}
        
        /// <summary>
        /// 发货通知单号
        /// </summary>
        public string DeliveryNo{get;set;}
       
    }




    /// <summary>
    /// Bazzar订单的状态(订单头的订单状态)
    /// </summary>
    public enum BazzarOrderStatus
    {


        /// <summary>
        /// Backorder订单
        /// </summary>
        Backorder,

        /// <summary>
        /// 取消
        /// </summary>
        Canceled,

        /// <summary>
        /// 用户自己取消
        /// </summary>
        Canceled_Unapproved,

        /// <summary>
        /// 买家已经付款
        /// </summary>
        Confirmed,

        /// <summary>
        /// 纠纷处理中
        /// </summary>
        In_Dispute,


        /// <summary>
        /// 挂起
        /// </summary>
        Pending,



        /// <summary>
        /// 处理中
        /// </summary>
        Processing,

        /// <summary>
        /// 退款
        /// </summary>
        Refunded,


        /// <summary>
        /// 已发货
        /// </summary>
        Shipped,

        /// <summary>
        /// 有物流信息可跟踪
        /// </summary>
        Traceable,



        /// <summary>
        /// 未知
        /// </summary>
        None,





    }





    /// <summary>
    /// k3订单的订单明细行状态
    /// </summary>
    public enum OrderLineStatus
    {
        /// <summary>
        /// Confirmed （买家已经付款，订单同步下来的默认明细行PCB状态）
        /// </summary>
        A,

        /// <summary>
        /// Info-PASSED（等待审核，已同步至供应商系统中时更新）
        /// </summary>
        B,

        /// <summary>
        /// PCB-FileReviewed（审核通过，供应商系统中状态为【审核通过】时更新）
        /// </summary>
        C,

        /// <summary>
        /// PCB-Production（生产中，供应商系统中状态为【生产中】时更新）
        /// </summary>
        D,

        /// <summary>
        /// PCB Finished（我方已收货，供应商系统中状态为【厂方已收货】时更新）
        /// </summary>
        E,

        /// <summary>
        /// Shipped（我方已发货，销售出库单审核时更新）
        /// </summary>
        F,

        /// <summary>
        /// Pending（挂起：PCB订单  供应商系统中状态为【取消】，PCPA订单 ERP订单明细行状态为【终止】时更新订单状态为取消）
        /// </summary>
        G,

        /// <summary>
        /// Refunded（退款）
        /// </summary>
        H,

        /// <summary>
        /// Waiting For Material（待料）
        /// </summary>
        I,

        /// <summary>
        /// Material Ready（已齐料）
        /// </summary>
        J,

        /// <summary>
        /// PCBA Finished（PCBA完成）
        /// </summary>
        K,

        /// <summary>
        /// Traceable（物流信息可追踪，发货通知单上的运单号不为空时）
        /// </summary>
        L,
        
        /// <summary>
        /// Canceled-Unapproved（用户取消  无需考虑此状态）
        /// </summary>
        M,

        /// <summary>
        ///Canceled （取消 ，ERP订单明细行状态为【终止】时更新订单状态为取消）
        /// </summary>
        N,

        /// <summary>
        /// SaleOrder approved(销售订单已审核)
        /// </summary>
        O,

        /// <summary>
        /// PurchaseOrder approved(采购订单已审核--非OPL)
        /// </summary>
        P,

        /// <summary>
        /// PurchaseOrder ToStorage(采购入库单已审核--非OPL)
        /// </summary>
        Q,

        /// <summary>
        /// ProductionStorageOrder Submit(生产入库单提交时间)
        /// </summary>
        R,

        /// <summary>
        /// Supplier Delivery Time(PCB供应商已发货)
        /// </summary>
        S
    }





    /// <summary>
    /// Bazzar订单行的产品状态
    /// </summary>
    public class BazzarOrderProductStatusInfo : AbsSynchroDataInfo
    {
        /// <summary>
        /// 订单号
        /// </summary>
        public string OrderNo{get;set;}
        /// <summary>
        /// 订单行号
        /// </summary>
        public string OrderRowId{get;set;}
        /// <summary>
        /// 订单行状态设置时间（UNIX时间戳）
        /// </summary>
        public string Createtime{get;set;}
        /// <summary>
        /// 订单行状态设置备注
        /// </summary>
        public string Comments{get;set;}
        /// <summary>
        /// 订单行状态变更是否通知用户(0：不邮件通知用户，1:邮件通知用户)
        /// </summary>
        public int IsNotified{get;set;}
        /// <summary>
        /// 订单行状态
        /// </summary>
        public int  BazzarSOLineStatus{get;set;}
        /// <summary>
        /// 订单物料文件名
        /// </summary>
        public string FileName{get;set;}
    }

    /// <summary>
    /// 销售订单单据类型
    /// </summary>
    public  struct K3SaleOrderBillType
    {
        /// <summary>
        /// Fusion订单
        /// </summary>
        public static string FusionOrder = "XSDD15_SYS";
        /// <summary>
        /// SMG-国际渠道订单
        /// </summary>
        public static string SMGInternationalOrder = "XSDD10_SYS";
        /// <summary>
        /// SMG-DropShipping订单
        /// </summary>
        public static string SMGDropShippingOrder = "XSDD17_SYS";

        /// <summary>
        /// PRZ标准订单
        /// </summary>
        public static string PRZOrder = "XSDD13_SYS";
        /// <summary>
        /// PRZ-DropShipping订单
        /// </summary>
        public static string PRZDropShippingOrder = "XSDD14_SYS";

        /// SMG电商订单
        /// </summary>
        public static string SMGESaleOrder = "XSDD12_SYS";
        /// <summary>
        /// 亚马逊FBA销售订单
        /// </summary>
        public static string FBASaleOrder = "FBA001";
        /// <summary>
        /// 风险备料单
        /// </summary>
        public static string PRZFXBL = "PRZFXBL";
    }

    /// <summary>
    /// 发货通知单单据类型
    /// </summary>
    public struct K3DeliveryNoticeBillType
    {
        /// <summary>
        /// Fusion订单
        /// </summary>
        public static string FusionOrder = "FHTZD11_SYS";
        /// <summary>
        /// 保宏补货订单
        /// </summary>
        public static string BHOrder = "FHTZD13_SYS";
        /// <summary>
        /// 亚马逊FBA发货通知单
        /// </summary>
        public static string FBAOrder = "FBA002";
    }
    /// <summary>
    /// 发货通知单单据类型ID
    /// </summary>
    public struct K3DeliveryNoticeBillTypeID
    {
        /// <summary>
        /// Fusion订单
        /// </summary>
        public static string FusionOrder = "5720783ffdd9d6";
    }

    /// <summary>
    /// 销售出库单单据类型
    /// </summary>
    public struct K3StockOutBillType
    {
        public static string FBAOrder = "FBA003";
    }

    /// <summary>
    /// 采购订单单据类型
    /// </summary>
    public struct K3PurchaseOrderBillType
    {
        /// <summary>
        /// Fusion订单
        /// </summary>
        public static string FusionOrder = "CGDD10_USER";
        /// <summary>
        /// SOC采购订单
        /// </summary>
        public static string SOCOrder = "CGDD08-SYS";
    }

    /// <summary>
    /// 生产订单单据类型
    /// </summary>
    public struct K3MOBillType
    {
        /// <summary>
        /// 512-Cell0工单
        /// </summary>
        public static string Cell0="512";
        /// <summary>
        /// 517-Fusion工单
        /// </summary>
        public static string Fusion = "517";
        /// <summary>
        /// 513-样品工单
        /// </summary>
        public static string Sample = "YPMO";
        /// <summary>
        /// 514-物料加工单
        /// </summary>
        public static string MaterialMO = "NBWLJG";
    }
    
    /// <summary>
    /// 委外订单
    /// </summary>
    public struct K3SUBBillType
    {
        /// <summary>
        /// 516-委外物料加工单
        /// </summary>
        public static string MaterialSUB = "WWWL";
    }
    /// <summary>
    /// 收款单
    /// </summary>
    public struct K3ReceiveBillType
    {
        /// <summary>
        /// 销售收款单
        /// </summary>
        public static string BZSKD = "SKDLX01_SYS";
        /// <summary>
        /// 其他业务收款单
        /// </summary>
        public static string QTYWSKD = "SKDLX02_SYS";
    }

    /// <summary>
    /// 销售订单Bazaar状态信息
    /// </summary>
    public class SalesOrderBazaarStatus : AbsSynchroDataInfo
    {
        /// <summary>
        /// BazaarID
        /// </summary>
        public string BazaarID { get; set; }
        /// <summary>
        /// Bazaar行ID
        /// </summary>
        public string BazaarLineID { get; set; }
        /// <summary>
        /// 类型：A单头；B:单身
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// 状态编码
        /// </summary>
        public string StatusCode { get; set; }
        /// <summary>
        /// 状态名称
        /// </summary>
        public string StatusName { get; set; }
        /// <summary>
        /// 来源
        /// </summary>
        public string Source { get; set; }
        /// <summary>
        /// 是否需要发送邮件
        /// </summary>
        public bool IsSendEmail { get; set; }
        /// <summary>
        /// 物料编码
        /// </summary>
        public string MaterialNumber { get; set; }
    }

    /// <summary>
    /// 同步Bazaar状态信息
    /// </summary>
    public class BazaarStatus
    {
        /// <summary>
        /// 销售订单审核
        /// </summary>
        public static string LineSalesOrderApproved = "A";
        /// <summary>
        /// //采购订单审核（非OPL）
        /// </summary>
        public static string PurchaseOrderApproved_50800001 = "B";
        /// <summary>
        /// 生成入库单提交
        /// </summary>
        public static string ProductionInStockSubmit = "C";
        /// <summary>
        /// 生成入库单审核-PCBA完成时间
        /// </summary>
        public static string ProductionInStockApproved_PCBA = "D";
        /// <summary>
        /// PCB发货时间--供应商发货时间
        /// </summary>
        public static string PCBDeliveryTimeFromSupplier = "E";
        /// <summary>
        /// 钢网采购入库审核
        /// </summary>
        public static string ReceivingOrder_FusionStencilService = "F";
        /// <summary>
        /// 非OPL采购入库审核
        /// </summary>
        public static string ReceivingOrder_NOPL = "G";
        /// <summary>
        /// PCB采购入库审核
        /// </summary>
        public static string ReceivingOrder_FusionPCBCategoryCode = "H";
        /// <summary>
        /// 钢网生成供应商订单
        /// </summary>
        public static string CreateSupplerOrder_FusionStencilService = "I";
        /// <summary>
        /// 非OPL生成供应商订单
        /// </summary>
        public static string CreateSupplerOrder_NOPL = "J";
        /// <summary>
        /// PCB生成供应商订单
        /// </summary>
        public static string CreateSupplerOrder_FusionPCBCategoryCode = "Q";
        /// <summary>
        /// 挂起
        /// </summary>
        public static string Pending = "L";
        /// <summary>
        /// PCB生产中
        /// </summary>
        public static string PCBInProduction = "M";
        /// <summary>
        /// 有物流号可追踪
        /// </summary>
        public static string Order_Traceable = "N";
        /// <summary>
        /// Processing
        /// </summary>
        public static string Order_Processing = "O";
        /// <summary>
        /// PCBA Awaiting Packing
        /// </summary>
        public static string Order_PCBAFinished = "P";
        /// <summary>
        /// Awaiting Dispatching
        /// </summary>
        public static string Order_AwaitingDispatching = "R";
        /// <summary>
        /// Shipped
        /// </summary>
        public static string Order_Shipped = "S";
        /// <summary>
        /// Cancel
        /// </summary>
        public static string Order_Cancel = "T";
        /// <summary>
        /// Confirmed
        /// </summary>
        public static string Order_Confirmed = "U";
        /// <summary>
        /// Order Pending
        /// </summary>
        public static string Order_Pending = "V";
        /// <summary>
        /// 根据状态编码获取对应的名称值
        /// </summary>
        /// <param name="StatusCode"></param>
        /// <returns></returns>
        public static string getStatusName(string StatusCode)
        {
            string sReturnResult = string.Empty;
            switch (StatusCode)
            {
                case"A":
                    sReturnResult = "Sales Order Approved";
                    break;
                case "B":
                    sReturnResult = "BOM Purchase Order Approved";
                    break;
                case "C":
                    sReturnResult = "Production Instock Submit";
                    break;
                case "D":
                    sReturnResult = "PCBA Awaiting Packing";
                    break;
                case "E":
                    sReturnResult = "PCB Delivery Time From Supplier";
                    break;
                case "F":
                    sReturnResult = "Stencil Awaiting Packing";
                    break;
                case "G":
                    sReturnResult = "BOM Awaiting Packing";
                    break;
                case "H":
                    sReturnResult = "PCB Awaiting Packing";
                    break;
                case "I":
                    sReturnResult = "Stencil Processing";
                    break;
                case "J":
                    sReturnResult = "BOM Processing";
                    break;
                case "Q":
                    sReturnResult = "PCB Processing";
                    break;
                case "L":
                    sReturnResult = "Awaiting Revised File";
                    break;
                case "M":
                    sReturnResult = "PCB In Production";
                    break;
                case "N":
                    sReturnResult = "Traceable";
                    break;
                case "O":
                    sReturnResult = "Processing";
                    break;
                case "P":
                    sReturnResult = "PCB Awaiting Packing";
                    break;
                case "R":
                    sReturnResult = "Awaiting Dispatching";
                    break;
                case "S":
                    sReturnResult = "Shipped";
                    break;
                case "T":
                    sReturnResult = "Cancel";
                    break;
                case "U":
                    sReturnResult = "Confirmed";
                    break;
                case "V":
                    sReturnResult = "Pending";
                    break;
                default:
                    break;
            }
            return sReturnResult;
        }
    }
    /// <summary>
    /// 同步Bazaar状态时的操作类型
    /// A:单头；B:单身
    /// </summary>
    public class BazaarStatusOperateType
    {
        /// <summary>
        /// 更新单头状态
        /// </summary>
        public static string Order = "A";
        /// <summary>
        /// 更新单身状态
        /// </summary>
        public static string Line = "B";
    }
    /// <summary>
    /// 同步Bazaar状态时的来源单据
    /// </summary>
    public class BazaarStatusSource
    {
        /// <summary>
        /// 销售订单审核
        /// </summary>
        public static string SalesOrderApproved = "销售订单审核";
        /// <summary>
        /// 采购订单审核
        /// </summary>
        public static string PurchaseOrderApproved = "采购订单审核";
        /// <summary>
        /// 采购订单创建
        /// </summary>
        public static string PurchaseOrderCreated = "采购订单创建";
        /// <summary>
        /// 生成入库单提交
        /// </summary>
        public static string ProductionInstockSubmit = "生成入库单提交";
        /// <summary>
        /// 生成入库单审核
        /// </summary>
        public static string ProductionInstockApproved = "生成入库单审核";
        /// <summary>
        /// 执行计划
        /// </summary>
        public static string Timer_GetSupplierDeliveryTime = "执行计划--获取供应商发货时间";
        /// <summary>
        /// 采购入库单审核
        /// </summary>
        public static string ReceivingOrderApproved = "采购入库单审核";
        /// <summary>
        /// 执行计划--采购订单下推供应商订单
        /// </summary>
        public static string Timer_CreateSupplierOrder = "执行计划--采购订单下推供应商订单";
        /// <summary>
        /// 执行计划--获取供应商订单状态
        /// </summary>
        public static string Timer_GetSupplierOrderStatus = "执行计划--获取供应商订单状态";
        /// <summary>
        /// Fusion采购异常单
        /// </summary>
        public static string FusionException = "Fusion采购异常单";
        /// <summary>
        /// 运单号导入
        /// </summary>
        public static string ImportTrackingNo = "运单号导入";
        /// <summary>
        /// 发货通知单审核
        /// </summary>
        public static string DeliveryNoticeApproved = "发货通知单审核";
        /// <summary>
        /// 生成订单行完工
        /// </summary>
        public static string MOLineFinished = "生成订单行完工";
        /// <summary>
        /// 生产订单行下达
        /// </summary>
        public static string MOLineRelease = "生产订单行下达";
        /// <summary>
        /// 销售出库单审核
        /// </summary>
        public static string SalesOutstockApproved = "销售出库单审核";
        /// <summary>
        /// 销售订单作废
        /// </summary>
        public static string SalesOrderCancel = "销售订单作废";
        /// <summary>
        /// 销售订单保存
        /// </summary>
        public static string SalesOrderSave= "销售订单保存";
        /// <summary>
        /// 采购订单明细手工操作
        /// </summary>
        public static string PurchaseOrder_Manual = "采购订单-手工挂起";
        /// <summary>
        /// 销售订单-手动关闭
        /// </summary>
        public static string SalesOrder_ManualClose = "销售订单-手动关闭";
    }

    /// <summary>
    /// 物料属性
    /// </summary>
    public class MaterialErpClsID
    {
        /// <summary>
        /// 外购
        /// </summary>
        public static string WaiGou = "1";
        /// <summary>
        /// 自制
        /// </summary>
        public static string ZiZhi = "2";
        /// <summary>
        /// 委外
        /// </summary>
        public static string WeiWai = "3";
    }
    
}
