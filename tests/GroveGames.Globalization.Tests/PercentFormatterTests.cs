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
    [InlineData(-0.05d, "-5%")]
    public void TryFormat_Fraction_WritesThePercentage(double value, string expected)
    {
        Span<char> destination = stackalloc char[PercentFormatter.MaxLength];

        var formatted = PercentFormatter.TryFormat(value, destination, out var charsWritten);

        Assert.True(formatted);
        Assert.Equal(expected, destination.Slice(0, charsWritten).ToString());
    }
}
