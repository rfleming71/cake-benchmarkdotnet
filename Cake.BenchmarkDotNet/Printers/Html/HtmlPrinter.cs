using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cake.BenchmarkDotNet.Dto;
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

            var resultGroups = results
                .GroupBy(x => $"{x.BaseResult.Namespace}.{x.BaseResult.Type}")
                .ToArray();

            var report = resultGroups
                .Select(x => new
                {
                    Class = $"[{x.Key}]({x.Key}.html)",
                    Slower = x.Count(t => t.Conclusion == EquivalenceTestConclusion.Slower),
                    Faster = x.Count(t => t.Conclusion == EquivalenceTestConclusion.Faster),
                    Same = x.Count(t => t.Conclusion == EquivalenceTestConclusion.Same),
                    Errors = x.Count(t => t.Conclusion == EquivalenceTestConclusion.Unknown),
                })
                .OrderBy(x => x.Class)
                .ToMarkdownTable()
                .WithHeaders("Class", "Slower", "Same", "Faster", "Errors")
                .ToHtml();

            File.WriteAllText(Path.Combine(outputPath, "index.html"), report);

            foreach (var group in resultGroups)
            {
                report = group
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

                File.WriteAllText(Path.Combine(outputPath, $"{group.Key}.html"), report);
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
    }
}
