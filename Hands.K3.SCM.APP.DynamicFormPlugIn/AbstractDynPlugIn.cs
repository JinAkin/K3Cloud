using System.Collections.Generic;
using System.Linq;
using Hands.K3.SCM.App.Synchro.Utils.SynchroService;
using Hands.K3.SCM.APP.Entity.EnumType;
using Hands.K3.SCM.APP.Entity.K3WebApi;
using Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject;
using Hands.K3.SCM.APP.Utils.Utils;
using HS.K3.Common.Abbott;
using Kingdee.BOS;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Core.Metadata.FormElement;

namespace Hands.K3.SCM.APP.DynamicFormPlugIn
{
    public abstract class AbstractDynPlugIn : AbstractListPlugIn
    {
        public abstract SynchroDataType DataType { get; }

        public virtual SynchroDirection Direction { get; }

        public virtual IEnumerable<string> SelectedNos
        {

            get
            {
                if (this.ListView != null)
                {
                    if (this.ListView.BusinessInfo.GetForm().Id.CompareTo("BOS_List") == 0)
                    {

                    }
                    if (this.ListView.SelectedRowsInfo.Select(s => s.Number) != null && this.ListView.SelectedRowsInfo.Select(s => s.Number).Count() > 0)
                    {
                        return this.ListView.SelectedRowsInfo.Select(s => s.Number);
                    }
                }
                else
                {
                    if (this.View.BusinessInfo.GetForm().Id.CompareTo("") == 0)
                    {

                    }
                    List<string> numbers = new List<string>();

                    if (this.View.Model.GetValue("FNumber") != null)
                    {
                        numbers.Add(JsonUtils.ConvertObjectToString(this.View.Model.GetValue("FNumber")));
                    }
                    else
                    {
                        numbers.Add(JsonUtils.ConvertObjectToString(this.View.Model.GetValue("FBillNo")));
                    }
                    return numbers;
                }

                return null;
            }

        }
        public virtual IEnumerable<AbsSynchroDataInfo> GetK3Datas(Context ctx, FormOperation oper = null)
        {
            return null;
        }

        public virtual HttpResponseResult SynK3Datas2HC(Context ctx, IEnumerable<AbsSynchroDataInfo> datas = null, FormOperation oper = null)
        {
            if (datas == null)
            {
                return SynchroDataHelper.SynchroDataToHC(ctx, this.DataType, GetK3Datas(ctx),null,false,this.Direction);
            }
            else
            {

                return SynchroDataHelper.SynchroDataToHC(ctx, this.DataType, datas, null, false, this.Direction);
            }
        }

        public HttpResponseResult BatchSynK3Datas2HC(Context ctx, IEnumerable<AbsSynchroDataInfo> datas, int batch)
        {
            List<AbsSynchroDataInfo> lstBacth = new List<AbsSynchroDataInfo>();
            HttpResponseResult result = null;

            if (datas != null && datas.Count() > 0)
            {
                if (datas.Count() >= batch)
                {
                    for (int j = 0; j < datas.Count(); j++)
                    {
                        lstBacth.Add(datas.ElementAt(j));

                        if (j > 0 && (j + 1) % batch == 0)
                        {
                            if (lstBacth != null && lstBacth.Count > 0)
                            {
                                result = SynK3Datas2HC(ctx, lstBacth);
                                lstBacth.Clear();
                            }
                        }
                        else
                        {
                            if (j == datas.Count() - 1)
                            {
                                if (lstBacth != null && lstBacth.Count > 0)
                                {
                                    result = SynK3Datas2HC(ctx, lstBacth);
                                    lstBacth.Clear();
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (datas != null && datas.Count() > 0)
                    {
                        result = SynK3Datas2HC(ctx, datas);
                    }
                }
            }
            return result;
        }
    }
}
