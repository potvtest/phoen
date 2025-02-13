using AutoMapper;
using Microsoft.Reporting.WebForms;
using Pheonix.Core.Report;
using Pheonix.Core.Services.Confirmation;
using Pheonix.Core.v1.Services;
using Pheonix.DBContext;
using Pheonix.Core.v1.Services.Seperation;
using Pheonix.DBContext.Repository;
using Pheonix.Models.Models.Confirmation;
using Pheonix.Models.ViewModels;
using Pheonix.Models.VM;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Pheonix.Web.Report
{
    public class ReportPrinting : IPrintReportInPDF, IPrintSeparationReportInPDF
    {
        public void GenerateReport(List<ReportConfirmationDataModel> dataObject, string reportFileName, string pdfFileName, out string mimeType, List<ReportSeparationDataModel> dataObject1, List<ReportTerminationDataModel> dataObject2, string fileType = "")
        {
            Warning[] warnings;
            string[] streamIds;
            mimeType = string.Empty;
            string encoding = string.Empty;
            string extension = string.Empty;

            // Setup the report viewer object and get the array of bytes
            ReportViewer viewer = new ReportViewer();
            viewer.ProcessingMode = ProcessingMode.Local;
            viewer.LocalReport.ReportPath = reportFileName;
            viewer.LocalReport.DataSources.Clear();

            if (dataObject != null)
                viewer.LocalReport.DataSources.Add(new ReportDataSource("DataSet1", dataObject));
            else if (dataObject1 != null)
                viewer.LocalReport.DataSources.Add(new ReportDataSource("SeparationDataSet", dataObject1));
            else if (dataObject2 != null)
                viewer.LocalReport.DataSources.Add(new ReportDataSource("TerminationDataSet", dataObject2));

            byte[] bytes;

            //if (dataObject != null)
            //    bytes = viewer.LocalReport.Render("PDF", null, out mimeType, out encoding, out extension, out streamIds, out warnings);
            //else
            if (fileType == "PDF")
            {
                bytes = viewer.LocalReport.Render("PDF", null, out mimeType, out encoding, out extension, out streamIds, out warnings);
            }
            else
            {
                bytes = viewer.LocalReport.Render("WORD", null, out mimeType, out encoding, out extension, out streamIds, out warnings);
            }

            try
            {
                string filepath = pdfFileName;
                System.IO.File.WriteAllBytes(filepath, bytes);
            }
            catch
            {

            }

            #region Old Code
            //PDFViewer

            //Doc theDoc = new Doc();
            //theDoc.Read(Server.MapPath("../Rez/Authorization.pdf"));
            //Signature theSig = (Signature)theDoc.Form["Signature"];
            //theSig.Location = "Washington";
            //theSig.Reason = "Schedule Agreed";
            //theSig.Sign(Server.MapPath("../Rez/JohnSmith.pfx"), "111111");
            //theDoc.Save(Server.MapPcalcath("Signed.pdf"));

            // Now that you have all the bytes representing the PDF report, buffer it and send it to the client.
            #endregion
        }
        public async Task<HttpResponseMessage> GetPDFPrint(IContextRepository _repo, Confirmations confirmation, string reportName)
        {
            if (confirmation != null && confirmation.PersonId != 0)
            {
                try
                {
                    string mimeType = string.Empty;
                    string pdfFileName = string.Concat("V2Solutions_VWRefresh_Confirmation(", confirmation.PersonId, ")_", DateTime.Now.ToString("yyyy_MM_dd"), ".pdf");
                    string folderPath = GerReportPath();
                    string PDFfolderPath = GerPDFReportPath();
                    HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);

                    string reportFileName = string.Concat(folderPath, reportName, ".rdlc");
                    string pdfFilepath = string.Concat(PDFfolderPath, pdfFileName);

                    byte[] reportArray = null;

                    if (System.IO.File.Exists(pdfFilepath))
                    {
                        System.IO.File.Delete(pdfFilepath);
                    }

                    var confirmations = new List<ReportConfirmationDataModel>();

                    var reportConfirmation = Mapper.Map<Confirmations, ReportConfirmationDataModel>(confirmation);
                    reportConfirmation = Mapper.Map<ConfirmationFeedback, ReportConfirmationDataModel>(confirmation.Feedback, reportConfirmation);
                    reportConfirmation.ConfirmationStatus = confirmation.Feedback.ConfirmationState;
                    reportConfirmation = Mapper.Map<EmployeeBasicProfile, ReportConfirmationDataModel>(confirmation.Employee, reportConfirmation);

                    reportConfirmation.Calculate(_repo);

                    confirmations.Add(reportConfirmation);
                    var person = _repo.FirstOrDefault<Person>(t => t.ID == confirmation.PersonId);
                    reportConfirmation.Salutation = GetSalutation(person);
                    var employment = _repo.GetAll<PersonEmployment>().ToList();
                    var emp = employment.FirstOrDefault(t => t.PersonID == confirmation.PersonId);
                    emp.ProbationReviewDate = emp.ProbationReviewDate;
                    reportConfirmation.ReviewDate = emp.ProbationReviewDate ?? DateTime.Now;
                    reportConfirmation.PersonId = confirmation.PersonId;

                    GenerateReport(confirmations, reportFileName, pdfFilepath, out mimeType, null, null, "PDF");

                    reportArray = System.IO.File.ReadAllBytes(pdfFilepath);


                    response.Content = new ByteArrayContent(reportArray);
                    response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
                    response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment"); //attachment will force download
                    response.Content.Headers.ContentDisposition.FileName = pdfFileName;

                    return response;
                }
                catch
                {
                    throw;
                }
            }
            return null;
        }

        private string GetSalutation(Person person)
        {
            string result = string.Empty;
            if (person.Gender == 2)
            {
                if (person.PersonPersonal.MaritalStatus == "Single") { result = "Ms."; } else { result = "Mrs."; }
            }
            else { result = "Mr."; }

            return result;
        }

        /// <summary>
        /// Get reports folder path
        /// </summary>
        /// <returns></returns>
        string GerReportPath()
        {
            string path = Convert.ToString(ConfigurationManager.AppSettings["ReportPath"]);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }
        /// <summary>
        /// Get PDF reports folder path
        /// </summary>
        /// <returns></returns>
        string GerPDFReportPath()
        {
            string path = string.Concat(GerReportPath(), @"PDF\");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }
        public async Task<HttpResponseMessage> GetSeparationPDFPrint(IContextRepository _repo, SeperationViewModel separation, string reportName, string fileType = "")
        {
            if (separation != null && separation.PersonID != 0)
            {
                string mimeType = string.Empty;
                string pdfFileName = "";

                string folderPath = GerReportPath();
                string PDFfolderPath = GerPDFReportPath();
                HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);

                var separations = new List<ReportSeparationDataModel>();

                var reportSeparation = Mapper.Map<SeperationViewModel, ReportSeparationDataModel>(separation);

                reportSeparation = Mapper.Map<EmployeeBasicProfile, ReportSeparationDataModel>(separation.EmployeeProfile, reportSeparation);               
                reportSeparation.Calculate(_repo);
                reportSeparation.CurrentDesignation = reportSeparation.CurrentDesignation.Contains("Consultant -") ? "Consultant" : reportSeparation.CurrentDesignation;
                separations.Add(reportSeparation);

                var person = _repo.FirstOrDefault<Person>(t => t.ID == separation.PersonID);
                reportSeparation.Salutation = GetSalutation(person);


                if (reportName == "ExperienceLetter")
                    pdfFileName = string.Concat("V2Solutions_HR_", separation.PersonID + "_" + separation.EmployeeProfile.FirstName.ToString() + "_" + separation.EmployeeProfile.LastName.ToString() + "_Experience_Certificate", ".pdf");
                else if (reportName == "RelievingLetter")
                    pdfFileName = string.Concat("V2Solutions_HR_", separation.PersonID + "_" + separation.EmployeeProfile.FirstName.ToString() + "_" + separation.EmployeeProfile.LastName.ToString() + "_Reliving_Letter", ".pdf");
                else if (reportName == "TerminationLetter")
                    pdfFileName = string.Concat("V2Solutions_HR_", separation.PersonID + "_" + separation.EmployeeProfile.FirstName.ToString() + "_" + separation.EmployeeProfile.LastName.ToString() + "_Separation_Letter", ".pdf");
                else if (reportName == "NDALetter")
                    pdfFileName = string.Concat("V2Solutions_HR_", separation.PersonID + "_" + separation.EmployeeProfile.FirstName.ToString() + "_" + separation.EmployeeProfile.LastName.ToString() + "_Non_Solicitation_NCCA", ".pdf");


                //List<int> letters = new List<int>();
                //letters.Add(1);
                //letters.Add(2);

                //foreach (var item in letters)
                //{
                //    switch (item)
                //    {
                //        case 1:
                //            reportName = "ExperienceLetter";
                //            pdfFileName = string.Concat("Exp-", separation.PersonID, ".pdf");
                //            break;
                //        case 2:
                //            reportName = "RelievingLetter";
                //            pdfFileName = string.Concat("Rel-", separation.PersonID, ".pdf");
                //            break;
                //        case 3:
                //            reportName = "TerminationLetter";
                //            break;
                //    }

                string reportFileName = string.Concat(folderPath, reportName, ".rdlc");
                string pdfFilepath = string.Concat(PDFfolderPath, pdfFileName);

                byte[] reportArray = null;

                if (System.IO.File.Exists(pdfFilepath))
                {
                    System.IO.File.Delete(pdfFilepath);
                }

                GenerateReport(null, reportFileName, pdfFilepath, out mimeType, separations, null, fileType);

                reportArray = System.IO.File.ReadAllBytes(pdfFilepath);
                response.Content = new ByteArrayContent(reportArray);
                response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment"); //attachment will force download
                response.Content.Headers.ContentDisposition.FileName = pdfFileName;
                // }
                return response;
            }
            return null;
        }
        public async Task<object> SaveDeemedPDF(IContextRepository _repo, Confirmations confirmation, string reportName)
        {
            if (confirmation != null && confirmation.PersonId != 0)
            {
                try
                {
                    string mimeType = string.Empty;
                    string pdfFileName = string.Concat("V2Solutions_VWRefresh_Confirmation(", confirmation.PersonId, ")_", DateTime.Now.ToString("yyyy_MM_dd"), ".pdf");
                    string folderPath = GerReportPath();
                    string PDFfolderPath = GerPDFReportPath();
                    HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);

                    string reportFileName = string.Concat(folderPath, reportName, ".rdlc");
                    string pdfFilepath = string.Concat(PDFfolderPath, pdfFileName);



                    if (System.IO.File.Exists(pdfFilepath))
                    {
                        System.IO.File.Delete(pdfFilepath);
                    }

                    confirmation.Employee = Mapper.Map<Person, EmployeeBasicProfile>(_repo.FirstOrDefault<Person>(x => x.ID == confirmation.PersonId));

                    var confirmations = new List<ReportConfirmationDataModel>();
                    ReportConfirmationDataModel reportConfirmation = new ReportConfirmationDataModel();

                    reportConfirmation = Mapper.Map<Confirmations, ReportConfirmationDataModel>(confirmation);
                    reportConfirmation = Mapper.Map<ConfirmationFeedback, ReportConfirmationDataModel>(confirmation.Feedback, reportConfirmation);
                    reportConfirmation.ConfirmationStatus = confirmation.Feedback.ConfirmationState;
                    reportConfirmation = Mapper.Map<EmployeeBasicProfile, ReportConfirmationDataModel>(confirmation.Employee, reportConfirmation);


                    var employment = _repo.GetAll<PersonEmployment>().ToList();
                    var emp = employment.FirstOrDefault(t => t.PersonID == confirmation.PersonId);
                    if (emp != null && emp.Person != null)
                    {
                        var address = emp.Person.PersonAddresses.FirstOrDefault(t => t.IsCurrent ?? false);
                        if (address != null)
                            reportConfirmation.Address = address.Address;
                    }

                    emp.ProbationReviewDate = emp.ProbationReviewDate;
                    reportConfirmation.ReviewDate = emp.ProbationReviewDate ?? DateTime.Now;
                    reportConfirmation.PersonId = confirmation.PersonId;
                    confirmations.Add(reportConfirmation);

                    GenerateReport(confirmations, reportFileName, pdfFilepath, out mimeType, null, null);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return "Success";
        }


        public async Task<HttpResponseMessage> GetTerminationPDFPrint(IContextRepository _repo, int personID, string reportName)
        {
            //if (separation != null && separation.PersonID != 0)
            //{
            string mimeType = string.Empty;
            string pdfFileName = "";

            string folderPath = GerReportPath();
            string PDFfolderPath = GerPDFReportPath();
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);

            var separations = new List<ReportTerminationDataModel>();

            //var reportSeparation = Mapper.Map<SeperationViewModel, ReportSeparationDataModel>(separation);

            //reportSeparation = Mapper.Map<EmployeeBasicProfile, ReportSeparationDataModel>(separation.EmployeeProfile, reportSeparation);
            //reportSeparation.Calculate(_repo);
            var reportSeparation = new ReportTerminationDataModel();
            reportSeparation.ID = personID;
            reportSeparation.PersonID = personID;

            reportSeparation.Calculate(_repo);

            separations.Add(reportSeparation);

            var person = _repo.FirstOrDefault<Person>(t => t.ID == personID);
            reportSeparation.Salutation = GetSalutation(person);

            if (reportName == "ShowCauseNotice1")
                pdfFileName = string.Concat("V2Solutions_VWRefresh_ShowCauseNotice1_", personID, ".pdf");
            else if (reportName == "ShowCauseNotice2")
                pdfFileName = string.Concat("V2Solutions_VWRefresh_ShowCauseNotice2_", personID, ".pdf");
            else if (reportName == "LegaleNotice1")
                pdfFileName = string.Concat("V2Solutions_VWRefresh_LegalNotice1_", personID, ".pdf");
            else if (reportName == "LegaleNotice2")
                pdfFileName = string.Concat("V2Solutions_VWRefresh_LegalNotice2_", personID, ".pdf");

            //List<int> letters = new List<int>();
            //letters.Add(1);
            //letters.Add(2);

            //foreach (var item in letters)
            //{
            //    switch (item)
            //    {
            //        case 1:
            //            reportName = "ExperienceLetter";
            //            pdfFileName = string.Concat("Exp-", separation.PersonID, ".pdf");
            //            break;
            //        case 2:
            //            reportName = "RelievingLetter";
            //            pdfFileName = string.Concat("Rel-", separation.PersonID, ".pdf");
            //            break;
            //        case 3:
            //            reportName = "TerminationLetter";
            //            break;
            //    }

            string reportFileName = string.Concat(folderPath, reportName, ".rdlc");
            string pdfFilepath = string.Concat(PDFfolderPath, pdfFileName);

            byte[] reportArray = null;

            if (System.IO.File.Exists(pdfFilepath))
            {
                System.IO.File.Delete(pdfFilepath);
            }

            GenerateReport(null, reportFileName, pdfFilepath, out mimeType, null, separations);

            reportArray = System.IO.File.ReadAllBytes(pdfFilepath);
            response.Content = new ByteArrayContent(reportArray);
            response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment"); //attachment will force download
            response.Content.Headers.ContentDisposition.FileName = pdfFileName;
            // }
            return response;
            //}
            //return null;
        }

        string GerDOCReportPath()
        {
            string path = string.Concat(GerReportPath(), @"DOC\");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }

        public async Task<HttpResponseMessage> GetSeparationDOCPrint(IContextRepository _repo, SeperationViewModel separation, string reportName, string fileType = "")
        {
            if (separation != null && separation.PersonID != 0)
            {
                string mimeType = string.Empty;
                string docFileName = "";

                string folderPath = GerReportPath();
                string DOCfolderPath = GerDOCReportPath();
                HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);

                var separations = new List<ReportSeparationDataModel>();

                var reportSeparation = Mapper.Map<SeperationViewModel, ReportSeparationDataModel>(separation);

                reportSeparation = Mapper.Map<EmployeeBasicProfile, ReportSeparationDataModel>(separation.EmployeeProfile, reportSeparation);
                reportSeparation.Calculate(_repo);
                reportSeparation.CurrentDesignation = reportSeparation.CurrentDesignation.Contains("Consultant -") ? "Consultant" : reportSeparation.CurrentDesignation;
                separations.Add(reportSeparation);

                var person = _repo.FirstOrDefault<Person>(t => t.ID == separation.PersonID);
                reportSeparation.Salutation = GetSalutation(person);


                if (reportName == "ExperienceLetter")
                    docFileName = string.Concat("V2Solutions_HR_", separation.PersonID + "_" + separation.EmployeeProfile.FirstName.ToString() + "_" + separation.EmployeeProfile.LastName.ToString() + "_Experience_Certificate", ".doc");
                else if (reportName == "RelievingLetter")
                    docFileName = string.Concat("V2Solutions_HR_", separation.PersonID + "_" + separation.EmployeeProfile.FirstName.ToString() + "_" + separation.EmployeeProfile.LastName.ToString() + "_Reliving_Letter", ".doc");
                else if (reportName == "TerminationLetter")
                    docFileName = string.Concat("V2Solutions_HR_", separation.PersonID + "_" + separation.EmployeeProfile.FirstName.ToString() + "_" + separation.EmployeeProfile.LastName.ToString() + "_Separation_Letter", ".doc");
                else if (reportName == "NDALetter")
                    docFileName = string.Concat("V2Solutions_HR_", separation.PersonID + "_" + separation.EmployeeProfile.FirstName.ToString() + "_" + separation.EmployeeProfile.LastName.ToString() + "_Non_Solicitation_NCCA", ".doc");

                string reportFileName = string.Concat(folderPath, reportName, ".rdlc");
                string docFilepath = string.Concat(DOCfolderPath, docFileName);

                byte[] reportArray = null;

                if (System.IO.File.Exists(docFilepath))
                {
                    System.IO.File.Delete(docFilepath);
                }

                GenerateReport(null, reportFileName, docFilepath, out mimeType, separations, null, fileType);

                reportArray = System.IO.File.ReadAllBytes(docFilepath);
                response.Content = new ByteArrayContent(reportArray);
                response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment"); //attachment will force download
                response.Content.Headers.ContentDisposition.FileName = docFileName;
                // }
                return response;
            }
            return null;
        }
    }
}