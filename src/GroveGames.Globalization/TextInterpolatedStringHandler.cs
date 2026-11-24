using System;
using System.Buffers;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace GroveGames.Globalization;

[InterpolatedStringHandler]
public ref struct TextInterpolatedStringHandler
{
    private const int FormattedLength = 16;

    private char[]? _buffer;
    private int _position;

    public readonly ReadOnlySpan<char> Written => _buffer.AsSpan(0, _position);

    private readonly int Capacity => _buffer?.Length ?? 0;

    public TextInterpolatedStringHandler(int literalLength, int formattedCount)
    {
        _buffer = ArrayPool<char>.Shared.Rent(literalLength + formattedCount * FormattedLength);
        _position = 0;
    }

    public void Dispose()
    {
        if (_buffer != null)
        {
            ArrayPool<char>.Shared.Return(_buffer);
            _buffer = null;
        }

        _position = 0;
    }

    public void AppendLiteral(string value)
    {
        Write(value.AsSpan());
    }

    public void AppendFormatted(scoped ReadOnlySpan<char> value)
    {
        Write(value);
    }

    public void AppendFormatted(string? value)
    {
        Write(value.AsSpan());
    }

    public void AppendFormatted(char value)
    {
        EnsureCapacity(1);
        _buffer![_position++] = value;
    }

    public void AppendFormatted(int value)
    {
        AppendFormatted(value, null);
    }

    public void AppendFormatted(int value, string? format)
    {
        int written;

        while (!value.TryFormat(_buffer.AsSpan(_position), out written, format.AsSpan(), CultureInfo.InvariantCulture))
        {
            Grow(Capacity + 1);
        }

        _position += written;
    }

    public void AppendFormatted(long value)
    {
        AppendFormatted(value, null);
    }

    public void AppendFormatted(long value, string? format)
    {
        int written;

        while (!value.TryFormat(_buffer.AsSpan(_position), out written, format.AsSpan(), CultureInfo.InvariantCulture))
        {
            Grow(Capacity + 1);
        }

        _position += written;
    }

    public void AppendFormatted(float value)
    {
        AppendFormatted(value, null);
    }

    public void AppendFormatted(float value, string? format)
    {
        int written;

        while (!value.TryFormat(_buffer.AsSpan(_position), out written, format.AsSpan(), CultureInfo.InvariantCulture))
        {
            Grow(Capacity + 1);
        }

        _position += written;
    }

    public void AppendFormatted(double value)
    {
        AppendFormatted(value, null);
    }

    public void AppendFormatted(double value, string? format)
    {
        int written;

        while (!value.TryFormat(_buffer.AsSpan(_position), out written, format.AsSpan(), CultureInfo.InvariantCulture))
        {
            Grow(Capacity + 1);
        }

        _position += written;
    }

#if NET6_0_OR_GREATER
    public void AppendFormatted<T>(T value) where T : ISpanFormattable
    {
        AppendFormatted(value, null);
    }

    public void AppendFormatted<T>(T value, string? format) where T : ISpanFormattable
    {
        int written;

        while (!value.TryFormat(_buffer.AsSpan(_position), out written, format, CultureInfo.InvariantCulture))
        {
            Grow(Capacity + 1);
        }

        _position += written;
    }
#else
    public void AppendFormatted<T>(T value)
    {
        AppendFormatted(value, null);
    }

    public void AppendFormatted<T>(T value, string? format)
    {
        var text = value switch
        {
            IFormattable formattable => formattable.ToString(format, CultureInfo.InvariantCulture),
            _ => value?.ToString()
        };
        Write(text.AsSpan());
    }
#endif

    private void Write(scoped ReadOnlySpan<char> value)
    {
        EnsureCapacity(value.Length);
        value.CopyTo(_buffer.AsSpan(_position));
        _position += value.Length;
    }

    private void EnsureCapacity(int count)
    {
        if (Capacity - _position < count)
        {
            Grow(_position + count);
        }
    }

    private void Grow(int minimumLength)
    {
        var buffer = ArrayPool<char>.Shared.Rent(Math.Max(minimumLength, Capacity * 2));

        if (_buffer != null)
        {
            _buffer.AsSpan(0, _position).CopyTo(buffer);
            ArrayPool<char>.Shared.Return(_buffer);
        }

        _buffer = buffer;
    }
}
