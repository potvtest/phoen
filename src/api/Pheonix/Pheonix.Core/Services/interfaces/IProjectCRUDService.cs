using Pheonix.Models;
using System.Collections.Generic;
using System.Net.Http;

namespace Pheonix.Core.Services
{
    public interface IProjectCRUDService<T>
    {
        IEnumerable<T> GetList(string filters = null);
        ActionResult Add(T model);
        ActionResult Update(T model);
        void Delete(int id);
        T GetProject(int id);
        IEnumerable<T> GetSubProjectDetails(int projId);        
    }
}
