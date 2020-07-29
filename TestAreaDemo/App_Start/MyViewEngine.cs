using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages.Razor;

namespace TestAreaDemo
{
    public class MyViewEngine : RazorViewEngine
    {
        public MyViewEngine()
        {
        }

        public override ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache)
        {
            //var plugin = (controllerContext.RouteData.DataTokens["plugin"] ?? "").ToString();
            //if (plugin != "")
            //{
            //    var MovieAssembly = (Assembly)HttpRuntime.Cache.Get("MovieAssembly");
            //    Assembly.GetExecutingAssembly().Modules.ToList().AddRange(MovieAssembly.Modules);

            //    //RazorBuildProvider.CodeGenerationStarted += (object sender, EventArgs e) =>
            //    //{
            //    //    RazorBuildProvider provider = (RazorBuildProvider)sender;
            //    //    var MovieAssembly = (Assembly)HttpRuntime.Cache.Get("MovieAssembly");
            //    //    if (MovieAssembly != null)
            //    //    {
            //    //        provider.AssemblyBuilder.AddAssemblyReference(MovieAssembly);
            //    //    }
            //    //};
            //}

            return base.FindView(controllerContext, viewName, masterName, useCache);
        }

        protected override IView CreateView(ControllerContext controllerContext, string viewPath, string masterPath)
        {
            //var ck = controllerContext.HttpContext.Request.Cookies.Get("p_lang");
            //string lg = "";
            //if (ck != null)
            //{
            //    lg = ck.Value;
            //    if (lg.ToLower() == "en")
            //    {
            //        viewPath = viewPath.Replace("Views", "Views/en");
            //    }
            //}
            return base.CreateView(controllerContext, viewPath, masterPath);
        }

        protected override IView CreatePartialView(ControllerContext controllerContext, string partialPath)
        {
            var ck = controllerContext.HttpContext.Request.Cookies.Get("p_lang");
            string lg = "";
            if (ck != null)
            {
                lg = ck.Value;
                if (lg.ToLower() == "en")
                {
                    partialPath = partialPath.Replace("Views", "Views/en");
                }
            }
            return base.CreatePartialView(controllerContext, partialPath);
        }

        ///// <summary>
        ///// 给运行时编译的页面加了引用程序集。
        ///// </summary>
        ///// <param name="pluginName"></param>
        //private void CodeGeneration(string pluginName)
        //{
        //    RazorBuildProvider.CodeGenerationStarted += (object sender, EventArgs e) =>
        //    {
        //        RazorBuildProvider provider = (RazorBuildProvider)sender;

        //        if (plugin != null)
        //        {
        //            provider.AssemblyBuilder.AddAssemblyReference(plugin.Assembly);
        //        }
        //    };
        //}
    }
}