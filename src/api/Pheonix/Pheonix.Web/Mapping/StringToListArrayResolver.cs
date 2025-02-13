using AutoMapper;
using Pheonix.DBContext;
using Pheonix.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Pheonix.Web.Mapping
{
    public class StringToListArrayResolver : IMemberValueResolver<PMSInvoiceDetails, InvoiceDetailsViewModel, string, List<string>>
    {
        public List<string> Resolve(PMSInvoiceDetails source, InvoiceDetailsViewModel destination, string sourceMember, List<string> destinationMember, ResolutionContext context)
        {
            if (string.IsNullOrEmpty(sourceMember))
            {
                return new List<string>();
            }
            else
            {
                return sourceMember.Split(',').ToList();
            }
        }
    }
}