using Pheonix.Core.Services.Confirmation;
using Pheonix.Models;
using Pheonix.Models.Models;
using Pheonix.Models.Models.Confirmation;
using Pheonix.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using Pheonix.Web.Extensions;
using Pheonix.Models.Models.Admin;
using Pheonix.DBContext;

namespace Pheonix.Web.Controllers.v1
{
    [RoutePrefix("v1/confirmations")]
//#if !DEBUG
//    [Authorize]
//#endif
    public class ConfirmationController : ApiController
    {
        IConfirmationService _ConfirmationService;
        public ConfirmationController(IConfirmationService service)
        {
            _ConfirmationService = service;
        }

        //[Route("getreviewfeedbackform"), HttpPost]
        //public List<ConfirmationFeedbackModel> ReviewFeedbackForm()
        //{
        //    return _ConfirmationService.ReviewFeedbackForm();
        //}

        [Route("getrecommendations"), HttpPost]
        public async Task<object> GetRecommendations()
        {
            var result = await _ConfirmationService.GetRecommendations();
            return new EnvelopeModel<Dictionary<int, string>>(result, false, "");
        }

        [Route("for-me"), HttpGet]
        public async Task<object> List()
        {
            var result = await _ConfirmationService.List(GetUserId());
            return new EnvelopeModel<PersonConfirmationViewModel>(result, false, "");
        }

        /// <summary>
        /// Api call for initiation for 
        /// </summary>
        /// <returns></returns>
        [Route("initiate"), HttpPost, HttpGet]
        public async Task<object> Initiate() /// input will be ab COnfirmationStatusObject{ employeeID, comment, date, raitings[4,3,2,4,5] }
        {
            var result = await _ConfirmationService.Initiate();
            return new EnvelopeModel<object>(result, false, "");
        }

        /// <summary>
        /// Api call for confirmation
        /// </summary>
        /// <param name="confirmation"></param>
        /// <returns></returns>
        [Route("confirm"), HttpPost]
        public async Task<object> Confirm([FromBody]Confirmations confirmation)
        {
            _ConfirmationService._UserId = GetUserId();
            var result = await _ConfirmationService.Confirm(confirmation);
            return new EnvelopeModel<object>(result, false, "");
        }

        /// <summary>
        /// Api call for reject confirmation
        /// </summary>
        /// <param name="confirmation"></param>
        /// <returns></returns>
        [Route("reject"), HttpPost]
        public async Task<object> Reject([FromBody]Confirmations confirmation)
        {
            _ConfirmationService._UserId = GetUserId();
            var result = await _ConfirmationService.Reject(confirmation);
            return new EnvelopeModel<object>(result, false, "");
        }

        /// <summary>
        /// Api call for PIP employee
        /// </summary>
        /// <param name="confirmation"></param>
        /// <returns></returns>
        [Route("pip"), HttpPost]
        public async Task<object> PIP([FromBody]Confirmations confirmation)
        {
            _ConfirmationService._UserId = GetUserId();
            var result = await _ConfirmationService.PIP(confirmation);
            return new EnvelopeModel<object>(result, false, "");
        }

        /// <summary>
        /// Api call for extended employee
        /// </summary>
        /// <param name="confirmation"></param>
        /// <returns></returns>
        [Route("extend"), HttpPost]
        public async Task<object> Extend([FromBody]Confirmations confirmation)
        {
            _ConfirmationService._UserId = GetUserId();
            var result = await _ConfirmationService.Extend(confirmation);
            return new EnvelopeModel<object>(result, false, "");
        }


        /// <summary>
        /// Api call for printing document
        /// </summary>
        /// <param name="confirmation"></param>
        /// <returns></returns>
        [Route("Print"), HttpPost]
        public async Task<HttpResponseMessage> PrintConfirmation([FromBody]Confirmations confirmation)
        {
            _ConfirmationService._UserId = GetUserId();
            var resp = System.Web.HttpContext.Current.Response;

            var result = await _ConfirmationService.Print(confirmation, resp);
            return result;

            //return new EnvelopeModel<object>(result, false, "");
        }

        /// <summary>
        /// Api call for printing document
        /// </summary>
        /// <param name="confirmation"></param>
        /// <returns></returns>
        [Route("PrintDoc/{confirmationId}"), HttpGet]
        public async Task<HttpResponseMessage> PrintDoc(int confirmationId = 0)
        {
            var result = await _ConfirmationService.PrintDoc(null, confirmationId);
            return result;
        }

        /// <summary>
        /// provides current user id.
        /// </summary>
        /// <returns></returns>
        int GetUserId()
        {
            return RequestContext.GetClaimInt(ClaimTypes.PrimarySid);
        }

        [Route("SubmitPIP"), HttpPost]
        public async Task<AdminActionResult> SubmitPIP([FromBody]Confirmations confirmation)
        {
            _ConfirmationService._UserId = GetUserId();

            var result = await _ConfirmationService.SubmitPIP(confirmation);
            return result;
        }

        [Route("autoconfirmemployee"), HttpPost]
        public async Task<object> AutoConfirmEmployee()
        {
            var result = await _ConfirmationService.AutoConfirmEmployee();
            return new EnvelopeModel<object>(result, false, "");
        }

        [Route("sendremindermail"), HttpPost]
        public async Task<object> SendConfirmationReminderMail()
        {
            var result = await _ConfirmationService.SendConfirmationReminderMail();
            return new EnvelopeModel<object>(result, false, "");
        }

        [Route("history/{isHR:bool}"), HttpGet]
        public async Task<object> ConfirmationHistory(bool isHR)
        {
            var result = await _ConfirmationService.ConfrimationHistory(RequestContext.GetClaimInt(ClaimTypes.PrimarySid), isHR);
            return new EnvelopeModel<object>(result, false, "");
        }

        [Route("HistoryFeedbackDetails/{id}"), HttpPost]
        public async Task<PersonConfirmation> HistoryFeedbackDetails(int id, PersonConfirmation model)
        {
            PhoenixEntities context = new PhoenixEntities();
            var result = context.PersonConfirmation.FirstOrDefault(r => r.PersonId == id);
            if (result != null)
            {
                result.TrainingFeedback = model.TrainingFeedback;
                result.BehaviourFeedback = model.BehaviourFeedback;
                result.OverallFeedback = model.OverallFeedback;
                await context.SaveChangesAsync();
            }
            return model;
        }

        [Route("Initiatedhistory"), HttpGet]
        public async Task<object> InitiatedHistory()
        {
            var result = await _ConfirmationService.InitiatedHistory(RequestContext.GetClaimInt(ClaimTypes.PrimarySid));
            return new EnvelopeModel<object>(result, false, "");
        }
        [Route("Initiatedhistory/{id}"), HttpPost]
        public async Task<PersonConfirmation> UpdateHr(int id, PersonConfirmation model)
        {
            PhoenixEntities context = new PhoenixEntities();
            var query = from p in context.PersonConfirmation
                        join d in context.Approval on p.PersonId equals d.RequestBy
                        join ad in context.ApprovalDetail on d.ID equals ad.ApprovalID
                        where (p.PersonId == id && d.RequestType == 9)
                        select new { p, d, ad };
            foreach (var m in query)
            {
                m.p.ReportingManagerId = model.ReportingManagerId;
                m.ad.ApproverID = model.ReportingManagerId;
                m.p.ReportingManager = model.ReportingManager;
            }
            await context.SaveChangesAsync();
            return model;
        }
    }
}
