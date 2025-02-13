using System.Collections.Generic;

namespace Pheonix.Models.ViewModels
{
    public class KRAInitiationDetail
    {
        public bool IsKRAInitiated { get; set; }
        public bool IsReviewer { get; set; }
        public bool IsKRAHistroryAvailable { get; set; }
        public string PersonIdList { get; set; }
        public List<PersonIdListDetails> PersonIdListDetails { get; set; }
    }

    public class PersonIdListDetails
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
