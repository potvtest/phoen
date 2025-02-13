using Pheonix.Models;
using System.Collections.Generic;
using System.Net;

namespace Pheonix.Core.Services
{
    public interface IPersonSkillMappingService
    {
        IEnumerable<PersonSkillMappingViewModel> GetList(string filters);

        HttpStatusCode Add(PersonSkillMappingViewModel model);

        HttpStatusCode Update(PersonSkillMappingViewModel model);

        void Delete(int id);
    }
}