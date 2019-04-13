using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Hands.K3.SCM.APP.Entity.SynDataObject;

using Hands.K3.SCM.APP.Entity.CommonObject;
using Hands.K3.SCM.APP.Entity.K3WebApi;
using Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject;
using Hands.K3.SCM.APP.Entity.StructType;
using Hands.K3.SCM.APP.Utils.Utils;
using Hands.K3.SCM.App.Synchro.Base.Abstract;
using Hands.K3.SCM.App.Synchro.Utils.SynchroService;
using HS.K3.Common.Abbott;
using Hands.K3.SCM.APP.Entity.EnumType;

namespace Hands.K3.SCM.App.Core.SynchroService.ToK3
{

    /// <summary>
    /// 同步 Bazzar 会员（用户）信息到K3CRM客户
    /// </summary>
    public class SynCustomerToK3 : AbstractSynchroToK3
    {
        public const int ORGID = 100035;

        private static List<string> synSuccessNos = new List<string>();

        private readonly static object SaveLock = new object();
        /// <summary>
        /// 数据类型
        /// </summary>
        override public SynchroDataType DataType
        {
            get
            {
                return SynchroDataType.Customer;
            }
        }
        /// <summary>
        /// 数据类型对应的k3cloud的formkey，参照 HSFormIdConst
        /// </summary>
        override public string FormKey
        {
            get
            {
                return HSFormIdConst.Customer;
            }
        }

        /// <summary>
        /// 元数据
        /// </summary>
        override public FormMetadata Metadata
        {
            get
            {
                return MetaDataServiceHelper.Load(this.K3CloudContext, this.FormKey) as FormMetadata;
            }
        }

        public bool BySelect
        {
            get;
            set;
        }
        public List<string> CustRedisKeys
        {
            get;
            set;
        }

        /// <summary>
        /// 执行数据同步钱需要做的事
        /// </summary>
        public override void BeforeDoSynchroData()
        {
            base.BeforeDoSynchroData();

            GetDefualtOrgNumber();

        }

        /// <summary>
        /// 获取需要同步的数据
        /// </summary>
        /// <param name="numbers"></param>
        /// <returns></returns>
        public override IEnumerable<AbsSynchroDataInfo> GetSynchroData(IEnumerable<string> numbers = null)
        {
            List<AbsSynchroDataInfo> customers = ServiceHelper.GetSynchroDatas(this.K3CloudContext, this.DataType, this.RedisDbId, numbers,this.Direction);
            if (customers != null && customers.Count > 0)
            {
                synCustomersLog = synCustomersLog.Concat(customers).ToList();
            }

            //从Redis获取客户信息 
            return customers;
        }

        /// <summary>
        /// 是否取消同步操作
        /// </summary>
        /// <param name="srcData"></param>
        /// <returns></returns>
        public override bool IsCancelSynchro(AbsSynchroDataInfo srcData)
        {

            K3CustomerInfo cust = srcData as K3CustomerInfo;

            if (SynSuccList != default(List<string>))
            {
                if (SynSuccList.Count > 0)
                {
                    foreach (var item in SynSuccList)
                    {
                        if (item.CompareTo(cust.FNumber) == 0)
                        {
                            return false;
                        }
                    }
                }
            }
            QueryBuilderParemeter para = new QueryBuilderParemeter();
            para.FormId = this.FormKey;
            para.SelectItems = SelectorItemInfo.CreateItems("FNumber");

            if (cust != null)
            {
                para.FilterClauseWihtKey = " FNumber='" + cust.FNumber + "' ";
            }

            var k3Data = Kingdee.BOS.App.ServiceHelper.GetService<IQueryService>().GetDynamicObjectCollection(this.K3CloudContext, para);

            if (k3Data == null || k3Data.Count == 0)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 筛选数据 
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="srcDatas"></param>
        /// <returns></returns>
        public override Dictionary<SynOperationType, IEnumerable<AbsSynchroDataInfo>> EntityDataSource(Context ctx, IEnumerable<AbsSynchroDataInfo> srcDatas)
        {
            Dictionary<SynOperationType, IEnumerable<AbsSynchroDataInfo>> dict = new Dictionary<SynOperationType, IEnumerable<AbsSynchroDataInfo>>();

            List<K3CustomerInfo> lstSame = null;
            List<AbsSynchroDataInfo> lstDiff = null;
            List<AbsSynchroDataInfo> lstAllot = null;
            List<AbsSynchroDataInfo> lstUpdate = null;
            List<AbsSynchroDataInfo> lstAllotAfterSumbit = null;
            List<AbsSynchroDataInfo> lstAllotAfterAudit = null;
            HashSet<string> K3CustNos = null;
            HashSet<string> synCustNos = null;
            HashSet<string> existNos = null;

            #region

            string sql = string.Format(@"/*dialect*/ select FNUMBER,FCUSTID,FDOCUMENTSTATUS,FUSEORGID from T_BD_CUSTOMER");
            DynamicObjectCollection coll = SQLUtils.GetObjects(ctx, sql);

            List<K3CustomerInfo> custs = srcDatas.Select(c => (K3CustomerInfo)c).ToList<K3CustomerInfo>();
            List<K3CustomerInfo> allCusts = BuildSynObjByCollection(ctx, coll);

            var groups = from l in allCusts
                         group l by l.FNumber
                         into g
                         select g;

            if (groups != null && groups.Count() > 0)
            {
                lstAllot = new List<AbsSynchroDataInfo>();
                lstAllotAfterAudit = new List<AbsSynchroDataInfo>();
                lstAllotAfterSumbit = new List<AbsSynchroDataInfo>();

                K3CustNos = new HashSet<string>();

                foreach (var g in groups)
                {
                    if (g != null && g.Count() == 1)
                    {
                        if (g.ElementAt(0).FUseOrgId.CompareTo("1") == 0)
                        {
                            if (g.ElementAt(0).FDOCUMENTSTATUS.CompareTo(BillDocumentStatus.Audit) == 0)
                            {
                                AbsSynchroDataInfo data = g.ElementAt(0) as AbsSynchroDataInfo;
                                lstAllot.Add(data);
                            }
                            else
                            {
                                if (g.ElementAt(0).FDOCUMENTSTATUS.CompareTo(BillDocumentStatus.Create) == 0)
                                {
                                    AbsSynchroDataInfo data = g.ElementAt(0) as AbsSynchroDataInfo;
                                    lstAllotAfterSumbit.Add(data);
                                }
                                if (g.ElementAt(0).FDOCUMENTSTATUS.CompareTo(BillDocumentStatus.Auditing) == 0)
                                {
                                    AbsSynchroDataInfo data = g.ElementAt(0) as AbsSynchroDataInfo;
                                    lstAllotAfterAudit.Add(data);
                                }

                            }
                        }

                    }

                    K3CustNos.Add(g.ElementAt(0).FNumber);

                }

                if (lstAllot != null && lstAllot.Count > 0)
                {
                    dict.Add(SynOperationType.ALLOT, lstAllot);
                }
                if (lstAllotAfterAudit != null && lstAllotAfterAudit.Count > 0)
                {
                    dict.Add(SynOperationType.ALLOT_AFTER_AUDIT, lstAllotAfterAudit);
                }
                if (lstAllotAfterSumbit != null && lstAllotAfterSumbit.Count > 0)
                {
                    dict.Add(SynOperationType.ALLOT_AFTER_SUMBIT, lstAllotAfterSumbit);
                }
            }

            if (custs != null && custs.Count > 0)
            {
                synCustNos = new HashSet<string>();

                foreach (var cust in custs)
                {
                    if (cust != null)
                    {
                        synCustNos.Add(cust.FNumber);
                    }
                }
            }

            if (K3CustNos != null && K3CustNos.Count > 0)
            {
                existNos = new HashSet<string>(K3CustNos.Intersect(synCustNos).ToList());
            }
            else
            {
                dict.Add(SynOperationType.SAVE, srcDatas);
                return dict;
            }
            if (existNos != null)
            {
                var same = from c in custs
                           where existNos.Contains(c.FNumber)
                           select c;

                var diff = from c in custs
                           where existNos.Contains(c.FNumber) == false
                           select c;
                if (same != null && same.Count() > 0)
                {
                    lstSame = same.ToList<K3CustomerInfo>();
                    lstUpdate = new List<AbsSynchroDataInfo>();

                    if (lstSame != null && lstSame.Count > 0)
                    {
                        foreach (var s in lstSame)
                        {
                            var uci = from c in allCusts
                                      where c.FNumber.Equals(s.FNumber)
                                      select c;
                            if (uci != null && uci.Count() == 2)
                            {
                                AbsSynchroDataInfo data = s as AbsSynchroDataInfo;
                                lstUpdate.Add(data);
                            }
                        }
                        if (lstUpdate != null && lstUpdate.Count > 0)
                        {
                            dict.Add(SynOperationType.UPDATE, lstUpdate);
                        }
                    }
                }

                if (diff != null && diff.Count() > 0)
                {
                    lstDiff = diff.ToList<AbsSynchroDataInfo>();

                    if (lstDiff != null && lstDiff.Count > 0)
                    {
                        dict.Add(SynOperationType.SAVE, lstDiff);
                    }
                }
            }
            return dict;

            #endregion
        }

        /// <summary>
        /// 将需要同步的会员（用户）数据进行打包（单个客户）
        /// </summary>
        /// <param name="sourceData"></param>
        /// <param name="log"></param>
        /// <param name="operationType"></param>
        /// <returns></returns>
        public override JObject BuildSynchroDataJson(AbsSynchroDataInfo sourceData, SynchroLog log, SynOperationType operationType)
        {
            JObject root = new JObject();

            root.Add("NeedUpDateFields", new JArray(""));
            root.Add("IsDeleteEntry", "false");
            root.Add("SubSystemId", "");
            root.Add("IsVerifyBaseDataField", "true");


            if (operationType == SynOperationType.SAVE)
            {
                root.Add("Model", ConvertSynDataToJObj(sourceData, operationType));
            }
            //数据更新时的Json格式
            else if (operationType == SynOperationType.UPDATE || operationType == SynOperationType.UPDATE_AFTER_ALLOT)
            {
                //更新单据时，表体信息必须填写明细表体的主键
                K3CustomerInfo custData = sourceData as K3CustomerInfo;
                string sql = string.Format(@"/*dialect*/ select FCUSTID,FUseOrgId from T_BD_CUSTOMER where FNumber = '{0}' {1}", custData.FNumber,
                                                         operationType == SynOperationType.UPDATE_AFTER_ALLOT ? string.Format(@"and FUseOrgId = {0}", ORGID) : string.Format(@"and FUseOrgId = {0}", 1));

                DynamicObjectCollection coll = SQLUtils.GetObjects(this.K3CloudContext, sql);
                JArray model = new JArray();

                if (coll.Count > 0)
                {
                    foreach (var c in coll)
                    {
                        if (!string.IsNullOrWhiteSpace(SQLUtils.GetFieldValue(c, "FCUSTID")))
                        {
                            JObject baseData = ConvertSynDataToJObj(sourceData, operationType);
                            //主组织记录
                            if (Convert.ToInt32(SQLUtils.GetFieldValue(c, "FUseOrgId")) == 1)
                            {
                                //主组织记录中的销售员和销售部门不能被更新
                                baseData.Add("FCUSTID", Convert.ToInt32(SQLUtils.GetFieldValue(c, "FUseOrgId")));
                            }
                            else
                            {
                                baseData.Add("FCUSTID", Convert.ToInt32(SQLUtils.GetFieldValue(c, "FUseOrgId")));
                                JObject FSELLER = new JObject();
                                FSELLER.Add("FNumber", custData.FSELLER);
                                baseData.Add("FSELLER", FSELLER);

                                JObject FSALDEPTID = new JObject();
                                FSALDEPTID.Add("FNumber", custData.FSALDEPTID);
                                baseData.Add("FSALDEPTID", FSALDEPTID);

                                //JObject FPRICELISTID = new JObject();//销售价目表
                                //FPRICELISTID.Add("FNumber", SetPriceList(custData, operationType));
                                //baseData.Add("FPRICELISTID", FPRICELISTID);
                            }
                            model.Add(baseData);
                        }
                    }
                }
                root.Add("Model", model);
            }
            return root;
        }

        /// <summary>
        /// 将需要同步的客户转化为JSON格式（客户集合）
        /// </summary>
        /// <param name="sourceDatas"></param>
        /// <param name="operationType"></param>
        /// <returns></returns>
        public override JObject BuildSynchroDataJsons(IEnumerable<AbsSynchroDataInfo> sourceDatas, SynOperationType operationType)
        {
            JObject root = null;
            int batchCount = BatchCount(sourceDatas);

            if (operationType == SynOperationType.SAVE)
            {
                root = new JObject();
                root.Add("NeedUpDateFields", new JArray(""));
                root.Add("NeedReturnFields", new JArray("FNumber"));
                root.Add("IsDeleteEntry", "false");
                root.Add("SubSystemId", "");
                root.Add("IsVerifyBaseDataField", "true");
                root.Add("BatchCount", batchCount);

                root.Add("Model", ConvertSynObjToJObj(sourceDatas, operationType));
                return root;
            }
            //数据更新时的Json格式
            else if (operationType == SynOperationType.UPDATE || operationType == SynOperationType.UPDATE_AFTER_ALLOT)
            {
                root = new JObject();
                JArray model = new JArray();

                foreach (var sourceData in sourceDatas)
                {
                    //更新单据时，表体信息必须填写明细表体的主键
                    K3CustomerInfo custData = sourceData as K3CustomerInfo;
                    string sql = string.Format(@"/*dialect*/ select FCUSTID,FUseOrgId from T_BD_CUSTOMER where FNumber = '{0}' {1}", custData.FNumber,
                                                             operationType == SynOperationType.UPDATE_AFTER_ALLOT ? string.Format(@"and FUseOrgId = {0}", ORGID) : string.Format(@"and FUseOrgId = {0}", 1));

                    DynamicObjectCollection coll = SQLUtils.GetObjects(this.K3CloudContext, sql);
                    var group = from c in coll
                                orderby c["FUseOrgId"] descending
                                select c;

                    if (group != null && group.Count() > 0)
                    {
                        foreach (var c in group)
                        {
                            if (!string.IsNullOrWhiteSpace(SQLUtils.GetFieldValue(c, "FCUSTID")))
                            {
                                JObject baseData = ConvertSynDataToJObj(sourceData, operationType);

                                //主组织记录中的销售员和销售部门不能被更新
                                baseData.Add("FCUSTID", Convert.ToInt32(SQLUtils.GetFieldValue(c, "FCUSTID")));

                                if (Convert.ToInt32(SQLUtils.GetFieldValue(c, "FUseOrgId")) == ORGID)
                                {
                                    JObject FSELLER = new JObject();
                                    FSELLER.Add("FNumber", custData.FSELLER);
                                    baseData.Add("FSELLER", FSELLER);

                                    JObject FSALDEPTID = new JObject();
                                    FSALDEPTID.Add("FNumber", custData.FSALDEPTID);
                                    baseData.Add("FSALDEPTID", FSALDEPTID);

                                    //JObject FPRICELISTID = new JObject();//销售价目表
                                    //FPRICELISTID.Add("FNumber", SetPriceList(custData, operationType));
                                    //baseData.Add("FPRICELISTID", FPRICELISTID);
                                }
                                model.Add(baseData);
                            }
                        }
                    }

                }

                root.Add("Model", model);
            }

            return root;
        }

        /// <summary>
        /// 将需要同步的客户转换为JSON格式对象（单个客户）
        /// </summary>
        /// <param name="sourceData"></param>
        /// <param name="operationType"></param>
        /// <returns></returns>
        private JObject ConvertSynDataToJObj(AbsSynchroDataInfo sourceData, SynOperationType operationType)
        {
            K3CustomerInfo cust = sourceData as K3CustomerInfo;
            JObject baseData = default(JObject);

            if (cust != null)
            {
                baseData = new JObject();
                //创建组织为必填项

                JObject FCreateOrgId = new JObject();
                FCreateOrgId.Add("FNumber", "100");
                baseData.Add("FCreateOrgId", FCreateOrgId);

                if (operationType == SynOperationType.SAVE)
                {
                    //组织内编码不允许变更
                    JObject FUseOrgId = new JObject();
                    FUseOrgId.Add("FNumber", "100");
                    baseData.Add("FUseOrgId", FUseOrgId);

                    baseData.Add("FNumber", cust.FNumber);//客户编码 
                }

                baseData.Add("FName", cust.FName);//客户名称
                baseData.Add("FShortName", cust.FShortName);

                JObject FCOUNTRY = new JObject();//国家
                FCOUNTRY.Add("FNumber", cust.FFCOUNTRY);
                baseData.Add("FCOUNTRY", FCOUNTRY);

                baseData.Add("FADDRESS", cust.FAddress);//通讯地址
                baseData.Add("FZIP", cust.FZIP);
                baseData.Add("FTEL", cust.FTEL);

                JObject FGroup = new JObject();//客户分组
                FGroup.Add("FNumber", cust.FGroup);
                baseData.Add("FGroup", FGroup);

                JObject FCustTypeId = new JObject();//客户类别
                FCustTypeId.Add("FNumber", cust.FCustTypeId);
                baseData.Add("FCustTypeId", FCustTypeId);

                baseData.Add("F_HS_SpecialDemand", cust.F_HS_SpecialDemand);//客户特殊要求

                baseData.Add("F_HS_TaxNum", cust.F_HS_TaxNum);

                JObject FTRADINGCURRID = new JObject();//结算币别
                FTRADINGCURRID.Add("FNumber", cust.FTRADINGCURRID);
                baseData.Add("FTRADINGCURRID", FTRADINGCURRID);

                JObject FRECCONDITIONID = new JObject();//收款条件
                FRECCONDITIONID.Add("FNumber", "SKTJ05");
                baseData.Add("FRECCONDITIONID", FRECCONDITIONID);

                baseData.Add("FInvoiceType", cust.FInvoiceType);//发票类型

                JObject FTaxType = new JObject();//税率类别
                FTaxType.Add("FNumber", cust.FTaxType);
                baseData.Add("FTaxType", FTaxType);

                JObject FTaxRate = new JObject();//默认税率
                FTaxRate.Add("FNumber", cust.FTaxRate);
                baseData.Add("FTaxRate", FTaxRate);

                baseData.Add("FPriority", cust.FPriority);//客户优先级
                baseData.Add("FIsTrade", cust.FIsTrade);//是否交易客户
                baseData.Add("F_HS_CustomerRegisteredMail", cust.F_HS_CustomerRegisteredMail);//客户电子邮箱
                baseData.Add("F_HS_Grade", cust.F_HS_Grade);//客户会员等级

                baseData.Add("F_HS_UseLocalPriceList", cust.F_HS_UseLocalPriceList);//是否使用收货地价目表
                baseData.Add("F_HS_CustomerPurchaseMail", cust.F_HS_CustomerPurchaseMail);//客户采购邮箱

                baseData.Add("F_HS_ReportingRequirements", cust.F_HS_ReportingRequirements);
                baseData.Add("F_HS_ShippingInformation", cust.F_HS_ShippingInformation);
                baseData.Add("F_HS_InvoiceComments", cust.F_HS_InvoiceComments);
                baseData.Add("F_HS_DiscountOrPoint", cust.F_HS_DiscountOrPoint);
                baseData.Add("F_HS_CompanyName", cust.F_HS_CompanyName);//客户公司名
            }

            return baseData;
        }

        /// <summary>
        /// 根据客户等级设置对应的价目表(编码)
        /// </summary>
        /// <param name="cust"></param>
        /// <param name="operationType"></param>
        /// <returns></returns>
        private string SetPriceList(K3CustomerInfo cust, SynOperationType operationType)
        {
            if (operationType == SynOperationType.SAVE)
            {
                return GetPriceListNo(cust.FCustTypeId);
            }
            else if (operationType == SynOperationType.UPDATE)
            {
                if (cust != null)
                {
                    if (!string.IsNullOrWhiteSpace(cust.FCustTypeId))
                    {
                        string sql = string.Format(@"/*dialect*/ select b.FNUMBER from T_BD_CUSTOMER a
                                                            inner join T_SAL_PRICELIST b
                                                            on a.FPRICELISTID = b.FID
                                                            where a.FNUMBER = '{0}' and b.FSALEORGID = 100035", cust.FNumber);

                        string priceListNo = JsonUtils.ConvertObjectToString(SQLUtils.GetObject(this.K3CloudContext, sql, "FNUMBER"));

                        if (!string.IsNullOrWhiteSpace(priceListNo))
                        {
                            string sql_ = string.Format(@"/*dialect*/ select FNUMBER from T_SAL_PRICELIST where FNUMBER = '{0}'", priceListNo);

                            if (SQLUtils.GetObject(this.K3CloudContext, sql, "FNUMBER") == null)
                            {
                                return priceListNo;
                            }
                        }

                        return GetPriceListNo(cust.FCustTypeId);
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// 根据客户等级设置对应的价目表(ID)
        /// </summary>
        /// <param name="cust"></param>
        /// <returns></returns>
        private string SetPriceListId(K3CustomerInfo cust)
        {
            if (cust != null)
            {
                if (!string.IsNullOrWhiteSpace(cust.FCustTypeId))
                {
                    string sql = string.Format(@"/*dialect*/ select b.FNUMBER from T_BD_CUSTOMER a
                                                            inner join T_SAL_PRICELIST b
                                                            on a.FPRICELISTID = b.FID
                                                            where a.FNUMBER = '{0}' and b.FSALEORGID = 100035", cust.FNumber);

                    string priceListNo = JsonUtils.ConvertObjectToString(SQLUtils.GetObject(this.K3CloudContext, sql, "FNUMBER"));

                    if (!string.IsNullOrWhiteSpace(priceListNo))
                    {
                        string sql_ = string.Format(@"/*dialect*/ select FNUMBER from T_SAL_PRICELIST where FNUMBER = '{0}'", priceListNo);

                        if (SQLUtils.GetObject(this.K3CloudContext, sql, "FNUMBER") == null)
                        {
                            return SQLUtils.GetPriceListId(this.K3CloudContext, cust.FNumber, priceListNo);
                        }
                    }

                    return GetPriceListId(cust.FCustTypeId);
                }
            }

            return string.Empty;
        }
        /// <summary>
        /// 不同客户类别对应的销售价目表(编码)
        /// </summary>
        /// <param name="custLevel"></param>
        /// <returns></returns>
        private string GetPriceListNo(string custLevel)
        {
            if (!string.IsNullOrWhiteSpace(custLevel))
            {
                switch (custLevel)
                {
                    case "Level0":
                        return "XSJMB0001";
                    case "Level1":
                        return "XSJMB0002";
                    case "Level2":
                        return "XSJMB0003";
                    case "Level3":
                        return "XSJMB0004";
                    case "Level4":
                        return "XSJMB0005";
                    case "Level5":
                        return "XSJMB0006";
                    case "Level6":
                        return "XSJMB0007";
                    case "Level7":
                        return "XSJMB0008";
                    case "Level8":
                        return "XSJMB0009";
                }
            }
            return null;
        }

        /// <summary>
        /// 不同客户类别对应的销售价目表(ID)
        /// </summary>
        /// <param name="custLevel"></param>
        /// <returns></returns>
        private string GetPriceListId(string custLevel)
        {
            if (!string.IsNullOrWhiteSpace(custLevel))
            {
                switch (custLevel)
                {
                    case "Level0":
                        return "758622";
                    case "Level1":
                        return "758624";
                    case "Level2":
                        return "758625";
                    case "Level3":
                        return "758628";
                    case "Level4":
                        return "758632";
                    case "Level5":
                        return "758638";
                    case "Level6":
                        return "758639";
                    case "Level7":
                        return "758829";
                    case "Level8":
                        return "758831";
                }
            }
            return null;
        }
        /// <summary>
        /// 将需要同步的客户转换为JSON格式对象（客户集合）
        /// </summary>
        /// <param name="sourceDatas"></param>
        /// <param name="opreationType"></param>
        /// <returns></returns>
        private JArray ConvertSynObjToJObj(IEnumerable<AbsSynchroDataInfo> sourceDatas, SynOperationType opreationType)
        {

            JArray model = new JArray();
            JObject baseData = default(JObject);

            if (sourceDatas != null)
            {
                foreach (var custData in sourceDatas)
                {
                    baseData = ConvertSynDataToJObj(custData, opreationType);
                    model.Add(baseData);
                }
            }

            return model;
        }

        /// <summary>
        /// 数据库查询的结果集封装成List<K3CustomerInfo>对象
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="coll"></param>
        /// <returns></returns>
        public virtual List<K3CustomerInfo> BuildSynObjByCollection(Context ctx, DynamicObjectCollection coll)
        {
            List<K3CustomerInfo> custs = null;
            K3CustomerInfo cust = null;

            if (coll != null)
            {
                if (coll.Count > 0)
                {
                    custs = new List<K3CustomerInfo>();
                    foreach (var item in coll)
                    {
                        cust = new K3CustomerInfo();

                        cust.FUseOrgId = SQLUtils.GetFieldValue(item, "FUseOrgId");
                        cust.FNumber = SQLUtils.GetFieldValue(item, "FNumber");
                        cust.FCUSTID = Convert.ToInt32(SQLUtils.GetFieldValue(item, "FCUSTID"));
                        cust.FDOCUMENTSTATUS = SQLUtils.GetFieldValue(item, "FDOCUMENTSTATUS");

                        custs.Add(cust);
                    }
                }
            }
            return custs;
        }

        /// <summary>
        /// 客户更新
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="custs"></param>
        /// <returns></returns>
        private List<string> UpdateCustomers(Context ctx, List<K3CustomerInfo> custs)
        {
            List<string> lstUpdate = null;


            if (custs != null && custs.Count > 0)
            {
                lstUpdate = new List<string>();

                foreach (var cust in custs)
                {
                    if (cust != null)
                    {
                        string mSql = string.Format(@"/*dialect*/ update a set a.F_HS_SPECIALDEMAND = '{0}',a.FTEL = '{1}',a.F_HS_CUSTOMERREGISTEREDMAIL = '{2}' 
                                                                ,a.FCustTypeId = '{3}',a.F_HS_TAXNUM = '{4}',a.FADDRESS = '{5}',a.FZIP = '{6}'
                                                                ,a.FCOUNTRY = '{7}',a.F_HS_CustomerPurchaseMail = '{8}',a.F_HS_ReportingRequirements = '{9}'
                                                                ,a.F_HS_ShippingInformation = '{10}',a.F_HS_InvoiceComments = '{11}',a.F_HS_DiscountOrPoint = '{12}'
                                                                ,a.F_HS_CompanyName = '{15}',a.F_HS_UseLocalPriceList = '{16}'
                                                                from T_BD_CUSTOMER a
                                                                inner join T_BD_CUSTOMER_L b 
                                                                on a.FCUSTID = b.FCUSTID
                                                                where a.FUSEORGID = {13} and a.FNumber = '{14}'", JsonUtils.HtmlESC(cust.F_HS_SpecialDemand), cust.FTEL, cust.F_HS_CustomerRegisteredMail
                                                                , SQLUtils.GetCustGroupId(ctx, cust.FCustTypeId), cust.F_HS_TaxNum
                                                                , JsonUtils.HtmlESC(cust.FAddress), cust.FZIP, SQLUtils.GetCountryId(ctx, cust.FFCOUNTRY), cust.F_HS_CustomerPurchaseMail, JsonUtils.HtmlESC(cust.F_HS_ReportingRequirements)
                                                                , JsonUtils.HtmlESC(cust.F_HS_ShippingInformation), JsonUtils.HtmlESC(cust.F_HS_InvoiceComments), JsonUtils.HtmlESC(cust.F_HS_DiscountOrPoint), 1, cust.FNumber, JsonUtils.HtmlESC(cust.F_HS_CompanyName), cust.F_HS_UseLocalPriceList ? "1" : "0");

                        string sSql = string.Format(@"/*dialect*/ update a set a.F_HS_SPECIALDEMAND = '{0}',a.FTEL = '{1}',a.F_HS_CUSTOMERREGISTEREDMAIL = '{2}' 
                                                                  ,a.FCustTypeId = '{3}',a.F_HS_TAXNUM = '{4}',a.FADDRESS = '{5}',a.FZIP = '{6}'
                                                                  ,a.FCOUNTRY = '{7}',a.F_HS_CustomerPurchaseMail = '{8}',a.FSALDEPTID = {9},a.FSELLER = {10}
                                                                  , a.F_HS_ReportingRequirements = '{11}',a.F_HS_ShippingInformation = '{12}',a.F_HS_InvoiceComments = '{13}'
                                                                  ,a.F_HS_DiscountOrPoint = '{14}',a.F_HS_CompanyName = '{17}',a.F_HS_UseLocalPriceList = '{18}'
                                                                  from T_BD_CUSTOMER a
                                                                  inner join T_BD_CUSTOMER_L b 
                                                                  on a.FCUSTID = b.FCUSTID
                                                                  where a.FUSEORGID = {15} and a.FNumber = '{16}'",
                                                                   JsonUtils.HtmlESC(cust.F_HS_SpecialDemand), cust.FTEL, cust.F_HS_CustomerRegisteredMail
                                                                , SQLUtils.GetCustGroupId(ctx, cust.FCustTypeId), cust.F_HS_TaxNum
                                                                , JsonUtils.HtmlESC(cust.FAddress), cust.FZIP, SQLUtils.GetCountryId(ctx, cust.FFCOUNTRY), cust.F_HS_CustomerPurchaseMail, SQLUtils.GetDeptId(ctx, cust.FSELLER)
                                                                , SQLUtils.GetSellerId(ctx, cust.FSELLER), JsonUtils.HtmlESC(cust.F_HS_ReportingRequirements)
                                                                , JsonUtils.HtmlESC(cust.F_HS_ShippingInformation), JsonUtils.HtmlESC(cust.F_HS_InvoiceComments), JsonUtils.HtmlESC(cust.F_HS_DiscountOrPoint), 100035, cust.FNumber, JsonUtils.HtmlESC(cust.F_HS_CompanyName), cust.F_HS_UseLocalPriceList ? "1" : "0");

                        string lSql = string.Format(@"/*dialect*/ update a set a.FNAME = '{0}',a.FSHORTNAME = '{1}' from T_BD_CUSTOMER_L a
                                                                    inner join T_BD_CUSTOMER b on a.FCUSTID = b.FCUSTID
                                                                    where b.FNUMBER = '{2}'", JsonUtils.HtmlESC(cust.FName), JsonUtils.HtmlESC(cust.FShortName), cust.FNumber);
                        lstUpdate.Add(mSql);
                        lstUpdate.Add(sSql);
                        lstUpdate.Add(lSql);

                        try
                        {
                            int count = DBUtils.ExecuteBatch(ctx, lstUpdate, 4);
                            if (count == 4)
                            {
                                synSuccessNos.Add(cust.FNumber);
                                lstUpdate.Clear();
                                LogUtils.WriteSynchroLog(ctx, SynchroDataType.Customer, "客户更新，客户编码【" + cust.FNumber + "】更新成功！");
                            }
                        }
                        catch (Exception ex)
                        {
                            LogUtils.WriteSynchroLog(ctx, SynchroDataType.Customer, "客户更新，异常信息：客户编码【" + cust.FNumber + "】" + ex.Message + System.Environment.NewLine + ex.StackTrace + System.Environment.NewLine + "SQL:" + mSql + sSql + lSql);
                        }
                    }

                }
            }
            return synSuccessNos;
        }
        private HttpResponseResult UpdateCustomersAfterAllot(Context ctx, List<K3CustomerInfo> custs)
        {
            HttpResponseResult result = new HttpResponseResult();
            result.SuccessEntityNos = new List<string>();

            if (custs != null && custs.Count > 0)
            {
                for (int i = 0; i < custs.Count; i++)
                {
                    try
                    {
                        string sql = string.Format(@"/*dialect*/ update  a
                                                        set a.FSELLER = {0},a.FSALDEPTID = {1}
                                                        from T_BD_CUSTOMER a
                                                        where a.FCUSTID = {2}", SQLUtils.GetSellerId(ctx, custs[i].FSELLER), SQLUtils.GetDeptId(ctx, custs[i].FSELLER), custs[i].FCUSTID);
                        int count = DBUtils.Execute(ctx, sql);

                        if (count > 0)
                        {
                            result.Success = true;
                            result.SuccessEntityNos.Add(custs[i].FNumber);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogUtils.WriteSynchroLog(ctx, SynchroDataType.Customer, "客户【" + custs[i].FNumber + "】分配后更新出现异常：" + ex.Message + Environment.NewLine + ex.StackTrace);
                    }

                }

            }

            return result;
        }
        /// <summary>
        /// 执行同步客户操作
        /// </summary>
        /// <param name="sourceDatas"></param>
        /// <param name="logs"></param>
        /// <param name="operationType"></param>
        /// <returns></returns>
        public override HttpResponseResult ExecuteSynchro(IEnumerable<AbsSynchroDataInfo> sourceDatas, List<SynchroLog> logs, SynOperationType operationType)
        {
            HttpResponseResult result = null;
            KDTransactionScope trans = null;

            List<K3CustomerInfo> custs = null;
            JObject bizData = null;
            List<int> pkIds = null;

            if (sourceDatas != null && sourceDatas.Count() > 0)
            {
                custs = sourceDatas.Select(c => (K3CustomerInfo)c).ToList<K3CustomerInfo>();
                bizData = BuildSynchroDataJsons(sourceDatas, operationType);
            }

            try
            {
                if (operationType == SynOperationType.SAVE)
                {
                    #region
                    using (trans = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
                    {
                        #region
                        //单据保存 
                        result = ExecuteOperate(SynOperationType.SAVE, null, null, bizData.ToString());

                        if (result != null && result.SuccessEntityNos != null && result.SuccessEntityNos.Count > 0)
                        {
                            //同步成功后删除Redis中的数据
                            RemoveRedisData(this.K3CloudContext, result.SuccessEntityNos);

                            //保存操作成功后返回的单据内码集合
                            if (result.GetNeedReturnValues("FCUSTID").Select(obj => Convert.ToInt32(obj)).ToList() != null)
                            {
                                pkIds = result.GetNeedReturnValues("FCUSTID").Select(obj => Convert.ToInt32(obj)).ToList();
                            }

                            //单据提交
                            result = ExecuteOperate(SynOperationType.SUBMIT, result.SuccessEntityNos, null, null);

                            if (result != null && result.SuccessEntityNos != null && result.SuccessEntityNos.Count > 0)
                            {
                                //单据审核
                                result = ExecuteOperate(SynOperationType.AUDIT, result.SuccessEntityNos, null, null);

                                //单据分配
                                result = ExecuteOperate(SynOperationType.ALLOT, null, pkIds, null);

                                //分配后更新提交审核
                                result = AfterAllot(this.K3CloudContext, custs);
                            }
                        }

                        //提交事务
                        trans.Complete();
                        #endregion
                    }
                    #endregion

                }
                else if (operationType == SynOperationType.ALLOT)
                {
                    using (trans = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
                    {
                        if (custs != null && custs.Count > 0)
                        {
                            result = Allot(this.K3CloudContext, custs);
                            result = AfterAllot(this.K3CloudContext, custs);
                        }
                        trans.Complete();
                    }
                }
                else if (operationType == SynOperationType.UPDATE)
                {
                    string sql = string.Empty;
                 
                    List<SqlObject> sqlObjects = null;
                    List<SqlParam> sqlParams = null;

                    using (trans = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
                    {
                        result = ExecuteOperate(SynOperationType.SAVE, null, null, bizData.ToString());

                        if (result != null && result.SuccessEntityNos != null && result.SuccessEntityNos.Count > 0)
                        {
                            //同步成功后删除Redis中的数据
                            RemoveRedisData(this.K3CloudContext, result.SuccessEntityNos);
                        }
                        trans.Complete();
                    }
                    if (custs != null && custs.Count > 0)
                    {
                        sqlObjects = new List<SqlObject>();
                        
                        foreach (var cust in custs)
                        {
                            if (cust != null)
                            {
                                sqlParams = new List<SqlParam>();
                                sql = string.Format(@"/*dialect*/ update T_BD_CUSTOMER set FSALDEPTID = @FSALDEPTID,FSELLER = @FSELLER where FUSEORGID = @FUSEORGID and FNUMBER = @FNUMBER");
                                sqlParams.Add(new SqlParam("@FSALDEPTID", KDDbType.Int32, SQLUtils.GetDeptId(this.K3CloudContext, cust.FSELLER)));
                                sqlParams.Add(new SqlParam("@FSELLER",KDDbType.Int32, SQLUtils.GetSellerId(this.K3CloudContext, cust.FSELLER)));
                                sqlParams.Add(new SqlParam("@FUSEORGID", KDDbType.Int32, ORGID));
                                sqlParams.Add(new SqlParam("@FNUMBER",KDDbType.String,cust.FNumber));
                                sqlObjects.Add(new SqlObject(sql,sqlParams));
                            }
                        }
                        DBUtils.ExecuteBatch(this.K3CloudContext, sqlObjects);
                    }

                }
                else if (operationType == SynOperationType.ALLOT_AFTER_SUMBIT)
                {
                    using (trans = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
                    {
                        result = ExecuteOperate(SynOperationType.SUBMIT, null, custs.Select(c => c.FCUSTID).ToList());

                        if (result != null && result.SuccessEntityIds != null && result.SuccessEntityIds.Count > 0)
                        {
                            result = ExecuteOperate(SynOperationType.AUDIT, null, result.SuccessEntityIds);
                            {
                                result = ExecuteOperate(SynOperationType.ALLOT, null, result.SuccessEntityIds);
                            }

                            result = AfterAllot(this.K3CloudContext, custs);
                        }
                        trans.Complete();
                    }
                }
                else if (operationType == SynOperationType.ALLOT_AFTER_AUDIT)
                {
                    using (trans = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
                    {
                        result = ExecuteOperate(SynOperationType.AUDIT, null, custs.Select(c => c.FCUSTID).ToList());

                        if (result != null && result.SuccessEntityIds != null && result.SuccessEntityIds.Count > 0)
                        {
                            result = ExecuteOperate(SynOperationType.ALLOT, null, result.SuccessEntityIds);
                        }

                        result = AfterAllot(this.K3CloudContext, custs);
                        trans.Complete();
                    }

                }
                return result;
            }
            catch (Exception ex)
            {
                if (logs != null && logs.Count > 0)
                {
                    foreach (var log in logs)
                    {
                        log.IsSuccess = 0;
                        log.ErrInfor = ex.Message + System.Environment.NewLine + ex.StackTrace;
                    }
                }

                LogUtils.WriteSynchroLog(this.K3CloudContext, this.DataType, ex.Message + System.Environment.NewLine + ex.StackTrace);
            }
            finally
            {
                if (trans != null)
                {
                    trans.Dispose();
                }
            }
            if (result == null)
            {
                return null;
            }

            if (result.Success == false && result.FailedResult == null && result.Result == null)
            {
                //同步出现错误之类：如令牌错误，url错误之类的
                if (logs != null && logs.Count > 0)
                {
                    foreach (var log in logs)
                    {
                        log.IsSuccess = 0;
                        log.ErrInfor = "数据同步失败：" + result.Message == null ? "" : result.Message;
                    }
                }

                return result;
            }

            return result;
        }

        public HttpResponseResult Allot(Context ctx, List<K3CustomerInfo> custs)
        {
            HttpResponseResult result = null;

            List<int> pkIds = new List<int>();

            if (custs != null && custs.Count > 0)
            {
                pkIds = custs.Select(c => c.FCUSTID).ToList();

                if (pkIds != null && pkIds.Count > 0)
                {
                    result = ExecuteOperate(SynOperationType.ALLOT, null, pkIds, null);
                }
            }

            return result;
        }
        public HttpResponseResult AfterAllot(Context ctx, List<K3CustomerInfo> custs)
        {
            HttpResponseResult result = null;

            List<int> pkIds = null;
            List<string> numbers = null;

            #region

            if (custs != null && custs.Count > 0)
            {
                numbers = custs.Select(c => c.FNumber).ToList<string>();

                //分配成功后单据编码集合,做为查询从组织的内码的条件
                string FNumber = FormatNumber(numbers);
                //根据单据编码集合查询出从组织的内码集合
                string sFCustId = string.Format(@"/*dialect*/ select FCUSTID,FNUMBER,FDOCUMENTSTATUS from T_BD_CUSTOMER where FNumber in ({0}) and FUSEORGID = {1}", FNumber, ORGID);
                DynamicObjectCollection items = SQLUtils.GetObjects(ctx, sFCustId);
                List<K3CustomerInfo> datas = new List<K3CustomerInfo>();

                if (items != null && items.Count > 0)
                {
                    pkIds = new List<int>();

                    for (int i = 0; i < items.Count; i++)
                    {
                        if (items[i] != null)
                        {
                            pkIds.Add(Convert.ToInt32(SQLUtils.GetFieldValue(items[i], "FCUSTID")));

                            if (SQLUtils.GetFieldValue(items[i], "FNumber").CompareTo(custs[i].FNumber) == 0)
                            {
                                custs[i].FCUSTID = Convert.ToInt32(SQLUtils.GetFieldValue(items[i], "FCUSTID"));
                                custs[i].FDOCUMENTSTATUS = SQLUtils.GetFieldValue(items[i], "FDOCUMENTSTATUS");

                                datas.Add(custs[i]);
                            }
                        }
                    }

                    List<int> nonAuditCustIds = new List<int>();
                    List<int> auditCustIds = new List<int>();

                    if (datas != null)
                    {
                        if (datas.Where(c => !c.FDOCUMENTSTATUS.Equals(BillDocumentStatus.Audit)) != null)
                        {
                            nonAuditCustIds = datas.Where(c => !c.FDOCUMENTSTATUS.Equals(BillDocumentStatus.Audit)).ToList().Select(c => c.FCUSTID).ToList();
                        }
                        if (datas.Where(c => c.FDOCUMENTSTATUS.Equals(BillDocumentStatus.Audit)) != null)
                        {
                            auditCustIds = datas.Where(c => c.FDOCUMENTSTATUS.Equals(BillDocumentStatus.Audit)).ToList().Select(c => c.FCUSTID).ToList();
                        }
                    }

                    JObject bizData = BuildSynchroDataJsons(datas.ToList<AbsSynchroDataInfo>(), SynOperationType.UPDATE_AFTER_ALLOT);
                    //单据反审核
                    result = ExecuteOperate(SynOperationType.UNAUDIT, null, auditCustIds, null);
                    //单据更新
                    result = ExecuteOperate(SynOperationType.SAVE, null, null, bizData.ToString());

                    if (nonAuditCustIds.Union(auditCustIds).ToList().Count > 0)
                    {
                        //单据提交
                        result = ExecuteOperate(SynOperationType.SUBMIT, null, nonAuditCustIds.Union(auditCustIds).ToList(), null);
                        //单据审核
                        result = ExecuteOperate(SynOperationType.AUDIT, null, nonAuditCustIds.Union(auditCustIds).ToList(), null);
                    }

                    pkIds.Clear();

                }
            }

            #endregion
            return result;
        }

        public string FormatNumber(List<string> numbers)
        {
            string FNumber = "";
            if (numbers != null && numbers.Count > 0)
            {
                for (int i = 0; i < numbers.Count; i++)
                {
                    if (i < numbers.Count - 1)
                    {
                        FNumber += "\'" + numbers[i] + "\',";
                    }
                    else if (i == numbers.Count - 1)
                    {
                        FNumber += "\'" + numbers[i] + "\'";
                    }
                }
                return FNumber;
            }

            return null;
        }

        public override AbsSynchroDataInfo BuildSynchroData(Context ctx, string json, AbsSynchroDataInfo data = null)
        {
            JObject jobj = JsonUtils.ParseJson2JObj(ctx, SynchroDataType.Customer, json);
            K3CustomerInfo cust = new K3CustomerInfo();

            cust.FCreateOrgId = "100";
            cust.FNumber = JsonUtils.GetFieldValue(jobj, "customers_id");//客户编码
            cust.FUseOrgId = "100";
            cust.FName = JsonUtils.GetFieldValue(jobj, "customers_firstname") + " " +
                             JsonUtils.GetFieldValue(jobj, "customers_lastname");//客户姓名

            cust.FShortName = JsonUtils.GetFieldValue(jobj, "customers_firstname");//客户简称

            cust.F_HS_CustomerRegisteredMail = JsonUtils.GetFieldValue(jobj, "customers_email_address");//客户电子邮箱地址

            if (string.IsNullOrWhiteSpace(cust.FName))
            {
                cust.FName = "None Name";
            }


            cust.FFCOUNTRY = JsonUtils.GetFieldValue(jobj, "delivery_country");//客户所在国家
            cust.FAddress = JsonUtils.GetFieldValue(jobj, "entry_street_address") + " "//客户详细地址
                           + JsonUtils.GetFieldValue(jobj, "entry_city") + " "
                           + JsonUtils.GetFieldValue(jobj, "entry_state") + " "
                           + JsonUtils.GetFieldValue(jobj, "delivery_country");


            cust.FZIP = JsonUtils.GetFieldValue(jobj, "entry_postcode");//客户邮编

            cust.FTEL = JsonUtils.GetFieldValue(jobj, "customers_telephone");//客户手机
            cust.FFAX = JsonUtils.GetFieldValue(jobj, "customers_FFAX");//客户传真

            cust.FIsGroup = false;
            cust.FIsDefPayer = false;

            string custLevel = JsonUtils.GetFieldValue(jobj, "customers_whole");//客户等级
            cust.FGroup = SetCustomerLevel(custLevel);
            cust.FCustTypeId = SetCustomerLevel(custLevel);

            cust.F_HS_TaxNum = JsonUtils.GetFieldValue(jobj, "customers_tax_id");//客户税号
            cust.FTRADINGCURRID = "USD";

            string sellerNo = JsonUtils.GetFieldValue(jobj, "account_manager_id");//销售员编码
            sellerNo = JsonUtils.HtmlESC(sellerNo);

            cust.FSELLER = string.IsNullOrEmpty(sellerNo) ? "NA" : sellerNo;
            cust.FSALDEPTID = SQLUtils.GetDeptNo(ctx, cust.FSELLER);//销售员

            cust.F_HS_SpecialDemand = JsonUtils.HtmlESC(JsonUtils.GetFieldValue(jobj, "special_requirements"));//客户特殊要求
            cust.F_HS_Grade = JsonUtils.GetFieldValue(jobj, "grade");//客户等级

            cust.F_HS_UseLocalPriceList = JsonUtils.GetFieldValue(jobj, "use_area_prices").CompareTo("1") == 0 ? true : false;//是否使用发货地价目表
            cust.F_HS_CustomerPurchaseMail = JsonUtils.GetFieldValue(jobj, "buyer_email");
            cust.F_HS_ReportingRequirements = JsonUtils.GetFieldValue(jobj, "declare");
            cust.F_HS_ShippingInformation = JsonUtils.GetFieldValue(jobj, "ship_info");
            cust.F_HS_InvoiceComments = JsonUtils.GetFieldValue(jobj, "invoice_remark");
            cust.F_HS_DiscountOrPoint = JsonUtils.GetFieldValue(jobj, "points_discounts");
            cust.F_HS_CompanyName = JsonUtils.GetFieldValue(jobj, "customers_company");

            cust.FSETTLETYPEID = "JSFS01_SYS";
            cust.FTaxType = "SZ01_SYS";
            cust.FTaxRate = "SL04_SYS";

            cust.FPriority = "1";
            cust.FRECEIVECURRID = "PRE001";

            cust.FISCREDITCHECK = false;
            cust.FIsTrade = true;

            return cust;
        }

        /// <summary>
        /// 设置客户等级
        /// </summary>
        /// <param name="custLevel"></param>
        /// <returns></returns>
        public static string SetCustomerLevel(string custLevel)
        {
            if (!string.IsNullOrEmpty(custLevel))
            {
                switch (custLevel)
                {
                    case "1":
                        return "Level1";
                    case "2":
                        return "Level2";
                    case "3":
                        return "Level3";
                    case "4":
                        return "Level4";
                    case "5":
                        return "Level5";
                    case "6":
                        return "Level6";
                    case "7":
                        return "Level7";
                    case "8":
                        return "Level8";
                    case "0":
                        return "Level0";
                }

            }
            return null;
        }
    }
}
