using FiftyOne.Pipeline.Engines.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;

namespace FiftyOne.Pipeline.Engines.Tests.Converters
{
    [TestClass]
    public class ConvertersTests
    {
        private static IEnumerable<object> SerializerSettings => ((IEnumerable<IContractResolver>)[
            new DefaultContractResolver(),
            new CamelCasePropertyNamesContractResolver(),
        ]).Select(x => new JsonSerializerSettings {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Converters = [
                new IPAddressValueConverter(),
                new WeightedValueConverter(),
            ],
            ContractResolver = x,
        });

        public static string DisplayNameForTestCase(MethodInfo methodInfo, object[] data)
            => $"{methodInfo.Name} ({((JsonSerializerSettings)data[0]).ContractResolver.GetType().Name})";

        [DataTestMethod]
        [DynamicData(nameof(SerializerSettings), DynamicDataDisplayName = nameof(DisplayNameForTestCase))]
        public void IPAddressCodec(JsonSerializerSettings serializerSettings)
        {
            var ip = IPAddress.Parse("8.12.234.98");

            string json = JsonConvert.SerializeObject(ip, serializerSettings);
            var newIP = JsonConvert.DeserializeObject<IPAddress>(json, serializerSettings);
            Assert.AreEqual(ip.ToString(), newIP.ToString());
        }
    }
}
