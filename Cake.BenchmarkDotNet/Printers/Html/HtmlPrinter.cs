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
                .ToHtml();


            File.WriteAllText(outputPath, report);
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
