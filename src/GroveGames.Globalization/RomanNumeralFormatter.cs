using System;

namespace GroveGames.Globalization;

public static class RomanNumeralFormatter
{
    private static readonly int[] s_values = { 1000, 900, 500, 400, 100, 90, 50, 40, 10, 9, 5, 4, 1 };
    private static readonly string[] s_numerals = { "M", "CM", "D", "CD", "C", "XC", "L", "XL", "X", "IX", "V", "IV", "I" };

    public static string Format(int value)
    {
        if (value <= 0)
        {
            return string.Empty;
        }

        return string.Create(GetLength(value), value, static (span, number) => Write(span, number));
    }

    public static bool TryFormat(int value, Span<char> destination, out int charsWritten)
    {
        if (value <= 0)
        {
            charsWritten = 0;
            return true;
        }

        var length = GetLength(value);

        if (length > destination.Length)
        {
            charsWritten = 0;
            return false;
        }

        Write(destination, value);
        charsWritten = length;
        return true;
    }

    public static int GetLength(int value)
    {
        var length = 0;

        for (var i = 0; i < s_values.Length && value > 0; i++)
        {
            var count = value / s_values[i];
            length += count * s_numerals[i].Length;
            value -= count * s_values[i];
        }

        return length;
    }

    private static void Write(Span<char> destination, int value)
    {
        var position = 0;

        for (var i = 0; i < s_values.Length && value > 0; i++)
        {
            var numeral = s_numerals[i];

            while (value >= s_values[i])
            {
                for (var j = 0; j < numeral.Length; j++)
                {
                    destination[position++] = numeral[j];
                }

                value -= s_values[i];
            }
        }
    }
}
