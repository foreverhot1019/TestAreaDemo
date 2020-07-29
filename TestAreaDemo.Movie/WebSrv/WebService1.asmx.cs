using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

namespace TestAreaDemo.Areas.Movie
{
    /// <summary>
    /// WebService1 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://localhost:51297/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消注释以下行。 
    // [System.Web.Script.Services.ScriptService]
    public class WebService1 : System.Web.Services.WebService,IHttpHandler
    {
        /// <summary>
        /// .Net默认WebService-soap协议 处理方法
        /// </summary>
        public static readonly System.Reflection.MethodInfo CoreGetHandler = typeof(System.Web.Services.Protocols.WebServiceHandlerFactory).
            GetMethod("CoreGetHandler",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
            null,
            new Type[] { typeof(Type), typeof(HttpContext), typeof(HttpRequest), typeof(HttpResponse) },
            null);

        [WebMethod]
        public string HelloWorld()
        {
            return "Hello World-Movie";
        }

        #region IHttpHandler 成员

        /// <summary>
        /// 
        /// </summary>
        public bool IsReusable
        {
            get { return false; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public void ProcessRequest(HttpContext context)
        {
            System.Web.Services.Protocols.WebServiceHandlerFactory oo = new System.Web.Services.Protocols.WebServiceHandlerFactory();
            IHttpHandler handler = (IHttpHandler)CoreGetHandler.Invoke(oo, new object[] { typeof(WebService1), context, context.Request, context.Response });
            handler.ProcessRequest(context);
        }

        #endregion
    }
}