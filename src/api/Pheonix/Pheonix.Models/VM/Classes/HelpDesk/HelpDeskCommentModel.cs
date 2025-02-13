using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Classes.HelpDesk
{
    public class HelpDeskCommentModel
    {
        public int ID { get; set; }
        public int PersonHelpDeskID { get; set; }
        public string Comments { get; set; }
        public int CommentedBy { get; set; }
        public string CommentedByName { get; set; }
        public string CommentedByRole { get; set; }
        public DateTime CommentedDate { get; set; }
        public string AttachedFile { get; set; }
    }
}
