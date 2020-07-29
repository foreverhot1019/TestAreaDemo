using DataContext.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System;
using System.Data.Entity;
using System.Web;
using TestAreaDemo.Models;
using TestAreaDemo.Services;
using Unity;
using Unity.AspNet.Mvc;
using Unity.Injection;
using Unity.Lifetime;

namespace TestAreaDemo
{
    /// <summary>
    /// Specifies the Unity configuration for the main container.
    /// </summary>
    public static class UnityConfig
    {
        #region Unity Container
        private static Lazy<IUnityContainer> container =new Lazy<IUnityContainer>(() =>
          {
              var container = new UnityContainer();
              RegisterTypes(container);
              return container;
          });

        /// <summary>
        /// Configured Unity Container.
        /// </summary>
        public static IUnityContainer Container {
            get { 
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

            //TransientLifetimeManager��Ĭ�ϣ�/PerThreadLifetimeManager/PerRequestLifetimeManager/HierarchicalLifetimeManager
            container.RegisterType<DbContext, WebDbContext>(new PerRequestLifetimeManager());
            container.RegisterType<WebDbContext>(new PerRequestLifetimeManager());//������������Ȼ �����Ļ�������DbContext

            //container.RegisterType<IAuthenticationManager>(new InjectionFactory(o => HttpContext.Current.GetOwinContext().Authentication));
            container.RegisterFactory<IAuthenticationManager>(x => { return HttpContext.Current.GetOwinContext().Authentication; });

            container.RegisterType<IUserStore<ApplicationUser>, UserStore<ApplicationUser>>(new HierarchicalLifetimeManager());
            container.RegisterType<IRoleStore<ApplicationRole, string>, RoleStore<ApplicationRole>>(new HierarchicalLifetimeManager());

            container.RegisterType<IMessageService, MessageService>();

            #region Unity 5.x ��ʽ ��֧�ֹ��캯��ֱ��ע��

            container.AddExtension(new Diagnostic());

            //TransientLifetimeManager��Ĭ�ϣ�/
            //PerThreadLifetimeManager ÿ���߳�һ����������
            //PerRequestLifetimeManager ÿ������ һ����������
            //HierarchicalLifetimeManager �ֲ���������
            //PerResolveLifetimeManager 
            //ContainerControlledLifetimeManager
            container.RegisterType<IWebDbContext, WebDbContext>("IWebDbContext", new PerRequestLifetimeManager());
            container.RegisterType<IRoleStore<ApplicationRole, string>, RoleStore<ApplicationRole>>(new HierarchicalLifetimeManager());
            container.RegisterType<IUserStore<ApplicationUser>, UserStore<ApplicationUser>>(new HierarchicalLifetimeManager());
            container.RegisterFactory<IAuthenticationManager>(x => { return HttpContext.Current.GetOwinContext().Authentication; });

            var _WebDbContext = new ResolvedParameter<WebDbContext>("WebDbContext");//��ȡ ���캯���������
            container.RegisterType<IMessageService, MessageService>("IMessageService", new InjectionConstructor(_WebDbContext));
            //container.RegisterType<IMessageService, MessageService>("IMessageService", new InjectionConstructor(new ResolvedParameter<IWebDbContext>("IWebDbContext")));
            container.RegisterType<IA_Service, A_Service>("IA_Service");

            #endregion

            #region Unity 4.x ��ʽ ֧�ֹ��캯��ֱ��ע��

            //container.RegisterType<IWebDbContext, WebDbContext>(new PerRequestLifetimeManager());
            //container.RegisterType<IRoleStore<ApplicationRole, string>, RoleStore<ApplicationRole>>(new HierarchicalLifetimeManager());
            //container.RegisterType<IUserStore<ApplicationUser>, UserStore<ApplicationUser>>(new HierarchicalLifetimeManager());
            //container.RegisterType<IAuthenticationManager>(new InjectionFactory(o => HttpContext.Current.GetOwinContext().Authentication));
            //container.RegisterType<IMessageService, MessageService>();
            //container.RegisterType<IA_Service, A_Service>();

            #endregion
        }
    }
}