using System;
using System.Globalization;

namespace GroveGames.Globalization;

public static class CompactFormatter
{
    public const int MaxLength = 32;

    private const double CompactThreshold = 10_000d;
    private const long CompactExponent = 4L;
    private const long GroupDigits = 3L;
    private const int CompactDigits = 3;
    private const double CompactLimit = 1_000d;
    private const int NamedUnitCount = 4;
    private const int FirstLetterCount = 2;
    private const int AlphabetLength = 26;
    private const int MaxLetterCount = 14;
    internal const long MaxExponent = long.MaxValue / 2L;
    private const double MidpointTolerance = 1e-9d;

    private static readonly char[] s_namedUnits = { 'K', 'M', 'B', 'T' };
    private static readonly double[] s_scales = { 1e-3d, 1e-2d, 1e-1d, 1e0d, 1e1d, 1e2d };
    private static readonly double[] s_powersOfTen = { 1e0d, 1e1d, 1e2d, 1e3d };

    public static string Format(int value, NumberRounding rounding)
    {
        return Format((double)value, rounding);
    }

    public static string Format(long value, NumberRounding rounding)
    {
        return Format((double)value, rounding);
    }

    public static string Format(float value, NumberRounding rounding)
    {
        return Format((double)value, rounding);
    }

    public static string Format(double value, NumberRounding rounding)
    {
        Span<char> buffer = stackalloc char[MaxLength];
        return TryFormat(value, rounding, buffer, out var written) ? new string(buffer.Slice(0, written)) : value.ToString(CultureInfo.InvariantCulture);
    }

    public static bool TryFormat(double value, NumberRounding rounding, Span<char> destination, out int charsWritten)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            return value.TryFormat(destination, out charsWritten, default, CultureInfo.InvariantCulture);
        }

        var magnitude = Math.Abs(value);

        if (magnitude < CompactThreshold)
        {
            var writer = new FormatWriter(destination);

            if (TryWritePlain(ref writer, value < 0d, magnitude, GetMagnitudeRounding(rounding, value < 0d)))
            {
                return writer.Complete(out charsWritten);
            }
        }

        var exponent = (long)Math.Floor(Math.Log10(magnitude));
        return TryFormat(value / Math.Pow(10d, exponent), exponent, rounding, destination, out charsWritten);
    }

    public static bool TryFormat(double mantissa, long exponent, NumberRounding rounding, Span<char> destination, out int charsWritten)
    {
        if (double.IsNaN(mantissa) || double.IsInfinity(mantissa))
        {
            return mantissa.TryFormat(destination, out charsWritten, default, CultureInfo.InvariantCulture);
        }

        var writer = new FormatWriter(destination);
        var negative = mantissa < 0d;
        var magnitude = Math.Abs(mantissa);
        var magnitudeRounding = GetMagnitudeRounding(rounding, negative);

        if (magnitude == 0d)
        {
            writer.Write('0');
            return writer.Complete(out charsWritten);
        }

        if (exponent > MaxExponent || exponent < -MaxExponent)
        {
            charsWritten = 0;
            return false;
        }

        Normalize(ref magnitude, ref exponent);

        if (exponent < CompactExponent)
        {
            if (TryWritePlain(ref writer, negative, magnitude * Math.Pow(10d, exponent), magnitudeRounding))
            {
                return writer.Complete(out charsWritten);
            }

            magnitude = 1d;
            exponent = CompactExponent;
        }

        var group = exponent / GroupDigits;
        var scaled = magnitude * s_scales[(int)(exponent - group * GroupDigits) + 3];
        var decimals = GetDecimals(scaled, CompactDigits, CompactDigits - 1);
        var units = RoundToUnits(scaled, decimals, magnitudeRounding);

        if (units >= CompactLimit * s_powersOfTen[decimals])
        {
            group++;
            scaled *= s_scales[0];
            decimals = GetDecimals(scaled, CompactDigits, CompactDigits - 1);
            units = RoundToUnits(scaled, decimals, magnitudeRounding);
        }

        if (negative)
        {
            writer.Write('-');
        }

        WriteFixed(ref writer, units, decimals);

        if (!TryWriteUnit(ref writer, group))
        {
            charsWritten = 0;
            return false;
        }

        return writer.Complete(out charsWritten);
    }

    private static bool TryWritePlain(ref FormatWriter writer, bool negative, double magnitude, NumberRounding rounding)
    {
        var units = RoundToUnits(magnitude, 0, rounding);

        if (units >= CompactThreshold)
        {
            return false;
        }

        if (negative && units > 0d)
        {
            writer.Write('-');
        }

        writer.Write((long)units);
        return true;
    }

    internal static void Normalize(ref double magnitude, ref long exponent)
    {
        while (magnitude >= 10d)
        {
            magnitude /= 10d;
            exponent++;
        }

        while (magnitude < 1d)
        {
            magnitude *= 10d;
            exponent--;
        }
    }

    private static bool TryWriteUnit(ref FormatWriter writer, long group)
    {
        if (group <= NamedUnitCount)
        {
            writer.Write(s_namedUnits[group - 1]);
            return true;
        }

        var index = group - NamedUnitCount - 1;
        var letterCount = FirstLetterCount;
        var combinations = (long)AlphabetLength * AlphabetLength;

        while (index >= combinations && combinations <= long.MaxValue / AlphabetLength)
        {
            index -= combinations;
            letterCount++;
            combinations *= AlphabetLength;
        }

        if (index >= combinations)
        {
            return false;
        }

        Span<char> letters = stackalloc char[MaxLetterCount];

        for (var i = letterCount - 1; i >= 0; i--)
        {
            letters[i] = (char)('a' + index % AlphabetLength);
            index /= AlphabetLength;
        }

        for (var i = 0; i < letterCount; i++)
        {
            writer.Write(letters[i]);
        }

        return true;
    }

    private static int GetDecimals(double magnitude, int digits, int maxDecimals)
    {
        var wholeDigits = 1;

        while (wholeDigits < digits && magnitude >= s_powersOfTen[wholeDigits])
        {
            wholeDigits++;
        }

        return Math.Min(maxDecimals, digits - wholeDigits);
    }

    private static NumberRounding GetMagnitudeRounding(NumberRounding rounding, bool negative)
    {
        if (!negative)
        {
            return rounding;
        }

        return rounding switch
        {
            NumberRounding.Floor => NumberRounding.Ceiling,
            NumberRounding.Ceiling => NumberRounding.Floor,
            _ => rounding
        };
    }

    internal static double RoundToUnits(double value, int decimals, NumberRounding rounding)
    {
        var units = value * s_powersOfTen[decimals];

        return rounding switch
        {
            NumberRounding.Floor => Math.Floor(units + MidpointTolerance),
            NumberRounding.Ceiling => Math.Ceiling(units - MidpointTolerance),
            _ => Math.Round(units + MidpointTolerance, MidpointRounding.AwayFromZero)
        };
    }

    private static void WriteFixed(ref FormatWriter writer, double units, int decimals)
    {
        var total = (long)units;
        var divisor = (long)s_powersOfTen[decimals];
        var fraction = total % divisor;
        var fractionDigits = decimals;

        writer.Write(total / divisor);

        while (fractionDigits > 0 && fraction % 10 == 0)
        {
            fraction /= 10;
            fractionDigits--;
        }

        if (fractionDigits == 0)
        {
            return;
        }

        writer.Write('.');

        for (var digit = fractionDigits - 1; digit >= 0; digit--)
        {
            writer.Write((char)('0' + fraction / (long)s_powersOfTen[digit] % 10));
        }
    }
}
