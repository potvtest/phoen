using Pheonix.Models;
using System.Collections.Generic;
using System.Net;

namespace Pheonix.Core.Services
{
    public interface IPersonPersonalService
    {
        IEnumerable<PersonPersonalViewModel> GetList(string filters);

        HttpStatusCode Add(PersonPersonalViewModel model);

        HttpStatusCode Update(PersonPersonalViewModel model);

        void Delete(int id);
    }
}