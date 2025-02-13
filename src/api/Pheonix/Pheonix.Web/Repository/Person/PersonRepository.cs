using AutoMapper;
using Pheonix.DBContext;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using Pheonix.Models;

namespace Pheonix.Web.Repository
{
    public class PersonRepository : IPersonRepository
    {
        private PhoenixEntities _phoenixEntity;

        public PersonRepository()
        {
            _phoenixEntity = new PhoenixEntities();
        }

        public IEnumerable<Person> GetList(string filters)
        {
            var employeeList = new List<Person>();
            using (var db = _phoenixEntity)
            {
                if (filters != null)
                {
                    employeeList = (db.PersonEmployment.Where(x => x.OrganizationEmail.Contains(filters)).ToList().Count != 0 ?
                                            db.PersonEmployment.Where(x => x.OrganizationEmail.Contains(filters)).Select(x => x.Person).ToList() :
                                            db.People.Take(10).Where(x => x.FirstName.Contains(filters)).ToList());
                }
                else
                    employeeList = db.People.Take(10).ToList();
            }

            return employeeList;
        }

        public HttpStatusCode Add(PersonViewModel model)
        {
            try
            {
                using (var db = _phoenixEntity)
                {
                    Person dbModel = Mapper.Map<PersonViewModel, Person>(model);

                    db.People.Add(dbModel);
                    db.SaveChanges();
                }
                return HttpStatusCode.OK;
            }
            catch
            {
                return HttpStatusCode.NotFound;
            }
        }

        public HttpStatusCode Update(PersonViewModel model)
        {
            try
            {
                using (var db = _phoenixEntity)
                {
                    Person dbModel = db.People.Where(x => x.ID == model.ID).SingleOrDefault();
                    if (dbModel != null)
                    {
                        db.Entry(dbModel).CurrentValues.SetValues(Mapper.Map<PersonViewModel, Person>(model));
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
                    var person = db.People.Where(x => x.ID == id).FirstOrDefault();
                    db.People.Remove(person);
                    db.SaveChanges();
                }
            }
            catch (SqlException ex)
            {
                throw new HttpException(500, ex.ToString());
            }
        }
    }
}