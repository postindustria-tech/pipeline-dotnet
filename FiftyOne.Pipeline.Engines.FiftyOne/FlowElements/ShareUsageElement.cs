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

using FiftyOne.Pipeline.Engines.Trackers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Xml;

[assembly: InternalsVisibleTo("FiftyOne.Pipeline.Engines.FiftyOne.Tests")]
namespace FiftyOne.Pipeline.Engines.FiftyOne.FlowElements
{
    /// <summary>
    /// Flow element that sends usage data to 51Degrees for analysis.
    /// The type and quantity of data being sent can be customised using the 
    /// options on the constructor.
    /// By default, data is queued until there are at least 50 items in memory.
    /// It is then serialised to an XML file and sent to the specified URL.
    /// </summary>
    public class ShareUsageElement : ShareUsageBase
    {
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
        internal ShareUsageElement(
            ILogger<ShareUsageElement> logger,
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
        /// <param name="tracker"></param>
        internal ShareUsageElement(
            ILogger<ShareUsageElement> logger,
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
            : base(logger,
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
                  tracker)
        {
        }

        /// <summary>
        /// Create the XML file from the specified data and send it to
        /// the configured URL.
        /// </summary>
        protected override void BuildAndSendXml()
        {
            List<ShareUsageData> allData = new List<ShareUsageData>();
            ShareUsageData currentData;
            while (EvidenceCollection.TryTake(out currentData, TakeTimeout) &&
                allData.Count < MinEntriesPerMessage * 2)
            {
                allData.Add(currentData);
            }

            // Create the zip stream and XML writer.
            using (MemoryStream memory = new MemoryStream())
            {
                using (GZipStream compressedStream = new GZipStream(
                    memory, CompressionMode.Compress, true))
                using (XmlWriter writer = XmlWriter.Create(compressedStream, WriterSettings))
                {
                    writer.WriteStartElement("Devices");
                    // Write each element in turn.
                    foreach (var data in allData)
                    {
                        WriteData(writer, data);
                    }
                    writer.WriteEndElement();
                }

                memory.Position = 0;
                using (var content = new StreamContent(memory))
                {
                    content.Headers.Add("content-encoding", "gzip");
                    content.Headers.Add("content-type", "text/xml");

                    var res = HttpClient.PostAsync(ShareUsageUri, content).Result;
                    if (res.StatusCode != HttpStatusCode.OK)
                    {
                        throw new HttpRequestException(
                            $"HTTP response was {res.StatusCode}: " +
                            $"{res.Content.ToString()}.");
                    }
                }
            }
        }
    }
}
