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
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace FiftyOne.Pipeline.Core.Data
{
    /// <summary>
    /// Used by the CloudRequestEngine to store details of accessible 
    /// products and properties based on the currently configured 
    /// resource key.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", 
        "CA2227:Collection properties should be read only", 
        Justification = "This is populated by JSON deserialization")]
    public class LicencedProducts
    {
        /// <summary>
        /// A collection of accessible products.
        /// Key is product name, value contains a list of the 
        /// accessible properties.
        /// </summary>
        public Dictionary<string, ProductMetaData> Products { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<string> Errors { get; set; }
    }

    /// <summary>
    /// Licensed properties class used to deserialize accessible property
    /// information from cloud services.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage",
        "CA2227:Collection properties should be read only",
        Justification = "This is populated by JSON deserialization")]
    public class ProductMetaData
    {
        /// <summary>
        /// Accessible data tiers
        /// </summary>
        public string DataTier { get; set; }


        /// <summary>
        /// Accessible Properties
        /// </summary>
        public IList<PropertyMetaData> Properties { get; set; }
    }


    /// <summary>
    /// Standalone instance of ElementPropertyMetaData, used to serialize 
    /// element or aspect properties.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage",
        "CA2227:Collection properties should be read only",
        Justification = "This is populated by JSON deserialization")]
    public class PropertyMetaData
    {
        /// <summary>
        /// Name of the property
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The property type name.
        /// Note that this is the JSON type, not the c# type.
        /// For example, any list types will just have the type name 'Array'.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The property category
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Properties of sub-items
        /// </summary>
        public IList<PropertyMetaData> ItemProperties { get; set; }
    }
}
