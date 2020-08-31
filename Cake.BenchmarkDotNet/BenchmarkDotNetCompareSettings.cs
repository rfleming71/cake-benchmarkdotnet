using System.Collections.Generic;
using System.Linq;

namespace Cake.BenchmarkDotNet
{
    public class BenchmarkDotNetCompareSettings
    {
        public IEnumerable<string> Filters { get; set; } = Enumerable.Empty<string>();
        public string TrxFilePath { get; set; }
        public string MarkdownFilePath { get; set; }
        public string HtmlFilePath { get; set; }
        public string NoiseThreshold { get; set; } = "0.3ns";
        public string TestThreshold { get; set; } = "5%";
    }
}
