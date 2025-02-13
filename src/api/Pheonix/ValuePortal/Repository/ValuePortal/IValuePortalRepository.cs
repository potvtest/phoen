using System.Collections.Generic;

namespace ValuePortal.Repository.ValuePortal
{
    public interface IValuePortalRepository
    {
        IEnumerable<object> GetList(string query, bool showInActive);
    }
}
