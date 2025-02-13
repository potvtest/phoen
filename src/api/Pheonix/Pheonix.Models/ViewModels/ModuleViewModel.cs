namespace Pheonix.Models
{
    public class ModuleViewModel
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Action { get; set; }

        public bool Active { get; set; }

        public int ParentID { get; set; }
    }
}