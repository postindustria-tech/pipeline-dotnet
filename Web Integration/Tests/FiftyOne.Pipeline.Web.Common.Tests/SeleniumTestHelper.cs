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

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.DevTools;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using Stubble.Core.Builders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace FiftyOne.Pipeline.Web.Common.Tests
{
    public static class SeleniumTestHelper
    {
        /// <summary>
        /// Creates a new Chrome driver and dev tools session.
        /// </summary>
        /// <returns></returns>
        public static Browser Chrome()
        {
            return new Browser(
                "Chrome",
                () =>
                {
                    var chromeOptions = new ChromeOptions();
                    chromeOptions.AcceptInsecureCertificates = true;
                    chromeOptions.AddArgument("--headless");
                    try
                    {
                        return new ChromeDriver(chromeOptions);
                    }
                    catch (WebDriverException)
                    {
                        Assert.Inconclusive(
                            "Could not create a ChromeDriver, check " +
                            "that the Chromium driver is installed");
                        return null;
                    }
                },
                (d) => ((ChromeDriver)d).GetDevToolsSession());
        }

        /// <summary>
        /// Creates a new Edge driver and dev tools session.
        /// </summary>
        /// <returns></returns>
        public static Browser Edge()
        {
            return new Browser(
                "Edge",
                () =>
                {
                    var edgeOptions = new EdgeOptions();
                    edgeOptions.AcceptInsecureCertificates = true;
                    edgeOptions.AddArgument("--headless");
                    try
                    {
                        return new EdgeDriver(edgeOptions);
                    }
                    catch (WebDriverException)
                    {
                        Assert.Inconclusive(
                            "Could not create a EdgeDriver, check " +
                            "that the Edge driver is installed");
                        return null;
                    }
                },
                (d) => ((EdgeDriver)d).GetDevToolsSession());
        }

        /// <summary>
        /// Creates a new Firefox driver and dev tools session.
        /// </summary>
        /// <returns></returns>
        public static Browser Firefox()
        {
            return new Browser(
                "Firefox",
                () =>
                {
                    var firefoxOptions = new FirefoxOptions();
                    firefoxOptions.AcceptInsecureCertificates = true;
                    firefoxOptions.AddArgument("--headless");
                    firefoxOptions.EnableDevToolsProtocol = true;
                    try
                    {
                        return new FirefoxDriver(firefoxOptions);
                    }
                    catch (WebDriverException)
                    {
                        Assert.Inconclusive(
                            "Could not create a FirefoxDriver, check " +
                            "that the Firefox driver is installed");
                        return null;
                    }
                },
                (d) => ((FirefoxDriver)d).GetDevToolsSession());
        }

        /// <summary>
        /// Returns an enumerable of browsers to use as DynamicData in test 
        /// methods.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<object[]> Browsers()
        {
            yield return new object[] { Firefox() };
            yield return new object[] { Chrome() };
            yield return new object[] { Edge() };
        }

        /// <summary>
        /// Returns the test display name given the arguments array.
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static string GetBrowserDisplayName(
            MethodInfo methodInfo, 
            object[] values)
        {
            var browser = (Browser)values[0];
            return $"{methodInfo.Name} {browser.Name}";
        }

        /// <summary>
        /// Returns the template in the same location as the T type as a 
        /// string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetTemplate<T>(string name)
        {
            using (var stream = typeof(T).Assembly.GetManifestResourceStream(
                typeof(T).Namespace + "." + name))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        /// <summary>
        /// Parses the template provided with the view data.
        /// </summary>
        /// <param name="template"></param>
        /// <param name="view"></param>
        /// <returns></returns>
        public static string ParseTemplate(
            string template, 
            IReadOnlyDictionary<string, string> view)
        {
            var stubble = new StubbleBuilder().Build();
            return stubble.Render(template, view);
        }

        /// <summary>
        /// Fetches the template from the name provided and then parses it with
        /// the view data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="view"></param>
        /// <returns></returns>
        public static string ParseTemplate<T>(
            string name, 
            IReadOnlyDictionary<string, string> view)
        {
            return ParseTemplate(GetTemplate<T>(name), view);
        }

        /// <summary>
        /// Initializes the web host and starts it.
        /// </summary>
        /// <typeparam name="T">
        /// Usually the startup class for the example used for the web host.
        /// </typeparam>
        /// <param name="configurationBuilder">
        /// Action used to set the configuration for the web host. Actions 
        /// might include adding PipelineOptions, or changing options ahead of 
        /// testing.
        /// </param>
        /// <param name="stopSource">
        /// Used when the web host is stopped and started to ensure it stops if
        /// the test is canceled.
        /// </param>
        public static WebHostInstance StartLocalHost<T>(
            Action<IConfigurationBuilder> configurationBuilder,
            CancellationToken stopToken) where T : class
        {
            return new WebHostInstance(BuildLocalHost<T>(
                new string[] { },
                configurationBuilder),
                stopToken);
        }

        /// <summary>
        /// Gets the request url from the enumerable of events. Only one URL 
        /// should be found as the events must relate to a single request.
        /// </summary>
        /// <param name="events"></param>
        /// <returns></returns>
        public static Uri GetRequestUrl(
            this IEnumerable<DevToolsEventReceivedEventArgs> events)
        {
            var value = events.Where(i =>
                i.EventData.SelectToken("request.url") != null).Select(i =>
                i.EventData.SelectToken("request.url")).Distinct().Single();
            if (Uri.TryCreate(
                value.Value<string>(), 
                UriKind.Absolute, 
                out var result))
            {
                return result;
            }
            return null;
        }

        /// <summary>
        /// Builds a web host ready to be run in a test with the configuration
        /// action used to modify the default configuration for the purposes
        /// of the test. A random free port is assigned to the web host.
        /// </summary>
        /// <typeparam name="T">
        /// Usually the startup class for the example used for the web host.
        /// </typeparam>
        /// <param name="args">
        /// Command line arguments to pass to the web host.
        /// </param>
        /// <param name="configurationBuilder">
        /// Action used to set the configuration for the web host. Actions 
        /// might include adding PipelineOptions, or changing options ahead of 
        /// testing.
        /// </param>
        /// <returns></returns>
        private static IWebHost BuildLocalHost<T>(
            string[] args,
            Action<IConfigurationBuilder> configurationBuilder) where T : class
        {
            // Use a random free port for the HTTP endpoint.
            var httpEndpoint = new Dictionary<string, string>
            {
                {
                    "Kestrel:Endpoints:Http:Url",
                    $"http://localhost:{TestHelpers.GetRandomUnusedPort()}"
                }
            };

            return Microsoft.AspNetCore.WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(configurationBuilder)
                .ConfigureAppConfiguration((options) =>
                    // Set the HTTP endpoint
                    options.AddInMemoryCollection(httpEndpoint))
                .UseStartup<T>()
                .Build();
        }
    }
}
