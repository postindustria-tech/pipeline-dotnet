namespace FiftyOne.Pipeline.CloudRequestEngine.FailHandling.Throttling
{
    /// <summary>
    /// Controls when to suspend requests
    /// due to recent query failures.
    /// </summary>
    public interface IFailThrottlingStrategy
    {
        /// <summary>
        /// Called when querying the server failed.
        /// </summary>
        void RecordFailure();

        /// <summary>
        /// Whether the new request may be sent already.
        /// </summary>
        /// <returns>true -- send, false -- skip</returns>
        bool MayTryNow();
    }
}
