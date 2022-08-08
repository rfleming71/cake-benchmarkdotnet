using System.Collections.Generic;
using System.Linq;

namespace Cake.BenchmarkDotNet.NuGetComparison
{
    public class BenchmarkDotNetNuGetCompareSettings
    {
        public IEnumerable<string> Filters { get; set; } = Enumerable.Empty<string>();
        public string TrxFilePath { get; set; }
        public string MarkdownFilePath { get; set; }
        public string HtmlFilePath { get; set; }
        public double TestThresholdPercentage { get; set; } = 10;
    }
}
