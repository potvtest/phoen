using Pheonix.Models;
using System.Collections.Generic;
using System.Net;

namespace Pheonix.Core.Services
{
    public interface ISkillMatrixService
    {
        IEnumerable<SkillMatrixViewModel> GetList(string filters);

        HttpStatusCode Add(SkillMatrixViewModel model);

        HttpStatusCode Update(SkillMatrixViewModel model);

        void Delete(int id);
    }
}