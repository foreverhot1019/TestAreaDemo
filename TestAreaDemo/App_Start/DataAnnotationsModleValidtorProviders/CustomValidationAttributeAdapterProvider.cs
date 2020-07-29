using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TestAreaDemo
{
    /// <summary>
    /// 数据验证适配器
    /// </summary>
    public class CustomValidationAttributeAdapterProvider : DataAnnotationsModelValidatorProvider
    {
        public CustomValidationAttributeAdapterProvider()
        {
            //var uiclu = System.Threading.Thread.CurrentThread.CurrentUICulture;
            //var clu = System.Threading.Thread.CurrentThread.CurrentCulture;
        }

        protected override IEnumerable<ModelValidator> GetValidators(ModelMetadata metadata, ControllerContext context, IEnumerable<Attribute> attributes)
        {
            var validators = attributes.OfType<ValidationAttribute>();
            var DisplayAttr = attributes.OfType<DisplayAttribute>().FirstOrDefault();
            //记录验证属性(去除验证属性)
            var allAttributes = attributes.Except(validators).ToList();
            ////已在MyModelMetadataProvider : DataAnnotationsModelMetadataProvider 中扩展
            //if (DisplayAttr != null)
            //{
            //    if (DisplayAttr.ResourceType == null)
            //    {
            //        var _type = System.Reflection.Assembly.GetAssembly(metadata.ContainerType).GetType(metadata.ContainerType.FullName.Replace("Models", "Views") + "s.Lang.Language");
            //        DisplayAttr.ResourceType = _type;
            //        DisplayAttr.ShortName = _type.GetProperty(metadata.PropertyName).GetValue(null, null).ToString();
            //    }
            //}
            //必填属性
            if (metadata.IsRequired)
            {
                var RequiredAttr = new RequiredAttribute();
                RequiredAttr.ErrorMessageResourceType = typeof(CommonLanguage.Language);
                RequiredAttr.ErrorMessageResourceName = "RequiredAttribute_ValidationError";
                allAttributes.Add(RequiredAttr);
            }
            //枚举
            if (metadata.ModelType.IsEnum)
            {
                var EnumAttr = new EnumDataTypeAttribute(metadata.ModelType);
                EnumAttr.ErrorMessageResourceType = typeof(CommonLanguage.Language);
                EnumAttr.ErrorMessageResourceName = "EnumDataTypeAttribute_TypeNeedsToBeAnEnum";
                allAttributes.Add(EnumAttr);
                ////EmailAddress/Enum/类
                //var DataTypeAttr = new DataTypeAttribute((DataType.e);
                //DataTypeAttr.ErrorMessageResourceType = typeof(CommonLanguage.Language);
                //DataTypeAttr.ErrorMessageResourceName = "DataTypeAttribute_ValidationError";
                //allAttributes.Add(DataTypeAttr);
            }

            foreach (ValidationAttribute validator in validators)
            {
                if (validator is RequiredAttribute)
                {
                    continue;//跳过
                    //if (validator.ErrorMessageResourceType != typeof(CommonLanguage.Language))
                    //{
                    //    validator.ErrorMessageResourceType = typeof(CommonLanguage.Language);
                    //    validator.ErrorMessageResourceName = "RequiredAttribute_ValidationError";
                    //    //var _type = System.Reflection.Assembly.GetAssembly(metadata.ContainerType).GetType(metadata.ContainerType.FullName.Replace("Models", "Views") + "s.Lang.Language");
                    //}
                    //allAttributes.Add(validator);
                    //continue;
                }
                if (validator is EnumDataTypeAttribute)
                {
                    continue;//跳过
                    //if (validator.ErrorMessageResourceType != typeof(CommonLanguage.Language))
                    //{
                    //    validator.ErrorMessageResourceType = typeof(CommonLanguage.Language);
                    //    validator.ErrorMessageResourceName = "EnumDataTypeAttribute_TypeNeedsToBeAnEnum";
                    //}
                    //allAttributes.Add(validator);
                    //continue;
                }
                if (validator is MinLengthAttribute)
                {
                    if (validator.ErrorMessageResourceType != typeof(CommonLanguage.Language))
                    {
                        validator.ErrorMessageResourceType = typeof(CommonLanguage.Language);
                        //StringLengthAttribute_ValidationError
                        validator.ErrorMessageResourceName = "MinLengthAttribute_ValidationError";
                    }
                    allAttributes.Add(validator);
                    continue;
                }
                if (validator is MaxLengthAttribute)
                {
                    if (validator.ErrorMessageResourceType != typeof(CommonLanguage.Language))
                    {
                        validator.ErrorMessageResourceType = typeof(CommonLanguage.Language);
                        //StringLengthAttribute_ValidationError
                        validator.ErrorMessageResourceName = "MaxLengthAttribute_ValidationError";
                    }
                    allAttributes.Add(validator);
                    continue;
                }
                if (validator is RangeAttribute)
                {
                    if (validator.ErrorMessageResourceType != typeof(CommonLanguage.Language))
                    {
                        validator.ErrorMessageResourceType = typeof(CommonLanguage.Language);
                        validator.ErrorMessageResourceName = "RangeAttribute_ValidationError";
                        //if (metadata.ModelType == typeof(int) || metadata.ModelType == typeof(long))
                        //else
                        //    validator.ErrorMessageResourceName = "ValidationDefault_FloatRange";
                    }
                    allAttributes.Add(validator);
                    continue;
                }
                if (validator is StringLengthAttribute)
                {
                    if (validator.ErrorMessageResourceType != typeof(CommonLanguage.Language))
                    {
                        StringLengthAttribute StrLenAttr = validator as StringLengthAttribute;
                        validator.ErrorMessageResourceType = typeof(CommonLanguage.Language);
                        if (StrLenAttr.MinimumLength > 0)
                            validator.ErrorMessageResourceName = "StringLengthAttribute_ValidationErrorIncludingMinimum";
                        else
                            validator.ErrorMessageResourceName = "StringLengthAttribute_ValidationError";
                    }
                    allAttributes.Add(validator);
                    continue;
                }
                if (validator is EmailAddressAttribute)
                {
                    if (validator.ErrorMessageResourceType != typeof(CommonLanguage.Language))
                    {
                        validator.ErrorMessageResourceType = typeof(CommonLanguage.Language);
                        validator.ErrorMessageResourceName = "EmailAddressAttribute_Invalid";
                    }
                    allAttributes.Add(validator);
                    continue;
                }
                if (validator is RegularExpressionAttribute)
                {
                    if (validator.ErrorMessageResourceType != typeof(CommonLanguage.Language))
                    {
                        validator.ErrorMessageResourceType = typeof(CommonLanguage.Language);
                        validator.ErrorMessageResourceName = "RegexAttribute_ValidationError";
                    }
                    allAttributes.Add(validator);
                    continue;
                }
                if (validator is System.ComponentModel.DataAnnotations.CompareAttribute)
                {
                    if (validator.ErrorMessageResourceType != typeof(CommonLanguage.Language))
                    {
                        //var CompareAttr = validator as CompareAttribute;
                        validator.ErrorMessageResourceType = typeof(CommonLanguage.Language);
                        validator.ErrorMessageResourceName = "CompareAttribute_MustMatch";
                    }
                    allAttributes.Add(validator);
                    continue;
                }
                if (validator is System.Web.Mvc.RemoteAttribute)
                {
                    if (validator.ErrorMessageResourceType != typeof(CommonLanguage.Language))
                    {
                        //var RemoteAttr = validator as System.Web.Mvc.RemoteAttribute;
                        validator.ErrorMessageResourceType = typeof(CommonLanguage.Language);
                        validator.ErrorMessageResourceName = "RemoteAttribute_NoUrlFound";
                    }
                    allAttributes.Add(validator);
                    continue;
                }
                ////EmailAddress/Enum/类
                //if (validator is DataTypeAttribute)
                //{
                //    if (validator.ErrorMessageResourceType != typeof(CommonLanguage.Language))
                //    {
                //        validator.ErrorMessageResourceType = typeof(CommonLanguage.Language);
                //        //StringLengthAttribute_ValidationError
                //        validator.ErrorMessageResourceName = "DataTypeAttribute_ValidationError";
                //    }
                //    allAttributes.Add(validator);
                //    continue;
                //}
            }
            return base.GetValidators(metadata, context, allAttributes);
        }
    }

    #region .Net Core 方法

    //services.AddSingleton <IValidationAttributeAdapterProvider, CustomValidationAttributeAdapterProvider> ();

    //using Microsoft.AspNetCore.Mvc.DataAnnotations;
    //using Microsoft.AspNetCore.Mvc.DataAnnotations.Internal;
    //using Microsoft.Extensions.Localization;
    //using System.ComponentModel.DataAnnotations;

    //public class CustomValidationAttributeAdapterProvider
    //    : ValidationAttributeAdapterProvider, IValidationAttributeAdapterProvider
    //{
    //    public CustomValidationAttributeAdapterProvider() { }

    //    IAttributeAdapter IValidationAttributeAdapterProvider.GetAttributeAdapter(
    //        ValidationAttribute attribute,
    //        IStringLocalizer stringLocalizer)
    //    {
    //        IAttributeAdapter adapter;
    //        if (attribute is RequiredAttribute)
    //        {
    //            adapter = new MyRequiredAttributeAdaptor((RequiredAttribute)attribute, stringLocalizer);
    //        }
    //        else if (attribute is EmailAddressAttribute)
    //        {
    //            attribute.ErrorMessage = "Invalid Email Address.";
    //            adapter = base.GetAttributeAdapter(attribute, stringLocalizer);
    //        }
    //        else if (attribute is CompareAttribute)
    //        {
    //            attribute.ErrorMessageResourceName = "InvalidCompare";
    //            attribute.ErrorMessageResourceType = typeof(Resources.ValidationMessages);
    //            var theNewattribute = attribute as CompareAttribute;
    //            adapter = new CompareAttributeAdapter(theNewattribute, stringLocalizer);
    //        }
    //        else
    //        {
    //            adapter = base.GetAttributeAdapter(attribute, stringLocalizer);
    //        }

    //        return adapter;
    //    }
    //}

    //public class MyRequiredAttributeAdaptor : AttributeAdapterBase<RequiredAttribute>
    //{
    //    public MyRequiredAttributeAdaptor(RequiredAttribute attribute, IStringLocalizer stringLocalizer)
    //        : base(attribute, stringLocalizer)
    //    {
    //    }

    //    public override void AddValidation(ClientModelValidationContext context)
    //    {
    //        if (context == null)
    //        {
    //            throw new ArgumentNullException(nameof(context));
    //        }

    //        MergeAttribute(context.Attributes, "data-val", "true");
    //        MergeAttribute(context.Attributes, "data-val-required", GetErrorMessage(context));
    //    }

    //    /// <inheritdoc />
    //    public override string GetErrorMessage(ModelValidationContextBase validationContext)
    //    {
    //        if (validationContext == null)
    //        {
    //            throw new ArgumentNullException(nameof(validationContext));
    //        }

    //        return GetErrorMessage(validationContext.ModelMetadata, validationContext.ModelMetadata.GetDisplayName());
    //    }
    //}

    #endregion
}