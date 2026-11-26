using System;

namespace GroveGames.Globalization.Tests;

public sealed class DurationFormatterTests
{
    [Theory]
    [InlineData(0, "00:00")]
    [InlineData(-5, "00:00")]
    [InlineData(55, "00:55")]
    [InlineData(90, "01:30")]
    [InlineData(3725, "01:02:05")]
    [InlineData(93600, "1d 2h")]
    public void TryFormat_Digital_WritesMinutesAndSeconds(int seconds, string expected)
    {
        Span<char> destination = stackalloc char[DurationFormatter.MaxLength];

        var formatted = DurationFormatter.TryFormat(seconds, DurationStyle.Digital, destination, out var charsWritten);

        Assert.True(formatted);
        Assert.Equal(expected, destination.Slice(0, charsWritten).ToString());
    }

    [Theory]
    [InlineData(0, "0s")]
    [InlineData(-5, "0s")]
    [InlineData(55, "55s")]
    [InlineData(60, "1m")]
    [InlineData(90, "1m 30s")]
    [InlineData(3600, "1h")]
    [InlineData(29520, "8h 12m")]
    [InlineData(93600, "1d 2h")]
    public void TryFormat_Narrow_WritesTheTwoLargestUnits(int seconds, string expected)
    {
        Span<char> destination = stackalloc char[DurationFormatter.MaxLength];

        var formatted = DurationFormatter.TryFormat(seconds, DurationStyle.Narrow, destination, out var charsWritten);

        Assert.True(formatted);
        Assert.Equal(expected, destination.Slice(0, charsWritten).ToString());
    }

    [Theory]
    [InlineData(0.5f, "0.5s")]
    [InlineData(0.55f, "0.55s")]
    [InlineData(1.15f, "1.15s")]
    [InlineData(1.25f, "1.25s")]
    [InlineData(1.05f, "1.05s")]
    [InlineData(0.999f, "1s")]
    [InlineData(2f, "2s")]
    [InlineData(0f, "0s")]
    [InlineData(-3f, "0s")]
    [InlineData(90.04f, "90.04s")]
    public void TryFormatSeconds_FloatSeconds_WritesHundredths(float seconds, string expected)
    {
        Span<char> destination = stackalloc char[DurationFormatter.MaxLength];

        var formatted = DurationFormatter.TryFormatSeconds(seconds, destination, out var charsWritten);

        Assert.True(formatted);
        Assert.Equal(expected, destination.Slice(0, charsWritten).ToString());
    }

    [Fact]
    public void TryFormatSeconds_DestinationTooSmall_ReturnsFalse()
    {
        Span<char> destination = stackalloc char[2];

        var formatted = DurationFormatter.TryFormatSeconds(1.5f, destination, out var charsWritten);

        Assert.False(formatted);
        Assert.Equal(0, charsWritten);
    }

    [Fact]
    public void FormatSeconds_FloatSeconds_MatchesTryFormatSeconds()
    {
        var text = DurationFormatter.FormatSeconds(0.756f);

        Assert.Equal("0.76s", text);
    }

    [Theory]
    [InlineData(5, "5s")]
    [InlineData(-1, "0s")]
    public void TryFormatSeconds_IntSeconds_WritesWholeSeconds(int seconds, string expected)
    {
        Span<char> destination = stackalloc char[DurationFormatter.MaxLength];

        var formatted = DurationFormatter.TryFormatSeconds(seconds, destination, out var charsWritten);

        Assert.True(formatted);
        Assert.Equal(expected, destination.Slice(0, charsWritten).ToString());
    }

    [Fact]
    public void TryFormat_AfterWarmUp_DoesNotAllocate()
    {
        Span<char> destination = stackalloc char[DurationFormatter.MaxLength];
        Write(destination);

        var before = GC.GetAllocatedBytesForCurrentThread();
        Write(destination);
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        Assert.Equal(0, allocated);
    }

    private static void Write(Span<char> destination)
    {
        Assert.True(DurationFormatter.TryFormat(3725, DurationStyle.Digital, destination, out _));
        Assert.True(DurationFormatter.TryFormat(90061, DurationStyle.Narrow, destination, out _));
        Assert.True(DurationFormatter.TryFormatSeconds(1.25f, destination, out _));
        Assert.True(DurationFormatter.TryFormatSeconds(42, destination, out _));
    }
}
