using System;
using System.Collections.Generic;
using Cake.BenchmarkDotNet;
using Cake.BenchmarkDotNet.NuGetComparison;
using Cake.BenchmarkDotNet.Printers;
using Cake.BenchmarkDotNet.Printers.Html;
using Cake.BenchmarkDotNet.Printers.Markdown;
using Cake.BenchmarkDotNet.Printers.Trx;
using Cake.Core;
using Cake.Core.Annotations;
using Cake.Core.Diagnostics;
using Perfolizer.Mathematics.Thresholds;

[CakeAliasCategory("BenchmarkDotNet")]
public static class BenchmarkDotNetAliases
{
    [CakeMethodAlias]
    [CakeNamespaceImport("Cake.BenchmarkDotNet")]
    public static void BenchmarkDotNetCompareResults(this ICakeContext context, string baseline, string diff, BenchmarkDotNetCompareSettings settings)
    {
        context.Log.Write(Verbosity.Normal, LogLevel.Information, $"Comparing {baseline} to {diff}");

        if (!Threshold.TryParse(settings.TestThreshold, out Threshold test))
        {
            throw new ArgumentException(nameof(settings.TestThreshold));
        }

        if (!Threshold.TryParse(settings.NoiseThreshold, out Threshold noise))
        {
            throw new ArgumentException(nameof(settings.NoiseThreshold));
        }

        var results = new Comparer(test, noise).Compare(baseline, diff, settings.Filters);
        Print(context, "trx", results, settings.TrxFilePath);
        Print(context, "html", results, settings.HtmlFilePath);
        Print(context, "md", results, settings.MarkdownFilePath);
    }

    [CakeMethodAlias]
    [CakeNamespaceImport("Cake.BenchmarkDotNet")]
    public static void BenchmarkDotNetCompareNuGetResults(this ICakeContext context, string benchmarkDotNetArtifactsDirectory, BenchmarkDotNetNuGetCompareSettings settings)
    {
        context.Log.Write(Verbosity.Normal, LogLevel.Information, $"Comparing artifacts from '{benchmarkDotNetArtifactsDirectory}'.");

        var nuGetCompareFinalResult = new NuGetComparer(settings.TestThresholdPercentage).Compare(benchmarkDotNetArtifactsDirectory, settings.Filters);
        Print(context, "trx", nuGetCompareFinalResult, settings.TrxFilePath);
        Print(context, "html", nuGetCompareFinalResult, settings.HtmlFilePath);
        Print(context, "md", nuGetCompareFinalResult, settings.MarkdownFilePath);
    }

    private static readonly Dictionary<string, IPrinter> _printers = new Dictionary<string, IPrinter>()
    {
        { "trx", new TrxPrinter() },
        { "md", new MarkdownPrinter() },
        { "html", new HtmlPrinter() },
    };

    private static void Print(ICakeContext context, string type, IEnumerable<CompareResult> results, string outputPath)
    {
        if (string.IsNullOrWhiteSpace(outputPath))
            return;

        if (!_printers.TryGetValue(type.ToLower(), out var printer))
        {
            context.Log.Write(Verbosity.Normal, LogLevel.Error, $"Unkown printer type: {type}");
            return;
        }

        context.Log.Write(Verbosity.Normal, LogLevel.Information, $"Writing comparision results to {outputPath}");
        printer.Print(results, outputPath);
    }

    private static void Print(ICakeContext context, string type, NuGetCompareFinalResult nuGetCompareFinalResult, string outputPath)
    {
        if (string.IsNullOrWhiteSpace(outputPath))
            return;

        if (!_printers.TryGetValue(type.ToLower(), out var printer))
        {
            context.Log.Write(Verbosity.Normal, LogLevel.Error, $"Unkown printer type: {type}");
            return;
        }

        context.Log.Write(Verbosity.Normal, LogLevel.Information, $"Writing comparision results to {outputPath}");
        printer.Print(nuGetCompareFinalResult, outputPath);
    }
}
