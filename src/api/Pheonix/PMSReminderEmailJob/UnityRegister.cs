using Unity;
using Unity.RegistrationByConvention;
using Pheonix.Core.v1.Services;
using Pheonix.DBContext.Repository;
using Pheonix.Web.Mapping;
using Pheonix.DBContext;

namespace PMSReminderEmailJob
{
    internal class UnityRegister
    {
        private static Unity.IUnityContainer _container;

        public static Unity.IUnityContainer LoadContainer()
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
