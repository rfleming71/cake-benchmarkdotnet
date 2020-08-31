using System;
using System.Collections.Generic;
using System.Text;
using Cake.BenchmarkDotNet.Printers;
using Xunit;

namespace Cake.BenchmarkDotnet.Tests.Printers
{
    public class PrinterHelpersTests
    {
        [Theory]
        [InlineData("00:00:00.0000100", 10_000)]
        [InlineData("00:00:40.0000100", 40_000_010_023)]
        [InlineData("01:06:40.0000100", 4_000_000_010_023)]
        public void FormatNsToTimeSpan(string expected, long value)
        {
            Assert.Equal(expected, PrinterHelpers.FormatNsToTimespan(value));
        }

        [Fact]
        public void FormatNsToTimeSpanAgain()
        {
            var test = TimeSpan.FromSeconds(43);
            Assert.Equal("00:00:43", PrinterHelpers.FormatNsToTimespan(test.Ticks * 100));

            test = TimeSpan.FromMinutes(75);
            Assert.Equal("01:15:00", PrinterHelpers.FormatNsToTimespan(test.Ticks * 100));

            test = TimeSpan.FromSeconds(365);
            Assert.Equal("00:06:05", PrinterHelpers.FormatNsToTimespan(test.Ticks * 100));
        }
    }
}
