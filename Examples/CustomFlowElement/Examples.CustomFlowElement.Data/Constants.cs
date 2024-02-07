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

using System.Collections.Generic;

namespace Examples.CustomFlowElement.Data
{
    public static class Constants
    {
        /// <summary>
        /// A list of all the available star signs and date ranges. Capricorn
        /// is included twice to handle year end.
        /// </summary>
        public static readonly IReadOnlyList<StarSign> StarSigns = new [] {
            new StarSign("Aries", "21/03","19/04"),
            new StarSign("Taurus","20/04","20/05"),
            new StarSign("Gemini","21/05","20/06"),
            new StarSign("Cancer","21/06","22/07"),
            new StarSign("Leo","23/07","22/08"),
            new StarSign("Virgo","23/08","22/09"),
            new StarSign("Libra","23/09","22/10"),
            new StarSign("Scorpio","23/10","21/11"),
            new StarSign("Sagittarius","22/11","21/12"),
            // Capricorn is provided twice to handle year end.
            new StarSign("Capricorn","22/12","31/12"),
            new StarSign("Capricorn","01/01","19/01"),
            new StarSign("Aquarius","20/01","18/02"),
            new StarSign("Pisces","19/02","20/03")
        };
    }
}
