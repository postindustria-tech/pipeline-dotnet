using BenchmarkDotNet.Attributes;
using FiftyOne.Pipeline.Engines.TestHelpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace FiftyOne.Pipeline.Benchmarks
{
    //[MemoryDiagnoser]
    public class AccessEvidenceBenchmarks
    {
        private TestFlowData _flowData;
        private TestFlowData _flowDataLargeDict;
        private TestFlowData _flowDataStringValues;

        public AccessEvidenceBenchmarks()
        {
            _flowData = new TestFlowData();
            _flowData.AddEvidence("test", "value");

            _flowDataLargeDict = new TestFlowData();
            for(int i = 0; i < 100; i++)
            {
                _flowDataLargeDict.AddEvidence($"test{i}", $"value{i}");
            }

            _flowDataStringValues = new TestFlowData();
            _flowDataStringValues.AddEvidence("test", new StringValues(new string[] { "value1", "value2", "value3" }));
            for (int i = 0; i < 100; i++)
            {
                _flowDataStringValues.AddEvidence($"test{i}", $"value{i}");
            }
        }

        [Benchmark]
        [BenchmarkCategory("Accessor-FlowData")]
        public void AccessEvidence_FlowData_TryGetEvidence()
        {
            _flowData.TryGetEvidence("test", out string value);
        }

        [Benchmark]
        [BenchmarkCategory("Accessor-AsDictionary")]
        public void AccessEvidence_AsDictionary_ToString()
        {
            var value = _flowData.GetEvidence().AsDictionary()["test"].ToString();
        }

        [Benchmark]
        [BenchmarkCategory("Accessor-FlowData")]
        public void AccessEvidence_FlowData_TryGetEvidence_NoEntry()
        {
            _flowData.TryGetEvidence("doesnotexist", out string value);
        }

        [Benchmark]
        [BenchmarkCategory("Accessor-FlowData")]
        public void AccessEvidenceLargeDict_FlowData_TryGetEvidence()
        {
            _flowDataLargeDict.TryGetEvidence("test50", out string value);
        }

        [Benchmark]
        [BenchmarkCategory("Accessor-AsDictionary")]
        public void AccessEvidenceLargeDict_AsDictionary_ToString()
        {
            var value = _flowDataLargeDict.GetEvidence().AsDictionary()["test50"].ToString();
        }

        [Benchmark]
        [BenchmarkCategory("Accessor-FlowData")]
        public void AccessEvidenceStringValues_FlowData_TryGetEvidence_WrongType()
        {
            _flowDataStringValues.TryGetEvidence("test", out string value);
        }

        [Benchmark]
        [BenchmarkCategory("Accessor-FlowData")]
        public void AccessEvidenceStringValues_FlowData_TryGetEvidence()
        {
            _flowDataStringValues.TryGetEvidence("test", out StringValues value);
        }

        [Benchmark]
        [BenchmarkCategory("Accessor-AsDictionary")]
        public void AccessEvidenceStringValues_AsDictionary_ToString()
        {
            var value = _flowDataStringValues.GetEvidence().AsDictionary()["test"].ToString();
        }
    }
}
