using System;
using System.Collections.Generic;
using System.Linq;

using Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Hands.K3.SCM.APP.Entity.SynDataObject.BacthAdjust;
using System.ComponentModel;
using Hands.K3.SCM.APP.Utils.Utils;
using HS.K3.Common.Abbott;
using Hands.K3.SCM.APP.Entity.K3WebApi;
using Hands.K3.SCM.APP.Entity.EnumType;
using Hands.K3.SCM.APP.Entity.SynDataObject.Material_;
using Hands.K3.SCM.APP.Entity.StructType;

namespace Hands.K3.SCM.App.ServicePlugIn
{
    [Description("批量调价服务插件-同步至HC网站")]
    public class BatchAdjustSerPlugIn : AbstractOSPlugIn
    {
        [Kingdee.BOS.Util.HotUpdate]
        public override SynchroDataType DataType
        {
            get
            {
                return SynchroDataType.BatchAdjust;
            }
        }
        /// <summary>
        /// 获取同步数据
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="objects"></param>
        /// <returns></returns>
        public override IEnumerable<AbsSynchroDataInfo> GetK3Datas(Context ctx, List<DynamicObject> objects,ref HttpResponseResult result)
        {
            K3BatchAdjust just = null;
            List<K3BatchAdjust> justs = null;

            result = new HttpResponseResult();
            result.Success = true;

            if (objects != null && objects.Count > 0)
            {
                justs = new List<K3BatchAdjust>();

                foreach (var obj in objects)
                {
                    if (obj != null)
                    {
                        bool yNInSync = Convert.ToBoolean(SQLUtils.GetFieldValue(obj, "F_HS_YNInSync"));

                        if (!yNInSync)
                        {
                            if(GetK3BatchAdjustEntry(ctx, obj) != null && GetK3BatchAdjustEntry(ctx, obj).Count > 0)
                            {
                                just = new K3BatchAdjust();

                                just.FBillNo = SQLUtils.GetFieldValue(obj, "BillNo");
                                just.SrcNo = just.FBillNo;
                                just.FDate = TimeHelper.GetTimeStamp(Convert.ToDateTime(SQLUtils.GetFieldValue(obj, "Date")));
                                just.F_HS_YNInSync = yNInSync;

                                just.Entry = GetK3BatchAdjustEntry(ctx, obj);
                                justs.Add(just);
                            } 
                        }
                    }
                }
            }
            return justs;
        }

        public override string GetExecuteUpdateSql(Context ctx, List<AbsSynchroDataInfo> datas)
        {
            string sql = string.Empty;

            if (datas != null && datas.Count() > 0)
            {
                foreach (var data in datas)
                {
                    if (data != null)
                    {
                        K3BatchAdjust adjust = data as K3BatchAdjust;

                        if (adjust != null)
                        {
                            sql += string.Format(@"/*dialect*/ update a  set a.F_HS_YNInSync = '{0}'
                                                            from T_SAL_BATCHADJUST a
                                                            inner join T_SAL_BATCHADJUSTENTRY b on a.FID = b.FID
                                                            where a.FBILLNO = '{1}'
                                                            and a.F_HS_YNInSync = '{2}'
                                                            ", "1", adjust.FBillNo, "0");
                            sql += string.Format(@"/*dialect*/ update a  set a.F_HS_YNEntryInSync = '{0}'
                                                            from T_SAL_BATCHADJUSTENTRY a
                                                            inner join T_SAL_BATCHADJUST b on a.FID = b.FID
                                                            where b.FBILLNO = '{1}'
                                                            and a.F_HS_YNEntryInSync = '{2}'
                                                            ", "1", adjust.FBillNo, "0");

                            if (adjust.Entry != null && adjust.Entry.Count > 0)
                            {
                                foreach (var en in adjust.Entry)
                                {
                                    if (en != null)
                                    {
                                        //如果批量调价单调的特价价目表，不反写销售价目表美国价格、澳洲价格、欧洲价格
                                        if (!en.F_HS_YNSpecialPrice)
                                        {
                                            if (!string.IsNullOrWhiteSpace(en.F_HS_AfterUSPrice))
                                            {
                                                if (Convert.ToDecimal(en.F_HS_AfterUSPrice) > 0)
                                                {
                                                    sql += string.Format(@"/*dialect*/update a set a.F_HS_USPrice = {0}
                                                                        from T_SAL_PRICELISTENTRY a
                                                                        inner join T_SAL_PRICELIST b on a.FID = b.FID
                                                                        inner join T_SAL_BATCHADJUSTENTRY c on c.FPRICELISTID = a.FID and a.FMATERIALID=c.FMATERIALID and a.FUNITID=c.FMATUNITID  and a.FEFFECTIVEDATE=c.FAFTEREFFDATE and a.FEXPRIYDATE=c.FAFTERUNEFFDATE
                                                                        inner join T_SAL_BATCHADJUST d on d.FID = c.FID
                                                                        inner join T_BD_MATERIAL e on a.FMATERIALID = e.FMATERIALID
                                                                        where d.FBILLNO = '{1}'
																		and e.FNUMBER = '{2}' 
																		and b.FNUMBER = '{3}'", en.F_HS_AfterUSPrice, adjust.FBillNo, en.FMaterialId, en.FPriceListId) + Environment.NewLine;
                                                }
                                            }
                                            if (!string.IsNullOrWhiteSpace(en.F_HS_AfterUSNoPostagePrice))
                                            {
                                                if (Convert.ToDecimal(en.F_HS_AfterUSNoPostagePrice) > 0)
                                                {
                                                    sql += string.Format(@"/*dialect*/update a set a.F_HS_USNoPostagePrice = {0}
                                                                        from T_SAL_PRICELISTENTRY a
                                                                        inner join T_SAL_PRICELIST b on a.FID = b.FID
                                                                        inner join T_SAL_BATCHADJUSTENTRY c on c.FPRICELISTID = a.FID and a.FMATERIALID=c.FMATERIALID and a.FUNITID=c.FMATUNITID  and a.FEFFECTIVEDATE=c.FAFTEREFFDATE and a.FEXPRIYDATE=c.FAFTERUNEFFDATE
                                                                        inner join T_SAL_BATCHADJUST d on d.FID = c.FID
                                                                        inner join T_BD_MATERIAL e on a.FMATERIALID = e.FMATERIALID
                                                                        where d.FBILLNO = '{1}'
																		and e.FNUMBER = '{2}' 
																		and b.FNUMBER = '{3}'", en.F_HS_AfterUSNoPostagePrice, adjust.FBillNo, en.FMaterialId, en.FPriceListId) + Environment.NewLine;
                                                }
                                            }
                                            if (!string.IsNullOrWhiteSpace(en.F_HS_AfterAUPrice))
                                            {
                                                if (Convert.ToDecimal(en.F_HS_AfterAUPrice) > 0)
                                                {
                                                    sql += string.Format(@"/*dialect*/update a set a.F_HS_AUPrice = {0}
                                                                        from T_SAL_PRICELISTENTRY a
                                                                        inner join T_SAL_PRICELIST b on a.FID = b.FID
                                                                        inner join T_SAL_BATCHADJUSTENTRY c on c.FPRICELISTID = a.FID and a.FMATERIALID=c.FMATERIALID and a.FUNITID=c.FMATUNITID  and a.FEFFECTIVEDATE=c.FAFTEREFFDATE and a.FEXPRIYDATE=c.FAFTERUNEFFDATE
                                                                        inner join T_SAL_BATCHADJUST d on d.FID = c.FID
                                                                        inner join T_BD_MATERIAL e on a.FMATERIALID = e.FMATERIALID
                                                                        where d.FBILLNO = '{1}'
																		and e.FNUMBER = '{2}' 
																		and b.FNUMBER = '{3}'", en.F_HS_AfterAUPrice, adjust.FBillNo, en.FMaterialId, en.FPriceListId) + Environment.NewLine;
                                                }
                                            }
                                            if (!string.IsNullOrWhiteSpace(en.F_HS_AfterAUNoPostagePrice))
                                            {
                                                if (Convert.ToDecimal(en.F_HS_AfterAUNoPostagePrice) > 0)
                                                {
                                                    sql += string.Format(@"/*dialect*/update a set a.F_HS_AUNoPostagePrice = {0}
                                                                        from T_SAL_PRICELISTENTRY a
                                                                        inner join T_SAL_PRICELIST b on a.FID = b.FID
                                                                        inner join T_SAL_BATCHADJUSTENTRY c on c.FPRICELISTID = a.FID and a.FMATERIALID=c.FMATERIALID and a.FUNITID=c.FMATUNITID  and a.FEFFECTIVEDATE=c.FAFTEREFFDATE and a.FEXPRIYDATE=c.FAFTERUNEFFDATE
                                                                        inner join T_SAL_BATCHADJUST d on d.FID = c.FID
                                                                        inner join T_BD_MATERIAL e on a.FMATERIALID = e.FMATERIALID
                                                                        where d.FBILLNO = '{1}'
																		and e.FNUMBER = '{2}' 
																		and b.FNUMBER = '{3}'", en.F_HS_AfterAUNoPostagePrice, adjust.FBillNo, en.FMaterialId, en.FPriceListId) + Environment.NewLine;
                                                }
                                            }
                                            if (!string.IsNullOrWhiteSpace(en.F_HS_AfterEUPrice))
                                            {
                                                if (Convert.ToDecimal(en.F_HS_AfterEUPrice) > 0)
                                                {
                                                    sql += string.Format(@"/*dialect*/update a set a.F_HS_EUPrice = {0}
                                                                        from T_SAL_PRICELISTENTRY a
                                                                        inner join T_SAL_PRICELIST b on a.FID = b.FID
                                                                        inner join T_SAL_BATCHADJUSTENTRY c on c.FPRICELISTID = a.FID and a.FMATERIALID=c.FMATERIALID and a.FUNITID=c.FMATUNITID  and a.FEFFECTIVEDATE=c.FAFTEREFFDATE and a.FEXPRIYDATE=c.FAFTERUNEFFDATE
                                                                        inner join T_SAL_BATCHADJUST d on d.FID = c.FID
                                                                        inner join T_BD_MATERIAL e on a.FMATERIALID = e.FMATERIALID
                                                                        where d.FBILLNO = '{1}'
																		and e.FNUMBER = '{2}' 
																		and b.FNUMBER = '{3}'", en.F_HS_AfterEUPrice, adjust.FBillNo, en.FMaterialId, en.FPriceListId) + Environment.NewLine;
                                                }
                                            }

                                            if (!string.IsNullOrWhiteSpace(en.F_HS_AfterJPNoPostagePrice))
                                            {
                                                if (Convert.ToDecimal(en.F_HS_AfterJPNoPostagePrice) > 0)
                                                {
                                                    sql += string.Format(@"/*dialect*/update a set a.F_HS_JPNoPostagePrice = {0}
                                                                        from T_SAL_PRICELISTENTRY a
                                                                        inner join T_SAL_PRICELIST b on a.FID = b.FID
                                                                        inner join T_SAL_BATCHADJUSTENTRY c on c.FPRICELISTID = a.FID and a.FMATERIALID=c.FMATERIALID and a.FUNITID=c.FMATUNITID  and a.FEFFECTIVEDATE=c.FAFTEREFFDATE and a.FEXPRIYDATE=c.FAFTERUNEFFDATE
                                                                        inner join T_SAL_BATCHADJUST d on d.FID = c.FID
                                                                        inner join T_BD_MATERIAL e on a.FMATERIALID = e.FMATERIALID
                                                                        where d.FBILLNO = '{1}'
																		and e.FNUMBER = '{2}' 
																		and b.FNUMBER = '{3}'", en.F_HS_AfterJPNoPostagePrice, adjust.FBillNo, en.FMaterialId, en.FPriceListId) + Environment.NewLine;
                                                }
                                            }

                                            if (!string.IsNullOrWhiteSpace(en.F_HS_AfterKRNoPostagePrice))
                                            {
                                                if (Convert.ToDecimal(en.F_HS_AfterKRNoPostagePrice) > 0)
                                                {
                                                    sql += string.Format(@"/*dialect*/update a set a.F_HS_KRNoPostagePrice = {0}
                                                                        from T_SAL_PRICELISTENTRY a
                                                                        inner join T_SAL_PRICELIST b on a.FID = b.FID
                                                                        inner join T_SAL_BATCHADJUSTENTRY c on c.FPRICELISTID = a.FID and a.FMATERIALID=c.FMATERIALID and a.FUNITID=c.FMATUNITID  and a.FEFFECTIVEDATE=c.FAFTEREFFDATE and a.FEXPRIYDATE=c.FAFTERUNEFFDATE
                                                                        inner join T_SAL_BATCHADJUST d on d.FID = c.FID
                                                                        inner join T_BD_MATERIAL e on a.FMATERIALID = e.FMATERIALID
                                                                        where d.FBILLNO = '{1}'
																		and e.FNUMBER = '{2}' 
																		and b.FNUMBER = '{3}'", en.F_HS_AfterKRNoPostagePrice, adjust.FBillNo, en.FMaterialId, en.FPriceListId) + Environment.NewLine;
                                                }
                                            }
                                            if (!string.IsNullOrWhiteSpace(en.F_HS_AfterEUNoPostagePrice))
                                            {
                                                if (Convert.ToDecimal(en.F_HS_AfterEUNoPostagePrice) > 0)
                                                {
                                                    sql += string.Format(@"/*dialect*/update a set a.F_HS_EUNoPostagePrice = {0}
                                                                        from T_SAL_PRICELISTENTRY a
                                                                        inner join T_SAL_PRICELIST b on a.FID = b.FID
                                                                        inner join T_SAL_BATCHADJUSTENTRY c on c.FPRICELISTID = a.FID and a.FMATERIALID=c.FMATERIALID and a.FUNITID=c.FMATUNITID  and a.FEFFECTIVEDATE=c.FAFTEREFFDATE and a.FEXPRIYDATE=c.FAFTERUNEFFDATE
                                                                        inner join T_SAL_BATCHADJUST d on d.FID = c.FID
                                                                        inner join T_BD_MATERIAL e on a.FMATERIALID = e.FMATERIALID
                                                                        where d.FBILLNO = '{1}'
																		and e.FNUMBER = '{2}' 
																		and b.FNUMBER = '{3}'", en.F_HS_AfterEUNoPostagePrice, adjust.FBillNo, en.FMaterialId, en.FPriceListId) + Environment.NewLine;
                                                }
                                            }
                                        } 
                                    }
                                }
                            }
                        }
                    }
                }
            }
            
            return sql;
        }
        /// <summary>
        /// 调价单明细
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        private List<K3BatchAdjustEntry> GetK3BatchAdjustEntry(Context ctx, DynamicObject obj)
        {
            DynamicObjectCollection coll = obj["SAL_BATCHADJUSTENTRY"] as DynamicObjectCollection;
            K3BatchAdjustEntry entry = null;
            List<K3BatchAdjustEntry> entrys = null;

            if (entrys == null || entrys.Count == 0)
            {
                if (coll != null && coll.Count > 0)
                {
                    entrys = new List<K3BatchAdjustEntry>();

                    foreach (var item in coll)
                    {
                        if (item != null)
                        {
                            bool entryInSync = Convert.ToBoolean(SQLUtils.GetFieldValue(item, "F_HS_YNEntryInSync"));

                            if (!entryInSync)
                            {
                                entry = new K3BatchAdjustEntry();

                                entry.FEntryId = Convert.ToInt32(SQLUtils.GetFieldValue(item, "Id"));
                                entry.FAdjustType = SQLUtils.GetFieldValue(item, "AdjustType");
                                entry.FAfterPrice = SQLUtils.GetFieldValue(item, "AfterPrice").Equals("0") ? "" : SQLUtils.GetFieldValue(item, "AfterPrice");

                                DynamicObject price = item["PriceListId"] as DynamicObject;
                                entry.FPriceListId = SQLUtils.GetFieldValue(price, "Number");
                                entry.FPriceLevel = SQLUtils.GetFieldValue(price, "Name");

                                DynamicObject currency = price["CurrencyId"] as DynamicObject;
                                entry.FCurrencyId = SQLUtils.GetFieldValue(currency, "Number");

                                DynamicObject material = item["MaterialId"] as DynamicObject;
                                entry.FMaterialId = SQLUtils.GetFieldValue(material, "Number");

                                if (this.Direction == SynchroDirection.ToB2B)
                                {
                                    Material mat = SQLUtils.GetMaterial(ctx, entry.FMaterialId);

                                    if (mat != null)
                                    {
                                        entry.F_HS_IsOil = mat.F_HS_IsOil;
                                        entry.F_HS_PRODUCTSTATUS = mat.F_HS_PRODUCTSTATUS;
                                        entry.F_HS_DropShipOrderPrefix = mat.F_HS_DropShipOrderPrefix;
                                    }
                                }

                                entry.FAfterEffDate = Convert.ToDateTime(SQLUtils.GetFieldValue(item, "AfterEffDate")).ToString("yyyy-MM-dd");
                                entry.FAfterUnEffDate = Convert.ToDateTime(SQLUtils.GetFieldValue(item, "AfterUnEffDate")).ToString("yyyy-MM-dd"); ;

                                DynamicObject plst = item["PriceListId"] as DynamicObject;
                                entry.F_HS_YNSpecialPrice = Convert.ToBoolean(SQLUtils.GetFieldValue(plst, "F_HS_YNSpecialPrice"));
                                entry.F_HS_YNEntryInSync = entryInSync;

                                if (!entry.F_HS_YNSpecialPrice)
                                {
                                    //如果批量调价单调的特价价目表，则忽略调后美国包邮单价、调后澳洲包邮单价、调后美国不包邮单价、调后澳洲不包邮单价、调后欧洲单价的同步
                                    entry.F_HS_AfterUSPrice = SQLUtils.GetFieldValue(item, "F_HS_AfterUSPrice").Equals("0") ? "" : SQLUtils.GetFieldValue(item, "F_HS_AfterUSPrice");
                                    entry.F_HS_AfterAUPrice = SQLUtils.GetFieldValue(item, "F_HS_AfterAUPrice").Equals("0") ? "" : SQLUtils.GetFieldValue(item, "F_HS_AfterAUPrice");
                                    entry.F_HS_AfterEUPrice = SQLUtils.GetFieldValue(item, "F_HS_AfterEUPrice").Equals("0") ? "" : SQLUtils.GetFieldValue(item, "F_HS_AfterEUPrice");
                                    entry.F_HS_AfterUSNoPostagePrice = SQLUtils.GetFieldValue(item, "F_HS_AfterUSNoPostagePrice").Equals("0") ? "" : SQLUtils.GetFieldValue(item, "F_HS_AfterUSNoPostagePrice");
                                    entry.F_HS_AfterAUNoPostagePrice = SQLUtils.GetFieldValue(item, "F_HS_AfterAUNoPostagePrice").Equals("0") ? "" : SQLUtils.GetFieldValue(item, "F_HS_AfterAUNoPostagePrice");

                                    entry.F_HS_AfterJPNoPostagePrice = SQLUtils.GetFieldValue(item, "F_HS_AfterJPNoPostagePrice").Equals("0") ? "" : SQLUtils.GetFieldValue(item, "F_HS_AfterJPNoPostagePrice");
                                    entry.F_HS_AfterKRNoPostagePrice = SQLUtils.GetFieldValue(item, "F_HS_AfterKRNoPostagePrice").Equals("0") ? "" : SQLUtils.GetFieldValue(item, "F_HS_AfterKRNoPostagePrice");

                                    entry.F_HS_AfterUKNoPostagePrice = SQLUtils.GetFieldValue(item, "F_HS_AfterUKNoPostagePrice").Equals("0") ? "" : SQLUtils.GetFieldValue(item, "F_HS_AfterUKNoPostagePrice");
                                    entry.F_HS_AfterDENoPostagePrice = SQLUtils.GetFieldValue(item, "F_HS_AfterDENoPostagePrice").Equals("0") ? "" : SQLUtils.GetFieldValue(item, "F_HS_AfterDENoPostagePrice");
                                    entry.F_HS_AfterFRNoPostagePrice = SQLUtils.GetFieldValue(item, "F_HS_AfterFRNoPostagePrice").Equals("0") ? "" : SQLUtils.GetFieldValue(item, "F_HS_AfterFRNoPostagePrice");
                                    entry.F_HS_AfterEUNoPostagePrice = SQLUtils.GetFieldValue(item, "F_HS_AfterEUNoPostagePrice").Equals("0") ? "" : SQLUtils.GetFieldValue(item, "F_HS_AfterEUNoPostagePrice");
                                    double diffDate = GetDiffDate(DateTime.Now, Convert.ToDateTime(entry.FAfterUnEffDate));

                                    if (diffDate >= 50)
                                    {
                                        entrys.Add(entry);
                                    }
                                }
                                else
                                {
                                    entrys.Add(entry);
                                }
                            }
                        }
                    }
                }
            }
            
            if (entrys != null && entrys.Count > 0)
            {
                string prefix = SQLUtils.GetAUB2BDropShipOrderPrefix(ctx);

                if (this.Direction == SynchroDirection.ToB2B)
                {
                    return entrys.Where(m => m != null
                                
                                && !string.IsNullOrWhiteSpace(m.F_HS_IsOil)
                                && ((!m.F_HS_PRODUCTSTATUS.Equals("SPTC")
                                && m.F_HS_IsOil.Equals("3"))
                                || (m.FMaterialId.Length == 13
                                && m.FMaterialId.Substring(m.FMaterialId.Length - 3, 3).Equals(prefix)))
                                ).ToList();
                }
                else
                {
                    return entrys.Where(m => m != null
                                  && !m.FMaterialId.Substring(m.FMaterialId.Length - 3, 3).Equals(prefix)
                              ).ToList();
                }
            }
            
            return null;
        }
        /// <summary>
        /// 根据调价单编码查询适用国家
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="billNo"></param>
        /// <returns></returns>
        private string GetApplicableState(Context ctx, string billNo)
        {
            string country = string.Empty;

            if (!string.IsNullOrEmpty(billNo))
            {
                string sql = string.Format(@"/*dialect*/ select d.FNUMBER from T_SAL_PRICELIST a
                                                          inner join T_SAL_BATCHADJUSTENTRY b on b.FPRICELISTID = a.FID
                                                          inner join T_SAL_BATCHADJUST c on c.FID = b.FID
                                                          inner join VW_BAS_ASSISTANTDATA_CountryName d on a.F_HS_APPLICABLESTATE = d.FCountry
                                                          where c.FBILLNO = '{0}'", billNo);
                country = JsonUtils.ConvertObjectToString(SQLUtils.GetObject(ctx, sql, "FNUMBER"));
            }

            return country;
        }
        /// <summary>
        /// 比较时间间隔
        /// </summary>
        /// <param name="dt1"></param>
        /// <param name="dt2"></param>
        /// <returns></returns>
        private Double GetDiffDate(DateTime dt1, DateTime dt2)
        {
            TimeSpan span = dt2.Subtract(dt1);
            double days = span.TotalDays;

            if (days > 0)
            {
                return days / 365;
            }
            return 0;
        }

        /// <summary>
        /// 批量调价单和销售价目表的对照表
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        private Dictionary<string, string> Match(K3BatchAdjustEntry entry)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add(entry.F_HS_AfterUSPrice, "F_HS_USPrice");
            dict.Add(entry.F_HS_AfterUSNoPostagePrice, "F_HS_USNoPostagePrice");
            dict.Add(entry.F_HS_AfterAUPrice, "F_HS_AUPrice");
            dict.Add(entry.F_HS_AfterAUNoPostagePrice, "F_HS_AUNoPostagePrice");
            dict.Add(entry.F_HS_AfterEUPrice, "F_HS_EUPrice");

            return dict;
        }

        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            e.FieldKeys.Add("FDate");
            e.FieldKeys.Add("FAdjustType");
            e.FieldKeys.Add("FAfterPrice");
            e.FieldKeys.Add("F_HS_AfterUSPrice");
            e.FieldKeys.Add("F_HS_AfterAUPrice");
            e.FieldKeys.Add("F_HS_AfterEUPrice");

            e.FieldKeys.Add("FPriceListId");
            e.FieldKeys.Add("FCurrencyId");
            e.FieldKeys.Add("FMaterialId");

            e.FieldKeys.Add("FAfterEffDate");
            e.FieldKeys.Add("FAfterUnEffDate");

            e.FieldKeys.Add("F_HS_YNInSync");
            e.FieldKeys.Add("F_HS_YNEntryInSync");

            e.FieldKeys.Add("F_HS_AfterUSNoPostagePrice");
            e.FieldKeys.Add("F_HS_AfterAUNoPostagePrice");

            e.FieldKeys.Add("F_HS_AfterJPNoPostagePrice");
            e.FieldKeys.Add("F_HS_AfterKRNoPostagePrice");
            e.FieldKeys.Add("F_HS_AfterUKNoPostagePrice");
            e.FieldKeys.Add("F_HS_AfterDENoPostagePrice");
            e.FieldKeys.Add("F_HS_AfterFRNoPostagePrice");
            e.FieldKeys.Add("F_HS_PRODUCTSTATUS");
            e.FieldKeys.Add("F_HS_IsOil");
            e.FieldKeys.Add("FMaterialId");
            e.FieldKeys.Add("F_HS_AfterEUNoPostagePrice");
           
        }

        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);

            if (e.DataEntitys == null)
            {
                return;
            }
           
            List<DynamicObject> objs = e.DataEntitys.ToList();
            HttpResponseResult result = OperateAfterAudit(this.Context, objs);

            if(result != null &&　!result.Success && !string.IsNullOrWhiteSpace(result.Message))
            {
                throw new Exception(result.Message);
            }
        }
    }
}
