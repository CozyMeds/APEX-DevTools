# APEX DevTools

A PlayStation 4 Black Ops III GSC compilation toolkit with a standalone CLI and a Windows Forms interface.

Designed for **PS4 BO3 1.33**. Runtime applications target **.NET Framework 4.8**.
The compiler core also targets .NET Standard 2.0 for testing and reuse.

![APEX Compiler GUI](assets/apex-compiler-gui.png?v=2)

The screenshot shows the earlier GUI; the current build also has a Cancel button.

## Download and installation

Download the [latest release](https://github.com/CozyMeds/APEX-DevTools/releases/latest).

| Package | Contents |
| --- | --- |
| APEX Compiler Bundle | CLI and GUI together; recommended |
| APEX Compiler CLI | Command-line compiler and its runtime dependencies |
| APEX Compiler GUI | Graphical interface and shared libraries; select a compatible CLI separately |

Extract the entire archive. Keep the supplied DLLs, configuration files and opcode
map together. Windows and the .NET Framework 4.8 runtime are required; Visual Studio
is not required to run a release.

In the bundle, launch `APEX.Compiler.GUI.exe`, select a source directory, choose a
`.gscc` output and click **COMPILE GSCC**. The GUI detects a sibling
`APEX.Compiler.exe`, or uses a configured compiler location. **CANCEL** stops the
current compiler process. Builds time out after two minutes; closing the window
also cancels an active build before exiting.

## Command-line usage

```powershell
.\APEX.Compiler.exe "C:\My Scripts" "C:\Builds\APEX.gscc"
.\APEX.Compiler.exe --check "C:\My Scripts"
.\APEX.Compiler.exe --check --json --define MY_FEATURE "C:\My Scripts"
.\APEX.Compiler.exe --version
.\APEX.Compiler.exe --help
```

Input can be a `.gsc` file or a directory. Directory builds recursively combine all
`.gsc` files in a stable path order into one script. Use a dedicated source folder;
there are currently no include/exclude globs or project manifests. `#include`
records a game-script dependency rather than loading an additional source file.

`--check` compiles and structurally validates the result without writing output.
`--json` writes one JSON result to stdout, including success, version, source paths,
output path, byte count and diagnostics. Syntax and preprocessor diagnostics report
the original filename and one-based line/column. Other compiler errors may have no
source position. `--define TOKEN` can be repeated; `BO3` is always defined.

Exit codes: **0** success/help/version, **1** compilation or I/O failure, **2** invalid
arguments. Existing output is replaced only after compilation, structural validation
and writing to a temporary file beside the destination succeed. Normal write and
replacement failures preserve the previous build. Abrupt power loss or process
termination can leave a temporary file behind.

Required CLI runtime files: `APEX.Compiler.exe`, `APEX.Compiler.exe.config`,
`APEX.Compiler.Core.dll`, `Irony.dll` and `apex_ps4_ops.bin`.

## Correctness changes in 1.2.0

- Reject out-of-range jumps, local/parameter counts, includes and serialized table counts.
- Preserve distinct import identities across namespaces, flags and argument counts.
- Diagnose recursive macros and limit expression expansion.
- Validate builtin signatures and discard unused builtin results correctly.
- Handle conditional identifiers with underscores, escaped backslashes and nested branches;
  reject duplicate `#else` and unmatched directives.
- Resolve trailing empty switch labels.
- Generate deterministic temporary names and use invariant identifier case folding.
- Share structural GSCC validation and atomic output replacement between the CLI and GUI.
- Bound GUI logs, support cancellation/timeouts and quote Windows paths correctly.

The bytecode supports at most 255 locals per function, including parameters and
compiler-generated temporaries, and 255 includes per script. Jump displacements
must fit a signed 16-bit value. Split large functions rather than relying on wrapped
counts or offsets. Macro nesting is limited to 128 and emission to two million steps.

## Build and test

Install the .NET SDK selected in `global.json` and the .NET Framework 4.8 developer
targeting pack on Windows. Visual Studio with .NET desktop development is optional.

```powershell
dotnet restore APEX.DevTools.sln --locked-mode
dotnet build APEX.DevTools.sln -c Release --no-restore
dotnet test tests/APEX.Compiler.Tests/APEX.Compiler.Tests.csproj -c Release -f net48 --no-build
dotnet test tests/APEX.Compiler.Tests/APEX.Compiler.Tests.csproj -c Release -f net10.0 --no-build
```

The .NET 10 tests exercise the portable core and the built CLI assembly. Native
Windows CI additionally tests .NET Framework and starts each packaged GUI.

```text
src/APEX.Compiler.Core/       Compiler, bytecode model, preprocessing and validation
src/APEX.Compiler/            CLI, argument parsing and structured output
src/APEX.Compiler.GUI/        Windows Forms interface
samples/Sample-APEX/          Sample script
tests/APEX.Compiler.Tests/    Compiler, CLI, preprocessing and output regression tests
scripts/                     Packaging and archive smoke tests
docs/                        Maintenance and release notes
```

## Package a release

`Directory.Build.props` is the source of truth for application and package versions.
Update it, rebuild the solution, then run on Windows:

```powershell
.\scripts\package.ps1
.\scripts\smoke-test-packages.ps1
```

Packaging refuses stale application versions. `artifacts/` contains CLI, GUI and
bundle ZIPs plus `SHA256SUMS.txt`. Every ZIP contains `manifest.json` with its source
commit, working-tree state and file hashes, as well as required notices. Packages
are tested after extraction. Packaging does not publish a GitHub release.

See [docs/RELEASING.md](docs/RELEASING.md) for release validation and rights status.

## Compatibility and credits

Compilation and structural validation do not guarantee successful in-game execution.
Other game versions are unvalidated. No PS4 injector or console payloads are included.
The GUI remains Windows-only. PS4 runtime verification must be performed separately.

Compiler source is derived from
[shiversoftdev/t7-compiler](https://github.com/shiversoftdev/t7-compiler) and
[someguythatmods/ps4-bo3-gsc-injector](https://github.com/someguythatmods/ps4-bo3-gsc-injector).
See [CREDITS_AND_RIGHTS.md](CREDITS_AND_RIGHTS.md) and
[THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md). Redistribution permissions for
inherited code and the opcode database remain unverified; this update grants none.

APEX DevTools is an independent community project and is not affiliated with
Activision, Treyarch or Sony Interactive Entertainment. Their trademarks remain
the property of their respective owners.
