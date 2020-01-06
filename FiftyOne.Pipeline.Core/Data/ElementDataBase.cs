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

using FiftyOne.Pipeline.Core.FlowElements;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace FiftyOne.Pipeline.Core.Data
{
    /// <summary>
    /// This class stores property values that have be determined by a 
    /// specific FlowElement based on the supplied evidence.
    /// </summary>
    public abstract class ElementDataBase : DataBase, IElementData
    {
        public IPipeline Pipeline { get; set; }

        /// <summary>
        /// Constructor
        /// Creates an <see cref="ElementDataBase"/> instance with a 
        /// non-thread-safe, case-insensitive dictionary.
        /// </summary>
        /// <param name="logger">
        /// Used for logging
        /// </param>
        /// <param name="flowData">
        /// The <see cref="IFlowData"/> instance this element data will
        /// be associated with.
        /// </param>
        public ElementDataBase(ILogger<ElementDataBase> logger, IFlowData flowData) :
            this(logger, flowData, new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase))
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger">
        /// Used for logging
        /// </param>
        /// <param name="flowData">
        /// The <see cref="IFlowData"/> instance this element data will
        /// be associated with.
        /// </param>
        /// <param name="dictionary">
        /// The dictionary instance to use internally when storing data values.
        /// </param>
        public ElementDataBase(ILogger<ElementDataBase> logger,
            IFlowData flowData,
            IDictionary<string, object> dictionary)
            : base(logger, dictionary)
        {
            Pipeline = flowData.Pipeline;
        }
    }
}
