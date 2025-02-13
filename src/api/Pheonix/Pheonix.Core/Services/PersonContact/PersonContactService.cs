using AutoMapper;
using Pheonix.Core.Repository;
using Pheonix.DBContext;
using Pheonix.Models;
using System.Collections.Generic;
using System.Net;

namespace Pheonix.Core.Services
{
    public class PersonContactService : IPersonContactService
    {
        private IPersonContactRepository _personContactRepository;

        public PersonContactService(IPersonContactRepository personContactRepository)
        {
            _personContactRepository = personContactRepository;
        }

        public IEnumerable<PersonContactViewModel> GetList(string filters)
        {
            var employeeContactList = Mapper.Map<IEnumerable<PersonContact>, IEnumerable<PersonContactViewModel>>(_personContactRepository.GetList(filters));

            return employeeContactList;
        }

        public HttpStatusCode Add(PersonContactViewModel model)
        {
            var statusCode = _personContactRepository.Add(model);
            return statusCode;
        }

        public HttpStatusCode Update(PersonContactViewModel model)
        {
            var statusCode = _personContactRepository.Update(model);
            return statusCode;
        }

        public void Delete(int id)
        {
            _personContactRepository.Delete(id);
        }
    }
}