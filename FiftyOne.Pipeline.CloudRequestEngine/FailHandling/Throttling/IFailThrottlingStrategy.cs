using System;
using System.Collections.Generic;
using System.Text;

namespace FiftyOne.Pipeline.CloudRequestEngine.FailHandling.Throttling
{
    internal interface IFailThrottlingStrategy
    {
        void RecordFailure();
        bool MayTryNow();
    }
}
