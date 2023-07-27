using System;

namespace FiftyOne.Pipeline.Core.Attributes
{
    /// <summary>
    /// This attribute can be applied to methods and parameters to specify what the default value 
    /// is for that setting.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter)]
    public class DefaultValueAttribute : Attribute
    {
        /// <summary>
        /// The default value for the property that is set by this method.
        /// </summary>
        public object DefaultValue { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="defaultValue">
        /// The default value for the property that is set by this method.
        /// </param>
        public DefaultValueAttribute(object defaultValue)
        {
            DefaultValue = defaultValue;
        }
    }
}
