
using Pheonix.Models.VM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Core.v1.Services.Business
{
    public interface IRRFService : IDataService
    {
        Task<IEnumerable<RRFReadOnlyViewModel>> GetRRFList(int userID, DateTime start, DateTime end);
        Task<AddEditRRFViewModel> GetRRFDetails(int RRFId);
        Task<AddEditRRFViewModel> UpdateRRFDetails(AddEditRRFViewModel updatedRRFModel);
        Task<AddEditRRFViewModel> AddRRF(AddEditRRFViewModel updatedRRFModel);
        Task<RRFCandidateViewModel> AssignCandidate(RRFCandidateViewModel assignedCandidate);
        Task<int> BifercateRRF(int rrfId, string rrfNumber, int positions);
    }
}