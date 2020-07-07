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

using FiftyOne.Pipeline.Core.Data;
using FiftyOne.Pipeline.Core.TypedMap;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using NiL.JS.Core;
using Newtonsoft.Json.Linq;
using FiftyOne.Pipeline.JsonBuilder.Data;
using FiftyOne.Pipeline.JavaScriptBuilder.Data;
using FiftyOne.Pipeline.JsonBuilder.FlowElement;
using FiftyOne.Pipeline.JavaScriptBuilder.FlowElement;
using System;
using FiftyOne.Pipeline.Core.FlowElements;
using FiftyOne.Pipeline.Engines.Data;
using FiftyOne.Pipeline.Engines;
using FiftyOne.Pipeline.Core.Exceptions;

namespace FiftyOne.Pipeline.JavaScript.Tests
{
    [TestClass]
    public class JavaScriptBuilderElementTests
    {
        private Context context;
        private Mock<IJsonBuilderElement> _mockjsonBuilderElement;
        private Mock<IElementData> _elementDataMock;
        private ILoggerFactory _loggerFactory;
        private JavaScriptBuilderElement _javaScriptBuilderElement;
        private IList<IElementPropertyMetaData> _elementPropertyMetaDatas;

        /// <summary>
        /// Initialise the test.
        /// </summary>
        [TestInitialize]
        public void Init()
        {
            context = new Context();

            _mockjsonBuilderElement = new Mock<IJsonBuilderElement>();

            _elementPropertyMetaDatas = new List<IElementPropertyMetaData>() {
                new ElementPropertyMetaData(_mockjsonBuilderElement.Object, "property", typeof(string), true)
            };

            _mockjsonBuilderElement.Setup(x => x.Properties).Returns(_elementPropertyMetaDatas);
            _loggerFactory = new LoggerFactory();

            _elementDataMock = new Mock<IElementData>();
            _elementDataMock.Setup(ed => ed.AsDictionary()).Returns(new Dictionary<string, object>() { { "property", "thisIsAValue" } });
        }

        /// <summary>
        /// This method tests the accessor functionality of the JavaScript 
        /// include.
        /// 
        /// For the supplied properties values, check that these properties can 
        /// be accessed via the 'fod' object in the JavaScript include.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="property"></param>
        /// <param name="value"></param>
        [DataTestMethod]
        [DataRow("device", "ismobile", true)]
        [DataRow("device", "ismobile", false)]
        [DataRow("device", "browsername", "Chrome")]
        [DataRow("device", "browsername", null)]
        public void JavaScriptBuilderElement_JavaScript(string key, string property, object value)
        {
            _javaScriptBuilderElement =
                new JavaScriptBuilderElementBuilder(_loggerFactory).Build();

            dynamic json = new JObject();

            if (value == null)
            {
                json[key] = new JObject(new JProperty(property, value), new JProperty(property + "nullreason", "No value set"));
                value = "No value set";
            } else
            {
                json[key] = new JObject(new JProperty(property, value));
            }

            var flowData = new Mock<IFlowData>();
            Configure(flowData,  json);

            IJavaScriptBuilderElementData result = null;
            flowData.Setup(d => d.GetOrAdd(
                It.IsAny<ITypedKey<IJavaScriptBuilderElementData>>(),
                It.IsAny<Func<IPipeline, IJavaScriptBuilderElementData>>()))
                .Returns<ITypedKey<IJavaScriptBuilderElementData>, Func<IPipeline, IJavaScriptBuilderElementData>>((k, f) =>
                {
                    result = f(flowData.Object.Pipeline);
                    return result;
                });

            _javaScriptBuilderElement.Process(flowData.Object);

            Assert.IsTrue(IsValidFodObject(result.JavaScript, key, property, value));
        }

        /// <summary>
        /// Check that the callback URL is generated correctly.
        /// </summary>
        /// <remarks>
        /// TODO: Add more tests verifying URL if other parameters are set 
        /// and if query parameters are in the evidence.
        /// </remarks>
        [TestMethod]
        public void JavaScriptBuilder_VerifyUrl()
        {
            _javaScriptBuilderElement =
                new JavaScriptBuilderElementBuilder(_loggerFactory)
                .SetEndpoint("/json")
                .Build();
            
            var flowData = new Mock<IFlowData>();
            Configure(flowData);

            IJavaScriptBuilderElementData result = null;
            flowData.Setup(d => d.GetOrAdd(
                It.IsAny<ITypedKey<IJavaScriptBuilderElementData>>(),
                It.IsAny<Func<IPipeline, IJavaScriptBuilderElementData>>()))
                .Returns<ITypedKey<IJavaScriptBuilderElementData>, Func<IPipeline, IJavaScriptBuilderElementData>>((k, f) =>
                {
                    result = f(flowData.Object.Pipeline);
                    return result;
                });

            _javaScriptBuilderElement.Process(flowData.Object);

            string expectedUrl = "https://localhost/json";
            Assert.IsTrue(result.JavaScript.Contains(expectedUrl),
                $"JavaScript does not contain expected URL '{expectedUrl}'.");
        }


        public enum ExceptionType
        {
            PropertyMissingException,
            PipelineDataException,
            InvalidCastException,
            KeyNotFoundException,
            Exception,
            None
        }

        /// <summary>
        /// Check that accessing the 'Promise' property works as intended 
        /// in a range of scenarios
        /// </summary>
        [DataTestMethod]
        [DataRow(ExceptionType.PropertyMissingException, false)]
        [DataRow(ExceptionType.Exception, true)]
        [DataRow(ExceptionType.InvalidCastException, false)]
        [DataRow(ExceptionType.KeyNotFoundException, false)]
        [DataRow(ExceptionType.None, false)]
        [DataRow(ExceptionType.PipelineDataException, false)]
        public void JavaScriptBuilderElement_Promise(ExceptionType exceptionThrownByPromiseProperty, bool exceptionExpected)
        {
            _javaScriptBuilderElement =
                new JavaScriptBuilderElementBuilder(_loggerFactory).Build();

            var flowData = new Mock<IFlowData>();
            Configure(flowData);

            IJavaScriptBuilderElementData result = null;
            flowData.Setup(d => d.GetOrAdd(
                It.IsAny<ITypedKey<IJavaScriptBuilderElementData>>(),
                It.IsAny<Func<IPipeline, IJavaScriptBuilderElementData>>()))
                .Returns<ITypedKey<IJavaScriptBuilderElementData>, Func<IPipeline, IJavaScriptBuilderElementData>>((k, f) =>
                {
                    result = f(flowData.Object.Pipeline);
                    return result;
                });

            switch (exceptionThrownByPromiseProperty)
            {
                case ExceptionType.PropertyMissingException:
                    flowData.Setup(d => d.GetAs<IAspectPropertyValue<string>>("Promise"))
                        .Throws(new PropertyMissingException("Problem!"));
                    break;
                case ExceptionType.PipelineDataException:
                    flowData.Setup(d => d.GetAs<IAspectPropertyValue<string>>("Promise"))
                        .Throws(new PipelineDataException("Problem!"));
                    break;
                case ExceptionType.InvalidCastException:
                    flowData.Setup(d => d.GetAs<IAspectPropertyValue<string>>("Promise"))
                        .Throws(new InvalidCastException("Problem!"));
                    break;
                case ExceptionType.KeyNotFoundException:
                    flowData.Setup(d => d.GetAs<IAspectPropertyValue<string>>("Promise"))
                        .Throws(new KeyNotFoundException("Problem!"));
                    break;
                case ExceptionType.Exception:
                    flowData.Setup(d => d.GetAs<IAspectPropertyValue<string>>("Promise"))
                        .Throws(new Exception("Problem!"));
                    break;
                case ExceptionType.None:
                    flowData.Setup(d => d.GetAs<IAspectPropertyValue<string>>("Promise"))
                        .Returns(new AspectPropertyValue<string>("Full"));
                    break;
                default:
                    break;
            }

            Exception thrown = null;

            try
            {
                _javaScriptBuilderElement.Process(flowData.Object);
            }
            catch(Exception ex)
            {
                thrown = ex;
            }

            if (exceptionExpected)
            {
                Assert.IsNotNull(thrown, "Expected an exception to be " +
                    "visible externally but it was not.");
            }
            else
            {
                Assert.IsNull(thrown, "Did not expect an exception " +
                    "to be visible externally but one was.");
                Assert.IsNotNull(result.JavaScript, "Expected JavaScript " +
                    "output to be populated but it was not.");
                Assert.AreNotEqual("", result.JavaScript, "Expected " +
                    "JavaScript output to be populated but it was not.");
            }
        }

        /// <summary>
        /// Verify that valid JavaScript is produced when there are
        /// delayed execution properties in the payload.
        /// </summary>
        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void JavaScriptBuilderElement_DelayExecution(bool minify)
        {
            _javaScriptBuilderElement = 
                new JavaScriptBuilderElementBuilder(_loggerFactory)
                .SetMinify(minify)
                .Build();
            
            dynamic json = new JObject();

            var locationData = new JObject();
            locationData.Add(new JProperty("postcode", null));
            locationData.Add(new JProperty("postcodenullreason", 
                "Evidence for this property has not been retrieved. Ensure the 'complete' method is called, passing the name of this property in the second parameter."));
            locationData.Add(new JProperty("postcodeevidenceproperties", new[] { "location.javascript" }));
            locationData.Add(new JProperty("javascript", "if (navigator.geolocation) { navigator.geolocation.getCurrentPosition(function() { // 51D replace this comment with callback function. }); }"));
            locationData.Add(new JProperty("javascriptdelayexecution", true));
            json["location"] = locationData;

            var flowData = new Mock<IFlowData>();
            Configure(flowData, json);

            IJavaScriptBuilderElementData result = null;
            flowData.Setup(d => d.GetOrAdd(
                It.IsAny<ITypedKey<IJavaScriptBuilderElementData>>(),
                It.IsAny<Func<IPipeline, IJavaScriptBuilderElementData>>()))
                .Returns<ITypedKey<IJavaScriptBuilderElementData>, Func<IPipeline, IJavaScriptBuilderElementData>>((k, f) =>
                {
                    result = f(flowData.Object.Pipeline);
                    return result;
                });

            _javaScriptBuilderElement.Process(flowData.Object);

            if (minify == false)
            {
                Assert.IsTrue(result.JavaScript.Contains("getEvidencePropertiesFromObject"),
                    "Expected the generated JavaScript to contain the " +
                    "'getEvidencePropertiesFromObject' function but it does not.");
            }
            // Attempt to evaluate the JavaScript.
            context.Eval(result.JavaScript);
        }

        /// <summary>
        /// Test the JavaScript include by accessing the given property.
        /// </summary>
        /// <param name="javaScript"></param>
        /// <param name="key"></param>
        /// <param name="property"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private bool IsValidFodObject(string javaScript, string key, string property, object value)
        {
            // Evaluate the JavaScript include.
            context.Eval(javaScript);

            var result = context.Eval($"fod.{key}.{property};");

            if (result.Value.ToString() == value.ToString())
                return true;
            else
                return false;
        }

        delegate void GetValueCallback(string key, out string result);

        /// <summary>
        /// Configure the flow data to respond in the way we want for 
        /// this test.
        /// </summary>
        /// <param name="flowData">
        /// The mock flow data instance to configure 
        /// </param>
        /// <param name="jsonData">
        /// The JSON data to embed in the flow data.
        /// This will be copied into the JavaScript that is produced.
        /// </param>
        /// <param name="hostName">
        /// The host name to add to the evidence.
        /// The JavaScriptBuilder should use this to generate the 
        /// callback URL.
        /// </param>
        /// <param name="protocol">
        /// The protocol to add to the evidence.
        /// The JavaScriptBuilder should use this to generate the 
        /// callback URL.
        /// </param>
        private void Configure(Mock<IFlowData> flowData, JObject jsonData = null, string hostName = "localhost", string protocol = "https")
        {
            if (jsonData == null)
            {
                jsonData = new JObject();
                jsonData["device"] = new JObject(new JProperty("ismobile", true));
            }

            flowData.Setup(d => d.Get<IJsonBuilderElementData>()).Returns(() =>
            {
                var d = new JsonBuilderElementData(new Mock<ILogger<JsonBuilderElementData>>().Object, flowData.Object.Pipeline);
                d.Json = jsonData.ToString();
                return d;
            });

            // Setup the TryGetEvidence methods that are used to get 
            // host and protocol for the callback URL
            flowData.Setup(d => d.TryGetEvidence(Pipeline.JavaScriptBuilder.Constants.EVIDENCE_HOST_KEY, out It.Ref<string>.IsAny))
                .Callback(new GetValueCallback((string key, out string result) => { result = "localhost"; }));
            flowData.Setup(d => d.TryGetEvidence(Pipeline.JavaScriptBuilder.Constants.EVIDENCE_PROTOCOL, out It.Ref<string>.IsAny))
                .Callback(new GetValueCallback((string key, out string result) => { result = "https"; }));

            flowData.Setup(d => d.GetAsString(It.IsAny<string>())).Returns("None");
            flowData.Setup(d => d.GetEvidence().AsDictionary()).Returns(new Dictionary<string, object>() {
                { Pipeline.JavaScriptBuilder.Constants.EVIDENCE_HOST_KEY, hostName },
                { Pipeline.JavaScriptBuilder.Constants.EVIDENCE_PROTOCOL, protocol },
            });
            flowData.Setup(d => d.Get(It.IsAny<string>())).Returns(_elementDataMock.Object);
        }
    }
}
