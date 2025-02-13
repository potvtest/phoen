using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Pheonix.Core.v1.Services;
using Pheonix.DBContext;
using Pheonix.DBContext.Repository;
using Pheonix.Web.Mapping;

namespace AbsenteeTracingJob
{
    internal class UnityRegister
    {
        private static IUnityContainer _container;

        public static IUnityContainer LoadContainer()
        {
            _container = new UnityContainer();

            // This will register all types with a ISample/Sample naming convention
            _container.RegisterTypes(
                AllClasses.FromLoadedAssemblies(),
                WithMappings.FromMatchingInterface,
                WithName.Default);

            _container.RegisterType<IBasicOperationsService, BasicOperationsService>();
            _container.RegisterType<IContextRepository, ContextRepository<PhoenixEntities>>();
            MappingDTOModelToModel.Configure();

            return _container;
        }
    }
}
