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
    public class PersonSkillMappingRepository : IPersonSkillMappingRepository
    {
        private PhoenixEntities _phoenixEntity;

        public PersonSkillMappingRepository()
        {
            _phoenixEntity = new PhoenixEntities();
        }

        public IEnumerable<PersonSkillMapping> GetList(string filters)
        {
            var personSkillMappingList = new List<PersonSkillMapping>();
            using (var db = _phoenixEntity)
            {
                //if (filters != null)
                //{
                //    skillMatrixList = db.PersonSkillMappings.Take(10).Where(x => x.Name.Contains(filters)).ToList();
                //}
                //else
                personSkillMappingList = db.PersonSkillMappings.Take(10).ToList();
            }

            return personSkillMappingList;
        }

        public System.Net.HttpStatusCode Add(PersonSkillMappingViewModel model)
        {
            try
            {
                using (var db = _phoenixEntity)
                {
                    PersonSkillMapping dbModel = Mapper.Map<PersonSkillMappingViewModel, PersonSkillMapping>(model);

                    db.PersonSkillMappings.Add(dbModel);
                    db.SaveChanges();
                }
                return HttpStatusCode.OK;
            }
            catch
            {
                return HttpStatusCode.NotFound;
            }
        }

        public System.Net.HttpStatusCode Update(PersonSkillMappingViewModel model)
        {
            try
            {
                using (var db = _phoenixEntity)
                {
                    PersonSkillMapping dbModel = db.PersonSkillMappings.Where(x => x.ID == model.ID).SingleOrDefault();
                    if (dbModel != null)
                    {
                        db.Entry(dbModel).CurrentValues.SetValues(Mapper.Map<PersonSkillMappingViewModel, PersonSkillMapping>(model));
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
                    var personSkillMapping = db.PersonSkillMappings.Where(x => x.ID == id).FirstOrDefault();
                    if (personSkillMapping != null)
                    {
                        db.PersonSkillMappings.Remove(personSkillMapping);
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