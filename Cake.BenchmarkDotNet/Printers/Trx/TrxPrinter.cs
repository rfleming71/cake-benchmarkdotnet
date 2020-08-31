using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using Cake.BenchmarkDotNet.Printer.Trx.Dto;
using Cake.BenchmarkDotNet.Printers;
using Perfolizer.Mathematics.SignificanceTesting;

namespace Cake.BenchmarkDotNet.Printers.Trx
{
    internal class TrxPrinter : IPrinter
    {
        public void Print(IEnumerable<CompareResult> results, string outputPath)
        {
            if (outputPath == null)
                return;

            var testRun = new TestRun();
            foreach (var result in results)
            {
                testRun.Add(BuildUnitTest(result), BuildUnitTestResult(result));
            }

            XmlSerializer ser = new XmlSerializer(typeof(TestRun));
            using (var writer = XmlWriter.Create(outputPath, new XmlWriterSettings() { Indent = true }))
            {
                ser.Serialize(writer, testRun);
            }
        }

        private UnitTest BuildUnitTest(CompareResult result) =>
            new UnitTest()
            {
                Name = result.Id,
                TestMethod = new TestMethod()
                {
                    Name = result.BaseResult.Method,
                    ClassName = result.BaseResult.Type,
                    CodeBase = result.BaseResult.Namespace,
                },
            };

        private UnitTestResult BuildUnitTestResult(CompareResult result)
        {
            var utr = new UnitTestResult()
            {
                TestName = result.BaseResult.Method,
                Outcome = GetOutcome(result),
            };

            if (result.Conclusion == EquivalenceTestConclusion.Slower)
            {
                utr.Output = new Output()
                {
                    ErrorInfo = new ErrorInfo()
                    {
                        Message = $"{result.Id} has regressed, was {result.BaseResult.Statistics.Median} is {result.DiffResult.Statistics.Median}.",
                    }
                };
            }
            else if (result.Conclusion == EquivalenceTestConclusion.Unknown)
            {
                utr.Output = new Output()
                {
                    ErrorInfo = new ErrorInfo()
                    {
                        Message = $"{result.Id} has encountered an error.",
                    }
                };
            }

            return utr;
        }

        private string GetOutcome(CompareResult result)
        {
            switch (result.Conclusion)
            {
                case EquivalenceTestConclusion.Slower:
                    return "Failed";
                case EquivalenceTestConclusion.Base:
                case EquivalenceTestConclusion.Faster:
                case EquivalenceTestConclusion.Same:
                    return "Passed";
                case EquivalenceTestConclusion.Unknown:
                default:
                    return "Error";
            }
        }
    }
}
