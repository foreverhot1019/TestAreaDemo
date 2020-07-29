using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Routing;
using System.Web.Routing;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Collections;
using System.Threading;
using System.Globalization;
using DataContext.Models;
using DataContext.Extensions;

namespace TestAreaDemo
{
    #region MyRegion

    public class MultiLangRouteHandler : MvcRouteHandler
    {
        protected override IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            #region 设置UI文化区域/语言

            try
            {
                string UserLanguages = (string)CacheHelper.GetCache("UserLanguages");
                if (string.IsNullOrEmpty(UserLanguages))
                {
                    UserLanguages = (requestContext.HttpContext.Request.UserLanguages == null || !requestContext.HttpContext.Request.UserLanguages.Any()) ? "zh-CN" : requestContext.HttpContext.Request.UserLanguages[0];
                    CacheHelper.SetCache("UserLanguages", UserLanguages);
                }

                string lang = (requestContext.RouteData.Values["lang"] ?? UserLanguages).ToString();

                Thread.CurrentThread.CurrentUICulture = new CultureInfo(lang);
                //Resources.Language.Culture = Thread.CurrentThread.CurrentUICulture;
            }
            catch (Exception ex)
            {
                var ErrMsg = Common.GetExceptionMsg(ex) + ex.StackTrace;
                var OMsg = new Models.Message
                {
                    Id = -1,
                    MsgType = EnumType.Log4NetMsgType.Error,
                    TargetPath = "MvcRouteHandler",
                    Content = ErrMsg,
                    CreatedUserId = "admin",
                    CreatedUserName = "admin",
                    CreatedDateTime = DateTime.Now
                };
                WriteLogHelper.AddMessageToRedis(OMsg, EnumType.RedisLogMsgType.SQLMessage, WriteLogHelper.RedisKeyMessageLog);
            }

            #endregion

            return base.GetHttpHandler(requestContext);
        }
    }

    #endregion

    #region 自定义 RouteCollection

    /// <summary>
    /// /自定义 RouteCollection
    /// </summary>
    public static class RouteCollectionExtensions
    {
        /*
         * RouteConfig.cs
         *  routes.MapLocalizedMvcAttributeRoutes(
         *      urlPrefix: "{culture}/",
         *      constraints: new { culture = new CultureConstraint(defaultCulture: "nl", pattern: "[a-z]{2}") }
         *  );//自定义 RouteCollection
         */
        public static void MapLocalizedMvcAttributeRoutes(this RouteCollection routes, string urlPrefix, object constraints)
        {
            MapLocalizedMvcAttributeRoutes(routes, urlPrefix, new RouteValueDictionary(constraints));
        }

        public static void MapLocalizedMvcAttributeRoutes(this RouteCollection routes, string urlPrefix, RouteValueDictionary constraints)
        {
            var routeCollectionRouteType = Type.GetType("System.Web.Mvc.Routing.RouteCollectionRoute, System.Web.Mvc");
            var subRouteCollectionType = Type.GetType("System.Web.Mvc.Routing.SubRouteCollection, System.Web.Mvc");
            FieldInfo subRoutesInfo = routeCollectionRouteType.GetField("_subRoutes", BindingFlags.NonPublic | BindingFlags.Instance);

            var subRoutes = Activator.CreateInstance(subRouteCollectionType);
            var routeEntries = Activator.CreateInstance(routeCollectionRouteType, subRoutes);

            // Add the route entries collection first to the route collection
            routes.Add((RouteBase)routeEntries);

            var localizedRouteTable = new RouteCollection();

            // Get a copy of the attribute routes
            localizedRouteTable.MapMvcAttributeRoutes();

            foreach (var routeBase in localizedRouteTable)
            {
                if (routeBase.GetType().Equals(routeCollectionRouteType))
                {
                    // Get the value of the _subRoutes field
                    var tempSubRoutes = subRoutesInfo.GetValue(routeBase);

                    // Get the PropertyInfo for the Entries property
                    PropertyInfo entriesInfo = subRouteCollectionType.GetProperty("Entries");

                    if (entriesInfo.PropertyType.GetInterfaces().Contains(typeof(IEnumerable)))
                    {
                        foreach (RouteEntry routeEntry in (IEnumerable)entriesInfo.GetValue(tempSubRoutes))
                        {
                            var route = routeEntry.Route;

                            // Create the localized route
                            var localizedRoute = CreateLocalizedRoute(route, urlPrefix, constraints);

                            // Add the localized route entry
                            var localizedRouteEntry = CreateLocalizedRouteEntry(routeEntry.Name, localizedRoute);
                            AddRouteEntry(subRouteCollectionType, subRoutes, localizedRouteEntry);

                            // Add the default route entry
                            AddRouteEntry(subRouteCollectionType, subRoutes, routeEntry);

                            // Add the localized link generation route
                            var localizedLinkGenerationRoute = CreateLinkGenerationRoute(localizedRoute);
                            routes.Add(localizedLinkGenerationRoute);

                            // Add the default link generation route
                            var linkGenerationRoute = CreateLinkGenerationRoute(route);
                            routes.Add(linkGenerationRoute);
                        }
                    }
                }
            }
        }

        private static Route CreateLocalizedRoute(Route route, string urlPrefix, RouteValueDictionary constraints)
        {
            // Add the URL prefix
            var routeUrl = urlPrefix + route.Url;

            // Combine the constraints
            var routeConstraints = new RouteValueDictionary(constraints);
            foreach (var constraint in route.Constraints)
            {
                routeConstraints.Add(constraint.Key, constraint.Value);
            }

            return new Route(routeUrl, route.Defaults, routeConstraints, route.DataTokens, route.RouteHandler);
        }

        private static RouteEntry CreateLocalizedRouteEntry(string name, Route route)
        {
            var localizedRouteEntryName = string.IsNullOrEmpty(name) ? null : name + "_Localized";
            return new RouteEntry(localizedRouteEntryName, route);
        }

        private static void AddRouteEntry(Type subRouteCollectionType, object subRoutes, RouteEntry newEntry)
        {
            var addMethodInfo = subRouteCollectionType.GetMethod("Add");
            addMethodInfo.Invoke(subRoutes, new[] { newEntry });
        }

        private static RouteBase CreateLinkGenerationRoute(Route innerRoute)
        {
            var linkGenerationRouteType = Type.GetType("System.Web.Mvc.Routing.LinkGenerationRoute, System.Web.Mvc");
            return (RouteBase)Activator.CreateInstance(linkGenerationRouteType, innerRoute);
        }
    }

    /// <summary>
    /// 方法二 扩展DefaultDirectRouteProvider
    /// </summary>
    public static class RouteCollectionExtensions1
    {
        public static void MapLocalizedMvcAttributeRoutes(this RouteCollection routes, string defaultCulture)
        {
            var routeProvider = new LocalizeDirectRouteProvider(
                "{culture}/",
                defaultCulture
                );
            routes.MapMvcAttributeRoutes(routeProvider);
        }
    }

    public class LocalizeDirectRouteProvider : DefaultDirectRouteProvider
    {
        //ILogger _log = LogManager.GetCurrentClassLogger();

        string _urlPrefix;
        string _defaultCulture;
        RouteValueDictionary _constraints;

        public LocalizeDirectRouteProvider(string urlPrefix, string defaultCulture)
        {
            _urlPrefix = urlPrefix;
            _defaultCulture = defaultCulture;
            _constraints = new RouteValueDictionary() { { "culture", new CultureConstraint(defaultCulture: defaultCulture, pattern: "[a-z]{2}") } };
        }

        protected override IReadOnlyList<RouteEntry> GetActionDirectRoutes(
                    ActionDescriptor actionDescriptor,
                    IReadOnlyList<IDirectRouteFactory> factories,
                    IInlineConstraintResolver constraintResolver)
        {
            var originalEntries = base.GetActionDirectRoutes(actionDescriptor, factories, constraintResolver);
            var finalEntries = new List<RouteEntry>();

            foreach (RouteEntry originalEntry in originalEntries)
            {
                var localizedRoute = CreateLocalizedRoute(originalEntry.Route, _urlPrefix, _constraints);
                var localizedRouteEntry = CreateLocalizedRouteEntry(originalEntry.Name, localizedRoute);
                finalEntries.Add(localizedRouteEntry);
                originalEntry.Route.Defaults.Add("culture", _defaultCulture);
                finalEntries.Add(originalEntry);
            }

            return finalEntries;
        }

        private Route CreateLocalizedRoute(Route route, string urlPrefix, RouteValueDictionary constraints)
        {
            // Add the URL prefix
            var routeUrl = urlPrefix + route.Url;

            // Combine the constraints
            var routeConstraints = new RouteValueDictionary(constraints);
            foreach (var constraint in route.Constraints)
            {
                routeConstraints.Add(constraint.Key, constraint.Value);
            }

            return new Route(routeUrl, route.Defaults, routeConstraints, route.DataTokens, route.RouteHandler);
        }

        private RouteEntry CreateLocalizedRouteEntry(string name, Route route)
        {
            var localizedRouteEntryName = string.IsNullOrEmpty(name) ? null : name + "_Localized";
            return new RouteEntry(localizedRouteEntryName, route);
        }
    }

    #endregion

    #region 自定义RouteConstraint

    /*
     * 可在此添加 特定的 Regx和允许的Route前缀，格式
     * 可验证 Culture，可以是一个自定义 Culture list
     * RouteConfig.cs
     *  routes.MapLocalizedMvcAttributeRoutes(
     *      urlPrefix: "{culture}/",
     *      constraints: new { culture = new CultureConstraint(defaultCulture: "nl", pattern: "[a-z]{2}") }
     *  );//自定义 RouteCollection
     */
    public class CultureConstraint : IRouteConstraint
    {
        private readonly string defaultCulture;
        private readonly string pattern;

        public CultureConstraint(string defaultCulture, string pattern)
        {
            this.defaultCulture = defaultCulture;
            this.pattern = pattern;
        }

        public bool Match(
            HttpContextBase httpContext,
            Route route,
            string parameterName,
            RouteValueDictionary values,
            RouteDirection routeDirection)
        {
            if (routeDirection == RouteDirection.UrlGeneration &&
                this.defaultCulture.Equals(values[parameterName]))
            {
                return false;
            }
            else
            {
                return Regex.IsMatch((string)values[parameterName], "^" + pattern + "$");
            }
        }
    }

    #endregion

    #region 自定义 RouteHandler

    /// <summary>
    /// Task-ActionResult，Execute报错
    /// </summary>
    public class CultureRouteHandler : IRouteHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            return new CultureHandler(requestContext);
        }
    }

    public class CultureHandler : IHttpHandler
    {
        private RequestContext Rcontext;
        public CultureHandler(RequestContext context)
        {
            Rcontext = context;

            string lang = (context.HttpContext.Request.UserLanguages == null || !context.HttpContext.Request.UserLanguages.Any()) ? "zh-CN" : context.HttpContext.Request.UserLanguages[0];
            var request = context.HttpContext.Request;
            string controller = (context.RouteData.Values["controller"] ?? "").ToString();
            string action = (context.RouteData.Values["action"] ?? "").ToString();

            if (context.RouteData.Values["lang"] != null && !string.IsNullOrWhiteSpace(context.RouteData.Values["lang"].ToString()))
            {
                //从路由数据(url)里设置语言
                lang = context.RouteData.Values["lang"].ToString();
                Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture(lang);
            }
            else
            {
                var OCultureInfo = Thread.CurrentThread.CurrentUICulture;
                //从cookie里读取语言设置
                var cookie = context.HttpContext.Request.Cookies["lang.CurrentUICulture"];
                if (cookie != null && cookie.Value != "")
                {
                    //根据cookie设置语言
                    lang = cookie.Value;
                    Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture(lang);
                    var ss = Thread.CurrentContext.ContextProperties;
                    //System.Web.WebPages.Resources.Culture;
                    //把语言值设置到路由值里
                    context.RouteData.Values["lang"] = lang;

                    //if (!IsAjaxOrPost(filterContext))
                    //{
                    //    //把语言值设置到路由值里
                    //    filterContext.RouteData.Values["lang"] = lang;
                    //}
                }
            }
            // 把设置保存进cookie
            HttpCookie _cookie = new HttpCookie("lang.CurrentUICulture", Thread.CurrentThread.CurrentUICulture.Name);
            _cookie.Expires = DateTime.Now.AddMonths(1);
            context.HttpContext.Response.SetCookie(_cookie);

            //if (request.HttpMethod != "POST")
            if (!IsAjaxOrPost(context))
            {
                if (request.Url.AbsolutePath.ToLower().IndexOf(lang.ToLower()) < 0)
                {
                    //if (controller.ToLower() == "home" || action.ToLower() == "login")
                    //{
                    //如果url中不包含语言设置则重定向到包含语言值设置的url里
                    string ReturnUrl = "/" + (context.RouteData.Values["lang"] ?? lang) + "/" + controller + "/" + action + request.Url.Query;
                    context.HttpContext.Response.Redirect(ReturnUrl);
                    //}
                }
            }
        }

        public void ProcessRequest(HttpContext context)
        {
            var controllerName = Rcontext.RouteData.GetRequiredString("controller");
            var CtrlFactory = ControllerBuilder.Current.GetControllerFactory();

            //ControllerBuilder.Current.SetControllerFactory(type);
            var controller = CtrlFactory.CreateController(Rcontext, controllerName);
            if (controller != null)
            {
                controller.Execute(Rcontext);
            }
        }

        public bool IsReusable
        {
            get { return true; }
        }

        /// <summary>
        /// ajax或者POST
        /// </summary>
        /// <param name="controllerContext"></param>
        /// <returns></returns>
        private bool IsAjaxOrPost(RequestContext Rcontext)
        {
            var request = Rcontext.HttpContext.Request;
            return request.IsAjaxRequest() || request.HttpMethod == "POST";
        }
    }

    /// <summary>
    /// 异步处理IHttpHandler
    /// </summary>
    public class AsyncCultureRouteHandler : IHttpAsyncHandler
    {
        public bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {

        }

        /// <summary>
        /// 启动对 HTTP 处理程序的异步调用
        /// </summary>
        /// <param name="context">一个 System.Web.HttpContext 对象，该对象提供对用于向 HTTP 请求提供服务的内部服务器对象（如 Request、Response、Session和 Server）的引用。</param>
        /// <param name="cb">异步方法调用完成时要调用的 System.AsyncCallback。 如果 cb 为 null，则不调用委托。</param>
        /// <param name="extraData">处理该请求所需的所有额外数据。</param>
        /// <returns>包含有关进程状态信息的 System.IAsyncResult。</returns>
        public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData)
        {
            context.Response.ContentType = "text/plain";
            AsyncOperation areault = new AsyncOperation(context, cb, extraData);
            areault.StartAsyncWork();
            return areault;
        }

        /// <summary>
        /// 进程结束时提供异步处理 End 方法。
        /// </summary>
        /// <param name="result">包含有关进程状态信息的 System.IAsyncResult。</param>
        public void EndProcessRequest(IAsyncResult result)
        {
        }
        public class AsyncOperation : IAsyncResult
        {
            HttpContext _context; //保存context的引用 
            AsyncCallback _cb;//保存回调委托的引用 
            object _state;//保存额外的信息 
            bool _iscomplate;//保存异步操作是否完成 


            /// <summary> 
            /// 构造函数，将AsyncHttpHandler的参数全部传递进来 
            /// </summary> 
            /// <param name="context"></param> 
            /// <param name="cb"></param> //该回调不可被重写，否则将会出现客户端永久等待的状态 
            /// <param name="state"></param> //构造时该值可以传递任意自己需要的数据 
            public AsyncOperation(HttpContext context, AsyncCallback cb, object state)
            {
                _context = context;
                _cb = cb;
                _state = state;
                _iscomplate = false; //表明当前异步操作未完成 
            }

            /// <summary> 
            /// 实现获得当前异步处理的状态 
            /// </summary> 
            bool IAsyncResult.IsCompleted
            {
                get
                {
                    return _iscomplate;
                }
            }

            /// <summary> 
            /// 返回 false 即可 
            /// </summary> 
            bool IAsyncResult.CompletedSynchronously
            {
                get
                {
                    return false;
                }
            }

            /// <summary> 
            /// 将返回额外的信息 
            /// </summary> 
            object IAsyncResult.AsyncState
            {
                get
                {
                    return _state;
                }
            }

            /// <summary> 
            /// 为空 
            /// </summary> 
            WaitHandle IAsyncResult.AsyncWaitHandle
            {
                get
                {
                    return null;
                }
            }
            public void send(string str)
            {
                _context.Response.Write(str);
            }
            /// <summary> 
            /// 表明开始异步处理的主函数（方法名可以改，但上面的调用也需要一起改） 
            /// </summary> 
            public void StartAsyncWork()
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(StartAsyncTask), null);//相信很多玩国.net winform 开发的一定认识 
            }

            private void StartAsyncTask(Object workItemState)
            {
                lock (this)
                {
                    Thread.Sleep(5000);
                    int sum = 0;
                    for (int i = 1; i <= 100; i++)
                    {
                        sum += i;
                    }
                    _context.Response.Write(sum.ToString());

                    _iscomplate = true;
                    _cb(this);
                }
            }

        }
    }

    #endregion

    #region 自定义IHttpModule

    /*IIS配置（可重复添加）
     * <system.web>
     *   <httpModules>
     *       <add name="TestMyModule" type="MyTestMVC.TestMyModule, MyTestMVC" />
     *   </httpModules>
     * </system.web>
     * IIS经典模式配置
     * <system.webServer>
     *     <modules>
     *         <add name="TestMyModule" type="MyTestMVC.TestMyModule, MyTestMVC" preCondition="integratedMode" />
     *     </modules>
     * </system.webServer>
     */
    public class TestMyModule : IHttpModule
    {
        public void Dispose()
        {
            //throw new NotImplementedException();
        }

        public void Init(HttpApplication app)
        {
            //事件注册
            app.BeginRequest += app_BeginRequest;
            app.EndRequest += app_EndRequest;
        }

        void app_EndRequest(object sender, EventArgs e)
        {
            var app = (HttpApplication)sender;
            app.Context.Response.Write("请求结束");
        }

        void app_BeginRequest(object sender, EventArgs e)
        {
            var app = (HttpApplication)sender;
            app.Context.Response.Write("请求开始");
        }
    }

    #endregion

    public class FolderControllerFactory : DefaultControllerFactory
    {
        public override IController CreateController(RequestContext requestContext, string controllerName)
        {
            var controllerType = GetControllerType(requestContext, controllerName);
            // 如果controller不存在，替换为FolderController  
            if (controllerType == null)
            {
                //// 用路由字段动态构建路由变量  
                //var dynamicRoute = string.Join("/", requestContext.RouteData.Values.Values);
                //controllerName = "Folder";
                //controllerType = GetControllerType(requestContext, controllerName);
                //requestContext.RouteData.Values["Controller"] = controllerName;
                //requestContext.RouteData.Values["action"] = "Index";
                //requestContext.RouteData.Values["dynamicRoute"] = dynamicRoute;
            }
            IController controller = GetControllerInstance(requestContext, controllerType);
            return controller;
        }

        protected override Type GetControllerType(RequestContext requestContext, string controllerName)
        {
            var controllerType = base.GetControllerType(requestContext, controllerName);
            if (controllerType == null)
            {
                var MovieAssembly = (Assembly)HttpRuntime.Cache.Get("MovieAssembly");
                string Namespace = "";
                if (requestContext.RouteData.DataTokens["Namespaces"] != null)
                {
                    Namespace = (requestContext.RouteData.DataTokens["Namespaces"] as string[])[0];
                }
                //TestAreaDemo.Areas.Movie.Controllers
                var typeName = Namespace + ".Controllers." + controllerName + "Controller";
                controllerType = MovieAssembly.GetType(typeName);
            }
            return controllerType;
        }
    }
}