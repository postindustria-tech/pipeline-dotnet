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

using Microsoft.Extensions.Logging;

namespace FiftyOne.Pipeline.Engines.FlowElements
{
    /// <summary>
    /// Base class for cloud pipeline builders. Sets the options required to
    /// access the cloud service including endpoints and resource keys.
    /// </summary>
    /// <typeparam name="TBuilder"></typeparam>
    public abstract class CloudPipelineBuilderBase<TBuilder> : PrePackagedPipelineBuilderBase<TBuilder>
        where TBuilder : CloudPipelineBuilderBase<TBuilder>
    {
        /// <summary>
        /// The URL to the JSON resource on the cloud service.
        /// </summary>
        protected string Url { get; set; } = string.Empty;
        /// <summary>
        /// The URL for the AccessileProperties endpoint on the cloud service.
        /// </summary>
        protected string PropertiesEndpoint { get; set; } = string.Empty;
        /// <summary>
        /// The URL for the EvidenceKeys endpoint on the cloud service.
        /// </summary>
        protected string EvidenceKeysEndpoint { get; set; } = string.Empty;
        /// <summary>
        /// The resource key used to access the cloud service. This is required.
        /// </summary>
        protected string ResourceKey { get; set; } = string.Empty;
        /// <summary>
        /// Optional license keys for additional products.
        /// </summary>
        protected string LicenceKey { get; set; } = string.Empty;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="loggerFactory">
        /// The logger factory to use.
        /// </param>
        public CloudPipelineBuilderBase(
            ILoggerFactory loggerFactory) : base(loggerFactory)
        {

        }

        /// <summary>
        /// Set the base path for the cloud service. This will update the URL 
        /// for the JSON endpoint and the AccessileProperties endpoint.
        /// </summary>
        /// <param name="url">
        /// The base URL to use.
        /// </param>
        /// <returns>This builder</returns>
        public TBuilder SetEndPoint(string url)
        {
            if (url[url.Length - 1] != '/') {
                url += '/';
            }
            return SetJSONEndpoint(url + "json")
                .SetPropertiesEndpoint(url + "accessibleproperties")
                .SetEvidenceKeysEndpoint(url + "evidencekeys");
        }

        /// <summary>
        /// Set the URL to the JSON endpoint on the cloud service.
        /// </summary>
        /// <param name="url">
        /// The URL which points to the JSON endpoint.
        /// </param>
        /// <returns>This builder</returns>
        public TBuilder SetJSONEndpoint(string url)
        {
            Url = url;
            return this as TBuilder;
        }

        /// <summary>
        /// Set the URL to the AccessibleProperties endpoint on the cloud 
        /// service.
        /// </summary>
        /// <param name="propertiesEndpoint">
        /// The URL to use
        /// </param>
        /// <returns>This builder</returns>
        public TBuilder SetPropertiesEndpoint(string propertiesEndpoint)
        {
            PropertiesEndpoint = propertiesEndpoint;
            return this as TBuilder;
        }

        /// <summary>
        /// Set the URL to the EvidenceKeys endpoint on the cloud 
        /// service.
        /// </summary>
        /// <param name="evidenceKeysEndpoint">
        /// The URL to use
        /// </param>
        /// <returns>This builder</returns>
        public TBuilder SetEvidenceKeysEndpoint(string evidenceKeysEndpoint)
        {
            EvidenceKeysEndpoint = evidenceKeysEndpoint;
            return this as TBuilder;
        }

        /// <summary>
        /// Set the resource key which will be used to query the cloud service.
        /// Obtain a resource key @ https://configure.51degrees.com
        /// </summary>
        /// <param name="key">The resource key to use</param>
        /// <returns>This builder</returns>
        public TBuilder SetResourceKey(string key)
        {
            ResourceKey = key;
            return this as TBuilder;
        }

        /// <summary>
        /// Set the license key or keys to be used when querying the cloud 
        /// service. This is an optional method to pass additional license keys
        /// not configured when obtaining a resource key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns>This builder</returns>
        public TBuilder SetLicenseKey(string key)
        {
            LicenceKey = key;
            return this as TBuilder;
        }
    }
}
