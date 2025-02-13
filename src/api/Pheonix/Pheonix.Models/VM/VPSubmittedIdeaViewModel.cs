using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM
{
    public class VPSubmittedIdeaViewModel
    {
        public long Id { get; set; }
        public int EmployeeId { get; set; }
        public int UserId { get; set; }
        public string IdeaHeadline { get; set; }
        public string IdeaBrief { get; set; }
        public string IdeaBenefits { get; set; }
        public string EffortRequired { get; set; }
        public string ResourcesRequired { get; set; }
        public string TechnologiesRequired { get; set; }
        public string ExecutionApproach { get; set; }
        public string UniquenessQuotient {get;set;}
        public string DeliveryUnit { get; set; }
        public string IdeaURL { get; set; }
        public string personOrganizationEmail { get; set; }
        public string approverOrganizationEmail { get; set; }
        /// <summary>
        /// Name of the Submitter
        /// </summary>
        public string submitterName { get; set; }
        public short StatusID { get; set; }
        public string CostDesc { get; set; }
    }
}
