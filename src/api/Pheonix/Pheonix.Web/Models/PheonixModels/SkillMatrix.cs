using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Pheonix.Web.Models
{
    public class SkillMatrix
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public bool Active { get; set; }

        public string SkillCategory { get; set; }
    }
}