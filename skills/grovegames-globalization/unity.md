## Unity

- The formatters are in the `GroveGames.Globalization` namespace. `GroveGames.Globalization.Unity` holds only the `BigNumber` formatters.
- Format a `BigNumber` with `BigNumberCompactFormatter` and `BigNumberPercentFormatter`. Never convert it to `double` first.
- An asmdef that overrides references lists `GroveGames.Globalization.dll` in its precompiled references.
