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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;

namespace FiftyOne.Pipeline.Web.Framework.Providers
{
    /// <summary>
    /// Extends the default HttpCapabilitiesProvider to return a new instance
    /// of the 51Degrees HttpBrowserCapabilities implementation when requested.
    public class CapabilitiesProvider : HttpCapabilitiesDefaultProvider
    {
        public CapabilitiesProvider()
        {
        }

        public CapabilitiesProvider(HttpCapabilitiesDefaultProvider parent) : base(parent)
        {
        }

        public override HttpBrowserCapabilities GetBrowserCapabilities(HttpRequest request)
        {
            HttpBrowserCapabilities caps;
            var baseCaps = base.GetBrowserCapabilities(request);
            
            var flowData = WebPipeline.Process(request);
            if (flowData != null)
            {
                // A provider is present so 51Degrees can be used to override
                // some of the returned values.
                caps = new PipelineCapabilities(
                    baseCaps,
                    request,
                    flowData);

                // Copy the adapters from the original.
                var adapters = baseCaps.Adapters.GetEnumerator();
                while (adapters.MoveNext())
                {
                    caps.Adapters.Add(adapters.Key, adapters.Value);
                }

                // Copy the browsers from the original to prevent the Browsers
                // property returning null.
                if (baseCaps.Browsers != null)
                {
                    foreach (string browser in baseCaps.Browsers)
                    {
                        caps.AddBrowser(browser);
                    }
                }
            }
            else
            {
                // No 51Degrees flow data is present so we have to use
                // the base capabilities only.
                caps = baseCaps;
            }
            return caps;
        }
    }
}
