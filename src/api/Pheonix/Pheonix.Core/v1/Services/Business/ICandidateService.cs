using Pheonix.Models.VM;
using Pheonix.Models.VM.Classes.Candidate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Core.v1.Services.Business
{
    public interface ICandidateService
    {
        Task<int> ManageCandidate(CandidateViewModel model);
        Task<List<CandidateViewModel>> GetCandidate(string ListToDisplay);
        Task<List<CandidateViewModel>> GetCandidateHistory();
        Task<int> MigrateCandidate(CandidateViewModel model, string CreateURL, string clientID, string clSecret);
        Task<IEnumerable<string>> GetRRFNumbers();
        bool UpdateCandidateStatus(int CandidateId);
        Task<int> SyncCandidate(string ListUrl, string DetailUrl, string clientID, string clSecret, int dayMargin);
    }
}
