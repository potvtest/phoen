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
    public class PersonEmploymentRepository : IPersonEmploymentRepository
    {
        private PhoenixEntities _phoenixEntity;

        public PersonEmploymentRepository()
        {
            _phoenixEntity = new PhoenixEntities();
            _phoenixEntity.Database.Connection.Open();
        }

        public IEnumerable<PersonEmployment> GetList(string filters)
        {
            var personEmploymentList = new List<PersonEmployment>();
            using (var db = _phoenixEntity)
            {
                if (filters != null)
                {
                    personEmploymentList = db.PersonEmployment.Take(10).Where(x => x.UserName.Contains(filters)).ToList();
                }
                else
                    personEmploymentList = db.PersonEmployment.Take(10).ToList();
            }

            return personEmploymentList;
        }

        public HttpStatusCode Add(PersonEmploymentViewModel model)
        {
            try
            {
                using (var db = _phoenixEntity)
                {
                    PersonEmployment dbModel = Mapper.Map<PersonEmploymentViewModel, PersonEmployment>(model);

                    db.PersonEmployment.Add(dbModel);
                    db.SaveChanges();
                }
                return HttpStatusCode.OK;
            }
            catch
            {
                return HttpStatusCode.NotFound;
            }
        }

        public HttpStatusCode Update(PersonEmploymentViewModel model)
        {
            try
            {
                using (var db = _phoenixEntity)
                {
                    PersonEmployment dbModel = db.PersonEmployment.Where(x => x.ID == model.ID).SingleOrDefault();
                    if (dbModel != null)
                    {
                        db.Entry(dbModel).CurrentValues.SetValues(Mapper.Map<PersonEmploymentViewModel, PersonEmployment>(model));
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
                    var personEmployment = db.PersonEmployment.Where(x => x.ID == id).FirstOrDefault();
                    if (personEmployment != null)
                    {
                        db.PersonEmployment.Remove(personEmployment);
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