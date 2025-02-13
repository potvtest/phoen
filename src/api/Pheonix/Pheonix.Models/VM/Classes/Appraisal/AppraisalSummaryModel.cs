using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Classes.Appraisal
{
    public class AppraisalSummaryModel
    {
        public IEnumerable<PersonApraisal> oneStar { get; set; }
        public IEnumerable<PersonApraisal> twoStar { get; set; }
        public IEnumerable<PersonApraisal> threeStar { get; set; }
        public IEnumerable<PersonApraisal> fourStar { get; set; }
        public IEnumerable<PersonApraisal> fiveStar { get; set; }

    }
}


public class PersonApraisal
{
    public int personId { get; set; }
    public string fullName { get; set; }
    public int status { get; set; }
    public int reviewerRating { get; set; }
    public string image { get; set; }
}