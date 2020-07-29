using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TestAreaDemo
{
    /// <summary>
    /// 
    /// </summary>
    public class MyModelValidatorProvider : ModelValidatorProvider
    {
        public override IEnumerable<ModelValidator> GetValidators(ModelMetadata metadata, ControllerContext context)
        {
            var ArrValidtors = metadata.GetValidators(context);
            return ArrValidtors;
        }
    }
}