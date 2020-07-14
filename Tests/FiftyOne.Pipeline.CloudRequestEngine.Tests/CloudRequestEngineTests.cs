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

using FiftyOne.Pipeline.CloudRequestEngine.FlowElements;
using FiftyOne.Pipeline.Core.Exceptions;
using FiftyOne.Pipeline.Core.FlowElements;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace FiftyOne.Pipeline.CloudRequestEngine.Tests
{
    [TestClass]
    public class CloudRequestEngineTests
    {
        HttpClient _httpClient;
        private static ILoggerFactory _loggerFactory = new LoggerFactory();
        private Mock<HttpMessageHandler> _handlerMock;

        private Uri expectedUri = new Uri("https://cloud.51degrees.com/api/v4/json");

        private string _jsonResponse = "{'device':{'value':'1'}}";
        private string _evidenceKeysResponse = "['query.User-Agent']";
        private string _accessiblePropertiesResponse = 
            "{'Products': {'device': {'DataTier': 'tier','Properties': [{'Name': 'value','Type': 'String','Category': 'Device'}]}}}";
        private HttpStatusCode _accessiblePropertiesResponseStatus = HttpStatusCode.OK;


        /// <summary>
        /// Test cloud request engine adds correct information to post request
        /// and returns the response in the ElementData
        /// </summary>
        [TestMethod]
        public void Process()
        {
            string resourceKey = "resource_key";
            string userAgent = "iPhone";
            ConfigureMockedClient(r => 
                r.Content.ReadAsStringAsync().Result.Contains($"resource={resourceKey}") // content contains resource key
                && r.Content.ReadAsStringAsync().Result.Contains($"User-Agent={userAgent}") // content contains licenseKey
            );

            var engine = new CloudRequestEngineBuilder(_loggerFactory, _httpClient)
                .SetResourceKey(resourceKey)
                .Build();

            using (var pipeline = new PipelineBuilder(_loggerFactory).AddFlowElement(engine).Build())
            {
                var data = pipeline.CreateFlowData();
                data.AddEvidence("query.User-Agent", userAgent);

                data.Process();

                var result = data.GetFromElement(engine).JsonResponse;
                Assert.AreEqual("{'device':{'value':'1'}}", result);

                dynamic obj = JValue.Parse(result);
                Assert.AreEqual(1, (int)obj.device.value);
            }

            _handlerMock.Protected().Verify(
               "SendAsync",
               Times.Exactly(1), // we expected a single external request
               ItExpr.Is<HttpRequestMessage>(req =>
                  req.Method == HttpMethod.Post  // we expected a POST request
                  && req.RequestUri == expectedUri // to this uri
               ),
               ItExpr.IsAny<CancellationToken>()
            );
        }

        /// <summary>
        /// Test cloud request engine adds correct information to post request
        /// and returns the response in the ElementData
        /// </summary>
        [TestMethod]
        public void Process_LicenseKey()
        {
            string resourceKey = "resource_key";
            string userAgent = "iPhone";
            string licenseKey = "ABCDEFG";
            ConfigureMockedClient(r =>
                  r.Content.ReadAsStringAsync().Result.Contains($"resource={resourceKey}") // content contains resource key
                  && r.Content.ReadAsStringAsync().Result.Contains($"license={licenseKey}") // content contains licenseKey
                  && r.Content.ReadAsStringAsync().Result.Contains($"User-Agent={userAgent}") // content contains user agent
            );

#pragma warning disable CS0618 // Type or member is obsolete
            // SetLicensekey is obsolete but we still want to test that
            // it works as intended.
            var engine = new CloudRequestEngineBuilder(_loggerFactory, _httpClient)
                .SetResourceKey(resourceKey)
                .SetLicenseKey(licenseKey)
#pragma warning restore CS0618 // Type or member is obsolete
                .Build();

            using (var pipeline = new PipelineBuilder(_loggerFactory).AddFlowElement(engine).Build())
            {
                var data = pipeline.CreateFlowData();
                data.AddEvidence("query.User-Agent", userAgent);

                data.Process();

                var result = data.GetFromElement(engine).JsonResponse;
                Assert.AreEqual("{'device':{'value':'1'}}", result);

                dynamic obj = JValue.Parse(result);
                Assert.AreEqual(1, (int)obj.device.value);
            }

            _handlerMock.Protected().Verify(
               "SendAsync",
               Times.Exactly(1), // we expected a single external request
               ItExpr.Is<HttpRequestMessage>(req =>
                  req.Method == HttpMethod.Post  // we expected a POST request
                  && req.RequestUri == expectedUri // to this uri
               ),
               ItExpr.IsAny<CancellationToken>()
            );
        }

        /// <summary>
        /// Verify that the CloudRequestEngine can correctly parse a 
        /// response from the accessible properties endpoint that contains
        /// meta-data for sub-properties.
        /// </summary>
        [TestMethod]
        public void SubProperties()
        {
            _accessiblePropertiesResponse = @"
{
    ""Products"": {
        ""device"": {
            ""DataTier"": ""CloudV4TAC"",
            ""Properties"": [
                {
                    ""Name"": ""IsMobile"",
                    ""Type"": ""Boolean"",
                    ""Category"": ""Device""
                },
                {
                    ""Name"": ""IsTablet"",
                    ""Type"": ""Boolean"",
                    ""Category"": ""Device""
                }
            ]
        },
        ""devices"": {
            ""DataTier"": ""CloudV4TAC"",
            ""Properties"": [
                {
                    ""Name"": ""Devices"",
                    ""Type"": ""Array"",
                    ""Category"": ""Unspecified"",
                    ""ItemProperties"": [
                        {
                            ""Name"": ""IsMobile"",
                            ""Type"": ""Boolean"",
                            ""Category"": ""Device""
                        },
                        {
                            ""Name"": ""IsTablet"",
                            ""Type"": ""Boolean"",
                            ""Category"": ""Device""
                        }
                    ]
                }
            ]
        }
    }
}";
            ConfigureMockedClient(r => true);

            var engine = new CloudRequestEngineBuilder(_loggerFactory, _httpClient)
                .SetResourceKey("key")
                .Build();

            Assert.AreEqual(2, engine.PublicProperties.Count);
            var deviceProperties = engine.PublicProperties["device"];
            Assert.AreEqual(2, deviceProperties.Properties.Count);
            Assert.IsTrue(deviceProperties.Properties.Any(p => p.Name.Equals("IsMobile")));
            Assert.IsTrue(deviceProperties.Properties.Any(p => p.Name.Equals("IsTablet")));
            var devicesProperties = engine.PublicProperties["devices"];
            Assert.AreEqual(1, devicesProperties.Properties.Count);
            Assert.AreEqual("Devices", devicesProperties.Properties[0].Name);
            Assert.IsTrue(devicesProperties.Properties[0].ItemProperties.Any(p => p.Name.Equals("IsMobile")));
            Assert.IsTrue(devicesProperties.Properties[0].ItemProperties.Any(p => p.Name.Equals("IsTablet")));
        }


        /// <summary>
        /// Test cloud request engine handles errors from the cloud service 
        /// as expected.
        /// A PipelineException should be thrown by the cloud request engine
        /// and the pipeline is configured to throw any exceptions up 
        /// the stack in an AggregateException.
        /// We also check that the exception message includes the content 
        /// from the JSON response.
        /// </summary>
        [TestMethod]
        public void ValidateErrorHandling()
        {
            string resourceKey = "resource_key";
            string userAgent = "iPhone";
            _jsonResponse = @"{ ""errors"":[""16384440: This resource key is not authorized for use with this domain: . Please visit https://configure.51degrees.com to update your resource key.""]}";

            ConfigureMockedClient(r =>
                r.Content.ReadAsStringAsync().Result.Contains($"resource={resourceKey}") // content contains resource key
                && r.Content.ReadAsStringAsync().Result.Contains($"User-Agent={userAgent}") // content contains licenseKey
            );

            var engine = new CloudRequestEngineBuilder(_loggerFactory, _httpClient)
                .SetResourceKey(resourceKey)
                .Build();

            Exception exception = null;

            try
            {
                using (var pipeline = new PipelineBuilder(_loggerFactory).AddFlowElement(engine).Build())
                {
                    var data = pipeline.CreateFlowData();
                    data.AddEvidence("query.User-Agent", userAgent);

                    data.Process();
                }
            }
            catch(Exception ex)
            {
                exception = ex;
            }

            Assert.IsNotNull(exception, "Expected exception to occur");
            Assert.IsInstanceOfType(exception, typeof(AggregateException));
            var aggEx = exception as AggregateException;
            Assert.AreEqual(aggEx.InnerExceptions.Count, 1);
            var realEx = aggEx.InnerExceptions[0];
            Assert.IsInstanceOfType(realEx, typeof(PipelineException));
            Assert.IsTrue(realEx.Message.Contains(
                "This resource key is not authorized for use with this domain"), 
                "Exception message did not contain the expected text.");
        }


        /// <summary>
        /// Test cloud request engine handles errors from the cloud service 
        /// as expected.
        /// An AggregateException should be thrown by the cloud request engine
        /// containing the errors from the cloud service
        /// and the pipeline is configured to throw any exceptions up 
        /// the stack in an AggregateException.
        /// We also check that the exception message includes the content 
        /// from the JSON response.
        /// </summary>
        [TestMethod]
        public void ValidateErrorHandling_InvalidResourceKey()
        {
            string resourceKey = "resource_key";
            string userAgent = "iPhone";
            _accessiblePropertiesResponse = @"{ ""errors"":[""58982060: resource_key not a valid resource key""]}";
            _accessiblePropertiesResponseStatus = HttpStatusCode.BadRequest;

            ConfigureMockedClient(r =>
                r.Content.ReadAsStringAsync().Result.Contains($"resource={resourceKey}") // content contains resource key
                && r.Content.ReadAsStringAsync().Result.Contains($"User-Agent={userAgent}") // content contains licenseKey
            );

            Exception exception = null;

            try { 
                var engine = new CloudRequestEngineBuilder(_loggerFactory, _httpClient)
                    .SetResourceKey(resourceKey)
                    .Build();
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            Assert.IsNotNull(exception, "Expected exception to occur");
            Assert.IsInstanceOfType(exception, typeof(AggregateException));
            var aggEx = exception as AggregateException;
            Assert.AreEqual(1, aggEx.InnerExceptions.Count);
            var realEx = aggEx.InnerExceptions[0];
            Assert.IsInstanceOfType(realEx, typeof(PipelineException));
            Assert.IsTrue(realEx.Message.Contains(
                "resource_key not a valid resource key"),
                "Exception message did not contain the expected text.");
        }


        /// <summary>
        /// Test cloud request engine handles multiple errors from the cloud 
        /// service as expected.
        /// An AggregateException should be thrown by the cloud request engine
        /// and the pipeline is configured to throw any exceptions up 
        /// the stack as another AggregateException.
        /// We also check that the exception messages include the content 
        /// from the JSON response.
        /// </summary>
        [TestMethod]
        public void ValidateErrorHandling_MultipleErrors()
        {
            string resourceKey = "resource_key";
            string userAgent = "iPhone";
            _jsonResponse = @"{ ""errors"":[""16384440: This resource key is not authorized for use with this domain: . Please visit https://configure.51degrees.com to update your resource key."",""Some other error""]}";

            ConfigureMockedClient(r =>
                r.Content.ReadAsStringAsync().Result.Contains($"resource={resourceKey}") // content contains resource key
                && r.Content.ReadAsStringAsync().Result.Contains($"User-Agent={userAgent}") // content contains licenseKey
            );

            var engine = new CloudRequestEngineBuilder(_loggerFactory, _httpClient)
                .SetResourceKey(resourceKey)
                .Build();

            Exception exception = null;

            try
            {
                using (var pipeline = new PipelineBuilder(_loggerFactory).AddFlowElement(engine).Build())
                {
                    var data = pipeline.CreateFlowData();
                    data.AddEvidence("query.User-Agent", userAgent);

                    data.Process();
                }
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            Assert.IsNotNull(exception, "Expected exception to occur");
            Assert.IsInstanceOfType(exception, typeof(AggregateException));
            var aggEx = (exception as AggregateException).Flatten();
            Assert.AreEqual(aggEx.InnerExceptions.Count, 2);
            Assert.IsInstanceOfType(aggEx.InnerExceptions[0], typeof(PipelineException));
            Assert.IsInstanceOfType(aggEx.InnerExceptions[1], typeof(PipelineException));
            Assert.IsTrue(aggEx.InnerExceptions.Any(e => e.Message.Contains(
                "This resource key is not authorized for use with this domain")),
                "Exception message did not contain the expected text.");
            Assert.IsTrue(aggEx.InnerExceptions.Any(e => e.Message.Contains(
                "Some other error")),
                "Exception message did not contain the expected text.");
        }

        /// <summary>
        /// Test cloud request engine handles a lack of data from the 
        /// cloud service as expected.
        /// An exception should be thrown by the cloud request engine
        /// and the pipeline is configured to throw any exceptions up 
        /// the stack as an AggregateException.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(AggregateException))]
        public void ValidateErrorHandling_NoData()
        {
            string resourceKey = "resource_key";
            string userAgent = "iPhone";
            _jsonResponse = @"{ }";

            ConfigureMockedClient(r =>
                r.Content.ReadAsStringAsync().Result.Contains($"resource={resourceKey}") // content contains resource key
                && r.Content.ReadAsStringAsync().Result.Contains($"User-Agent={userAgent}") // content contains licenseKey
            );

            var engine = new CloudRequestEngineBuilder(_loggerFactory, _httpClient)
                .SetResourceKey(resourceKey)
                .Build();

            using (var pipeline = new PipelineBuilder(_loggerFactory).AddFlowElement(engine).Build())
            {
                var data = pipeline.CreateFlowData();
                data.AddEvidence("query.User-Agent", userAgent);

                data.Process();
            }
        }
        
        /// <summary>
        /// Verify that the 'DelayExecution' and 'EvidenceProperties'
        /// properties are populated correctly by the CloudRequestEngine.
        /// </summary>
        [TestMethod]
        public void ValidateDelayedExecutionProperties()
        {
            _accessiblePropertiesResponse =
                "{'Products': {'location': {'DataTier': 'tier','Properties': [" +
                    "{'Name': 'javascript','Type': 'JavaScript','Category': 'Unspecified','DelayExecution':true}," +
                    "{'Name': 'postcode','Type': 'String','Category': 'Unspecified','EvidenceProperties':[ 'location.javascript' ]}]}}}";

            ConfigureMockedClient(r => true);

            var engine = new CloudRequestEngineBuilder(_loggerFactory, _httpClient)
                .SetResourceKey("key")
                .Build();

            Assert.AreEqual(1, engine.PublicProperties.Count);
            var locationProperties = engine.PublicProperties["location"];
            Assert.AreEqual(2, locationProperties.Properties.Count);
            var javascript = locationProperties.Properties.Where(p => p.Name.Equals("javascript")).Single();
            var postcode = locationProperties.Properties.Where(p => p.Name.Equals("postcode")).Single();
            Assert.AreEqual(true, javascript.DelayExecution);
            Assert.AreEqual(1, postcode.EvidenceProperties.Count);
            Assert.AreEqual("location.javascript", postcode.EvidenceProperties.Single());
        }


        /// <summary>
        /// Setup _httpClient to respond with the configured messages.
        /// </summary>
        private void ConfigureMockedClient(
            Func<HttpRequestMessage, bool> expectedJsonParameters)
        {
            // ARRANGE
            _handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            // Set up the JSON response.
            _handlerMock
               .Protected()
               // Setup the PROTECTED method to mock
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.Is<HttpRequestMessage>(r => expectedJsonParameters(r)
                      && r.RequestUri.AbsolutePath.ToLower().EndsWith("json")),
                  ItExpr.IsAny<CancellationToken>()
               )
               // prepare the expected response of the mocked http call
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent(_jsonResponse),
               })
               .Verifiable();
            // Set up the evidencekeys response.
            _handlerMock
               .Protected()
               // Setup the PROTECTED method to mock
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.Is<HttpRequestMessage>(r =>
                      r.RequestUri.AbsolutePath.ToLower().EndsWith("evidencekeys")),
                  ItExpr.IsAny<CancellationToken>()
               )
               // prepare the expected response of the mocked http call
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent(_evidenceKeysResponse),
               })
               .Verifiable();
            // Set up the accessibleproperties response.
            _handlerMock
               .Protected()
               // Setup the PROTECTED method to mock
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.Is<HttpRequestMessage>(r =>
                      r.RequestUri.AbsolutePath.ToLower().EndsWith("accessibleproperties")),
                  ItExpr.IsAny<CancellationToken>()
               )
               // prepare the expected response of the mocked http call
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = _accessiblePropertiesResponseStatus,
                   Content = new StringContent(_accessiblePropertiesResponse),
               })
               .Verifiable();

            // use real http client with mocked handler here
            _httpClient = new HttpClient(_handlerMock.Object);
        }
    }
}
