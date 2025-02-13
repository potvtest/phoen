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
    public class SkillMatrixRepository : ISkillMatrixRepository
    {
        private PhoenixEntities _phoenixEntity;

        public SkillMatrixRepository()
        {
            _phoenixEntity = new PhoenixEntities();
        }

        public IEnumerable<SkillMatrix> GetList(string filters)
        {
            var skillMatrixList = new List<SkillMatrix>();
            using (var db = _phoenixEntity)
            {
                if (filters != null)
                {
                    skillMatrixList = db.SkillMatrices.Take(10).Where(x => x.Name.Contains(filters)).ToList();
                }
                else
                    skillMatrixList = db.SkillMatrices.Take(10).ToList();
            }

            return skillMatrixList;
        }

        public HttpStatusCode Add(SkillMatrixViewModel model)
        {
            try
            {
                using (var db = _phoenixEntity)
                {
                    SkillMatrix dbModel = Mapper.Map<SkillMatrixViewModel, SkillMatrix>(model);

                    db.SkillMatrices.Add(dbModel);
                    db.SaveChanges();
                }
                return HttpStatusCode.OK;
            }
            catch
            {
                return HttpStatusCode.NotFound;
            }
        }

        public HttpStatusCode Update(SkillMatrixViewModel model)
        {
            try
            {
                using (var db = _phoenixEntity)
                {
                    SkillMatrix dbModel = db.SkillMatrices.Where(x => x.ID == model.ID).SingleOrDefault();
                    if (dbModel != null)
                    {
                        db.Entry(dbModel).CurrentValues.SetValues(Mapper.Map<SkillMatrixViewModel, SkillMatrix>(model));
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
                    var skillMatrix = db.SkillMatrices.Where(x => x.ID == id).FirstOrDefault();
                    if (skillMatrix != null)
                    {
                        db.SkillMatrices.Remove(skillMatrix);
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