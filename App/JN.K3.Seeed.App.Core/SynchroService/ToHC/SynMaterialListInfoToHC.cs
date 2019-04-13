using System.Collections.Generic;
using Kingdee.BOS.Orm.DataEntity;
using Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject;
using Kingdee.BOS.ServiceHelper;
using Hands.K3.SCM.APP.Utils.Utils;
using Hands.K3.SCM.App.Synchro.Base.Abstract;
using HS.K3.Common.Abbott;
using Hands.K3.SCM.APP.Entity.SynDataObject.Material_;

namespace Hands.K3.SCM.App.Core.SynchroService.ToHC
{
    class SynMaterialListInfoToHC : AbstractSynchroToHC
    {
        public override SynchroDataType DataType
        {
            get
            {
                return SynchroDataType.SynchroListInfo;
            }
        }
        public List<ListInfo> GetListInfos()
        {
            if (!string.IsNullOrWhiteSpace(GetSql()))
            {
                DynamicObjectCollection collection = DBServiceHelper.ExecuteDynamicObject(this.K3CloudContext, GetSql());
                List<ListInfo> infos = new List<ListInfo>();

                ListInfo info = default(ListInfo);

                if (collection != null)
                {
                    if (collection.Count > 0)
                    {
                        foreach (var item in collection)
                        {
                            info = new ListInfo();

                            info.SrcNo = SQLUtils.GetFieldValue(item, "F_HS_ListID");
                            info.F_HS_ListID = SQLUtils.GetFieldValue(item, "F_HS_ListID");
                            info.F_HS_ListName = SQLUtils.GetFieldValue(item, "F_HS_LISTNAME");
                            
                            //info.F_HS_BatteryMod = SQLUtils.GetFieldValue(item, "F_HS_BatteryMod");
                            //info.F_HS_IsPuHuo = SQLUtils.GetFieldValue(item, "F_HS_IsPuHuo");
                            //info.F_HS_IsOil = SQLUtils.GetFieldValue(item, "F_HS_IsOil");

                            infos.Add(info);
                        }
                    }

                    return infos;
                }
            }

            return default(List<ListInfo>);
        }
        private string GetSql()
        {
            if (IsConnectSuccess())
            {
                if (IsFirstSynchro())
                {
                    return string.Format(@"
											/*dialect*/declare @guidString nvarchar(50)
                                            select @guidString = NEWID()

                                            insert into HS_T_SynMaterialListInfo(F_HS_LISTID,F_HS_LISTNAME,F_HS_BatteryMod,F_HS_IsPuHuo,F_HS_IsOil,F_HS_PRODUCTSTATUS,
                                            FGROSSWEIGHT,F_HS_BasePrice,F_HS_ProductLimitState,F_HS_ExtraCommissionCountry,F_HS_BasePriceCountry,
                                            UpdateTag,UpdateTime,GuidString)

                                            select  a.F_HS_LISTID,F_HS_LISTNAME,F_HS_BatteryMod,F_HS_IsPuHuo,F_HS_IsOil, i.FNUMBER as F_HS_PRODUCTSTATUS   
													,max(FGROSSWEIGHT) FGROSSWEIGHT,F_HS_BasePrice,F_HS_ProductLimitState,F_HS_ExtraCommissionCountry
													,F_HS_BasePriceCountry,'original',getDate(), @guidString
                                            from T_BD_MATERIAL a 
                                            inner join T_BD_MATERIAL_L b on b.FMATERIALID = a.FMATERIALID and b.FLOCALEID=2052
                                            inner join T_BD_MATERIALBASE c on c.FMATERIALID = a.FMATERIALID 
                                            inner join T_BD_MATERIALSALE d on d.FMATERIALID = a.FMATERIALID
                                            inner join T_BD_MATERIALSTOCK e on e.FMATERIALID = a.FMATERIALID
                                            inner join T_ORG_ORGANIZATIONS f on a.FUSEORGID=f.FORGID
                                            left join T_BAS_ASSISTANTDATAENTRY_L h on a.F_HS_PRODUCTSTATUS = h.FENTRYID and h.flocaleid=2052
                                            left join T_BAS_ASSISTANTDATAENTRY i on h.FentryID=i.FentryID 
                                            where f.FNUMBER='100' and f_Hs_ListID<>'' and a.FNUMBER not like '%99%' 
                                            group by a.F_HS_LISTID,F_HS_LISTNAME,
	                                            F_HS_BatteryMod,F_HS_IsPuHuo,F_HS_IsOil,i.FNUMBER
	                                            ,F_HS_BasePrice,F_HS_ProductLimitState,F_HS_ExtraCommissionCountry,F_HS_BasePriceCountry

                                            insert into HS_T_SynMaterialListInfoLog(F_HS_LISTID,F_HS_LISTNAME,F_HS_BatteryMod,F_HS_IsPuHuo,F_HS_IsOil,F_HS_PRODUCTSTATUS,
                                            FGROSSWEIGHT,F_HS_BasePrice,F_HS_ProductLimitState,F_HS_ExtraCommissionCountry,F_HS_BasePriceCountry,
                                            UpdateTag,UpdateTime,GuidString)

                                            select F_HS_LISTID,F_HS_LISTNAME,F_HS_BatteryMod,F_HS_IsPuHuo,F_HS_IsOil,F_HS_PRODUCTSTATUS,
                                            FGROSSWEIGHT,isnull(F_HS_BasePrice,0),F_HS_ProductLimitState,F_HS_ExtraCommissionCountry,F_HS_BasePriceCountry,
                                            UpdateTag,UpdateTime,GuidString from HS_T_SynMaterialListInfo

                                            select F_HS_LISTID,F_HS_LISTNAME,F_HS_BatteryMod,F_HS_IsPuHuo,F_HS_IsOil,F_HS_PRODUCTSTATUS,FGROSSWEIGHT,
                                            F_HS_BasePrice,F_HS_ProductLimitState,F_HS_ExtraCommissionCountry,F_HS_BasePriceCountry from HS_T_SynMaterialListInfo where GuidString=@guidString

                                            delete from HS_T_SynMaterialListInfo where GuidString != @guidString and UpdateTag = 'original'
	                                ");
                }

                return string.Format(@"/*dialect*/ declare @guidString nvarchar(50)
                                        select @guidString = NEWID()

                                        select  a.F_HS_LISTID,F_HS_LISTNAME,F_HS_BatteryMod,F_HS_IsPuHuo,F_HS_IsOil, i.FNUMBER as F_HS_PRODUCTSTATUS
                                        ,max(FGROSSWEIGHT) FGROSSWEIGHT,F_HS_ProductLimitState,F_HS_BasePrice,F_HS_ExtraCommissionCountry,F_HS_BasePriceCountry
                                        into #HS_T_SynMaterialListInfo
                                        from T_BD_MATERIAL a 
                                        inner join T_BD_MATERIAL_L b on b.FMATERIALID = a.FMATERIALID and b.FLOCALEID=2052
                                        inner join T_BD_MATERIALBASE c on c.FMATERIALID = a.FMATERIALID 
                                        inner join T_BD_MATERIALSALE d on d.FMATERIALID = a.FMATERIALID
                                        inner join T_BD_MATERIALSTOCK e on e.FMATERIALID = a.FMATERIALID
                                        inner join T_ORG_ORGANIZATIONS f on a.FUSEORGID=f.FORGID
                                        left join T_BAS_ASSISTANTDATAENTRY_L h on a.F_HS_PRODUCTSTATUS = h.FENTRYID and h.flocaleid=2052
                                        left join T_BAS_ASSISTANTDATAENTRY i on h.FentryID=i.FentryID 
                                        where f.FNUMBER='100' and f_Hs_ListID<>'' and a.FNUMBER not like '%99%'
                                        group by a.F_HS_LISTID,F_HS_LISTNAME,
	                                        F_HS_BatteryMod,F_HS_IsPuHuo,F_HS_IsOil,i.FNUMBER
	                                        ,F_HS_BasePrice,F_HS_ProductLimitState,F_HS_ExtraCommissionCountry,F_HS_BasePriceCountry

                                        update  HS_T_SynMaterialListInfo set F_HS_LISTID = b.F_HS_LISTID,F_HS_LISTNAME = b.F_HS_LISTNAME,F_HS_BatteryMod = b.F_HS_BatteryMod,F_HS_IsPuHuo = b.F_HS_IsPuHuo,
                                        F_HS_IsOil = b.F_HS_IsOil,F_HS_PRODUCTSTATUS = b.F_HS_PRODUCTSTATUS,FGROSSWEIGHT = b.FGROSSWEIGHT
                                        , F_HS_BasePrice = b.F_HS_BasePrice,F_HS_ProductLimitState = b.F_HS_ProductLimitState,F_HS_ExtraCommissionCountry = b.F_HS_ExtraCommissionCountry,F_HS_BasePriceCountry = b.F_HS_BasePriceCountry     
                                        ,UpdateTag = 'update',UpdateTime = GETDATE(),GuidString = @guidString
                                        from HS_T_SynMaterialListInfo a 
                                        inner join #HS_T_SynMaterialListInfo b on a.F_HS_LISTID = b.F_HS_LISTID
                                        where a.F_HS_LISTID <> b.F_HS_LISTID or a.F_HS_LISTNAME <> b.F_HS_LISTNAME or a.F_HS_BatteryMod <> b.F_HS_BatteryMod or a.F_HS_IsPuHuo <> b.F_HS_IsPuHuo
                                        or a.F_HS_IsOil <> b.F_HS_IsOil or a.F_HS_PRODUCTSTATUS <> b.F_HS_PRODUCTSTATUS or a.FGROSSWEIGHT <> b.FGROSSWEIGHT
                                        or a.F_HS_BasePrice <> b.F_HS_BasePrice or a.F_HS_ProductLimitState <> b.F_HS_ProductLimitState or a.F_HS_ExtraCommissionCountry <> b.F_HS_ExtraCommissionCountry
                                        or a.F_HS_BasePriceCountry <> b.F_HS_BasePriceCountry

                                        insert into HS_T_SynMaterialListInfo(F_HS_LISTID,F_HS_LISTNAME,F_HS_BatteryMod,F_HS_IsPuHuo,F_HS_IsOil,F_HS_PRODUCTSTATUS,FGROSSWEIGHT,
                                        F_HS_BasePrice,F_HS_ProductLimitState,F_HS_ExtraCommissionCountry,F_HS_BasePriceCountry,UpdateTag,UpdateTime,GuidString)

                                        select b.* ,'insert',getDate(),@guidString from  #HS_T_SynMaterialListInfo b 
                                        where not exists(select * from HS_T_SynMaterialListInfo where F_HS_LISTID = b.F_HS_LISTID)
		                                        
                                        select F_HS_LISTID,F_HS_LISTNAME,F_HS_BatteryMod,F_HS_IsPuHuo,F_HS_IsOil,F_HS_PRODUCTSTATUS,FGROSSWEIGHT,F_HS_BasePrice,F_HS_ProductLimitState
											   ,F_HS_ExtraCommissionCountry,F_HS_BasePriceCountry 
										from HS_T_SynMaterialListInfo 
                                        where UpdateTag in ('update','insert') and GuidString = @guidString 
													   
                                        insert into HS_T_SynMaterialListInfoLog(F_HS_LISTID,F_HS_LISTNAME,F_HS_BatteryMod,F_HS_IsPuHuo,F_HS_IsOil,F_HS_PRODUCTSTATUS,FGROSSWEIGHT, 
                                        F_HS_BasePrice,F_HS_ProductLimitState,F_HS_ExtraCommissionCountry,F_HS_BasePriceCountry,UpdateTag,UpdateTime,GuidString)
                                        
                                        select F_HS_LISTID,F_HS_LISTNAME,F_HS_BatteryMod,F_HS_IsPuHuo,F_HS_IsOil,F_HS_PRODUCTSTATUS,FGROSSWEIGHT, 
											   isnull(F_HS_BasePrice,0),F_HS_ProductLimitState,F_HS_ExtraCommissionCountry,F_HS_BasePriceCountry,UpdateTag,UpdateTime,GuidString
										from HS_T_SynMaterialListInfo 
										where UpdateTag in ('update','insert') and GuidString = @guidString

                                        update HS_T_SynMaterialListInfo set UpdateTag = null where updateTag in ('update','insert') and guidString=@guidString
                                        drop table #HS_T_SynMaterialListInfo																	
							");
            }

            return null;
        }

        private bool IsFirstSynchro()
        {
            string sql = string.Format(@"/*dialect*/ select * From  HS_T_SynMaterialListInfo");
            DynamicObjectCollection items = SQLUtils.GetObjects(this.K3CloudContext, sql);

            if (items == null || items.Count == 0)
            {
                return true;
            }
            return false;
        }
        public override IEnumerable<AbsSynchroDataInfo> GetK3Datas(IEnumerable<string> billNos = null,bool flag = true)
        {
            return GetListInfos();
        }

    }
}
