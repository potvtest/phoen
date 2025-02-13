using System.Collections.Generic;
using System.Net.Http;

namespace Pheonix.Web.Service
{
    internal interface ICRUDService<T>
    {
        IEnumerable<T> GetList(string filters = null);

        HttpResponseMessage Add(T model);

        HttpResponseMessage Update(T model);

        void Delete(int id);
    }
}