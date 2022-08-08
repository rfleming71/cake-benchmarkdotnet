using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Cake.BenchmarkDotNet.Dto;
using Cake.BenchmarkDotNet.NuGetComparison;
using MarkdownLog;
using Perfolizer.Mathematics.SignificanceTesting;

namespace Cake.BenchmarkDotNet.Printers.Html
{
    public class HtmlPrinter : IPrinter
    {
        public void Print(IEnumerable<CompareResult> results, string outputPath)
        {
            if (outputPath == null)
                return;

            var runtimes = results
                .GroupBy(x => x.Runtime)
                .ToArray();

            var report = runtimes
                .Select(x => new
                {
                    Runtime = $"[{x.Key}]({HttpUtility.UrlPathEncode(x.Key)}/index.html)",
                    Slower = x.Count(t => t.Conclusion == EquivalenceTestConclusion.Slower),
                    Same = x.Count(t => t.Conclusion == EquivalenceTestConclusion.Same),
                    Faster = x.Count(t => t.Conclusion == EquivalenceTestConclusion.Faster),
                    Errors = x.Count(t => t.Conclusion == EquivalenceTestConclusion.Unknown),
                })
                .OrderBy(x => x.Runtime)
                .ToMarkdownTable()
                .WithHeaders("Runtime", "Slower", "Same", "Faster", "Errors")
                .ToHtml();

            File.WriteAllText(Path.Combine(outputPath, "index.html"), string.Format(Template, "./", report));
            
            foreach (var runtime in runtimes)
            {
                var testGroups = runtime
                    .GroupBy(x => $"{x.BaseResult.Namespace}.{x.BaseResult.Type}")
                    .ToArray();

                report = testGroups
                    .Select(x => new
                    {
                        Class = $"[{x.Key}]({x.Key}.html)",
                        Slower = x.Count(t => t.Conclusion == EquivalenceTestConclusion.Slower),
                        Same = x.Count(t => t.Conclusion == EquivalenceTestConclusion.Same),
                        Faster = x.Count(t => t.Conclusion == EquivalenceTestConclusion.Faster),
                        Errors = x.Count(t => t.Conclusion == EquivalenceTestConclusion.Unknown),
                    })
                    .OrderBy(x => x.Class)
                    .ToMarkdownTable()
                    .WithHeaders("Class", "Slower", "Same", "Faster", "Errors")
                    .ToHtml();

                var runtimePath = Path.Combine(outputPath, runtime.Key);
                Directory.CreateDirectory(runtimePath);
                File.WriteAllText(Path.Combine(runtimePath, "index.html"), string.Format(Template, "../", report));

                foreach (var tests in testGroups)
                {
                    report = tests
                        .OrderByDescending(x => x.Conclusion)
                        .Select(x => new
                        {
                            x.Conclusion,
                            x.BaseResult.Namespace,
                            TestName = $"{x.BaseResult.Type}.{x.BaseResult.Method}",
                            Ratio = GetRatio(x.Conclusion, x.BaseResult, x.DiffResult),
                            BaseMedian = $"{x.BaseResult.Statistics.Median}",
                            DiffMedian = $"{x.DiffResult.Statistics.Median}",
                        })
                        .OrderByDescending(x => x.Conclusion)
                        .ThenBy(x => x.TestName)
                        .ToMarkdownTable()
                        .WithHeaders("Conclusion", "Namespace", "Test Name", "Speed Difference", "Base Median (ns)", "Diff Median (ns)")
                        .ToHtml();

                    File.WriteAllText(Path.Combine(runtimePath, $"{tests.Key}.html"), string.Format(Template, "../", report));
                }
            }
        }

        public void Print(NuGetCompareFinalResult nuGetCompareFinalResult, string outputPath)
        {
            if (outputPath == null)
                return;

            var runtimes = nuGetCompareFinalResult.TestResults
                .GroupBy(x => x.RuntimeName)
                .ToArray();

            var runtimesSummaryTable = runtimes
                .Select(x => new
                {
                    Runtime = $"[{x.Key}]({HttpUtility.UrlPathEncode(x.Key)}/index.html)",
                    Slower = x.Count(t => t.Conclusion == EquivalenceTestConclusion.Slower),
                    Same = x.Count(t => t.Conclusion == EquivalenceTestConclusion.Same),
                    Faster = x.Count(t => t.Conclusion == EquivalenceTestConclusion.Faster),
                    Errors = x.Count(t => t.Conclusion == EquivalenceTestConclusion.Unknown),
                })
                .OrderByDescending(x => x.Errors)
                .ThenByDescending(x => x.Slower)
                .ThenBy(x => x.Runtime)
                .ToMarkdownTable()
                .WithHeaders("Runtime", "Slower", "Same", "Faster", "Errors")
                .ToHtml();

            var runtimesSummaryReportBody = @$"
                {GetHtmlPageHeader(nuGetCompareFinalResult.BaselineVersion, nuGetCompareFinalResult.BenchmarkVersion)}
                {runtimesSummaryTable}
            ";

            File.WriteAllText(Path.Combine(outputPath, "index.html"), string.Format(Template, "./", runtimesSummaryReportBody));

            foreach (var runtime in runtimes)
            {
                var testGroups = runtime
                    .GroupBy(x => $"{x.NamespaceName}.{x.ClassName}")
                    .ToArray();

                var testClassesSummaryTable = testGroups
                    .Select(x => new
                    {
                        Class = $"[{x.Key}]({HttpUtility.UrlPathEncode(x.Key)}.html)",
                        Slower = x.Count(t => t.Conclusion == EquivalenceTestConclusion.Slower),
                        Same = x.Count(t => t.Conclusion == EquivalenceTestConclusion.Same),
                        Faster = x.Count(t => t.Conclusion == EquivalenceTestConclusion.Faster),
                        Errors = x.Count(t => t.Conclusion == EquivalenceTestConclusion.Unknown),
                    })
                    .OrderByDescending(x => x.Errors)
                    .ThenByDescending(x => x.Slower)
                    .ThenBy(x => x.Class)
                    .ToMarkdownTable()
                    .WithHeaders("Class", "Slower", "Same", "Faster", "Errors")
                    .ToHtml();

                var testClassesSummaryReportBody = @$"
                    {GetHtmlPageHeader(nuGetCompareFinalResult.BaselineVersion, nuGetCompareFinalResult.BenchmarkVersion)}
                    <p><b>Runtime:</b> {runtime.Key}</p>
                    {testClassesSummaryTable}
                ";

                var runtimePath = Path.Combine(outputPath, runtime.Key);
                Directory.CreateDirectory(runtimePath);
                File.WriteAllText(Path.Combine(runtimePath, "index.html"), string.Format(Template, "../", testClassesSummaryReportBody));

                foreach (var tests in testGroups)
                {
                    var slowerTestsTable = tests
                        .Where(x => x.Conclusion == EquivalenceTestConclusion.Slower)
                        .OrderByDescending(x => x.SpeedDifferencePercentage)
                        .ThenBy(x => x.MethodName)
                        .Select(x => new
                        {
                            TestName = x.MethodName,
                            Ratio = $"{Math.Round(Math.Abs(x.SpeedDifferencePercentage) * 100, 2)}%",
                            BaseMedian = x.BaselineMedianSpeedNs,
                            DiffMedian = x.BenchmarkMedianSpeedNs,
                        })
                        .ToMarkdownTable()
                        .WithHeaders("Test Name", "% Slower", "Base Median (ns)", "Benchmark Median (ns)")
                        .ToHtml();

                    var fasterTestsTable = tests
                        .Where(x => x.Conclusion == EquivalenceTestConclusion.Faster)
                        .OrderBy(x => x.SpeedDifferencePercentage)
                        .ThenBy(x => x.MethodName)
                        .Select(x => new
                        {
                            TestName = x.MethodName,
                            Ratio = $"{Math.Round(Math.Abs(x.SpeedDifferencePercentage) * 100, 2)}%",
                            BaseMedian = x.BaselineMedianSpeedNs,
                            DiffMedian = x.BenchmarkMedianSpeedNs,
                        })
                        .ToMarkdownTable()
                        .WithHeaders("Test Name", "% Faster", "Base Median (ns)", "Benchmark Median (ns)")
                        .ToHtml();

                    var otherTestsTable = tests
                        .Where(x => x.Conclusion != EquivalenceTestConclusion.Slower && x.Conclusion != EquivalenceTestConclusion.Faster)
                        .OrderByDescending(x => x.SpeedDifferencePercentage)
                        .ThenBy(x => x.MethodName)
                        .Select(x => new
                        {
                            Conclusion = x.Conclusion == EquivalenceTestConclusion.Same ? $"{EquivalenceTestConclusion.Same} (within threshold)" : x.Conclusion.ToString(),
                            TestName = x.MethodName,
                            Ratio = $"{Math.Round(Math.Abs(x.SpeedDifferencePercentage) * 100, 2)}% {(x.SpeedDifferencePercentage < 0 ? "faster" : "slower" )}",
                            BaseMedian = x.BaselineMedianSpeedNs,
                            DiffMedian = x.BenchmarkMedianSpeedNs,
                        })
                        .ToMarkdownTable()
                        .WithHeaders("Conclusion", "Test Name", "% Difference", "Base Median (ns)", "Benchmark Median (ns)")
                        .ToHtml();

                    var testClassSummaryReportBody = @$"
                        {GetHtmlPageHeader(nuGetCompareFinalResult.BaselineVersion, nuGetCompareFinalResult.BenchmarkVersion)}
                        <p><b>Runtime:</b> {runtime.Key} &#8594 <b>TestClass:</b> {tests.Key}</p>
                        <h2>Slower Tests (focus on these)</h2>
                        {slowerTestsTable}
                        <br />
                        <h2>Faster Tests</h2>
                        {fasterTestsTable}
                        <br />
                        <h2>Other Tests</h2>
                        {otherTestsTable}
                        <br />
                    ";

                    File.WriteAllText(Path.Combine(runtimePath, $"{tests.Key}.html"), string.Format(Template, "../", testClassSummaryReportBody));
                }
            }
        }

        private string GetRatio(EquivalenceTestConclusion conclusion, Benchmark baseResult, Benchmark diffResult)
        {
            if (conclusion == EquivalenceTestConclusion.Same)
            {
                return "0.0%";
            }

            if (conclusion == EquivalenceTestConclusion.Unknown)
            {
                return "----";
            }

            if (baseResult.Statistics.Median == 0)
            {
                return "???";
            }

            var diff = (diffResult.Statistics.Median - baseResult.Statistics.Median) / baseResult.Statistics.Median * 100;
            return $"{diff:0.00} %";
        }

        private const string Template = @"<head><link rel=""stylesheet"" type=""text/css"" href=""{0}report_style.css""></head><body>{1}</body>";

        private static string GetHtmlPageHeader(string baselineVersion, string benchmarkVersion) =>
            @$"
                <h1>Perfomance Test Results ({DateTime.UtcNow} UTC)</h1>
                <p><b>Baseline:</b> {baselineVersion}</p>
                <p><b>Benchmark:</b> {(benchmarkVersion.ToLower() == "default" ? "Current Source Code" : benchmarkVersion)}</p>
            ";
    }
}
