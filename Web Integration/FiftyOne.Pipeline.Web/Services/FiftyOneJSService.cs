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

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace FiftyOne.Pipeline.Web.Services
{
    /// <summary>
    /// Service that provides the 51Degrees javascript when requested
    /// </summary>
    public class FiftyOneJSService : IFiftyOneJSService
    {
        protected IClientsidePropertyService _clientsidePropertyService;
        protected IOptions<PipelineWebIntegrationOptions> _options;

        public FiftyOneJSService(
            IClientsidePropertyService clientsidePropertyService,
            IOptions<PipelineWebIntegrationOptions> options)
        {
            _clientsidePropertyService = clientsidePropertyService;
            _options = options;
        }

        /// <summary>
        /// Check if the 51Degrees JavaScript is being requested and
        /// write it to the response if it is
        /// </summary>
        /// <param name="context">
        /// The HttpContext
        /// </param>
        /// <returns>
        /// True if JavaScript was written to the response, false otherwise.
        /// </returns>
        public bool ServeJS(HttpContext context)
        {
            bool result = false;
            if (context.Request.Path.Value.EndsWith("51Degrees.core.js")) // todo bug
            {
                ServeCoreJS(context);
                result = true;
            }
            return result;
        }

        private void ServeCoreJS(HttpContext context)
        {
            if (_options.Value.ClientSideEvidenceEnabled)
            {
                _clientsidePropertyService.ServeJavascript(context);
            }
        }
    }
}
