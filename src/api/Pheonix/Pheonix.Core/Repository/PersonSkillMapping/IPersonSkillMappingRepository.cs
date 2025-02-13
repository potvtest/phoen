using Pheonix.DBContext;
using Pheonix.Models;
using System.Collections.Generic;
using System.Net;

namespace Pheonix.Core.Repository
{
    public interface IPersonSkillMappingRepository
    {
        IEnumerable<PersonSkillMapping> GetList(string filters);

        HttpStatusCode Add(PersonSkillMappingViewModel model);

        HttpStatusCode Update(PersonSkillMappingViewModel model);

        void Delete(int id);
    }
}