/* *********************************************************************
 * This Original Work is copyright of 51 Degrees Mobile Experts Limited.
 * Copyright 2019 51 Degrees Mobile Experts Limited, 5 Charlotte Close,
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
using System.Text;
using FiftyOne.Pipeline.Core.FlowElements;
using Microsoft.AspNetCore.Http;

namespace FiftyOne.Pipeline.Web.Services
{
    public class PipelineResultService : IPipelineResultService
    {
        private IPipeline _pipeline;

        private IWebRequestEvidenceService _evidenceService;
        
        /// <summary>
        /// Construct a new instance of the PipelineResultService.
        /// </summary>
        /// <param name="evidenceService">
        /// Service used to retrieve evidence from a web request
        /// </param>
        /// <param name="pipeline">
        /// Pipeline used to process the evidence
        /// </param>
        public PipelineResultService(
            IWebRequestEvidenceService evidenceService,
            IPipeline pipeline)
        {
            _evidenceService = evidenceService;
            _pipeline = pipeline;
        }

        public void Process(HttpContext context)
        {
            // Create the flowData
            var flowData = _pipeline.CreateFlowData();
            // Extract the required pieces of evidence from the request
            _evidenceService.AddEvidenceFromRequest(flowData, context.Request);
            // Start processing the data
            flowData.Process();
            // Store the FlowData in the HttpContext
            context.Items.Add(Constants.HTTPCONTEXT_FLOWDATA, flowData);
        }
    }
}
