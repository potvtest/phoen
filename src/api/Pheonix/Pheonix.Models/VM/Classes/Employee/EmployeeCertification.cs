using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM
{
    public class EmployeeCertification : IEmployeeCertification
    {
        public string CertificationName { get; set; }

        public int CertificationID { get; set; }

        public DateTime? CertificationDate { get; set; }

        public string Grade { get; set; }

        public int StatusID { get; set; }

        public int ID { get; set; }

        public bool IsDeleted { get; set; }

        public int StageStatusID { get; set; }

        public string CertificationNumber { get; set; }

        public int SearchUserID { get; set; }
        public int ApprovalID { get; set; }
    }
}
