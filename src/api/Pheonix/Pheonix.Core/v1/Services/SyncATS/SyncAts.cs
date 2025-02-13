using Newtonsoft.Json;
using Pheonix.Models.VM.Classes.Candidate;
using Pheonix.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Pheonix.DBContext;
using log4net;
using System.Reflection;

namespace Pheonix.Core.v1.Services.syncATS
{
    public class SyncAts : ISyncAts
    {
        private static readonly ILog Log4Net = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public async Task<string> sendToMNH(CandidateToEmployee candidateToEmployee, string createURL, string clientID, string clSecret)
        {
            bool syncSuccess = false;
            HttpClient client = new HttpClient();
            string Url = createURL;

            //The data that needs to be sent. Any object works.
            var newEmployee = new
            {
                empRefNo = candidateToEmployee.EmployeeCode,
                firstName = candidateToEmployee.FirstName,
                lastName = candidateToEmployee.LastName,
                locationRefNo = GetMNHOfficeLocation(candidateToEmployee.OfficeLocation),
                joiningDate = candidateToEmployee.JoiningDate,
                buRefNo = GetMNHBURef(candidateToEmployee.DeliveryUnit),
                currEmailAddr = candidateToEmployee.OrganizationEmail,
                hiringManager = false,
                updateHiringManager = false
            };


            //Converting the object to a json string. NOTE: Make sure the object doesn't contain circular references.
            string json = JsonConvert.SerializeObject(newEmployee);

            //Needed to setup the body of the request
            HttpContent data = new StringContent(json, Encoding.UTF8, "application/json");

            ////Pass in the full URL and the json string content
            client.DefaultRequestHeaders.Add("client-id", clientID);
            client.DefaultRequestHeaders.Add("client-secret", clSecret);

            HttpResponseMessage response = client.PostAsync(Url, data).Result;

            //It would be better to make sure this request actually made it through
            string result = await response.Content.ReadAsStringAsync();

            return result;
        }

        private string GetMNHBURef(int? deliveryUnit)
        {
            switch (deliveryUnit)
            {
                case 11:
                    return "C1_6";
                case 22:
                    return "C1_3";
                case 23:
                    return "C1_1";
                case 24:
                    return "C1_4";
                case 25:
                    return "C1_2";
                case 26:
                    return "C1_5";
                default:
                    return "";
            }
        }

        private string GetMNHOfficeLocation(int officeLocation)
        {
            switch (officeLocation)
            {
                case 0:
                    return "1";
                case 1:
                    return "2";
                case 10:
                    return "3";
                case 12:
                    return "4";
                case 13:
                    return "5";
                default:
                    return "";
            }
        }

        public async Task<bool> InitiateEmployeeSynchronization(string ListUrl, string DetailUrl, string clientID, string clSecret, int dayMargin)
        {
            Log4Net.Info("Start InitiateEmployeeSynchronization");

            bool syncSuccess = false;

            HttpClient client = new HttpClient();

            string Url = ListUrl;
            //"https://api.mynexthire.com/apigateway/application_offered_hired_metadata_list/get";

            string fromdate = DateTime.Now.AddDays(0 - dayMargin).ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'");
            string todate = DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'");


            //The data that needs to be sent. Any object works.
            var pocoObject = new
            {
                fromDate = fromdate,
                toDate = todate,
                pageNo = 1
            };

            //Converting the object to a json string. NOTE: Make sure the object doesn't contain circular references.
            string json = JsonConvert.SerializeObject(pocoObject);

            //Needed to setup the body of the request
            HttpContent data = new StringContent(json, Encoding.UTF8, "application/json");

            ////Pass in the full URL and the json string content
            client.DefaultRequestHeaders.Add("client-id", clientID);
            client.DefaultRequestHeaders.Add("client-secret", clSecret);

            HttpResponseMessage response = client.PostAsync(Url, data).Result;

            Log4Net.Info("ListUrl : " + Url);
            Log4Net.Info("Post Data : " + data);

            //It would be better to make sure this request actually made it through
            string result = await response.Content.ReadAsStringAsync();

            Log4Net.Info("Response Data : " + result);

            if (!string.IsNullOrEmpty(result))
            {
                List<EmployeeSync> appList = JsonConvert.DeserializeObject<List<EmployeeSync>>(result);

                Log4Net.Info("Deserialize Response Data : " + appList);

                if (appList.Count > 0)
                {
                    foreach (EmployeeSync item in appList)
                    {
                        if (item.ApplicationCurrentStepId >= 1201 && item.ApplicationCurrentStepId <= 1209 && !isCandidateApplicationExists(Convert.ToString(item.ApplicationId)))
                        {
                            Url = DetailUrl + Convert.ToString(item.ApplicationId);
                            //"https://api.mynexthire.com/apigateway/application_details/get/" + Convert.ToString(item.ApplicationId);
                            HttpResponseMessage res = client.GetAsync(Url).Result;

                            Log4Net.Info("DetailUrl : " + Url);

                            

                            string empResult = await res.Content.ReadAsStringAsync();

                            Log4Net.Info("DetailUrl Response Data : " + empResult);
                            if (!string.IsNullOrEmpty(empResult))
                            {
                                try
                                {
                                    ATSEmployeeData fh = JsonConvert.DeserializeObject<ATSEmployeeData>(empResult);
                                    //if (fh.ApplicantPIFDetails != null)
                                    //{
                                    //    if (!string.IsNullOrEmpty(fh.ApplicantPIFDetails.Contact.Email1) && !string.IsNullOrEmpty(fh.ApplicantPIFDetails.Contact.Phone1))
                                    //    {
                                    //        if (!IsCandidateExist(fh.ApplicantPIFDetails.Contact.Email1, fh.ApplicantPIFDetails.Contact.Phone1))
                                    //        {
                                    //            using (PhoenixEntities dbContext = new PhoenixEntities())
                                    //            {
                                    //                Candidate cnd = new Candidate();
                                    //                cnd.CandidateStatus = 4;
                                    //                cnd.CurrentCTC = fh.ApplicationCurrentCtc;
                                    //                cnd.DateOfBirth = fh.ApplicantDob;
                                    //                cnd.ExperienceMonths = int.Parse((Convert.ToDecimal(fh.ApplicantTotalExperience) % 1).ToString().Replace("0.", ""));
                                    //                cnd.ExperienceYears = int.Parse(Math.Truncate(fh.ApplicantTotalExperience).ToString());
                                    //                cnd.FirstName = fh.ApplicantFirstName;
                                    //                cnd.Gender = fh.ApplicantGender == "M" ? 1 : 2;
                                    //                cnd.Image = "";
                                    //                cnd.IsDeleted = false;
                                    //                cnd.LastName = fh.ApplicantLastName;
                                    //                cnd.MiddleName = "";
                                    //                cnd.NoticePeriod = fh.ApplicationNoticePeriod;
                                    //                cnd.PanNumber = "";
                                    //                cnd.PFNumber = "";
                                    //                cnd.Reason = "";
                                    //                cnd.ReleventExperienceMonths = int.Parse((Convert.ToDecimal(fh.ApplicationRelevantExp) % 1).ToString().Replace("0.", ""));
                                    //                cnd.ReleventExperienceYears = int.Parse(Math.Truncate(Convert.ToDecimal(fh.ApplicationRelevantExp)).ToString());
                                    //                cnd.ResumePath = "";
                                    //                cnd.RRFNumber = Convert.ToString(fh.RequisitionId) + ".1";
                                    //                cnd.Salutation = fh.ApplicantGender == "M" ? "Mr." : "Ms.";
                                    //                cnd.SourceName = "MNH";
                                    //                cnd.SourceType = 6;
                                    //                cnd.Image = Convert.ToString(fh.ApplicationId);

                                    //                dbContext.Candidate.Add(cnd);
                                    //                dbContext.SaveChanges();

                                    //                TARRF dummyRRF = new TARRF();
                                    //                dummyRRF.CreatedBy = 3487;
                                    //                dummyRRF.CreatedDate = DateTime.Now;
                                    //                dummyRRF.DeliveryUnit = fh.BuId;
                                    //                //    dummyRRF.Designation = fh.RequisitionDesignation;
                                    //                // dummyRRF.EmploymentType = fh.EmploymentType;
                                    //                dummyRRF.ExpectedClosureDate = DateTime.Now;
                                    //                dummyRRF.HRApprover = 3487;
                                    //                dummyRRF.HRApproverComments = "MNH candidate moved to VW";
                                    //                dummyRRF.IsDraft = false;
                                    //                dummyRRF.JD = null;
                                    //                dummyRRF.MaxYrs = 0;
                                    //                dummyRRF.MinYrs = 0;
                                    //                dummyRRF.ModifiedBy = 3487;
                                    //                dummyRRF.ModifiedDate = DateTime.Now;
                                    //                dummyRRF.OtherSkills = "";
                                    //                dummyRRF.Position = 1;
                                    //                dummyRRF.PrimaryApprover = 3487;
                                    //                dummyRRF.PrimaryApproverComments = "MNH candidate moved to VW";
                                    //                dummyRRF.RequestDate = DateTime.Now;
                                    //                dummyRRF.Requestor = 3487;
                                    //                dummyRRF.RequestorComments = "";
                                    //                dummyRRF.RRFNo = int.Parse(Convert.ToString(fh.RequisitionId));
                                    //                dummyRRF.RRFStatus = 1;
                                    //                dummyRRF.SLA = 0;
                                    //                dummyRRF.EmploymentType = fh.EmploymentType.ToLower() == "permanent" ? 2 : 1;
                                    //                dummyRRF.DeliveryUnit = dbContext.DeliveryUnit.Where(a => a.Name == fh.BuName).FirstOrDefault().ID;
                                    //                dummyRRF.RequestorComments = "Opening for :" + fh.RequisitionLocation;

                                    //                var tempDesignation = dbContext.Designations.Where(d => d.Name == fh.RequisitionDesignation).FirstOrDefault();
                                    //                if (tempDesignation == null)
                                    //                {
                                    //                    Designation newDesig = new Designation();
                                    //                    newDesig.Description = fh.RequisitionDesignation;
                                    //                    newDesig.Grade = 1;
                                    //                    newDesig.IsCompoff = false;
                                    //                    newDesig.IsDeleted = false;
                                    //                    newDesig.Name = fh.RequisitionDesignation;

                                    //                    dbContext.Designations.Add(newDesig);
                                    //                    dbContext.SaveChanges();
                                    //                    dummyRRF.Designation = newDesig.ID;

                                    //                }
                                    //                else
                                    //                    dummyRRF.Designation = tempDesignation.ID;

                                    //                dbContext.TARRF.Add(dummyRRF);
                                    //                dbContext.SaveChanges();

                                    //                TARRFDetail tar = new TARRFDetail();
                                    //                tar.comments = "";
                                    //                tar.RRFNo = (int)dummyRRF.RRFNo;
                                    //                tar.RRFNumber = 1;
                                    //                tar.Status = 0;

                                    //                dbContext.TARRFDetail.Add(tar);
                                    //                dbContext.SaveChanges();

                                    //                CandidatePersonal objCandidatePersonal = new CandidatePersonal();

                                    //                objCandidatePersonal.AlternateContactNo = fh.ApplicantPIFDetails.Contact.Phone2;
                                    //                objCandidatePersonal.AlternateEmail = fh.ApplicantPIFDetails.Contact.Email2;
                                    //                objCandidatePersonal.IsDeleted = false;
                                    //                objCandidatePersonal.Mobile = fh.ApplicantPIFDetails.Contact.Phone1;
                                    //                objCandidatePersonal.PersonalEmail = fh.ApplicantPIFDetails.Contact.Email1;
                                    //                objCandidatePersonal.CandidateID = cnd.ID;
                                    //                objCandidatePersonal.MaritalStatus = fh.ApplicantPIFDetails.Personal.MaritalStatus;

                                    //                dbContext.CandidatePersonal.Add(objCandidatePersonal);
                                    //                dbContext.SaveChanges();

                                    //                string[] currAdd = null, permAdd = null;

                                    //                if (!string.IsNullOrEmpty(Convert.ToString(fh.ApplicantPIFDetails.Contact.CurrentAddress)))
                                    //                {
                                    //                    currAdd = Convert.ToString(fh.ApplicantPIFDetails.Contact.CurrentAddress).Split(',');
                                    //                }
                                    //                if (!string.IsNullOrEmpty(Convert.ToString(fh.ApplicantPIFDetails.Contact.PermanentAddress)))
                                    //                {
                                    //                    permAdd = Convert.ToString(fh.ApplicantPIFDetails.Contact.PermanentAddress).Split(',');
                                    //                }

                                    //                CandidateAddress cndAdd = new CandidateAddress();
                                    //                if (currAdd != null && currAdd.Length > 0)
                                    //                {
                                    //                    cndAdd.Address = !string.IsNullOrEmpty(currAdd[0]) ? currAdd[0] : "";
                                    //                    cndAdd.CandidateID = cnd.ID;
                                    //                    cndAdd.IsCurrent = true;
                                    //                    cndAdd.IsDeleted = false;
                                    //                    if (currAdd.Length >= 2)
                                    //                        cndAdd.City = !string.IsNullOrEmpty(currAdd[1]) ? currAdd[1] : "";
                                    //                    if (currAdd.Length >= 3)
                                    //                        cndAdd.State = !string.IsNullOrEmpty(currAdd[2]) ? currAdd[2] : "";
                                    //                    if (currAdd.Length >= 5)
                                    //                        cndAdd.Country = !string.IsNullOrEmpty(currAdd[4]) ? currAdd[4] : "";
                                    //                    if (currAdd.Length >= 4)
                                    //                        cndAdd.Pin = !string.IsNullOrEmpty(currAdd[3]) ? currAdd[3] : "";


                                    //                    dbContext.CandidateAddress.Add(cndAdd);
                                    //                    dbContext.SaveChanges();
                                    //                }

                                    //                if (permAdd != null && permAdd.Length > 0)
                                    //                {
                                    //                    cndAdd.Address = !string.IsNullOrEmpty(permAdd[0]) ? permAdd[0] : "";
                                    //                    cndAdd.CandidateID = cnd.ID;
                                    //                    cndAdd.IsCurrent = false;
                                    //                    cndAdd.IsDeleted = false;
                                    //                    if (currAdd.Length >= 2)
                                    //                        cndAdd.City = !string.IsNullOrEmpty(permAdd[1]) ? permAdd[1] : "";
                                    //                    if (currAdd.Length >= 3)
                                    //                        cndAdd.State = !string.IsNullOrEmpty(permAdd[2]) ? permAdd[2] : "";
                                    //                    if (currAdd.Length >= 5)
                                    //                        cndAdd.Country = !string.IsNullOrEmpty(permAdd[4]) ? permAdd[4] : "";
                                    //                    if (currAdd.Length >= 4)
                                    //                        cndAdd.Pin = !string.IsNullOrEmpty(permAdd[3]) ? permAdd[3] : "";


                                    //                    dbContext.CandidateAddress.Add(cndAdd);
                                    //                    dbContext.SaveChanges();
                                    //                }

                                    //                if (fh.ApplicantPIFDetails.Employment != null && fh.ApplicantPIFDetails.Employment.Count() > 0)
                                    //                {
                                    //                    foreach (Employment eDetails in fh.ApplicantPIFDetails.Employment)
                                    //                    {
                                    //                        CandidateEmploymentHistory cndEmploymentHistory = new CandidateEmploymentHistory();
                                    //                        cndEmploymentHistory.CandidateID = cnd.ID;
                                    //                        cndEmploymentHistory.EmploymentType = "";
                                    //                        cndEmploymentHistory.IsDeleted = false;
                                    //                        cndEmploymentHistory.JoiningDate = eDetails.FromDate;
                                    //                        cndEmploymentHistory.LastDesignation = eDetails.Designation;
                                    //                        cndEmploymentHistory.Location = "";
                                    //                        cndEmploymentHistory.OrganisationName = eDetails.Company;
                                    //                        cndEmploymentHistory.RoleDescription = "";
                                    //                        cndEmploymentHistory.WorkedTill = eDetails.ToDate;

                                    //                        dbContext.CandidateEmploymentHistory.Add(cndEmploymentHistory);
                                    //                        dbContext.SaveChanges();
                                    //                    }
                                    //                }

                                    //                if (fh.ApplicantEducationDetails != null && fh.ApplicantEducationDetails.Count() > 0)
                                    //                {
                                    //                    foreach (Applicanteducationdetail eDetails in fh.ApplicantEducationDetails)
                                    //                    {
                                    //                        CandidateQualificationMapping candidateEducation = new CandidateQualificationMapping();

                                    //                        candidateEducation.CandidateID = cnd.ID;
                                    //                        candidateEducation.Grade_Class = Convert.ToString(eDetails.Marks);
                                    //                        candidateEducation.Institute = !string.IsNullOrEmpty(eDetails.Institute) ? eDetails.Institute : "Institute not available";
                                    //                        candidateEducation.IsDeleted = false;
                                    //                        candidateEducation.Percentage = Convert.ToString(eDetails.Marks);

                                    //                        var tempQualification = dbContext.Qualification.Where(e => e.QualificationName.ToLower() == eDetails.Degree.ToLower()).FirstOrDefault();
                                    //                        if (tempQualification == null)
                                    //                        {
                                    //                            Qualification newQual = new Qualification();
                                    //                            tempQualification = new Qualification();
                                    //                            newQual.Active = true;
                                    //                            newQual.IsDeleted = false;
                                    //                            newQual.QualificationName = eDetails.Degree;
                                    //                            newQual.QualificationGroupID = 4;

                                    //                            dbContext.Qualification.Add(newQual);
                                    //                            dbContext.SaveChanges();

                                    //                            tempQualification.ID = newQual.ID;
                                    //                        }
                                    //                        candidateEducation.QualificationID = tempQualification.ID;
                                    //                        candidateEducation.QualificationType = "1";
                                    //                        candidateEducation.Specialization = "";
                                    //                        candidateEducation.StatusId = 0;
                                    //                        candidateEducation.University = !string.IsNullOrEmpty(eDetails.UniversityName) ? eDetails.UniversityName : "University not available";
                                    //                        candidateEducation.Year = eDetails.Year != null ? int.Parse(eDetails.Year) : 0;

                                    //                        dbContext.CandidateQualificationMapping.Add(candidateEducation);
                                    //                        dbContext.SaveChanges();
                                    //                    }
                                    //                }

                                    //            }
                                    //        }
                                    //    }
                                    //}
                                    //else
                                    //{
                                        if (!string.IsNullOrEmpty(fh.ApplicantEmail) && !string.IsNullOrEmpty(fh.ApplicantMobile1))
                                        {
                                            if (!IsCandidateExist(fh.ApplicantEmail, fh.ApplicantMobile1))
                                            {
                                                using (PhoenixEntities dbContext = new PhoenixEntities())
                                                {
                                                    Candidate cnd = new Candidate();
                                                    cnd.CandidateStatus = 4;
                                                    cnd.CurrentCTC = fh.ApplicationCurrentCtc;
                                                    cnd.DateOfBirth = fh.ApplicantDob;
                                                    cnd.ExperienceMonths = int.Parse((Convert.ToDecimal(fh.ApplicantTotalExperience) % 1).ToString().Replace("0.", ""));
                                                    cnd.ExperienceYears = int.Parse(Math.Truncate(fh.ApplicantTotalExperience).ToString());
                                                    cnd.FirstName = fh.ApplicantFirstName;
                                                    cnd.Gender = fh.ApplicantGender == "M" ? 1 : 2;
                                                    cnd.Image = "";
                                                    cnd.IsDeleted = false;
                                                    cnd.LastName = fh.ApplicantLastName;
                                                    cnd.MiddleName = "";
                                                    cnd.NoticePeriod = fh.ApplicationNoticePeriod;
                                                    cnd.PanNumber = "";
                                                    cnd.PFNumber = "";
                                                    cnd.Reason = "";
                                                    cnd.ReleventExperienceMonths = int.Parse((Convert.ToDecimal(fh.ApplicationRelevantExp) % 1).ToString().Replace("0.", ""));
                                                    cnd.ReleventExperienceYears = int.Parse(Math.Truncate(Convert.ToDecimal(fh.ApplicationRelevantExp)).ToString());
                                                    cnd.ResumePath = "";
                                                    cnd.RRFNumber = Convert.ToString(fh.RequisitionId) + "." + Convert.ToString(fh.ApplicantId);
                                                    cnd.Salutation = fh.ApplicantGender == "M" ? "Mr." : "Ms.";
                                                    var srcID = string.IsNullOrEmpty(Convert.ToString(fh.ApplicationSourceId)) ? "" : Convert.ToString(fh.ApplicationSourceId);
                                                    var srcName = string.IsNullOrEmpty(Convert.ToString(fh.ApplicationSourceName)) ? "MNH" : Convert.ToString(fh.ApplicationSourceName);
                                                    cnd.SourceName = srcID + ":" + srcName;
                                                    cnd.SourceType = 6;
                                                    cnd.Image = Convert.ToString(fh.ApplicationId);

                                                    dbContext.Candidate.Add(cnd);
                                                    dbContext.SaveChanges();

                                                Log4Net.Info("Inserted Candidate : " + fh.ApplicantFirstName + " - " + fh.ApplicantLastName);

                                                int hrApprover = GetHRTarrfApprover();

                                                    TARRF dummyRRF = new TARRF();
                                                    dummyRRF.CreatedBy = !string.IsNullOrEmpty(Convert.ToString(fh.HiringManagerEmployeeReference)) ? Convert.ToInt32(fh.HiringManagerEmployeeReference) : hrApprover;
                                                    dummyRRF.CreatedDate = DateTime.Now;
                                                    dummyRRF.DeliveryUnit = fh.BuId;
                                                    //    dummyRRF.Designation = fh.RequisitionDesignation;
                                                    // dummyRRF.EmploymentType = fh.EmploymentType;
                                                    dummyRRF.ExpectedClosureDate = DateTime.Now;
                                                    dummyRRF.HRApprover = hrApprover;
                                                    dummyRRF.HRApproverComments = "MNH candidate moved to VW";
                                                    dummyRRF.IsDraft = false;
                                                    dummyRRF.JD = null;
                                                    dummyRRF.MaxYrs = 0;
                                                    dummyRRF.MinYrs = 0;
                                                    dummyRRF.ModifiedBy = hrApprover;
                                                    dummyRRF.ModifiedDate = DateTime.Now;
                                                    dummyRRF.OtherSkills = "";
                                                    dummyRRF.Position = 1;
                                                    dummyRRF.PrimaryApprover = hrApprover;
                                                    dummyRRF.PrimaryApproverComments = "MNH candidate moved to VW";
                                                    dummyRRF.RequestDate = DateTime.Now;
                                                    dummyRRF.Requestor = !string.IsNullOrEmpty(Convert.ToString(fh.HiringManagerEmployeeReference)) ? Convert.ToInt32(fh.HiringManagerEmployeeReference) : hrApprover; ;
                                                    dummyRRF.RequestorComments = "";
                                                    dummyRRF.RRFNo = int.Parse(Convert.ToString(fh.RequisitionId));
                                                    dummyRRF.RRFStatus = 1;
                                                    dummyRRF.SLA = 0;
                                                    dummyRRF.EmploymentType = fh.EmploymentType.ToLower() == "permanent" ? 2 : 1;
                                                    dummyRRF.DeliveryUnit = dbContext.DeliveryUnit.Where(a => a.Name == fh.BuName).FirstOrDefault().ID;
                                                    dummyRRF.RequestorComments = "Opening for :" + GetLocationName(Convert.ToInt32(fh.RequisitionLocation));

                                                    var tempDesignation = dbContext.Designations.Where(d => d.Name == fh.RequisitionDesignation).FirstOrDefault();
                                                    if (tempDesignation == null)
                                                    {
                                                        Designation newDesig = new Designation();
                                                        newDesig.Description = fh.RequisitionDesignation;
                                                        newDesig.Grade = 1;
                                                        newDesig.IsCompoff = false;
                                                        newDesig.IsDeleted = false;
                                                        newDesig.Name = fh.RequisitionDesignation;

                                                        dbContext.Designations.Add(newDesig);
                                                        dbContext.SaveChanges();
                                                        dummyRRF.Designation = newDesig.ID;

                                                    }
                                                    else
                                                        dummyRRF.Designation = tempDesignation.ID;

                                                    dbContext.TARRF.Add(dummyRRF);
                                                    dbContext.SaveChanges();

                                                    TARRFDetail tar = new TARRFDetail();
                                                    tar.comments = "";
                                                    tar.RRFNo = (int)dummyRRF.RRFNo;
                                                    tar.RRFNumber = fh.ApplicantId;
                                                    tar.Status = 0;

                                                    dbContext.TARRFDetail.Add(tar);
                                                    dbContext.SaveChanges();

                                                    CandidatePersonal objCandidatePersonal = new CandidatePersonal();

                                                    objCandidatePersonal.AlternateContactNo = fh.ApplicantMobile2;
                                                    objCandidatePersonal.IsDeleted = false;
                                                    objCandidatePersonal.Mobile = fh.ApplicantMobile1;
                                                    objCandidatePersonal.PersonalEmail = fh.ApplicantEmail;
                                                    objCandidatePersonal.CandidateID = cnd.ID;
                                                    objCandidatePersonal.MaritalStatus = fh.ApplicantMaritalStatus;

                                                    dbContext.CandidatePersonal.Add(objCandidatePersonal);
                                                    dbContext.SaveChanges();
                                                    
                                                    ///Candidate address details to be saved in single field as MNH doesn't return address in VW's format
                                                    CandidateAddress cndAdd = new CandidateAddress();
                                                    if (!string.IsNullOrEmpty(Convert.ToString(fh.ApplicantAddress)))
                                                    {
                                                        cndAdd.Address = Convert.ToString(fh.ApplicantAddress).Length > 500 ? Convert.ToString(fh.ApplicantAddress).Substring(0, 499) : Convert.ToString(fh.ApplicantAddress);
                                                    }
                                                    else
                                                        cndAdd.Address = "Address not available";
                                                    cndAdd.CandidateID = cnd.ID;
                                                    cndAdd.IsCurrent = true;
                                                    cndAdd.IsDeleted = false;
                                                    dbContext.CandidateAddress.Add(cndAdd);
                                                    dbContext.SaveChanges();

                                                    if (!string.IsNullOrEmpty(Convert.ToString(fh.ApplicantPermanentAddress)))
                                                    {
                                                        cndAdd.Address = Convert.ToString(fh.ApplicantAddress).Length > 500 ? Convert.ToString(fh.ApplicantAddress).Substring(0, 499) : Convert.ToString(fh.ApplicantAddress);
                                                        cndAdd.CandidateID = cnd.ID;
                                                        cndAdd.IsCurrent = false;
                                                        cndAdd.IsDeleted = false;
                                                        dbContext.CandidateAddress.Add(cndAdd);
                                                        dbContext.SaveChanges();
                                                    }

                                                    if (fh.ApplicantEducationDetails != null && fh.ApplicantEducationDetails.Count() > 0)
                                                    {
                                                        foreach (Applicanteducationdetail eDetails in fh.ApplicantEducationDetails)
                                                        {
                                                            CandidateQualificationMapping candidateEducation = new CandidateQualificationMapping();

                                                            candidateEducation.CandidateID = cnd.ID;
                                                            candidateEducation.Grade_Class = Convert.ToString(eDetails.Marks);
                                                            candidateEducation.Institute = !string.IsNullOrEmpty(eDetails.Institute) ? eDetails.Institute : "Institute not available";
                                                            candidateEducation.IsDeleted = false;
                                                            candidateEducation.Percentage = Convert.ToString(eDetails.Marks);

                                                            var tempQualification = dbContext.Qualification.Where(e => e.QualificationName.ToLower() == eDetails.Degree.ToLower()).FirstOrDefault();
                                                            if (tempQualification == null)
                                                            {
                                                                Qualification newQual = new Qualification();
                                                                tempQualification = new Qualification();
                                                                newQual.Active = true;
                                                                newQual.IsDeleted = false;
                                                                newQual.QualificationName = eDetails.Degree;
                                                                newQual.QualificationGroupID = 4;

                                                                dbContext.Qualification.Add(newQual);
                                                                dbContext.SaveChanges();

                                                                tempQualification.ID = newQual.ID;
                                                            }
                                                            candidateEducation.QualificationID = tempQualification.ID;
                                                            candidateEducation.QualificationType = "1";
                                                            candidateEducation.Specialization = "";
                                                            candidateEducation.StatusId = 0;
                                                            candidateEducation.University = !string.IsNullOrEmpty(eDetails.UniversityName) ? eDetails.UniversityName : "University not available"; 
                                                            candidateEducation.Year = eDetails.Year != null ? int.Parse(eDetails.Year) : 0;

                                                            dbContext.CandidateQualificationMapping.Add(candidateEducation);
                                                            dbContext.SaveChanges();
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    //}
                                    syncSuccess = true;
                                }
                                catch (Exception e)
                                {
                                    Log4Net.Info("Response: " + empResult);
                                    Log4Net.Error("ApplicationId:" + item.ApplicationId + " Error InnerException :" + e.InnerException + "Message :" + e.Message + "StackTrace :" + e.StackTrace);
                                    syncSuccess = false;
                                }

                            }
                        }
                    }
                }
            }
            //close out the client
            client.Dispose();
            Log4Net.Info("End InitiateEmployeeSynchronization");
            return syncSuccess;
        }

        private int GetHRTarrfApprover()
        {
            int recruitmentHead = 0;
            using (PhoenixEntities dbContext = new PhoenixEntities())
            {
                recruitmentHead = (from x in dbContext.PersonInRole
                                   where x.RoleID == 46 && x.Person.Active == true
                                   select x.PersonID).FirstOrDefault();

            }
            return recruitmentHead;
        }

        private string GetLocationName(int requisitionLocation)
        {
            switch (requisitionLocation)
            {
                case 1:
                    return "Mumbai";
                case 2:
                    return "Bengaluru";
                case 3:
                    return "Udaipur";
                case 4:
                    return "Vadodara";
                case 5:
                    return "Bhubaneswar";
                default:
                    return "Mumbai";
            }
        }

        private bool IsCandidateExist(string emailID, string mobileNumber)
        {
            bool isCandidateExist = false;
            using (PhoenixEntities dbContext = new PhoenixEntities())
            {
                int candidateID = (from x in dbContext.CandidatePersonal
                                   where x.PersonalEmail == emailID && x.Mobile == mobileNumber
                                   select x.CandidateID).FirstOrDefault();

                if (candidateID > 0)
                {
                    isCandidateExist = true;
                }
            }
            return isCandidateExist;
        }

        private bool isCandidateApplicationExists(string applicationNo)
        {
            using (PhoenixEntities dbContext = new PhoenixEntities())
            {
                int candidateID = (from c in dbContext.Candidate
                                   where c.Image == applicationNo
                                   select c.ID).FirstOrDefault();

                if (candidateID > 0)
                {
                    return true;
                }
            }
            return false;
        }

    }
}
