/* *********************************************************************
 * This Original Work is copyright of 51 Degrees Mobile Experts Limited.
 * Copyright 2023 51 Degrees Mobile Experts Limited, Davidson House,
 * Forbury Square, Reading, Berkshire, United Kingdom RG1 3EU.
 *
 * This Original Work is licensed under the European Union Public Licence
 * (EUPL) v.1.2 and is subject to its terms as set out below.
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
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using FiftyOne.Pipeline.Core.Data.Types;
using Examples.CustomFlowElement.Data;
using Examples.ClientSideEvidence.Shared.Data;

namespace Examples.ClientSideEvidence.Shared
{
    //! [class]
    //! [constructor]
    public class SimpleClientSideElement : 
        FlowElementBase<IStarSignDataClientSide, IElementPropertyMetaData>
    {
        public SimpleClientSideElement(
            ILogger<FlowElementBase<IStarSignDataClientSide, IElementPropertyMetaData>> 
                logger,
            Func<IPipeline, 
                FlowElementBase<IStarSignDataClientSide, IElementPropertyMetaData>,
                IStarSignDataClientSide> elementDataFactory)
            : base(logger, elementDataFactory)
        {
        }
        //! [constructor]
       
        /// <summary>
        /// The date of birth will be stored with the key "starsign" in the 
        /// FlowData. 
        /// </summary>
        public override string ElementDataKey => "starsign";

        /// <summary>
        /// The only item of evidence needed is "date-of-birth". 
        /// </summary>
        public override IEvidenceKeyFilter EvidenceKeyFilter =>
            new EvidenceKeyFilterWhitelist(new List<string>() 
            { 
                "cookie.date-of-birth" 
            });

        public override IList<IElementPropertyMetaData> Properties =>
            new List<IElementPropertyMetaData>()
            {
                // The properties which will be returned are "starsign" and the
                // JavaScript to get the date of birth from the web browser.
                new ElementPropertyMetaData(
                    this, 
                    "starsign", 
                    typeof(string), 
                    true),
                new ElementPropertyMetaData(
                    this, 
                    "dobjavascript", 
                    typeof(JavaScript), 
                    true)
            };

        protected override void ProcessInternal(IFlowData data)
        { 
            // This is a comment that signals to the JavaScript template that
            // a call back function should replace the comment. Needed to
            // ensure the fod.complete function is always called.
            const string CALLBACK_COMMENT = 
                "// 51D replace this comment with callback function.";

            // Must be prefixed with 51D_ to indicate that it's a special
            // cookie used to persist data.
            const string COOKIE_NAME = "51D_date-of-birth";

            // When the cookies are processed by the pipeline they appear in
            // the evidence as keys prefixed with cookie.
            const string EVIDENCE_KEY =
                FiftyOne.Pipeline.Core.Constants.EVIDENCE_COOKIE_PREFIX + 
                FiftyOne.Pipeline.Core.Constants.EVIDENCE_SEPERATOR +
                COOKIE_NAME;

            var dob = DateTime.MinValue;

            // Create a new IStarSignData, and cast to StarSignData so the
            // 'setter' is available.
            var starSignData = (StarSignDataClientSide)data.GetOrAdd(
                ElementDataKey, 
                CreateElementData);

            var validDob = false;
            if (data.TryGetEvidence(EVIDENCE_KEY, out string value))
            {
                // "date-of-birth" is there, so parse it.
                validDob = DateTime.TryParse(value, out dob);
            }

            if (validDob)
            {
                // Include the source data in the response data to surface all
                // the data in the same data structure. Input evidence doesn't
                // get passed to client side data structures.
                starSignData.DateOfBirth = dob;

                // "date-of-birth" is valid, so set the star sign.
                starSignData.Name = dob.GetStarSign().Name;

                // Just the callback is needed to ensure the complete function
                // is executed.
                starSignData.DobJavaScript = null;
            }
            else
            {
                // No value for the date of birth.
                starSignData.DateOfBirth = null;

                // "date-of-birth" is not there, so set the star sign to
                // unknown.
                starSignData.Name = "Unknown";

                // Set the client side JavaScript to get the date of birth.
                starSignData.DobJavaScript = new JavaScript(
                    "var dob = window.prompt(" +
                    "'Enter your date of birth.'," +
                    "'dd/mm/yyyy');" +
                    "if (dob != null) {" +
                    $"document.cookie='{COOKIE_NAME}='+dob;" +
                    "}" + CALLBACK_COMMENT);
            }
        }

        protected override void ManagedResourcesCleanup()
        {
            // Nothing to clean up here.
        }

        protected override void UnmanagedResourcesCleanup()
        {
            // Nothing to clean up here.
        }
    }
    //! [class]
}