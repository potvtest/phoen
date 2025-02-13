using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM
{
    public class EmployeePassportViewModel : IBaseModel
    {
        public string NameAsInPassport { get; set; }
        public DateTime? DateOfIssue { get; set; }
        public DateTime? DateOfExpiry { get; set; }
        public string PlaceIssued { get; set; }
        public int BlankPagesLeft { get; set; }
        public bool IsDeleted { get; set; }
        public string PassportNumber { get; set; }
        public int ID { get; set; }
        public int StageStatusID { get; set; }
        public string passportFileUrl { get; set; }
        public int SearchUserID { get; set; }
    }
}
