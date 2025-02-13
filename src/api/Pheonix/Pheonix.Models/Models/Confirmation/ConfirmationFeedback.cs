using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.Models.Confirmation
{
    public class ConfirmationFeedback
    {
        public ConfirmationFeedback()
        {
            TrainingFeedback = "";
            BehaviourFeedback = "";
            ConfirmationState = 0;
            ExtendedTill = 0;
            PIPTill = 0;
            PersonId = 0;
        }
        public string TrainingFeedback { get; set; }
        public string BehaviourFeedback { get; set; }
        public string OverallFeedback { get; set; }
        public int ConfirmationState { get; set; }
        public int ExtendedTill { get; set; }
        public int PIPTill { get; set; }
        public int PersonId { get; set; }
        public bool IsHRReviewDone { get; set; }
    }
}
