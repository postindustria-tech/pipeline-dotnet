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

using FiftyOne.Pipeline.Core.Configuration;
using FiftyOne.Pipeline.Core.FlowElements;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace FiftyOne.Pipeline.Engines.FiftyOne.FlowElements
{
    /// <summary>
    /// Pipeline builder class that allows the 51Degrees share usage 
    /// element to be enabled/disabled.
    /// </summary>
    public class FiftyOnePipelineBuilder : PipelineBuilder
    {
        private bool _shareUsageEnabled = true;

        /// <summary>
        /// Default constructor
        /// </summary>
        public FiftyOnePipelineBuilder()
            : base(new LoggerFactory())
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="loggerFactory"></param>
        public FiftyOnePipelineBuilder(
            ILoggerFactory loggerFactory) 
            : base(loggerFactory)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="loggerFactory"></param>
        /// <param name="services"></param>
        public FiftyOnePipelineBuilder(
            ILoggerFactory loggerFactory, 
            IServiceProvider services) 
            : base(loggerFactory, services)
        {
        }

        /// <summary>
        /// Set share usage enabled/disabled.
        /// Defaults to enabled.
        /// </summary>
        /// <param name="enabled">
        /// true to enable usage sharing. False to disable.
        /// </param>
        /// <returns>
        /// This builder instance.
        /// </returns>
        public FiftyOnePipelineBuilder SetShareUsage(bool enabled)
        {
            _shareUsageEnabled = enabled;
            return this;
        }

        /// <summary>
        /// Called just before the pipeline is built.
        /// </summary>
        protected override void OnPreBuild()
        {
            // Add the share usage element if it does not exist in the list.
            if (_shareUsageEnabled &&
                FlowElements.Any(e => e.GetType() == typeof(ShareUsageElement)) == false)
            {
                FlowElements.Add(new ShareUsageBuilder(LoggerFactory, new HttpClient()).Build());
            }
        }

    }
}
