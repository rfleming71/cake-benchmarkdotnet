using System.Collections.Generic;

namespace Cake.BenchmarkDotNet.NuGetComparison
{
    public class NuGetCompareFinalResult
    {
        public string BaselineVersion { get; }
        public string BenchmarkVersion { get; }
        public double TestThresholdPercentage { get; }
        public IEnumerable<NuGetCompareTestResult> TestResults { get; }

        public NuGetCompareFinalResult(
            string baselineVersion,
            string benchmarkVersion,
            double testThresholdPercentage,
            IEnumerable<NuGetCompareTestResult> testResults)
        {
            BaselineVersion = baselineVersion;
            BenchmarkVersion = benchmarkVersion;
            TestThresholdPercentage = testThresholdPercentage;
            TestResults = testResults;
        }
    }
}
