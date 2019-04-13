using Kingdee.BOS.Orm.DataEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject;
using Hands.K3.SCM.APP.Utils.Utils;
using Hands.K3.SCM.App.Synchro.Base.Abstract;
using HS.K3.Common.Abbott;
using Hands.K3.SCM.APP.Entity.SynDataObject.Material_;

namespace Hands.K3.SCM.App.Core.SynchroService.ToHC
{
    public class SynMaterialToHC : AbstractSynchroToHC
    {
        public override SynchroDataType DataType
        {
            get { return SynchroDataType.Material; }
        }

        public List<ListInfo> GetMaterials()
        {
            List<ListInfo> lists = null;
            ListInfo list = null;

            Material material = null;
            List<Material> materials = null;

            if (!string.IsNullOrWhiteSpace(GetSql()))
            {
                //DynamicObjectCollection coll = DBServiceHelper.ExecuteDynamicObject(this.K3CloudContext, GetSql());
                DynamicObjectCollection coll = null;
                if (coll != null)
                {
                    if (coll.Count > 0)
                    {
                        var group = from c in coll
                                    where c["F_HS_LISTID"] != null && SQLUtils.GetFieldValue(c, "F_HS_PRODUCTSTATUS").CompareTo("SPXJ") != 0
                                    group c by c["F_HS_LISTID"]
                                    into g
                                    select g;

                        lists = new List<ListInfo>();
                       
                        if(group != null && group.Count() > 0)
                        {
                            foreach (var gr in group)
                            {
                                if (gr != null)
                                {
                                    list = new ListInfo();

                                    list.SrcNo = SQLUtils.GetFieldValue(gr.ElementAt(0), "F_HS_LISTID");
                                    list.F_HS_ListID = SQLUtils.GetFieldValue(gr.ElementAt(0), "F_HS_LISTID");
                                    list.F_HS_ListName = SQLUtils.GetFieldValue(gr.ElementAt(0), "F_HS_ListName");
                                    
                                    foreach (var item in gr)
                                    {
                                        materials = new List<Material>();

                                        if (item != null)
                                        {
                                            material = new Material();
                                            material.FNumber = SQLUtils.GetFieldValue(item, "FNumber");

                                            material.F_HS_IsPuHuo = SQLUtils.GetFieldValue(item, "F_HS_IsPuHuo");
                                            material.F_HS_BatteryMod = SQLUtils.GetFieldValue(item, "F_HS_BatteryMod");
                                            material.F_HS_IsOil = SQLUtils.GetFieldValue(item, "F_HS_IsOil");

                                            material.FSaleUnitId = SQLUtils.GetFieldValue(item, "UnitName");
                                            material.FGROSSWEIGHT = Convert.ToDecimal(SQLUtils.GetFieldValue(item, "F_HS_GROSSWEIGHT"));
                                            material.F_HS_ProductLimitState = SQLUtils.GetFieldValue(item, "F_HS_ProductLimitState");
                                            material.F_HS_ProductSKU = SQLUtils.GetFieldValue(item, "F_HS_ProductSKU");
                                            material.F_HS_SKUGroupCode = SQLUtils.GetFieldValue(item, "F_HS_SKUGroupCode");

                                            material.F_HS_Specification1 = SQLUtils.GetFieldValue(item, "F_HS_Specification1");
                                            material.F_HS_Specification2 = SQLUtils.GetFieldValue(item, "F_HS_Specification2");

                                            material.F_HS_BrandNumber = SQLUtils.GetFieldValue(item, "F_HS_BrandNumber");
                                            material.F_HS_BrandName = SQLUtils.GetFieldValue(item, "F_HS_BrandName");
                                            material.F_HS_PRODUCTSTATUS = SQLUtils.GetFieldValue(item, "F_HS_PRODUCTSTATUS");

                                            material.F_HS_ExtraPercentage = Convert.ToDecimal(SQLUtils.GetFieldValue(item, "F_HS_ExtraPercentage"));
                                            material.F_HS_ExtraCommissionCountry = SQLUtils.GetFieldValue(item, "F_HS_ExtraCommissionCountry");
                                            material.F_HS_BasePrice = Convert.ToDecimal(SQLUtils.GetFieldValue(item, "F_HS_BasePrice"));
                                            material.F_HS_BasePriceCountry = SQLUtils.GetFieldValue(item, "F_HS_BasePriceCountry");
                                            material.F_HS_FreeMailMod = SQLUtils.GetFieldValue(item, "F_HS_FreeMailMod").CompareTo("1") == 0 ? true : false;
                                            material.F_HS_IsOnSale = SQLUtils.GetFieldValue(item, "F_HS_IsOnSale").CompareTo("1") == 0?true:false;


                                            materials.Add(material);
                                        }
                                    }

                                    list.Materials = materials;
                                }
                                lists.Add(list);
                            }
                        }
                        
                    }   
                }
            }
            return lists;
        }

        public string GetSql()
        {
            if (IsConnectSuccess())
            {
                if (IsFirstSynchro())
                {
                    return string.Format(@"/*dialect*/ declare @guidString nvarchar(50)
                                            select @guidString = NEWID()

                                            insert into HS_T_SynMaterial(FNumber,FName,FSaleUnitId,FSpecification,UnitName,F_HS_ProductSKU,F_HS_SKUGroupCode,F_HS_Specification1,F_HS_Specification2,F_HS_BrandNumber,F_HS_BrandName 
                                            ,F_HS_PRODUCTSTATUS,F_HS_LISTID,F_HS_LISTNAME,F_HS_GROSSWEIGHT,F_HS_ProductLimitState,F_HS_BatteryMod,F_HS_IsPuHuo,F_HS_IsOil
											,F_HS_ExtraPercentage,F_HS_ExtraCommissionCountry,F_HS_BasePrice,F_HS_BasePriceCountry,F_HS_FreeMailMod,F_HS_IsOnSale,UpdateTag,UpdateTime,GuidString)

                                            select a.FNumber
                                            ,b.FName,d.FSaleUnitId,b.FSpecification,h.FNAME,F_HS_ProductSKU,o.F_HS_SKUGroupCode,F_HS_Specification1,F_HS_Specification2,j.FNumber F_HS_BrandNumber,p.FNAME F_HS_BrandName,n.FNumber as F_HS_PRODUCTSTATUS,
											k.FNumber F_HS_LISTID,l.FName F_HS_LISTNAME,c.FGROSSWEIGHT,a.F_HS_ProductLimitState,a.F_HS_BatteryMod,a.F_HS_IsPuHuo,
											a.F_HS_IsOil,a.F_HS_ExtraPercentage,a.F_HS_ExtraCommissionCountry,a.F_HS_BasePrice,a.F_HS_BasePriceCountry,a.F_HS_FreeMailMod,a.F_HS_IsOnSale
                                            ,'original',getDate(), @guidString
                                            from T_BD_MATERIAL a 
                                            inner join T_BD_MATERIAL_L b on b.FMATERIALID = a.FMATERIALID and b.FLOCALEID=2052
                                            inner join T_BD_MATERIALBASE c on c.FMATERIALID = a.FMATERIALID 
                                            inner join T_BD_MATERIALSALE d on d.FMATERIALID = a.FMATERIALID
                                            left join T_BD_unit_L h on d.FSaleUnitId =h.FUNITID and h.FLOCALEID=2052
                                            inner join T_BD_MATERIALSTOCK e on e.FMATERIALID = a.FMATERIALID
                                            inner join T_ORG_ORGANIZATIONS f on a.FUSEORGID=f.FORGID  
											inner join HS_T_Brand j on j.FID = a.F_HS_Brand
											inner join HS_T_BRAND_L p on p.FID = j.FID
											inner join HS_T_BD_LISTID k on k.FID = a.F_HS_ListIDNew
											inner join HS_T_BD_LISTID_L l on l.FID = k.FID
											inner join T_BAS_ASSISTANTDATAENTRY_L m ON a.F_HS_PRODUCTSTATUS=m.FENTRYID
                                            inner join T_BAS_ASSISTANTDATAENTRY n ON m.FentryID=n.FentryID
											inner join T_BD_MATERIALGROUP o on o.FID = a.FMaterialGroup
                                            where f.FNUMBER='100' and a.FNUMBER not like '99.%'
											and a.FNumber <> 'SPXJ'

                                            insert into HS_T_SynMaterialLog(FNumber,FName,FSaleUnitId,FSpecification,UnitName,F_HS_ProductSKU,F_HS_SKUGroupCode,F_HS_Specification1,F_HS_Specification2,F_HS_BrandNumber,F_HS_BrandName
                                            ,F_HS_PRODUCTSTATUS,F_HS_LISTID,F_HS_LISTNAME,F_HS_GROSSWEIGHT,F_HS_ProductLimitState,F_HS_BatteryMod,F_HS_IsPuHuo,F_HS_IsOil
											,F_HS_ExtraPercentage,F_HS_ExtraCommissionCountry,F_HS_BasePrice,F_HS_BasePriceCountry,F_HS_FreeMailMod,F_HS_IsOnSale,UpdateTag,UpdateTime,GuidString)

                                            select FNumber,FName,FSaleUnitId,FSpecification,UnitName,F_HS_ProductSKU,F_HS_SKUGroupCode,F_HS_Specification1,F_HS_Specification2,F_HS_BrandNumber,F_HS_BrandName 
                                            ,F_HS_PRODUCTSTATUS,F_HS_LISTID,F_HS_LISTNAME,F_HS_GROSSWEIGHT,F_HS_ProductLimitState,F_HS_BatteryMod,F_HS_IsPuHuo,F_HS_IsOil
											,F_HS_ExtraPercentage,F_HS_ExtraCommissionCountry,F_HS_BasePrice,F_HS_BasePriceCountry,F_HS_FreeMailMod,UpdateTag,F_HS_IsOnSale,UpdateTime,GuidString 
											from HS_T_SynMaterial

                                            select FNumber,FName,FSaleUnitId,FSpecification,UnitName,F_HS_ProductSKU,F_HS_SKUGroupCode,F_HS_Specification1,F_HS_Specification2,F_HS_BrandNumber,F_HS_BrandName  
                                            ,F_HS_PRODUCTSTATUS,F_HS_LISTID,F_HS_LISTNAME,F_HS_GROSSWEIGHT,F_HS_ProductLimitState,F_HS_BatteryMod,F_HS_IsPuHuo,F_HS_IsOil
											,F_HS_ExtraPercentage,F_HS_ExtraCommissionCountry,F_HS_BasePrice,F_HS_BasePriceCountry,F_HS_FreeMailMod,F_HS_IsOnSale
											from HS_T_SynMaterial

                                            delete from HS_T_SynMaterial where GuidString != @guidString and UpdateTag = 'original'

                                 ");
                }

                return string.Format(@"/*dialect*/ declare @guidString nvarchar(50)
                                                    select @guidString = NEWID()

                                                    select a.FNumber,b.FName,FSaleUnitId,b.FSpecification,h.FNAME as FUNITNAME,F_HS_ProductSKU,o.F_HS_SKUGroupCode,F_HS_Specification1,F_HS_Specification2,j.FNumber F_HS_BrandNumber,p.FNAME F_HS_BrandName
														   ,n.FNumber as F_HS_PRODUCTSTATUS,F_HS_LISTID,F_HS_LISTNAME,c.FGROSSWEIGHT,a.F_HS_ProductLimitState,a.F_HS_BatteryMod,a.F_HS_IsPuHuo,a.F_HS_IsOil
														   ,F_HS_ExtraPercentage,F_HS_ExtraCommissionCountry,F_HS_BasePrice,F_HS_BasePriceCountry,a.F_HS_FreeMailMod,a.F_HS_IsOnSale
                                                    into #HS_T_SynMaterial
                                                    from T_BD_MATERIAL a 
                                                    inner join T_BD_MATERIAL_L b on b.FMATERIALID = a.FMATERIALID and b.FLOCALEID=2052
                                                    inner join T_BD_MATERIALBASE c on c.FMATERIALID = a.FMATERIALID 
                                                    inner join T_BD_MATERIALSALE d on d.FMATERIALID = a.FMATERIALID
                                                    left join T_BD_unit_L h on d.FSaleUnitId =h.FUNITID and h.FLOCALEID=2052
                                                    inner join T_BD_MATERIALSTOCK e on e.FMATERIALID = a.FMATERIALID
                                                    inner join T_ORG_ORGANIZATIONS f on a.FUSEORGID=f.FORGID
													inner join HS_T_Brand j on j.FID = a.F_HS_Brand
													inner join HS_T_BRAND_L p on p.FID = j.FID
													inner join HS_T_BD_LISTID k on k.FID = a.F_HS_ListIDNew
													inner join HS_T_BD_LISTID_L l on l.FID = k.FID
                                                    inner join T_BAS_ASSISTANTDATAENTRY_L m ON a.F_HS_PRODUCTSTATUS=m.FENTRYID
                                                    inner join T_BAS_ASSISTANTDATAENTRY n ON m.FentryID=n.FentryID
													inner join T_BD_MATERIALGROUP o on o.FID = a.FMATERIALGROUP
                                                    where f.FNUMBER='100' and a.FNUMBER not like '99.%'
													and a.FNumber <> 'SPXJ'

                                                    update HS_T_SynMaterial set FName = b.FName,FSaleUnitId = b.FSaleUnitId,FSpecification = b.FSpecification,UnitName = b.FNAME,F_HS_ProductSKU = b.F_HS_ProductSKU,
                                                    F_HS_SKUGroupCode = b.F_HS_SKUGroupCode,F_HS_Specification1 = b.F_HS_Specification1,F_HS_Specification2 = b.F_HS_Specification2,F_HS_BrandNumber = b.F_HS_BrandNumber,F_HS_BrandName = b.F_HS_BrandName,F_HS_PRODUCTSTATUS = b.F_HS_PRODUCTSTATUS
                                                    ,F_HS_LISTID = b.F_HS_LISTID,F_HS_LISTNAME = b.F_HS_LISTNAME,F_HS_GROSSWEIGHT = b.FGROSSWEIGHT,F_HS_ProductLimitState = b.F_HS_ProductLimitState
													,F_HS_BatteryMod = b.F_HS_BatteryMod,F_HS_IsPuHuo = b.F_HS_IsPuHuo,F_HS_IsOil = b.F_HS_IsOil,F_HS_ExtraPercentage = b.F_HS_ExtraPercentage
													,F_HS_ExtraCommissionCountry = b.F_HS_ExtraCommissionCountry,F_HS_BasePrice = b.F_HS_BasePrice,F_HS_BasePriceCountry = b.F_HS_BasePriceCountry,F_HS_FreeMailMod = b.F_HS_FreeMailMod,F_HS_IsOnSale=b.F_HS_IsOnSale
                                                    ,UpdateTag = 'update',UpdateTime = GETDATE(),GuidString = @guidString
                                                    from HS_T_SynMaterial a 
                                                    inner join #HS_T_SynMaterial b on a.FNumber = b.FNUMBER
                                                    where a.FName <> b.FName or a.FSaleUnitId <> b.FSaleUnitId or a.FSpecification <> b.FSpecification or a.UnitName <> b.FUNITNAME or a.F_HS_ProductSKU <> b.F_HS_ProductSKU or a.F_HS_SKUGroupCode<>b.F_HS_SKUGroupCode
		                                                    or a.F_HS_Specification1 <> b.F_HS_Specification1 or a.F_HS_Specification2 <> b.F_HS_Specification2 or a.F_HS_BrandNumber <> b.F_HS_BrandNumber or a.F_HS_BrandName <> b.F_HS_BrandName or a.F_HS_PRODUCTSTATUS <> b.F_HS_PRODUCTSTATUS
		                                                    or a.F_HS_LISTID <> b.F_HS_LISTID or a.F_HS_LISTNAME <> b.F_HS_LISTNAME or a.F_HS_GROSSWEIGHT <> b.FGROSSWEIGHT or a.F_HS_ProductLimitState <> b.F_HS_ProductLimitState
															or a.F_HS_BatteryMod <> b.F_HS_BatteryMod or a.F_HS_IsPuHuo <> b.F_HS_IsPuHuo or a.F_HS_IsOil <> b.F_HS_IsOil or a.F_HS_ExtraPercentage <> b.F_HS_ExtraPercentage
															or a.F_HS_ExtraCommissionCountry <> b.F_HS_ExtraCommissionCountry or a.F_HS_BasePrice <> b.F_HS_BasePrice or a.F_HS_BasePriceCountry <> b.F_HS_BasePriceCountry
															or a.F_HS_FreeMailMod <> b.F_HS_FreeMailMod or a.F_HS_IsOnSale <> b.F_HS_IsOnSale

                                                    insert into HS_T_SynMaterial(FNumber,FName,FSaleUnitId,FSpecification,UnitName,F_HS_ProductSKU,F_HS_SKUGroupCode,F_HS_Specification1,F_HS_Specification2,
																F_HS_BrandNumber,F_HS_BrandName,F_HS_PRODUCTSTATUS,F_HS_LISTID,F_HS_LISTNAME,F_HS_GROSSWEIGHT,F_HS_ProductLimitState,F_HS_BatteryMod,F_HS_IsPuHuo,F_HS_IsOil
																,F_HS_ExtraPercentage,F_HS_ExtraCommissionCountry,F_HS_BasePrice,F_HS_BasePriceCountry,F_HS_FreeMailMod,F_HS_IsOnSale,UpdateTag,UpdateTime,GuidString)
                                                   
                                                    select b.* ,'insert',getDate(),@guidString from #HS_T_SynMaterial b 
                                                    where not exists(select * from HS_T_SynMaterial where FNUMBER = b.FNUMBER)

													select * from HS_T_SynMaterial where UpdateTag in ('update','insert') and GuidString = @guidString

                                                    insert into HS_T_SynMaterialLog(FNumber,FName,FSaleUnitId,FSpecification,UnitName,F_HS_ProductSKU,F_HS_SKUGroupCode,F_HS_Specification1,F_HS_Specification2,F_HS_BrandNumber,F_HS_BrandName,F_HS_PRODUCTSTATUS
																,F_HS_LISTID,F_HS_LISTNAME,F_HS_GROSSWEIGHT,F_HS_ProductLimitState,F_HS_BatteryMod,F_HS_IsPuHuo,F_HS_IsOil,F_HS_ExtraPercentage,F_HS_ExtraCommissionCountry
																,F_HS_BasePrice,F_HS_BasePriceCountry,F_HS_FreeMailMod,F_HS_IsOnSale,UpdateTag,UpdateTime,GuidString)
                                                    select * from HS_T_SynMaterial where UpdateTag in ('update','insert') and GuidString = @guidString

                                                    update HS_T_SynMaterial set UpdateTag = null where UpdateTag in ('update','insert') and GuidString = @guidString
                                                    drop table #HS_T_SynMaterial
                         ");
            }
            return null;
        }

        private bool IsFirstSynchro()
        {
            string sql = string.Format(@"/*dialect*/ select * From  HS_T_SynMaterial");
            DynamicObjectCollection items = SQLUtils.GetObjects(this.K3CloudContext, sql);

            if (items == null || items.Count == 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 获取物料数据
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public override IEnumerable<AbsSynchroDataInfo> GetK3Datas(IEnumerable<string> billNos = null,bool flag = true)
        {
            return GetMaterials();
        }   
    }
}
