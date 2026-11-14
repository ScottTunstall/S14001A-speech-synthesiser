# Building the Solution

[Back to the README](../README.md)

Needs the .NET 10 SDK and Windows (the .NET Framework 4.8 target, the speech voices and audio playback are
all Windows-only).

```console
dotnet build              :: both target frameworks
dotnet test               :: 156 tests on each framework
```

The programs land in `src\S14001ASpeak\bin\Debug\net48\` and `src\S14001ASpeak\bin\Debug\net10.0\`. Both
produce byte-identical speech files.

All compiler, .NET analyzer (`AnalysisLevel` `latest-all`), IDE code-style and
[Roslynator](https://github.com/dotnet/roslynator) diagnostics are build errors. The house rules
(Clean Code and SOLID, command/query separation, one type per file, enums and constants in their own
files, principle of least surprise) are recorded at the top of [`.editorconfig`](../.editorconfig).

The tests build synthetic ROM banks and signals, so they need neither ROMs nor a speech voice.

`InvariantGlobalization` must stay off: `System.Speech` on .NET 10 matches voices by culture and fails with a
`NullReferenceException` without culture data.
