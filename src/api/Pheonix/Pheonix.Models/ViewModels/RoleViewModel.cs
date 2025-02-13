using System;

namespace Pheonix.Models
{
    public class RoleViewModel
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int SliceFromRole { get; set; }

        public DateTime From { get; set; }

        public DateTime To { get; set; }

        public bool IsTemporary { get; set; }
    }
}