﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.ViewModels
{
   public  class ApprovalsViewModel
    {
        public string Module { get; set; }
        public int Count { get; set; }
        public string Url { get; set; } = string.Empty;
    }
}
