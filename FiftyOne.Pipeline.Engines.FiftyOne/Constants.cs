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

namespace FiftyOne.Pipeline.Engines.FiftyOne.FlowElements
{
    /// <summary>
    /// Constants used by 51Degrees Aspect Engines.
    /// </summary>
    public class Constants
    {
        /// <summary>
        /// The maximum length of a piece of evidence's value which can be
        /// added to the usage data being sent.
        /// </summary>
        internal const int SHARE_USAGE_MAX_EVIDENCE_LENGTH = 10000;

        /// <summary>
        /// The default maximum size of the usage data queue which is stored
        /// before sending to the remote service.
        /// </summary>
        internal const int SHARE_USAGE_DEFAULT_MAX_QUEUE_SIZE = 1000;

        /// <summary>
        /// The default timeout in milliseconds to use when adding usage data
        /// to the queue.
        /// </summary>
        internal const int SHARE_USAGE_DEFAULT_ADD_TIMEOUT = 5;

        /// <summary>
        /// The default timeout in milliseconds to use when taking usage data
        /// from the queue.
        /// </summary>
        internal const int SHARE_USAGE_DEFAULT_TAKE_TIMEOUT = 100;

        /// <summary>
        /// The default repeat evidence interval in minutes.
        /// This is the maximum time to consider a set of evidence as a 
        /// match for another (and thus, prevent it from being shared)
        /// </summary>
        internal const int SHARE_USAGE_DEFAULT_REPEAT_EVIDENCE_INTERVAL = 20;

        /// <summary>
        /// The default percentage of requests to include in usage sharing.
        /// 1 = 100%
        /// 0.5 = 50%
        /// etc.
        /// </summary>
        internal const float SHARE_USAGE_DEFAULT_SHARE_PERCENTAGE = 1;

        /// <summary>
        /// The default minimum entries per usage sharing message.
        /// No data will be sent until this amount is reached.
        /// </summary>
        internal const int SHARE_USAGE_DEFAULT_MIN_ENTRIES_PER_MESSAGE = 50;

        /// <summary>
        /// The default URL to send usage data to
        /// </summary>
        internal const string SHARE_USAGE_DEFAULT_URL = "https://devices-v4.51degrees.com/new.ashx";

        public const string EVIDENCE_SESSIONID_SUFFIX = "session-id";

        /// <summary>
        /// session id evidence constant.
        /// </summary>
        public const string EVIDENCE_SESSIONID =
            Core.Constants.EVIDENCE_QUERY_PREFIX +
            Core.Constants.EVIDENCE_SEPERATOR +
            EVIDENCE_SESSIONID_SUFFIX;

        public const string EVIDENCE_SEQUENCE_SUFIX = "sequence";

        /// <summary>
        /// Sequence evidence contant.
        /// </summary>
        public const string EVIDENCE_SEQUENCE =
            Core.Constants.EVIDENCE_QUERY_PREFIX +
            Core.Constants.EVIDENCE_SEPERATOR +
            EVIDENCE_SEQUENCE_SUFIX;

        /// <summary>
        /// Constant Added to blocked HTTP Headers so that the cookie header is 
        /// blocked by default.
        /// </summary>
        internal const string EVIDENCE_HTTPHEADER_COOKIE_SUFFIX = "cookie";

    }
}
