using Cake.BenchmarkDotNet.Dto;
using Perfolizer.Mathematics.SignificanceTesting;

namespace Cake.BenchmarkDotNet
{
    public class CompareResult
    {
        public CompareResult(string id, string runtime, Benchmark baseResult, Benchmark diffResult, EquivalenceTestConclusion conclusion)
        {
            Id = id;
            Runtime = runtime;
            BaseResult = baseResult;
            DiffResult = diffResult;
            Conclusion = conclusion;
        }

        public string Id { get; }
        public string Runtime { get; }
        public Benchmark BaseResult { get; }
        public Benchmark DiffResult { get; }
        public EquivalenceTestConclusion Conclusion { get; }
    }
}
