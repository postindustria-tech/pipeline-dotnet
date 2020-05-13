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
using System.Threading.Tasks;
using FiftyOne.Pipeline.Web.Services;
using FiftyOne.Pipeline.Core.Data;
using FiftyOne.Pipeline.Core.FlowElements;

namespace FiftyOne.Pipeline.Web
{
    /// <summary>
    /// The 51Degrees middleware component.
    /// This carries out the processing on the device making the request using
    /// the Pipeline which has been constructed, and intercepts requests for
    /// the 51Degrees JavaScript.
    /// </summary>
    public class FiftyOneMiddleware
    {
        /// <summary>
        /// The next component in the MVC pipeline
        /// </summary>
        protected RequestDelegate Next { get; private set; }
        /// <summary>
        /// The JavaScript service handles requests for dynamic JavaScript
        /// resources such as 51degrees.core.js
        /// </summary>
        protected IFiftyOneJSService JsService { get; private set; }
        /// <summary>
        /// The PipelineResultService passes the current request 
        /// to the <see cref="IPipeline"/> and makes the results 
        /// accessible through the <see cref="HttpContext"/>.
        /// </summary>
        protected IPipelineResultService PipelineResultService { get; private set; }

        /// <summary>
        /// Create a new FiftyOneMiddleware object.
        /// </summary>
        /// <param name="next">
        /// The next component in the Pipeline
        /// </param>
        /// <param name="pipelineResultService">
        /// A service that will determine the device making the request
        /// and store details of the device against the HttpContext for 
        /// use further down the Pipeline
        /// </param>
        /// <param name="jsService">
        /// A service that can serve the 51Degrees JavaScript if needed
        /// </param>
        public FiftyOneMiddleware(RequestDelegate next,
            IPipelineResultService pipelineResultService, 
            IFiftyOneJSService jsService)
        {
            Next = next;
            PipelineResultService = pipelineResultService;
            JsService = jsService;
        }

        /// <summary>
        /// Called as part of the MVC pipeline.
        /// The component serves JavaScript if needed.
        /// If not, it populates an <see cref="IFlowData"/> object from
        /// the <see cref="HttpContext"/> and processes it using the 
        /// <see cref="IPipeline"/>. The <see cref="IFlowData"/> instance
        /// is added to the HttpContext so it can easily be accessed by other
        /// code.
        /// </summary>
        /// <param name="context">
        /// The <see cref="HttpContext"/> 
        /// </param>
        /// <returns>
        /// The <see cref="Task"/> to be run 
        /// </returns>
        public async Task Invoke(HttpContext context)
        {
            // Populate the request properties and store against the 
            // HttpContext.
            PipelineResultService.Process(context);
            
            // If 51Degrees JavaScript is being requested then serve it.
            // Otherwise continue down the middleware Pipeline.
            if (JsService.ServeJS(context) == false)
            {
                await Next.Invoke(context).ConfigureAwait(false);
            }
        }
    }
}
