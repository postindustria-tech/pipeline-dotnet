using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FiftyOne.Pipeline.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            var report = BenchmarkRunner.Run<AccessEvidenceBenchmarks>(
                ManualConfig.Create(DefaultConfig.Instance)
                    .WithOptions(ConfigOptions.DisableOptimizationsValidator));

            bool fail = false;
            List<string> failDetail = new List<string>();
            List<string> failedCategories = new List<string>();

            Dictionary<string, int> maxTimes = new Dictionary<string, int>()
            {
                { "Accessor-FlowData", 300 },
                { "Accessor-AsDictionary", 400 },
                { "Accessor-StringValues", 800 }
            };

            foreach (var category in maxTimes)
            {
                var reportsForCategory = report.Reports.Where(r =>
                       r.BenchmarkCase.Descriptor.HasCategory(category.Key));

                if (reportsForCategory.Any(r => r.ResultStatistics.Percentiles.P95 > category.Value))
                {
                    fail = true;
                    var failedReports = reportsForCategory.Where(r => r.ResultStatistics.Percentiles.P95 > category.Value).ToList();
                    var names = string.Join(", ", failedReports.Select(s => s.BenchmarkCase.Descriptor.WorkloadMethod.Name));
                    var times = string.Join(", ", failedReports.Select(s => s.ResultStatistics.Percentiles.P95.ToString("0.00")));
                    failDetail.Add($"{names} were over the limit of " +
                        $"{category.Value}ns at the 95th percentile. " +
                        $"Actual times: {times}. See test output for full report.");
                    failedCategories.Add(category.Key);
                }
            }

            if (fail)
            {
                // write out the full benchmark report.
                Console.Write(report);
                throw new Exception($"Benchmarks outside limits for the following " +
                    $"categories: {string.Join(", ", failedCategories)}.\r\n\t" +
                    string.Join("\r\n\t", failDetail));
            }
        }
    }
}
