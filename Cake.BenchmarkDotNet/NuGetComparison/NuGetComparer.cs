using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CsvHelper;
using Perfolizer.Mathematics.SignificanceTesting;

namespace Cake.BenchmarkDotNet.NuGetComparison
{
    public class NuGetComparer
    {
        private static readonly Regex testClassNameRegex = new Regex(@"-report.[Cc][Ss][Vv]$", RegexOptions.Compiled);

        private readonly double _testThresholdPercentage;

        public NuGetComparer(double testThresholdPercentage)
        {
            _testThresholdPercentage = testThresholdPercentage;
        }

        public NuGetCompareFinalResult Compare(string benchmarkDotNetArtifactsDirectory, IEnumerable<string> filterPatterns)
        {
            var compareTestResults = new List<NuGetCompareTestResult>();
            var filters = filterPatterns.Select(pattern => new Regex(WildcardToRegex(pattern), RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)).ToArray();

            var runtimeDirectories = GetRuntimesDirectories(benchmarkDotNetArtifactsDirectory);
            foreach (var runtimeDirectory in runtimeDirectories)
            {
                var runtimeName = new DirectoryInfo(runtimeDirectory).Name;
                var csvReportFilePaths = GetCsvReportFilePaths(runtimeDirectory);
                foreach (var csvReportFilePath in csvReportFilePaths)
                {
                    var testClassName = testClassNameRegex.Replace(new FileInfo(csvReportFilePath).Name, string.Empty);

                    using var csvReportFileStreamReader = new StreamReader(csvReportFilePath);
                    using var csvReader = new CsvReader(csvReportFileStreamReader, CultureInfo.InvariantCulture);
                    csvReader.Context.RegisterClassMap<CsvReportRecordClassMap>();

                    var csvReportRecordsGroups = csvReader.GetRecords<CsvReportRecord>()
                        .Where(record => !filters.Any() || filters.Any(filter => filter.IsMatch(testClassName)))
                        .GroupBy(record =>
                            !string.IsNullOrWhiteSpace(record.MethodNameSuffix)
                            && record.MethodNameSuffix != "?"
                            ? $"{record.Method}[{record.MethodNameSuffix}]"
                            : record.Method)
                        .ToList();

                    foreach (var csvReportRecordsGroup in csvReportRecordsGroups)
                    {
                        compareTestResults.Add(
                            CompareBenchmarkToBaseline(
                                testClassName,
                                runtimeName,
                                _testThresholdPercentage,
                                csvReportRecordsGroup));
                    }
                }
            }
            
            return new NuGetCompareFinalResult(
                compareTestResults?[0].BaselineVersion ?? "Unknown",
                compareTestResults?[0].BenchmarkVersion ?? "Unknown",
                compareTestResults);
        }

        // https://stackoverflow.com/a/6907849/5852046 not perfect but should work for all we need
        private static string WildcardToRegex(string pattern) => $"^{Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", ".")}$";

        private static string[] GetRuntimesDirectories(string benchmarkDotNetArtifactsDirectory) =>
            Directory.GetDirectories(Path.GetFullPath(benchmarkDotNetArtifactsDirectory));

        private static IEnumerable<string> GetCsvReportFilePaths(string runtimeDirectory) =>
            Directory.GetFiles(Path.Combine(runtimeDirectory, "results"))
                .Where(filePath => filePath.ToLower().EndsWith(".csv"));

        private static NuGetCompareTestResult CompareBenchmarkToBaseline(
            string testClassName,
            string runtimeName,
            double testThresholdPercentage,
            IGrouping<string, CsvReportRecord> csvReportRecordsGroup)
        {
            var baselineRecord = csvReportRecordsGroup.Single(record => record.Job == "BASELINE");
            var benchmarkRecord = csvReportRecordsGroup.Single(record => record.Job == "BENCHMARK");

            var classNameSeparatorIndex = testClassName.LastIndexOf(".");
            var namespaceName = testClassName.Substring(0, classNameSeparatorIndex);
            var className = testClassName.Substring(classNameSeparatorIndex + 1);
            var methodName = csvReportRecordsGroup.Key;

            var speedDifferencePercentage = benchmarkRecord.Ratio != CsvReportRecord.ValueNotAvailable
                ? double.Parse(benchmarkRecord.Ratio.Contains("%") ? benchmarkRecord.Ratio.TrimEnd('%') : benchmarkRecord.Ratio) / 100
                : double.NaN;
            var baselineMedianSpeedNs = baselineRecord.Median != CsvReportRecord.ValueNotAvailable
                ? double.Parse(baselineRecord.Median.Contains(" ") ? baselineRecord.Median.Split(" ").First() : baselineRecord.Median)
                : double.NaN;
            var benchmarkMedianSpeedNs = benchmarkRecord.Median != CsvReportRecord.ValueNotAvailable
                ? double.Parse(benchmarkRecord.Median.Contains(" ") ? benchmarkRecord.Median.Split(" ").First() : benchmarkRecord.Median)
                : double.NaN;

            var testThresholdPercentageAsRawValue = testThresholdPercentage / 100;

            var conclusion = speedDifferencePercentage != double.NaN
                ? Math.Abs(speedDifferencePercentage) <= testThresholdPercentageAsRawValue
                    ? EquivalenceTestConclusion.Same
                    : speedDifferencePercentage > 0
                        ? EquivalenceTestConclusion.Slower
                        : speedDifferencePercentage < 0
                            ? EquivalenceTestConclusion.Faster
                            : EquivalenceTestConclusion.Same
                : EquivalenceTestConclusion.Unknown;

            return new NuGetCompareTestResult(
                baselineRecord.NuGetReferences,
                benchmarkRecord.NuGetReferences,
                runtimeName,
                conclusion,
                className,
                namespaceName,
                methodName,
                speedDifferencePercentage,
                baselineMedianSpeedNs,
                benchmarkMedianSpeedNs);
        }
    }
}
