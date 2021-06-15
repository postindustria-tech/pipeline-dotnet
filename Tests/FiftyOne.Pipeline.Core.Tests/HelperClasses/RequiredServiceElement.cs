using FiftyOne.Pipeline.Core.Data;
using FiftyOne.Pipeline.Core.FlowElements;
using FiftyOne.Pipeline.Engines.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using static FiftyOne.Pipeline.Core.Tests.HelperClasses.RequiredServiceElementBuilder;

namespace FiftyOne.Pipeline.Core.Tests.HelperClasses
{
    public class RequiredServiceElement : FlowElementBase<IElementData, IElementPropertyMetaData>
    {

        public ILoggerFactory LoggerFactory { get; private set; } = null;
        public EmptyService Service { get; private set; } = null;
        public IDataUpdateService UpdateService { get; private set; } = null;

        public override string ElementDataKey => "requiredservice";

        public override IEvidenceKeyFilter EvidenceKeyFilter => new EvidenceKeyFilterWhitelist(new List<string>());

        public override IList<IElementPropertyMetaData> Properties => new List<IElementPropertyMetaData>();

        public RequiredServiceElement(
            ILoggerFactory loggerFactory,
            EmptyService service,
            IDataUpdateService updateService)
            : base(loggerFactory.CreateLogger<RequiredServiceElement>())
        {
            LoggerFactory = loggerFactory;
            Service = service;
            UpdateService = updateService;
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
