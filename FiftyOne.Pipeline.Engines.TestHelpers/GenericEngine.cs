using FiftyOne.Pipeline.Core.Data;
using FiftyOne.Pipeline.Core.FlowElements;
using FiftyOne.Pipeline.Engines.Data;
using FiftyOne.Pipeline.Engines.FlowElements;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace FiftyOne.Pipeline.Engines.TestHelpers
{
    public class GenericEngine<T> : AspectEngineBase<T, IAspectPropertyMetaData>
        where T : IAspectData
    {
        public override string DataSourceTier { get; }

        public override string ElementDataKey { get; }

        public override IEvidenceKeyFilter EvidenceKeyFilter => new EvidenceKeyFilterWhitelist(new List<string>());

        public override IList<IAspectPropertyMetaData> Properties { get; }

        public GenericEngine(
            ILogger<GenericEngine<T>> logger,
            string tier,
            string dataKey,
            IList<IAspectPropertyMetaData> properties) :
            base(logger, (p, e) => default(T))
        {
            DataSourceTier = tier;
            ElementDataKey = dataKey;
            Properties = properties;
        }

        protected override void ProcessEngine(IFlowData data, T aspectData)
        {
        }

        protected override void UnmanagedResourcesCleanup()
        {
        }
    }
}
