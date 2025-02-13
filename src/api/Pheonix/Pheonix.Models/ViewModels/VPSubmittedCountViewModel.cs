using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.ViewModels
{
    public class VPSubmittedCountViewModel
    {
        public int NoofIdeasSubmittedbyme { get; set; }
        public int ReviewedCountbyme { get; set; }
        public int selfDraftIdeaCount { get; set; }
        public int selfShrtlistedIdeaCount { get; set; }
        public int selfInReviewIdeaCount { get; set; }
        public int selfNAI1IdeaCount { get; set; }
        public int selfNAI2IdeaCount { get; set; }
        public int selfOnHold1IdeaCount { get; set; }
        public int selfOnHold2IdeaCount { get; set; }
        public int selfReject1IdeaCount { get; set; }
        public int selfReject2IdeaCount { get; set; }
        public int Sponsoredbyme { get; set; }
        public int selfReqReviewIdeaCount { get; set; }
        public int selfDeprcatedIdeaCount { get; set; }
        public int Completedbyme { get; set; }
        public int selfDeferredIdeaCount { get; set; }
        public int selfInExeIdeaCount { get; set; }
        //public int selfRejectedIdeaCount { get; set; }

        public int ALLNoofIdeasSubmitted { get; set; }
        public int ALLReviewedCount { get; set; }
        public int allRejectedCount { get; set; }
        public int ALLSponsored { get; set; }
        public int ALLCompleted { get; set; }

        public int overAllShrtlistedIdeaCount { get; set; }
        public int overAllInReviewIdeaCount { get; set; }
        public int overAllNAI1IdeaCount { get; set; }
        public int overAllNAI2IdeaCount { get; set; }
        public int overAllOnHold1IdeaCount { get; set; }
        public int overAllOnHold2IdeaCount { get; set; }
        public int overAllReject1IdeaCount { get; set; }
        public int overAllReject2IdeaCount { get; set; }
        public int overAllReqReviewIdeaCount { get; set; }
        public int overAllDeprcatedIdeaCount { get; set; }
        public int overAllDeferredIdeaCount { get; set; }
        public int overAllInExeIdeaCount { get; set; }
    }
}