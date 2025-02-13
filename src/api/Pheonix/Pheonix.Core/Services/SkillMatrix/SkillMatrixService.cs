using AutoMapper;
using Pheonix.Core.Repository;
using Pheonix.DBContext;
using Pheonix.Models;
using System.Collections.Generic;
using System.Net;

namespace Pheonix.Core.Services
{
    public class SkillMatrixService : ISkillMatrixService
    {
        private ISkillMatrixRepository _skillMatrixRepository;

        public SkillMatrixService(ISkillMatrixRepository skillMatrixRepository)
        {
            _skillMatrixRepository = skillMatrixRepository;
        }

        public IEnumerable<SkillMatrixViewModel> GetList(string filters)
        {
            var skillMatrixList = Mapper.Map<IEnumerable<SkillMatrix>, IEnumerable<SkillMatrixViewModel>>(_skillMatrixRepository.GetList(filters));
            return skillMatrixList;
        }

        public HttpStatusCode Add(SkillMatrixViewModel model)
        {
            var statusCode = _skillMatrixRepository.Add(model);
            return statusCode;
        }

        public System.Net.HttpStatusCode Update(SkillMatrixViewModel model)
        {
            var statusCode = _skillMatrixRepository.Update(model);
            return statusCode;
        }

        public void Delete(int id)
        {
            _skillMatrixRepository.Delete(id);
        }
    }
}