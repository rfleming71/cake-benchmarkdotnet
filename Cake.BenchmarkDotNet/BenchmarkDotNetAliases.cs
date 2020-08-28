using System;
using Cake.BenchmarkDotNet;
using Cake.BenchmarkDotNet.Printer.Trx;
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

        Threshold test;
        if (!Threshold.TryParse(settings.TestThreshold, out test))
        {
            throw new ArgumentException(nameof(settings.TestThreshold));
        }

        Threshold noise;
        if (!Threshold.TryParse(settings.NoiseThreshold, out noise))
        {
            throw new ArgumentException(nameof(settings.NoiseThreshold));
        }

        var results = new Comparer(test, noise).Compare(baseline, diff, settings.Filters);
        if (!string.IsNullOrWhiteSpace(settings.TrxFilePath))
        {
            context.Log.Write(Verbosity.Normal, LogLevel.Information, $"Writing comparision results to {settings.TrxFilePath}");
            new TrxPrinter().Print(results, settings.TrxFilePath);
        }
    }
}