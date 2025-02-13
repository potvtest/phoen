using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace Pheonix.Web.Providers
{
    public class ImageMultipartFormDataStreamProvider : MultipartFormDataStreamProvider
    {
        private int _userId { get; set; }
        public ImageMultipartFormDataStreamProvider(string path, int userId)
            : base(path)
        {
            _userId = userId;
        }

        public override string GetLocalFileName(System.Net.Http.Headers.HttpContentHeaders headers)
        {
            string timeConstant = DateTime.Now.TimeOfDay.Hours.ToString() + DateTime.Now.TimeOfDay.Minutes.ToString() + DateTime.Now.TimeOfDay.Seconds.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + DateTime.Now.Year.ToString();

            //Make the file name URL safe and then use it & is the only disallowed url character allowed in a windows filename
            var name = !string.IsNullOrWhiteSpace(headers.ContentDisposition.FileName) ? headers.ContentDisposition.FileName.Replace("\"", "") : "NoName";
            var fileExtension = Path.GetExtension(name);
            name = _userId.ToString() + "_" + timeConstant + fileExtension;
            return name.Trim(new char[] { '"' })
                        .Replace("&", "and");
        }

    }
}