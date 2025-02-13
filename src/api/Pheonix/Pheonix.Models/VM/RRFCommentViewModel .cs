using Pheonix.Models.VM.Interfaces.RRF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM
{
    public class RRFCommentViewModel : IRRFComment
    {
        public int RRFStage { get; set; }
        public string Comments { get; set; }
        public DateTime CommentedDate { get; set; }
    }
}