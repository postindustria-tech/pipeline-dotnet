using System;

namespace FiftyOne.Pipeline.Core.Attributes
{
    /// <summary>
    /// This attribute can be applied to methods to specify that they can only be used when
    /// configuring a fluent builder in code.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter)]
    public class CodeConfigOnlyAttribute : Attribute
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public CodeConfigOnlyAttribute()
        {
        }
    }
}
