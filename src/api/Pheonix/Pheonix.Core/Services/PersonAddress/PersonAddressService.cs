using AutoMapper;
using Pheonix.Core.Repository;
using Pheonix.DBContext;
using Pheonix.Models;
using System.Collections.Generic;
using System.Net;

namespace Pheonix.Core.Services
{
    public class PersonAddressService : IPersonAddressService
    {
        private IPersonAddressRepository _personAddressRepository;

        public PersonAddressService(IPersonAddressRepository personAddressRepository)
        {
            _personAddressRepository = personAddressRepository;
        }

        public IEnumerable<PersonAddressViewModel> GetList(string filters)
        {
            var employeeAddressList = Mapper.Map<IEnumerable<PersonAddress>, IEnumerable<PersonAddressViewModel>>(_personAddressRepository.GetList(filters));

            return employeeAddressList;
        }

        public HttpStatusCode Add(PersonAddressViewModel model)
        {
            var statusCode = _personAddressRepository.Add(model);
            return statusCode;
        }

        public HttpStatusCode Update(PersonAddressViewModel model)
        {
            var statusCode = _personAddressRepository.Update(model);
            return statusCode;
        }

        public void Delete(int id)
        {
            _personAddressRepository.Delete(id);
        }
    }
}