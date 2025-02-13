using Pheonix.DBContext;
using Pheonix.Models;
using System.Collections.Generic;
using System.Net;

namespace Pheonix.Core.Repository
{
    public interface IPersonPersonalRepository
    {
        IEnumerable<PersonPersonal> GetList(string filters);

        HttpStatusCode Add(PersonPersonalViewModel model);

        HttpStatusCode Update(PersonPersonalViewModel model);

        void Delete(int id);
    }
}