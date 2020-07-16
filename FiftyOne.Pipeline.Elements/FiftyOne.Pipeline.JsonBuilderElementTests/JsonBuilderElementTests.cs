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
        private Mock<IPipeline> _pipeline;

        private Mock<IAspectEngine> _testEngine;

        [TestInitialize]
        public void Init()
        {
            _testEngine = new Mock<IAspectEngine>();
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

            _pipeline = new Mock<IPipeline>();
            _pipeline.Setup(p => p.GetHashCode()).Returns(1);
            var propertyMetaData = new Dictionary<string, IReadOnlyDictionary<string, IElementPropertyMetaData>>();
            _pipeline.Setup(p => p.ElementAvailableProperties).Returns(propertyMetaData);
        }

        /// <summary>
        /// Check that the JSON produced by the JsonBuilder is valid.
        /// </summary>
        [TestMethod]
        public void JsonBuilder_ValidJson()
        {
            var json = TestIteration(1);

            Assert.IsTrue(IsExpectedJson(json));
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
                var jsProperties = new Dictionary<string, object>() 
                {
                    { "test.jsproperty", new AspectPropertyValue<JavaScript>(new JavaScript("var = 'some js code';")) }
                };
                var json = TestIteration(i, null, jsProperties);
                var result = ContainsJavaScriptProperties(json);

                if (i >= Constants.MAX_JAVASCRIPT_ITERATIONS)
                {
                    Assert.IsFalse(result);
                    break;
                }
                else
                {
                    Assert.IsTrue(result);
                }
            }
        }

        /// <summary>
        /// Check that entries will not appear in the output 
        /// for blacklisted elements.
        /// </summary>
        [TestMethod]
        public void JsonBuilder_ElementBlacklist()
        {
            var json = TestIteration(1,
               new Dictionary<string, object>() {
                    { "test", _elementDataMock.Object },
                    { "cloud-response", _elementDataMock.Object },
                    { "json-builder", _elementDataMock.Object }
               });

            Assert.IsTrue(IsExpectedJson(json));
            JObject obj = JObject.Parse(json);
            Assert.AreEqual(1, obj.Children().Count(),
                $"There should only be the 'test' key at the top level as " +
                $"the other elements should have been ignored. Complete JSON: " +
                Environment.NewLine + json);
        }

        /// <summary>
        /// Data class used in the nested properties test.
        /// </summary>
        private class NestedData : ElementDataBase
        {
            public NestedData(ILogger<ElementDataBase> logger, IPipeline pipeline)
                : base(logger, pipeline)
            { }

            public string Value1
            {
                get { return this["Value1"] as string; }
                set { this["Value1"] = value; }
            }
            public int Value2
            {
                get { return (int)this["Value2"]; }
                set { this["Value2"] = value; }
            }
        }

        /// <summary>
        /// Check that nested properties are serialised as expected
        /// </summary>
        [TestMethod]
        public void JsonBuilder_NestedProperties()
        {
            _elementDataMock.Setup(ed => ed.AsDictionary()).
                Returns(new Dictionary<string, object>() {
                    { "property", new List<NestedData>() {
                        new NestedData(_loggerFactory.CreateLogger<NestedData>(), _pipeline.Object) { Value1 = "abc", Value2 = 123 },
                        new NestedData(_loggerFactory.CreateLogger<NestedData>(), _pipeline.Object) { Value1 = "xyz", Value2 = 789 }
                    } },
                });

            // Configure the property meta-data as needed for
            // this test.
            var propertyMetaData = new Dictionary<string, IReadOnlyDictionary<string, IElementPropertyMetaData>>();
            var testElementMetaData = new Dictionary<string, IElementPropertyMetaData>();
            var nestedMetaData = new List<IElementPropertyMetaData>() {
                new ElementPropertyMetaData(_testEngine.Object, "value1", typeof(string), true),
                new ElementPropertyMetaData(_testEngine.Object, "value2", typeof(int), true),
            };
            var p1 = new ElementPropertyMetaData(_testEngine.Object, "property", typeof(List<NestedData>), true, "", nestedMetaData);
            testElementMetaData.Add("property", p1);
            propertyMetaData.Add("test", testElementMetaData);
            _pipeline.Setup(p => p.ElementAvailableProperties).Returns(propertyMetaData);

            var json = TestIteration(1);

            JObject obj = JObject.Parse(json);
            Assert.IsTrue(obj.Children().Count() == 1 &&
                (obj.Children().Single() as JProperty)?.Name == "test",
                $"There should only be the 'test' key at the top level. " +
                $"Complete JSON: " + Environment.NewLine + json);
            var element = obj.Children().Single();
            var x = element.Children().Single().Children().Single();
            Assert.IsTrue(element.Children().Children().Single().Count() == 1 &&
                (element.Children().Single().Children().Single() as JProperty)?.Name == "property",
                $"There should only be one property, named 'property'," +
                $" under the 'test' element. " +
                $"Complete JSON: " + Environment.NewLine + json);
            element = element.Children().Single().Children().Single();
            Assert.AreEqual(2, element.Children().Single().Children().Count(),
                $"There should be two entries under the 'test.property' property. " +
                $"Complete JSON: " + Environment.NewLine + json);
            var v1 = element.Children().Single().Children().ElementAt(0);
            var v2 = element.Children().Single().Children().ElementAt(1);
            Assert.IsTrue(v1.Children().Count() == 2 &&
                v1.Children().Any(t => (t as JProperty)?.Name == "value1") &&
                v1.Children().Any(t => (t as JProperty)?.Name == "value2"),
                $"There should be two properties, 'value1' and 'value2', " +
                $"under the 'test.property[0]' entry. " +
                $"Complete JSON: " + Environment.NewLine + json);
            Assert.IsTrue(v2.Children().Count() == 2 &&
                v2.Children().Any(t => (t as JProperty)?.Name == "value1") &&
                v2.Children().Any(t => (t as JProperty)?.Name == "value2"),
                $"There should be two properties, 'value1' and 'value2', " +
                $"under the 'test.property[0]' entry. " +
                $"Complete JSON: " + Environment.NewLine + json);
        }

        /// <summary>
        /// Check that delayed execution and evidence properties values
        /// are populated correctly.
        /// </summary>
        [DataTestMethod]
        [DataRow(true, true)]
        [DataRow(false, true)]
        [DataRow(true, false)]
        [DataRow(false, false)]
        public void JsonBuilder_DelayedExecution(
            bool delayExecution, 
            bool propertyValueNull)
        {
            // If the flag is set then initialise the 'property' value to null.
            if (propertyValueNull)
            {
                _elementDataMock.Setup(ed => ed.AsDictionary()).
                    Returns(new Dictionary<string, object>() {
                    { "property", null },
                    { "jsproperty", "var = 'some js code';" }
                    });
            }

            // Configure the property meta-data as needed for
            // this test.
            var propertyMetaData = new Dictionary<string, IReadOnlyDictionary<string, IElementPropertyMetaData>>();
            var testElementMetaData = new Dictionary<string, IElementPropertyMetaData>();
            var p1 = new ElementPropertyMetaData(_testEngine.Object, "property", typeof(string), true, "", null, false, new List<string>() { "jsproperty" });
            testElementMetaData.Add("property", p1);
            var p2 = new ElementPropertyMetaData(_testEngine.Object, "jsproperty", typeof(JavaScript), true, "", null, delayExecution);
            testElementMetaData.Add("jsproperty", p2);
            propertyMetaData.Add("test", testElementMetaData);
            _pipeline.Setup(p => p.ElementAvailableProperties).Returns(propertyMetaData);

            // Run the test
            var json = TestIteration(1);

            // Verify that the *delayexecution and *evidenceproperties
            // values are populated as expected.
            JObject obj = JObject.Parse(json);
            var results = obj["test"].Children().ToList();

            bool hasEvidenceProperties = results.Any(t => t.ToString().Contains("propertyevidenceproperties"));
            bool hasDelayedExecution = results.Any(t => t.ToString().Contains("jspropertydelayexecution"));

            Assert.AreEqual(delayExecution ? true : false, hasEvidenceProperties,
                $"The JSON data does {(delayExecution ? "not" : "")} " +
                $"contain a 'propertyevidenceproperties' property. " +
                $"Complete JSON: {json}");
            Assert.AreEqual(delayExecution ? true : false, hasDelayedExecution,
                $"The JSON data does {(delayExecution ? "not" : "")} " +
                $"contain a 'jspropertydelayexecution' property. " +
                $"Complete JSON: {json}");
            if (delayExecution)
            {
                var evidenceProperties = results.Single(t => (t as JProperty)?.Name.Equals("propertyevidenceproperties") ?? false) as JProperty;
                Assert.IsInstanceOfType(evidenceProperties.Value, typeof(JArray));
                Assert.AreEqual("test.jsproperty", (evidenceProperties.Value as JArray).Single().ToString());
            }
        }
        
        /// <summary>
        /// Check that delayed execution and evidence properties values
        /// are populated correctly when a property has multiple 
        /// evidence properties
        /// </summary>
        [DataTestMethod]
        public void JsonBuilder_MultipleEvidenceProperties()
        {
            // Configure the property meta-data as needed for
            // this test.
            // property is populated by 2 JavaScript properties: 
            // jsproperty and jsproperty2.
            // jsproperty has delayed execution true and
            // jsproperty2 does not.
            var propertyMetaData = new Dictionary<string, IReadOnlyDictionary<string, IElementPropertyMetaData>>();
            var testElementMetaData = new Dictionary<string, IElementPropertyMetaData>();
            var p1 = new ElementPropertyMetaData(_testEngine.Object, "property", typeof(string), true, "", null, false, new List<string>() { "jsproperty", "jsproperty2" });
            testElementMetaData.Add("property", p1);
            var p2 = new ElementPropertyMetaData(_testEngine.Object, "jsproperty", typeof(JavaScript), true, "", null, true);
            testElementMetaData.Add("jsproperty", p2);
            var p3 = new ElementPropertyMetaData(_testEngine.Object, "jsproperty2", typeof(JavaScript), true, "", null, false);
            testElementMetaData.Add("jsproperty2", p2);
            propertyMetaData.Add("test", testElementMetaData);
            _pipeline.Setup(p => p.ElementAvailableProperties).Returns(propertyMetaData);

            _elementDataMock.Setup(ed => ed.AsDictionary()).
                Returns(new Dictionary<string, object>() {
                    { "property", "thisIsAValue" },
                    { "jsproperty", "var = 'some js code';" },
                    { "jsproperty2", "var = 'some js code';" }
                });

            // Run the test
            var json = TestIteration(1);

            // Verify that the *delayexecution and *evidenceproperties
            // values are populated as expected.
            JObject obj = JObject.Parse(json);
            var results = obj["test"].Children().ToList();

            Assert.IsTrue(results.Any(t => t.ToString().Contains("propertyevidenceproperties")),
                $"Expected the JSON to contain a 'propertyevidenceproperties' " +
                $"item." +
                $"Complete JSON: {json}");
            var pep = results.Where(t => t.ToString().Contains("propertyevidenceproperties")).Single();
            Assert.AreEqual("test.jsproperty", pep.Values().Single().ToString(),
                $"Expected the JSON to contain a 'propertyevidenceproperties' " +
                $"item where the value is an array with one item, 'test.jsproperty'." +
                $"Complete JSON: {json}");
            Assert.IsTrue(results.Any(t => t.ToString().Contains("jspropertydelayexecution")),
                $"Expected the JSON to contain a 'jspropertydelayexecution' " +
                $"item." +
                $"Complete JSON: {json}");
            Assert.IsFalse(results.Any(t => t.ToString().Contains("jsproperty2delayexecution")),
                $"Expected the JSON not to contain a 'jsproperty2delayexecution' " +
                $"item." +
                $"Complete JSON: {json}");
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

        private string TestIteration(int iteration,
            Dictionary<string, object> data = null, 
            Dictionary<string, object> jsProperties = null)
        {
            if(data == null)
            {
                data = new Dictionary<string, object>() { { "test", _elementDataMock.Object } };
            }
            if(jsProperties == null)
            {
                jsProperties = new Dictionary<string, object>();
            }

            var flowData = new Mock<IFlowData>();
            var _missingPropertyService = new Mock<IMissingPropertyService>();

            flowData.Setup(d => d.ElementDataAsDictionary()).Returns(data);
            string session = "somesessionid";
            flowData.Setup(d => d.TryGetEvidence("query.session-id", out session)).Returns(true);
            flowData.Setup(d => d.TryGetEvidence("query.sequence", out iteration)).Returns(true);
            flowData.Setup(d => d.GetWhere(It.IsAny<Func<IElementPropertyMetaData, bool>>())).Returns(jsProperties);

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
            flowData.Setup(d => d.Pipeline).Returns(_pipeline.Object);

            _jsonBuilderElement.Process(flowData.Object);

            var json = result["json"].ToString();

            return json;
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

        private bool ContainsJavaScriptProperties(string json)
        {
            JObject obj = JObject.Parse(json);
            var results = obj.Children().ToList();

            foreach (var result in results)
            {
                var res = result.ToString();
                if (res.Contains("javascriptProperties"))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
