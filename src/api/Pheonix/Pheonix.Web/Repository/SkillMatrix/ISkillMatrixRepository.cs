using Pheonix.DBContext;
using System.Collections.Generic;
using System.Net;
using Pheonix.Models;

namespace Pheonix.Web.Repository
{
    public interface ISkillMatrixRepository
    {
        IEnumerable<SkillMatrix> GetList(string filters);

        HttpStatusCode Add(SkillMatrixViewModel model);

        HttpStatusCode Update(SkillMatrixViewModel model);

        void Delete(int id);
    }
}
