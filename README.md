# Cake.BenchmarkDotNet

Cake Addin for generating report files from BenchmarkDotNet artifacts.

## Usage: BenchmarkDotNetCompareResults(...)
This method compares a set of BenchmarkDotNet artifacts against a baseline set of artifacts from a prior BenchmarkDotNet execution then outputs the results into HTML/MD/TRX file formats. The TRX file format can be used in continuous integration environments such as TeamCity.

**Note**: The comparison is executed against the `[FILENAME]-report-full.json` BenchmarkDotNet artifact files.

### Parameters

* `baseline` - File path to the baseline artifacts directory.
* `diff` - File path to the current benchmark artifacts directory.
* `settings` - `BenchmarkDotNetCompareSettings` settings to configure the report generation.
  * `Filters` - Set of filter strings to limit the scope of benchmark artifact comparison (default = compare all artifacts).
  * `TrxFilePath` - File path for the output TRX report (should specify a filename ending in `.trx`).
  * `MarkdownFilePath` - File path for the output Markdown report (should specify a filename ending in `.md`).
  * `HtmlFilePath` - File path for the output HTML report (should specify a directory name as multiple files will be generated within the specified directory).
  * `NoiseThreshold` - A `Threshold` value for the test noise floor (default = `0.3ns`).
  * `TestThreshold` - A `Threshold` value for the test comparison results (default = `5%`).

### Example

```
#addin "nuget:?package=Cake.BenchmarkDotNet&version=0.2.23&loaddependencies=true"
Task("CompareBenchmarks")
.Does(() => {
    BenchmarkDotNetCompareResults(
        @"C:\temp\bdn\baseline",
        @"C:\temp\bdn\benchmarkArtifacts",
        new BenchmarkDotNetCompareSettings {
          TrxFilePath  =  @"C:\temp\bdn\test-results.trx",
          TestThreshold  =  "5%"
        });
});
```

## Usage: BenchmarkDotNetCompareNuGetResults(...)
This method compares a set of baseline and benchmark BenchmarkDotNet artifacts then outputs the results into HTML/MD/TRX file formats. The TRX file format can be used in continuous integration environments such as TeamCity. This method is used in conjunction with the BenchmarkDotNet NuGet package version comparison functionality.

**Note**: The comparison is executed against the `[FILENAME]-report.csv` BenchmarkDotNet artifact files.

**Note**: This method assumes that the CSV report files were generated with the following configuration parameters. These configuration parameters provide the stable report data format required for final report generation.

```
SummaryStyle = BenchmarkDotNet.Reports.SummaryStyle
    .Default
    .WithRatioStyle(RatioStyle.Percentage)
    .WithTimeUnit(TimeUnit.Nanosecond)
    .WithSizeUnit(SizeUnit.B);
```

### Parameters

* `benchmarkDotNetArtifactsDirectory` - File path to the benchmark artifacts directory.
* `settings` - `BenchmarkDotNetNuGetCompareSettings` settings to configure the report generation.
  * `Filters` - Set of filter strings to limit the scope of benchmark artifact comparison (default = compare all artifacts).
  * `TrxFilePath` - File path for the output TRX report (should specify a filename ending in `.trx`).
  * `MarkdownFilePath` - File path for the output Markdown report (should specify a filename ending in `.md`).
  * `HtmlFilePath` - File path for the output HTML report (should specify a directory name as multiple files will be generated within the specified directory).
  * `NoiseThreshold` - A `Threshold` value for the test noise floor (default = `0.3ns`).
  * `TestThresholdPercentage` - A percentage threshold value for the test comparison results (default = `10`). Example: 10% threshold is specified as `10`, not `0.10`.

### Example
```
#addin "nuget:?package=Cake.BenchmarkDotNet&version=0.2.23&loaddependencies=true"
Task("CompareBenchmarks")
.Does(() => {
    BenchmarkDotNetCompareNuGetResults(
        @"C:\temp\bdn\benchmarkArtifacts",
        new BenchmarkDotNetNuGetCompareSettings
        {
            TrxFilePath = $".\\outputDirectory\\performance-results.trx",
            MarkdownFilePath = $".\\outputDirectory\\performance-results.md",
            HtmlFilePath = $".\\outputDirectory\\performance-results",
            TestThresholdPercentage = 10
        });
});
```
