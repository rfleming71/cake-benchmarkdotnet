using System.Collections.Generic;
using Cake.BenchmarkDotNet.NuGetComparison;

namespace Cake.BenchmarkDotNet.Printers
{
    public interface IPrinter
    {
        void Print(IEnumerable<CompareResult> results, string outputPath);

        void Print(NuGetCompareFinalResult nuGetCompareFinalResult, string outputPath);
    }
}
