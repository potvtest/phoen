using Pheonix.DBContext;
using Pheonix.Models;
using System.Collections.Generic;
using System.Net;

namespace Pheonix.Core.Repository
{
    public interface ISkillMatrixRepository
    {
        IEnumerable<SkillMatrix> GetList(string filters);

        HttpStatusCode Add(SkillMatrixViewModel model);

        HttpStatusCode Update(SkillMatrixViewModel model);

        void Delete(int id);
    }
}