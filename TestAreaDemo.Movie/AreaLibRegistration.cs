using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace TestAreaDemo.Areas.Movie
{
    public class AreaLibRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Movie";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.Routes.IgnoreRoute("Movie/{*x}", new { x = @".*\.asmx(/.*)?" });
            //context.MapRoute("Movie_WebSrv",
            //    "Movie/{*x}", new { x = @".asmx(/.*)?" },);
            context.MapRoute(
                "Movie_Default",
                "Movie/{controller}/{action}/{apiId}",
                new { controller = "Movie", action = "Index", apiId = UrlParameter.Optional },
                new string[] { "TestAreaDemo.Areas.Movie.Controllers" });
            context.MapRoute(
                "LangMovie_Default",
                "{lang}/Movie/{controller}/{action}/{apiId}",
                new { lang = "zh-CN", controller = "Movie", action = "Index", apiId = UrlParameter.Optional },
                new { lang = "zh-CN|zh-TW|zh-Hant|en-US|ja-JP" },
                new string[] { "TestAreaDemo.Areas.Movie.Controllers" });
        }
    }
}