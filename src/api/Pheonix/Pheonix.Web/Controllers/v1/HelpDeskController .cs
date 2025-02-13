using Pheonix.Core.v1.Services.Business;
using Pheonix.DBContext;
using Pheonix.Models.VM;
using Pheonix.Models.VM.Classes.Expense;
using Pheonix.Web.Providers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Pheonix.Web.Extensions;
using System.Security.Claims;
using System.Web.Http.Cors;
using Pheonix.Models.VM.Classes.HelpDesk;
using Pheonix.Core.v1.Services;
using Pheonix.Core.v1.Services.Approval;

namespace Pheonix.Web.Controllers.v1
{
    [RoutePrefix("v1/HelpDesk")]
    public class HelpDeskController : ApiController
    {
        private IHelpDeskService _service;
        static string uploadFolder = ConfigurationManager.AppSettings["UploadHelpDeskFolder"].ToString();
        static string fileUrl = ConfigurationManager.AppSettings["UploadedTicketUrl"].ToString();
        private IApprovalService approvalService;
        private IBasicOperationsService _opsBasicService;

        public HelpDeskController(IHelpDeskService service, IApprovalService opsApprovalService, IBasicOperationsService opsBasicService)
        {
            _service = service;
            approvalService = opsApprovalService;
            _opsBasicService = opsBasicService;
        }

        [HttpGet, Route("get-tickets-list/{fromDashboard}/{status}/{currentDate}")]
        public async Task<IEnumerable<HelpDeskListModel>> GetTicketList(int fromDashboard, int status, string currentDate)
        {
            var todaysDate = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(Double.Parse(currentDate));
            return await _service.GetHelpDeskTicketsList(Convert.ToInt32(RequestContext.GetClaim(ClaimTypes.PrimarySid)), fromDashboard, status, todaysDate);
        }

        [HttpPost, Route("get-team-tickets-list/{currentDate}")]
        public async Task<IEnumerable<HelpDeskListModel>> GetTicketList(string currentDate, HelpDeskListFilterObj helpDeskListFilterObj)
        {
            var todaysDate = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(Double.Parse(currentDate));
            return await _service.GetTeamHelpDeskTicketsList(todaysDate, helpDeskListFilterObj);
        }

        [HttpGet, Route("get-ticket/{ID}/{currentDate}")]
        public async Task<HelpDeskViewModel> GetTicketDetails(int ID, string currentDate)
        {
            var todaysDate = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(Double.Parse(currentDate));
            return await _service.GetTicketDetails(ID, Convert.ToInt32(RequestContext.GetClaim(ClaimTypes.PrimarySid)), todaysDate);
        }

        [HttpPost, Route("for-addupdate-ticket/{issueDate}")]
        public Task<bool> AddUpdateTicket(HelpDeskModel helpDeskViewModel, string issueDate)
        {
            helpDeskViewModel.IssueDate = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(Double.Parse(issueDate));
            return _service.AddUpdateTicket(Convert.ToInt32(RequestContext.GetClaim(ClaimTypes.PrimarySid)), helpDeskViewModel);
        }

        [HttpPost, Route("for-approve-ticket/{issueDate}")]
        public Task<bool> ApproveRejectTicket(HelpDeskListModel helpDeskListModel, string issueDate)
        {
            helpDeskListModel.IssueDate = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(Double.Parse(issueDate));
            return _service.ApproveRejectTicket(Convert.ToInt32(RequestContext.GetClaim(ClaimTypes.PrimarySid)), helpDeskListModel);
        }

        [HttpGet, Route("get-dropdown")]
        public Task<Dictionary<string, List<DropdownItems>>> GetDropDown()
        {
            return _service.GetCategoriesDropdowns();
        }

        [HttpGet, Route("get-subcategory/{ID}")]
        public Task<Dictionary<string, List<DropdownItems>>> GetSubCategories(int ID)
        {
            return _service.GetSubCategories(ID);
        }

        [HttpGet, Route("poke-for-status/{ID}/{pokedDate}")]
        public Task<DateTime> PokeForStatus(int ID, string pokedDate)
        {
            return _service.PokeForStatus(ID, new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(Double.Parse(pokedDate)));
        }

        [Route("upload"), HttpPost]
        public async Task<IHttpActionResult> Upload()
        {
            if (!Request.Content.IsMimeMultipartContent("form-data"))
            {
                return BadRequest("Unsupported media type");
            }

            var provider = new ImageMultipartFormDataStreamProvider(uploadFolder, RequestContext.GetClaimInt(ClaimTypes.PrimarySid));

            await Request.Content.ReadAsMultipartAsync(provider);

            var photos = new PhotoViewModel();

            foreach (var file in provider.FileData)
            {
                var fileInfo = new FileInfo(file.LocalFileName);

                photos = (new PhotoViewModel
                {
                    Name = fileUrl + fileInfo.Name,
                });
            }

            return Ok(photos);
        }

        [HttpGet, Route("get-status-count")]
        public IHttpActionResult GetStatusCount()
        {
            return Ok(ApprovalStatusFactory.ExecuteAllFactories(_opsBasicService, RequestContext.GetClaimInt(ClaimTypes.PrimarySid)));
        }

        [HttpPost, Route("getAssigneeDropdownList")]
        public async Task<List<DropdownItems>> GetAssigneeDropdowns(int[] catagory)
        {
            return await _service.getAssigneeDropdownList(catagory);
        }
    }
}

