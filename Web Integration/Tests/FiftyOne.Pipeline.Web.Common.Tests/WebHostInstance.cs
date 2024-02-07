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
using Microsoft.AspNetCore.Hosting.Server.Features;
using System.Threading;
using System.Threading.Tasks;

namespace FiftyOne.Pipeline.Web.Common.Tests
{
    public class WebHostInstance
    {
        /// <summary>
        /// Cancellation token used to stop the web host and listener.
        /// </summary>
        public readonly CancellationTokenSource StopSource;

        /// <summary>
        /// The web host running the JavaScript and Json endpoints.
        /// </summary>
        public IWebHost Host { get; private set; }

        /// <summary>
        /// The task for the running web host.
        /// </summary>
        public Task Task { get; private set; }

        /// <summary>
        /// The addresses that the web host is listening on.
        /// </summary>
        public IServerAddressesFeature? ServerAddresses =>
            Host.ServerFeatures.Get<IServerAddressesFeature>();

        /// <summary>
        /// Constructs the instance starting the host provided.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="stopSource"></param>
        public WebHostInstance(
            IWebHost host, 
            CancellationTokenSource stopSource)
        {
            Host = host;
            StopSource = stopSource;
            Task = host.RunAsync(stopSource.Token);
        }

        /// <summary>
        /// Stops the web host and waits for it to shut down.
        /// </summary>
        public void Stop()
        {
            // Stop the web host and any other task that uses the stop
            // source.
            StopSource.Cancel();

            // Wait for the web host task to stop.
            Task.Wait();
        }
    }
}
