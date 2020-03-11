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

namespace FiftyOne.Pipeline.Web
{
    /// <summary>
    /// Configuration options for MVC Pipeline operation.
    /// </summary>
    public class PipelineWebIntegrationOptions
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public PipelineWebIntegrationOptions()
        {
            ClientSideEvidenceEnabled = true;
            UseAsyncScript = true;
        }

        /// <summary>
        /// Flag to enable/disable client side evidence functionality.
        /// Client-side evidence comes into effect when there is not enough 
        /// information in the request to determine certain properties.
        /// For example the exact model of iPhone cannot be determined
        /// from the User-Agent.
        /// If enabled, this option allows the Pipeline to inject JavaScript 
        /// into the page and use this to determine a value for a property 
        /// using more information.
        /// Defaults to true.
        /// </summary>
        public bool ClientSideEvidenceEnabled { get; set; }

        /// <summary>
        /// Flag to enable/disable the use of the async attribute for
        /// the client side script.
        /// Defaults to true.
        /// </summary>
        public bool UseAsyncScript { get; set; }
    }
}
