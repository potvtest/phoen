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
    public class PersonPersonalRepository : IPersonPersonalRepository
    {
        private PhoenixEntities _phoenixEntity;

        public PersonPersonalRepository()
        {
            _phoenixEntity = new PhoenixEntities();
            _phoenixEntity.Database.Connection.Open();
        }

        public IEnumerable<PersonPersonal> GetList(string filters)
        {
            var personPersonalList = new List<PersonPersonal>();
            using (var db = _phoenixEntity)
            {
                if (filters != null)
                {
                    personPersonalList = db.PersonPersonals.Take(10).Where(x => x.Person.FirstName.Contains(filters)).ToList();
                }
                else
                    personPersonalList = db.PersonPersonals.Take(10).ToList();
            }

            return personPersonalList;
        }

        public HttpStatusCode Add(PersonPersonalViewModel model)
        {
            try
            {
                using (var db = _phoenixEntity)
                {
                    PersonPersonal dbModel = db.PersonPersonals.Where(x => x.PersonID == model.PersonID).SingleOrDefault();
                    if (dbModel == null)
                    {
                        dbModel = Mapper.Map<PersonPersonalViewModel, PersonPersonal>(model);

                        db.PersonPersonals.Add(dbModel);
                        db.SaveChanges();
                    }
                    else
                    {
                        Update(model);
                    }
                }
                return HttpStatusCode.OK;
            }
            catch
            {
                return HttpStatusCode.NotFound;
            }
        }

        public HttpStatusCode Update(PersonPersonalViewModel model)
        {
            try
            {
                using (var db = _phoenixEntity)
                {
                    PersonPersonal dbModel = db.PersonPersonals.Where(x => x.PersonID == model.PersonID).SingleOrDefault();
                    if (dbModel != null)
                    {
                        db.Entry(dbModel).CurrentValues.SetValues(Mapper.Map<PersonPersonalViewModel, PersonPersonal>(model));
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
                    var personPersonal = db.PersonPersonals.Where(x => x.PersonID == id).FirstOrDefault();
                    if (personPersonal != null)
                    {
                        db.PersonPersonals.Remove(personPersonal);
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