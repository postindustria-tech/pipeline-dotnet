namespace FiftyOne.Pipeline.CloudRequestEngine.FailHandling.Throttling
{
    /// <summary>
    /// Drops all server calls after first failure.
    /// </summary>
    public class NoRetryStrategy : IFailThrottlingStrategy
    {
        private bool _didFail = false;

        /// <summary>
        /// Called when querying the server failed.
        /// </summary>
        public void RecordFailure() { _didFail = true; }

        /// <summary>
        /// Whether the new request may be sent already.
        /// </summary>
        /// <returns>true -- send, false -- skip</returns>
        public bool MayTryNow() => !_didFail;
    }
}
