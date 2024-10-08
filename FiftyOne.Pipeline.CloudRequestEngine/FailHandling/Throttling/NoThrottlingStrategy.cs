namespace FiftyOne.Pipeline.CloudRequestEngine.FailHandling.Throttling
{
    internal class NoThrottlingStrategy : IFailThrottlingStrategy
    {
        public void RecordFailure() { /* nop */ }
        public bool MayTryNow() => true;
    }
}
