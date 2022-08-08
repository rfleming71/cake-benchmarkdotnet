using System.Globalization;
using CsvHelper.Configuration;

namespace Cake.BenchmarkDotNet.NuGetComparison
{
    internal class CsvReportRecord
    {
        public static string ValueNotAvailable = "?";

        public string Method { get; set; }
        public string MethodNameSuffix { get; set; }
        public string Job { get; set; }
        public string NuGetReferences { get; set; }
        public string Mean { get; set; }
        public string Error { get; set; }
        public string StdDev { get; set; }
        public string Median { get; set; }
        public string Ratio { get; set; }
        public string RatioSD { get; set; }
        public string Gen0 { get; set; }
        public string Gen1 { get; set; }
        public string Allocated { get; set; }
    }

    internal class CsvReportRecordClassMap : ClassMap<CsvReportRecord>
    {
        public CsvReportRecordClassMap()
        {
            AutoMap(CultureInfo.InvariantCulture);

            Map(m => m.MethodNameSuffix).Name("methodNameSuffix").Optional().Default(null);

            Map(m => m.Median).Optional().Default(CsvReportRecord.ValueNotAvailable);
            Map(m => m.Ratio).Optional().Default(CsvReportRecord.ValueNotAvailable);
            Map(m => m.RatioSD).Optional().Default(CsvReportRecord.ValueNotAvailable);

            Map(m => m.Gen0).Name("Gen 0").Optional().Default(CsvReportRecord.ValueNotAvailable);
            Map(m => m.Gen1).Name("Gen 1").Optional().Default(CsvReportRecord.ValueNotAvailable);
            Map(m => m.Allocated).Optional().Default(CsvReportRecord.ValueNotAvailable);
        }
    }
}
