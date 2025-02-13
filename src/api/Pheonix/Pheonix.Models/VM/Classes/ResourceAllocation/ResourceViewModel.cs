﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Classes.ResourceAllocation
{
    public class ResourceViewModel
    {
        public int PersonId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public List<int> BGCParameters { get; set; }
}
}
