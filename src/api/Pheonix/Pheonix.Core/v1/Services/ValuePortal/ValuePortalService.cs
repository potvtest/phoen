using AutoMapper;
using Pheonix.Core.v1.Services.Business;
using Pheonix.Core.v1.Services.Email;
using Pheonix.Models.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pheonix.DBContext.Repository;
using ValuePortal;
using Pheonix.Models.VM;
using Pheonix.DBContext;
//using Pheonix.DBContext;
//using Pheonix.DBContext.Repository.ValuePortal;

namespace Pheonix.Core.v1.Services.ValuePortal// ValuePortal.Services //
{
    public class ValuePortalService : IValuePortalService
    {
        private IMainOperationsService service;
        private IEmployeeService _employeeService;
        private IEmailService emailService;
        private IBasicOperationsService _basicService;
        public ValuePortalService(IVPContextRepository repository, IMainOperationsService opsService, IEmployeeService employeeService, IEmailService opsEmailService, IBasicOperationsService basicService)
        {
            service = opsService;
            _employeeService = employeeService;
            emailService = opsEmailService;
            _basicService = basicService;
        }
        public ValuePortalService(IVPContextRepository repository, IMainOperationsService opsService)
        {
            service = opsService;
        }

        public async Task<IEnumerable<VCIdeaMasterViewModel>> GetIdeaList(VPApproverFlowHandlerModel approverFlowPayload)
        {
            return await Task.Run(async () =>
            {
                List<VPIdeaDetail> ideaList = null;
                if (approverFlowPayload != null)
                {
                    int userId = approverFlowPayload.userId;
                    //Commented on 8th March 22 after Sync-up call as per disscussion with Amit Sharma
                    //bool success = await LockUnlockTheIdea(0, userId);
                    ideaList = new List<VPIdeaDetail>();
                    if (approverFlowPayload.isGlobalApprover || approverFlowPayload.isVCFAdmin)
                    {
                        ideaList = executeQueryForGloabalOrAdminUser(userId, approverFlowPayload);
                    }
                    else if (approverFlowPayload.statusList != null)
                    {
                        ideaList = executeQueryForGeneralUser(userId, approverFlowPayload);
                    }
                }
                return setFinalScore(ideaList);
            });
        }

        public async Task<List<LimitedDataIdeaDetailsViewModel>> getLimitedDataIdealist(VPApproverFlowHandlerModel approverFlowPayload)
        {
            return await Task.Run(async () =>
            {
                List<VPIdeaDetail> ideaList = null;
                if (approverFlowPayload != null)
                {
                    int userId = approverFlowPayload.userId;
                    ideaList = new List<VPIdeaDetail>();
                    if (approverFlowPayload.isOtherUserData)
                    {
                        ideaList = getLimitedDataForGeneralUser(userId, approverFlowPayload);
                    }
                }
                return getMappedLimitedIdeasDataModel(ideaList);
            });
        }

        public async Task<IEnumerable<VCIdeaMasterViewModel>> GetIdeaListForBUApprover(long userId, int status, VPBUApproverHandlerModel buApproverPayload)
        {
            return await Task.Run(async () =>
            {
                List<VPIdeaDetail> ideaList = new List<VPIdeaDetail>();
                if (buApproverPayload != null)
                {
                    // Commented on 8th March 22 after Sync-up call as per disscussion with Amit Sharma
                    // bool success = await LockUnlockTheIdea(0, buApproverPayload.userId);
                    //copyOfBUList, buApproverPayload.isSelfIdea, buApproverPayload.isBUFilterOn, buApproverPayload.isSearchByEmp
                   // List<long> bussinessUnitList = buApproverPayload.assignedBUList;
                    //List<long> copyOfBUList = buApproverPayload.copyOfAssignedBUList;
                    ideaList = executeQueryForBUApprover(userId, status, buApproverPayload);
                }
                return setFinalScore(ideaList);
            });
        }

        public List<VPIdeaDetail> executeQueryForGeneralUser(long userId, VPApproverFlowHandlerModel approverFlowPayload)
        {
            List<long> statusList = approverFlowPayload.statusList;
            List<long> filterBUList = approverFlowPayload.assignedBUList;
            bool isBUFilterOn = approverFlowPayload.isBUFilterOn;
            bool isByEmpFilterOn = approverFlowPayload.isSearchByEmp;
            string queryEmpNameOrIds = approverFlowPayload.queryEmpNameOrIds;
            string[] queryEmpIds = null;
            int[] intEmpIds = null;
            if (!String.IsNullOrEmpty(queryEmpNameOrIds))
            {
                queryEmpIds = approverFlowPayload.queryEmpNameOrIds.Split(',');
                intEmpIds = Array.ConvertAll(queryEmpIds, s => int.Parse(s));
            }
            List<VPIdeaDetail> ideaList = new List<VPIdeaDetail>();
            if (statusList != null && statusList.Count() > 0)
            {
                if (isBUFilterOn)
                {
                    if (isByEmpFilterOn)
                    {
                        ideaList = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && statusList.Contains(v.StatusID) && filterBUList.Contains(v.BusinessUnit) && (intEmpIds.Contains(v.SubmittedBy) || v.TeammemberIds.Contains(queryEmpNameOrIds))).OrderByDescending(v => v.CreatedDate).ToList();
                    }
                    else
                    {
                        ideaList = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && statusList.Contains(v.StatusID) && filterBUList.Contains(v.BusinessUnit) && (v.SubmittedBy == userId || v.TeammemberIds.Contains(userId.ToString()))).OrderByDescending(v => v.CreatedDate).ToList();
                    }
                }
                else
                {
                     if (isByEmpFilterOn)
                    {
                        ideaList = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && statusList.Contains(v.StatusID) && (intEmpIds.Contains(v.SubmittedBy) || v.TeammemberIds.Contains(queryEmpNameOrIds))).OrderByDescending(v => v.CreatedDate).ToList();
                    }
                    else
                    {
                        ideaList = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && statusList.Contains(v.StatusID) && (v.SubmittedBy == userId || v.TeammemberIds.Contains(userId.ToString()))).OrderByDescending(v => v.CreatedDate).ToList();
                    }
                }

            }
            return ideaList;
        }

        public List<VPIdeaDetail> getLimitedDataForGeneralUser(long userId, VPApproverFlowHandlerModel approverFlowPayload)
        {
            List<long> statusList = approverFlowPayload.statusList;
            bool isByEmpFilterOn = approverFlowPayload.isSearchByEmp;
            string queryEmpNameOrIds = approverFlowPayload.queryEmpNameOrIds;
            string[] queryEmpIds = null;
            int[] intEmpIds = null;
            if (!String.IsNullOrEmpty(queryEmpNameOrIds))
            {
                queryEmpIds = approverFlowPayload.queryEmpNameOrIds.Split(',');
                intEmpIds = Array.ConvertAll(queryEmpIds, s => int.Parse(s));
            }
            List<VPIdeaDetail> ideaList = new List<VPIdeaDetail>();
            if (statusList != null && statusList.Count() > 0)
            {
                if (isByEmpFilterOn)
                {
                    ideaList = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && statusList.Contains(v.StatusID) && (intEmpIds.Contains(v.SubmittedBy) || v.TeammemberIds.Contains(queryEmpNameOrIds))).OrderByDescending(v => v.CreatedDate).ToList();
                }
                else
                {
                    ideaList = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && statusList.Contains(v.StatusID)).OrderByDescending(v => v.CreatedDate).ToList();
                }
            }
            return ideaList;
        }

        public List<VPIdeaDetail> executeQueryForBUApprover(long userId, int status, VPBUApproverHandlerModel buApproverPayload)
        {
            bool isSelfIdea = buApproverPayload.isSelfIdea;
            bool isBUFilterOn = buApproverPayload.isBUFilterOn;
            bool isByEmpFilterOn = buApproverPayload.isSearchByEmp;
            List<long> statusList = buApproverPayload.statusList;
            List<long> bussinessUnitList = buApproverPayload.assignedBUList;
            List<long> originalBUList = buApproverPayload.copyOfAssignedBUList;
            string queryEmpNameOrIds = buApproverPayload.queryEmpNameOrIds;
            string[] queryEmpIds = null;
            int[] intEmpIds = null;
            if (!String.IsNullOrEmpty(queryEmpNameOrIds))
            {
                queryEmpIds = buApproverPayload.queryEmpNameOrIds.Split(',');
                intEmpIds = Array.ConvertAll(queryEmpIds, s => int.Parse(s));
            }
            List<VPIdeaDetail> ideaList = null;
            if (bussinessUnitList != null && bussinessUnitList.Count > 0 && originalBUList != null && originalBUList.Count() > 0)
            {
                ideaList = new List<VPIdeaDetail>();
                if (status == 0)
                {
                    if (isSelfIdea)
                    {
                        if (isBUFilterOn)
                        {
                            ideaList = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && statusList.Contains(v.StatusID) && bussinessUnitList.Contains(v.BusinessUnit) && (v.SubmittedBy == userId || v.TeammemberIds.Contains(userId.ToString()))).OrderByDescending(v => v.CreatedDate).ToList();
                        }
                        else
                        {
                            ideaList = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && statusList.Contains(v.StatusID) && (v.SubmittedBy == userId || v.TeammemberIds.Contains(userId.ToString()))).OrderByDescending(v => v.CreatedDate).ToList();
                        }
                    }
                    else
                    {
                        if (isBUFilterOn)
                        {
                            if (isByEmpFilterOn)
                            {
                                ideaList = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && statusList.Contains(v.StatusID) && bussinessUnitList.Contains(v.BusinessUnit) && (intEmpIds.Contains(v.SubmittedBy) || v.TeammemberIds.Contains(queryEmpNameOrIds))).OrderByDescending(v => v.CreatedDate).ToList();
                            }
                            else //if (originalBUList.Contains(bussinessUnitList.ElementAt(0)))
                            {
                                ideaList = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && statusList.Contains(v.StatusID) && bussinessUnitList.Contains(v.BusinessUnit)).OrderByDescending(v => v.CreatedDate).ToList();
                            }
                        }
                        else
                        {
                            if (isByEmpFilterOn)
                            {
                                ideaList = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && statusList.Contains(v.StatusID) && (intEmpIds.Contains(v.SubmittedBy) || v.TeammemberIds.Contains(queryEmpNameOrIds))).OrderByDescending(v => v.CreatedDate).ToList();
                            }
                            else
                            {
                                // optimized logic ( fetch overall ideas for BU apporver self + other user ideas)
                                // Earlier code condition prior to 2nd May //&& (bussinessUnitList.Contains(v.BusinessUnit.Value) || (v.SubmittedBy == userId || v.TeammemberIds.Contains(userId.ToString())))
                                ideaList = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && statusList.Contains(v.StatusID)).OrderByDescending(v => v.CreatedDate).ToList();
                            }
                        }
                    }
                }
                else if (status > 0)
                {
                    if (isSelfIdea)
                    {
                        if (isBUFilterOn)
                        {
                            ideaList = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && statusList.Contains(v.StatusID) && bussinessUnitList.Contains(v.BusinessUnit) && (v.SubmittedBy == userId || v.TeammemberIds.Contains(userId.ToString()))).OrderByDescending(v => v.CreatedDate).ToList();
                        }
                        else
                        {
                            ideaList = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && statusList.Contains(v.StatusID) && (v.SubmittedBy == userId || v.TeammemberIds.Contains(userId.ToString()))).OrderByDescending(v => v.CreatedDate).ToList();
                        }
                    }
                    else
                    {
                        if (isBUFilterOn)
                        {
                            if (isByEmpFilterOn)
                            {
                                ideaList = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && statusList.Contains(v.StatusID) && bussinessUnitList.Contains(v.BusinessUnit) && (intEmpIds.Contains(v.SubmittedBy) || v.TeammemberIds.Contains(queryEmpNameOrIds))).OrderByDescending(v => v.CreatedDate).ToList();
                            }
                            else
                            {
                                ideaList = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && statusList.Contains(v.StatusID) && bussinessUnitList.Contains(v.BusinessUnit)).OrderByDescending(v => v.CreatedDate).ToList();
                            }
                        }
                        else
                        {
                            if (isByEmpFilterOn)
                            {
                                ideaList = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && statusList.Contains(v.StatusID) && (bussinessUnitList.Contains(v.BusinessUnit) && (intEmpIds.Contains(v.SubmittedBy) || v.TeammemberIds.Contains(queryEmpNameOrIds)))).OrderByDescending(v => v.CreatedDate).ToList();
                            }
                            else
                            {
                                ideaList = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && statusList.Contains(v.StatusID)).OrderByDescending(v => v.CreatedDate).ToList();
                            }
                        }

                    }
                }

            }
            return ideaList;
        }

        public List<VPIdeaDetail> executeQueryForGloabalOrAdminUser(long userId, VPApproverFlowHandlerModel approverFlowPayload)
        {
            List<long> statusList = approverFlowPayload.statusList;
            bool isSelfIdea = approverFlowPayload.isSelfIdea;
            bool isBUFilterOn = approverFlowPayload.isBUFilterOn;
            List<long> filterBUList = approverFlowPayload.assignedBUList;
            bool isByEmpFilterOn = approverFlowPayload.isSearchByEmp;
            string queryEmpNameOrIds = approverFlowPayload.queryEmpNameOrIds;
            string[] queryEmpIds = null;
            int[] intEmpIds = null;
            if (!String.IsNullOrEmpty(queryEmpNameOrIds))
            {
                queryEmpIds = approverFlowPayload.queryEmpNameOrIds.Split(',');
                intEmpIds = Array.ConvertAll(queryEmpIds, s => int.Parse(s));
            }
            List<VPIdeaDetail> ideaList = new List<VPIdeaDetail>();
            if (statusList != null && statusList.Count() > 0)
            {
                if (isSelfIdea)
                {
                    if (isBUFilterOn)
                    {
                        ideaList = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && statusList.Contains(v.StatusID) && filterBUList.Contains(v.BusinessUnit)
                        && (v.SubmittedBy == userId || v.TeammemberIds.Contains(userId.ToString()))).OrderByDescending(v => v.CreatedDate).ToList();
                    }
                    else
                    {
                        ideaList = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && statusList.Contains(v.StatusID) && (v.SubmittedBy == userId || v.TeammemberIds.Contains(userId.ToString()))).OrderByDescending(v => v.CreatedDate).ToList();
                    }
                }
                else
                {
                    if (isBUFilterOn)
                    {
                        if (isByEmpFilterOn)
                        {
                            ideaList = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && statusList.Contains(v.StatusID) && filterBUList.Contains(v.BusinessUnit) && (intEmpIds.Contains(v.SubmittedBy) || v.TeammemberIds.Contains(queryEmpNameOrIds))).OrderByDescending(v => v.CreatedDate).ToList();
                        }
                        else
                        {
                            ideaList = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && statusList.Contains(v.StatusID) && filterBUList.Contains(v.BusinessUnit)).OrderByDescending(v => v.CreatedDate).ToList();
                        }
                    }
                    else
                    {
                        if (isByEmpFilterOn)
                        {
                            ideaList = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && statusList.Contains(v.StatusID) && (intEmpIds.Contains(v.SubmittedBy) || v.TeammemberIds.Contains(queryEmpNameOrIds))).OrderByDescending(v => v.CreatedDate).ToList();
                        }
                        else
                        {
                            ideaList = service.Top<VPIdeaDetail>(20, v => v.IsDeleted == false && statusList.Contains(v.StatusID)).OrderByDescending(v => v.CreatedDate).ToList();
                        }
                    }

                }
            }
            return ideaList;
        }
        public List<VCIdeaMasterViewModel> setFinalScore(List<VPIdeaDetail> ideaList)
        {

            List<VCIdeaMasterViewModel> ideaListViewModel = Mapper.Map<IEnumerable<VPIdeaDetail>, IEnumerable<VCIdeaMasterViewModel>>(ideaList).ToList();

            foreach (var idea in ideaListViewModel)
            {
                //idea.SubmittedByDetails = await _employeeService.GetProfile(idea.SubmittedBy, false);
                idea.SubmittedByName = _employeeService.GetCommaSeparatedEmployeeNames(!string.IsNullOrEmpty(idea.TeammemberIds) ? idea.TeammemberIds +","+ idea.SubmittedBy.ToString() : idea.SubmittedBy.ToString());
                foreach (var i in ideaList)
                {
                    if (i.ID == idea.ID)
                    {
                        var p = i.VPReviewerDetails;
                        foreach (var ii in p)
                        {
                            var Final = p.OfType<dynamic>().FirstOrDefault();
                            if (Final != null)
                            {
                                idea.Cost = Final.Cost;
                                idea.FinalScore = Convert.ToInt16(((VPReviewerDetail)Final).FinalScore);
                            }
                            goto outer;
                        }
                    }
                }
            outer:;
            }
            return ideaListViewModel;
        }
         
        public List<LimitedDataIdeaDetailsViewModel> getMappedLimitedIdeasDataModel(List<VPIdeaDetail> ideaList)
        {
            List<LimitedDataIdeaDetailsViewModel> limitedIdeaVMList = null;
            try
            {
                limitedIdeaVMList = Mapper.Map<IEnumerable<VPIdeaDetail>, IEnumerable<LimitedDataIdeaDetailsViewModel>>(ideaList).ToList();
                foreach (var limitedDataIdea in limitedIdeaVMList)
                {
                    limitedDataIdea.SubmittedByName = _employeeService.GetCommaSeparatedEmployeeNames(!string.IsNullOrEmpty(limitedDataIdea.TeammemberIds) ? limitedDataIdea.TeammemberIds + "," + limitedDataIdea.SubmittedBy.ToString() : limitedDataIdea.SubmittedBy.ToString());
                }
            }
            catch(Exception ex)
            {
               throw ex;
            }
            return limitedIdeaVMList;
        }

        public async Task<VCIdeaMasterViewModel> GetIdea(long id, long loginuserId)
        {
            return await Task.Run(async () =>
            {
                // Commented on 8th March 22 after Sync-up call as per disscussion with Amit Sharma
                // bool success = await LockUnlockTheIdea((int)id, (int)loginuserId);
                var idea = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && v.ID == id).FirstOrDefault();
                var ReviewData = service.Top<VPReviewerDetail>(0, v => v.VPIdeaDetailID == id).FirstOrDefault();
                var ideaViewModel = Mapper.Map<VPIdeaDetail, VCIdeaMasterViewModel>(idea);
                if (ReviewData != null)
                {
                    ideaViewModel.Cost = Convert.ToDouble(((VPReviewerDetail)ReviewData).Cost);
                    ideaViewModel.FinalScore = Convert.ToInt16(((VPReviewerDetail)ReviewData).FinalScore);
                    //ideaViewModel.ReviewerComment1 = ((VPReviewerDetail)ReviewData).ReviewerComments;
                    //ideaViewModel.ReviewerComment2 = ((VPReviewerDetail)ReviewData).ReviewerComments2;
                    //ideaViewModel.ReviewerComment3 = ((VPReviewerDetail)ReviewData).ReviewerComments3;
                    ideaViewModel.Benefit = Convert.ToDouble(((VPReviewerDetail)ReviewData).BenefitScore);
                }

                ideaViewModel.SubmittedByDetails = await _employeeService.GetProfile(idea.SubmittedBy, false);
                foreach (var comment in ideaViewModel.VPComments)
                {
                   comment.SubmittedByDetails = await _employeeService.GetProfile(comment.ReviewerId, false);
                }
                ideaViewModel.TeammemberNames = _employeeService.GetCommaSeparatedEmployeeNames(!string.IsNullOrEmpty(idea.TeammemberIds) ? idea.TeammemberIds +"," +idea.SubmittedBy.ToString() : idea.SubmittedBy.ToString());
                return ideaViewModel;

            });
        }

        public async Task<LimitedDataIdeaDetailsViewModel> getLimitedIdeaDetailsData(long id, long loginuserId)
        {
            return await Task.Run(async () =>
            {
                var idea = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && v.ID == id).FirstOrDefault();
                var limitedIdeaViewModel = Mapper.Map<VPIdeaDetail, LimitedDataIdeaDetailsViewModel>(idea);
                limitedIdeaViewModel.SubmittedByDetails = await _employeeService.GetProfile(idea.SubmittedBy, false);
                foreach (var comment in limitedIdeaViewModel.VPComments)
                {
                    comment.SubmittedByDetails = await _employeeService.GetProfile(comment.ReviewerId, false);
                }
                limitedIdeaViewModel.TeammemberNames = _employeeService.GetCommaSeparatedEmployeeNames(!string.IsNullOrEmpty(idea.TeammemberIds) ? idea.TeammemberIds + "," + idea.SubmittedBy.ToString() : idea.SubmittedBy.ToString());
                return limitedIdeaViewModel;
            });
        }

        private string GetTeamList(string teamIds)
        {
            string teamList = string.Empty;
            var empCode = teamIds.Split(',');
            foreach (string item in empCode)
            {
                var name = _employeeService.GetEmployeeName(int.Parse(item));
                teamList = string.IsNullOrEmpty(teamList) ? name : teamList + ", " + name;

            }
            return teamList;
        }

        public async Task<VPAllMastersdataViewModel> GetMastersdata()
        {

            VPAllMastersdataViewModel All = new VPAllMastersdataViewModel();

            var priorityList = service.Top<VPPriority>(0, v => v.IsActive == true).OrderBy(v => v.ID).ToList();
            All.PriorityCollection = Mapper.Map<IEnumerable<VPPriority>, IEnumerable<VPPriorityViewModel>>(priorityList).ToList();

            var StatusList = service.Top<VPStatu>(0, v => v.IsActive == true).OrderBy(v => v.ID).ToList();
            All.StatusCollection = Mapper.Map<IEnumerable<VPStatu>, IEnumerable<VPStatusViewModel>>(StatusList).ToList();

            var BenefitList = service.Top<VPBenefit>(0, v => v.IsActive == true).OrderBy(v => v.ID).ToList();
            All.BenefitCollection = Mapper.Map<IEnumerable<VPBenefit>, IEnumerable<VPBenefitViewModel>>(BenefitList).ToList();

            var CostList = service.Top<VPCost>(0, v => v.IsActive == true).OrderBy(v => v.ID).ToList();
            All.CostCollection = Mapper.Map<IEnumerable<VPCost>, IEnumerable<VPCostViewModel>>(CostList).ToList();
            
            var ScopeList = service.Top<VPBenefitScope>(0, v => v.IsActive == true).OrderBy(v => v.ID).ToList();
            All.ScopeCollection = Mapper.Map<IEnumerable<VPBenefitScope>, IEnumerable<VPBenefitScopeViewModel>>(ScopeList).ToList();
            return All;

        }

        public async Task<IEnumerable<VPConfigurationViewModel>> GetConfigurationdata()
        {
            return await Task.Run(async () =>
            {
                var priorityList = service.Top<VPConfiguration>(0, v => v.IsActive == true).OrderBy(v => v.Name).ToList();
                return Mapper.Map<IEnumerable<VPConfiguration>, IEnumerable<VPConfigurationViewModel>>(priorityList).ToList();
            });



        }

        //public async Task<IEnumerable<VPPriorityViewModel>> GetPriorityList()
        //{
        //    return await Task.Run(() =>
        //    {
        //        var priorityList = service.Top<VPPriority>(0, v => v.IsActive == true).OrderBy(v => v.Name).ToList();
        //       // return Mapper.Map<IEnumerable<VPPriority>, IEnumerable<VPPriorityViewModel>>(priorityList).ToList();


        //        //var ideaListViewModel = Mapper.Map<IEnumerable<VPPriority>, IEnumerable<VPPriorityViewModel>>(priorityList).ToList();


        //        //var StatusList = service.Top<VPStatu>(0, v1 => v1.IsActive == true).OrderBy(v1 => v1.Name).ToList();


        //        //VPPriorityViewModel v3 = new VPPriorityViewModel();
        //        //v3.StatusCollection = Mapper.Map<IEnumerable<VPStatu>, IEnumerable<VPStatusViewModel>>(StatusList).ToList();




        //        return Mapper.Map<IEnumerable<VPPriority>, IEnumerable<VPPriorityViewModel>>(priorityList).ToList();
        //    });
        //}
        public async Task<IEnumerable<VPStatusViewModel>> GetStatusList()
        {
            return await Task.Run(() =>
            {
                var priorityList = service.Top<VPStatu>(0, v => v.IsActive == true).OrderBy(v => v.Name).ToList();
                return Mapper.Map<IEnumerable<VPStatu>, IEnumerable<VPStatusViewModel>>(priorityList).ToList();
            });
        }
        public async Task<bool> SaveIdea(VCIdeaMasterViewModel ideaVM)//,int userId)
        {
            using (ValuePortalEntities context = new ValuePortalEntities())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        return await Task.Run(() =>
                        {
                            var idea = Mapper.Map<VCIdeaMasterViewModel, VPIdeaDetail>(ideaVM);
                            idea.SubmittedBy = ideaVM.SubmittedBy;
                            if (idea.ID == 0)
                            {
                                var isCreated = true;
                                idea.ID = 0;
                                idea.CreatedDate = DateTime.Now;
                                idea.LastUpdatedDate = DateTime.Now;
                                isCreated = service.Create<VPIdeaDetail>(idea, x => x.ID == 0);
                                var model = new VPReviewerDetail();
                                if (ideaVM != null)
                                {
                                    model.VPIdeaDetailID = ideaVM.ID;
                                    model.BenefitScore = ideaVM.Benefit;
                                    model.Cost = ideaVM.Cost;
                                    model.FinalScore = ideaVM.FinalScore;
                                    model.UpdatedBy = ideaVM.SubmittedBy;
                                }
                                model.CreatedDate = DateTime.Now;
                                model.LastUpdatedDate = DateTime.Now;
                                var pass = service.Create<VPReviewerDetail>(model, null);

                                service.Finalize(true);
                                transaction.Commit();
                            }
                            else if (idea.ID > 0)
                                {
                                var isUpdated = true;
                                var isUpdatedCost = true;
                                idea.LastUpdatedDate = DateTime.Now;
                                VPIdeaDetail editableIdeaObj = service.First<VPIdeaDetail>(v => v.IsDeleted == false && v.ID == ideaVM.ID);
                                if(editableIdeaObj != null && idea != null)
                                {
                                    editableIdeaObj.IdeaHeadline = idea.IdeaHeadline;
                                    editableIdeaObj.IdeaDescription = idea.IdeaDescription;
                                    editableIdeaObj.BusinessUnit = idea.BusinessUnit;
                                    editableIdeaObj.StatusID = idea.StatusID;
                                    editableIdeaObj.BenefitFactor = idea.BenefitFactor;
                                    editableIdeaObj.BenefitScope = idea.BenefitScope;
                                    editableIdeaObj.TeammemberIds = idea.TeammemberIds;
                                    editableIdeaObj.LastUpdatedDate = idea.LastUpdatedDate;
                                    if(editableIdeaObj.StatusID == 6 || editableIdeaObj.StatusID == 8)
                                    {
                                        editableIdeaObj.ExecutionApproach = idea.ExecutionApproach;
                                        editableIdeaObj.RequiredEffort = idea.RequiredEffort;
                                        editableIdeaObj.RequiredTechnologies = idea.RequiredTechnologies;
                                        editableIdeaObj.UniquenessQuotient = idea.UniquenessQuotient;

                                        VPReviewerDetail editableCostObj = service.First<VPReviewerDetail>(v => v.VPIdeaDetailID == ideaVM.ID);
                                        if (editableCostObj != null)
                                        {
                                            editableCostObj.Cost = ideaVM.Cost;
                                            editableCostObj.UpdatedBy = ideaVM.SubmittedBy;
                                            editableCostObj.LastUpdatedDate = DateTime.Now;
                                            isUpdatedCost = service.Update(editableCostObj);
                                        }
                                    }
                                    isUpdated = service.Update(editableIdeaObj);
                                }
                                //var ReviewerDetail = service.Top<VPReviewerDetail>(0, m => m.VPIdeaDetailID == ideaVM.ID).ToList();
                                //if (ReviewerDetail.Count == 0)
                                //{
                                //    var model = new VPReviewerDetail();
                                //    model.VPIdeaDetailID = ideaVM.ID;
                                //    model.BenefitScore = ideaVM.Benefit;
                                //    model.Cost = ideaVM.Cost;
                                //    model.FinalScore = ideaVM.FinalScore;
                                //    model.CreatedDate = DateTime.Now;
                                //    model.UpdatedBy = ideaVM.SubmittedBy;
                                //    model.LastUpdatedDate = DateTime.Now;
                                //    var pass = service.Create<VPReviewerDetail>(model, null);
                                //}
                                //else
                                //{
                                //    ReviewerDetail[0].Cost = ideaVM.Cost;
                                //    ReviewerDetail[0].BenefitScore = ideaVM.Benefit;
                                //    ReviewerDetail[0].FinalScore = ideaVM.FinalScore;
                                //    idea.VPReviewerDetails.Add(ReviewerDetail[0]);
                                //    isUpdated = service.Update(ReviewerDetail[0]);
                                //}

                                service.Finalize(true);
                                transaction.Commit();
                            }
                           
                            if (idea.StatusID == 1 || idea.StatusID == 8)
                            {
                                VPSubmittedIdeaViewModel vpSubmittedIdeaViewModel = new VPSubmittedIdeaViewModel();
                                var requester = _basicService.First<Person>(x => x.ID == ideaVM.SubmittedBy);
                                var requesterName = requester.FirstName + " " + requester.LastName;
                                var benefitScopeObj = service.First<VPBenefitScope>(v => v.IsActive == true && v.ID == ideaVM.BenefitScope);
                                if (benefitScopeObj != null)
                                {
                                    ideaVM.benefitScopeValue = benefitScopeObj.Name;
                                }
                                ideaVM.ID = idea.ID;
                                vpSubmittedIdeaViewModel.UserId = ideaVM.SubmittedBy;
                                vpSubmittedIdeaViewModel.IdeaHeadline = ideaVM.IdeaHeadline;
                                vpSubmittedIdeaViewModel.IdeaBrief = ideaVM.IdeaDescription;
                                vpSubmittedIdeaViewModel.IdeaBenefits = ideaVM.IdeaBenefits;
                                if (idea.StatusID == 8)
                                {
                                    vpSubmittedIdeaViewModel.ExecutionApproach = ideaVM.ExecutionApproach;
                                    vpSubmittedIdeaViewModel.EffortRequired = ideaVM.RequiredEffort;
                                    vpSubmittedIdeaViewModel.TechnologiesRequired = ideaVM.RequiredTechnologies;
                                    ideaVM.SubmittedByName = _employeeService.GetCommaSeparatedEmployeeNames(!string.IsNullOrEmpty(idea.TeammemberIds) ? idea.TeammemberIds + "," + idea.SubmittedBy.ToString() : idea.SubmittedBy.ToString());
                                    var ideaStatusObj = service.First<VPStatu>(v => v.IsActive == true && v.ID == ideaVM.StatusID);
                                    var costDBRow = service.First<VPCost>(x => x.IsActive.Value && x.ID == ideaVM.Cost);
                                    if (ideaStatusObj != null)
                                    {
                                        ideaVM.StatusName = ideaStatusObj.Name;
                                    }
                                    if (costDBRow != null)
                                    {
                                        vpSubmittedIdeaViewModel.CostDesc = costDBRow.Description;
                                    }
                                }

                                vpSubmittedIdeaViewModel.personOrganizationEmail = requester.PersonEmployment.First().OrganizationEmail;
                                vpSubmittedIdeaViewModel.approverOrganizationEmail = requester.PersonEmployment.First().OrganizationEmail;  // this should be changed to group email based on the department
                                vpSubmittedIdeaViewModel.submitterName = requesterName;
                                vpSubmittedIdeaViewModel.EmployeeId = requester.ID;

                                if (idea.StatusID == 1)
                                {
                                    emailService.sendEmailOnVCFPhaseISubmit(vpSubmittedIdeaViewModel,ideaVM);

                                }else if(idea.StatusID == 8)
                                {
                                    emailService.SendValuePortalIdeaSubmitted(vpSubmittedIdeaViewModel, ideaVM);
                                }
                                
                            }

                            return true;
                        });
                    }
                    //catch (System.Data.Entity.Validation.DbEntityValidationException ex)
                    //{
                    //    foreach (var entityValidationErrors in ex.EntityValidationErrors)
                    //    {
                    //        foreach (var validationError in entityValidationErrors.ValidationErrors)
                    //        {
                    //            Console.Write("Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage);
                    //        }
                    //    }
                    //    transaction.Rollback();
                    //    return false;
                    //}
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }

        public async Task<bool> UpdateIdea(VCIdeaMasterViewModel ideaVM, int userId, bool isGlobalApprover, bool isBUApprover, string DirtyValuesList)
        {
            using (ValuePortalEntities context = new ValuePortalEntities())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        int senderEmpId = userId;
                        int receiverEmpId = ideaVM.SubmittedBy;
                        string[] dirtyValuesListArray = { };

                        if (DirtyValuesList != null)
                        {
                            dirtyValuesListArray = DirtyValuesList.Split(',');
                        }

                        return await Task.Run(() =>
                        {
                            var isUpdated = true;
                            var idea = Mapper.Map<VCIdeaMasterViewModel, VPIdeaDetail>(ideaVM);
                          //  idea.CreatedDate = DateTime.Now;
                            idea.LastUpdatedDate = DateTime.Now;
                            isUpdated = service.Update(idea);
                            var ReviewerDetail = service.Top<VPReviewerDetail>(0, m => m.VPIdeaDetailID == ideaVM.ID).ToList();
                            if (ReviewerDetail.Count == 0)
                            {

                                var model = new VPReviewerDetail();
                                model.VPIdeaDetailID = ideaVM.ID;
                                model.BenefitScore = ideaVM.Benefit;
                                model.Cost = ideaVM.Cost;
                                model.FinalScore = ideaVM.FinalScore;
                                model.CreatedDate = DateTime.Now;
                                model.UpdatedBy = ideaVM.SubmittedBy;
                                model.LastUpdatedDate = DateTime.Now;
                                var pass = service.Create<VPReviewerDetail>(model, null);
                            }
                            else
                            {
                                ReviewerDetail[0].Cost = ideaVM.Cost;
                                ReviewerDetail[0].BenefitScore = ideaVM.Benefit;
                                ReviewerDetail[0].FinalScore = ideaVM.FinalScore;
                                idea.VPReviewerDetails.Add(ReviewerDetail[0]);
                                isUpdated = service.Update(ReviewerDetail[0]);
                            }

                            var vpCommentModel = new VPComment();
                            vpCommentModel.VPIdeaDetailID = ideaVM.ID;
                            vpCommentModel.ReviewerId = ideaVM.ReviewerId;
                            vpCommentModel.ReviewerComments = ideaVM.UserComment;
                            vpCommentModel.CreatedDate = DateTime.Now;
                            var vpCommentPass = service.Create<VPComment>(vpCommentModel, null);

                            service.Finalize(true);
                            transaction.Commit();

                            var ideaDetails = service.Top<VPIdeaDetail>(0, m => m.ID == ideaVM.ID).FirstOrDefault();
                            var ideaViewModel = Mapper.Map<VPIdeaDetail, VCIdeaMasterViewModel>(ideaDetails);

                            var statusDBRow = service.First<VPStatu>(x => x.IsActive && x.ID == ideaViewModel.StatusID);
                            if(statusDBRow != null)
                            {
                                ideaViewModel.StatusName = statusDBRow.Name;
                            }

                            var priorityDBRow = service.First<VPPriority>(x => x.IsActive && x.ID == ideaViewModel.PriorityID);
                            if(priorityDBRow != null)
                            {
                                ideaViewModel.PriorityName = priorityDBRow.Name;
                            }
                            
                            var costDBRow = service.First<VPCost>(x => x.IsActive.Value && x.ID == ideaVM.Cost);
                            if(costDBRow != null)
                            {
                                ideaViewModel.CostDesc = costDBRow.Name;
                            }

                            var benefitsDBRow = service.First<VPBenefit>(x => x.IsActive.Value && x.ID == ideaVM.Benefit);
                            if (benefitsDBRow != null)
                            {
                                ideaViewModel.BenefitValue = benefitsDBRow.Name;
                            }

                            ideaViewModel.UserComment = ideaVM.UserComment;

                            emailService.SendVCFPIIdeaUpdate(ideaViewModel, senderEmpId, receiverEmpId, isGlobalApprover, isBUApprover, dirtyValuesListArray);
                            return true;
                        });
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }

        //public async Task<bool> LockUnlockTheIdea(int ideaId, int userId)
        //{
        //    using (ValuePortalEntities context = new ValuePortalEntities())
        //    {
        //        using (var transaction = context.Database.BeginTransaction())
        //        {
        //            try
        //            {                      
        //                var isUpdated = true;                          
        //                if (ideaId == 0)
        //                {
        //                    var ideaofuserid = service.Top<VPIdeaDetail>(0, m => m.IsLockedBy.Contains(userId.ToString()));
        //                    foreach (var items in ideaofuserid)
        //                    {                             
        //                        items.IsLockedBy = "";
        //                        items.LastUpdatedDate = DateTime.Now;                               
        //                        var pass = service.Update<VPIdeaDetail>(items);
        //                    }                         
        //                }
        //                else
        //                {
        //                    var idea = service.Top<VPIdeaDetail>(0, m => m.ID == ideaId).FirstOrDefault();
        //                    var ideaViewModel = Mapper.Map<VPIdeaDetail, VCIdeaMasterViewModel>(idea);
        //                    ideaViewModel.SubmittedByDetails = await _employeeService.GetProfile(userId, false);
        //                    idea.IsLockedBy = ideaViewModel.SubmittedByDetails.FirstName + " " + ideaViewModel.SubmittedByDetails.LastName + '(' + userId + ')';
        //                    idea.LastUpdatedDate = DateTime.Now;
        //                    isUpdated = service.Update(idea);
        //                }

        //                    service.Finalize(true);
        //                    transaction.Commit();
        //                    return true;
        //             //   });
        //            }
        //            catch (Exception ex)
        //            {
        //                transaction.Rollback();
        //                return false;
        //            }
        //        }
        //    }
        //}

        public async Task<VPSubmittedCountViewModel> GetSubmittedIdeaCountbyId(VPApproverFlowHandlerModel vcfUserModel)
        {
            long userId = vcfUserModel.userId;
            return await Task.Run(() =>
            {
                return getIdeasCountForGenralGlobalUser(userId);
            });
            
            // Comented on 2nd May as now BU approver user can also see ideas of other BU in readonly-mode
            //if (vcfUserModel.isBUApprover)
            //{
            //    return await Task.Run(() =>
            //    {
            //        return getIdeasCountForBUApprover(userId,vcfUserModel);
            //    });
            //}
            //else
            //{
               
            //}
        }

        public VPSubmittedCountViewModel getIdeasCountForBUApprover(long userId, VPApproverFlowHandlerModel vcfUserModel)
        {
                var allSubmittedIdeaCount = 0;
                var allReviewedCount = 0;
                var allRejectedCount = 0;
                var allSponsoredCount = 0;
                var allCompletedCount = 0;
                var selfSubmittedIdeaCount = 0;
                var selfReviewedIdeaCount = 0;
                var selfRejectedIdeaCount = 0;
                var selfSponsoredIdeaCount = 0;
                var selfCompletedIdeaCount = 0;

                allSubmittedIdeaCount = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && v.StatusID == 1 && (vcfUserModel.assignedBUList.Contains(v.BusinessUnit) || (v.SubmittedBy == userId || v.TeammemberIds.Contains(userId.ToString())))).ToList().Count();
                allReviewedCount = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && v.StatusID == 2 && (vcfUserModel.assignedBUList.Contains(v.BusinessUnit) || (v.SubmittedBy == userId || v.TeammemberIds.Contains(userId.ToString())))).ToList().Count();
                allRejectedCount = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && v.StatusID == 3 && (vcfUserModel.assignedBUList.Contains(v.BusinessUnit) || (v.SubmittedBy == userId || v.TeammemberIds.Contains(userId.ToString())))).ToList().Count();
                allSponsoredCount = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && v.StatusID == 4 && (vcfUserModel.assignedBUList.Contains(v.BusinessUnit) || (v.SubmittedBy == userId || v.TeammemberIds.Contains(userId.ToString())))).ToList().Count();
                allCompletedCount = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && v.StatusID == 5 && (vcfUserModel.assignedBUList.Contains(v.BusinessUnit) || (v.SubmittedBy == userId || v.TeammemberIds.Contains(userId.ToString())))).ToList().Count();

                selfSubmittedIdeaCount = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && (v.SubmittedBy == userId || v.TeammemberIds.Contains(userId.ToString())) && v.StatusID == 1).ToList().Count();
                selfReviewedIdeaCount = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && (v.SubmittedBy == userId || v.TeammemberIds.Contains(userId.ToString())) && v.StatusID == 2).ToList().Count();
                selfRejectedIdeaCount = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && (v.SubmittedBy == userId || v.TeammemberIds.Contains(userId.ToString())) && v.StatusID == 3).ToList().Count();
                selfSponsoredIdeaCount = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && (v.SubmittedBy == userId || v.TeammemberIds.Contains(userId.ToString())) && v.StatusID == 4).ToList().Count();
                selfCompletedIdeaCount = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && (v.SubmittedBy == userId || v.TeammemberIds.Contains(userId.ToString())) && v.StatusID == 5).ToList().Count();

                VPSubmittedCountViewModel vcfIdeaCountVM = new VPSubmittedCountViewModel();
                vcfIdeaCountVM.NoofIdeasSubmittedbyme = selfSubmittedIdeaCount;
                vcfIdeaCountVM.ReviewedCountbyme = selfReviewedIdeaCount;
                vcfIdeaCountVM.Sponsoredbyme = selfSponsoredIdeaCount;
                vcfIdeaCountVM.Completedbyme = selfCompletedIdeaCount;

                vcfIdeaCountVM.ALLNoofIdeasSubmitted = allSubmittedIdeaCount;
                vcfIdeaCountVM.allRejectedCount = allRejectedCount;
                vcfIdeaCountVM.ALLReviewedCount = allReviewedCount;
                vcfIdeaCountVM.ALLSponsored = allSponsoredCount;
                vcfIdeaCountVM.ALLCompleted = allCompletedCount;

                return vcfIdeaCountVM;
        }

        public VPSubmittedCountViewModel getIdeasCountForGenralGlobalUser(long userId)
        {
            /* Over all Idea Count vatiables */
            var allSubmittedIdeaCount = 0;
            var allReviewedCount = 0;
            var allRejectedCount = 0;
            var allSponsoredCount = 0;
            var allCompletedCount = 0;
            var overAllShrtlistedIdeaCount = 0;
            var overAllInReviewIdeaCount = 0;
            var overAllNAI1IdeaCount = 0;
            var overAllNAI2IdeaCount = 0;
            var overAllOnHold1IdeaCount = 0;
            var overAllOnHold2IdeaCount = 0;
            var overAllReject1IdeaCount = 0;
            var overAllReject2IdeaCount = 0;
            var overAllReqReviewIdeaCount = 0;
            var overAllDeprcatedIdeaCount = 0;
            var overAllDeferredIdeaCount = 0;
            var overAllInExeIdeaCount = 0;

            /* Self Idea Count vatiables */
            var selfDraftIdeaCount = 0;
            var selfSubmittedIdeaCount = 0;
            var selfShrtlistedIdeaCount = 0;
            var selfInReviewIdeaCount = 0;
            var selfNAI1IdeaCount = 0;
            var selfNAI2IdeaCount = 0;
            var selfOnHold1IdeaCount = 0;
            var selfOnHold2IdeaCount = 0;
            var selfReject1IdeaCount = 0;
            var selfReject2IdeaCount = 0;
            var selfReviewedIdeaCount = 0;
            var selfSponsoredIdeaCount = 0;
            var selfReqReviewIdeaCount = 0;
            var selfDeprcatedIdeaCount = 0;
            var selfCompletedIdeaCount = 0;
            var selfDeferredIdeaCount = 0;
            var selfInExeIdeaCount = 0;

            //Old Rejected Status ID code
            //var selfRejectedIdeaCount = 0;
            // allRejectedCount = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && v.StatusID == 3).ToList().Count();

            // Over all Submitted Table Header Count
            allSubmittedIdeaCount = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && v.StatusID == 1).Count();

            // Over all Shortlisted Table Header Count
            overAllShrtlistedIdeaCount = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && v.StatusID == 6).Count();
            overAllInReviewIdeaCount = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && v.StatusID == 8).Count();
            overAllNAI1IdeaCount = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && v.StatusID == 17).Count();
            overAllReject1IdeaCount = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && v.StatusID == 18).Count();
            overAllOnHold1IdeaCount = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && v.StatusID == 19).Count();

            // Over all Reviewed Table Header Count
            allReviewedCount = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && v.StatusID == 2).Count();
            overAllNAI2IdeaCount = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && v.StatusID == 14).Count();
            overAllReject2IdeaCount = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && v.StatusID == 15).Count();
            overAllOnHold2IdeaCount = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && v.StatusID == 16).Count();

            // Over all Sponsored Table Header Count
            allSponsoredCount = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && v.StatusID == 4).Count();
            overAllDeprcatedIdeaCount = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && v.StatusID == 9).Count();
            overAllReqReviewIdeaCount = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && v.StatusID == 13).Count();

            // Over all Completed Table Header Count
            allCompletedCount = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && v.StatusID == 5).Count();
            overAllDeferredIdeaCount = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && v.StatusID == 10).Count();
            overAllInExeIdeaCount = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && v.StatusID == 11).Count();

            //Self Submitted Table Header Count
            selfSubmittedIdeaCount = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && (v.SubmittedBy == userId || v.TeammemberIds.Contains(userId.ToString())) && v.StatusID == 1).Count();
            selfDraftIdeaCount = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && (v.SubmittedBy == userId || v.TeammemberIds.Contains(userId.ToString())) && v.StatusID == 7).Count();

            //Self Shortlisted Table Header Count
            selfShrtlistedIdeaCount = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && (v.SubmittedBy == userId || v.TeammemberIds.Contains(userId.ToString())) && v.StatusID == 6).Count();
            selfInReviewIdeaCount = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && (v.SubmittedBy == userId || v.TeammemberIds.Contains(userId.ToString())) && v.StatusID == 8).Count();
            selfNAI1IdeaCount = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && (v.SubmittedBy == userId || v.TeammemberIds.Contains(userId.ToString())) && v.StatusID == 17).Count();
            selfReject1IdeaCount = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && (v.SubmittedBy == userId || v.TeammemberIds.Contains(userId.ToString())) && v.StatusID == 18).Count();
            selfOnHold1IdeaCount = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && (v.SubmittedBy == userId || v.TeammemberIds.Contains(userId.ToString())) && v.StatusID == 19).Count();

            //Self Reviewed Table Header Count
            selfReviewedIdeaCount = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && (v.SubmittedBy == userId || v.TeammemberIds.Contains(userId.ToString())) && v.StatusID == 2).Count();
            selfNAI2IdeaCount = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && (v.SubmittedBy == userId || v.TeammemberIds.Contains(userId.ToString())) && v.StatusID == 14).Count();
            selfReject2IdeaCount = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && (v.SubmittedBy == userId || v.TeammemberIds.Contains(userId.ToString())) && v.StatusID == 15).Count();
            selfOnHold2IdeaCount = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && (v.SubmittedBy == userId || v.TeammemberIds.Contains(userId.ToString())) && v.StatusID == 16).Count();

            //Self Sponsored Table Header Count
            selfSponsoredIdeaCount = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && (v.SubmittedBy == userId || v.TeammemberIds.Contains(userId.ToString())) && v.StatusID == 4).Count();
            selfDeprcatedIdeaCount = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && (v.SubmittedBy == userId || v.TeammemberIds.Contains(userId.ToString())) && v.StatusID == 9).Count();
            selfReqReviewIdeaCount = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && (v.SubmittedBy == userId || v.TeammemberIds.Contains(userId.ToString())) && v.StatusID == 13).Count();

            //Self Completed Table Header Count
            selfCompletedIdeaCount = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && (v.SubmittedBy == userId || v.TeammemberIds.Contains(userId.ToString())) && v.StatusID == 5).Count();
            selfDeferredIdeaCount = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && (v.SubmittedBy == userId || v.TeammemberIds.Contains(userId.ToString())) && v.StatusID == 10).Count();
            selfInExeIdeaCount = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && (v.SubmittedBy == userId || v.TeammemberIds.Contains(userId.ToString())) && v.StatusID == 11).Count();

            //Old Rejected Status ID code
            //selfRejectedIdeaCount = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && (v.SubmittedBy == userId || v.TeammemberIds.Contains(userId.ToString())) && v.StatusID == 3).Count();

            VPSubmittedCountViewModel vcfIdeaCountVM = new VPSubmittedCountViewModel();

            // Self Ideas related Counts
            vcfIdeaCountVM.NoofIdeasSubmittedbyme = selfSubmittedIdeaCount;
            vcfIdeaCountVM.ReviewedCountbyme = selfReviewedIdeaCount;
            vcfIdeaCountVM.Sponsoredbyme = selfSponsoredIdeaCount;
            vcfIdeaCountVM.Completedbyme = selfCompletedIdeaCount;

            vcfIdeaCountVM.selfDraftIdeaCount = selfDraftIdeaCount;
            vcfIdeaCountVM.selfShrtlistedIdeaCount = selfShrtlistedIdeaCount;
            vcfIdeaCountVM.selfInReviewIdeaCount = selfInReviewIdeaCount;
            vcfIdeaCountVM.selfNAI1IdeaCount = selfNAI1IdeaCount;
            vcfIdeaCountVM.selfNAI2IdeaCount = selfNAI2IdeaCount;
            vcfIdeaCountVM.selfOnHold1IdeaCount = selfOnHold1IdeaCount;
            vcfIdeaCountVM.selfOnHold2IdeaCount = selfOnHold2IdeaCount;
            vcfIdeaCountVM.selfReject1IdeaCount = selfReject1IdeaCount;
            vcfIdeaCountVM.selfReject2IdeaCount = selfReject2IdeaCount;
            vcfIdeaCountVM.selfReqReviewIdeaCount = selfReqReviewIdeaCount;
            vcfIdeaCountVM.selfDeprcatedIdeaCount = selfDeprcatedIdeaCount;
            vcfIdeaCountVM.selfDeferredIdeaCount = selfDeferredIdeaCount;
            vcfIdeaCountVM.selfInExeIdeaCount = selfInExeIdeaCount;

            // Over all Ideas related Counts
            vcfIdeaCountVM.ALLNoofIdeasSubmitted = allSubmittedIdeaCount;
            vcfIdeaCountVM.allRejectedCount = allRejectedCount;
            vcfIdeaCountVM.ALLReviewedCount = allReviewedCount;
            vcfIdeaCountVM.ALLSponsored = allSponsoredCount;
            vcfIdeaCountVM.ALLCompleted = allCompletedCount;

            vcfIdeaCountVM.overAllShrtlistedIdeaCount = overAllShrtlistedIdeaCount;
            vcfIdeaCountVM.overAllInReviewIdeaCount = overAllInReviewIdeaCount;
            vcfIdeaCountVM.overAllNAI1IdeaCount = overAllNAI1IdeaCount;
            vcfIdeaCountVM.overAllNAI2IdeaCount = overAllNAI2IdeaCount;
            vcfIdeaCountVM.overAllOnHold1IdeaCount = overAllOnHold1IdeaCount;
            vcfIdeaCountVM.overAllOnHold2IdeaCount = overAllOnHold2IdeaCount;
            vcfIdeaCountVM.overAllReject1IdeaCount = overAllReject1IdeaCount;
            vcfIdeaCountVM.overAllReject2IdeaCount = overAllReject2IdeaCount;
            vcfIdeaCountVM.overAllReqReviewIdeaCount = overAllReqReviewIdeaCount;
            vcfIdeaCountVM.overAllDeprcatedIdeaCount = overAllDeprcatedIdeaCount;
            vcfIdeaCountVM.overAllDeferredIdeaCount = overAllDeferredIdeaCount;
            vcfIdeaCountVM.overAllInExeIdeaCount = overAllInExeIdeaCount;

            return vcfIdeaCountVM;
        }
        public async Task<bool> SaveComment(VPCommentsViewModel ideaNewCommentVM)
        {
            using (ValuePortalEntities context = new ValuePortalEntities())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                            var model = new VPComment();                            
                            model.id = ideaNewCommentVM.id;
                            model.VPIdeaDetailID = ideaNewCommentVM.VPIdeaDetailID;
                            model. ReviewerId = ideaNewCommentVM.ReviewerId;
                            model. ReviewerComments = ideaNewCommentVM.ReviewerComments;
                            model.CreatedDate = DateTime.Now;
                            var pass = service.Create<VPComment>(model, null);                         

                            service.Finalize(true);
                            transaction.Commit();

                            var ideaDetails = service.Top<VPIdeaDetail>(0, m => m.ID == ideaNewCommentVM.VPIdeaDetailID).FirstOrDefault();
                            var ideaViewModel = Mapper.Map<VPIdeaDetail, VCIdeaMasterViewModel>(ideaDetails);

                            // var ideaCostBenefitsDetail = service.Top<VPReviewerDetail>(0, m => m.ID == ideaNewCommentVM.VPIdeaDetailID).FirstOrDefault();
                            
                            // Commented on 29th June 22 due to Phase 2 Shorter Email Template
                           // var ideaCostBenefitsDetail = service.Top<VPReviewerDetail>(0, m => m.VPIdeaDetailID == ideaNewCommentVM.VPIdeaDetailID).FirstOrDefault();

                            int senderEmpId = ideaNewCommentVM.ReviewerId;
                            int receiverEmpId = ideaDetails.SubmittedBy;

                            var statusDBRow = service.First<VPStatu>(x => x.IsActive && x.ID == ideaViewModel.StatusID);
                            if (statusDBRow != null)
                            {
                                ideaViewModel.StatusName = statusDBRow.Name;
                            }

                            // Commented on 29th June 22 due to Phase 2 Shorter Email Template
                            //var priorityDBRow = service.First<VPPriority>(x => x.IsActive && x.ID == ideaViewModel.PriorityID);
                            //if (priorityDBRow != null)
                            //{
                            //    ideaViewModel.PriorityName = priorityDBRow.Name;
                            //}

                            //var costDBRow = service.First<VPCost>(x => x.IsActive.Value && x.ID == ideaCostBenefitsDetail.Cost);
                            //var costDBRow = service.First<VPCost>(x => x.IsActive == true && x.ID == ideaCostBenefitsDetail.Cost);
                            //    if (costDBRow != null)
                            //        {
                            //            ideaViewModel.CostDesc = costDBRow.Name;
                            //        }

                            // var benefitsDBRow = service.First<VPBenefit>(x => x.IsActive.Value && x.ID == ideaCostBenefitsDetail.BenefitScore);
                      
                            ideaViewModel.UserComment = ideaNewCommentVM.ReviewerComments;
                            emailService.SendValuePortalCommentUpdate(senderEmpId, receiverEmpId, ideaViewModel, ideaNewCommentVM.ReviewerComments);

                            return true;                   
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }

        public Task<VPBUApproverHandlerModel> checkIsBUApprover(VPBUApproverHandlerModel vPBUApproverVM)
        {
           return _employeeService.checkIsBUApprover(vPBUApproverVM);
        }
    }

}
