using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace TestAreaDemo
{
    public class UserAuthAttribute : AuthorizeAttribute
    {
        //代码顺序为：OnAuthorization-->AuthorizeCore-->HandleUnauthorizedRequest 
        //如果AuthorizeCore返回false时，才会走HandleUnauthorizedRequest 方法，并且Request.StausCode会返回401，401错误又对应了Web.config中的
        //<authentication mode="Forms">
        //<forms loginUrl="~/" timeout="2880" />
        //</authentication>
        //所有，AuthorizeCore==false 时，会跳转到 web.config 中定义的  loginUrl="~/"

        //在OnAuthorization验证是否有权限
        private bool isAllowed = true;

        /// <summary>
        /// 在过程请求授权时调用。
        /// </summary>
        /// <param name="filterContext">筛选器上下文，它封装用于 System.Web.Mvc.AuthorizeAttribute 的信息。</param>
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            //设置默认语言
            string lang = (filterContext.HttpContext.Request.UserLanguages == null || !filterContext.HttpContext.Request.UserLanguages.Any()) ? "zh-CN" : filterContext.HttpContext.Request.UserLanguages[0];
            var request = filterContext.RequestContext.HttpContext.Request;
            string controller = (filterContext.RouteData.Values["controller"] ?? "").ToString();
            string action = (filterContext.RouteData.Values["action"] ?? "").ToString();

            if (filterContext.RouteData.Values["lang"] != null && !string.IsNullOrWhiteSpace(filterContext.RouteData.Values["lang"].ToString()))
            {
                //从路由数据(url)里设置语言
                lang = filterContext.RouteData.Values["lang"].ToString();
                CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture(lang);
                //var tCulture = CultureInfo.InvariantCulture;
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
                    CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture(lang);
                    //var ss = Thread.CurrentContext.ContextProperties;
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
                    var area = (filterContext.RouteData.DataTokens["area"] ?? "").ToString();//获取Area前缀
                    if (!string.IsNullOrEmpty(area))
                        ReturnUrl = "/" + (filterContext.RouteData.Values["lang"] ?? lang) + "/" + area + "/" + controller + "/" + action + request.Url.Query;
                    filterContext.Result = new RedirectResult(ReturnUrl);
                    //}
                }
            }

            base.OnAuthorization(filterContext);
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            //return base.AuthorizeCore(httpContext);
            return true;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext.RequestContext.HttpContext.Request.IsAjaxRequest())
            {
                string ErrMsg = CommonLanguage.Language.ResourceManager.GetString("Unauthorized");
                var OJsonResult = new JsonResult();
                OJsonResult.Data = new { Success = false, ErrMsg = ErrMsg };
                OJsonResult.ContentType = "application/json";
                filterContext.Result = OJsonResult;
            }
            else
                base.HandleUnauthorizedRequest(filterContext);
        }

        /// <summary>
        /// ajax或者POST
        /// </summary>
        /// <param name="controllerContext"></param>
        /// <returns></returns>
        private bool IsAjaxOrPost(AuthorizationContext filterContext)
        {
            var request = filterContext.RequestContext.HttpContext.Request;
            return request.IsAjaxRequest() || request.HttpMethod == "POST";
        }
    }
}