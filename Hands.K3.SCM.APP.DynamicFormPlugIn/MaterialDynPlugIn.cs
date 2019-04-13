using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Hands.K3.SCM.APP.Entity.EnumType;
using Hands.K3.SCM.APP.Entity.StructType;
using Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject;
using Hands.K3.SCM.APP.Entity.SynDataObject.Material_;
using Hands.K3.SCM.APP.Utils.Utils;
using HS.K3.Common.Abbott;
using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.FormElement;
using Kingdee.BOS.Orm.DataEntity;


namespace Hands.K3.SCM.APP.DynamicFormPlugIn
{
    [Kingdee.BOS.Util.HotUpdate]
    [Description("物料同步到HC网站表单插件")]
    public class MaterialDynPlugIn : AbstractDynPlugIn
    {
        public List<AbsSynchroDataInfo> delDatas = null;
        private const int batch = 200;

        public override SynchroDataType DataType
        {
            get
            {
                return SynchroDataType.Material;
            }
        }
        public override SynchroDirection Direction
        {
            get
            {
                return SynchroDirection.ToHC;
            }
        }

        public string FormId
        {
            get
            {
                if (this.View != null)
                {
                    if (this.View.BillBusinessInfo.GetForm().Id.CompareTo(HSFormIdConst.BD_MATERIAL) == 0)
                    {
                        return HSFormIdConst.BD_MATERIAL;
                    }
                    if (this.View.BillBusinessInfo.GetForm().Id.CompareTo(HSFormIdConst.HS_List_ID) == 0)
                    {
                        return HSFormIdConst.HS_List_ID;
                    }
                }

                return null;
            }
        }

        public override IEnumerable<AbsSynchroDataInfo> GetK3Datas(Context ctx, FormOperation oper = null)
        {
            DataBaseConst.K3CloudContext = ctx;
            List<DynamicObject> objs = null;

            if (GetDynamicObjects(ctx, oper) != null && GetDynamicObjects(ctx, oper).Count() > 0)
            {
                objs = GetDynamicObjects(ctx, oper).ToList();
            }

            Material material = null;
            List<Material> materials = null;

            if (materials == null || materials.Count == 0)
            {
                if (objs != null && objs.Count > 0)
                {
                    var group = from o in objs
                                where !string.IsNullOrWhiteSpace(SQLUtils.GetFieldValue(o, "FNumber"))
                                group o by o["FNumber"]
                                into g
                                select g;

                    if (group != null && group.Count() > 0)
                    {
                        materials = new List<Material>();

                        foreach (var lst in group)
                        {
                            if (lst != null && lst.Count() > 0)
                            {
                                foreach (var mat in lst)
                                {
                                    if (mat != null)
                                    {
                                        material = new Material();

                                        material.F_HS_ListID = SQLUtils.GetFieldValue(mat, "F_HS_ListID");
                                        material.F_HS_ListName = SQLUtils.GetFieldValue(mat, "F_HS_ListName");
                                        string isConver = SQLUtils.GetFieldValue(mat, "F_HS_NotCoverMaterialName");
                                        if (!string.IsNullOrWhiteSpace(isConver))
                                        {
                                            material.F_HS_NotCoverMaterialName = isConver.Equals("0") ? false : true;
                                        }

                                        material.FNumber = SQLUtils.GetFieldValue(mat, "FNumber");
                                        material.SrcNo = material.FNumber;
                                        material.F_HS_IsPuHuo = SQLUtils.GetFieldValue(mat, "F_HS_IsPuHuo");
                                        material.F_HS_BatteryMod = SQLUtils.GetFieldValue(mat, "F_HS_BatteryMod");
                                        material.F_HS_IsOil = SQLUtils.GetFieldValue(mat, "F_HS_IsOil");
                                        material.F_HS_ProductLimitState = SQLUtils.GetFieldValue(mat, "F_HS_ProductLimitState");

                                        material.FForbidStatus = SQLUtils.GetFieldValue(mat, "FForbidStatus");
                                        material.FSaleUnitId = SQLUtils.GetFieldValue(mat, "FSaleUnitId");
                                        material.F_HS_PRODUCTSTATUS = SQLUtils.GetFieldValue(mat, "F_HS_PRODUCTSTATUS");
                                        material.F_HS_BrandNumber = SQLUtils.GetFieldValue(mat, "F_HS_BrandNumber");
                                        material.F_HS_BrandName = SQLUtils.GetFieldValue(mat, "F_HS_BrandName");
                                        material.F_HS_SKUGroupCode = SQLUtils.GetFieldValue(mat, "F_HS_SKUGroupCode");
                                        material.FGROSSWEIGHT = Convert.ToDecimal(SQLUtils.GetFieldValue(mat, "FGROSSWEIGHT"));
                                        material.F_HS_IsOnSale = SQLUtils.GetFieldValue(mat, "F_HS_IsOnSale").CompareTo("1") == 0 ? true : false;

                                        material.F_HS_ProductSKU = SQLUtils.GetFieldValue(mat, "F_HS_ProductSKU");
                                        material.F_HS_Specification1 = SQLUtils.GetFieldValue(mat, "F_HS_Specification1");
                                        material.F_HS_Specification2 = SQLUtils.GetFieldValue(mat, "F_HS_Specification2");

                                        material.F_HS_ExtraPercentage = Convert.ToDecimal(SQLUtils.GetFieldValue(mat, "F_HS_ExtraPercentage"));
                                        material.F_HS_ExtraCommissionCountry = SQLUtils.GetFieldValue(mat, "F_HS_ExtraCommissionCountry");
                                        material.F_HS_BasePrice = Convert.ToDecimal(SQLUtils.GetFieldValue(mat, "F_HS_BasePrice"));
                                        material.F_HS_BasePriceCountry = SQLUtils.GetFieldValue(mat, "F_HS_BasePriceCountry");
                                        material.F_HS_FreeMailMod = SQLUtils.GetFieldValue(objs.ElementAt(0), "F_HS_FreeMailMod").Equals("1") ? true : false;

                                        material.F_HS_MinSalesNum = Convert.ToInt32(SQLUtils.GetFieldValue(mat, "F_HS_MinSalesNum"));
                                        material.F_HS_MaxSalesNum = Convert.ToInt32(SQLUtils.GetFieldValue(mat, "F_HS_MaxSalesNum"));
                                        material.F_HS_TouristPrice = Convert.ToDecimal(SQLUtils.GetFieldValue(mat, "F_HS_TouristPrice"));
                                        material.F_HS_YNZeroMgSmokeOil = SQLUtils.GetFieldValue(mat, "F_HS_YNZeroMgSmokeOil").Equals("1") ? true : false;
                                        material.F_HS_ISCanTogetherPureBT = SQLUtils.GetFieldValue(mat, "F_HS_ISCanTogetherPureBT").Equals("1") ? true : false;
                                        material.FDocumentStatus = SQLUtils.GetFieldValue(mat, "FDocumentStatus");
                                        material.FForbidStatus = SQLUtils.GetFieldValue(mat, "FForbidStatus");
                                        material.F_HS_YNHomePageMarkup = SQLUtils.GetFieldValue(mat, "F_HS_YNHomePageMarkup").Equals("1")?true:false;

                                        materials.Add(material);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            
            if (materials != null && materials.Count > 0)
            {
                string prefix = SQLUtils.GetAUB2BDropShipOrderPrefix(ctx);

                if (this.Direction == SynchroDirection.ToB2B)
                {   // 1.2  物料同步数据过滤：物料为物料.液体属性=="非液体"  且  物料.商品状态<>"停产" 或 len(物料编码)==12 且 right(物料编码,2)==客户.dropship订单前缀
                    return materials.Where(m => !string.IsNullOrWhiteSpace(m.FNumber)
                                                && !string.IsNullOrWhiteSpace(m.F_HS_IsOil)
                                                && (m.F_HS_IsOil.Equals("3")
                                                && (!m.F_HS_PRODUCTSTATUS.Equals("SPTC"))
                                                || (m.FNumber.Length == 13
                                                && m.FNumber.Substring(m.FNumber.Length - 3, 3).Equals(prefix)))
                                                );
                }
                else
                {
                    return materials.Where(m => !string.IsNullOrWhiteSpace(m.FNumber)
                                                && !m.FNumber.Substring(m.FNumber.Length - 3, 3).Equals(prefix)
                                          );
                }
            }
            return null;
        }
        private string GetSql(Context ctx)
        {
            if (SelectedNos != null && SelectedNos.Count() > 0)
            {
                string sql = string.Format(@"/*dialect*/ select a.FNumber,a.FForbidStatus,u.FNUMBER as FCATEGORYID,k.F_HS_NotCoverMaterialName,F_HS_MinSalesNum,F_HS_MaxSalesNum
                                            ,b.FName,p.FNUMBER as FSaleUnitId,b.FSpecification,h.FNAME,F_HS_ProductSKU,q.F_HS_SKUGroupCode,F_HS_Specification1,F_HS_Specification2,j.FNumber F_HS_BrandNumber,r.FNAME F_HS_BrandName,
											n.FNumber F_HS_PRODUCTSTATUS,k.FNumber F_HS_ListID,l.FName F_HS_ListName,c.FGROSSWEIGHT,a.F_HS_ProductLimitState,a.F_HS_BatteryMod,a.F_HS_IsPuHuo,
											a.F_HS_IsOil,a.F_HS_ExtraPercentage,a.F_HS_ExtraCommissionCountry,a.F_HS_BasePrice,a.F_HS_BasePriceCountry,a.F_HS_FreeMailMod,a.F_HS_IsOnSale,a.F_HS_TouristPrice,a.F_HS_YNZeroMgSmokeOil
                                            ,a.F_HS_IntegralReturnRate,a.F_HS_ISCanTogetherPureBT,a.FDocumentStatus,a.FForbidStatus,a.F_HS_YNHomePageMarkup
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
                                            and u.FNUMBER = 'CHLB08_SYS'");

                if (!string.IsNullOrWhiteSpace(this.FormId))
                {
                    if (this.FormId.CompareTo(HSFormIdConst.BD_MATERIAL) == 0)
                    {
                        sql += string.Format(Environment.NewLine + "and a.FNUMBER in ('{0}')", string.Join("','", SelectedNos));
                        return sql;
                    }
                    if (this.FormId.CompareTo(HSFormIdConst.HS_List_ID) == 0)
                    {
                        sql += string.Format(Environment.NewLine + "and k.FNUMBER in ('{0}')", string.Join("','", SelectedNos));
                        return sql;
                    }
                }
            }
            return null;
        }

        private IEnumerable<DynamicObject> GetDynamicObjects(Context ctx, FormOperation oper = null)
        {
            string sql = GetSql(ctx);
            List<DynamicObject> objs = null;

            if (!string.IsNullOrEmpty(sql))
            {
                DynamicObjectCollection coll = SQLUtils.GetObjects(ctx, sql);

                if (coll != null && coll.Count > 0)
                {
                    objs = coll.Select(o => (DynamicObject)o).ToList();

                    if (oper != null)
                    {
                        if (oper.Operation.ToUpper().CompareTo(SynOperationType.DELETE.ToString()) == 0)
                        {
                            objs.ForEach(o => o["FForbidStatus"] = "C");
                        }
                    }
                }
            }
            return objs;
        }
        /// <summary>
        /// 当list_ID基础资料.不覆盖物料名称==true,则仅更新物料.ListName ,不更新物料.FName 否则同时更新物料.ListName ,物料.FName
        ///物料列表审核时，更新积分返点率（澳洲2C积分返点率）的值（服务类物料除外）：  ”液体：1 ；硬件：5，清仓品：0
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="datas"></param>
        /// <returns></returns>
        private int UpdateMaterialInfo(Context ctx, IEnumerable<AbsSynchroDataInfo> datas)
        {
            int count = 0;

            List<Material> materials = null;
            string sql = string.Empty;
            List<SqlObject> sqlObjects = null;
            List<SqlParam> sqlParams = null;
            SqlParam sqlParam = null;
            SqlParam sqlParam1 = null;
            SqlObject sqlObject = null;
           

            if (datas != null && datas.Count() > 0)
            {
                materials = datas.Select(d => (Material)d).ToList();
            }

            if (materials != null && materials.Count() > 0)
            {
                sqlObjects = new List<SqlObject>();

                if (this.FormId.CompareTo(HSFormIdConst.Material) == 0)
                {
                    foreach (var info in materials)
                    {
                        if (info != null)
                        {
                            if (materials != null && materials.Count > 0)
                            {
                                foreach (var material in materials)
                                {
                                    sql = string.Format(@"/*dialect*/update a set a.F_HS_IntegralReturnRate = @F_HS_IntegralReturnRate
                                                            from T_BD_MATERIAL a 
											                where a.FNUMBER not like '99.%'
											                and a.FNUMBER = @FNUMBER
											                ") + System.Environment.NewLine;

                                    sqlParams = new List<SqlParam>();
                                    sqlParam = new SqlParam("@F_HS_IntegralReturnRate", KDDbType.Decimal, material.F_HS_IntegralReturnRate);
                                    sqlParam1 = new SqlParam("@FNUMBER", KDDbType.String, material.FNumber);
                                    sqlParams.Add(sqlParam);
                                    sqlParams.Add(sqlParam1);
                                    sqlObject = new SqlObject(sql, sqlParams);
                                    sqlObjects.Add(sqlObject);
                                }

                            }
                        }
                    }
                }

                if (this.FormId.CompareTo(HSFormIdConst.HS_List_ID) == 0)
                {
                    foreach (var info in materials)
                    {
                        if (info != null)
                        {
                            sql = string.Format(@"/*dialect*/update a set a.F_HS_LISTNAME = @F_HS_LISTNAME
                                                    from T_BD_MATERIAL a 
											        inner join HS_T_BD_LISTID k on k.FID = a.F_HS_ListIDNew
											        where a.FNUMBER not like '99.%'
											        and k.FNUMBER = @FNUMBER
											        ") + System.Environment.NewLine;

                            sqlParams = new List<SqlParam>();
                            sqlParam = new SqlParam("@F_HS_LISTNAME",KDDbType.String, SQLUtils.DealQuotes(info.F_HS_ListName));
                            sqlParam1 = new SqlParam("@FNUMBER", KDDbType.String, info.F_HS_ListID);
                            sqlParams.Add(sqlParam);
                            sqlParams.Add(sqlParam1);
                            sqlObject = new SqlObject(sql, sqlParams);
                            sqlObjects.Add(sqlObject);

                            if (!info.F_HS_NotCoverMaterialName)
                            {
                                sql = string.Format(@"/*dialect*/update b set b.FName  = @FName
                                                                from T_BD_MATERIAL a 
                                                                inner join T_BD_MATERIAL_L b on b.FMATERIALID = a.FMATERIALID and b.FLOCALEID=2052
                                                                inner join HS_T_BD_LISTID k on k.FID = a.F_HS_ListIDNew
											                    where a.FNUMBER not like '99.%'
											                    and k.FNUMBER = @FNUMBER");

                                sqlParams = new List<SqlParam>();
                                sqlParam = new SqlParam("@FName", KDDbType.String, SQLUtils.DealQuotes(info.F_HS_ListName));
                                sqlParam1 = new SqlParam("@FNUMBER", KDDbType.String, info.F_HS_ListID);
                                sqlParams.Add(sqlParam);
                                sqlParams.Add(sqlParam1);
                                sqlObject = new SqlObject(sql, sqlParams);
                                sqlObjects.Add(sqlObject);
                            }
                        }
                    }
                }

                count = DBUtils.ExecuteBatch(ctx, sqlObjects);
            }

            return count;
        }

        public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
        {
            base.BeforeDoOperation(e);

            string oper = e.Operation.FormOperation.Operation;

            if (!string.IsNullOrWhiteSpace(oper))
            {
                if (e.Result.IsSuccess)
                {
                    if (oper.ToUpper().CompareTo(SynOperationType.DELETE.ToString()) == 0)
                    {
                        if (GetK3Datas(this.Context, e.Operation.FormOperation) != null)
                        {
                            delDatas = GetK3Datas(this.Context, e.Operation.FormOperation).ToList();
                        }
                    }
                }
            }
        }
        public override void AfterDoOperation(AfterDoOperationEventArgs e)
        {
            base.AfterDoOperation(e);
            string oper = e.Operation.Operation;

            if (!string.IsNullOrWhiteSpace(oper))
            {
                if (e.ExecuteResult)
                {
                    IEnumerable<AbsSynchroDataInfo> datas = null;

                    if (oper.ToUpper().CompareTo(SynOperationType.AUDIT.ToString()) == 0 || oper.ToUpper().CompareTo(SynOperationType.FORBID.ToString()) == 0
                        || oper.ToUpper().CompareTo(SynOperationType.ENABLE.ToString()) == 0 || oper.ToUpper().CompareTo(SynOperationType.DELETE.ToString()) == 0)
                    {
                        datas = GetK3Datas(this.Context);
                    }

                    if (datas != null && datas.Count() > 0)
                    {
                        if (oper.ToUpper().CompareTo(SynOperationType.AUDIT.ToString()) == 0)
                        {
                            datas = datas.Select(d => (Material)d).ToList().Where(m => m.FDocumentStatus.CompareTo("C") == 0).ToList();
                            BatchSynK3Datas2HC(this.Context, datas, batch);
                            UpdateMaterialInfo(this.Context, datas);
                        }
                        else if (oper.ToUpper().CompareTo(SynOperationType.FORBID.ToString()) == 0)
                        {
                            datas = datas.Select(d => (Material)d).ToList().Where(m => m.FForbidStatus.CompareTo("A") == 0).ToList();
                            BatchSynK3Datas2HC(this.Context, datas, batch);
                            UpdateMaterialInfo(this.Context, datas);
                        }
                        else if (oper.ToUpper().CompareTo(SynOperationType.ENABLE.ToString()) == 0)
                        {
                            datas = datas.Select(d => (Material)d).ToList().Where(m => m.FForbidStatus.CompareTo("B") == 0).ToList();
                            BatchSynK3Datas2HC(this.Context, datas, batch);
                            UpdateMaterialInfo(this.Context, datas);
                        }
                        if (oper.ToUpper().CompareTo(SynOperationType.DELETE.ToString()) == 0)
                        {
                            datas = datas.Select(d => (Material)d).ToList().Where(m => m.FForbidStatus.CompareTo("C") == 0).ToList();
                            BatchSynK3Datas2HC(this.Context, delDatas, batch);
                        }


                    }
                }

            }
        }
        public override void BarItemClick(BarItemClickEventArgs e)
        {
            base.BarItemClick(e);
        }
        public override void ButtonClick(ButtonClickEventArgs e)
        {
            base.ButtonClick(e);
        }
        public override void ContextMenuItemClick(ContextMenuItemClickEventArgs e)
        {
            base.ContextMenuItemClick(e);
        }
    }
}
