using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Web.Configuration;
using TestAreaDemo.Areas.Movie.Models;
using TestAreaDemo.Areas.Movie.Services;
using Unity;
using Unity.AspNet.Mvc;
using Unity.Injection;
using Unity.Lifetime;

namespace TestAreaDemo.Areas.Movie
{
    /// <summary>
    /// Specifies the Unity configuration for the main container.
    /// </summary>
    public static class UnityConfig
    {
        #region Unity Container

        private static Lazy<IUnityContainer> container =
          new Lazy<IUnityContainer>(() =>
          {
              var container = new UnityContainer();
              RegisterTypes(container);
              return container;
          });

        /// <summary>
        /// Configured Unity Container.
        /// </summary>
        public static IUnityContainer Container
        {
            get
            {
                return container.Value;
            }
        }

        #endregion

        /// <summary>
        /// Registers the type mappings with the Unity container.
        /// </summary>
        /// <param name="container">The unity container to configure.</param>
        /// <remarks>
        /// There is no need to register concrete types such as controllers or
        /// API controllers (unless you want to change the defaults), as Unity
        /// allows resolving a concrete type even if it was not previously
        /// registered.
        /// </remarks>
        public static void RegisterTypes(IUnityContainer container)
        {
            // NOTE: To load from web.config uncomment the line below.
            // Make sure to add a Unity.Configuration to the using statements.
            // container.LoadConfiguration();

            // TODO: Register your type's mappings here.
            // container.RegisterType<IProductRepository, ProductRepository>();

            #region Unity 5.x 方式 不支持构造函数直接注入

            //TransientLifetimeManager（默认）/PerThreadLifetimeManager/PerRequestLifetimeManager/HierarchicalLifetimeManager
            container.RegisterType<IMovieDbContext, MovieDbContext>("IMovieDbContext", new PerRequestLifetimeManager());//必须加这个，不然 上下文会产生多个DbContext

            var _MovieDbContext = new ResolvedParameter<MovieDbContext>("MovieDbContext");
            container.RegisterType<IMovieService, MovieService>("IMovieService", new InjectionConstructor(_MovieDbContext));// new ResolvedParameter<IMovieDbContext>("IMovieDbContext")));
            container.RegisterType<IActorService, ActorService>("IActorService", new InjectionConstructor(_MovieDbContext));// new ResolvedParameter<IMovieDbContext>("IMovieDbContext")));
            container.RegisterType<IA_Service, A_Service>("IA_Service", new InjectionConstructor(_MovieDbContext));// new ResolvedParameter<IMovieDbContext>("IMovieDbContext")));

            #endregion

            #region Unity 4.x 方式 不支持构造函数直接注入

            ////TransientLifetimeManager（默认）/PerThreadLifetimeManager/PerRequestLifetimeManager/HierarchicalLifetimeManager
            //container.RegisterType<DbContext, MovieDbContext>(new PerRequestLifetimeManager());
            //container.RegisterType<MovieDbContext>(new PerRequestLifetimeManager());//必须加这个，不然 上下文会产生多个DbContext

            //container.RegisterType<IMovieService, MovieService>();
            //container.RegisterType<IActorService, ActorService>();

            #endregion
        }
    }
}