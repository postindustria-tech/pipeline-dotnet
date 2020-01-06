/* *********************************************************************
 * This Original Work is copyright of 51 Degrees Mobile Experts Limited.
 * Copyright 2019 51 Degrees Mobile Experts Limited, 5 Charlotte Close,
 * Caversham, Reading, Berkshire, United Kingdom RG4 7BY.
 *
 * This Original Work is licensed under the European Union Public Licence (EUPL) 
 * v.1.2 and is subject to its terms as set out below.
 *
 * If a copy of the EUPL was not distributed with this file, You can obtain
 * one at https://opensource.org/licenses/EUPL-1.2.
 *
 * The 'Compatible Licences' set out in the Appendix to the EUPL (as may be
 * amended by the European Commission) shall be deemed incompatible for
 * the purposes of the Work and the provisions of the compatibility
 * clause in Article 5 of the EUPL shall not apply.
 * 
 * If using the Work as, or as part of, a network application, by 
 * including the attribution notice(s) required under Article 5 of the EUPL
 * in the end user terms of the application under an appropriate heading, 
 * such notice(s) shall fulfill the requirements of that article.
 * ********************************************************************* */

using FiftyOne.Pipeline.Engines.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace FiftyOne.Pipeline.Engines.Data
{
    /// <summary>
    /// This class can be used where engines have a property that 
    /// may be populated and may not.
    /// It can be used to let the user know details of why a property
    /// value is not available.
    /// For example, if a property requires additional evidence from
    /// client side code before it can be populated.
    /// </summary>
    /// <typeparam name="T">
    /// The type of data stored within the instance.
    /// </typeparam>
    public class AspectPropertyValue<T> : IAspectPropertyValue<T>
    {
        /// <summary>
        /// The value stored within this instance
        /// </summary>
        private T _value;

        /// <summary>
        /// The message in the Exception if the Value property is accessed
        /// and the instance does not have a set value.
        /// </summary>
        public string NoValueMessage { get; set; } =
            "This instance does not have a set value";

        /// <summary>
        /// True if this instance contains a value, false otherwise.
        /// </summary>
        public bool HasValue { get; private set; }

        /// <summary>
        /// Get/set the underlying value.
        /// </summary>
        /// <remarks>
        /// The underlying value can be set to null. In this case,
        /// the <see cref="HasValue"/> property will return true and
        /// <see cref="Value"/> will return null.
        /// </remarks>
        /// <exception cref="NoValueException">
        /// This exception will be thrown if the instance does not 
        /// contain a value.
        /// </exception>
        public T Value
        {
            get
            {
                if(HasValue == false)
                {
                    throw new NoValueException(NoValueMessage);
                }
                return _value;
            }
            set
            {
                HasValue = true;
                _value = value;
            }
        }

        /// <summary>
        /// Get/set the underlying value as an object.
        /// </summary>
        /// <exception cref="NoValueException">
        /// This exception will be thrown if the instance does not 
        /// contain a value.
        /// </exception>
        object IAspectPropertyValue.Value {
            get => Value;
            set
            {
                Value = (T)value;
            }
        }

        /// <summary>
        /// Default constructor. Used to create an instance 
        /// with no initial underlying value.
        /// </summary>
        public AspectPropertyValue()
        {
        }

        /// <summary>
        /// Constructor. Used to create an instance with an 
        /// underlying value.
        /// </summary>
        /// <param name="value">
        /// The underlying value to assign to this instance.
        /// </param>
        public AspectPropertyValue(T value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value?.ToString() ?? "NULL";
        }

        public override bool Equals(object obj)
        {
            bool result = false;
            var other = obj as AspectPropertyValue<T>;
            if (other != null)
            {
                result = this.Value?.Equals(other.Value) 
                    ?? other.Value == null;
            }
            return result;
        }

        public override int GetHashCode()
        {
            return Value?.GetHashCode() ?? 0;
        }
    }
}
