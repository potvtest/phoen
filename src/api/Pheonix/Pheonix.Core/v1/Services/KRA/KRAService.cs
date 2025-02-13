using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using log4net;
using Pheonix.DBContext;
using Pheonix.DBContext.Repository;
using Pheonix.Models.ViewModels;
using Pheonix.Models.VM;
using static Pheonix.Models.ViewModels.KRAAllEmployeesViewModel;
using System.Data.Entity.Core.Objects;
using System.Configuration;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml;
using System.IO;

namespace Pheonix.Core.v1.Services.KRA
{
    public class KRAService : IKRAService
    {
        private readonly IContextRepository _repo;
        private readonly IBasicOperationsService _service;
        private static readonly ILog Log4Net = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public enum KRADropDownType
        {
            Categories,
            Grade
        }

        public KRAService(IContextRepository repo, IBasicOperationsService service)
        {
            _repo = repo;
            _service = service;
        }

        public async Task<IEnumerable<GetKRADetails_Result>> GetDetails(int id, int personId)
        {
            try
            {
                var personKRADetailList = new List<GetKRADetails_Result>();
                try
                {
                    using (PhoenixEntities context = new PhoenixEntities())
                    {
                        var personKRADetailsList = context.GetKRADetails(personId,id);
                        personKRADetailList.AddRange(personKRADetailsList);
                    }
                    
                    return await Task.FromResult(personKRADetailList).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Log4Net.Error("Error occured in the GetDetails() method - InnerException :" + ex.InnerException + "Message :" + ex.Message + "StackTrace :" + ex.StackTrace);
                    throw ex;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> AddUpdateDetails(int userId, bool isCloned, PersonKRADetailViewModel viewModel)
        {
            int ParentId = viewModel.ParentKRAId;
            int KRAClonedBy = 0;
            DateTime KRAClonedDate = new DateTime(1753, 1, 1);
            int existingKRAGoalId = viewModel.KRAGoalId;
            if (isCloned)
            {
                //ParentId = viewModel.ParentKRAId;
                KRAClonedBy = userId;
                KRAClonedDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                viewModel.Id = 0;
            }

            using (PhoenixEntities context = new PhoenixEntities())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        if (viewModel != null)
                        {
                            var existingKraDetails = context.PersonKRADetails.Where(k => k.KRAGoalId == existingKRAGoalId && k.IsDelete == false).ToList();
                            int newKRAGoalId = existingKraDetails.Any() ? existingKRAGoalId : (context.PersonKRADetails.Max(k => (int?)k.KRAGoalId) ?? 0) + 1;

                            foreach (var kraInitiationDetail in viewModel.KRADetails)
                            { 
                            var _mappedData = Mapper.Map<PersonKRADetailViewModel, PersonKRADetail>(viewModel);

                                // Update specific properties from kraInitiationDetail
                                _mappedData.Id = kraInitiationDetail.Id;
                                _mappedData.KRAInitiationId = kraInitiationDetail.KRAInitiationId;
                                _mappedData.ParentKRAId = kraInitiationDetail.ParentKRAId;
                                _mappedData.Q1 = kraInitiationDetail.Q1;
                                _mappedData.Q2 = kraInitiationDetail.Q2;
                                _mappedData.Q3 = kraInitiationDetail.Q3;
                                _mappedData.Q4 = kraInitiationDetail.Q4;
                                _mappedData.YearId = kraInitiationDetail.YearId;
                                _mappedData.KRAPercentageCompletion = viewModel.KRAPercentageCompletion;

                                // Set the common KRAGoalId for all KRAs added in the same transaction
                                _mappedData.KRAGoalId = newKRAGoalId;

                                var startEndDate = GetStartDateEndDate((bool)_mappedData.Q1, (bool)_mappedData.Q2, (bool)_mappedData.Q3, (bool)_mappedData.Q4);   //To Get KRA Start Date and End Date from Quarter Calculation
                            _mappedData.KRAStartDate = startEndDate.KRAStartDate;
                            _mappedData.KRAEndDate = startEndDate.KRAEndDate;
                            _mappedData.KRAInitiationEndDate = null;
                            viewModel.KRAHistoryClonedDate = null;
                            viewModel.KRAHistoryClonedBy = _mappedData.KRAHistoryClonedBy;
                            viewModel.KRAHistoryId = _mappedData.KRAHistoryId;
                            viewModel.IsClonedFromHistory = _mappedData.IsClonedFromHistory;
                            context.SaveOrUpdateKRA(_mappedData.Id, _mappedData.KRACategoryId, _mappedData.PersonId, _mappedData.KRA, _mappedData.YearId,
                                _mappedData.Description, _mappedData.Weightage, ParentId, _mappedData.Q1, _mappedData.Q2,
                                _mappedData.Q3, _mappedData.Q4, _mappedData.KRAInitiationId, _mappedData.Comments, _mappedData.KRAPercentageCompletion, _mappedData.KRAStartDate,
                                _mappedData.KRAEndDate, _mappedData.KRAInitiationEndDate, isCloned, userId, userId, 
                                KRAClonedBy, KRAClonedDate, viewModel.KRAHistoryClonedDate, viewModel.KRAHistoryClonedBy, viewModel.KRAHistoryId, viewModel.IsClonedFromHistory, _mappedData.KRAGoalId);
                        }
                            foreach (var existingKraDetail in existingKraDetails)
                            {
                                // Find if the current KRA detail has been unchecked in the updated viewModel
                                var updatedKraDetail = viewModel.KRADetails.FirstOrDefault(k => k.Id == existingKraDetail.Id);
                                if (updatedKraDetail == null || !(updatedKraDetail.Q1 || updatedKraDetail.Q2 || updatedKraDetail.Q3 || updatedKraDetail.Q4))
                                {
                                    // If the record no longer has active quarters, mark it as deleted
                                    existingKraDetail.IsDelete = true;
                                    context.Entry(existingKraDetail).State = System.Data.Entity.EntityState.Modified;
                                }
                            }

                            context.SaveChanges();
                            transaction.Commit();
                        }
                        return await Task.FromResult(true).ConfigureAwait(false);
                    }
                    catch (DbUpdateException ex)
                    {
                        transaction.Rollback();
                        Log4Net.Error("Error occured in the AddUpdateKRADetails() method - InnerException :" + ex.InnerException + "Message :" + ex.Message + "StackTrace :" + ex.StackTrace);
                        throw ex;
                    }
                }
            }
        }

        public async Task<bool> CloneHistoryDetails(int userId, PersonKRADetailViewModel viewModel)
        {
            using (PhoenixEntities context = new PhoenixEntities())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        if (viewModel != null)
                        {
                            int newKRAGoalId = (context.PersonKRADetails.Max(k => (int?)k.KRAGoalId) ?? 0) + 1;

                            foreach (var kraInitiationDetail in viewModel.KRADetails)
                            { 
                            bool isCloned = (viewModel.ParentKRAId > 0) ? true : false;
                            
                                viewModel.KRAInitiationId = kraInitiationDetail.KRAInitiationId;
                                viewModel.ParentKRAId = kraInitiationDetail.ParentKRAId;
                                viewModel.Q1 = kraInitiationDetail.Q1;
                                viewModel.Q2 = kraInitiationDetail.Q2;
                                viewModel.Q3 = kraInitiationDetail.Q3;
                                viewModel.Q4 = kraInitiationDetail.Q4;
                                viewModel.YearId = kraInitiationDetail.YearId;
                                viewModel.KRAGoalId = newKRAGoalId;

                                var startEndDate = GetStartDateEndDate((bool)viewModel.Q1, (bool)viewModel.Q2, (bool)viewModel.Q3, (bool)viewModel.Q4);   //To Get KRA Start Date and End Date from Quarter Calculation
                                viewModel.KRAStartDate = startEndDate.KRAStartDate;
                                viewModel.KRAEndDate = startEndDate.KRAEndDate;
                                viewModel.KRAInitiationEndDate = null;
                                viewModel.KRAHistoryClonedDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);

                                context.CloneFromKRAHistoryDetails(viewModel.Id, viewModel.KRACategoryId, viewModel.PersonId, viewModel.KRA, viewModel.YearId,
                            viewModel.Description, viewModel.Weightage, viewModel.ParentKRAId, viewModel.Q1, viewModel.Q2,
                                viewModel.Q3, viewModel.Q4, isCloned, viewModel.KRAInitiationId, viewModel.Comments, viewModel.KRAStartDate,
                                viewModel.KRAEndDate, viewModel.KRAInitiationEndDate, userId, userId,viewModel.KRAHistoryClonedBy,viewModel.KRAHistoryClonedDate,
                            viewModel.KRAHistoryId,viewModel.KRAGoalId);
                            }
                            context.SaveChanges();
                            transaction.Commit();
                        }
                        return await Task.FromResult(true).ConfigureAwait(false);
                    }
                    catch (DbUpdateException ex)
                    {
                        transaction.Rollback();
                        Log4Net.Error("Error occured in the CloneHistoryDetails() method - InnerException :" + ex.InnerException + "Message :" + ex.Message + "StackTrace :" + ex.StackTrace);
                        throw ex;
                    }
                }
            }
        }

        public async Task<IEnumerable<ValidateDetailsToCloneKRA_Result>> ValidateCloneFromHistory(int PersonId)
        {
            try
            {
                var validateDetailsToClone = new List<ValidateDetailsToCloneKRA_Result>();
                using (PhoenixEntities context = new PhoenixEntities())
                {
                  var validationDetailsList = context.ValidateDetailsToCloneKRA(PersonId);
                  validateDetailsToClone.AddRange(validationDetailsList);
                }
                return await Task.FromResult(validateDetailsToClone).ConfigureAwait(false);
             }
             catch (DbUpdateException ex)
             {
                Log4Net.Error("Error occured in the ValidateCloneFromHistory() method - InnerException :" + ex.InnerException + "Message :" + ex.Message + "StackTrace :" + ex.StackTrace);
                throw ex;
             }
        }

        public async Task<IEnumerable<object>> GetActiveByPersonId(int personId, int[] yearIds)
        {
            try
            {
                if (personId <= 0 || yearIds == null || yearIds.Length == 0)
                {
                    return Enumerable.Empty<object>();
                }

                using (PhoenixEntities context = new PhoenixEntities())
                {
                    var allKRADetails = new List<GetActiveKRAByPersonId_Result>();
                    foreach (var yearId in yearIds)
                    {
                        var personKRADetailsList = context.GetActiveKRAByPersonId(personId, yearId).ToList();
                        allKRADetails.AddRange(personKRADetailsList);
                    }

                    // Group records by KRAGoalId where KRAGoalId > 0
                    var groupedKRAs = allKRADetails.Where(kra => kra.KRAGoalId > 0).GroupBy(kra => kra.KRAGoalId).Select(group => new
                    {
                        KRACategoryId = group.First().KRACategoryId,
                        PersonId = group.First().PersonId,
                        Name = group.First().Name,
                        KRA = group.First().KRA,
                        Description = group.First().Description,
                        Weightage = group.First().Weightage,
                        IsManagerEdit = group.First().IsManagerEdit,
                        IsEmployeeEdit = group.First().IsEmployeeEdit,
                        IsDelete = group.First().IsDelete,
                        IsCompleted = group.First().IsCompleted,
                        IsCloned = group.First().IsCloned,
                        IsKRADone = group.First().IsKRADone,
                        CreatedOn = group.First().CreatedOn.ToString("MM/dd/yyyy"),
                        ModifiedOn = group.First().ModifiedOn.ToString("MM/dd/yyyy"),
                        IsValid = group.First().IsValid,
                        Comments = group.First().Comments,
                        KRAPercentageCompletion = group.First().KRAPercentageCompletion,
                        IsKRAQuarterCompleted = group.First().IsKRAQuarterCompleted,
                        IsKRAInitiationCompleted = group.First().IsKRAInitiationCompleted,
                        KRAInitiationEndDate = group.First().KRAInitiationEndDate?.ToString("MM/dd/yyyy"),
                        IsKRAAvailableForClone = group.First().IsKRAAvailableForClone,
                        HasAttachment = group.First().HasAttachment,
                        KRAGoalId = group.First().KRAGoalId,
                        KRADetails = group.Select(kraDetail => new
                        {
                            kraDetail.Id,
                            kraDetail.ParentKRAId,
                            kraDetail.Q1,
                            kraDetail.Q2,
                            kraDetail.Q3,
                            kraDetail.Q4,
                            kraDetail.YearId,
                            KRAStartDate = kraDetail.KRAStartDate?.ToString("MM/dd/yyyy"),
                            KRAEndDate = kraDetail.KRAEndDate?.ToString("MM/dd/yyyy")
                        }).ToList<object>()
                    }).ToList();

                    return await Task.FromResult(groupedKRAs).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Log4Net.Error("Error occurred in the GetActiveByPersonId() method - InnerException :" + ex.InnerException + "Message :" + ex.Message + "StackTrace :" + ex.StackTrace);
                throw;
            }
        }

        public async Task<bool> SaveOrUpdateProgress(KRAProgressViewModel viewModel, int userId)
        {
            using (PhoenixEntities context = new PhoenixEntities())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        if (viewModel != null)
                        {
                            foreach (var detail in viewModel.KRAProgressDetails)
                            {
                                var mappedKRAProgress = new KRAProgress
                                {
                                    Id = detail.Id,
                                    KRAId = detail.KRAId,
                                    VibrantName = viewModel.VibrantName,
                                    Comment = viewModel.Comment,
                                    KRAGoalId = viewModel.KRAGoalId,
                                    CreatedBy = userId,
                                    ModifiedBy = userId,
                                    CreatedDate = DateTime.Now,
                                    ModifiedDate = DateTime.Now
                                };

                                context.SaveUpdateKRAProgress(mappedKRAProgress.Id, mappedKRAProgress.KRAId, mappedKRAProgress.VibrantName, mappedKRAProgress.Comment, userId, userId, mappedKRAProgress.KRAGoalId);
                            }
                            context.SaveChanges();
                            transaction.Commit();
                        }
                        return await Task.FromResult(true).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        Log4Net.Error("Error occured in the KRAProgressSaveOrUpdate() method - InnerException :" + ex.InnerException + "Message :" + ex.Message + "StackTrace :" + ex.StackTrace);
                        transaction.Rollback();
                        throw ex;
                    }
                }
            }
        }

        public async Task<bool> DeleteProgressEntry(KRAProgressViewModel model)
        {
            bool isDeleted = false;
            using (PhoenixEntities context = new PhoenixEntities())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (var detail in model.KRAProgressDetails)
                        {
                            int id = detail.Id;
                            int modifiedBy = model.ModifiedBy;
                            int kRAGoalId = model.KRAGoalId;

                            // Call the stored procedure for each entry
                            context.DeleteKRAProgressEntry(id, modifiedBy, kRAGoalId);
                        }
                        await context.SaveChangesAsync().ConfigureAwait(false);
                        isDeleted = true;
                        transaction.Commit();
                        return await Task.FromResult(isDeleted).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw ex;
                    }
                }
            }
        }

        public async Task<IEnumerable<KRAAllEmployeesViewModel>> GetInitiationEmployeesList(string initListType, int year)
        {
            var kRAAllEmployeesDefaultList = new List<KRAAllEmployeesViewModel>();
            /* Invalid Employee status*/
            int[] employeeStatusIds = { 3, 4, 10, 11, 6, 7, 8, 14 };
            /* Employee id not eligible for kra */
            var restrictedPersonIDList = new List<int> { 2001, 9, 900012 };
            /* Commonly set designation as Consultant for the below ids */
            var consultantDesignation = new List<string> { "38", "1052", "1053", "1054", "1055", "1056", "1057" };
            try
            {
                int quarterId = 0;
                var quarterStartDate = GetCycleStartDate(GetFinancialQuarter());
                var quarterEndDate = GetCycleEndDate(GetFinancialQuarter());
                //DateTime dt = DateTime.Now;
                //int year = dt.Year;
                quarterId = GetFinancialQuarter();

                if (initListType.Equals("activeemployeeslist"))
                {
                    var initiationList = await GetInitiatedEmployeeList(initListType, employeeStatusIds, restrictedPersonIDList, consultantDesignation, year);
                    kRAAllEmployeesDefaultList = GetActiveEmployeeList(initiationList, kRAAllEmployeesDefaultList, quarterId, year, consultantDesignation);
                }
                else if (initListType.Equals("eligibilitylist"))
                {
                    var initiationList = await GetInitiatedEmployeeList(initListType, employeeStatusIds, restrictedPersonIDList, consultantDesignation, year);
                    kRAAllEmployeesDefaultList = GetEligibilityEmployeeList(initiationList, kRAAllEmployeesDefaultList, quarterId, year, consultantDesignation);
                }
                else if (initListType.Equals("initiationlist"))
                {
                    var initiationList = await GetInitiatedEmployeeList(initListType, employeeStatusIds, restrictedPersonIDList, consultantDesignation, year);
                    kRAAllEmployeesDefaultList.AddRange(initiationList);
                }
                return await Task.FromResult(kRAAllEmployeesDefaultList).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log4Net.Error("Error occured in the GetInitiationEmployeesList() method - InnerException :" + ex.InnerException + "Message :" + ex.Message + "StackTrace :" + ex.StackTrace);
                throw ex;
            }
        }

        private List<KRAAllEmployeesViewModel> GetActiveEmployeeList(IEnumerable<KRAAllEmployeesViewModel> initiationList, List<KRAAllEmployeesViewModel> kRAAllEmployeesDefaultList, int quarterId, int year, IEnumerable<string> consultantDesignation)
        {
            var activeList = new List<KRAAllEmployeesViewModel>();

            /*Query start */
            using (PhoenixEntities context1 = new PhoenixEntities())
            {
                var employeeKRAListObject = context1.GetActiveOrEligibleListForKRA("activeemployeeslist").ToList();

                foreach (var item in employeeKRAListObject)
                {
                    var reviewerList = new List<KRAReviewerListViewModel>();
         
                        var reviewerListFromSP = context1.ReviewerList(item.Id).ToList();
                        foreach (var reviewerItem in reviewerListFromSP)
                        {
                            var reviewerIndividual = new KRAReviewerListViewModel()
                            {
                                FirstName = reviewerItem.FirstName,
                                LastName = reviewerItem.LastName,
                                Id = reviewerItem.Id
                            };
                            reviewerList.Add(reviewerIndividual);
                        }

                    var kraActiveList = new KRAAllEmployeesViewModel()
                    {
                        KraInitiationID = 0,
                        PersonGradeID = item.PersonGradeID,
                        Id = item.Id,
                        FirstName = item.FirstName,
                        LastName = item.LastName,
                        WorkLocation = item.WorkLocation,
                        PersonDesignationID = item.PersonDesignationID,
                        Designation = consultantDesignation.Contains(item.PersonDesignationID.ToString()) ? "Consultant" : item.Designation,
                        CurrentQuarterId = quarterId,
                        InitiatedQuartersList = new List<KRAAllEmployeesViewModel.InitiatedQuartersGroup>
                                                             {
                                                            new InitiatedQuartersGroup
                                                            {
                                                                Q1 = false,
                                                                Q2 = false,
                                                                Q3 = false,
                                                                Q4 = false
                                                            }

                                                             },
                        YearId = year,
                        Q1InitiatedFor = 0,
                        Q2InitiatedFor = 0,
                        Q3InitiatedFor = 0,
                        Q4InitiatiedFor = 0,
                        FirstKRAInitiatedBy = 0,
                        SecondKRAInitiatedBy = 0,
                        ThirdKRAInitiatedBy = 0,
                        FourthKRAInitiatedBy = 0,
                        ReviewerPersonId = item.ReviewerPersonId,
                        ReviewerFullName = reviewerList
                    };

                    activeList.Add(kraActiveList);
                    /*Query end */
                }
            };

            kRAAllEmployeesDefaultList.AddRange(initiationList);
            /* Removing ids existing in the InitiationList from Active Employee list to prevent duplication */
            foreach (var itemRecord in initiationList)
                activeList.RemoveAll(x => x.Id == itemRecord.Id);
            /*Query-end*/

            kRAAllEmployeesDefaultList.AddRange(activeList);
            /*Removing entries in Active/Eligibility list whose all/last quarters are completed*/

            foreach (var items in initiationList)
            {
                if (items.InitiatedQuartersList.ToList<KRAAllEmployeesViewModel.InitiatedQuartersGroup>().ToList<InitiatedQuartersGroup>().FirstOrDefault().Q4 == true)
                {
                    kRAAllEmployeesDefaultList.RemoveAll(x => x.Id == items.Id);
                }
            }
            return kRAAllEmployeesDefaultList;
        }

        private List<KRAAllEmployeesViewModel> GetEligibilityEmployeeList(IEnumerable<KRAAllEmployeesViewModel> initiationList, List<KRAAllEmployeesViewModel> kRAAllEmployeesDefaultList, int quarterId, int year, IEnumerable<string> consultantDesignation)
        {
            var eligibilityList = new List<KRAAllEmployeesViewModel>();

            /*Query start */
            using (PhoenixEntities context1 = new PhoenixEntities())
            {
                var employeeKRAListObject = context1.GetActiveOrEligibleListForKRA("eligibilitylist").ToList();

                foreach (var item in employeeKRAListObject)
                {
                    List<KRAReviewerListViewModel> reviewerList = new List<KRAReviewerListViewModel>();
             
                        var reviewerListFromSP = context1.ReviewerList(item.Id).ToList();
                        foreach (var reviewerItem in reviewerListFromSP)
                        {
                            KRAReviewerListViewModel reviewerIndividual = new KRAReviewerListViewModel()
                            {
                                FirstName = reviewerItem.FirstName,
                                LastName = reviewerItem.LastName,
                                Id = reviewerItem.Id
                            };
                            reviewerList.Add(reviewerIndividual);
                        }

                    KRAAllEmployeesViewModel kraEligibilityList = new KRAAllEmployeesViewModel()
                    {
                        KraInitiationID = 0,
                        PersonGradeID = item.PersonGradeID,
                        Id = item.Id,
                        FirstName = item.FirstName,
                        LastName = item.LastName,
                        WorkLocation = item.WorkLocation,
                        PersonDesignationID = item.PersonDesignationID,
                        Designation = consultantDesignation.Contains(item.PersonDesignationID.ToString()) ? "Consultant" : item.Designation,
                        CurrentQuarterId = quarterId,
                        InitiatedQuartersList = new List<KRAAllEmployeesViewModel.InitiatedQuartersGroup>
                                                             {
                                                            new InitiatedQuartersGroup
                                                            {
                                                                Q1 = false,
                                                                Q2 = false,
                                                                Q3 = false,
                                                                Q4 = false
                                                            }

                                                             },
                        YearId = year,
                        Q1InitiatedFor = 0,
                        Q2InitiatedFor = 0,
                        Q3InitiatedFor = 0,
                        Q4InitiatiedFor = 0,
                        FirstKRAInitiatedBy = 0,
                        SecondKRAInitiatedBy = 0,
                        ThirdKRAInitiatedBy = 0,
                        FourthKRAInitiatedBy = 0,
                        ReviewerPersonId = item.ReviewerPersonId,
                        ReviewerFullName = reviewerList
                    };

                    eligibilityList.Add(kraEligibilityList);

                    /*Query end */
                }
            };

            kRAAllEmployeesDefaultList.AddRange(initiationList);
            /* Removing ids existing in the InitiationList from Eligibility Employee list to prevent duplication */
            foreach (var itemRecord in initiationList)
                eligibilityList.RemoveAll(x => x.Id == itemRecord.Id);
            /*Query-end*/

            kRAAllEmployeesDefaultList.AddRange(eligibilityList);
            /*Removing entries in Active/Eligibility list whose all/last quarters are completed*/

            foreach (var items in initiationList)
            {
                if (items.InitiatedQuartersList.ToList<KRAAllEmployeesViewModel.InitiatedQuartersGroup>().ToList<InitiatedQuartersGroup>().FirstOrDefault().Q4 == true)
                {
                    kRAAllEmployeesDefaultList.RemoveAll(x => x.Id == items.Id);
                }
            }
            return kRAAllEmployeesDefaultList;
        }

        private async Task<IEnumerable<KRAAllEmployeesViewModel>> GetInitiatedEmployeeList(string type, int[] employeeStatusIds, IEnumerable<int> restrictedPersonIDList, IEnumerable<string> consultantDesignation, int year)
        {
            var kRAAllEmployeesDefaultList = new List<KRAAllEmployeesViewModel>();
            //DateTime dt = DateTime.Now;
            int quarterId = GetFinancialQuarter();
            //int month = dt.Month;
            //int year = dt.Year;
            try
            {
                using (PhoenixEntities context = new PhoenixEntities())
                {
                    var initiationListFromSP = context.InitiationList(year, type).ToList();
                    foreach (var item in initiationListFromSP)
                    {
                        var reviewerList = new List<KRAReviewerListViewModel>();

                        var reviewerListFromSP = context.ReviewerList(item.Id).ToList();
                            foreach (var reviewerItem in reviewerListFromSP)
                            {
                                var reviewerIndividual = new KRAReviewerListViewModel()
                                {
                                    FirstName = reviewerItem.FirstName,
                                    LastName = reviewerItem.LastName,
                                    Id = reviewerItem.Id
                                };
                                reviewerList.Add(reviewerIndividual);
                            }
                     
                        var initList = new KRAAllEmployeesViewModel()
                        {
                            KraInitiationID = item.KraInitiationID,
                            PersonGradeID = item.PersonGradeID,
                            Id = item.Id,
                            FirstName = item.FirstName,
                            LastName = item.LastName,
                            WorkLocation = item.WorkLocation,
                            PersonDesignationID = item.PersonDesignationID,
                            Designation = consultantDesignation.Contains(item.PersonDesignationID.ToString()) ? "Consultant" : item.Designation,
                            CurrentQuarterId = quarterId,
                            InitiatedQuartersList = new List<KRAAllEmployeesViewModel.InitiatedQuartersGroup>
                                                             {
                                                            new InitiatedQuartersGroup
                                                            {
                                                                Q1 = item.Q1,
                                                                Q2 = item.Q2,
                                                                Q3 = item.Q3,
                                                                Q4 = item.Q4
                                                            }

                                                             },
                            YearId = item.yearId,
                            Q1InitiatedFor = item.Q1InitiatedFor,
                            Q2InitiatedFor = item.Q2InitiatedFor,
                            Q3InitiatedFor = item.Q3InitiatedFor,
                            Q4InitiatiedFor = item.Q4InitiatiedFor,
                            FirstKRAInitiatedBy = item.FirstKRAInitiatedBy,
                            SecondKRAInitiatedBy = item.SecondKRAInitiatedBy,
                            ThirdKRAInitiatedBy = item.ThirdKRAInitiatedBy,
                            FourthKRAInitiatedBy = item.FourthKRAInitiatedBy,
                            ReviewerPersonId = item.ReviewerPersonId,
                            ReviewerFullName = reviewerList
                        };
                        kRAAllEmployeesDefaultList.Add(initList);
                    }
                };

                return await Task.FromResult(kRAAllEmployeesDefaultList).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<GetKRAAllocatedEmployeesByReviewerId_Result>> GetMyAllocatedEmployees(int reviewerId,int yearId)
        {
            var kRAInitiatedEmployeesList = new List<GetKRAAllocatedEmployeesByReviewerId_Result>();

            try
            {
                using (PhoenixEntities context = new PhoenixEntities())
                {
                    var initiationList = context.GetKRAAllocatedEmployeesByReviewerId(reviewerId, yearId);
                    kRAInitiatedEmployeesList.AddRange(initiationList);
                }
                return await Task.FromResult(kRAInitiatedEmployeesList).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log4Net.Error("Error occured in the GetMyAllocatedEmployees() method - InnerException :" + ex.InnerException + "Message :" + ex.Message + "StackTrace :" + ex.StackTrace);
                throw ex;
            }
        }

        public async Task<IDictionary<string, IEnumerable<DropdownItems>>> GetCategoryDropdown()
        {
            try
            {
                var Items = new Dictionary<string, IEnumerable<DropdownItems>>();
                var lstItems = new List<DropdownItems>();

                using (PhoenixEntities context = new PhoenixEntities())
                {
                    var categoryItems = context.GetCategoryList();
                    bool hasCategory = _service.All<KRACategory>().Any(x => x.IsDeleted == false);
                    if (hasCategory)
                    {
                        foreach (var item in categoryItems)
                        {
                            DropdownItems dropdownItem = new DropdownItems
                            {
                                ID = item.id,
                                Text = item.Name.Trim()
                            };
                            lstItems.Add(dropdownItem);
                        }
                        Items.Add(KRADropDownType.Categories.ToString(), lstItems.ToList());
                    }
                }
                return await Task.FromResult(Items).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log4Net.Error("Error occured in the GetCategoryDropdown() method - InnerException :" + ex.InnerException + "Message :" + ex.Message + "StackTrace :" + ex.StackTrace);
                throw ex;
            }
        }

        public async Task<IDictionary<string, IEnumerable<DropdownItems>>> GetGradeList()
        {
            try
            {
                var Items = new Dictionary<string, IEnumerable<DropdownItems>>();
                var lstItems = new List<DropdownItems>();

                using (PhoenixEntities context = new PhoenixEntities())
                {
                    var gradeItems = context.GetGradeList();
                    bool hasGrades = _service.All<Designation>().Any(x => x.Grade != null);
                    if (hasGrades)
                    {
                        foreach (var item in gradeItems)
                        {
                            DropdownItems dropdownItem = new DropdownItems
                            {
                                ID = Convert.ToInt32(item.Id),
                                Text = item.Grade.ToString()
                            };
                            lstItems.Add(dropdownItem);
                        }
                        Items.Add(KRADropDownType.Grade.ToString(), lstItems.ToList());
                    }
                }
                return await Task.FromResult(Items).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log4Net.Error("Error occured in the GetGradeDropdown() method - InnerException :" + ex.InnerException + "Message :" + ex.Message + "StackTrace :" + ex.StackTrace);
                throw ex;
            }
        }

        public async Task<IEnumerable<KRAProgressViewModel>> GetProgressList(int kraGoalId)
        {
            try
            {
                var kraProgressList = new List<KRAProgressViewModel>();

                if (kraGoalId > 0)
                {
                    using (PhoenixEntities context = new PhoenixEntities())
                    {
                        var resultKRAProgress = context.GetProgressListById(kraGoalId);

                        var filteredResults = resultKRAProgress.Where(r => r.KRAGoalId == kraGoalId).ToList();

                        // Grouping the filtered results based on distinct properties, creating the desired view model
                        var groupedResults = filteredResults
                            .GroupBy(x => new
                            {
                                x.VibrantName,
                                x.CreatedBy,
                                x.CreatedDate,
                                x.ModifiedBy,
                                x.ModifiedDate,
                                x.KRAGoalId,
                                x.Comment,
                                x.CommentedBy,
                                x.IsDeleted
                            })
                            .Select(g => new KRAProgressViewModel
                            {
                                VibrantName = g.Key.VibrantName,
                                CreatedBy = g.Key.CreatedBy,
                                CreatedDate = g.Key.CreatedDate,
                                ModifiedBy = g.Key.ModifiedBy,
                                ModifiedDate = g.Key.ModifiedDate,
                                KRAGoalId = g.Key.KRAGoalId,
                                Comment = g.Key.Comment,
                                CommentedBy = g.Key.CommentedBy,
                                IsDeleted = g.Key.IsDeleted,
                                KRAProgressDetails = g.Select(d => new KRAProgressDetail
                                {
                                    Id = d.Id,
                                    KRAId = d.KRAId
                                }).ToList()
                            }).ToList();

                        kraProgressList.AddRange(groupedResults);
                    }
                }

                return await Task.FromResult(kraProgressList).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log4Net.Error("Error occured in the GetProgressList() method - InnerException :" + ex.InnerException + "Message :" + ex.Message + "StackTrace :" + ex.StackTrace);
                throw ex;
            }
        }

        public async Task<IEnumerable<KRAFileAttachment>> GetAttachment(int kraGoalId)
        {
            try
            {
                using (PhoenixEntities context = new PhoenixEntities())
                {
                    // Fetch all attachments related to the given kraGoalId
                    var resultKRAAttachment = context.GetKRAAttachmentById(kraGoalId).ToList();
                    if (resultKRAAttachment == null || !resultKRAAttachment.Any())
                        return Enumerable.Empty<KRAFileAttachment>();

                    var lstKRAFileAttachment = resultKRAAttachment
                        .GroupBy(attachment => new
                        {
                            attachment.FileUrl,
                            attachment.FileName,
                            attachment.FileUploadedBy,
                            attachment.IsDeleted
                        })
                        .Select(g => new KRAFileAttachment
                        {
                            FileURL = g.Key.FileUrl,
                            FileName = g.Key.FileName,
                            FileUploadedOn = g.First().FileUploadedOn.GetValueOrDefault(DateTime.Now),
                            FileUploadedBy = g.Key.FileUploadedBy,
                            IsDeleted = g.Key.IsDeleted,
                            KRAGoalId = kraGoalId,
                            KRAAttachments = g.Select(a => new KRAAttachment
                            {
                                Id = a.Id,
                                KRAId = a.KRAId
                            }).ToList()
                        })
                        .ToList();

                    return await Task.FromResult(lstKRAFileAttachment).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Log4Net.Error("Error occurred in the GetAttachment() method - InnerException: " + ex.InnerException + " Message: " + ex.Message + " StackTrace: " + ex.StackTrace);
                throw;
            }
        }

        private PersonKRA GetPerson(int kraInitiationId, int kraYear, int personId)
        {
            try
            {
                return _service.First<PersonKRA>(x => x.KRAYearId == kraYear && x.PersonId == personId);
            }
            catch (Exception ex)
            {
                Log4Net.Error("Error occured in the GetDetails() method - InnerException :" + ex.InnerException + "Message :" + ex.Message + "StackTrace :" + ex.StackTrace);
                return new PersonKRA();
            }
        }

        public async Task<bool> InitiatePerson(IEnumerable<PersonKRAViewModel> viewModel)
        {
            var initiatedPersonId = new List<int>();
            using (PhoenixEntities context = new PhoenixEntities())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (var item in viewModel)
                        {
                            var _mappedData = Mapper.Map<PersonKRAViewModel, PersonKRA>(item);
                            var kraExistingRecord = context.ExistingKRAInitiation(item.PersonId, item.KRAYearId).FirstOrDefault();

                            int cycle = 0;
                            int latestQuarterForCycle = 0;
                            int earliestQuarterForCycle = 0;

                            _mappedData.FirstKRAInitiatedOn = new DateTime(1753, 1, 1);
                            _mappedData.SecondKRAInitiatedOn = new DateTime(1753, 1, 1);
                            _mappedData.ThirdKRAInitiatedOn = new DateTime(1753, 1, 1);
                            _mappedData.FourthKRAInitiatedOn = new DateTime(1753, 1, 1);

                            _mappedData.FirstKRAInitiationStartDate = new DateTime(1753, 1, 1);
                            _mappedData.FirstKRAInitiationEndDate = new DateTime(1753, 1, 1);
                            _mappedData.FirstKRAInitiationFreezedDate = new DateTime(1753, 1, 1);

                            _mappedData.SecondKRAInitiationStartDate = new DateTime(1753, 1, 1);
                            _mappedData.SecondKRAInitiationEndDate = new DateTime(1753, 1, 1);
                            _mappedData.SecondKRAInitiationFreezedDate = new DateTime(1753, 1, 1);

                            _mappedData.ThirdKRAInitiationStartDate = new DateTime(1753, 1, 1);
                            _mappedData.ThirdKRAInitiationEndDate = new DateTime(1753, 1, 1);
                            _mappedData.ThirdKRAInitiationFreezedDate = new DateTime(1753, 1, 1);

                            _mappedData.FourthKRAInitiationStartDate = new DateTime(1753, 1, 1);
                            _mappedData.FourthKRAInitiationEndDate = new DateTime(1753, 1, 1);
                            _mappedData.FourthKRAInitiationFreezedDate = new DateTime(1753, 1, 1);

                            /* New/Fresh KRA Initiation */
                            if (_mappedData.KRAInitiationId == 0)
                            {
                                cycle = 1;
                                latestQuarterForCycle = (_mappedData.Q4 == true) ? 4 : (_mappedData.Q3 == true) ? 3 : (_mappedData.Q2 == true) ? 2 :
                                        (_mappedData.Q1 == true) ? 1 : 0;
                                earliestQuarterForCycle = ((_mappedData.Q1 == true) ? 1 : (_mappedData.Q2 == true) ? 2 : (_mappedData.Q3 == true) ? 3 : (_mappedData.Q4 == true) ? 4 : 0);
                            }
                            else
                            {
                                cycle = (_mappedData.FourthKRAInitiatedBy > 0) ? 4 : (_mappedData.ThirdKRAInitiatedBy > 0) ? 3 : (_mappedData.SecondKRAInitiatedBy > 0 == true) ? 2 :
                                (_mappedData.FirstKRAInitiatedBy > 0) ? 1 : 0;

                                latestQuarterForCycle = (_mappedData.Q4 == true) ? 4 : (_mappedData.Q3 == true) ? 3 : (_mappedData.Q2 == true) ? 2 :
                                        (_mappedData.Q1 == true) ? 1 : 0;
                                earliestQuarterForCycle = ((_mappedData.Q1 != kraExistingRecord.Q1) ? 1 : (_mappedData.Q2 != kraExistingRecord.Q2) ? 2 : (_mappedData.Q3 != kraExistingRecord.Q3) ? 3 : (_mappedData.Q4 != kraExistingRecord.Q4) ? 4 : 0);
                            }

                            switch (cycle)
                            {
                                case 4:
                                    _mappedData.FourthKRAInitiationStartDate = GetCycleStartDate(earliestQuarterForCycle);
                                    _mappedData.FourthKRAInitiationEndDate = GetCycleEndDate(latestQuarterForCycle);
                                    _mappedData.FourthKRAInitiatedOn = DateTime.Now;

                                    /* Getting the previous initiation dates from database */
                                    _mappedData.ThirdKRAInitiationStartDate = kraExistingRecord.ThirdKRAInitiationStartDate;
                                    _mappedData.ThirdKRAInitiationEndDate = kraExistingRecord.ThirdKRAInitiationEndDate;
                                    _mappedData.ThirdKRAInitiatedOn = kraExistingRecord.ThirdKRAInitiatedOn;
                                    _mappedData.SecondKRAInitiationStartDate = kraExistingRecord.SecondKRAInitiationStartDate;
                                    _mappedData.SecondKRAInitiationEndDate = kraExistingRecord.SecondKRAInitiationEndDate;
                                    _mappedData.SecondKRAInitiatedOn = kraExistingRecord.SecondKRAInitiatedOn;
                                    _mappedData.FirstKRAInitiationStartDate = kraExistingRecord.FirstKRAInitiationStartDate;
                                    _mappedData.FirstKRAInitiationEndDate = kraExistingRecord.FirstKRAInitiationEndDate;
                                    _mappedData.FirstKRAInitiatedOn = kraExistingRecord.FirstKRAInitiatedOn;
                                    /* End: Getting the previous initiation dates from database */

                                    _mappedData.Q1InitiatedFor = kraExistingRecord.Q1InitiatedFor;
                                    _mappedData.Q2InitiatedFor = kraExistingRecord.Q2InitiatedFor;
                                    _mappedData.Q3InitiatedFor = kraExistingRecord.Q3InitiatedFor;
                                    _mappedData.Q4InitiatiedFor = (_mappedData.Q4 == true && kraExistingRecord.Q4InitiatiedFor == 0) ? 4 : kraExistingRecord.Q4InitiatiedFor;
                                    break;
                                case 3:
                                    _mappedData.ThirdKRAInitiationStartDate = GetCycleStartDate(earliestQuarterForCycle);
                                    _mappedData.ThirdKRAInitiationEndDate = GetCycleEndDate(latestQuarterForCycle);
                                    _mappedData.ThirdKRAInitiatedOn = DateTime.Now;

                                    /* Getting the previous initiation dates from database */
                                    _mappedData.SecondKRAInitiationStartDate = kraExistingRecord.SecondKRAInitiationStartDate;
                                    _mappedData.SecondKRAInitiationEndDate = kraExistingRecord.SecondKRAInitiationEndDate;
                                    _mappedData.SecondKRAInitiatedOn = kraExistingRecord.SecondKRAInitiatedOn;
                                    _mappedData.FirstKRAInitiationStartDate = kraExistingRecord.FirstKRAInitiationStartDate;
                                    _mappedData.FirstKRAInitiationEndDate = kraExistingRecord.FirstKRAInitiationEndDate;
                                    _mappedData.FirstKRAInitiatedOn = kraExistingRecord.FirstKRAInitiatedOn;
                                    /* End: Getting the previous initiation dates from database */

                                    _mappedData.Q1InitiatedFor = kraExistingRecord.Q1InitiatedFor;
                                    _mappedData.Q2InitiatedFor = kraExistingRecord.Q2InitiatedFor;
                                    _mappedData.Q3InitiatedFor = (_mappedData.Q3 == true && kraExistingRecord.Q3InitiatedFor == 0) ? 3 : kraExistingRecord.Q3InitiatedFor;
                                    _mappedData.Q4InitiatiedFor = (_mappedData.Q4 == true && kraExistingRecord.Q4InitiatiedFor == 0) ? 3 : kraExistingRecord.Q4InitiatiedFor;
                                    break;
                                case 2:
                                    _mappedData.SecondKRAInitiationStartDate = GetCycleStartDate(earliestQuarterForCycle);
                                    _mappedData.SecondKRAInitiationEndDate = GetCycleEndDate(latestQuarterForCycle);
                                    _mappedData.SecondKRAInitiatedOn = DateTime.Now;

                                    /* Getting the previous initiation dates from database */
                                    _mappedData.FirstKRAInitiationStartDate = kraExistingRecord.FirstKRAInitiationStartDate;
                                    _mappedData.FirstKRAInitiationEndDate = kraExistingRecord.FirstKRAInitiationEndDate;
                                    _mappedData.FirstKRAInitiatedOn = kraExistingRecord.FirstKRAInitiatedOn;
                                    /* End: Getting the previous initiation dates from database */

                                    _mappedData.Q1InitiatedFor = kraExistingRecord.Q1InitiatedFor;
                                    _mappedData.Q2InitiatedFor = (_mappedData.Q2 == true && kraExistingRecord.Q2InitiatedFor == 0) ? 2 : kraExistingRecord.Q2InitiatedFor;
                                    _mappedData.Q3InitiatedFor = (_mappedData.Q3 == true && kraExistingRecord.Q3InitiatedFor == 0) ? 2 : kraExistingRecord.Q3InitiatedFor;
                                    _mappedData.Q4InitiatiedFor = (_mappedData.Q4 == true && kraExistingRecord.Q4InitiatiedFor == 0) ? 2 : kraExistingRecord.Q4InitiatiedFor;
                                    break;
                                case 1:
                                    _mappedData.FirstKRAInitiationStartDate = GetCycleStartDate(earliestQuarterForCycle);
                                    _mappedData.FirstKRAInitiationEndDate = GetCycleEndDate(latestQuarterForCycle);
                                    _mappedData.FirstKRAInitiatedOn = DateTime.Now;
                                    _mappedData.Q1InitiatedFor = (_mappedData.Q1 == true) ? 1 : 0;
                                    _mappedData.Q2InitiatedFor = (_mappedData.Q2 == true) ? 1 : 0;
                                    _mappedData.Q3InitiatedFor = (_mappedData.Q3 == true) ? 1 : 0;
                                    _mappedData.Q4InitiatiedFor = (_mappedData.Q4 == true) ? 1 : 0;
                                    break;
                                default:
                                    break;
                            }

                                context.InitiatePersonKRA(_mappedData.PersonId, _mappedData.KRAYearId, _mappedData.Q1,
                                        _mappedData.Q2, _mappedData.Q3, _mappedData.Q4, _mappedData.ReviewerPersonId, _mappedData.FirstKRAInitiatedOn,
                                        _mappedData.FirstKRAInitiationStartDate, _mappedData.FirstKRAInitiationEndDate, _mappedData.FirstKRAInitiatedBy, _mappedData.FirstKRAInitiationFreezedDate,
                                        _mappedData.FirstKRAInitiationFreezedBy, _mappedData.IsFirstKRAInitiationFreezed, _mappedData.SecondKRAInitiatedOn, _mappedData.SecondKRAInitiationStartDate,
                                        _mappedData.SecondKRAInitiationEndDate, _mappedData.SecondKRAInitiatedBy, _mappedData.SecondKRAInitiationFreezedDate, _mappedData.SecondKRAInitiationFreezedBy,
                                        _mappedData.IsSecondKRAInitiationFreezed, _mappedData.ThirdKRAInitiatedOn, _mappedData.ThirdKRAInitiationStartDate, _mappedData.ThirdKRAInitiationEndDate, _mappedData.ThirdKRAInitiatedBy,
                                        _mappedData.ThirdKRAInitiationFreezedDate, _mappedData.ThirdKRAInitiationFreezedBy, _mappedData.IsThirdKRAInitiationFreezed, _mappedData.FourthKRAInitiatedOn, _mappedData.FourthKRAInitiationStartDate,
                                        _mappedData.FourthKRAInitiationEndDate, _mappedData.FourthKRAInitiatedBy, _mappedData.FourthKRAInitiationFreezedDate, _mappedData.FourthKRAInitiationFreezedBy, _mappedData.IsFourthKRAInitiationFreezed,
                                        _mappedData.FirstKRAInitiationPercentageCompletion, _mappedData.SecondKRAInitiationPercentageCompletion, _mappedData.ThirdKRAInitiationPercentageCompletion, _mappedData.FourthKRAInitiationPercentageCompletion,
                                        _mappedData.OverAllYearlyPercentageCompletion, _mappedData.Q1InitiatedFor, _mappedData.Q2InitiatedFor, _mappedData.Q3InitiatedFor, _mappedData.Q4InitiatiedFor, cycle);
                                context.SaveChanges();
                                initiatedPersonId.Add(_mappedData.PersonId);
                                MapPersonInitation(_mappedData, cycle);
                        }

                        //_service.Finalize(true);
                        //MapInitiationIdFromPersonId(initiatedPersonId);
                        var stringsInitiatedPersonArray = initiatedPersonId.Select(i => i.ToString()).ToArray();
                        var values = string.Join(",", stringsInitiatedPersonArray);
                    
                        context.MapKRAInitiation(values);
                        context.SaveChanges();
                     
                        transaction.Commit();

                        return await Task.FromResult(true).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Log4Net.Error("Error occured in the InitiatePersonKra() method - InnerException :" + ex.InnerException + "Message :" + ex.Message + "StackTrace :" + ex.StackTrace);
                        throw ex;
                    }
                }
            }
        }

        private void MapInitiationIdFromPersonId(List<int> initiatedPersonId)
        {
            try
            {
                using (PhoenixEntities context = new PhoenixEntities())
                {
                    foreach (var personId in initiatedPersonId)
                    {
                        var kraInitiationId = (from p in context.PersonKRAs
                                               where p.PersonId == personId && p.KRAYearId == DateTime.Now.Year
                                               select p.KRAInitiationId
                                              ).FirstOrDefault();

                        var existingKRAMapping = _service.All<PersonKRAMapping>(x => x.PersonId == personId && x.KRAInitiationYear == DateTime.Now.Year).FirstOrDefault();
                        if (existingKRAMapping != null)
                        {
                            existingKRAMapping.ParentKRAInitiationId = kraInitiationId;
                            _service.Update(existingKRAMapping);
                            _service.Finalize(true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log4Net.Error("Error occured in the GetDetails() method - InnerException :" + ex.InnerException + "Message :" + ex.Message + "StackTrace :" + ex.StackTrace);
                throw;
            }
        }

        private DateTime GetCycleEndDate(int kraLatestQuarter)
        {
            DateTime datetime = DateTime.Now;
            if (kraLatestQuarter == 4)
            {
                var dtLastDay = new DateTime((datetime.Year + 1), (3 * kraLatestQuarter + 1) % 12, 1).AddDays(-1);
                return dtLastDay;
            }
            else
            {
                var dtLastDay = new DateTime(datetime.Year, 3 * kraLatestQuarter + 1, 1).AddDays(-1);
                return dtLastDay;
            }
        }

        private DateTime GetCycleStartDate(int kraEarliestQuarter)
            => new DateTime(DateTime.Now.Year, (3 * kraEarliestQuarter) - 2, 1);

        public async Task<KRAInitiationDetail> GetInitiationStatus(int personId)
        {
            try
            {
                var mappedInitiationStatus = new KRAInitiationDetail();
                try
                {
                    using (PhoenixEntities context = new PhoenixEntities())
                    {
                        
                        var personKRAInitiation = context.KRAInitiationStatus(personId, DateTime.Now.Year).FirstOrDefault();
                        mappedInitiationStatus = Mapper.Map<KRAInitiationStatus_Result, KRAInitiationDetail>(personKRAInitiation);

                        List<PersonIdListDetails> personIdListDetails = new List<PersonIdListDetails>();
                        if(mappedInitiationStatus.PersonIdList.Length > 1)
                        {
                            foreach (var item in mappedInitiationStatus.PersonIdList.Split(',').ToList())
                            {
                                personIdListDetails.Add(context.People.Where(x => x.ID.ToString() == item).Select(x => new PersonIdListDetails()
                                {
                                    Id = x.ID,
                                    FirstName = x.FirstName,
                                    LastName = x.LastName
                                }).FirstOrDefault());
                            };
                            mappedInitiationStatus.PersonIdListDetails = personIdListDetails;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log4Net.Error("Error occured in the GetInitiationStatus() method - InnerException :" + ex.InnerException + "Message :" + ex.Message + "StackTrace :" + ex.StackTrace);
                    throw ex;
                }
                return await Task.FromResult(mappedInitiationStatus).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log4Net.Error("Error occured in the GetInitiationStatus() method - InnerException :" + ex.InnerException + "Message :" + ex.Message + "StackTrace :" + ex.StackTrace);
                throw;
            }
        }

        private static int GetFinancialQuarter() => (DateTime.Now.Month + 2) / 3;

        public async Task<List<KRACycleConfiguration>> GetCycleConfiguration(int personId, int[] yearIds)
        {           
            try
            {
                List<KRACycleConfiguration> kraCycleConfigurations = new List<KRACycleConfiguration>(); // Store all results
                using (PhoenixEntities context = new PhoenixEntities())
                {
                    foreach (var yearId in yearIds)
                    {
                        var personKRAInitiation = context.GetCycleConfiguration(personId, yearId).FirstOrDefault();
                        if (personKRAInitiation != null)
                        {
                            var kraCycleConfiguration = new KRACycleConfiguration()
                            {
                                PersonId = personKRAInitiation.PersonId,
                                KRAInitiationId = personKRAInitiation.KRAInitiationId,
                                Q1 = personKRAInitiation.Q1,
                                Q2 = personKRAInitiation.Q2,
                                Q3 = personKRAInitiation.Q3,
                                Q4 = personKRAInitiation.Q4,
                                Year = personKRAInitiation.Year,
                                ReviewerPersonId = personKRAInitiation.ReviewerPersonId,
                                ReviewerPersonName = personKRAInitiation.ReviewerPersonName
                            };
                            kraCycleConfigurations.Add(kraCycleConfiguration); // Add the result to the list
                        }
                    }
                }
                return await Task.FromResult(kraCycleConfigurations).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log4Net.Error("Error occured in the GetCycleConfiguration() method - InnerException :" + ex.InnerException + "Message :" + ex.Message + "StackTrace :" + ex.StackTrace);
                throw ex;
            }
        }

        public async Task<bool> MarkAsInvalid(InvalidKRADetails viewModel)
        {
            try
            {
                using (PhoenixEntities context = new PhoenixEntities())
                {
                    using (var transaction = context.Database.BeginTransaction())
                    {
                        context.MarkAsInvalid(viewModel.IsValid,viewModel.Comments,viewModel.KRAGoalId);
                        context.SaveChanges();
                        transaction.Commit();
                        return await Task.FromResult(true).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                Log4Net.Error("Error occured in the MarkKraAsInvalid() method - InnerException :" + ex.InnerException + "Message :" + ex.Message + "StackTrace :" + ex.StackTrace);
                throw ex;
            }
        }

        public async Task<bool> MarkAsDone(KRAMarkDone kraMarkDone) 
        {
            try
            {
                using (PhoenixEntities context = new PhoenixEntities())
                {
                    using (var transaction = context.Database.BeginTransaction())
                    {
                        if (kraMarkDone.KRAGoalId > 0)
                        {
                            context.MarkAsDone(kraMarkDone.PersonId, true, DateTime.Now, kraMarkDone.KRADoneDate, kraMarkDone.KRAGoalId);
                            context.SaveChanges();
                        }
                        transaction.Commit();
                        return await Task.FromResult(true).ConfigureAwait(false);
                    }
                }
            }
            catch (DbUpdateException ex)
            {
                Log4Net.Error("Error occured in the MarkKraAsDone() method - InnerException :" + ex.InnerException + "Message :" + ex.Message + "StackTrace :" + ex.StackTrace);
                throw ex;
            }
        }

        private bool MapPersonInitation(PersonKRA personKRA, int cycle)
        {
            bool isQ1Initiated = false,
                isQ2Initiated = false,
                isQ3Initiated = false,
                isQ4Initiated = false;
            try
            {
                IEnumerable<PersonKRAMapping> personKRAMappings = _service.All<PersonKRAMapping>(x => x.PersonId == personKRA.PersonId && x.KRAInitiationYear == personKRA.KRAYearId).AsEnumerable();

                if (personKRAMappings.Count() != 0)
                {
                    foreach (var personKRAMap in personKRAMappings)
                    {
                        if (personKRAMap.Q1)
                            isQ1Initiated = true;
                        if (personKRAMap.Q2)
                            isQ2Initiated = true;
                        if (personKRAMap.Q3)
                            isQ3Initiated = true;
                        if (personKRAMap.Q4)
                            isQ4Initiated = true;
                    }
                    _service.Create(new PersonKRAMapping()
                    {
                        InitiatedFor = cycle,
                        ParentKRAInitiationId = personKRA.KRAInitiationId,
                        KRAInitiationYear = personKRA.KRAYearId,
                        PersonId = personKRA.PersonId,
                        Q1 = isQ1Initiated ? false : personKRA.Q1,
                        Q2 = isQ2Initiated ? false : personKRA.Q2,
                        Q3 = isQ3Initiated ? false : personKRA.Q3,
                        Q4 = isQ4Initiated ? false : personKRA.Q4,
                        IsParent = false
                    }, null);
                }
                else
                {
                    _service.Create(new PersonKRAMapping()
                    {
                        InitiatedFor = cycle,
                        ParentKRAInitiationId = personKRA.KRAInitiationId,
                        KRAInitiationYear = personKRA.KRAYearId,
                        PersonId = personKRA.PersonId,
                        Q1 = personKRA.Q1,
                        Q2 = personKRA.Q2,
                        Q3 = personKRA.Q3,
                        Q4 = personKRA.Q4,
                        IsParent = true
                    }, null);
                }
                return true;
            }
            catch (Exception ex)
            {
                Log4Net.Error("Error occured in the MapPersonKraInitation() method - InnerException :" + ex.InnerException + "Message :" + ex.Message + "StackTrace :" + ex.StackTrace);
                return false;
            }
        }

        private bool IsAvailableForClone(int kRAId)
        {
            try
            {
                var existingKRA = _service.All<PersonKRADetail>(x => x.Id == kRAId).FirstOrDefault();
                if (existingKRA.KRAEndDate < DateTime.Now && existingKRA.IsKRAAvailableForClone == true && existingKRA.IsKRAQuarterCompleted == true)
                    return true;

                return false;
            }
            catch (Exception ex)
            {
                Log4Net.Error("Error occured in the GetDetails() method - InnerException :" + ex.InnerException + "Message :" + ex.Message + "StackTrace :" + ex.StackTrace);
                throw;
            }
        }

        private static KRAData GetStartDateEndDate(bool Q1, bool Q2, bool Q3, bool Q4)
        {
            var kRAData = new KRAData();
            var objDate= new ObjectParameter("startEndDate", typeof(string));
            var strStartEndDate = string.Empty;
            try
            {
                using (PhoenixEntities context = new PhoenixEntities())
                {
                    var kraStartEndDate= context.GetKRAStartEndDate(Q1, Q2, Q3, Q4, objDate);
                    foreach (var item in kraStartEndDate)
                    {
                        strStartEndDate = item;
                    }
                    string[] KRAStartEndDate= strStartEndDate.Split('#');
                    kRAData.KRAStartDate = DateTime.Parse(KRAStartEndDate[0].ToString());
                    kRAData.KRAEndDate = DateTime.Parse(KRAStartEndDate[1].ToString());
                }
            }
            catch (Exception ex)
            {
                Log4Net.Error("Error occured in the GetStartDateEndDate() method - InnerException :" + ex.InnerException + "Message :" + ex.Message + "StackTrace :" + ex.StackTrace);
                throw ex;
            }
            return kRAData;          
        }

        public async Task<bool> UpdateProgressBar(int kraGoalId, int percentageValue)
        {
            try
            {
                var personKRAs = _service.All<PersonKRADetail>().Where(x => x.KRAGoalId == kraGoalId && !x.IsDelete).ToList();

                if (personKRAs == null || !personKRAs.Any())
                    return false;

                foreach (var personKRA in personKRAs)
                {
                    personKRA.KRAPercentageCompletion = percentageValue;
                    _service.Update(personKRA);
                }

                _service.Finalize(true);
                return await Task.FromResult(true).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log4Net.Error($"Error occurred in UpdateProgressBar() method. Message: {ex.Message}, StackTrace: {ex.StackTrace}");
                return false;
            }
        }

        public async Task<bool> ChangeReviewerById(int kraInitiationId, int newReviewerId)
        {
            try
            {
                using (PhoenixEntities context = new PhoenixEntities())
                {
                    using (var transaction = context.Database.BeginTransaction())
                    {
                        context.ChangeKRAReviewerById(kraInitiationId, DateTime.Now.Year, newReviewerId);
                        context.SaveChanges();
                        transaction.Commit();
                        return await Task.FromResult(true).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                Log4Net.Error("Error occured in the ChangeReviewer() method - InnerException :" + ex.InnerException + "Message :" + ex.Message + "StackTrace :" + ex.StackTrace);
                throw ex;
            }
        }

        public async Task<IEnumerable<GetKRAAllocatedEmployeesByReviewerId_Result>> GetMyAllocatedEmployeesForReports(int reviewerId, int yearId)
        {
            var kRAInitiatedEmployeesList = new List<GetKRAAllocatedEmployeesByReviewerId_Result>();

            try
            {
                using (PhoenixEntities context = new PhoenixEntities())
                {
                    var initiationList = context.GetKRAAllocatedEmployeesByReviewerId(reviewerId, yearId);
                    kRAInitiatedEmployeesList.AddRange(initiationList);
                }
                return await Task.FromResult(kRAInitiatedEmployeesList).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log4Net.Error("Error occured in the GetMyAllocatedEmployeesForReports() method - InnerException :" + ex.InnerException + "Message :" + ex.Message + "StackTrace :" + ex.StackTrace);
                throw ex;
            }
        }

        public async Task<IEnumerable<KRAReportViewModel>> SearchAllKRADetail(string personId, string kraCategoryId, string yearId, string weightageId, string quarters, string isValid, string isInValid, string isKRADone)
        {
            var kRASearchResult = new List<KRAReportViewModel>();

            try
            {
                using (PhoenixEntities context = new PhoenixEntities())
                {
                    personId = personId ?? "";
                    kraCategoryId = kraCategoryId ?? "";
                    yearId = yearId ?? DateTime.Now.Year.ToString();
                    weightageId = weightageId ?? "";
                    quarters = quarters ?? "";
                    if((isValid == null && isInValid == null) || (isValid == "1" && isInValid == "1"))
                    {
                        isValid = "";
                    }
                    else if(isValid == "1" && isInValid == null)
                    {
                        isValid = "1";
                    }
                    else if(isInValid == "1" && isValid == null)
                    {
                        isValid = "0";
                    }
                    isKRADone = isKRADone ?? "";

                    var result = new List<Rpt_SearchAllKRADetail_Result>();
                    if(yearId == DateTime.Now.Year.ToString())
                    {
                        result = context.Rpt_SearchAllKRADetail(personId, kraCategoryId, yearId, weightageId, quarters, isValid.ToString(), isKRADone.ToString()).ToList();
                    }
                    else
                    {
                        var mappedHistoryData = context.SearchAllKRAHistoryDetail(personId, yearId, isKRADone.ToString()).ToList();
                        result = Mapper.Map<List<SearchAllKRAHistoryDetail_Result>, List<Rpt_SearchAllKRADetail_Result>>(mappedHistoryData);
                    }
                    var finalResult = Mapper.Map<List<Rpt_SearchAllKRADetail_Result>, List<KRAReportViewModel>>(result);
                    kRASearchResult.AddRange(finalResult);
                }
                return await Task.FromResult(kRASearchResult).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log4Net.Error("Error occured in the SearchAllKRADetail() method - InnerException :" + ex.InnerException + "Message :" + ex.Message + "StackTrace :" + ex.StackTrace);
                throw ex;
            }
        }

        public async Task<bool> DownloadReport(string personId, string kraCategoryId, string yearId, string weightageId, string quarters, string isValid, string isKRADone)
        {
            var kRASearchResult = new List<Rpt_SearchAllKRADetail_Result>();
            DateTime currentDateTime = DateTime.Now;
            try
            {
                using (PhoenixEntities context = new PhoenixEntities())
                {
                    var result = context.Rpt_SearchAllKRADetail(personId, kraCategoryId, yearId, weightageId, quarters, isValid.ToString(), isKRADone.ToString());
                    kRASearchResult.AddRange(result);

                    GetReportDetailsAttachment(kRASearchResult, currentDateTime);
                }
                return await Task.FromResult(true).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log4Net.Error("Error occured in the DownloadReport() method - InnerException :" + ex.InnerException + "Message :" + ex.Message + "StackTrace :" + ex.StackTrace);
                throw ex;
            }
        }

        public async Task<IEnumerable<object>> SearchAllKRAHistoryDetails(string personId, string yearId, string isKRADone)
        {
            var kRASearchResult = new List<SearchAllKRAHistoryDetail_Result>();

            try
            {
                using (PhoenixEntities context = new PhoenixEntities())
                {
                    personId = personId ?? "";
                    yearId = yearId ?? "";
                    isKRADone = isKRADone ?? "";
                    var result = context.SearchAllKRAHistoryDetail(personId, yearId, isKRADone.ToString()).ToList();

                    var groupedKRAs = result.Where(kra => kra.KRAGoalId > 0).GroupBy(kra => kra.KRAGoalId).Select(group => new
                    {
                        KRACategoryId = group.First().KRACategoryId,
                        PersonId = group.First().PersonId,
                        EmployeeName = group.First().EmployeeName, 
                        KRA = group.First().KRA,
                        Description = group.First().Description,
                        IsValid = group.First().IsValid,
                        IsCloned = group.First().IsCloned,
                        IsKRADone = group.First().IsKRADone,
                        KRADoneOn = group.First().KRADoneOn,
                        KRAPercentageCompletion = group.First().KRAPercentageCompletion,
                        CreatedBY = group.First().CreatedBY,
                        ModifiedBy = group.First().ModifiedBy,
                        ReviewerPersonId = group.First().ReviewerPersonId,
                        ReviewerName = group.First().ReviewerName,
                        IsCloneAvailable = group.First().IsCloneAvailable,
                        KRAGoalId = group.First().KRAGoalId,
                        SearchAllKRAHistory = group.Select(SearchAllKRAHistory => new
                        {
                            Id = SearchAllKRAHistory.ID,
                            YearId = SearchAllKRAHistory.YearId,
                            Q1 = SearchAllKRAHistory.Q1,
                            Q2 = SearchAllKRAHistory.Q2,
                            Q3 = SearchAllKRAHistory.Q3,
                            Q4 = SearchAllKRAHistory.Q4,
                            KRAStartDate = SearchAllKRAHistory.KRAStartDate,
                            KRAEndDate = SearchAllKRAHistory.KRAEndDate
                        }).ToList()
                    }).ToList();

                    return await Task.FromResult(groupedKRAs).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Log4Net.Error("Error occured in the SearchAllKRAHistoryDetail() method - InnerException :" + ex.InnerException + "Message :" + ex.Message + "StackTrace :" + ex.StackTrace);
                throw ex;
            }
        }

        private static string GetReportDetailsAttachment(List<Rpt_SearchAllKRADetail_Result> model, DateTime currentDateTime)
        {
            try
            {
                string fileName = string.Concat("KRASummary", "_", currentDateTime.ToString("yyyy_MM_dd"), ".xlsx");
                string filePath = string.Concat(GetFolderPath(), fileName);
                bool isExcelCreated = false;
                isExcelCreated = GenerateAttachmentFileAsExcel(model, filePath);
                return filePath;
            }
            catch (Exception ex)
            {
                Log4Net.Error("Exception Message: " + ex.StackTrace);
                return string.Empty;
            }
        }

        static string GetFolderPath()
        {
            try
            {
                string path = Convert.ToString(ConfigurationManager.AppSettings["ReportPath"]);
                path = string.Concat(path, @"KRAReports\");

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                return path;
            }
            catch (Exception ex)
            {
                Log4Net.Error("Exception Message: " + ex.StackTrace);
                return string.Empty;
            }
        }

        private static bool GenerateAttachmentFileAsExcel(List<Rpt_SearchAllKRADetail_Result> model, string fileName)
        {
            try
            {
                using (SpreadsheetDocument document = SpreadsheetDocument.Create(fileName, SpreadsheetDocumentType.Workbook))
                {
                    var objPhoenixEntity = new PhoenixEntities();
                    var workbookPart = document.AddWorkbookPart();
                    workbookPart.Workbook = new Workbook();
                    var sheets = workbookPart.Workbook.AppendChild<Sheets>(new Sheets()); //typical line. need to write outside of inner scope.
                    UInt32Value sheetCount = 1;

                    if (model != null && model.Count > 0)
                    {
                        string[] dataColumns = GetDataColumnNames();
                        var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                        var sheetData = new SheetData();
                        worksheetPart.Worksheet = new Worksheet(sheetData);
                        var sheet = new Sheet() { Id = workbookPart.GetIdOfPart(worksheetPart), SheetId = sheetCount, Name = "KRASummary" };
                        sheets.Append(sheet);
                        #region //adding columns to sheet

                        worksheetPart = AddColumnsToSheet(worksheetPart, dataColumns);

                        #endregion //end columns to sheet
                        #region //Add Header Row

                        Row headerRow = GenerateHeaderRow(dataColumns);
                        sheetData.AppendChild(headerRow);

                        #endregion //Add Header Row
                        #region //adding data to sheet

                        sheetData = GenerateDataRows(sheetData, model);

                        #endregion //adding data to sheet
                        sheetCount++;
                    }
                    workbookPart.Workbook.Save();
                    document.Dispose();
                }
            }
            catch (Exception ex)
            {
                Log4Net.Error("Exception in GenerateAttachmentFileAsExcel Method Exception Message: " + ex.StackTrace);
                throw;
            }

            return true;
        }

        private static SheetData GenerateDataRows(SheetData sheetData, List<Rpt_SearchAllKRADetail_Result> model)
        {
            try
            {
                foreach (var item in model)
                {
                    var newRow = new Row();
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(Convert.ToString(item.KRACategoryId)) ? string.Empty : Convert.ToString(item.KRACategoryId)));
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(Convert.ToString(item.PersonId)) ? string.Empty : Convert.ToString(item.PersonId)));
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(item.EmployeeName) ? string.Empty : item.EmployeeName));
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(item.KRA) ? string.Empty : item.KRA));
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(item.Description) ? string.Empty : item.Description));
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(Convert.ToString(item.YearId)) ? string.Empty : Convert.ToString(item.YearId)));
                  //  newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(Convert.ToString(item.Weightage)) ? string.Empty : Convert.ToString(item.Weightage)));
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(Convert.ToString(item.IsValid)) ? string.Empty : Convert.ToString(item.IsValid)));
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(Convert.ToString(item.Q1)) ? string.Empty : Convert.ToString(item.IsValid)));
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(Convert.ToString(item.Q2)) ? string.Empty : Convert.ToString(item.Q2)));
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(Convert.ToString(item.Q3)) ? string.Empty : Convert.ToString(item.Q3)));
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(Convert.ToString(item.Q4)) ? string.Empty : Convert.ToString(item.Q4)));
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(Convert.ToString(item.IsCloned)) ? string.Empty : Convert.ToString(item.IsCloned)));
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(Convert.ToString(item.IsKRADone)) ? string.Empty : Convert.ToString(item.IsKRADone)));
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(Convert.ToString(item.KRADoneOn)) ? string.Empty : Convert.ToString(item.KRADoneOn)));
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(Convert.ToString(item.KRAPercentageCompletion)) ? string.Empty : Convert.ToString(item.KRAPercentageCompletion)));
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(Convert.ToString(item.KRAStartDate)) ? string.Empty : Convert.ToString(item.KRAStartDate)));
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(Convert.ToString(item.KRAEndDate)) ? string.Empty : Convert.ToString(item.KRAEndDate)));
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(Convert.ToString(item.CreatedBY)) ? string.Empty : Convert.ToString(item.CreatedBY)));
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(Convert.ToString(item.ModifiedBy)) ? string.Empty : Convert.ToString(item.ModifiedBy)));
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(Convert.ToString(item.ReviewerPersonId)) ? string.Empty : Convert.ToString(item.ReviewerPersonId)));
                    newRow.AppendChild(GetGeneratedCell(string.IsNullOrEmpty(Convert.ToString(item.ReviewerName)) ? string.Empty : Convert.ToString(item.ReviewerName)));

                    sheetData.AppendChild(newRow);
                }
            }
            catch (Exception ex)
            {
                Log4Net.Error("Exception in the method GenerateDataRows Exception Message: " + ex.StackTrace);
                throw;
            }
            return sheetData;
        }

        private static Cell GetGeneratedCell(string cellValue)
        {
            var objCell = new Cell();
            objCell.DataType = CellValues.String;
            objCell.CellValue = new CellValue(cellValue);
            return objCell;
        }

        private static Row GenerateHeaderRow(string[] dataColumns)
        {
            var headerRow = new Row();
            //adding column
            var columns = new List<string>();

            for (int n = 0; n < dataColumns.Length; n++)
            {
                columns.Add(dataColumns[n]);
                var cell = new Cell();
                cell.DataType = CellValues.InlineString;

                //execute run for bold
                var run1 = new Run();
                run1.Append(new Text(dataColumns[n]));
                var run1Properties = new RunProperties();
                run1Properties.Append(new Bold());
                run1.RunProperties = run1Properties;
                //complete run
                //create a new inline string and append it
                var instr = new InlineString();
                instr.Append(run1);
                cell.Append(instr);
                headerRow.AppendChild(cell);
            }
            return headerRow;
        }

        private static WorksheetPart AddColumnsToSheet(WorksheetPart worksheetPart, string[] dataColumns)
        {
            Columns lstColumns = worksheetPart.Worksheet.GetFirstChild<Columns>();
            Boolean needToInsertColumns = false;
            if (lstColumns == null)
            {
                lstColumns = new Columns();
                needToInsertColumns = true;
            }
            for (UInt32Value i = 1; i <= dataColumns.Length; i++)
            {
                lstColumns.Append(new Column() { Min = i, Max = i, Width = 10, BestFit = true, CustomWidth = true });
            }
            if (needToInsertColumns)
                worksheetPart.Worksheet.InsertAt(lstColumns, 0);

            return worksheetPart;
        }

        private static string[] GetDataColumnNames()
        {
            string[] objColumnNames = new string[] {
                                                       "KRA Category ID",
                                                       "Person ID",
                                                       "Employee Name",
                                                       "KRA",
                                                       "Description",
                                                       "Year ID",
                                                       "Is Valid",
                                                       "Q1",
                                                       "Q2",
                                                       "Q3",
                                                       "Q4",
                                                       "Is Cloned",
                                                       "Is KRA Done",
                                                       "KRA Done On",
                                                       "KRA Percentage Completion",
                                                       "KRA Start Date",
                                                       "KRA End Date",
                                                       "Created By",
                                                       "Modified By",
                                                       "Reviewer Person ID",
                                                       "Reviewer Name"

            };
            return objColumnNames;
        }

        public async Task<IEnumerable<GetKRAHistoryDetails_Result>> GetHistoryDetailsForClone(int id, int personId)
        {
            try
            {
                var personKRAHistoryList = new List<GetKRAHistoryDetails_Result>();
                try
                {
                    using (PhoenixEntities context = new PhoenixEntities())
                    {
                        var personKRAHistoryDetailList = context.GetKRAHistoryDetails(personId, id);
                        personKRAHistoryList.AddRange(personKRAHistoryDetailList);
                    }

                    return await Task.FromResult(personKRAHistoryList).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Log4Net.Error("Error occured in the GetHistoryDetailsForClone() method - InnerException :" + ex.InnerException + "Message :" + ex.Message + "StackTrace :" + ex.StackTrace);
                    throw ex;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<UpdateKRAHistory_Result> UpdateKRAHistoryDetails(int userId, PersonKRAUpdateHistoryViewModel viewModel)
        {
            UpdateKRAHistory_Result historyResult = new UpdateKRAHistory_Result();
            if (viewModel == null)
            {
                historyResult.Status = false;
                historyResult.Message = "Input viewModel is null.";               
                return historyResult;
            }

            using (PhoenixEntities context = new PhoenixEntities())
            {
                try
                {
                    foreach (var kraHistory in viewModel.PersonKRAHistorys)
                    {
                        var result = context.UpdateKRAHistory(userId, kraHistory.Id, viewModel.PersonId, viewModel.IsValid, viewModel.Comments, viewModel.IsKRADone, viewModel.KRADoneOn, viewModel.KRAPercentageCompletion, viewModel.KRAGoalId).FirstOrDefault();

                        if (result == null || !(result.Status ?? false))
                        {
                            historyResult.Status = false;
                            historyResult.Message = result?.Message ?? "Failed to update one of the KRA history records.";
                            return await Task.FromResult(historyResult).ConfigureAwait(false);
                        }
                    }

                    historyResult.Status = true;
                    historyResult.Message = "KRA History Updated Successfully!";
                    return await Task.FromResult(historyResult).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Log4Net.Error($"Error while updating KRA History details: {ex.Message}");
                    historyResult.Status = false;
                    historyResult.Message = ex.Message;
                    return historyResult;
                }
            }
        }

        public async Task<bool> SaveKRAAttachment(KRAFileAttachment fileAttachment, int loggedInUserId)
        {
            using (PhoenixEntities context = new PhoenixEntities())
            {
                try
                {
                    foreach (var attachment in fileAttachment.KRAAttachments)
                    {
                        KRAFileUpload fileUpload = new KRAFileUpload
                        {
                            FileName = fileAttachment.FileName,
                            FileUrl = fileAttachment.FileURL,
                            FileUploadedBy = loggedInUserId,
                            FileUploadedOn = DateTime.Now,
                            IsDeleted = false,
                            KRAId = attachment.KRAId
                        };

                        if (fileAttachment.KRAGoalId > 0)
                        {
                            fileUpload.KRAGoalId = fileAttachment.KRAGoalId;
                        }

                        context.KRAFileUpload.Add(fileUpload);
                    }

                    context.SaveChanges();
                    return await Task.FromResult(true).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Log4Net.Error($"Error while uploading KRA Attachment: Message: {ex.Message} ' Inner Exception : ' {ex.InnerException} ");
                    return false;
                }
            }
        }

        public async Task<bool> DeleteKRAAttachment(int Id, int KRAId, int UserId, int KRAGoalId)
        {
            try
            {
                using (PhoenixEntities context = new PhoenixEntities())
                {
                    var attachmentdata = _service.All<KRAFileUpload>().Where(x => x.KRAId == KRAId && x.Id == Id && x.KRAGoalId == KRAGoalId).FirstOrDefault();

                    if (attachmentdata != null)
                    {
                        attachmentdata.IsDeleted = true;
                        attachmentdata.FileDeletedBy = UserId;
                        attachmentdata.FileDeletedOn = DateTime.Now;
                        attachmentdata.FileUploadedBy = UserId;

                        _service.Update(attachmentdata);
                        _service.Finalize(true);
                        return await Task.FromResult(true).ConfigureAwait(false);
                    }
                    else
                    {
                        Log4Net.Error("No KRA attachment file found for KRAId " + KRAId + "and KRAGoalId: " + KRAGoalId + " for delete");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Log4Net.Error($"Error while deleting the file: {ex.Message}");
                return false;
            }
        }

        public async Task<IEnumerable<GetKRAFeedbackForm_Result>> GetReviewerFeedbackDetails(int kraGoalId, int personId)
        {
            using (PhoenixEntities context = new PhoenixEntities())
            {
                try
                {
                    var reviewerFeedback = context.GetKRAFeedbackForm(kraGoalId, personId).ToList();
                    return reviewerFeedback;
                }
                catch (Exception ex)
                {
                    Log4Net.Error("Error occured in the GetReviewerFeedbackDetails() method - InnerException :" + ex.InnerException + "Message :" + ex.Message + "StackTrace :" + ex.StackTrace);
                    throw ex;
                }
            }
        }
        public async Task<bool> AddUpdateReviewerFeedbackDetails(KRAFeedbackViewModel viewModel)
        {
            using (PhoenixEntities context = new PhoenixEntities())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        if (viewModel != null)
                        {
                            var _mappedData = Mapper.Map<KRAFeedbackViewModel, PersonKRAFeedback>(viewModel);
                            context.AddOrUpdateKRAFeedbackForm(_mappedData.ID, _mappedData.KRAGoalId, _mappedData.Year, _mappedData.Quarter, _mappedData.PersonId, _mappedData.Feedback, _mappedData.CreatedBy, _mappedData.UpdatedBy, _mappedData.CompletionDate);
                            context.SaveChanges();
                            transaction.Commit();
                        }
                        return await Task.FromResult(true).ConfigureAwait(false);
                    }
                    catch (DbUpdateException ex)
                    {
                        transaction.Rollback();
                        Log4Net.Error("Error occured in the AddUpdateReviewerFeedbackDetails() method - InnerException :" + ex.InnerException + "Message :" + ex.Message + "StackTrace :" + ex.StackTrace);
                        throw ex;
                    }
                }
            }
        }

        public async Task<IEnumerable<KRALogViewModel>> GetLogs(int personId, string yearList)
        {
            using (PhoenixEntities context = new PhoenixEntities())
            {
                try
                {
                    List<KRALogViewModel> kraLogList = new List<KRALogViewModel>();
                    var kraLog = context.GetKRALogs(personId, yearList).ToList();
                    foreach (var klogs in kraLog)
                    {
                        var kraLogRecord = new KRALogViewModel()
                        {
                            Id = klogs.Id,
                            TableName = klogs.TableName,
                            ColumnName = klogs.ColumnName,
                            Table_PK_Id = klogs.Table_PK_Id,
                            OldValue = klogs.OldValue,
                            NewValue = klogs.NewValue,
                            ModifiedById = klogs.ModifiedById,
                            ModifiedByName = klogs.ModifiedByName,
                            ModifiedDate = (klogs.ModifiedDate).ToString()
                        };
                        kraLogList.Add(kraLogRecord);
                    }
                    return await Task.FromResult(kraLogList).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Log4Net.Error("Error occured in the GetLogs() method - InnerException :" + ex.InnerException + "Message :" + ex.Message + "StackTrace :" + ex.StackTrace);
                    throw ex;
                }
            }
        }

        public async Task<bool> DeleteFeedback(KRAFeedbackDeleteViewModel model)
        {
            bool isDeleted = false;
            using (PhoenixEntities context = new PhoenixEntities())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        int id = model.Id;
                        int kRAGoalId = model.KraGoalId;
                        int updatedBy = model.UpdatedBy;
                        context.DeleteKRAFeedback(id, kRAGoalId, updatedBy);
                        await context.SaveChangesAsync().ConfigureAwait(false);
                        isDeleted = true;
                        transaction.Commit();
                        return await Task.FromResult(isDeleted).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw ex;
                    }
                }
            }
        }
    }

    public class KRAData
    {
        public DateTime KRAStartDate { get; set; }
        public DateTime KRAEndDate { get; set; }
    }  
}