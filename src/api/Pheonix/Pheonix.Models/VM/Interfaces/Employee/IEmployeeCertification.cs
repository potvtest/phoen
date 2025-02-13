using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM
{
    public interface IEmployeeCertification : IBaseModel
    {
        string CertificationName { get; set; }
        int CertificationID { get; set; }
        DateTime? CertificationDate { get; set; }
        string Grade { get; set; }
        int StatusID { get; set; }
        bool IsDeleted { get; set; }
        string CertificationNumber { get; set; }
    }
}
