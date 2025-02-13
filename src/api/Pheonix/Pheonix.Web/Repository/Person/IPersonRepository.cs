using Pheonix.DBContext;
using System.Collections.Generic;
using System.Net;
using Pheonix.Models;

namespace Pheonix.Web.Repository
{
    public interface IPersonRepository
    {
        IEnumerable<Person> GetList(string filters);

        HttpStatusCode Add(PersonViewModel model);

        HttpStatusCode Update(PersonViewModel model);

        void Delete(int id);
    }
}