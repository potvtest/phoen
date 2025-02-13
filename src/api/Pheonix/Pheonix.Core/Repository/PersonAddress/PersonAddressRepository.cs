using AutoMapper;
using Pheonix.DBContext;
using Pheonix.Models;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;

namespace Pheonix.Core.Repository
{
    public class PersonAddressRepository : IPersonAddressRepository
    {
        private PhoenixEntities _phoenixEntity;

        public PersonAddressRepository()
        {
            _phoenixEntity = new PhoenixEntities();
            _phoenixEntity.Database.Connection.Open();
        }

        public IEnumerable<PersonAddress> GetList(string filters)
        {
            var personAddressList = new List<PersonAddress>();
            using (var db = _phoenixEntity)
            {
                if (filters != null)
                {
                    personAddressList = db.PersonAddresses.Take(10).Where(x => x.Address.Contains(filters)).ToList();
                }
                else
                    personAddressList = db.PersonAddresses.Take(10).ToList();
            }

            return personAddressList;
        }

        public HttpStatusCode Add(PersonAddressViewModel model)
        {
            try
            {
                using (var db = _phoenixEntity)
                {
                    PersonAddress dbModel = Mapper.Map<PersonAddressViewModel, PersonAddress>(model);

                    db.PersonAddresses.Add(dbModel);
                    db.SaveChanges();
                }
                return HttpStatusCode.OK;
            }
            catch
            {
                return HttpStatusCode.NotFound;
            }
        }

        public HttpStatusCode Update(PersonAddressViewModel model)
        {
            try
            {
                using (var db = _phoenixEntity)
                {
                    PersonAddress dbModel = db.PersonAddresses.Where(x => x.ID == model.ID).SingleOrDefault();
                    if (dbModel != null)
                    {
                        db.Entry(dbModel).CurrentValues.SetValues(Mapper.Map<PersonAddressViewModel, PersonAddress>(model));
                        db.SaveChanges();
                    }
                }
                return HttpStatusCode.OK;
            }
            catch
            {
                return HttpStatusCode.NotFound;
            }
        }

        public void Delete(int id)
        {
            try
            {
                using (var db = _phoenixEntity)
                {
                    var personAddress = db.PersonAddresses.Where(x => x.ID == id).FirstOrDefault();
                    if (personAddress != null)
                    {
                        db.PersonAddresses.Remove(personAddress);
                        db.SaveChanges();
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new HttpException(500, ex.ToString());
            }
        }
    }
}