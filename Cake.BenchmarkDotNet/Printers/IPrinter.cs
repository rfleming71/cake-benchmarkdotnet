using System.Collections.Generic;

namespace Cake.BenchmarkDotNet.Printers
{
    public interface IPrinter
    {
        void Print(IEnumerable<CompareResult> results, string outputPath);
    }
}
