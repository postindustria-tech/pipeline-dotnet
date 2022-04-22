/* *********************************************************************
 * This Original Work is copyright of 51 Degrees Mobile Experts Limited.
 * Copyright 2022 51 Degrees Mobile Experts Limited, Davidson House,
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

using System.Resources;
using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("FiftyOne.Pipeline.Engines.Tests")]

namespace FiftyOne.Pipeline.Engines
{
    /// <summary>
    /// Static class containing various constants that are used by the 
    /// Pipeline and/or are helpful to callers. 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming",
        "CA1707:Identifiers should not contain underscores",
        Justification = "51Degrees coding style is for constant names " +
            "to be all-caps with an underscore to separate words.")]
    public static class Constants
    {
        /// <summary>
        /// The prefix that is added to all cookies set by 51Degrees
        /// client-side code that can be used as evidence.
        /// </summary>
        public const string FIFTYONE_COOKIE_PREFIX = "51d_";
        /// <summary>
        /// The default name of the cookie that holds the ID for the 
        /// ASP.NET session.
        /// </summary>
        public const string DEFAULT_ASP_COOKIE_NAME = "asp.net_sessionid";

        /// <summary>
        /// Default polling interval for the data update service
        /// in seconds.
        /// </summary>
        public const int DATA_UPDATE_POLLING_DEFAULT = 30 * 60;
        /// <summary>
        /// Default randomization to be applied to the calculated
        /// update timer interval in seconds.
        /// This is used to help prevent many requests hitting the 
        /// update distribution endpoint at exactly the same time.
        /// </summary>
        public const int DATA_UPDATE_RANDOMISATION_DEFAULT = 10 * 60;
    }
}
