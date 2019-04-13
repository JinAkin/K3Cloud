using System;
using System.Collections.Generic;
using System.Linq;

using Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using System.ComponentModel;
using Kingdee.BOS.App.Data;
using Hands.K3.SCM.APP.Utils.Utils;
using HS.K3.Common.Abbott;
using Hands.K3.SCM.APP.Entity.K3WebApi;
using Hands.K3.SCM.APP.Entity.SynDataObject.Material_;

namespace Hands.K3.SCM.App.ServicePlugIn
{
    [Description("物料，LISTID同步--物料，LISTID同步插件")]
    public class MaterialSerPlugIn : AbstractOSPlugIn
    {
        private static List<AbsSynchroDataInfo> synDatas = new List<AbsSynchroDataInfo>();
      
        public override SynchroDataType DataType
        {
            get
            {
                return SynchroDataType.Material;
            }
        }

        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            e.FieldKeys.Add("F_HS_ListID");
            e.FieldKeys.Add("F_HS_ListIDNew");
            e.FieldKeys.Add("F_HS_ListName");
            e.FieldKeys.Add("F_HS_ProductLimitState");
            e.FieldKeys.Add("F_HS_IsPuHuo");
            e.FieldKeys.Add("F_HS_IsOil");
            e.FieldKeys.Add("F_HS_BatteryMod");
            e.FieldKeys.Add("F_HS_ExtraPercentage");
            e.FieldKeys.Add("F_HS_ExtraCommissionCountry");
            e.FieldKeys.Add("F_HS_BasePrice");
            e.FieldKeys.Add("F_HS_BasePriceCountry");
            e.FieldKeys.Add("F_HS_FreeMailMod");

            e.FieldKeys.Add("F_HS_ProductSKU");
            e.FieldKeys.Add("F_HS_Specification1");
            e.FieldKeys.Add("F_HS_Specification2");
            e.FieldKeys.Add("F_HS_Brand");
            e.FieldKeys.Add("F_HS_PRODUCTSTATUS");
            e.FieldKeys.Add("FMaterialGroup");
            e.FieldKeys.Add("FGROSSWEIGHT");
            e.FieldKeys.Add("F_HS_IsOnSale");
            e.FieldKeys.Add("FForbidStatus");
            e.FieldKeys.Add("SubHeadEntity2");
            e.FieldKeys.Add("MaterialSale");
            e.FieldKeys.Add("FSaleUnitId");
            e.FieldKeys.Add("FCategoryID");
        }

        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);
            HttpResponseResult result = null;
           
            bool isSuccess = this.OperationResult.IsSuccess;
            string name = this.FormOperation.Operation;

            List<DynamicObject> objects = null;

            if (this.BusinessInfo.GetForm().Id.CompareTo("HS_List_ID") == 0)
            {
                objects = GetObjects(this.Context, e.DataEntitys.ToList());
                if (name.CompareTo("Delete") == 0)
                {
                    if (isSuccess)
                    {
                        objects.ForEach(o => o["FForbidStatus"] = "C");
                    }   
                } 
            }
            else
            {
                objects = e.DataEntitys.ToList();
                if (name.CompareTo("Delete") == 0)
                {
                    if (isSuccess)
                    {
                        objects.ForEach(o => o["ForbidStatus"] = "C");
                    }
                }    
            }

            if (objects == null || objects.Count <= 0)
            {
                return;
            }

            this.DyamicObjects = objects;

           
            IEnumerable<AbsSynchroDataInfo> datas = GetK3Datas(this.Context, objects,ref result);

            if (datas != null && datas.Count() > 0)
            {
                synDatas.AddRange(datas);
            }
              
           
            if (objects != null && objects.Count < 20)
            {
                SynchroK3DataToWebSite(this.Context, synDatas);

                if (this.BusinessInfo.GetForm().Id.CompareTo("HS_List_ID") == 0)
                {
                    if (synDatas != null && synDatas.Count() > 0)
                    {
                        List<ListInfo> listInfos = synDatas.Select(l => (ListInfo)l).ToList();
                        UpdateMaterialInfo(this.Context, listInfos);
                    }
                }
                synDatas.Clear();
            }
            
        }

        public override void AfterExecuteOperationTransaction(AfterExecuteOperationTransaction e)
        {
            //base.AfterExecuteOperationTransaction(e);

            //var task = Task.Factory.StartNew(() =>
            //{
            //    SynchroK3DataToWebSite(this.Context,synDatas);
            //    if (this.BusinessInfo.GetForm().Id.CompareTo("HS_List_ID") == 0)
            //    {
            //        if (synDatas != null && synDatas.Count() > 0)
            //        {
            //            List<ListInfo> listInfos = synDatas.Select(i => (ListInfo)i).ToList();
            //            UpdateMaterialInfo(this.Context, listInfos);
            //        }
            //    }
            //}
                                            //);
        }

        /// <summary>
        /// 获取勾选审核的数据
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="objects"></param>
        /// <returns></returns>
        public override IEnumerable<AbsSynchroDataInfo> GetK3Datas(Context ctx, List<DynamicObject> objects,ref HttpResponseResult result)
        {
            ListInfo info = null;
            List<ListInfo> infos = null;

            Material material = null;
            List<Material> materials = null;

            result = new HttpResponseResult();
            result.Success = true;

            string listId = string.Empty;
            string listName = string.Empty;
            string productStatus = string.Empty;

            string number = string.Empty;
            string name = string.Empty;
            string specification = string.Empty;

            if (this.BusinessInfo.GetForm().Id.CompareTo("HS_List_ID") == 0)
            {
                listId = "f_hs_listid";
                listName = "f_hs_listname";
                productStatus = "f_hs_productstatus";
                number = "FNumber";
                name = "FName";
                specification = "FSpecification";
            }
            else
            {
                listId = "F_HS_ListIDNew";
                listName = "F_HS_ListName";
                productStatus = "F_HS_PRODUCTSTATUS";

                number = "Number";
                name = "Name";
                specification = "Specification";
            }

            if (objects != null && objects.Count > 0)
            {
                infos = new List<ListInfo>();

                var group = from o in objects
                            where !string.IsNullOrWhiteSpace(SQLUtils.GetFieldValue(o,listId)) 
                            group o by o[listId]
                            into g
                            select g;

                if (group != null)
                {
                    if (group.Count() > 0)
                    {
                        foreach (var objs in group)
                        {
                            if (objs != null)
                            {
                                info = new ListInfo();
                                materials = new List<Material>();

                                if (!string.IsNullOrWhiteSpace(listId))
                                {
                                    if (listId.CompareTo("F_HS_ListIDNew") == 0)
                                    {
                                        DynamicObject nlst = objs.ElementAt(0)["F_HS_ListIDNew"] as DynamicObject;
                                        info.F_HS_ListID = SQLUtils.GetFieldValue(nlst, "Number");
                                        info.F_HS_ListName = SQLUtils.GetFieldValue(nlst,"Name");
                                        info.SrcNo = info.F_HS_ListID;
                                    }
                                    else
                                    {
                                        info.F_HS_ListID = SQLUtils.GetFieldValue(objs.ElementAt(0), listId);
                                        info.SrcNo = info.F_HS_ListID;
                                        info.F_HS_ListName = SQLUtils.GetFieldValue(objs.ElementAt(0), listName);
                                    }
                                }

                                foreach (var obj in objs)
                                {
                                    if (obj != null)
                                    {
                                        string cateNo = string.Empty;
                                        if (this.BusinessInfo.GetForm().Id.CompareTo("BD_MATERIAL") == 0)
                                        {
                                            var mbase = obj["MaterialBase"] as DynamicObjectCollection;
                                            if (mbase != null && mbase.Count > 0)
                                            {
                                                foreach (var item in mbase)
                                                {
                                                    if (item != null)
                                                    {
                                                        var cate = item["CategoryID"] as DynamicObject;
                                                        cateNo = SQLUtils.GetFieldValue(cate, "Number");
                                                    }
                                                }
                                            }
                                           
                                        }
                                        if ((this.BusinessInfo.GetForm().Id.CompareTo("HS_List_ID") == 0 && SQLUtils.GetFieldValue(obj, "FCATEGORYID").CompareTo("CHLB08_SYS") == 0)
                                        || (this.BusinessInfo.GetForm().Id.CompareTo("BD_MATERIAL") == 0 && cateNo.CompareTo("CHLB08_SYS") == 0))
                                        {
                                            material = new Material();

                                            material.FNumber = SQLUtils.GetFieldValue(obj, number);
                                            material.F_HS_IsPuHuo = SQLUtils.GetFieldValue(objs.ElementAt(0), "F_HS_IsPuHuo");
                                            material.F_HS_BatteryMod = SQLUtils.GetFieldValue(objs.ElementAt(0), "F_HS_BatteryMod");
                                            material.F_HS_IsOil = SQLUtils.GetFieldValue(objs.ElementAt(0), "F_HS_IsOil");
                                            material.F_HS_ProductLimitState = SQLUtils.GetFieldValue(obj, "F_HS_ProductLimitState");


                                            if (this.BusinessInfo.GetForm().Id.CompareTo("HS_List_ID") == 0)
                                            {
                                                material.FForbidStatus = SQLUtils.GetFieldValue(obj, "FForbidStatus");
                                                material.FSaleUnitId = SQLUtils.GetFieldValue(obj, "FSaleUnitId");
                                                material.F_HS_PRODUCTSTATUS = SQLUtils.GetFieldValue(obj, "F_HS_PRODUCTSTATUS");
                                                material.F_HS_BrandNumber = SQLUtils.GetFieldValue(obj, "F_HS_BrandNumber");
                                                material.F_HS_BrandName = SQLUtils.GetFieldValue(obj, "F_HS_BrandName");
                                                material.F_HS_SKUGroupCode = SQLUtils.GetFieldValue(obj, "F_HS_SKUGroupCode");
                                                material.FGROSSWEIGHT = Convert.ToDecimal(SQLUtils.GetFieldValue(obj, "FGROSSWEIGHT"));
                                                material.F_HS_IsOnSale = SQLUtils.GetFieldValue(obj, "F_HS_IsOnSale").CompareTo("1") == 0 ? true : false;

                                            }
                                            else
                                            {

                                                material.FForbidStatus = SQLUtils.GetFieldValue(obj, "ForbidStatus");
                                                var mSale = obj["MaterialSale"] as DynamicObjectCollection;
                                                material.FSaleUnitId = GetSaleUnitId(mSale);

                                                var status = obj["F_HS_PRODUCTSTATUS"] as DynamicObject;
                                                material.F_HS_PRODUCTSTATUS = SQLUtils.GetFieldValue(status, "FNumber");

                                                var brand = obj["F_HS_Brand"] as DynamicObject;
                                                material.F_HS_BrandNumber = SQLUtils.GetFieldValue(brand, "Number");
                                                material.F_HS_BrandName = SQLUtils.GetFieldValue(brand, "Name");

                                                DynamicObject mgr = obj["MATERIALGROUP"] as DynamicObject;
                                                material.F_HS_SKUGroupCode = SQLUtils.GetSKUCode(ctx, mgr, "Number");

                                                DynamicObjectCollection mbase = obj["MaterialBase"] as DynamicObjectCollection;
                                                material.FGROSSWEIGHT = GetGrossWeight(mbase);
                                                material.F_HS_IsOnSale = Convert.ToBoolean(SQLUtils.GetFieldValue(obj, "F_HS_IsOnSale"));

                                            }

                                            material.F_HS_ProductSKU = SQLUtils.GetFieldValue(obj, "F_HS_ProductSKU");
                                            material.F_HS_Specification1 = SQLUtils.GetFieldValue(obj, "F_HS_Specification1");
                                            material.F_HS_Specification2 = SQLUtils.GetFieldValue(obj, "F_HS_Specification2");

                                            material.F_HS_ExtraPercentage = Convert.ToDecimal(SQLUtils.GetFieldValue(obj, "F_HS_ExtraPercentage"));
                                            material.F_HS_ExtraCommissionCountry = SQLUtils.GetFieldValue(obj, "F_HS_ExtraCommissionCountry");
                                            material.F_HS_BasePrice = Convert.ToDecimal(SQLUtils.GetFieldValue(obj, "F_HS_BasePrice"));
                                            material.F_HS_BasePriceCountry = SQLUtils.GetFieldValue(obj, "F_HS_BasePriceCountry");
                                            material.F_HS_FreeMailMod = SQLUtils.GetFieldValue(obj, "F_HS_FreeMailMod").Equals("1") ? true : false;
                                            material.F_HS_IntegralReturnRate = Convert.ToDecimal(SQLUtils.GetFieldValue(obj, "F_HS_IntegralReturnRate"));

                                            //if (!string.IsNullOrWhiteSpace(material.F_HS_PRODUCTSTATUS))
                                            //{
                                            //    if (material.F_HS_PRODUCTSTATUS.CompareTo("SPXJ") != 0)
                                            //    {
                                            //        materials.Add(material);
                                            //    }
                                            //}
                                            materials.Add(material);
                                         
                                        }
                                        
                                    }                                  
                                }
                                info.Materials = materials;
                                if (info.Materials != null && info.Materials.Count > 0)
                                {
                                    infos.Add(info);
                                }
                            }
                            
                        }
                        
                    }
                }
            }
            if (infos != null)
            {
                if (infos.Count > 0)
                {
                    return infos;
                }
            }
            return null;
        }

        /// <summary>
        /// 获取单位编码
        /// </summary>
        /// <param name="coll"></param>
        /// <returns></returns>
        private string GetSaleUnitId(DynamicObjectCollection coll)
        {
            if (coll != null && coll.Count() > 0)
            {
                var obj = coll.ElementAt(0) as DynamicObject;
                var obj_ = obj["SaleUnitId"] as DynamicObject;
                return SQLUtils.GetFieldValue(obj_, "Number");
            }

            return null;
        }

        /// <summary>
        /// 获取物料毛重
        /// </summary>
        /// <param name="coll"></param>
        /// <returns></returns>
        private decimal GetGrossWeight(DynamicObjectCollection coll)
        {
            decimal weight = 0;
            if (coll != null && coll.Count > 0)
            {
                var obj = coll.ElementAt(0) as DynamicObject;
                weight = Convert.ToDecimal(SQLUtils.GetFieldValue(obj, "GROSSWEIGHT"));
            }
            return weight;
        }
        

        /// <summary>
        /// 关联物料相关信息
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="objects"></param>
        /// <returns></returns>
        private List<DynamicObject> GetObjects(Context ctx, List<DynamicObject> objects)
        {
            if (objects != null && objects.Count > 0)
            {
                List<string> listIds = objects.Select(o => SQLUtils.GetFieldValue(o, "Number")).ToList();

                string sql = string.Format(@"select a.FNumber,a.FForbidStatus,u.FNUMBER as FCATEGORYID
                                            ,b.FName,p.FNUMBER as FSaleUnitId,b.FSpecification,h.FNAME,F_HS_ProductSKU,q.F_HS_SKUGroupCode,F_HS_Specification1,F_HS_Specification2,j.FNumber F_HS_BrandNumber,r.FNAME F_HS_BrandName,
											n.FNumber F_HS_PRODUCTSTATUS,k.FNumber F_HS_ListID,l.FName F_HS_ListName,c.FGROSSWEIGHT,a.F_HS_ProductLimitState,a.F_HS_BatteryMod,a.F_HS_IsPuHuo,
											a.F_HS_IsOil,a.F_HS_ExtraPercentage,a.F_HS_ExtraCommissionCountry,a.F_HS_BasePrice,a.F_HS_BasePriceCountry,a.F_HS_FreeMailMod,a.F_HS_IsOnSale,a.F_HS_IntegralReturnRate      
                                            from T_BD_MATERIAL a 
                                            inner join T_BD_MATERIAL_L b on b.FMATERIALID = a.FMATERIALID and b.FLOCALEID=2052
                                            inner join T_BD_MATERIALBASE c on c.FMATERIALID = a.FMATERIALID 
                                            inner join T_BD_MATERIALSALE d on d.FMATERIALID = a.FMATERIALID
                                            inner join T_BD_MATERIALSTOCK e on e.FMATERIALID = a.FMATERIALID
                                            inner join T_ORG_ORGANIZATIONS f on a.FUSEORGID=f.FORGID
                                            left join T_BD_unit_L h on d.FSaleUnitId =h.FUNITID and h.FLOCALEID=2052
											inner join HS_T_Brand j on j.FID = a.F_HS_Brand
											inner join HS_T_BRAND_L r on r.FID = j.FID
											inner join HS_T_BD_LISTID k on k.FID = a.F_HS_ListIDNew
											inner join HS_T_BD_LISTID_L l on l.FID = k.FID
											inner join T_BAS_ASSISTANTDATAENTRY_L m ON a.F_HS_PRODUCTSTATUS=m.FENTRYID
                                            inner join T_BAS_ASSISTANTDATAENTRY n ON m.FentryID=n.FentryID
											inner join T_BD_MATERIALSALE o on a.FMATERIALID=o.FMATERIALID 
                                            inner join T_BD_UNIT p on o.FSALEUNITID=p.FUNITID
                                            inner join T_BD_MATERIALGROUP q on q.FID = a.FMaterialGroup
                                            inner join  T_BD_MATERIALCATEGORY u on u.FCATEGORYID = c.FCATEGORYID
                                            where f.FNUMBER='100' 
											and a.FNUMBER not like '99.%'
                                            and a.FNumber <> 'SPXJ'
											and k.FNUMBER in('{0}')
                                            and u.FNUMBER = '{1}'", string.Join("','", listIds), "CHLB08_SYS");


                DynamicObjectCollection coll = SQLUtils.GetObjects(ctx, sql);

                if (coll != null && coll.Count > 0)
                {
                    return coll.ToList();
                }

            }

            return null;

        }

        /// <summary>
        /// List_ID基础资料批量审核成功后,更新物料的相关信息
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="infos"></param>
        /// <returns></returns>
        private int UpdateMaterialInfo(Context ctx, IEnumerable<AbsSynchroDataInfo> datas)
        {
            int count = 0;
            List<ListInfo> infos = null;

            if (datas != null && datas.Count() > 0)
            {
                infos = datas.Select(d =>(ListInfo)d).ToList();
            }
            
            if (infos != null && infos.Count() > 0)
            {
                foreach(var info in infos)
                {
                    if (info != null)
                    {
                        string sql = string.Format(@"/*dialect*/update a set a.F_HS_LISTNAME = '{0}'
                                                                from T_BD_MATERIAL a 
                                                                inner join T_BD_MATERIAL_L b on b.FMATERIALID = a.FMATERIALID and b.FLOCALEID=2052
                                                                inner join T_BD_MATERIALBASE c on c.FMATERIALID = a.FMATERIALID 
                                                                inner join T_BD_MATERIALSALE d on d.FMATERIALID = a.FMATERIALID
                                                                inner join T_BD_MATERIALSTOCK e on e.FMATERIALID = a.FMATERIALID
                                                                inner join T_ORG_ORGANIZATIONS f on a.FUSEORGID=f.FORGID
                                                                left join T_BD_unit_L h on d.FSaleUnitId =h.FUNITID and h.FLOCALEID=2052
											                    inner join HS_T_Brand j on j.FID = a.F_HS_Brand
											                    inner join HS_T_BRAND_L r on r.FID = j.FID
											                    inner join HS_T_BD_LISTID k on k.FID = a.F_HS_ListIDNew
											                    inner join HS_T_BD_LISTID_L l on l.FID = k.FID
											                    inner join T_BAS_ASSISTANTDATAENTRY_L m ON a.F_HS_PRODUCTSTATUS=m.FENTRYID
                                                                inner join T_BAS_ASSISTANTDATAENTRY n ON m.FentryID=n.FentryID
                                                                inner join T_BD_MATERIALGROUP q on q.FID = a.FMaterialGroup  
											                    where a.FNUMBER not like '99.%'
											                    and k.FNUMBER = '{1}'

											            ", SQLUtils.DealQuotes(info.F_HS_ListName),info.F_HS_ListID) + System.Environment.NewLine;
                        sql += string.Format(@"/*dialect*/    update b set b.FName  ='{0}'
                                                                from T_BD_MATERIAL a 
                                                                inner join T_BD_MATERIAL_L b on b.FMATERIALID = a.FMATERIALID and b.FLOCALEID=2052
                                                                inner join T_BD_MATERIALBASE c on c.FMATERIALID = a.FMATERIALID 
                                                                inner join T_BD_MATERIALSALE d on d.FMATERIALID = a.FMATERIALID
                                                                inner join T_BD_MATERIALSTOCK e on e.FMATERIALID = a.FMATERIALID
                                                                inner join T_ORG_ORGANIZATIONS f on a.FUSEORGID=f.FORGID
                                                                left join T_BD_unit_L h on d.FSaleUnitId =h.FUNITID and h.FLOCALEID=2052
											                    inner join HS_T_Brand j on j.FID = a.F_HS_Brand
											                    inner join HS_T_BRAND_L r on r.FID = j.FID
											                    inner join HS_T_BD_LISTID k on k.FID = a.F_HS_ListIDNew
											                    inner join HS_T_BD_LISTID_L l on l.FID = k.FID
											                    inner join T_BAS_ASSISTANTDATAENTRY_L m ON a.F_HS_PRODUCTSTATUS=m.FENTRYID
                                                                inner join T_BAS_ASSISTANTDATAENTRY n ON m.FentryID=n.FentryID
                                                                inner join T_BD_MATERIALGROUP q on q.FID = a.FMaterialGroup  
											                    where a.FNUMBER not like '99.%'
											                    and k.FNUMBER = '{1}'", SQLUtils.DealQuotes(info.F_HS_ListName), info.F_HS_ListID);

                        count += DBUtils.Execute(ctx, sql);
                    }
                }
            }

            return count;
        }
    }
}
