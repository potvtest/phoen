using Pheonix.DBContext;
using Pheonix.Models;
using System.Collections.Generic;
using System.Net;

namespace Pheonix.Core.Repository
{
    public interface IPersonRepository
    {
        IEnumerable<Person> GetList(string filters);

        HttpStatusCode Add(PersonViewModel model);

        HttpStatusCode Update(PersonViewModel model);

        void Delete(int id);
    }
}