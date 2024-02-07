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

using FiftyOne.Pipeline.Web.Common.Tests;
using FiftyOne.Pipeline.Web.Selenium.Tests.Templates;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using static FiftyOne.Pipeline.Web.Common.Tests.SeleniumTestHelper;

namespace FiftyOne.Pipeline.Web.Selenium.Tests
{
    /// <summary>
    /// Checks that the object name override that changes the default "fod"
    /// name to another name operates as expected.
    /// </summary>
    [TestClass]
    public class ObjectName : MinFlowElementsBase
    {

        /// <summary>
        /// Listener used for the test pages that include the JavaScript 
        /// resource served from the web host.
        /// </summary>
        private static HttpListener _clientListener;

        /// <summary>
        /// The page URL
        /// </summary>
        private static string _pageUrl;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            // Start the web host to serve the 51degrees.core.js include used
            // in the test pages. Change the object name used by the
            // 51Degrees.core.js script in the configuration to the same value
            // as that used in the template page script.
            MinFlowElementsBase.StartWebHost(
                context.CancellationTokenSource, 
                new Dictionary<string, string>()
            {
                ["PipelineOptions:Elements:1:BuildParameters:ObjectName"] =
                "testObjectName"
            });

            // Start the web host that will contain the test page asset.
            var view = new Dictionary<string, string>()
            {
                ["javaScriptUrl"] = 
                    _webHost.ServerAddresses.Addresses.First() + 
                    "/51degrees.core.js",
            };
            var testPage = SeleniumTestHelper.ParseTemplate<PlaceHolder>(
                "objectName.mustache", 
                view);
            _pageUrl = 
                $"http://localhost:{TestHelpers.GetRandomUnusedPort()}/";
            _clientListener = TestHelpers.SimpleListener(
                _pageUrl, 
                testPage,
                _webHost.StopSource.Token);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            // Stop the web host and trigger the stop token.
            _webHost.Stop();

            // Close the listener.
            _clientListener.Stop();
            while (_clientListener.IsListening)
            {
                _clientListener.Stop();
                Thread.Sleep(1000);
            }
            _clientListener.Close();
        }

        [TestMethod]
        [DynamicData(
            nameof(SeleniumTestHelper.Browsers), 
            typeof(SeleniumTestHelper),
            DynamicDataSourceType.Method,
            DynamicDataDisplayName = 
                nameof(SeleniumTestHelper.GetBrowserDisplayName),
            DynamicDataDisplayNameDeclaringType = typeof(SeleniumTestHelper))]
        public void Test(Browser browser)
        {
            // Act
            var sessionId = GotoPage(browser);

            // Verify
            Assert.IsFalse(
                String.IsNullOrWhiteSpace(sessionId),
                "Session id must not be empty string");
            Console.WriteLine($"Session Id: {sessionId}");
        }

        private string GotoPage(Browser browser)
        {
            // Goto the test page.
            browser.Driver.Navigate().GoToUrl(_pageUrl);

            // Wait until the session id has been populated.
            new WebDriverWait(browser.Driver,
                TimeSpan.FromSeconds(10)).Until(
                webDriver => browser.Driver.ExecuteScript(
                    "return sessionId").Equals("") == false);

            // Record the session id.
            return browser.Driver.ExecuteScript(
                "return sessionId") as string;
        }
    }
}
