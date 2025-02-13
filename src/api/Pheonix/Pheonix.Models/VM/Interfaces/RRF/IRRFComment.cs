using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Interfaces.RRF
{
    public interface IRRFComment
    {
        int RRFStage { get; set; }
        string Comments { get; set; }
        DateTime CommentedDate { get; set; }
    }
}