/* *********************************************************************
 * This Original Work is copyright of 51 Degrees Mobile Experts Limited.
 * Copyright 2020 51 Degrees Mobile Experts Limited, 5 Charlotte Close,
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

using System;
using System.Collections.Generic;
using System.Text;

namespace FiftyOne.Pipeline.Core.Data.Types
{
    /// <summary>
    /// JavaScript type which can be returned as a value by an ElementData.
    /// A value being of type JavaScript indicates that it is intended to be
    /// run on a client browser.
    /// </summary>
    public class JavaScript : IComparable<string>, IEquatable<string>
    {
        #region Private Properties

        /// <summary>
        /// String value of the JavaScript.
        /// </summary>
        private string _value;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="value">String value containing the JavaScript</param>
        public JavaScript(string value)
        {
            _value = value;
        }

        #endregion

        #region Public Interface Methods

        public int CompareTo(string other)
        {
            return _value.CompareTo(other);
        }

        public bool Equals(string other)
        {
            return _value.Equals(other);
        }

        #endregion

        #region Public Overrides

        public override bool Equals(object obj)
        {
            if (obj.GetType().Equals(typeof(JavaScript)))
            {
                return Equals(((JavaScript)obj)._value);
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public override string ToString()
        {
            return _value;
        }

        #endregion
    }
}
