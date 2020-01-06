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

using FiftyOne.Caching;
using FiftyOne.Pipeline.Core.Data;
using FiftyOne.Pipeline.Core.FlowElements;
using FiftyOne.Pipeline.Engines.Configuration;
using FiftyOne.Pipeline.Engines.FiftyOne.Data;
using FiftyOne.Pipeline.Engines.FiftyOne.Trackers;
using FiftyOne.Pipeline.Engines.Trackers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

[assembly: InternalsVisibleTo("FiftyOne.Pipeline.Engines.FiftyOne.Tests")]
namespace FiftyOne.Pipeline.Engines.FiftyOne.FlowElements
{
    /// <summary>
    /// Abstract base class for ShareUsage elements. 
    /// Contains common functionality such as filtering the evidence and
    /// building the XML records.
    /// </summary>
    public abstract class ShareUsageBase : 
        FlowElementBase<IElementData, IElementPropertyMetaData>, IDisposable
    {
        /// <summary>
        /// The HttpClient to use when sending the data.
        /// </summary>
        protected HttpClient _httpClient;

        /// <summary>
        /// Inner class that is used to store details of data in memory
        /// prior to it being sent to 51Degrees.
        /// </summary>
        protected class ShareUsageData
        {
            public string SessionId { get; set; }
            public int Sequence { get; set; }
            public string ClientIP { get; set; }
            public Dictionary<string, Dictionary<string, string>> EvidenceData { get; set; } =
                new Dictionary<string, Dictionary<string, string>>();
        }

        /// <summary>
        /// IP Addresses of local host device.
        /// </summary>
        private static readonly IPAddress[] LOCALHOSTS = new IPAddress[]
        {
            IPAddress.Parse("127.0.0.1"),
            IPAddress.Parse("::1")
        };

        /// <summary>
        /// Queue used to store entries in memory prior to them being sent
        /// to 51Degrees.
        /// </summary>
        protected BlockingCollection<ShareUsageData> _evidenceCollection;

        /// <summary>
        /// The current task sending data to the remote service.
        /// </summary>
        private volatile Task _sendDataTask = null;

        /// <summary>
        /// Lock to use when starting a new send data task.
        /// </summary>
        private volatile object _lock = new object();

        /// <summary>
        /// Timeout to use when adding to the queue.
        /// </summary>
        private int _addTimeout;

        /// <summary>
        /// Timeout to use when taking from the queue.
        /// </summary>
        protected int _takeTimeout;

        private Random _rng = new Random();

        /// <summary>
        /// The tracker to use to determine if a <see cref="FlowData"/>
        /// instance should be shared or not.
        /// </summary>
        private ITracker _tracker;

        /// <summary>
        /// The minimum number of request entries per message sent to 51Degrees.
        /// </summary>
        protected int _minEntriesPerMessage = Constants.SHARE_USAGE_DEFAULT_MIN_ENTRIES_PER_MESSAGE;

        /// <summary>
        /// The interval is a timespan which is used to determine if a piece
        /// of repeated evidence should be considered new evidence to share.
        /// If the evidence from a request matches that in the tracker but this
        /// interval has elapsed then the tracker will track it as new evidence.
        /// </summary>
        private TimeSpan _interval = new TimeSpan(0, Constants.SHARE_USAGE_DEFAULT_REPEAT_EVIDENCE_INTERVAL, 0);

        /// <summary>
        /// The approximate proportion of requests to be shared.
        /// 1 = 100%, 0.5 = 50%, etc.
        /// </summary>
        private double _sharePercentage = Constants.SHARE_USAGE_DEFAULT_SHARE_PERCENTAGE;

        private List<string> _flowElements = null;

        /// <summary>
        /// Return a list of <see cref="IFlowElement"/> in the pipeline. 
        /// If the list is null then populate from the pipeline.
        /// If there are multiple or no pipelines then log an error.
        /// </summary>
        private List<string> FlowElements
        {
            get
            {
                if (_flowElements == null)
                {
                    IPipeline pipeline = null;
                    if (Pipelines.Count == 1)
                    {
                        pipeline = Pipelines.Single();
                        _flowElements = new List<string>(pipeline.FlowElements
                            .Select(e => e.GetType().FullName));
                    }
                    else
                    {
                        // This element has somehow been registered to too 
                        // many (or zero) pipelines.
                        // This means we cannot know the flow elements that
                        // make up the pipeline so a warning is logged
                        // but otherwise, the system can continue as normal.
                        _logger.LogWarning($"Share usage element registered " +
                            $"to {(Pipelines.Count > 0 ? "too many" : "no")}" +
                            $" pipelines. Unable to send share usage information.");
                        _flowElements = new List<string>();
                    }
                }
                return _flowElements;
            }
        }

        private string _osVersion = "";
        private string _languageVersion = "";
        private string _coreVersion = "";
        private string _enginesVersion = "";
        protected string _shareUsageUrl = "";

        protected XmlWriterSettings _writerSettings = new XmlWriterSettings()
        {
            ConformanceLevel = ConformanceLevel.Document,
            Encoding = Encoding.UTF8,
            CheckCharacters = true,
            NewLineHandling = NewLineHandling.None,
            CloseOutput = true,
        };

        public override string ElementDataKey
        {
            get { return "shareusage"; }
        }

        private IEvidenceKeyFilter _evidenceKeyFilter;

        /// <summary>
        /// Get the evidence key filter for this element.
        /// </summary>
        public override IEvidenceKeyFilter EvidenceKeyFilter
        {
            get { return _evidenceKeyFilter; }
        }

        private IEvidenceKeyFilter _evidenceKeyFilterExclSession;

        private List<KeyValuePair<string, string>> _ignoreDataEvidenceFilter;

        private string _hostAddress;
        private IList<IElementPropertyMetaData> _properties;

        /// <summary>
        /// Set to true if the evidence within a flow data contains invalid XML
        /// characters such as control characters.
        /// </summary>
        private bool _flagBadSchema;

        /// <summary>
        /// Get the IP address of the machine that this code is running on.
        /// </summary>
        private string HostAddress
        {
            get
            {
                if (_hostAddress == null)
                {
                    IPAddress[] addresses = Dns.GetHostAddresses(Dns.GetHostName());

                    var address = addresses.FirstOrDefault(a => !IsLocalHost(a) &&
                        a.AddressFamily == AddressFamily.InterNetwork);
                    if (address == null)
                    {
                        address = addresses.FirstOrDefault(a => !IsLocalHost(a));
                    }

                    _hostAddress = address == null ? "" : address.ToString();
                }
                return _hostAddress;
            }
        }

        public override IList<IElementPropertyMetaData> Properties
        {
            get { return _properties; }
        }

        /// <summary>
        /// Indicates whether share usage has been canceled as a result of an
        /// error.
        /// </summary>
        protected internal bool IsCanceled { get; set; } = false;

        /// <summary>
        /// True if there is a task running to send usage data to the remote
        /// service.
        /// </summary>
        internal bool IsRunning
        {
            get => SendDataTask != null &&
                SendDataTask.IsCompleted == false &&
                SendDataTask.IsFaulted == false;
        }

        /// <summary>
        /// Currently running task which is sending the usage data to the
        /// remote service.
        /// </summary>
        internal Task SendDataTask
        {
            get => _sendDataTask;
            private set => _sendDataTask = value;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger">
        /// The logger to use.
        /// </param>
        /// <param name="httpClient">
        /// The <see cref="HttpClient"/> to use when sending request data.
        /// </param>
        /// <param name="sharePercentage">
        /// The approximate proportion of requests to share. 
        /// 1 = 100%, 0.5 = 50%, etc.
        /// </param>
        /// <param name="minimumEntriesPerMessage">
        /// The minimum number of request entries per message sent to 51Degrees.
        /// </param>
        /// <param name="maximumQueueSize">
        /// The maximum number of items to hold in the queue at one time. This
        /// must be larger than minimum entries.
        /// </param>
        /// <param name="addTimeout">
        /// The timeout in milliseconds to allow when attempting to add an
        /// item to the queue. If this timeout is exceeded then usage sharing
        /// will be disabled.
        /// </param>
        /// <param name="takeTimeout">
        /// The timeout in milliseconds to allow when attempting to take an
        /// item to the queue.
        /// </param>
        /// <param name="repeatEvidenceIntervalMinutes">
        /// The interval (in minutes) which is used to decide if repeat 
        /// evidence is old enough to consider a new session.
        /// </param>
        /// <param name="trackSession">
        /// Set if the tracker should consider sessions in share usage.
        /// </param>
        /// <param name="shareUsageUrl">
        /// The URL to send data to
        /// </param>
        /// <param name="blockedHttpHeaders">
        /// A list of the names of the HTTP headers that share usage should
        /// not send to 51Degrees.
        /// </param>
        /// <param name="includedQueryStringParameters">
        /// A list of the names of query string parameters that share 
        /// usage should send to 51Degrees.
        /// </param>
        /// <param name="ignoreDataEvidenceFilter"></param>
        /// <param name="aspSessionCookieName">
        /// The name of the cookie that contains the asp.net session id.
        /// </param>
        protected ShareUsageBase(
            ILogger<ShareUsageBase> logger,
            HttpClient httpClient,
            double sharePercentage,
            int minimumEntriesPerMessage,
            int maximumQueueSize,
            int addTimeout,
            int takeTimeout,
            int repeatEvidenceIntervalMinutes,
            bool trackSession,
            string shareUsageUrl,
            List<string> blockedHttpHeaders,
            List<string> includedQueryStringParameters,
            List<KeyValuePair<string, string>> ignoreDataEvidenceFilter,
            string aspSessionCookieName = Engines.Constants.DEFAULT_ASP_COOKIE_NAME)
            : this(logger,
                  httpClient,
                  sharePercentage,
                  minimumEntriesPerMessage,
                  maximumQueueSize,
                  addTimeout,
                  takeTimeout,
                  repeatEvidenceIntervalMinutes,
                  trackSession,
                  shareUsageUrl,
                  blockedHttpHeaders,
                  includedQueryStringParameters,
                  ignoreDataEvidenceFilter,
                  aspSessionCookieName,
                  null)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger">
        /// The logger to use.
        /// </param>
        /// <param name="httpClient">
        /// <see cref="HttpClient"/> to use when sending request data.
        /// </param>
        /// <param name="sharePercentage">
        /// The approximate proportion of requests to share. 
        /// 1 = 100%, 0.5 = 50%, etc.
        /// </param>
        /// <param name="minimumEntriesPerMessage">
        /// The minimum number of request entries per message sent to 51Degrees.
        /// </param>
        /// <param name="maximumQueueSize">
        /// The maximum number of items to hold in the queue at one time. This
        /// must be larger than minimum entries.
        /// </param>
        /// <param name="addTimeout">
        /// The timeout in milliseconds to allow when attempting to add an
        /// item to the queue. If this timeout is exceeded then usage sharing
        /// will be disabled.
        /// </param>
        /// <param name="takeTimeout">
        /// The timeout in milliseconds to allow when attempting to take an
        /// item to the queue.
        /// </param>
        /// <param name="repeatEvidenceIntervalMinutes">
        /// The interval (in minutes) which is used to decide if repeat 
        /// evidence is old enough to consider a new session.
        /// </param>
        /// <param name="trackSession">
        /// Set if the tracker should consider sessions in share usage.
        /// </param>
        /// <param name="shareUsageUrl">
        /// The URL to send data to
        /// </param>
        /// <param name="blockedHttpHeaders">
        /// A list of the names of the HTTP headers that share usage should
        /// not send to 51Degrees.
        /// </param>
        /// <param name="includedQueryStringParameters">
        /// A list of the names of query string parameters that share 
        /// usage should send to 51Degrees.
        /// </param>
        /// <param name="ignoreDataEvidenceFilter"></param>
        /// <param name="aspSessionCookieName">
        /// The name of the cookie that contains the asp.net session id.
        /// </param>
        /// <param name="tracker">
        /// The <see cref="ITracker"/> to use to determine if a given 
        /// <see cref="IFlowData"/> instance should be shared or not.
        /// </param>
        protected ShareUsageBase(
            ILogger<ShareUsageBase> logger,
            HttpClient httpClient,
            double sharePercentage,
            int minimumEntriesPerMessage,
            int maximumQueueSize,
            int addTimeout,
            int takeTimeout,
            int repeatEvidenceIntervalMinutes,
            bool trackSession,
            string shareUsageUrl,
            List<string> blockedHttpHeaders,
            List<string> includedQueryStringParameters,
            List<KeyValuePair<string, string>> ignoreDataEvidenceFilter,
            string aspSessionCookieName,
            ITracker tracker)
            : base(logger)
        {
            if (minimumEntriesPerMessage > maximumQueueSize)
            {
                throw new ArgumentException(
                    "The minimum entries per message cannot be larger than " +
                    "the maximum size of the queue.");
            }

            // Make sure the cookie headers are ignored.
            if (!blockedHttpHeaders.Contains(Constants.EVIDENCE_HTTPHEADER_COOKIE_SUFFIX))
            {
                blockedHttpHeaders.Add(Constants.EVIDENCE_HTTPHEADER_COOKIE_SUFFIX);
            }

            _logger = logger;
            _httpClient = httpClient;

            _evidenceCollection = new BlockingCollection<ShareUsageData>(maximumQueueSize);

            _addTimeout = addTimeout;
            _takeTimeout = takeTimeout;
            _sharePercentage = sharePercentage;
            _minEntriesPerMessage = minimumEntriesPerMessage;
            _interval = TimeSpan.FromMinutes(repeatEvidenceIntervalMinutes);
            _shareUsageUrl = shareUsageUrl;

            // Some data is going to stay the same on all requests so we can 
            // gather that now.
            _languageVersion = Environment.Version.ToString();
            _osVersion = Environment.OSVersion.VersionString;

            _enginesVersion = GetType().Assembly
                .GetCustomAttribute<AssemblyFileVersionAttribute>().Version;
            _coreVersion = typeof(IPipeline).Assembly
                .GetCustomAttribute<AssemblyFileVersionAttribute>().Version;

            includedQueryStringParameters.Add(Constants.EVIDENCE_SESSIONID_SUFFIX);
            includedQueryStringParameters.Add(Constants.EVIDENCE_SEQUENCE_SUFIX);

            _evidenceKeyFilter = new EvidenceKeyFilterShareUsage(
                blockedHttpHeaders, includedQueryStringParameters, true, aspSessionCookieName);
            _evidenceKeyFilterExclSession = new EvidenceKeyFilterShareUsage(
                blockedHttpHeaders, includedQueryStringParameters, false, aspSessionCookieName);

            _ignoreDataEvidenceFilter = ignoreDataEvidenceFilter;

            _tracker = tracker;
            // If no tracker was supplied then create the default one.
            if (_tracker == null)
            {
                _tracker = new ShareUsageTracker(new CacheConfiguration()
                {
                    Builder = new LruPutCacheBuilder(),
                    Size = 1000
                },
                _interval,
               new EvidenceKeyFilterShareUsageTracker(blockedHttpHeaders, includedQueryStringParameters, trackSession, aspSessionCookieName));
            }

            _properties = new List<IElementPropertyMetaData>();
        }

        /// <summary>
        /// Add 
        /// </summary>
        /// <param name="pipeline"></param>
        public override void AddPipeline(IPipeline pipeline)
        {
            if (Pipelines.Count > 0)
            {
                throw new Exception($"Cannot add ShareUsageElement to " +
                    $"multiple pipelines.");
            }
            base.AddPipeline(pipeline);
        }

        /// <summary>
        /// Process the data
        /// </summary>
        /// <param name="data">
        /// The <see cref="IFlowData"/> instance that provides the evidence
        /// </param>
        protected override void ProcessInternal(IFlowData data)
        {
            bool ignoreData = false;
            var evidence = data.GetEvidence().AsDictionary();

            if (_ignoreDataEvidenceFilter != null)
            {
                foreach (var kvp in _ignoreDataEvidenceFilter)
                {
                    if (evidence.ContainsKey(kvp.Key))
                    {
                        if (evidence[kvp.Key].ToString() == kvp.Value)
                        {
                            ignoreData = true;
                            break;
                        }
                    }
                }
            }

            // If the evidence does not contain a session id then create a new one.
            if (evidence.ContainsKey(Constants.EVIDENCE_SESSIONID) == false)
            {
                data.AddEvidence(Constants.EVIDENCE_SESSIONID, GetNewSessionId());
            }

            // If the evidence does not have a sequence then add one. Otherwise
            // increment it.
            if (evidence.ContainsKey(Constants.EVIDENCE_SEQUENCE) == false)
            {
                data.AddEvidence(Constants.EVIDENCE_SEQUENCE, 1);
            }
            else if (evidence.TryGetValue(Constants.EVIDENCE_SEQUENCE, out object sequence))
            {
                if (sequence is int result || (sequence is string seq && int.TryParse(seq, out result)))
                {
                    data.AddEvidence(Constants.EVIDENCE_SEQUENCE, result + 1);
                }
                else
                {
                    _logger.LogError("Failed to increment usage sequence number.");
                }
            }
            else
            {
                _logger.LogError("Failed to retrieve sequence number.");
            }

            if (IsCanceled == false && ignoreData == false)
            {
                ProcessData(data);
            }
        }

        /// <summary>
        /// Send any data which has built up locally and not yet been sent to
        /// the remote service.
        /// </summary>
        protected override void ManagedResourcesCleanup()
        {
            TrySendData();
            if (IsRunning)
            {
                SendDataTask.Wait();
            }
        }

        /// <summary>
        /// Clean up any unmanaged resources.
        /// </summary>
        protected override void UnmanagedResourcesCleanup()
        {
        }

        /// <summary>
        /// Returns true if the request is from the local host IP address.
        /// </summary>
        /// <param name="address">
        /// The IP address to be checked.
        /// </param>
        /// <returns>
        /// True if from the local host IP address.
        /// </returns>
        private static bool IsLocalHost(IPAddress address)
        {
            return LOCALHOSTS.Any(host => host.Equals(address));
        }

        /// <summary>
        /// Process the supplied request data
        /// </summary>
        /// <param name="data">
        /// The <see cref="IFlowData"/> instance that provides the evidence
        /// </param>
        private void ProcessData(IFlowData data)
        {
            if (_rng.NextDouble() <= _sharePercentage)
            {
                // Check if the tracker will allow sharing of this data
                if (_tracker.Track(data))
                {
                    // Extract the data we want from the evidence and add
                    // it to the collection.
                    if (_evidenceCollection.TryAdd(
                        GetDataFromEvidence(data.GetEvidence()),
                        _addTimeout) == true)
                    {
                        // If the collection has enough entries then start
                        // taking data from it to be sent.
                        if (_evidenceCollection.Count >= _minEntriesPerMessage)
                        {
                            TrySendData();
                        }
                    }
                    else
                    {
                        IsCanceled = true;
                        _logger.LogError("Share usage was canceled after " +
                            "failing to add data to the collection. This " +
                            "may mean that the max collection size is too " +
                            "low for the amount of traffic / min devices to " +
                            "send, or that the 'send' thread has stopped " +
                            "taking data from the collection.");

                    }
                }
            }
        }

        /// <summary>
        /// Extract the desired data from the evidence.
        /// In order to avoid problems with the evidence data being disposed 
        /// before it is sent, the data placed into a new object rather 
        /// than being a reference to the existing evidence instance.
        /// </summary>
        /// <param name="evidence">
        /// An <see cref="IEvidence"/> instance that contains the data to be
        /// extracted.
        /// </param>
        /// <returns>
        /// A <see cref="ShareUsageData"/> instance populated with data from
        /// the evidence.
        /// </returns>
        private ShareUsageData GetDataFromEvidence(IEvidence evidence)
        {
            ShareUsageData data = new ShareUsageData();

            foreach (var entry in evidence.AsDictionary())
            {
                if (entry.Key.Equals(Core.Constants.EVIDENCE_CLIENTIP_KEY))
                {
                    // The client IP is dealt with separately for backwards
                    // compatibility purposes.
                    data.ClientIP = entry.Value.ToString();
                }
                else if (entry.Key.Equals(Constants.EVIDENCE_SESSIONID))
                {
                    // The SessionID is dealt with separately.
                    data.SessionId = entry.Value.ToString();
                }
                else if (entry.Key.Equals(Constants.EVIDENCE_SEQUENCE))
                {
                    // The Sequence is dealt with separately.
                    var sequence = 0;
                    if (int.TryParse(entry.Value.ToString(), out sequence))
                    {
                        data.Sequence = sequence;
                    }
                }
                else
                {
                    // Check if we can send this piece of evidence
                    bool addToData = _evidenceKeyFilterExclSession.Include(entry.Key);

                    if (addToData)
                    {
                        // Get the category and field names from the evidence key.
                        string category = "";
                        string field = entry.Key;

                        int firstSeperator = entry.Key.IndexOf(Core.Constants.EVIDENCE_SEPERATOR);
                        if (firstSeperator > 0)
                        {
                            category = entry.Key.Remove(firstSeperator);
                            field = entry.Key.Substring(firstSeperator + 1);
                        }

                        // Get the evidence value.
                        string evidenceValue = entry.Value.ToString();
                        // If the value is longer than the permitted length 
                        // then truncate it.
                        if (evidenceValue.Length > Constants.SHARE_USAGE_MAX_EVIDENCE_LENGTH)
                        {
                            evidenceValue = "[TRUNCATED BY USAGE SHARING] " +
                                evidenceValue.Remove(Constants.SHARE_USAGE_MAX_EVIDENCE_LENGTH);
                        }

                        // Add the evidence to the dictionary.
                        Dictionary<string, string> categoryDict;
                        if (data.EvidenceData.TryGetValue(category, out categoryDict) == false)
                        {
                            categoryDict = new Dictionary<string, string>();
                            data.EvidenceData.Add(category, categoryDict);
                        }
                        categoryDict.Add(field, evidenceValue);
                    }
                }
            }

            return data;
        }

        /// <summary>
        /// Attempt to send the data to the remote service. This only happens
        /// if there is not a task already running.
        /// 
        /// If any error occurs while sending the data, then usage sharing is
        /// stopped.
        /// </summary>
        /// <returns></returns>
        protected void TrySendData()
        {
            if (IsCanceled == false &&
                IsRunning == false)
            {
                lock (_lock)
                {
                    if (IsRunning == false)
                    {
                        SendDataTask = Task.Run(() =>
                        {
                            try
                            {
                                BuildAndSendXml();
                            }
                            catch (Exception ex)
                            {
                                IsCanceled = true;
                                _logger.LogError(
                                    ex,
                                    "Share usage was canceled due to an error.");
                            }
                        });
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected abstract void BuildAndSendXml();

        /// <summary>
        /// Virtual method to be overridden in extending usage share elements.
        /// Write the specified data using the specified writer.
        /// </summary>
        /// <param name="writer">
        /// The <see cref="XmlWriter"/> to use.
        /// </param>
        /// <param name="data">
        /// The <see cref="ShareUsageData"/> to write.
        /// </param>
        protected virtual void WriteData(XmlWriter writer, ShareUsageData data)
        {
            writer.WriteStartElement("Device");

            WriteDeviceData(writer, data);

            writer.WriteEndElement();
        }

        /// <summary>
        /// Write the specified device data using the specified writer.
        /// </summary>
        /// <param name="writer">
        /// The <see cref="XmlWriter"/> to use.
        /// </param>
        /// <param name="data">
        /// The <see cref="ShareUsageData"/> to write.
        /// </param>
        protected void WriteDeviceData(XmlWriter writer, ShareUsageData data)
        {
            _flagBadSchema = false;

            // The SessionID used to track a series of requests
            writer.WriteElementString("SessionId", data.SessionId);
            // The sequence number of the request in a series of requests.
            writer.WriteElementString("Sequence", data.Sequence.ToString());
            // The UTC date/time this entry was written
            writer.WriteElementString("DateSent", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss"));
            // The version number of the Pipeline API
            writer.WriteElementString("Version", _coreVersion);
            // Write Pipeline information
            WritePipelineInfo(writer);
            // The software language
            writer.WriteElementString("Language", "dotnet");
            // The software language version
            writer.WriteElementString("LanguageVersion", _languageVersion);
            // The client IP of the request
            writer.WriteElementString("ClientIP", data.ClientIP);
            // The IP of this server
            writer.WriteElementString("ServerIP", HostAddress);
            // The OS name and version
            writer.WriteElementString("Platform", _osVersion);

            // Write all other evidence data that has been included.
            foreach (var category in data.EvidenceData)
            {
                foreach (var entry in category.Value)
                {
                    if (category.Key.Length > 0)
                    {
                        writer.WriteStartElement(category.Key);
                        writer.WriteAttributeString("Name", EncodeInvalidXMLChars(entry.Key));
                        writer.WriteCData(EncodeInvalidXMLChars(entry.Value));
                        writer.WriteEndElement();
                    }
                    else
                    {
                        writer.WriteElementString(EncodeInvalidXMLChars(entry.Key),
                            EncodeInvalidXMLChars(entry.Value));
                    }
                }
            }
            if (_flagBadSchema)
            {
                writer.WriteElementString("BadSchema", "true");
            }
        }

        /// <summary>
        /// Virtual method to write details about the pipeline.
        /// </summary>
        protected virtual void WritePipelineInfo(XmlWriter writer)
        {
            // The product name
            writer.WriteElementString("Product", "Pipeline");
            // The flow elements in the current pipeline
            foreach (var flowElement in FlowElements)
            {
                writer.WriteElementString("FlowElement", flowElement);
            }
        }

        /// <summary>
        /// encodes any unusual characters into their hex representation
        /// </summary>
        public string EncodeInvalidXMLChars(string text)
        {
            // Validate characters in string. If not valid check chars 
            // individually and build new string with encoded chars. Set _flag 
            // to add "bad schema" element into usage data.

            try
            {
                return XmlConvert.VerifyXmlChars(text);
            }
            catch (XmlException)
            {
                _flagBadSchema = true;
                var tmp = new StringBuilder();
                foreach (var c in text)
                {
                    if (XmlConvert.IsXmlChar(c))
                    {
                        tmp.Append(c);
                    }
                    else
                    {
                        tmp.Append("\\x" + Convert.ToByte(c).ToString("x4"));
                    }
                };

                return tmp.ToString();
            }
        }

        private string GetNewSessionId()
        {
            Guid g = Guid.NewGuid();
            return g.ToString();
        }
    }
}
