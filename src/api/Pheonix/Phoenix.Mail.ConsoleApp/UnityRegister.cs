using Microsoft.Practices.Unity;
using Pheonix.Core.v1.Services;
using Pheonix.DBContext;
using Pheonix.DBContext.Repository;

namespace Phoenix.Mail.ConsoleApp
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

            return _container;
        }
    }
}