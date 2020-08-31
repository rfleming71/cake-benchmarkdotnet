using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using Cake.BenchmarkDotNet;
using Cake.BenchmarkDotNet.Printers;
using Cake.BenchmarkDotNet.Printers.Html;
using Cake.BenchmarkDotNet.Printers.Markdown;
using Cake.BenchmarkDotNet.Printers.Trx;
using Cake.Core;
using Cake.Core.Annotations;
using Cake.Core.Diagnostics;
using Perfolizer.Mathematics.Thresholds;

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


    static Dictionary<string, IPrinter> _printers = new Dictionary<string, IPrinter>()
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
}