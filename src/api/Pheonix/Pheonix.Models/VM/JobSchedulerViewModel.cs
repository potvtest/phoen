using System;
using System.Collections.Generic;

namespace Pheonix.Models.VM
{
    public class JobSchedulerViewModel
    {
        public string SchedulerType { get; set; }
        public List<JobSchedulerDetails> Details { get; set; }
    }

    public class JobSchedulerDetails
    {
        public int SchedulerId { get; set; }
        public string SchedulerType { get; set; }
        public int MonthId { get; set; }
        public DateTime SchedulerDate { get; set; }
        public bool StatusType { get; set; }
    }
}
