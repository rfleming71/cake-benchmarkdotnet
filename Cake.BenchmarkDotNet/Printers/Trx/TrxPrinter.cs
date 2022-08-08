using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using Cake.BenchmarkDotNet.NuGetComparison;
using Cake.BenchmarkDotNet.Printer.Trx.Dto;
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

        public void Print(NuGetCompareFinalResult nuGetCompareFinalResult, string outputPath)
        {
            if (outputPath == null)
                return;

            var testRun = new TestRun();
            foreach (var result in nuGetCompareFinalResult.TestResults)
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
                    Name = FormatUnitTestName(result),
                    ClassName = result.BaseResult.Type,
                    CodeBase = result.BaseResult.Namespace,
                },
            };

        private UnitTest BuildUnitTest(NuGetCompareTestResult result) =>
            new UnitTest()
            {
                Name = GetResultId(result),
                TestMethod = new TestMethod()
                {
                    Name = FormatUnitTestName(result),
                    ClassName = result.ClassName,
                    CodeBase = result.NamespaceName,
                },
            };

        private string FormatUnitTestName(CompareResult result) => 
            string.IsNullOrWhiteSpace(result.Runtime)
            ? result.BaseResult.Method
            : $"{result.BaseResult.Method} ({result.Runtime})";

        private string FormatUnitTestName(NuGetCompareTestResult result) =>
            string.IsNullOrWhiteSpace(result.RuntimeName)
            ? result.MethodName
            : $"{result.MethodName} ({result.RuntimeName})";

        private UnitTestResult BuildUnitTestResult(CompareResult result)
        {
            var utr = new UnitTestResult()
            {
                TestName = FormatUnitTestName(result),
                Outcome = GetOutcome(result.Conclusion),
                Duration = PrinterHelpers.FormatNsToTimespan((long)result.DiffResult.Statistics.Median)
            };

            if (result.Conclusion == EquivalenceTestConclusion.Slower)
            {
                utr.Output = new Output()
                {
                    ErrorInfo = new ErrorInfo()
                    {
                        Message = $"{result.Id} ({result.Runtime}) has regressed, was {result.BaseResult.Statistics.Median}ns is {result.DiffResult.Statistics.Median}ns.",
                    }
                };
            }
            else if (result.Conclusion == EquivalenceTestConclusion.Unknown)
            {
                utr.Output = new Output()
                {
                    ErrorInfo = new ErrorInfo()
                    {
                        Message = $"{result.Id} ({result.Runtime}) has encountered an error.",
                    }
                };
            }

            return utr;
        }

        private UnitTestResult BuildUnitTestResult(NuGetCompareTestResult result)
        {
            var utr = new UnitTestResult()
            {
                TestName = FormatUnitTestName(result),
                Outcome = GetOutcome(result.Conclusion),
                Duration = PrinterHelpers.FormatNsToTimespan((long)result.BenchmarkMedianSpeedNs)
            };

            var resultId = GetResultId(result);

            if (result.Conclusion == EquivalenceTestConclusion.Slower)
            {
                utr.Output = new Output()
                {
                    ErrorInfo = new ErrorInfo()
                    {
                        Message = $"{resultId} ({result.RuntimeName}) has regressed, was {result.BaselineMedianSpeedNs}ns is {result.BenchmarkMedianSpeedNs}ns.",
                    }
                };
            }
            else if (result.Conclusion == EquivalenceTestConclusion.Unknown)
            {
                utr.Output = new Output()
                {
                    ErrorInfo = new ErrorInfo()
                    {
                        Message = $"{resultId} ({result.RuntimeName}) has encountered an error.",
                    }
                };
            }

            return utr;
        }

        private string GetOutcome(EquivalenceTestConclusion conclusion)
        {
            switch (conclusion)
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

        private string GetResultId(NuGetCompareTestResult result) =>
            $"{result.NamespaceName}.{result.ClassName}.{result.MethodName}";
    }
}
