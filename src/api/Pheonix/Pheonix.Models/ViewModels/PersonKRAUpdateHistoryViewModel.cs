using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.ViewModels
{
    public class PersonKRAUpdateHistoryViewModel
    {
        public int PersonId { get; set; }
        public bool IsValid { get; set; }
        public string Comments { get; set; }
        public bool IsKRADone { get; set; }
        public DateTime? KRADoneOn { get; set; }
        public int KRAPercentageCompletion { get; set; }
        public int KRAGoalId { get; set; }
        public List<PersonKRAHistory> PersonKRAHistorys { get; set; }
    }

    public class PersonKRAHistory
    {
        public int Id { get; set; }
    }
}
