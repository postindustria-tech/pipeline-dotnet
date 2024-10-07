/* *********************************************************************
 * This Original Work is copyright of 51 Degrees Mobile Experts Limited.
 * Copyright 2023 51 Degrees Mobile Experts Limited, Davidson House,
 * Forbury Square, Reading, Berkshire, United Kingdom RG1 3EU.
 *
 * This Original Work is licensed under the European Union Public Licence
 * (EUPL) v.1.2 and is subject to its terms as set out below.
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
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace FiftyOne.Pipeline.CloudRequestEngine.FlowElements
{
    /// <summary>
    /// Engine that makes requests to the 51Degrees cloud service.
    /// See the <see href="https://github.com/51Degrees/specifications/blob/main/pipeline-specification/pipeline-elements/cloud-request-engine.md">Specification</see> 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming",
        "CA1724:Type names should not match namespaces",
        Justification = "This would be a breaking change so will be " +
        "addressed in a future version.")]
    public class CloudRequestEngine :
        AspectEngineBase<CloudRequestData, IAspectPropertyMetaData>,
        ICloudRequestEngine
    {
        /// <summary>
        /// Separator as a character array.
        /// </summary>
        private static readonly char[] EVIDENCE_SEPARATOR_CHAR_ARRAY = 
            Core.Constants.EVIDENCE_SEPERATOR.ToCharArray();

        private HttpClient _httpClient;

        private string _dataEndpoint;
        private string _resourceKey;
        private string _licenseKey;
        private string _propertiesEndpoint;
        private string _evidenceKeysEndpoint;
        private string _cloudRequestOrigin;

        /// <summary>
        /// Used to cancel HTTP requests that are in progress. Usually the
        /// application's cancellation token. 
        /// </summary>
        private CancellationToken? _stopToken;

        /// <summary>
        /// Not currently used.
        /// </summary>
        private List<string> _requestedProperties;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="logger">
        /// The logger for this instance
        /// </param>
        /// <param name="aspectDataFactory">
        /// A factory function to use when creating new 
        /// <see cref="CloudRequestData"/> instances.
        /// </param>
        /// <param name="httpClient">
        /// The HttpClient instance to use when making requests
        /// </param>
        /// <param name="licenseKey">
        /// The license key to use when making requests.
        /// This parameter is obsolete, use resourceKey instead.
        /// </param>
        /// <param name="resourceKey">
        /// The resource key to use when making requests.
        /// A resource key encapsulates details such as any license keys,
        /// the properties that should be returned and the domains that
        /// requests are permitted from. A new resource key can be
        /// generated for free at https://configure.51degrees.com
        /// </param>
        /// <param name="dataEndpoint">
        /// The URL for the cloud endpoint that will take the supplied
        /// evidence and return JSON formatted data.
        /// </param>
        /// <param name="propertiesEndpoint">
        /// The URL for the cloud endpoint that will return meta-data
        /// on properties that will be populated when the data endpoint 
        /// is called using the given resource key.
        /// </param>
        /// <param name="evidenceKeysEndpoint">
        /// The URL for the cloud endpoint that will return meta-data
        /// on the evidence that will be used when the data endpoint 
        /// is called using the given resource key.
        /// </param>
        /// <param name="timeout">
        /// The timeout for HTTP requests in seconds.
        /// </param>
        /// <param name="requestedProperties">
        /// Not currently used.
        /// </param>
        /// <param name="cloudRequestOrigin">
        /// The value to use for the Origin header when making requests 
        /// to cloud.
        /// </param>
        /// <param name="stopToken">
        /// Used to cancel HTTP requests that are in progress. Usually the
        /// application's cancellation token. 
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if a required parameter is null.
        /// </exception>
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
            List<string> requestedProperties,
            string cloudRequestOrigin = null,
            CancellationToken? stopToken = null)
            : base(logger, aspectDataFactory)
        {
            if (httpClient == null) throw new ArgumentNullException(nameof(httpClient));

            try
            {
                _dataEndpoint = dataEndpoint;
                _resourceKey = resourceKey;
                _licenseKey = licenseKey;
                _propertiesEndpoint = propertiesEndpoint;
                _evidenceKeysEndpoint = evidenceKeysEndpoint;
                _requestedProperties = requestedProperties;
                _cloudRequestOrigin = cloudRequestOrigin;
                _stopToken = stopToken;

                _httpClient = httpClient;
                if (timeout > 0)
                {
                    _httpClient.Timeout = new TimeSpan(0, 0, timeout);
                }
                else
                {
                    _httpClient.Timeout = new TimeSpan(0, 0, 0, 0, -1);
                }

                _propertyMetaData = new List<IAspectPropertyMetaData>()
                {
                     new AspectPropertyMetaData(this, 
                         "json-response", 
                         typeof(string), 
                         "",
                         new List<string>(), 
                         true),
                     new AspectPropertyMetaData(
                         this, 
                         "process-started", 
                         typeof(bool), 
                         "", 
                         new List<string>(), 
                         true)
                };

                // Start the tasks to get the evidence keys and the public
                // properties from the cloud service. If the stop token is
                // not provided then warn via logging.
                if (stopToken != null && stopToken.HasValue)
                {
                    _evidenceKeyFilterTask = Task.Run(() => 
                        GetCloudEvidenceKeys(), 
                        stopToken.Value);
                    _publicPropertiesTask = Task.Run(() => 
                        GetCloudProperties(), 
                        stopToken.Value);
                }
                else
                {
                    _evidenceKeyFilterTask = Task.Run(() => 
                        GetCloudEvidenceKeys());
                    _publicPropertiesTask = Task.Run(() => 
                        GetCloudProperties());
                    Logger?.LogWarning(
                        $"Parameter '{nameof(stopToken)}' should not be null");
                }
            }
            catch (Exception ex)
            {
                Logger?.LogCritical(ex, $"Error creating {this.GetType().Name}");
                throw;
            }
        }

        private List<IAspectPropertyMetaData> _propertyMetaData;

        /// <summary>
        /// A collection of the properties available in the aspect
        /// data instance that is populated by this engine.
        /// </summary>
        public override IList<IAspectPropertyMetaData> Properties => 
            _propertyMetaData;

        /// <summary>
        /// The 'tier' of the source data used to service this request.
        /// </summary>
        public override string DataSourceTier => "cloud";

        /// <summary>
        /// The key for this element's data within an <see cref="IFlowData"/>
        /// instance.
        /// </summary>
        public override string ElementDataKey => "cloud-response";

        /// <summary>
        /// A task that is started in the constructor and when complete returns
        /// the instance of IEvidenceKeyFilter.
        /// </summary>
        private Task<IEvidenceKeyFilter> _evidenceKeyFilterTask;

        /// <summary>
        /// A task that is started in the constructor and when complete returns
        /// the instance of IReadOnlyDictionary{string, ProductMetaData}.
        /// </summary>
        private Task<IReadOnlyDictionary<string, ProductMetaData>> 
            _publicPropertiesTask;

        /// <summary>
        /// A filter object that indicates the evidence keys that can be used 
        /// by this engine. This will vary based on the supplied resource key 
        /// so will be populated after a call to the cloud service as part of 
        /// object initialization.
        /// </summary>
        /// <exception cref="CloudRequestException">
        /// Thrown if there is an error from the cloud service or
        /// there is no data in the response.
        /// </exception>
        public override IEvidenceKeyFilter EvidenceKeyFilter 
        {
            get
            {
                try
                {
                    _evidenceKeyFilter = _evidenceKeyFilterTask.Result;
                }
                catch (AggregateException ex)
                {
                    if (ex.InnerException is CloudRequestException cloudException)
                    {
                        throw new CloudRequestException(
                            cloudException.Message,
                            cloudException.HttpStatusCode,
                            cloudException.ResponseHeaders,
                            ex);
                    }
                    _evidenceKeyFilter = new EvidenceKeyFilterWhitelist(
                        new List<string>(0));
                    Logger?.LogWarning(
                        "Could not fetch evidence key filter from '{0}'",
                        _evidenceKeysEndpoint);
                }
                return _evidenceKeyFilter;
            }
        }
        private IEvidenceKeyFilter _evidenceKeyFilter;

        /// <summary>
        /// A collection of the properties that the cloud service can
        /// populate in the JSON response.
        /// Keyed on property name.
        /// </summary>
        /// <remarks>
        /// Returns null if the task has not finished.
        /// </remarks>
        /// <exception cref="CloudRequestException">
        /// Thrown if there is an error from the cloud service or
        /// there is no data in the response.
        /// </exception>
        public IReadOnlyDictionary<string, ProductMetaData> PublicProperties {
            get
            {
                try
                {
                    _publicProperties = _publicPropertiesTask.Result;
                }
                catch (AggregateException ex)
                {
                    if (ex.InnerException is CloudRequestException cloudException)
                    {
                        throw new CloudRequestException(
                            cloudException.Message,
                            cloudException.HttpStatusCode,
                            cloudException.ResponseHeaders,
                            ex);
                    }
                    _publicProperties = 
                        new Dictionary<string, ProductMetaData>(0);
                    Logger?.LogWarning(
                        "Could not fetch public properties from '{0}'",
                        _propertiesEndpoint);
                }
                return _publicProperties;
            }
        }
        private IReadOnlyDictionary<string, ProductMetaData> _publicProperties;

        /// <summary>
        /// Send evidence to the cloud and get back a JSON result.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="aspectData"></param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if a required parameter is null.
        /// </exception>
        /// <exception cref="CloudRequestException">
        /// Thrown if there is an error from the cloud service or
        /// there is no data in the response.
        /// </exception>
        protected override void ProcessEngine(
            IFlowData data, 
            CloudRequestData aspectData)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            if (aspectData == null)
            {
                throw new ArgumentNullException(nameof(aspectData));
            }

            aspectData.ProcessStarted = true;

            string jsonResult = string.Empty;

            using (var content = GetContent(data))
            using (var requestMessage =
                new HttpRequestMessage(HttpMethod.Post, _dataEndpoint))
            {
                if (Logger?.IsEnabled(LogLevel.Debug) == true)
                {
                    Logger?.LogDebug($"Sending request to cloud service at " +
                        $"'{_dataEndpoint}'. Content: {content}");
                }

                requestMessage.Content = content;
                jsonResult = ProcessResponse(
                    AddCommonHeadersAndSend(requestMessage, data.ProcessingCancellationToken));
            }

            aspectData.JsonResponse = jsonResult;
        }

        /// <summary>
        /// Validate the JSON response from the cloud service.
        /// An exception will be throw if any type of error has 
        /// occurred.
        /// </summary>
        /// <param name="response">
        /// The JSON content that is returned from the cloud service.
        /// </param>
        /// <param name="checkForErrorMessages">
        /// Set to false if the response will never contain error message
        /// text.
        /// </param>
        /// <exception cref="CloudRequestException">
        /// Thrown if there is an error from the cloud service or
        /// there is no data in the response.
        /// </exception>
        private string ProcessResponse(
            HttpResponseMessage response, 
            bool checkForErrorMessages = true)
        {
            var jsonResult = response.Content.ReadAsStringAsync().Result;
            var hasData = string.IsNullOrEmpty(jsonResult) == false;
            List<string> messages = new List<string>();

            Func<Dictionary<string, string>> GetHeaders = () =>
            {
                // Get the response headers. 
                return response.Headers.ToDictionary(
                    kvp => kvp.Key,
                    kvp => string.Join(", ", kvp.Value));
            };

            if (hasData && checkForErrorMessages)
            {
                JObject jObj;
                try
                {
                    jObj = JObject.Parse(jsonResult);
                }
                catch (JsonReaderException ex)
                {
                    throw new CloudRequestException(
                        "Failed to parse server's response as JSON", 
                        (int)response.StatusCode, GetHeaders(), ex);
                }
                var hasErrors = jObj.ContainsKey("errors");
                hasData = hasErrors ?
                    jObj.Values().Count() > 1 :
                    jObj.Values().Any();

                if (hasErrors)
                {
                    var errors = jObj.Value<JArray>("errors");
                    messages.AddRange(errors.Children<JValue>().Select(t => 
                        t.Value.ToString()));
                }

                // Log any warnings that were returned.
                if (jObj.ContainsKey("warnings"))
                {
                    var warnings = jObj.Value<JArray>("warnings");
                    foreach (var warning in warnings.Children<JValue>()
                        .Select(t => t.Value.ToString()))
                    {
                        Logger?.LogWarning(warning);
                    }
                }
            }
            
            // If there were no errors but there was also no other data
            // in the response then add an explanation to the list of 
            // messages.
            if (messages.Count == 0 &&
                hasData == false)
            {
                var msg = string.Format(CultureInfo.InvariantCulture,
                    Messages.MessageNoDataInResponse,
                    _dataEndpoint);
                messages.Add(msg);
            }
            // If there is no detailed error message, but we got a
            // non-success status code, then add a message to the list
            else if (messages.Count == 0 && 
                response.IsSuccessStatusCode == false)
            {
                var msg = string.Format(CultureInfo.InvariantCulture,
                    Messages.MessageErrorCodeReturned,
                    _dataEndpoint,
                    response.StatusCode,
                    jsonResult);
                messages.Add(msg);
            }

            // If there are any errors returned from the cloud service 
            // then throw an exception
            if (messages.Count > 1)
            {
                var headers = GetHeaders();
                var aggregated = new AggregateException(
                    Messages.ExceptionCloudErrorsMultiple,
                    messages.Select(m => new CloudRequestException(m, 
                        (int)response.StatusCode, headers)));
                throw new CloudRequestException(
                    Messages.ExceptionCloudErrorsMultiple,
                    (int)response.StatusCode, GetHeaders(), aggregated);
            }
            else if (messages.Count == 1)
            {
                var msg = string.Format(CultureInfo.InvariantCulture,
                    Messages.ExceptionCloudError,
                    messages[0]);
                throw new CloudRequestException(msg, 
                    (int)response.StatusCode, GetHeaders());
            }

            return jsonResult;
        }

        /// <summary>
        /// Generate the Content to send in the POST request. The evidence keys
        /// e.g. 'query.' and 'header.' have an order of precedence. These are
        /// added to the evidence in reverse order, if there is conflict then 
        /// the queryData value is overwritten. 
        /// 
        /// 'query.' evidence should take precedence over all other evidence.
        /// If there are evidence keys other than 'query.' that conflict then
        /// this is unexpected so an error will be logged.
        /// </summary>
        /// <param name="data"></param>
        /// <returns>Evidence in a FormUrlEncodedContent object</returns>
        private FormUrlEncodedContent GetContent(IFlowData data)
        {
            var queryData = new Dictionary<string, string>();

            queryData.Add("resource", _resourceKey);

            if (string.IsNullOrWhiteSpace(_licenseKey) == false)
            {
                queryData.Add("license", _licenseKey);
            }

            var evidence = data.GetEvidence().AsDictionary();

            // Add evidence in reverse alphabetical order, excluding special
            // keys. 
            AddQueryData(queryData, evidence, evidence
                .Where(e =>
                    KeyHasPrefix(e, Core.Constants.EVIDENCE_QUERY_PREFIX) == false &&
                    KeyHasPrefix(e, Core.Constants.EVIDENCE_HTTPHEADER_PREFIX) == false &&
                    KeyHasPrefix(e, Core.Constants.EVIDENCE_COOKIE_PREFIX) == false)
                .OrderByDescending(e => e.Key));
            // Add cookie evidence.
            AddQueryData(queryData, evidence, evidence
                .Where(e => KeyHasPrefix(e, Core.Constants.EVIDENCE_COOKIE_PREFIX)));
            // Add header evidence.
            AddQueryData(queryData, evidence, evidence
                .Where(e => KeyHasPrefix(e, Core.Constants.EVIDENCE_HTTPHEADER_PREFIX)));
            // Add query evidence.
            AddQueryData(queryData, evidence, evidence
                .Where(e => KeyHasPrefix(e, Core.Constants.EVIDENCE_QUERY_PREFIX)));

            var content = new FormUrlEncodedContent(queryData);
            return content;
        }
        
        /// <summary>
        /// Check that the key of a KeyValuePair has the given prefix.
        /// </summary>
        /// <param name="item">The KeyValuePair to check.</param>
        /// <param name="prefix">The prefix to check for.</param>
        /// <returns>True if the key has the prefix.</returns>
        private static bool KeyHasPrefix(
            KeyValuePair<string, object> item, 
            string prefix) 
        {
            var key = item.Key.Split(EVIDENCE_SEPARATOR_CHAR_ARRAY);
            return key[0].Equals(
                prefix, 
                StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Add query data to the evidence.
        /// </summary>
        /// <param name="queryData">
        /// The destination dictionary to add query data to.
        /// </param>
        /// <param name="allEvidence">
        /// All evidence in the flow data. This is used to report which 
        /// evidence keys are conflicting.
        /// </param>
        /// <param name="evidence">
        /// Evidence to add to the query Data.
        /// </param>
        private void AddQueryData(
            Dictionary<string, string> queryData,
            IReadOnlyDictionary<string, object> allEvidence,
            IEnumerable<KeyValuePair<string, object>> evidence)
        {
            foreach (var item in evidence)
            {
                // Get the key parts
                var key = item.Key.Split(EVIDENCE_SEPARATOR_CHAR_ARRAY);
                var prefix = key[0];
                var suffix = key.Last();

                // Check and add the evidence to the query parameters.
                if (queryData.ContainsKey(suffix) == false)
                {
                    queryData.Add(suffix, item.Value.ToString());
                }
                // If the queryParameter exists already...
                else
                {
                    // Get the conflicting pieces of evidence and then log a 
                    // warning, if the evidence prefix is not query. Otherwise
                    // a warning is not needed as query evidence is expected 
                    // to overwrite any existing evidence with the same suffix.
                    if (prefix.Equals(Core.Constants.EVIDENCE_QUERY_PREFIX, 
                        StringComparison.InvariantCultureIgnoreCase) == false)
                    {
                        var oldValue = queryData[suffix];
                        var conflicts = allEvidence
                            .Where(e => e.Key != item.Key)
                            .Where(e => e.Key.EndsWith(suffix, 
                                StringComparison.InvariantCultureIgnoreCase))
                            .Select(e => $"{e.Key}={e.Value}");

                        Logger?.LogWarning(
                            $"'{item.Key}={item.Value}' evidence conflicts " +
                            $"with '{string.Join("', '", conflicts)}'");
                    }
                    // Overwrite the existing queryParameter value.
                    queryData[suffix] = item.Value.ToString();
                }
            }
        }

        /// <summary>
        /// Cleanup any unmanaged resources
        /// </summary>
        protected override void UnmanagedResourcesCleanup()
        {
        }

        /// <summary>
        /// Get the properties that are available from the cloud service.
        /// </summary>
        /// <returns>
        /// The value to be saved into <see cref="PublicProperties"/>
        /// </returns>
        /// <exception cref="CloudRequestException">
        /// Thrown if there is an error from the cloud service or
        /// there is no data in the response.
        /// </exception>
        private IReadOnlyDictionary<string, ProductMetaData> GetCloudProperties()
        {
            var jsonResult = string.Empty;
            Func<string> ErrorMessage = () => 
                "Failed to retrieve available properties " +
                $"from cloud service at {_propertiesEndpoint}.";

            var url = $"{_propertiesEndpoint}?resource={_resourceKey}";
            using (var requestMessage =
                new HttpRequestMessage(HttpMethod.Get, url))
            {
                try
                {
                    jsonResult = ProcessResponse(AddCommonHeadersAndSend(
                        requestMessage,
                        _stopToken ?? default));
                }
                catch (Exception ex)
                {
                    Logger?.LogError(ErrorMessage(), ex);
                    throw;
                }
            }

            if (string.IsNullOrEmpty(jsonResult) == false)
            {
                var accessiblePropertyData = JsonConvert
                    .DeserializeObject<LicencedProducts>(jsonResult);

                return accessiblePropertyData.Products;
            }
            else
            {
                throw new Exception(ErrorMessage());
            }
        }

        /// <summary>
        /// Get the evidence keys that can be consumed by the cloud service.
        /// </summary>
        /// <returns>
        /// The value to be saved into <see cref="EvidenceKeyFilter"/>.
        /// </returns>
        private IEvidenceKeyFilter GetCloudEvidenceKeys()
        {
            var jsonResult = string.Empty;
            Func<string> ErrorMessage = () => 
                "Failed to retrieve evidence keys " +
                $"from cloud service at {_evidenceKeysEndpoint}.";

            using (var requestMessage =
                new HttpRequestMessage(HttpMethod.Get, _evidenceKeysEndpoint))
            {
                try
                {
                    // Note - Don't check for error messages in the response
                    // as it is a flat JSON array.
                    jsonResult = ProcessResponse(
                        AddCommonHeadersAndSend(requestMessage, _stopToken ?? default), false);
                }
                catch (Exception ex)
                {
                    Logger?.LogError(ErrorMessage(), ex);
                    return null;
                }
            }

            if (string.IsNullOrEmpty(jsonResult) == false)
            {
                var evidenceKeys = JsonConvert
                    .DeserializeObject<List<string>>(jsonResult);
                return new EvidenceKeyFilterWhitelist(evidenceKeys);
            }
            else
            {
                Logger?.LogWarning(ErrorMessage());
                return null;
            }
        }

        /// <summary>
        /// Add the common headers to the specified message and send it.
        /// </summary>
        /// <param name="request">
        /// The request to send
        /// </param>
        /// <param name="cancellationToken">
        /// Token to cancel HTTP request.
        /// </param>
        /// <returns>
        /// The response
        /// </returns>
        private HttpResponseMessage AddCommonHeadersAndSend(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(_cloudRequestOrigin) == false &&
                (request.Headers.Contains(Constants.ORIGIN_HEADER_NAME) == false ||
                request.Headers.GetValues(Constants.ORIGIN_HEADER_NAME).Contains(
                    _cloudRequestOrigin) == false))
            {
                request.Headers.Add(Constants.ORIGIN_HEADER_NAME, _cloudRequestOrigin);
            }
            return SendRequestAsync(request, cancellationToken);
        }

        /// <summary>
        /// Send a request and handle any exception if one is thrown.
        /// </summary>
        /// <param name="request">
        /// The request to send
        /// </param>
        /// <param name="cancellationToken">
        /// Token to cancel HTTP request.
        /// </param>
        /// <returns>
        /// The response
        /// </returns>
        private HttpResponseMessage SendRequestAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            try
            {
                return _httpClient.SendAsync(request, cancellationToken).Result;
            }
            catch (AggregateException ex)
            {
                throw new CloudRequestException(
                    Messages.ExceptionCloudResponseFailure,
                    ex);
            }
        } 
    }
}
