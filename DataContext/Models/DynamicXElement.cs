using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace DataContext.Models
{
    /// <summary>
    /// XML动态类
    /// </summary>
    public class DynamicXElement : DynamicObject
    {
        public DynamicXElement(XElement node)
        {
            this.XContent = node;
        }

        public DynamicXElement()
        {
        }

        public DynamicXElement(String name)
        {
            this.XContent = new XElement(name);
        }

        public XElement XContent
        {
            get;
            private set;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            XElement setNode = this.XContent.Element(binder.Name);
            if (setNode != null)
                setNode.SetValue(value);
            else
            {
                //creates an XElement without a value.
                if (value.GetType() == typeof(DynamicXElement))
                    this.XContent.Add(new XElement(binder.Name));
                else
                    this.XContent.Add(new XElement(binder.Name, value));
            }
            return true;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            XElement getNode = this.XContent.Element(binder.Name);
            if (getNode != null)
            {
                result = new DynamicXElement(getNode);
            }
            else
            {
                result = new DynamicXElement(binder.Name);
            }
            return true;
        }

        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            if (binder.Type == typeof(String))
            {
                result = this.XContent.Value;
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }
    }

    public class DynamicXml : DynamicObject, IEnumerable
    {
        private readonly List<XElement> _elements;

        public DynamicXml(string text)
        {
            var doc = XDocument.Parse(text);
            _elements = new List<XElement> { doc.Root };
        }

        protected DynamicXml(XElement element)
        {
            _elements = new List<XElement> { element };
        }

        protected DynamicXml(IEnumerable<XElement> elements)
        {
            _elements = new List<XElement>(elements);
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = null;
            if (binder.Name == "Value")
                result = _elements[0].Value;
            else if (binder.Name == "Count")
                result = _elements.Count;
            else
            {
                var attr = _elements[0].Attribute(XName.Get(binder.Name));
                if (attr != null)
                    result = attr;
                else
                {
                    var items = _elements.Descendants(XName.Get(binder.Name));
                    if (items == null || items.Count() == 0)
                        return false;
                    result = new DynamicXml(items);
                }
            }
            return true;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            int ndx = (int)indexes[0];
            result = new DynamicXml(_elements[ndx]);
            return true;
        }

        public IEnumerator GetEnumerator()
        {
            foreach (var element in _elements)
                yield return new DynamicXml(element);
        }

        public override string ToString()
        {
            if (_elements.Count == 1 && !_elements[0].HasElements)
            {
                return _elements[0].Value;
            }

            return string.Join("\n", _elements);
        }
    }
}