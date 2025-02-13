using AutoMapper;
using Pheonix.Core.Repository;
using Pheonix.DBContext;
using Pheonix.Models;
using System.Collections.Generic;
using System.Net;

namespace Pheonix.Core.Services
{
    public class PersonEmploymentService : IPersonEmploymentService
    {
        private IPersonEmploymentRepository _personEmploymentRepository;

        public PersonEmploymentService(IPersonEmploymentRepository personEmploymentRepository)
        {
            _personEmploymentRepository = personEmploymentRepository;
        }

        public IEnumerable<PersonEmploymentViewModel> GetList(string filters)
        {
            var personEmploymentList = Mapper.Map<IEnumerable<PersonEmployment>, IEnumerable<PersonEmploymentViewModel>>(_personEmploymentRepository.GetList(filters));

            return personEmploymentList;
        }

        public HttpStatusCode Add(PersonEmploymentViewModel model)
        {
            var statusCode = _personEmploymentRepository.Add(model);
            return statusCode;
        }

        public HttpStatusCode Update(PersonEmploymentViewModel model)
        {
            var statusCode = _personEmploymentRepository.Update(model);
            return statusCode;
        }

        public void Delete(int id)
        {
            _personEmploymentRepository.Delete(id);
        }
    }
}