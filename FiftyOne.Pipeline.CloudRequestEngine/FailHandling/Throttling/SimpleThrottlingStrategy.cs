using System;
using System.Collections.Generic;
using System.Text;

namespace FiftyOne.Pipeline.CloudRequestEngine.FailHandling.Throttling
{
    /// <summary>
    /// Disallows calling the server for
    /// <see cref="RecoveryMilliseconds"/>.
    /// </summary>
    public class SimpleThrottlingStrategy : IFailThrottlingStrategy
    {
        /// <summary>
        /// For how long to disallow server calls after failure.
        /// </summary>
        public readonly double RecoveryMilliseconds;

        private DateTime _recoveryDateTime = DateTime.MinValue;
        private readonly object _lock = new object();

        /// <summary>
        /// Designated constructor.
        /// </summary>
        /// <param name="recoveryMilliseconds">
        /// For how long to disallow server calls after failure.
        /// </param>
        public SimpleThrottlingStrategy(double recoveryMilliseconds)
        {
            RecoveryMilliseconds = recoveryMilliseconds;
        }

        /// <summary>
        /// Called when querying the server failed.
        /// </summary>
        public void RecordFailure()
        {
            var newRecoveryTime = DateTime.Now.AddMilliseconds(RecoveryMilliseconds);
            lock (_lock)
            {
                _recoveryDateTime = newRecoveryTime;
            }
        }

        /// <summary>
        /// Whether the new request may be sent already.
        /// </summary>
        /// <returns>true -- send, false -- skip</returns>
        public bool MayTryNow()
        {
            DateTime recoveryDateTime;
            lock (_lock)
            {
                recoveryDateTime = _recoveryDateTime;
            }
            return recoveryDateTime < DateTime.Now;
        }
    }
}
