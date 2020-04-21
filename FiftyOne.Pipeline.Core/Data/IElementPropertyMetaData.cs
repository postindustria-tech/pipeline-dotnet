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

using FiftyOne.Pipeline.Core.FlowElements;
using System;
using System.Collections.Generic;
using System.Text;

namespace FiftyOne.Pipeline.Core.Data
{
    /// <summary>
    /// Represents meta-data relating to a property that is populated 
    /// by a <see cref="IFlowElement"/> instance.
    /// </summary>
    public interface IElementPropertyMetaData
    { 
        /// <summary>
        /// The name of the property. Must match the string key used to
        /// store the property value in the <see cref="IElementData"/> instance.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The <see cref="IFlowElement"/> that this property is associated 
        /// with.
        /// </summary>
        IFlowElement Element { get; }

        /// <summary>
        /// The category the property belongs to.
        /// </summary>
        string Category { get; }

        /// <summary>
        /// The type of the property values
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// True if the property is available in the results for the
        /// associated <see cref="IFlowElement"/>, false otherwise.
        /// </summary>
        bool Available { get; }

        /// <summary>
        /// This is only relevant where Type is a collection of complex 
        /// objects. 
        /// It contains a list of the property meta-data for the
        /// items in the value for this property.
        /// For example, if this meta-data instance represents a list of 
        /// hardware devices, ItemProperties will contain a list of the 
        /// meta-data for properties available on each hardware device
        /// element within that list.
        /// </summary>
        IReadOnlyList<IElementPropertyMetaData> ItemProperties { get; }
    }
}
