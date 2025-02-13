using Pheonix.Core.v1.Services.Business;
using Pheonix.Models.VM;
using Pheonix.Models.VM.Classes.Candidate;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Pheonix.Web.Controllers.v1
{
    [RoutePrefix("v1/candidate"), Authorize]
    public class CandidateController : ApiController
    {
        private ICandidateService service;
        public CandidateController(ICandidateService service)
        {
            this.service = service;
        }

        [Route("manage-candidate"), HttpPost]
        public async Task<int> ManageCandidate(CandidateViewModel model)
        {
            return await service.ManageCandidate(model);
        }

        [Route("get-candidate/{ListToDisplay}"), HttpGet]
        public async Task<List<CandidateViewModel>> GetCandidate(string ListToDisplay)
        {
            return await service.GetCandidate(ListToDisplay);
        }

        [Route("get-candidate-history"), HttpGet]
        public async Task<List<CandidateViewModel>> GetCandidateHistory()
        {
            return await service.GetCandidateHistory();
        }

        [HttpPost, Route("migrate-candidate")]
        public async Task<int> MigrateCandidate(CandidateViewModel model)
        {
            string CreateURL = ConfigurationManager.AppSettings["CreateURL"].ToString();
            string clientID = ConfigurationManager.AppSettings["ClientID"].ToString();
            string clSecret = ConfigurationManager.AppSettings["ClientSecret"].ToString();

            var personID = await service.MigrateCandidate(model, CreateURL, clientID, clSecret);

            return personID;
        }

        [HttpGet, Route("get-rrfnumbers")]
        public async Task<IEnumerable<string>> GetRRFNumbers()
        {
            return await service.GetRRFNumbers();
        }

        [Route("get-atscandidate"), HttpPost]
        public async Task<int> SyncATS()
        {
            string ListUrl = ConfigurationManager.AppSettings["ListURL"].ToString();
            string DetailUrl = ConfigurationManager.AppSettings["DetailURL"].ToString();
            string clientID = ConfigurationManager.AppSettings["ClientID"].ToString();
            string clSecret = ConfigurationManager.AppSettings["ClientSecret"].ToString();
            int dayMargin = int.Parse(ConfigurationManager.AppSettings["DayMargin"].ToString());

            return await service.SyncCandidate(ListUrl, DetailUrl, clientID, clSecret, dayMargin);
        }
    }
}
