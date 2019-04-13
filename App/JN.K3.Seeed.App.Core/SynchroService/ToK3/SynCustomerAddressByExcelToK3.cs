using Kingdee.BOS;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

using Hands.K3.SCM.APP.Entity.StructType;
using Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject;
using Hands.K3.SCM.APP.Entity.SynDataObject;
using Hands.K3.SCM.APP.Entity.CommonObject;
using Hands.K3.SCM.APP.Entity.K3WebApi;
using Hands.K3.SCM.APP.Utils.Utils;
using Hands.K3.SCM.App.Synchro.Base.Abstract;
using Hands.K3.SCM.App.Synchro.Utils.SynchroService;
using HS.K3.Common.Abbott;
using Hands.K3.SCM.APP.Entity.EnumType;

namespace Hands.K3.SCM.App.Core.SynchroService.ToK3
{
    public class SynCustomerAddressByExcelToK3 : AbstractSynchroToK3
    {
        /// <summary>
        /// 同步客户地址信息
        /// </summary>

        public const int ORGID = 100035;
        /// <summary>
        /// 数据类型
        /// </summary>
        override public SynchroDataType DataType
        {
            get
            {
                return SynchroDataType.CustomerAddressByExcel;
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
        /// <param name="dbId"></param>
        /// <param name="numbers"></param>
        /// <returns></returns>
        public override IEnumerable<AbsSynchroDataInfo> GetSynchroData(IEnumerable<string> numbers = null)
        {
            return ServiceHelper.GetSynchroDatas(this.K3CloudContext,this.DataType,this.RedisDbId,numbers,this.Direction);
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
        /// <param name="srcDatas"></param>
        /// <returns></returns>
        public override Dictionary<SynOperationType, IEnumerable<AbsSynchroDataInfo>> EntityDataSource(Context ctx, IEnumerable<AbsSynchroDataInfo> srcDatas)
        {
            Dictionary<SynOperationType, IEnumerable<AbsSynchroDataInfo>> dict = new Dictionary<SynOperationType, IEnumerable<AbsSynchroDataInfo>>();
            dict.Add(SynOperationType.UPDATE, srcDatas);

            return dict;
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
                //root.Add("BatchCount", batchCount);

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
                                            //baseData.Add("FT_BD_CUSTCONTACT", BuildK3CustContactJsons(custData));//客户地址信息
                                        }
                                        //else
                                        //{
                                        //    baseData.Add("FCUSTID", Convert.ToInt32(JsonUtils.ConvertJObjectToString(c["FCUSTID"])));
                                        //    JObject FSELLER = new JObject();
                                        //    FSELLER.Add("FNumber", custData.FSELLER);
                                        //    baseData.Add("FSELLER", FSELLER);

                                        //    JObject FSALDEPTID = new JObject();
                                        //    FSALDEPTID.Add("FNumber", custData.FSALDEPTID);
                                        //    baseData.Add("FSALDEPTID", FSALDEPTID);
                                        //}
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
        ///子类转换为父类
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
            K3CustomerInfo cust = sourceData as K3CustomerInfo;
            JObject baseData = default(JObject);

            if (cust != null)
            {
                baseData = new JObject();
                //创建组织为必填项
                JObject FCreateOrgId = new JObject();
                FCreateOrgId.Add("FNumber", cust.FCreateOrgId);
                baseData.Add("FCreateOrgId", FCreateOrgId);

                if (opreationType == SynOperationType.SAVE)
                {
                    baseData.Add("FNumber", cust.FNumber);
                    //组织内编码不允许变更
                    JObject FUseOrgId = new JObject();
                    FUseOrgId.Add("FNumber", cust.FUseOrgId);
                    baseData.Add("FUseOrgId", FUseOrgId);
                }

                if (opreationType == SynOperationType.UPDATE)
                {
                    baseData.Add("FT_BD_CUSTCONTACT", BuildK3CustContactJsons(cust));//客户地址信息
                }
                baseData.Add("FName", cust.FName);//客户名称
                baseData.Add("FShortName", cust.FShortName);

                JObject FCOUNTRY = new JObject();//国家
                FCOUNTRY.Add("FNumber", cust.FFCOUNTRY);
                baseData.Add("FCOUNTRY", FCOUNTRY);

                baseData.Add("FADDRESS", cust.FAddress);//通讯地址
                baseData.Add("FZIP", cust.FZIP);
                baseData.Add("FTEL", cust.FTEL);

                //JObject FGroup = new JObject();//客户分组
                //FGroup.Add("FNumber", cust.FGroup);
                //baseData.Add("FGroup", FGroup);

                JObject FCustTypeId = new JObject();//客户类别
                FCustTypeId.Add("FNumber", cust.FCustTypeId);
                baseData.Add("FCustTypeId", FCustTypeId);

                JObject FTRADINGCURRID = new JObject();//结算币别
                FTRADINGCURRID.Add("FNumber", cust.FTRADINGCURRID);
                baseData.Add("FTRADINGCURRID", FTRADINGCURRID);

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

            }

            return baseData;
        }

        /// <summary>
        /// 客户地址信息
        /// </summary>
        /// <param name="cust"></param>
        /// <returns></returns>
        public JArray BuildK3CustContactJsons(K3CustomerInfo cust)
        {
            JArray FT_BD_CUSTCONTACT = null;
            JObject contactObj = null;

            if (cust != null)
            {
                FT_BD_CUSTCONTACT = new JArray();

                if (cust.lstCustCtaInfo != null && cust.lstCustCtaInfo.Count > 0)
                {
                    for (int i = 0; i < cust.lstCustCtaInfo.Count; i++)
                    {
                        contactObj = new JObject();

                        K3CustContactInfo contactData = cust.lstCustCtaInfo.ElementAt(i);
                        contactObj.Add("FNUMBER1", contactData.FNUMBER1);//地址编码
                        contactObj.Add("FNAME1", contactData.FNAME1);//地点名称
                        contactObj.Add("FADDRESS1", contactData.FADDRESS1);//详细地址
                        contactObj.Add("FTTel", contactData.FMOBILE);//移动电话
                        contactObj.Add("F_HS_DeliveryName", contactData.F_HS_DeliveryName);//交货联系人
                        contactObj.Add("F_HS_PostCode", contactData.F_HS_PostCode);//交货邮编
                        contactObj.Add("F_HS_DeliveryCity", contactData.F_HS_DeliveryCity);//交货城市
                        contactObj.Add("F_HS_DeliveryProvinces", contactData.F_HS_DeliveryProvinces);//交货省份

                        JObject F_HS_RecipientCountry = new JObject();//国家
                        F_HS_RecipientCountry.Add("FNumber", contactData.F_HS_RecipientCountry);
                        contactObj.Add("F_HS_RecipientCountry", F_HS_RecipientCountry);

                        FT_BD_CUSTCONTACT.Add(contactObj);
                    }

                }
            }

            return FT_BD_CUSTCONTACT;
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
                    baseData = ConvertSynObjToJObj(custData, opreationType);
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
                        cust = new K3CustomerInfo();

                        cust.FCreateOrgId = "100";
                        cust.FUseOrgId = "100";

                        cust.FNumber = JsonUtils.ConvertObjectToString(item["customers_id"]);
                        cust.FShortName = JsonUtils.ConvertObjectToString(item["customers_firstname"]);
                        cust.FName = JsonUtils.ConvertObjectToString(item["customers_firstname"]) + " "+
                                     JsonUtils.ConvertObjectToString(item["customers_lastname"]);

                        if (string.IsNullOrWhiteSpace(cust.FName))
                        {
                            cust.FName = "None Name";
                        }

                        string sConutry = string.Format(@"/*dialect*/ select FNUMBER from VW_BAS_ASSISTANTDATA_CountryName
                                                                  where CountryName = '{0}'", JsonUtils.ConvertObjectToString(item["delivery_country"]));

                        cust.FFCOUNTRY = JsonUtils.ConvertObjectToString(item["delivery_country"]).CompareTo("NULL") == 0 ? "US" : JsonUtils.ConvertObjectToString(item["delivery_country"]);

                        string sProvincial = string.Format(@"/*dialect*/ select a.FNUMBER,b.FDATAVALUE from T_ECC_LOGISTICSAREADETAIL a
                                                                     inner join T_ECC_LOGISTICSAREADETAIL_L b
                                                                     on a.FDETAILID = b.FDETAILID
                                                                     where b.FDATAVALUE like N'%{0}%'", JsonUtils.ConvertObjectToString(item["entry_state"]));

                        cust.FAddress = JsonUtils.ConvertObjectToString(item["entry_street_address"]) + " " +
                                        JsonUtils.ConvertObjectToString(item["entry_city"]) + " " +
                                        JsonUtils.ConvertObjectToString(item["entry_state"]) + " " +
                                        JsonUtils.ConvertObjectToString(item["delivery_country"]);
                        cust.FZIP = JsonUtils.ConvertObjectToString(item["entry_postcode"]);

                        cust.FTEL = JsonUtils.ConvertObjectToString(item["customers_telephone"]);

                        string custLevel = JsonUtils.ConvertObjectToString(item["customers_whole"]);

                        cust.FGroup = ServiceHelper.SetCustomerLevel(custLevel);
                        cust.FCustTypeId = ServiceHelper.SetCustomerLevel(custLevel);

                        cust.FTRADINGCURRID = "USD";


                        string sellerNo = JsonUtils.ConvertObjectToString(item["account_manager_id"]);
                        sellerNo = sellerNo.CompareTo("null") == 0 ? "" : sellerNo;
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
            return custs;
        }
        public static List<K3CustomerInfo> BuildSynObjByCollection_(Context ctx, DynamicObjectCollection coll)
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

                        cust.FCreateOrgId = "100";
                        cust.FUseOrgId = "100";

                        cust.FNumber = JsonUtils.ConvertObjectToString(item["FNumber"]);
                        cust.FShortName = JsonUtils.ConvertObjectToString(item["FShortName"]);
                        cust.FName = JsonUtils.ConvertObjectToString(item["FName"]);

                        if (string.IsNullOrWhiteSpace(cust.FName))
                        {
                            cust.FName = "None Name";
                        }

                        cust.FFCOUNTRY = JsonUtils.ConvertObjectToString(item["FCOUNTRY"]).CompareTo("NULL") == 0 ? "US" : JsonUtils.ConvertObjectToString(item["FCOUNTRY"]);
                        cust.FAddress = JsonUtils.ConvertObjectToString(item["FAddress"]);
                        cust.FZIP = JsonUtils.ConvertObjectToString(item["FZIP"]);
                        cust.FTEL = JsonUtils.ConvertObjectToString(item["FTEL"]);
                        cust.FSELLER = JsonUtils.ConvertObjectToString(item["FSELLER"]);
                        cust.FSALDEPTID = JsonUtils.ConvertObjectToString(item["FSALDEPTID"]);

                        cust.FTaxType = JsonUtils.ConvertObjectToString(item["FTaxType"]);
                        cust.FTaxRate = JsonUtils.ConvertObjectToString(item["FTaxRate"]);
                        cust.FPriority = JsonUtils.ConvertObjectToString(item["FPriority"]);
                        cust.FIsTrade = Convert.ToBoolean(Convert.ToInt32(JsonUtils.ConvertObjectToString(item["FIsTrade"])) == 1 ? true : false);

                        custs.Add(cust);
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
            List<K3CustomerInfo> custs = ConvertAbsSynchroObject(sourceDatas);
            JObject bizData = BuildSynchroDataJsons(sourceDatas, operationType);

            try
            {

                if (operationType == SynOperationType.UPDATE)
                {
                    //单据更新
                    result = ExecuteOperate(SynOperationType.SAVE,null,null,bizData.ToString());
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

        public override AbsSynchroDataInfo BuildSynchroData(Context ctx, string fNumber, AbsSynchroDataInfo data = null)
        {

            QueryBuilderParemeter para = new QueryBuilderParemeter();
            para.FormId = "BD_Customer";
            para.SelectItems = SelectorItemInfo.CreateItems("FCreateOrgId,FUseOrgId,FNumber,FShortName,FName,FCOUNTRY,FAddress,FZIP,FTEL,FSELLER,FSALDEPTID,FCustTypeId,FTRADINGCURRID,FPriority,FIsTrade,F_HS_CustomerRegisteredMail");

            if (!string.IsNullOrEmpty(fNumber))
            {
                para.FilterClauseWihtKey = " FNumber ='" + fNumber + "' and  FUseOrgId = 1";
                var k3Data = Kingdee.BOS.App.ServiceHelper.GetService<IQueryService>().GetDynamicObjectCollection(ctx, para);

                if (k3Data != null && k3Data.Count > 0)
                {
                    List<K3CustomerInfo> custs = BuildSynObjByCollection(ctx, k3Data);
                    return custs[0];
                }

            }
            return null;
        }

        /// <summary>
        /// 客户地址(数据源：execl)
        /// </summary>
        /// <returns></returns>
        public static List<K3CustContactInfo> GetK3CustContactInfoByExcel(Context ctx)
        {
            List<K3CustContactInfo> contacts = null;
            K3CustContactInfo contact = null;

            string sql = null;
            DynamicObjectCollection coll = null;

            sql = string.Format(@"/*dialect*/ select * from address_book  t 
                                                where countries_iso_code_2 != 'NULL'
                                                and  not exists (
					                                                select  distinct b.FNUMBER , a.FNUMBER  
					                                                from T_BD_CUSTLOCATION a
					                                                inner join T_BD_CUSTOMER b on a.FCUSTID = b.FCUSTID
					                                                where  b.FNUMBER = t.customers_id and a.FNUMBER = t.address_book_id
				                                                ) 
                                                  ");
            coll = SQLUtils.GetObjects(ctx, sql);

            if (coll != null && coll.Count > 0)
            {
                contacts = new List<K3CustContactInfo>();

                foreach (var item in coll)
                {
                    contact = new K3CustContactInfo();

                    contact.FCustNo = SQLUtils.GetFieldValue(item, "customers_id");
                    contact.FNUMBER1 = SQLUtils.GetFieldValue(item, "address_book_id");

                    contact.FNAME1 = SQLUtils.GetFieldValue(item, "entry_street_address") + " "
                                    + SQLUtils.GetFieldValue(item, "entry_city") + "  "
                                    + SQLUtils.GetFieldValue(item, "entry_state") + " "
                                    + SQLUtils.GetFieldValue(item, "countries_iso_code_2");

                    contact.FADDRESS1 = SQLUtils.GetFieldValue(item, "entry_street_address");
                    contact.FTTel = SQLUtils.GetFieldValue(item, "entry_telephone");

                    contact.F_HS_DeliveryName = SQLUtils.GetFieldValue(item, "entry_firstname") + " "
                                                + SQLUtils.GetFieldValue(item, "entry_lastname");

                    contact.F_HS_PostCode = SQLUtils.GetFieldValue(item, "entry_postcode");
                    contact.F_HS_DeliveryCity = SQLUtils.GetFieldValue(item, "entry_city");
                    contact.F_HS_DeliveryProvinces = SQLUtils.GetFieldValue(item, "entry_state");
                    contact.F_HS_RecipientCountry = SQLUtils.GetFieldValue(item, "countries_iso_code_2");

                    contacts.Add(contact);
                }
            }


            return contacts;

        }

        /// <summary>
        /// 根据编码获取客户信息
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="fNumber"></param>
        /// <returns></returns>
        public static K3CustomerInfo GetCustomerByNo(Context ctx, string fNumber)
        {
            QueryBuilderParemeter para = new QueryBuilderParemeter();
            para.FormId = "BD_Customer";
            para.SelectItems = SelectorItemInfo.CreateItems("FCreateOrgId,FUseOrgId,FNumber,FShortName,FName,FCOUNTRY,FAddress,FZIP,FTEL,FSELLER,FSALDEPTID,FCustTypeId,FTRADINGCURRID,FPriority,FIsTrade,F_HS_CustomerRegisteredMail");

            if (!string.IsNullOrEmpty(fNumber))
            {
                para.FilterClauseWihtKey = " FNumber ='" + fNumber + "' and  FUseOrgId = 1";
                var k3Data = Kingdee.BOS.App.ServiceHelper.GetService<IQueryService>().GetDynamicObjectCollection(ctx, para);

                if (k3Data != null && k3Data.Count > 0)
                {
                    List<K3CustomerInfo> custs = BuildSynObjByCollection(ctx, k3Data);

                    if (custs != null && custs.Count > 0)
                    {
                        return custs[0];
                    }                   
                }

            }
            return null;

        }

        /// <summary>
        /// 通过地址信息查询出客户，并赋值地址表至客户
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static List<K3CustomerInfo> GetK3CustomerInfoByExcelAddress(Context ctx)
        {
            List<K3CustomerInfo> custs = null;
            List<K3CustContactInfo> contacts = GetK3CustContactInfoByExcel(ctx);

            if (contacts != null && contacts.Count > 0)
            {
                var c = from l in contacts group l by l.FCustNo into g select g;

                if (c != null && c.Count() > 0)
                {
                    custs = new List<K3CustomerInfo>();

                    foreach (var item in c)
                    {
                        K3CustomerInfo cust = GetCustomerByNo(ctx, item.Key);

                        if (cust != null)
                        {
                            cust.lstCustCtaInfo = item.ToList<K3CustContactInfo>();
                            custs.Add(cust);
                        }
                    }
                }
            }

            return custs;
        }  
    }
}
