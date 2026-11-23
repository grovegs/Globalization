using System;
using System.Globalization;

namespace GroveGames.Globalization;

public static class PercentFormatter
{
    public const int MaxLength = 32;

    private const double MaxWritableHundredths = 1e18d;
    private const long PercentExponent = 2L;
    private const long CompactExponent = 4L;
    private const double CompactThreshold = 10_000d;
    private const int PlainDecimals = 2;
    private const char PercentSign = '%';

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

    public static bool TryFormat(double mantissa, long exponent, Span<char> destination, out int charsWritten)
    {
        if (mantissa == 0d || double.IsNaN(mantissa) || double.IsInfinity(mantissa))
        {
            return TryFormat(mantissa, destination, out charsWritten);
        }

        if (exponent > CompactFormatter.MaxExponent || exponent < -CompactFormatter.MaxExponent)
        {
            charsWritten = 0;
            return false;
        }

        var magnitude = Math.Abs(mantissa);
        CompactFormatter.Normalize(ref magnitude, ref exponent);
        var normalized = mantissa < 0d ? -magnitude : magnitude;
        var percentExponent = exponent + PercentExponent;

        if (percentExponent < CompactExponent && Math.Round(magnitude * Math.Pow(10d, percentExponent), PlainDecimals, MidpointRounding.AwayFromZero) < CompactThreshold)
        {
            return TryFormat(normalized * Math.Pow(10d, exponent), destination, out charsWritten);
        }

        if (!CompactFormatter.TryFormat(normalized, percentExponent, NumberRounding.Nearest, destination, out var written) || written >= destination.Length)
        {
            charsWritten = 0;
            return false;
        }

        destination[written] = PercentSign;
        charsWritten = written + 1;
        return true;
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
