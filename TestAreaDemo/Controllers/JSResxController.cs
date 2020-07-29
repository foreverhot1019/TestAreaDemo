using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TestAreaDemo.Controllers
{
    /// <summary>
    /// 为前台输出对应语言的Json格式
    /// 包括 前端的错误信息
    /// </summary>
    public class JSResxController : Controller
    {
        // GET: JSResx
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Language(string CtrlName, string AreaName = "")
        {
            string Lang = (RouteData.Values["lang"] ?? "").ToString();
            if (string.IsNullOrEmpty(Lang))
            {
                Lang = "zh-cn";
            }
            else
                Lang = Lang.ToString().ToLower();
            if (string.IsNullOrEmpty(AreaName))
                AreaName = (Request.RequestContext.RouteData.DataTokens["area"] ?? "").ToString();
            return new JavascriptResourceResult(AreaName, CtrlName, Lang.Replace("-", "_"));
        }
    }
}