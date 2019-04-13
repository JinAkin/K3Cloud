using Hands.K3.SCM.APP.Entity.K3WebApi;
using Kingdee.BOS;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.App.ServicePlugIn
{
    public class AuditValidator : AbstractValidator
    {
        public HttpResponseResult Result { get; set; }

        public string FormId { get; set; }
        public AuditValidator(HttpResponseResult result,string formId)
        {
            this.Result = result;
            this.FormId = formId;
        }
        public override void Validate(ExtendedDataEntity[] dataEntities, ValidateContext validateContext, Context ctx)
        {
            if (Result != null && !Result.Success && !string.IsNullOrEmpty(Result.Message))
            {
                ValidationErrorInfo info = new ValidationErrorInfo("FBillNo", Convert.ToString(new Guid()), 0, 0, FormId, Result.Message,"");
                validateContext.AddError(new Guid(), info);
            }
        }
    }
}
