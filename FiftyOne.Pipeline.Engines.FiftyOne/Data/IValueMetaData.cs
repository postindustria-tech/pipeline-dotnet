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

using System;
using System.Collections;
using System.Collections.Generic;

namespace FiftyOne.Pipeline.Engines.FiftyOne.Data
{
    /// <summary>
    /// Meta data relating to a specific value within a data set.
    /// </summary>
    public interface IValueMetaData : IEquatable<IValueMetaData>, IComparable<IValueMetaData>, IDisposable
    {
        /// <summary>
        /// The property which the value relates to i.e. the value is a value
        /// which can be returned by the property.
        /// </summary>
        IFiftyOneAspectPropertyMetaData Property { get; }

        /// <summary>
        /// The name of the value e.g. "True" or "Samsung".
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Full description of the meaning of the value.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// URL relating to the value if more information is available.
        /// </summary>
        string Url { get; }

        /// <summary>
        /// Get the property which the value relates to i.e. the value is a
        /// value which can be returned by the property.
        /// </summary>
        /// <returns>The property</returns>
        IFiftyOneAspectPropertyMetaData GetProperty();
    }
}
