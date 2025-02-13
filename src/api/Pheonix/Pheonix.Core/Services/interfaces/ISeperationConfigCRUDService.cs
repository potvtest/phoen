using Pheonix.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Core.Services
{
    public interface ISeperationConfigCRUDService<T>
    {
        IEnumerable<T> GetList(string filters = null);

        ActionResult Add(T model);

        ActionResult Update(T model);
        ActionResult Delete(int id);
    }
}
