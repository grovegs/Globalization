# GroveGames.Globalization

[![Build Status](https://github.com/grovegs/Globalization/actions/workflows/release.yml/badge.svg)](https://github.com/grovegs/Globalization/actions/workflows/release.yml)
[![Tests](https://github.com/grovegs/Globalization/actions/workflows/tests.yml/badge.svg)](https://github.com/grovegs/Globalization/actions/workflows/tests.yml)
[![NuGet](https://img.shields.io/nuget/v/GroveGames.Globalization)](https://www.nuget.org/packages/GroveGames.Globalization)
[![Latest Release](https://img.shields.io/github/v/release/grovegs/Globalization)](https://github.com/grovegs/Globalization/releases/latest)

Compact numbers, percentages, durations and Roman numerals formatted without allocations for .NET and Unity

## Features

- `CompactFormatter`: `12345` becomes `12.3K`, with `K`, `M`, `B`, `T` and then letter suffixes such as `aa`
- `PercentFormatter`: a fraction becomes a percentage with up to two decimals
- `DurationFormatter`: seconds become `01:02:05` or `1m 30s`
- `RomanNumeralFormatter`: integers become Roman numerals
- Every formatter has a `TryFormat` that writes into a `Span<char>` and a `Format` that returns a string

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

The formatters are in the core namespace, `GroveGames.Globalization`. With [GroveGames.Mathematics](https://github.com/grovegs/Mathematics) installed, the Unity package adds `BigNumberCompactFormatter` and `BigNumberPercentFormatter` in `GroveGames.Globalization.Unity`:

```csharp
using GroveGames.Globalization;
using GroveGames.Globalization.Unity;

Span<char> buffer = stackalloc char[BigNumberCompactFormatter.MaxLength];
BigNumberCompactFormatter.TryFormat(balance, NumberRounding.Floor, buffer, out var written);
```

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
