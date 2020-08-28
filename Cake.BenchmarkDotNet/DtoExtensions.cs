using System.Linq;
using Cake.BenchmarkDotNet.Dto;

namespace Cake.BenchmarkDotNet
{
    public static class DtoExtensions
    {
        public static double[] GetOriginalValues(this Benchmark benchmark)
            => benchmark
                .Measurements
                .Where(measurement => measurement.IterationStage == "Result")
                .Select(measurement => measurement.Nanoseconds / measurement.Operations)
                .ToArray();
    }
}
