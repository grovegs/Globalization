namespace GroveGames.Globalization.Tests;

public sealed class PercentFormatterTests
{
    [Theory]
    [InlineData(0d, 0L, "0%")]
    [InlineData(5d, -2L, "5%")]
    [InlineData(1.576d, -1L, "15.76%")]
    [InlineData(1.5d, 0L, "150%")]
    [InlineData(-5d, -2L, "-5%")]
    [InlineData(9.9994d, 1L, "9999.4%")]
    [InlineData(9.9999996d, 1L, "10K%")]
    [InlineData(1d, 2L, "10K%")]
    [InlineData(1.234d, 3L, "123K%")]
    [InlineData(1.9d, 6L, "190M%")]
    [InlineData(1.5d, 13L, "1.5aa%")]
    public void TryFormat_MantissaAndExponent_WritesThePercentage(double mantissa, long exponent, string expected)
    {
        Span<char> destination = stackalloc char[PercentFormatter.MaxLength];

        var formatted = PercentFormatter.TryFormat(mantissa, exponent, destination, out var charsWritten);

        Assert.True(formatted);
        Assert.Equal(expected, destination.Slice(0, charsWritten).ToString());
    }

    [Theory]
    [InlineData(150d, -2L, "150%")]
    [InlineData(0.5d, -1L, "5%")]
    [InlineData(1900d, 3L, "190M%")]
    public void TryFormat_MantissaNotNormalized_WritesTheSamePercentage(double mantissa, long exponent, string expected)
    {
        Span<char> destination = stackalloc char[PercentFormatter.MaxLength];

        var formatted = PercentFormatter.TryFormat(mantissa, exponent, destination, out var charsWritten);

        Assert.True(formatted);
        Assert.Equal(expected, destination.Slice(0, charsWritten).ToString());
    }

    [Fact]
    public void TryFormat_MantissaAndExponentDestinationTooSmall_ReturnsFalse()
    {
        Span<char> destination = stackalloc char[4];

        var formatted = PercentFormatter.TryFormat(1.9d, 6L, destination, out var charsWritten);

        Assert.False(formatted);
        Assert.Equal(0, charsWritten);
    }

    [Theory]
    [InlineData(0.05d, "5%")]
    [InlineData(0.1576d, "15.76%")]
    [InlineData(1.5d, "150%")]
    [InlineData(99.9999d, "9999.99%")]
    [InlineData(150d, "15K%")]
    [InlineData(-150d, "-15K%")]
    [InlineData(-0.05d, "-5%")]
    public void TryFormat_Fraction_WritesThePercentage(double value, string expected)
    {
        Span<char> destination = stackalloc char[PercentFormatter.MaxLength];

        var formatted = PercentFormatter.TryFormat(value, destination, out var charsWritten);

        Assert.True(formatted);
        Assert.Equal(expected, destination.Slice(0, charsWritten).ToString());
    }

    [Theory]
    [InlineData(150d, 1.5d, 2L)]
    [InlineData(99.99996d, 9.999996d, 1L)]
    [InlineData(-150d, -1.5d, 2L)]
    [InlineData(1e16d, 1d, 16L)]
    [InlineData(1e40d, 1d, 40L)]
    public void TryFormat_FractionOfTenThousandPercentOrMore_MatchesTheMantissaForm(double value, double mantissa, long exponent)
    {
        Span<char> fraction = stackalloc char[PercentFormatter.MaxLength];
        Span<char> mantissaForm = stackalloc char[PercentFormatter.MaxLength];

        var fractionFormatted = PercentFormatter.TryFormat(value, fraction, out var fractionWritten);
        var mantissaFormatted = PercentFormatter.TryFormat(mantissa, exponent, mantissaForm, out var mantissaWritten);

        Assert.True(fractionFormatted);
        Assert.True(mantissaFormatted);
        Assert.Equal(mantissaForm.Slice(0, mantissaWritten).ToString(), fraction.Slice(0, fractionWritten).ToString());
    }

    [Fact]
    public void Format_HugeFraction_WritesACompactPercentage()
    {
        var text = PercentFormatter.Format(1e40d);

        Assert.Equal("1aj%", text);
    }

    [Fact]
    public void TryFormat_AfterWarmUp_DoesNotAllocate()
    {
        Span<char> destination = stackalloc char[PercentFormatter.MaxLength];
        Write(destination);

        var before = GC.GetAllocatedBytesForCurrentThread();
        Write(destination);
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        Assert.Equal(0, allocated);
    }

    private static void Write(Span<char> destination)
    {
        Assert.True(PercentFormatter.TryFormat(0.1576d, destination, out _));
        Assert.True(PercentFormatter.TryFormat(0.05f, destination, out _));
        Assert.True(PercentFormatter.TryFormat(150d, destination, out _));
        Assert.True(PercentFormatter.TryFormat(1.9d, 6L, destination, out _));
    }

    [Fact]
    public void Format_FloatThatOverflowsAtOneHundredTimes_WritesACompactPercentage()
    {
        var text = PercentFormatter.Format(1e37f);

        Assert.EndsWith("%", text);
        Assert.DoesNotContain("Infinity", text);
    }

    [Fact]
    public void Format_DoubleThatOverflowsAtOneHundredTimes_WritesACompactPercentage()
    {
        var text = PercentFormatter.Format(double.MaxValue);

        Assert.EndsWith("%", text);
        Assert.DoesNotContain("Infinity", text);
    }
}
