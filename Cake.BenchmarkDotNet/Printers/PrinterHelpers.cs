using System;
using System.Collections.Generic;
using System.Text;

namespace Cake.BenchmarkDotNet.Printers
{
    internal class PrinterHelpers
    {
        public static string FormatNsToTimespan(long nanoseconds) => 
            TimeSpan.FromTicks(nanoseconds / 100).ToString();
    }
}
