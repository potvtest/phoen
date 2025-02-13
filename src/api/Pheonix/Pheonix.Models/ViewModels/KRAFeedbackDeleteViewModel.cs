using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.ViewModels
{
    public class KRAFeedbackDeleteViewModel
    {
        public int Id { get; set; }
        public int KraGoalId { get; set; }
        public int UpdatedBy { get; set; }
    }
}
