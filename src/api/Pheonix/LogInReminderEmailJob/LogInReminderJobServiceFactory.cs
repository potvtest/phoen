using Unity;

namespace LogInReminderEmailJob
{
    public class LogInReminderJobServiceFactory
    {
        public static ILogInReminderJobServiceBase GetService<T>(IUnityContainer unityContainer) where T : ILogInReminderJobServiceBase, new()
        {
            return new T();
        }
    }
}
