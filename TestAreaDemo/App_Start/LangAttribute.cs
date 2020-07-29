using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace TestAreaDemo
{
    public class LangAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// 在执行操作方法之前由 ASP.NET MVC 框架调用。
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            bool isSkipLocalization = filterContext.ActionDescriptor.IsDefined(typeof(WithoutLangAttribute), inherit: true) ||
                filterContext.ActionDescriptor.ControllerDescriptor.IsDefined(typeof(WithoutLangAttribute), inherit: true);

            if (!isSkipLocalization)
            {
                //设置默认语言
                string lang = filterContext.HttpContext.Request.UserLanguages[0];
                var request = filterContext.RequestContext.HttpContext.Request;
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
                if (!IsAjaxOrPost(filterContext))
                {
                    if (request.Url.AbsolutePath.ToLower().IndexOf(lang.ToLower()) < 0)
                    {
                        //if (controller.ToLower() == "home" || action.ToLower() == "login")
                        //{
                        //如果url中不包含语言设置则重定向到包含语言值设置的url里
                        string ReturnUrl = "/" + (filterContext.RouteData.Values["lang"] ?? lang) + "/" + controller + "/" + action + request.Url.Query;
                        filterContext.Result = new RedirectResult(ReturnUrl);
                        //}
                    }
                }

                base.OnActionExecuting(filterContext);
            }
        }

        /// <summary>
        /// ajax或者POST
        /// </summary>
        /// <param name="controllerContext"></param>
        /// <returns></returns>
        private bool IsAjaxOrPost(ActionExecutingContext filterContext)
        {
            var request = filterContext.RequestContext.HttpContext.Request;
            return request.IsAjaxRequest() || request.HttpMethod == "POST";
        }
    }

    /// <summary>
    /// 无需验证
    /// </summary>
    public class WithoutLangAttribute : Attribute
    {
    }
}