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
using FiftyOne.Pipeline.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Net;

namespace FiftyOne.Pipeline.Web.Tests
{

    [TestClass]
    public class WebRequestEvidenceServiceTest
    {
        private static string expectedValue = "expected";

        private static string requiredKey = "requiredkey";
        private static string notRequiredKey = "notrequiredkey";

        private static IPAddress ip = new IPAddress(123321);

        private Mock<IFlowData> flowData;
        private WebRequestEvidenceService service;
        private Mock<HttpRequest> request;

        /// <summary>
        /// Set up the test by creating a new service and initialising any
        /// required objects and keys.
        /// </summary>
        [TestInitialize]
        public void StartUp()
        {
            var values = new Dictionary<string, string>() {
                { requiredKey, expectedValue },
                { "null", null } };
            var valuesV = new Dictionary<string, StringValues>() {
                { requiredKey, new StringValues(expectedValue) },
                { "null", new StringValues((string)null) } };

            var headers = new HeaderDictionary(valuesV);
            var cookies = new RequestCookieCollection(values);
            var query = new QueryCollection(valuesV);

            request = new Mock<HttpRequest>();
            flowData = new Mock<IFlowData>();
            service = new WebRequestEvidenceService();


            request.SetupGet(r => r.Headers).Returns(headers);
            request.SetupGet(r => r.Cookies).Returns(cookies);
            request.SetupGet(r => r.Query).Returns(query);
            request.SetupGet(r => r.HttpContext.Connection.LocalIpAddress)
                .Returns(ip);
        }

        /// <summary>
        /// Set a single required key in the flow data.
        /// </summary>
        /// <param name="key">required key to set</param>
        private void SetRequiredKey(string key)
        {
            flowData.SetupGet(f => f.EvidenceKeyFilter)
                .Returns(new EvidenceKeyFilterWhitelist(
                    new List<string>() { key }));
        }

        /// <summary>
        /// Set the required key in the flow data to "prefix.requiredkey" where
        /// prefix is supplied, and the required key is a constant which exists
        /// in headers, cookies and query parameters.
        /// 
        /// Then test that the evidence for the required key (and only the
        /// required key) is added to the flow data evidence collection.
        /// </summary>
        /// <param name="prefix">prefix to test</param>
        private void CheckRequired(string prefix)
        {
            SetRequiredKey(prefix + "." + requiredKey);
            service.AddEvidenceFromRequest(flowData.Object, request.Object);
            flowData.Verify(f => f.AddEvidence(
                It.IsAny<string>(), It.IsAny<string>()),
                Times.Once);
            flowData.Verify(f => f.AddEvidence(
                prefix + "." + requiredKey, expectedValue),
                Times.Once);
        }

        /// <summary>
        /// Set the required key in the flow data to "prefix.notrequiredkey"
        /// where prefix is supplied, and the not required key is a constant
        /// which does not exist anywhere in the request.
        /// 
        /// Then test that no evidence is added to the flow data evidence
        /// collection.
        /// </summary>
        /// <param name="prefix"></param>
        private void CheckNotRequired(string prefix)
        {
            SetRequiredKey(prefix + "." + notRequiredKey);
            service.AddEvidenceFromRequest(flowData.Object, request.Object);
            flowData.Verify(f => f.AddEvidence(
                It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }

        /// <summary>
        /// Test that a client IP address required by a flow data is correctly
        /// added to its evidence collection.
        /// </summary>
        [TestMethod]
        public void WebRequestEvidenceService_AddClientIp()
        {
            var key = "server.client-ip";
            SetRequiredKey(key);
            service.AddEvidenceFromRequest(flowData.Object, request.Object);
            flowData.Verify(f => f.AddEvidence(
                It.IsAny<string>(), It.IsAny<string>()),
                Times.Once);
            flowData.Verify(f => f.AddEvidence(key, ip.ToString()), Times.Once);
        }

        /// <summary>
        /// Test that an existing header required by a flow data is correctly
        /// added to its evidence collection.
        /// </summary>
        [TestMethod]
        public void WebRequestEvidenceService_AddRequiredHeader()
        {
            CheckRequired("header");
        }

        /// <summary>
        /// Test that a non-existent header required by a flow data is not
        /// added to its evidence collection (and neither is anything else).
        /// </summary>
        [TestMethod]
        public void WebRequestEvidenceService_AddNotRequiredHeader()
        {
            CheckNotRequired("header");
        }

        /// <summary>
        /// Test that an existing cookie required by a flow data is correctly
        /// added to its evidence collection.
        /// </summary>
        [TestMethod]
        public void WebRequestEvidenceService_AddRequiredCookie()
        {
            CheckRequired("cookie");
        }

        /// <summary>
        /// Test that a non-existent cookie required by a flow data is not
        /// added to its evidence collection (and neither is anything else).
        /// </summary>
        [TestMethod]
        public void WebRequestEvidenceService_AddNotRequiredCookie()
        {
            CheckNotRequired("cookie");
        }

        /// <summary>
        /// Test that an existing query parameter required by a flow data is
        /// correctly added to its evidence collection.
        /// </summary>
        [TestMethod]
        public void WebRequestEvidenceService_AddRequiredParam()
        {
            CheckRequired("query");
        }

        /// <summary>
        /// Test that a non-existent query parameter  required by a flow data
        /// is not added to its evidence collection (and neither is anything
        /// else).
        /// </summary>
        [TestMethod]
        public void WebRequestEvidenceService_AddNotRequiredParam()
        {
            CheckNotRequired("query");
        }

        /// <summary>
        /// Set the required key in the flow data to "prefix.null" where prefix
        /// if provided, and null is a null value which exists in headers,
        /// cookies and query parameters.
        /// 
        /// Then test that the evidence for the required key (and only the
        /// required key) is added to the flow data evidence collection as an
        /// empty string, and that a null reference exception does not occur.
        /// </summary>
        /// <param name="prefix">prefix to test</param>
        private void CheckNullValue(string prefix)
        {
            SetRequiredKey(prefix + ".null");
            service.AddEvidenceFromRequest(flowData.Object, request.Object);
            flowData.Verify(f => f.AddEvidence(
                It.IsAny<string>(), It.IsAny<string>()),
                Times.Once);
            flowData.Verify(f => f.AddEvidence(
                prefix + ".null", ""),
                Times.Once);
        }

        /// <summary>
        /// Test that a null header value is added to the flow data evidence
        /// collection as an empty string, and that a null reference exception
        /// does not occur.
        /// </summary>
        [TestMethod]
        public void WebRequestEvidenceService_NullHeaderValue()
        {
            CheckNullValue("header");
        }

        /// <summary>
        /// Test that a null cookie value is added to the flow data evidence
        /// collection as an empty string, and that a null reference exception
        /// does not occur.
        /// </summary>
        [TestMethod]
        public void WebRequestEvidenceService_NullCookieValue()
        {
            CheckNullValue("cookie");
        }
        /// <summary>
        /// Test that a null query parameter value is added to the flow data
        /// evidence collection as an empty string, and that a null reference
        /// exception does not occur.
        /// </summary>
        [TestMethod]
        public void WebRequestEvidenceService_NullParamValue()
        {
            CheckNullValue("query");
        }
    }
}
