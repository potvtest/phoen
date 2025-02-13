using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM
{
    public interface IEmployeePassport : IBaseModel
    {
        string NameAsInPassport { get; set; }
        DateTime? DateOfIssue { get; set; }
        DateTime? DateOfExpiry { get; set; }
        string PlaceIssued { get; set; }
        int BlankPagesLeft { get; set; }
        string PassportNumber { get; set; }
        bool IsDeleted { get; set; }
        string passportFileUrl { get; set; }
    }
}
