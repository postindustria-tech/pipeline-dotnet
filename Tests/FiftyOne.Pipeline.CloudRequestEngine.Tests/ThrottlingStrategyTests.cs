
using FiftyOne.Pipeline.CloudRequestEngine.FailHandling.Throttling;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

namespace FiftyOne.Pipeline.CloudRequestEngine.Tests
{
    [TestClass]
    public class ThrottlingStrategyTests
    {
        #region NoThrottlingStrategy

        [TestMethod]
        public void NoThrottlingStrategyShouldReturnTrue()
        {
            IFailThrottlingStrategy strategy = new NoThrottlingStrategy();

            Assert.IsTrue(strategy.MayTryNow(), 
                $"{nameof(NoThrottlingStrategy)}.{nameof(IFailThrottlingStrategy.MayTryNow)}"
                + " should return true.");
        }

        [TestMethod]
        public void NoThrottlingStrategyShouldReturnTrueAfterFailure()
        {
            IFailThrottlingStrategy strategy = new NoThrottlingStrategy();

            strategy.RecordFailure();

            Assert.IsTrue(strategy.MayTryNow(),
                $"{nameof(NoThrottlingStrategy)}.{nameof(IFailThrottlingStrategy.MayTryNow)}"
                + " should return true.");
        }

        #endregion

        #region NoRetryStrategy

        [TestMethod]
        public void NoRetryStrategyShouldReturnTrue()
        {
            IFailThrottlingStrategy strategy = new NoRetryStrategy();

            Assert.IsTrue(strategy.MayTryNow(),
                $"{nameof(NoRetryStrategy)}.{nameof(IFailThrottlingStrategy.MayTryNow)}"
                + " should return true.");
        }

        [TestMethod]
        public void NoRetryStrategyShouldReturnTrueAfterFailure()
        {
            IFailThrottlingStrategy strategy = new NoRetryStrategy();

            strategy.RecordFailure();

            Assert.IsFalse(strategy.MayTryNow(),
                $"{nameof(NoRetryStrategy)}.{nameof(IFailThrottlingStrategy.MayTryNow)}"
                + " should return false.");
        }

        #endregion

        #region SimpleThrottlingStrategy

        [TestMethod]
        public void SimpleThrottlingStrategyShouldReturnTrue()
        {
            IFailThrottlingStrategy strategy 
                = new SimpleThrottlingStrategy(recoveryMilliseconds: 3000);

            Assert.IsTrue(strategy.MayTryNow(),
                $"{nameof(SimpleThrottlingStrategy)}.{nameof(IFailThrottlingStrategy.MayTryNow)}"
                + " should return true.");
        }

        [TestMethod]
        public void SimpleThrottlingStrategyShouldReturnFalseAfterFailure()
        {
            IFailThrottlingStrategy strategy 
                = new SimpleThrottlingStrategy(recoveryMilliseconds: 5000);

            strategy.RecordFailure();

            Assert.IsFalse(strategy.MayTryNow(),
                $"{nameof(SimpleThrottlingStrategy)}.{nameof(IFailThrottlingStrategy.MayTryNow)}"
                + " should return false.");
        }

        [TestMethod]
        public void SimpleThrottlingStrategyShouldReturnTrueAfterRecovery()
        {
            IFailThrottlingStrategy strategy 
                = new SimpleThrottlingStrategy(recoveryMilliseconds: 100);

            strategy.RecordFailure();

            Assert.IsFalse(strategy.MayTryNow(),
                $"{nameof(SimpleThrottlingStrategy)}.{nameof(IFailThrottlingStrategy.MayTryNow)}"
                + " should return false before failure.");

            Thread.Sleep(millisecondsTimeout: 200);

            Assert.IsTrue(strategy.MayTryNow(),
                $"{nameof(SimpleThrottlingStrategy)}.{nameof(IFailThrottlingStrategy.MayTryNow)}"
                + " should return true after recovery.");
        }

        #endregion
    }
}
