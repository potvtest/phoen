using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.ViewModels
{
    public class ReportAccessConfigViewModel
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string RedirectionText { get; set; }
        public string Description { get; set; }
        public string RouteUrl { get; set; }
        public string ImageUrl { get; set; }
        public string SelectedReportText { get; set; }
        public string ReportHeaderText { get; set; }
        public bool DefaultToAll { get; set; }
        public int? ParentReportID { get; set; }
        public bool? IsActive { get; set; }
    }
}