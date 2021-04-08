using FiftyOne.Pipeline.Core.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FiftyOne.Pipeline.Core.Tests.Data
{
    [TestClass]
    public class EvidenceTests
    {
        private Evidence _evidence;

        [TestInitialize]
        public void Init()
        {
            var loggerFactory = new LoggerFactory();
            _evidence = new Evidence(loggerFactory.CreateLogger<Evidence>());
        }

        [TestMethod]
        public void GetStringValues_String()
        {
            TestSingleValue("value");
        }

        [TestMethod]
        public void GetStringValues_Int()
        {
            TestSingleValue(1);
        }

        [TestMethod]
        public void GetStringValues_Complex()
        {
            TestSingleValue(new KeyValuePair<int, int>(3, 4));
        }

        [TestMethod]
        public void GetStringValues_StringList()
        {
            var value = new List<string>() 
            {
                "value1",
                "value2"
            };
            TestMultiValue(value);
        }

        [TestMethod]
        public void GetStringValues_ObjectList()
        {
            var value = new Dictionary<string, int>()
            {
                { "value1", 1 },
                { "value2", 2 }
            };
            TestMultiValue(value);
        }

        [TestMethod]
        public void GetStringValues_StringValues()
        {
            var value = new StringValues(
                new string[] {
                    "value1",
                    "value2"
                });
            TestMultiValue(value);
        }

        private void TestSingleValue(object value)
        {
            var key = "test";
            _evidence[key] = value;
            var result = _evidence.GetStringValues(key);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(value.ToString(), result.ElementAt(0));
        }

        private void TestMultiValue(IEnumerable value)
        {
            var key = "test";
            _evidence[key] = value;
            var result = _evidence.GetStringValues(key);

            var expectedCount = 0;
            var expectedValues = new List<string>();
            var enumerator = value.GetEnumerator();
            while (enumerator.MoveNext())
            {
                expectedCount++;
                expectedValues.Add(enumerator.Current.ToString());
            }

            Assert.AreEqual(expectedCount, result.Count());
            foreach(var entry in expectedValues)
            {
                Assert.IsTrue(result.Contains(entry));
            }
        }
    }
}
