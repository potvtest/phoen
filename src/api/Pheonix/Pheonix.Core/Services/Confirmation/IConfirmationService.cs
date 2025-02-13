using Pheonix.DBContext.Repository;
using Pheonix.Models;
using Pheonix.Models.Models.Admin;
using Pheonix.Models.Models.Confirmation;
using Pheonix.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Pheonix.Core.Services.Confirmation
{
    public interface IConfirmationService
    {
        int _UserId { get; set; }
        Task<PersonConfirmationViewModel> List(int userId, string additionalParam = "");
        Task<Dictionary<int, string>> GetRecommendations();
        Task<object> Initiate();
        Task<object> Confirm(Confirmations confirmation);
        Task<object> Reject(Confirmations confirmation);
        Task<object> PIP(Confirmations confirmation);
        Task<object> Extend(Confirmations confirmation);
        Task<HttpResponseMessage> Print(Confirmations confirmation, System.Web.HttpResponse resp);
        Task<HttpResponseMessage> PrintDoc(Confirmations confirmation, int userId);
        Task<AdminActionResult> SubmitPIP(Confirmations confirmation);
        Task<object> AutoConfirmEmployee();
        Task<object> SendConfirmationReminderMail();
        Task<PersonConfirmationViewModel> ConfrimationHistory(int userId, bool isHR);
        Task<PersonConfirmationViewModel> InitiatedHistory(int userId);
    }


    public interface IPrintReportInPDF
    {
        Task<HttpResponseMessage> GetPDFPrint(IContextRepository _repo, Confirmations confirmation, string reportName);
        Task<object> SaveDeemedPDF(IContextRepository _repo, Confirmations confirmation, string reportName);
    }
}
