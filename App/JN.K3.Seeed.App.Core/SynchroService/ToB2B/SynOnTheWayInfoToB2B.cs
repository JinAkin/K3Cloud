using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hands.K3.SCM.App.Core.SynchroService.ToHC;
using Hands.K3.SCM.APP.Entity.EnumType;
using Hands.K3.SCM.APP.Entity.StructType;
using HS.K3.Common.Abbott;

namespace Hands.K3.SCM.App.Core.SynchroService.ToB2B
{
    public class SynOnTheWayInfoToB2B: SynOnTheWayInfo
    {
        public override SynchroDirection Direction
        {
            get
            {
                return SynchroDirection.ToB2B;
            }
        }

        public override string GetSQL()
        {
            return string.Format(@"/*dialect*/ select t1.FNUMBER as FMaterialId,m1.FDeliveryDate,m1.FQTY,m1.FStockId
				                    from T_BD_MATERIAL t1
				                    inner join (
					                    select t5.FNUMBER,Convert(nvarchar(100),ISNULL(t1.F_HS_DUEDATE,''),23) as FDeliveryDate,
					                    (t2.FQTY-t3.FRECEIVEQTY-t4.FJOINPATHLOSSQTY) as FQTY,
					                    ISNULL(t8.FNUMBER,'') as FStockId
					                    from T_STK_STKTRANSFEROUT t1
					                    inner join T_STK_STKTRANSFEROUTENTRY t2 on t1.FID=t2.FID
					                    inner join T_STK_STKTRANSFEROUTENTRY_R t3 on t2.FENTRYID=t3.FENTRYID
					                    inner join T_STK_STKTRANSFEROUTENTRY_T t4 on t2.FENTRYID=t4.FENTRYID
										inner join T_SAL_DELIVERYNOTICEENTRY_LK t9 on t9.FENTRYID = t2.FENTRYID
										inner join T_SAL_OrderEntry t10 on t9.FSBILLID = t10.FID and t9.FSID = t10.FENTRYID
										inner join T_SAL_Order t11 on t11.FID = t10.FID
					                    inner join T_BD_MATERIAL t5 on t2.FMATERIALID=t5.FMATERIALID
					                    left join T_BD_STOCK t6 on t2.FDESTSTOCKID=t6.FSTOCKID
										inner join T_BAS_ASSISTANTDATAENTRY_L t7 ON t6.F_HS_DLC=t7.FENTRYID
                                        inner join T_BAS_ASSISTANTDATAENTRY t8 ON t8.FENTRYID=t7.FENTRYID
										inner join T_BAS_ASSISTANTDATAENTRY_L t12 ON t11.F_HS_ORDERSOURCE=t12.FENTRYID
                                        inner join T_BAS_ASSISTANTDATAENTRY t13 ON t3.FENTRYID=t2.FENTRYID
										inner join T_BD_CUSTOMER t14 on t14.FCUSTID = t11.FCUSTID
		                                where t1.FDOCUMENTSTATUS='C' and t1.FCANCELSTATUS<>'B'
					                    and t2.FQTY > t3.FRECEIVEQTY+t4.FJOINPATHLOSSQTY and t1.FVESTONWAY='B'
										and t1.F_HS_ConfirmDeliveryDate = '1'
										and t5.FNUMBER not like '99.%'
                                        and t6.F_HS_TJ = '1'
										and t13.FNUMBER = 'DropShippingB2BOrder'
										and t14.FNUMBER = '{0}'
                                      )m1 on m1.FNUMBER=t1.FNUMBER and t1.FMASTERID=t1.FMATERIALID", DataBaseConst.Param_AUB2B_customerID
                                   );

        }
    }
}
