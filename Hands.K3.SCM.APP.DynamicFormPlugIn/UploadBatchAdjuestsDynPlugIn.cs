using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Hands.K3.SCM.App.Synchro.Utils.SynchroService;
using Hands.K3.SCM.APP.Entity.EnumType;
using Hands.K3.SCM.APP.Entity.K3WebApi;
using Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject;
using Hands.K3.SCM.APP.Entity.SynDataObject.BacthAdjust;
using Hands.K3.SCM.APP.Entity.SynDataObject.Material_;
using Hands.K3.SCM.APP.Utils.Utils;
using HS.K3.Common.Abbott;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;

namespace Hands.K3.SCM.APP.DynamicFormPlugIn
{
    [Kingdee.BOS.Util.HotUpdate]
    [Description("生成批量调价单")]
    public class UploadBatchAdjuestsDynPlugIn : AbstractDynPlugIn
    {
        string msg = string.Empty;
        public override SynchroDataType DataType
        {
            get
            {
                return SynchroDataType.BatchAdjust;
            }
        }

        /// <summary>
        /// 获取客户第一次输入信息
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public Dictionary<string, Dictionary<string, string>> GetFirstInput(Context ctx, DynamicObjectCollection coll)
        {
            Dictionary<string, Dictionary<string, string>> all = new Dictionary<string, Dictionary<string, string>>();
            Dictionary<string, string> dict = null;

            if (coll != null && coll.Count > 0)
            {
                foreach (var item in coll)
                {
                    if (item != null)
                    {
                        DynamicObject oBillNo = item["F_HS_BillNo"] as DynamicObject;
                        string billNo = SQLUtils.GetFieldValue(oBillNo, "Number");

                        string levelPrice = SQLUtils.GetFieldValue(item, "F_HS_LevelPrice");
                        Regex re = new Regex(@"^[0-9]*$-^[0-9]*$-^[0-9]*$-^[0-9]*$-^[0-9]*$-^[0-9]*$-^[0-9]*$-^[0-9]*$-^[0-9]*$-^[0-9]*$-^[0-9]*$-^[0-9]*$-^[0-9]*$");
                        string regex = string.Format(@"\d-\d-\d-\d-\d-\d-\d-\d-\d-\d-\d-\d-\d");

                        if (!string.IsNullOrWhiteSpace(levelPrice))
                        {
                            Match match = Regex.Match(levelPrice, regex);

                            if (levelPrice.Contains("-"))
                            {
                                if (levelPrice.Split('-') != null && levelPrice.Split('-').Count() == 14)
                                {
                                    dict = GetItems<Dictionary<string,string>>(levelPrice, '-');
                                    all.Add(billNo, dict);
                                    LogHelper.WriteSynchroLog_Succ(ctx, this.DataType, string.Format("客户输入：物料编码【{0}】,价格系列【{1}】", billNo, levelPrice));
                                }
                                else
                                {
                                    string msg = string.Format("物料编码【{0}】对应的价格【{1}】系列输入格式错误", billNo, levelPrice);
                                    this.View.ShowErrMessage(msg, "错误提示", MessageBoxType.Error);
                                    LogUtils.WriteSynchroLog(ctx, this.DataType, msg);
                                }
                            }
                            //if (!match.Success)
                            //{
                            //    string msg = string.Format("物料编码【{0}】对应的价格【{1}】系列输入格式错误", billNo, levelPrice);
                            //    this.View.ShowErrMessage(msg, "错误提示", MessageBoxType.Error);
                            //    LogUtils.WriteSynchroLog(ctx, this.DataType, msg);
                            //}
                            //else
                            //{
                            //    Dictionary<string, string> dict = JsonUtils.GetItems(levelPrice, '-');
                            //    all.Add(billNo, dict);
                            //    LogHelper.WriteSynchroLog_Succ(ctx, this.DataType, string.Format("客户输入：物料编码【{0}】,价格系列【{1}】", billNo, levelPrice));
                            //}
                        }
                        else
                        {
                            string msg = string.Format("物料编码【{0}】对应的价格【{1}】系列输入格式错误", billNo, levelPrice);
                            this.View.ShowErrMessage(msg, "错误提示", MessageBoxType.Error);
                            LogUtils.WriteSynchroLog(ctx, this.DataType, msg);
                        }

                        
                    }
                }
            }
            return all;
        }

        /// <summary>
        /// 获取用户输入
        /// </summary>
        /// <param name="identify"></param>
        /// <returns></returns>
        private DynamicObjectCollection GetInput(string identify)
        {
            if (!string.IsNullOrWhiteSpace(identify))
            {
                return this.View.Model.DataObject[identify] as DynamicObjectCollection;
            }

            return null;
        }

        /// <summary>
        /// 根据用户第一次输入生成价格系列
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public Dictionary<KeyValuePair<string, string>, Dictionary<string, string>> GeneratePriceSeries(Context ctx, DynamicObjectCollection coll)
        {

            HashSet<string> selects = new HashSet<string>();
            Dictionary<string, Dictionary<string, string>> all = new Dictionary<string, Dictionary<string, string>>();
            List<K3BatchAdjustEntry> entries = new List<K3BatchAdjustEntry>();
            Dictionary<KeyValuePair<string, string>, string> dict = null;

            bool select1 = Convert.ToBoolean(this.View.Model.GetValue("F_HS_YNLevel0")) ? selects.Add("0") : false;
            bool select2 = Convert.ToBoolean(this.View.Model.GetValue("F_HS_YNLevel1")) ? selects.Add("1") : false;
            bool select3 = Convert.ToBoolean(this.View.Model.GetValue("F_HS_YNLevel2")) ? selects.Add("2") : false;
            bool select4 = Convert.ToBoolean(this.View.Model.GetValue("F_HS_YNLevel3")) ? selects.Add("3") : false;
            bool select5 = Convert.ToBoolean(this.View.Model.GetValue("F_HS_YNLevel4")) ? selects.Add("4") : false;
            bool select6 = Convert.ToBoolean(this.View.Model.GetValue("F_HS_YNLevel5")) ? selects.Add("5") : false;
            bool select7 = Convert.ToBoolean(this.View.Model.GetValue("F_HS_YNLevel6")) ? selects.Add("6") : false;
            bool select8 = Convert.ToBoolean(this.View.Model.GetValue("F_HS_YNLevel7")) ? selects.Add("7") : false;
            bool select9 = Convert.ToBoolean(this.View.Model.GetValue("F_HS_YNLevel8")) ? selects.Add("8") : false;

            if (select1 && select2 && select3 && select4 && select5 && select6 && select7 && select8 && select9)
            {
                selects.Add("0");
                selects.Add("1");
                selects.Add("2");
                selects.Add("3");
                selects.Add("4");
                selects.Add("5");
                selects.Add("6");
                selects.Add("7");
                selects.Add("8");
                selects.Add("9");
            }



            if (coll != null && coll.Count > 0)
            {
                dict = new Dictionary<KeyValuePair<string, string>, string>();
                foreach (var item in coll)
                {
                    if (item != null)
                    {

                        DynamicObject oBillNo = item["F_HS_BillNo"] as DynamicObject;
                        string materialNo = SQLUtils.GetFieldValue(oBillNo, "Number");

                        string posUsSeries = SQLUtils.GetFieldValue(item, "F_HS_USPriceSeries");
                        Add(dict, materialNo, "F_HS_USPriceSeries", posUsSeries);

                        string upPosUsSeries = SQLUtils.GetFieldValue(item, "F_HS_USNoPostagePriceSeries");
                        Add(dict, materialNo, "F_HS_USNoPostagePriceSeries", upPosUsSeries);

                        string posAuSeries = SQLUtils.GetFieldValue(item, "F_HS_AUPriceSeries");
                        Add(dict, materialNo, "F_HS_AUPriceSeries", posAuSeries);

                        string unposAuSeries = SQLUtils.GetFieldValue(item, "F_HS_AUNoPostagePriceSeries");
                        Add(dict, materialNo, "F_HS_AUNoPostagePriceSeries", unposAuSeries);

                        string unposJPSeries = SQLUtils.GetFieldValue(item, "F_HS_JPNoPostagePriceSeries");
                        Add(dict, materialNo, "F_HS_JPNoPostagePriceSeries", unposJPSeries);

                        string unposKRSeries = SQLUtils.GetFieldValue(item, "F_HS_KRNoPostagePriceSeries");
                        Add(dict, materialNo, "F_HS_KRNoPostagePriceSeries", unposKRSeries);

                        string unposUKSeries = SQLUtils.GetFieldValue(item, "F_HS_UKNoPostagePriceSeries");
                        Add(dict, materialNo, "F_HS_UKNoPostagePriceSeries", unposUKSeries);

                        string unposDESeries = SQLUtils.GetFieldValue(item, "F_HS_DENoPostagePriceSeries");
                        Add(dict, materialNo, "F_HS_DENoPostagePriceSeries", unposDESeries);

                        string unposFRSeries = SQLUtils.GetFieldValue(item, "F_HS_FRNoPostagePriceSeries");
                        Add(dict, materialNo, "F_HS_FRNoPostagePriceSeries", unposFRSeries);

                        string unposEUSeries = SQLUtils.GetFieldValue(item, "F_HS_EUNoPostagePriceSeries");
                        Add(dict, materialNo, "F_HS_EUNoPostagePriceSeries", unposEUSeries);

                    }
                }
            }
            return GetItems<Dictionary<KeyValuePair<string, string>, Dictionary<string, string>>>(dict, '-', selects);
        }

        private Dictionary<string, string> FieldsKeyValues()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("F_HS_AfterUSPrice", "F_HS_USPriceSeries");
            dict.Add("F_HS_AfterUSNoPostagePrice", "F_HS_USNoPostagePriceSeries");
            dict.Add("F_HS_AfterAUPrice", "F_HS_AUPriceSeries");
            dict.Add("F_HS_AfterAUNoPostagePrice", "F_HS_AUNoPostagePriceSeries");
            dict.Add("F_HS_AfterJPNoPostagePrice", "F_HS_JPNoPostagePriceSeries");
            dict.Add("F_HS_AfterKRNoPostagePrice", "F_HS_KRNoPostagePriceSeries");
            dict.Add("F_HS_AfterUKNoPostagePrice", "F_HS_UKNoPostagePriceSeries");
            dict.Add("F_HS_AfterDENoPostagePrice", "F_HS_DENoPostagePriceSeries");
            dict.Add("F_HS_AfterFRNoPostagePrice", "F_HS_FRNoPostagePriceSeries");
            dict.Add("F_HS_AfterEUNoPostagePrice", "F_HS_EUNoPostagePriceSeries");

            return dict;
        }
        private void Add(Dictionary<KeyValuePair<string, string>, string> dict, string materilNo, string idVal, string txt)
        {
            Dictionary<string, string> ids = FieldsKeyValues();
            KeyValuePair<string, string> kv = default(KeyValuePair<string, string>);


            if (dict != null && !string.IsNullOrEmpty(txt) && !string.IsNullOrEmpty(materilNo) && !string.IsNullOrEmpty(idVal))
            {
                if (ids != null && ids.Count > 0)
                {
                    foreach (var id in ids)
                    {
                        if (!string.IsNullOrEmpty(id.Value) && id.Value.Equals(idVal))
                        {
                            kv = new KeyValuePair<string, string>(materilNo, id.Key);
                            dict.Add(kv, txt);

                        }
                    }
                }
            }
        }
        /// <summary>
        /// 根据用户输入分装要生成的批量调价单数据
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="entries"></param>
        /// <returns></returns>
        private List<K3BatchAdjust> GetK3BatchAdjusts(Context ctx, List<K3BatchAdjustEntry> entries)
        {
            List<K3BatchAdjust> justs = null;
            K3BatchAdjust just = null;

            if (entries != null && entries.Count > 0)
            {
                var result = entries.Where(e => !string.IsNullOrWhiteSpace(e.FMaterialId) && !string.IsNullOrWhiteSpace(e.FPriceListId))
                                    .GroupBy(e => e.FPriceListId);

                if (result != null && result.Count() > 0)
                {
                    justs = new List<K3BatchAdjust>();

                    foreach (var items in result)
                    {
                        if (items != null && items.Count() > 0)
                        {
                            just = new K3BatchAdjust();
                            just.FName = string.Format(@"{0}价目表调价", GetPriceLevel(items.FirstOrDefault().FPriceListId));
                            just.Entry = new List<K3BatchAdjustEntry>();
                            
                            var group = items.Where(i => !string.IsNullOrWhiteSpace(i.FMaterialId)) 
                                             .GroupBy(i => i.FMaterialId);

                            if (group != null && group.Count() > 0)
                            {
                                foreach (var item in group)
                                {
                                    if (item != null)
                                    {
                                        foreach (var en in item)
                                        {
                                            if (en != null)
                                            {
                                                Material material = GetMaterial(ctx, en.FMaterialId);
                                                en.FMatUnitId = material.FSaleUnitId;
                                                en.FAfterEffDate = material.FCreateDate.ToString();

                                                if (IsExist(ctx, en))
                                                {
                                                    SetBatchAdjust(ctx, en);
                                                }
                                                else
                                                {
                                                    en.FAdjustType = "A";
                                                    en.FBeforeEffDate = "2018-04-01";
                                                    en.FBeforeUnEffDate = "2100-01-01";
                                                    en.FAfterEffDate = "2018-04-01";
                                                    en.FAfterUnEffDate = "2100-01-01";
                                                }
                                            }

                                            just.Entry.Add(en);
                                        }   
                                    }
                                }
                            }
                        }

                        justs.Add(just);
                    }
                }
            }

            return justs;
        }

        /// <summary>
        /// 根据用户输入分装要生成的批量调价单数据
        /// </summary>
        /// <param name="materialId"></param>
        /// <param name="dict"></param>
        /// <param name="identify"></param>
        /// <param name="choices"></param>
        /// <returns></returns>
        public List<K3BatchAdjustEntry> GetK3BatchAdjustEntry(string materialId, Dictionary<string, string> dict, string identify, HashSet<string> choices)
        {
            List<K3BatchAdjustEntry> entries = new List<K3BatchAdjustEntry>();
            K3BatchAdjustEntry entry = null;

            if (!string.IsNullOrWhiteSpace(materialId) && !string.IsNullOrWhiteSpace(identify))
            {
                if (dict != null && dict.Count > 0)
                {
                    var result = dict.Where(e => !string.IsNullOrWhiteSpace(e.Key) && choices.Contains(e.Key));

                    if (result != null && result.Count() > 0)
                    {
                        foreach (var item in result)
                        {
                            if (!string.IsNullOrWhiteSpace(item.Key) && !string.IsNullOrWhiteSpace(item.Value))
                            {
                                entry = new K3BatchAdjustEntry();
                                entry.FMaterialId = materialId;
                                entry.FPriceListId = GetPriceListNumber(item.Key);
                                entry.FAfterPrice = item.Value;
                                string propertyName = GetPropertyName(typeof(K3BatchAdjustEntry), identify);

                                if (identify.Equals(propertyName))
                                {
                                    GetPropertyInfo(entry.GetType(),identify).SetValue(entry, item.Value, null);
                                }

                                entries.Add(entry);
                            }
                        }
                    }
                }
                else
                {
                    this.View.ShowErrMessage("单价系列不能为空", "错误提示", MessageBoxType.Error);
                }
            }

            return entries;
        }

        private PropertyInfo GetPropertyInfo(Type type, string property)
        {
            if (type != null && !string.IsNullOrWhiteSpace(property))
            {
                return type.GetProperty(property);
            }

            return null;
        }
        /// <summary>
        /// 获取类/对象的属性名称
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        private string GetPropertyName(Type type, string property)
        {
            PropertyInfo propertyInfo = GetPropertyInfo(type, property);

            if (propertyInfo != null)
            {
                return propertyInfo.Name;
            }
            return string.Empty;
        }
        private string GetPropertyValue<T>(T obj, string property)
        {
            PropertyInfo propertyInfo = GetPropertyInfo(obj.GetType(), property);

            if (propertyInfo != null)
            {
                object pro = propertyInfo.GetValue(obj, null);

                if (pro != null)
                {
                    return pro.ToString();
                }
            }
            return string.Empty;
        }
        /// <summary>
        /// 选择生成指定的level批量调价单
        /// </summary>
        /// <param name="choices"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        private bool IsChoice(HashSet<string> choices, string level)
        {
            if (choices != null && choices.Count > 0)
            {
                var choice = from c in choices
                             where c != null && c.CompareTo(level) == 0
                             select c;

                if (choice != null && choice.Count() > 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 根据标准价目表获取调后美国，澳洲，欧洲单价
        /// </summary>
        /// <param name="priceList"></param>
        /// <returns></returns>
        public List<K3BatchAdjustEntry> GetAfterPrice(Context ctx, Dictionary<string, Dictionary<string, string>> input)
        {
            Dictionary<string, Dictionary<string, string>> calcDict = null;
            Dictionary<string, string> dict = null;
            List<K3BatchAdjustEntry> entries = null;
            K3BatchAdjustEntry entry = null;

            if (input != null && input.Count > 0)
            {
                calcDict = new Dictionary<string, Dictionary<string, string>>();
                entries = new List<K3BatchAdjustEntry>();

                foreach (var items in input)
                {
                    dict = new Dictionary<string, string>();
                    string property = GetMaterialProperty(ctx, items.Key);

                    foreach (var item in items.Value)
                    {
                        if (!string.IsNullOrWhiteSpace(item.Key))
                        {
                            string priceListNo = GetPriceListNumber(item.Key);

                            if (!string.IsNullOrWhiteSpace(priceListNo))
                            {
                                decimal normalPrice = Convert.ToDecimal(item.Value);
                                decimal pos_mult = 0;
                                decimal unpos_mult = 0;

                                string posPrice = string.Empty;
                                string unposPrice = string.Empty;

                                if (!string.IsNullOrWhiteSpace(property) && property.CompareTo("3") == 0)
                                {

                                    switch (priceListNo)
                                    {
                                        case "XSJMB0001":

                                            unpos_mult = 1M;
                                            posPrice = (Ceiling(normalPrice * 100M) / 100M).ToString();
                                            unposPrice = (Ceiling(normalPrice * unpos_mult * 100M) / 100M).ToString();

                                            if (entry == null)
                                            {
                                                entry = new K3BatchAdjustEntry();
                                                entry.FMaterialId = items.Key;
                                                entry.FAfterPrice = normalPrice.ToString();
                                                entry.FPriceListId = priceListNo;
                                            }
                                            
                                            if (item.Key.CompareTo("USL0") != 0 && item.Key.CompareTo("AUL0") != 0 
                                                && item.Key.CompareTo("JPL0") != 0 && item.Key.CompareTo("KRL0") != 0 && item.Key.CompareTo("EUL0") != 0)
                                            {
                                                entry.F_HS_AfterUSPrice = posPrice;
                                                entry.F_HS_AfterAUPrice = posPrice;
                                                entry.F_HS_AfterUKNoPostagePrice = unposPrice;
                                                entry.F_HS_AfterFRNoPostagePrice = unposPrice;
                                                entry.F_HS_AfterDENoPostagePrice = unposPrice;
                                                
                                            }
                                            else
                                            {
                                                if (item.Key.CompareTo("USL0") == 0)
                                                {
                                                    entry.F_HS_AfterUSNoPostagePrice = string.IsNullOrWhiteSpace(item.Value) ? "" : (Ceiling(Convert.ToDecimal(item.Value) * 100M) / 100M).ToString();
                                                }

                                                if (item.Key.CompareTo("AUL0") == 0)
                                                {
                                                    entry.F_HS_AfterAUNoPostagePrice = string.IsNullOrWhiteSpace(item.Value) ? "" : (Ceiling(Convert.ToDecimal(item.Value) * 100M) / 100M).ToString();
                                                }

                                                if (item.Key.CompareTo("JPL0") == 0)
                                                {
                                                    entry.F_HS_AfterJPNoPostagePrice = string.IsNullOrWhiteSpace(item.Value) ? "" : (Ceiling(Convert.ToDecimal(item.Value) * 100M) / 100M).ToString();
                                                }
                                                if (item.Key.CompareTo("KRL0") == 0)
                                                {
                                                    entry.F_HS_AfterKRNoPostagePrice = string.IsNullOrWhiteSpace(item.Value) ? "" : (Ceiling(Convert.ToDecimal(item.Value) * 100M) / 100M).ToString();
                                                }
                                                if (item.Key.CompareTo("EUL0") == 0)
                                                {
                                                    entry.F_HS_AfterEUNoPostagePrice = string.IsNullOrWhiteSpace(item.Value) ? "" : (Ceiling(Convert.ToDecimal(item.Value) * 100M) / 100M).ToString();
                                                }
                                            }
                                            Add(entries, ref entry);
                                            break;
                                        case "XSJMB0002":
                                        case "XSJMB0003":
                                        case "XSJMB0004":
                                        case "XSJMB0005":
                                        case "XSJMB0006":
                                           
                                            pos_mult = 1.085M;
                                            unpos_mult = 1M;

                                            posPrice = (Ceiling(normalPrice * pos_mult * 100M) / 100M).ToString();
                                            unposPrice = (Ceiling(normalPrice * unpos_mult * 100M) / 100M).ToString();

                                            if (entry == null)
                                            {
                                                entry = new K3BatchAdjustEntry();
                                                entry.FMaterialId = items.Key;
                                                entry.FPriceListId = priceListNo;
                                                entry.FAfterPrice = normalPrice.ToString();
                                            }

                                            entry.F_HS_AfterUSPrice = posPrice;
                                            entry.F_HS_AfterUSNoPostagePrice = unposPrice;
                                            entry.F_HS_AfterAUPrice = posPrice;
                                            entry.F_HS_AfterAUNoPostagePrice = unposPrice;
                                            entry.F_HS_AfterJPNoPostagePrice = unposPrice;
                                            entry.F_HS_AfterKRNoPostagePrice = unposPrice;
                                            entry.F_HS_AfterUKNoPostagePrice = unposPrice;
                                            entry.F_HS_AfterFRNoPostagePrice = unposPrice;
                                            entry.F_HS_AfterDENoPostagePrice = unposPrice;
                                            entry.F_HS_AfterEUNoPostagePrice = unposPrice;
                                            Add(entries, ref entry);
                                            break;
                                        case "XSJMB0007":
                                        case "XSJMB0008":
                                            
                                            if (true)
                                            {
                                                pos_mult = 1.08M;
                                                unpos_mult = 1.01M;

                                                posPrice = (Ceiling(normalPrice * pos_mult * 100M) / 100M).ToString();
                                                unposPrice = (Ceiling(normalPrice * unpos_mult * 100M) / 100M).ToString();
                                            }
                                            if (entry == null)
                                            {
                                                entry = new K3BatchAdjustEntry();
                                                entry.FMaterialId = items.Key;
                                                entry.FPriceListId = priceListNo;
                                                entry.FAfterPrice = normalPrice.ToString();
                                            }
                                            //if (normalPrice > 20 && normalPrice <= 35)
                                            //{
                                            //    pos_mult = 1.078M;
                                            //    unpos_mult = 1.008M;
                                            //}
                                            //if (normalPrice > 35)
                                            //{
                                            //    pos_mult = 1.075M;
                                            //    unpos_mult = 1.005M;
                                            //}


                                            entry.F_HS_AfterUSPrice = posPrice;
                                            entry.F_HS_AfterUSNoPostagePrice = unposPrice;
                                            entry.F_HS_AfterAUPrice = posPrice;
                                            entry.F_HS_AfterAUNoPostagePrice = unposPrice;
                                            entry.F_HS_AfterJPNoPostagePrice = (Ceiling(normalPrice * 1 * 100M) / 100M).ToString();
                                            entry.F_HS_AfterKRNoPostagePrice = (Ceiling(normalPrice * 1 * 100M) / 100M).ToString();
                                            entry.F_HS_AfterUKNoPostagePrice = (Ceiling(normalPrice * 1 * 100M) / 100M).ToString();
                                            entry.F_HS_AfterFRNoPostagePrice = (Ceiling(normalPrice * 1 * 100M) / 100M).ToString();
                                            entry.F_HS_AfterDENoPostagePrice = (Ceiling(normalPrice * 1 * 100M) / 100M).ToString();
                                            entry.F_HS_AfterDENoPostagePrice = (Ceiling(normalPrice * 1 * 100M) / 100M).ToString();
                                            entry.F_HS_AfterEUNoPostagePrice = (Ceiling(normalPrice * 1 * 100M) / 100M).ToString();
                                            Add(entries, ref entry);
                                            break;
                                        case "XSJMB0009":
                                            
                                            pos_mult = 1.08M;
                                            unpos_mult = 1M;

                                            posPrice = (Ceiling(normalPrice * pos_mult * 100M) / 100M).ToString();
                                            unposPrice = (Ceiling(normalPrice * unpos_mult * 100M) / 100M).ToString();

                                            if (entry == null)
                                            {
                                                entry = new K3BatchAdjustEntry();
                                                entry.FMaterialId = items.Key;
                                                entry.FPriceListId = priceListNo;
                                                entry.FAfterPrice = normalPrice.ToString();
                                            }

                                            entry.F_HS_AfterUSPrice = posPrice;
                                            entry.F_HS_AfterUSNoPostagePrice = unposPrice;
                                            entry.F_HS_AfterAUPrice = posPrice;
                                            entry.F_HS_AfterAUNoPostagePrice = unposPrice;
                                            entry.F_HS_AfterJPNoPostagePrice = unposPrice;
                                            entry.F_HS_AfterKRNoPostagePrice = unposPrice;
                                            entry.F_HS_AfterUKNoPostagePrice = unposPrice;
                                            entry.F_HS_AfterFRNoPostagePrice = unposPrice;
                                           
                                            entry.F_HS_AfterDENoPostagePrice = unposPrice;
                                            entry.F_HS_AfterEUNoPostagePrice = unposPrice;
                                            Add(entries, ref entry);
                                            break;
                                    }
                                }
                                else
                                {
                                    posPrice = (Ceiling(normalPrice * 100M) / 100M).ToString();

                                   
                                    if (!string.IsNullOrWhiteSpace(priceListNo) && priceListNo.CompareTo("XSJMB0001") != 0)
                                    {
                                        if (entry == null)
                                        {
                                            entry = new K3BatchAdjustEntry();
                                            entry.FMaterialId = items.Key;
                                            entry.FAfterPrice = normalPrice.ToString();
                                            entry.FPriceListId = priceListNo;
                                        }

                                        entry.F_HS_AfterUSPrice = posPrice;
                                        entry.F_HS_AfterUSNoPostagePrice = posPrice;
                                        entry.F_HS_AfterAUPrice = posPrice;
                                        entry.F_HS_AfterAUNoPostagePrice = posPrice;
                                        entry.F_HS_AfterJPNoPostagePrice = posPrice;
                                        entry.F_HS_AfterKRNoPostagePrice = posPrice;
                                        entry.F_HS_AfterUKNoPostagePrice = posPrice;
                                        entry.F_HS_AfterFRNoPostagePrice = posPrice;
                                        entry.F_HS_AfterDENoPostagePrice = posPrice;
                                        entry.F_HS_AfterEUNoPostagePrice = posPrice;

                                        Add(entries, ref entry);
                                    }
                                    else
                                    {
                                        if (entry == null)
                                        {
                                            entry = new K3BatchAdjustEntry();
                                            entry.FMaterialId = items.Key;
                                            entry.FAfterPrice = normalPrice.ToString();
                                            entry.FPriceListId = priceListNo;
                                        }

                                        entry.F_HS_AfterUSPrice = posPrice;
                                        entry.F_HS_AfterAUPrice = posPrice;
                                        entry.F_HS_AfterUKNoPostagePrice = posPrice;
                                        entry.F_HS_AfterFRNoPostagePrice = posPrice;
                                        entry.F_HS_AfterDENoPostagePrice = posPrice;
                                        

                                        switch (item.Key)
                                        {
                                            case "USL0":
                                                entry.F_HS_AfterUSNoPostagePrice = string.IsNullOrWhiteSpace(item.Value) ? "" : (Ceiling(Convert.ToDecimal(item.Value) * 100M) / 100M).ToString(); 
                                                break;
                                            case "AUL0":
                                                entry.F_HS_AfterAUNoPostagePrice = string.IsNullOrWhiteSpace(item.Value) ? "" : (Ceiling(Convert.ToDecimal(item.Value) * 100M) / 100M).ToString(); 
                                                break;
                                            case "JPL0":
                                                entry.F_HS_AfterJPNoPostagePrice = string.IsNullOrWhiteSpace(item.Value) ? "" : (Ceiling(Convert.ToDecimal(item.Value) * 100M) / 100M).ToString(); 
                                                break;
                                            case "KRL0":
                                                entry.F_HS_AfterKRNoPostagePrice = string.IsNullOrWhiteSpace(item.Value) ? "" : (Ceiling(Convert.ToDecimal(item.Value) * 100M) / 100M).ToString(); 
                                                break;
                                            case "EUL0":
                                                entry.F_HS_AfterEUNoPostagePrice = string.IsNullOrWhiteSpace(item.Value) ? "" : (Ceiling(Convert.ToDecimal(item.Value) * 100M) / 100M).ToString();
                                                break;
                                            default:
                                                break;
                                        }

                                        Add(entries, ref entry);
                                    }
                                }
                            }
                        }
                    }

                }

            }
            return entries;
        }

        private void Add(List<K3BatchAdjustEntry> entries, ref K3BatchAdjustEntry entry)
        {
            if (entries != null && entry != null)
            {
                if (!string.IsNullOrWhiteSpace(entry.F_HS_AfterUSPrice) && !string.IsNullOrWhiteSpace(entry.F_HS_AfterUSNoPostagePrice)
                    && !string.IsNullOrWhiteSpace(entry.F_HS_AfterAUPrice) && !string.IsNullOrWhiteSpace(entry.F_HS_AfterAUNoPostagePrice)
                    && !string.IsNullOrWhiteSpace(entry.F_HS_AfterJPNoPostagePrice) && !string.IsNullOrWhiteSpace(entry.F_HS_AfterKRNoPostagePrice)
                     && !string.IsNullOrWhiteSpace(entry.F_HS_AfterUKNoPostagePrice) && !string.IsNullOrWhiteSpace(entry.F_HS_AfterDENoPostagePrice)
                    && !string.IsNullOrWhiteSpace(entry.F_HS_AfterFRNoPostagePrice) && !string.IsNullOrWhiteSpace(entry.F_HS_AfterEUNoPostagePrice))
                {
                    entries.Add(entry);
                    entry = null;
                }
            }
        }
        /// <summary>
        /// 获取物料关联的信息
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="materialNo"></param>
        /// <returns></returns>
        private Material GetMaterial(Context ctx, string materialNo)
        {
            Material material = null;

            if (!string.IsNullOrWhiteSpace(materialNo))
            {
                string sql = string.Format(@"/*dialect*/ select a.FCREATEDATE,c.FNAME as FSaleUnitId from T_BD_MATERIAL a 
                                                        inner join t_BD_MaterialSale b on b.FMATERIALID = a.FMATERIALID
                                                        inner join T_BD_unit_L c on c.FUNITID = b.FSaleUnitId and c.FLOCALEID=2052
                                                        where a.FNUMBER = '{0}' and FUSEORGID = 100035", materialNo);

                DynamicObjectCollection coll = SQLUtils.GetObjects(ctx, sql);

                if (coll != null && coll.Count > 0)
                {
                    material = new Material();
                    foreach (var item in coll)
                    {
                        material.FCreateDate = Convert.ToDateTime(SQLUtils.GetFieldValue(item, "FCREATEDATE"));
                        material.FSaleUnitId = SQLUtils.GetFieldValue(item, "FSaleUnitId");
                    }
                }
            }

            return material;
        }

        /// <summary>
        /// 根据物料编码获取物料的液体属性
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="materialNo"></param>
        /// <returns></returns>
        private string GetMaterialProperty(Context ctx, string materialNo)
        {
            if (!string.IsNullOrWhiteSpace(materialNo))
            {
                string sql = string.Format(@"select F_HS_IsOil from T_BD_MATERIAL where FNUMBER = '{0}' and FUSEORGID = 1", materialNo);
                return JsonUtils.ConvertObjectToString(SQLUtils.GetObject(ctx, sql, "F_HS_IsOil"));
            }
            return string.Empty;
        }

        /// <summary>
        /// 获取价目表编码
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        private string GetPriceListNumber(string level)
        {
            if (!string.IsNullOrWhiteSpace(level))
            {
                switch (level)
                {
                    case "0":
                    case "USL0":
                    case "AUL0":
                    case "JPL0":
                    case "KRL0":
                    case "EUL0":
                        return "XSJMB0001";
                    case "1":
                        return "XSJMB0002";
                    case "2":
                        return "XSJMB0003";
                    case "3":
                        return "XSJMB0004";
                    case "4":
                        return "XSJMB0005";
                    case "5":
                        return "XSJMB0006";
                    case "6":
                        return "XSJMB0007";
                    case "7":
                        return "XSJMB0008";
                    case "8":
                        return "XSJMB0009";
                    default:
                        return string.Empty;
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// 获取价格等级
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        private string GetPriceLevel(string priceListNumber)
        {
            if (!string.IsNullOrWhiteSpace(priceListNumber))
            {
                switch (priceListNumber)
                {
                    case "XSJMB0001":
                        return "level0";
                    case "XSJMB0002":
                        return "level1";
                    case "XSJMB0003":
                        return "level2";
                    case "XSJMB0004":
                        return "level3";
                    case "XSJMB0005":
                        return "level4";
                    case "XSJMB0006":
                        return "level5";
                    case "XSJMB0007":
                        return "level6";
                    case "XSJMB0008":
                        return "level7";
                    case "XSJMB0009":
                        return "level8";
                    default:
                        return string.Empty;
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// 判断物料在价目表是否有关联
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="entry"></param>
        /// <returns></returns>
        private bool IsExist(Context ctx, K3BatchAdjustEntry entry)
        {
            if (entry != null)
            {
                string sql = string.Format(@"/*dialect*/select e.FNUMBER from T_SAL_PRICELISTENTRY a
			                                            inner join T_SAL_PRICELIST b on a.FID = b.FID	                                    
			                                            inner join T_BD_MATERIAL e on a.FMATERIALID = e.FMATERIALID
			                                            where e.FNUMBER = '{0}'
			                                            and b.FNUMBER = '{1}'", entry.FMaterialId, entry.FPriceListId);
                DynamicObjectCollection coll = SQLUtils.GetObjects(ctx, sql);
                string no = JsonUtils.ConvertObjectToString(SQLUtils.GetObject(ctx, sql, "FNUMBER"));

                if (!string.IsNullOrWhiteSpace(no))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 返回操作结果
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="formView"></param>
        /// <param name="result"></param>
        /// <param name="count"></param>
        private void BindDataToView(Context ctx, IDynamicFormView formView, HttpResponseResult result, int count)
        {
            if (result != null)
            {
                if (!string.IsNullOrWhiteSpace(result.Message))
                {
                    for (int i = 0; i < count; i++)
                    {
                        formView.Model.SetValue("F_HS_Result", result.Message, i);
                        formView.UpdateView("F_HS_Entity");
                    }
                }
            }
        }
        private void BindDataToView(Context ctx, IDynamicFormView formView, List<K3BatchAdjustEntry> entries)
        {
            string priceSeries = string.Empty;
            Dictionary<string, string> dict = FieldsKeyValues();
          
            if (formView != null && entries != null && entries.Count > 0)
            {
                var result = entries.GroupBy(d => d.FMaterialId);
                foreach (var item in dict)
                {
                    if (!string.IsNullOrWhiteSpace(item.Key))
                    {
                        if (result != null && result.Count() > 0)
                        {
                            for (int i = 0; i < result.Count(); i++)
                            {
                                for (int j = 0; j < result.ElementAt(i).Count(); j++)
                                {
                                    string propertyName = GetPropertyName(typeof(K3BatchAdjustEntry), item.Key);

                                    if (!string.IsNullOrEmpty(propertyName) && propertyName.Equals(item.Key))
                                    {
                                        string propertyValue = GetPropertyValue(result.ElementAt(i).ElementAt(j), item.Key);
                                       
                                        if (j < result.ElementAt(i).Count() - 1)
                                        {
                                            priceSeries += string.Format("{0}{1}", propertyValue, "-");
                                        }
                                        else
                                        {
                                            priceSeries += string.Format("{0}", propertyValue);
                                        }
                                        
                                    }
                                }
                                if (!string.IsNullOrWhiteSpace(priceSeries) && priceSeries.Contains("-"))
                                {

                                    if (priceSeries.Split('-').Count() == 9)
                                    {
                                        formView.Model.SetValue(item.Value, priceSeries, i);
                                        priceSeries = string.Empty;
                                    }
                                    else
                                    {
                                        this.View.ShowErrMessage(string.Format("物料编码【{0}】生成的价格系列错误【{1}】", result.FirstOrDefault().Key, priceSeries), "错误提示", MessageBoxType.Error);
                                    }
                                }
                                else
                                {
                                    this.View.ShowErrMessage(string.Format("物料编码【{0}】生成的价格系列错误【{1}】", result.FirstOrDefault().Key, priceSeries), "错误提示", MessageBoxType.Error);
                                }
                            }

                            formView.UpdateView("F_HS_Entity");
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 更新批量调价单
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="entry"></param>
        /// <returns></returns>
        private void SetBatchAdjust(Context ctx, K3BatchAdjustEntry entry)
        {
            if (entry != null)
            {
                string sql = string.Format(@"/*dialect*/select a.FPrice,a.F_HS_USPrice,F_HS_USNoPostagePrice,a.F_HS_AUPrice,F_HS_AUNoPostagePrice
                                                        ,F_HS_KRNoPostagePrice,F_HS_JPNoPostagePrice,F_HS_UKNoPostagePrice,F_HS_DENoPostagePrice
                                                        ,F_HS_FRNoPostagePrice,F_HS_EUNoPostagePrice,a.FEFFECTIVEDATE,a.FEXPRIYDATE 
                                                        from T_SAL_PRICELISTENTRY a
                                                        inner join T_SAL_PRICELIST b on a.FID = b.FID
                                                        inner join T_BD_MATERIAL e on a.FMATERIALID = e.FMATERIALID
			                                            where e.FNUMBER = '{0}'
			                                            and b.FNUMBER = '{1}'", entry.FMaterialId, entry.FPriceListId);

                DynamicObjectCollection coll = SQLUtils.GetObjects(ctx, sql);

                if (coll != null && coll.Count > 0)
                {
                    foreach (var item in coll)
                    {
                        entry.FAdjustType = "B";
                        entry.FBeforePrice = SQLUtils.GetFieldValue(item, "FPrice");
                        entry.F_HS_BeforeUSPrice = SQLUtils.GetFieldValue(item, "F_HS_USPrice");
                        entry.F_HS_BeforeUSNoPostagePrice = SQLUtils.GetFieldValue(item, "F_HS_USNoPostagePrice");
                        entry.F_HS_BeforeAUPrice = SQLUtils.GetFieldValue(item, "F_HS_AUPrice");
                        entry.F_HS_BeforeAUNoPostagePrice = SQLUtils.GetFieldValue(item, "F_HS_AUNoPostagePrice");


                        entry.F_HS_BeforeJPNoPostagePrice = SQLUtils.GetFieldValue(item, "F_HS_JPNoPostagePrice");
                        entry.F_HS_BeforeKRNoPostagePrice = SQLUtils.GetFieldValue(item, "F_HS_KRNoPostagePrice");
                        //entry.F_HS_BeforeUKNoPostagePrice = SQLUtils.GetFieldValue(item, "F_HS_UKNoPostagePrice");
                        //entry.F_HS_BeforeDENoPostagePrice = SQLUtils.GetFieldValue(item, "F_HS_DENoPostagePrice");
                        //entry.F_HS_BeforeFRNoPostagePrice = SQLUtils.GetFieldValue(item, "F_HS_FRNoPostagePrice");
                        entry.F_HS_BeforeEUNoPostagePrice = SQLUtils.GetFieldValue(item, "F_HS_EUNoPostagePrice");

                        entry.FBeforeEffDate = SQLUtils.GetFieldValue(item, "FEFFECTIVEDATE");
                        entry.FBeforeUnEffDate = SQLUtils.GetFieldValue(item, "FEXPRIYDATE");
                        entry.FAfterEffDate = entry.FBeforeEffDate;
                        entry.FAfterUnEffDate = entry.FBeforeUnEffDate;
                    }
                }
                else
                {
                    entry.F_HS_BeforeUSNoPostagePrice = string.Empty;
                    entry.F_HS_BeforeAUNoPostagePrice = string.Empty;
                }
            }
        }

        /// <summary>
        /// 检查是否有未生效的批量调价单
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        private bool IsEffective(Context ctx, IEnumerable<AbsSynchroDataInfo> datas)
        {
            if (datas != null && datas.Count() > 0)
            {
                List<K3BatchAdjust> adjusts = datas.Select(d => (K3BatchAdjust)d).ToList();
                List<string> materialNos = adjusts.Select(d => d.Entry).SelectMany(s => s.Select(o => o.FMaterialId)).ToList();

                string sql = string.Format(@"/*dialect*/ select FBILLNO 
			                                from T_SAL_BATCHADJUST a
			                                inner join T_SAL_BATCHADJUSTENTRY b on a.FID = b.FID
			                                inner join T_BD_MATERIAL c on b.FMATERIALID = c.FMATERIALID
			                                where a.FEFFECTIVESTATUS != 'S'
			                                and c.FNUMBER in ('{0}')", string.Join("','", materialNos));
                DynamicObjectCollection coll = SQLUtils.GetObjects(ctx, sql);

                if (coll == null || coll.Count == 0)
                {
                    return true;
                }
                else
                {
                    List<string> billNos = coll.Select(o => SQLUtils.GetFieldValue(o, "FBILLNO")).ToList();
                    this.View.ShowErrMessage(string.Format("有未生效的批量调价单【{0}】，请先生效后操作！", string.Join(",", billNos)), "错误提示", MessageBoxType.Error);
                    return false;
                }
            }

            return false;
        }

        private void ModifyGeneratePriceSeries(ref List<K3BatchAdjustEntry> entries, Dictionary<KeyValuePair<string, string>, Dictionary<string, string>> priceSeries)
        {
            List<string> choices = new List<string>();

            if (entries != null && entries.Count > 0 && priceSeries != null && priceSeries.Count > 0)
            {
                foreach (var entry in entries)
                {
                    foreach (var item in priceSeries)
                    {
                        if (!string.IsNullOrEmpty(item.Key.Key) && !string.IsNullOrEmpty(item.Key.Value)
                            && !string.IsNullOrEmpty(entry.FMaterialId))

                            if (item.Key.Key.Equals(entry.FMaterialId) && item.Key.Value.Equals(GetPropertyName(entry.GetType(), item.Key.Value)))
                            {
                                foreach (var priSer in item.Value)
                                {
                                    if (GetPriceListNumber(priSer.Key).Equals(entry.FPriceListId))
                                    {
                                        choices.Add(entry.FPriceListId);

                                        if (!string.IsNullOrWhiteSpace(GetPropertyValue(entry, item.Key.Value)))
                                        {
                                            if (!string.IsNullOrWhiteSpace(GetPropertyValue(entry, item.Key.Value)) 
                                                && !GetPropertyValue(entry, item.Key.Value).Equals(priSer.Value))
                                            {
                                                GetPropertyInfo(entry.GetType(), item.Key.Value).SetValue(entry, priSer.Value);  
                                            }
                                        }
                                    }
                                }
                            }
                    }
                }
            }
            entries = entries.Where(e => !string.IsNullOrWhiteSpace(e.FPriceListId) && choices.Contains(e.FPriceListId)).ToList();
        }
        /// <summary>
        /// 执行生成批量调价单操作
        /// </summary>
        /// <param name="ctx"></param>
        private void ExecuteOperate(Context ctx)
        {
            HttpResponseResult result = null;
            DynamicObjectCollection coll = GetInput("F_HS_Entity");

            Dictionary<KeyValuePair<string, string>, Dictionary<string, string>> priceSeries = GeneratePriceSeries(ctx, coll);
            Dictionary<string, Dictionary<string, string>> input = GetFirstInput(ctx, coll);

            List<K3BatchAdjustEntry> entries = GetAfterPrice(ctx, input);
            ModifyGeneratePriceSeries(ref entries, priceSeries);
            List<K3BatchAdjust> justs = GetK3BatchAdjusts(ctx, entries);
            Dictionary<SynOperationType, IEnumerable<AbsSynchroDataInfo>> dict = null;

            if (justs != null && justs.Count > 0)
            {
                if (IsEffective(ctx, justs))
                {
                    dict = new Dictionary<SynOperationType, IEnumerable<AbsSynchroDataInfo>>();
                    dict.Add(SynOperationType.SAVE, justs);
                    result = SynchroDataHelper.SynchroDataToK3(ctx, SynchroDataType.BatchAdjust, true, null, dict);
                }

                BindDataToView(ctx, this.View, result, coll.Count);
            }

            if (!string.IsNullOrWhiteSpace(msg))
            {
                this.View.ShowErrMessage(msg, "错误提示", MessageBoxType.Error);
                msg = string.Empty;
            }
        }

        /// <summary>
        /// 显示生成的价格系列
        /// </summary>
        /// <param name="ctx"></param>
        private void ShowPriceSeries(Context ctx)
        {
            DynamicObjectCollection coll = this.View.Model.DataObject["F_HS_Entity"] as DynamicObjectCollection;
            Dictionary<string, Dictionary<string, string>> dicts = GetFirstInput(this.Context, coll);
            List<K3BatchAdjustEntry> entries = GetAfterPrice(this.Context, dicts);
            BindDataToView(ctx, this.View, entries);
        }

        private decimal Ceiling(decimal d)
        {
            string strDecimal = d.ToString();
            string[] datas = null;

            if (strDecimal.Contains("."))
            {
                datas = strDecimal.Split('.');
                int index = strDecimal.IndexOf(".");
                string str = strDecimal.Substring(index + 1, 1);

                if (datas != null && datas.Count() == 2)
                {
                    if (!string.IsNullOrWhiteSpace(str) && str.Equals("0"))
                    {
                        return Convert.ToDecimal(datas[0]);
                    }
                    else
                    {
                        return Convert.ToDecimal(datas[0]) + 1;
                    }
                }
            }
            else
            {
                return d;
            }

            return 0;
        }

        public T GetItems<T>(object obj, char separator, HashSet<string> selects = null)
        {
            Dictionary<string, string> dicStr = null;

            if (obj != null)
            {
                if (obj.GetType().IsAssignableFrom(typeof(string)))
                {
                    string txt = obj as string;

                    if (!string.IsNullOrWhiteSpace(txt) && !char.IsWhiteSpace(separator))
                    {
                        if (txt.Contains(separator.ToString()))
                        {
                            string[] arrTxt = txt.Split(separator);

                            if (arrTxt != null && arrTxt.Length > 0)
                            {
                                dicStr = new Dictionary<string, string>();

                                for (int i = 0; i < arrTxt.Length; i++)
                                {
                                    if (!string.IsNullOrWhiteSpace(arrTxt[i]))
                                    {
                                        if (arrTxt.Length == 14)
                                        {
                                            if (i == 0)
                                            {
                                                dicStr.Add(i.ToString(), arrTxt[i]);
                                            }
                                            if (i == 1)
                                            {
                                                dicStr.Add("USL0", arrTxt[i]);
                                            }
                                            if (i == 2)
                                            {
                                                dicStr.Add("AUL0", arrTxt[i]);
                                            }
                                            if (i == 3)
                                            {
                                                dicStr.Add("JPL0", arrTxt[i]);
                                            }
                                            if (i == 4)
                                            {
                                                dicStr.Add("KRL0", arrTxt[i]);
                                            }
                                            if (i == 5)
                                            {
                                                dicStr.Add("EUL0", arrTxt[i]);
                                            }
                                            if (i > 5)
                                            {
                                                dicStr.Add((i - 5).ToString(), arrTxt[i]);
                                            }
                                        }
                                        else
                                        {
                                            dicStr.Add(i.ToString(), arrTxt[i]);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    return (T)(object)dicStr;
                }
                if (obj.GetType().IsAssignableFrom(typeof(Dictionary<KeyValuePair<string, string>, string>)) || obj.GetType() == typeof(Dictionary<KeyValuePair<string, string>, string>))
                {
                    Dictionary<KeyValuePair<string, string>, Dictionary<string, string>> prices = null;
                    Dictionary<KeyValuePair<string, string>, string> priceSeries = obj as Dictionary<KeyValuePair<string, string>, string>;
                    Dictionary<string, string> dict = null;

                    string txt = string.Empty;

                    if (priceSeries != null && priceSeries.Count > 0)
                    {
                        prices = new Dictionary<KeyValuePair<string, string>, Dictionary<string, string>>();

                        foreach (var item in priceSeries)
                        {
                            dict = new Dictionary<string, string>();

                            if (!string.IsNullOrWhiteSpace(item.Value) && item.Value.Contains(separator))
                            {
                                string[] arrTxt = item.Value.Split(separator);

                                for (int i = 0; i < arrTxt.Length; i++)
                                {
                                    if (!string.IsNullOrWhiteSpace(arrTxt[i]) && IsChoice(selects, i.ToString()))
                                    {
                                        dict.Add(i.ToString(), arrTxt[i]);
                                    }
                                }
                            }
                            prices.Add(item.Key, dict);
                        }

                    }

                    return (T)(object)prices;
                }
            }




            return default(T);
        }

        public override void ButtonClick(ButtonClickEventArgs e)
        {
            switch (e.Key.ToUpper())
            {
                case "F_HS_GENERATEBATCHADJUST":
                    ExecuteOperate(this.Context);
                    break;
            }
        }

        public override void BarItemClick(BarItemClickEventArgs e)
        {
            switch (e.BarItemKey)
            {
                case "tbCreateSeries":
                    ShowPriceSeries(this.Context);
                    break;
                case "tbGenerateBatchAdjust":
                    ExecuteOperate(this.Context);
                    break;
                default:
                    break;

            }
        }
    }
}
