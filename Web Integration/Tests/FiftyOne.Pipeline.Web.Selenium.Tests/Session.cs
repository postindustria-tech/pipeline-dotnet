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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;

namespace FiftyOne.Pipeline.Web.Selenium.Tests
{
    /// <summary>
    /// Checks that the session is report consistently across a series of web
    /// page interactions.
    /// </summary>
    [TestClass]
    public class Session : MinFlowElementsBase
    {

        /// <summary>
        /// Listener used for the test pages that include the JavaScript 
        /// resource served from the web host.
        /// </summary>
        private static HttpListener _clientListener;

        /// <summary>
        /// The URLs that will be accessed when checking the session id.
        /// </summary>
        private static string[] _pageUrls;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            // Start the web host to serve the 51degrees.core.js include used
            // in the test pages.
            MinFlowElementsBase.StartWebHost(
                context.CancellationTokenSource.Token);
            
            // Start the web host that will contain the test page asset.
            var view = new Dictionary<string, string>()
            {
                ["javaScriptUrl"] = 
                    _webHost.ServerAddresses.Addresses.First() + 
                    "/51degrees.core.js",
            };
            var testPage = SeleniumTestHelper.ParseTemplate<PlaceHolder>(
                "session.mustache", 
                view);
            var baseUrl = 
                $"http://localhost:{TestHelpers.GetRandomUnusedPort()}";
            _pageUrls = Enumerable.Range(1, 2).Select(i =>
                $"{baseUrl}/{i}/").ToArray();
            _clientListener = TestHelpers.SimpleListener(
                _pageUrls, 
                testPage,
                context.CancellationTokenSource.Token);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            // Close the listener.
            _clientListener.Stop();
            while (_clientListener.IsListening)
            {
                _clientListener.Stop();
                Thread.Sleep(1000);
            }
            _clientListener.Close();

            // Stop the web host.
            _webHost.Stop().Wait();
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
            var sessionIds = new HashSet<string>();
            foreach (var pageUrl in _pageUrls)
            {
                GotoPage(browser, sessionIds, pageUrl);
            }

            // Verify
            Assert.AreEqual(
                1, 
                sessionIds.Count, 
                "All pages should return same session id");
            var sessionId = sessionIds.Single();
            Assert.IsFalse(
                String.IsNullOrWhiteSpace(sessionId),
                "Session id must not be empty string");
            Console.WriteLine($"Session Id: {sessionId}");

            browser.Dispose();
        }

        private static void GotoPage(
            Browser browser, 
            HashSet<string> sessionIds, 
            string pageUrl)
        {
            // Goto the test page.
            browser.Driver.Navigate().GoToUrl(pageUrl);

            // Wait until the session id has been populated.
            new WebDriverWait(browser.Driver,
                TimeSpan.FromSeconds(10)).Until(
                webDriver => browser.Driver.ExecuteScript(
                    "return sessionId").Equals("") == false);

            // Record the session id.
            var sessionid = browser.Driver.ExecuteScript(
                "return sessionId") as string;
            sessionIds.Add(sessionid);
        }
    }
}
