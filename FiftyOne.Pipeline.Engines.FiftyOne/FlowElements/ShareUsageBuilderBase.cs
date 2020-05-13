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

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("FiftyOne.Pipeline.Engines.FiftyOne.Tests")]
namespace FiftyOne.Pipeline.Engines.FiftyOne.FlowElements
{
    /// <summary>
    /// Abstract base class for ShareUsageElement builders.
    /// </summary>
    /// <typeparam name="T">Element type</typeparam>
    public abstract class ShareUsageBuilderBase<T>
    {
        /// <summary>
        /// The logger factory used by this builder
        /// </summary>
        protected ILoggerFactory LoggerFactory { get; private set; }

        /// <summary>
        /// The logger to be used by this builder
        /// </summary>
        protected ILogger Logger { get; private set; }

        /// <summary>
        /// Where a set of evidence values exactly matches a previously seen
        /// set of evidence values, it will not be shared if that situation 
        /// occurs within this time interval. (in minutes)
        /// </summary>
        protected int RepeatEvidenceInterval { get; private set; } = Constants.SHARE_USAGE_DEFAULT_REPEAT_EVIDENCE_INTERVAL;
        /// <summary>
        /// The approximate proportion of events to share.
        /// Specified as a floating point number from 0 to 1.
        /// </summary>
        protected double SharePercentage { get; private set; } = Constants.SHARE_USAGE_DEFAULT_SHARE_PERCENTAGE;
        /// <summary>
        /// The minimum number of entries to be present in the XML
        /// PAyload before it is sent to the usage sharing endpoint.
        /// </summary>
        protected int MinimumEntriesPerMessage { get; private set; } = Constants.SHARE_USAGE_DEFAULT_MIN_ENTRIES_PER_MESSAGE;
        /// <summary>
        /// Set the maximum number of entries to be stored in the queue to be
        /// sent. This must be more than the minimum entries per message.
        /// </summary>
        protected int MaximumQueueSize { get; private set; } = Constants.SHARE_USAGE_DEFAULT_MAX_QUEUE_SIZE;
        /// <summary>
        /// The timeout in milliseconds to allow when attempting to add an
        /// item to the queue. If this timeout is exceeded then usage sharing
        /// will be disabled.
        /// </summary>
        protected int AddTimeout { get; private set; } = Constants.SHARE_USAGE_DEFAULT_ADD_TIMEOUT;
        /// <summary>
        /// The timeout in milliseconds to allow when attempting to take an
        /// item from the queue in order to send to the remote service.
        /// </summary>
        protected int TakeTimeout { get; private set; } = Constants.SHARE_USAGE_DEFAULT_TAKE_TIMEOUT;
        /// <summary>
        /// The remote endpoint to send usage data to. 
        /// </summary>
        [Obsolete("Use the ShareUsageUri property instead. This property may be removed in the future.")]
#pragma warning disable CA1056 // Uri properties should not be strings
        protected string ShareUsageUrl => ShareUsageUri.AbsoluteUri;
#pragma warning restore CA1056 // Uri properties should not be strings
        /// <summary>
        /// The remote endpoint to send usage data to. 
        /// </summary>
        protected Uri ShareUsageUri { get; private set; } = new Uri(Constants.SHARE_USAGE_DEFAULT_URL);
        /// <summary>
        /// The name of the cookie that contains the asp.net session id.
        /// This is used to help prevent the same usage data being shared
        /// multiple times.
        /// </summary>
        protected string AspSessionCookieName { get; private set; } = Engines.Constants.DEFAULT_ASP_COOKIE_NAME;
        /// <summary>
        /// A list of HTTP headers that should not be shared.
        /// </summary>
        protected List<string> BlockedHttpHeaders { get; private set; } = new List<string>();
        /// <summary>
        /// A list of query string parameters to be shared.
        /// </summary>
        protected List<string> IncludedQueryStringParameters { get; private set; } = new List<string>();
        /// <summary>
        /// A collection of evidence keys and values which, if present,
        /// cause the event to be ignored for the purposes of usage sharing.
        /// </summary>
        protected List<KeyValuePair<string, string>> IgnoreDataEvidenceFilter { get; private set; } = new List<KeyValuePair<string, string>>();
        /// <summary>
        /// Controls whether session tracking is enabled or disabled.
        /// If enabled, requests from a single user session will only be 
        /// shared once.
        /// </summary>
        protected bool TrackSession { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="loggerFactory">
        /// The <see cref="ILoggerFactory"/> to use when creating loggers for
        /// a <see cref="ShareUsageElement"/>.
        /// </param>
        public ShareUsageBuilderBase(
            ILoggerFactory loggerFactory)
        {
            LoggerFactory = loggerFactory;
            Logger = LoggerFactory.CreateLogger<ShareUsageBuilder>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loggerFactory"></param>
        /// <param name="logger"></param>
        public ShareUsageBuilderBase(
            ILoggerFactory loggerFactory,
            ILogger logger)
        {
            LoggerFactory = loggerFactory;
            Logger = logger;
        }

        /// <summary>
        /// Add parameter names to the (case insensitive) list of names 
        /// of query string parameters that will be sent to 51Degrees.
        /// </summary>
        /// <param name="queryStringParameterNames">
        /// The names of the query string parameter to include.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the parameter is null
        /// </exception>
        public ShareUsageBuilderBase<T> SetIncludedQueryStringParameters(List<string> queryStringParameterNames)
        {
            if (queryStringParameterNames == null)
            {
                throw new ArgumentNullException(nameof(queryStringParameterNames));
            }

            foreach (var name in queryStringParameterNames)
            {
                IncludedQueryStringParameters.Add(name);
            }
            return this;
        }

        /// <summary>
        /// Add parameter names to the (case insensitive) list of names 
        /// of query string parameters that will be sent to 51Degrees.
        /// </summary>
        /// <param name="queryStringParameterNames">
        /// A comma separated list of names of the query string parameter to
        /// include.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the parameter is null
        /// </exception>
        public ShareUsageBuilderBase<T> SetIncludedQueryStringParameters(string queryStringParameterNames)
        {
            if (queryStringParameterNames == null)
            {
                throw new ArgumentNullException(nameof(queryStringParameterNames));
            }

            return SetIncludedQueryStringParameters(
                new List<string>(queryStringParameterNames.Split(',')));
        }

        /// <summary>
        /// Add a parameter name to the (case insensitive) list of names 
        /// of query string parameters that will be sent to 51Degrees.
        /// </summary>
        /// <param name="queryStringParameterName">
        /// The name of the query string parameter to include.
        /// </param>
        public ShareUsageBuilderBase<T> SetIncludedQueryStringParameter(string queryStringParameterName)
        {
            IncludedQueryStringParameters.Add(queryStringParameterName);
            return this;
        }

        /// <summary>
        /// Set the (case insensitive) names of HTTP headers that will 
        /// not be sent to 51 degrees.
        /// Some headers, such as 'cookies' are hard-coded to be blocked
        /// regardless of this setting. 
        /// </summary>
        /// <param name="blockedHeaders">
        /// The names of the headers to block.
        /// </param>
        public ShareUsageBuilderBase<T> SetBlockedHttpHeaders(List<string> blockedHeaders)
        {
            BlockedHttpHeaders = blockedHeaders;
            return this;
        }

        /// <summary>
        /// Add a header to the (case insensitive) list of names of HTTP 
        /// headers that will not be sent to 51 degrees.
        /// Some headers, such as 'cookies' are hard-coded to be blocked
        /// regardless of this setting.  
        /// </summary>
        /// <param name="blockedHeader">
        /// The name of the header to block.
        /// </param>
        public ShareUsageBuilderBase<T> SetBlockedHttpHeader(string blockedHeader)
        {
            BlockedHttpHeaders.Add(blockedHeader);
            return this;
        }

        /// <summary>
        /// Adds evidence key:values to the filter such that requests which 
        /// contain this evidence are ignored.
        /// </summary>
        /// <param name="evidenceFilter">Comma separated string containing 
        /// evidence keys and evidence to ignore.</param>
        /// <returns></returns>
        public ShareUsageBuilderBase<T> SetIgnoreFlowDataEvidenceFilter(string evidenceFilter)
        {
            if (string.IsNullOrWhiteSpace(evidenceFilter) == false)
            {
                foreach (var kvpString in evidenceFilter.Split(','))
                {
                    if (kvpString.Contains(":"))
                    {
                        KeyValuePair<string, string> kvp =
                            new KeyValuePair<string, string>(kvpString.Split(':')[0], kvpString.Split(':')[1]);
                        IgnoreDataEvidenceFilter.Add(kvp);
                    }
                    else
                    {
                        string msg = string.Format(CultureInfo.InvariantCulture,
                            Messages.MessageShareUsageInvalidConfig,
                            "IgnoreFlowDataEvidenceFilter",
                            kvpString);
                        Logger.LogWarning(msg);
                    }
                }
            }
            else
            {
                string msg = string.Format(CultureInfo.InvariantCulture,
                    Messages.MessageShareUsageInvalidConfig,
                    "IgnoreFlowDataEvidenceFilter", "");
                Logger.LogWarning(msg);
            }
            return this;
        }

        /// <summary>
        /// Set the percentage of data that the <see cref="ShareUsageElement"/>
        /// should be sharing.
        /// </summary>
        /// <param name="sharePercentage">
        /// The proportion of events sent to the pipeline that should be
        /// shared to 51Degrees.
        /// 1 = 100%, 0.5 = 50%, etc.
        /// </param>
        public ShareUsageBuilderBase<T> SetSharePercentage(double sharePercentage)
        {
            SharePercentage = sharePercentage;
            return this;
        }


        /// <summary>
        /// Set the minimum number of entries to be aggregated by the
        /// <see cref="ShareUsageElement"/> before they are sent to the
        /// remote service.
        /// </summary>
        /// <param name="minimumEntriesPerMessage">
        /// The minimum number of entries to be aggregated by the
        /// <see cref="ShareUsageElement"/> before they are sent to the
        /// remote service.
        /// </param>
        public ShareUsageBuilderBase<T> SetMinimumEntriesPerMessage(int minimumEntriesPerMessage)
        {
            MinimumEntriesPerMessage = minimumEntriesPerMessage;
            return this;
        }

        /// <summary>
        /// Set the maximum number of entries to be stored in the queue to be
        /// sent. This must be more than the minimum entries per message.
        /// </summary>
        /// <param name="size">Size to set</param>
        public ShareUsageBuilderBase<T> SetMaximumQueueSize(int size)
        {
            MaximumQueueSize = size;
            return this;
        }

        /// <summary>
        /// Set the timeout in milliseconds to allow when attempting to add an
        /// item to the queue. If this timeout is exceeded then usage sharing
        /// will be disabled.
        /// </summary>
        /// <param name="milliseconds">Timeout to set</param>
        public ShareUsageBuilderBase<T> SetAddTimeout(int milliseconds)
        {
            AddTimeout = milliseconds;
            return this;
        }

        /// <summary>
        /// Set the timeout in milliseconds to allow when attempting to take an
        /// item from the queue in order to send to the remote service.
        /// </summary>
        /// <param name="milliseconds">Timeout to set</param>
        public ShareUsageBuilderBase<T> SetTakeTimeout(int milliseconds)
        {
            TakeTimeout = milliseconds;
            return this;
        }

        /// <summary>
        /// Set the URL to use when sharing usage data.
        /// </summary>
        /// <param name="shareUsageUrl">
        /// The URL to use when sharing usage data.
        /// </param>
        public ShareUsageBuilderBase<T> SetShareUsageUrl(string shareUsageUrl)
        {
            ShareUsageUri = new Uri(shareUsageUrl);
            return this;
        }

        /// <summary>
        /// Set the URL to use when sharing usage data.
        /// </summary>
        /// <param name="shareUsageUrl">
        /// The URL to use when sharing usage data.
        /// </param>
        public ShareUsageBuilderBase<T> SetShareUsageUrl(Uri shareUsageUrl)
        {
            ShareUsageUri = shareUsageUrl;
            return this;
        }

        /// <summary>
        /// Set the name of the cookie that contains the asp.net session id.
        /// </summary>
        /// <param name="cookieName">
        /// The name of the cookie that contains the asp.net session id.
        /// </param>
        public ShareUsageBuilderBase<T> SetAspSessionCookieName(string cookieName)
        {
            AspSessionCookieName = cookieName;
            return this;
        }

        /// <summary>
        /// Set the interval which determines if a non-unique piece of evidence 
        /// is repeat evidence or new.
        /// </summary>
        /// <param name="interval">
        /// The interval in minutes.
        /// </param>
        public ShareUsageBuilderBase<T> SetRepeatEvidenceIntervalMinutes(int interval)
        {
            RepeatEvidenceInterval = interval;
            return this;
        }

        /// <summary>
        /// Enable or disable session tracking.
        /// </summary>
        /// <param name="track">Boolean value set's whether the usage 
        /// element should track sessions.</param>
        /// <returns></returns>
        public ShareUsageBuilderBase<T> SetTrackSession(bool track)
        {
            TrackSession = track;
            return this;
        }

        /// <summary>
        /// Create the <see cref="ShareUsageElement"/>
        /// </summary>
        /// <returns>
        /// The newly created element.
        /// </returns>
        public abstract T Build();
    }
}
