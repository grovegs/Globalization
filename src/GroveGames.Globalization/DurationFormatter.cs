using System;

namespace GroveGames.Globalization;

public static class DurationFormatter
{
    public const int MaxLength = 32;

    private const int Minute = 60;
    private const int Hour = Minute * 60;
    private const int Day = Hour * 24;
    private const int Tenths = 10;
    private const int Hundredths = 100;

    public static string Format(float seconds, DurationStyle style)
    {
        return Format(ToSeconds(Math.Floor(seconds)), style);
    }

    public static string Format(double seconds, DurationStyle style)
    {
        return Format(ToSeconds(Math.Truncate(seconds)), style);
    }

    public static string Format(int seconds, DurationStyle style)
    {
        Span<char> buffer = stackalloc char[MaxLength];
        TryFormat(seconds, style, buffer, out var written);
        return new string(buffer.Slice(0, written));
    }

    public static bool TryFormat(int seconds, DurationStyle style, Span<char> destination, out int charsWritten)
    {
        seconds = Math.Max(seconds, 0);
        var writer = new FormatWriter(destination);

        if (seconds >= Day)
        {
            WriteUnits(ref writer, seconds / Day, 'd', seconds % Day / Hour, 'h');
        }
        else if (style == DurationStyle.Digital)
        {
            WriteDigital(ref writer, seconds);
        }
        else if (seconds >= Hour)
        {
            WriteUnits(ref writer, seconds / Hour, 'h', seconds % Hour / Minute, 'm');
        }
        else if (seconds >= Minute)
        {
            WriteUnits(ref writer, seconds / Minute, 'm', seconds % Minute, 's');
        }
        else
        {
            writer.Write(seconds);
            writer.Write('s');
        }

        return writer.Complete(out charsWritten);
    }

    public static string FormatSeconds(float seconds)
    {
        Span<char> buffer = stackalloc char[MaxLength];
        TryFormatSeconds(seconds, buffer, out var written);
        return new string(buffer.Slice(0, written));
    }

    public static string FormatSeconds(int seconds)
    {
        Span<char> buffer = stackalloc char[MaxLength];
        TryFormatSeconds(seconds, buffer, out var written);
        return new string(buffer.Slice(0, written));
    }

    public static bool TryFormatSeconds(int seconds, Span<char> destination, out int charsWritten)
    {
        var writer = new FormatWriter(destination);
        writer.Write(Math.Max(seconds, 0));
        writer.Write('s');
        return writer.Complete(out charsWritten);
    }

    public static bool TryFormatSeconds(float seconds, Span<char> destination, out int charsWritten)
    {
        var hundredths = (long)ToSeconds(Math.Round((double)seconds * Hundredths, MidpointRounding.AwayFromZero));
        var fraction = hundredths % Hundredths;
        var writer = new FormatWriter(destination);
        writer.Write(hundredths / Hundredths);

        if (fraction != 0L)
        {
            writer.Write('.');

            if (fraction % Tenths == 0L)
            {
                writer.Write((char)('0' + fraction / Tenths));
            }
            else
            {
                writer.WriteTwoDigits((int)fraction);
            }
        }

        writer.Write('s');
        return writer.Complete(out charsWritten);
    }

    private static void WriteDigital(ref FormatWriter writer, int seconds)
    {
        if (seconds >= Hour)
        {
            writer.WriteTwoDigits(seconds / Hour);
            writer.Write(':');
        }

        writer.WriteTwoDigits(seconds % Hour / Minute);
        writer.Write(':');
        writer.WriteTwoDigits(seconds % Minute);
    }

    private static void WriteUnits(ref FormatWriter writer, int major, char majorUnit, int minor, char minorUnit)
    {
        writer.Write(major);
        writer.Write(majorUnit);

        if (minor > 0)
        {
            writer.Write(' ');
            writer.Write(minor);
            writer.Write(minorUnit);
        }
    }

    private static int ToSeconds(double seconds)
    {
        if (double.IsNaN(seconds) || seconds <= 0d)
        {
            return 0;
        }

        return seconds >= int.MaxValue ? int.MaxValue : (int)seconds;
    }
}
