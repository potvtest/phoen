using Pheonix.DBContext;
using Pheonix.Models;
using System.Collections.Generic;
using System.Net;

namespace Pheonix.Core.Repository
{
    public interface IPersonAddressRepository
    {
        IEnumerable<PersonAddress> GetList(string filters);

        HttpStatusCode Add(PersonAddressViewModel model);

        HttpStatusCode Update(PersonAddressViewModel model);

        void Delete(int id);
    }
}