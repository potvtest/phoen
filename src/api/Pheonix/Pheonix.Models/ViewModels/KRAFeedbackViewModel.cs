using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Pheonix.Models.ViewModels
{
	public class KRAFeedbackViewModel
	{
		public int Id { get; set; }
		public int KraGoalId { get; set; }
		public int Year { get; set; }
		public string Quarter { get; set; }
		public int PersonId { get; set; }
		public string Feedback { get; set; }
		public int CreatedBy { get; set; }
		public int UpdatedBy { get; set; }
		public Nullable<System.DateTime> CompletionDate { get; set; }
	}
}