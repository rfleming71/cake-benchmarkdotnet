using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Cake.BenchmarkDotnet.Tests.Resources;
using Cake.BenchmarkDotNet;
using Perfolizer.Mathematics.Thresholds;
using Xunit;

namespace Cake.BenchmarkDotnet.Tests
{
    public class ComparerTests : IDisposable
    {
        private readonly string _baselineFolder;
        private readonly string _diffFolder;

        public ComparerTests()
        {
            var path = Path.GetTempPath();
            _baselineFolder = Path.Combine(path, "baseline");
            _diffFolder = Path.Combine(path, "diff");

            Directory.CreateDirectory(_baselineFolder);
            Directory.CreateDirectory(_diffFolder);

            File.WriteAllText(Path.Combine(_baselineFolder, "results-full.json"), Encoding.UTF8.GetString(Resource1.SampleBaseline));
            File.WriteAllText(Path.Combine(_diffFolder, "results-full.json"), Encoding.UTF8.GetString(Resource1.SampleBaseline));
        }

        public void Dispose()
        {
            Directory.Delete(_baselineFolder, true);
            Directory.Delete(_diffFolder, true);
        }

        [Fact]
        public void CompareAllResults()
        {
            var comparer = new Comparer(Threshold.Create(ThresholdUnit.Ratio, 0.05), Threshold.Create(ThresholdUnit.Nanoseconds, 3));
            var results = comparer.Compare(_baselineFolder, _diffFolder, new string[0]).ToList();

            Assert.Equal(8, results.Count());
            Assert.Equal(2, results.Count(x => x.Runtime == "Not Specified"));
            Assert.Equal(2, results.Count(x => x.Runtime == ".NET 4.8"));
            Assert.Equal(2, results.Count(x => x.Runtime == ".NET Core 3.1"));
            Assert.Equal(2, results.Count(x => x.Runtime == ".NET 4.6.1"));
        }
    }
}
