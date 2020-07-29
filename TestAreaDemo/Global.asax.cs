using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using RedisSessionProvider.Config;
using StackExchange.Redis;

namespace TestAreaDemo
{
    public class MvcApplication : System.Web.HttpApplication
    {
        public static ConfigurationOptions redisConfigOpts { get; set; }

        // Application_Init：在应用程序被实例化或第一次被调用时，该事件被触发对于所有的HttpApplication 对象实例，它都会被调用
        // Application_Disposed：在应用程序被销毁之前触发这是清除以前所用资源的理想位置
        // Application_Error：当应用程序中遇到一个未处理的异常时，该事件被触发
        // Application_Start：在HttpApplication 类的第一个实例被创建时，该事件被触发它允许你创建可以由所有HttpApplication 实例访问的对象
        // Application_End：在HttpApplication 类的最后一个实例被销毁时，该事件被触发在一个应用程序的生命周期内它只被触发一次
        // Application_AuthenticateRequest：在安全模块建立起当前用户的有效的身份时，该事件被触发在这个时候，用户的凭据将会被验证
        // Application_AuthorizeRequest：当安全模块确认一个用户可以访问资源之后，该事件被触发
        // Session_Start：在一个新用户访问应用程序 Web 站点时，该事件被触发
        // Session_End：在一个用户的会话超时结束或他们离开应用程序 Web 站点时，该事件被触发
        // Application_BeginRequest：在接收到一个应用程序请求时触发对于一个请求来说，它是第一个被触发的事件，请求一般是用户输入的一个页面请求（URL）
        // Application_EndRequest：针对应用程序请求的最后一个事件
        // Application_PreRequestHandlerExecute：在 ASP.NET 页面框架开始执行诸如页面或 Web 服务之类的事件处理程序之前，该事件被触发
        // Application_PostRequestHandlerExecute：在 ASP.NET 页面框架结束执行一个事件处理程序时，该事件被触发
        // Applcation_PreSendRequestHeaders：在 ASP.NET 页面框架发送 HTTP 头给请求客户（浏览器）时，该事件被触发·Application_PreSendContent：在 ASP.NET 页面框架发送内容给请求客户（浏览器）时，该事件被触发
        // Application_AcquireRequestState：在 ASP.NET 页面框架得到与当前请求相关的当前状态（Session 状态）时，该事件被触发
        // Application_ReleaseRequestState：在 ASP.NET 页面框架执行完所有的事件处理程序时，该事件被触发这将导致所有的状态模块保存它们当前的状态数据
        // Application_ResolveRequestCache：在 ASP.NET 页面框架完成一个授权请求时，该事件被触发它允许缓存模块从缓存中为请求提供服务，从而绕过事件处理程序的执行
        // Application_UpdateRequestCache：在 ASP.NET 页面框架完成事件处理程序的执行时，该事件被触发，从而使缓存模块存储响应数据，以供响应后续的请求时使用
        // 这个事件列表看起来好像多得吓人，但是在不同环境下这些事件可能会非常有用
        // 使用这些事件的一个关键问题是知道它们被触发的顺序Application_Init 和Application_Start 事件在应用程序第一次启动时被触发一次相似地，
        // Application_Disposed 和 Application_End 事件在应用程序终止时被触发一次此外，
        // 基于会话的事件（Session_Start 和 Session_End）只在用户进入和离开站点时被使用其余的事件则处理应用程序请求，这些事件被触发的顺序是：
        // Application_BeginRequest
        // Application_AuthenticateRequest
        // Application_AuthorizeRequest
        // Application_ResolveRequestCache
        // Application_AcquireRequestState
        // Application_PreRequestHandlerExecute         
        // Application_PreSendRequestHeaders
        // Application_PreSendRequestContent
        // <<执行代码>>
        // Application_PostRequestHandlerExecute
        // Application_ReleaseRequestState
        // Application_UpdateRequestCache
        // Application_EndRequest

        protected void Application_Start()
        {
            var ArrAssembly = AppDomain.CurrentDomain.GetAssemblies();
            //foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
            //{
            //    foreach (Type t in a.GetTypes())
            //    {
            //        // ... do something with 't' ...
            //    }
            //}

            AreaRegistration.RegisterAllAreas();

            //默认错误信息资源文件
            ClientDataTypeModelValidatorProvider.ResourceClassKey = "CommonLanguage.MVCLang.MvcResources";
            DefaultModelBinder.ResourceClassKey = "CommonLanguage.MVCLang.MvcResources";

            ModelMetadataProviders.Current = new MyModelMetadataProvider();//数据显示 适配器

            #region Model数据验证 适配器

            ModelValidatorProviders.Providers.Clear();
            ////ModelValidatorProviders.Providers.Add(new MyModelValidatorProvider());//Model验证适配器
            ModelValidatorProviders.Providers.Add(new CustomValidationAttributeAdapterProvider());//自定义数据验证 适配器

            ////注册默认适配器
            //DataAnnotationsModelValidatorProvider.RegisterDefaultAdapter(typeof(CustomValidationAttributeAdapterProvider));
            ////自定义验证
            //DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(ValidInteger), typeof(ValidIntegerValidator));
            //DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(ValidDecimal), typeof(ValidDecimalValidator));

            #endregion

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            ////自定义控制器 工厂
            //ControllerBuilder.Current.SetControllerFactory(new FolderControllerFactory());
            //全局注册自定义视图
            //ViewEngines.Engines.Clear();
            //ViewEngines.Engines.Add(new MyViewEngine());

            #region Session 分布式插件

            //var RedisConStr = System.Configuration.ConfigurationManager.ConnectionStrings["RedisExchangeHosts"]?.ConnectionString;
            //redisConfigOpts = ConfigurationOptions.Parse(RedisConStr);

            //RedisSessionProvider.Config.RedisConnectionConfig.GetSERedisServerConfig = (HttpContextBase context) =>
            //{
            //    return new KeyValuePair<string, StackExchange.Redis.ConfigurationOptions>(
            //        "DefaultConnection",
            //        redisConfigOpts);
            //};

            #endregion
        }

        /// <summary>
        /// 在接收到一个应用程序请求时触发对于一个请求来说，它是第一个被触发的事件，请求一般是用户输入的一个页面请求（URL）
        /// </summary>
        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            try
            {
                ////var context = HttpContext.Current;
                //HttpApplication application = (HttpApplication)sender;
                //HttpContext context = application.Context;
                //HttpContextWrapper contextBase = new HttpContextWrapper(context);
                //string filePath = context.Request.FilePath;
                //string fileExtension = VirtualPathUtility.GetExtension(filePath);
                //var ArrRoute = RouteTable.Routes.Select(x => x.GetRouteData(contextBase));

                //var RouteData = RouteTable.Routes.GetRouteData(contextBase);
                //Object AreaName = "";
                //RouteData.DataTokens.TryGetValue("area", out AreaName);
                //if (context.Request.PhysicalPath.ToLower().IndexOf(".asmx") > 0)
                //{
                //    var UrlStr = context.Request.Url.ToString().ToLower().Replace("/" + AreaName.ToString().ToLower(), "/Areas/" + AreaName);
                //    context.Response.Redirect(UrlStr);
                //    context.Response.End();
                //}
                var areaNames = RouteTable.Routes.OfType<Route>()
                .Where(d => d.DataTokens != null && d.DataTokens.ContainsKey("area"))
                .Select(r => r.DataTokens["area"]).ToArray();
            }
            catch (Exception ex)
            {

            }
        }

        private void RouteCulture()
        {
            var filterContext = Request.RequestContext;
            //设置默认语言
            string lang = (Request.UserLanguages == null || !Request.UserLanguages.Any()) ? "zh-CN" : Request.UserLanguages[0];
            var request = Request.RequestContext.HttpContext.Request;
            string controller = (filterContext.RouteData.Values["controller"] ?? "").ToString();
            string action = (filterContext.RouteData.Values["action"] ?? "").ToString();

            if (filterContext.RouteData.Values["lang"] != null && !string.IsNullOrWhiteSpace(filterContext.RouteData.Values["lang"].ToString()))
            {
                //从路由数据(url)里设置语言
                lang = filterContext.RouteData.Values["lang"].ToString();
                Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture(lang);
            }
            else
            {
                var OCultureInfo = Thread.CurrentThread.CurrentUICulture;
                //从cookie里读取语言设置
                var cookie = filterContext.HttpContext.Request.Cookies["lang.CurrentUICulture"];
                if (cookie != null && cookie.Value != "")
                {
                    //根据cookie设置语言
                    lang = cookie.Value;
                    Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture(lang);
                    var ss = Thread.CurrentContext.ContextProperties;
                    //System.Web.WebPages.Resources.Culture;
                    //把语言值设置到路由值里
                    filterContext.RouteData.Values["lang"] = lang;

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
            filterContext.HttpContext.Response.SetCookie(_cookie);

            //if (request.HttpMethod != "POST")
            if (!(Request.RequestContext.HttpContext.Request.IsAjaxRequest() || Request.HttpMethod != "POST"))
            {
                if (request.Url.AbsolutePath.ToLower().IndexOf(lang.ToLower()) < 0)
                {
                    //if (controller.ToLower() == "home" || action.ToLower() == "login")
                    //{
                    //如果url中不包含语言设置则重定向到包含语言值设置的url里
                    //string ReturnUrl = "/" + (filterContext.RouteData.Values["lang"] ?? lang) + "/" + controller + "/" + action + request.Url.Query;
                    Response.RedirectToRoute(filterContext.RouteData.Values);
                    //}
                }
            }
        }

        /// <summary>
        /// 为类添加 多语言
        /// </summary>
        public void AddLangToModel()
        {
            //foreach (var assemblyName in Assembly.GetExecutingAssembly())
            //{
            Assembly assembly = Assembly.GetExecutingAssembly();
            var ArrTypes = assembly.GetTypes();
            foreach (var type in ArrTypes.Where(x => x.Name == "Message"))
            {
                //if (type.GetInterface("DbEntity", false) != null)
                //{
                var ArrPro = type.GetProperties();
                foreach (var pri in ArrPro)
                {
                    var attributes = pri.GetCustomAttributes();
                    var DisplayAttr = attributes.OfType<System.ComponentModel.DataAnnotations.DisplayAttribute>().FirstOrDefault();
                    if (DisplayAttr != null)
                    {
                        if (DisplayAttr.ResourceType == null)
                        {
                            var _type = assembly.GetType(type.FullName.Replace("Models", "Views") + "s.Lang.Language");
                            DisplayAttr.ResourceType = _type;
                            DisplayAttr.ShortName = _type.GetProperty(pri.Name).GetValue(null, null).ToString();
                        }
                    }
                }
                //}
            }
            //}
        }
    }
}