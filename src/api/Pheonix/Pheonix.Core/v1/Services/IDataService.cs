using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pheonix.Core.v1.Services
{
    public interface IDataService
    {
        Task<IEnumerable<T>> List<T>(string filters = null);

        Task<IEnumerable<T>> Single<T>(string filters = null);

        int Add<T>(T model);

        int Update<T>(T model);

        int Delete<T>(int id);
    }
}