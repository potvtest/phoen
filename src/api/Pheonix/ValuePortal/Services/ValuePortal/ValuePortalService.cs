//using AutoMapper;
//using Pheonix.Core.v1.Services.Business;
//using Pheonix.Core.v1.Services.Email;
//using Pheonix.Models.ViewModels;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using ValuePortal.Repository;

//namespace ValuePortal.Services
//{
//    public class ValuePortalService : IValuePortalService
//    {        
//        private IMainOperationsService service;
//        private IEmployeeService _employeeService;
//        private IEmailService emailService;
//        public ValuePortalService(IVPContextRepository repository, IMainOperationsService opsService, IEmployeeService employeeService,IEmailService opsEmailService)
//        {
//            service = opsService;
//            _employeeService = employeeService;
//            emailService = opsEmailService;
//        }
//        public ValuePortalService(IVPContextRepository repository, IMainOperationsService opsService)
//        {
//            service = opsService;
//        }

//        public async Task<IEnumerable<VCIdeaMasterViewModel>> GetIdeaList(long userId,string status,long loginuserId)
//        {
//            return await Task.Run(async () =>
//            {

//                bool success = await LockUnlockTheIdea(0, (int)loginuserId);
//                var ideaList = service.Top<VPIdeaDetail>(0, v => v.IsDeleted  == false).OrderByDescending(v => v.CreatedDate).ToList();
//                if (status == "IdeasSubmitted" && userId!=0)
//                {
//                    ideaList = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && v.StatusID>0 && v.SubmittedBy == userId || v.TeammemberIds.Contains(userId.ToString())).OrderByDescending(v => v.CreatedDate).ToList();
//                }
//                else if (status == "Reviewed" && userId != 0)
//                {
//                    ideaList = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && v.StatusID > 1 && v.SubmittedBy == userId || v.TeammemberIds.Contains(userId.ToString())).OrderByDescending(v => v.CreatedDate).ToList();
//                }
//                else if (status == "Sponsored" && userId != 0)
//                {
//                    ideaList = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && v.StatusID ==4 && v.SubmittedBy == userId || v.TeammemberIds.Contains(userId.ToString())).OrderByDescending(v => v.CreatedDate).ToList();
//                }
//                else if (status == "Completed" && userId != 0)
//                {
//                    ideaList = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && v.StatusID == 5 && v.SubmittedBy == userId || v.TeammemberIds.Contains(userId.ToString())).OrderByDescending(v => v.CreatedDate).ToList();
//                }
//                else if (status == "IdeasSubmitted" && userId == 0)
//                {
//                    ideaList = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && v.StatusID > 0).OrderByDescending(v => v.CreatedDate).ToList();
//                }
//                else if (status == "Reviewed" && userId == 0)
//                {
//                    ideaList = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && v.StatusID > 1 ).OrderByDescending(v => v.CreatedDate).ToList();
//                }
//                else if (status == "Sponsored" && userId == 0)
//                {
//                    ideaList = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && v.StatusID == 4).OrderByDescending(v => v.CreatedDate).ToList();
//                }
//                else if (status == "Completed" && userId == 0)
//                {
//                    ideaList = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && v.StatusID == 5).OrderByDescending(v => v.CreatedDate).ToList();
//                }
//                var ideaListViewModel = Mapper.Map<IEnumerable<VPIdeaDetail>, IEnumerable<VCIdeaMasterViewModel>>(ideaList).ToList();

               
               
//                foreach (var idea in ideaListViewModel)
//                {                   
//                    foreach (var i in ideaList)
//                    {
//                        if(i.ID==idea.ID)
//                        {                           
//                            var p = i.VPReviewerDetails;
//                            foreach (var ii in p)
//                            {                            
//                                var Final = p.OfType<dynamic>().FirstOrDefault();
//                                idea.Cost= Final.Cost;                                
//                                idea.FinalScore = Convert.ToInt16(((ValuePortal.VPReviewerDetail)Final).FinalScore);                               
//                                goto outer;
//                            }                            
//                        }
//                    }
//                outer:;
//                }
//                foreach (var idea in ideaListViewModel)
//                {
//                    idea.SubmittedByDetails = await _employeeService.GetProfile(idea.SubmittedBy, false);
//                }
//                return ideaListViewModel;
//            });
//        }
//        //public async Task<IEnumerable<VCIdeaMasterViewModel>> GetIdeaListByUser(int userId)
//        //{
//        //    return await Task.Run(() =>
//        //    {
//        //        var ideaList = service.Top<VPIdeaDetail>(100, v => v.IsDeleted == false && v.SubmittedBy == userId).OrderByDescending(v => v.CreatedDate).ToList();
//        //        return Mapper.Map<IEnumerable<VPIdeaDetail>, IEnumerable<VCIdeaMasterViewModel>>(ideaList).ToList();
//        //    });
//        //}
//        public async Task<VCIdeaMasterViewModel> GetIdea(long id,long loginuserId)
//        {
//            return await Task.Run(async() =>
//            {
//                bool success = await LockUnlockTheIdea((int)id, (int)loginuserId);
//                var idea = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && v.ID == id).FirstOrDefault();
//                var ReviewData = service.Top<VPReviewerDetail>(0, v => v.VPIdeaDetailID == id).FirstOrDefault();
//                var ideaViewModel = Mapper.Map<VPIdeaDetail, VCIdeaMasterViewModel>(idea);
//                if (ReviewData != null)
//                {
//                    ideaViewModel.Cost = Convert.ToDouble(((ValuePortal.VPReviewerDetail)ReviewData).Cost);
//                    ideaViewModel.FinalScore = Convert.ToInt16(((ValuePortal.VPReviewerDetail)ReviewData).FinalScore);
//                    ideaViewModel.ReviewerComment1 = ((ValuePortal.VPReviewerDetail)ReviewData).ReviewerComments;
//                    ideaViewModel.ReviewerComment2 = ((ValuePortal.VPReviewerDetail)ReviewData).ReviewerComments2;
//                    ideaViewModel.ReviewerComment3 = ((ValuePortal.VPReviewerDetail)ReviewData).ReviewerComments3;
//                    ideaViewModel.Benefit = Convert.ToDouble(((ValuePortal.VPReviewerDetail)ReviewData).BenefitScore);
//                }              

//                ideaViewModel.SubmittedByDetails = await _employeeService.GetProfile(idea.SubmittedBy, false);
//                foreach (var comment in ideaViewModel.VPComments)
//                {
//                    comment.SubmittedByDetails = await _employeeService.GetProfile(comment.ReviewerId, false);
//                }
//                return ideaViewModel;
//            });
//        }

//        public async Task<VPAllMastersdataViewModel> GetMastersdata()
//        {
           
//               VPAllMastersdataViewModel All = new VPAllMastersdataViewModel();

//               var priorityList = service.Top<VPPriority>(0, v => v.IsActive == true).OrderBy(v => v.ID).ToList();
//               All.PriorityCollection = Mapper.Map<IEnumerable<VPPriority>, IEnumerable<VPPriorityViewModel>>(priorityList).ToList();

//               var StatusList = service.Top<VPStatu>(0, v => v.IsActive == true).OrderBy(v => v.ID).ToList();
//               All.StatusCollection = Mapper.Map<IEnumerable<VPStatu>, IEnumerable<VPStatusViewModel>>(StatusList).ToList();

//               var BenefitList = service.Top<VPBenefit>(0, v => v.IsActive == true).OrderBy(v => v.ID).ToList();
//               All.BenefitCollection = Mapper.Map<IEnumerable<VPBenefit>, IEnumerable<VPBenefitViewModel>>(BenefitList).ToList();

//               var CostList = service.Top<VPCost>(0, v => v.IsActive == true).OrderBy(v => v.ID).ToList();
//               All.CostCollection = Mapper.Map<IEnumerable<VPCost>, IEnumerable<VPCostViewModel>>(CostList).ToList();
//               return  All;
          

//        }

//        public async Task<IEnumerable<VPConfigurationViewModel>> GetConfigurationdata()
//        {
//            return await Task.Run(async() =>
//            {
//                var priorityList = service.Top<VPConfiguration>(0, v => v.IsActive == true).OrderBy(v => v.Name).ToList();
//                return Mapper.Map<IEnumerable<VPConfiguration>, IEnumerable<VPConfigurationViewModel>>(priorityList).ToList();
//            });
          


//        }
//        //public async Task<IEnumerable<VPPriorityViewModel>> GetPriorityList()
//        //{
//        //    return await Task.Run(() =>
//        //    {
//        //        var priorityList = service.Top<VPPriority>(0, v => v.IsActive == true).OrderBy(v => v.Name).ToList();
//        //       // return Mapper.Map<IEnumerable<VPPriority>, IEnumerable<VPPriorityViewModel>>(priorityList).ToList();


//        //        //var ideaListViewModel = Mapper.Map<IEnumerable<VPPriority>, IEnumerable<VPPriorityViewModel>>(priorityList).ToList();

            
//        //        //var StatusList = service.Top<VPStatu>(0, v1 => v1.IsActive == true).OrderBy(v1 => v1.Name).ToList();


//        //        //VPPriorityViewModel v3 = new VPPriorityViewModel();
//        //        //v3.StatusCollection = Mapper.Map<IEnumerable<VPStatu>, IEnumerable<VPStatusViewModel>>(StatusList).ToList();




//        //        return Mapper.Map<IEnumerable<VPPriority>, IEnumerable<VPPriorityViewModel>>(priorityList).ToList();
//        //    });
//        //}
//        public async Task<IEnumerable<VPStatusViewModel>> GetStatusList()
//        {
//            return await Task.Run(() =>
//            {
//                var priorityList = service.Top<VPStatu>(0, v => v.IsActive == true).OrderBy(v => v.Name).ToList();
//                return Mapper.Map<IEnumerable<VPStatu>, IEnumerable<VPStatusViewModel>>(priorityList).ToList();
//            });
//        }
//        public async Task<bool> SaveIdea(VCIdeaMasterViewModel ideaVM)//,int userId)
//        {
//            using (ValuePortalEntities context = new ValuePortalEntities())
//            {
//                using (var transaction = context.Database.BeginTransaction())
//                {
//                    try
//                    {
//                        return await Task.Run(() =>
//                        {
//                            var isCreated = true;

//                            var idea = Mapper.Map<VCIdeaMasterViewModel, VPIdeaDetail>(ideaVM);
//                            idea.SubmittedBy = ideaVM.SubmittedBy;// userId;
//                            idea.ID = 0;
//                            idea.CreatedDate = DateTime.Now;
//                            idea.LastUpdatedDate = DateTime.Now;
//                            isCreated = service.Create<VPIdeaDetail>(idea, x => x.ID == 0);



//                            var model = new VPReviewerDetail();
//                            model.VPIdeaDetailID = ideaVM.ID;
//                            model.BenefitScore = 1;
//                            model.Cost = 1;
//                            model.FinalScore = 1;
//                            model.CreatedDate = DateTime.Now;
//                            model.UpdatedBy = ideaVM.SubmittedBy;
//                            model.LastUpdatedDate = DateTime.Now;
//                            var pass = service.Create<VPReviewerDetail>(model, null);

//                            service.Finalize(true);
//                            transaction.Commit();
//                            return true;
//                        });
//                    }
//                    catch (Exception ex)
//                    {
//                        transaction.Rollback();
//                        return false;
//                    }
//                }
//            }
//        }

//        public async Task<bool> UpdateIdea(VCIdeaMasterViewModel ideaVM, int userId)
//        {
//            using (ValuePortalEntities context = new ValuePortalEntities())
//            {
//                using (var transaction = context.Database.BeginTransaction())
//                {
//                    try
//                    {
//                        return await Task.Run(() =>
//                        {
//                            var isUpdated = true;                           
//                            var idea = Mapper.Map<VCIdeaMasterViewModel, VPIdeaDetail>(ideaVM);                           
//                            idea.CreatedDate = DateTime.Now;
//                            idea.LastUpdatedDate = DateTime.Now;
//                            isUpdated = service.Update(idea);
//                            var ReviewerDetail = service.Top<VPReviewerDetail>(0, m => m.VPIdeaDetailID == ideaVM.ID).ToList();
//                            if (ReviewerDetail.Count == 0)
//                            {                               

//                                var model = new VPReviewerDetail();
//                                model.VPIdeaDetailID = ideaVM.ID;
//                                model.BenefitScore = ideaVM.Benefit;
//                                model.Cost = ideaVM.Cost;
//                                model.FinalScore = ideaVM.FinalScore;
//                                model.CreatedDate = DateTime.Now;
//                                model.UpdatedBy= ideaVM.SubmittedBy;
//                                model.LastUpdatedDate = DateTime.Now;
//                                var pass = service.Create<VPReviewerDetail>(model, null);
//                            }
//                            else
//                            {
//                                ReviewerDetail[0].Cost = ideaVM.Cost;
//                                ReviewerDetail[0].BenefitScore = ideaVM.Benefit;
//                                ReviewerDetail[0].FinalScore = ideaVM.FinalScore;
//                                idea.VPReviewerDetails.Add(ReviewerDetail[0]);
//                                isUpdated = service.Update(ReviewerDetail[0]);
//                            }                                                  

//                            service.Finalize(true);
//                            transaction.Commit();
//                            return true;
//                        });
//                    }
//                    catch (Exception ex)
//                    {
//                        transaction.Rollback();
//                        return false;
//                    }
//                }
//            }
//        }

//        public async Task<bool> LockUnlockTheIdea(int ideaId, int userId)
//        {
//            using (ValuePortalEntities context = new ValuePortalEntities())
//            {
//                using (var transaction = context.Database.BeginTransaction())
//                {
//                    try
//                    {                      
//                        var isUpdated = true;                          
//                        if (ideaId == 0)
//                        {
//                            var ideaofuserid = service.Top<VPIdeaDetail>(0, m => m.IsLockedBy.Contains(userId.ToString()));
//                            foreach (var items in ideaofuserid)
//                            {                             
//                                items.IsLockedBy = "";
//                                items.LastUpdatedDate = DateTime.Now;                               
//                                var pass = service.Update<VPIdeaDetail>(items);
//                            }                         
//                        }
//                        else
//                        {
//                            var idea = service.Top<VPIdeaDetail>(0, m => m.ID == ideaId).FirstOrDefault();
//                            var ideaViewModel = Mapper.Map<VPIdeaDetail, VCIdeaMasterViewModel>(idea);
//                            ideaViewModel.SubmittedByDetails = await _employeeService.GetProfile(userId, false);
//                            idea.IsLockedBy = ideaViewModel.SubmittedByDetails.FirstName + " " + ideaViewModel.SubmittedByDetails.LastName + '(' + userId + ')';
//                            idea.LastUpdatedDate = DateTime.Now;
//                            isUpdated = service.Update(idea);
//                        }

//                            service.Finalize(true);
//                            transaction.Commit();
//                            return true;
//                     //   });
//                    }
//                    catch (Exception ex)
//                    {
//                        transaction.Rollback();
//                        return false;
//                    }
//                }
//            }
//        }
//        public async Task<VPSubmittedCountViewModel> GetSubmittedIdeaCountbyId(long userId)
//        {
//            return await Task.Run(() =>
//            {
//                var ALLVPIdeaCount = 0;
//                var ALLReviewedCount = 0;
//                var ALLSponsored = 0;
//                var ALLCompleted = 0;
//                var VPIdeaCountbyme = 0;
//                var ReviewedCountbyme = 0;
//                var Sponsoredbyme = 0;
//                var Completedbyme = 0;

//                ALLVPIdeaCount = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false ).OrderByDescending(v => v.CreatedDate).ToList().Count();
//                ALLReviewedCount = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && v.StatusID!=1).OrderByDescending(v => v.CreatedDate).ToList().Count();
//                ALLSponsored = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && v.StatusID == 4).OrderByDescending(v => v.CreatedDate).ToList().Count();
//                ALLCompleted = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && v.StatusID == 5).OrderByDescending(v => v.CreatedDate).ToList().Count();
//                VPIdeaCountbyme = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && v.SubmittedBy == userId || v.TeammemberIds.Contains(userId.ToString())).OrderByDescending(v => v.CreatedDate).ToList().Count();
                                 
//                ReviewedCountbyme = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && v.SubmittedBy == userId && v.StatusID != 1).OrderByDescending(v => v.CreatedDate).ToList().Count();
//                Sponsoredbyme = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && v.SubmittedBy == userId && v.StatusID==4).OrderByDescending(v => v.CreatedDate).ToList().Count();
//                Completedbyme = service.Top<VPIdeaDetail>(0, v => v.IsDeleted == false && v.SubmittedBy == userId && v.StatusID == 5).OrderByDescending(v => v.CreatedDate).ToList().Count();
               
//                VPSubmittedCountViewModel VI = new VPSubmittedCountViewModel();
//                VI.NoofIdeasSubmittedbyme = VPIdeaCountbyme;
//                VI.ReviewedCountbyme = ReviewedCountbyme;
//                VI.Sponsoredbyme = Sponsoredbyme;
//                VI.Completedbyme = Completedbyme;

//                VI.ALLNoofIdeasSubmitted = ALLVPIdeaCount;
//                VI.ALLReviewedCount = ALLReviewedCount;
//                VI.ALLSponsored = ALLSponsored;
//                VI.ALLCompleted = ALLCompleted;

//                return VI;
//            });
//        }



//        public async Task<bool> SaveComment(VPCommentsViewModel ideaVM)
//        {
//            using (ValuePortalEntities context = new ValuePortalEntities())
//            {
//                using (var transaction = context.Database.BeginTransaction())
//                {
//                    try
//                    {
//                            var model = new VPComment();                            
//                            model.id = ideaVM.id;
//                            model.VPIdeaDetailID = ideaVM.VPIdeaDetailID;
//                            model. ReviewerId = ideaVM.ReviewerId;
//                            model. ReviewerComments = ideaVM.ReviewerComments;
//                            model.CreatedDate = DateTime.Now;
//                            var pass = service.Create<VPComment>(model, null);                         


//                            service.Finalize(true);
//                            transaction.Commit();
                        
//                            var ideaDetails = service.Top<VPIdeaDetail>(0, m => m.ID == ideaVM.VPIdeaDetailID).FirstOrDefault();
//                            if (ideaDetails.IsEmailReceiptRequired == true)
//                            {
//                                var ideaViewModel = Mapper.Map<VPIdeaDetail, VCIdeaMasterViewModel>(ideaDetails);

//                                ideaViewModel.SubmittedByDetails = await _employeeService.GetProfile(ideaDetails.SubmittedBy, false);
//                                var ReceiverName = ideaViewModel.SubmittedByDetails.FirstName + " " + ideaViewModel.SubmittedByDetails.LastName;
//                                var ReceiverEmail = ideaViewModel.SubmittedByDetails.OrganizationEmail;
//                                var ReceiverEmpId = ideaViewModel.SubmittedByDetails.ID;

//                                ideaViewModel.SubmittedByDetails = await _employeeService.GetProfile(ideaVM.ReviewerId, false);
//                                var SenderName = ideaViewModel.SubmittedByDetails.FirstName + " " + ideaViewModel.SubmittedByDetails.LastName;
//                                var SenderEmail = ideaViewModel.SubmittedByDetails.OrganizationEmail;
//                                var SenderEmpId = ideaViewModel.SubmittedByDetails.ID;

//                                emailService.SendValuePortalCommentUpdate(ideaDetails.RequiredEffort, SenderName, SenderEmail, SenderEmpId.ToString(), ReceiverName, ReceiverEmail, ReceiverEmpId.ToString(), ideaVM.ReviewerComments, ideaDetails.ID.ToString(), ideaDetails.IdeaHeadline, ideaDetails.IdeaDescription, ideaDetails.IdeaBenefits);

//                            }
//                        return true;                   
//                    }
//                    catch (Exception ex)
//                    {
//                        transaction.Rollback();
//                        return false;
//                    }
//                }
//            }
//        }       
//    }
//}
