using Hands.K3.SCM.APP.Entity.K3WebApi;
using Hands.K3.SCM.APP.Entity.StructType;
using Hands.K3.SCM.APP.Entity.SynDataObject.DeliveryNotice;
using Hands.K3.SCM.APP.Entity.SynDataObject.SaleOrder;
using Hands.K3.SCM.APP.Utils.Utils;
using Hands.K3.SCM.APP.WebService;
using Kingdee.BOS;
using Kingdee.BOS.App;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Orm.DataEntity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Hands.K3.SCM.APP.DynamicFormPlugIn
{
    [Description("出货表--推单至物流商")]
    public class DeliveryNoticePlugIn : AbstractListPlugIn
    {
        static object objLock = new object();

        /// <summary>
        /// 获取被勾选中的出货表编码
        /// </summary>
        /// <returns></returns>
        private List<string> GetSelectedShipmentNos()
        {
            ListSelectedRowCollection rows = this.ListView.SelectedRowsInfo;

            return rows.Select(r => r.BillNo).ToList();
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="numbers"></param>
        /// <returns></returns>
        private DynamicObjectCollection GetObjects(Context ctx, List<string> numbers)
        {
            if (numbers != null && numbers.Count > 0)
            {
                string sql = string.Format(@"/*dialect*/ select a.FBillNo as FDelBillNo,g.FBILLNO as FShpBillNo,a.F_HS_DeliveryName,a.F_HS_MobilePhone,a.F_HS_DeliveryCity,a.F_HS_DeliveryProvinces,a.F_HS_DeliveryAddress
                                            ,a.F_HS_PostCode,k.F_HS_IsOil,b.F_HS_TotalWeight,l.FNAME,l.FSpecification,h.FNUMBER as FSettleCurrId,o.FPrice,b.FQTY,q.FNumber as unitNo
                                            ,j.FNUMBER as F_HS_RecipientCountry,c.FBillAmount,e.FLENGTH,e.FWIDTH,e.FHIGH,g.FBILLINGWEIGHT,t.FNUMBER as logisticsNo,g.FDecAmount
                                            from T_SAL_DELIVERYNOTICE a
                                            inner join T_SAL_DELIVERYNOTICEENTRY b on a.FID = b.FID
                                            inner join T_SAL_DELIVERYNOTICEENTRY_F o on o.FID = b.FID
                                            inner join T_SAL_DELIVERYNOTICEFIN c on a.FID = c.FID
                                            inner join HS_T_PackEntity d on d.FNOTCIENO = a.FBillNo and d.FNOTICEENTITYNUM = b.FENTRYID
                                            inner join HS_T_Pack e on e.FID = d.FID
                                            inner join HS_T_ShipmentEntry f on f.FBOXNO = e.FCARTONNO
                                            inner join HS_T_Shipment g on g.FID = f.FID
                                            inner join T_BD_CURRENCY h on h.FCURRENCYID = c.FSettleCurrId
                                            inner join VW_BAS_ASSISTANTDATA_CountryName j on j.FCountry = a.F_HS_RecipientCountry
                                            inner join T_BD_MATERIAL k on b.FMATERIALID = k.FMATERIALID
                                            inner join T_BD_MATERIAL_L l on l.FMATERIALID = k.FMATERIALID
                                            inner join T_BD_MATERIALSALE p on l.FMATERIALID=p.FMATERIALID 
                                            inner join T_BD_UNIT q on p.FSALEUNITID=q.FUNITID 
                                            inner join T_HS_ShipMethods t on t.FID = g.FSHIPMETHODS
                                            where g.FBILLNO in ('{0}')", string.Join("','", numbers));
                return SQLUtils.GetObjects(ctx, sql);
            }
            return null;
        }
        /// <summary>
        /// 获取发货通知单
        /// </summary>
        /// <returns></returns>
        private List<DeliveryNotice> GetDeliveryNotices(Context ctx)
        {
            DeliveryNotice notice = new DeliveryNotice();
            List<DeliveryNotice> notices = new List<DeliveryNotice>();

            List<string> numbers = GetSelectedShipmentNos();
            DynamicObjectCollection coll = GetObjects(ctx, numbers);

            var group = from c in coll
                        where c["FDelBillNo"] != null
                        group c by c["FDelBillNo"]
                        into g
                        select g;

            if (group != null && group.Count() > 0)
            {
                notices = new List<DeliveryNotice>();

                foreach (var item in group)
                {
                    if (item != null)
                    {
                        notice = new DeliveryNotice();

                        foreach (var obj in item)
                        {
                            if (obj != null)
                            {
                                notice.FShipmentBillNo.Add(SQLUtils.GetFieldValue(obj, "FShpBillNo"));
                            }
                        }

                        notice.FBillNo = SQLUtils.GetFieldValue(item.ElementAt(0), "FDelBillNo");

                        notice.F_HS_DeliveryName = SQLUtils.GetFieldValue(item.ElementAt(0), "F_HS_DeliveryName");
                        notice.F_HS_MobilePhone = SQLUtils.GetFieldValue(item.ElementAt(0), "F_HS_MobilePhone");

                        notice.F_HS_DeliveryCity = SQLUtils.GetFieldValue(item.ElementAt(0), "F_HS_DeliveryCity");
                        notice.F_HS_DeliveryProvinces = SQLUtils.GetFieldValue(item.ElementAt(0), "F_HS_DeliveryProvinces");

                        notice.F_HS_DeliveryAddress = SQLUtils.GetFieldValue(item.ElementAt(0), "F_HS_DeliveryAddress");
                        notice.F_HS_RecipientCountry = SQLUtils.GetFieldValue(item.ElementAt(0), "F_HS_RecipientCountry");

                        notice.F_HS_PostCode = SQLUtils.GetFieldValue(item.ElementAt(0), "F_HS_PostCode");

                        notice.orderFin = new K3SaleOrderFinance();
                        notice.orderFin.FSettleCurrID = SQLUtils.GetFieldValue(item.ElementAt(0), "FSettleCurrID");
                        notice.orderFin.FBillAmount = Convert.ToDecimal(JsonUtils.ConvertObjectToString(SQLUtils.GetFieldValue(item.ElementAt(0), "FBillAmount")));
                        notice.F_HS_ShippingMethod = SQLUtils.GetFieldValue(item.ElementAt(0), "logisticsNo");
                        notice.F_HS_AllTotalWeight = Convert.ToDecimal(SQLUtils.GetFieldValue(item.ElementAt(0), "F_HS_AllTotalWeight"));
                        notice.FDecAmount = Convert.ToDecimal(SQLUtils.GetFieldValue(item.ElementAt(0), "FDecAmount"));

                        List<DynamicObject> entry = item.ToList();
                        SetDeliveryNoticeEntry(ctx, entry, notice);

                        Dimension dim = new Dimension();
                        dim.Length = SQLUtils.GetFieldValue(item.ElementAt(0), "FLENGTH");
                        dim.Width = SQLUtils.GetFieldValue(item.ElementAt(0), "FWIDTH");
                        dim.Height = SQLUtils.GetFieldValue(item.ElementAt(0), "FHIGH");
                        dim.Units = "CM";

                        Package pac = new Package();
                        pac.Weight = Convert.ToDecimal(SQLUtils.GetFieldValue(item.ElementAt(0), "FBILLINGWEIGHT")) / 100;
                        pac.Units = "KG";
                        pac.Dimension = dim;

                        notice.Packages.Add(pac);
                        notices.Add(notice);
                    }
                }
            }

            return notices;
        }

        /// <summary>
        /// 获取发货通知单
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="notices"></param>
        /// <returns></returns>
        private List<DeliveryNotice> GetDeliveryNotices(Context ctx, List<DeliveryNotice> notices)
        {
            Dictionary<string, Object> dict = ShipWebService.GetLogisticsDetail<Object>(ctx, notices);
            foreach (var item in dict)
            {
                if (item.Key.CompareTo("notices") == 0)
                {
                    return (List<DeliveryNotice>)item.Value;
                }
            }
            return null;
        }

        /// <summary>
        /// 保存物流跟踪明细
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        private int SaveLogisticsTrace(Context ctx, List<DeliveryNotice> notices)
        {

            int count = 0;

            lock (objLock)
            {
                if (notices != null && notices.Count > 0)
                {
                    foreach (var notice in notices)
                    {
                        if (notice != null)
                        {
                            if (notice.TraceEntry != null && notice.TraceEntry.Count > 0)
                            {
                                foreach (var trace in notice.TraceEntry)
                                {
                                    string sql = string.Format(@"/*dialect*/ 
                                                                 insert into {0} (FID,FEntryID,F_HS_SHIPMETHODS,F_HS_DELIDATE,F_HS_CARRYBILLNO,F_HS_QUERYURL,F_HS_IsTrack)
                                                                 select @FID as FID,@FEntryID as FEntryID,@F_HS_SHIPMETHODS as F_HS_SHIPMETHODS,@F_HS_DELIDATE as F_HS_DELIDATE,@F_HS_CARRYBILLNO as F_HS_CARRYBILLNO,
                                                                 @F_HS_QUERYURL as F_HS_QUERYURL,@F_HS_IsTrack as F_HS_IsTrack

                                           ", HSTableConst.HS_T_LogisTrack);
                                    try
                                    {
                                        var para = new List<SqlParam>();
                                        para.Add(new SqlParam("@FID", KDDbType.Int32, GetFId(ctx, notice.FBillNo)));
                                        para.Add(new SqlParam("@FEntryID", KDDbType.Int32, GetPrimaryKey(ctx, HSTableConst.HS_T_LogisTrack, "FEntryID")));
                                        para.Add(new SqlParam("@F_HS_SHIPMETHODS", KDDbType.Int32, SQLUtils.GetShipMethodId(ctx, "CN-FEDEX")));
                                        para.Add(new SqlParam("@F_HS_DELIDATE", KDDbType.Date, DateTime.Now));
                                        para.Add(new SqlParam("@F_HS_CARRYBILLNO", KDDbType.String, trace.F_HS_CarryBillNO));
                                        para.Add(new SqlParam("@F_HS_QUERYURL", KDDbType.String, HSDeliveryNoticeConst.F_HS_QUERYURL));
                                        para.Add(new SqlParam("@F_HS_IsTrack", KDDbType.String, "0"));

                                        count += ServiceHelper.GetService<IDBService>().Execute(ctx, sql, para);

                                        if (count > 0)
                                        {
                                            string sql_ = string.Format(@"/*dialect*/ insert into {0} (FID,FBILLNO,F_HS_CARRYBILLNO,F_HS_SHIPMETHODS,F_HS_DOCUMENTSTATUS) values ({1},'{2}','{3}','{4}','{5}')",
                                                                                      HSTableConst.HS_T_LogisticsInfo, GetFId(ctx, notice.FBillNo), notice.FBillNo, "449044304137821", SQLUtils.GetShipMethodId(ctx, "CN-FEDEX"), LogisticsQuery.Query
                                                                       );
                                            int count_ = DBUtils.Execute(ctx, sql_);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        this.View.ShowErrMessage("", "发货通知单单号【" + notice.FBillNo + "】物流跟踪明细更新失败" + System.Environment.NewLine + System.Environment.NewLine + ex.Message + ex.StackTrace);
                                    }
                                }

                            }

                        }
                    }
                }

            }

            //UpdateShipment(ctx,notices);

            return count;
        }

        /// <summary>
        /// 保存物流轨迹信息
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public int SaveLogisticsLocus(Context ctx)
        {
            List<DeliveryNotice> notices = TraceWebService.GetLogisticsTraceDetail(ctx);
            int count = 0;

            lock (objLock)
            {
                if (notices != null && notices.Count > 0)
                {
                    foreach (var notice in notices)
                    {
                        if (notice != null)
                        {
                            if (notice.LocusEntry != null && notice.LocusEntry.Count > 0)
                            {
                                foreach (var locus in notice.LocusEntry)
                                {
                                    string sql = string.Format(@"/*dialect*/ if not exists(select * from {0} where FEntryID = @FEntryID and FDetailID = @FDetailID 
                                                                             and F_HS_SIGNTIME = @F_HS_SIGNTIME and F_HS_TRACKINFO = @F_HS_TRACKINFO and F_HS_AREACODE = @F_HS_AREACODE 
                                                                             and F_HS_AREANAME = @F_HS_AREANAME and F_HS_TARCKSTATUS = @F_HS_TARCKSTATUS)

                                                                             insert into {1} (FEntryID,FDetailID,F_HS_SIGNTIME,F_HS_TRACKINFO,F_HS_AREACODE,F_HS_AREANAME,F_HS_TARCKSTATUS)
                                                                             select @FEntryID as FEntryID,@FDetailID as FDetailID,@F_HS_SIGNTIME as F_HS_SIGNTIME,
                                                                             @F_HS_TRACKINFO as F_HS_TRACKINFO,@F_HS_AREACODE as F_HS_AREACODE,@F_HS_AREANAME as F_HS_AREANAME,@F_HS_TARCKSTATUS as F_HS_TARCKSTATUS

                                           ", HSTableConst.HS_SAL_NOTICELocus, HSTableConst.HS_SAL_NOTICELocus);
                                    try
                                    {
                                        var para = new List<SqlParam>();

                                        int entryId = GetFEntryId(ctx, GetFId(ctx, notice.FBillNo));
                                        para.Add(new SqlParam("@FEntryID", KDDbType.Int32, GetFEntryId(ctx, GetFId(ctx, notice.FBillNo))));
                                        int primaryKey = GetPrimaryKey(ctx, HSTableConst.HS_SAL_NOTICELocus, "FDetailID");
                                        para.Add(new SqlParam("@FDetailID", KDDbType.Int32, GetPrimaryKey(ctx, HSTableConst.HS_SAL_NOTICELocus, "FDetailID")));
                                        para.Add(new SqlParam("@F_HS_SIGNTIME", KDDbType.Date, DateTime.Now/*locus.F_HS_Signtime*/));
                                        para.Add(new SqlParam("@F_HS_TRACKINFO", KDDbType.String, string.IsNullOrEmpty(locus.F_HS_TrackInfo) ? "" : locus.F_HS_TrackInfo));
                                        para.Add(new SqlParam("@F_HS_AREACODE", KDDbType.String, string.IsNullOrEmpty(locus.F_HS_AreaCode) ? "" : locus.F_HS_AreaCode));
                                        para.Add(new SqlParam("@F_HS_AREANAME", KDDbType.String, string.IsNullOrEmpty(locus.F_HS_AreaName) ? "" : locus.F_HS_AreaName));
                                        para.Add(new SqlParam("@F_HS_TARCKSTATUS", KDDbType.String, string.IsNullOrEmpty(locus.F_HS_TarckStatus) ? "" : locus.F_HS_TarckStatus));

                                        count += ServiceHelper.GetService<IDBService>().Execute(ctx, sql, para);
                                    }
                                    catch (Exception ex)
                                    {
                                        this.View.ShowErrMessage("", "发货通知单单号【" + notice.FBillNo + "】物流轨迹明细更新失败" + System.Environment.NewLine + ex.Message + System.Environment.NewLine + ex.StackTrace);
                                    }

                                }
                            }
                        }
                    }
                }

            }

            return 0;
        }

        /// <summary>
        /// 更新出货表运单号字段信息
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="notices"></param>
        /// <returns></returns>
        private int UpdateShipment(Context ctx, List<DeliveryNotice> notices)
        {
            int count = 0;
            if (notices != null && notices.Count > 0)
            {
                foreach (var notice in notices)
                {
                    if (notice != null)
                    {
                        if (notice.FShipmentBillNo != null && notice.FShipmentBillNo.Count > 0)
                        {
                            foreach (var item in notice.FShipmentBillNo)
                            {
                                if (notice.TraceEntry != null && notice.TraceEntry.Count > 0)
                                {
                                    string sql = string.Format(@"/*dialect*/ update HS_T_Shipment set FCARRIAGENO = '{0}' where FBillNo = '{1}'", notice.TraceEntry.ElementAt(0).F_HS_CarryBillNO, item.ToString());
                                    count += DBUtils.Execute(ctx, sql);
                                }
                            }
                        }
                    }

                }
            }
            return count;
        }
        /// <summary>
        /// 获取发货通知单内码
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="FDelBillNo"></param>
        /// <returns></returns>
        private int GetFId(Context ctx, string FBillNo)
        {
            if (!string.IsNullOrWhiteSpace(FBillNo))
            {
                string sql = string.Format(@"/*dialect*/ select FID from {0} where FBillNo = '{1}'", HSTableConst.T_SAL_DELIVERYNOTICE, FBillNo);
                return Convert.ToInt32(JsonUtils.ConvertObjectToString(SQLUtils.GetObject(ctx, sql, "FID")));
            }
            return 0;
        }

        private int GetFEntryId(Context ctx, int FId)
        {
            string sql = string.Format(@"/*dialect*/ select FEntryID from {0} where FID = {1}", HSTableConst.HS_T_LogisTrack, FId);
            return Convert.ToInt32(JsonUtils.ConvertObjectToString(SQLUtils.GetObject(ctx, sql, "FEntryID")));
        }

        private int GetPrimaryKey(Context ctx, string tableName, string fieldName)
        {
            string sql = string.Format(@"/*dialect*/ select MAX(" + fieldName + ") " + fieldName + " from {0} ", tableName);
            return Convert.ToInt32(JsonUtils.ConvertObjectToString(SQLUtils.GetObject(ctx, sql, fieldName))) + 1;
        }
        /// <summary>
        /// 设置发货通知单明细信息
        /// </summary>
        /// <param name="notice"></param>
        /// <param name="coll"></param>
        /// <param name="ctx"></param>
        private void SetDeliveryNoticeEntry(Context ctx, List<DynamicObject> objs, DeliveryNotice notice)
        {
            if (objs != null && objs.Count > 0)
            {
                notice.OrderEntry = new List<K3SalOrderEntryInfo>();

                foreach (var item in objs)
                {
                    if (item != null)
                    {
                        K3SalOrderEntryInfo entry = new K3SalOrderEntryInfo();

                        entry.FUnitId = SQLUtils.GetFieldValue(item, "unitNo");
                        //DynamicObject mat = item["MaterialID"] as DynamicObject;
                        //entry.material.FNumber = SQLUtils.GetFieldValue(mat, "Name");
                        entry.FMaterialName = SQLUtils.GetFieldValue(item, "FNAME");
                        entry.F_HS_IsOil = SQLUtils.GetFieldValue(item, "F_HS_IsOil");
                        entry.F_HS_TotalWeight = Convert.ToDecimal(SQLUtils.GetFieldValue(item, "F_HS_TotalWeight")) / 100;
                        entry.FQTY = Convert.ToDecimal(SQLUtils.GetFieldValue(item, "FQTY"));
                        entry.FTAXPRICE = SetPriceByDecAmount(item);

                        notice.OrderEntry.Add(entry);
                    }

                }
            }
        }

        /// <summary>
        /// 商品金额占比
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private decimal GetProportion(DynamicObject obj)
        {
            if (obj != null)
            {
                decimal amount = Convert.ToDecimal(SQLUtils.GetFieldValue(obj, "FBillAmount"));
                decimal price = Convert.ToDecimal(SQLUtils.GetFieldValue(obj, "FPrice"));
                decimal qty = Convert.ToDecimal(SQLUtils.GetFieldValue(obj, "FQTY"));

                if (amount > 0)
                {
                    if (price > 0)
                    {
                        return price * qty / amount;
                    }
                    else
                    {
                        return 1 * qty / amount;
                    }
                }
            }
            return 0;
        }

        /// <summary>
        /// 商品单价（以申报金额重新计算）
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private decimal SetPriceByDecAmount(DynamicObject obj)
        {
            if (obj != null)
            {
                decimal decAmount = Convert.ToDecimal(SQLUtils.GetFieldValue(obj, "FDecAmount"));
                decimal qty = Convert.ToDecimal(SQLUtils.GetFieldValue(obj, "FQTY"));

                if (decAmount > 0 && qty > 0)
                {
                    return decAmount * GetProportion(obj) / qty;
                }
            }
            return 1;
        }

        private List<string> GetCarryNos(List<DeliveryNotice> notices)
        {
            List<string> numbers = new List<string>();
           
            if(notices != null && notices.Count > 0)
            {
                foreach (var notice in notices)
                {
                    if (notice.TraceEntry != null && notice.TraceEntry.Count > 0)
                    {
                        foreach (var entry in notice.TraceEntry)
                        {
                            if(entry != null)
                            {
                                numbers.Add(entry.F_HS_CarryBillNO);
                            }
                        }
                    }
                }
            }
           
            return numbers;
        }

        private void ShowMessage(Context ctx, HttpResponseResult result)
        {
            if (result != null && !string.IsNullOrWhiteSpace(result.Message))
            {
                this.View.ShowErrMessage("", result.Message, MessageBoxType.Error);
            }
            else
            {
                this.View.ShowErrMessage("", "Fedex无响应信息返回！", MessageBoxType.Error);
            }

        }
        public override void ButtonClick(ButtonClickEventArgs e)
        {
            base.ButtonClick(e);

            switch (e.Key)
            {
                case "tbPlaceOrderFedx":
                    List<DeliveryNotice> notices = GetDeliveryNotices(this.Context, GetDeliveryNotices(this.Context));
                    SaveLogisticsTrace(this.Context, notices);
                    UpdateShipment(this.Context, notices);
                    break;
            }
        }
        public override void BarItemClick(BarItemClickEventArgs e)
        {
            base.BarItemClick(e);

            switch (e.BarItemKey)
            {
                case "tbPlaceOrderFedx":
                    //SaveLogisticsLocus(this.Context);
                    List<DeliveryNotice> notices = GetDeliveryNotices(this.Context);
                    HttpResponseResult result = null;
                    Dictionary<string, Object> dict = ShipWebService.GetLogisticsDetail<Object>(this.Context, notices);
                    foreach (var item in dict)
                    {
                        if (item.Key.CompareTo("notices") == 0)
                        {
                            notices = (List<DeliveryNotice>)item.Value;
                        }
                        if (item.Key.CompareTo("result") == 0)
                        {
                            result = (HttpResponseResult)item.Value;
                        }
                    }
                    SaveLogisticsTrace(this.Context, notices);

                    UpdateShipment(this.Context, notices);
                    //ShowMessage(this.Context, result);
                    DynamicFormShowParameter showParam = new DynamicFormShowParameter();
                
                    showParam.CustomParams.Add("trackingNumber", JsonConvert.SerializeObject(GetCarryNos(notices)));
                    showParam.FormId = "HS_SELECTPRINTER";

                    this.View.ShowForm(showParam);
                    
                    //SaveLogisticsLocus(this.Context,notices);


                    break;
            }
        }

        
    }
}


