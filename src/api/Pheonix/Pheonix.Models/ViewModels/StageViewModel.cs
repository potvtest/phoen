using System;

namespace Pheonix.Models
{
    public class StageViewModel
    {
        public int ID { get; set; }

        public string PreviousEntry { get; set; }

        public string NewEntry { get; set; }

        public int By { get; set; }

        public DateTime Date { get; set; }

        public bool IsApproved { get; set; }

        public string Comments { get; set; }

        public int ApprovedBy { get; set; }

        public DateTime ApprovedDate { get; set; }

        public int ModuleID { get; set; }

        public int RecordID { get; set; }
    }
}