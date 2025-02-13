using AutoMapper;
using Pheonix.Core.Helpers;
using Pheonix.Core.v1.Services.Approval;
using Pheonix.Core.v1.Services.Email;
using Pheonix.DBContext;
using Pheonix.DBContext.Repository;
using Pheonix.Models.ViewModels;
using Pheonix.Models.VM;
using Pheonix.Models.VM.Classes.Appraisal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using DTO = Pheonix.DBContext;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using log4net;
using System.Reflection;

namespace Pheonix.Core.v1.Services.Business
{
    //0 -Not Initiated
    //1 -Initiated
    //2 -Submitted
    //3 -Appraised
    //4- Reviewed
    //5 -Closed
    public class AppraisalService : IAppraisalService
    {
        private IBasicOperationsService service;
        private IEmailService emailService;
        private static readonly ILog Log4Net = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public AppraisalService(IContextRepository repository, IBasicOperationsService opsService, IEmailService opsEmailService)
        {
            service = opsService;
            emailService = opsEmailService;
        }

        public async Task<AppraiseQuestionsViewModel> GetAppraiseQuestionsBasedOnGrade(int userId)
        {
            return await Task.Run(() =>
            {
                var appraiseQuestionsViewModel = new AppraiseQuestionsViewModel();
                var personGrade = service.Top<PersonEmployment>(0, x => x.PersonID == userId).FirstOrDefault().Designation.Grade;
                var appraisalReporting = service.Top<AppraisalReporting>(0, x => x.PersonID == userId && x.AppraiserYear == DateTime.Now.Year).FirstOrDefault();
                appraiseQuestionsViewModel.Appraiser = string.Format("{0},{1}", appraisalReporting.Person1.FirstName, appraisalReporting.Person1.LastName);
                appraiseQuestionsViewModel.Reviewer = string.Format("{0},{1}", appraisalReporting.Person3.FirstName, appraisalReporting.Person3.LastName);
                appraiseQuestionsViewModel.AppraiseeQuestions = Mapper.Map<List<ApprisalQuestions>, List<AppraiseeQuestion>>(service.Top<ApprisalQuestions>(0, x => x.Levels == personGrade && x.IsDelete == false).ToList());
                return appraiseQuestionsViewModel;
            });
        }

        public async Task<List<AppraiseeParametersViewModel>> GetAppraiseeParametersBasedOnGrade(int userId)
        {
            return await Task.Run(() =>
            {
                var personGrade = service.Top<PersonEmployment>(0, x => x.PersonID == userId).FirstOrDefault().Designation.Grade;
                return Mapper.Map<List<ApprisalParameters>, List<AppraiseeParametersViewModel>>(service.Top<ApprisalParameters>(0, x => x.Levels == personGrade && x.IsDelete == false).ToList());
            });
        }

        public async Task<AppraisalFormViewModel> GetAppraiseeForm(int userId)
        {
            return await Task.Run(() =>
            {
                var appraisalFormViewModel = new AppraisalFormViewModel();
                appraisalFormViewModel.AppraiseForm = Mapper.Map<List<AppraiseForm>, List<AppraiseeFormModel>>(service.Top<AppraiseForm>(0, x => x.PersonID == userId && x.AppraiserYear == DateTime.Now.Year).ToList());
                return appraisalFormViewModel;
            });
        }

        /// <summary>
        /// Get the Appraisee question/anwers and parameters to grade the employee
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<AppraisalFormViewModel> GetAppraiserForm(int userId)
        {
            var appraisalFormViewModel = new AppraisalFormViewModel();
            var param = new AppraiserForm();
            return await Task.Run(() =>
            {
                var personGrade = service.Top<PersonEmployment>(0, x => x.PersonID == userId).FirstOrDefault().Designation.Grade;
                appraisalFormViewModel.AppraiseForm = Mapper.Map<List<AppraiseForm>, List<AppraiseeFormModel>>(service.Top<AppraiseForm>(0, x => x.PersonID == userId && x.AppraiserYear == DateTime.Now.Year).ToList());
                appraisalFormViewModel.AppraiserParameters = Mapper.Map<List<ApprisalParameters>, List<AppraiseeParametersViewModel>>(service.Top<ApprisalParameters>(0, x => x.Levels == personGrade && x.IsDelete == false).ToList());
                foreach (var paramter in appraisalFormViewModel.AppraiserParameters)
                {
                    param = service.First<AppraiserForm>(x => x.PersonID == userId && x.ParameterID == paramter.ID && x.AppraiserYear == DateTime.Now.Year);
                    if (param != null)
                    {
                        paramter.Score = Convert.ToInt32(param.AppraiserScore);
                    }
                    else
                    {
                        paramter.Score = 0;
                    }
                }
                var appraisalReporting = service.Top<AppraisalReporting>(0, x => x.PersonID == userId && x.AppraiserYear == DateTime.Now.Year).FirstOrDefault();
                var reviewerComment = service.Top<ReviewerComment>(0, x => x.AppraisalReportingId == appraisalReporting.ID).FirstOrDefault();
                if (reviewerComment != null)
                {
                    appraisalFormViewModel.OrgCategoryId = reviewerComment.OrgCategoryId;
                    var organizationCategory = service.First<OrganizationCategory>(x => x.Id == reviewerComment.OrgCategoryId);
                    if (organizationCategory != null)
                    {
                        appraisalFormViewModel.OrgCategoryName = organizationCategory.CategoryName;
                    }
                    appraisalFormViewModel.OrgCategoryDescription = reviewerComment.OrgCategoryDescription;
                }
                appraisalFormViewModel.ReviewerParameters = Mapper.Map<List<ApprisalParameters>, List<AppraiseeParametersViewModel>>(service.Top<ApprisalParameters>(0, x => x.Levels == personGrade && x.IsDelete == false).ToList());
                foreach (var paramter in appraisalFormViewModel.ReviewerParameters)
                {
                    param = service.First<AppraiserForm>(x => x.PersonID == userId && x.ParameterID == paramter.ID && x.AppraiserYear == DateTime.Now.Year);
                    if (param != null)
                    {
                        paramter.Score = Convert.ToInt32(param.ReviewerScore);
                    }
                    else
                    {
                        paramter.Score = 0;
                    }
                }
                appraisalFormViewModel.Status = service.Top<AppraisalReporting>(0, x => x.PersonID == userId && x.AppraiserYear == DateTime.Now.Year).FirstOrDefault().Status.Value;
                appraisalFormViewModel.AppraiserComments = appraisalReporting.AppraiserComments;
                appraisalFormViewModel.ReviewerComments = appraisalReporting.ReviewerComments;
                appraisalFormViewModel.OneToOneComments = appraisalReporting.OneToOneComment;
                appraisalFormViewModel.OneToOneImprovementComment = appraisalReporting.OneToOneImprovementComment;
                appraisalFormViewModel.FinalReviewerRating = appraisalReporting.FinalReviewerRating;
                appraisalFormViewModel.ReviewerRating = appraisalReporting.ReviewerRating;
                appraisalFormViewModel.AppraiserRating = appraisalReporting.AppraiserRating;
                appraisalFormViewModel.IsPromotion = appraisalReporting.IsPromotion;
                appraisalFormViewModel.PromotionFor = appraisalReporting.PromotionFor;
                appraisalFormViewModel.IsPromotionByRiviwer = appraisalReporting.IsPromotionByRiviwer;
                appraisalFormViewModel.PromotionForByRiviwer = appraisalReporting.PromotionForByRiviwer;
                appraisalFormViewModel.IsTrainingRequired = appraisalReporting.IsTrainingRequired;
                appraisalFormViewModel.TrainingFor = appraisalReporting.TrainingFor;
                appraisalFormViewModel.IsCriticalForOrganize = appraisalReporting.IsCriticalForOrganize;
                appraisalFormViewModel.IsCriticalForProject = appraisalReporting.IsCriticalForProject;
                appraisalFormViewModel.CriticalForOrganizeFor = appraisalReporting.CriticalForOrganizeFor;
                appraisalFormViewModel.CriticalForProject = appraisalReporting.CriticalForProject;
                return appraisalFormViewModel;
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<AppraisalFormViewModel> GetReviewerForm(int userId)
        {
            var appraisalFormViewModel = new AppraisalFormViewModel();
            return await Task.Run(() =>
            {
                var personGrade = service.Top<PersonEmployment>(0, x => x.PersonID == userId).FirstOrDefault().Designation.Grade;
                appraisalFormViewModel.AppraiseForm = Mapper.Map<List<AppraiseForm>, List<AppraiseeFormModel>>(service.Top<AppraiseForm>(0, x => x.PersonID == userId && x.AppraiserYear == DateTime.Now.Year).ToList());
                appraisalFormViewModel.AppraiserParameters = Mapper.Map<List<ApprisalParameters>, List<AppraiseeParametersViewModel>>(service.Top<ApprisalParameters>(0, x => x.Levels == personGrade && x.IsDelete == false).ToList());
                foreach (var paramter in appraisalFormViewModel.AppraiserParameters)
                    paramter.Score = Convert.ToInt32(service.Top<AppraiserForm>(0, x => x.PersonID == userId && x.ParameterID == paramter.ID && x.AppraiserYear == DateTime.Now.Year).FirstOrDefault().AppraiserScore);
                var appraisalReporting = service.Top<AppraisalReporting>(0, x => x.PersonID == userId && x.AppraiserYear == DateTime.Now.Year).FirstOrDefault();
                appraisalFormViewModel.Status = appraisalReporting.Status.Value;
                appraisalFormViewModel.AppraiserComments = appraisalReporting.AppraiserComments;
                appraisalFormViewModel.AppraiserRating = appraisalReporting.AppraiserRating;
                appraisalFormViewModel.IsPromotion = appraisalReporting.IsPromotion;
                appraisalFormViewModel.PromotionFor = appraisalReporting.PromotionFor;
                appraisalFormViewModel.IsCriticalForOrganize = appraisalReporting.IsCriticalForOrganize;
                appraisalFormViewModel.IsCriticalForProject = appraisalReporting.IsCriticalForProject;
                appraisalFormViewModel.CriticalForOrganizeFor = appraisalReporting.CriticalForOrganizeFor;
                appraisalFormViewModel.CriticalForProject = appraisalReporting.CriticalForProject;
                return appraisalFormViewModel;
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<AppraisalFormViewModel> GetOneToOneForm(int userId)
        {
            var appraisalFormViewModel = new AppraisalFormViewModel();
            return await Task.Run(() =>
            {
                var personGrade = service.Top<PersonEmployment>(0, x => x.PersonID == userId).FirstOrDefault().Designation.Grade;
                appraisalFormViewModel.AppraiseForm = Mapper.Map<List<AppraiseForm>, List<AppraiseeFormModel>>(service.Top<AppraiseForm>(0, x => x.PersonID == userId && x.AppraiserYear == DateTime.Now.Year).ToList());
                appraisalFormViewModel.AppraiserParameters = Mapper.Map<List<ApprisalParameters>, List<AppraiseeParametersViewModel>>(service.Top<ApprisalParameters>(0, x => x.Levels == personGrade && x.IsDelete == false).ToList());
                foreach (var paramter in appraisalFormViewModel.AppraiserParameters)
                    paramter.Score = Convert.ToInt32(service.Top<AppraiserForm>(0, x => x.PersonID == userId && x.ParameterID == paramter.ID && x.AppraiserYear == DateTime.Now.Year).FirstOrDefault().AppraiserScore);
                var appraisalReporting = service.Top<AppraisalReporting>(0, x => x.PersonID == userId && x.AppraiserYear == DateTime.Now.Year).FirstOrDefault();
                appraisalFormViewModel.ReviewerParameters = Mapper.Map<List<ApprisalParameters>, List<AppraiseeParametersViewModel>>(service.Top<ApprisalParameters>(0, x => x.Levels == personGrade && x.IsDelete == false).ToList());
                foreach (var paramter in appraisalFormViewModel.ReviewerParameters)
                    paramter.Score = Convert.ToInt32(service.Top<AppraiserForm>(0, x => x.PersonID == userId && x.ParameterID == paramter.ID && x.AppraiserYear == DateTime.Now.Year).FirstOrDefault().ReviewerScore);
                appraisalFormViewModel.Status = appraisalReporting.Status.Value;
                appraisalFormViewModel.AppraiserComments = appraisalReporting.AppraiserComments;
                appraisalFormViewModel.ReviewerComments = appraisalReporting.ReviewerComments;
                appraisalFormViewModel.FinalReviewerRating = appraisalReporting.FinalReviewerRating;
                appraisalFormViewModel.ReviewerRating = appraisalReporting.ReviewerRating;
                appraisalFormViewModel.AppraiserRating = appraisalReporting.AppraiserRating;
                appraisalFormViewModel.IsPromotion = appraisalReporting.IsPromotion;
                appraisalFormViewModel.PromotionFor = appraisalReporting.PromotionFor;
                appraisalFormViewModel.IsPromotionByRiviwer = appraisalReporting.IsPromotionByRiviwer;
                appraisalFormViewModel.PromotionForByRiviwer = appraisalReporting.PromotionForByRiviwer;
                appraisalFormViewModel.IsCriticalForOrganize = appraisalReporting.IsCriticalForOrganize;
                appraisalFormViewModel.IsCriticalForProject = appraisalReporting.IsCriticalForProject;
                appraisalFormViewModel.CriticalForOrganizeFor = appraisalReporting.CriticalForOrganizeFor;
                appraisalFormViewModel.CriticalForProject = appraisalReporting.CriticalForProject;
                return appraisalFormViewModel;
            });
        }

        /// <summary>
        /// Save Appraisee answers
        /// </summary>
        /// <param name="appraiseFormModel"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<int> SaveAppraiseeForm(List<AppraiseeAnswerModel> appraiseFormModel, int userId)
        {
            using (PhoenixEntities context = new PhoenixEntities())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        var appraisalReportingOld = service.Top<AppraisalReporting>(0, x => x.PersonID == userId && x.AppraiserYear == DateTime.Now.Year).FirstOrDefault();
                        if (appraisalReportingOld.IsFreezed == true) { return 0; }
                        appraisalReportingOld.Status = 1;
                        appraisalReportingOld.AssignedTo = appraisalReportingOld.AppraiserID;
                        await this.HookApproval(userId, appraisalReportingOld.ID, appraisalReportingOld.AppraiserID);

                        return await Task.Run(() =>
                        {                         
                            foreach (var form in appraiseFormModel)
                            {
                                var appraiseeAnswer = Mapper.Map<AppraiseeAnswerModel, AppraiseForm>(form);
                                var oldAppraiseAnswer = service.First<AppraiseForm>(x => x.ID == appraiseeAnswer.ID && x.PersonID == userId && x.AppraiserYear == DateTime.Now.Year);
                                if (oldAppraiseAnswer != null)
                                {
                                    var isUpdated = false;
                                    oldAppraiseAnswer.Answer = appraiseeAnswer.Answer;
                                    isUpdated = service.Update<AppraiseForm>(oldAppraiseAnswer);
                                }
                                else
                                {
                                    var isCreated = false;
                                    appraiseeAnswer.PersonID = userId;
                                    appraiseeAnswer.ID = 0;
                                    appraiseeAnswer.AppraiserYear = DateTime.Now.Year;
                                    isCreated = service.Create<AppraiseForm>(appraiseeAnswer, x => x.ID == 0);
                                }
                            };
                            service.Finalize(true);
                            transaction.Commit();
                            return 1;
                        });
                    }
                    catch
                    {
                        transaction.Rollback();
                        return 2;
                    }
                }
            }
        }

        /// <summary>
        /// Save Appraisee answers as Draft
        /// </summary>
        /// <param name="appraiseFormModel"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<int> DraftAppraiseeForm(List<AppraiseeAnswerModel> appraiseFormModel, int userId)
        {
            using (PhoenixEntities context = new PhoenixEntities())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        var appraisalReportingOld = service.Top<AppraisalReporting>(0, x => x.PersonID == userId && x.AppraiserYear == DateTime.Now.Year).FirstOrDefault();
                        if (appraisalReportingOld.IsFreezed == true) { return 0; }
                   
                        return await Task.Run(() =>
                        {
                            foreach (var form in appraiseFormModel)
                            {                                
                                var appraiseeAnswer = Mapper.Map<AppraiseeAnswerModel, AppraiseForm>(form);
                                var oldAppraiseAnswer = service.First<AppraiseForm>(x => x.ID == appraiseeAnswer.ID && x.PersonID == userId && x.AppraiserYear == DateTime.Now.Year);
                                if (oldAppraiseAnswer != null)
                                {
                                    var isUpdated = false;
                                    oldAppraiseAnswer.Answer = appraiseeAnswer.Answer;
                                    isUpdated = service.Update<AppraiseForm>(oldAppraiseAnswer);
                                }                                    
                                else
                                {
                                    var isCreated = false;
                                    appraiseeAnswer.PersonID = userId;
                                    appraiseeAnswer.ID = 0;
                                    appraiseeAnswer.AppraiserYear = DateTime.Now.Year;
                                    isCreated = service.Create<AppraiseForm>(appraiseeAnswer, x => x.ID == 0);
                                }
                            }

                            service.Finalize(true);
                            transaction.Commit();
                            return 1;
                        });
                    }
                    catch
                    {
                        transaction.Rollback();
                        return 2;
                    }
                }
            }
        }

        /// <summary>
        /// Save the appraiser and reviewers score
        /// </summary>
        /// <param name="appraiserForm"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<int> SaveAppraiserForm(AppraiserFormModel appraiserForm, int userId, int averageRating, int finalReviewerRating, decimal systemRaiting)
        {
            using (PhoenixEntities context = new PhoenixEntities())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        var appraisalReportingOld = service.Top<AppraisalReporting>(0, x => x.PersonID == appraiserForm.AppraiseeId && x.AppraiserYear == DateTime.Now.Year).FirstOrDefault();
                        var approvalOldRecord = service.Top<DTO.Approval>(0, x => x.RequestBy == appraiserForm.AppraiseeId && x.Status == 0 && x.RequestType == 7).FirstOrDefault();
                        if (appraisalReportingOld.Status == 2)
                        {
                            approvalOldRecord = service.Top<DTO.Approval>(0, x => x.RequestBy == appraiserForm.AppraiseeId && x.Status == 0 && x.RequestType == 7).FirstOrDefault();
                        }
                        else if (appraisalReportingOld.Status == 4)
                        {
                            approvalOldRecord = service.Top<DTO.Approval>(0, x => x.RequestBy == appraiserForm.AppraiseeId && x.Status == 0 && x.RequestType == 8).FirstOrDefault();
                        }
                        if (appraisalReportingOld.Status != 3)
                        {
                            var approvalDetailsOldRecord = service.Top<DTO.ApprovalDetail>(0, x => x.ApprovalID == approvalOldRecord.ID).FirstOrDefault();
                            var approvalStategy = ApprovalStrategyFactory.GetStrategy(ApprovalStrategy.OneLevelOnly, userId);
                            approvalStategy.opsService = this.service;
                        }
                        ApprovalService approvalService = new ApprovalService(this.service);
                        if (appraisalReportingOld.Status == 4)
                        {
                            var isApprovalUpdated = await approvalService.UpdateApproval(userId, 8, appraisalReportingOld.ID, 1, "Approved");
                        }
                        else
                        {
                            if (appraisalReportingOld.Status != 3)
                            {
                                var isApprovalUpdated = await approvalService.UpdateApproval(userId, 7, appraisalReportingOld.ID, 1, "Approved");
                            }
                        }
                        if (appraisalReportingOld.Status == 1 && (appraisalReportingOld.AppraiserID != appraisalReportingOld.ReviewerID))
                        {
                            await this.HookApproval(appraiserForm.AppraiseeId, appraisalReportingOld.ID, appraisalReportingOld.ReviewerID);
                        }
                        else if (appraisalReportingOld.Status == 3)
                        {
                            await this.HookApprovalOneToOne(appraiserForm.AppraiseeId, appraisalReportingOld.ID, appraisalReportingOld.AppraiserID);
                        }
                        var isAppraiserCreatedOrUpdated = await CreateOrUpdateAppraiserForm(appraisalReportingOld, appraiserForm, averageRating, finalReviewerRating, systemRaiting);
                        if (appraiserForm.OrgCategoryId != 0)
                        {
                            using (var entities = new PhoenixEntities())
                            {
                                var response = entities.ReviewerComment.Add(new ReviewerComment()
                                {
                                    AppraisalReportingId = appraisalReportingOld.ID,
                                    OrgCategoryId = appraiserForm.OrgCategoryId,
                                    OrgCategoryDescription = appraiserForm.OrgCategoryDescription
                                });
                                await entities.SaveChangesAsync();
                            }
                        }
                        return await Task.Run(() =>
                        {
                            int isUpdated = service.Finalize(isAppraiserCreatedOrUpdated);
                            return isUpdated;
                        });
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        return 0;
                    }
                }
            }
        }

        /// <summary>
        /// Creates or updates the Appraiser/Reveiwer score records
        /// </summary>
        /// <param name="appraisalReportingOld"></param>
        /// <param name="appraiserForm"></param>
        /// <returns></returns>
        private async Task<bool> CreateOrUpdateAppraiserForm(AppraisalReporting appraisalReportingOld, AppraiserFormModel appraiserForm, int averageRating, int finalReviewerRating, decimal systemRaiting)
        {
            return await Task.Run(() =>
           {
               var isAppraiserCreatedOrUpdated = false;
               if (appraisalReportingOld.Status < 3)
               {
                   foreach (var appraiserScoreForm in appraiserForm.Parameters)
                   {
                       var newAppraiserForm = Mapper.Map<AppraiseeParametersViewModel, AppraiserForm>(appraiserScoreForm);
                       newAppraiserForm.PersonID = appraiserForm.AppraiseeId;
                       var oldRecord = service.Top<AppraiserForm>(0, x => x.ParameterID == appraiserScoreForm.ID && x.PersonID == appraiserForm.AppraiseeId && x.AppraiserYear == DateTime.Now.Year).FirstOrDefault();
                       if (oldRecord != null)
                       {
                           newAppraiserForm.ID = oldRecord.ID;
                           newAppraiserForm.ReviewerScore = appraiserScoreForm.Score;
                           isAppraiserCreatedOrUpdated = service.Update<AppraiserForm>(newAppraiserForm, oldRecord);
                       }
                       else
                       {
                           newAppraiserForm.AppraiserScore = appraiserScoreForm.Score;
                           newAppraiserForm.AppraiserYear = DateTime.Now.Year;
                           if (appraisalReportingOld.AppraiserID == appraisalReportingOld.ReviewerID)
                               newAppraiserForm.ReviewerScore = appraiserScoreForm.Score;
                           else
                               newAppraiserForm.ReviewerScore = 0;
                           isAppraiserCreatedOrUpdated = service.Create<AppraiserForm>(newAppraiserForm, x => x.ID == 0);
                       }
                   }
               }
               if (appraisalReportingOld.Status == 1)
               {
                   appraisalReportingOld.AppraiserRating = averageRating;
                   appraisalReportingOld.AssignedTo = appraisalReportingOld.ReviewerID;
                   appraisalReportingOld.Status = 2;
                   appraisalReportingOld.AppraiserComments = appraiserForm.Comments;
                   appraisalReportingOld.IsPromotion = appraiserForm.IsPromotion;
                   appraisalReportingOld.PromotionFor = appraiserForm.PromotionFor;
                   appraisalReportingOld.AppraiserRatingBySystem = systemRaiting;
                   appraisalReportingOld.IsCriticalForOrganize = appraiserForm.IsCriticalForOrganize;
                   appraisalReportingOld.IsCriticalForProject = appraiserForm.IsCriticalForProject;
                   appraisalReportingOld.CriticalForOrganizeFor = appraiserForm.CriticalForOrganizeFor;
                   appraisalReportingOld.CriticalForProject = appraiserForm.CriticalForProject;
                   if (appraisalReportingOld.AppraiserID == appraisalReportingOld.ReviewerID)
                   {
                       appraisalReportingOld.ReviewerRating = averageRating;
                       appraisalReportingOld.FinalReviewerRating = finalReviewerRating;
                       appraisalReportingOld.Status = 3;
                       appraisalReportingOld.ReviewerComments = appraiserForm.Comments;
                       appraisalReportingOld.ReviewerRatingBySystem = systemRaiting;
                       appraisalReportingOld.IsPromotionByRiviwer = appraiserForm.IsPromotion;
                       appraisalReportingOld.PromotionForByRiviwer = appraiserForm.PromotionFor;
                   }
               }
               else if (appraisalReportingOld.Status == 2)
               {
                   appraisalReportingOld.ReviewerRating = averageRating;
                   appraisalReportingOld.FinalReviewerRating = finalReviewerRating;
                   appraisalReportingOld.Status = 3;
                   appraisalReportingOld.ReviewerRatingBySystem = systemRaiting;
                   appraisalReportingOld.ReviewerComments = appraiserForm.Comments;
                   appraisalReportingOld.IsPromotionByRiviwer = appraiserForm.IsPromotionByRiviwer;
                   appraisalReportingOld.PromotionForByRiviwer = appraiserForm.PromotionForByRiviwer;
                   appraisalReportingOld.IsCriticalForOrganize = appraiserForm.IsCriticalForOrganize;
                   appraisalReportingOld.IsCriticalForProject = appraiserForm.IsCriticalForProject;
                   appraisalReportingOld.CriticalForOrganizeFor = appraiserForm.CriticalForOrganizeFor;
                   appraisalReportingOld.CriticalForProject = appraiserForm.CriticalForProject;
               }
               else if (appraisalReportingOld.Status == 3)
               {
                   appraisalReportingOld.Status = 4;
                   appraisalReportingOld.FinalReviewerRating = finalReviewerRating;
                   isAppraiserCreatedOrUpdated = true;
               }
               else if (appraisalReportingOld.Status == 4)
               {
                   appraisalReportingOld.Status = 5;
                   appraisalReportingOld.OneToOneComment = appraiserForm.Comments;
                   appraisalReportingOld.OneToOneImprovementComment = appraiserForm.ImprovementComments;
                   appraisalReportingOld.IsTrainingRequired = appraiserForm.IsTrainingRequired;
                   appraisalReportingOld.TrainingFor = appraiserForm.TrainingFor;
                   isAppraiserCreatedOrUpdated = true;
               }

               return isAppraiserCreatedOrUpdated;
           });
        }

        /// <summary>
        /// get all the tickets for approval
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public async Task<IEnumerable<AppraisalListModel>> GetTicketsForApproval(int userID, int approvalFor)
        {
            return await Task.Run(() =>
            {
                IEnumerable<AppraisalReporting> appraisalForms;
                using (PhoenixEntities context = new PhoenixEntities())
                {
                    var approvalIds = QueryHelper.GetApprovalsForUser(userID, approvalFor);
                    appraisalForms = service.Top<AppraisalReporting>(0, a => approvalIds.Contains(a.ID) && a.AppraiserYear == DateTime.Now.Year).Where(x => x.IsFreezed == false);
                    // appraisalForms = appraisalForms.Where(x => x.IsFreezed == false);
                    var appraisalFormList = Mapper.Map<List<AppraisalReporting>, List<AppraisalListModel>>(appraisalForms.ToList());
                    foreach (var model in appraisalFormList)
                    {
                        model.EmployeeProfile = Mapper.Map<Person, EmployeeBasicProfile>(appraisalForms.Where(x => x.ID == model.ID).First().Person);
                        model.Grade = Convert.ToInt32(appraisalForms.Where(x => x.ID == model.ID).FirstOrDefault().Person.PersonEmployment.Where(x => x.PersonID == model.EmployeeProfile.ID).FirstOrDefault().Designation.Grade.Value);

                    }
                    return appraisalFormList;
                }
            });

        }

        /// <summary>
        /// Add entry to approval table when appraisee enters the form
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="recordID"></param>
        /// <param name="approverId"></param>
        /// <returns></returns>
        private async Task<int> HookApproval(int userId, int recordID, int approverId)
        {
            return await Task.Run(() =>
             {
                 var approvalStategy = ApprovalStrategyFactory.GetStrategy(ApprovalStrategy.OneLevelOnly, userId);
                 approvalStategy.opsService = this.service;
                 ApprovalService service = new ApprovalService(this.service);
                 int[] fetchedApprovers = new int[1];
                 fetchedApprovers[0] = approverId;
                 service.SendForApproval(userId, 7, recordID, fetchedApprovers);
                 return fetchedApprovers.FirstOrDefault();
             });
        }

        private async Task<int> HookApprovalOneToOne(int userId, int recordID, int approverId)
        {
            return await Task.Run(() =>
            {
                var approvalStategy = ApprovalStrategyFactory.GetStrategy(ApprovalStrategy.OneLevelOnly, userId);
                approvalStategy.opsService = this.service;
                ApprovalService service = new ApprovalService(this.service);
                int[] fetchedApprovers = new int[1];
                fetchedApprovers[0] = approverId;
                service.SendForApproval(userId, 8, recordID, fetchedApprovers);
                return fetchedApprovers.FirstOrDefault();
            });
        }

        public async Task<AppraisalEmployeeViewModel> GetAppraisalAssignedTo(int userId)
        {
            return await Task.Run(() =>
            {
                AppraisalEmployeeViewModel appraisalEmployeeViewModel = null;
                var employes = service.Top<AppraisalReporting>(0, x => x.PersonID == userId && x.IsInitiat == true && x.AppraiserYear == DateTime.Now.Year).FirstOrDefault();
                if (employes != null)
                {
                    appraisalEmployeeViewModel = Mapper.Map<AppraisalReporting, AppraisalEmployeeViewModel>(employes);

                    appraisalEmployeeViewModel.ReviewerImage = GetImage(appraisalEmployeeViewModel.ReviewerId);
                    appraisalEmployeeViewModel.AppraiserImage = GetImage(appraisalEmployeeViewModel.AppraiserId);
                }
                return appraisalEmployeeViewModel;
            });
        }

        public async Task<IEnumerable<AppraisalEmployeeViewModel>> GetAllEmployess(string EmpListFor)
        {
            return await Task.Run(() =>
            {
                using (PhoenixEntities context = new PhoenixEntities())
                {
                    if (EmpListFor == "eligibility" || EmpListFor == "activeemployee")
                    {
                        List<AppraisalEmployeeViewModel> _AppraisalEmployeeViewModelList = new List<AppraisalEmployeeViewModel>();
                        if (EmpListFor == "eligibility")
                        {
                            var eligibleEmpData = context.GetAllEligibleEmployees();
                            //var eligibleEmployeeViewModel = Mapper.Map<List<GetAllEligibleEmployees_Result>, List<AppraisalEmployeeViewModel>>(eligibleEmpData.ToList());

                            foreach (var item in eligibleEmpData)
                            {
                                AppraisalEmployeeViewModel _AppraisalEmployeeViewModel = new AppraisalEmployeeViewModel();
                                _AppraisalEmployeeViewModel.ID = item.ID.Value;
                                _AppraisalEmployeeViewModel.EmpID = item.PersonID;
                                _AppraisalEmployeeViewModel.EmpName = item.EmpName;
                                _AppraisalEmployeeViewModel.ReviewerId = item.ReviewerID.Value;
                                _AppraisalEmployeeViewModel.AppraiserId = item.AppraiserID.Value;
                                _AppraisalEmployeeViewModel.ReviewerName = item.ReviewerName;
                                _AppraisalEmployeeViewModel.AppraiserName = item.AppraiserName;
                                _AppraisalEmployeeViewModel.Location = item.Location;
                                _AppraisalEmployeeViewModel.LocationId = item.LocationId;
                                _AppraisalEmployeeViewModel.Grade = item.Grade;
                                _AppraisalEmployeeViewModel.FreezedComment = item.FreezedComments.ToString();
                                _AppraisalEmployeeViewModel.Designation = item.Designation;
                                _AppraisalEmployeeViewModel.Status = item.Status;
                                _AppraisalEmployeeViewModelList.Add(_AppraisalEmployeeViewModel);
                            }
                        }
                        else if (EmpListFor == "activeemployee")
                        {
                            var activeEmpData = context.GetAllActiveEmployee();
                            //var activeEmployeeViewModel = Mapper.Map<List<GetAllActiveEmployee_Result>, List<AppraisalEmployeeViewModel>>(activeEmpData.ToList());

                            foreach (var item in activeEmpData)
                            {
                                AppraisalEmployeeViewModel _AppraisalEmployeeViewModel = new AppraisalEmployeeViewModel();
                                _AppraisalEmployeeViewModel.ID = item.ID.Value;
                                _AppraisalEmployeeViewModel.EmpID = item.PersonID;
                                _AppraisalEmployeeViewModel.EmpName = item.EmpName;
                                _AppraisalEmployeeViewModel.ReviewerId = item.ReviewerID.Value;
                                _AppraisalEmployeeViewModel.AppraiserId = item.AppraiserID.Value;
                                _AppraisalEmployeeViewModel.ReviewerName = item.ReviewerName;
                                _AppraisalEmployeeViewModel.AppraiserName = item.AppraiserName;
                                _AppraisalEmployeeViewModel.Location = item.Location;
                                _AppraisalEmployeeViewModel.LocationId = item.LocationId;
                                _AppraisalEmployeeViewModel.Grade = item.Grade;
                                _AppraisalEmployeeViewModel.FreezedComment = item.FreezedComments.ToString();
                                _AppraisalEmployeeViewModel.Designation = item.Designation;
                                _AppraisalEmployeeViewModel.Status = item.Status;
                                _AppraisalEmployeeViewModelList.Add(_AppraisalEmployeeViewModel);
                            }
                            //return activeEmployeeViewModel;
                        }
                        return _AppraisalEmployeeViewModelList;
                    }
                    else
                    {
                        var employes = service.All<AppraisalReporting>().Where(x => x.IsInitiat == false && x.IsFreezed == false && x.AppraiserYear == DateTime.Now.Year);

                        if (EmpListFor == "initiated")
                        {
                            employes = service.All<AppraisalReporting>().Where(x => x.IsInitiat == true && x.AppraiserYear == DateTime.Now.Year && x.IsFreezed == false);
                        }
                        else if (EmpListFor == "freeze")
                        {
                            employes = service.All<AppraisalReporting>().Where(x => x.IsFreezed == true && x.AppraiserYear == DateTime.Now.Year);
                        }
                        var appraisalEmployeeViewModel = Mapper.Map<List<AppraisalReporting>, List<AppraisalEmployeeViewModel>>(employes.ToList());
                        return appraisalEmployeeViewModel;
                    }
                }
            });
        }

        public async Task<bool> InitiatEmployesAppraisal(int userId, List<AppraisalEmployeeViewModel> EmpList)
        {
            using (PhoenixEntities context = new PhoenixEntities())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        return await Task.Run(() =>
                        {
                            var approverPerson = service.First<Person>(x => x.ID == userId);
                            string approverName = approverPerson.FirstName + " " + approverPerson.LastName;

                            foreach (var item in EmpList)
                            {
                                var _AppraisalReporting = Mapper.Map<AppraisalEmployeeViewModel, AppraisalReporting>(item);
                                var empOldReport = service.Top<AppraisalReporting>(0, x => x.PersonID == item.EmpID && x.AppraiserYear == DateTime.Now.Year).FirstOrDefault();
                                var empPersonData = service.First<Person>(x => x.ID == item.EmpID);

                                if (empOldReport != null)
                                {
                                    if (empOldReport.Status == null || empOldReport.Status == 0)
                                    {
                                        var empNewReport = empOldReport;
                                        empNewReport.Status = 0;
                                        empNewReport.IsInitiat = true;
                                        empNewReport.AssignedTo = item.EmpID;
                                        empNewReport.AppraiserID = item.AppraiserId;
                                        empNewReport.ReviewerID = item.ReviewerId;

                                        if (item.AppraiserName.Contains("("))
                                        {
                                            empNewReport.AppraiserName = item.AppraiserName.Substring(0, item.AppraiserName.LastIndexOf(@"("));
                                        }
                                        else
                                        {
                                            empNewReport.AppraiserName = item.AppraiserName;
                                        }

                                        if (item.ReviewerName.Contains("("))
                                        {
                                            empNewReport.ReviewerName = item.ReviewerName.Substring(0, item.ReviewerName.LastIndexOf(@"("));
                                        }
                                        else
                                        {
                                            empNewReport.ReviewerName = item.ReviewerName;
                                        }

                                        empNewReport.FreezedComments = empOldReport.FreezedComments + " <br><b>Initiate by - " + approverName + ": </b>";
                                        empNewReport.IsEligible = true;
                                        empNewReport.IsPromotion = false;

                                        service.Update<AppraisalReporting>(empNewReport, empOldReport);
                                    }
                                }
                                else
                                {
                                    _AppraisalReporting.ID = 0;
                                    _AppraisalReporting.Status = 0;
                                    _AppraisalReporting.IsInitiat = true;
                                    _AppraisalReporting.AssignedTo = item.EmpID;
                                    _AppraisalReporting.AppraiserID = item.AppraiserId;
                                    _AppraisalReporting.ReviewerID = item.ReviewerId;

                                    if (item.AppraiserName.Contains("("))
                                    {
                                        _AppraisalReporting.AppraiserName = item.AppraiserName.Substring(0, item.AppraiserName.LastIndexOf(@"("));
                                    }
                                    else
                                    {
                                        _AppraisalReporting.AppraiserName = item.AppraiserName;
                                    }

                                    if (item.ReviewerName.Contains("("))
                                    {
                                        _AppraisalReporting.ReviewerName = item.ReviewerName.Substring(0, item.ReviewerName.LastIndexOf(@"("));
                                    }
                                    else
                                    {
                                        _AppraisalReporting.ReviewerName = item.ReviewerName;
                                    }

                                    _AppraisalReporting.FreezedComments = "<b>Initiate by - " + approverName + ": </b>";
                                    _AppraisalReporting.AppraiserYear = DateTime.Now.Year;
                                    _AppraisalReporting.PersonID = item.EmpID;
                                    _AppraisalReporting.IsEligible = true;
                                    _AppraisalReporting.IsPromotion = false;
                                    _AppraisalReporting.IsTrainingRequired = false;
                                    _AppraisalReporting.AppraiserRatingBySystem = 0;
                                    _AppraisalReporting.ReviewerRatingBySystem = 0;
                                    _AppraisalReporting.IsPromotionByRiviwer = false;
                                    _AppraisalReporting.IsFreezed = false;
                                    _AppraisalReporting.DeliveryUnit = empPersonData.PersonEmployment.FirstOrDefault().DeliveryUnit;
                                    _AppraisalReporting.DeliveyTeam = empPersonData.PersonEmployment.FirstOrDefault().DeliveryTeam;

                                    service.Create<AppraisalReporting>(_AppraisalReporting, x => x.ID == 0);
                                }
                                var requester = service.First<Person>(x => x.ID == item.EmpID);
                                emailService.SendAppraisalInitiat(approverPerson.PersonEmployment.First().OrganizationEmail, requester.PersonEmployment.First().OrganizationEmail);
                            }
                            service.Finalize(true);
                            transaction.Commit();
                            return true;
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

        public async Task<bool> FreezedEmployesAppraisal(int userId, List<AppraisalEmployeeViewModel> EmpList, int isFreezed)
        {
            using (PhoenixEntities context = new PhoenixEntities())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        return await Task.Run(() =>
                        {
                            foreach (var item in EmpList)
                            {
                                var empOldReport = service.Top<AppraisalReporting>(0, x => x.PersonID == item.EmpID && x.AppraiserYear == DateTime.Now.Year).FirstOrDefault();
                                if (empOldReport != null)
                                {
                                    var empNewReport = empOldReport;
                                    var approverPerson = service.First<Person>(x => x.ID == userId);
                                    string approverName = approverPerson.FirstName + " " + approverPerson.LastName;
                                    if (isFreezed == 1)
                                    {
                                        empNewReport.IsFreezed = true;
                                        empNewReport.FreezedComments = empOldReport.FreezedComments + " <br><b> Freezed by - " + approverName + ": </b>" + item.FreezedComment;
                                        if (empOldReport.IsInitiat == true)
                                        {
                                            var approval = new Pheonix.DBContext.Approval();
                                            approval = service.Top<Pheonix.DBContext.Approval>(0, x => x.RequestBy == empOldReport.PersonID && x.Status == 0 && (x.RequestType == 7 || x.RequestType == 8)).FirstOrDefault();
                                            if (approval != null)
                                            {
                                                var newApproval = approval;
                                                newApproval.IsDeleted = true;
                                                var approvalDetail = new ApprovalDetail();
                                                approvalDetail = service.Top<ApprovalDetail>(0, x => x.ApprovalID == approval.ID).FirstOrDefault();
                                                var newApprovalDetail = approvalDetail;
                                                newApprovalDetail.IsDeleted = true;
                                                service.Update<Pheonix.DBContext.Approval>(newApproval, approval);
                                                service.Update<ApprovalDetail>(newApprovalDetail, approvalDetail);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        empNewReport.IsFreezed = false;
                                        empNewReport.FreezedComments = empOldReport.FreezedComments + " <br><b>  Unfreezed by - " + approverName + ": </b>" + item.FreezedComment;
                                        if (empOldReport.IsInitiat == true)
                                        {
                                            var approval = new Pheonix.DBContext.Approval();
                                            approval = service.Top<Pheonix.DBContext.Approval>(0, x => x.RequestBy == empOldReport.PersonID && x.Status == 0 && (x.RequestType == 7 || x.RequestType == 8)).FirstOrDefault();
                                            if (approval != null)
                                            {
                                                var newApproval = approval;
                                                newApproval.IsDeleted = false;
                                                var approvalDetail = new ApprovalDetail();
                                                approvalDetail = service.Top<ApprovalDetail>(0, x => x.ApprovalID == approval.ID).FirstOrDefault();
                                                var newApprovalDetail = approvalDetail;
                                                newApprovalDetail.IsDeleted = false;
                                                service.Update<Pheonix.DBContext.Approval>(newApproval, approval);
                                                service.Update<ApprovalDetail>(newApprovalDetail, approvalDetail);
                                            }
                                        }
                                    }
                                    service.Update<AppraisalReporting>(empNewReport, empOldReport);
                                }
                                else//Added on 16/02/2018 -- To freez appraisal for Active & Eligible employee's but not initiated
                                {
                                    var approverPerson = service.First<Person>(x => x.ID == userId);
                                    string approverName = approverPerson.FirstName + " " + approverPerson.LastName;

                                    var _AppraisalReporting = Mapper.Map<AppraisalEmployeeViewModel, AppraisalReporting>(item);

                                    var empPersonData = service.First<Person>(x => x.ID == item.EmpID);

                                    if (isFreezed == 1)
                                    {
                                        _AppraisalReporting.IsFreezed = true;
                                        _AppraisalReporting.FreezedComments = _AppraisalReporting.FreezedComments + " <br><b> Freezed by - " + approverName + ": </b>" + item.FreezedComment;
                                        _AppraisalReporting.ID = 0;
                                        _AppraisalReporting.Status = 0;
                                        _AppraisalReporting.IsInitiat = false;
                                        _AppraisalReporting.AssignedTo = item.EmpID;
                                        _AppraisalReporting.AppraiserID = item.AppraiserId;
                                        _AppraisalReporting.ReviewerID = item.ReviewerId;

                                        if (item.AppraiserName.Contains("("))
                                        {
                                            _AppraisalReporting.AppraiserName = item.AppraiserName.Substring(item.AppraiserName.LastIndexOf(@"("), 0);
                                        }
                                        else
                                        {
                                            _AppraisalReporting.AppraiserName = item.AppraiserName;
                                        }

                                        if (item.ReviewerName.Contains("("))
                                        {
                                            _AppraisalReporting.ReviewerName = item.ReviewerName.Substring(item.ReviewerName.LastIndexOf(@"("), 0);
                                        }
                                        else
                                        {
                                            _AppraisalReporting.ReviewerName = item.ReviewerName;
                                        }

                                        _AppraisalReporting.AppraiserYear = DateTime.Now.Year;
                                        _AppraisalReporting.PersonID = item.EmpID;
                                        _AppraisalReporting.IsEligible = false;
                                        _AppraisalReporting.IsPromotion = false;
                                        _AppraisalReporting.IsTrainingRequired = false;
                                        _AppraisalReporting.AppraiserRatingBySystem = 0;
                                        _AppraisalReporting.ReviewerRatingBySystem = 0;
                                        _AppraisalReporting.IsPromotionByRiviwer = false;
                                        _AppraisalReporting.DeliveryUnit = empPersonData.PersonEmployment.FirstOrDefault().DeliveryUnit;
                                        _AppraisalReporting.DeliveyTeam = empPersonData.PersonEmployment.FirstOrDefault().DeliveryTeam;

                                        service.Create<AppraisalReporting>(_AppraisalReporting, x => x.ID == 0);

                                        var approval = new Pheonix.DBContext.Approval();
                                        approval = service.Top<Pheonix.DBContext.Approval>(0, x => x.RequestBy == _AppraisalReporting.PersonID && x.Status == 0 && (x.RequestType == 7 || x.RequestType == 8)).FirstOrDefault();
                                        if (approval != null)
                                        {
                                            var newApproval = approval;
                                            newApproval.IsDeleted = true;
                                            var approvalDetail = new ApprovalDetail();
                                            approvalDetail = service.Top<ApprovalDetail>(0, x => x.ApprovalID == approval.ID).FirstOrDefault();
                                            var newApprovalDetail = approvalDetail;
                                            newApprovalDetail.IsDeleted = true;
                                            service.Update<Pheonix.DBContext.Approval>(newApproval, approval);
                                            service.Update<ApprovalDetail>(newApprovalDetail, approvalDetail);
                                        }
                                    }
                                }
                            }
                            service.Finalize(true);
                            transaction.Commit();
                            return true;
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

        public async Task<List<DropdownItems>> GetManagerDropdowns()
        {
            return await Task.Run(() =>
            {
                List<DropdownItems> lstItems = new List<DropdownItems>();

                var managers = service.All<AppraisalReporting>().Where(m => m.Grade >= 3 && m.AppraiserYear == DateTime.Now.Year).OrderBy(x => x.EmpName);
                var personName = service.All<Person>().Where(per => per.ID != 0);
                foreach (var item in managers)
                {
                    var executive = personName.Where(pn => pn.ID == item.PersonID).FirstOrDefault();
                    DropdownItems dropdownItem = new DropdownItems
                    {
                        ID = item.PersonID,
                        Text = String.Format("{0} {1}", executive.FirstName, executive.LastName)
                    };
                    lstItems.Add(dropdownItem);
                }
                return lstItems;
            });
        }

        public async Task<AppraisalFormViewModel> GetAppraiseFormDetail(int userId)
        {
            var appraisalFormViewModel = new AppraisalFormViewModel();
            return await Task.Run(() =>
            {
                var apprasee = service.First<AppraisalReporting>(x => x.PersonID == userId && x.AppraiserYear == DateTime.Now.Year);
                appraisalFormViewModel.AppraiseForm = Mapper.Map<List<AppraiseForm>, List<AppraiseeFormModel>>(service.Top<AppraiseForm>(0, x => x.PersonID == userId && x.AppraiserYear == DateTime.Now.Year).ToList());
                return appraisalFormViewModel;
            });
        }

        public async Task<IEnumerable<AppraisalListModel>> GetTicketsHistoryOfApproval(int userId, int historyof)
        {
            return await Task.Run(() =>
            {
                IEnumerable<AppraisalReporting> appraisalForms;
                using (PhoenixEntities context = new PhoenixEntities())
                {
                    var approvalIds = (from d in context.ApprovalDetail
                                       join a in context.Approval
                                       on d.ApprovalID equals a.ID
                                       where d.ApproverID == userId && a.RequestType == historyof && (a.Status == 1)
                                       select a.RequestID.HasValue ? a.RequestID.Value : -1).ToList();
                    appraisalForms = service.Top<AppraisalReporting>(0, a => approvalIds.Contains(a.ID) && a.AppraiserYear == DateTime.Now.Year);
                    if (historyof == 7)
                    {
                        appraisalForms = appraisalForms.Where(x => (x.Status != 4 && x.Status != 5) || x.ReviewerID == userId);
                    }
                    var appraisalFormList = Mapper.Map<List<AppraisalReporting>, List<AppraisalListModel>>(appraisalForms.ToList());
                    foreach (var model in appraisalFormList)
                    {
                        model.EmployeeProfile = Mapper.Map<Person, EmployeeBasicProfile>(appraisalForms.Where(x => x.ID == model.ID).First().Person);
                        model.Grade = Convert.ToInt32(appraisalForms.Where(x => x.ID == model.ID).FirstOrDefault().Person.PersonEmployment.Where(x => x.PersonID == model.EmployeeProfile.ID).FirstOrDefault().Designation.Grade.Value);

                    }
                    return appraisalFormList;
                }
            });
        }

        public async Task<IEnumerable<AppraisalReportViewModel>> GetAppraisalReport(int? year)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (PhoenixEntities context = new PhoenixEntities())
                    {
                        var appraisalYearList = context.GetAppraisalReportByYear(year).ToList();
                        List<AppraisalReportViewModel> appraisalYearModelData = Mapper.Map<List<GetAppraisalReportByYear_Result>, List<AppraisalReportViewModel>>(appraisalYearList);
                        return appraisalYearModelData;
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            });
        }


        public async Task<AppraisalSummaryModel> GetAppraisalSummary(int? rating, int? delivertUnit, int? deliveryTeam, int? year)
        {
            return await Task.Run(() =>
            {
                AppraisalSummaryModel model = new AppraisalSummaryModel();

                List<GetAppraisalSummary_Result> alldata = QueryHelper.GetAppraisalSummary(delivertUnit, deliveryTeam, rating, year);
                var mappeddata = Mapper.Map<IEnumerable<GetAppraisalSummary_Result>, IEnumerable<PersonApraisal>>(alldata);
                model.oneStar = mappeddata.Where(x => x.reviewerRating == 1);
                model.twoStar = mappeddata.Where(x => x.reviewerRating == 2);
                model.threeStar = mappeddata.Where(x => x.reviewerRating == 3);
                model.fourStar = mappeddata.Where(x => x.reviewerRating == 4);
                model.fiveStar = mappeddata.Where(x => x.reviewerRating == 5);

                return model;
            });

        }

        public async Task<IEnumerable<AppraisalReportViewModel>> GetNegotiationDetails()
        {
            var appraisalReportViewModel = new List<AppraisalReportViewModel>();
            return await Task.Run(() =>
            {
                var reportDetails = service.Top<AppraisalReporting>(0, x => x.IsInitiat == true && x.IsFreezed == false && x.Status == 3 && x.AppraiserYear == DateTime.Now.Year).ToList();
                foreach (var item in reportDetails)
                {
                    AppraisalReportViewModel report = new AppraisalReportViewModel
                    {
                        Status = item.Status,
                        EmpId = item.PersonID,
                        EmpName = item.EmpName,
                        AppraiserComments = item.AppraiserComments,
                        ReviewerComments = item.ReviewerComments,
                        OneToOneComments = item.OneToOneComment,
                        AppraiserRating = item.AppraiserRating,
                        ReviewerRating = item.ReviewerRating,
                        OneToOneRating = item.FinalReviewerRating,
                        Grade = item.Grade,
                        IsPromotion = item.IsPromotion,
                        PromotionFor = item.PromotionFor,
                        AppraiserName = item.AppraiserName,
                        ReviewerName = item.ReviewerName,
                        Location = item.Location,
                        AppraiserRatingBySystem = item.AppraiserRatingBySystem,
                        ReviewerRatingBySystem = item.ReviewerRatingBySystem,
                        IsPromotionByRiviwer = item.IsPromotionByRiviwer,
                        PromotionForByRiviwer = item.PromotionForByRiviwer,
                        IsTrainingRequired = item.IsTrainingRequired,
                        TrainingFor = item.TrainingFor,
                        DeliveyTeam = item.DeliveyTeam,
                        ProjectName = item.ProjectName,
                        PromotionforByNorm = item.PromotionforByNorm,
                        IsPromotionNorm = item.IsPromotionNorm
                    };
                    appraisalReportViewModel.Add(report);
                }
                return appraisalReportViewModel;
            });
        }

        public async Task<bool> NormalizedEmployesAppraisal(int userId, List<AppraisalReportViewModel> EmpList)
        {
            using (PhoenixEntities context = new PhoenixEntities())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        var approverPerson = service.First<Person>(x => x.ID == userId);
                        string approverName = approverPerson.FirstName + " " + approverPerson.LastName;
                        foreach (var item in EmpList)
                        {
                            var appraisalReportingOld = service.Top<AppraisalReporting>(0, x => x.PersonID == item.EmpId && x.AppraiserYear == DateTime.Now.Year).FirstOrDefault();
                            var appraisalReportingNew = appraisalReportingOld;
                            appraisalReportingNew.Status = 4;
                            appraisalReportingNew.FinalReviewerRating = item.OneToOneRating;
                            appraisalReportingNew.IsPromotionNorm = item.IsPromotionNorm;
                            appraisalReportingNew.PromotionforByNorm = item.PromotionforByNorm;
                            appraisalReportingNew.FreezedComments = "<b>Normalized by - " + approverName + ": </b>";
                            await this.HookApprovalOneToOne(appraisalReportingOld.PersonID, appraisalReportingOld.ID, appraisalReportingOld.AppraiserID);
                            service.Update<AppraisalReporting>(appraisalReportingNew, appraisalReportingOld);
                        }
                        return await Task.Run(() =>
                        {
                            service.Finalize(true);
                            transaction.Commit();
                            return true;
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

        public async Task<IEnumerable<AppraiseeQuestion>> GetAllQuestions()
        {
            var questionModel = new List<AppraiseeQuestion>();
            return await Task.Run(() =>
            {
                var questionsDetails = service.All<ApprisalQuestions>().ToList().Where(x => x.IsDelete == false).OrderBy(x => x.Levels);
                var appraisalList = service.All<AppraisalReporting>().ToList().Where(x => x.AppraiserYear == DateTime.Now.Year).OrderBy(x => x.PersonID);
                foreach (var item in questionsDetails)
                {
                    var count = appraisalList.Where(x => x.Grade == item.Levels && x.IsInitiat == true).Count();
                    var isEdit = true;
                    if (count > 0)
                        isEdit = false;
                    AppraiseeQuestion questions = new AppraiseeQuestion
                    {
                        ID = item.ID,
                        Question = item.Questions,
                        Levels = item.Levels,
                        Sequence = item.Sequence,
                        IsEditeble = isEdit
                    };
                    questionModel.Add(questions);
                }
                return questionModel;
            });
        }

        public async Task<bool> AddAllQuestions(List<AppraiseeQuestion> questions)
        {
            using (PhoenixEntities context = new PhoenixEntities())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (var item in questions)
                        {
                            var tempdata = new ApprisalQuestions();
                            var question = service.Top<ApprisalQuestions>(0, x => x.Levels == item.Levels).ToList();
                            var sequence = question.OrderBy(a => a.Sequence).LastOrDefault();
                            tempdata.Sequence = sequence == null ? 1 : sequence.Sequence + 1;
                            tempdata.Levels = item.Levels;
                            tempdata.Questions = item.Question;
                            tempdata.IsDelete = false;
                            service.Create<ApprisalQuestions>(tempdata, x => x.ID == 0);
                        }
                        return await Task.Run(() =>
                        {
                            service.Finalize(true);
                            transaction.Commit();
                            return true;
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

        public async Task<bool> UpdateAllQuestions(int isDelete, AppraiseeQuestion questions)
        {
            using (PhoenixEntities context = new PhoenixEntities())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        var question = service.Top<ApprisalQuestions>(0, x => x.ID == questions.ID).FirstOrDefault();
                        var newQuestion = question;
                        if (isDelete == 0)
                        {
                            newQuestion.Levels = questions.Levels;
                            newQuestion.Questions = questions.Question;
                        }
                        else
                        {
                            newQuestion.IsDelete = true;
                        }
                        var isUpdate = service.Update<ApprisalQuestions>(newQuestion, question);
                        return await Task.Run(() =>
                        {
                            service.Finalize(isUpdate);
                            transaction.Commit();
                            return true;
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

        public async Task<IEnumerable<AppraiseeParametersViewModel>> GetAllParameters()
        {
            var parameterModel = new List<AppraiseeParametersViewModel>();
            return await Task.Run(() =>
            {
                var parametersDetails = service.All<ApprisalParameters>().ToList().Where(x => x.IsDelete == false).OrderBy(x => x.Levels);
                var appraisalList = service.All<AppraisalReporting>().ToList().Where(x => x.AppraiserYear == DateTime.Now.Year).OrderBy(x => x.PersonID);
                foreach (var item in parametersDetails)
                {
                    var count = appraisalList.Where(x => x.Grade == item.Levels && x.IsInitiat == true).Count();
                    var isEdit = true;
                    if (count > 0)
                        isEdit = false;
                    AppraiseeParametersViewModel parameters = new AppraiseeParametersViewModel
                    {
                        ID = item.ID,
                        Parameter = item.Parameter,
                        Weightage = item.Weightage,
                        Sequence = item.Sequence,
                        Levels = item.Levels,
                        IsEditeble = isEdit
                    };
                    parameterModel.Add(parameters);
                }
                return parameterModel;
            });
        }

        public async Task<bool> AddAllParameters(AppraiseeParametersViewModel parameter)
        {
            using (PhoenixEntities context = new PhoenixEntities())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        var tempdata = new ApprisalParameters();
                        var parameters = service.Top<ApprisalParameters>(0, x => x.Levels == parameter.Levels).ToList();
                        var sequence = parameters.OrderBy(a => a.Sequence).LastOrDefault();
                        tempdata.Sequence = sequence == null ? 1 : sequence.Sequence + 1;
                        tempdata.Levels = parameter.Levels;
                        tempdata.Parameter = parameter.Parameter;
                        tempdata.IsDelete = false;
                        tempdata.Weightage = parameter.Weightage;
                        var isCreate = service.Create<ApprisalParameters>(tempdata, x => x.ID == 0);
                        return await Task.Run(() =>
                        {
                            service.Finalize(isCreate);
                            transaction.Commit();
                            return true;
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

        public async Task<bool> UpdateAllParameters(int isDelete, AppraiseeParametersViewModel parameters)
        {
            using (PhoenixEntities context = new PhoenixEntities())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        var parameter = service.Top<ApprisalParameters>(0, x => x.ID == parameters.ID).FirstOrDefault();
                        var newParameter = parameter;
                        if (isDelete == 0)
                        {
                            newParameter.Levels = parameters.Levels;
                            newParameter.Parameter = parameters.Parameter;
                            newParameter.Weightage = parameters.Weightage;
                        }
                        else
                        {
                            newParameter.IsDelete = true;
                        }
                        var isUpdate = service.Update<ApprisalParameters>(newParameter, parameter);
                        return await Task.Run(() =>
                        {
                            service.Finalize(isUpdate);
                            transaction.Commit();
                            return true;
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

        public async Task<AppraisalFormViewModel> GetQuesitionsParameters(int level)
        {
            var model = new AppraisalFormViewModel();
            return await Task.Run(() =>
            {
                var parametersDetails = service.All<ApprisalParameters>().ToList().Where(x => x.IsDelete == false && x.Levels == level).OrderBy(x => x.Levels);
                var questionsDetails = service.All<ApprisalQuestions>().ToList().Where(x => x.IsDelete == false && x.Levels == level).OrderBy(x => x.Levels);
                var questionsModel = new List<AppraiseeFormModel>();
                var parametersModel = new List<AppraiseeParametersViewModel>();
                foreach (var item in questionsDetails)
                {
                    AppraiseeFormModel parameters = new AppraiseeFormModel
                    {
                        ID = item.ID,
                        Question = item.Questions
                    };
                    questionsModel.Add(parameters);
                }
                foreach (var item in parametersDetails)
                {
                    AppraiseeParametersViewModel parameter = new AppraiseeParametersViewModel
                    {
                        ID = item.ID,
                        Parameter = item.Parameter,
                        Weightage = item.Weightage
                    };
                    parametersModel.Add(parameter);
                }
                model.AppraiseForm = questionsModel;
                model.AppraiserParameters = parametersModel;
                return model;
            });
        }

        public async Task<List<rpt_AppraisalCurrentStatus_Result>> GetAppraisalCurrentStatus(int year, int location)
        {
            return await Task.Run(() =>
            {
                using (PhoenixEntities context = new PhoenixEntities())
                {
                    List<rpt_AppraisalCurrentStatus_Result> data = context.rpt_AppraisalCurrentStatus(year, location).ToList();
                    return data;
                }
            });
        }

        public async Task<AppraisalFormViewModel> GetAppraiseeFinalReport(int userId)
        {
            var appraisalFormViewModel = new AppraisalFormViewModel();
            var param = new AppraiserForm();
            return await Task.Run(() =>
            {
                var personGrade = service.Top<PersonEmployment>(0, x => x.PersonID == userId).FirstOrDefault().Designation.Grade;
                appraisalFormViewModel.AppraiseForm = Mapper.Map<List<AppraiseForm>, List<AppraiseeFormModel>>(service.Top<AppraiseForm>(0, x => x.PersonID == userId && x.AppraiserYear == DateTime.Now.Year).ToList());
                appraisalFormViewModel.AppraiserParameters = Mapper.Map<List<ApprisalParameters>, List<AppraiseeParametersViewModel>>(service.Top<ApprisalParameters>(0, x => x.Levels == personGrade && x.IsDelete == false).ToList());
                var appraisalReporting = service.Top<AppraisalReporting>(0, x => x.PersonID == userId && x.AppraiserYear == DateTime.Now.Year).FirstOrDefault();
                appraisalFormViewModel.ReviewerParameters = Mapper.Map<List<ApprisalParameters>, List<AppraiseeParametersViewModel>>(service.Top<ApprisalParameters>(0, x => x.Levels == personGrade && x.IsDelete == false).ToList());
                foreach (var paramter in appraisalFormViewModel.ReviewerParameters)
                {
                    param = service.First<AppraiserForm>(x => x.PersonID == userId && x.ParameterID == paramter.ID && x.AppraiserYear == DateTime.Now.Year);
                    if (param != null)
                    {
                        paramter.Score = Convert.ToInt32(param.ReviewerScore);
                    }
                    else
                    {
                        paramter.Score = 0;
                    }
                }
                appraisalFormViewModel.Status = service.Top<AppraisalReporting>(0, x => x.PersonID == userId && x.AppraiserYear == DateTime.Now.Year).FirstOrDefault().Status.Value;
                appraisalFormViewModel.OneToOneComments = appraisalReporting.OneToOneComment;
                appraisalFormViewModel.FinalReviewerRating = appraisalReporting.FinalReviewerRating;
                appraisalFormViewModel.IsPromotionByRiviwer = appraisalReporting.IsPromotionByRiviwer;
                appraisalFormViewModel.PromotionForByRiviwer = appraisalReporting.PromotionForByRiviwer;
                appraisalFormViewModel.IsTrainingRequired = appraisalReporting.IsTrainingRequired;
                appraisalFormViewModel.TrainingFor = appraisalReporting.TrainingFor;
                return appraisalFormViewModel;
            });
        }

        /// <summary>
        /// To fetch year wise appraisal data for the respective user: Rahul R
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        public async Task<AppraisalFormViewModel> GetAppraiseeFinalReport(int userId, int year)
        {
            var appraisalFormViewModel = new AppraisalFormViewModel();
            var param = new AppraiserForm();
            return await Task.Run(() =>
            {
                var appraisalReporting = service.Top<AppraisalReporting>(0, x => x.PersonID == userId && x.AppraiserYear == year && x.Status == 5).FirstOrDefault();
                var personGrade = appraisalReporting.Grade; //service.Top<PersonEmployment>(0, x => x.PersonID == userId).FirstOrDefault().Designation.Grade;
                appraisalFormViewModel.AppraiseForm = Mapper.Map<List<AppraiseForm>, List<AppraiseeFormModel>>(service.Top<AppraiseForm>(0, x => x.PersonID == userId && x.AppraiserYear == year).ToList());
                appraisalFormViewModel.AppraiserParameters = Mapper.Map<List<ApprisalParameters>, List<AppraiseeParametersViewModel>>(service.Top<ApprisalParameters>(0, x => x.Levels == personGrade && x.IsDelete == false).ToList());

                appraisalFormViewModel.ReviewerParameters = Mapper.Map<List<ApprisalParameters>, List<AppraiseeParametersViewModel>>(service.Top<ApprisalParameters>(0, x => x.Levels == personGrade && x.IsDelete == false).ToList());
                foreach (var paramter in appraisalFormViewModel.ReviewerParameters)
                {
                    param = service.First<AppraiserForm>(x => x.PersonID == userId && x.ParameterID == paramter.ID && x.AppraiserYear == year);
                    if (param != null)
                    {
                        paramter.Score = Convert.ToInt32(param.ReviewerScore);
                    }
                    else
                    {
                        paramter.Score = 0;
                    }
                }
                appraisalFormViewModel.Status = service.Top<AppraisalReporting>(0, x => x.PersonID == userId && x.AppraiserYear == year).FirstOrDefault().Status.Value;
                appraisalFormViewModel.OneToOneComments = appraisalReporting.OneToOneComment;
                appraisalFormViewModel.OneToOneImprovementComment = appraisalReporting.OneToOneImprovementComment;
                appraisalFormViewModel.FinalReviewerRating = appraisalReporting.FinalReviewerRating;
                appraisalFormViewModel.IsPromotionByRiviwer = appraisalReporting.IsPromotionByRiviwer;
                appraisalFormViewModel.PromotionForByRiviwer = appraisalReporting.PromotionForByRiviwer;
                appraisalFormViewModel.IsTrainingRequired = appraisalReporting.IsTrainingRequired;
                appraisalFormViewModel.TrainingFor = appraisalReporting.TrainingFor;
                appraisalFormViewModel.AppraiserComments = appraisalReporting.AppraiserComments;
                appraisalFormViewModel.ReviewerComments = appraisalReporting.ReviewerComments;
                return appraisalFormViewModel;
            });
        }

        /// <summary>
        /// To fetch the yeas of appraisal faced by the respective user: Rahul R
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public Task<List<int>> GetAppraisalYears(int userId)
        {
            var y = new List<AppraisalReporting>();
            List<int> years = new List<int>();
            return Task.Run(() =>
            {
                y = service.All<AppraisalReporting>().Where(x => x.PersonID == userId && x.Status == 5).OrderBy(x => x.AppraiserYear).ToList();
                years = y.AsEnumerable().Select(s => Convert.ToInt32(s.AppraiserYear)).ToList();
                return years;
            });
        }


        /// <summary>
        /// To fetch the yeas of appraisal carried out in the system: Rahul R
        /// </summary>
        /// <returns></returns>
        public Task<List<int>> GetAppraisalYears()
        {
            var y = new List<AppraisalReporting>();
            List<int> years = new List<int>();
            return Task.Run(() =>
            {
                y = service.All<AppraisalReporting>().OrderBy(x => x.AppraiserYear).ToList();
                years = y.AsEnumerable().Select(s => Convert.ToInt32(s.AppraiserYear)).Distinct().ToList();
                return years;
            });
        }

        //This will update record of Initiated appraisal 
        public async Task<bool> UpdateEmployesAppraisal(int userId, List<AppraisalEmployeeViewModel> EmpList)
        {
            using (PhoenixEntities context = new PhoenixEntities())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        return await Task.Run(() =>
                        {
                            var approverPerson = service.First<Person>(x => x.ID == userId);
                            string approverName = approverPerson.FirstName + " " + approverPerson.LastName;

                            foreach (var item in EmpList)
                            {

                                var empOldReport = service.Top<AppraisalReporting>(0, x => x.PersonID == item.EmpID && x.AppraiserYear == DateTime.Now.Year).FirstOrDefault();
                                if (empOldReport.Status == null || empOldReport.Status == 0)
                                {
                                    var empNewReport = empOldReport;
                                    empNewReport.Status = 0;
                                    empNewReport.IsInitiat = true;
                                    empNewReport.AssignedTo = item.EmpID;
                                    empNewReport.AppraiserID = item.AppraiserId;
                                    empNewReport.ReviewerID = item.ReviewerId;

                                    if (item.AppraiserName.Contains("("))
                                    {
                                        empNewReport.AppraiserName = item.AppraiserName.Substring(0, item.AppraiserName.LastIndexOf(@"("));
                                    }
                                    else
                                    {
                                        empNewReport.AppraiserName = item.AppraiserName;
                                    }

                                    if (item.ReviewerName.Contains("("))
                                    {
                                        empNewReport.ReviewerName = item.ReviewerName.Substring(0, item.ReviewerName.LastIndexOf(@"("));
                                    }
                                    else
                                    {
                                        empNewReport.ReviewerName = item.ReviewerName;
                                    }

                                    empNewReport.FreezedComments = empOldReport.FreezedComments + " <br><b>Initiat by - " + approverName + ": </b>";
                                    service.Update<AppraisalReporting>(empNewReport, empOldReport);
                                }

                                var requester = service.First<Person>(x => x.ID == item.EmpID);
                                emailService.SendAppraisalInitiat(approverPerson.PersonEmployment.First().OrganizationEmail, requester.PersonEmployment.First().OrganizationEmail);
                            }
                            service.Finalize(true);
                            transaction.Commit();
                            return true;
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

        public HttpResponseMessage AppraisalReport(string location, int? status, int? grade, int? empID, int? year)
        {
            List<rpt_AppraisalReport_Result> AppraisalSummaryList = QueryHelper.GetAppraisalReport(location, status, grade, empID, year);
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            var AppraisalList = new StringBuilder();
            for (int i = 0; i < AppraisalSummaryList.Count;)
            {
                JObject json = JObject.FromObject(AppraisalSummaryList[i]);
                foreach (var property in json)
                {
                    string key = property.Key.ToString();
                    key = key.Replace('_', ' ');
                    AppraisalList.Append("\"" + key + "\",");
                }
                AppraisalList.Append("\r\n");
                break;
            }
            for (var i = 0; i < AppraisalSummaryList.Count; i++)
            {
                JObject json = JObject.FromObject(AppraisalSummaryList[i]);
                foreach (var property in json)
                {
                    if (property.Value.Type == JTokenType.Date)
                    {
                        AppraisalList.AppendFormat("\"{0}\",", Convert.ToDateTime(property.Value).ToString("MM/dd/yyyy"));
                    }
                    else
                    {
                        AppraisalList.AppendFormat("\"{0}\",", property.Value);
                    }

                }
                AppraisalList.AppendFormat("\r\n");

            }
            response.Content = new StringContent(AppraisalList.ToString());
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment"); //attachment will force download
            response.Content.Headers.ContentDisposition.FileName = "Appraisal.csv";
            return response;
        }

        //To get Normalized history detail for current year
        public async Task<IEnumerable<AppraisalReportViewModel>> GetNegotiationHistoryDetails()
        {
            var appraisalReportViewModel = new List<AppraisalReportViewModel>();
            return await Task.Run(() =>
            {
                var reportDetails = service.Top<AppraisalReporting>(0, x => x.IsInitiat == true && x.IsFreezed == false && x.Status >= 4 && x.AppraiserYear == DateTime.Now.Year).ToList();
                foreach (var item in reportDetails)
                {
                    AppraisalReportViewModel report = new AppraisalReportViewModel
                    {
                        Status = item.Status,
                        EmpId = item.PersonID,
                        EmpName = item.EmpName,
                        AppraiserComments = item.AppraiserComments,
                        ReviewerComments = item.ReviewerComments,
                        OneToOneComments = item.OneToOneComment,
                        AppraiserRating = item.AppraiserRating,
                        ReviewerRating = item.ReviewerRating,
                        OneToOneRating = item.FinalReviewerRating,
                        Grade = item.Grade,
                        IsPromotion = item.IsPromotion,
                        PromotionFor = item.PromotionFor,
                        AppraiserName = item.AppraiserName,
                        ReviewerName = item.ReviewerName,
                        Location = item.Location,
                        AppraiserRatingBySystem = item.AppraiserRatingBySystem,
                        ReviewerRatingBySystem = item.ReviewerRatingBySystem,
                        IsPromotionByRiviwer = item.IsPromotionByRiviwer,
                        PromotionForByRiviwer = item.PromotionForByRiviwer,
                        IsPromotionNorm = item.IsPromotionNorm,
                        PromotionforByNorm = item.PromotionforByNorm,
                        IsTrainingRequired = item.IsTrainingRequired,
                        TrainingFor = item.TrainingFor,
                        DeliveyTeam = item.DeliveyTeam,
                        ProjectName = item.ProjectName
                    };
                    appraisalReportViewModel.Add(report);
                }
                return appraisalReportViewModel;
            });
        }

        //To update rating for normalized history  
        public async Task<bool> UpdateNormalizedEmployesAppraisal(int userId, List<AppraisalReportViewModel> EmpList, int personID, int rating, bool isPromotionNorm, string promotionforByNorm)
        {
            using (PhoenixEntities context = new PhoenixEntities())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        if (isPromotionNorm == false)
                        {
                            promotionforByNorm = "";
                        }
                        var approverPerson = service.First<Person>(x => x.ID == userId);
                        string approverName = approverPerson.FirstName + " " + approverPerson.LastName;
                        EmpList = EmpList.Where(x => x.EmpId == personID).ToList();

                        foreach (var item in EmpList)
                        {
                            var appraisalReportingOld = service.Top<AppraisalReporting>(0, x => x.PersonID == personID && x.AppraiserYear == DateTime.Now.Year).FirstOrDefault();
                            var appraisalReportingNew = appraisalReportingOld;
                            appraisalReportingNew.FinalReviewerRating = rating;
                            appraisalReportingNew.IsPromotionNorm = isPromotionNorm;
                            //appraisalReportingNew.PromotionforByNorm = promotionforByNorm;
                            //appraisalReportingNew.FreezedComments = "<b>Normalized by - " + approverName + ": </b>";
                            //await this.HookApprovalOneToOne(appraisalReportingOld.PersonID, appraisalReportingOld.ID, appraisalReportingOld.AppraiserID);
                            service.Update<AppraisalReporting>(appraisalReportingNew, appraisalReportingOld);
                        }
                        return await Task.Run(() =>
                        {
                            service.Finalize(true);
                            transaction.Commit();
                            return true;
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



        /// <summary>
        /// fetch the details yearwise as the year changes.: Rahul R
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        public async Task<AppraisalEmployeeViewModel> GetAppraisalAssignedTo(int userId, int year)
        {
            return await Task.Run(() =>
            {
                var employes = service.Top<AppraisalReporting>(0, x => x.PersonID == userId && x.IsInitiat == true && x.AppraiserYear == year).FirstOrDefault();
                var appraisalEmployeeViewModel = Mapper.Map<AppraisalReporting, AppraisalEmployeeViewModel>(employes);
                return appraisalEmployeeViewModel;
            });
        }

        public string GetImage(int PersonID)
        {
            string personImage = string.Empty;
            //var person = service.Top<Person>(0, x => x.Active == true && x.ID == PersonID).FirstOrDefault().Image;
            ///Commented on 8th Sep 2021 to resolve appraisal issue VR202132225
            var person = service.Top<Person>(0, x => x.ID == PersonID).FirstOrDefault().Image;
            return personImage = person.ToString();
        }

        public HttpResponseMessage PendingAppraisalStatus()
        {
            List<rpt_PendingAppraisalStatus_Result> AppraisalSummaryList = QueryHelper.GetPendingAppraisalStatus();
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            var AppraisalList = new StringBuilder();
            for (int i = 0; i < AppraisalSummaryList.Count;)
            {
                JObject json = JObject.FromObject(AppraisalSummaryList[i]);
                foreach (var property in json)
                {
                    string key = property.Key.ToString();
                    key = key.Replace('_', ' ');
                    AppraisalList.Append("\"" + key + "\",");
                }
                AppraisalList.Append("\r\n");
                break;
            }
            for (var i = 0; i < AppraisalSummaryList.Count; i++)
            {
                JObject json = JObject.FromObject(AppraisalSummaryList[i]);
                foreach (var property in json)
                {
                    if (property.Value.Type == JTokenType.Date)
                    {
                        AppraisalList.AppendFormat("\"{0}\",", Convert.ToDateTime(property.Value).ToString("MM/dd/yyyy"));
                    }
                    else
                    {
                        AppraisalList.AppendFormat("\"{0}\",", property.Value);
                    }

                }
                AppraisalList.AppendFormat("\r\n");

            }
            response.Content = new StringContent(AppraisalList.ToString());
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment"); //attachment will force download
            response.Content.Headers.ContentDisposition.FileName = "PendingAppraisalStatus.csv";
            return response;
        }

        public bool AllowUserToFeatchAppraisalData(int LogedInUserId, int AppraiseeId, int? year)
        {
            bool accessData = false;
            using (PhoenixEntities dbContext = new PhoenixEntities())
            {
                try
                {
                    int AppraiserID = (from ar in dbContext.AppraisalReporting
                                       where ar.PersonID == AppraiseeId && ar.AppraiserYear == DateTime.Now.Year
                                       select ar.AppraiserID).SingleOrDefault();

                    int ReviewerID = (from ar in dbContext.AppraisalReporting
                                      where ar.PersonID == AppraiseeId && ar.AppraiserYear == DateTime.Now.Year
                                      select ar.ReviewerID).SingleOrDefault();

                    int? status = (from ar in dbContext.AppraisalReporting
                                   where ar.PersonID == AppraiseeId && ar.AppraiserYear == year
                                   select ar.Status).SingleOrDefault();

                    List<PersonInRole> lstNormalizer = ((from pir in dbContext.PersonInRole
                                                         where pir.PersonID == LogedInUserId
                                                         select pir)).ToList();
                    bool isNormalizer = lstNormalizer.Any(p => p.RoleID == 40);
                    //bool isInReviewerID = lstAppraiserID.IndexOf(LogedInUserId) != -1;


                    if (AppraiserID == LogedInUserId)
                    {
                        accessData = true;
                    }
                    else if (isNormalizer)
                    {
                        accessData = true;
                    }
                    else if (ReviewerID == LogedInUserId)
                    {
                        accessData = true;
                    }
                    else if (LogedInUserId == AppraiseeId && status >= 5)
                    {
                        accessData = true;
                    }
                    else
                    {
                        accessData = false;
                    }
                    return accessData;
                }
                catch
                {
                    return accessData;
                }
            }

        }

        public async Task<List<DropdownItems>> GetOrganizationCategory()
        {
            return await Task.Run(() =>
            {
                List<DropdownItems> lstItems = new List<DropdownItems>();
                var organizationcategory = service.All<OrganizationCategory>()
                .Where(x => x.Active == true).OrderBy(x => x.CategoryName);
                foreach (var item in organizationcategory)
                {
                    DropdownItems dropdownItem = new DropdownItems
                    {
                        ID = item.Id,
                        Text = item.CategoryName
                    };
                    lstItems.Add(dropdownItem);
                }
                return lstItems;
            });
        }
        /// To get Rating and Promo for last 5 years
        public async Task<IEnumerable<GetAppraisalRatingByYears_Result>> AppraisalRatingLast5Years(int userId)
        {
            var outputList = new List<GetAppraisalRatingByYears_Result>();
            try
            {
                using (PhoenixEntities context = new PhoenixEntities())
                {
                    var resultList = context.GetAppraisalRatingByYears(userId);
                    outputList.AddRange(resultList);
                }
                return await Task.FromResult(outputList).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log4Net.Error("Error occured in the AppraisalRatingLast5Years() method - InnerException :" + ex.InnerException + "Message :" + ex.Message + "StackTrace :" + ex.StackTrace);
                throw ex;
            }
        }

    }
}