using Pheonix.Models.VM.Interfaces.RRF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM
{
    public class RRFCandidateViewModel : IRRFCandidate
    {
        public int ID { get; set; }

        public int RRFID { get; set; }

        public int CandidateSource { get; set; }

        public int EmployeeCode { get; set; }

        public bool IsReplacement { get; set; }

        public int? ReplacementFor { get; set; }

        public int CandidateId { get; set; }

        public int CandidiateStage { get; set; }

    }
}