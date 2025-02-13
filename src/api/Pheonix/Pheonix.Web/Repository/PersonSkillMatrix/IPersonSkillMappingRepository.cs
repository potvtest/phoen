using Pheonix.DBContext;
using System.Collections.Generic;
using System.Net;
using Pheonix.Models;

namespace Pheonix.Web.Repository
{
    public interface IPersonSkillMappingRepository
    {
        IEnumerable<PersonSkillMapping> GetList(string filters);

        HttpStatusCode Add(PersonSkillMappingViewModel model);

        HttpStatusCode Update(PersonSkillMappingViewModel model);

        void Delete(int id);
    }
}
