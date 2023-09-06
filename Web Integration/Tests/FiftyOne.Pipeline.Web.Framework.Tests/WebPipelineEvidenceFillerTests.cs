using FiftyOne.Pipeline.Core.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using System;

namespace FiftyOne.Pipeline.Web.Framework.Tests
{
    [TestClass]
    public class WebPipelineEvidenceFillerTests
    {
        [TestMethod]
        public void TestConstructor()
        {
            var filter = Substitute.For<IEvidenceKeyFilter>();
            var fakeData = Substitute.For<IFlowData>();
            fakeData.EvidenceKeyFilter.Returns(filter);
            
            var filler = new WebPipeline.EvidenceFiller(fakeData);

            _ = fakeData.Received(1).EvidenceKeyFilter;
            Assert.IsNull(filler.Errors);
        }
    }
}
