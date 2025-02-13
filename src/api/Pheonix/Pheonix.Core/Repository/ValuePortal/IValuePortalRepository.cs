using System.Collections.Generic;

namespace Pheonix.Core.Repository.ValuePortal//ValuePortal.Repository.ValuePortal
{
    public interface IValuePortalRepository
    {
        IEnumerable<object> GetList(string query, bool showInActive);
    }
}
