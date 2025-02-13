using Pheonix.Core.Helpers;
using Pheonix.Core.v1.Services;
using Pheonix.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailBuilder
{
    public static class EmailBuilderFactory
    {


        public static EmailBuilder Get(IBasicOperationsService service, Emails email)
        {
            EmailBuilder builder = null;
            switch ((EnumHelpers.EmailTemplateType)email.TemplateType)
            {
                case EnumHelpers.EmailTemplateType.Leave:
                    builder = new LeaveAppliedEmailBuilder(service, email);
                    break;
                case EnumHelpers.EmailTemplateType.ExpenseApproval:
                    builder = new ExpenseApproverEmailBuilder(service, email);
                    break;
                default:
                    builder = new DefaultEmailBuilder(service, email);
                    break;
            }
            return builder;
        }
    }



}
