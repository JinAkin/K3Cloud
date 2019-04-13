using Hands.K3.SCM.APP.Entity.StructType;
using Hands.K3.SCM.APP.Entity.SynDataObject.Material_;
using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Orm.Metadata.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hands.K3.SCM.APP.Utils.Utils
{
    [Kingdee.BOS.Util.HotUpdate]
    public class SQLUtils
    {
        private const int ORGID = 100035;//使用组织

        /// <summary>
        /// 获取K3Cloud字段值
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetFieldValue(DynamicObject obj, string fileName)
        {
            if (!string.IsNullOrWhiteSpace(fileName))
            {
                if (obj != null)
                {
                    if (obj.DynamicObjectType.Properties.Contains(fileName))
                    {
                        return JsonUtils.ConvertObjectToString(obj[fileName]);
                    }
                }
            }
            return default(string);
        }

        /// <summary>
        /// 根据组织ID获取组织编码
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public static string GetOrgNo(Context ctx, int orgId)
        {
            string sOrgId = string.Format(@"/*dialect*/ select FNUMBER  From T_ORG_ORGANIZATIONS where FORGID= {0} ", orgId);
            return JsonUtils.ConvertObjectToString(GetObject(ctx, sOrgId, "FNUMBER"));
        }
        /// <summary>
        /// 根据组织编码获取组织ID
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="orgNo"></param>
        /// <returns></returns>
        public static int GetOrgId(Context ctx, string orgNo)
        {
            if (!string.IsNullOrWhiteSpace(orgNo))
            {
                string sOrgNo = string.Format(@"/*dialect*/ select FORGID from T_ORG_ORGANIZATIONS where FNUMBER = '{0}'", orgNo);
                return Convert.ToInt32(JsonUtils.ConvertObjectToString(GetObject(ctx, sOrgNo, "FORGID")));
            }
            return 0;
        }
        /// <summary>
        ///根据员工编码获取部门编码
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static string GetDeptNo(Context ctx, string empNo)
        {
            string deptNo = null;
            if (!string.IsNullOrWhiteSpace(empNo))
            {
                string sDeptNo = string.Format(@"/*dialect*/select b.FNUMBER from T_BD_STAFFTEMP	a 
                                                            inner join T_BD_DEPARTMENT b on a.FDEPTID=b.FDEPTID
                                                            inner join T_BD_STAFF c on a.FSTAFFID=c.FSTAFFID
                                                            where c.FNUMBER='{0}'", empNo);
                deptNo = JsonUtils.ConvertObjectToString(GetObject(ctx, sDeptNo, "FNUMBER"));
            }
            return deptNo;
        }

        /// <summary>
        /// 根据员工编码获取部门ID
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="empNo"></param>
        /// <returns></returns>
        public static int GetDeptId(Context ctx, string empNo)
        {
            int deptNo = 0;
            if (!string.IsNullOrWhiteSpace(empNo))
            {
                string sDeptNo = string.Format(@"/*dialect*/ select b.FDEPTID from T_BD_STAFFTEMP	a 
                                                             inner join T_BD_DEPARTMENT b on a.FDEPTID=b.FDEPTID
                                                             inner join T_BD_STAFF c on a.FSTAFFID=c.FSTAFFID
                                                             where c.FNUMBER='{0}'", empNo);
                deptNo = Convert.ToInt32(JsonUtils.ConvertObjectToString(GetObject(ctx, sDeptNo, "FDEPTID")));
            }
            return deptNo;
        }
        /// <summary>
        /// 根据销售员ID获取销售员编码
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetSellerNo(Context ctx, DynamicObject obj, string fieldName)
        {
            string sellerNo = default(string);
            if (!string.IsNullOrWhiteSpace(fieldName))
            {
                string sellerId = GetFieldValue(obj, fieldName);
                string sSellerNo = string.Format(@"/*dialect*/ select FNUMBER from V_BD_SALESMAN where FID = {0}", Convert.ToInt32(sellerId));
                sellerNo = JsonUtils.ConvertObjectToString(GetObject(ctx, sSellerNo, "FNumber"));
            }
            return sellerNo;

        }

        /// <summary>
        /// 判断销售员编码是否存在
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="sellerNo"></param>
        /// <returns></returns>
        public static string GetSellerNo(Context ctx, string sellerNo)
        {
            string sql = string.Empty;
            string salerNo = string.Empty;

            if (!string.IsNullOrWhiteSpace(sellerNo))
            {
                sql = string.Format(@"/*dialect*/ select FNUMBER from V_BD_SALESMAN where FNUMBER = '{0}' and FFORBIDSTATUS = 'A'", sellerNo);
                salerNo = JsonUtils.ConvertObjectToString(GetObject(ctx, sql, "FNUMBER"));

                if (!string.IsNullOrWhiteSpace(salerNo))
                {
                    return salerNo;
                }
                else
                {
                    //sql = string.Format(@"/*dialect*/ select FNUMBER from T_HR_EMPINFO where FEMAIL = '{0}'", DataBaseConst.SalerEmail);
                    //salerNo = JsonUtils.ConvertObjectToString(GetObject(ctx, sql, "FNUMBER"));
                    return "1177";
                }
            }
            else
            {
                return "NA";
            }
            return salerNo;
        }

        /// <summary>
        /// 根据销售员编码获取销售员ID
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="sellerNo"></param>
        /// <returns></returns>
        public static int GetSellerId(Context ctx, string sellerNo)
        {
            if (!string.IsNullOrWhiteSpace(sellerNo))
            {
                string sql = string.Format(@"/*dialect*/ select FID from V_BD_SALESMAN  where FNUMBER='{0}'", sellerNo);
                int sellerId = Convert.ToInt32(JsonUtils.ConvertObjectToString(GetObject(ctx, sql, "FID")));
                return sellerId;
            }
            return 0;
        }

        /// <summary>
        /// 根据客户ID获取客户编码
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetCustomerNo(Context ctx, DynamicObject obj, string fieldName)
        {
            string customerNo = default(string);
            if (!string.IsNullOrWhiteSpace(fieldName))
            {
                string custId = GetFieldValue(obj, fieldName);
                string sCustNo = string.Format(@"/*dialect*/ select FNUMBER from T_BD_CUSTOMER where FCUSTID = {0}", Convert.ToInt32(custId));
                customerNo = JsonUtils.ConvertObjectToString(GetObject(ctx, sCustNo, "FNumber"));
            }
            return customerNo;

        }

        /// <summary>
        /// 根据客户ID获取客户编码
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="fieldValue"></param>
        /// <returns></returns>
        public static string GetCustomerNo(Context ctx, int fieldValue)
        {
            string sCustNo = string.Format(@"/*dialect*/ select FNUMBER from T_BD_CUSTOMER where FCUSTID = {0}", fieldValue);
            return JsonUtils.ConvertObjectToString(GetObject(ctx, sCustNo, "FNumber"));
        }
        /// <summary>
        /// 根据客户编码获取客户ID
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="obj"></param>
        /// <param name="custNo"></param>
        /// <returns></returns>
        public static int GetCustomerId(Context ctx, string custNo, int OrgId)
        {
            int customerId = default(int);
            if (!string.IsNullOrWhiteSpace(custNo))
            {
                string sCustNo = string.Format(@"/*dialect*/ select FCUSTID from T_BD_CUSTOMER where FNUMBER = '{0}' and FUSEORGID = {1}", custNo, OrgId);
                customerId = Convert.ToInt32(JsonUtils.ConvertObjectToString(GetObject(ctx, sCustNo, "FCUSTID")));
            }
            return customerId;
        }


        /// <summary>
        /// 根据客户类型ID获取相应的编码
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="custTypeId"></param>
        /// <returns></returns>
        public static string GetCustTypeNo(Context ctx, string custTypeId)
        {
            string custTypeNo = null;

            if (!string.IsNullOrWhiteSpace(custTypeId))
            {
                string sFCustType = string.Format(@"/*dialect*/ select distinct k.fnumber from  T_BD_CUSTOMER a
                                                            inner join T_BAS_ASSISTANTDATAENTRY_L j ON a.FCUSTTYPEID=j.FENTRYID
                                                            inner join T_BAS_ASSISTANTDATAENTRY k ON j.FentryID=k.FentryID
                                                            where a.FCUSTTYPEID = '{0}'", custTypeId);

                custTypeNo = JsonUtils.ConvertObjectToString(GetObject(ctx, sFCustType, "FNUMBER"));
            }

            return custTypeNo;
        }

        /// <summary>
        /// 根据客户类型ID获取相应的编码
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="obj"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static string GetCustTypeNo(Context ctx, DynamicObject obj, string fieldName)
        {
            string custTypeNo = default(string);

            if (!string.IsNullOrWhiteSpace(fieldName))
            {
                custTypeNo = GetFieldValue(obj, fieldName);

                string sCustTypeNo = string.Format(@"/*dialect*/ select distinct k.fnumber from  T_BD_CUSTOMER a
                                                            inner join T_BAS_ASSISTANTDATAENTRY_L j ON a.FCUSTTYPEID=j.FENTRYID
                                                            inner join T_BAS_ASSISTANTDATAENTRY k ON j.FentryID=k.FentryID
                                                            where a.FCUSTTYPEID = '{0}'", custTypeNo);

                custTypeNo = JsonUtils.ConvertObjectToString(GetObject(ctx, sCustTypeNo, "FNumber"));
            }
            return custTypeNo;
        }

        /// <summary>
        /// 根据客户分组ID获取客户分组编码
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static string GetCustGroupNo(Context ctx, DynamicObject obj, string fieldName)
        {
            string custGroupNo = default(string);

            if (!string.IsNullOrWhiteSpace(fieldName))
            {
                string custGroupId = GetFieldValue(obj, fieldName);
                string scustGroupNo = string.Format(@"/*dialect*/ select distinct k.FNUMBER from  T_BD_CUSTOMER a
                                                            inner join T_BAS_ASSISTANTDATAENTRY_L j ON a.FCUSTTYPEID=j.FENTRYID
                                                            inner join T_BAS_ASSISTANTDATAENTRY k ON j.FentryID=k.FentryID
                                                            where a.FCUSTTYPEID = '{0}'", custGroupId);
                custGroupNo = JsonUtils.ConvertObjectToString(GetObject(ctx, scustGroupNo, "FNUMBER"));
            }
            return custGroupNo;
        }

        /// <summary>
        /// 根据客户分组编码获取相应客户分组ID
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="custGroupNo"></param>
        /// <returns></returns>
        public static string GetCustGroupId(Context ctx, string custGroupNo)
        {
            if (!string.IsNullOrWhiteSpace(custGroupNo))
            {

                string scustGroupNo = string.Format(@"/*dialect*/ select distinct a.FCUSTTYPEID from  T_BD_CUSTOMER a
                                                            inner join T_BAS_ASSISTANTDATAENTRY_L j ON a.FCUSTTYPEID=j.FENTRYID
                                                            inner join T_BAS_ASSISTANTDATAENTRY k ON j.FentryID=k.FentryID
                                                            where k.FNUMBER = '{0}'", custGroupNo);
                return JsonUtils.ConvertObjectToString(GetObject(ctx, scustGroupNo, "FCUSTTYPEID"));
            }
            return null;
        }

        /// <summary>
        /// 根据物料ID获取物料编码
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static string GetMaterialNo(Context ctx, DynamicObject obj, string fieldName)
        {
            string materialNo = default(string);

            if (!string.IsNullOrWhiteSpace(fieldName))
            {

                string matId = GetFieldValue(obj, fieldName);
                string sMatNo = string.Format(@"/*dialect*/ select FNumber from T_BD_MATERIAL where FMATERIALID = {0} ", Convert.ToInt32(matId));
                materialNo = JsonUtils.ConvertObjectToString(GetObject(ctx, sMatNo, "FNumber"));

            }

            return materialNo;
        }

        /// <summary>
        /// 根据仓库ID获取仓库编码
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static string GetStockNo(Context ctx, DynamicObject obj, string fieldName)
        {
            string stockNo = default(string);

            if (!string.IsNullOrWhiteSpace(fieldName))
            {
                string stockId = GetFieldValue(obj, fieldName);
                string sStockNo = string.Format(@"/*dialect*/ select FNUMBER FROM T_BD_STOCK where FSTOCKID = {0}", Convert.ToInt32(stockId));
                stockNo = JsonUtils.ConvertObjectToString(GetObject(ctx, sStockNo, "FNUMBER"));
            }

            return stockNo;
        }

        /// <summary>
        /// 根据物料编码获取仓库编码
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="materialNo"></param>
        /// <returns></returns>
        public static string GetStockNo(Context ctx, string materialNo)
        {
            string stockNo = null;
            if (!string.IsNullOrWhiteSpace(materialNo))
            {
                string sql = string.Format(@"/*dialect*/ select c.FNUMBER from T_BD_MATERIAL a
                                                                        inner join T_BD_MATERIALSTOCK b on a.FMATERIALID=b.FMATERIALID 
                                                                        inner join T_BD_STOCK c on b.FSTOCKID=c.FSTOCKID
                                                                        where a.FNUMBER='{0}' and a.FUSEORGID={1}", materialNo, ORGID);
                stockNo = JsonUtils.ConvertObjectToString(GetObject(ctx, sql, "FNUMBER"));
            }
            return stockNo;
        }

        /// <summary>
        /// 根据仓库ID获取仓库编码
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="materialNo"></param>
        /// <returns></returns>
        public static string GetStockNo(Context ctx, int stockId)
        {
            string sql = string.Format(@"/*dialect*/select FNUMBER from T_BD_STOCK where FSTOCKID = {0}", stockId);
            return JsonUtils.ConvertObjectToString(GetObject(ctx, sql, "FNUMBER"));
        }

        /// <summary>
        ///根据结算币别ID 获取结算币别编码
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static string GetSettleCurrNo(Context ctx, DynamicObject obj, string fieldName)
        {
            string SettleCurrNo = default(string);

            if (!string.IsNullOrWhiteSpace(fieldName))
            {
                string settleCurrID = GetFieldValue(obj, fieldName);
                string sSettleCurrNo = string.Format(@"/*dialect*/ select FNUMBER from T_BD_CURRENCY where FCURRENCYID = {0} ", Convert.ToInt32(settleCurrID));
                SettleCurrNo = JsonUtils.ConvertObjectToString(GetObject(ctx, sSettleCurrNo, "FNUMBER"));
            }
            return SettleCurrNo;
        }

        public static int GetSettleCurrId(Context ctx, string settleCurrNo)
        {


            if (!string.IsNullOrWhiteSpace(settleCurrNo))
            {

                string sSettleCurrId = string.Format(@"/*dialect*/ select FCURRENCYID from T_BD_CURRENCY where FNUMBER = '{0}' ", settleCurrNo);
                return Convert.ToInt32(JsonUtils.ConvertObjectToString(GetObject(ctx, sSettleCurrId, "FCURRENCYID")));
            }
            return 0;
        }

        /// <summary>
        /// 根据结算方式ID获取结算方式编码
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static string GetSettleTypeNo(Context ctx, DynamicObject obj, string fieldName)
        {
            string SettleTypeNo = default(string);

            if (!string.IsNullOrWhiteSpace(fieldName))
            {
                string settleTypeId = GetFieldValue(obj, fieldName);
                string sSettleNo = string.Format(@"/*dialect*/ select FNUMBER from T_BD_SETTLETYPE where FID = {0}", Convert.ToInt32(settleTypeId));
                SettleTypeNo = JsonUtils.ConvertObjectToString(GetObject(ctx, sSettleNo, "FNUMBER"));
            }
            return SettleTypeNo;
        }

        /// <summary>
        /// 根据国家ID获取国家编码
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static string GetCountryNo(Context ctx, DynamicObject obj, string fieldName)
        {
            string countryNo = default(string);

            if (!string.IsNullOrWhiteSpace(fieldName))
            {
                string countryId = GetFieldValue(obj, fieldName);
                string sSettleNo = string.Format(@"/*dialect*/ select FNUMBER from VW_BAS_ASSISTANTDATA_CountryName where FCountry = '{0}'", countryId);
                countryNo = JsonUtils.ConvertObjectToString(GetObject(ctx, sSettleNo, "FNUMBER"));
            }
            return countryNo;
        }

        /// <summary>
        /// 根据国家ID获取国家编码
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="countryId"></param>
        /// <returns></returns>
        public static string GetCountryNo(Context ctx, string countryId)
        {
            string countryNo = default(string);

            if (!string.IsNullOrWhiteSpace(countryId))
            {
                string sCountryNo = string.Format(@"/*dialect*/ select FNUMBER from VW_BAS_ASSISTANTDATA_CountryName where FCountry = '{0}'", countryId);
                countryNo = JsonUtils.ConvertObjectToString(GetObject(ctx, sCountryNo, "FNUMBER"));
            }
            return countryNo;
        }

        /// <summary>
        /// 根据国家编码获取国家ID
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="countryNo"></param>
        /// <returns></returns>
        public static string GetCountryId(Context ctx, string countryNo)
        {
            string countryId = default(string);

            if (!string.IsNullOrWhiteSpace(countryNo))
            {
                string sCountryNo = string.Format(@"/*dialect*/ select FCountry from VW_BAS_ASSISTANTDATA_CountryName where FNUMBER = '{0}'", countryNo);
                countryId = JsonUtils.ConvertObjectToString(GetObject(ctx, sCountryNo, "FCountry"));
            }
            return countryId;
        }

        /// <summary>
        /// 根据订单来源ID获取订单来源编码
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="obj"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static string GetSaleOrderSourceNo(Context ctx, DynamicObject obj, string fieldName)
        {
            string sourceNo = default(string);

            if (!string.IsNullOrWhiteSpace(fieldName))
            {
                string sourceId = GetFieldValue(obj, fieldName);
                string sSourceNo = string.Format(@"/*dialect*/   select distinct k.FNUMBER from  T_SAL_ORDER a
                                                            inner join T_BAS_ASSISTANTDATAENTRY_L j ON a.F_HS_SaleOrderSource=j.FENTRYID
                                                            inner join T_BAS_ASSISTANTDATAENTRY k ON j.FentryID=k.FentryID
                                                            where j.FENTRYID = '{0}'", sourceId);
                sourceNo = JsonUtils.ConvertObjectToString(GetObject(ctx, sSourceNo, "FNUMBER"));
            }

            return sourceNo;
        }

        /// <summary>
        /// 根据物料编码获取相应的单位编码
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="materialNo"></param>
        /// <returns></returns>
        public static string GetUnitNo(Context ctx, string materialNo)
        {
            string unitNo = null;

            if (!string.IsNullOrWhiteSpace(materialNo))
            {
                string sql = string.Format(@"/*dialect*/ select c.FNUMBER from T_BD_MATERIAL a
                                        inner join T_BD_MATERIALSALE b on a.FMATERIALID=b.FMATERIALID 
                                        inner join T_BD_UNIT c on b.FSALEUNITID=c.FUNITID
                                        where a.FNUMBER='{0}' and a.FUSEORGID={1}", materialNo, ORGID);

                unitNo = JsonUtils.ConvertObjectToString(GetObject(ctx, sql, "FNUMBER"));
            }
            return unitNo;
        }

        /// <summary>
        /// 根据付款方式ID获取付款方式编码
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="obj"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static string GetPaymentNo(Context ctx, DynamicObject obj, string fieldName)
        {
            if (!string.IsNullOrWhiteSpace(fieldName))
            {
                string paymentId = GetFieldValue(obj, fieldName);
                string sql = string.Format(@"/*dialect*/ select distinct k.fnumber from  T_SAL_ORDER a
                                                    inner join T_BAS_ASSISTANTDATAENTRY_L j ON a.F_HS_PaymentModeNew=j.FENTRYID
                                                    inner join T_BAS_ASSISTANTDATAENTRY k ON j.FentryID=k.FentryID
                                                    where a.F_HS_PaymentModeNew = '{0}'", paymentId);
                return JsonUtils.ConvertObjectToString(GetObject(ctx, sql, "fnumber"));
            }

            return null;

        }

        /// <summary>
        /// 根据价目表编码获取相应的ID
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="custNo"></param>
        /// <param name="priceListNo"></param>
        /// <returns></returns>
        public static string GetPriceListId(Context ctx, string custNo, string priceListNo)
        {
            if (!string.IsNullOrWhiteSpace(custNo) && !string.IsNullOrWhiteSpace(priceListNo))
            {
                string sql = string.Format(@"/*dialect*/ select b.FID from T_BD_CUSTOMER a
                                                    inner join T_SAL_PRICELIST b
                                                    on a.FPRICELISTID = b.FID
                                                    where a.FNUMBER = '{0}' and b.FNUMBER = '{1}' and b.FSALEORGID = 100035", custNo, priceListNo);
                return JsonUtils.ConvertObjectToString(GetObject(ctx, sql, "FID"));
            }

            return null;
        }

        /// <summary>
        /// 根据物流渠道ID获取相应的物流渠道编码
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="logisticsChannelId"></param>
        /// <returns></returns>
        public static string GetLogisticsChannelNo(Context ctx, string logisticsChannelId)
        {
            if (!string.IsNullOrWhiteSpace(logisticsChannelId))
            {
                string sql = string.Format(@"/*dialect*/ select distinct k.fnumber from  T_HS_ShipMethods a
                                                            inner join T_BAS_ASSISTANTDATAENTRY_L j ON a.F_HS_Channel=j.FENTRYID
                                                            inner join T_BAS_ASSISTANTDATAENTRY k ON j.FentryID=k.FentryID
                                                            where a.F_HS_Channel = '{0}'", logisticsChannelId);

                return JsonUtils.ConvertObjectToString(GetObject(ctx, sql, "fnumber"));
            }
            return null;
        }

        /// <summary>
        /// 根据物流渠道编码获取相应的物流渠道ID
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="logisticsChannelNo"></param>
        /// <returns></returns>
        public static string GetLogisticsChannelId(Context ctx, string logisticsChannelNo)
        {
            if (!string.IsNullOrWhiteSpace(logisticsChannelNo))
            {
                string sql = string.Format(@"/*dialect*/    select distinct a.F_HS_Channel from  T_HS_ShipMethods a
                                                            inner join T_BAS_ASSISTANTDATAENTRY_L j ON a.F_HS_Channel=j.FENTRYID
                                                            inner join T_BAS_ASSISTANTDATAENTRY k ON j.FentryID=k.FentryID
                                                            where k.fnumber = '{0}'", logisticsChannelNo);

                return JsonUtils.ConvertObjectToString(GetObject(ctx, sql, "fnumber"));
            }
            return null;
        }

        /// <summary>
        /// 根据物流方式编码获取物流方式的ID
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="shipMethodNo"></param>
        /// <returns></returns>
        public static int GetShipMethodId(Context ctx, string shipMethodNo)
        {
            if (!string.IsNullOrWhiteSpace(shipMethodNo))
            {
                string sql = string.Format(@"/*dialect*/    select distinct a.FID from  T_HS_ShipMethods a
                                                            inner join T_BAS_ASSISTANTDATAENTRY_L j ON a.F_HS_Channel=j.FENTRYID
                                                            inner join T_BAS_ASSISTANTDATAENTRY k ON j.FentryID=k.FentryID
                                                            where a.FNumber = '{0}'", shipMethodNo);

                return Convert.ToInt32(JsonUtils.ConvertObjectToString(GetObject(ctx, sql, "FID")));
            }
            return 0;
        }

        /// <summary>
        /// 根据物流方式ID获取物流方式的编码
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="shipMethodId"></param>
        /// <returns></returns>
        public static string GetShipMethodNo(Context ctx, int shipMethodId)
        {
            if (shipMethodId != 0)
            {
                string sql = string.Format(@"/*dialect*/    select distinct a.FNumber from  T_HS_ShipMethods a
                                                            inner join T_BAS_ASSISTANTDATAENTRY_L j ON a.F_HS_Channel=j.FENTRYID
                                                            inner join T_BAS_ASSISTANTDATAENTRY k ON j.FentryID=k.FentryID
                                                            where a.FID = {0}", shipMethodId);

                return JsonUtils.ConvertObjectToString(GetObject(ctx, sql, "FNumber"));
            }
            return null;
        }

        /// <summary>
        /// 根据单据类型ID获取单据类型编码
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="item"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static string GetBillTypeNo(Context ctx, DynamicObject item, string fieldName)
        {
            if (!string.IsNullOrWhiteSpace(fieldName))
            {
                string billTypeId = GetFieldValue(item, fieldName);
                if (!string.IsNullOrWhiteSpace(billTypeId))
                {
                    string sql = string.Format(@"/*dialect*/select  b.FNumber from T_SAL_ORDER a
                                                            inner join T_BAS_BILLTYPE b ON a.FBILLTypeID=b.FBILLTypeID
                                                            where a.FBILLTypeID = '{0}'", billTypeId);
                    return JsonUtils.ConvertObjectToString(GetObject(ctx, sql, "FNumber"));
                }
            }
            return null;
        }

        /// <summary>
        /// 根据付款方式获取银行账号编码
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="paymentMethod"></param>
        /// <returns></returns>
        public static string GetBankAccountId(Context ctx, string paymentMethod)
        {
            if (!string.IsNullOrWhiteSpace(paymentMethod))
            {
                string sql = string.Format(@"/*dialect*/ select F_HS_BANKACCOUNT from T_BD_SETTLETYPE where FNUMBER = '{0}'", paymentMethod, ORGID);
                return JsonUtils.ConvertObjectToString(GetObject(ctx, sql, "F_HS_BANKACCOUNT"));
            }

            return null;
        }

        /// <summary>
        /// 根据银行账号ID获取银行账号编码
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="bankAccountId"></param>
        /// <returns></returns>
        public static string GetBankAccountNo(Context ctx, string bankAccountId)
        {
            if (!string.IsNullOrWhiteSpace(bankAccountId))
            {
                string sql = string.Format(@"/*dialect*/ select FNUMBER from T_CN_BANKACNT where FBANKACNTID = {0} and FUSEORGID = {1}", bankAccountId, ORGID);
                return string.IsNullOrWhiteSpace(JsonUtils.ConvertObjectToString(GetObject(ctx, sql, "FNUMBER"))) ? "" : JsonUtils.ConvertObjectToString(GetObject(ctx, sql, "FNUMBER"));
            }
            return "";
        }
        /// <summary>
        /// 根据付款方式编码获取付款方式ID
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="paymentMethodNo"></param>
        /// <returns></returns>
        public static string GetPaymentMethodId(Context ctx, string paymentMethodNo)
        {
            if (!string.IsNullOrWhiteSpace(paymentMethodNo))
            {
                string sql = string.Format(@"/*dialect*/ select distinct a.F_HS_PaymentModeNew from  T_SAL_ORDER a
	                                                    inner join T_BAS_ASSISTANTDATAENTRY_L j ON a.F_HS_PaymentModeNew=j.FENTRYID
	                                                    inner join T_BAS_ASSISTANTDATAENTRY k ON j.FentryID=k.FentryID
	                                                    where k.fnumber = '{0}'", paymentMethodNo);
                return JsonUtils.ConvertObjectToString(GetObject(ctx, sql, "F_HS_PaymentModeNew"));
            }

            return null;
        }

        /// <summary>
        /// 根据付款方式编码获取结算方式编码
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="paymentMethodId"></param>
        /// <returns></returns>
        public static string GetSettleTypeNo(Context ctx, string paymentMethodId)
        {
            if (!string.IsNullOrWhiteSpace(paymentMethodId))
            {
                string sql = string.Format(@"/*dialect*/ select FNUMBER from T_BD_SETTLETYPE a
                                                        inner join T_BD_SETTLETYPE_L b on a.FID = b.FID
                                                        where F_HS_WebPaymentMethod = '{0}' and b.FLOCALEID = 2052", paymentMethodId);

                string settleTypeNo = JsonUtils.ConvertObjectToString(GetObject(ctx, sql, "FNUMBER"));
                return string.IsNullOrWhiteSpace(settleTypeNo) ? "" : settleTypeNo;
            }

            return "";
        }

        /// <summary>
        /// 根据销售订单编码获取销售订单内码
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="saleOrderNo"></param>
        /// <returns></returns>
        public static int GetSaleOrderId(Context ctx, string saleOrderNo)
        {
            if (!string.IsNullOrWhiteSpace(saleOrderNo))
            {
                string sql = string.Format(@"/*dialect*/ select FID from T_SAL_ORDER where FBILLNO = '{0}' and FDOCUMENTSTATUS != 'D' and FDOCUMENTSTATUS != 'B'", saleOrderNo);
                return Convert.ToInt32(JsonUtils.ConvertObjectToString(GetObject(ctx, sql, "FID")));
            }
            return 0;
        }

        /// <summary>
        /// 获取收款退款单退款类型编码
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="obj"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static string GetReFundType(Context ctx, DynamicObject obj, string fieldName)
        {
            if (!string.IsNullOrWhiteSpace(fieldName))
            {
                string reFundType = GetFieldValue(obj, fieldName);

                string sql = string.Format(@"/*dialect*/ select distinct k.fnumber from  T_AR_REFUNDBILL a
                                                    inner join T_BAS_ASSISTANTDATAENTRY_L j ON a.F_HS_RefundType=j.FENTRYID
                                                    inner join T_BAS_ASSISTANTDATAENTRY k ON j.FentryID=k.FentryID
                                                    where a.F_HS_RefundType = '{0}'", reFundType);

                return JsonUtils.ConvertObjectToString(GetObject(ctx, sql, "fnumber"));
            }
            return null;
        }

        /// <summary>
        /// 获取收款退款单退款方式编码
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="obj"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static string GetReFundMethod(Context ctx, DynamicObject obj, string fieldName)
        {
            if (!string.IsNullOrWhiteSpace(fieldName))
            {
                string reFundMethod = GetFieldValue(obj, fieldName);

                string sql = string.Format(@"/*dialect*/ select distinct k.fnumber from  T_AR_REFUNDBILL a
                                                    inner join T_BAS_ASSISTANTDATAENTRY_L j ON a.F_HS_REFUNDMETHOD=j.FENTRYID
                                                    inner join T_BAS_ASSISTANTDATAENTRY k ON j.FentryID=k.FentryID
                                                    where a.F_HS_REFUNDMETHOD = '{0}'", reFundMethod);

                return JsonUtils.ConvertObjectToString(GetObject(ctx, sql, "fnumber"));
            }
            return null;
        }

        /// <summary>
        /// 根据品牌编码获取品牌名称
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="brandNo"></param>
        /// <returns></returns>
        public static string GetBrandName(Context ctx, string brandNo)
        {
            if (!string.IsNullOrWhiteSpace(brandNo))
            {
                string sql = string.Format(@"/*dialect*/ select b.FNAME from HS_T_Brand a
										        inner join HS_T_BRAND_L b
										        on a.FID = b.FID
										        where a.FNUMBER = '{0}'", brandNo);
                return JsonUtils.ConvertObjectToString(GetObject(ctx, sql, "FNAME"));
            }
            return null;
        }

        /// <summary>
        /// 根据物料分组获取SKU分组码
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="materialGroup"></param>
        /// <returns></returns>
        public static string GetSKUCode(Context ctx, DynamicObject obj, string fieldName)
        {
            string materialGroup = GetFieldValue(obj, fieldName);

            if (!string.IsNullOrWhiteSpace(materialGroup))
            {
                string sql = string.Format(@"/*dialect*/ select b.F_HS_SKUGroupCode from T_BD_MATERIAL a
											            inner join T_BD_MATERIALGROUP b
											            on a.FMaterialGroup = b.FID
											            where b.FNUMBER = '{0}'", materialGroup);
                return JsonUtils.ConvertObjectToString(GetObject(ctx, sql, "F_HS_SKUGroupCode"));
            }
            return null;
        }
        /// <summary>
        /// 获取表中某个指定字段的值
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="sql"></param>
        /// <param name="selectItem"></param>
        /// <returns></returns>
        public static object GetObject(Context ctx, string sql, string selectItem)
        {
            if (ctx != null)
            {
                DynamicObjectCollection coll = DBServiceHelper.ExecuteDynamicObject(ctx, sql);

                if (coll.Count > 0)
                {
                    foreach (var item in coll)
                    {
                        if (item != null)
                        {
                            return item[selectItem];
                        }
                    }
                }
            }
            return default(object);
        }
        /// <summary>
        /// 获取表中字段的值
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static DynamicObjectCollection GetObjects(Context ctx, string sql)
        {
            if(!string.IsNullOrWhiteSpace(sql))
            {
                return DBServiceHelper.ExecuteDynamicObject(ctx, sql);
            }
            return null;
        }

        /// <summary>
        /// 查询表中的编码是否存在，先在已同步的历史记录查询，不存在则查询数据库
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="lstNum"></param>
        /// <param name="sql"></param>
        /// <param name="newNum"></param>
        /// <returns></returns>
        public static bool IsExistNum(Context ctx, List<string> lstNum, string sql, string newNum)
        {
            if (lstNum != default(List<string>))
            {
                if (lstNum.Count > 0)
                {
                    foreach (var custNum in lstNum)
                    {
                        if (custNum != default(string))
                        {
                            if (custNum.CompareTo(newNum) == 0)
                            {
                                if (GetObjects(ctx, string.Format(sql)).Count > 0)
                                {
                                    return true;
                                }

                            }
                        }
                    }
                }
                else
                {
                    if (GetObjects(ctx, string.Format(sql)).Count > 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public static DynamicObject[] GetDynamicObjects(Context ctx, string formId, string filterClauseWihtKey)
        {
            if (!string.IsNullOrEmpty(formId))
            {
                FormMetadata metaData = MetaDataServiceHelper.Load(ctx, formId) as FormMetadata;
                DynamicObjectType type = metaData.BusinessInfo.GetDynamicObjectType();
                QueryBuilderParemeter queryParemeter = new QueryBuilderParemeter();
                queryParemeter.FormId = formId;

                if (!string.IsNullOrEmpty(filterClauseWihtKey))
                {
                    queryParemeter.FilterClauseWihtKey = filterClauseWihtKey;
                }

                DynamicObject[] objects = BusinessDataServiceHelper.Load(ctx, type, queryParemeter);
                return objects;
            }

            return null;
        }

        /// <summary>
        /// 执行SQL
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static int Execute(Context ctx, string sql)
        {
            if (!string.IsNullOrWhiteSpace(sql))
            {
                return DBUtils.Execute(ctx, sql);
            }

            return 0;
        }

        /// <summary>
        /// 根据仓库地理仓编码获取仓库的编码
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="stockCode">仓库编码或地理仓编码</param>
        /// <param name="flag">true:查询仓库编码，false:查询仓库地理仓编码</param>
        /// <returns></returns>
        public static string GetStockNo(Context ctx, string stockCode, bool flag = true)
        {
            string stockNo = string.Empty;

            if (!string.IsNullOrWhiteSpace(stockCode))
            {

                string sql = string.Format(@"/*dialect*/ select {0} 
														from T_BD_STOCK a
														inner join HS_t_DropShipStockEntity b on a.FSTOCKID = b.F_HS_DROPSHIPSTOCK
														inner join T_BD_CUSTOMER c on c.FCUSTID = b.FCUSTID
														inner join T_BAS_ASSISTANTDATAENTRY_L d on a.F_HS_DLC = d.FENTRYID
                                                        inner join T_BAS_ASSISTANTDATAENTRY e on d.FENTRYID = e.FENTRYID
														where {1}='{2}'
                                                        and c.FUSEORGID = 100035", flag ? "a.FNUMBER" : "e.FNUMBER", flag ? "e.FNUMBER" : "a.FNUMBER", stockCode);

                stockNo = JsonUtils.ConvertObjectToString(SQLUtils.GetObject(ctx, sql, "FNUMBER"));
            }
            return stockNo;
        }

        /// <summary>
        /// 获取物料信息
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="materilNo"></param>
        /// <returns></returns>
        public static Material GetMaterial(Context ctx, string materilNo)
        {
            Material material = null;

            if (!string.IsNullOrWhiteSpace(materilNo))
            {
                string sql = string.Format(@"/*dialect*/  select a.FNumber as FMaterialId,a.F_HS_IsOil,d.FNUMBER as F_HS_PRODUCTSTATUS
                                                from T_BD_MATERIAL a 
                                                inner join T_ORG_ORGANIZATIONS b on a.FUSEORGID=b.FORGID 
											    inner join T_BAS_ASSISTANTDATAENTRY_L c ON a.F_HS_PRODUCTSTATUS=c.FENTRYID
                                                inner join T_BAS_ASSISTANTDATAENTRY d ON c.FentryID=d.FentryID
                                                where b.FNUMBER='100' 
											    and a.FNUMBER not like '99.%'
											    and a.FNUMBER = '{0}'", materilNo);

                DynamicObjectCollection objs = GetObjects(ctx,sql);

                if (objs != null && objs.Count > 0)
                {
                    DynamicObject obj = objs.FirstOrDefault();

                    if (obj != null)
                    {
                        material = new Material();
                        material.FNumber = GetFieldValue(obj, "FMaterialId");
                        material.F_HS_IsOil = GetFieldValue(obj, "F_HS_IsOil");
                        material.F_HS_PRODUCTSTATUS = GetFieldValue(obj, "F_HS_PRODUCTSTATUS");
                        material.F_HS_DropShipOrderPrefix = GetFieldValue(obj, "F_HS_DropShipOrderPrefix");
                        return material;
                    }
                }
            }

            return material;
        }

        /// <summary>
        /// 获取B2B客户DropShipOrderPrefix订单前缀
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static string GetAUB2BDropShipOrderPrefix(Context ctx)
        {
            string sql = string.Format(@"/*dialect*/ select F_HS_DROPSHIPORDERPREFIX from T_BD_CUSTOMER
												    where FNUMBER = '{0}'
												    and FUSEORGID = 100035",DataBaseConst.Param_AUB2B_customerID);
            return JsonUtils.ConvertObjectToString(SQLUtils.GetObject(ctx, sql, "F_HS_DROPSHIPORDERPREFIX"));
        }
        /// <summary>
        /// 获取客户Dropship平台
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="custNo"></param>
        /// <returns></returns>
        public static string GetDropshipPlatform(Context ctx,string custNo)
        {
            if (!string.IsNullOrWhiteSpace(custNo))
            {
                string sql = string.Format(@"/*dialect*/ select c.FNUMBER 
				                                        from T_BD_CUSTOMER a
				                                        inner join T_BAS_ASSISTANTDATAENTRY_L b on a.F_HS_DropshipPlatform = b.FENTRYID
				                                        inner join T_BAS_ASSISTANTDATAENTRY c on c.FENTRYID = b.FENTRYID
				                                        where a.FNUMBER = '{0}'
													    and a.FUSEORGID = 100035", custNo);
                return JsonUtils.ConvertObjectToString(SQLUtils.GetObject(ctx, sql, "FNUMBER"));
            }

            return string.Empty;
        }
        /// <summary>
        /// 处理字符串中单引号和双引号在SQL的问题
        /// </summary>
        /// <param name="fieldValue"></param>
        /// <returns></returns>
        public static string DealQuotes(string fieldValue)
        {
            if (!string.IsNullOrWhiteSpace(fieldValue))
            {
                if (fieldValue.Contains("'"))
                {
                    fieldValue = fieldValue.Replace("'", "''");
                }
                else if (fieldValue.Contains("\""))
                {
                    fieldValue = fieldValue.Replace("\"", "");
                }
            }
            return fieldValue;
        }

       
    }

}
