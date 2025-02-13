using Microsoft.Practices.Unity;
using Pheonix.Core.v1.Services;
using Pheonix.DBContext;
using Pheonix.DBContext.Repository;
using Pheonix.Web.Mapping;

namespace ResignationJob
{
    internal class UnityRegister
    {
        private static IUnityContainer _container;
        public static IUnityContainer LoadContainer()
        {
            _container = new UnityContainer();
          
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