using Pheonix.Core.v1.Services.Business;
using Pheonix.Models.VM;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Linq;
using Pheonix.Models;


namespace Pheonix.Web.Controllers.v1
{
    [RoutePrefix("v1/RRF")]
    public class RRFController : ApiController
    {
        private RRFService _service;

        public RRFController(RRFService service)
        {
            _service = service;
        }

        [HttpGet, Route("for-user-{ID}/{startDate}/{endDate}")]
        public async Task<IEnumerable<RRFReadOnlyViewModel>> GetRRFList(int ID, DateTime startDate, DateTime endDate)
        {
            return await _service.GetRRFList(ID, startDate, endDate);
        }

        [HttpGet, Route("for-RRF-{ID}")]
        public async Task<AddEditRRFViewModel> GetRRFDetails(int ID)
        {
            return await _service.GetRRFDetails(ID);
        }

        [HttpPost, Route("for-update-RRF")]
        public async Task<AddEditRRFViewModel> UpdateRRF(AddEditRRFViewModel updatedRRFModel)
        {
            return await _service.UpdateRRFDetails(updatedRRFModel);
        }

        [HttpPost, Route("for-add-RRF")]
        public async Task<AddEditRRFViewModel> AddRRF(AddEditRRFViewModel updatedRRFModel)
        {
            return await _service.AddRRF(updatedRRFModel);
        }

        [HttpGet, Route("bifercateRRF-{ID}/{rrfNumber}/{positions}")]
        public async Task<int> BifercateRRF(int ID, string rrfNumber, int positions)
        {
            return await _service.BifercateRRF(ID, rrfNumber, positions);
        }

    }
}