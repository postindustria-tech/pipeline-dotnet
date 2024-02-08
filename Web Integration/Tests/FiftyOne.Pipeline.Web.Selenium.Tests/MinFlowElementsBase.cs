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

using Examples.ClientSideEvidence.MVC;
using FiftyOne.Pipeline.Web.Common.Tests;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading;

namespace FiftyOne.Pipeline.Web.Selenium.Tests
{
    /// <summary>
    /// Uses the Examples.ClientSideEvidence.MVC web host without the client 
    /// side evidence flow element for tests of the JavaScript and Json 
    /// flow elements. Avoids creating yet another project example for these
    /// low level minimal tests.
    /// </summary>
    public class MinFlowElementsBase
    {
        /// <summary>
        /// Instance of the web host used for all tests in the class.
        /// </summary>
        public static WebHostInstance _webHost;

        /// <summary>
        /// Configures and starts the web host that servers 51Degrees.core.js 
        /// with the minimal flow elements JavaScript and Json builder.
        /// </summary>
        /// <param name="initialData">
        /// Configuration key value pairs.
        /// </param>
        protected static void StartWebHost(
            CancellationToken stopToken,
            IEnumerable<KeyValuePair<string, string>> initialData = null)
        {
            // Start the web host to serve the 51degrees.core.js include used
            // in the test pages. Configures only the minimal flow elements.
            _webHost = SeleniumTestHelper.StartLocalHost<Startup>(
                (options) => 
                {
                    // Clear all configuration as not needed and might change
                    // in the examples.
                    options.Sources.Clear();

                    // Add the minimal configuration to that provided in the
                    // parameter.
                    var data = initialData == null ?
                        new Dictionary<string, string>() :
                        new Dictionary<string, string>(initialData);

                    // No need to record the logging information in tests
                    data["Logging:LogLevel:Default"] = "None";
                    
                    // Only use the two elements needed for client side
                    // minimal tests.
                    data["PipelineOptions:Elements:0:BuilderName"] =
                        "JsonBuilderElement";
                    data["PipelineOptions:Elements:1:BuilderName"] =
                        "JavaScriptBuilderElement";
                
                    // Set the configuration.
                    options.AddInMemoryCollection(data);
                }, stopToken);
        }
    }
}
