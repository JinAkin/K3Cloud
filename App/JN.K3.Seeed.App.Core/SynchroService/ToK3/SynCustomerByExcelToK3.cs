using Hands.K3.SCM.App.Synchro.Base.Abstract;
using Hands.K3.SCM.App.Synchro.Utils.SynchroService;
using Hands.K3.SCM.APP.Entity.CommonObject;
using Hands.K3.SCM.APP.Entity.EnumType;
using Hands.K3.SCM.APP.Entity.K3WebApi;
using Hands.K3.SCM.APP.Entity.StructType;
using Hands.K3.SCM.APP.Entity.SynDataObject;
using Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject;
using Hands.K3.SCM.APP.Utils.Utils;
using HS.K3.Common.Abbott;
using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Hands.K3.SCM.App.Core.SynchroService.ToK3
{
    public class SynCustomerByExcelToK3 : AbstractSynchroToK3
    {
        public const int ORGID = 100035;
        /// <summary>
        /// 数据类型
        /// </summary>
        override public SynchroDataType DataType
        {
            get
            {
                return SynchroDataType.CustomerByExcel;
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
        /// 执行数据同步前需要做的事
        /// </summary>
        public override void BeforeDoSynchroData()
        {
            base.BeforeDoSynchroData();

            GetDefualtOrgNumber();

        }
        /// <summary>
        /// 获取需要同步的数据
        /// </summary>
        /// <param name="dbId"></param>
        /// <param name="numbers"></param>
        /// <returns></returns>
        public override IEnumerable<AbsSynchroDataInfo> GetSynchroData(IEnumerable<string> numbers = null)
        {
            //从Redis获取订单信息
            List<K3CustomerInfo> lstCust = null;
            if (BySelect)
            {
                lstCust = DownLoadCustomersByExcel(this.K3CloudContext);
            }
            else
            {
                lstCust = DownLoadCustomersByExcel(this.K3CloudContext);
            }

            return lstCust.ToList<AbsSynchroDataInfo>();
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
        /// 筛选需要保存或更新的客户资料
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="srcDatas"></param>
        /// <returns></returns>
        public override Dictionary<SynOperationType, IEnumerable<AbsSynchroDataInfo>> EntityDataSource(Context ctx, IEnumerable<AbsSynchroDataInfo> srcDatas)
        {
            Dictionary<SynOperationType, IEnumerable<AbsSynchroDataInfo>> dict = new Dictionary<SynOperationType, IEnumerable<AbsSynchroDataInfo>>();

           
            List<AbsSynchroDataInfo> lstDiff = null;

            List<string> K3CustNos = null;
            List<string> synCustNos = null;
            List<string> existNos = null;

            #region

            string sql = string.Format(@"/*dialect*/ select distinct FNUMBER from T_BD_CUSTOMER");
            DynamicObjectCollection coll = SQLUtils.GetObjects(ctx, sql);

            if (coll != null && coll.Count > 0)
            {
                K3CustNos = new List<string>();
                foreach (var item in coll)
                {
                    K3CustNos.Add(JsonUtils.ConvertObjectToString(item["FNUMBER"]));
                }
                coll = null;
            }

            List<K3CustomerInfo> custs = ConvertAbsSynchroObject(srcDatas);

            if (custs != null && custs.Count > 0)
            {
                synCustNos = new List<string>();

                foreach (var cust in custs)
                {
                    synCustNos.Add(cust.FNumber);
                }
            }

            if (K3CustNos != null && K3CustNos.Count > 0)
            {
                existNos = K3CustNos.Intersect(synCustNos).ToList();
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
                //if (same != null && same.Count() > 0)
                //{
                //    lstSame = same.ToList<AbsSynchroDataInfo>();
                //    if (lstSame != null && lstSame.Count > 0)
                //    {
                //        dict.Add(SynOperationType.UPDATE, lstSame);
                //    }
                //}

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
                root.Add("Model", ConvertSynObjToJObj(sourceData, operationType));
                return root;
            }
            //数据更新时的Json格式
            else
            {
                //更新单据时，表体信息必须填写明细表体的主键
                K3CustomerInfo custData = sourceData as K3CustomerInfo;
                string sFCustId = string.Format(@"/*dialect*/ select FCUSTID from T_BD_CUSTOMER where FNumber = '{0}'", custData.FNumber);

                string sMainFCustId = string.Format(@"/*dialect*/ select FCUSTID from T_BD_CUSTOMER where FNumber = '{0}' and FUseOrgId != 1", custData.FNumber);
                int subFCustId = Convert.ToInt32(JsonUtils.ConvertObjectToString(SQLUtils.GetObject(this.K3CloudContext, sMainFCustId, "FCUSTID")));

                DynamicObjectCollection coll = SQLUtils.GetObjects(this.K3CloudContext, sFCustId);

                JArray model = new JArray();

                if (coll.Count > 0)
                {
                    foreach (var item in coll)
                    {
                        if (item["FCUSTID"] != null)
                        {
                            JObject baseData = ConvertSynObjToJObj(sourceData, operationType);
                            baseData.Add("FCUSTID", Convert.ToInt32(JsonUtils.ConvertObjectToString(item["FCUSTID"])));

                            if (subFCustId != Convert.ToInt32(JsonUtils.ConvertObjectToString(item["FCUSTID"])))
                            {
                                K3CustomerInfo soData = sourceData as K3CustomerInfo;
                                baseData = ConvertSynObjToJObj(soData, operationType);
                                baseData.Add("FCUSTID", Convert.ToInt32(JsonUtils.ConvertObjectToString(item["FCUSTID"])));
                            }
                            model.Add(baseData);
                        }
                    }
                }
                root.Add("Model", model);
                return root;
            }

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
            else if (operationType == SynOperationType.UPDATE)
            {
                //更新单据时，表体信息必须填写明细表体的主键
                root = new JObject();

                root.Add("NeedUpDateFields", new JArray(""));
                root.Add("NeedReturnFields", new JArray("FNumber"));
                root.Add("IsDeleteEntry", "false");
                root.Add("SubSystemId", "");
                root.Add("IsVerifyBaseDataField", "true");
                root.Add("BatchCount", batchCount);

                if (sourceDatas != null)
                {
                    if (sourceDatas.Count() > 0)
                    {
                        JArray model = new JArray();

                        for (int i = 0; i < sourceDatas.Count(); i++)
                        {
                            K3CustomerInfo custData = sourceDatas.ElementAt(i) as K3CustomerInfo;
                            string sFCustId = string.Format(@"/*dialect*/ select FCUSTID,FUseOrgId from T_BD_CUSTOMER where FNumber = '{0}'", custData.FNumber);
                            DynamicObjectCollection coll = SQLUtils.GetObjects(this.K3CloudContext, sFCustId);

                            if (coll.Count > 0)
                            {
                                foreach (var c in coll)
                                {
                                    if (c["FCUSTID"] != null)
                                    {
                                        JObject baseData = ConvertSynObjToJObj(sourceDatas.ElementAt(i), operationType);
                                        //主组织记录
                                        if (Convert.ToInt32(JsonUtils.ConvertObjectToString(c["FUseOrgId"])) == 1)
                                        {
                                            //主组织记录中的销售员和销售部门不能被更新
                                            baseData.Add("FCUSTID", Convert.ToInt32(JsonUtils.ConvertObjectToString(c["FCUSTID"])));
                                        }
                                        else
                                        {
                                            baseData.Add("FCUSTID", Convert.ToInt32(JsonUtils.ConvertObjectToString(c["FCUSTID"])));
                                            JObject FSELLER = new JObject();
                                            FSELLER.Add("FNumber", custData.FSELLER);
                                            baseData.Add("FSELLER", FSELLER);

                                            JObject FSALDEPTID = new JObject();
                                            FSALDEPTID.Add("FNumber", custData.FSALDEPTID);
                                            baseData.Add("FSALDEPTID", FSALDEPTID);
                                        }
                                        model.Add(baseData);
                                    }
                                }
                            }

                        }

                        root.Add("Model", model);
                        return root;
                    }

                }

            }
            return null;
        }

        /// <summary>
        /// 客户分配完后所做的操作，更新--提交--审核
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="sourceDatas"></param>
        /// <param name="operationType"></param>
        /// <returns></returns>
        public JObject BuildSyschroDataJsonForAfterAllot(Context ctx, List<AbsSynchroDataInfo> sourceDatas, SynOperationType operationType)
        {
            JObject root = null;

            if (operationType == SynOperationType.SAVE)
            {
                root = new JObject();
                root.Add("NeedUpDateFields", new JArray(""));
                root.Add("NeedReturnFields", new JArray("FNumber"));
                root.Add("IsDeleteEntry", "false");
                root.Add("SubSystemId", "");
                root.Add("IsVerifyBaseDataField", "true");


                root.Add("Model", ConvertSynObjToJObj(sourceDatas, operationType));
                return root;
            }
            //数据更新时的Json格式
            else if (operationType == SynOperationType.UPDATE)
            {
                //更新单据时，表体信息必须填写明细表体的主键
                root = new JObject();

                root.Add("NeedUpDateFields", new JArray(""));
                root.Add("NeedReturnFields", new JArray("FNumber"));
                root.Add("IsDeleteEntry", "false");
                root.Add("SubSystemId", "");
                root.Add("IsVerifyBaseDataField", "true");

                if (sourceDatas != null)
                {
                    if (sourceDatas.Count > 0)
                    {
                        JArray model = new JArray();

                        for (int i = 0; i < sourceDatas.Count; i++)
                        {
                            K3CustomerInfo custData = sourceDatas[i] as K3CustomerInfo;
                            string sFCustId = string.Format(@"/*dialect*/ select FCUSTID,FUseOrgId from T_BD_CUSTOMER where FNumber = '{0}' and FUseOrgId = {1}", custData.FNumber, ORGID);
                            DynamicObjectCollection coll = SQLUtils.GetObjects(this.K3CloudContext, sFCustId);

                            if (coll.Count > 0)
                            {
                                foreach (var c in coll)
                                {
                                    if (c["FCUSTID"] != null)
                                    {
                                        JObject baseData = ConvertSynObjToJObj(sourceDatas[i], operationType);

                                        baseData.Add("FCUSTID", Convert.ToInt32(JsonUtils.ConvertObjectToString(c["FCUSTID"])));

                                        JObject FSELLER = new JObject();
                                        FSELLER.Add("FNumber", string.IsNullOrEmpty(custData.FSELLER) ? "NA" : custData.FSELLER);
                                        baseData.Add("FSELLER", FSELLER);

                                        JObject FSALDEPTID = new JObject();
                                        FSALDEPTID.Add("FNumber", string.IsNullOrEmpty(custData.FSALDEPTID) ? "BM000001" : custData.FSALDEPTID);
                                        baseData.Add("FSALDEPTID", FSALDEPTID);

                                        model.Add(baseData);
                                    }
                                }
                            }

                        }

                        root.Add("Model", model);
                        return root;
                    }

                }

            }
            return null;
        }

        /// <summary>
        /// 将抽象类转换为具体的客户对象
        /// </summary>
        /// <param name="sourceDatas"></param>
        /// <returns></returns>
        public List<K3CustomerInfo> ConvertAbsSynchroObject(IEnumerable<AbsSynchroDataInfo> sourceDatas)
        {
            if (sourceDatas != null)
            {
                if (sourceDatas.Count() > 0)
                {
                    List<K3CustomerInfo> customers = new List<K3CustomerInfo>();

                    foreach (var item in sourceDatas)
                    {
                        if (item != null)
                        {
                            K3CustomerInfo customer = item as K3CustomerInfo;
                            customers.Add(customer);
                        }
                    }

                    return customers;
                }
            }
            return null;
        }
        /// <summary>
        ///子类转化为父类
        /// </summary>
        /// <param name="custs"></param>
        /// <returns></returns>
        public List<AbsSynchroDataInfo> ConvertSynchroObjectAbs(List<K3CustomerInfo> custs)
        {
            if (custs != null)
            {
                if (custs.Count > 0)
                {
                    List<AbsSynchroDataInfo> absInfos = new List<AbsSynchroDataInfo>();

                    foreach (var item in custs)
                    {
                        if (item != null)
                        {
                            AbsSynchroDataInfo absInfo = item as AbsSynchroDataInfo;
                            absInfos.Add(absInfo);
                        }
                    }

                    return absInfos;
                }
            }
            return null;
        }

        /// <summary>
        /// 将需要同步的客户转换为JSON格式对象（单个客户）
        /// </summary>
        /// <param name="sourceData"></param>
        /// <param name="opreationType"></param>
        /// <returns></returns>
        private JObject ConvertSynObjToJObj(AbsSynchroDataInfo sourceData, SynOperationType opreationType)
        {
            K3CustomerInfo custData = sourceData as K3CustomerInfo;
            JObject baseData = default(JObject);

            if (custData != null)
            {

                baseData = new JObject();
                //创建组织为必填项
                JObject FCreateOrgId = new JObject();
                FCreateOrgId.Add("FNumber", custData.FCreateOrgId);
                baseData.Add("FCreateOrgId", FCreateOrgId);

                if (opreationType == SynOperationType.SAVE)
                {
                    baseData.Add("FNumber", custData.FNumber);
                    //组织内编码不允许变更
                    JObject FUseOrgId = new JObject();
                    FUseOrgId.Add("FNumber", custData.FUseOrgId);
                    baseData.Add("FUseOrgId", FUseOrgId);
                }

                baseData.Add("FName", custData.FName);//客户名称
                baseData.Add("FShortName", custData.FShortName);

                JObject FCOUNTRY = new JObject();//国家
                FCOUNTRY.Add("FNumber", custData.FFCOUNTRY);
                baseData.Add("FCOUNTRY", FCOUNTRY);

                baseData.Add("FADDRESS", custData.FAddress);//通讯地址
                baseData.Add("FZIP", custData.FZIP);
                baseData.Add("FTEL", custData.FTEL);

                JObject FGroup = new JObject();//客户分组
                FGroup.Add("FNumber", custData.FGroup);
                baseData.Add("FGroup", FGroup);

                JObject FCustTypeId = new JObject();//客户类别
                FCustTypeId.Add("FNumber", custData.FCustTypeId);
                baseData.Add("FCustTypeId", FCustTypeId);

                JObject FTRADINGCURRID = new JObject();//结算币别
                FTRADINGCURRID.Add("FNumber", custData.FTRADINGCURRID);
                baseData.Add("FTRADINGCURRID", FTRADINGCURRID);

                baseData.Add("FInvoiceType", custData.FInvoiceType);//发票类型

                JObject FTaxType = new JObject();//税率类别
                FTaxType.Add("FNumber", custData.FTaxType);
                baseData.Add("FTaxType", FTaxType);

                JObject FTaxRate = new JObject();//默认税率
                FTaxRate.Add("FNumber", custData.FTaxRate);
                baseData.Add("FTaxRate", FTaxRate);

                //baseData.Add("FPriority", custData.FPriority);//客户优先级
                baseData.Add("FIsTrade", custData.FIsTrade);

                baseData.Add("FISCREDITCHECK", false);
                baseData.Add("F_HS_CustomerRegisteredMail", custData.F_HS_CustomerRegisteredMail);

            }

            return baseData;
        }

        private JArray ConvertSynObjToJObj(IEnumerable<AbsSynchroDataInfo> sourceDatas, SynOperationType opreationType)
        {

            JArray model = new JArray();
            JObject baseData = default(JObject);

            if (sourceDatas != null)
            {
                foreach (var custData in sourceDatas)
                {
                    baseData = ConvertSynObjToJObj(custData, opreationType);
                    model.Add(baseData);
                }
            }

            return model;
        }

        /// <summary>
        /// 同步通过Excel导入数据库的客户资料
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static List<K3CustomerInfo> DownLoadCustomersByExcel(Context ctx)
        {
            string sql = string.Format(@"/*dialect*/  select * from customers a where not exists(select FNUMBER from T_BD_CUSTOMER where FNUMBER = a.customers_id) and a.customers_email_address is not null ");
            DynamicObjectCollection coll = DBServiceHelper.ExecuteDynamicObject(ctx, sql);
            List<K3CustomerInfo> custs = BuildSynObjByCollection(ctx, coll);

            return custs;

        }

        /// <summary>
        /// 数据库查询的结果集封装成List<K3CustomerInfo>对象
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="coll"></param>
        /// <returns></returns>
        public static List<K3CustomerInfo> BuildSynObjByCollection(Context ctx, DynamicObjectCollection coll)
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
                        if (item["customers_email_address"] != null)
                        {
                            cust = new K3CustomerInfo();

                            cust.FCreateOrgId = "100";
                            cust.FUseOrgId = "100";

                            cust.FNumber = JsonUtils.ConvertObjectToString(item["customers_id"]);
                            cust.FShortName = JsonUtils.ConvertObjectToString(item["customers_firstname"]);
                            cust.FName = JsonUtils.ConvertObjectToString(item["customers_firstname"]) + " " +
                                         JsonUtils.ConvertObjectToString(item["customers_lastname"]);

                            if (string.IsNullOrWhiteSpace(cust.FName))
                            {
                                cust.FName = "None Name";
                            }

                            string sConutry = string.Format(@"/*dialect*/ select FNUMBER from VW_BAS_ASSISTANTDATA_CountryName
                                                                  where CountryName = '{0}'", JsonUtils.ConvertObjectToString(item["countries_iso_code_2"]));

                            cust.FFCOUNTRY = JsonUtils.ConvertObjectToString(item["countries_iso_code_2"]).CompareTo("NULL") == 0 ? "US" : JsonUtils.ConvertObjectToString(item["countries_iso_code_2"]);

                            string sProvincial = string.Format(@"/*dialect*/ select a.FNUMBER,b.FDATAVALUE from T_ECC_LOGISTICSAREADETAIL a
                                                                     inner join T_ECC_LOGISTICSAREADETAIL_L b
                                                                     on a.FDETAILID = b.FDETAILID
                                                                     where b.FDATAVALUE like N'%{0}%'", JsonUtils.ConvertObjectToString(item["entry_state"]));

                            cust.FAddress = JsonUtils.ConvertObjectToString(item["entry_street_address"]) + " "
                                            + JsonUtils.ConvertObjectToString(item["entry_city"]) + " "
                                             + JsonUtils.ConvertObjectToString(item["entry_state"]);

                            cust.FZIP = JsonUtils.ConvertObjectToString(item["entry_postcode"]);

                            cust.FTEL = JsonUtils.ConvertObjectToString(item["customers_telephone"]);
                            cust.F_HS_CustomerRegisteredMail = JsonUtils.ConvertObjectToString(item["customers_email_address"]);

                            string custLevel = JsonUtils.ConvertObjectToString(item["customers_whole"]);

                            cust.FGroup = ServiceHelper.SetCustomerLevel(custLevel);
                            cust.FCustTypeId = ServiceHelper.SetCustomerLevel(custLevel);

                            cust.FTRADINGCURRID = "USD";

                            string sellerNo = JsonUtils.ConvertObjectToString(item["account_manager_id"]);

                            if (!string.IsNullOrEmpty(sellerNo))
                            {
                                if (sellerNo.Contains("X"))
                                {
                                    sellerNo = sellerNo.Replace("X", "");
                                }
                                else if (sellerNo.Contains("x"))
                                {
                                    sellerNo = sellerNo.Replace("x", "");
                                }
                            }
                            sellerNo = string.IsNullOrEmpty(sellerNo) ? "NA" : sellerNo;
                            cust.FSELLER = sellerNo;

                            string sDeptNo = string.Format(@"/*dialect*/   select b.FNUMBER from T_BD_STAFFTEMP	a 
                                            inner join T_BD_DEPARTMENT b on a.FDEPTID=b.FDEPTID
                                            inner join T_BD_STAFF c on a.FSTAFFID=c.FSTAFFID
                                            where c.FNUMBER='{0}'", cust.FSELLER);

                            cust.FSALDEPTID = JsonUtils.ConvertObjectToString(SQLUtils.GetObject(ctx, sDeptNo, "FNUMBER"));


                            cust.FTaxType = "SFL02_SYS";
                            cust.FTaxRate = "SL04_SYS";


                            cust.FPriority = "1";
                            cust.FIsTrade = true;

                            custs.Add(cust);
                        }
                    }
                }
            }
            return custs;
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

            List<K3CustomerInfo> custs = ConvertAbsSynchroObject(sourceDatas);
            JObject bizData = BuildSynchroDataJsons(sourceDatas, operationType);

            List<string> numbers = null;
            List<int> pkIds = null;

            try
            {
                if (operationType == SynOperationType.SAVE)
                {
                    #region
                    try
                    {

                        #region
                        using (trans = new KDTransactionScope(System.Transactions.TransactionScopeOption.Required))
                        {
                            //单据保存 
                            result = ExecuteOperate(SynOperationType.SAVE,null,null,bizData.ToString());

                            if (result != null && result.Success == true)
                            {
                                //保存操作成功后返回单据的编码集合
                                if (result.GetNeedReturnValues("FNumber").ConvertAll(obj => string.Format("{0}", obj)) != null)
                                {
                                    numbers = result.GetNeedReturnValues("FNumber").ConvertAll(obj => string.Format("{0}", obj));
                                }
                                //保存操作成功后返回的单据内码集合
                                if (result.GetNeedReturnValues("FCUSTID").Select(obj => Convert.ToInt32(obj)).ToList() != null)
                                {
                                    pkIds = result.GetNeedReturnValues("FCUSTID").Select(obj => Convert.ToInt32(obj)).ToList();
                                }
                            }
                            ////单据提交
                            //result = InvokeWebApi.InvokeBatchSubmit(this.K3CloudContext, this.DataType, FormKey, numbers);
                            ////单据审核
                            //result = InvokeWebApi.InvokeBatchAudit(this.K3CloudContext, this.DataType, FormKey, numbers);
                            ////单据分配
                            //result = InvokeWebApi.InvokeBatchAllot(this.K3CloudContext, this.DataType, FormKey, pkIds);
                            ////分配后更新提交审核
                            //result = AfterAllot(this.K3CloudContext, custs);
                            ////提交事务
                            trans.Complete();

                        }
                    }

                        #endregion

                    #region
                    ////单据保存 
                    //result = InvokeWebApi.InvokeBatchSave(this.K3CloudContext, this.DataType, FormKey, bizData.ToString());

                            //if (result != null && result.Success == true)
                    //{
                    //    //保存操作成功后返回单据的编码集合
                    //    if (result.ReturnValues("FNumber").ConvertAll(obj => string.Format("{0}", obj)) != null)
                    //    {
                    //        numbers = result.ReturnValues("FNumber").ConvertAll(obj => string.Format("{0}", obj));
                    //    }
                    //    //保存操作成功后返回的单据内码集合
                    //    if (result.ReturnValues("FCUSTID").Select(obj => Convert.ToInt32(obj)).ToList() != null)
                    //    {
                    //        pkIds = result.ReturnValues("FCUSTID").Select(obj => Convert.ToInt32(obj)).ToList();
                    //    }
                    //}
                    ////单据提交
                    //result = InvokeWebApi.InvokeBatchSumbit(this.K3CloudContext, this.DataType, FormKey, numbers);

                            ////单据审核
                    //result = InvokeWebApi.InvokeBatchAudit(this.K3CloudContext, this.DataType, FormKey, numbers);


                            ////单据分配
                    ////result = InvokeWebApi.InvokeBatchAllot(this.K3CloudContext, this.DataType, FormKey, pkIds);

                            ////分配后更新提交审核
                    ////result = AfterAllot(this.K3CloudContext, custs);

                            //trans.Complete();

                    #endregion

                    catch (Exception ex)
                    {
                        LogUtils.WriteSynchroLog(this.K3CloudContext, this.DataType, "提交事务出现异常，异常信息：" + ex.Message + System.Environment.NewLine + ex.StackTrace);
                    }
                    finally
                    {
                        if (trans != null)
                        {
                            trans.Dispose();
                        }
                    }

                    #endregion

                }
                else if (operationType == SynOperationType.ALLOT)
                {
                    //单据分配
                    try
                    {
                        if (custs != null && custs.Count > 0)
                        {
                            AfterAllot(this.K3CloudContext, custs);
                        }

                    }
                    catch (Exception ex)
                    {

                        LogUtils.WriteSynchroLog(this.K3CloudContext, this.DataType, "分配操作出现异常，异常信息：" + ex.Message + System.Environment.NewLine + ex.StackTrace);
                    }
                }

                else if (operationType == SynOperationType.UPDATE)
                {
                    //单据更新
                    //result = InvokeWebApi.InvokeBatchSave(this.K3CloudContext, this.DataType, FormKey, bizData.ToString());
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
        public HttpResponseResult AfterAllot(Context ctx, List<K3CustomerInfo> custs)
        {

            HttpResponseResult result = null;
            List<int> pkIds = new List<int>();
            List<string> numbers = new List<string>();

            if (custs != null && custs.Count > 0)
            {
                numbers = new List<string>();

                foreach (var cust in custs)
                {
                    numbers.Add(cust.FNumber);
                    pkIds.Add(cust.FCUSTID);
                }
            }

            #region

            if (custs != null)
            {
                string FNumber = "";

                //分配成功后单据编码集合,做为查询从组织的内码的条件
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
                }

                //根据单据编码集合查询出从组织的内码集合
                string sFCustId = string.Format(@"/*dialect*/ select FCUSTID,FNUMBER from T_BD_CUSTOMER where FNumber in ({0}) and FUSEORGID = {1}", FNumber, ORGID);
                DynamicObjectCollection items = SQLUtils.GetObjects(ctx, sFCustId);
                List<AbsSynchroDataInfo> datas = new List<AbsSynchroDataInfo>();

                if (items != null && items.Count > 0)
                {
                    if (pkIds != null && pkIds.Count > 0)
                    {
                        pkIds.Clear();
                        foreach (var item in items)
                        {
                            pkIds.Add(Convert.ToInt32(JsonUtils.ConvertObjectToString(item["FCUSTID"])));
                        }
                    }


                    for (int i = 0; i < items.Count; i++)
                    {
                        if (JsonUtils.ConvertObjectToString(items[i]["FNUMBER"]).CompareTo(custs[i].FNumber) == 0)
                        {
                            custs[i].FCUSTID = Convert.ToInt32(JsonUtils.ConvertObjectToString(items[i]["FCUSTID"]));
                            AbsSynchroDataInfo data = custs[i] as AbsSynchroDataInfo;
                            datas.Add(data);
                        }
                    }

                    string json = BuildSyschroDataJsonForAfterAllot(ctx, datas, SynOperationType.UPDATE).ToString();
                    //更新单据
                    //result = InvokeWebApi.InvokeBatchSave(ctx, this.DataType, this.FormKey, json);

                    //numbers.Clear();
                    //if (result.Success == true)
                    //{
                    //    //单据提交
                    //    result = InvokeWebApi.InvokeBatchSubmit(ctx, this.DataType, FormKey, numbers, pkIds);

                    //    if (result != null && result.Success == true)
                    //    {
                    //        //单据审核
                    //        result = InvokeWebApi.InvokeBatchAudit(ctx, this.DataType, FormKey, numbers, pkIds);
                    //        pkIds.Clear();
                    //    }
                    //}

                }

            }

            #endregion
            return result;
        }

        public HttpResponseResult AfterAllot(Context ctx)
        {
            HttpResponseResult result = null;
            List<K3CustomerInfo> custs = DownLoadCustomersByExcel(ctx);

            List<K3CustomerInfo> lstBatch = null;
            try
            {
                #region
                if (custs != null && custs.Count > 0)
                {
                    lstBatch = new List<K3CustomerInfo>();
                    if (custs.Count >= 500)
                    {
                        #region
                        for (int i = 0; i < custs.Count; i++)
                        {
                            lstBatch.Add(custs[i]);
                            if (i > 0 && (i + 1) % 500 == 0)
                            {
                                if (lstBatch != null && lstBatch.Count > 0)
                                {
                                    //result = AfterAllot(ctx, lstBatch);
                                    lstBatch.Clear();
                                }
                            }
                            else
                            {
                                if (i == custs.Count - 1)
                                {
                                    if (lstBatch != null && lstBatch.Count > 0)
                                    {
                                        //result = AfterAllot(ctx, lstBatch);
                                        lstBatch.Clear();
                                    }
                                }

                            }
                        }
                        #endregion
                    }
                    else
                    {
                        //result = AfterAllot(ctx, custs);
                        lstBatch.Clear();
                    }

                }
                #endregion
            }
            catch (Exception ex)
            {
                LogUtils.WriteSynchroLog(this.K3CloudContext, this.DataType, "分配操作出现异常，异常信息：" + ex.Message + System.Environment.NewLine + ex.StackTrace);
                AfterAllot(ctx);

            }

            return result;

        }

        public override AbsSynchroDataInfo BuildSynchroData(Context ctx, string json, AbsSynchroDataInfo data = null)
        {
            throw new NotImplementedException();
        }
    }
}
