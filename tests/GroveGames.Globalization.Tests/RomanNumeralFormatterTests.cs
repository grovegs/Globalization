namespace GroveGames.Globalization.Tests;

public sealed class RomanNumeralFormatterTests
{
    [Theory]
    [InlineData(0, "")]
    [InlineData(4, "IV")]
    [InlineData(3999, "MMMCMXCIX")]
    public void TryFormat_Value_WritesTheNumeral(int value, string expected)
    {
        Span<char> destination = stackalloc char[16];

        var formatted = RomanNumeralFormatter.TryFormat(value, destination, out var charsWritten);

        Assert.True(formatted);
        Assert.Equal(expected, destination.Slice(0, charsWritten).ToString());
    }

    [Fact]
    public void TryFormat_AfterWarmUp_DoesNotAllocate()
    {
        Span<char> destination = stackalloc char[16];
        RomanNumeralFormatter.TryFormat(3999, destination, out _);

        var before = GC.GetAllocatedBytesForCurrentThread();
        var formatted = RomanNumeralFormatter.TryFormat(3999, destination, out _);
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        Assert.True(formatted);
        Assert.Equal(0, allocated);
    }
}
