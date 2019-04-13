
using Hands.K3.SCM.APP.Entity.SynDataObject;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using System;
using System.Collections.Generic;
using Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject;
using Hands.K3.SCM.APP.Utils.Utils;
using Hands.K3.SCM.App.Synchro.Base.Abstract;
using HS.K3.Common.Abbott;
using Hands.K3.SCM.APP.Entity.EnumType;
using Hands.K3.SCM.APP.Entity.StructType;

namespace Hands.K3.SCM.App.Core.SynchroService.ToHC
{

    public class SynchroInventoryToHC : AbstractSynchroToHC
    {

        override public SynchroDataType DataType
        {
            get
            {
                return SynchroDataType.Inventroy;
            }
        }

        /// <summary>
        /// 获取库存数据
        /// </summary>
        /// <returns></returns>
        public List<InventoryInfo> GetInventorys()
        {
            string sql = GetSql();

            if (!string.IsNullOrWhiteSpace(sql))
            {
                DynamicObjectCollection collection = DBServiceHelper.ExecuteDynamicObject(this.K3CloudContext, sql);
                InventoryInfo inventory = default(InventoryInfo);
                List<InventoryInfo> inventories = new List<InventoryInfo>();

                if (collection != null)
                {
                    if (collection.Count > 0)
                    {
                        foreach (var item in collection)
                        {
                            inventory = new InventoryInfo();

                            inventory.StockId = SQLUtils.GetFieldValue(item,"FSTOCKID");
                            inventory.SrcNo = SQLUtils.GetFieldValue(item, "FNUMBER");
                            inventory.FixId = SQLUtils.GetFieldValue(item, "FNUMBER");
                            inventory.Quantity = Convert.ToDouble(SQLUtils.GetFieldValue(item, "availableQOH"));

                            inventories.Add(inventory);
                        }
                    }

                    return inventories;
                }
            }
            
            return default(List<InventoryInfo>);
        }

        public virtual string GetSql()
        {
            DataBaseConst.K3CloudContext = this.K3CloudContext;

            if (IsConnectSuccess())
            {
                //全部可用库存
                if (IsFirstSynchro())
                {
                    return string.Format(@"/*dialect*/ declare @guidString nvarchar(50)
                                            select @guidString=NewID()
                                            select  e.FNAME productGroupName,k.Fnumber FSTOCKID,
                                                   b.FNUMBER,d.FNAME,d.FSPECIFICATION ,f.FNAME unitName	
	                                               ,sum(a.FBASEQTY/h.fConvertnumerator) quantity
                                            into #jskc	  
                                            From  T_STK_INVENTORY a
                                            inner join T_BD_MATERIAL b on a.FMATERIALID=b.FMATERIALID
                                            inner join T_BD_MATERIAL_L d on b.FMATERIALID=d.FMATERIALID and d.FLOCALEID=2052
                                            left join T_BD_MATERIALGROUP_L e on b.Fmaterialgroup=e.fid and e.FLOCALEID=2052
                                            left join T_BD_unit_L f on a.FSTOCKUNITID=f.FUNITID and f.FLOCALEID=2052
                                            left join T_BD_LOTMASTER g on a.fLot=g.flotID
                                            left join T_BD_UNITCONVERTRATE h on a.FSTOCKUNITID=h.fcurrentUnitID and a.fbaseunitID=h.FdestUnitID
                                            inner join T_BD_STOCK i on a.FSTOCKID=i.FSTOCKID
                                            inner join T_BAS_ASSISTANTDATAENTRY_L j ON i.F_HS_DLC=j.FENTRYID
                                            inner join T_BAS_ASSISTANTDATAENTRY k ON j.FentryID=k.FentryID
                                            where   a.FBASEQTY>0 and  a.FSTOCKSTATUSID=10000 and i.F_HS_TJ='1'
                                            group by  e.FNAME,k.Fnumber,
                                                   b.FNUMBER,d.FNAME,d.FSPECIFICATION,f.FNAME
                                            order by e.FNAME desc

                                            insert into tbl_store_availableQOH(FSTOCKID,FNUMBER,FNAME,FSPECIFICATION ,unitName,availableQOH, updateTag , updateTime, guidString) 
                                            select a.FSTOCKID,a.FNUMBER,a.FNAME,a.FSPECIFICATION , a.unitName, a.quantity-isnull(b.FQty ,0) availableQOH,'original',getDate(), @guidString
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
                                            insert into tbl_store_availableQOH_Log(FSTOCKID,FNUMBER,FNAME,FSPECIFICATION ,unitName,availableQOH,updateTag,updateTime,guidString)
                                            select FSTOCKID,FNUMBER,FNAME,FSPECIFICATION ,unitName,availableQOH,'original',getDate(),@guidString from tbl_store_availableQOH where guidString=@guidString

                                            select FSTOCKID,FNUMBER,FNAME,FSPECIFICATION ,unitName,availableQOH,guidString from tbl_store_availableQOH where guidString=@guidString

                                            delete from tbl_store_availableQOH where guidString<>@guidString and updateTag='original'
                                            delete from HS_T_synchroDataLog where success =0 and redisDBID={0}  and SynchroDataType='{1}'

                                            drop table #jskc

                                    ",this.RedisDbId,this.DataType);

                }

                //随时变化的可用库存
                return string.Format(@"/*dialect*/ declare @guidString nvarchar(50)
                                    select @guidString=NewID()
                                    select e.FNAME productGroupName,k.Fnumber FSTOCKID,
                                           b.FNUMBER,d.FNAME,d.FSPECIFICATION ,f.FNAME unitName	
	                                       ,sum(a.FBASEQTY/h.fConvertnumerator) quantity
                                    into #jskc	  
                                    From  T_STK_INVENTORY a
                                    inner join T_BD_MATERIAL b on a.FMATERIALID=b.FMATERIALID
                                    inner join T_BD_MATERIAL_L d on b.FMATERIALID=d.FMATERIALID and d.FLOCALEID=2052
                                    left join T_BD_MATERIALGROUP_L e on b.Fmaterialgroup=e.fid and e.FLOCALEID=2052
                                    left join T_BD_unit_L f on a.FSTOCKUNITID=f.FUNITID and f.FLOCALEID=2052
                                    left join T_BD_LOTMASTER g on a.fLot=g.flotID
                                    left join T_BD_UNITCONVERTRATE h on a.FSTOCKUNITID=h.fcurrentUnitID and a.fbaseunitID=h.FdestUnitID
                                    inner join T_BD_STOCK i on a.FSTOCKID=i.FSTOCKID
                                    inner join T_BAS_ASSISTANTDATAENTRY_L j ON i.F_HS_DLC=j.FENTRYID
                                    inner join T_BAS_ASSISTANTDATAENTRY k ON j.FentryID=k.FentryID
                                    where a.FSTOCKSTATUSID=10000 and i.F_HS_TJ='1'
                                    group by  e.FNAME,k.Fnumber,
                                           b.FNUMBER,d.FNAME,d.FSPECIFICATION,f.FNAME
                                    order by e.FNAME desc


                                    select a.FSTOCKID,a.FNUMBER,a.FNAME,a.FSPECIFICATION , a.unitName, (case when a.quantity-isnull(b.FQty ,0)<0 then 0 else a.quantity-isnull(b.FQty ,0) end) availableQOH
                                    into #tbl_store_availableQOH
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


                                    update tbl_store_availableQOH
                                    set   availableQOH=b.availableQOH,  updateTag='update',updateTime=getDate(),guidString=@guidString,unitName=b.unitName
                                    from tbl_store_availableQOH a
                                    inner join #tbl_store_availableQOH b on a.fnumber=b.fnumber and a.fstockID=b.fstockID
                                    where a.availableQOH<>b.availableQOH

                                    insert into tbl_store_availableQOH(FSTOCKID,FNUMBER,FNAME,FSPECIFICATION ,unitName,availableQOH,updateTag,updateTime,guidString)
                                    select t.* ,'insert',getDate(),@guidString From #tbl_store_availableQOH  t
                                    where not exists(select * From tbl_store_availableQOH where fnumber=t.fnumber and fstockID=t.fstockID)


                                    select FSTOCKID,FNUMBER,FNAME,FSPECIFICATION ,unitName,availableQOH from tbl_store_availableQOH
                                    where updateTag in ('update','insert')  and guidString=@guidString

                                    insert into tbl_store_availableQOH_Log(FSTOCKID,FNUMBER,FNAME,FSPECIFICATION ,unitName,availableQOH,updateTag,updateTime,guidString)
                                    select FSTOCKID,FNUMBER,FNAME,FSPECIFICATION ,unitName,availableQOH,updateTag,updateTime,guidString from tbl_store_availableQOH
                                    where updateTag in ('update','insert')  and guidString=@guidString

                                    update tbl_store_availableQOH set updateTag=null where updateTag in ('update','insert') and guidString=@guidString

                                    drop table #jskc
                                    drop table #tbl_store_availableQOH
                                ");

            }
            return null;

        }

        public override IEnumerable<AbsSynchroDataInfo> GetK3Datas(IEnumerable<string> billNos,bool flag = true)
        {
            return GetInventorys();
        }

        /// <summary>
        /// 判断全部可用库存是否已经同步，全部库存在第一次同步后再也不同步
        /// </summary>
        /// <returns></returns>
        public bool IsFirstSynchro()
        {
            string sql = string.Format(@"/*dialect*/ select * From  tbl_store_availableQOH ");
            DynamicObjectCollection items = SQLUtils.GetObjects(this.K3CloudContext, sql);

            if (items == null || items.Count == 0)
            {
                return true;
            }
            return false;
        }

    }
}
