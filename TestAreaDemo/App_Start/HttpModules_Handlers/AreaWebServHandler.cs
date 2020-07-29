using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TestAreaDemo
{
    /// <summary>
    /// 区域WebService-Http处理接口
    /// 可以去除.asmx文件
    /// </summary>
    public class AreaWebServHandler:IHttpHandler
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

        /// <summary>
        /// 构造函数
        /// </summary>
        public AreaWebServHandler()
        {
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
            var type = typeof(WebService1);//WebService 类型（名称）
            //WebService处理工厂
            System.Web.Services.Protocols.WebServiceHandlerFactory oo = new System.Web.Services.Protocols.WebServiceHandlerFactory();
            IHttpHandler handler = (IHttpHandler)CoreGetHandler.Invoke(oo, new object[] { type, context, context.Request, context.Response });
            handler.ProcessRequest(context);
        }

        #endregion
    }
}