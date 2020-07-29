using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using DataContext.Models;

namespace DataContext.Extensions
{
    public class DynamicHelper
    {
        public static string ToXml(dynamic dynamicObject)
        {
            DynamicXElement xmlNode = dynamicObject;
            return xmlNode.XContent.ToString();
        }

        public static dynamic ToObject(string xml, dynamic dynamicResult)
        {
            XElement element = XElement.Parse(xml);
            dynamicResult = new DynamicXElement(element);
            return dynamicResult;
        }

        public static dynamic ToObject(string xml)
        {
            XElement element = XElement.Parse(xml);
            dynamic dynamicResult = new DynamicXElement(element);
            return dynamicResult;
        }
    }
}