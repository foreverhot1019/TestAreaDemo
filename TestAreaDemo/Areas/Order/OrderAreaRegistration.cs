using System.Web.Mvc;

namespace TestAreaDemo.Areas.Order
{
    public class OrderAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Order";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Order_lang",
                "{lang}/Order/{controller}/{action}/{id}",
                new { lang="zh-cn",action = "Index", id = UrlParameter.Optional },
                constraints: new { lang = "zh-cn|zh-tw|en-us" } //限制可输入的语言项 new { lang = "^[a-zA-Z]{2}(-[a-zA-Z]{2})?$" },
            );
            context.MapRoute(
                "Order_default",
                "Order/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}