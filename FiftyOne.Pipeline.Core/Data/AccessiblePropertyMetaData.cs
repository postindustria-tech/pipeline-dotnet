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

namespace FiftyOne.Pipeline.Core.Data
{
    public class LicencedProducts
    {
        public Dictionary<string, ProductMetaData> Products;
    }
    
    /// <summary>
    /// Licenced properties class used to deserialise accessible property
    /// information from cloud services.
    /// </summary>
    public class ProductMetaData
    {
        /// <summary>
        /// Accessible data tiers
        /// </summary>
        public string DataTier;
        /// <summary>
        /// Accessible Properties
        /// </summary>
        public  IList<PropertyMetaData> Properties;
    }


    /// <summary>
    /// Standalone instance of ElementPropertyMetaData, used to serialise 
    /// element or aspect properties.
    /// </summary>
    public class PropertyMetaData
    {
        /// <summary>
        /// Name of the property
        /// </summary>
        public string Name;

        /// <summary>
        /// The property type name.
        /// </summary>
        public string Type;

        /// <summary>
        /// The property category
        /// </summary>
        public string Category;

        /// <summary>
        /// Gets the actual Type of the property from the Type Name.
        /// </summary>
        /// <returns></returns>
        public Type GetPropertyType()
        {
            switch (Type)
            {
                case "String":
                    return typeof(string);
                case "Int32":
                    return typeof(int);
                case "IList`1":
                    return typeof(IList<string>);
                case "IReadOnlyList`1":
                    return typeof(IReadOnlyList<string>);
                case "Boolean":
                    return typeof(bool);
                case "JavaScript":
                    return typeof(Pipeline.Core.Data.Types.JavaScript);
                case "Double":
                    return typeof(double);
                default:
                    throw new TypeAccessException("Type does not map");
            }
        }
    }
}
