
using System;
using System.Collections.Generic;
using Hands.K3.SCM.APP.Entity.SynDataObject.AbsSynObject;
using Kingdee.BOS;
using Hands.K3.SCM.App.Synchro.Base.Abstract;
using HS.K3.Common.Abbott;

namespace Hands.K3.SCM.App.Core.SynchroService.ToHC
{
    public class SynReFundBillToHC : AbstractSynchroToHC
    {
        public override SynchroDataType DataType
        {
            get
            {
                return SynchroDataType.ReFundBill;
            }
        }

        public override IEnumerable<AbsSynchroDataInfo> GetK3Datas(IEnumerable<string> billNos = null,bool flag = true)
        {
            return null;
        }
    }
}
