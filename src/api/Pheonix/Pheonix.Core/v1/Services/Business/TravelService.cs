using AutoMapper;
using Pheonix.Core.Helpers;
using Pheonix.Core.v1.Services.Approval;
using Pheonix.Core.v1.Services.Email;
using Pheonix.DBContext;
using Pheonix.DBContext.Repository;
using Pheonix.Models;
using Pheonix.Models.VM;
using Pheonix.Models.VM.Classes;
using Pheonix.Models.VM.Classes.Travel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Pheonix.Core.v1.Services.Business
{
    public class TravelService : ITravelService
    {
        private IBasicOperationsService service;
        private IEmailService emailService;
        static int travelRequestType = 6;

        public TravelService(IBasicOperationsService opsService, IEmailService opsEmailService)
        {
            service = opsService;
            emailService = opsEmailService;
        }

        public Task<bool> SaveTravel(TravelViewModel model, int id)
        {
            return Task.Run(() =>
            {
                bool isPassportCreated = false;

                var travel = Mapper.Map<TravelViewModel, Traveller>(model);

                var person = service.First<Person>(x => x.ID == id);
                travel.Person = person;

                //For :148333193 on 24/08/2017 Changes done to fetch client list from Customer table instead of ClientName
                //travel.ClientInformation.ClientName = service.First<ClientName>(x => x.ClientNameId == model.clientInformation.clientId);                
                ClientName lstItems = new ClientName();
                var clientNames = service.First<Customer>(x => x.ID == model.clientInformation.clientId);
                lstItems.Name = clientNames.Name.Trim();
                lstItems.ClientNameId = clientNames.ID;
                lstItems.IsDeleted = clientNames.IsDeleted;

                travel.ClientInformation.ClientName = lstItems;

                var isCreated = service.Create<Traveller>(travel, x => x.Id == 0);

                isPassportCreated = true;

                if (isCreated && isPassportCreated)
                {
                    service.Finalize(isCreated);
                    HookApproval(id, travel.Id, travel.PrimaryApproverId.Value);
                    emailService.SendTravelApproval(travel, person, ApprovalStage.Submitted, "");
                }

                return isCreated;
            });
        }

        public Task<List<TravelViewModel>> GetMyTravelRequests(int id)
        {
            using (PhoenixEntities context = new PhoenixEntities())
            {
                var travel = service.Top<Traveller>(0, x => x.PersonId == id);
                List<TravelViewModel> lstTravel = new List<TravelViewModel>();
                List<GetMySubmittedTravel_Result> travelStatus = null;
                int totalStages = 0;
                travelStatus = QueryHelper.GetMySubmittedTravel( id);

                foreach (var item in travel.OrderByDescending(x => x.CreatedDate))
                {
                    var mappedTravel = Mapper.Map<Traveller, TravelViewModel>(item);
                    totalStages = travelStatus.Where(x => x.RequestID == item.Id).GroupBy(p => p.RequestID, (key, g) => new { stageCount = g.ToList().Count() }).FirstOrDefault().stageCount;
                    var stages = travelStatus.Where(x => x.RequestID == item.Id);

                    if (stages.Any())
                    {
                        mappedTravel.travelStatus = stages.Select(x => new StageStatus { Stage = x.Stage.Value, Status = x.Status.Value, comment = x.StatusComment });
                        foreach (var stage in stages)
                        {
                            if (stage.Stage == 1)
                            {
                                var primaryID = stage.ApproverID;
                                var primaryApproverName = context.People.Where(x => x.ID == primaryID).Select(x => x.FirstName).FirstOrDefault() +
                                              ' ' + context.People.Where(x => x.ID == primaryID).Select(x => x.LastName).FirstOrDefault();
                                mappedTravel.primaryApproverName = primaryApproverName;
                            }
                            else if (stage.Stage == 2)
                            {
                                var adminID = stage.ApproverID;
                                var adminApproverName = context.People.Where(x => x.ID == adminID).Select(x => x.FirstName).FirstOrDefault() +
                                              ' ' + context.People.Where(x => x.ID == adminID).Select(x => x.LastName).FirstOrDefault();
                                mappedTravel.adminApproverName = adminApproverName;
                            }
                        }
                    }
                    var traveller = travel.FirstOrDefault(x => x.Id == item.Id);
                    if (traveller != null)
                    {
                        mappedTravel.employeeProfile = Mapper.Map<Person, EmployeeBasicProfile>(traveller.Person);
                    }

                    mappedTravel.totalStages = totalStages;
                    lstTravel.Add(mappedTravel);
                }

                return Task.Run(() => { return lstTravel; });
            }
        }

        public Task<EmployeeEmergencyContact> SaveEmergencyContacts(EmployeeEmergencyContact model, int id)
        {
            return Task.Run(() =>
            {
                var contactlist = new PersonContact();
                if (model != null && model.ID != 0)
                {
                    contactlist = service.First<PersonContact>(x => x.PersonID == id && x.IsDeleted == false && x.ID == model.ID);
                }

                if (model.ID != 0)
                {
                    var oldModel = service.First<PersonContact>(x => x.ID == model.ID);
                    var newModel = Mapper.Map<EmployeeEmergencyContact, PersonContact>(model);
                    var updated = service.Update<PersonContact>(newModel, oldModel);
                    if (updated)
                        service.Finalize(updated);
                    contactlist = service.First<PersonContact>(x => x.PersonID == id && x.IsDeleted == false && x.ID == model.ID);

                }
                else
                {
                    var contact = Mapper.Map<EmployeeEmergencyContact, PersonContact>(model);
                    contact.Person = service.First<Person>(x => x.ID == id);
                    var created = service.Create<PersonContact>(contact, x => id == 0);

                    if (created)
                        service.Finalize(created);
                    contactlist = service.Top<PersonContact>(0, x => x.Person.ID == id && x.IsDeleted == false).OrderByDescending(x => x.ID).First();
                }

                if (contactlist != null)
                    model = Mapper.Map<PersonContact, EmployeeEmergencyContact>(contactlist);

                return model;
            });
        }

        public Task<bool> DeleteEmergencyContact(EmployeeEmergencyContact model, int id)
        {
            var contact = service.First<PersonContact>(x => x.ID == model.ID);
            var deleted = service.SoftRemove<PersonContact>(contact, x => x.ID == model.ID);

            if (deleted)
                service.Finalize(deleted);

            return Task.Run(() => { return deleted; });
        }

        public Task<EmployeeVisa> SaveVisaDetail(EmployeeVisa model, int id)
        {
            return Task.Run(() =>
            {
                var personVisa = new PersonVisa();
                if (model != null && model.ID != 0)
                {
                    personVisa = service.First<PersonVisa>(x => x.Person.ID == id && x.IsDeleted == false && x.ID == model.ID);
                }

                if (model.ID != 0)
                {
                    PersonVisa visa = new PersonVisa();

                    using (PhoenixEntities entites = new PhoenixEntities())
                    {
                        visa = entites.PersonVisa.Where(x => x.ID == model.ID).FirstOrDefault();
                        visa.Person = entites.People.Where(x => x.ID == id).FirstOrDefault();
                        visa.ValidTill = model.ValidTill;
                        visa.VisaType = entites.VisaType.Where(x => x.ID == model.VisaTypeID).FirstOrDefault();
                        visa.Country = entites.Country.Where(x => x.ID == model.CountryID).FirstOrDefault();
                        visa.VisaFileURL = model.visaFileUrl;
                        entites.SaveChanges();
                    }
                    personVisa = visa;

                }
                else
                {
                    var visa = Mapper.Map<EmployeeVisa, PersonVisa>(model);
                    visa.Person = service.First<Person>(x => x.ID == id);
                    visa.VisaType = service.First<VisaType>(x => x.ID == model.VisaTypeID);
                    visa.Country = service.First<Country>(x => x.ID == model.CountryID);
                    var created = service.Create<PersonVisa>(visa, x => id == 0);

                    if (created)
                        service.Finalize(created);
                    personVisa = service.Top<PersonVisa>(0, x => x.Person.ID == id && x.IsDeleted == false).OrderByDescending(x => x.ID).First();
                }

                if (personVisa != null)
                    model = Mapper.Map<PersonVisa, EmployeeVisa>(personVisa);

                return model;

            });
        }

        public Task<bool> DeleteEmergencyContact(EmployeeVisa model, int id)
        {
            var visa = service.First<PersonVisa>(x => x.ID == model.ID);
            var deleted = service.SoftRemove<PersonVisa>(visa, x => x.ID == model.ID);

            if (deleted)
                service.Finalize(deleted);

            return Task.Run(() => { return deleted; });
        }

        public Task<IEnumerable<TravelViewModel>> GetRequestsToApprove(int id)
        {
            return Task.Run(() =>
            {
                using (PhoenixEntities context = new PhoenixEntities())
                {
                    int totalStages = 0;

                    List<GetTravelToApprove_Result> approvals = QueryHelper.GetTravelRequestsToApprove(id);
                    //List<int> travelIds = approvals.GroupBy(x => x.RequestID, (key, g) => new { key.Value }).Select(x => x.Value).ToList();

                    List<int> travelIds = QueryHelper.GetApprovalForTravel(travelRequestType, Convert.ToInt32(ConfigurationManager.AppSettings["TravelRoleId"].ToString()), id);
                    List<Traveller> lsttraveller = context.Traveller.Where(t => travelIds.Contains(t.Id)).OrderByDescending(x => x.CreatedDate).ToList();

                    var mappedTravel = Mapper.Map<List<Traveller>, IEnumerable<TravelViewModel>>(lsttraveller);
                    if (mappedTravel.Any())
                    {
                        foreach (var item in mappedTravel)
                        {
                            totalStages = approvals.Where(x => x.RequestID == item.Id).GroupBy(p => p.RequestID, (key, g) => new { stageCount = g.ToList().Count() }).FirstOrDefault().stageCount;
                            var stages = approvals.Where(x => x.RequestID == item.Id);

                            if (stages.Any())
                            {
                                item.travelStatus = stages.Select(x => new StageStatus { Stage = x.Stage.Value, Status = x.Status.Value, comment = x.StatusComment });
                                foreach (var stage in stages)
                                {
                                    if (stage.Stage == 1)
                                    {
                                        var primaryID = stage.ApproverID;
                                        var primaryApproverName = context.People.Where(x => x.ID == primaryID).Select(x => x.FirstName).FirstOrDefault() +
                                                      ' ' + context.People.Where(x => x.ID == primaryID).Select(x => x.LastName).FirstOrDefault();
                                        item.primaryApproverName = primaryApproverName;
                                    }
                                    else if (stage.Stage == 2)  
                                    {
                                        var adminID = stage.ApproverID;
                                        var adminApproverName = context.People.Where(x => x.ID == adminID).Select(x => x.FirstName).FirstOrDefault() +
                                                      ' ' + context.People.Where(x => x.ID == adminID).Select(x => x.LastName).FirstOrDefault();
                                        item.adminApproverName = adminApproverName;
                                    }
                                }
                            }
                            item.employeeProfile = Mapper.Map<Person, EmployeeBasicProfile>(lsttraveller.Where(x => x.Id == item.Id).First().Person);
                            item.totalStages = totalStages;
                        }

                        return mappedTravel;
                    }
                    else { return null; }
                }
            });
        }

        public Task<List<TravelListViewModel>> GetRequestsToApproveList(int id, int year)
        {
            return Task.Run(() =>
            {
                try
                {
                    using (PhoenixEntities context = new PhoenixEntities())
                    {
                        List<GetDetailsToApproveTravelRequest_Result> list = context.GetDetailsToApproveTravelRequest(id, year).ToList();
                        var mappedTravelList = Mapper.Map<List<GetDetailsToApproveTravelRequest_Result>, List<TravelListViewModel>>(list);
                        if(mappedTravelList.Any())
                        {
                            foreach(var item in mappedTravelList)
                            {
                                item.employeeProfile = Mapper.Map<GetDetailsToApproveTravelRequest_Result, TravelListViewModel.EmployeeProfile>(list.Where(x => x.ID == item.id).FirstOrDefault());
                                item.clientInformation = Mapper.Map<GetDetailsToApproveTravelRequest_Result, TravelListViewModel.ClientInformation>(list.Where(x => x.ID == item.id).FirstOrDefault());
                                item.travelDetails = Mapper.Map<GetDetailsToApproveTravelRequest_Result, TravelListViewModel.TravelDetails>(list.Where(x => x.ID == item.id).FirstOrDefault());
                                item.travelStatus = Mapper.Map<GetDetailsToApproveTravelRequest_Result, TravelListViewModel.TravelStatus>(list.Where(x => x.ID == item.id).FirstOrDefault());
                            }
                        }
                        return mappedTravelList;
                    }
                }
                catch(Exception ex)
                {
                    return null;
                }
            });
        }
        public Task<IEnumerable<TravelViewModel>> GetPendingRequests(int id)
        {
            return Task.Run(() =>
            {
                using (PhoenixEntities context = new PhoenixEntities())
                {
                    int totalStages = 0;

                    List<GetTravelToApprove_Result> approvals = QueryHelper.GetTravelRequestsToApprove(id);
                    //List<int> travelIds = approvals.GroupBy(x => x.RequestID, (key, g) => new { key.Value }).Select(x => x.Value).ToList();

                    //List<int> travelIds = QueryHelper.GetApprovalForTravel(context, travelRequestType, Convert.ToInt32(ConfigurationManager.AppSettings["TravelRoleId"].ToString()), id);
                    List<int?> travelIds = approvals.Where(a => a.Stage == 1 && (a.Status == 0 || a.Status == 3)).Select(s => s.RequestID).ToList();
                    List<Traveller> lsttraveller = context.Traveller.Where(t => travelIds.Contains(t.Id)).OrderByDescending(x => x.CreatedDate).ToList();

                    var mappedTravel = Mapper.Map<List<Traveller>, IEnumerable<TravelViewModel>>(lsttraveller);
                    if (mappedTravel.Any())
                    {
                        foreach (var item in mappedTravel)
                        {
                            totalStages = approvals.Where(x => x.RequestID == item.Id).GroupBy(p => p.RequestID, (key, g) => new { stageCount = g.ToList().Count() }).FirstOrDefault().stageCount;
                            var stages = approvals.Where(x => x.RequestID == item.Id);

                            if (stages.Any())
                            {
                                item.travelStatus = stages.Select(x => new StageStatus { Stage = x.Stage.Value, Status = x.Status.Value, comment = x.StatusComment });
                            }
                            item.employeeProfile = Mapper.Map<Person, EmployeeBasicProfile>(lsttraveller.Where(x => x.Id == item.Id).First().Person);
                            item.totalStages = totalStages;
                        }

                        return mappedTravel;
                    }
                    else { return null; }
                }
            });
        }

        public Task<TravelViewModel> ViewTravelRequest(int requestId, int id)
        {
            using (PhoenixEntities context = new PhoenixEntities())
            {
                var travelRequest = service.First<Traveller>(x => x.Id == requestId);
                var mappedTravel = Mapper.Map<Traveller, TravelViewModel>(travelRequest);

                var approval = service.Top<Pheonix.DBContext.ApprovalDetail>(10, x => x.Approval.RequestID == requestId && x.Approval.RequestType == travelRequestType && x.IsDeleted == false);

                var stages = approval.ToList();
                if (stages.Any())
                {
                    mappedTravel.travelStatus = stages.Select(x => new StageStatus { Stage = x.Stage.Value, Status = x.Status.Value, comment = x.StatusComment });

                    foreach (var stage in stages)
                    {
                        if (stage.Stage == 1)
                        {
                            var primaryID = stage.ApproverID;
                            var primaryApproverName = context.People.Where(x => x.ID == primaryID).Select(x => x.FirstName).FirstOrDefault() +
                                          ' ' + context.People.Where(x => x.ID == primaryID).Select(x => x.LastName).FirstOrDefault();
                            mappedTravel.primaryApproverName = primaryApproverName;
                        }
                        else if (stage.Stage == 2)
                        {
                            var adminID = stage.ApproverID;
                            var adminApproverName = context.People.Where(x => x.ID == adminID).Select(x => x.FirstName).FirstOrDefault() +
                                          ' ' + context.People.Where(x => x.ID == adminID).Select(x => x.LastName).FirstOrDefault();
                            mappedTravel.adminApproverName = adminApproverName;
                        }
                    }
                }

                mappedTravel.employeeProfile = Mapper.Map<Person, EmployeeBasicProfile>(travelRequest.Person);
                mappedTravel.employeeProfile.PersonalEmail = service.First<PersonPersonal>(x => x.PersonID == mappedTravel.employeeProfile.ID).PersonalEmail;
                mappedTravel.moneyTransactions = Mapper.Map<IEnumerable<TravelMoneyTransactions>, IEnumerable<MoneyTransactionViewModel>>(travelRequest.TravelMoneyTransactions.OrderByDescending(x => x.CreatedDate).ToList());
                mappedTravel.uploadedDocuments = Mapper.Map<IEnumerable<TravelUploads>, IEnumerable<UploadedDocumentViewModel>>(travelRequest.TravelUploads.Where(x => x.IsDeleted == false));

                if (mappedTravel.travelDetails.travelType == 2)
                {
                    mappedTravel.employeePassport = Mapper.Map<PersonPassport, EmployeePassport>(travelRequest.Person.PersonPassport.Where(x => x.IsDeleted == false).FirstOrDefault());
                    mappedTravel.employeeVisas = Mapper.Map<IEnumerable<PersonVisa>, IEnumerable<EmployeeVisa>>(travelRequest.Person.PersonVisa.Where(x => x.IsDeleted == false));
                }

                if (mappedTravel.nomineeDetails != null && mappedTravel.nomineeDetails.Count() > 0)
                    updateRelationshipValue(mappedTravel.nomineeDetails.FirstOrDefault());
                if (mappedTravel.travelDetails.reportingManagerId != null && mappedTravel.travelDetails.reportingManagerId != 0)
                {
                    mappedTravel.travelDetails.reportingManagerName = service.First<Person>(x => x.ID == mappedTravel.travelDetails.reportingManagerId).FirstName + ' ' + service.First<Person>(x => x.ID == mappedTravel.travelDetails.reportingManagerId).LastName;
                }
                if (mappedTravel.travelDetails.exitManagerId != null && mappedTravel.travelDetails.exitManagerId != 0)
                {
                    mappedTravel.travelDetails.exitManagerName = service.First<Person>(x => x.ID == mappedTravel.travelDetails.exitManagerId).FirstName + ' ' + service.First<Person>(x => x.ID == mappedTravel.travelDetails.exitManagerId).LastName;
                }
                mappedTravel.flightBooking = Mapper.Map<IEnumerable<Flight>, IEnumerable<TravelFlight>>(travelRequest.Flight.Where(x => x.IsDeleted == false).OrderByDescending(x => x.Id).ToList());
                mappedTravel.hotelBooking = Mapper.Map<IEnumerable<HotelBooking>, IEnumerable<TravelHotelBooking>>(travelRequest.HotelBooking.Where(x => x.IsDeleted == false).OrderByDescending(x => x.HotelId).ToList());

                SetOrganizaionDetails(id, travelRequest, mappedTravel);

                mappedTravel.employeeEmergencyContacts = Mapper.Map<IEnumerable<PersonContact>, IEnumerable<EmployeeEmergencyContact>>(travelRequest.Person.PersonContacts.Where(x => x.IsDeleted == false));
                mappedTravel.travelExtension = Mapper.Map<IEnumerable<TravelExtensionHistory>, IEnumerable<TravelExtension>>(travelRequest.TravelDetails.TravelExtensionHistory.OrderByDescending(x => x.Id).ToList());

                int financeRoleId = Convert.ToInt32(ConfigurationManager.AppSettings["FinanceRoleId"]);
                mappedTravel.financeAdmin = service.First<PersonInRole>(x => x.RoleID == financeRoleId).PersonID;

                int travelAdminRoleId = Convert.ToInt32(ConfigurationManager.AppSettings["TravelRoleId"]);
                mappedTravel.travelAdmin = service.First<PersonInRole>(x => x.RoleID == travelAdminRoleId).PersonID;

                if (approval != null)
                    mappedTravel.totalStages = approval.Count();

                return Task.Run(() => { return mappedTravel; });
            }
        }

        public Task<List<MoneyTransactionViewModel>> UploadMoney(MoneyTransactionViewModel model, int id)
        {
            return Task.Run(() =>
            {
                bool isCardCreated = false;
                bool isCashCreated = false;
                List<TravelMoneyTransactions> lstmodels = new List<TravelMoneyTransactions>();

                var travel = Mapper.Map<MoneyTransactionViewModel, TravelMoneyTransactions>(model);
                var traveller = service.First<Traveller>(x => x.Id == model.travelId); ;
                travel.CreatedDate = DateTime.Now;

                if (travel.CurrencygiveninCash != null)
                {
                    TravelMoneyTransactions cashTravel = new TravelMoneyTransactions() { CurrencygiveninCash = travel.CurrencygiveninCash, Comments = travel.Comments, CreatedDate = DateTime.Now ,CurrencyType = travel.CurrencyType, CurrencyGivenDays = travel.CurrencyGivenDays};
                    cashTravel.Traveller = traveller;
                    isCashCreated = service.Create<TravelMoneyTransactions>(cashTravel, x => x.Id == 0);

                    if (isCashCreated)
                    {
                        service.Finalize(true);
                        lstmodels.Add(cashTravel);
                    }
                }
                travel.CurrencygiveninCash = "";

                travel.Traveller = service.First<Traveller>(x => x.Id == model.travelId);

                isCardCreated = service.Create<TravelMoneyTransactions>(travel, x => x.Id == 0);
                var person = service.First<Person>(x => x.ID == id);

                if (isCardCreated)
                {
                    service.Finalize(isCardCreated);
                    lstmodels.Add(travel);
                    //emailService.SendTravelApproval(traveller, person, ApprovalStage.MoneyReload, model.comments);
                }

                return Mapper.Map<List<TravelMoneyTransactions>, List<MoneyTransactionViewModel>>(lstmodels);
            });
        }

        public Task<UploadedDocumentViewModel> UploadDocuments(UploadedDocumentViewModel model, int id)
        {
            var document = Mapper.Map<UploadedDocumentViewModel, TravelUploads>(model);
            document.Traveller = service.First<Traveller>(x => x.Id == model.travelId);
            document.CreatedDate = DateTime.Now;

            var isCreated = service.Create<TravelUploads>(document, x => x.Id == 0);

            if (isCreated)
                service.Finalize(isCreated);

            return Task.Run(() => { return Mapper.Map<TravelUploads, UploadedDocumentViewModel>(document); });
        }

        public Task<bool> DeleteDocument(int documentId, int id)
        {
            int travelRoleId = Convert.ToInt32(ConfigurationManager.AppSettings["TravelRoleId"]);
            var document = service.First<TravelUploads>(x => x.Id == documentId);

            var travelAdminId = service.First<PersonInRole>(x => x.RoleID == travelRoleId).PersonID == id;
            bool isDeleted = service.SoftRemove<TravelUploads>(document, x => x.Id == documentId && (x.Traveller.PrimaryApproverId == id || travelAdminId));

            if (isDeleted)
                service.Finalize(true);

            return Task.Run(() => { return isDeleted; });
        }

        public Task<TravelHotelBooking> SaveUpdateHotelBooking(TravelHotelBooking model, int id)
        {
            return Task.Run(() =>
            {
                var hotelBooking = Mapper.Map<TravelHotelBooking, HotelBooking>(model);

                if (model.hotelId == 0)
                {
                    hotelBooking.Traveller = service.First<Traveller>(x => x.Id == model.travelId);
                    var isCreated = service.Create<HotelBooking>(hotelBooking, x => x.HotelId == 0);

                    if (isCreated)
                        service.Finalize(true);

                    return Mapper.Map<HotelBooking, TravelHotelBooking>(hotelBooking);
                }
                else
                {
                    var oldModel = service.First<HotelBooking>(x => x.HotelId == model.hotelId);
                    var newModel = Mapper.Map<TravelHotelBooking, HotelBooking>(model);

                    var updated = service.Update<HotelBooking>(newModel, oldModel);



                    if (updated)
                    {
                        service.Finalize(true);
                        return Mapper.Map<HotelBooking, TravelHotelBooking>(newModel);
                    }
                    else { return Mapper.Map<HotelBooking, TravelHotelBooking>(oldModel); }
                }

            });
        }

        public Task<TravelFlight> SaveFligtBooking(TravelFlight model, int id)
        {
            try
            {
                if (model.id != 0)
                {
                    return Task.Run(() => { return UpdateFlight(model); });
                }
                else
                {
                    var flightBooking = Mapper.Map<TravelFlight, Flight>(model);
                    flightBooking.CreatedDate = DateTime.Now;
                    flightBooking.Traveller = service.First<Traveller>(x => x.Id == model.travelId);

                    var isCreated = service.Create<Flight>(flightBooking, x => x.Id == 0);

                    if (isCreated)
                        service.Finalize(true);

                    return Task.Run(() => { return Mapper.Map<Flight, TravelFlight>(flightBooking); });
                }
            }            
            catch(Exception ex)
            {
                Console.WriteLine($"An error occurred while saving flight booking details: {ex.Message}");
                throw;
            }
        }

        public Task<IEnumerable<TravelFlight>> GetFlightDetails(int travelId)
        {

            var flight = service.Top<Flight>(0, x => x.Traveller.Id == travelId);

            var model = Mapper.Map<IEnumerable<Flight>, IEnumerable<TravelFlight>>(flight);



            return Task.Run(() => { return model; });
        }

        public Task<List<ApprovalDetailVM>> ApproveTravel(int travelId, int id)
        {
            return Task.Run(() =>
            {
                var travel = service.First<Traveller>(x => x.Id == travelId);

                var approvalDetails = service.Top<ApprovalDetail>(10, x => x.Approval.RequestID == travelId && x.Approval.RequestType == travelRequestType && x.IsDeleted == false);
                int stageID = (int)approvalDetails.Where(x => x.ApproverID == id).Select(t => t.Stage).First();
                int approvalUpdatedID = UpdateTravelApproval(travel.Person.ID, travel.Id, 1, "Approved", id);
                var person = service.First<Person>(x => x.ID == id);

                if (!Convert.ToBoolean(approvalUpdatedID))
                {
                    service.Finalize(true);
                    emailService.SendTravelApproval(travel, person, ApprovalStage.TravelAdmin, "Approved");
                }
                else
                {
                    if(stageID == 1)
                    {
                        emailService.SendTravelApproval(travel, person, ApprovalStage.PrimaryApprover, "Approved");
                    }
                    else
                    {
                        emailService.SendTravelApproval(travel, person, ApprovalStage.TravelAdmin, "Approved");
                    }
                }
                return Mapper.Map<List<ApprovalDetail>, List<ApprovalDetailVM>>(approvalDetails.ToList());

            });
        }

        public Task<bool> RejectTravel(int travelId, int id, string comments)
        {
            var travel = service.First<Traveller>(x => x.Id == travelId);

            bool updated = Convert.ToBoolean(UpdateTravelApproval(travel.Person.ID, travel.Id, 2, comments, id));
            var person = service.First<Person>(x => x.ID == id);

            if (!updated)
            {
                service.Finalize(true);
                emailService.SendTravelApproval(travel, person, ApprovalStage.Rejected, comments);
            }
            else
            {
                emailService.SendTravelApproval(travel, person, ApprovalStage.Rejected, comments);
            }

            return Task.Run(() => { return true; });
        }

        public Task<List<ApprovalDetailVM>> OnHoldTravel(int travelId, int id, string comments)
        {
            var approvalDetails = new List<ApprovalDetail>();
            var travel = service.First<Traveller>(x => x.Id == travelId);

            bool updated = Convert.ToBoolean(UpdateTravelApproval(travel.Person.ID, travel.Id, 3, comments, id));
            var person = service.First<Person>(x => x.ID == id);

            if (!updated)
            {
                service.Finalize(true);
                emailService.SendTravelApproval(travel, person, ApprovalStage.OnHold, comments);
            }
            approvalDetails = service.Top<ApprovalDetail>(0, x => x.Approval.RequestType == travelRequestType && x.Approval.RequestID == travelId && x.IsDeleted == false).ToList();

            return Task.Run(() => { return Mapper.Map<List<ApprovalDetail>, List<ApprovalDetailVM>>(approvalDetails); });
        }

        public Task<bool> AddTravelExtension(TravelExtension model, int id)
        {
            DateTime extArrival = new DateTime();

            var travelDetails = service.First<TravelDetails>(x => x.TravelId == model.travelId);
            extArrival = travelDetails.ReturnJourneyDate;
            travelDetails.ReturnJourneyDate = model.arrival;

            var isUpdated = service.Update<TravelDetails>(travelDetails, travelDetails);

            var extension = Mapper.Map<TravelExtension, TravelExtensionHistory>(model);
            extension.TravelDetails = travelDetails;
            extension.Arrival = extArrival;

            var created = service.Create<TravelExtensionHistory>(extension, x => x.Id == 0);

            if (created && isUpdated)
            {
                QueryHelper.ResetApproval( model.travelId);
                HookApproval(id, model.travelId, travelDetails.Traveller.PrimaryApproverId.Value);
                service.Finalize(true);
            }

            return Task.Run(() => { return created && isUpdated; });
        }

        public Task<bool> CloseTravelRequest(int travelId, int id, string comments)
        {
            var newApprovalDetailModel = new ApprovalDetail() { ApprovalDate = DateTime.Now, Status = 4, StatusComment = comments };
            var newApprovalModel = new DBContext.Approval() { StatusDate = DateTime.Now, Status = 4 };

            var approvalDetails = service.Top<ApprovalDetail>(0, x => x.Approval.RequestID == travelId && x.Approval.RequestType == travelRequestType && x.IsDeleted == false).ToList();


            foreach (var item in approvalDetails)
            {
                var updated = service.Update<ApprovalDetail>(newApprovalDetailModel, item);

                if (updated)
                    service.Finalize(true);
            }

            var approvalUpdate = service.Update<DBContext.Approval>(newApprovalModel, approvalDetails.First().Approval);

            if (approvalUpdate)
                service.Finalize(true);


            return Task.Run(() => { return true; });
        }

        public Task<IEnumerable<TravelViewModel>> ApprovalHistory(int userId)
        {
            return Task.Run(() =>
            {
                using (PhoenixEntities entites = new PhoenixEntities())
                {
                    int totalStages = 0;
                    List<GetApprovedTravelRequests_Result> approvals = QueryHelper.GetApprovedTravelRequests(userId);
                    var requestIds = approvals.Where(x => x.ApproverID.Value == userId && (x.ApprovalStatus == 4 || x.Status == 2)).Select(x => x.RequestID).Distinct();
                    var rejectedTravelIdsFinalStage = approvals.Where(x => x.RequestType == 6 && x.Stage == 2 && x.Status == 2).Select(y => y.RequestID).Distinct();
                    var rejectedRequestIds = (rejectedTravelIdsFinalStage != null && rejectedTravelIdsFinalStage.Count() > 0) ? approvals.Where(x => rejectedTravelIdsFinalStage.Contains(x.RequestID) && x.Stage == 1 && x.Status == 1).Select(y => y.RequestID).Distinct() : null;
                    if(rejectedRequestIds != null)
                        requestIds = requestIds.Union(rejectedRequestIds);
                    var rejectedTravelIdsFirstStage = approvals.Where(x => x.RequestType == 6 && x.Stage == 1 && x.Status == 2).Select(y => y.RequestID).Distinct();
                    rejectedRequestIds = (rejectedTravelIdsFirstStage != null && rejectedTravelIdsFirstStage.Count() > 0) ? approvals.Where(x => rejectedTravelIdsFirstStage.Contains(x.RequestID) && x.Stage == 2 && x.Status == 0).Select(y => y.RequestID).Distinct() : null;
                    if (rejectedRequestIds != null)
                        requestIds = requestIds.Union(rejectedRequestIds);
                    List<Traveller> lstTravel = entites.Traveller.Where(e => requestIds.Contains(e.Id)).OrderByDescending(x => x.Id).ToList();


                    var mappedExpense = Mapper.Map<List<Traveller>, IEnumerable<TravelViewModel>>(lstTravel);

                    foreach (TravelViewModel item in mappedExpense)
                    {
                        //totalStages = QueryHelper.GetTotalStages(context, item.expenseId, null);
                        totalStages = approvals.Where(x => x.RequestID == item.Id).GroupBy(p => p.RequestID, (key, g) => new { stageCount = g.ToList().Count() }).FirstOrDefault().stageCount;
                        var stages = approvals.Where(x => x.RequestID == item.Id);

                        if (stages.Any())
                        {
                            item.travelStatus = stages.Select(x => new StageStatus { Stage = x.Stage.Value, Status = x.Status.Value, comment = x.StatusComment });
                            foreach (var stage in stages)
                            {
                                if (stage.Stage == 1)
                                {
                                    var primaryID = stage.ApproverID;
                                    var primaryApproverName = entites.People.Where(x => x.ID == primaryID).Select(x => x.FirstName).FirstOrDefault() +
                                                  ' ' + entites.People.Where(x => x.ID == primaryID).Select(x => x.LastName).FirstOrDefault();
                                    item.primaryApproverName = primaryApproverName;
                                }
                                else if (stage.Stage == 2)
                                {
                                    var adminID = stage.ApproverID;
                                    var adminApproverName = entites.People.Where(x => x.ID == adminID).Select(x => x.FirstName).FirstOrDefault() +
                                                  ' ' + entites.People.Where(x => x.ID == adminID).Select(x => x.LastName).FirstOrDefault();
                                    item.adminApproverName = adminApproverName;
                                }
                            }
                        }
                        
                        item.employeeProfile = Mapper.Map<Person, EmployeeBasicProfile>(lstTravel.Where(x => x.Id == item.Id).First().Person);
                        item.totalStages = totalStages;
                    }

                    if (lstTravel.Any())
                        return mappedExpense;
                    else
                        return null;

                }
            });
        }

        public Task<bool> IsApproverOrAdmin(int userID)
        {
            return Task.Run(() =>
            {
                var result = false;
                var ApprovelList = service.Top<Pheonix.DBContext.Approval>(0, x => x.RequestType == travelRequestType).ToList();
                int travelRoleId = Convert.ToInt32(ConfigurationManager.AppSettings["TravelRoleId"]);
                int financeRoleId = Convert.ToInt32(ConfigurationManager.AppSettings["FinanceRoleId"]);

                foreach (var item in ApprovelList)
                {
                    var isApprove = service.Top<ApprovalDetail>(0, x => x.ApprovalID == item.ID && x.ApproverID == userID).ToList();
                    if (isApprove.Count() > 0)
                    {
                        result = true;
                        break;
                    }
                }

                if (service.First<PersonInRole>(x => x.RoleID == travelRoleId).PersonID == userID || service.First<PersonInRole>(x => x.RoleID == financeRoleId).PersonID == userID)
                    result = true;

                return result;
            });
        }

        public Task<Dictionary<string, List<DropdownItems>>> GetDropdowns(int userId)
        {
            Dictionary<string, List<DropdownItems>> Items = new Dictionary<string, List<DropdownItems>>();
            List<DropdownItems> lstItems = new List<DropdownItems>();

            var travelBookingAgency = service.All<TravelBookingAgency>().ToList();

            foreach (var item in travelBookingAgency.OrderBy(x => x.Description))
            {
                DropdownItems dropdownItem = new DropdownItems
                {
                    ID = item.Id,
                    Text = item.Description.Trim()
                };
                lstItems.Add(dropdownItem);
            }
            Items.Add(TravelDropDowns.TravelBookingAgency.ToString(), lstItems);


            var travelAirlines = service.All<TravelFLightAirlines>();

            lstItems = new List<DropdownItems>().ToList();
            foreach (var item in travelAirlines.OrderBy(x => x.Description))
            {
                DropdownItems dropdownItem = new DropdownItems
                {
                    ID = item.Id,
                    Text = item.Description.Trim()
                };
                lstItems.Add(dropdownItem);
            }
            Items.Add(TravelDropDowns.TravelFlightAirlines.ToString(), lstItems);


            var hotelRoomType = service.All<HotelRoomType>().ToList();
            lstItems = new List<DropdownItems>();

            foreach (var item in hotelRoomType.OrderBy(x => x.Description))
            {
                DropdownItems dropdownItem = new DropdownItems
                {
                    ID = item.Id,
                    Text = item.Description.Trim()
                };
                lstItems.Add(dropdownItem);
            }

            Items.Add(TravelDropDowns.HotelRoom.ToString(), lstItems);

            var purposeType = service.All<PurposeOfVisit>().ToList();
            lstItems = new List<DropdownItems>();

            foreach (var item in purposeType)
            {
                DropdownItems dropdownItem = new DropdownItems
                {
                    ID = item.ID,
                    Text = item.PurposeDescription.Trim()
                };
                lstItems.Add(dropdownItem);
            }

            Items.Add(TravelDropDowns.Purpose.ToString(), lstItems);

            var travelCardType = service.All<TravelCardType>().ToList();
            lstItems = new List<DropdownItems>();

            foreach (var item in travelCardType.OrderBy(x => x.Description))
            {
                DropdownItems dropdownItem = new DropdownItems
                {
                    ID = item.Id,
                    Text = item.Description.Trim()
                };
                lstItems.Add(dropdownItem);
            }
            Items.Add(TravelDropDowns.TravelCard.ToString(), lstItems);

            var flightClass = service.All<FlightClass>();
            lstItems = new List<DropdownItems>();

            foreach (var item in flightClass.OrderBy(x => x.Title))
            {
                DropdownItems dropdownItem = new DropdownItems
                {
                    ID = item.Id,
                    Text = item.Title.Trim()
                };
                lstItems.Add(dropdownItem);
            }
            Items.Add(TravelDropDowns.FlightClass.ToString(), lstItems);

            var mealPrefeerence = service.All<MealPreference>();
            lstItems = new List<DropdownItems>();

            foreach (var item in mealPrefeerence.OrderBy(x => x.Description))
            {
                DropdownItems dropdownItem = new DropdownItems
                {
                    ID = item.Id,
                    Text = item.Description.Trim()
                };
                lstItems.Add(dropdownItem);
            }
            Items.Add(TravelDropDowns.Meal.ToString(), lstItems);


            var seatLocation = service.All<SeatLocationPreference>();
            lstItems = new List<DropdownItems>();

            foreach (var item in seatLocation.OrderBy(x => x.Description))
            {
                DropdownItems dropdownItem = new DropdownItems
                {
                    ID = item.Id,
                    Text = item.Description.Trim()
                };
                lstItems.Add(dropdownItem);
            }
            Items.Add(TravelDropDowns.SeatLocation.ToString(), lstItems);

            return Task.Run(() => { return Items; });
        }

        public Task<TravelDetailsVM> GetTravelDetails(int travelId)
        {
            var details = service.Top<TravelDetails>(0, x => x.TravelId == travelId);

            var map = Mapper.Map<TravelDetails, TravelDetailsVM>(details.FirstOrDefault());

            return Task.Run(() => { return map; });
        }

        public Task<bool> DeleteFlight(int flightId)
        {

            var flightDetails = service.Top<FlightDetails>(0, x => x.FlightId == flightId);

            foreach (var item in flightDetails)
            {
                var isDeleted = service.SoftRemove<FlightDetails>(item, x => x.FlightId == flightId);
            }
            var flight = service.First<Flight>(x => x.Id == flightId);

            var flighDelete = service.SoftRemove<Flight>(flight, x => x.Id == flightId);

            return Task.Run(() => { return service.Finalize(true) > 0; });
        }

        public Task<bool> DeleteHotelBooking(int hotelId)
        {
            var hotel = service.First<HotelBooking>(x => x.HotelId == hotelId);

            var isDeleted = service.SoftRemove<HotelBooking>(hotel, x => x.HotelId == hotelId);

            if (isDeleted)
                service.Finalize(true);

            return Task.Run(() => { return isDeleted; });
        }

        public IEnumerable<TravelViewModel> DashBoardTravelCardView(int userId)
        {
            using (PhoenixEntities context = new PhoenixEntities())
            {
                int totalStages = 0;
                List<TravelViewModel> viewModel = new List<TravelViewModel>();
                List<GetLatestActivity_Result> travelActivity = QueryHelper.GetLatestActivity( userId, travelRequestType);

                var requestIds = travelActivity.Select(x => x.RequestID).Distinct().Take(2);

                foreach (var item in requestIds)
                {
                    var expense = context.Traveller.Where(x => x.Id == item.Value).FirstOrDefault();
                    var mappedExpense = Mapper.Map<Traveller, TravelViewModel>(expense);
                    var stages = travelActivity.Where(x => x.RequestID == item.Value);

                    if (stages.Any())
                    {
                        mappedExpense.travelStatus = stages.Select(x => new StageStatus { Stage = x.Stage.Value, Status = x.Status.Value, comment = x.StatusComment });
                    }

                    mappedExpense.totalStages = totalStages;

                    viewModel.Add(mappedExpense);
                }
                return viewModel;
            }
        }

        #region Private Methods

        private bool SavePassportDetails(EmployeePassport model, Person person)
        {
            var passport = Mapper.Map<EmployeePassport, PersonPassport>(model);
            bool isCreated = false;

            if (model.ID == 0)
            {
                passport.Person = person;
                passport.RelationWithPPHolder = 1;
                isCreated = service.Create<PersonPassport>(passport, x => x.ID == 0);
                if (isCreated)
                    service.Finalize(true);
                return isCreated;
            }
            else
            {
                var oldModel = service.First<PersonPassport>(x => x.ID == model.ID);
                var newModel = Mapper.Map<EmployeePassport, PersonPassport>(model);
                var updated = service.Update<PersonPassport>(newModel, oldModel);
                if (updated)
                    service.Finalize(true);
                return updated;
            }
        }

        private int HookApproval(int userId, int recordID, int primaryApproverId)
        {
            // ND: If there is no Secondary Approver we need to skip the Secondary and make Finance as Final Approver.
            // ND: Code not done for the above thing.
            var strategy = ApprovalStrategyFactory.GetStrategy(ApprovalStrategy.MultiLevel, userId, primaryApproverId, Convert.ToInt32(ConfigurationManager.AppSettings["TravelRoleId"].ToString()));
            strategy.opsService = this.service;
            ApprovalService service = new ApprovalService(this.service);
            service.SendForApproval(userId, travelRequestType, recordID, strategy.FetchApprovers()); // RequestType for Travel, keeping it as 6.
            return strategy.FetchApprovers().First();
        }

        private void SetOrganizaionDetails(int id, Traveller travelRequest, TravelViewModel mappedTravel)
        {
            var empOrganizationDetails = service.First<PersonReporting>(x => x.PersonID == id);
            mappedTravel.employeeOrganizationdetails = Mapper.Map<PersonEmployment, EmployeeOrganizaionDetails>(travelRequest.Person.PersonEmployment.FirstOrDefault());
            if (empOrganizationDetails != null)
            {
                mappedTravel.employeeOrganizationdetails.ReportingTo = empOrganizationDetails.ReportingTo;
                var reportingManager = service.First<Person>(x => x.ID == empOrganizationDetails.ReportingTo);
                mappedTravel.employeeOrganizationdetails.ReportingManager = reportingManager.FirstName + " " + reportingManager.LastName;
            }
            else
            {
                mappedTravel.employeeOrganizationdetails.ReportingManager = "";
                mappedTravel.employeeOrganizationdetails.ReportingTo = 0;
            }
        }

        private int UpdateTravelApproval(int userID, int recordID, int statusID, string comments, int approverID)
        {
            ApprovalService service = new ApprovalService(this.service);
            return service.UpdateMultiLevelApproval(userID, travelRequestType, recordID, statusID, comments, approverID);
        }

        private TravelFlight UpdateFlight(TravelFlight model)
        {
            bool detailUpdated = false;
            var oldFlight = service.First<Flight>(x => x.Id == model.id);
            var flight = Mapper.Map<TravelFlight, Flight>(model);

            foreach (var item in flight.FlightDetails)
            {
                if (item.Id != 0)
                {
                    var oldDetail = oldFlight.FlightDetails.Where(x => x.Id == item.Id).FirstOrDefault();
                    oldDetail.FlightId = model.id;

                    detailUpdated = service.Update<FlightDetails>(item, oldDetail);
                }
                else
                {
                    oldFlight.FlightDetails.Add(item);
                }
            }

            var isUpdated = service.Update<Flight>(flight, oldFlight);

            if (isUpdated && detailUpdated)
            {
                service.Finalize(true);
                return Mapper.Map<Flight, TravelFlight>(flight);
            }
            else { return Mapper.Map<Flight, TravelFlight>(oldFlight); }
        }

        #endregion

        public Task<bool> UpdateTravel(TravelViewModel model, int id)
        {
            return Task.Run(() =>
            {
                bool isTravelExtensionUpdated = false;

                var travel = Mapper.Map<TravelViewModel, Traveller>(model);
                var _Traveller = service.First<Traveller>(x => x.Id == model.Id);
                var person = service.First<Person>(x => x.ID == _Traveller.PersonId);
                travel.Person = person;

                var travelDetails = service.First<TravelDetails>(x => x.TravelId == model.Id);

                //To update Travel Extension data
                if (travelDetails.ReturnJourneyDate != model.travelDetails.returnJourneyDate)
                {
                    TravelExtension _TravelExtension = new TravelExtension();
                    _TravelExtension.id = 0;
                    _TravelExtension.travelId = model.Id;
                    _TravelExtension.departure = model.travelDetails.journeyDate;
                    _TravelExtension.arrival = model.travelDetails.returnJourneyDate;
                    _TravelExtension.comments = "Date extented by Travel Admin";
                    _TravelExtension.visaDate = null;
                    _TravelExtension.I94Date = null;

                    var extension = Mapper.Map<TravelExtension, TravelExtensionHistory>(_TravelExtension);

                    extension.TravelDetails = travelDetails;
                    //extension.Arrival = extArrival;
                    isTravelExtensionUpdated = service.Create<TravelExtensionHistory>(extension, x => x.Id == 0);
                    service.Finalize(isTravelExtensionUpdated);
                }

                //To update Travel Requirement data
                var travelDetailsModel = Mapper.Map<TravelDetailsVM, TravelDetails>(model.travelDetails);
                travelDetailsModel.TravelId = model.Id;
                var isTravelRequirementUpdated = service.Update<TravelDetails>(travelDetailsModel, travelDetails);
                service.Finalize(isTravelRequirementUpdated);

                //if (isPassportCreated || isTravelExtensionUpdated || isClientInfoUpdated)
                if (isTravelExtensionUpdated || isTravelRequirementUpdated)
                    return true;
                else
                    return false;
            });
        }

        //This will return the travel model detail based on Search Employee
        public Task<TravelViewModel> GetSearchTravelCardDetl(int personID, int id)
        {
            //var travelRequest = service.First<Traveller>(x => x.Id == personID);
            //var mappedTravel = Mapper.Map<Traveller, TravelViewModel>(travelRequest);

            //var approval = service.Top<Pheonix.DBContext.ApprovalDetail>(10, x => x.Approval.RequestID == personID && x.Approval.RequestType == travelRequestType && x.IsDeleted == false);

            //mappedTravel.travelStatus = approval.Select(x => new StageStatus { comment = x.StatusComment, Stage = x.Stage.Value, Status = x.Status.Value });
            var mappedTravel = new TravelViewModel();
            var personData = service.First<Person>(x => x.ID == personID);
            mappedTravel.employeeProfile = Mapper.Map<Person, EmployeeBasicProfile>(personData);
            mappedTravel.moneyTransactions = null;
            mappedTravel.uploadedDocuments = null;

            mappedTravel.employeePassport = Mapper.Map<PersonPassport, EmployeePassport>(personData.PersonPassport.Where(x => x.IsDeleted == false).FirstOrDefault());
            mappedTravel.employeeVisas = Mapper.Map<IEnumerable<PersonVisa>, IEnumerable<EmployeeVisa>>(personData.PersonVisa.Where(x => x.IsDeleted == false));
            //}

            mappedTravel.flightBooking = null;
            mappedTravel.hotelBooking = null;

            mappedTravel.employeeEmergencyContacts = Mapper.Map<IEnumerable<PersonContact>, IEnumerable<EmployeeEmergencyContact>>(personData.PersonContacts.Where(x => x.IsDeleted == false));
            mappedTravel.travelExtension = null;

            int financeRoleId = Convert.ToInt32(ConfigurationManager.AppSettings["FinanceRoleId"]);
            mappedTravel.financeAdmin = service.First<PersonInRole>(x => x.RoleID == financeRoleId).PersonID;

            int travelAdminRoleId = Convert.ToInt32(ConfigurationManager.AppSettings["TravelRoleId"]);
            mappedTravel.travelAdmin = service.First<PersonInRole>(x => x.RoleID == travelAdminRoleId).PersonID;

            //To get Organizational Detail for Search Employee
            var empOrganizationDetails = service.First<PersonReporting>(x => x.PersonID == id);
            mappedTravel.employeeOrganizationdetails = Mapper.Map<PersonEmployment, EmployeeOrganizaionDetails>(personData.PersonEmployment.FirstOrDefault());
            if (empOrganizationDetails != null)
            {
                mappedTravel.employeeOrganizationdetails.ReportingTo = empOrganizationDetails.ReportingTo;
                var reportingManager = service.First<Person>(x => x.ID == empOrganizationDetails.ReportingTo);
                mappedTravel.employeeOrganizationdetails.ReportingManager = reportingManager.FirstName + " " + reportingManager.LastName;
            }
            else
            {
                mappedTravel.employeeOrganizationdetails.ReportingManager = "";
                mappedTravel.employeeOrganizationdetails.ReportingTo = 0;
            }


            //if (approval != null)
            mappedTravel.totalStages = 2;

            return Task.Run(() => { return mappedTravel; });
        }

        //This will save travel request detail for the Search Employee by Travel Admin
        public Task<bool> AddOtherEmpTravelRequest(TravelViewModel model, int personID, int id)
        {
            return Task.Run(() =>
            {
                bool isPassportCreated = false;
                var loggedInUser = service.First<Person>(x => x.ID == id);
                var person = service.First<Person>(x => x.ID == personID);
                model.primaryApproverId = person.PersonEmployment.FirstOrDefault().ExitProcessManager.Value;

                var travel = Mapper.Map<TravelViewModel, Traveller>(model);
                travel.Person = person;

                //For :148333193 on 24/08/2017 Changes done to fetch client list from Customer table instead of ClientName
                //travel.ClientInformation.ClientName = service.First<ClientName>(x => x.ClientNameId == model.clientInformation.clientId);                
                ClientName lstItems = new ClientName();
                var clientNames = service.First<Customer>(x => x.ID == model.clientInformation.clientId);
                lstItems.Name = clientNames.Name.Trim();
                lstItems.ClientNameId = clientNames.ID;
                lstItems.IsDeleted = clientNames.IsDeleted;

                travel.ClientInformation.ClientName = lstItems;

                var isCreated = service.Create<Traveller>(travel, x => x.Id == 0);
                if (travel.TravelDetails.TravelType == 2)
                {
                    isPassportCreated = SavePassportDetails(model.employeePassport, person);
                }
                else { isPassportCreated = true; }


                int travelAdminRoleId = Convert.ToInt32(ConfigurationManager.AppSettings["TravelRoleId"]);
                int approverID = person.PersonEmployment.FirstOrDefault().ExitProcessManager.Value;

                if (isCreated && isPassportCreated)
                {
                    service.Finalize(isCreated);
                    ApprovalForEmployeeRequest(personID, travel.Id, approverID);
                    emailService.SendTravelApproval(travel, loggedInUser, ApprovalStage.BehalfOfEmpByTravelAdmin, "");
                }

                return isCreated;
            });
        }

        private int ApprovalForEmployeeRequest(int userId, int recordID, int primaryApproverId)
        {
            var strategy = ApprovalStrategyFactory.GetStrategy(ApprovalStrategy.MultiLevel, userId, primaryApproverId, Convert.ToInt32(ConfigurationManager.AppSettings["TravelRoleId"].ToString()));
            strategy.opsService = this.service;
            ApprovalService service = new ApprovalService(this.service);
            service.SendForApprovalForTravelAdminRequest(userId, travelRequestType, recordID, strategy.FetchApprovers()); // RequestType for Travel, keeping it as 6.
            return strategy.FetchApprovers().First();
        }

        private void updateRelationshipValue(NomineeDetailsVM nominee)
        {
            var relationList = new Dictionary<int, string>();
            relationList.Add(1, "Mother");
            relationList.Add(2, "Father");
            relationList.Add(3, "Brother");
            relationList.Add(4, "Sister");
            relationList.Add(5, "Wife");
            relationList.Add(6, "Son");
            relationList.Add(7, "Daughter");
            relationList.Add(8, "Mother-in-law");
            relationList.Add(9, "Father-in-law");
            relationList.Add(10, "Grandfather");
            relationList.Add(11, "Grandmother");
            relationList.Add(12, "Friend");
            relationList.Add(13, "Husband");
            relationList.Add(14, "Other");
            string value;
            if (relationList.TryGetValue((int)nominee.Relationship, out value))
            {
                nominee.RelationshipValue = value;
            }
            else
            {
                nominee.RelationshipValue = null;
            }
        }

    }
}

public enum TravelDropDowns
{
    TravelBookingAgency,
    TravelFlightAirlines,
    HotelRoom,
    Purpose,
    TravelCard,
    FlightClass,
    Meal,
    SeatLocation
}