namespace GroveGames.Globalization.Tests;

public sealed class TextInterpolatedStringHandlerTests
{
    [Fact]
    public void Written_LiteralOnly_ReturnsTheLiteral()
    {
        var text = Format($"No batteries left");

        Assert.Equal("No batteries left", text);
    }

    [Fact]
    public void Written_Integers_WritesThemBetweenTheLiterals()
    {
        var stage = 12;
        var minutes = 5L;

        var text = Format($"Reach Stage {stage} in {minutes} minutes");

        Assert.Equal("Reach Stage 12 in 5 minutes", text);
    }

    [Fact]
    public void Written_FloatingPoint_UsesTheInvariantCulture()
    {
        var ratio = 1.5f;
        var total = 2.25d;

        var text = Format($"{ratio} of {total}");

        Assert.Equal("1.5 of 2.25", text);
    }

    [Fact]
    public void Written_Format_AppliesTheFormat()
    {
        var ratio = 1.5f;
        var count = 7;

        var text = Format($"{ratio:F2} and {count:D3}");

        Assert.Equal("1.50 and 007", text);
    }

    [Fact]
    public void Written_StringSpanAndChar_WritesThemAsText()
    {
        var name = "Stash";
        ReadOnlySpan<char> amount = "12.3K";
        var unit = 'x';

        var text = Format($"{name}: {amount}{unit}");

        Assert.Equal("Stash: 12.3Kx", text);
    }

    [Fact]
    public void Written_NullString_WritesNothing()
    {
        string? name = null;

        var text = Format($"[{name}]");

        Assert.Equal("[]", text);
    }

    [Fact]
    public void Written_ValueLongerThanTheFirstBuffer_GrowsAndKeepsEverything()
    {
        var value = new string('a', 4096);

        var text = Format($"<{value}>");

        Assert.Equal(4098, text.Length);
        Assert.StartsWith("<aaa", text);
        Assert.EndsWith("aaa>", text);
    }

    [Fact]
    public void Written_OtherSpanFormattable_WritesTheValue()
    {
        var id = new Guid("0f8fad5b-d9cb-469f-a165-70867728950e");

        var text = Format($"id {id}");

        Assert.Equal("id 0f8fad5b-d9cb-469f-a165-70867728950e", text);
    }

    [Fact]
    public void Dispose_AfterWriting_ClearsTheText()
    {
        var handler = new TextInterpolatedStringHandler(4, 0);
        handler.AppendLiteral("text");

        handler.Dispose();

        Assert.Equal(0, handler.Written.Length);
    }

    [Fact]
    public void Written_StackSpan_WritesTheCharacters()
    {
        Span<char> amount = stackalloc char[CompactFormatter.MaxLength];
        CompactFormatter.TryFormat(12345d, NumberRounding.Floor, amount, out var length);

        var text = Format($"Costs {amount.Slice(0, length)}");

        Assert.Equal("Costs 12.3K", text);
    }

    [Fact]
    public void Written_EmptyLiteralAndSpan_WritesNothing()
    {
        var handler = new TextInterpolatedStringHandler(0, 1);

        handler.AppendLiteral(string.Empty);
        handler.AppendFormatted(ReadOnlySpan<char>.Empty);

        Assert.Equal(0, handler.Written.Length);
        handler.Dispose();
    }

    [Fact]
    public void Dispose_CalledTwice_DoesNotThrow()
    {
        var handler = new TextInterpolatedStringHandler(4, 0);
        handler.AppendLiteral("text");

        handler.Dispose();
        handler.Dispose();

        Assert.Equal(0, handler.Written.Length);
    }

    [Fact]
    public void Default_WrittenAndDispose_AreEmptyAndSafe()
    {
        var handler = default(TextInterpolatedStringHandler);

        Assert.Equal(0, handler.Written.Length);
        handler.Dispose();
    }

    [Fact]
    public void Default_Append_RentsABuffer()
    {
        var handler = default(TextInterpolatedStringHandler);

        handler.AppendLiteral("Stage ");
        handler.AppendFormatted(7);

        Assert.Equal("Stage 7", handler.Written.ToString());
        handler.Dispose();
    }

    private static string Format(TextInterpolatedStringHandler text)
    {
        var result = text.Written.ToString();
        text.Dispose();
        return result;
    }
}
