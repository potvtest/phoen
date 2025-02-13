using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Interfaces.RRF
{
    public interface IRRFCandidate
    {
        int ID { get; set; }
        int CandidateSource { get; set; }
        int EmployeeCode { get; set; }
        bool IsReplacement { get; set; }
        Nullable<int> ReplacementFor { get; set; }
        int CandidateId { get; set; }
        int CandidiateStage { get; set; }
    }
}