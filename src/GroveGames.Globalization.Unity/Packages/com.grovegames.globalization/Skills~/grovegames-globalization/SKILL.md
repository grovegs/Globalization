---
name: grovegames-globalization
description: Rules for formatting numbers, percentages, durations and Roman numerals for the player with GroveGames.Globalization. Use when writing or reviewing code that turns a value into text for a label, such as a price, a balance, a bonus, a timer, a countdown or a tier.
---

# Using GroveGames.Globalization

Rules for game code that uses this package. They hold in every game. The API is in the package's `README.md`.

- Every number, percentage and duration the player sees goes through a formatter of this package. Never call `ToString` with a format string, and never write a suffix table in game code.
- For text that changes often, such as a timer or a balance, call `TryFormat` into a `stackalloc char[X.MaxLength]` buffer and hand the span to the label. `Format` allocates a string; use it only for text that is set once.
- Pick the rounding by meaning: `Floor` for what the player owns, `Ceiling` for what the player must pay, `Nearest` for everything else. A balance must never look larger than it is.
- `PercentFormatter` takes a fraction: pass `0.05` for 5%.
- `DurationStyle.Digital` is for a running clock, `DurationStyle.Narrow` for a remaining time or a length.
- Format only when the value changed, not every frame.

## Unity

- The formatters are in the `GroveGames.Globalization` namespace. The Unity package adds no types.
- Format a `BigNumber` with `BigNumberCompactFormatter` and `BigNumberPercentFormatter` from GroveGames.Mathematics. Never convert it to `double` first.
- An asmdef that overrides references lists `GroveGames.Globalization.dll` in its precompiled references.
