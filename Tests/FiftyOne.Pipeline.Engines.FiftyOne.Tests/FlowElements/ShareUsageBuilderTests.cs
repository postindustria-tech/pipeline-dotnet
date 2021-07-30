using FiftyOne.Pipeline.Engines.FiftyOne.FlowElements;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace FiftyOne.Pipeline.Engines.FiftyOne.Tests.FlowElements
{
    [TestClass]
    public class ShareUsageBuilderTests
    {
        /// <summary>
        /// An implementation of ShareUsageBuilderBase that we can
        /// use for testing.
        /// </summary>
        private class TestBuilder : ShareUsageBuilderBase<ShareUsageElement>
        {
            public TestBuilder() : base(new LoggerFactory())
            {
            }

            public override ShareUsageElement Build()
            {
                throw new NotImplementedException();
            }

            public int GetMaximumQueueSize()
            {
                return MaximumQueueSize;
            }
        }


        TestBuilder _builder = new TestBuilder();

        /// <summary>
        /// Verify that the calculation of the MaximumQueueSize 
        /// setting is correct.
        /// Also verify that specifying the MaximumQueueSize value
        /// will override the calculation.
        /// </summary>
        /// <param name="minEntries"></param>
        /// <param name="maxQueueSize"></param>
        /// <param name="expectedMaxQueueSize"></param>
        [DataTestMethod]
        [DataRow(null, null, 1000)]
        [DataRow(10, null, 1000)]
        [DataRow(100, null, 1000)]
        [DataRow(200, null, 2000)]
        [DataRow(2500, null, 25000)]
        [DataRow(null, 100, 100)]
        [DataRow(10, 100, 100)]
        [DataRow(100, 100, 100)]
        [DataRow(200, 100, 100)]
        [DataRow(2500, 100, 100)]
        [DataRow(null, 10000, 10000)]
        [DataRow(10, 10000, 10000)]
        [DataRow(100, 10000, 10000)]
        [DataRow(200, 10000, 10000)]
        [DataRow(2500, 10000, 10000)]
        public void VerifyMaximumQueueSizeCalculation(
            int? minEntries,
            int? maxQueueSize, 
            int expectedMaxQueueSize)
        {
            if (minEntries.HasValue)
            {
                _builder.SetMinimumEntriesPerMessage(minEntries.Value);
            }
            if (maxQueueSize.HasValue)
            {
                _builder.SetMaximumQueueSize(maxQueueSize.Value);
            }

            Assert.AreEqual(expectedMaxQueueSize, _builder.GetMaximumQueueSize());
        }

    }

}
