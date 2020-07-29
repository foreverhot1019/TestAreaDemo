using System;
using System.Collections;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Routing;
using System.Web.Routing;

namespace TestAreaDemo
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            //routes.MapMvcAttributeRoutes();//允许Action中制定Route

            routes.MapRoute(
                name: "Language",
                url: "{lang}/{controller}/{action}/{id}",
                defaults: new { lang = "zh-CN", controller = "Home", action = "Index", id = UrlParameter.Optional },
                constraints: new { lang = "zh-CN|zh-TW|zh-Hant|en-US|ja-JP" }, //限制可输入的语言项 new { lang = "^[a-zA-Z]{2}(-[a-zA-Z]{2})?$" },
                namespaces:new string[] { "TestAreaDemo.Controllers" }//控制器命名控件
            ).RouteHandler = new MultiLangRouteHandler();

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                namespaces:new string[] { "TestAreaDemo.Controllers" }//控制器命名控件
            );

            //var Movie_Default = routes.MapRoute(
            //    "Movie_Default",
            //    "Movie/{controller}/{action}/{apiId}",
            //    new { controller = "Movie", action = "Index", apiId = UrlParameter.Optional },
            //    new[] { "TestAreaDemo.Areas.Movie" });
            //Movie_Default.DataTokens["area"] = "Movie";
            //Movie_Default.DataTokens["plugin"] = "Movie";

            //var LangMovie_Default = routes.MapRoute(
            //    "LangMovie_Default",
            //    "{lang}/Movie/{controller}/{action}/{apiId}",
            //    new { lang = "zh-CN", controller = "Movie", action = "Index", apiId = UrlParameter.Optional },
            //    new { lang = "zh-cn|zh-tw|zh-Hant|en-us|ja-JP" },
            //    new[] { "TestAreaDemo.Areas.Movie" });
            //LangMovie_Default.DataTokens["area"] = "Movie";
            //LangMovie_Default.DataTokens["plugin"] = "Movie";
            ////LangMovie_Default.RouteHandler = new CultureRouteHandler();
            
            //routes.Add(new Route(
            //    //name: "Language",
            //    url: "{lang}/{controller}/{action}/{id}",
            //    defaults:new RouteValueDictionary(new
            //    {
            //        controller = "Investor",
            //        action = "InvestorIndex",
            //        id = UrlParameter.Optional,
            //        lang = "zh-CN"
            //    }),
            //    constraints:  new RouteValueDictionary(new 
            //    { 
            //        lang = "zh-CN|zh-TW|zh-Hant|en-US|ja-JP" 
            //    }),
            //    routeHandler:new MultiLangRouteHandler()
            //));
        }
    }
}
