using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TestAreaDemo
{
    /// <summary>
    /// 自定义 Attribute和Attribute验证
    /// </summary>
    public class SameAsAttribute : ValidationAttribute
    {
        public string Property { get; set; }
        public SameAsAttribute(string Property)
        {
            this.Property = Property;

        }
        public override bool IsValid(object value)
        {
            //Any additional validation logic specific to the property can go here.
            return true;
        }
    }

    public class SameAsValidator : DataAnnotationsModelValidator
    {
        public SameAsValidator(ModelMetadata metadata, ControllerContext context, ValidationAttribute attribute)
            : base(metadata, context, attribute)
        {

        }

        public override IEnumerable<ModelValidationResult> Validate(object container)
        {
            var dependentField = Metadata.ContainerType.GetProperty
             (((SameAsAttribute)Attribute).Property);
            var field = Metadata.ContainerType.GetProperty(this.Metadata.PropertyName);
            if (dependentField != null && field != null)
            {
                object dependentValue = dependentField.GetValue(container, null);
                object value = field.GetValue(container, null);
                if ((dependentValue != null && dependentValue.Equals(value)))
                {
                    if (!Attribute.IsValid(this.Metadata.Model))
                    {
                        yield return new ModelValidationResult { Message = ErrorMessage };
                    }
                }
                else

                    yield return new ModelValidationResult { Message = ErrorMessage };
            }
        }
    }

}