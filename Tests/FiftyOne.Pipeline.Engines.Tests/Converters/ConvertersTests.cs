using FiftyOne.Pipeline.Core.Data;
using FiftyOne.Pipeline.Engines.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
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

        [DataTestMethod]
        [DynamicData(nameof(SerializerSettings), DynamicDataDisplayName = nameof(DisplayNameForTestCase))]
        public void WeightedIPAddressCodec(JsonSerializerSettings serializerSettings)
        {
            var ip = IPAddress.Parse("8.12.234.98");
            var w = new WeightedValue<IPAddress>(15672, ip);

            string json = JsonConvert.SerializeObject(w, serializerSettings);
            var newW = JsonConvert.DeserializeObject<IWeightedValue<IPAddress>>(json, serializerSettings);
            Assert.AreEqual(ip.ToString(), newW.Value.ToString());
            Assert.AreEqual(w.RawWeighting, newW.RawWeighting);
        }

        [DataTestMethod]
        [DynamicData(nameof(SerializerSettings), DynamicDataDisplayName = nameof(DisplayNameForTestCase))]
        public void WeightedStringCodec(JsonSerializerSettings serializerSettings)
        {
            var w = new WeightedValue<string>(15672, "qwerty567xyz");

            string json = JsonConvert.SerializeObject(w, serializerSettings);
            var newW = JsonConvert.DeserializeObject<IWeightedValue<string>>(json, serializerSettings);
            Assert.AreEqual(w.Value, newW.Value);
            Assert.AreEqual(w.RawWeighting, newW.RawWeighting);
        }

        [DataTestMethod]
        [DynamicData(nameof(SerializerSettings), DynamicDataDisplayName = nameof(DisplayNameForTestCase))]
        public void WeightedIntCodec(JsonSerializerSettings serializerSettings)
        {
            var w = new WeightedValue<int>(15672, 42);

            string json = JsonConvert.SerializeObject(w, serializerSettings);
            var newW = JsonConvert.DeserializeObject<IWeightedValue<int>>(json, serializerSettings);
            Assert.AreEqual(w.Value, newW.Value);
            Assert.AreEqual(w.RawWeighting, newW.RawWeighting);
        }

        [DataTestMethod]
        [DynamicData(nameof(SerializerSettings), DynamicDataDisplayName = nameof(DisplayNameForTestCase))]
        public void WeightedWeightedIntCodec(JsonSerializerSettings serializerSettings)
        {
            var w = new WeightedValue<WeightedValue<int>>(13, new(ushort.MaxValue, -29));

            string json = JsonConvert.SerializeObject(w, serializerSettings);
            var newW = JsonConvert.DeserializeObject<IWeightedValue<IWeightedValue<int>>>(json, serializerSettings);
            Assert.AreEqual(w.Value.Value, newW.Value.Value);
            Assert.AreEqual(w.Value.RawWeighting, newW.Value.RawWeighting);
            Assert.AreEqual(w.RawWeighting, newW.RawWeighting);
        }

        [DataTestMethod]
        [DynamicData(nameof(SerializerSettings), DynamicDataDisplayName = nameof(DisplayNameForTestCase))]
        public void WeightedFloatCodec(JsonSerializerSettings serializerSettings)
        {
            var w = new WeightedValue<float>(15672, (float)Math.PI);

            string json = JsonConvert.SerializeObject(w, serializerSettings);
            var newW = JsonConvert.DeserializeObject<IWeightedValue<float>>(json, serializerSettings);
            Assert.AreEqual(w.Value, newW.Value, 1e-8f);
            Assert.AreEqual(w.RawWeighting, newW.RawWeighting);
        }
    }
}
