namespace FiftyOne.Pipeline.CloudRequestEngine.FailHandling.Throttling
{
    /// <summary>
    /// Always allows to make new server call
    /// regardless of previous failures.
    /// </summary>
    public class NoThrottlingStrategy : IFailThrottlingStrategy
    {
        /// <summary>
        /// Called when querying the server failed.
        /// </summary>
        public void RecordFailure() { /* nop */ }

        /// <summary>
        /// Whether the new request may be sent already.
        /// </summary>
        /// <returns>true -- send, false -- skip</returns>
        public bool MayTryNow() => true;
    }
}
