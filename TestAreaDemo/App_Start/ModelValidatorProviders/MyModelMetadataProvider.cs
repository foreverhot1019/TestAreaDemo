using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace TestAreaDemo
{
    /// <summary>
    /// 数据显示-适配器
    /// </summary>
    public class MyModelMetadataProvider : DataAnnotationsModelMetadataProvider
    {
        protected override ModelMetadata CreateMetadata(IEnumerable<Attribute> attributes, Type containerType, Func<object> modelAccessor, Type modelType, string propertyName)
        {
            ModelMetadata metadata = base.CreateMetadata(attributes, containerType, modelAccessor, modelType, propertyName);
            if (containerType != null)
            {
                var TypeName = containerType.FullName.Replace("Models", "Views") + "s.Lang.Language";
                var _type = CacheHelper.GetCache(TypeName) as Type;
                if (_type == null) {
                    _type = Assembly.GetAssembly(containerType).GetType(TypeName);
                    if (_type != null)
                        CacheHelper.SetCache(TypeName, _type);
                }
                if (_type != null)
                {
                    List<PropertyInfo> ArrProperty = CacheHelper.GetCache(TypeName + "_Property") as List<PropertyInfo>;
                    if (ArrProperty == null)
                    {
                        ArrProperty = _type.GetProperties().ToList();
                        CacheHelper.SetCache(TypeName + "_Property", ArrProperty);
                    }
                    var Property = ArrProperty.Where(x => x.Name == propertyName).FirstOrDefault();
                    if (Property != null)
                        metadata.DisplayName = Property.GetValue(null, null).ToString();
                }
            }
            return metadata;
        }
    }
}