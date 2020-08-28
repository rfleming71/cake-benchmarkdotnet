using System.Collections.Generic;

namespace Cake.BenchmarkDotNet.Dto
{
    public class RunReport
    {
        public string Title { get; set; }
        public HostEnvironmentInfo HostEnvironmentInfo { get; set; }
        public List<Benchmark> Benchmarks { get; set; }
    }
}
