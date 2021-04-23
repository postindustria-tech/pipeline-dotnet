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
using FiftyOne.Pipeline.Engines.Data;
using FiftyOne.Pipeline.Engines.FiftyOne.FlowElements;
using FiftyOne.Pipeline.Engines.FlowElements;
using FiftyOne.Pipeline.Web.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace FiftyOne.Pipeline.Web.Services
{
    /// <inheritdoc/>
    public class SetHeaderService : ISetHeadersService
    {
        private ILogger<SetHeaderService> _logger;

        /// <summary>
        /// Data provider
        /// </summary>
        private IFlowDataProvider _flowDataProvider;

        /// <summary>
        /// The active property flow element that generates the dictionary
        /// of response header values.
        /// </summary>
        private ISetHeadersElement _setHeadersElement;

        private IOptions<PipelineWebIntegrationOptions> _options;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="pipeline"></param>
        /// <param name="flowDataProvider"></param>
        /// <param name="options"></param>
        public SetHeaderService(
            ILogger<SetHeaderService> logger,
            IPipeline pipeline,
            IFlowDataProvider flowDataProvider,
            IOptions<PipelineWebIntegrationOptions> options)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (pipeline == null) throw new ArgumentNullException(nameof(pipeline));
            if (flowDataProvider == null) throw new ArgumentNullException(nameof(flowDataProvider));

            _logger = logger;
            _flowDataProvider = flowDataProvider;
            _setHeadersElement = pipeline.GetElement<ISetHeadersElement>();
            _options = options;
        }

        /// <inheritdoc/>
        public void SetHeaders(HttpContext context)
        {
            if (_setHeadersElement != null &&
                _options.Value.UseSetHeaderProperties)
            {
                var headersToSet = _flowDataProvider.GetFlowData()
                    .GetFromElement(_setHeadersElement).ResponseHeaderDictionary;
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    foreach (var header in headersToSet)
                    {
                        _logger.LogDebug($"Adding response header " +
                            $"'{header.Key}' with value '{header.Value}'");
                    }
                }
                SetHeaders(context, headersToSet);
            }
        }

        /// <summary>
        /// Set the HTTP headers in the response using the supplied
        /// dictionary.
        /// If the supplied headers already have values in the response
        /// then they will be amended rather than replaced.
        /// </summary>
        /// <param name="context">
        /// The <see cref="HttpContext"/> to set the response headers in
        /// </param>
        /// <param name="headersToSet">
        /// A dictionary containing the names and values of the headers 
        /// to set.
        /// </param>
        public static void SetHeaders(HttpContext context, 
            IReadOnlyDictionary<string, string> headersToSet)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (headersToSet == null) throw new ArgumentNullException(nameof(headersToSet));

            foreach (var header in headersToSet)
            {
                if (context.Response.Headers.ContainsKey(header.Key))
                {
                    context.Response.Headers.Append(header.Key, header.Value);
                }
                else
                {
                    context.Response.Headers.Add(header.Key, header.Value);
                }
            }
        }
    }
}