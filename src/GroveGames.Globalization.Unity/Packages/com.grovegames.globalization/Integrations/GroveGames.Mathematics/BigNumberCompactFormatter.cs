using System;
using GroveGames.Mathematics;

namespace GroveGames.Globalization.Unity
{
    public static class BigNumberCompactFormatter
    {
        public const int MaxLength = CompactFormatter.MaxLength;

        public static bool TryFormat(BigNumber value, NumberRounding rounding, Span<char> destination, out int charsWritten)
        {
            return CompactFormatter.TryFormat(value.Mantissa, value.Exponent, rounding, destination, out charsWritten);
        }
    }
}
