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

using FiftyOne.Pipeline.CloudRequestEngine.Data;
using FiftyOne.Pipeline.Core.Data;
using FiftyOne.Pipeline.Core.FlowElements;
using FiftyOne.Pipeline.Engines.Data;
using FiftyOne.Pipeline.Engines.FlowElements;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace FiftyOne.Pipeline.CloudRequestEngine.FlowElements
{
    /// <summary>
    /// Engine that makes requests to the 51Degrees cloud service based 
    /// on the details passed at creation and the evidence in the
    /// FlowData instance.
    /// The unprocessed JSON response is stored in the FlowData
    /// for other engines to make use of.
    /// </summary>
    public class CloudRequestEngine : AspectEngineBase<CloudRequestData, IAspectPropertyMetaData>
    {
        private HttpClient _httpClient;

        private string _dataEndpoint;
        private string _resourceKey;
        private string _licenseKey;
        private string _propertiesEndpoint;
        private string _evidenceKeysEndpoint;
        private List<string> _requestedProperties;
        private IEvidenceKeyFilter _evidenceKeyFilter;

        /// <summary>
        /// Constructor used if no licence key is set.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="aspectDataFactory"></param>
        /// <param name="httpClient"></param>
        /// <param name="dataEndpoint"></param>
        /// <param name="propertiesEndpoint"></param>
        /// <param name="evidenceKeysEndpoint"></param>
        /// <param name="timeout"></param>
        public CloudRequestEngine(
            ILogger<AspectEngineBase<CloudRequestData, IAspectPropertyMetaData>> logger,
            Func<IPipeline, FlowElementBase<CloudRequestData, IAspectPropertyMetaData>, 
                CloudRequestData> aspectDataFactory,
            HttpClient httpClient,
            string dataEndpoint,
            string propertiesEndpoint,
            string evidenceKeysEndpoint,
            int timeout,
            List<string> requestedProperties) : 
            this(logger, aspectDataFactory, httpClient, dataEndpoint, null, null, propertiesEndpoint, evidenceKeysEndpoint, timeout, requestedProperties)
        {
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="aspectDataFactory"></param>
        /// <param name="httpClient"></param>
        /// <param name="dataEndpoint"></param>
        /// <param name="licenseKey"></param>
        /// <param name="propertiesEndpoint"></param>
        /// <param name="evidenceKeysEndpoint"></param>
        /// <param name="timeout"></param>
        public CloudRequestEngine(
            ILogger<AspectEngineBase<CloudRequestData, IAspectPropertyMetaData>> logger, 
            Func<IPipeline, FlowElementBase<CloudRequestData, IAspectPropertyMetaData>, 
                CloudRequestData> aspectDataFactory,
            HttpClient httpClient,
            string dataEndpoint,
            string resourceKey,
            string licenseKey,
            string propertiesEndpoint,
            string evidenceKeysEndpoint,
            int timeout,
            List<string> requestedProperties) 
            : base(logger, aspectDataFactory)
        {
            try
            {
                _dataEndpoint = dataEndpoint;
                _resourceKey = resourceKey;
                _licenseKey = licenseKey;
                _propertiesEndpoint = propertiesEndpoint;
                _evidenceKeysEndpoint = evidenceKeysEndpoint;
                _requestedProperties = requestedProperties;

                _httpClient = httpClient;
                if (timeout > 0)
                {
                    _httpClient.Timeout = new TimeSpan(0, 0, timeout);
                }
                else
                {
                    _httpClient.Timeout = new TimeSpan(0, 0, 0, 0, -1);
                }

                GetCloudProperties();
                GetCloudEvidenceKeys();

                _propertyMetaData = new List<IAspectPropertyMetaData>()
                {
                     new AspectPropertyMetaData(this, "json-response", typeof(string), "", new List<string>(), true)
                };
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, $"Error creating {this.GetType().Name}");
                throw;
            }
        }

        private List<IAspectPropertyMetaData> _propertyMetaData;
        private Dictionary<string, ProductMetaData> _publicProperties;

        /// <summary>
        /// A collection of the properties available in the aspect
        /// data instance that is populated by this engine.
        /// </summary>
        public override IList<IAspectPropertyMetaData> Properties => _propertyMetaData;

        public override string DataSourceTier => "cloud";

        public override string ElementDataKey => "cloud-response";

        public override IEvidenceKeyFilter EvidenceKeyFilter => _evidenceKeyFilter;

        /// <summary>
        /// A collection of the properties that the cloud service can
        /// populate in the JSON response.
        /// Keyed on property name.
        /// </summary>
        public Dictionary<string, ProductMetaData> PublicProperties => _publicProperties;

        /// <summary>
        /// Send evidence to the cloud and get back a JSON result.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="aspectData"></param>
        protected override void ProcessEngine(IFlowData data, CloudRequestData aspectData)
        {
            string jsonResult = string.Empty;

            var content = GetContent(data);
            if (_logger != null && _logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug($"Sending request to cloud service at " +
                    $"'{_dataEndpoint}'. Content: {content}");
            }

            var request = _httpClient.PostAsync(_dataEndpoint, content);
            jsonResult = request.Result.Content.ReadAsStringAsync().Result;

            aspectData.JsonResponse = jsonResult;
        }

        /// <summary>
        /// Generate the Content to send in the POST request
        /// </summary>
        /// <param name="data"></param>
        /// <returns>Evidence in a FormUrlEncodedContent object</returns>
        private FormUrlEncodedContent GetContent(IFlowData data)
        {
            var evidence = data.GetEvidence().AsDictionary();

            var queryData = new Dictionary<string, string>();

            queryData.Add("resource", _resourceKey);

            if(string.IsNullOrWhiteSpace(_licenseKey) == false)
            {
                queryData.Add("license", _licenseKey);
            }

            foreach (var item in evidence) {
                var key = item.Key.Split(Core.Constants.EVIDENCE_SEPERATOR.ToCharArray());
                queryData.Add(key[key.Length - 1], item.Value.ToString());
            }

            var content = new FormUrlEncodedContent(queryData);
            return content;
        }

        protected override void UnmanagedResourcesCleanup()
        {
        }

        /// <summary>
        /// Get the properties that are available from the cloud service.
        /// </summary>
        private void GetCloudProperties()
        {
            string jsonResult = string.Empty;

            try
            {
                var request = _httpClient.GetAsync($"{_propertiesEndpoint}?resource={_resourceKey}");
                jsonResult = request.Result.Content.ReadAsStringAsync().Result;
            }
            catch (Exception ex)
            {
                throw new Exception ($"Failed to retrieve available properties " +
                    $"from cloud service at {_propertiesEndpoint}.", ex);
            }

            if (string.IsNullOrEmpty(jsonResult) == false)
            {
                var accessiblePropertyData = JsonConvert
                    .DeserializeObject<LicencedProducts>(jsonResult);

                _publicProperties = accessiblePropertyData.Products;
            }
            else
            {
                throw new Exception($"Failed to retrieve available properties " +
                    $"from cloud service at {_propertiesEndpoint}.");
            }
        }

        /// <summary>
        /// Get the evidence keys that are required by the cloud service.
        /// </summary>
        private void GetCloudEvidenceKeys()
        {
            string jsonResult = string.Empty;

            try
            {
                var request = _httpClient.GetAsync(_evidenceKeysEndpoint);
                jsonResult = request.Result.Content.ReadAsStringAsync().Result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve evidence keys " +
                    $"from cloud service at {_evidenceKeysEndpoint}.", ex);
            }

            if (string.IsNullOrEmpty(jsonResult) == false)
            {
                var evidenceKeys = JsonConvert
                    .DeserializeObject<List<string>>(jsonResult);

                _evidenceKeyFilter = new EvidenceKeyFilterWhitelist(evidenceKeys);
            }
            else
            {
                throw new Exception($"Failed to retrieve evidence keys " +
                    $"from cloud service at {_evidenceKeysEndpoint}.");
            }
        }
    }
}
