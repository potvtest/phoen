using Pheonix.DBContext;
using Pheonix.Models;
using System.Collections.Generic;
using System.Net;

namespace Pheonix.Core.Repository
{
    public interface IPersonEmploymentRepository
    {
        IEnumerable<PersonEmployment> GetList(string filters);

        HttpStatusCode Add(PersonEmploymentViewModel model);

        HttpStatusCode Update(PersonEmploymentViewModel model);

        void Delete(int id);
    }
}