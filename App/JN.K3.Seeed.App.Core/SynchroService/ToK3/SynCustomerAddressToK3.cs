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
    /// <summary>
    /// 同步客户地址信息
    /// </summary>
    public class SynCustomerAddressToK3 : AbstractSynchroToK3
    {

        /// <summary>
        /// 数据类型
        /// </summary>
        override public SynchroDataType DataType
        {
            get
            {
                return SynchroDataType.CustomerAddress;
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
            //从Redis获取客户地址信息
            List<AbsSynchroDataInfo> synDatas = null;

            synDatas = ServiceHelper.GetSynchroDatas(this.K3CloudContext, this.DataType, this.RedisDbId, numbers,this.Direction);

            return synDatas;
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
        ///  筛选数据
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="srcDatas"></param>
        /// <returns></returns>
        public override Dictionary<SynOperationType, IEnumerable<AbsSynchroDataInfo>> EntityDataSource(Context ctx, IEnumerable<AbsSynchroDataInfo> srcDatas)
        {
            Dictionary<SynOperationType, IEnumerable<AbsSynchroDataInfo>> dict = null;

            List<K3CustomerInfo> custUpdate = null;
            List<K3CustomerInfo> custSave = null;

            List<K3CustContactInfo> addrUpdate = null;
            List<K3CustContactInfo> addrSave = null;

            if (srcDatas != null && srcDatas.Count() > 0)
            {
                List<K3CustomerInfo> custs = srcDatas.Select(c => (K3CustomerInfo)c).ToList();

                if (custs != null && custs.Count > 0)
                {
                    dict = new Dictionary<SynOperationType, IEnumerable<AbsSynchroDataInfo>>();
                    custUpdate = new List<K3CustomerInfo>();
                    custSave = new List<K3CustomerInfo>();

                    foreach (var cust in custs)
                    {
                        if (cust != null && cust.lstCustCtaInfo != null && cust.lstCustCtaInfo.Count > 0)
                        {
                            addrUpdate = new List<K3CustContactInfo>();
                            addrSave = new List<K3CustContactInfo>();

                            foreach (var cta in cust.lstCustCtaInfo)
                            {
                                if (CustAdrrIsExistInK3(ctx, cta))
                                {
                                    addrUpdate.Add(cta);
                                }
                                else
                                {
                                    addrSave.Add(cta);
                                }
                            }
                            if (addrUpdate.Count > 0)
                            {
                                cust.lstCustCtaInfo = addrUpdate;
                                custUpdate.Add(cust);
                            }
                            if (addrSave.Count > 0)
                            {
                                cust.lstCustCtaInfo = addrSave;
                                custSave.Add(cust);
                            }
                        }
                    }

                    if (custUpdate.Count > 0)
                    {
                        dict.Add(SynOperationType.UPDATE, custUpdate);
                    }
                    if (custSave.Count > 0)
                    {
                        dict.Add(SynOperationType.SAVE, custSave);
                    }
                }
            }

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

                DynamicObjectCollection coll = SQLUtils.GetObjects(this.K3CloudContext, sFCustId);

                JArray model = new JArray();

                if (coll.Count > 0)
                {
                    foreach (var item in coll)
                    {
                        if (item["FCUSTID"] != null)
                        {
                            JObject baseData = ConvertSynObjToJObj(sourceData, operationType);
                            baseData.Add("FCUSTID", Convert.ToInt32(SQLUtils.GetFieldValue(item, "FCUSTID")));

                            //if (subFCustId != Convert.ToInt32(JsonUtils.ConvertObjectToString(item["FCUSTID"])))
                            //{
                            //    K3CustomerInfo soData = sourceData as K3CustomerInfo;
                            //    baseData = ConvertSynObjToJObj(soData, operationType);
                            //    baseData.Add("FCUSTID", Convert.ToInt32(JsonUtils.ConvertObjectToString(item["FCUSTID"])));
                            //}
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
            JArray jArr = ConvertSynObjToJObj(sourceDatas, operationType);

          
            //数据更新时的Json格式
            if (operationType == SynOperationType.SAVE)
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
                                        if (Convert.ToInt32(SQLUtils.GetFieldValue(c, "FUseOrgId")) == 1)
                                        {
                                            baseData.Add("FCUSTID", Convert.ToInt32(SQLUtils.GetFieldValue(c, "FCUSTID")));
                                            model.Add(baseData);
                                        }
                                    }
                                }
                            }
                        }

                        if (model != null && model.Count > 0)
                        {
                            root.Add("Model", model);
                            return root;
                        }
                    }
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
                //组织内编码不允许变更
                JObject FUseOrgId = new JObject();
                FUseOrgId.Add("FNumber", cust.FUseOrgId);
                baseData.Add("FUseOrgId", FUseOrgId);

                baseData.Add("FNumber", cust.FNumber);


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

                baseData.Add("FT_BD_CUSTCONTACT", BuildK3CustContactJsons(cust));//客户地址信息
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
                        //contactObj.Add("FTTel", contactData.FTTel);//移动电话
                        contactObj.Add("FMOBILE", contactData.FMOBILE);
                        contactObj.Add("F_HS_DeliveryName", contactData.F_HS_DeliveryName);//交货联系人
                        contactObj.Add("F_HS_PostCode", contactData.F_HS_PostCode);//交货邮编
                        contactObj.Add("F_HS_DeliveryCity", contactData.F_HS_DeliveryCity);//交货城市
                        contactObj.Add("F_HS_DeliveryProvinces", contactData.F_HS_DeliveryProvinces);//交货省份

                        if (string.IsNullOrWhiteSpace(contactData.F_HS_RecipientCountry))
                        {
                            string errorInfo = "客户地址同步，客户编码为:[" + contactData.FCustNo + "]的信息国家编码为空！";
                            LogUtils.WriteSynchroLog(this.K3CloudContext, SynchroDataType.CustomerAddress, errorInfo);
                        }

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
        public List<K3CustomerInfo> BuildSynObjByCollection(Context ctx, DynamicObjectCollection coll)
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

                        cust.FNumber = SQLUtils.GetFieldValue(item, "FNumber");
                        cust.FShortName = SQLUtils.GetFieldValue(item, "FShortName");
                        cust.FName = SQLUtils.GetFieldValue(item, "FName");

                        if (string.IsNullOrWhiteSpace(cust.FName))
                        {
                            cust.FName = "None Name";
                        }

                        string sConutry = string.Format(@"/*dialect*/ select FNUMBER from VW_BAS_ASSISTANTDATA_CountryName
                                                                  where FCountry = '{0}'", SQLUtils.GetFieldValue(item, "FCOUNTRY"));

                        cust.FFCOUNTRY = JsonUtils.ConvertObjectToString(SQLUtils.GetObject(ctx, sConutry, "FNUMBER"));
                        cust.FAddress = SQLUtils.GetFieldValue(item, "FAddress");

                        cust.FZIP = SQLUtils.GetFieldValue(item, "FZIP");
                        cust.FTEL = SQLUtils.GetFieldValue(item, "FTEL");
                        cust.F_HS_CustomerRegisteredMail = SQLUtils.GetFieldValue(item, "F_HS_CustomerRegisteredMail");
                        cust.FCustTypeId = SQLUtils.GetCustTypeNo(ctx, item, "FCustTypeId");
                        cust.FTRADINGCURRID = SQLUtils.GetSettleCurrNo(ctx, item, "FTRADINGCURRID");

                        cust.FSELLER = string.IsNullOrWhiteSpace(SQLUtils.GetSellerNo(ctx, item, "FSELLER")) ? "NA" : SQLUtils.GetSellerNo(ctx, item, "FSELLER");

                        string sDeptNo = string.Format(@"/*dialect*/   select b.FNUMBER from T_BD_STAFFTEMP	a 
                                            inner join T_BD_DEPARTMENT b on a.FDEPTID=b.FDEPTID
                                            inner join T_BD_STAFF c on a.FSTAFFID=c.FSTAFFID
                                            where c.FNUMBER='{0}'", cust.FSELLER);

                        cust.FSALDEPTID = JsonUtils.ConvertObjectToString(SQLUtils.GetObject(ctx, sDeptNo, "FNUMBER"));
                        cust.F_HS_Grade = SQLUtils.GetFieldValue(item, "F_HS_Grade");
                        cust.F_HS_SpecialDemand = SQLUtils.GetFieldValue(item, "F_HS_SpecialDemand");
                        cust.F_HS_TaxNum = SQLUtils.GetFieldValue(item, "F_HS_TaxNum");
                        cust.FPRICELISTID = SQLUtils.GetFieldValue(item, "FPRICELISTID");
                        cust.F_HS_CustomerPurchaseMail = SQLUtils.GetFieldValue(item, "F_HS_CustomerPurchaseMail");

                        cust.FSETTLETYPEID = "JSFS01_SYS";
                        cust.FTaxType = "SZ01_SYS";
                        cust.FTaxRate = "SL04_SYS";

                        cust.FPriority = "1";
                        cust.FRECEIVECURRID = "PRE001";

                        cust.FISCREDITCHECK = false;
                        cust.FIsTrade = true;

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
            List<K3CustomerInfo> custs = null;

            JObject bizData = null;
            string msgs = string.Empty;

            if (sourceDatas != null && sourceDatas.Count() > 0)
            {
                custs = sourceDatas.Select(c => (K3CustomerInfo)c).ToList();
                bizData = BuildSynchroDataJsons(sourceDatas, operationType);
            }
            try
            {
                if (operationType == SynOperationType.SAVE)
                {
                    if (bizData == null)
                    {
                        return null;
                    }
                    else
                    {
                        if (bizData["Model"] == null)
                        {
                            return null;
                        }
                    }
                    //客户地址新增
                    result = ExecuteOperate(SynOperationType.SAVE, null, null, bizData.ToString());

                    //客户地址新增成功后返回的单据编码集合
                    if (result != null && result.Success)
                    {
                        RemoveRedisData(this.K3CloudContext, result.SuccessEntityNos);
                    }
                }
                else if (operationType == SynOperationType.UPDATE)
                {
                    List<SqlObject> sqlObjects = null;
                    List<SqlParam> sqlParams = null;
                    List<string> custNos = new List<string>();

                    int count = 0;

                    try
                    {
                        if (custs != null && custs.Count > 0)
                        {
                            custNos = new List<string>();

                            foreach (var cust in custs)
                            {
                                if (cust != null && cust.lstCustCtaInfo != null && cust.lstCustCtaInfo.Count > 0)
                                {
                                    foreach (var contact in cust.lstCustCtaInfo)
                                    {
                                        if (contact != null)
                                        {
                                            string sql = string.Format(@"/*dialect*/ update a 
                                                                            set FNAME = @FNAME,FADDRESS = @FADDRESS,FMOBILE = @FMOBILE,F_HS_POSTCODE = @F_HS_POSTCODE,
                                                                            F_HS_DELIVERYCITY = @F_HS_DELIVERYCITY,F_HS_DELIVERYPROVINCES = @F_HS_DELIVERYPROVINCES
                                                                            ,F_HS_RECIPIENTCOUNTRY = @F_HS_RECIPIENTCOUNTRY,F_HS_DELIVERYNAME = @F_HS_DELIVERYNAME
                                                                            from T_BD_CUSTLOCATION a
                                                                            inner join T_BD_CUSTOMER b
                                                                            on a.FCUSTID = b.FCUSTID
                                                                            where b.FNUMBER = @BFNUMBER 
                                                                            and a.FNUMBER = @AFNUMBER
                                                                            and FUSEORGID = 1");

                                            sqlObjects = new List<SqlObject>();
                                            sqlParams = new List<SqlParam>();

                                            sqlParams.Add(new SqlParam("@FNAME", KDDbType.String, SQLUtils.DealQuotes(contact.FNAME1)));
                                            sqlParams.Add(new SqlParam("@FADDRESS", KDDbType.String, SQLUtils.DealQuotes(contact.FADDRESS1)));
                                            sqlParams.Add(new SqlParam("@FMOBILE", KDDbType.String, SQLUtils.DealQuotes(contact.FMOBILE)));
                                            sqlParams.Add(new SqlParam("@F_HS_POSTCODE", KDDbType.String, SQLUtils.DealQuotes(contact.F_HS_PostCode)));
                                            sqlParams.Add(new SqlParam("@F_HS_DELIVERYCITY", KDDbType.String, SQLUtils.DealQuotes(contact.F_HS_DeliveryCity)));
                                            sqlParams.Add(new SqlParam("@F_HS_DELIVERYPROVINCES", KDDbType.String, SQLUtils.DealQuotes(contact.F_HS_DeliveryProvinces)));
                                            sqlParams.Add(new SqlParam("@F_HS_RECIPIENTCOUNTRY", KDDbType.String, SQLUtils.DealQuotes(SQLUtils.GetCountryId(this.K3CloudContext, contact.F_HS_RecipientCountry))));
                                            sqlParams.Add(new SqlParam("@F_HS_DELIVERYNAME", KDDbType.String, SQLUtils.DealQuotes(contact.F_HS_DeliveryName)));
                                            sqlParams.Add(new SqlParam("@BFNUMBER", KDDbType.String, contact.FCustNo));
                                            sqlParams.Add(new SqlParam("@AFNUMBER", KDDbType.String, contact.FNUMBER1));
                                            sqlObjects.Add(new SqlObject(sql, sqlParams));

                                            count = DBUtils.ExecuteBatch(this.K3CloudContext, sqlObjects);

                                            if (count > 0)
                                            {
                                                custNos.Add(contact.FCustNo);
                                                msgs += string.Format("客户[{0}]地址[{1}]更新成功！", contact.FCustNo, contact.FNUMBER1) + Environment.NewLine;
                                                LogUtils.WriteSynchroLog(this.K3CloudContext, SynchroDataType.CustomerAddress, string.Format("客户[{0}]地址[{1}]更新成功！", contact.FCustNo, contact.FNUMBER1));
                                            }
                                        }
                                    }
                                }
                            }

                            if (!string.IsNullOrWhiteSpace(msgs))
                            {
                                result = new HttpResponseResult();
                                result.Success = true;
                                result.SuccessEntityNos = custNos;
                                result.Message = msgs;

                                RemoveRedisData(this.K3CloudContext, custNos);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        result = new HttpResponseResult();
                        result.Success = false;
                        result.Message = "客户地址更新出现异常：" + ex.Message + Environment.NewLine + ex.StackTrace;
                        LogUtils.WriteSynchroLog(this.K3CloudContext, SynchroDataType.CustomerAddress, "客户地址更新出现异常：" + result.Message);
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

        public override AbsSynchroDataInfo BuildSynchroData(Context ctx, string json, AbsSynchroDataInfo data = null)
        {
            JArray jArr = JArray.Parse(json);

            List<K3CustContactInfo> contacts = null;
            K3CustContactInfo contact = null;

            if (jArr != null && jArr.Count > 0)
            {
                contacts = new List<K3CustContactInfo>();

                for (int i = 0; i < jArr.Count; i++)
                {
                    JObject jObj = jArr[i] as JObject;
                    contact = new K3CustContactInfo();

                    contact.FCustNo = JsonUtils.GetFieldValue(jObj, "customers_id");
                    contact.FNUMBER1 = JsonUtils.GetFieldValue(jObj, "address_book_id");

                    contact.FNAME1 = JsonUtils.GetFieldValue(jObj, "entry_street_address") + " "
                                    + JsonUtils.GetFieldValue(jObj, "entry_city") + "  "
                                    + JsonUtils.GetFieldValue(jObj, "entry_state") + " "
                                    + JsonUtils.GetFieldValue(jObj, "countries_iso_code_2");

                    contact.FADDRESS1 = JsonUtils.GetFieldValue(jObj, "entry_street_address") + System.Environment.NewLine
                                        + JsonUtils.GetFieldValue(jObj, "entry_suburb");
                    //contact.FTTel = JsonUtils.GetFieldValue(jObj, "entry_telephone");
                    contact.FMOBILE = JsonUtils.GetFieldValue(jObj, "entry_telephone");
                    contact.F_HS_DeliveryName = JsonUtils.GetFieldValue(jObj, "entry_firstname") + " "
                                                + JsonUtils.GetFieldValue(jObj, "entry_lastname");

                    contact.F_HS_PostCode = JsonUtils.GetFieldValue(jObj, "entry_postcode");
                    contact.F_HS_DeliveryCity = JsonUtils.GetFieldValue(jObj, "entry_city");
                    contact.F_HS_DeliveryProvinces = JsonUtils.GetFieldValue(jObj, "entry_state");
                    contact.F_HS_RecipientCountry = JsonUtils.GetFieldValue(jObj, "countries_iso_code_2");

                    if (string.IsNullOrWhiteSpace(contact.F_HS_RecipientCountry))
                    {
                        string errorInfo = "客户地址同步，客户编码为:[" + contact.FCustNo + "]的信息国家编码为空！";
                        LogUtils.WriteSynchroLog(ctx, SynchroDataType.CustomerAddress, errorInfo);
                    }

                    contacts.Add(contact);

                }
            }

            if (contacts != null && contacts.Count > 0)
            {
                K3CustomerInfo k3Cust = GetCustomerByNo(ctx, contacts.ElementAt(0).FCustNo);
                if (k3Cust != null)
                {
                    if (string.IsNullOrWhiteSpace(k3Cust.FFCOUNTRY))
                    {
                        k3Cust.FFCOUNTRY = contacts.Select(c => c.F_HS_RecipientCountry).ToList().ElementAt(0);
                    }
                    k3Cust.lstCustCtaInfo = contacts;
                    return k3Cust;
                }
                else
                {
                    k3Cust = new K3CustomerInfo();
                    k3Cust.lstCustCtaInfo = contacts;
                    return k3Cust;
                }
            }
            return null;
        }

        /// <summary>
        /// 验证客户地址是否已经存在
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="contact"></param>
        /// <returns></returns>
        public bool CustAdrrIsExistInK3(Context ctx, K3CustContactInfo contact)
        {
            string sql = string.Empty;

            try
            {
                if (contact != null)
                {

                    sql = string.Format(@"/*dialect*/ select FNUMBER from T_BD_CUSTLOCATION where FNUMBER = '{0}'", contact.FNUMBER1);
                    DynamicObjectCollection coll = SQLUtils.GetObjects(ctx, sql);

                    if (coll != null)
                    {
                        if (coll.Count >= 2)
                        {
                            return true;
                        }
                        else
                        {
                            if (coll.Count == 1)
                            {
                                sql = string.Format(@"/*dialect*/ delete from T_BD_CUSTLOCATION where FNUMBER = '{0}'", contact.FNUMBER1);
                                int count = DBUtils.Execute(ctx, sql);

                                if (count > 0)
                                {
                                    return false;
                                }
                            }
                            else if (coll.Count == 0)
                            {
                                return false;
                            }
                        }
                    }

                    return false;

                }
            }
            catch (Exception ex)
            {
                LogUtils.WriteSynchroLog(ctx, SynchroDataType.CustomerAddress, "客户地址同步失败：" + System.Environment.NewLine + ex.Message + System.Environment.NewLine + ex.StackTrace);
            }

            return false;
        }

        /// <summary>
        /// 根据编码获取客户信息
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="fNumber"></param>
        /// <returns></returns>
        private K3CustomerInfo GetCustomerByNo(Context ctx, string fNumber)
        {
            if (GetCustomerByNo(fNumber) != null)
            {
                return GetCustomerByNo(fNumber);
            }

            QueryBuilderParemeter para = new QueryBuilderParemeter();
            para.FormId = "BD_Customer";
            para.SelectItems = SelectorItemInfo.CreateItems("FCreateOrgId,FUseOrgId,FNumber,FShortName,FName,FCOUNTRY,FAddress,FZIP,FTEL,FSELLER,FSALDEPTID,FCustTypeId,FTRADINGCURRID,FTaxType,FTaxRate,FPriority,FIsTrade,F_HS_CustomerRegisteredMail,F_HS_Grade,F_HS_SpecialDemand,F_HS_TaxNum,FPRICELISTID,F_HS_CustomerPurchaseMail");

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

        private K3CustomerInfo GetCustomerByNo(string custNo)
        {
            if (synCustomersLog != null && synCustomersLog.Count > 0)
            {
                if (!string.IsNullOrWhiteSpace(custNo))
                {
                    IEnumerable<K3CustomerInfo> custs = synCustomersLog.Select(c => (K3CustomerInfo)c).Where(c => c.FNumber.Equals(custNo));

                    if (custs != null && custs.Count() > 0)
                    {
                        synCustomersLog.Remove(custs.ElementAt(0));
                        return custs.ElementAt(0);
                    }
                }
            }

            return null;
        }
    }
}
