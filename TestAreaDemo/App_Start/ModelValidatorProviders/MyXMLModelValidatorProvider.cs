//using System;
//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
//using System.IO;
//using System.Linq;
//using System.Reflection;
//using System.Web;
//using System.Web.Mvc;
//using System.Xml;
//using System.Xml.Linq;

//namespace TestAreaDemo
//{
//    public class MyXMLModelValidatorProvider : ModelValidatorProvider
//    {
//        // 用来保存System.ComponentModel.DataAnnotations中已经存在的验证规则，也就是MVC自带的Required等验证规则， 因为我们只是验证规则的"源"不一样，一个是代码中，一个是xml，但是验证过程是一样的，所以要重用MVC中的已经写好的验证。
//        private readonly Dictionary<string, Type> _validatorTypes;

//        private readonly string _xmlFolderPath = HttpContext.Current.Server.MapPath("~//Content//Validation//Rules");

//        public MyXMLModelValidatorProvider()
//        {
//            _validatorTypes = Assembly.Load("System.ComponentModel.DataAnnotations").GetTypes()
//                .Where(t => t.IsSubclassOf(typeof(ValidationAttribute)))
//                .ToDictionary(t => t.Name, t => t);
//        }

//        #region Stolen from DataAnnotationsModelValidatorProvider
//        // delegate that converts ValidationAttribute into DataAnnotationsModelValidator
//        internal static DataAnnotationsModelValidationFactory DefaultAttributeFactory =
//            (metadata, context, attribute) => new DataAnnotationsModelValidator(metadata, context, attribute);

//        internal static Dictionary<Type, DataAnnotationsModelValidationFactory> AttributeFactories =
//            new Dictionary<Type, DataAnnotationsModelValidationFactory>{
//                {
//                    typeof (RangeAttribute),( metadata, context, attribute)=>
//                    new RangeAttributeAdapter (metadata, context, ( RangeAttribute ) attribute)
//                },
//                {
//                    typeof (RegularExpressionAttribute),( metadata, context, attribute)=>
//                    new RegularExpressionAttributeAdapter (metadata, context, ( RegularExpressionAttribute ) attribute)
//                },
//                {
//                    typeof (RequiredAttribute),( metadata, context, attribute) =>
//                    new RequiredAttributeAdapter (metadata, context, ( RequiredAttribute ) attribute)
//                },
//                {
//                    typeof (StringLengthAttribute),( metadata, context, attribute) =>
//                    new StringLengthAttributeAdapter (metadata, context, ( StringLengthAttribute ) attribute)
//                }
//            };

//        #endregion

//        // 重写GetValidators方法，该从xml文件中获取。根据xml的配置，返回对应的Validator集合
//        public override IEnumerable<ModelValidator> GetValidators(ModelMetadata metadata, ControllerContext context)
//        {
//            var results = new List<ModelValidator>();

//            // whether the validation is for a property or model
//            // (remember we can apply validation attributes to a property or model and same applies here as well)
//            var isPropertyValidation = metadata.ContainerType != null && !String.IsNullOrEmpty(metadata.PropertyName);

//            var rulesPath = String.Format("{0}\\{1}.xml", _xmlFolderPath,
//                                          isPropertyValidation ? metadata.ContainerType.Name : metadata.ModelType.Name);

//            var rules = File.Exists(rulesPath)? XElement.Load(rulesPath).Elements(String.Format(
//                                "./validator[@property='{0}']",
//                                isPropertyValidation ? metadata.PropertyName : metadata.ModelType.Name)).ToList()
//                            : new List<XElement>();

//            // Produce a validator for each validation attribute we find
//            foreach (var rule in rules)
//            {
//                DataAnnotationsModelValidationFactory factory;

//                var validatorType = _validatorTypes[String.Concat(rule.Attribute("type").Value, "Attribute")];

//                if (!AttributeFactories.TryGetValue(validatorType, out factory))
//                {
//                    factory = DefaultAttributeFactory;
//                }

//                var validator = (ValidationAttribute)Activator.CreateInstance(validatorType, GetValidationArgs(rule));
//                validator.ErrorMessage = rule.Attribute("message") != null &&
//                                         !String.IsNullOrEmpty(rule.Attribute("message").Value)
//                                             ? GetValidationMessage(isPropertyValidation ? metadata.ContainerType.Name : metadata.ModelType.Name, rule.Attribute("message").Value)
//                                             : null;
//                results.Add(factory(metadata, context, validator));
//            }

//            return results;
//        }

//        private string GetValidationMessage(string model, string key)
//        {
//            return "";// MessageProvider.GetViewModelValidationMessage(model, key);
//        }

//        // read the arguments passed to the validation attribute and cast it their respective type.
//        private object[] GetValidationArgs(XElement rule)
//        {
//            var validatorArgs = rule.Attributes().Where(a => a.Name.ToString().StartsWith("arg"));
//            var args = new object[validatorArgs.Count()];
//            var i = 0;

//            foreach (var arg in validatorArgs)
//            {
//                var argName = arg.Name.ToString();
//                var argValue = arg.Value;

//                if (!argName.Contains("-"))
//                {
//                    args[i] = argValue;
//                }
//                else
//                {
//                    var argType = argName.Split('-')[1];

//                    switch (argType)
//                    {
//                        case "int":
//                            args[i] = int.Parse(argValue);
//                            break;

//                        case "datetime":
//                            args[i] = DateTime.Parse(argValue);
//                            break;

//                        case "char":
//                            args[i] = Char.Parse(argValue);
//                            break;

//                        case "double":
//                            args[i] = Double.Parse(argValue);
//                            break;

//                        case "decimal":
//                            args[i] = Decimal.Parse(argValue);
//                            break;

//                            args[i] = Boolean.Parse(argValue);
//                            break;

//                        default:
//                            args[i] = argValue;
//                            break;
//                    }
//                }
//            }

//            return args;
//        }
//    }
//}