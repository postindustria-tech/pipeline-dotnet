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

using FiftyOne.Pipeline.Engines.Data;
using System;
using System.Collections.Generic;

namespace FiftyOne.Pipeline.Engines.FiftyOne.Data
{
    /// <summary>
    /// 51Degrees specific meta data. This adds meta data properties which are
    /// available in 51Degrees Engines.
    /// </summary>
    public interface IFiftyOneAspectPropertyMetaData : IAspectPropertyMetaData, 
        IEquatable<IFiftyOneAspectPropertyMetaData>, 
        IComparable<IFiftyOneAspectPropertyMetaData>, IEquatable<string>, IDisposable
    {
        /// <summary>
        /// URL relating to the property.
        /// </summary>
        string Url { get; }

        /// <summary>
        /// Order to display the property.
        /// </summary>
        byte DisplayOrder { get; }

        /// <summary>
        /// True if the property is mandatory.
        /// </summary>
        bool Mandatory { get; }

        /// <summary>
        /// True if the property value type is a list.
        /// </summary>
        bool List { get; }

        /// <summary>
        /// True if the property is obsolete.
        /// </summary>
        bool Obsolete { get; }

        /// <summary>
        /// True if the property should be shown.
        /// </summary>
        bool Show { get; }

        /// <summary>
        /// True if the values of the property should be shown.
        /// </summary>
        bool ShowValues { get; }

        /// <summary>
        /// The component which the property belongs to.
        /// </summary>
        IComponentMetaData Component { get; }

        /// <summary>
        /// The list of values which relate to the property i.e. the values
        /// which can be returned for this property.
        /// </summary>
        IReadOnlyList<IValueMetaData> Values { get; }

        /// <summary>
        /// The default value for this property.
        /// </summary>
        IValueMetaData DefaultValue { get; }

        /// <summary>
        /// Get the component which the property belongs to.
        /// </summary>
        /// <returns>The component</returns>
        IComponentMetaData GetComponent();

        /// <summary>
        /// Get the values which relate to the property i.e. the values which
        /// can be returned for this property.
        /// </summary>
        /// <returns>Values enumerable</returns>
        IEnumerable<IValueMetaData> GetValues();

        /// <summary>
        /// Get the value from the property which has the name provided.
        /// Null is returned if the property does not contain a value with the
        /// name provided
        /// </summary>
        /// <param name="valueName">
        /// Name of the value to return
        /// </param>
        /// <returns>The value or null if not in this property</returns>
        IValueMetaData GetValue(string valueName);

        /// <summary>
        /// Get the default value for this property.
        /// </summary>
        /// <returns>Default value</returns>
        IValueMetaData GetDefaultValue();
    }
}
