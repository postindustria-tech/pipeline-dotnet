using FiftyOne.Pipeline.Engines.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace FiftyOne.Pipeline.Engines
{
    /// <summary>
    /// Static utility methods
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Check if the specified type is a specific type 'T' or an
        /// implementation of <see cref="IAspectPropertyValue{T}"/>
        /// that wraps 'T'.
        /// </summary>
        /// <typeparam name="T">
        /// The type to check for
        /// </typeparam>
        /// <param name="type">
        /// The type to check
        /// </param>
        /// <returns>
        /// True if 'type' is of type 'T' or a wrapper around 'T' 
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if required parameters are null
        /// </exception>
        public static bool IsTypeOrAspectPropertyValue<T>(Type type)
        {
            if(type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            return type.Equals(typeof(T)) ||
                typeof(IAspectPropertyValue<T>).IsAssignableFrom(type);
        }


    }
}
