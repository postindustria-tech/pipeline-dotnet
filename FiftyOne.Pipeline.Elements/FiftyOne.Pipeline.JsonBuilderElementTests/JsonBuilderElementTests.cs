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

using FiftyOne.Pipeline.JsonBuilder;
using FiftyOne.Pipeline.JsonBuilder.FlowElement;
using FiftyOne.Pipeline.Core.Data;
using FiftyOne.Pipeline.Core.Data.Types;
using FiftyOne.Pipeline.Core.FlowElements;
using FiftyOne.Pipeline.Core.TypedMap;
using FiftyOne.Pipeline.Engines.Data;
using FiftyOne.Pipeline.Engines.FlowElements;
using FiftyOne.Pipeline.Engines.Services;
using FiftyOne.Pipeline.JsonBuilder.Data;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FiftyOne.Pipeline.Engines.TestHelpers;
using FiftyOne.Pipeline.Engines.FiftyOne.FlowElements;
using Newtonsoft.Json;
using System.Diagnostics;

namespace FiftyOne.Pipeline.JsonBuilderElementTests
{
    [TestClass]
    public class JsonBuilderElementTests
    {
        private IJsonBuilderElement _jsonBuilderElement;
        private Mock<IElementData> _elementDataMock;
        private ILoggerFactory _loggerFactory;
        
        private Mock<IAspectEngine> _testEngine;

        [TestInitialize]
        public void Init()
        {
            _testEngine = new Mock<IAspectEngine>();
            _testEngine.Setup(e => e.Properties).Returns(new List<IAspectPropertyMetaData>()
            {
                new AspectPropertyMetaData(_testEngine.Object, "property", typeof(string), "", new List<string>(), true),
                new AspectPropertyMetaData(_testEngine.Object, "jsproperty", typeof(JavaScript), "", new List<string>(), true)
            });
            _testEngine.Setup(e => e.ElementDataKey).Returns("test");

            _loggerFactory = new LoggerFactory();

            _jsonBuilderElement = new JsonBuilderElementBuilder(_loggerFactory)
                .Build();

            _elementDataMock = new Mock<IElementData>();
            _elementDataMock.Setup(ed => ed.AsDictionary()).
                Returns(new Dictionary<string, object>() {
                    { "property", "thisIsAValue" },
                    { "jsproperty", "var = 'some js code';" }
                });
        }

        /// <summary>
        /// Check that the JSON produced by the JsonBuilder is valid.
        /// </summary>
        [TestMethod]
        public void JsonBuilder_ValidJson()
        {
            var flowData = new Mock<IFlowData>();
            IJsonBuilderElementData result = null;
            var _missingPropertyService = new Mock<IMissingPropertyService>();
            flowData.Setup(d => d.ElementDataAsDictionary()).Returns(new Dictionary<string, object>() { { "test", _elementDataMock.Object } });
            flowData.Setup(d => d.GetOrAdd(
                It.IsAny<TypedKey<IJsonBuilderElementData>>(),
                It.IsAny<Func<IPipeline, IJsonBuilderElementData>>()))
                .Returns<TypedKey<IJsonBuilderElementData>, Func<IPipeline, IJsonBuilderElementData>>(
                (k, f) =>
                {
                    result = f(flowData.Object.Pipeline);
                    return result;
                });

            string session = "somesessionid";
            flowData.Setup(d => d.TryGetEvidence("query.session-id", out session)).Returns(true);
            int iteration = 1;
            flowData.Setup(d => d.TryGetEvidence("query.sequence", out iteration)).Returns(true);

            _jsonBuilderElement.Process(flowData.Object);

            Assert.IsTrue(IsExpectedJson(result.Json));
        }

        /// <summary>
        /// Check that the JSON element removes JavaScript properties from the 
        /// response after max number of iterations has been reached.
        /// </summary>
        [TestMethod]
        public void JsonBuilder_MaxIterations()
        {
            for (var i = 0; true; i++)
            {
                var json = TestIteration(i);
                var result = TestJsonIterations(json);

                if (i >= Pipeline.JsonBuilder.Constants.MAX_JAVASCRIPT_ITERATIONS)
                {
                    Assert.IsTrue(result);
                    break;
                }
                else
                {
                    Assert.IsFalse(result);
                }
            }

        }

        /// <summary>
        /// Check that the JSON produced by the JsonBuilder is correct 
        /// when lazy loading is enabled.
        /// </summary>
        [TestMethod]
        public void JsonBuilder_LazyLoading()
        {
            var engine = new EmptyEngineBuilder(_loggerFactory)
                .SetLazyLoadingTimeout(1000)
                .SetProcessCost(TimeSpan.FromMilliseconds(500).Ticks)
                .Build();
            var jsonBuilder = new JsonBuilderElementBuilder(_loggerFactory)
                .Build();
            var sequenceElement = new SequenceElementBuilder(_loggerFactory)
                .Build();
            var pipeline = new PipelineBuilder(_loggerFactory)
                .AddFlowElement(sequenceElement)
                .AddFlowElement(engine)
                .AddFlowElement(jsonBuilder)
                .Build();

            var flowData = pipeline.CreateFlowData();
            Trace.WriteLine("Process starting");
            flowData.Process();
            Trace.WriteLine("Process complete");

            var jsonResult = flowData.Get<IJsonBuilderElementData>();
            Assert.IsNotNull(jsonResult);
            Assert.IsNotNull(jsonResult.Json);

            var jsonData = JsonConvert.DeserializeObject<JsonData>(jsonResult.Json);
            Assert.AreEqual(1, jsonData.EmptyAspect.Valueone);
            Assert.AreEqual(2, jsonData.EmptyAspect.Valuetwo);
            Trace.WriteLine("Data validated");
        }

        /// <summary>
        /// Check that entries will not appear in the output 
        /// for blacklisted elements.
        /// </summary>
        [TestMethod]
        public void JsonBuilder_ElementBlacklist()
        {
            var flowData = new Mock<IFlowData>();
            IJsonBuilderElementData result = null;
            var _missingPropertyService = new Mock<IMissingPropertyService>();
            flowData.Setup(d => d.ElementDataAsDictionary()).Returns(
                new Dictionary<string, object>() {
                    { "test", _elementDataMock.Object },
                    { "cloud-response", _elementDataMock.Object },
                    { "json-builder", _elementDataMock.Object }
                });
            flowData.Setup(d => d.GetOrAdd(
                It.IsAny<TypedKey<IJsonBuilderElementData>>(),
                It.IsAny<Func<IPipeline, IJsonBuilderElementData>>()))
                .Returns<TypedKey<IJsonBuilderElementData>, Func<IPipeline, IJsonBuilderElementData>>(
                (k, f) =>
                {
                    result = f(flowData.Object.Pipeline);
                    return result;
                });

            string session = "somesessionid";
            flowData.Setup(d => d.TryGetEvidence("query.session-id", out session)).Returns(true);
            int iteration = 1;
            flowData.Setup(d => d.TryGetEvidence("query.sequence", out iteration)).Returns(true);

            _jsonBuilderElement.Process(flowData.Object);

            Assert.IsTrue(IsExpectedJson(result.Json));
            JObject obj = JObject.Parse(result.Json);
            Assert.AreEqual(1, obj.Children().Count(), 
                $"There should only be the 'test' key at the top level as " +
                $"the other elements should have been ignored. Complete JSON: " +
                Environment.NewLine + result.Json);
        }

        public class JsonData
        {
            [JsonProperty("empty-aspect")]
            public EmptyAspect EmptyAspect { get; set; }

            [JsonProperty("json-builder")]
            public JsonBuilder JsonBuilder { get; set; }
        }

        public class EmptyAspect
        {
            [JsonProperty("valueone")]
            public long Valueone { get; set; }

            [JsonProperty("valuetwo")]
            public long Valuetwo { get; set; }
        }

        public class JsonBuilder
        {
        }

        private bool IsExpectedJson(string json)
        {
            JObject obj = JObject.Parse(json);
            var results = obj["test"].Children().ToList();

            foreach (var result in results)
            {
                var res = result.ToString();
                if (res.Contains("property") && res.Contains("thisIsAValue"))
                {
                    return true;
                }
            }
            return false;
        }

        private string TestIteration(int iteration)
        {
            var flowData = new Mock<IFlowData>();
            var _missingPropertyService = new Mock<IMissingPropertyService>();

            flowData.Setup(d => d.ElementDataAsDictionary()).Returns(new Dictionary<string, object>() { { "test", _elementDataMock.Object } });
            string session = "somesessionid";
            flowData.Setup(d => d.TryGetEvidence("query.session-id", out session)).Returns(true);
            flowData.Setup(d => d.TryGetEvidence("query.sequence", out iteration)).Returns(true);
            flowData.Setup(d => d.GetWhere(It.IsAny<Func<IElementPropertyMetaData, bool>>())).Returns(new List<KeyValuePair<string, object>>() { new KeyValuePair<string, object>("test.jsproperty", new AspectPropertyValue<JavaScript>(new JavaScript("var = 'some js code';"))) }.AsEnumerable());

            IJsonBuilderElementData result = null;
            flowData.Setup(d => d.GetOrAdd(
                It.IsAny<TypedKey<IJsonBuilderElementData>>(),
                It.IsAny<Func<IPipeline, IJsonBuilderElementData>>()))
                .Returns<TypedKey<IJsonBuilderElementData>, Func<IPipeline, IJsonBuilderElementData>>(
                (k, f) =>
                {
                    result = f(flowData.Object.Pipeline);
                    return result;
                });

            _jsonBuilderElement.Process(flowData.Object);

            var json = result["json"].ToString();

            return json;
        }

        private bool TestJsonIterations(string json)
        {
            JObject obj = JObject.Parse(json);
            var results = obj.Children().ToList();

            foreach (var result in results)
            {
                var res = result.ToString();
                if (res.Contains("javascriptProperties"))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
