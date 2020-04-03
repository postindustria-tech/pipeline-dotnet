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

using FiftyOne.Pipeline.CloudRequestEngine.Data;
using FiftyOne.Pipeline.CloudRequestEngine.FlowElements;
using FiftyOne.Pipeline.Core.Data;
using FiftyOne.Pipeline.Core.FlowElements;
using FiftyOne.Pipeline.Core.TypedMap;
using FiftyOne.Pipeline.Engines.Data;
using FiftyOne.Pipeline.Engines.FlowElements;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FiftyOne.Pipeline.CloudRequestEngine.Tests
{
    [TestClass]
    public class CloudAspectEngineBaseTests
    {
        private class TestData : AspectDataBase
        {
            public TestData(ILogger<AspectDataBase> logger,
                IPipeline pipeline,
                IAspectEngine engine) :
                base(logger, pipeline, engine)
            {
            }
        }
        private class TestInstance : CloudAspectEngineBase<TestData>
        {
            public TestInstance() :
                base(new Logger<TestInstance>(new LoggerFactory()), CreateData)
            {
            }

            private static TestData CreateData(IPipeline pipeline,
                FlowElementBase<TestData, IAspectPropertyMetaData> element)
            {
                return new TestData(null,
                    pipeline, element as IAspectEngine);
            }

            public override string ElementDataKey => "test";

            public override IEvidenceKeyFilter EvidenceKeyFilter =>
                new EvidenceKeyFilterWhitelist(new List<string>());

            protected override void ProcessEngine(IFlowData data, TestData aspectData)
            {
            }
        }
        private class TestRequestEngine : AspectEngineBase<CloudRequestData, IAspectPropertyMetaData>, ICloudRequestEngine
        {
            public TestRequestEngine() : base(new Logger<TestRequestEngine>(new LoggerFactory()), CreateData)
            {
            }

            private static CloudRequestData CreateData(IPipeline pipeline,
                FlowElementBase<CloudRequestData, IAspectPropertyMetaData> element)
            {
                return new CloudRequestData(null,
                    pipeline, element as IAspectEngine);
            }

            public IReadOnlyDictionary<string, ProductMetaData> PublicProperties { get; set; }

            public override string DataSourceTier => "";

            public override string ElementDataKey => "cloud";

            public override IEvidenceKeyFilter EvidenceKeyFilter => new EvidenceKeyFilterWhitelist(new List<string>());

            public override IList<IAspectPropertyMetaData> Properties => new List<IAspectPropertyMetaData>();

            protected override void ProcessEngine(IFlowData data, CloudRequestData aspectData)
            {
            }

            protected override void UnmanagedResourcesCleanup()
            {
            }
        }

        private TestInstance _engine;
        private TestRequestEngine _requestEngine;
        private IPipeline _pipeline;

        private Dictionary<string, ProductMetaData> _propertiesReturnedByRequestEngine;

        [TestInitialize]
        public void Init()
        {
            _propertiesReturnedByRequestEngine = new Dictionary<string, ProductMetaData>();

        }

        [TestMethod]
        public void LoadProperties()
        {
            List<PropertyMetaData> properties = new List<PropertyMetaData>();
            properties.Add(new PropertyMetaData() { Name = "ismobile", Type = "Boolean" });
            properties.Add(new PropertyMetaData() { Name = "hardwarevendor", Type = "String" });
            properties.Add(new PropertyMetaData() { Name = "hardwarevariants", Type = "Array" });
            ProductMetaData devicePropertyData = new ProductMetaData();
            devicePropertyData.Properties = properties;
            _propertiesReturnedByRequestEngine.Add("test", devicePropertyData);

            CreatePipeline();

            Assert.AreEqual(3, _engine.Properties.Count);
            Assert.IsTrue(_engine.Properties.Any(p => p.Name == "ismobile"));
            Assert.IsTrue(_engine.Properties.Any(p => p.Name == "hardwarevendor"));
            Assert.IsTrue(_engine.Properties.Any(p => p.Name == "hardwarevariants"));
        }

        [TestMethod]
        public void LoadProperties_SubProperties()
        {
            List<PropertyMetaData> subproperties = new List<PropertyMetaData>();
            subproperties.Add(new PropertyMetaData() { Name = "ismobile", Type = "Boolean" });
            subproperties.Add(new PropertyMetaData() { Name = "hardwarevendor", Type = "String" });
            subproperties.Add(new PropertyMetaData() { Name = "hardwarevariants", Type = "Array" });

            List<PropertyMetaData> properties = new List<PropertyMetaData>();
            properties.Add(new PropertyMetaData() { Name = "devices", Type = "Array", ItemProperties = subproperties });

            ProductMetaData devicePropertyData = new ProductMetaData();
            devicePropertyData.Properties = properties;
            _propertiesReturnedByRequestEngine.Add("test", devicePropertyData);

            CreatePipeline();

            Assert.AreEqual(1, _engine.Properties.Count);
            Assert.AreEqual("devices", _engine.Properties[0].Name);
            Assert.AreEqual(3, _engine.Properties[0].ItemProperties.Count);
            Assert.IsTrue(_engine.Properties[0].ItemProperties.Any(p => p.Name == "ismobile"));
            Assert.IsTrue(_engine.Properties[0].ItemProperties.Any(p => p.Name == "hardwarevendor"));
            Assert.IsTrue(_engine.Properties[0].ItemProperties.Any(p => p.Name == "hardwarevariants"));
        }


        private void CreatePipeline()
        {
            _engine = new TestInstance();
            _requestEngine = new TestRequestEngine();
            _requestEngine.PublicProperties = _propertiesReturnedByRequestEngine;
            _pipeline = new PipelineBuilder(new LoggerFactory())
                .AddFlowElement(_requestEngine)
                .AddFlowElement(_engine)
                .Build();
        }

    }
}
