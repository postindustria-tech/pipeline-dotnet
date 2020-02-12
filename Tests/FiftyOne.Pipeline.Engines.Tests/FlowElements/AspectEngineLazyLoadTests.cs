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

using FiftyOne.Common.TestHelpers;
using FiftyOne.Pipeline.Core.Data;
using FiftyOne.Pipeline.Core.FlowElements;
using FiftyOne.Pipeline.Core.TypedMap;
using FiftyOne.Pipeline.Engines.Caching;
using FiftyOne.Pipeline.Engines.Configuration;
using FiftyOne.Pipeline.Engines.TestHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FiftyOne.Pipeline.Engines.Tests.FlowElements
{
    [TestClass]
    public class AspectEngineLazyLoadTests
    {
        private EmptyEngine _engine;

        private TestLoggerFactory _loggerFactory;

        private int _timeoutMS = 1000;
        private CancellationTokenSource _cancellationTokenSource = 
            new CancellationTokenSource();

        [TestInitialize]
        public void Init()
        {
            _loggerFactory = new TestLoggerFactory();
            _engine = new EmptyEngineBuilder(_loggerFactory)
                .SetLazyLoading(new LazyLoadingConfiguration(_timeoutMS, 
                    _cancellationTokenSource.Token))
                .Build();
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Check that no errors or warnings were logged.
            foreach (var logger in _loggerFactory.Loggers)
            {
                logger.AssertMaxErrors(0);
                logger.AssertMaxWarnings(0);
            }
        }

        /// <summary>
        /// Check that lazy loading works as expected:
        /// 1. The process method returns immediately.
        /// 2. Getting the value from the engine data will cause the 
        /// process to wait until processing has finished.
        /// </summary>
        [TestMethod]
        public void AspectEngineLazyLoad_Process()
        {
            // Arrange
            // Set the process time to half of the configured timeout
            _engine.SetProcessCost(TimeSpan.TicksPerMillisecond * (_timeoutMS / 2));

            // Act
            var evidence = new Dictionary<string, object>()
            {
                { "user-agent", "1234" }
            };
            var mockData = MockFlowData.CreateFromEvidence(evidence, false);
            // Use the mock flow data to populate this variable with the 
            // engine data from the call to process.
            EmptyEngineData engineData = null;
            mockData.Setup(d => d.GetOrAdd(It.IsAny<ITypedKey<EmptyEngineData>>(),
                It.IsAny<Func<IPipeline, EmptyEngineData>>()))
                .Callback((ITypedKey<EmptyEngineData> k, Func<IPipeline, EmptyEngineData> f) =>
                {
                    engineData = f(mockData.Object.Pipeline);
                })
                .Returns((ITypedKey<EmptyEngineData> k, Func<IPipeline, EmptyEngineData> f) =>
                {
                    return engineData;
                });
            var data = mockData.Object;

            // Process the data.
            _engine.Process(data);
            // Check that the task is not complete when process returns.
            bool processReturnedBeforeTaskComplete = 
                engineData.ProcessTask.IsCompleted == false;

            // Assert
            Assert.AreEqual(1, engineData.ValueOne);
            // Check that the task is now complete (because the code to get
            // the property value will wait until it is complete)
            bool valueReturnedAfterTaskComplete =
                engineData.ProcessTask.IsCompleted == true;
            Assert.IsTrue(processReturnedBeforeTaskComplete);
            Assert.IsTrue(valueReturnedAfterTaskComplete);
        }

        /// <summary>
        /// Check that accessing a lazy loaded property that
        /// takes longer than the timeout works as expected.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(TimeoutException))]
        public void AspectEngineLazyLoad_PropertyTimeout()
        {
            // Arrange
            // Set the process time to double the configured timeout
            _engine.SetProcessCost(TimeSpan.TicksPerMillisecond * (_timeoutMS * 2));

            // Act
            var evidence = new Dictionary<string, object>()
            {
                { "user-agent", "1234" }
            };
            // Use the mock flow data to populate this variable with the 
            // engine data from the call to process.
            var mockData = MockFlowData.CreateFromEvidence(evidence, false);
            EmptyEngineData engineData = null;
            mockData.Setup(d => d.GetOrAdd(It.IsAny<ITypedKey<EmptyEngineData>>(),
                It.IsAny<Func<IPipeline, EmptyEngineData>>()))
                .Callback((ITypedKey<EmptyEngineData> k, Func<IPipeline, EmptyEngineData> f) =>
                {
                    engineData = f(mockData.Object.Pipeline);
                })
                .Returns((ITypedKey<EmptyEngineData> k, Func<IPipeline, EmptyEngineData> f) =>
                {
                    return engineData;
                });
            var data = mockData.Object;

            // Process the data
            _engine.Process(data);
            // Attempt to get the value. This should cause the timeout 
            // to be triggered.
            var result = engineData.ValueOne;

            // No asserts needed. Just the ExpectedException attribute
            // on the method.
        }

        /// <summary>
        /// Check that activating the cancellation token while
        /// waiting for processing for a lazy loaded property to 
        /// complete will function as expected.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(OperationCanceledException))]
        public void AspectEngineLazyLoad_ProcessCancelled()
        {
            // Arrange
            // Set the process time to half the configured timeout
            _engine.SetProcessCost(TimeSpan.TicksPerMillisecond * (_timeoutMS / 2));

            // Act
            var evidence = new Dictionary<string, object>()
            {
                { "user-agent", "1234" }
            };
            // Use the mock flow data to populate this variable with the 
            // engine data from the call to process.
            var mockData = MockFlowData.CreateFromEvidence(evidence, false);
            EmptyEngineData engineData = null;
            mockData.Setup(d => d.GetOrAdd(It.IsAny<ITypedKey<EmptyEngineData>>(),
                It.IsAny<Func<IPipeline, EmptyEngineData>>()))
                .Callback((ITypedKey<EmptyEngineData> k, Func<IPipeline, EmptyEngineData> f) =>
                {
                    engineData = f(mockData.Object.Pipeline);
                })
                .Returns((ITypedKey<EmptyEngineData> k, Func<IPipeline, EmptyEngineData> f) =>
                {
                    return engineData;
                });
            var data = mockData.Object;

            // Process the data
            _engine.Process(data);
            // Start a new task that will wait a short time and then trigger
            // the cancellation token.
            Task.Run(() =>
            {
                Task.Delay(_timeoutMS / 10);
                _cancellationTokenSource.Cancel();
            });

            // Attempt to get the value.
            var result = engineData.ValueOne;
        }

        /// <summary>
        /// Check that throwing an exception from the engine's process method
        /// will be properly passed up through the lazy loading method.
        /// </summary>
        [TestMethod]
        public void AspectEngineLazyLoad_ProcessErrored()
        {
            // Arrange
            // Set the engine to throw an exception while processing
            var exceptionMessage = "an exception message";
            _engine.SetException(new Exception(exceptionMessage));
            // Act
            var evidence = new Dictionary<string, object>()
            {
                { "user-agent", "1234" }
            };
            // Use the mock flow data to populate this variable with the 
            // engine data from the call to process.
            var mockData = MockFlowData.CreateFromEvidence(evidence, false);
            EmptyEngineData engineData = null;
            mockData.Setup(d => d.GetOrAdd(It.IsAny<ITypedKey<EmptyEngineData>>(),
                It.IsAny<Func<IPipeline, EmptyEngineData>>()))
                .Callback((ITypedKey<EmptyEngineData> k, Func<IPipeline, EmptyEngineData> f) =>
                {
                    engineData = f(mockData.Object.Pipeline);
                })
                .Returns((ITypedKey<EmptyEngineData> k, Func<IPipeline, EmptyEngineData> f) =>
                {
                    return engineData;
                });
            var data = mockData.Object;

            // Process the data
            _engine.Process(data);


            // Attempt to get the value.
            try
            {
                var result = engineData.ValueOne;
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is Exception);
                Assert.IsNotNull(e.InnerException);
                Assert.AreEqual(
                    $"One or more errors occurred. ({exceptionMessage})",
                    e.InnerException.Message);
            }
        }

        /// <summary>
        /// Check that throwing an exception from more than one engine's
        /// process method will be properly passed up through the lazy loading
        /// method and result in an AggregateException.
        /// </summary>
        [TestMethod]
        public void AspectEngineLazyLoad_ProcessMultipleErrored()
        {
            // Arrange
            // Set the engine to throw an exception while processing
            var exceptionMessage = "an exception message";
            var engine2 = new EmptyEngineBuilder(_loggerFactory)
                .SetLazyLoading(new LazyLoadingConfiguration(_timeoutMS,
                    _cancellationTokenSource.Token))
                .Build();
            _engine.SetException(new Exception(exceptionMessage));
            engine2.SetException(new Exception(exceptionMessage));
            // Act
            var evidence = new Dictionary<string, object>()
            {
                { "user-agent", "1234" }
            };
            // Use the mock flow data to populate this variable with the 
            // engine data from the call to process.
            var mockData = MockFlowData.CreateFromEvidence(evidence, false);
            EmptyEngineData engineData = null;
            mockData.Setup(d => d.GetOrAdd(It.IsAny<ITypedKey<EmptyEngineData>>(),
                It.IsAny<Func<IPipeline, EmptyEngineData>>()))
                .Callback((ITypedKey<EmptyEngineData> k, Func<IPipeline, EmptyEngineData> f) =>
                {
                    if (engineData == null)
                    {
                        engineData = f(mockData.Object.Pipeline);
                    }
                })
                .Returns((ITypedKey<EmptyEngineData> k, Func<IPipeline, EmptyEngineData> f) =>
                {
                    return engineData;
                });
            var data = mockData.Object;

            // Process the data twice
            _engine.Process(data);
            engine2.Process(data);

            // Attempt to get the value.
            try
            {
                var result = engineData.ValueOne;
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is AggregateException);
                Assert.IsNotNull(((AggregateException)e).InnerExceptions);
                Assert.AreEqual(2, ((AggregateException)e).InnerExceptions.Count);
                Assert.AreEqual(
                    $"One or more errors occurred. ({exceptionMessage})",
                    ((AggregateException)e).InnerExceptions[0].Message);
                Assert.AreEqual(
                    $"One or more errors occurred. ({exceptionMessage})",
                    ((AggregateException)e).InnerExceptions[1].Message);
            }
        }
    }
}
