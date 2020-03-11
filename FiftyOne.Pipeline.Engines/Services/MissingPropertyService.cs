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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using FiftyOne.Pipeline.Engines.Data;
using FiftyOne.Pipeline.Engines.FlowElements;

namespace FiftyOne.Pipeline.Engines.Services
{
    public class MissingPropertyService : IMissingPropertyService
    {
        private static IMissingPropertyService _instance;
        private static object _lock = new object();

        public static IMissingPropertyService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if(_instance == null)
                        {
                            _instance = new MissingPropertyService();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Constructor is private to ensure the single instance accessible
        /// through the 'Instance' property is used.
        /// </summary>
        private MissingPropertyService() { }

        /// <summary>
        /// Get the reason that a property is not available from an engine.
        /// </summary>
        /// <param name="propertyName">
        /// The name of the property to look for
        /// </param>
        /// <param name="engines">
        /// The engines that are expected to supply the property value.
        /// </param>
        /// <returns>
        /// A <see cref="MissingPropertyResult"/> instance that includes an
        /// enum giving the reason and a developer-facing description of 
        /// the reason.
        /// </returns>
        public MissingPropertyResult GetMissingPropertyReason(string propertyName, IReadOnlyList<IAspectEngine> engines)
        {
            MissingPropertyResult result = null;
            foreach (var engine in engines.Where(e => e != null))
            {
                result = GetMissingPropertyReason(propertyName, engine);
                if (result.Reason != MissingPropertyReason.Unknown)
                {
                    return result;
                }
            }
            return result;
        }

        /// <summary>
        /// Get the reason that a property is not available from an engine.
        /// </summary>
        /// <param name="propertyName">
        /// The name of the property to look for
        /// </param>
        /// <param name="engine">
        /// The engine that is expected to supply the property value.
        /// </param>
        /// <returns>
        /// A <see cref="MissingPropertyResult"/> instance that includes an
        /// enum giving the reason and a developer-facing description of 
        /// the reason.
        /// </returns>
        public MissingPropertyResult GetMissingPropertyReason(string propertyName, IAspectEngine engine)
        {
            MissingPropertyResult result = new MissingPropertyResult();
            MissingPropertyReason reason = MissingPropertyReason.Unknown;

            IAspectPropertyMetaData property = null;
            property = engine.Properties.FirstOrDefault(p => p.Name == propertyName);

            if (property != null)
            {
                // Check if the property is available in the data file that is 
                // being used by the engine.
                if (property.DataTiersWherePresent.Any(t => t == engine.DataSourceTier) == false)
                {
                    reason = MissingPropertyReason.DataFileUpgradeRequired;
                }
                // Check if the property is excluded from the results.
                else if (property.Available == false)
                {
                    reason = MissingPropertyReason.PropertyExculdedFromEngineConfiguration;
                }
            }

            if(reason == MissingPropertyReason.Unknown &&
                typeof(ICloudAspectEngine).IsAssignableFrom(engine.GetType()))
            {
                reason = MissingPropertyReason.CloudEngine;
            }

            // Build the message string to return to the caller.
            StringBuilder message = new StringBuilder();
            message.AppendLine($"Property '{propertyName}' is not present in the results.");
            switch (reason)
            {
                case MissingPropertyReason.DataFileUpgradeRequired:
                    message.Append("This is because your license and/or data file " +
                        "does not include this property. The property is available ");                    
                    message.Append("with the ");
                    message.Append(string.Join(",", property.DataTiersWherePresent));
                    message.Append($" license/data for the {engine.GetType().Name}");
                    break;
                case MissingPropertyReason.PropertyExculdedFromEngineConfiguration:
                    message.Append("This is because the property has been " +
                        "excluded when configuring the engine.");
                    break;
                case MissingPropertyReason.CloudEngine:
                    message.Append("This may be because your resource key " +
                        "does not include this property. " +
                        "Check the property name is correct. Compare this " +
                        "to the properties available using the supplied " +
                        "resource key: ");
                    message.Append(string.Join(",", engine.Properties.Select(p => p.Name)));
                    break;
                case MissingPropertyReason.Unknown:
                    message.Append("The reason for this is unknown. Please " +
                        "check that the aspect and property name are correct.");
                    break;
                default:
                    break;
            }

            result.Description = message.ToString();
            result.Reason = reason;
            return result;
        }
    }
}
