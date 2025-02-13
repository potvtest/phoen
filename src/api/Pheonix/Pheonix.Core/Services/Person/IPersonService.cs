using Pheonix.Models;
using System.Collections.Generic;
using System.Net;

namespace Pheonix.Core.Services
{
    public interface IPersonService
    {
        IEnumerable<PersonViewModel> GetList(string filters);

        HttpStatusCode Add(PersonViewModel model);

        HttpStatusCode Update(PersonViewModel model);

        void Delete(int id);
    }
}