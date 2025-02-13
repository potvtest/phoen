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
    public class PersonContactRepository : IPersonContactRepository
    {
        private PhoenixEntities _phoenixEntity;

        public PersonContactRepository()
        {
            _phoenixEntity = new PhoenixEntities();
            _phoenixEntity.Database.Connection.Open();
        }

        public IEnumerable<PersonContact> GetList(string filters)
        {
            var personContactList = new List<PersonContact>();
            using (var db = _phoenixEntity)
            {
                if (filters != null)
                {
                    personContactList = db.PersonContacts.Take(10).Where(x => x.Address.Contains(filters)).ToList();
                }
                else
                    personContactList = db.PersonContacts.Take(10).ToList();
            }

            return personContactList;
        }

        public HttpStatusCode Add(PersonContactViewModel model)
        {
            try
            {
                using (var db = _phoenixEntity)
                {
                    PersonContact dbModel = Mapper.Map<PersonContactViewModel, PersonContact>(model);

                    db.PersonContacts.Add(dbModel);
                    db.SaveChanges();
                }
                return HttpStatusCode.OK;
            }
            catch
            {
                return HttpStatusCode.NotFound;
            }
        }

        public HttpStatusCode Update(PersonContactViewModel model)
        {
            try
            {
                using (var db = _phoenixEntity)
                {
                    PersonContact dbModel = db.PersonContacts.Where(x => x.ID == model.ID).SingleOrDefault();
                    if (dbModel != null)
                    {
                        db.Entry(dbModel).CurrentValues.SetValues(Mapper.Map<PersonContactViewModel, PersonContact>(model));
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
                    var personContact = db.PersonContacts.Where(x => x.ID == id).FirstOrDefault();
                    if (personContact != null)
                    {
                        db.PersonContacts.Remove(personContact);
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