using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;
using TestAreaDemo.Models;

namespace TestAreaDemo
{
    /// <summary>
    /// 自定义 输出语言的JavaScriptResult格式
    /// </summary>
    public sealed class JavascriptResourceResult : JavaScriptResult
    {
        private string AjaxLangPath { get; set; }
        private string LangPath { get; set; }

        /// <summary>
        /// 输出语言的 JSON格式
        /// </summary>
        /// <param name="AreaName">区域名称</param>
        /// <param name="ControllerName">控制器</param>
        /// <param name="lang">语言种类</param>
        public JavascriptResourceResult(string AreaName, string ControllerName, string lang)
        {
            string ViewsPath = (string.IsNullOrEmpty(AreaName) ? "" : "/Areas/" + AreaName) + "/Views/";
            AjaxLangPath = HttpContext.Current.Server.MapPath(ViewsPath + ControllerName + "/Lang/AjaxLang.xml");
            LangPath = HttpContext.Current.Server.MapPath(ViewsPath + ControllerName + "/Lang/Language.resx");

            string baselangStr = GetBaseLangString(lang);
            string ajaxLangStr = GetAjaxLangString(AjaxLangPath, lang);
            this.Script = string.Format("var language = {{lang:'" + lang + "',baselang :{{{0}}},ajaxlang:{1} }};", baselangStr, ajaxLangStr);
        }

        private string GetNamespaceDefinitionString(string javascriptObjectName)
        {
            var names = javascriptObjectName.Split('.');

            var namespaces = names.Take(names.Length - 1).Select((x, index) =>
            {
                return String.Join(".", names.Take(index + 1).ToArray());
            });

            var sb = new StringBuilder();
            sb.AppendFormat("var {0} = {0} || {{}}", namespaces.First());
            foreach (var item in namespaces.Skip(1))
            {
                sb.AppendFormat("{0} = {0} || {{}}", item);
            }

            return sb.ToString();
        }

        /// <summary>
        /// 获取 Resource.Language
        /// </summary>
        /// <returns></returns>
        private string GetBaseLangString(string lang)
        {
            var resourceType = typeof(CommonLanguage.Language);
            List<string> ArrKeyVal = new List<string>();
            var OCultureInfo = System.Threading.Thread.CurrentThread.CurrentUICulture;
            if (OCultureInfo == null)
            {
                if (!string.IsNullOrEmpty(lang))
                    lang = HttpContext.Current.Request.UserLanguages[0];
                OCultureInfo = System.Globalization.CultureInfo.CreateSpecificCulture(lang);
            }
            else
                lang = OCultureInfo.Name;

            var BaseLangProp = HttpRuntime.Cache[resourceType.FullName + "_Prop_" + lang.ToLower()];
            if (BaseLangProp == null)
            {
                var ArrResTypeProp = HttpRuntime.Cache[resourceType.FullName];
                //System.Resources.ResourceManager rm = new global::System.Resources.ResourceManager(resourceType);
                //System.Resources.ResourceManager rm = (System.Resources.ResourceManager)resourceType.GetProperty("ResourceManager").GetValue(null);
                System.Resources.ResourceManager rm = CommonLanguage.Language.ResourceManager;
                if (ArrResTypeProp == null)
                {
                    ArrResTypeProp = resourceType.GetProperties(BindingFlags.Static | BindingFlags.NonPublic);
                    HttpRuntime.Cache.Insert(resourceType.FullName, ArrResTypeProp);
                }
                var ArrNoProp = new String[] { "ResourceManager", "Culture" };
                ArrKeyVal = ((IEnumerable<System.Reflection.PropertyInfo>)ArrResTypeProp).Where(x => !ArrNoProp.Contains(x.Name)).
                Select(x =>
                {
                    return string.Format("{0}: '{1}'", x.Name, rm.GetString(x.Name, OCultureInfo));
                }).ToList();
                HttpRuntime.Cache.Insert(resourceType.FullName + "_Prop", ArrKeyVal);
            }
            else
                ArrKeyVal = (List<string>)BaseLangProp;

            return String.Join(",", ArrKeyVal);
        }

        /// <summary>
        /// 获取AjaxLang
        /// </summary>
        /// <returns></returns>
        private string GetAjaxLangString(string AjaxLangPath, string lang)
        {
            if (string.IsNullOrEmpty(lang))
                lang = System.Threading.Thread.CurrentThread.CurrentUICulture.Name.ToString();

            string retStr = "";

            string AjaxLangStr = string.Empty;
            var ObjCacheLang = HttpRuntime.Cache[AjaxLangPath];
            if (ObjCacheLang != null)
                AjaxLangStr = (string)ObjCacheLang;
            XmlDocument XDoc = new XmlDocument();
            if (string.IsNullOrEmpty(AjaxLangStr))
            {
                XDoc.Load(AjaxLangPath);
                AjaxLangStr = XDoc.OuterXml;
                HttpRuntime.Cache.Add(
                    AjaxLangPath,
                    AjaxLangStr,
                    new System.Web.Caching.CacheDependency(AjaxLangPath),
                    System.Web.Caching.Cache.NoAbsoluteExpiration, //从不过期
                    System.Web.Caching.Cache.NoSlidingExpiration, //禁用可调过期
                    System.Web.Caching.CacheItemPriority.Default, null);
            }
            else
                XDoc.LoadXml(AjaxLangStr);
            //DynamicXml ODycXElem = new DynamicXml(XDoc.DocumentElement.OuterXml);
            //dynamic dyn = Newtonsoft.Json.JsonConvert.DeserializeObject<ExpandoObject>(jsonText);
            //var Langdynamic = (dyn.AjaxLang as IDictionary<String, object>)[lang];
            var XNode = XDoc.SelectSingleNode("AjaxLang/" + lang);
            retStr = Newtonsoft.Json.JsonConvert.SerializeXmlNode(XNode);

            return retStr;
        }
    }
}