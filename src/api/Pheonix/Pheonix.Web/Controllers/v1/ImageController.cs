using Pheonix.Models.VM.Classes.Expense;
using Pheonix.Web.Providers;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Pheonix.Web.Extensions;
using System.Security.Claims;
using System.Collections.Generic;
using System;

namespace Pheonix.Web.Controllers.v1
{
    [RoutePrefix("v1/image"), Authorize]
    public class ImageController : ApiController
    {
        static string fileUrl = ConfigurationManager.AppSettings["UploadedFileUrl"].ToString();

        [Route("upload/{moduleType}"), HttpPost]
        public async Task<IHttpActionResult> Upload(string moduleType)
        {
            int loggedInUser = RequestContext.GetClaimInt(ClaimTypes.PrimarySid);
            string uploadFolder = ConfigurationManager.AppSettings["UploadFolder"].ToString() + moduleType;
            if (!Request.Content.IsMimeMultipartContent("form-data"))
            {
                return BadRequest("Unsupported media type");
            }

            if (moduleType != "timesheet" && moduleType != "leaves" && moduleType != "termination" && moduleType != "PC" && moduleType != "kra")
            {
                var provider = new ImageMultipartFormDataStreamProvider(uploadFolder, loggedInUser);

                await Request.Content.ReadAsMultipartAsync(provider);

                var photos = new PhotoViewModel();

                foreach (var file in provider.FileData)
                {
                    var fileInfo = new FileInfo(file.LocalFileName);

                    photos = (new PhotoViewModel
                    {
                        Name = fileUrl + moduleType + "/" + fileInfo.Name,
                    });
                }

                return Ok(photos);
            }
            else if (moduleType == "timesheet" || moduleType == "leaves")
            {
                //string fileSaveLocation = HttpContext.Current.Server.MapPath("~/App_Data");
                var provider = new CustomMultipartFormDataStreamProvider(uploadFolder, loggedInUser);
                List<string> files = new List<string>();

                var temp = new PhotoViewModel();
                // Read all contents of multipart message into CustomMultipartFormDataStreamProvider.
                await Request.Content.ReadAsMultipartAsync(provider);

                //foreach (MultipartFileData file in provider.FileData)
                //{
                //    files.Add(Path.GetFileName(file.LocalFileName));
                //}

                foreach (var file in provider.FileData)
                {
                    var fileInfo = new FileInfo(file.LocalFileName);

                    temp = (new PhotoViewModel
                    {
                        Name = fileUrl + moduleType + "/" + fileInfo.Name,
                    });
                }

                // Send OK Response along with saved file names to the client.
                return Ok(temp);


                // return Ok();
            }

            if (moduleType == "kra")
            {
                var provider = new ImageMultipartFormDataStreamProvider(uploadFolder, loggedInUser);

                await Request.Content.ReadAsMultipartAsync(provider);

                var photos = new PhotoViewModel();

                foreach (var file in provider.FileData)
                {
                    var fileInfo = new FileInfo(file.LocalFileName);

                    photos = (new PhotoViewModel
                    {
                        Name = fileUrl + moduleType + "/" + fileInfo.Name,
                    });
                }

                return Ok(photos); 
            }

            else // (moduleType.Contains("termination/"))
            {
                var provider = new CustomMultipartFormDataStreamProvider(uploadFolder, loggedInUser);

                await Request.Content.ReadAsMultipartAsync(provider);

                var photos = new PhotoViewModel();

                //if (!File.Exists(fileUrl + moduleType))
                //{
                //    File.Create(fileUrl + moduleType);
                //}

                foreach (var file in provider.FileData)
                {
                    var fileInfo = new FileInfo(file.LocalFileName);

                    photos = (new PhotoViewModel
                    {
                        Name = fileUrl + moduleType + "/" + fileInfo.Name,
                    });
                }

                return Ok(photos);
            }
        }

        [Route("upload/{moduleType}/{separationUserID}"), HttpPost]
        public async Task<IHttpActionResult> UploadTest(string moduleType, string separationUserID)
        {
            string uploadFolder = ConfigurationManager.AppSettings["UploadFolder"].ToString() + moduleType;
            if (!Request.Content.IsMimeMultipartContent("form-data"))
            {
                return BadRequest("Unsupported media type");
            }            

            uploadFolder = uploadFolder + @"\" + separationUserID;
            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            var provider = new CustomMultipartFormDataStreamProvider(uploadFolder, RequestContext.GetClaimInt(ClaimTypes.PrimarySid));

            await Request.Content.ReadAsMultipartAsync(provider);

            var photos = new PhotoViewModel();

            foreach (var file in provider.FileData)
            {
                var fileName = string.Empty;

                if (file.LocalFileName.Contains(@"\"))
                    fileName = file.LocalFileName.Substring(file.LocalFileName.LastIndexOf(@"\") + 1);
                else
                    fileName = file.LocalFileName;

                //File.Create(uploadFolder + @"\" + fileName);
                photos = (new PhotoViewModel
                {
                     Name = fileUrl + moduleType + "/" + separationUserID + "/" + fileName,
                    //Name = ConfigurationManager.AppSettings["SeparationFileFolder"].ToString()+ @"\" + separationUserID + @"\" + fileName,
                });
            }

            return Ok(photos);
        }
    }
}