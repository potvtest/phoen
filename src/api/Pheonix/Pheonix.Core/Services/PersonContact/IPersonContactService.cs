using Pheonix.Models;
using System.Collections.Generic;
using System.Net;

namespace Pheonix.Core.Services
{
    public interface IPersonContactService
    {
        IEnumerable<PersonContactViewModel> GetList(string filters);

        HttpStatusCode Add(PersonContactViewModel model);

        HttpStatusCode Update(PersonContactViewModel model);

        void Delete(int id);
    }
}