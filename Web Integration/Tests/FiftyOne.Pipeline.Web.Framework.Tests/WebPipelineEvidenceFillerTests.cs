using FiftyOne.Pipeline.Core.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Net.Configuration;

namespace FiftyOne.Pipeline.Web.Framework.Tests
{
    [TestClass]
    public class WebPipelineEvidenceFillerTests
    {
        private class FakeException : Exception
        {
            public FakeException(): base() { }
            public FakeException(string msg) : base(msg) { }
            public FakeException(string msg, Exception ex) : base(msg, ex) { }
        }


        [TestMethod]
        public void TestConstructor_Ok()
        {
            var filter = Substitute.For<IEvidenceKeyFilter>();
            var fakeData = Substitute.For<IFlowData>();
            fakeData.EvidenceKeyFilter.Returns(filter);
            
            var filler = new WebPipeline.EvidenceFiller(fakeData);

            _ = fakeData.Received(1).EvidenceKeyFilter;
            Assert.IsNull(filler.Errors);
        }

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))] // pass through (not explicitly handled)
        public void TestConstructor_NoData()
        {
            var filler = new WebPipeline.EvidenceFiller(null);
        }

        [TestMethod]
        public void TestConstructor_NullFilter() // ignored till later
        {
            IEvidenceKeyFilter noFilter = null;
            var fakeData = Substitute.For<IFlowData>();
            fakeData.EvidenceKeyFilter.Returns(noFilter);

            var filler = new WebPipeline.EvidenceFiller(fakeData);

            _ = fakeData.Received(1).EvidenceKeyFilter;
            Assert.IsNull(filler.Errors);
        }

        [TestMethod]
        [ExpectedException(typeof(FakeException))]
        public void TestConstructor_ErrorOnFilter()
        {
            var fakeData = Substitute.For<IFlowData>();
            fakeData.EvidenceKeyFilter.Returns(x => { throw new FakeException("no filter"); });

            var filler = new WebPipeline.EvidenceFiller(fakeData);
        }

        [TestMethod]
        public void TestCheckToAdd_Normal()
        {
            var fakeData = Substitute.For<IFlowData>();

            var filter = Substitute.For<IEvidenceKeyFilter>();
            filter.Include(Arg.Any<string>()).Returns(x => ((string)x[0]).Contains("s"));
            fakeData.EvidenceKeyFilter.Returns(filter);

            var savedData = new Dictionary<string, object>();
            fakeData
                .When(x => x.AddEvidence(Arg.Any<string>(), Arg.Any<object>()))
                .Do(x => savedData[(string)x[0]] = x[1]);

            var testData = new Dictionary<string, string>()
            {
                { "crabby", "crabs" },
                { "sleepy", "beetle" },
                { "zummy", "fish" },
                { "gloomy", "day" },
                { "fishy", "cat" },
                { "raw", "beef" },
            };


            var filler = new WebPipeline.EvidenceFiller(fakeData);
            foreach ( var data in testData )
            {
                filler.CheckAndAdd(data.Key, data.Value);
            }

            _ = fakeData.Received(1).EvidenceKeyFilter;
            Assert.IsNull(filler.Errors);
            CollectionAssert.AreEquivalent(new Dictionary<string, object>()
            {
                { "sleepy", "beetle" },
                { "fishy", "cat" },
            }, savedData);
        }
    }
}
