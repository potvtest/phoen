using Pheonix.Models;
using System.Collections.Generic;
using System.Net;

namespace Pheonix.Core.Services
{
    public interface IPersonAddressService
    {
        IEnumerable<PersonAddressViewModel> GetList(string filters);

        HttpStatusCode Add(PersonAddressViewModel model);

        HttpStatusCode Update(PersonAddressViewModel model);

        void Delete(int id);
    }
}