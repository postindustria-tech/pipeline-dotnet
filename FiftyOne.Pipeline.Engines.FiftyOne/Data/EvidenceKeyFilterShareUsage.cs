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
using FiftyOne.Pipeline.Engines.FiftyOne.FlowElements;
using FiftyOne.Pipeline.Engines.FiftyOne.Trackers;
using System.Collections.Generic;

namespace FiftyOne.Pipeline.Engines.FiftyOne.Data
{
    /// <summary>
    /// This filter is used by the <see cref="ShareUsageElement"/>.
    /// It will include anything that is:
    /// 1) An HTTP header that is not blocked by the constructor parameter.
    /// 2) A cookie that starts with <see cref="Constants.FIFTYONE_COOKIE_PREFIX"/>
    ///     or is the asp session cookie (if configured in the constructor).
    /// 3) An query string parameters that have been configured to be shared
    ///     using the constructor parameter.
    /// 4) Not a header, cookie or query string parameter.
    /// </summary>
    /// <remarks>
    /// As this filter is generally inclusive, it will often cause far more
    /// evidence to be passed into a pipeline than the engine-specific 
    /// filters, which tend to be based on a white list such as
    /// <see cref="EvidenceKeyFilterWhitelist"/>.
    /// </remarks>
    public class EvidenceKeyFilterShareUsage : IEvidenceKeyFilter
    {
        /// <summary>
        /// If true then the asp.net session cookie will be included in
        /// the filter.
        /// </summary>
        /// <remarks>
        /// The session cookie is used by the <see cref="ShareUsageTracker"/> 
        /// but we do not actually want to share it.
        /// </remarks>
        private bool _includeSession;

        /// <summary>
        /// The cookie name being used to store the asp.net session id.
        /// </summary>
        private string _aspSessionCookieName;

        /// <summary>
        /// The content of HTTP headers in this array will not be included in 
        /// the request information sent to 51degrees.
        /// Any header names added here are hard-coded to be blocked
        /// regardless of the settings passed to the constructor.
        /// </summary>
        private HashSet<string> _blockedHttpHeaders = new HashSet<string>()
        {
            "cookies"
        };

        /// <summary>
        /// Query string parameters will not be shared by default.
        /// Any query string parameters to be shared must be added to this
        /// collection.
        /// </summary>
        private HashSet<string> _includedQueryStringParams = new HashSet<string>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="blockedHttpHeaders">
        /// A list of the names of the HTTP headers that share usage should
        /// not send to 51Degrees.
        /// </param>
        /// <param name="includedQueryStringParams">
        /// A list of the names of query string parameters that share 
        /// usage should send to 51Degrees.
        /// </param>
        /// <param name="includeSession">
        /// If true then the asp.net session cookie will be included in
        /// the filter.
        /// </param>
        /// <param name="aspSessionCookieName">
        /// The name of the cookie that contains the asp.net session id. 
        /// </param>
        public EvidenceKeyFilterShareUsage(
            List<string> blockedHttpHeaders,
            List<string> includedQueryStringParams,
            bool includeSession,
            string aspSessionCookieName)
        {
            _includeSession = includeSession;
            _aspSessionCookieName = aspSessionCookieName.ToLower();
            foreach (var header in blockedHttpHeaders)
            {
                var lowerHeader = header.ToLower();
                if (!_blockedHttpHeaders.Contains(lowerHeader))
                {
                    _blockedHttpHeaders.Add(lowerHeader);
                }
            }
            foreach (var parameter in includedQueryStringParams)
            {
                var lowerParameter = parameter.ToLower();
                if (!_includedQueryStringParams.Contains(lowerParameter))
                {
                    _includedQueryStringParams.Add(lowerParameter);
                }
            }
        }

        /// <summary>
        /// Check if the specified evidence key is included by this filter.
        /// </summary>
        /// <param name="key">
        /// The key to check
        /// </param>
        /// <returns>
        /// True if the key is included and false if not.
        /// </returns>
        public virtual bool Include(string key)
        {
            bool result = false;
            int firstSeperator = key.IndexOf(Core.Constants.EVIDENCE_SEPERATOR);
            if (firstSeperator > 0)
            {
                string firstPart = key.Remove(firstSeperator);
                string lastPart = key.Substring(firstSeperator + 1);

                if (firstPart == Core.Constants.EVIDENCE_HTTPHEADER_PREFIX)
                {
                    // Add the header to the list if the header name does not 
                    // appear in the list of blocked headers.
                    result = _blockedHttpHeaders
                        .Contains(lastPart.ToLower()) == false;
                }
                else if (firstPart == Core.Constants.EVIDENCE_COOKIE_PREFIX)
                {
                    // Only add cookies that start with the 51Degrees cookie 
                    // prefix.
                    result = lastPart.StartsWith(Constants.FIFTYONE_COOKIE_PREFIX) ||
                        (_includeSession && lastPart.Equals(_aspSessionCookieName));
                }
                else if (firstPart == Core.Constants.EVIDENCE_SESSION_PREFIX)
                {
                    // Only add session values that start with the 51Degrees
                    // cookie prefix.
                    result = lastPart.StartsWith(Constants.FIFTYONE_COOKIE_PREFIX);
                }
                else if (firstPart == Core.Constants.EVIDENCE_QUERY_PREFIX)
                {
                    // Only include query string parameters that have been
                    // specified in the constructor.
                    result = _includedQueryStringParams.Contains(lastPart.ToLower());
                }
                else
                { 
                    // Add anything that is not a cookie or a header.
                    result = true;
                }
            }
            else
            {
                result = true;
            }

            return result;
        }

        public int? Order(string key)
        {
            return 100;
        }
    }
}
