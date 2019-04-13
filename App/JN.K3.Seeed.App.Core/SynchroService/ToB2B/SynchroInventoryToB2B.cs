using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hands.K3.SCM.App.Core.SynchroService.ToHC;
using Hands.K3.SCM.APP.Entity.EnumType;
using Hands.K3.SCM.APP.Entity.StructType;

namespace Hands.K3.SCM.App.Core.SynchroService.ToB2B
{
    public class SynchroInventoryToB2B : SynchroInventoryToHC
    {
        public override SynchroDirection Direction
        {
            get
            {
                return SynchroDirection.ToB2B;
            }
        }

        public override string GetSql()
        {
            string sql = string.Empty;

            if (IsConnectSuccess())
            {
                //全部可用库存
                if (IsFirstSynchro())
                {
                    sql = string.Format(@"/*dialect*/ declare @guidString nvarchar(50)
                                                    select @guidString=NewID()
                                                    select  e.FNAME productGroupName,k.Fnumber FSTOCKID,
                                                           b.FNUMBER,d.FNAME,d.FSPECIFICATION ,f.FNAME unitName	
	                                                       ,sum(a.FBASEQTY/h.fConvertnumerator) quantity,o.customerNumber as FCUSTID,o.F_HS_DropShipRequestToken
                                                    into #jskc	  
                                                    From  T_STK_INVENTORY a
                                                    inner join T_BD_MATERIAL b on a.FMATERIALID=b.FMATERIALID
                                                    inner join T_BD_MATERIAL_L d on b.FMATERIALID=d.FMATERIALID and d.FLOCALEID=2052
                                                    left join T_BD_MATERIALGROUP_L e on b.Fmaterialgroup=e.fid and e.FLOCALEID=2052
                                                    left join T_BD_unit_L f on a.FSTOCKUNITID=f.FUNITID and f.FLOCALEID=2052
                                                    left join T_BD_LOTMASTER g on a.fLot=g.flotID
                                                    left join T_BD_UNITCONVERTRATE h on a.FSTOCKUNITID=h.fcurrentUnitID and a.fbaseunitID=h.FdestUnitID
                                                    inner join T_BD_STOCK i on a.FSTOCKID=i.FSTOCKID
                                                    inner join T_BAS_ASSISTANTDATAENTRY_L j ON i.F_HS_DLC=j.FENTRYID and j.FLOCALEID=2052
                                                    inner join T_BAS_ASSISTANTDATAENTRY k ON j.FentryID=k.FentryID
													inner join (
													        SELECT distinct h.FNUMBER customerNumber , h.F_HS_DropShipRequestToken,g.FNUMBER DLCNumber 
															FROM HS_t_DropShipStockEntity a 
		                                                    inner join T_BD_STOCK c on a.F_HS_DropShipStock=c.FSTOCKID
                                                            inner join T_BAS_ASSISTANTDATAENTRY_L f ON c.F_HS_DLC=f.FENTRYID and f.FLOCALEID=2052
		                                                    inner join T_BAS_ASSISTANTDATAENTRY g ON f.FentryID=g.FentryID
															inner join T_BD_CUSTOMER h on a.FCUSTID=h.FCUSTID
		                                                    Where h.FNUMBER = '{0}'
		                                                    ) o on k.fnumber=o.DLCNumber
                                                    where   a.FBASEQTY>0 and  a.FSTOCKSTATUSID=10000 and i.F_HS_TJ='1'{1}
                                                    group by  e.FNAME,k.Fnumber,
                                                           b.FNUMBER,d.FNAME,d.FSPECIFICATION,f.FNAME,o.customerNumber,o.F_HS_DropShipRequestToken
                                                    order by e.FNAME desc

                                                    insert into tbl_store_availableQOH_ds(FCUSTID,F_HS_DropShipRequestToken,FSTOCKID,FNUMBER,FNAME,FSPECIFICATION ,unitName,availableQOH, updateTag , updateTime, guidString) 
                                                    select a.FCUSTID,a.F_HS_DropShipRequestToken,a.FSTOCKID,a.FNUMBER,a.FNAME,a.FSPECIFICATION , a.unitName, a.quantity-isnull(b.FQty ,0) availableQOH,'original',getDate(), @guidString
                                                    from #jskc a
                                                    left join 
		                                                    (SELECT d.FNUMBER,g.Fnumber fstockID,sum(c.FREMAINOUTQTY) FQty 	FROM T_SAL_ORDER a 
		                                                    INNER JOIN T_SAL_ORDERENTRY b ON a.FID=b.FID 
		                                                    inner join T_SAL_ORDERENTRY_R c on a.fid=c.fid and b.fentryID=c.fentryID
		                                                    INNER JOIN T_BD_MATERIAL d on b.FMATERIALID=d.FMATERIALID
		                                                    inner join T_BD_STOCK e on b.F_HS_STOCKID=e.FSTOCKID
                                                            inner join T_BAS_ASSISTANTDATAENTRY_L f ON e.F_HS_DLC=f.FENTRYID
		                                                    inner join T_BAS_ASSISTANTDATAENTRY g ON f.FentryID=g.FentryID
		                                                    Where a.FCLOSESTATUS<>'B' AND a.FCANCELSTATUS<>'B' AND b.FMRPCLOSESTATUS<>'B' and a.FDOCUMENTSTATUS<>'Z'
		                                                    group by d.FNUMBER,g.Fnumber
		                                                    ) b on a.fnumber=b.fnumber and a.fstockID=b.fstockID

                                                    where a.quantity-isnull(b.FQty ,0)>0 
                                                    insert into tbl_store_availableQOH_Log_ds(FCUSTID,F_HS_DropShipRequestToken,FSTOCKID,FNUMBER,FNAME,FSPECIFICATION ,unitName,availableQOH,updateTag,updateTime,guidString)
                                                    select FCUSTID,F_HS_DropShipRequestToken,FSTOCKID,FNUMBER,FNAME,FSPECIFICATION ,unitName,availableQOH,'original',getDate(),@guidString from tbl_store_availableQOH_ds where guidString=@guidString

                                                    select FCUSTID,F_HS_DropShipRequestToken,FSTOCKID,FNUMBER,FNAME,FSPECIFICATION ,unitName,availableQOH,guidString from tbl_store_availableQOH_ds where guidString=@guidString

                                                    delete from tbl_store_availableQOH_ds where guidString<>@guidString and updateTag='original'
                                                    delete from HS_T_synchroDataLog where success =0 and redisDBID={2}  and SynchroDataType='{3}'
                                                    drop table #jskc",  DataBaseConst.Param_AUB2B_customerID, Environment.NewLine + "and (b.F_HS_IsOil = '3' and b.F_HS_PRODUCTSTATUS <> 'SPTC')"
                                                    + Environment.NewLine + "or (len(b.FNUMBER) = 13" + string.Format(" and right(b.FNUMBER,3)= '{0}')", "o.F_HS_DropShipRequestToken"),this.RedisDbId,this.DataType);
                }
               
                else
                {
                    sql = string.Format(@"/*dialect*/	declare @guidString nvarchar(50)
                                                    select @guidString=NewID()
                                                    select  e.FNAME productGroupName,k.Fnumber FSTOCKID,
                                                           b.FNUMBER,d.FNAME,d.FSPECIFICATION ,f.FNAME unitName	
	                                                       ,sum(a.FBASEQTY/h.fConvertnumerator) quantity,o.customerNumber as FCUSTID,o.F_HS_DropShipRequestToken
                                                    into #jskc	  
                                                    From  T_STK_INVENTORY a
                                                    inner join T_BD_MATERIAL b on a.FMATERIALID=b.FMATERIALID
                                                    inner join T_BD_MATERIAL_L d on b.FMATERIALID=d.FMATERIALID and d.FLOCALEID=2052
                                                    left join T_BD_MATERIALGROUP_L e on b.Fmaterialgroup=e.fid and e.FLOCALEID=2052
                                                    left join T_BD_unit_L f on a.FSTOCKUNITID=f.FUNITID and f.FLOCALEID=2052
                                                    left join T_BD_LOTMASTER g on a.fLot=g.flotID
                                                    left join T_BD_UNITCONVERTRATE h on a.FSTOCKUNITID=h.fcurrentUnitID and a.fbaseunitID=h.FdestUnitID
                                                    inner join T_BD_STOCK i on a.FSTOCKID=i.FSTOCKID
                                                    inner join T_BAS_ASSISTANTDATAENTRY_L j ON i.F_HS_DLC=j.FENTRYID and j.FLOCALEID=2052
                                                    inner join T_BAS_ASSISTANTDATAENTRY k ON j.FentryID=k.FentryID
													inner join (
													        SELECT distinct h.FNUMBER customerNumber , h.F_HS_DropShipRequestToken,g.FNUMBER DLCNumber 
															FROM HS_t_DropShipStockEntity a 
		                                                    inner join T_BD_STOCK c on a.F_HS_DropShipStock=c.FSTOCKID
                                                            inner join T_BAS_ASSISTANTDATAENTRY_L f ON c.F_HS_DLC=f.FENTRYID and f.FLOCALEID=2052
		                                                    inner join T_BAS_ASSISTANTDATAENTRY g ON f.FentryID=g.FentryID
															inner join T_BD_CUSTOMER h on a.FCUSTID=h.FCUSTID
		                                                    Where h.FNUMBER = '{0}'
		                                                    ) o on k.fnumber=o.DLCNumber
                                                    where   a.FBASEQTY>0 and  a.FSTOCKSTATUSID=10000 and i.F_HS_TJ='1'{1}
                                                    group by  e.FNAME,k.Fnumber,
                                                           b.FNUMBER,d.FNAME,d.FSPECIFICATION,f.FNAME,o.customerNumber,o.F_HS_DropShipRequestToken
                                                    order by e.FNAME desc

                                                    select a.FCUSTID,a.F_HS_DropShipRequestToken,a.FSTOCKID, a.FNUMBER, a.FNAME, a.FSPECIFICATION, a.unitName, (case when a.quantity - isnull(b.FQty, 0) < 0 then 0 else a.quantity - isnull(b.FQty, 0) end) availableQOH
                                                            into #tbl_store_availableQOH_ds
                                                    from #jskc a
                                                    left join
                                                            (SELECT d.FNUMBER,g.Fnumber fstockID, sum(c.FREMAINOUTQTY) FQty FROM T_SAL_ORDER a
                                                            INNER JOIN T_SAL_ORDERENTRY b ON a.FID = b.FID
                                                            inner join T_SAL_ORDERENTRY_R c on a.fid = c.fid and b.fentryID = c.fentryID
                                                            INNER JOIN T_BD_MATERIAL d on b.FMATERIALID = d.FMATERIALID
                                                            inner join T_BD_STOCK e on b.F_HS_STOCKID = e.FSTOCKID
                                                            inner join T_BAS_ASSISTANTDATAENTRY_L f ON e.F_HS_DLC = f.FENTRYID
                                                            inner join T_BAS_ASSISTANTDATAENTRY g ON f.FentryID = g.FentryID
                                                            Where a.FCLOSESTATUS <> 'B' AND a.FCANCELSTATUS <> 'B' AND b.FMRPCLOSESTATUS <> 'B' and a.FDOCUMENTSTATUS <> 'Z'
                                                            group by d.FNUMBER,g.Fnumber
		                                                    ) b on a.fnumber = b.fnumber and a.fstockID = b.fstockID


                                                            update tbl_store_availableQOH_ds
                                                            set availableQOH = b.availableQOH, updateTag = 'update', updateTime = getDate(), guidString = @guidString, unitName = b.unitName
                                                            from tbl_store_availableQOH_ds a
                                                            inner join #tbl_store_availableQOH_ds b on a.fnumber=b.fnumber and a.fstockID=b.fstockID
                                                            where a.availableQOH <> b.availableQOH

                                                            insert into tbl_store_availableQOH_ds(FCUSTID,F_HS_DropShipRequestToken,FSTOCKID, FNUMBER, FNAME, FSPECIFICATION, unitName, availableQOH, updateTag, updateTime, guidString)
                                                            select t.* ,'insert',getDate(),@guidString From #tbl_store_availableQOH_ds  t
                                                            where not exists(select * From tbl_store_availableQOH_ds where fnumber = t.fnumber and fstockID = t.fstockID)


                                                            select FCUSTID,F_HS_DropShipRequestToken,FSTOCKID, FNUMBER, FNAME, FSPECIFICATION, unitName, availableQOH from tbl_store_availableQOH_ds
                                                                where updateTag in ('update', 'insert')  and guidString = @guidString

                                                            insert into tbl_store_availableQOH_Log_ds(FCUSTID,F_HS_DropShipRequestToken,FSTOCKID, FNUMBER, FNAME, FSPECIFICATION, unitName, availableQOH, updateTag, updateTime, guidString)
                                                            select FCUSTID,F_HS_DropShipRequestToken,FSTOCKID, FNUMBER, FNAME, FSPECIFICATION, unitName, availableQOH, updateTag, updateTime, guidString from tbl_store_availableQOH_ds
                                                                   where updateTag in ('update', 'insert')  and guidString = @guidString

                                                            update tbl_store_availableQOH_ds set updateTag = null where updateTag in ('update', 'insert') and guidString = @guidString

                                                            drop table #jskc
                                                            drop table #tbl_store_availableQOH_ds", DataBaseConst.Param_AUB2B_customerID, Environment.NewLine + "and (b.F_HS_IsOil = '3' and b.F_HS_PRODUCTSTATUS <> 'SPTC')"
                                                        + Environment.NewLine + "or (len(b.FNUMBER) = 13" + string.Format(" and right(b.FNUMBER,3)= '{0}')", "o.F_HS_DropShipRequestToken"));
                }
            }

            return sql;
        }
    }
}
