# GroveGames.Globalization

[![Build Status](https://github.com/grovegs/Globalization/actions/workflows/release.yml/badge.svg)](https://github.com/grovegs/Globalization/actions/workflows/release.yml)
[![Tests](https://github.com/grovegs/Globalization/actions/workflows/tests.yml/badge.svg)](https://github.com/grovegs/Globalization/actions/workflows/tests.yml)
[![NuGet](https://img.shields.io/nuget/v/GroveGames.Globalization)](https://www.nuget.org/packages/GroveGames.Globalization)
[![Latest Release](https://img.shields.io/github/v/release/grovegs/Globalization)](https://github.com/grovegs/Globalization/releases/latest)

Compact numbers, percentages, durations and Roman numerals formatted without allocations for .NET and Unity

## Features

- `CompactFormatter`: `12345` becomes `12.3K`, with `K`, `M`, `B`, `T` and then letter suffixes such as `aa`
- `PercentFormatter`: a fraction becomes a percentage with up to two decimals, and from 10,000% on a compact one such as `15K%`
- `DurationFormatter`: seconds become `01:02:05` or `1m 30s`
- `RomanNumeralFormatter`: integers become Roman numerals
- Every formatter has a `TryFormat` that writes into a `Span<char>` and a `Format` that returns a string
- `TextInterpolatedStringHandler`: lets a method take `$"Stage {number}"` and read it as a span from a pooled buffer, with no string built

## .NET

### Installation

```bash
dotnet add package GroveGames.Globalization
```

### Usage

```csharp
using GroveGames.Globalization;

var price = CompactFormatter.Format(12345, NumberRounding.Nearest);
var clock = DurationFormatter.Format(3725, DurationStyle.Digital);
var remaining = DurationFormatter.Format(90, DurationStyle.Narrow);
var bonus = PercentFormatter.Format(0.05f);
var tier = RomanNumeralFormatter.Format(4);

Span<char> buffer = stackalloc char[CompactFormatter.MaxLength];
CompactFormatter.TryFormat(12345d, NumberRounding.Floor, buffer, out var written);
```

A method that takes text can accept an interpolated string without a string being built. Read `Written`, then call `Dispose` to return the buffer:

```csharp
public void SetText(TextInterpolatedStringHandler text)
{
    SetText(text.Written);
    text.Dispose();
}

label.SetText($"Reach Stage {stage}");
```

`int`, `long`, `float`, `double`, `char`, strings and spans are written without allocating on every target. Other values are written without allocating on .NET 6 and later, and through `ToString` on `netstandard2.1`.

`CompactFormatter` and `PercentFormatter` also take a mantissa and a base-ten exponent, for values beyond the range of `double`:

```csharp
CompactFormatter.TryFormat(1.5d, 400L, NumberRounding.Nearest, buffer, out written);
PercentFormatter.TryFormat(1.9d, 6L, buffer, out written);
```

The output does not depend on the current culture. `TryFormat` returns `false` when the destination is too small; a buffer of the formatter's `MaxLength` always fits.

## Unity

### Installation

1. Install the core package via [NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity):
   - Open Window > NuGet > Manage NuGet Packages
   - Search for `GroveGames.Globalization` and install

2. Add the Unity package via Git URL:
   - Open Package Manager (Window > Package Manager)
   - Click "+" > "Add package from git URL"
   - Enter: `https://github.com/grovegs/Globalization.git?path=src/GroveGames.Globalization.Unity/Packages/com.grovegames.globalization`

3. Create a `csc.rsp` file in your `Assets/` directory with the following content to enable C# 10 features:

   ```text
   -langversion:10
   -nullable:enable
   ```

### Usage

All formatters are in the core namespace, `GroveGames.Globalization`. The Unity package adds no types; it makes the core available to asmdefs that reference `GroveGames.Globalization.Unity`.

## Development

### Project Structure

- `src/GroveGames.Globalization/` - Core .NET library (`net10.0`, `netstandard2.1`)
- `src/GroveGames.Globalization.Unity/` - Unity package
- `tests/` - xUnit tests
- `sandbox/` - Sample applications for testing

### Commands

```bash
dotnet build                        # Build all projects
dotnet format                       # Format code
dotnet test                         # Run tests
dotnet pack -c Release              # Create NuGet packages
```

### Requirements

- .NET 10.0 SDK

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
