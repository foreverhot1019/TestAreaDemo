using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TestAreaDemo.App_Start
{
    //[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    //public abstract class ValidatorBaseAttribute : ValidationAttribute, IClientValidatable
    //{

    //    public string RuleName { get; set; }
    //    public string MessageCategory { get; private set; }
    //    public string MessageId { get; private set; }
    //    public string Culture { get; set; }

    //    public ValidatorBaseAttribute(MessageManager messageManager, string messageCategory, string messageId, params object[] args)
    //        : base(() => messageManager.FormatMessage(messageCategory, messageId, args))
    //    {
    //        this.MessageCategory = messageCategory;
    //        this.MessageId = messageId;
    //    }

    //    public ValidatorBaseAttribute(string messageCategory, string messageId, params object[] args)
    //        : this(MessageManagerFactory.GetMessageManager(), messageCategory, messageId, args)
    //    { }

    //    public virtual bool Match(ValidatorContext context, IEnumerable<ValidatorBaseAttribute> validators)
    //    {
    //        if (!string.IsNullOrEmpty(this.RuleName))
    //        {
    //            if (this.RuleName != context.RuleName)
    //            {
    //                return false;
    //            }
    //        }

    //        if (!string.IsNullOrEmpty(this.Culture))
    //        {
    //            if (string.Compare(this.Culture, context.Culture.Name, true) != 0)
    //            {
    //                return false;
    //            }
    //        }

    //        if (string.IsNullOrEmpty(this.Culture))
    //        {
    //            if (validators.Any(validator => validator.GetType() == this.GetType() && string.Compare(validator.Culture, context.Culture.Name, true) == 0))
    //            {
    //                return false;
    //            }
    //        }
    //        return true;
    //    }
    //    public abstract IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context);
    //    private object typeId;
    //    public override object TypeId
    //    {
    //        get { return (null == typeId) ? (typeId = new object()) : typeId; }
    //    }
    //}

    //public class ExtendedDataAnnotationsModelValidatorProvider : DataAnnotationsModelValidatorProvider
    //{
    //    protected override IEnumerable<ModelValidator> GetValidators(ModelMetadata metadata, ControllerContext context, IEnumerable<Attribute> attributes)
    //    {
    //        var validators = attributes.OfType<ValidationAttribute>();
    //        var allAttributes = attributes.Except(validators).ToList();
    //        foreach (ValidationAttribute validator in validators)
    //        {
    //            if (validator.Match(ValidationContext.Current, validators))
    //            {
    //                allAttributes.Add(validator);
    //            }
    //        }
    //        return base.GetValidators(metadata, context, allAttributes);
    //    }
    //}
}