using AutoMapper;
using Pheonix.Core.Repository;
using Pheonix.DBContext;
using Pheonix.Models;
using System.Collections.Generic;
using System.Net;

namespace Pheonix.Core.Services
{
    public class PersonPersonalService : IPersonPersonalService
    {
        private IPersonPersonalRepository _personPersonalRepository;

        public PersonPersonalService(IPersonPersonalRepository personPersonalRepository)
        {
            _personPersonalRepository = personPersonalRepository;
        }

        public IEnumerable<PersonPersonalViewModel> GetList(string filters)
        {
            var employeeAddressList = Mapper.Map<IEnumerable<PersonPersonal>, IEnumerable<PersonPersonalViewModel>>(_personPersonalRepository.GetList(filters));

            return employeeAddressList;
        }

        public HttpStatusCode Add(PersonPersonalViewModel model)
        {
            var statusCode = _personPersonalRepository.Add(model);
            return statusCode;
        }

        public HttpStatusCode Update(PersonPersonalViewModel model)
        {
            var statusCode = _personPersonalRepository.Update(model);
            return statusCode;
        }

        public void Delete(int id)
        {
            _personPersonalRepository.Delete(id);
        }
    }
}