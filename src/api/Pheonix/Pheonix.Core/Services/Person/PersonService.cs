using AutoMapper;
using Pheonix.Core.Repository;
using Pheonix.DBContext;
using Pheonix.Models;
using System.Collections.Generic;
using System.Net;

namespace Pheonix.Core.Services
{
    public class PersonService : IPersonService
    {
        private IPersonRepository _personRepository;

        public PersonService(IPersonRepository personRepository)
        {
            _personRepository = personRepository;
        }

        public IEnumerable<PersonViewModel> GetList(string filters)
        {
            var employeeList = Mapper.Map<IEnumerable<Person>, IEnumerable<PersonViewModel>>(_personRepository.GetList(filters));

            return employeeList;
        }

        public HttpStatusCode Add(PersonViewModel model)
        {
            var statusCode = _personRepository.Add(model);
            return statusCode;
        }

        public HttpStatusCode Update(PersonViewModel model)
        {
            var statusCode = _personRepository.Update(model);
            return statusCode;
        }

        public void Delete(int id)
        {
            _personRepository.Delete(id);
        }
    }
}