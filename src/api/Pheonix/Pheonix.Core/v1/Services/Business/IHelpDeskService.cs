using Pheonix.Models.VM;
using Pheonix.Models.VM.Classes.HelpDesk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Core.v1.Services.Business
{
    public interface IHelpDeskService : IDataService
    {
        Task<IEnumerable<HelpDeskListModel>> GetHelpDeskTicketsList(int userId, int ticketType, int status, DateTime currentDate);
        Task<IEnumerable<HelpDeskListModel>> GetTeamHelpDeskTicketsList(DateTime currentDate, HelpDeskListFilterObj helpDeskListFilterObj);
        Task<HelpDeskViewModel> GetTicketDetails(int id, int userId, DateTime todaysDate);
        Task<bool> AddUpdateTicket(int userId, HelpDeskModel helpDeskModel);
        Task<Dictionary<string, List<DropdownItems>>> GetCategoriesDropdowns();
        Task<Dictionary<string, List<DropdownItems>>> GetSubCategories(int categoryId);
        Task<IEnumerable<HelpDeskListModel>> GetTicketsForApproval(int userID);
        Task<bool> ApproveRejectTicket(int userId, HelpDeskListModel helpDeskModel);
        //Task<List<HelpDeskStatusCountViewModel>> GetMyTicketStatus(int userId);
        Task<DateTime> PokeForStatus(int ticketId, DateTime pokedDate);
        Task<List<DropdownItems>> getAssigneeDropdownList(int[] catagory);
    }
}
