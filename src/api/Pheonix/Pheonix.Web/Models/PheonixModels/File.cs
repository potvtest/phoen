namespace Pheonix.Web.Models
{
    public class File
    {
        public int ID { get; set; }

        public string Location { get; set; }

        public int ModuleID { get; set; }

        public int RecordID { get; set; }

        public string Extension { get; set; }

        public string MimeType { get; set; }
    }
}