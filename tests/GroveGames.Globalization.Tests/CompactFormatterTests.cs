using System;

#pragma warning disable xUnit1025

namespace GroveGames.Globalization.Tests;

public sealed class CompactFormatterTests
{
    [Theory]
    [InlineData(0d, "0")]
    [InlineData(999d, "999")]
    [InlineData(12.5d, "13")]
    [InlineData(12.04d, "12")]
    [InlineData(9999d, "9999")]
    [InlineData(-12.5d, "-13")]
    [InlineData(-0.04d, "0")]
    [InlineData(-0d, "0")]
    [InlineData(-0.05d, "0")]
    [InlineData(1.45d, "1")]
    [InlineData(2500d, "2500")]
    [InlineData(9999.4d, "9999")]
    [InlineData(1234.5d, "1235")]
    [InlineData(999.96d, "1000")]
    [InlineData(999.5d, "1000")]
    public void TryFormat_BelowTenThousand_WritesTheNumber(double value, string expected)
    {
        Assert.Equal(expected, Format(value));
    }

    [Theory]
    [InlineData(9999.96d, "10K")]
    [InlineData(10000d, "10K")]
    [InlineData(12345d, "12.3K")]
    [InlineData(125000d, "125K")]
    [InlineData(999950d, "1M")]
    [InlineData(1250000d, "1.25M")]
    [InlineData(3.4e9d, "3.4B")]
    [InlineData(4.2e12d, "4.2T")]
    [InlineData(-12345d, "-12.3K")]
    [InlineData(10050d, "10.1K")]
    [InlineData(24050d, "24.1K")]
    [InlineData(806950d, "807K")]
    [InlineData(9050000d, "9.05M")]
    [InlineData(11500d, "11.5K")]
    [InlineData(100000d, "100K")]
    [InlineData(125400d, "125K")]
    [InlineData(1234000d, "1.23M")]
    [InlineData(999500d, "1M")]
    [InlineData(9995000d, "10M")]
    [InlineData(1050000d, "1.05M")]
    public void TryFormat_FromTenThousand_UsesNamedUnits(double value, string expected)
    {
        Assert.Equal(expected, Format(value));
    }

    [Theory]
    [InlineData(1e15d, "1aa")]
    [InlineData(1.5e15d, "1.5aa")]
    [InlineData(7e18d, "7ab")]
    [InlineData(1e90d, "1az")]
    [InlineData(1e93d, "1ba")]
    [InlineData(999e15d, "999aa")]
    [InlineData(1.234e15d, "1.23aa")]
    [InlineData(999.5e12d, "1aa")]
    public void TryFormat_BeyondTrillions_UsesLetterPairs(double value, string expected)
    {
        Assert.Equal(expected, Format(value));
    }

    [Theory]
    [InlineData(1d, 2040L, "1zz")]
    [InlineData(1d, 2043L, "1aaa")]
    [InlineData(1.5d, 400L, "15ey")]
    [InlineData(9.9995d, 5L, "1M")]
    [InlineData(1d, 3L, "1000")]
    [InlineData(4d, -2L, "0")]
    [InlineData(0d, 50L, "0")]
    [InlineData(12.5d, 3L, "12.5K")]
    [InlineData(0.5d, 5L, "50K")]
    [InlineData(-1d, -5L, "0")]
    [InlineData(9.05d, 6L, "9.05M")]
    [InlineData(9.9996d, 3L, "10K")]
    public void TryFormat_MantissaAndExponent_UsesTheSameUnits(double mantissa, long exponent, string expected)
    {
        Span<char> destination = stackalloc char[CompactFormatter.MaxLength];

        var formatted = CompactFormatter.TryFormat(mantissa, exponent, NumberRounding.Nearest, destination, out var charsWritten);

        Assert.True(formatted);
        Assert.Equal(expected, destination.Slice(0, charsWritten).ToString());
    }

    [Theory]
    [InlineData(1249600d, "1.24M")]
    [InlineData(1250000d, "1.25M")]
    [InlineData(999.96d, "999")]
    [InlineData(9999.96d, "9999")]
    [InlineData(999999d, "999K")]
    [InlineData(12.04d, "12")]
    [InlineData(1.25e15d, "1.25aa")]
    [InlineData(999.5e12d, "999T")]
    [InlineData(0d, "0")]
    [InlineData(0.00001d, "0")]
    [InlineData(-12.09d, "-13")]
    [InlineData(-0.04d, "-1")]
    public void TryFormat_Floor_NeverShowsMoreThanTheValue(double value, string expected)
    {
        Assert.Equal(expected, Format(value, NumberRounding.Floor));
    }

    [Theory]
    [InlineData(1250001d, "1.26M")]
    [InlineData(1250000d, "1.25M")]
    [InlineData(12.01d, "13")]
    [InlineData(999.91d, "1000")]
    [InlineData(9999.4d, "10K")]
    [InlineData(999100d, "1M")]
    [InlineData(999.1e12d, "1aa")]
    [InlineData(1000d, "1000")]
    [InlineData(0d, "0")]
    [InlineData(0.00001d, "1")]
    [InlineData(-12.09d, "-12")]
    [InlineData(-0.04d, "0")]
    public void TryFormat_Ceiling_NeverShowsLessThanTheValue(double value, string expected)
    {
        Assert.Equal(expected, Format(value, NumberRounding.Ceiling));
    }

    [Theory]
    [InlineData(1.2496d, 6L, NumberRounding.Floor, "1.24M")]
    [InlineData(1.2501d, 6L, NumberRounding.Ceiling, "1.26M")]
    [InlineData(9.9991d, 5L, NumberRounding.Ceiling, "1M")]
    [InlineData(9.9999d, 5L, NumberRounding.Floor, "999K")]
    [InlineData(1.2499999999999998d, 6L, NumberRounding.Floor, "1.25M")]
    [InlineData(1.2500000000000002d, 6L, NumberRounding.Ceiling, "1.25M")]
    [InlineData(-1.2496d, 6L, NumberRounding.Floor, "-1.25M")]
    [InlineData(-1.2496d, 6L, NumberRounding.Ceiling, "-1.24M")]
    public void TryFormat_MantissaAndExponentRounding_RoundsTheValue(double mantissa, long exponent, NumberRounding rounding, string expected)
    {
        Span<char> destination = stackalloc char[CompactFormatter.MaxLength];

        var formatted = CompactFormatter.TryFormat(mantissa, exponent, rounding, destination, out var charsWritten);

        Assert.True(formatted);
        Assert.Equal(expected, destination.Slice(0, charsWritten).ToString());
    }

    [Theory]
    [InlineData(NumberRounding.Nearest)]
    [InlineData(NumberRounding.Floor)]
    [InlineData(NumberRounding.Ceiling)]
    public void TryFormat_MantissaAndExponent_MatchesTheDoubleOverload(NumberRounding rounding)
    {
        var values = new[] { 0.04d, 12.05d, 999.95d, 9999.4d, 9999.96d, 12345d, 999500d, 1249600d, 1250001d, 9.05e6d, 999.1e12d, 1.234e15d, -12.09d, -1249600d };
        Span<char> mantissaDestination = stackalloc char[CompactFormatter.MaxLength];

        for (var i = 0; i < values.Length; i++)
        {
            var value = values[i];
            var exponent = (long)Math.Floor(Math.Log10(Math.Abs(value)));

            var formatted = CompactFormatter.TryFormat(value / Math.Pow(10d, exponent), exponent, rounding, mantissaDestination, out var charsWritten);

            Assert.True(formatted);
            Assert.Equal(Format(value, rounding), mantissaDestination.Slice(0, charsWritten).ToString());
        }
    }

    [Theory]
    [InlineData(double.NaN, "NaN")]
    [InlineData(double.PositiveInfinity, "Infinity")]
    [InlineData(double.NegativeInfinity, "-Infinity")]
    public void TryFormat_NotFinite_WritesTheInvariantText(double value, string expected)
    {
        Assert.Equal(expected, Format(value));
    }

    [Theory]
    [InlineData(long.MaxValue)]
    [InlineData(long.MinValue)]
    public void TryFormat_ExponentBeyondTheUnits_ReturnsFalse(long exponent)
    {
        Span<char> destination = stackalloc char[CompactFormatter.MaxLength];

        var formatted = CompactFormatter.TryFormat(1d, exponent, NumberRounding.Nearest, destination, out var charsWritten);

        Assert.False(formatted);
        Assert.Equal(0, charsWritten);
    }

    [Fact]
    public void TryFormat_MantissaAndExponentDestinationTooSmall_ReturnsFalse()
    {
        Span<char> destination = stackalloc char[3];

        var formatted = CompactFormatter.TryFormat(1.5d, 15L, NumberRounding.Nearest, destination, out var charsWritten);

        Assert.False(formatted);
        Assert.Equal(0, charsWritten);
    }

    [Fact]
    public void TryFormat_DestinationTooSmall_ReturnsFalse()
    {
        Span<char> destination = stackalloc char[2];

        var formatted = CompactFormatter.TryFormat(12345d, NumberRounding.Nearest, destination, out var charsWritten);

        Assert.False(formatted);
        Assert.Equal(0, charsWritten);
    }

    [Fact]
    public void TryFormat_AfterWarmUp_DoesNotAllocate()
    {
        Span<char> destination = stackalloc char[CompactFormatter.MaxLength];
        Write(destination);

        var before = GC.GetAllocatedBytesForCurrentThread();
        Write(destination);
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        Assert.Equal(0, allocated);
    }

    private static string Format(double value)
    {
        return Format(value, NumberRounding.Nearest);
    }

    private static string Format(double value, NumberRounding rounding)
    {
        Span<char> destination = stackalloc char[CompactFormatter.MaxLength];
        return CompactFormatter.TryFormat(value, rounding, destination, out var charsWritten) ? destination.Slice(0, charsWritten).ToString() : string.Empty;
    }

    private static void Write(Span<char> destination)
    {
        Assert.True(CompactFormatter.TryFormat(999d, NumberRounding.Nearest, destination, out _));
        Assert.True(CompactFormatter.TryFormat(-12345d, NumberRounding.Floor, destination, out _));
        Assert.True(CompactFormatter.TryFormat(1e15d, NumberRounding.Ceiling, destination, out _));
        Assert.True(CompactFormatter.TryFormat(1.5d, 400L, NumberRounding.Nearest, destination, out _));
    }
}
