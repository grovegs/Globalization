using System;
using System.Globalization;

namespace GroveGames.Globalization;

internal ref struct FormatWriter
{
    private readonly Span<char> _destination;
    private int _position;
    private bool _overflowed;

    public int Position => _position;
    public bool Overflowed => _overflowed;

    public FormatWriter(Span<char> destination)
    {
        _destination = destination;
        _position = 0;
        _overflowed = false;
    }

    public void Write(char value)
    {
        if (_position >= _destination.Length)
        {
            _overflowed = true;
            return;
        }

        _destination[_position++] = value;
    }

    public void Write(string value)
    {
        for (var i = 0; i < value.Length; i++)
        {
            Write(value[i]);
        }
    }

    public void Write(long value)
    {
        if (!value.TryFormat(_destination.Slice(Math.Min(_position, _destination.Length)), out var written, default, CultureInfo.InvariantCulture))
        {
            _overflowed = true;
            return;
        }

        _position += written;
    }

    public void WriteTwoDigits(int value)
    {
        Write((char)('0' + value / 10 % 10));
        Write((char)('0' + value % 10));
    }

    public bool Complete(out int charsWritten)
    {
        charsWritten = _overflowed ? 0 : _position;
        return !_overflowed;
    }
}
