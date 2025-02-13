using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.ViewModels
{
    public class KRAFileAttachment
    {
        public string FileURL { get; set; }
        public string FileName { get; set; }
        public DateTime FileUploadedOn { get; set; }
        public int? FileUploadedBy { get; set; }
        public bool IsDeleted { get; set; }
        public int KRAGoalId { get; set; }
        public List<KRAAttachment> KRAAttachments { get; set; }
    }

    public class KRAAttachment
    {
        public int Id { get; set; }
        public int KRAId { get; set; }
    }
}
