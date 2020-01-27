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

using FiftyOne.Pipeline.CloudRequestEngine.Data;
using FiftyOne.Pipeline.Core.Data;
using FiftyOne.Pipeline.Core.Exceptions;
using FiftyOne.Pipeline.Core.FlowElements;
using FiftyOne.Pipeline.Engines.Data;
using FiftyOne.Pipeline.Engines.FlowElements;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net.Http;

namespace FiftyOne.Pipeline.CloudRequestEngine.FlowElements
{
    public class CloudRequestEngineBuilder : 
        AspectEngineBuilderBase<CloudRequestEngineBuilder, CloudRequestEngine>
    {
        #region Private Properties

        private ILoggerFactory _loggerFactory;
        private HttpClient _httpClient;

        private string _dataEndpoint = "https://cloud.51degrees.com/api/v4/json";
        private string _propertiesEndpoint = "https://cloud.51degrees.com/api/v4/accessibleproperties";
        private string _evidenceKeysEndpoint = "https://cloud.51degrees.com/api/v4/evidencekeys";
        private string _resourceKey = null;
        private string _licenseKey = null;
        private int _timeout = 100;

        #endregion

        #region Constructor

        /// <summary>
        /// Construct a new instance of the builder.
        /// </summary>
        /// <param name="loggerFactory">
        /// Factory used to create loggers for the engine
        /// </param>
        /// <param name="httpClient">
        /// HttpClient instance used to make http requests
        /// </param>
        public CloudRequestEngineBuilder(
            ILoggerFactory loggerFactory,
            HttpClient httpClient)
        {
            _loggerFactory = loggerFactory;
            _httpClient = httpClient;
        }

        #endregion
        
        /// <summary>
        /// The root endpoint which the CloudRequestsEngine will query.
        /// This will set the data, properties and evidence keys endpoints.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public CloudRequestEngineBuilder SetEndPoint(string uri)
        {
            if (uri.EndsWith("/") == false)
            {
                uri += '/';
            }
            return SetDataEndpoint(uri + (_resourceKey != null ? _resourceKey + "." : "") + "json")
                .SetPropertiesEndpoint(uri + "accessibleproperties" + (_resourceKey != null ? "?Resource=" + _resourceKey : ""))
                .SetEvidenceKeysEndpoint(uri + "evidencekeys");

        }

        /// <summary>
        /// The endpoint the CloudRequestEngine will query to get a processing
        /// result.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public CloudRequestEngineBuilder SetDataEndpoint(string uri)
        {
            _dataEndpoint = uri;
            return this;
        }

        /// <summary>
        /// The endpoint the cloudRequestEngine will query to get the available
        /// properties.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public CloudRequestEngineBuilder SetPropertiesEndpoint(string uri)
        {
            _propertiesEndpoint = uri;
            return this;
        }

        /// <summary>
        /// The endpoint the cloudRequestEngine will query to get the required
        /// evidence keys.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public CloudRequestEngineBuilder SetEvidenceKeysEndpoint(string uri)
        {
            _evidenceKeysEndpoint = uri;
            return this;
        }

        /// <summary>
        /// The resource key to query the endpoint with.
        /// </summary>
        /// <param name="licenseKey"></param>
        /// <returns></returns>
        public CloudRequestEngineBuilder SetResourceKey(string resourceKey)
        {
            _resourceKey = resourceKey;
            return this;
        }

        /// <summary>
        /// The license key to query the endpoint with.
        /// </summary>
        /// <param name="licenseKey"></param>
        /// <returns></returns>
        public CloudRequestEngineBuilder SetLicenseKey(string licenseKey)
        {
            _licenseKey = licenseKey;
            return this;
        }

        /// <summary>
        /// Timeout in seconds for the request to the endpoint.
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public CloudRequestEngineBuilder SetTimeOutSeconds(int timeout)
        {
            _timeout = timeout;
            return this;
        }

        public CloudRequestEngine Build()
        {
            if (string.IsNullOrWhiteSpace(_resourceKey))
            {
                throw new PipelineConfigurationException("A resource key is " +
                    "required to access the cloud server. Please use the " +
                    "'SetResourceKey(string) method to supply your resource " +
                    "key obtained from https://configure.51degrees.com");
            }

            return BuildEngine();
        }

        private CloudRequestData CreateAspectData(IFlowData flowData, 
            FlowElementBase<CloudRequestData, IAspectPropertyMetaData> engine)
        {
            return new CloudRequestData(
                _loggerFactory.CreateLogger<CloudRequestData>(),
                flowData,
                (IAspectEngine)engine);
        }

        protected override CloudRequestEngine NewEngine(List<string> properties)
        {
            if (string.IsNullOrWhiteSpace(_resourceKey))
            {
                throw new PipelineConfigurationException("A resource key is " +
                        "required to access the cloud server. Please use the " +
                        "'setResourceKey(String) method to supply your resource " +
                        "key obtained from https://configure.51degrees.com");
            }

            return new CloudRequestEngine(
                _loggerFactory.CreateLogger<CloudRequestEngine>(),
                CreateAspectData,
                _httpClient,
                _dataEndpoint,
                _resourceKey,
                _licenseKey,
                _propertiesEndpoint,
                _evidenceKeysEndpoint,
                _timeout,
                properties);
        }
    }
}
