using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Cake.BenchmarkDotNet;
using Cake.BenchmarkDotNet.Dto;
using Cake.BenchmarkDotNet.Printers.Html;
using Perfolizer.Mathematics.SignificanceTesting;
using Xunit;

namespace Cake.BenchmarkDotnet.Tests.Printers.Html
{
    public class HtmlPrinterTests : IDisposable
    {
        private readonly HtmlPrinter _printer;
        private readonly string _outputPath;

        public HtmlPrinterTests()
        {
            _outputPath = Path.GetTempFileName();
            File.Delete(_outputPath);
            Directory.CreateDirectory(_outputPath);
            _printer = new HtmlPrinter();
        }

        public void Dispose()
        {
            Directory.Delete(_outputPath, true);
        }

        [Fact]
        public void NullOutputPath_DoesNothing()
        {
            var results = new[] {
                 new CompareResult("123", null, null, null, EquivalenceTestConclusion.Slower)
            };

            _printer.Print(results, null);
        }

        [Fact]
        public void PrintReport()
        {
            var results = new[] {
                 new CompareResult("123", "Net Core 1", GetBenchmark("Cake.BenchmarkDotnet.Tests.Printers", "test1", 100, 101), GetBenchmark("Cake.BenchmarkDotnet.Tests.Printers", "test1", 102, 103), EquivalenceTestConclusion.Slower),
                 new CompareResult("124", "Net Core 1", GetBenchmark("Cake.BenchmarkDotnet.Tests", "test2", 100, 101), GetBenchmark("Cake.BenchmarkDotnet.Tests", "test1", 90, 90), EquivalenceTestConclusion.Faster),
                 new CompareResult("125", "Net Core 1", GetBenchmark("Cake.BenchmarkDotnet.Tests.Printers", "test3", 100, 101), GetBenchmark("Cake.BenchmarkDotnet.Tests.Printers", "test1", 100, 101), EquivalenceTestConclusion.Same),
            };

            _printer.Print(results, _outputPath);

            Assert.True(File.Exists(_outputPath + "/index.html"));
            Assert.True(File.Exists(_outputPath + "/Cake.BenchmarkDotnet.Tests.html"));
            Assert.True(File.Exists(_outputPath + "/Cake.BenchmarkDotnet.Tests.Printers.html"));
        }

        private Benchmark GetBenchmark(string nameSpace, string methodName, int median, int mean) => new Benchmark()
        {
            Method = methodName,
            Type = "HtmlPrinterTests",
            Namespace = nameSpace,
            Statistics = new Statistics()
            {
                Median = median,
                Mean = mean,
            }
        };
    }
}
