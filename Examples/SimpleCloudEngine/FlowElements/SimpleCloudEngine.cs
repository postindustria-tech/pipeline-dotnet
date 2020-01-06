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

using FiftyOne.Pipeline.Core.Data;
using FiftyOne.Pipeline.Core.FlowElements;
using FiftyOne.Pipeline.Engines.Data;
using SimpleCloudEngine.Data;
using FiftyOne.Pipeline.Engines.FlowElements;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FiftyOne.Pipeline.CloudRequestEngine.FlowElements;
using System.Net.Http;
using FiftyOne.Pipeline.Core.Exceptions;
using Newtonsoft.Json;
using FiftyOne.Pipeline.CloudRequestEngine.Data;

namespace SimpleCloudEngine.FlowElements
{
    //! [class]
    //! [constructor]
    public class SimpleCloudEngine : CloudAspectEngineBase<IStarSignData, IAspectPropertyMetaData>
    {
        private IList<IAspectPropertyMetaData> _aspectProperties;
        private string _dataSourceTier;

        public SimpleCloudEngine(
            ILogger<SimpleCloudEngine> logger,
            Func<IFlowData, FlowElementBase<IStarSignData, IAspectPropertyMetaData>, IStarSignData> deviceDataFactory,
            CloudRequestEngine engine)
            : base(logger, deviceDataFactory)
        {
            if (engine == null)
            {
                // There is no cloud request engine in the pipeline. This means
                // there is no result available.
                throw new PipelineConfigurationException(
                    $"The '{GetType().Name}' requires a 'CloudRequestEngine' " +
                    $"before it in the Pipeline. This engine will be unable " +
                    $"to produce results until this is corrected.");
            }

            // Get the properties from the cloud service.
            if (LoadAspectProperties(engine) == false)
            {
                _logger.LogCritical("Failed to load aspect properties");
            }
        }
        //! [constructor]

        public override IList<IAspectPropertyMetaData> Properties => _aspectProperties;

        public override string DataSourceTier => _dataSourceTier;

        public override string ElementDataKey => "starsign";

        public override IEvidenceKeyFilter EvidenceKeyFilter =>
            // This engine needs no evidence. 
            // It works from the cloud request data.
            new EvidenceKeyFilterWhitelist(new List<string>());

        protected override void ProcessEngine(IFlowData data, IStarSignData aspectData)
        {
            // Cast aspectData to StarSignData so the 'setter' is available.
            StarSignData starSignData = (StarSignData)aspectData;
            
            // Get the JSON response which the cloud request engine has
            // fetched from the cloud service.
            var requestData = data.Get<CloudRequestData>();
            var json = requestData.JsonResponse;

            // Extract data from json to the aspectData instance.
            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            // Get the results for the star sign component.
            var starSign = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            JsonConvert.PopulateObject(dictionary["starsign"].ToString(), starSign);
            // Now get the values from the star sign results.
            starSignData.StarSign = starSign["starsign"].ToString();
        }

        protected override void UnmanagedResourcesCleanup()
        {
            // Nothing to clean up here.
        }

        //! [loadaspectproperties]
        private bool LoadAspectProperties(CloudRequestEngine engine)
        {
            if (engine.PublicProperties != null &&
                engine.PublicProperties.Count > 0 &&
                engine.PublicProperties.ContainsKey(ElementDataKey))
            {
                // Create the properties list and set the data tier from the
                // cloud service.
                _aspectProperties = new List<IAspectPropertyMetaData>();
                _dataSourceTier = engine.PublicProperties[ElementDataKey].DataTier;

                // For each of the properties returned by the cloud service,
                // add it to the list.
                foreach (var item in engine.PublicProperties[ElementDataKey].Properties)
                {
                    _aspectProperties.Add(new AspectPropertyMetaData(
                        this,
                        item.Name,
                        item.GetPropertyType(),
                        item.Category,
                        new List<string>(),
                        true));
                }
                return true;
            }
            else
            {
                _logger.LogError($"Aspect properties could not be loaded for" +
                    $" the cloud engine", this);
                return false;
            }
        }
        //! [loadaspectproperties]
    }
    //! [class]
}