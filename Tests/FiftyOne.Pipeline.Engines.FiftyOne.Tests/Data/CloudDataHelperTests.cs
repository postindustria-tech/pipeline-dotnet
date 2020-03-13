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

using FiftyOne.Common.TestHelpers;
using FiftyOne.Pipeline.Core.Data;
using FiftyOne.Pipeline.Core.Data.Types;
using FiftyOne.Pipeline.Core.FlowElements;
using FiftyOne.Pipeline.Engines.Data;
using FiftyOne.Pipeline.Engines.FiftyOne.Data;
using FiftyOne.Pipeline.Engines.FlowElements;
using FiftyOne.Pipeline.Engines.TestHelpers;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace FiftyOne.Pipeline.Engines.FiftyOne.Tests.Data
{
    [TestClass]
    public class CloudDataHelperTests
    {
        private ILoggerFactory _loggerFactory;

        private IAspectEngine _engine;

        private Mock<IPipeline> _pipeline;

        private Mock<IFlowData> _flowData;

        [TestInitialize]
        public void Initialize()
        {
            _loggerFactory = new TestLoggerFactory();
            _engine = new EmptyEngineBuilder(_loggerFactory)
                .Build();
            _pipeline = new Mock<IPipeline>();
            _flowData = new Mock<IFlowData>();
            _flowData.SetupGet(f => f.Pipeline).Returns(_pipeline.Object);
        }

        private static object NO_VALUE = "this has no value....";

        private IAspectData SetUpProperty<T>(object expectedValue)
        {
            var engineProperties = new Dictionary<string, IElementPropertyMetaData>();
            engineProperties["propertyname"] = new ElementPropertyMetaData(
                _engine,
                "propertyname",
                typeof(T),
                true);
            var availableProperties = new Dictionary<string, IReadOnlyDictionary<string, IElementPropertyMetaData>>();
            availableProperties[_engine.ElementDataKey] = engineProperties;
            _pipeline.SetupGet(p => p.ElementAvailableProperties)
                .Returns(availableProperties);
            var data = new EmptyEngineData(
                _loggerFactory.CreateLogger<EmptyEngineData>(),
                _pipeline.Object,
                _engine,
                null);
            if (expectedValue != NO_VALUE)
            {
                data["propertyname"] = expectedValue;
            }

            return data;
        }

        /// <summary>
        /// Test that a string value which exists in the internal data is
        /// successfully returned as an AspectPropertyValue.
        /// </summary>
        [TestMethod]
        public void GetAspectPropertyValue_String()
        {
            // Arrange
            var expectedValue = "expected value";
            var data = SetUpProperty<string>(expectedValue);
            
            // Act
            var result = CloudDataHelpers.TryGetAspectPropertyValue(
                data,
                null,
                "propertyname",
                out AspectPropertyValue<string> value);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(typeof(AspectPropertyValue<string>), value.GetType());
            Assert.IsTrue(value.HasValue);
            Assert.AreEqual(expectedValue, value.Value);
        }

        /// <summary>
        /// Test that an int value which exists in the internal data is
        /// successfully returned as an AspectPropertyValue.
        /// </summary>
        [TestMethod]
        public void GetAspectPropertyValue_Int()
        {
            // Arrange
            var expectedValue = 12;
            var data = SetUpProperty<int>(expectedValue);
            
            // Act
            var result = CloudDataHelpers.TryGetAspectPropertyValue(
                data,
                null,
                "propertyname",
                out AspectPropertyValue<int> value);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(typeof(AspectPropertyValue<int>), value.GetType());
            Assert.IsTrue(value.HasValue);
            Assert.AreEqual(expectedValue, value.Value);
        }

        /// <summary>
        /// Test that a double value which exists in the internal data is
        /// successfully returned as an AspectPropertyValue.
        /// </summary>
        [TestMethod]
        public void GetAspectPropertyValue_Double()
        {
            // Arrange
            var expectedValue = 12.21;
            var data = SetUpProperty<double>(expectedValue);

            // Act
            var result = CloudDataHelpers.TryGetAspectPropertyValue(
                data,
                null,
                "propertyname",
                out AspectPropertyValue<double> value);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(typeof(AspectPropertyValue<double>), value.GetType());
            Assert.IsTrue(value.HasValue);
            Assert.AreEqual(expectedValue, value.Value);
        }

        /// <summary>
        /// Test that a bool value which exists in the internal data is
        /// successfully returned as an AspectPropertyValue.
        /// </summary>
        [TestMethod]
        public void GetAspectPropertyValue_Bool()
        {
            // Arrange
            var expectedValue = true;
            var data = SetUpProperty<bool>(expectedValue);
            
            // Act
            var result = CloudDataHelpers.TryGetAspectPropertyValue(
                data,
                null,
                "propertyname",
                out AspectPropertyValue<bool> value);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(typeof(AspectPropertyValue<bool>), value.GetType());
            Assert.IsTrue(value.HasValue);
            Assert.AreEqual(expectedValue, value.Value);
        }

        /// <summary>
        /// Test that a JavaScript value which exists in the internal data is
        /// successfully returned as an AspectPropertyValue.
        /// </summary>
        [TestMethod]
        public void GetAspectPropertyValue_JavaScript()
        {
            // Arrange
            var expectedValue = new JavaScript("expected value");
            var data = SetUpProperty<JavaScript>(expectedValue);

            // Act
            var result = CloudDataHelpers.TryGetAspectPropertyValue(
                data,
                null,
                "propertyname",
                out AspectPropertyValue<JavaScript> value);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(typeof(AspectPropertyValue<JavaScript>), value.GetType());
            Assert.IsTrue(value.HasValue);
            Assert.AreEqual(expectedValue, value.Value);
        }

        /// <summary>
        /// Test that an IReadOnlyList value which exists in the internal data
        /// is successfully returned as an AspectPropertyValue.
        /// </summary>
        [TestMethod]
        public void GetAspectPropertyValue_IReadOnlyList()
        {
            // Arrange
            var expectedValue = new List<string>() { "expected value" };
            var data = SetUpProperty<IReadOnlyList<string>>(expectedValue);

            // Act
            var result = CloudDataHelpers.TryGetAspectPropertyValue(
                data,
                null,
                "propertyname",
                out AspectPropertyValue<IReadOnlyList<string>> value);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(typeof(AspectPropertyValue<IReadOnlyList<string>>), value.GetType());
            Assert.IsTrue(value.HasValue);
            Assert.AreEqual(expectedValue, value.Value);
        }

        /// <summary>
        /// Test that a value which exists in the internal data, but as the
        /// incorrect type, throws an appropriate exception.
        /// </summary>
        [TestMethod]
        public void GetAspectPropertyValue_WrongType()
        {
            // Arrange
            var intValue = 12;
            var data = SetUpProperty<string>(intValue);

            // Assert
            Assert.ThrowsException<Exception>(() =>
            {
                var result = CloudDataHelpers.TryGetAspectPropertyValue(
                data,
                null,
                "propertyname",
                out AspectPropertyValue<string> value);
            },
            $"Expected property 'propertyname' to be of type '{typeof(AspectPropertyValue<string>).Name}' but it is '{typeof(int).Name}'");
        }

        /// <summary>
        /// Test that a value which is null in the internal data returns an
        /// AspectPropertyValue with the correct reason set.
        /// </summary>
        [TestMethod]
        public void GetAspectPropertyValue_NullValue()
        {
            // Arrange
            var data = SetUpProperty<string>(null);
            var expectedReason = "expected reason";
            var noValueReasons = new Dictionary<string, string>();
            noValueReasons[$"{_engine.ElementDataKey}.propertyname"] = expectedReason;
            // Act
            var result = CloudDataHelpers.TryGetAspectPropertyValue(
                data,
                noValueReasons,
                "propertyname",
                out AspectPropertyValue<string> value);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(typeof(AspectPropertyValue<string>), value.GetType());
            Assert.IsFalse(value.HasValue);
            Assert.AreEqual(expectedReason, value.NoValueMessage);
        }

        /// <summary>
        /// Test that a value which is null in the internal data, and a reason
        /// keyed without the engine key as a prefix, returns an
        /// AspectPropertyValue with the correct reason set.
        /// </summary>
        [TestMethod]
        public void GetAspectPropertyValue_NullValueReasonWithoutPrefix()
        {
            // Arrange
            var data = SetUpProperty<string>(null);
            var expectedReason = "expected reason";
            var noValueReasons = new Dictionary<string, string>();
            noValueReasons["propertyname"] = expectedReason;
            // Act
            var result = CloudDataHelpers.TryGetAspectPropertyValue(
                data,
                noValueReasons,
                "propertyname",
                out AspectPropertyValue<string> value);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(typeof(AspectPropertyValue<string>), value.GetType());
            Assert.IsFalse(value.HasValue);
            Assert.AreEqual(expectedReason, value.NoValueMessage);
        }

        /// <summary>
        /// Test that a value which does not exist in the internal data returns
        /// false, indicating that the value is not in the results.
        /// </summary>
        [TestMethod]
        public void GetAspectPropertyValue_NoValue()
        {
            // Arrange
            var data = SetUpProperty<string>(NO_VALUE);

            // Act
            var result = CloudDataHelpers.TryGetAspectPropertyValue(
                data,
                null,
                "propertyname",
                out AspectPropertyValue<string> value);

            // Assert
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Test that a value which is null in the internal data, with no
        /// reason supplied, returns an AspectPropertyValue with a reason set.
        /// </summary>
        [TestMethod]
        public void GetAspectPropertyValue_NullValueNoReason()
        {
            // Arrange
            var data = SetUpProperty<string>(null);
            var noValueReasons = new Dictionary<string, string>();

            // Act
            var result = CloudDataHelpers.TryGetAspectPropertyValue(
                data,
                noValueReasons,
                "propertyname",
                out AspectPropertyValue<string> value);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(typeof(AspectPropertyValue<string>), value.GetType());
            Assert.IsFalse(value.HasValue);
            Assert.IsFalse(string.IsNullOrEmpty(value.NoValueMessage));
        }
    }
}
