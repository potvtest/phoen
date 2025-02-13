using Pheonix.Models;
using System.Collections.Generic;
using System.Net;

namespace Pheonix.Core.Services
{
    public interface IPersonEmploymentService
    {
        IEnumerable<PersonEmploymentViewModel> GetList(string filters);

        HttpStatusCode Add(PersonEmploymentViewModel model);

        HttpStatusCode Update(PersonEmploymentViewModel model);

        void Delete(int id);
    }
}