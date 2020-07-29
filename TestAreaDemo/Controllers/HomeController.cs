using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace TestAreaDemo.Controllers
{
    public class HomeController : Controller
    {
        [Authorize]
        public ActionResult Index()
        {
            int CountApp = 0;
            var CountAppStr = Session["CountApp"]??"0";
            int.TryParse(CountAppStr.ToString(), out CountApp);
            Session["CountApp"] = ++CountApp;
            var userIdentity = (System.Security.Claims.ClaimsIdentity)User.Identity;
            var claims = userIdentity.Claims;
            var OClaim = claims.Where(x => x.Type == "Gender").FirstOrDefault();

            var AppUserManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var AppUser = AppUserManager.FindByNameAsync(User.Identity.Name).Result;

            var a = Request["id"] ?? "";
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            byte[] buffer = System.IO.File.ReadAllBytes(@"E:\项目\LocalTfs_60_186\TestArea\SourceCode\TestAreaDemo\TestAreaDemo.Movie\bin\Movie.dll");
            //Load assembly using byte array
            byte[] bufferMovie = System.IO.File.ReadAllBytes(Server.MapPath("/App_Data/Movie.dll"));
            Assembly assembly = Assembly.Load(bufferMovie);
            HttpRuntime.Cache.Insert("MovieAssembly", assembly);
            //System.Web.Compilation.BuildManager.AddReferencedAssembly(assembly);//必须在应用程序启动时 注册
            //var routes = System.Web.Routing.RouteTable.Routes;
            //var Movie_Default = routes.MapRoute(
            //    "Movie_Default",
            //    "Movie/{controller}/{action}/{apiId}",
            //    new { controller = "Movie", action = "Index", apiId = UrlParameter.Optional },
            //    new[] { "TestAreaDemo.Areas.Movie" });
            //Movie_Default.DataTokens["area"] = "Movie";

            //var LangMovie_Default = routes.MapRoute(
            //    "LangMovie_Default",
            //    "{lang}/Movie/{controller}/{action}/{apiId}",
            //    new { lang = "zh-CN", controller = "Movie", action = "Index", apiId = UrlParameter.Optional },
            //    new { lang = "zh-cn|zh-tw|zh-Hant|en-us|ja-JP" },
            //    new[] { "TestAreaDemo.Areas.Movie" });
            //LangMovie_Default.DataTokens["area"] = "Movie";
            ////LangMovie_Default.RouteHandler = new CultureRouteHandler();

            var ArrTypes = assembly.GetTypes();
            var CurrtAssm = Assembly.GetExecutingAssembly();
            var ArrCurrTypes = CurrtAssm.GetTypes();

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}