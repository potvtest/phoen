using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pheonix.Core.v1.Services.Business
{
    public class ComponentService : IComponentService
    {
        public async Task<Dictionary<string, bool>> GetComponents()
        {
            return await Task.Run(() =>
            {
                Dictionary<string, bool> componentList = new Dictionary<string, bool>();
                componentList.Add("pDCard", true);
                componentList.Add("eduCard", false);
                componentList.Add("cDCard", false);
                componentList.Add("medHCard", true);
                componentList.Add("empHCard", false);
                componentList.Add("empDCard", false);
                componentList.Add("proDCard", true);
                componentList.Add("skDCard", true);

                return componentList;
            });
        }
    }
}