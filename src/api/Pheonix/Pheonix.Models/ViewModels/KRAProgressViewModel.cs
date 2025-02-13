using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.ViewModels
{
    public class KRAProgressViewModel
    {
        public string VibrantName { get; set; }
        public string Comment { get; set; }
        public bool IsDeleted { get; set; }
        public string CommentedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int CreatedBy { get; set; }
        public int ModifiedBy { get; set; }
        public int KRAGoalId { get; set; }
        public List<KRAProgressDetail> KRAProgressDetails { get; set; }
    }

    public class KRAProgressDetail
    {
        public int Id { get; set; }
        public int KRAId { get; set; }
    }
}