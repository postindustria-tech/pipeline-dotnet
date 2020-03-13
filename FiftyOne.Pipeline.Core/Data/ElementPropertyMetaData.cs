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
    /// Meta-data relating to properties that are populated by Flow Elements.
    /// </summary>
    public class ElementPropertyMetaData : IElementPropertyMetaData
    {
        /// <summary>
        /// The name of the property. Must match the string key used to
        /// store the property value in the <see cref="IElementData"/> instance.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The <see cref="IFlowElement"/> that this property is associated 
        /// with.
        /// </summary>
        public IFlowElement Element { get; private set; }

        /// <summary>
        /// The category the property belongs to.
        /// </summary>
        public string Category { get; private set; }

        /// <summary>
        /// The type of the property values
        /// </summary>
        public Type Type { get; private set; }

        /// <summary>
        /// True if the property is available in the results for the
        /// associated <see cref="IFlowElement"/>, false otherwise.
        /// </summary>
        public bool Available { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="element">
        /// The <see cref="IFlowElement"/> that this property is associated 
        /// with.
        /// </param>
        /// <param name="name">
        /// The name of the property. Must match the string key used to
        /// store the property value in the <see cref="IElementData"/> instance.
        /// </param>
        /// <param name="type">
        /// The type of the property values.
        /// </param>
        /// <param name="category">
        /// The category the property belongs to.
        /// </param>
        /// <param name="available">
        /// True if the property is available.
        /// </param>
        public ElementPropertyMetaData(
            IFlowElement element,
            string name,
            Type type,
            bool available,
            string category = "")
        {
            Element = element;
            Name = name;
            Type = type;
            Category = category;
            Available = available;
        }
    }
}
