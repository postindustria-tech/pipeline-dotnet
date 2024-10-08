using System;
using System.Collections.Generic;
using System.Text;

namespace FiftyOne.Pipeline.CloudRequestEngine.FailHandling.Throttling
{
    internal class NoRetryStrategy : IFailThrottlingStrategy
    {
        private bool _didFail = false;

        public void RecordFailure() { _didFail = true; }
        public bool MayTryNow() => !_didFail;
    }
}
