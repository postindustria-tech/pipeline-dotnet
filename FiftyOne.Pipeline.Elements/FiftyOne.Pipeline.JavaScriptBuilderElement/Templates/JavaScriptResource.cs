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
using System.Collections.Generic;

namespace FiftyOne.Pipeline.JavaScriptBuilder.Templates
{
    /// <summary>
    /// A helper class that packages the parameters required by the 
    /// JavaScript mustache template into the required format.
    /// </summary>
    public class JavaScriptResource
    {
        private string _objName;
        private string _jsonObject;
        private bool _supportsPromises;
        private Uri _url;
        private bool _enableCookies;
        private bool _updateEnabled;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks>
        /// The callback mechanism is a feature that allows the client-side
        /// data to be updated in the background when any properties that 
        /// contain JavaScript code have been executed on the client and
        /// therefore, new evidence is available.
        /// </remarks>
        /// <param name="objName">
        /// The name of the global-scope JavaScript object that will be 
        /// created on the client-side by the JavaScript produced by 
        /// the template.
        /// </param>
        /// <param name="jsonObject">
        /// The JSON data payload to be inserted into the template.
        /// </param>
        /// <param name="supportsPromises">
        /// If true, the template will produce JavaScript that makes 
        /// use of promises.
        /// If false, promises will not be used.
        /// </param>
        /// <param name="url">
        /// The complete URL to use for the callback mechanism described 
        /// in remarks for this constructor.
        /// </param>
        /// <param name="enableCookies">
        /// If false, any cookies created by JavaScript properties that
        /// execute on the client-side and that start with '51D_' will 
        /// be deleted automatically.
        /// </param>
        /// <param name="updateEnabled">
        /// True to use the callback mechanism that is described in 
        /// remarks for this constructor.
        /// False to disable that mechanism. In this case, a second request 
        /// must be initiated by the user in order for the server to 
        /// access the additional evidence gathered by client-side code.
        /// </param>
        public JavaScriptResource(
            string objName,
            string jsonObject,
            bool supportsPromises,
            string url,
            bool enableCookies,
            bool updateEnabled)
        {
            _objName = objName;
            _jsonObject = string.IsNullOrWhiteSpace(jsonObject) == false
                ? jsonObject : "{\"errors\":[\"Json data missing.\"]}";
            _supportsPromises = supportsPromises;
            _url = new Uri(url);
            _enableCookies = enableCookies;
            _updateEnabled = updateEnabled;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks>
        /// The callback mechanism is a feature that allows the client-side
        /// data to be updated in the background when any properties that 
        /// contain JavaScript code have been executed on the client and
        /// therefore, new evidence is available.
        /// </remarks>
        /// <param name="objName">
        /// The name of the global-scope JavaScript object that will be 
        /// created on the client-side by the JavaScript produced by 
        /// the template.
        /// </param>
        /// <param name="jsonObject">
        /// The JSON data payload to be inserted into the template.
        /// </param>
        /// <param name="supportsPromises">
        /// If true, the template will produce JavaScript that makes 
        /// use of promises.
        /// If false, promises will not be used.
        /// </param>
        /// <param name="url">
        /// The complete URL to use for the callback mechanism described 
        /// in remarks for this constructor.
        /// </param>
        /// <param name="enableCookies">
        /// If false, any cookies created by JavaScript properties that
        /// execute on the client-side and that start with '51D_' will 
        /// be deleted automatically.
        /// </param>
        /// <param name="updateEnabled">
        /// True to use the callback mechanism that is described in 
        /// remarks for this constructor.
        /// False to disable that mechanism. In this case, a second request 
        /// must be initiated by the user in order for the server to 
        /// access the additional evidence gathered by client-side code.
        /// </param>
        public JavaScriptResource(
            string objName,
            string jsonObject,
            bool supportsPromises,
            Uri url,
            bool enableCookies,
            bool updateEnabled)
        {
            _objName = objName;
            _jsonObject = string.IsNullOrWhiteSpace(jsonObject) == false
                ? jsonObject : "{\"errors\":[\"Json data missing.\"]}";
            _supportsPromises = supportsPromises;
            _url = url;
            _enableCookies = enableCookies;
            _updateEnabled = updateEnabled;
        }

        /// <summary>
        /// Get the parameters supplied to this class as a 
        /// <code><![CDATA[Dictionary<string, object>]]></code>
        /// </summary>
        /// <returns>
        /// A new  <code><![CDATA[Dictionary<string, object>]]></code>
        /// containing the parameters passed to the constructor.
        /// </returns>
        public Dictionary<string, object> AsDictionary()
        {
            var hash = new Dictionary<string, object>();

            hash.Add("_objName", _objName);
            hash.Add("_jsonObject", _jsonObject);
            hash.Add("_supportsPromises", _supportsPromises);
            hash.Add("_url", _url?.AbsoluteUri);
            hash.Add("_enableCookies", _enableCookies);
            hash.Add("_updateEnabled", _updateEnabled);

            return hash;
        }
    }
}