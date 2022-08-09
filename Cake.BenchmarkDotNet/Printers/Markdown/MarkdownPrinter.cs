using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cake.BenchmarkDotNet.Dto;
using Cake.BenchmarkDotNet.NuGetComparison;
using MarkdownLog;
using Perfolizer.Mathematics.SignificanceTesting;

namespace Cake.BenchmarkDotNet.Printers.Markdown
{
    internal class MarkdownPrinter : IPrinter
    {
        public void Print(IEnumerable<CompareResult> results, string outputPath)
        {
            if (outputPath == null)
                return;

            var report = results
                .OrderByDescending(x => x.Conclusion)
                .Select(x => new
                {
                    Conclusion = x.Conclusion,
                    Namespace = x.BaseResult.Namespace,
                    TestName = $"{x.BaseResult.Type}.{x.BaseResult.Method}",
                    Ratio = GetRatio(x.Conclusion, x.BaseResult, x.DiffResult),
                    BaseMedian = $"{x.BaseResult.Statistics.Median}",
                    DiffMedian = $"{x.DiffResult.Statistics.Median}",
                })
                .ToMarkdownTable()
                .WithHeaders("Conclusion", "Namespace", "Test Name", "Speed Difference", "Base Median (ns)", "Diff Median (ns)")
                .ToMarkdown();

            File.WriteAllText(outputPath, report);
        }

        public void Print(NuGetCompareFinalResult nuGetCompareFinalResult, string outputPath)
        {
            if (outputPath == null)
                return;

            var reportHeader = $"# Perfomance Test Results ({DateTime.UtcNow} UTC){Environment.NewLine}{Environment.NewLine}"
                + $"**Baseline:** {nuGetCompareFinalResult.BaselineVersion}{Environment.NewLine}{Environment.NewLine}"
                + $"**Benchmark:** {(nuGetCompareFinalResult.BenchmarkVersion.ToLower() == "default" ? "Current Source Code" : nuGetCompareFinalResult.BenchmarkVersion)} (**Test Threshold:** {nuGetCompareFinalResult.TestThresholdPercentage}%){Environment.NewLine}{Environment.NewLine}";

            var slowerResultsTable = nuGetCompareFinalResult.TestResults
                .Where(x => x.Conclusion == EquivalenceTestConclusion.Slower)
                .OrderByDescending(x => x.SpeedDifferencePercentage)
                .ThenBy(x => $"{x.NamespaceName}.{x.ClassName}.{x.MethodName}")
                .Select(x => new
                {
                    TestName = $"{x.NamespaceName}.{x.ClassName}.{x.MethodName} ({x.RuntimeName})",
                    Ratio = $"{Math.Round(Math.Abs(x.SpeedDifferencePercentage) * 100, 2)}%",
                    BaseMedian = x.BaselineMedianSpeedNs,
                    DiffMedian = x.BenchmarkMedianSpeedNs,
                })
                .ToMarkdownTable()
                .WithHeaders("Test Name", "% Slower", "Baseline Median (ns)", "Benchmark Median (ns)")
                .ToMarkdown();

            var fasterTestsTable = nuGetCompareFinalResult.TestResults
                .Where(x => x.Conclusion == EquivalenceTestConclusion.Faster)
                .OrderBy(x => x.SpeedDifferencePercentage)
                .ThenBy(x => $"{x.NamespaceName}.{x.ClassName}.{x.MethodName}")
                .Select(x => new
                {
                    TestName = $"{x.NamespaceName}.{x.ClassName}.{x.MethodName} ({x.RuntimeName})",
                    Ratio = $"{Math.Round(Math.Abs(x.SpeedDifferencePercentage) * 100, 2)}%",
                    BaseMedian = x.BaselineMedianSpeedNs,
                    DiffMedian = x.BenchmarkMedianSpeedNs,
                })
                .ToMarkdownTable()
                .WithHeaders("Test Name", "% Faster", "Base Median (ns)", "Benchmark Median (ns)")
                .ToMarkdown();

            var otherTestsTable = nuGetCompareFinalResult.TestResults
                .Where(x => x.Conclusion != EquivalenceTestConclusion.Slower && x.Conclusion != EquivalenceTestConclusion.Faster)
                .OrderByDescending(x => x.SpeedDifferencePercentage)
                .ThenBy(x => $"{x.NamespaceName}.{x.ClassName}.{x.MethodName}")
                .Select(x => new
                {
                    Conclusion = x.Conclusion == EquivalenceTestConclusion.Same ? $"{EquivalenceTestConclusion.Same} (within threshold)" : x.Conclusion.ToString(),
                    TestName = $"{x.NamespaceName}.{x.ClassName}.{x.MethodName} ({x.RuntimeName})",
                    Ratio = $"{Math.Round(Math.Abs(x.SpeedDifferencePercentage) * 100, 2)}% {(x.SpeedDifferencePercentage < 0 ? "faster" : "slower")}",
                    BaseMedian = x.BaselineMedianSpeedNs,
                    DiffMedian = x.BenchmarkMedianSpeedNs,
                })
                .ToMarkdownTable()
                .WithHeaders("Conclusion", "Test Name", "% Difference", "Base Median (ns)", "Benchmark Median (ns)")
                .ToMarkdown();

            var reportBody = $"{reportHeader}"
                + $"## Slower Tests (focus on these){Environment.NewLine}{Environment.NewLine}"
                + $"{slowerResultsTable}{Environment.NewLine}"
                + $"## Faster Tests{Environment.NewLine}{Environment.NewLine}"
                + $"{fasterTestsTable}{Environment.NewLine}"
                + $"## Other Tests{Environment.NewLine}{Environment.NewLine}"
                + $"{otherTestsTable}{Environment.NewLine}";

            File.WriteAllText(outputPath, reportBody);
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
    }
}
