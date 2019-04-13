
using Hands.K3.SCM.APP.Entity.K3WebApi;
using Hands.K3.SCM.APP.Entity.StructType;
using Hands.K3.SCM.APP.Entity.SynDataObject;
using Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject;
using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using Hands.K3.SCM.APP.Entity.CommonObject;
using Kingdee.BOS.Core.Metadata;
using Newtonsoft.Json.Linq;
using Kingdee.BOS.ServiceHelper;
using Hands.K3.SCM.APP.Utils.Utils;
using Hands.K3.SCM.App.Synchro.Base.Abstract;
using Hands.K3.SCM.App.Synchro.Utils.SynchroService;
using Kingdee.BOS.Orm.DataEntity;
using HS.K3.Common.Abbott;
using Hands.K3.SCM.APP.Entity.EnumType;

namespace Hands.K3.SCM.App.Core.SynchroService.ToK3
{
    public class SynCustomerAddress_DelToK3 : AbstractSynchroToK3
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
                return SynchroDataType.DelCustomerAddress;
            }
        }
        /// <summary>
        /// 数据类型对应的k3cloud的formkey，参照 HSFormIdConst
        /// </summary>
        override public string FormKey
        {
            get
            {
                return HSFormIdConst.CUSTCONTACT;
            }
        }
        public override FormMetadata Metadata
        {
            get
            {
                return MetaDataServiceHelper.Load(this.K3CloudContext, this.FormKey) as FormMetadata;
            }
        }

        /// <summary>
        /// 获取客户地址编码集合
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<AbsSynchroDataInfo> GetSynchroData(IEnumerable<string> numbers = null)
        {
            return ServiceHelper.GetSynchroDatas(this.K3CloudContext, this.DataType, this.RedisDbId, numbers,this.Direction);
        }

        public override JObject BuildSynchroDataJson(AbsSynchroDataInfo sourceData, SynchroLog log, SynOperationType operationType)
        {
            return null;
        }

        public override JObject BuildSynchroDataJsons(IEnumerable<AbsSynchroDataInfo> sourceDatas, SynOperationType operationType)
        {
            return null;
        }
        public override Dictionary<SynOperationType, IEnumerable<AbsSynchroDataInfo>> EntityDataSource(Context ctx, IEnumerable<AbsSynchroDataInfo> srcDatas)
        {
            if (srcDatas != null && srcDatas.Count() > 0)
            {
                Dictionary<SynOperationType, IEnumerable<AbsSynchroDataInfo>> dict = new Dictionary<SynOperationType, IEnumerable<AbsSynchroDataInfo>>();
                dict.Add(SynOperationType.DELETE, srcDatas);

                return dict;
            }
            return null;
        }
        public override AbsSynchroDataInfo BuildSynchroData(Context ctx, string json, AbsSynchroDataInfo data = null)
        {
            JArray jArr = JArray.Parse(json);

            List<K3CustContactInfo> contacts = null;
            K3CustContactInfo contact = null;
            K3CustomerInfo k3Cust = null;

            List<string> addrNos = null;

            if (jArr != null && jArr.Count > 0)
            {
                addrNos = jArr.Select(j => JsonUtils.GetFieldValue(j, "address_book_id")).ToList();   
            }

            if (addrNos != null && addrNos.Count > 0)
            {
                string sql = string.Format(@"/*dialect*/ select a.FNUMBER as CUSTNO,b.FNUMBER as ADDRNO
                                                        from T_BD_CUSTOMER a
                                                        inner join T_BD_CUSTLOCATION b
                                                        on a.FCUSTID = b.FCUSTID
                                                        where b.FNUMBER in( '{0}') ", string.Join("','", addrNos));

                DynamicObjectCollection coll = SQLUtils.GetObjects(ctx,sql);
                var group = from o in coll
                            group o by SQLUtils.GetFieldValue(o, "CUSTNO")
                            into c
                            select c;

                if (group != null && group.Count() > 0)
                {
                    foreach (var g in group)
                    {
                        if (g != null)
                        {
                            contacts = new List<K3CustContactInfo>();
                            k3Cust = new K3CustomerInfo();
                            k3Cust.FNumber = SQLUtils.GetFieldValue(g.ElementAt(0), "CUSTNO");

                            foreach (var s in g)
                            {
                                if (s != null)
                                {
                                    contact = new K3CustContactInfo();
                                    contact.FNUMBER1 = SQLUtils.GetFieldValue(s, "ADDRNO");
                                    contacts.Add(contact);
                                }
                            }
                            k3Cust.lstCustCtaInfo = contacts;
                        }
                    } 
                }  
            }
            return k3Cust;
        }

        public override HttpResponseResult ExecuteSynchro(IEnumerable<AbsSynchroDataInfo> sourceDatas, List<SynchroLog> logs, SynOperationType operationType)
        {
            HttpResponseResult result = null;
            List<K3CustomerInfo> infos = sourceDatas.Select(c => (K3CustomerInfo)c).ToList();

            
                string sql = string.Empty;
                List<string> addrNos = null;

                if (infos != null && infos.Count > 0)
                {
                    addrNos = infos.Select(a => a.lstCustCtaInfo).SelectMany(a => a.Select(b => b.FNUMBER1)).ToList();

                    foreach (var info in infos)
                    {
                        if (info != null && info.lstCustCtaInfo != null && info.lstCustCtaInfo.Count > 0)
                        {
                            sql += string.Format(@"/*dialect*/delete from T_BD_CUSTLOCATION where FNUMBER in ('{0}')", string.Join("','", info.lstCustCtaInfo.Select(i => i.FNUMBER1)));

                            try
                            {
                                int count = DBUtils.Execute(this.K3CloudContext, sql);

                                if (count > 0)
                                {
                                    result = new HttpResponseResult();
                                    result.Message = string.Format("客户编码【{0}】删除客户地址【{1}】成功!", info.FNumber, string.Join(",", info.lstCustCtaInfo.Select(i => i.FNUMBER1)));
                                    result.Success = true;

                                    LogUtils.WriteSynchroLog(this.K3CloudContext, this.DataType, result.Message);
                                    RemoveRedisData(this.K3CloudContext, addrNos);
                                }
                                else
                                {
                                    result = new HttpResponseResult();
                                    result.Message = "无客户地址需要删除";
                                    result.Success = false;
                                    LogUtils.WriteSynchroLog(this.K3CloudContext, this.DataType, result.Message);
                                }
                            }
                            catch (Exception ex)
                            {
                                result = new HttpResponseResult();
                                result.Success = false;
                                result.Message = string.Format("客户编码【{0}】删除客户地址【{1}】失败:", info.FNumber, string.Join(",", info.lstCustCtaInfo.Select(i => i.FNUMBER1))) + System.Environment.NewLine + ex.Message;
                                LogUtils.WriteSynchroLog(this.K3CloudContext, this.DataType, "删除客户地址失败:" + System.Environment.NewLine + ex.Message + System.Environment.NewLine + ex.StackTrace);
                                
                            }   
                        }
                    }
                }
            
            return result;
        }
    }
}
