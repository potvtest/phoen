using AutoMapper;
using Pheonix.Core.Repository;
using Pheonix.DBContext;
using Pheonix.Models;
using System.Collections.Generic;
using System.Net;

namespace Pheonix.Core.Services
{
    public class PersonSkillMappingService : IPersonSkillMappingService
    {
        private IPersonSkillMappingRepository _personSkillMappingRepository;

        public PersonSkillMappingService(IPersonSkillMappingRepository personSkillMappingRepository)
        {
            _personSkillMappingRepository = personSkillMappingRepository;
        }

        public IEnumerable<PersonSkillMappingViewModel> GetList(string filters)
        {
            var personSkillMappingList = Mapper.Map<IEnumerable<PersonSkillMapping>, IEnumerable<PersonSkillMappingViewModel>>(_personSkillMappingRepository.GetList(filters));
            return personSkillMappingList;
        }

        public HttpStatusCode Add(PersonSkillMappingViewModel model)
        {
            var statusCode = _personSkillMappingRepository.Add(model);
            return statusCode;
        }

        public HttpStatusCode Update(PersonSkillMappingViewModel model)
        {
            var statusCode = _personSkillMappingRepository.Update(model);
            return statusCode;
        }

        public void Delete(int id)
        {
            _personSkillMappingRepository.Delete(id);
        }
    }
}