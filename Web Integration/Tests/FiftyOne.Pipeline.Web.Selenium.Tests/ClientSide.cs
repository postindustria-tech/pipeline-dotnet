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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeleniumExtras.WaitHelpers;
using System;
using System.Linq;

namespace FiftyOne.Pipeline.Web.Selenium.Tests
{
    /// <summary>
    /// Checks the client side 
    /// <see cref="Examples.ClientSideEvidence.MVC.Startup"/> example.
    /// </summary>
    [TestClass]
    public class ClientSide
    {
        /// <summary>
        /// Instance of the web host used for all tests in the class.
        /// </summary>
        public static WebHostInstance _webHost;

        /// <summary>
        /// The URLs that will be accessed when checking the alert and message.
        /// </summary>
        private static string _pageUrl;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            // Start the web host for the example using the configuration for
            // the example unaltered.
            _webHost = SeleniumTestHelper.StartLocalHost<Startup>(
                (options) => { },
                context.CancellationTokenSource.Token);

            // The page url to visit.
            _pageUrl = _webHost.ServerAddresses.Addresses.First();
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
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
            // JavaScript to get the message.
            const string messageJs = 
                "return document.getElementById(\"starSignMessage\").innerText";

            // Date of birth to input.
            const string dob = "01/01/2000";

            // Expected message for the date of birth input.
            const string expectedMessage =
                "With a date of birth of '2000-01-01T00:00:00', your star " +
                "sign is 'Capricorn'.";
            
            // Act

            // Goto the test page.
            browser.Driver.Navigate().GoToUrl(_pageUrl);

            // Wait for the alert to be displayed and store it in a variable
            var alert = browser.Wait.Until(ExpectedConditions.AlertIsPresent());

            // Enter the date of birth.
            alert.SendKeys(dob);

            // Press the OK button
            alert.Accept();

            // Wait until the processing is finished.
            browser.Wait.Until(webDriver =>
            {
                var message = browser.Driver.ExecuteScript(messageJs);
                return message.Equals("processing...") == false;
            });

            // Verify

            // Check that the message is the expected one.
            var message = browser.Driver.ExecuteScript(messageJs) as string;
            Assert.AreEqual(expectedMessage, message);

            browser.Dispose();
        }
    }
}
