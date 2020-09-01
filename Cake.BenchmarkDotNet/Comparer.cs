using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Cake.BenchmarkDotNet.Dto;
using Newtonsoft.Json;
using Perfolizer.Mathematics.SignificanceTesting;
using Perfolizer.Mathematics.Thresholds;

namespace Cake.BenchmarkDotNet
{
    // https://github.com/dotnet/performance/blob/master/src/tools/ResultsComparer/Program.cs
    public class Comparer
    {
        private readonly Threshold _testThreshold;
        private readonly Threshold _noiseThreshold;

        public Comparer(Threshold testThreshold, Threshold noiseThreshold)
        {
            _testThreshold = testThreshold;
            _noiseThreshold = noiseThreshold;
        }

        public IEnumerable<CompareResult> Compare(string baseFile, string diffFile, IEnumerable<string> filters)
        {
            foreach (var pair in ReadResults(baseFile, diffFile, filters))
            {
                if (pair.baseResult.Statistics == null || pair.diffResult.Statistics == null)
                {
                    yield return new CompareResult(pair.id, pair.runtime, pair.baseResult, pair.diffResult, EquivalenceTestConclusion.Unknown);
                }

                var baseValues = pair.baseResult.GetOriginalValues();
                var diffValues = pair.diffResult.GetOriginalValues();

                var userTresholdResult = StatisticalTestHelper.CalculateTost(MannWhitneyTest.Instance, baseValues, diffValues, _testThreshold);
                var conclusion = userTresholdResult.Conclusion;
                if (userTresholdResult.Conclusion != EquivalenceTestConclusion.Same)
                {
                    var noiseResult = StatisticalTestHelper.CalculateTost(MannWhitneyTest.Instance, baseValues, diffValues, _noiseThreshold);
                    conclusion = noiseResult.Conclusion;
                }

                yield return new CompareResult(pair.id, pair.runtime, pair.baseResult, pair.diffResult, conclusion);
            }
        }

        private IEnumerable<(string id, string runtime, Benchmark baseResult, Benchmark diffResult)> ReadResults(string baseFile, string diffFile, IEnumerable<string> filterPatterns)
        {
            var baseFiles = GetFilesToParse(baseFile);
            var diffFiles = GetFilesToParse(diffFile);

            if (!baseFiles.Any() || !diffFiles.Any())
                throw new ArgumentException($"Provided paths contained no *full.json files.");

            var baseResults = baseFiles.Select(ReadFromFile);
            var diffResults = diffFiles.Select(ReadFromFile);

            var filters = filterPatterns.Select(pattern => new Regex(WildcardToRegex(pattern), RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)).ToArray();

            var benchmarkIdToDiffResults = diffResults
                .SelectMany(result => result.Benchmarks)
                .Where(benchmarkResult => !filters.Any() || filters.Any(filter => filter.IsMatch(benchmarkResult.FullName)))
                .ToDictionary(benchmarkResult => (benchmarkResult.FullName, GetRuntimeFromDisplayInfo(benchmarkResult.DisplayInfo)), benchmarkResult => benchmarkResult);

            return baseResults
                .SelectMany(result => result.Benchmarks)
                .ToDictionary(benchmarkResult => (benchmarkResult.FullName, GetRuntimeFromDisplayInfo(benchmarkResult.DisplayInfo)), benchmarkResult => benchmarkResult) // we use ToDictionary to make sure the results have unique IDs
                .Where(baseResult => benchmarkIdToDiffResults.ContainsKey(baseResult.Key))
                .Select(baseResult => (baseResult.Key.FullName, baseResult.Key.Item2, baseResult.Value, benchmarkIdToDiffResults[baseResult.Key]));
        }

        private string[] GetFilesToParse(string path)
        {
            if (Directory.Exists(path))
                return Directory.GetFiles(path, $"*full.json", SearchOption.AllDirectories);
            else if (File.Exists(path) || !path.EndsWith("full.json"))
                return new[] { path };
            else
                throw new FileNotFoundException($"Provided path does NOT exist or is not a {path} file", path);
        }

        private RunReport ReadFromFile(string resultFilePath)
        {
            try
            {
                return JsonConvert.DeserializeObject<RunReport>(File.ReadAllText(resultFilePath));
            }
            catch (Exception)
            {
                Console.WriteLine($"Exception while reading the {resultFilePath} file.");
                throw;
            }
        }

        // https://stackoverflow.com/a/6907849/5852046 not perfect but should work for all we need
        private string WildcardToRegex(string pattern) => $"^{Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", ".")}$";

        private string GetRuntimeFromDisplayInfo(string displayInfo)
        {
            var result = Regex.Match(displayInfo, "Runtime=(.*?),", RegexOptions.IgnoreCase);
            if (!result.Success)
            {
                return string.Empty;
            }

            return result.Groups[1].Value;
        }

    }
}
