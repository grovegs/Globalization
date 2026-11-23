using System;
using System.Globalization;

namespace GroveGames.Globalization;

public static class PercentFormatter
{
    public const int MaxLength = 32;

    private const double MaxWritableHundredths = 1e18d;

    public static string Format(int value)
    {
        return Format((double)value);
    }

    public static string Format(float value)
    {
        Span<char> buffer = stackalloc char[MaxLength];
        return TryFormat(value, buffer, out var written) ? new string(buffer.Slice(0, written)) : value.ToString("P2", CultureInfo.InvariantCulture);
    }

    public static string Format(double value)
    {
        Span<char> buffer = stackalloc char[MaxLength];
        return TryFormat(value, buffer, out var written) ? new string(buffer.Slice(0, written)) : value.ToString("P2", CultureInfo.InvariantCulture);
    }

    public static bool TryFormat(float value, Span<char> destination, out int charsWritten)
    {
        return TryFormatPercentage(value * 100f, destination, out charsWritten);
    }

    public static bool TryFormat(double value, Span<char> destination, out int charsWritten)
    {
        return TryFormatPercentage(value * 100d, destination, out charsWritten);
    }

    private static bool TryFormatPercentage(double percentage, Span<char> destination, out int charsWritten)
    {
        var hundredths = Math.Round(percentage * 100d, MidpointRounding.AwayFromZero);

        if (double.IsNaN(hundredths) || Math.Abs(hundredths) >= MaxWritableHundredths)
        {
            return percentage.TryFormat(destination, out charsWritten, "0.##", CultureInfo.InvariantCulture);
        }

        var writer = new FormatWriter(destination);
        var total = (long)hundredths;

        if (total < 0)
        {
            writer.Write('-');
            total = -total;
        }

        var whole = total / 100;
        var fraction = (int)(total % 100);

        writer.Write(whole);

        if (fraction != 0)
        {
            writer.Write('.');

            if (fraction % 10 == 0)
            {
                writer.Write((char)('0' + fraction / 10));
            }
            else
            {
                writer.WriteTwoDigits(fraction);
            }
        }

        writer.Write('%');
        return writer.Complete(out charsWritten);
    }
}
