using Microsoft.Practices.Unity;
using System.Web.Http;
using ShoppingCart.Interfaces;
using ShoppingCart.Managers;
using Unity.WebApi;

namespace ShoppingCart.WebAPI
{
    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
			var container = new UnityContainer();
            
            // register all your components with the container here
            // it is NOT necessary to register your controllers
            
            container.RegisterType<IManager, UserManager>(new ContainerControlledLifetimeManager());
            
            GlobalConfiguration.Configuration.DependencyResolver = new UnityDependencyResolver(container);
        }
    }
}