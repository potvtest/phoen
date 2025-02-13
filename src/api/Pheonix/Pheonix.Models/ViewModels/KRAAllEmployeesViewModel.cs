using System.Collections.Generic;

namespace Pheonix.Models.ViewModels
{
    public class KRAAllEmployeesViewModel
    {
        public int KraInitiationID { get; set; }
        public int Id { get; set; }
        public int? PersonGradeID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Name
        {
            get
            {
                return FirstName + " " + LastName;
            }
        }
        public string WorkLocation { get; set; }
        public string Designation { get; set; }
        public int? PersonDesignationID { get; set; }
        public int CurrentQuarterId { get; set; }
        //public string InitiatedQuarters { get; set; }
        public class InitiatedQuartersGroup
        {
            public bool Q1 { get; set; }
            public bool Q2 { get; set; }
            public bool Q3 { get; set; }
            public bool Q4 { get; set; }
        }
        public IEnumerable<InitiatedQuartersGroup> InitiatedQuartersList { get; set; }
        public int YearId { get; set; }
        public int ReviewerPersonId { get; set; }
        public List<KRAReviewerListViewModel> ReviewerFullName { get; set; }

        public int Q1InitiatedFor { get; set; }
        public int Q2InitiatedFor { get; set; }
        public int Q3InitiatedFor { get; set; }
        public int Q4InitiatiedFor { get; set; }
        public int FirstKRAInitiatedBy { get; set; }
        public int SecondKRAInitiatedBy { get; set; }
        public int ThirdKRAInitiatedBy { get; set; }
        public int FourthKRAInitiatedBy { get; set; }
    }
}
