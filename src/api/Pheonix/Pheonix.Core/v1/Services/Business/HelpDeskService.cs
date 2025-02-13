using AutoMapper;
using Pheonix.Core.Helpers;
using Pheonix.Core.v1.Services.Approval;
using Pheonix.Core.v1.Services.Email;
using Pheonix.DBContext;
using Pheonix.DBContext.Repository;
using Pheonix.Models.VM;
using Pheonix.Models.VM.Classes.HelpDesk;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web.Routing;


namespace Pheonix.Core.v1.Services.Business
{
    public class HelpDeskService : IHelpDeskService
    {
        private IBasicOperationsService service;
        private ISaveToStageService _stageService;
        private IEmailService emailService;

        public HelpDeskService(IContextRepository repository, IBasicOperationsService opsService, ISaveToStageService stageService, IEmailService opsEmailService)
        {
            service = opsService;
            _stageService = stageService;
            emailService = opsEmailService;
        }

        public async Task<IEnumerable<HelpDeskListModel>> GetHelpDeskTicketsList(int userId, int ticketType, int status, DateTime currentDate)
        {
            return await Task.Run(() =>
            {
                var db = new PhoenixEntities();
                var helpDeskList = new List<PersonHelpDesk>();
                if (ticketType == 0)
                    helpDeskList = service.Top<PersonHelpDesk>(0, x => x.AssignedTo == userId || x.PersonID == userId).OrderByDescending(x => x.IssueDate).ToList();
                else if (ticketType == 1)
                    helpDeskList = service.Top<PersonHelpDesk>(0, x => x.PersonID == userId).OrderByDescending(x => x.IssueDate).ToList();
                else if (ticketType == 2)
                    helpDeskList = service.Top<PersonHelpDesk>(0, x => x.AssignedTo == userId).OrderByDescending(x => x.IssueDate).ToList();
                else if (ticketType == 3)
                    helpDeskList = service.Top<PersonHelpDesk>(0, x => x.PersonID == userId).OrderByDescending(x => x.IssueDate).Take(2).ToList();
                if (status != 0)
                    helpDeskList = helpDeskList.Where(x => x.Status == status).ToList();
                else
                    helpDeskList = helpDeskList.ToList(); //Added on 18/01/2018 -Swapnali //Updated on 5/05/2022 to get All status tickets


                var helpDeskListViewModel = Mapper.Map<List<PersonHelpDesk>, List<HelpDeskListModel>>(helpDeskList);
                foreach (var model in helpDeskListViewModel)
                {
                    var helpDesk = helpDeskList.Where(x => x.ID == model.ID).FirstOrDefault();
                    if (helpDesk.PokedDate == null)
                        model.IsPokeEnabled = true;
                    else if ((helpDesk.PokedDate.Value.Date - currentDate.Date).TotalDays == 2)
                        model.IsPokeEnabled = true;
                    else
                        model.IsPokeEnabled = false;
                    model.Description = helpDeskList.Where(x => x.ID == model.ID).FirstOrDefault().HelpDeskComments.FirstOrDefault().Comments;
                    model.EmployeeProfile = Mapper.Map<Person, EmployeeBasicProfile>(helpDeskList.Where(x => x.ID == model.ID).First().Person1);
                    var result = db.PersonHelpDeskChangeLog.Where(x => x.PersonHelpDeskID == model.ID).FirstOrDefault();
                    if (model.Status == 6)
                    {
                        if(result!=null)
                        {
                            model.ResolvedDate = db.PersonHelpDeskChangeLog.Where(x => x.PersonHelpDeskID == model.ID).OrderByDescending(x => x.ActionDate).FirstOrDefault().ActionDate;
                        }
                    }
                    if(result!=null)
                    {
                        model.EndDate = db.PersonHelpDeskChangeLog.Where(x => x.PersonHelpDeskID == model.ID).OrderByDescending(x => x.ActionDate).FirstOrDefault().ActionDate;
                    }
                }
                return helpDeskListViewModel;
            });
        }

        public async Task<IEnumerable<HelpDeskListModel>> GetTeamHelpDeskTicketsList(DateTime currentDate, HelpDeskListFilterObj helpDeskListFilterObj)
        {
            return await Task.Run(() =>
            {
                var helpDeskList = new List<PersonHelpDesk>();
                var catId = new List<HelpDeskCategories>();
                if(helpDeskListFilterObj != null)
                {
                    int[] validStatusIdList = { 1, 2, 4, 5 };
                    var statusIDFilter = helpDeskListFilterObj.statusID;
                    var assignedToIDFilter = helpDeskListFilterObj.assignedToID;
                    var raisedByPersonIDFilter = helpDeskListFilterObj.raisedByPersonID;
                    foreach (var item in helpDeskListFilterObj.categoriesIdList)
                {
                    catId = service.All<HelpDeskCategories>().Where(x => x.AssignedRole == item && x.IsDeleted == false).ToList();
                    foreach (var cat in catId)
                    {
                        var temp = new List<PersonHelpDesk>();
                        temp = service.Top<PersonHelpDesk>(0, x => x.CategoryID== cat.ID).OrderByDescending(x => x.IssueDate).ToList();
                            if (statusIDFilter > 0)
                            {
                                if (assignedToIDFilter > 0)
                                {
                                    if (raisedByPersonIDFilter > 0)
                                    {
                                        temp = temp.Where(x => x.Status == statusIDFilter && x.AssignedTo == assignedToIDFilter && x.PersonID == raisedByPersonIDFilter).ToList();
                                    }
                                    else
                                    {
                                        temp = temp.Where(x => x.Status == statusIDFilter && x.AssignedTo == assignedToIDFilter).ToList();
                                    }
                                }
                                else
                                {
                                    if (raisedByPersonIDFilter > 0)
                                    {
                                        temp = temp.Where(x => x.Status == statusIDFilter && x.PersonID == raisedByPersonIDFilter).ToList();
                                    }
                                    else
                                    {
                                        temp = temp.Where(x => x.Status == statusIDFilter).OrderByDescending(x => x.IssueDate).ToList();
                                    }
                                }
                            }
                            else if (statusIDFilter == 0)
                            {

                                if (assignedToIDFilter > 0)
                                {
                                    if (raisedByPersonIDFilter > 0)
                                    {
                                        temp = temp.Where(x => x.AssignedTo == assignedToIDFilter && x.PersonID == raisedByPersonIDFilter && validStatusIdList.Contains(x.Status)).ToList();
                                    }
                                    else
                                    {
                                        temp = temp.Where(x => x.AssignedTo == assignedToIDFilter && validStatusIdList.Contains(x.Status)).ToList();
                                    }
                                }
                                else
                                {
                                    if (raisedByPersonIDFilter > 0)
                                    {
                                        temp = temp.Where(x => x.PersonID == raisedByPersonIDFilter && validStatusIdList.Contains(x.Status)).ToList();
                                    }
                                    else
                                    {
                                        temp = temp.Where(x => validStatusIdList.Contains(x.Status)).ToList();
                                    }
                                }
                            }

                            //if (helpDeskListFilterObj.statusID != 0)
                            //    temp = temp.Where(x => x.Status == helpDeskListFilterObj.statusID).ToList();
                            //else
                            //    temp = temp.Where(x => x.Status == 2).ToList();
                            //temp = temp.Where(x => x.Status == 1 || x.Status == 2 || x.Status == 4 || x.Status == 5).ToList();

                            foreach (var data in temp)
                            {
                                helpDeskList.Add(data);
                            }
                    }
                }
                }

                var helpDeskListViewModel = Mapper.Map<List<PersonHelpDesk>, List<HelpDeskListModel>>(helpDeskList);
                foreach (var model in helpDeskListViewModel)
                {
                    var helpDesk = helpDeskList.Where(x => x.ID == model.ID).FirstOrDefault();
                    if (helpDesk.PokedDate == null)
                        model.IsPokeEnabled = true;
                    else if ((helpDesk.PokedDate.Value.Date - currentDate.Date).TotalDays == 2)
                        model.IsPokeEnabled = true;
                    else
                        model.IsPokeEnabled = false;
                    model.Description = helpDeskList.Where(x => x.ID == model.ID).FirstOrDefault().HelpDeskComments.FirstOrDefault().Comments;
                    model.EmployeeProfile = Mapper.Map<Person, EmployeeBasicProfile>(helpDeskList.Where(x => x.ID == model.ID).First().Person1);
                }
                return helpDeskListViewModel.OrderByDescending(x => x.IssueDate);
            });
        }

        public async Task<HelpDeskViewModel> GetTicketDetails(int id, int userId, DateTime todaysDate)
        {
            var helpDeskListViewModel = new HelpDeskViewModel();
            var helpDesk = service.Top<PersonHelpDesk>(0, x => (x.ID == id)).FirstOrDefault();
            helpDeskListViewModel.Assigness = await this.GetAssigneeDropdowns(helpDesk.HelpDeskCategories.AssignedExecutiveRole, helpDesk.HelpDeskCategories.AssignedRole);
            helpDeskListViewModel.OtherDepartmentAdmin = await this.GetOtherDepartmentAssigneeDropdowns(helpDesk.HelpDeskCategories.AssignedRole, userId);
            return await Task.Run(() =>
            {
                helpDeskListViewModel.HelpDesk = Mapper.Map<PersonHelpDesk, HelpDeskReadOnlyModel>(helpDesk);
                helpDeskListViewModel.HelpDeskComments = Mapper.Map<List<HelpDeskComments>, List<HelpDeskCommentModel>>(helpDesk.HelpDeskComments.ToList());
                helpDeskListViewModel.EmployeeProfile = Mapper.Map<Person, EmployeeBasicProfile>(helpDesk.Person1);

                helpDeskListViewModel.HelpDesk.PhoneExtension = helpDesk.Person1.PersonEmployment.FirstOrDefault().OfficeExtension;
                helpDeskListViewModel.HelpDesk.SeatingLocation = helpDesk.Person1.PersonEmployment.FirstOrDefault().SeatingLocation;
                //Need to optimize this code
                using (PhoenixEntities context = new PhoenixEntities())
                {
                    GetReportingManager_Result data = QueryHelper.GetManger(helpDesk.PersonID);
                    if (data != null)
                        helpDeskListViewModel.HelpDesk.ReportingTo = data.Name;
                    else
                        helpDeskListViewModel.HelpDesk.ReportingTo = string.Empty;

                }
                //if (helpDesk.Duration != 0 && helpDesk.Duration != null && helpDesk.Status == 6)  //For #138317331 : 09/08/2017 Change done to hide remaining day's text if ticket is Resolved/Cancelled/Rejected
                if (helpDesk.Duration != 0 && helpDesk.Duration != null && (helpDesk.Status == 4 || helpDesk.Status == 5) && helpDesk.AssignedTo != null)
                {
                    //var commentedDate = helpDeskListViewModel.HelpDeskComments.Where(co => co.PersonHelpDeskID == helpDesk.ID && co.CommentedBy == helpDesk.AssignedTo).FirstOrDefault().CommentedDate;
                    var commentedDate = helpDeskListViewModel.HelpDeskComments.Where(co => co.PersonHelpDeskID == helpDesk.ID).OrderByDescending(x => x.CommentedDate).FirstOrDefault().CommentedDate;
                    var shouldExpireOn = commentedDate.AddDays(Convert.ToDouble(helpDesk.Duration));
                    helpDeskListViewModel.DurationRemaining = Convert.ToInt32((shouldExpireOn - DateTime.Now).TotalDays);
                }
                else
                    helpDeskListViewModel.DurationRemaining = -9999;

                if (helpDesk.PokedDate == null)
                    helpDeskListViewModel.HelpDesk.IsPokeEnabled = true;
                else if ((helpDesk.PokedDate.Value.Date - todaysDate.Date).TotalDays == 2)
                {
                    helpDeskListViewModel.HelpDesk.IsPokeEnabled = true;
                }
                else
                    helpDeskListViewModel.HelpDesk.IsPokeEnabled = false;

                return helpDeskListViewModel;
            });
        }

        public async Task<bool> AddUpdateTicket(int userId, HelpDeskModel helpDeskModel)
        {
            using (PhoenixEntities context = new PhoenixEntities())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        var isTicketCreatedorUpdated = false;
                        var ticket = Mapper.Map<HelpDeskModel, PersonHelpDesk>(helpDeskModel);

                        //For: #146428051 : Change done to add syatem generated comment if department get changed -09/08/2017
                        string oldDeptName = "";
                        string newDeptName = "";

                        if (helpDeskModel.IsOtherDepartmant)
                        {
                            var ticketTobeUpdated = service.Top<PersonHelpDesk>(1, x => x.ID == ticket.ID).FirstOrDefault();
                            oldDeptName = service.First<HelpDeskCategories>(hc => hc.ID == ticketTobeUpdated.CategoryID).Name;
                            newDeptName = service.First<HelpDeskCategories>(hc => hc.AssignedRole == ticket.AssignedTo).Name;
                        }

                        #region To set Seating Location and Phone Extension
                        var personEmploymentDetl = service.All<PersonEmployment>().Where(x => x.PersonID == userId).First();
                        string _OfficeExtension = "";
                        string _SeatingLocation = "";
                        Boolean isUpdateStatus = false;

                        //if (string.IsNullOrEmpty(personEmploymentDetl.OfficeExtension) || string.IsNullOrEmpty(personEmploymentDetl.SeatingLocation))
                        //{
                        _OfficeExtension = helpDeskModel.PhoneExtension;
                        _SeatingLocation = helpDeskModel.SeatingLocation;

                        var newpersonDetl1 = new PersonEmployment
                        {
                            OfficeExtension = _OfficeExtension,
                            SeatingLocation = _SeatingLocation
                        };

                        isUpdateStatus = service.Update<PersonEmployment>(newpersonDetl1, personEmploymentDetl);

                        if (isUpdateStatus)
                            service.Finalize(true);
                        //}

                        #endregion

                        if (helpDeskModel.ID == 0)
                        {
                            ticket.PersonID = userId;
                            //ticket.IssueDate = DateTime.Now;
                            var lastTicketNumber = service.Top<PersonHelpDesk>(1, x => x.ID != 0).OrderByDescending(x => x.ID).FirstOrDefault() == null ? 1 : service.Top<PersonHelpDesk>(1, x => x.ID != 0).OrderByDescending(x => x.ID).FirstOrDefault().ID + 1;

                            //replace enum class of HelpDeskCategoriesList to table HelpDeskCategories as i have added prefix column
                            var obj = service.First<HelpDeskCategories>(hc => hc.ID == ticket.CategoryID);
                            ticket.number = obj.Prefix + DateTime.Now.Year + lastTicketNumber;

                            //ticket.number = Enum.GetName(typeof(HelpDeskCategoriesList), ticket.CategoryID - 1) + DateTime.Now.Year + lastTicketNumber;
                            var subCategories = service.Top<HelpDeskSubCategories>(1, x => x.ID == helpDeskModel.SubCategoryID).FirstOrDefault();
                            ticket.HelpDeskCategories = service.First<HelpDeskCategories>(x => x.ID == ticket.CategoryID);
                            isTicketCreatedorUpdated = await this.AddNewTicket(ticket, subCategories.IsApprovalRequired);
                        }
                        else
                            isTicketCreatedorUpdated = await this.UpdateTicket(ticket, helpDeskModel.IsOtherDepartmant);

                        if (isTicketCreatedorUpdated)
                            service.Finalize(true);

                        return await Task.Run(() =>
                        {
                            var addedOrUpdatedTicket = new PersonHelpDesk();
                            if (helpDeskModel.ID == 0)
                                addedOrUpdatedTicket = service.Top<PersonHelpDesk>(1, x => x.PersonID == ticket.PersonID).OrderByDescending(x => x.IssueDate).FirstOrDefault();
                            else if (helpDeskModel.ID != 0)
                                addedOrUpdatedTicket = service.Top<PersonHelpDesk>(1, x => x.ID == helpDeskModel.ID).FirstOrDefault();

                            //For: #146428051 : Change done to add syatem generated comment if department get changed -09/08/2017
                            if (helpDeskModel.IsOtherDepartmant)
                                helpDeskModel.comments = "Ticket is moved from " + oldDeptName.ToString() + " to " + newDeptName.ToString() + " department : " + helpDeskModel.comments;

                            //Add comments
                            var newHelpDeskComments = new HelpDeskComments
                            {
                                CommentedBy = userId,
                                CommentedDate = helpDeskModel.IssueDate,
                                Comments = helpDeskModel.comments,
                                PersonHelpDeskID = addedOrUpdatedTicket.ID,
                                AttachedFile = helpDeskModel.AttachedFiles
                            };
                            var isCommentCreated = service.Create<HelpDeskComments>(newHelpDeskComments, x => x.ID == 0);

                            if (isCommentCreated)
                                service.Finalize(true);

                            #region ND: This will be always the requester who has raised the helpdesk ticket/issue
                            var requester = service.First<Person>(x => x.ID == addedOrUpdatedTicket.PersonID);
                            var requesterName = requester.FirstName + " " + requester.LastName;
                            #endregion

                            #region ND: This will be the Assigned-To approver while Creating or Updating the ticket
                            var approverName = string.Empty;
                            var sendEmailTo = string.Empty;
                            var IsdepartmentChange = false;
                            if (addedOrUpdatedTicket.AssignedTo == null)
                            {
                                sendEmailTo = service.First<HelpDeskCategories>(x => x.ID == addedOrUpdatedTicket.CategoryID).EmailGroup;
                                IsdepartmentChange = true;
                            }
                            else
                            {
                                var approver = service.First<Person>(x => x.ID == addedOrUpdatedTicket.AssignedTo);
                                approverName = approver.FirstName + " " + approver.LastName;
                                sendEmailTo = approver.PersonEmployment.First().OrganizationEmail;
                            }
                            #endregion

                            #region ND: This will always be the Logged-In user while Updating the helpdesk ticket
                            var manager = service.First<Person>(x => x.ID == userId);
                            var managerName = manager.FirstName + " " + manager.LastName;
                            var managerComments = service.Top<HelpDeskComments>(0, x => x.PersonHelpDeskID == ticket.ID && x.CommentedBy == userId).OrderByDescending(x => x.ID).First().Comments;
                            #endregion

                            if (ticket.Status == 1 && ticket.HelpDeskSubCategories.IsApprovalRequired)
                            {
                                var reportingTo = (Int32)helpDeskModel.AssignedTo;
                                this.HookApproval(userId, addedOrUpdatedTicket.ID, reportingTo);
                                emailService.SendHelpdeskApproval(ticket.number, ticket, requesterName, requester.PersonEmployment.First().OrganizationEmail, approverName, sendEmailTo, ticket.Status, string.Empty, string.Empty, string.Empty);
                            }

                            else if (ticket.Status == 2)
                            {
                                ticket = GetTicketAfterUpdating(helpDeskModel, ticket);
                                emailService.SendHelpdeskApproval(ticket.number, ticket, requesterName, requester.PersonEmployment.First().OrganizationEmail, approverName, sendEmailTo, ticket.Status, managerName, manager.PersonEmployment.First().OrganizationEmail, managerComments);
                            }
                            else if (IsdepartmentChange == true)
                            {
                                ticket = GetTicketAfterUpdating(helpDeskModel, ticket);
                                emailService.SendHelpdeskApproval(ticket.number, ticket, managerName, manager.PersonEmployment.First().OrganizationEmail, approverName, sendEmailTo, 98, managerName, manager.PersonEmployment.First().OrganizationEmail, managerComments);
                            }
                            else if (approverName != managerName)
                            {
                                ticket = GetTicketAfterUpdating(helpDeskModel, ticket);
                                emailService.SendHelpdeskApproval(ticket.number, ticket, managerName, manager.PersonEmployment.First().OrganizationEmail, approverName, sendEmailTo, 99, managerName, manager.PersonEmployment.First().OrganizationEmail, managerComments);
                            }
                            else if (ticket.Status == 3) // Rejected
                            {
                                ticket = GetTicketAfterUpdating(helpDeskModel, new PersonHelpDesk());
                                emailService.SendHelpdeskApproval(ticket.number, ticket, requesterName, requester.PersonEmployment.First().OrganizationEmail,
                                    managerName, manager.PersonEmployment.First().OrganizationEmail, ticket.Status,
                                    managerName, manager.PersonEmployment.First().OrganizationEmail, managerComments);
                            }
                            else
                            {
                                ticket = GetTicketAfterUpdating(helpDeskModel, new PersonHelpDesk());
                                emailService.SendHelpdeskApproval(ticket.number, ticket, requesterName, requester.PersonEmployment.First().OrganizationEmail, managerName, manager.PersonEmployment.First().OrganizationEmail, ticket.Status, string.Empty, string.Empty, managerComments);
                            }
                            transaction.Commit();
                            return isTicketCreatedorUpdated && isCommentCreated;
                        });

                    }
                    catch
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }

        private PersonHelpDesk GetTicketAfterUpdating(HelpDeskModel helpDeskModel, PersonHelpDesk newticket)
        {
            PersonHelpDesk ticket;
            ticket = service.Top<PersonHelpDesk>(1, x => x.ID == (helpDeskModel.ID != 0 ? helpDeskModel.ID : newticket.ID)).FirstOrDefault();
            ticket.HelpDeskSubCategories = service.Top<HelpDeskSubCategories>(1, x => x.ID == ticket.SubCategoryID).FirstOrDefault();
            ticket.HelpDeskCategories = service.First<HelpDeskCategories>(x => x.ID == ticket.CategoryID);
            ticket.HelpDeskComments = service.Top<HelpDeskComments>(0, x => x.PersonHelpDeskID == (helpDeskModel.ID != 0 ? helpDeskModel.ID : newticket.ID) && x.CommentedBy == ticket.PersonID).ToList();
            return ticket;
        }

        private async Task<bool> AddNewTicket(PersonHelpDesk newHelpDeskTicket, bool IsApprovalRequired)
        {
            return await Task.Run(() =>
            {
                var isTicketCreated = false;
                if (IsApprovalRequired)
                {
                    newHelpDeskTicket.Status = 1;
                    newHelpDeskTicket.IsApprovalRequired = true;
                }
                else
                {
                    newHelpDeskTicket.Status = 2;
                    newHelpDeskTicket.AssignedTo = null;
                }
                isTicketCreated = service.Create<PersonHelpDesk>(newHelpDeskTicket, x => x.ID == newHelpDeskTicket.ID);
                return isTicketCreated;
            });
        }

        private async Task<bool> UpdateTicket(PersonHelpDesk updatedHelpDeskTicket, bool IsOtherDepartmant)
        {
            return await Task.Run(() =>
            {
                var isTicketUpdated = false;
                var catagory = new HelpDeskCategories();
                var ticketTobeUpdated = service.Top<PersonHelpDesk>(1, x => x.ID == updatedHelpDeskTicket.ID).FirstOrDefault();
                if (IsOtherDepartmant == true)
                {
                    catagory = service.First<HelpDeskCategories>(a => a.AssignedRole == updatedHelpDeskTicket.AssignedTo);
                    //var newNombur = Enum.GetName(typeof(HelpDeskCategoriesList), catagory.ID - 1) + DateTime.Now.Year + updatedHelpDeskTicket.ID;

                    //replace enum class of HelpDeskCategoriesList to table HelpDeskCategories as i have added prefix column
                    var obj = service.First<HelpDeskCategories>(hc => hc.ID == catagory.ID);
                    var newNombur = obj.Prefix + DateTime.Now.Year + updatedHelpDeskTicket.ID;

                    updatedHelpDeskTicket.number = newNombur;
                    updatedHelpDeskTicket.AssignedTo = null;
                    ticketTobeUpdated.AssignedTo = null;
                }
                else
                {
                    catagory.ID = ticketTobeUpdated.CategoryID;
                    updatedHelpDeskTicket.AssignedTo = updatedHelpDeskTicket.AssignedTo;
                }
                updatedHelpDeskTicket.CategoryID = catagory.ID;
                updatedHelpDeskTicket.SubCategoryID = ticketTobeUpdated.SubCategoryID;
                updatedHelpDeskTicket.PersonID = ticketTobeUpdated.PersonID;
                updatedHelpDeskTicket.IssueDate = ticketTobeUpdated.IssueDate;
                updatedHelpDeskTicket.PokedDate = ticketTobeUpdated.PokedDate;
                updatedHelpDeskTicket.RequiredCompletionDate = ticketTobeUpdated.RequiredCompletionDate;

                //// ticket no 141767723 ////
                updatedHelpDeskTicket.Severity = ticketTobeUpdated.Severity;
                updatedHelpDeskTicket.Type = ticketTobeUpdated.Type;
                //// ticket no 141767723 ////

                //if (updatedHelpDeskTicket.Status == 2)
                //    updatedHelpDeskTicket.AssignedTo = service.Top<HelpDeskCategories>(1, x => x.ID == updatedHelpDeskTicket.CategoryID).FirstOrDefault().Role.PersonInRole.FirstOrDefault().PersonID;
                if (ticketTobeUpdated.Status == 1 && updatedHelpDeskTicket.Status == 2)
                {
                    updatedHelpDeskTicket.AssignedTo = null;
                    ticketTobeUpdated.AssignedTo = null;
                }
                isTicketUpdated = service.Update<PersonHelpDesk>(updatedHelpDeskTicket, ticketTobeUpdated);
                return isTicketUpdated;
            });
        }

        private int HookApproval(int userId, int recordID, int approverId)
        {
            var approvalStategy = ApprovalStrategyFactory.GetStrategy(ApprovalStrategy.OneLevelOnly, userId);
            approvalStategy.opsService = this.service;
            ApprovalService service = new ApprovalService(this.service);
            int[] fetchedApprovers = new int[1];
            fetchedApprovers[0] = approverId;
            service.SendForApproval(userId, 5, recordID, fetchedApprovers);
            return fetchedApprovers.FirstOrDefault();
        }

        public async Task<IEnumerable<HelpDeskListModel>> GetTicketsForApproval(int userID)
        {
            return await Task.Run(() =>
            {
                IEnumerable<PersonHelpDesk> personHelpDeskTickets;
                using (PhoenixEntities context = new PhoenixEntities())
                {
                    var approvalIds = QueryHelper.GetApprovalsForUser(userID, 5);
                    personHelpDeskTickets = service.Top<PersonHelpDesk>(0, a => approvalIds.Contains(a.ID)).OrderByDescending(t => t.IssueDate);
                    var personHelpDeskModelList = Mapper.Map<IEnumerable<PersonHelpDesk>, IEnumerable<HelpDeskListModel>>(personHelpDeskTickets);
                    foreach (var model in personHelpDeskModelList)
                    {
                        model.Description = personHelpDeskTickets.Where(x => x.ID == model.ID).FirstOrDefault().HelpDeskComments.OrderBy(t => t.CommentedDate).FirstOrDefault().Comments;
                        model.EmployeeProfile = Mapper.Map<Person, EmployeeBasicProfile>(personHelpDeskTickets.Where(x => x.ID == model.ID).First().Person1);
                    }

                    return personHelpDeskModelList;
                }
            });
        }

        public Task<Dictionary<string, List<DropdownItems>>> GetCategoriesDropdowns()
        {
            Dictionary<string, List<DropdownItems>> Items = new Dictionary<string, List<DropdownItems>>();
            List<DropdownItems> lstItems = new List<DropdownItems>();

            var categories = service.All<HelpDeskCategories>().Where(hc => hc.IsDeleted == false);

            foreach (var item in categories)
            {
                DropdownItems dropdownItem = new DropdownItems
                {
                    ID = item.ID,
                    Text = item.Name.Trim(),
                    PrefixText = item.Prefix,
                    AssignRole = item.AssignedRole

                };
                lstItems.Add(dropdownItem);
            }
            lstItems = lstItems.OrderBy(cat => cat.Text).ToList();
            Items.Add(HelpDeskDropDownType.Categories.ToString(), lstItems.OrderBy(cat => cat.Text).ToList());
            return Task.Run(() => { return Items; });
        }

        public Task<Dictionary<string, List<DropdownItems>>> GetSubCategories(int categoryId)
        {
            Dictionary<string, List<DropdownItems>> Items = new Dictionary<string, List<DropdownItems>>();
            List<DropdownItems> lstItems = new List<DropdownItems>();

            var subCategories = service.Top<HelpDeskSubCategories>(1, x => x.CategoryID == categoryId && x.IsDeleted==false);
            lstItems = new List<DropdownItems>();
            foreach (var item in subCategories)
            {
                DropdownItems dropdownItem = new DropdownItems
                {
                    ID = item.ID,
                    Text = item.Name.Trim()
                };
                lstItems.Add(dropdownItem);
            }
            Items.Add(HelpDeskDropDownType.SubCategories.ToString(), lstItems.OrderBy(cat => cat.Text).ToList());

            return Task.Run(() => { return Items; });
        }

        public async Task<bool> ApproveRejectTicket(int userId, HelpDeskListModel helpDeskModel)
        {

            ApprovalService approvalService = new ApprovalService(this.service);
            var updatedApproval = await UpdateHookedApproval(userId, helpDeskModel.ID, helpDeskModel.Status, helpDeskModel.Comments);

            //// ticket no 141767723 ////
            int Severity = 0;
            switch (helpDeskModel.Severity)
            {
                case "High":
                    Severity = 0;
                    break;
                case "Medium":
                    Severity = 1;
                    break;
                case "Low":
                    Severity = 2;
                    break;
            }

            int Type = 0;
            switch (helpDeskModel.Type)
            {
                case "Request":
                    Type = 0;
                    break;
                case "Issue":
                    Type = 1;
                    break;
            }
            //// ticket no 141767723 ////

            var newHelpDeskTicket = new PersonHelpDesk
            {
                ID = helpDeskModel.ID,
                Duration = helpDeskModel.Status == 1 ? helpDeskModel.Duration : null,
                Status = helpDeskModel.Status == 1 ? 2 : 3,
                IssueDate = helpDeskModel.IssueDate,
                //// ticket no 141767723 ////
                Severity = Severity,
                Type = Type
                //// ticket no 141767723 ////
            };
            var personHelpDeskModel = Mapper.Map<PersonHelpDesk, HelpDeskModel>(newHelpDeskTicket);
            personHelpDeskModel.comments = helpDeskModel.Comments;
            return await this.AddUpdateTicket(userId, personHelpDeskModel);

        }

        private async Task<int> UpdateHookedApproval(int userId, int recordID, int statusID, string statusComment)
        {
            return await Task.Run(() =>
            {
                ApprovalService approvalService = new ApprovalService(this.service);
                return approvalService.UpdateApproval(userId, 5, recordID, statusID, statusComment);
            });
        }

        //public async Task<List<HelpDeskStatusCountViewModel>> GetMyTicketStatus(int userId)
        //{
        //    return await Task.Run(() =>
        //    {
        //        var helpDeskCountViewModel = new List<HelpDeskStatusCountViewModel>();
        //        var personHelpDesk = service.Top<PersonHelpDesk>(0, x => (x.AssignedTo == userId)).ToList();
        //        int[] statusArray = { 2, 4, 5, 6, 7 };
        //        for (int i = 0; i < statusArray.Length; i++)
        //        {
        //            helpDeskCountViewModel.Add(new HelpDeskStatusCountViewModel
        //            {
        //                Count = personHelpDesk.Where(hp => hp.Status == statusArray[i]).Count(),
        //                Status = statusArray[i],
        //                Url = string.Empty
        //            });
        //        }
        //        return helpDeskCountViewModel;
        //    });
        //}

        public async Task<DateTime> PokeForStatus(int ticketId, DateTime pokedDate)
        {
            var ticket = service.Top<PersonHelpDesk>(0, x => (x.ID == ticketId)).FirstOrDefault();
            var assignedToEmailAddress = string.Empty;
            var assignedToName = string.Empty;
            if (ticket.AssignedTo == null)
            {
                assignedToEmailAddress = ticket.HelpDeskCategories.EmailGroup;
                assignedToName = ticket.HelpDeskCategories.Name;
            }
            else
            {
                var assignedTo = service.First<Person>(x => x.ID == ticket.AssignedTo);
                assignedToEmailAddress = assignedTo.PersonEmployment.First().OrganizationEmail;
                assignedToName = string.Format("{0} {1}", assignedTo.FirstName, assignedTo.LastName);
            }
            var requester = service.First<Person>(x => x.ID == ticket.PersonID);
            var requesterName = string.Format("{0} {1}", requester.FirstName, requester.LastName);
            ticket.PokedDate = pokedDate;

            var isTicketUpdated = await this.UpdateTicket(ticket, false);
            return await Task.Run(() =>
            {
                var newHelpDeskComments = new HelpDeskComments
                {
                    CommentedBy = ticket.PersonID,
                    CommentedDate = pokedDate,
                    Comments = "Please provide an exact status of the ticket",
                    PersonHelpDeskID = ticket.ID,
                };
                var isCommentCreated = service.Create<HelpDeskComments>(newHelpDeskComments, x => x.ID == 0);
                if (isCommentCreated)
                    service.Finalize(true);
                emailService.SendHelpdeskApproval(ticket.number, ticket, requesterName, requester.PersonEmployment.First().OrganizationEmail, assignedToName, assignedToEmailAddress, 8, string.Empty, string.Empty, string.Empty);
                return DateTime.Now;
            });
        }

        public async Task<List<DropdownItems>> GetAssigneeDropdowns(int? roleId, int? adminRoleId)
        {
            return await Task.Run(() =>
            {
                IEnumerable<DropdownItems> lstItems = service.All<PersonInRole>(hc => hc.Person != null && (hc.RoleID == roleId || hc.RoleID == adminRoleId))
                                                        .GroupBy(g => g.Person)
                                                        .Select(t => t.First())
                                                        .Select(t => new DropdownItems()
                                                        {
                                                            ID = t.Person.ID,
                                                            Text = string.Concat(t.Person.FirstName, " ", t.Person.LastName)
                                                        });
                return lstItems.Distinct().ToList();
            });
        }

        public async Task<List<DropdownItems>> getAssigneeDropdownList(int[] catagories)
        {
            List<DropdownItems> finalAssignessList = new List<DropdownItems>();
            var categoryIds = new List<HelpDeskCategories>();
            foreach (var category in catagories)
            {
                List<DropdownItems> tempAssignessList = new List<DropdownItems>();
                categoryIds = service.All<HelpDeskCategories>().Where(x => x.AssignedRole == category).ToList();
                foreach (var categoryDBRow in categoryIds)
                {
                    tempAssignessList = await this.GetAssigneeDropdowns(categoryDBRow.AssignedExecutiveRole, categoryDBRow.AssignedRole);
                }
                finalAssignessList.AddRange(tempAssignessList);
            }
            return finalAssignessList;
        }

        public async Task<List<DropdownItems>> GetOtherDepartmentAssigneeDropdowns(int? roleId, int userId)
        {
            return await Task.Run(() =>
            {
                using (PhoenixEntities context = new PhoenixEntities())
                {
                    List<DropdownItems> lstAssignToOtherDepartment = new List<DropdownItems>();
                    /* Old Code*/
                    //int[] roleIdList = new int[] { 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38 };
                    /* End */

                    /* New Code* Ticket number - 146612987 */
                    int[] roleIdList = new int[] { 22, 23, 24, 25, 26, 28, 29, 30, 41 };
                    /* End */

                    var role = service.All<Role>();
                    foreach (var item in roleIdList)
                    {
                        var userRole = role.Where(pn => pn.ID == item).FirstOrDefault();
                        var pesonInRole = service.First<PersonInRole>(x => x.RoleID == item);
                        if (pesonInRole != null && (pesonInRole.PersonID != userId))
                        {
                            DropdownItems dropdownItem = new DropdownItems
                            {
                                ID = userRole.ID,
                                Text = userRole.Name
                            };
                            lstAssignToOtherDepartment.Add(dropdownItem);
                        }
                    }
                    lstAssignToOtherDepartment = lstAssignToOtherDepartment.GroupBy(x => x.ID).Select(y => y.First()).ToList();
                    return lstAssignToOtherDepartment;

                }
            });
        }

        public Task<IEnumerable<T>> List<T>(string filters = null)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<T>> Single<T>(string filters = null)
        {
            throw new NotImplementedException();
        }

        public int Add<T>(T model)
        {
            throw new NotImplementedException();
        }

        public int Update<T>(T model)
        {
            throw new NotImplementedException();
        }

        public int Delete<T>(int id)
        {
            throw new NotImplementedException();
        }
    }
}


public enum HelpDeskDropDownType
{
    Categories,
    SubCategories,
}