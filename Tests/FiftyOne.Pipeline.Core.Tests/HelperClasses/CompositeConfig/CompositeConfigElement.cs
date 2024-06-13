using FiftyOne.Pipeline.Core.Data;
using FiftyOne.Pipeline.Core.FlowElements;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;

namespace FiftyOne.Pipeline.Core.Tests.HelperClasses.CompositeConfig
{
    public class CompositeConfigElement : FlowElementBase<TestElementData, IElementPropertyMetaData>
    {
        public override string ElementDataKey => "compositeConfig";

        public override IEvidenceKeyFilter EvidenceKeyFilter => new EvidenceKeyFilterWhitelist(new List<string>());

        public override IList<IElementPropertyMetaData> Properties => new List<IElementPropertyMetaData>();

        public int Number { get; private set; }
        public string Text { get; private set; }

        public CompositeConfigElement(int number, string text) :
            base(new Mock<ILogger<CompositeConfigElement>>().Object)
        {
            Number = number;
            Text = text;
        }

        protected override void ProcessInternal(IFlowData data)
        {
        }

        protected override void ManagedResourcesCleanup()
        {
        }

        protected override void UnmanagedResourcesCleanup()
        {
        }
    }
}
