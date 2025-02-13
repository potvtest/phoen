using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Classes.Employee
{
    public class EmployeeProfileAttachmentDetails
    {
        public int ID { get; set; }
        public int ApprovalID { get; set; }
        public string FieldName { get; set; }
        public string FileName { get; set; }
        public string UniqueFileName { get; set; }
    }
}
