using System.Web;
using System.Web.Mvc;

namespace TestAreaDemo
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            //filters.Add(new HandleErrorAttribute());
            //自定义 错误页
            filters.Add(new MyExceptionHandleFilter());
            filters.Add(new UserAuthAttribute());
        }
    }
}