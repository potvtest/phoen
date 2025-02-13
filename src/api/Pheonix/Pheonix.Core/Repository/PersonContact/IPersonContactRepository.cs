using Pheonix.DBContext;
using Pheonix.Models;
using System.Collections.Generic;
using System.Net;

namespace Pheonix.Core.Repository
{
    public interface IPersonContactRepository
    {
        IEnumerable<PersonContact> GetList(string filters);

        HttpStatusCode Add(PersonContactViewModel model);

        HttpStatusCode Update(PersonContactViewModel model);

        void Delete(int id);
    }
}