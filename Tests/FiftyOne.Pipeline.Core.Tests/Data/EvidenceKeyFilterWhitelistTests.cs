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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FiftyOne.Pipeline.Core.Tests.Data
{
    [TestClass]
    public class EvidenceKeyFilterWhitelistTests
    {
        private EvidenceKeyFilterWhitelist _filter;

        [TestMethod]
        /// <summary>
        /// Check that the Include method works as expected for a simple 
        /// white list filter.
        /// </summary>
        public void EvidenceKeyFilterWhitelist_Include_CheckKeys()
        {
            _filter = new EvidenceKeyFilterWhitelist(new List<string>() { "key1", "key2" });

            Assert.IsTrue(_filter.Include("key1"));
            Assert.IsTrue(_filter.Include("key2"));
            Assert.IsFalse(_filter.Include("key3"));
        }

        [TestMethod]
        /// <summary>
        /// Check that the List property works as expected for a simple
        /// white list filter.
        /// </summary>
        public void EvidenceKeyFilterWhitelist_List()
        {
            _filter = new EvidenceKeyFilterWhitelist(new List<string>() { "key1", "key2" });

            var result = _filter.Whitelist;

            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.Keys.Contains("key1"));
            Assert.IsTrue(result.Keys.Contains("key2"));
        }
    }
}
