using Pheonix.Models.VM.Interfaces.Travel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Classes.Travel
{
    public class UploadedDocumentViewModel : IUploadedDocumentViewModel
    {
        public int id { get; set; }
        public string description { get; set; }
        public string fileName { get; set; }
        public string url { get; set; }
        public int travelId { get; set; }
    }
}
