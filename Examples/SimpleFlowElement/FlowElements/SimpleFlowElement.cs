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
using SimpleFlowElement.Data;
using FiftyOne.Pipeline.Core.FlowElements;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleFlowElement.FlowElements
{
    //! [class]
    //! [constructor]
    public class SimpleFlowElement : FlowElementBase<IStarSignData, IElementPropertyMetaData>
    {
        public SimpleFlowElement(
            ILogger<FlowElementBase<IStarSignData, IElementPropertyMetaData>> logger,
            Func<IFlowData, FlowElementBase<IStarSignData, IElementPropertyMetaData>, IStarSignData> elementDataFactory)
            : base(logger, elementDataFactory)
        {
            Init();
        }
        //! [constructor]

        private IList<StarSign> _starSigns;

        private static string[][] _starSignData = {
            new string[3] {"Aries","21/03","19/04"},
            new string[3] {"Taurus","20/04","20/05"},
            new string[3] {"Gemini","21/05","20/06"},
            new string[3] {"Cancer","21/06","22/07"},
            new string[3] {"Leo","23/07","22/08"},
            new string[3] {"Virgo","23/08","22/09"},
            new string[3] {"Libra","23/09","22/10"},
            new string[3] {"Scorpio","23/10","21/11"},
            new string[3] {"Sagittarius","22/11","21/12"},
            new string[3] {"Capricorn","22/12","19/01"},
            new string[3] {"Aquarius","20/01","18/02"},
            new string[3] {"Pisces","19/02","20/03"}
        };

        //! [init]
        private void Init()
        {
            var starSigns = new List<StarSign>();
            foreach (var starSign in _starSignData)
            {
                starSigns.Add(new StarSign(
                    starSign[0],
                    DateTime.Parse(starSign[1]),
                    DateTime.Parse(starSign[2])));
            }
            _starSigns = starSigns;
        }
        //! [init]

        // The IStarSignData will be stored with the key "starsign" in the FlowData.
        public override string ElementDataKey => "starsign";

        // The only item of evidence needed is "date-of-birth".
        public override IEvidenceKeyFilter EvidenceKeyFilter =>
            new EvidenceKeyFilterWhitelist(new List<string>() { "date-of-birth" });

        public override IList<IElementPropertyMetaData> Properties =>
            new List<IElementPropertyMetaData>()
            {
                // The only property which will be returned is "starsign" which will be
                // a string.
                new ElementPropertyMetaData(this, "starsign", typeof(string), true)
            };

        protected override void ProcessInternal(IFlowData data)
        {
            // Create a new IStarSignData, and cast to StarSignData so the 'setter' is available.
            StarSignData starSignData = (StarSignData)data.GetOrAdd(ElementDataKey, CreateElementData);

            if (data.TryGetEvidence("date-of-birth", out DateTime dateOfBirth))
            {
                // "date-of-birth" is there, so set the star sign.
                var monthAndDay = new DateTime(1, dateOfBirth.Month, dateOfBirth.Day);
                foreach (var starSign in _starSigns)
                {
                    if (monthAndDay > starSign.Start &&
                        monthAndDay < starSign.End)
                    {
                        // The star sign has been found, so set it in the
                        // results.
                        starSignData.StarSign = starSign.Name;
                        break;
                    }
                }
            }
            else
            {
                // "date-of-birth" is not there, so set the star sign to unknown.
                starSignData.StarSign = "Unknown";
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
}
//! [class]
