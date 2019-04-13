using System;
using System.ComponentModel;
using Hands.K3.SCM.APP.Utils.Utils;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Orm.DataEntity;

namespace Hands.K3.SCM.APP.DynamicFormPlugIn
{
    [Description("产品下单出库汇总表--动态表单插件")]
    [Kingdee.BOS.Util.HotUpdate]
    public class OrdersShipmentSummary : AbstractDynamicFormPlugIn
    {
        private string sql = string.Empty;
        private string initSql = string.Empty;

        public void LoadData(Context ctx)
        {
            string beginTime = Convert.ToString(this.View.Model.GetValue("F_HS_BeginDate"));
            string endTime = Convert.ToString(this.View.Model.GetValue("F_HS_EndDate"));

            sql = string.Format(@"/*dialect*/ select a.F_HS_ListID,a.F_HS_FirstInStockDate,q.F_HS_SKUGroupCode,r.FNAME F_HS_BrandName,a.FNUMBER as F_HS_MaterialId,b.FNAME as F_HS_MaterialName,b.FSpecification as F_HS_Specification,
                                    d.FNUMBER as F_HS_SalUnit,sum(k.F_HS_OrderQty) as F_HS_OrderQty ,sum(k.F_HS_OrderAmount) as F_HS_OrderAmount,sum(l.F_HS_StockQty) as F_HS_StockQty,sum(l.F_HS_StockAmount) as F_HS_StockAmount
                                    from T_BD_MATERIAL a
                                    inner join T_BD_MATERIAL_L b on a.FMATERIALID = b.FMATERIALID and b.FLOCALEID=2052
                                    inner join T_BD_MATERIALSALE c on c.FMATERIALID = a.FMATERIALID
                                    inner join T_BD_UNIT d on d.FUNITID = c.FSALEUNITID
                                    left join HS_T_BD_LISTID o on o.FID = a.F_HS_ListIDNew
                                    left join HS_T_BD_LISTID_L p on o.FID = p.FID and p.FLOCALEID=2052
                                    left join HS_T_Brand j on j.FID = a.F_HS_Brand 
                                    left join HS_T_BRAND_L r on r.FID = j.FID and r.FLOCALEID=2052
                                    inner join T_BD_MATERIALGROUP q on q.FID = a.FMaterialGroup
                                    inner join T_ORG_ORGANIZATIONS z on z.FORGID = a.fuseOrgID
                                    left join 
                                    (select a.FNUMBER as F_HS_MaterialId,sum(e.FQTY) as F_HS_OrderQty,sum(case when f.F_HS_RATETOUSA = 0 then 0 else g.FAMOUNT/f.F_HS_RATETOUSA end) as F_HS_OrderAmount
                                    from T_BD_MATERIAL a
                                    inner join T_SAL_ORDERENTRY e on e.FMATERIALID = a.FMATERIALID
                                    inner join T_SAL_ORDER f on e.FID = f.FID
                                    inner join T_SAL_ORDERENTRY_F g on g.FENTRYID = e.FENTRYID and g.FID = e.FID
                                    inner join T_SAL_ORDERFIN h on h.FID = f.FID
                                    inner join T_ORG_ORGANIZATIONS c on f.FSALEORGID = c.FORGID 
                                    where  not (f.FCLoseSTATUS = 'B' and e.FMRPCLOSESTATUS='A') 
                                    and a.FNUMBER not like '99.%' and f.FDOCUMENTSTATUS='C'
                                    and c.FORGID = {0}
                                    group by a.FNUMBER
                                    ) k on k.F_HS_MaterialId = a.FNUMBER
                                    left join 
                                    ( select a.FNUMBER as F_HS_MaterialId,sum(l.FPriceUnitQty) F_HS_StockQty,sum(case when k.F_HS_RATETOUSA =0 then 0 else l.FAmount/k.F_HS_RATETOUSA end) F_HS_StockAmount 
                                    from T_BD_MATERIAL a
                                    inner join T_SAL_OUTSTOCKENTRY j on j.FMATERIALID = a.FMATERIALID
                                    inner join T_SAL_OUTSTOCK k on k.FID = j.FID
                                    inner join T_SAL_OUTSTOCKENTRY_F l on k.FID = l.FID and j.FENTRYID = l.FENTRYID
                                    inner join T_SAL_OUTSTOCKFIN m on m.FID = k.FID
                                    inner join T_ORG_ORGANIZATIONS w on k.FSALEORGID = w.FORGID
                                    where a.FNUMBER not like '99.%' and k.FDOCUMENTSTATUS='C'
                                    and w.FORGID = {1}
                                    group by a.FNUMBER
                                    )  l on l.F_HS_MaterialId = a.FNUMBER 

                                    where a.FNUMBER not like '99.%' 
                                    and (k.F_HS_OrderQty is not null or k.F_HS_OrderAmount is not null or l.F_HS_StockQty is not null or l.F_HS_StockAmount is not null)
                                    and z.FORGID = {2}                    
                                   ", ctx.CurrentOrganizationInfo.ID, ctx.CurrentOrganizationInfo.ID, ctx.CurrentOrganizationInfo.ID);

            int index1 = 0;
            int index2 = 0;
            index1 = sql.IndexOf(string.Format("and c.FORGID = {0}",ctx.CurrentOrganizationInfo.ID));
            if (!string.IsNullOrWhiteSpace(beginTime))
            {
                if (!string.IsNullOrWhiteSpace(endTime))
                {
                    sql = sql.Insert(index1, Environment.NewLine + string.Format(@"and f.FDATE between '{0}' and '{1}'", beginTime, endTime = endTime.Replace("00:00:00", "23:59:59")));
                    index2 = sql.IndexOf(string.Format("and w.FORGID = {0}", ctx.CurrentOrganizationInfo.ID));
                    sql = sql.Insert(index2, Environment.NewLine + string.Format(@"and k.FDATE between '{0}' and '{1}'", beginTime, endTime = endTime.Replace("00:00:00", "23:59:59")));
                    
                }
                else
                {
                    sql = sql.Insert(index1, Environment.NewLine + string.Format(@"and f.FDATE between '{0}' and '{1}'", beginTime, beginTime = beginTime.Replace("00:00:00", "23:59:59")));
                    index2 = sql.IndexOf(string.Format("and w.FORGID = {0}", ctx.CurrentOrganizationInfo.ID));
                    sql = sql.Insert(index2, Environment.NewLine + string.Format(@"and k.FDATE between '{0}' and '{1}'", beginTime, beginTime = beginTime.Replace("00:00:00", "23:59:59")));
                    
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(endTime))
                {
                    sql = sql.Insert(index1, Environment.NewLine + string.Format(@"and f.FDATE between '{0}' and '{1}'", endTime, endTime = endTime.Replace("00:00:00", "23:59:59")));
                    index2 = sql.IndexOf(string.Format("and w.FORGID = {0}", ctx.CurrentOrganizationInfo.ID));
                    sql = sql.Insert(index2, Environment.NewLine + string.Format(@"and k.FDATE between '{0}' and '{1}'", endTime, endTime = endTime.Replace("00:00:00", "23:59:59")));
                }
            }


            sql += Environment.NewLine + string.Format(@"group by a.F_HS_ListID,a.F_HS_FirstInStockDate,q.F_HS_SKUGroupCode,r.FNAME,a.FNUMBER,b.FNAME,b.FSpecification,d.FNUMBER");

            try
            {
                BindDataToView(ctx, this.View, sql.ToString(), "F_HS_Entity");
            }
            catch (Exception ex)
            {
                this.View.ShowErrMessage(ex.ToString(), "操作出错了！" + Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace, MessageBoxType.Error);
            }

        }

        public void BindDataToView(Context ctx, IDynamicFormView formView, string sql, string entryName)
        {
            DynamicObjectCollection coll = GetObjects(ctx, sql);
            int i = 0;

            if (!string.IsNullOrWhiteSpace(entryName))
            {
                formView.Model.DeleteEntryData(entryName);

                if (coll != null && coll.Count > 0)
                {
                    foreach (var item in coll)
                    {
                        if (item != null)
                        {
                            formView.Model.InsertEntryRow(entryName, i);

                            formView.Model.SetValue("F_HS_ListID", SQLUtils.GetFieldValue(item, "F_HS_ListID"), i);
                            formView.Model.SetValue("F_HS_BrandName", SQLUtils.GetFieldValue(item, "F_HS_BrandName"), i);
                            formView.Model.SetValue("F_HS_MaterialId", SQLUtils.GetFieldValue(item, "F_HS_MaterialId"), i);
                            formView.Model.SetValue("F_HS_MaterialName", SQLUtils.GetFieldValue(item, "F_HS_MaterialName"), i);
                            formView.Model.SetValue("F_HS_Specification", SQLUtils.GetFieldValue(item, "F_HS_Specification"), i);
                            formView.Model.SetValue("F_HS_SalUnit", SQLUtils.GetFieldValue(item, "F_HS_SalUnit"), i);
                            formView.Model.SetValue("F_HS_OrderQty", SQLUtils.GetFieldValue(item, "F_HS_OrderQty"), i);
                            formView.Model.SetValue("F_HS_OrderAmount", SQLUtils.GetFieldValue(item, "F_HS_OrderAmount"), i);
                            formView.Model.SetValue("F_HS_StockQty", SQLUtils.GetFieldValue(item, "F_HS_StockQty"), i);
                            formView.Model.SetValue("F_HS_StockAmount", SQLUtils.GetFieldValue(item, "F_HS_StockAmount"), i);
                            formView.Model.SetValue("F_HS_FirstInStockDate", SQLUtils.GetFieldValue(item, "F_HS_FirstInStockDate"), i);
                            formView.Model.SetValue("F_HS_SKUGroupCode", SQLUtils.GetFieldValue(item, "F_HS_SKUGroupCode"), i);

                            i++;
                        }
                    }

                    formView.UpdateView(entryName);
                }
                else
                {
                    this.View.ShowErrMessage("", "没有需要查询的数据", MessageBoxType.Error);
                }
            }
        }
        public DynamicObjectCollection GetObjects(Context ctx, string sql)
        {
            return SQLUtils.GetObjects(ctx, sql);
        }

        //public override void OnInitialize(InitializeEventArgs e)
        //{
        //    base.OnInitialize(e);

        //    initSql = string.Format(@"/*dialect*/ select a.F_HS_ListID,r.FNAME F_HS_BrandName,a.FNUMBER as F_HS_MaterialId,b.FNAME as F_HS_MaterialName,b.FSpecification as F_HS_Specification,
        //                                d.FNUMBER as F_HS_SalUnit,sum(k.F_HS_OrderQty) as F_HS_OrderQty ,sum(k.F_HS_OrderAmount) as F_HS_OrderAmount,sum(l.F_HS_StockQty) as F_HS_StockQty,sum(l.F_HS_StockAmount) as F_HS_StockAmount
        //                                from T_BD_MATERIAL a
        //                                inner join T_BD_MATERIAL_L b on a.FMATERIALID = b.FMATERIALID and b.FLOCALEID=2052
        //                                inner join T_BD_MATERIALSALE c on c.FMATERIALID = a.FMATERIALID
        //                                inner join T_BD_UNIT d on d.FUNITID = c.FSALEUNITID
        //                                inner join HS_T_BD_LISTID o on o.FID = a.F_HS_ListIDNew
        //                                inner join HS_T_BD_LISTID_L p on o.FID = p.FID
        //                                inner join HS_T_Brand j on j.FID = a.F_HS_Brand
        //                                inner join HS_T_BRAND_L r on r.FID = j.FID
        //                                inner join T_ORG_ORGANIZATIONS z on z.FORGID = a.fuseOrgID
        //                                left join 
        //                                (select a.FNUMBER as F_HS_MaterialId,f.FDATE,sum(e.FQTY) as F_HS_OrderQty,sum(case when f.F_HS_RATETOUSA = 0 then 0 else g.FAMOUNT/f.F_HS_RATETOUSA end) as F_HS_OrderAmount
	       //                                 from T_BD_MATERIAL a
	       //                                 inner join T_SAL_ORDERENTRY e on e.FMATERIALID = a.FMATERIALID
	       //                                 inner join T_SAL_ORDER f on e.FID = f.FID
	       //                                 inner join T_SAL_ORDERENTRY_F g on g.FENTRYID = e.FENTRYID and g.FID = e.FID
	       //                                 inner join T_SAL_ORDERFIN h on h.FID = f.FID
	       //                                 inner join T_ORG_ORGANIZATIONS c on f.FSALEORGID = c.FORGID 
	       //                                 where  not (f.FCLoseSTATUS = 'B' and e.FMRPCLOSESTATUS='A') 
        //                                        and a.FNUMBER not like '99.%' and f.FDOCUMENTSTATUS='C'
		      //                                  and c.FNUMBER = '100.01'
	       //                                 group by a.FNUMBER,f.FDATE
	       //                                 ) k on k.F_HS_MaterialId = a.FNUMBER
        //                                left join 
	       //                                 ( select a.FNUMBER as F_HS_MaterialId,k.FDATE,sum(l.FPriceUnitQty) F_HS_StockQty,sum(case when k.F_HS_RATETOUSA =0 then 0 else l.FAmount/k.F_HS_RATETOUSA end) F_HS_StockAmount 
	       //                                 from T_BD_MATERIAL a
	       //                                 inner join T_SAL_OUTSTOCKENTRY j on j.FMATERIALID = a.FMATERIALID
	       //                                 inner join T_SAL_OUTSTOCK k on k.FID = j.FID
	       //                                 inner join T_SAL_OUTSTOCKENTRY_F l on k.FID = l.FID
	       //                                 inner join T_SAL_OUTSTOCKFIN m on m.FID = k.FID
	       //                                 inner join T_ORG_ORGANIZATIONS c on k.FSALEORGID = c.FORGID
	       //                                 where a.FNUMBER not like '99.%' and k.FDOCUMENTSTATUS='C'
	       //                                 and c.FNUMBER = '100.01'
	       //                                 group by a.FNUMBER,k.FDATE
	       //                                 )  l on l.F_HS_MaterialId = a.FNUMBER 

	       //                                 where a.FNUMBER not like '99.%' 
	       //                                 and (k.F_HS_OrderQty is not null or k.F_HS_OrderAmount is not null or l.F_HS_StockQty is not null or l.F_HS_StockAmount is not null)
	       //                                 and z.FNUMBER='100.01'
	       //                                 group by a.F_HS_ListID,r.FNAME,a.FNUMBER,b.FNAME,b.FSpecification,d.FNUMBER
        //                               ");
        //}

        //public override void CreateNewData(BizDataEventArgs e)
        //{
        //    base.CreateNewData(e);

        //    DynamicObjectCollection coll = GetObjects(this.Context, initSql);
        //    DynamicObjectType dtType = this.View.BusinessInfo.GetDynamicObjectType();
        //    EntryEntity entity = (EntryEntity)this.View.BusinessInfo.GetEntity("F_HS_Entity");
        //    DynamicObject objData = new DynamicObject(dtType);
        //    DynamicObject entityObj = null;

        //    if (coll != null && coll.Count > 0)
        //    {
        //        int seq = 1;
        //        foreach (var item in coll)
        //        {
        //            if (item != null)
        //            {
        //                if (entity != null)
        //                {
        //                    entityObj = new DynamicObject(entity.DynamicObjectType);
        //                    entity.DynamicProperty.GetValue<DynamicObjectCollection>(objData).Add(entityObj);

        //                    entityObj["seq"] = seq;
        //                    entityObj["F_HS_ListID"] = SQLUtils.GetFieldValue(item, "F_HS_ListID");
        //                    entityObj["F_HS_BrandName"] = SQLUtils.GetFieldValue(item, "F_HS_BrandName");
        //                    entityObj["F_HS_MaterialId"] = SQLUtils.GetFieldValue(item, "F_HS_MaterialId");
        //                    entityObj["F_HS_MaterialName"] = SQLUtils.GetFieldValue(item, "F_HS_MaterialName");
        //                    entityObj["F_HS_Specification"] = SQLUtils.GetFieldValue(item, "F_HS_Specification");
        //                    entityObj["F_HS_SalUnit"] = SQLUtils.GetFieldValue(item, "F_HS_SalUnit");
        //                    entityObj["F_HS_OrderQty"] = SQLUtils.GetFieldValue(item, "F_HS_OrderQty");
        //                    entityObj["F_HS_OrderAmount"] = SQLUtils.GetFieldValue(item, "F_HS_OrderAmount");
        //                    entityObj["F_HS_StockQty"] = SQLUtils.GetFieldValue(item, "F_HS_StockQty");
        //                    entityObj["F_HS_StockAmount"] = SQLUtils.GetFieldValue(item, "F_HS_StockAmount");

        //                    seq++;

        //                }
        //            }
        //        }

        //        e.BizDataObject = objData;
        //    }

        //}

    }
}
