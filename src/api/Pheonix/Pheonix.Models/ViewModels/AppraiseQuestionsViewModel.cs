using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.ViewModels
{
    public class AppraiseQuestionsViewModel
    {
        public List<AppraiseeQuestion> AppraiseeQuestions { get; set; }
        public string Reviewer {get;set;}
        public string Appraiser { get; set; }
    }

    public class AppraiseeQuestion
    {
        public int ID { get; set; }
        public string Question { get; set; }
        public Nullable<int> Levels { get; set; }
        public Nullable<int> Sequence { get; set; }
        public Nullable<bool> IsEditeble { get; set; }
    }
}
