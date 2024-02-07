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

using System;
using System.Collections.Generic;
using System.Linq;

namespace Examples.CustomFlowElement.Data
{
    public static class Extensions
    {
        /// <summary>
        /// Checks if a date of birth relates to the star sign.
        /// </summary>
        /// <param name="starSign"></param>
        /// <param name="dob"></param>
        /// <returns>
        /// True if the date of birth matches the star sign, otherwise false.
        /// </returns>
        public static bool Match(this StarSign starSign, DateTime dob)
        {
            var start = new DateTime(
                dob.Year,
                starSign.Start.Month, 
                starSign.Start.Day);
            var end = new DateTime(
                dob.Year,
                starSign.End.Month,
                starSign.End.Day);
            return dob >= start && dob <= end;
        }

        /// <summary>
        /// Returns the star sign related to the date provided.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static StarSign GetStarSign(this DateTime date)
        {
            return date.GetStarSign(Constants.StarSigns);
        }

        /// <summary>
        /// Returns the star sign related to the date provided from the list
        /// of options provided.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static StarSign GetStarSign(
            this DateTime date, 
            IEnumerable<StarSign> data)
        {
            return data.Single(i => i.Match(date));
        }
    }
}
