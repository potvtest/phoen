using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Core.v1.Services.syncATS
{
    public interface ISyncAts
    {
        #region Sync from ATS Job
        Task<bool> InitiateEmployeeSynchronization(string ListUrl, string DetailUrl, string clientID, string clSecret, int dayMargin);
        #endregion
    }
}
