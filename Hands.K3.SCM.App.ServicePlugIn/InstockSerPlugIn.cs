using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hands.K3.SCM.APP.Entity.K3WebApi;
using Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject;
using Hands.K3.SCM.APP.Entity.SynDataObject.InStockBillObject;
using Hands.K3.SCM.APP.Utils.Utils;
using HS.K3.Common.Abbott;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;

namespace Hands.K3.SCM.App.ServicePlugIn
{
    public class InstockSerPlugIn : AbstractOSPlugIn
    {
        [Description("采购入库单服务插件-将物料中有首页标记的物料同步至HC网站")]

        public override SynchroDataType DataType
        {
            get
            {
                return SynchroDataType.InStock;
            }
        }
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            e.FieldKeys.Add("FApproveDate");
        }

        public override void OnAddValidators(AddValidatorsEventArgs e)
        {
            base.OnAddValidators(e);

            //List<DynamicObject> objects = e.DataEntities.ToList();
            //HttpResponseResult result = OperateAfterAudit(this.Context, objects);

            //AuditValidator validator = new AuditValidator(result, this.BusinessInfo.GetForm().Id);
            //e.Validators.Add(validator);
        }

        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);

            List<DynamicObject> objects = e.DataEntitys.ToList();
            HttpResponseResult result = OperateAfterAudit(this.Context, objects);

            if (result != null && !result.Success && !string.IsNullOrWhiteSpace(result.Message))
            {
                throw new Exception(result.Message);
            }
        }
        /// <summary>
        /// 采购入库单
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="objects"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public override IEnumerable<AbsSynchroDataInfo> GetK3Datas(Context ctx, List<DynamicObject> objects, ref HttpResponseResult result)
        {

            List<FInStockEntry> entries = null;
            result = new HttpResponseResult();
            result.Success = true;

            if (objects != null && objects.Count > 0)
            {
                entries = new List<FInStockEntry>();

                foreach (var obj in objects)
                {
                    if (obj != null)
                    {
                       
                       List<FInStockEntry> sEntries = GetStockEntry(ctx,obj);

                        entries.AddRange(sEntries);
                    }
                }
            }

            return entries;
        }

        /// <summary>
        /// 物料是否是首页标记
        /// </summary>
        /// <param name="materialId"></param>
        /// <returns></returns>
        public void GetMaterialInfo(Context ctx,ref FInStockEntry entry)
        {
            if (entry != null &&　!string.IsNullOrWhiteSpace(entry.FMaterialId))
            {
                string sql = string.Format(@"/*dialect*/ select c.FNUMBER as F_HS_ListID,a.FNumber as FMaterialId,a.F_HS_YNHomePageMarkup
                                                        from T_BD_MATERIAL a
                                                        inner join HS_T_BD_LISTID c on c.FID = a.F_HS_ListIDNew
                                                        inner join HS_T_BD_LISTID_L d on d.FID = c.FID
                                                        inner join T_ORG_ORGANIZATIONS e on a.FUSEORGID=e.FORGID
                                                        where a.FNUMBER = '{0}'
                                                        and e.FNUMBER = '100'  ", entry.FMaterialId);
                DynamicObjectCollection coll =SQLUtils.GetObjects(ctx,sql);

                if (coll != null && coll.Count > 0)
                {
                    foreach(var item in coll)
                    {
                        if (item != null)
                        {
                            entry.F_HS_ListID = SQLUtils.GetFieldValue(item, "F_HS_ListID");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 采购入库单.明细信息 
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public List<FInStockEntry> GetStockEntry(Context ctx,DynamicObject obj)
        {
            List<FInStockEntry> entries = null;
            FInStockEntry entry = null;

            if (obj != null)
            {
                DynamicObjectCollection coll = obj["InStockEntry"] as DynamicObjectCollection;

                if (coll != null && coll.Count > 0)
                {
                    entries = new List<FInStockEntry>();

                    foreach (var item in coll)
                    {
                        if (item != null)
                        {
                            entry = new FInStockEntry();

                            DynamicObject mat = item["MaterialId"] as DynamicObject;
                            entry.FMaterialId = SQLUtils.GetFieldValue(mat, "Number");
                            entry.SrcNo = entry.FMaterialId;

                            entry.FApproveDate = SQLUtils.GetFieldValue(obj, "ApproveDate");
                            GetMaterialInfo(ctx, ref entry);
                            entries.Add(entry);
                        }
                    }
                } 
            }

            return entries;
        }
      
    }
}
