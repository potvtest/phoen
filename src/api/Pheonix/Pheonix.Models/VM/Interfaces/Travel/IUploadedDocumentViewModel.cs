using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Interfaces.Travel
{
    public interface IUploadedDocumentViewModel
    {
        int id { get; set; }
        string description { get; set; }
        string url { get; set; }
        int travelId { get; set; }
    }
}
