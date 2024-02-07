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

using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.DevTools;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FiftyOne.Pipeline.Web.Common.Tests
{
    /// <summary>
    /// Common return class for the different browsers.
    /// </summary>
    public class Browser
    {
        /// <summary>
        /// Common name of the browser to display in tests.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Web driver
        /// </summary>
        public WebDriver Driver { get; }

        /// <summary>
        /// Developer tools session
        /// </summary>
        public DevToolsSession Session { get; }

        /// <summary>
        /// Waiter for the browser driver set to 10 second timeout.
        /// </summary>
        public WebDriverWait Wait
        {
            get
            {
                if (_wait == null)
                {
                    _wait = new WebDriverWait(
                        Driver,
                        TimeSpan.FromSeconds(10));
                }
                return _wait;
            }
        }
        private WebDriverWait _wait;

        /// <summary>
        /// Enumeration of networks events received since call to 
        /// <see cref="EnableNetwork"/>.
        /// </summary>
        public IEnumerable<DevToolsEventReceivedEventArgs> NetworkEvents =>
            _networkEvents;
        private Queue<DevToolsEventReceivedEventArgs> _networkEvents;

        /// <summary>
        /// The network events grouped by the request.id field. Used to 
        /// relate events to the same request / response activity.
        /// </summary>
        public IEnumerable<IGrouping<string, DevToolsEventReceivedEventArgs>>
            GroupedNetworkEvents => NetworkEvents.GroupBy(i =>
                {
                    var id = i.EventData["request.id"];
                    return id == null ? null : id.Value<string>();
                });

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">
        /// Common name of the browser to display in tests
        /// </param>
        /// <param name="driver">
        /// Driver
        /// </param>
        /// <param name="session">
        /// Developer tools session
        /// </param>
        public Browser(
            string name,
            WebDriver driver,
            DevToolsSession session)
        {
            Name = name;
            Driver = driver;
            Session = session;
        }

        /// <summary>
        /// Enable network events recording.
        /// </summary>
        public void EnableNetwork()
        {
            _networkEvents = new Queue<DevToolsEventReceivedEventArgs>();
            Session.Domains.Network.EnableNetwork();
            Session.DevToolsEventReceived += Session_DevToolsEventReceived;
        }

        /// <summary>
        /// Disable network events recording.
        /// </summary>
        public void DisableNetwork()
        {
            Session.Domains.Network.DisableNetwork();
            Session.DevToolsEventReceived -= Session_DevToolsEventReceived;
            _networkEvents = null;
        }

        /// <summary>
        /// Remove any existing events from the <see cref="NetworkEvents"/>
        /// </summary>
        public void ClearNetwork()
        {
            _networkEvents.Clear();
        }

        private void Session_DevToolsEventReceived(
            object sender,
            DevToolsEventReceivedEventArgs e)
        {
            _networkEvents.Enqueue(e);
        }
    }
}
