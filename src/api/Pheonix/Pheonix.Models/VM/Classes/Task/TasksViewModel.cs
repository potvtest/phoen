using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Classes.Task
{

    public class TasksViewModel
    {
        public int id { get; set; }

        public string srno { get; set; }

        public string taskname { get; set; }

        public string featuretype { get; set; }

        public string description { get; set; }

        public string defaultFields { get; set; }

        public string acceptance { get; set; }

        public Nullable<DateTime> createdDate { get; set; }

        public bool isdeleted { get; set; }

        public string hours { get; set; }

        public string status { get; set; }

        public Nullable<int> projectId { get; set; }



    }
}
