using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace DataContext.Extensions
{
    public static class LangHelper
    {
        /// <summary>
        /// 获取Resources资源
        /// </summary>
        /// <param name="htmlhelper"></param>
        /// <param name="CtrlName">控制器名称</param>
        /// <param name="key">Resources-字段名</param>
        /// <returns></returns>
        public static string GetLangbyKey(this HtmlHelper htmlhelper,string CtrlName, string key,string Area="")
        {
            var TypeName = "TestAreaDemo";
            if (!string.IsNullOrEmpty(Area))
            {
                if (!string.IsNullOrEmpty(CtrlName))
                    TypeName += ".Areas." + Area + ".Views." + CtrlName + ".Lang.Language";
                else
                    TypeName += ".Areas." + Area + "." + CtrlName + ".Language";
            }
            else
            {
                if (!string.IsNullOrEmpty(CtrlName))
                    TypeName += ".Views." + CtrlName + ".Lang.Language";
                else
                    TypeName = "Resources.Language";
            }
            System.Resources.ResourceManager ResourceManager;
            Assembly assem = Assembly.GetExecutingAssembly();
            Type resourceType = assem.GetType(TypeName);
            var prop = resourceType.GetProperty("ResourceManager",BindingFlags.Static);
            if (prop != null)
                ResourceManager = (System.Resources.ResourceManager)prop.GetValue(null);
            else
                return "";

            return ResourceManager.GetString(key);
        }

        //js定义多语言弹出框
        public static string LangOutJsVar(this HtmlHelper htmlhelper, string key)
        {
            //string lang = "zh-CN";

            //Thread.CurrentThread.CurrentUICulture = new CultureInfo(lang);
            //Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture;

            //Type resourceType = (Thread.CurrentThread.CurrentUICulture.Name == "zh-CN") ? typeof(WebApplication4.Resources.zh_CN) : typeof(WebApplication4.Resources.zh_TW);
            //PropertyInfo p = resourceType.GetProperty(key);
            //if (p != null)
            //    return string.Format("var {0} = '{1}'", key, p.GetValue(null, null).ToString());
            //else
            //    return string.Format("var {0} = '{1}'", key, "undefined");
            return "var a={}";
        }
    }
}