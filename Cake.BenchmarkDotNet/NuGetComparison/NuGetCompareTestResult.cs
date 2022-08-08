using Perfolizer.Mathematics.SignificanceTesting;

namespace Cake.BenchmarkDotNet.NuGetComparison
{
    public class NuGetCompareTestResult
    {
        public string BaselineVersion { get; }
        public string BenchmarkVersion { get; }
        public string RuntimeName { get; }
        public EquivalenceTestConclusion Conclusion { get; }
        public string ClassName { get; }
        public string NamespaceName { get; }
        public string MethodName { get; }
        public double SpeedDifferencePercentage { get; }
        public double BaselineMedianSpeedNs { get; }
        public double BenchmarkMedianSpeedNs { get; }

        public NuGetCompareTestResult(
            string baselineVersion,
            string benchmarkVersion,
            string runtimeName,
            EquivalenceTestConclusion conclusion,
            string className,
            string namespaceName,
            string methodName,
            double speedDifferencePercentage,
            double baselineMedianSpeedNs,
            double benchmarkMedianSpeedNs)
        {
            BaselineVersion = baselineVersion;
            BenchmarkVersion = benchmarkVersion;
            RuntimeName = runtimeName;
            Conclusion = conclusion;
            ClassName = className;
            NamespaceName = namespaceName;
            MethodName = methodName;
            SpeedDifferencePercentage = speedDifferencePercentage;
            BaselineMedianSpeedNs = baselineMedianSpeedNs;
            BenchmarkMedianSpeedNs = benchmarkMedianSpeedNs;
        }
    }
}
