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

using FiftyOne.Pipeline.Core.Data;
using FiftyOne.Pipeline.Core.FlowElements;
using FiftyOne.Pipeline.Web.Shared.Data;
using Microsoft.Extensions.Logging;

namespace FiftyOne.Pipeline.Web.Shared.FlowElements
{
    /// <summary>
    /// Builder used to create new <see cref="JavaScriptBundlerElement"/> 
    /// instances.
    /// </returns>
    public class JavaScriptBundlerElementBuilder
    {
        /// <summary>
        /// The logger factory to use.
        /// </summary>
        private ILoggerFactory _loggerFactory;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="loggerFactory">
        /// The logger factory to use.
        /// </param>
        public JavaScriptBundlerElementBuilder(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        /// <summary>
        /// Create a new <see cref="JavaScriptBundlerElement"/>.
        /// </summary>
        /// <returns>
        /// A new <see cref="JavaScriptBundlerElement"/>.
        /// </returns>
        public JavaScriptBundlerElement Build()
        {
            return new JavaScriptBundlerElement(_loggerFactory.CreateLogger<JavaScriptBundlerElement>(), CreateData);
        }

        /// <summary>
        /// Method to create element data instances.
        /// </summary>
        /// <param name="pipeline">
        /// The pipeline that is creating the data instance.
        /// </param>
        /// <returns>
        /// The new <see cref="JavaScriptBundlerData"/> instance.
        /// </returns>
        private JavaScriptData CreateData(IPipeline pipeline, 
            FlowElementBase<JavaScriptData, IElementPropertyMetaData> element)
        {
            return new JavaScriptData(_loggerFactory.CreateLogger<JavaScriptData>(), pipeline);
        }
    }
}
