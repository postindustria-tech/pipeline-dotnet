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

using System;
using FiftyOne.Pipeline.Core.Data;
using Microsoft.AspNetCore.Http;

namespace FiftyOne.Pipeline.Web.Services
{
    public class WebRequestEvidenceService : IWebRequestEvidenceService
    {

        /// <summary>
        /// True if session is enabled.
        /// </summary>
        private bool _sessionEnabled;
        
        /// <summary>
        /// True if session has been checked for. Once it has been checked it
        /// will not change.
        /// </summary>
        private bool _checkedForSession = false;

        /// <summary>
        /// Check whether or not session is enabled. If it is not, then don't
        /// try to get evidence from it as an exception may be thrown.
        /// </summary>
        /// <param name="httpRequest">
        /// The request to check for a session.
        /// </param>
        /// <returns>
        /// True if session is enabled.
        /// </returns>
        private bool GetSessionEnabled(HttpRequest httpRequest)
        {
            if (_checkedForSession == false)
            {
                try
                {
                    if (httpRequest.HttpContext != null &&
                        httpRequest.HttpContext.Session != null)
                    {
                        _sessionEnabled = true;
                    }
                    else
                    {
                        _sessionEnabled = false;
                    }
                }
                catch (Exception)
                {
                    _sessionEnabled = false;
                }
                _checkedForSession = true;
            }
            return _sessionEnabled;
        }

        /// <summary>
        /// Use the specified <see cref="HttpRequest"/> to populated the 
        /// <see cref="IFlowData"/> with evidence.
        /// </summary>
        /// <param name="flowData">
        /// The <see cref="IFlowData"/> to populate.
        /// </param>
        /// <param name="httpRequest">
        /// The <see cref="HttpRequest"/> to pull values from.
        /// </param>
        public void AddEvidenceFromRequest(IFlowData flowData, HttpRequest httpRequest)
        {
            foreach (var header in httpRequest.Headers)
            {
                string evidenceKey = Core.Constants.EVIDENCE_HTTPHEADER_PREFIX + 
                    Core.Constants.EVIDENCE_SEPERATOR + header.Key;
                CheckAndAdd(flowData, evidenceKey, header.Value.ToString());
            }
            foreach (var cookie in httpRequest.Cookies)
            {
                string evidenceKey = Core.Constants.EVIDENCE_COOKIE_PREFIX +
                    Core.Constants.EVIDENCE_SEPERATOR + cookie.Key;
                CheckAndAdd(
                    flowData, 
                    evidenceKey, 
                    cookie.Value == null ? "" : cookie.Value.ToString());
            }
            foreach (var queryValue in httpRequest.Query)
            {
                string evidenceKey = Core.Constants.EVIDENCE_QUERY_PREFIX +
                    Core.Constants.EVIDENCE_SEPERATOR + queryValue.Key;
                CheckAndAdd(flowData, evidenceKey, queryValue.Value.ToString());
            }
            if (GetSessionEnabled(httpRequest))
            {
                foreach (var sessionKey in httpRequest.HttpContext.Session.Keys)
                {
                    string evidenceKey = Core.Constants.EVIDENCE_SESSION_PREFIX +
                        Core.Constants.EVIDENCE_SEPERATOR + sessionKey;
                    CheckAndAdd(flowData, evidenceKey, httpRequest.HttpContext.Session.GetString(sessionKey));
                }
                CheckAndAdd(flowData, Core.Constants.EVIDENCE_SESSION_KEY,
                    new AspCoreSession(httpRequest.HttpContext.Session));
            }

            CheckAndAdd(flowData, Core.Constants.EVIDENCE_CLIENTIP_KEY,
                httpRequest.HttpContext.Connection.LocalIpAddress.ToString());
            
        }

        /// <summary>
        /// Check if the given key is needed by the given flowdata.
        /// If it is then add it as evidence.
        /// </summary>
        /// <param name="flowData">
        /// The <see cref="IFlowData"/> to add the evidence to.
        /// </param>
        /// <param name="key">
        /// The evidence key
        /// </param>
        /// <param name="value">
        /// The evidence value
        /// </param>
        private void CheckAndAdd(IFlowData flowData, string key, object value)
        {
            if (flowData.EvidenceKeyFilter.Include(key))
            {
                flowData.AddEvidence(key, value);   
            }
        }
    }
}
