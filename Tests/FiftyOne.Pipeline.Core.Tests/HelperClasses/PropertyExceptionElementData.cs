using FiftyOne.Pipeline.Core.Data;
using FiftyOne.Pipeline.Core.FlowElements;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace FiftyOne.Pipeline.Core.Tests.HelperClasses
{
    /// <summary>
    /// A test helper class that can be used to configure element data 
    /// to throw an exception when accessing certain properties.
    /// </summary>
    public class PropertyExceptionElementData : ElementDataBase
    {
        private Dictionary<string, Exception> _propertiesToThrowExceptionFor;

        public PropertyExceptionElementData(ILogger<ElementDataBase> logger, IPipeline pipeline)
            : base(logger, pipeline)
        {
        }

        public void ConfigureExceptionForProperty(
            string propertyName,
            Exception exceptionToThrow)
        {
            _propertiesToThrowExceptionFor.Add(propertyName, exceptionToThrow);
        }

        public override object this[string key]
        {
            get
            {
                if (_propertiesToThrowExceptionFor.ContainsKey(key))
                {
                    throw _propertiesToThrowExceptionFor[key];
                }
                return base[key];
            }
            set => base[key] = value;
        }
    }
}
