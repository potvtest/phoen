using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using log4net;
using Pheonix.Core.v1.Services.Business;
using Pheonix.Core.v1.Services.KRA;
using Pheonix.DBContext;
using Pheonix.Models;
using Pheonix.Models.ViewModels;
using Pheonix.Models.VM;
using Pheonix.Models.VM.Classes.Employee;
using Pheonix.Web.Extensions;

namespace Pheonix.Web.Controllers.v1
{
    [RoutePrefix("v1/KRA")]
    [Authorize]
    public class KRAController : ApiController
    {
        private readonly IKRAService _kraService;
        private readonly IEmployeeService _employeeService;
        private static readonly ILog Log4Net = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public KRAController(IKRAService kraService, IEmployeeService employeeService)
        {
            _kraService = kraService;
            _employeeService = employeeService;
        }

        [HttpGet, Route("GetKRADetails/{id}/{personId}")]
        public async Task<IHttpActionResult> GetDetails(int id, int personId)
        {
            try
            {
                if (id <= 0 && personId <= 0)
                    return Content(HttpStatusCode.BadRequest, "KRA Id and PersonId should not be zero");

                var result = await _kraService.GetDetails(id, personId);
                if (result != null)
                    return Content(HttpStatusCode.OK, result);

                return Content(HttpStatusCode.NotFound, "No record found!!");
            }
            catch (Exception ex)
            {
                Log4Net.Error("Error while adding/updating KRA details: Inner Exception: " + ex.InnerException + "StackTrace :" + ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "Error while adding/updating KRA details");
            }
        }

        [HttpPost, Route("AddUpdateKRADetails/{isClonedKRA}")]
        public async Task<IHttpActionResult> AddUpdateDetails(PersonKRADetailViewModel personKRADetail, bool isClonedKRA = false)
        {
            try
            {
                var successMessage = personKRADetail.Id == 0 ? "KRA added successfully !!" : "KRA updated successfully !!";
                if (isClonedKRA)
                    successMessage = "KRA cloned successfully !!";

                var isSuccess = await _kraService.AddUpdateDetails(RequestContext.GetClaimInt(ClaimTypes.PrimarySid), isClonedKRA, personKRADetail);
                return Ok(new
                {
                    isSuccess,
                    message = successMessage
                });
            }
            catch (Exception ex)
            {
                Log4Net.Error("Error while adding/updating KRA details: Inner Exception: " + ex.InnerException + "Inner Exception Message :" + ex.InnerException.InnerException.Message);
                return Content(HttpStatusCode.InternalServerError, "Error while adding/updating KRA details");
            }
        }

        [HttpPost, Route("CloneKRAFromHistory")]
        public async Task<IHttpActionResult> CloneFromHistory(PersonKRADetailViewModel personKRADetail)
        {
            try
            {
                var successMessage = "KRA successfully cloned from History !!";
                
                var isSuccess = await _kraService.CloneHistoryDetails(RequestContext.GetClaimInt(ClaimTypes.PrimarySid), personKRADetail);
                return Ok(new
                {
                    isSuccess,
                    message = successMessage
                });
            }
            catch (Exception ex)
            {
                Log4Net.Error("Error while cloning from KRA History details: Inner Exception: " + ex.InnerException + "Inner Exception Message :" + ex.InnerException.InnerException.Message);
                return Content(HttpStatusCode.InternalServerError, "Error while cloning from KRA History details");
            }
        }

        [HttpGet, Route("ValidateCloneKRAFromHistory/{personId}")]
        public async Task<IHttpActionResult> ValidateCloneFromHistory(int personId)
        {
            try
            {
                var validationResults = await _kraService.ValidateCloneFromHistory(personId);

                if (validationResults != null && validationResults.Any())
                {
                    var records = validationResults.ToList();
                    var response = new List<object>();

                    foreach (var record in records)
                    {
                        response.Add(new
                        {
                            YearInitiated = record.YearInitiated,
                            PersonId = record.PersonId,
                            FirstName = record.FirstName,
                            LastName = record.LastName
                        });
                    }

                    return Ok(new
                    {
                        isSuccess = true,
                        message = "KRA initiation details retrieved successfully.",
                        records = response
                    });
                }

                return Content(HttpStatusCode.NotFound, "No record found for the specified person.");

            }
            catch (Exception ex)
            {
                Log4Net.Error("Error while validating cloning from KRA History details: Inner Exception: " + ex.InnerException + "Inner Exception Message :" + ex.InnerException.InnerException.Message);
                return Content(HttpStatusCode.InternalServerError, "Error while validating cloning from KRA History details");
            }
        }

        [HttpGet, Route("GetActiveKRAByPersonId/{personId}")]
        public async Task<IHttpActionResult> GetActiveByPersonId(int personId, [FromUri] int[] yearIds)
        {
            try
            {
                if (personId <= 0)
                    return Content(HttpStatusCode.BadRequest, "PersonId should not be zero!");

                var result = await _kraService.GetActiveByPersonId(personId,yearIds);
                if (result != null)
                    return Content(HttpStatusCode.OK, result);
                
                return Content(HttpStatusCode.NotFound, "No record found!");
            }
            catch (Exception ex)
            {
                Log4Net.Error("Error while searching KRA details: Inner Exception: " + ex.InnerException + "StackTrace :" + ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "Error while searching KRA details");
            }
        }

        [HttpPost, Route("KRAProgress/save-update")]
        public async Task<IHttpActionResult> SaveOrUpdate(KRAProgressViewModel viewModel)
        {
            try
            {
                bool isNew = viewModel.KRAProgressDetails.Any(d => d.Id == 0);
                var successMessage = isNew ? "KRA Progress added successfully!!" : "KRA Progress updated successfully!!";
                var isSuccess = await _kraService.SaveOrUpdateProgress(viewModel, RequestContext.GetClaimInt(ClaimTypes.PrimarySid));
                return Ok(new
                {
                    isSuccess,
                    message = successMessage
                });
            }
            catch (Exception ex)
            {
                Log4Net.Error("Error while adding/updating KRA Progress: Inner Exception: " + ex.InnerException + "Inner Exception Message :" + ex.InnerException.InnerException.Message);
                return Content(HttpStatusCode.InternalServerError, "Error while adding/updating KRA Progress");
            }
        }

        [HttpGet, Route("KRAInitiationEmployeesList/{getKRAInitListType}/{year}")]
        public async Task<IHttpActionResult> GetInitiationEmployeesList(string getKraInitListType, int year)
        {
            try
            {
                var result = await _kraService.GetInitiationEmployeesList(getKraInitListType, year);
                if (result != null)
                    return Content(HttpStatusCode.OK, result);
                
                return Content(HttpStatusCode.NotFound, "No record found!");
            }
            catch (Exception ex)
            {
                Log4Net.Error("Error while fetching Employee list details for KRA Inititation: Inner Exception: " + ex.InnerException + "StackTrace :" + ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "Error while fetching Employee list details for KRA Initiation");
            }
        }

        [HttpGet, Route("KRAAllocatedEmployeesList/{ReviewerId}/{yearId}")]
        public async Task<IHttpActionResult> GetMyAllocatedEmployees(int reviewerId,int yearId)
        {
            try
            {
                var result = await _kraService.GetMyAllocatedEmployees(reviewerId,yearId);
                if (result != null)
                    return Content(HttpStatusCode.OK, result);
                
                return Content(HttpStatusCode.NotFound, "No record found!");
            }
            catch (Exception ex)
            {
                Log4Net.Error("Error while fetching KRA allocated Employee list for Reviewer: Inner Exception: " + ex.InnerException + "StackTrace :" + ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "Error while fetching KRA allocated Employee list for Reviewer");
            }
        }

        [HttpGet, Route("CategoryList")]
        public async Task<IDictionary<string, IEnumerable<DropdownItems>>> GetCategoryDropdown()
        {
            try
            {
                return await _kraService.GetCategoryDropdown();
            }
            catch (Exception ex)
            {
                Log4Net.Error("Error while fetching KRA Category Data: Inner Exception: " + ex.InnerException + "StackTrace :" + ex.StackTrace);
                throw new HttpException(500, ex.ToString());
            }
        }

        [HttpGet, Route("GradeList")]
        public async Task<IDictionary<string, IEnumerable<DropdownItems>>> GetGradeList()
        {
            try
            {
                return await _kraService.GetGradeList();
            }
            catch (Exception ex)
            {
                Log4Net.Error("Error while fetching Grade Data: Inner Exception: " + ex.InnerException + "StackTrace :" + ex.StackTrace);
                throw new HttpException(500, ex.ToString());
            }
        }

        [HttpGet, Route("EmpOfficeLocation")]
        public async Task<IEnumerable<EmployeeWorkLocation>> GetEmpOfficeLocation()
        {
            try
            {
                return await _employeeService.GetEmpOfficeLocation();
            }
            catch (Exception ex)
            {
                Log4Net.Error($"Error while fetching Office Location Data: Inner Exception: {ex.InnerException}  StackTrace : {ex.StackTrace}");
                throw new HttpException(500, ex.ToString());
            }
        }

        [HttpGet, Route("GetKRAProgressList/{kraGoalId}")]
        public async Task<IHttpActionResult> GetProgressList(int kraGoalId)
        {
            try
            {
                var result = await _kraService.GetProgressList(kraGoalId);
                if (result != null && result.Count() >= 0)
                    return Content(HttpStatusCode.OK, result);

                return Content(HttpStatusCode.InternalServerError, "Error while fetching KRA Progress Data");
            }
            catch (Exception ex)
            {
                Log4Net.Error($"Error while fetching KRA Progress Data: Inner Exception: { ex.InnerException } StackTrace : {ex.StackTrace}");
                return Content(HttpStatusCode.InternalServerError, "Error while fetching KRA Progress Data");
            }
        }

        [HttpGet, Route("GetKRAAttachment/{kraGoalId}")]
        public async Task<IHttpActionResult> GetAttachment(int kraGoalId)
        {
            try
            {
                var result = await _kraService.GetAttachment(kraGoalId);
                if (result != null && result.Count() >= 0)
                    return Content(HttpStatusCode.OK, result);

                return Content(HttpStatusCode.InternalServerError, "Error while fetching KRA Attachment Data");
            }
            catch (Exception ex)
            {
                Log4Net.Error($"Error while fetching KRA Attachment Data: Inner Exception: { ex.InnerException } StackTrace : {ex.StackTrace}");
                return Content(HttpStatusCode.InternalServerError, "Error while fetching KRA Attachment Data");
            }
        }

        [HttpPost, Route("InitiatePersonKRA")]
        public async Task<IHttpActionResult> InitiatePerson(IEnumerable<PersonKRAViewModel> personKraInitiationViewModel)
        {
            try
            {
                var result = await _kraService.InitiatePerson(personKraInitiationViewModel);
                return Content(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                Log4Net.Error($"Error while intiating KRA: Inner Exception: {ex.InnerException} StackTrace : {ex.StackTrace}");
                return Content(HttpStatusCode.InternalServerError, $"Error while intiating KRA: Inner Exception: {ex.InnerException}");
            }
        }

        [HttpGet, Route("GetKRAIntiationStatus/{personId}")]
        public async Task<IHttpActionResult> GetInitiationStatus(int personId)
        {
            try
            {
                var result = await _kraService.GetInitiationStatus(personId);
                return Content(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                Log4Net.Error("Error while retriving KRAIntiation Status: Inner Exception: " + ex.InnerException + "StackTrace :" + ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "Error while retriving KRAIntiation Status: Inner Exception" + ex.InnerException);
            }
        }

        [HttpGet, Route("GetKRACycleConfiguration/{personId}")]
        public async Task<IHttpActionResult> GetCycleConfiguration(int personId, [FromUri] int[] yearIds)
        {
            try
            {
                var result = await _kraService.GetCycleConfiguration(personId,yearIds);
                return Content(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                Log4Net.Error("Error while retriving KRA Cycle Configuration: Inner Exception: " + ex.InnerException + "StackTrace :" + ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "Error while retriving Get KRA Cycle Configuration: Inner Exception" + ex.InnerException);
            }
        }

        [Route("KRAProgress/Delete"), HttpPost]
        public async Task<IHttpActionResult> DeleteEntry([FromBody] KRAProgressViewModel model)
        {
            try
            {
                var isSuccess = await _kraService.DeleteProgressEntry(model);
                if (isSuccess)
                    return Ok(new
                    {
                        isSuccess,
                        message = "KRA Progress Deleted Successfully!!"
                    });

                return Content(HttpStatusCode.NotFound, $"Not found any KRA Progress record for the given Id to delete!");
            }
            catch (Exception ex)
            {
                Log4Net.Error($"Error while deleting KRA Progress: Inner Exception: {ex.InnerException} Inner Exception Message : {ex.InnerException.InnerException.Message}");
                return Content(HttpStatusCode.InternalServerError, "Error while deleting KRA Progress");
            }
        }

        [HttpPost, Route("MarkKRAsInValid")]
        public async Task<IHttpActionResult> MarkAsInvalid(InvalidKRADetails invalidKraDetails)
        {
            try
            {
                var isSuccess = await _kraService.MarkAsInvalid(invalidKraDetails);
                return Ok(new {
                        isSuccess,
                        message = "KRA Successfully marked as Invalid!!"
                    });
            }
            catch (Exception ex)
            {
                Log4Net.Error($"Error while marking KRA As Invalid: Inner Exception: { ex.InnerException } Inner Exception Message : {ex.InnerException.InnerException.Message}");
                return Content(HttpStatusCode.InternalServerError, "Error while marking KRA As Invalid");
            }
        }

        [HttpPost, Route("MarkKRAAsDone")]
        public async Task<IHttpActionResult> MarkAsDone(KRAMarkDone kraMarkDone)
        {
            try
            {
                var isSuccess = await _kraService.MarkAsDone(kraMarkDone);
                return Ok(new {
                                    isSuccess,
                                    message = "KRA Successfully marked as Done!!"
                                });
            }
            catch (Exception ex)
            {
                Log4Net.Error($"Error while marking KRA As Done: Inner Exception: {ex.InnerException} Inner Exception Message : {ex.InnerException.InnerException.Message}");
                return Content(HttpStatusCode.InternalServerError, "Error while marking KRA As Done");
            }
        }

        [Route("UpdateKRAProgressBar/{kraGoalId}/{percentageValue}"), HttpPost]
        public async Task<IHttpActionResult> UpdateProgressBar(int kraGoalId, int percentageValue)
        {
            try
            {
                var isSuccess = await _kraService.UpdateProgressBar(kraGoalId, percentageValue);
                if (isSuccess)
                    return Ok(new {
                                        isSuccess,
                                        message = "KRA Progress Bar Updated Successfully!!"
                                      });
                    
                return Content(HttpStatusCode.NotFound, $"Not found any KRA Progress record for the given Id: {kraGoalId} to update the KRA Progress Bar");
            }
            catch (Exception ex)
            {
                Log4Net.Error($"Error while updating KRA Progress Bar: Inner Exception: { ex.InnerException} Inner Exception Message : {ex.InnerException.InnerException.Message}");
                return Content(HttpStatusCode.InternalServerError, "Error while updating KRA Progress Bar");
            }
        }

        [HttpPost, Route("ChangeReviewer/{kraInitiationId}/{newReviewerId}")]
        public async Task<IHttpActionResult> ChangeReviewerById(int kraInitiationId, int newReviewerId)
        {
            try
            {
                var isSuccess = await _kraService.ChangeReviewerById(kraInitiationId, newReviewerId);
                return Ok(new
                {
                    isSuccess,
                    message = "Reviewer has been changed successfully"
                });
            }
            catch (Exception ex)
            {
                Log4Net.Error($"Error while Change Reviewer: Inner Exception: { ex.InnerException } Inner Exception Message : {ex.InnerException.InnerException.Message}");
                return Content(HttpStatusCode.InternalServerError, "Error while Change Reviewer");
            }
        }

        [HttpGet, Route("KRAEmployeesListForReports/{ReviewerId}/{yearId}")]
        public async Task<IHttpActionResult> GetMyAllocatedEmployeesForReports(int reviewerId, int yearId)
        {
            try
            {
                var result = await _kraService.GetMyAllocatedEmployeesForReports(reviewerId, yearId);
                if (result != null)
                    return Content(HttpStatusCode.OK, result);

                return Content(HttpStatusCode.NotFound, "No record found!");
            }
            catch (Exception ex)
            {
                Log4Net.Error("Error while fetching KRA allocated Employee list for Reviewer report: Inner Exception: " + ex.InnerException + "StackTrace :" + ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "Error while fetching KRA allocated Employee list for Reviewer report");
            }
        }

        [HttpGet, Route("SearchAllKRADetails")]
        public async Task<IHttpActionResult> SearchAllKRADetails(string personId, string kraCategoryId, string yearId, string weightageId, string quarters, string isValid, string isInValid, string isKRADone)
        {
            try
            {
                var result = await _kraService.SearchAllKRADetail(personId, kraCategoryId, yearId, weightageId, quarters, isValid, isInValid, isKRADone);
                if (result != null)
                    return Content(HttpStatusCode.OK, result);

                return Content(HttpStatusCode.NotFound, "No record found!");
            }
            catch (Exception ex)
            {
                Log4Net.Error("Error while fetching KRA allocated Employee list for Reviewer report: Inner Exception: " + ex.InnerException + "StackTrace :" + ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "Error while fetching KRA allocated Employee list for Reviewer report");
            }
        }

        [HttpGet, Route("DownloadReport")]
        public async Task<IHttpActionResult> DownloadReport(string personId, string kraCategoryId, string yearId, string weightageId, string quarters, string isValid, string isKRADone)
        {
            try
            {
                var isSuccess = await _kraService.DownloadReport(personId, kraCategoryId, yearId, weightageId, quarters, isValid, isKRADone);
                var successMessage = isSuccess == true ? "Report Downloaded Successfully" : "Not able to Download";
                return Ok(new
                {
                    isSuccess,
                    message = successMessage
                });
            }
            catch (Exception ex)
            {
                Log4Net.Error("Error while downloading KRA Summary report: Inner Exception: " + ex.InnerException + "StackTrace :" + ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "Error while downloading KRA Summary report");
            }
        }

        [HttpGet, Route("GetKRAHistoryDetailsForClone/{id}/{personId}")]
        public async Task<IHttpActionResult> GetHistoryDetailsForClone(int id, int personId)
        {
            try
            {
                if (id <= 0 && personId <= 0)
                    return Content(HttpStatusCode.BadRequest, "KRA Id and PersonId should not be zero");

                var result = await _kraService.GetHistoryDetailsForClone(id, personId);
                if (result != null)
                    return Content(HttpStatusCode.OK, result);

                return Content(HttpStatusCode.NotFound, "No record found!!");
            }
            catch (Exception ex)
            {
                Log4Net.Error("Error occured in the GetHistoryDetailsForClone method: Inner Exception: " + ex.InnerException + "StackTrace :" + ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "Error occured in the GetHistoryDetailsForClone method");
            }
        }

        [HttpGet, Route("SearchAllKRAHistoryDetails")]
        public async Task<IHttpActionResult> SearchAllKRAHistoryDetails(string personId, string yearId, string isKRADone)
        {
            try
            {
                var result = await _kraService.SearchAllKRAHistoryDetails(personId, yearId, isKRADone);
                if (result != null && result.Any())
                    return Content(HttpStatusCode.OK, result);

                return Content(HttpStatusCode.OK, new { Message = "No records found." });
            }
            catch (Exception ex)
            {
                Log4Net.Error("Error in SearchAllKRAHistoryDetails method: Inner Exception: " + ex.InnerException + "StackTrace :" + ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "Error in SearchAllKRAHistoryDetails method: Inner Exception");
            }
        }

        [HttpPost, Route("UpdateKRAHistoryDetails")]
        public async Task<IHttpActionResult> UpdateKRAHistoryDetails(PersonKRAUpdateHistoryViewModel viewModel)
        {
            try
            {
                var isSuccess = await _kraService.UpdateKRAHistoryDetails(RequestContext.GetClaimInt(ClaimTypes.PrimarySid), viewModel);
                return Ok(new
                {
                    isSuccess = isSuccess.Status,
                    message = isSuccess.Message
                });
            }
            catch (Exception ex)
            {
                Log4Net.Error("Error while updating KRA History details: Inner Exception: " + ex.InnerException + "Inner Exception Message :" + ex.InnerException.InnerException.Message);
                return Content(HttpStatusCode.InternalServerError, "Error while updating KRA History details");
            }      
        }

        [HttpPost, Route("DeleteKRAAttachment")]
        public async Task<IHttpActionResult> DeleteKRAAttachment([FromBody] KRAFileAttachment request)
        {
            try
            {
                int userId = RequestContext.GetClaimInt(ClaimTypes.PrimarySid);
                bool result = true;

                foreach (var attachment in request.KRAAttachments)
                {
                    bool deleteResult = await _kraService.DeleteKRAAttachment(attachment.Id, attachment.KRAId, userId, request.KRAGoalId);

                    if (!deleteResult)
                    {
                        result = false;
                        break;
                    }
                }

                return Ok(new
                {
                    isSuccess = result,
                    message = result ? "File deleted successfully." : "Error while deleting a file."
                });
            }    
            catch(Exception ex)
            {
                Log4Net.Error("Error While Deleting the file:" + ex.InnerException + "Inner Exception Meassage: " + ex.InnerException.InnerException.Message);
                return Content(HttpStatusCode.InternalServerError, "Error While Deleting the file");
            }
        }

        [HttpPost, Route("SaveKRAAttachment")]
        public async Task<IHttpActionResult> SaveKRAAttachment(KRAFileAttachment fileAttachment)
        {
            try
            {
                var isSuccess = await _kraService.SaveKRAAttachment(fileAttachment, RequestContext.GetClaimInt(ClaimTypes.PrimarySid));
                return Ok(new
                {
                    isSuccess = isSuccess,
                    message = isSuccess ? "File uploaded successfully." : "Error while uploading a file."
                });
            }
            catch (Exception ex)
            {
                Log4Net.Error("Error while uploading file : Inner Exception: " + ex.InnerException + "Inner Exception Message :" + ex.InnerException.InnerException.Message);
                return Content(HttpStatusCode.InternalServerError, "Error while uploading file");
            }
        }

        [HttpGet, Route("GetKRAReviewerFeedback/{kraGoalId}/{personId}")]
        public async Task<IHttpActionResult> GetReviewerFeedback(int kraGoalId, int personId)
        {
            try
            {
                var result = await _kraService.GetReviewerFeedbackDetails(kraGoalId, personId);
                if (result != null)
                    return Content(HttpStatusCode.OK, result);
                return Content(HttpStatusCode.NotFound, "No record found!");
            }
            catch (Exception ex)
            {
                Log4Net.Error("Error while fetching Reviewer feedback: " + ex.InnerException + "StackTrace :" + ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "Error while fetching KRA Reviewer feedback");
            }
        }

        [HttpPost, Route("AddOrUpdateKRAReviewerFeedback")]
        public async Task<IHttpActionResult> AddUpdateReviewerFeedback(KRAFeedbackViewModel feedback)
        {
            try
            {
                var result = await _kraService.AddUpdateReviewerFeedbackDetails(feedback);
                if (result != null)
                    return Content(HttpStatusCode.OK, result);
                return Content(HttpStatusCode.NotFound, "No record found!");
            }
            catch (Exception ex)
            {
                Log4Net.Error("Error while Add or Update Reviewer feedback: " + ex.InnerException + "StackTrace :" + ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "Error while Add or Update KRA Reviewer feedback");
            }
        }

        [HttpGet, Route("GetKRALogs/{personId}/{yearIdsString}")]
        public async Task<IHttpActionResult> GetLogs(int personId, string yearIdsString)
        {
            try
            {
                var result = await _kraService.GetLogs(personId, yearIdsString);
                if (result != null)
                    return Content(HttpStatusCode.OK, result);
                return Content(HttpStatusCode.NotFound, "No record found!");
            }
            catch (Exception ex)
            {
                Log4Net.Error("Error while fetching KRA logs: " + ex.InnerException + "StackTrace :" + ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "Error while fetching KRA logs");
            }
        }

        [Route("DeleteKRAFeedback"), HttpPost]
        public async Task<IHttpActionResult> DeleteFeedback([FromBody] KRAFeedbackDeleteViewModel model)
        {
            try
            {
                var isSuccess = await _kraService.DeleteFeedback(model);
                if (isSuccess)
                    return Ok(new
                    {
                        isSuccess,
                        message = "KRA Feedback Deleted Successfully!!"
                    });

                return Content(HttpStatusCode.NotFound, $"Not found any KRA Feedback record for the given Id to delete!");
            }
            catch (Exception ex)
            {
                Log4Net.Error($"Error while deleting KRA Feedback: Inner Exception: {ex.InnerException} Inner Exception Message : {ex.InnerException.InnerException.Message}");
                return Content(HttpStatusCode.InternalServerError, "Error while deleting KRA Feedback");
            }
        }
    }
}   
