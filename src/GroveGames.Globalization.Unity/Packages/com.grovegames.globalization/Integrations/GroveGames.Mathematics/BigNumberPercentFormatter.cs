using System;
using GroveGames.Mathematics;

namespace GroveGames.Globalization.Unity
{
    public static class BigNumberPercentFormatter
    {
        public const int MaxLength = CompactFormatter.MaxLength + 1;

        private const double PercentScale = 100d;
        private const long CompactExponent = 4L;
        private const double CompactThreshold = 10_000d;
        private const int PlainDecimals = 2;
        private const char PercentSign = '%';

        public static bool TryFormat(BigNumber value, Span<char> destination, out int charsWritten)
        {
            var percentage = value * PercentScale;

            if (percentage.Exponent < CompactExponent && Math.Round(Math.Abs(percentage.ToDouble()), PlainDecimals, MidpointRounding.AwayFromZero) < CompactThreshold)
            {
                return PercentFormatter.TryFormat(value.ToDouble(), destination, out charsWritten);
            }

            if (!CompactFormatter.TryFormat(percentage.Mantissa, percentage.Exponent, NumberRounding.Nearest, destination, out var written) || written >= destination.Length)
            {
                charsWritten = 0;
                return false;
            }

            destination[written] = PercentSign;
            charsWritten = written + 1;
            return true;
        }
    }
}
