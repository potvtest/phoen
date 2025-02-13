using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Classes.Travel
{
    public class MoneyTransactionViewModel
    {
        public int id { get; set; }
        public Nullable<int> currencyType { get; set; }
        public string currencyGivenDays { get; set; }
        public string forexCardName { get; set; }
        public string currencygiveninCash { get; set; }
        public string currencyloadedinForexCard { get; set; }
        public string vendorName { get; set; }
        public string totalCurrencyCost { get; set; }
        public string comments { get; set; }
        public DateTime createdDate { get; set; }
        public int travelId { get; set; }
        
    }
}
