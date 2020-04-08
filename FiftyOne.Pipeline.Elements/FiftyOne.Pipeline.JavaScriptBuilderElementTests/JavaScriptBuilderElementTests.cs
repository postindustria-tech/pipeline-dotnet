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
            
            if(value == null)
            {
                json[key] = new JObject(new JProperty(property, value), new JProperty(property+"nullreason", "No value set"));
                value = "No value set";
            } else
            {
                json[key] = new JObject(new JProperty(property, value));
            }
            IJavaScriptBuilderElementData result = null;
            var flowData = new Mock<IFlowData>();

            flowData.Setup(d => d.Get<IJsonBuilderElementData>()).Returns(() =>
            {
                var d = new JsonBuilderElementData(new Mock<ILogger<JsonBuilderElementData>>().Object, flowData.Object.Pipeline);
                d.Json = json.ToString();
                return d;
            });
            
            flowData.Setup(d => d.GetAsString(It.IsAny<string>())).Returns("None");
            flowData.Setup(d => d.GetEvidence().AsDictionary()).Returns(new Dictionary<string, object>() {
                { Pipeline.JavaScriptBuilder.Constants.EVIDENCE_HOST_KEY, "localhost" },
                { Pipeline.JavaScriptBuilder.Constants.EVIDENCE_PROTOCOL, "https" },
            });
            flowData.Setup(d => d.Get(It.IsAny<string>())).Returns(_elementDataMock.Object);
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

        delegate void GetValueCallback(string key, out string result);

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
            flowData.Setup(d => d.Get<IJsonBuilderElementData>()).Returns(() =>
            {
                var d = new JsonBuilderElementData(new Mock<ILogger<JsonBuilderElementData>>().Object, null);
                d.Json = @"{ ""test"": ""value"" }";
                return d;
            });
            // Setup the TryGetEvidence methods that are used to get 
            // host and protocol for the callback URL
            flowData.Setup(d => d.TryGetEvidence(Pipeline.JavaScriptBuilder.Constants.EVIDENCE_HOST_KEY, out It.Ref<string>.IsAny))
                .Callback(new GetValueCallback((string key, out string result) => { result = "localhost"; }));
            flowData.Setup(d => d.TryGetEvidence(Pipeline.JavaScriptBuilder.Constants.EVIDENCE_PROTOCOL, out It.Ref<string>.IsAny))
                .Callback(new GetValueCallback((string key, out string result) => { result = "https"; }));
            // Setup the evidence dictionary accessor.
            // This is used to get the query parameters to add to the 
            // callback URL.
            flowData.Setup(d => d.GetEvidence().AsDictionary()).Returns(new Dictionary<string, object>() {
                { Pipeline.JavaScriptBuilder.Constants.EVIDENCE_HOST_KEY, "localhost" },
                { Pipeline.JavaScriptBuilder.Constants.EVIDENCE_PROTOCOL, "https" },
            });
            // Setup the GetOrAdd method to catch the data object that is
            // set by the element so we can check it's values.
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
    }
}
