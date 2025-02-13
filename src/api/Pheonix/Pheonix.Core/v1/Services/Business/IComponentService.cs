using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pheonix.Core.v1.Services.Business
{
    public interface IComponentService
    {
        Task<Dictionary<string, bool>> GetComponents();
    }
}