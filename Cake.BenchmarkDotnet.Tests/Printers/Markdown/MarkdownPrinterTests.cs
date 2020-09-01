using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Cake.BenchmarkDotNet;
using Cake.BenchmarkDotNet.Dto;
using Cake.BenchmarkDotNet.Printers.Markdown;
using Perfolizer.Mathematics.SignificanceTesting;
using Xunit;

namespace Cake.BenchmarkDotnet.Tests.Printers.Markdown
{
    public class MarkdownPrinterTests : IDisposable
    {
        private readonly MarkdownPrinter _printer;
        private readonly string _outputPath;

        public MarkdownPrinterTests()
        {
            _outputPath = Path.GetTempFileName();
            _printer = new MarkdownPrinter();
        }

        public void Dispose()
        {
            File.Delete(_outputPath);
        }

        [Fact]
        public void NullOutputPath_DoesNothing()
        {
            var results = new[] {
                 new CompareResult("123", "Net Core 1", null, null, EquivalenceTestConclusion.Slower)
            };

            _printer.Print(results, null);
        }

        [Fact]
        public void PrintReport()
        {
            var results = new[] {
                 new CompareResult("123", "Net Core 1", GetBenchmark("test1", 100, 101), GetBenchmark("test1", 102, 103), EquivalenceTestConclusion.Slower),
                 new CompareResult("124", "Net Core 1", GetBenchmark("test2", 100, 101), GetBenchmark("test1", 90, 90), EquivalenceTestConclusion.Faster),
                 new CompareResult("125", "Net Core 1", GetBenchmark("test3", 100, 101), GetBenchmark("test1", 100, 101), EquivalenceTestConclusion.Same),
            };

            _printer.Print(results, _outputPath);

            Assert.True(File.Exists(_outputPath));
        }

        private Benchmark GetBenchmark(string methodName, int median, int mean) => new Benchmark()
        {
            Method = methodName,
            Type = "MarkdownPrinterTests",
            Namespace = "Cake.BenchmarkDotnet.Tests.Printers.Markdown",
            Statistics = new Statistics()
            {
                Median = median,
                Mean = mean,
            }
        };
    }
}
