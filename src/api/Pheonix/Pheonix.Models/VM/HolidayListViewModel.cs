using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pheonix.Models.VM.Interfaces;


namespace Pheonix.Models.VM
{
    public class HolidayListViewModel : IHolidayList
    {
        public int ID { get; set; }
        public DateTime HolidayDate { get; set; }
        public string Description { get; set; }
        public int HolidayType { get; set; }
    }

    public class HolidaysListViewModel
    {
        public List<HolidayListViewModel> MumbaiHolidayDate { get; set; }
        public string MumbaiHolidayNote { get; set; }
        public string MumbaiDayLightSavingNote { get; set; }
        public int[] MumbaiSelectedHolidays { get; set; }
        public List<HolidayListViewModel> BangloreHolidayDate { get; set; }
        public string BangloreHolidayNote { get; set; }
        public string BangloreDayLightSavingNote { get; set; }
        public int[] BangloreSelectedHolidays { get; set; }
        public List<HolidayListViewModel> UdaipurHolidayDate { get; set; }
        public string UdaipurHolidayNote { get; set; }
        public string UdaipurDayLightSavingNote { get; set; }
        public int[] UdaipurSelectedHolidays { get; set; }
        public List<HolidayListViewModel> USAHolidayDate { get; set; }
        public string USHolidayNote { get; set; }
        public string USDayLightSavingNote { get; set; }
        public int[] USSelectedHolidays { get; set; }
        public List<HolidayListViewModel> VadodaraHolidayDate { get; set; }
        public string VadodaraHolidayNote { get; set; }
        public string VadodaraDayLightSavingNote { get; set; }
        public int[] VadodaraSelectedHolidays { get; set; }
    }
}
