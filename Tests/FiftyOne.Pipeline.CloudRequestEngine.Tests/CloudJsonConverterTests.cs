using FiftyOne.Pipeline.CloudRequestEngine.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FiftyOne.Pipeline.CloudRequestEngine.Tests
{
    [TestClass]
    public class CloudJsonConverterTests
    {
        private static JsonConverter[] JSON_CONVERTERS = new JsonConverter[]
        {
            new CloudJsonConverter()
        };

        [TestMethod]
        public void Test_Simple()
        {
            string json = @"{ ""booleanProperty"": ""True"", ""stringProperty"": ""ABC"", ""listProperty"": [ ""Item1"", ""Item2"" ] }";

            var output = JsonConvert.DeserializeObject<Dictionary<string, object>>(json,
                new JsonSerializerSettings()
                {
                    Converters = JSON_CONVERTERS,
                });

            Assert.AreEqual(3, output.Count);
            Assert.IsTrue(output.Any(i => i.Key == "booleanProperty"));
            Assert.IsTrue(output.Any(i => i.Key == "stringProperty"));
            Assert.IsTrue(output.Any(i => i.Key == "listProperty"));
            Assert.AreEqual("True", output["booleanProperty"]);
            Assert.AreEqual("ABC", output["stringProperty"]);
            Assert.IsInstanceOfType(output["listProperty"], typeof(IEnumerable<string>));
            var list = output["listProperty"] as IEnumerable<string>;
            Assert.IsTrue(list.Any(i => i == "Item1"));
            Assert.IsTrue(list.Any(i => i == "Item2"));
        }
    }
}
