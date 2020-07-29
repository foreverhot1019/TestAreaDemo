using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TestAreaDemo
{
    /// <summary>
    /// 数字验证属性
    /// IClientValidatable 前台验证返回值
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false)]
    public class ValidInteger : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || value.ToString().Length == 0)
            {
                return ValidationResult.Success;
            }
            int i;

            return !int.TryParse(value.ToString(), out i) ? new ValidationResult(ErrorMessage) : ValidationResult.Success;
        }

        ////前端验证返回值 IClientValidatable
        ////参考：http://cn.voidcc.com/question/p-earyoamq-wc.html
        ////参考：https://blogs.msdn.microsoft.com/simonince/2011/02/04/conditional-validation-in-asp-net-mvc-3/
        ////参考：https://haacked.com/archive/2009/11/19/aspnetmvc2-custom-validation.aspx/
        //public System.Collections.Generic.IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        //{
        //    ModelClientValidationRule modelClientValidationRule = new ModelClientValidationRule()
        //    {
        //        ErrorMessage = FormatErrorMessage(metadata.DisplayName),
        //        ValidationType = "validinteger"
        //    };
        //    modelClientValidationRule.ValidationParameters.Add("validinteger", ValidInteger);
        //    yield return modelClientValidationRule;
        //}
    }

    /// <summary>
    /// 数字验证属性 验证代理
    /// </summary>
    public class ValidIntegerValidator : DataAnnotationsModelValidator<ValidInteger>
    {
        int _intVal;
        string _message;

        public ValidIntegerValidator(ModelMetadata metadata, ControllerContext context, ValidInteger attribute)
            : base(metadata, context, attribute)
        {
            var propertyName = metadata.PropertyName;
            var intVal = context.HttpContext.Request.Form[propertyName] ?? "";
            int.TryParse(intVal, out _intVal);
            _message = attribute.ErrorMessage;
            if (!attribute.IsValid(intVal))
            {
                context.Controller.ViewData.ModelState[propertyName].Errors.Clear();
                context.Controller.ViewData.ModelState[propertyName].Errors.Add(attribute.ErrorMessage);
            }
        }

        /// <summary>
        /// 返回前端验证
        /// 也可在ValidationAttribute 中 实现接口IClientValidatable
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules()
        {
            var rule = new ModelClientValidationRule
            {
                ErrorMessage = _message,
                ValidationType = "validinteger"
            };
            rule.ValidationParameters.Add("validinteger", _intVal);

            return new[] { rule };
        }
    }

}