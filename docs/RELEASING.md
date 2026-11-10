# Release maintenance

## Source and versioning

Application assembly/file versions, GUI version text, CLI `--version` and package
versions originate in `Directory.Build.props`. The SDK is selected in `global.json`;
NuGet resolution is recorded in each project's `packages.lock.json`. When changing
a dependency, regenerate and review the affected lock files and run the full suite.
Do not edit generated resources or settings files to change the product version.

## Validation

Windows CI restores in locked mode, builds with warnings treated as errors, runs
the net48 and net10.0 regression suites, packages all three distributions and tests
the extracted archives. Package smoke tests compile the bundled sample and verify
GUI startup/shutdown. They do not verify PS4 execution or every interactive GUI flow.

Before tagging a release, verify GUI cancellation, timeout/close behavior, existing
output preservation, paths containing spaces/trailing separators, Windows scaling
and keyboard navigation on a Windows desktop. Verify representative generated scripts
in the supported PS4 BO3 1.33 environment and record the results in release notes.

The GSCC validator checks magic, section ordering/ranges, function entry offsets,
include strings and import/string references. It is not a VM interpreter, complete
bytecode verifier, or a decompiler. Keep it independent of incomplete deserialization
routines in the inherited bytecode model.

## Archives and provenance

Build a clean tagged checkout and run `scripts/package.ps1`, followed by
`scripts/smoke-test-packages.ps1`. Each archive records source commit and working-tree
state in manifest.json, along with a SHA-256 digest for every payload file.
SHA256SUMS.txt covers the archives. Preserve those files with release assets.
A dirty-tree manifest is useful for development and is not release provenance.
ZIP timestamps are not normalized, so archive byte-for-byte reproducibility is not
promised. The compiler's GSCC output is deterministic for identical inputs and map.

There is no automatic publishing workflow. Binary signing requires a maintainer-owned
certificate or signing service; none is configured or claimed by this update.

## Attribution and permissions

Irony 1.1.0 declares MIT licensing; its upstream notice is included in
THIRD_PARTY_NOTICES.md and runtime archives. Compiler-source and opcode-map origin,
exact upstream revisions and redistribution permissions still require confirmation.
Record confirmed file-level terms or permissions in CREDITS_AND_RIGHTS.md. Do not
apply a blanket license to inherited components without the rights to do so.

## Audit changes and remaining work

The correctness audit cases are regression tests: count/jump bounds, import identity,
macro cycles, builtin signatures/results, conditional scanning, switch labels,
locale independence, reproducible builds, source locations and failed output writes.
CLI tests cover help/version/usage codes, JSON/check mode, preservation and new folders.

The compiler core now has typed script/function/opcode operands rather than dynamic
binding. Some inherited unused APIs, incomplete readers and legacy comments remain;
remove or redesign them only with relevant fixtures. Future editor integration can
use the JSON result, and project manifests/exclusion rules can be added separately.
The source's inherited rights status and PS4 compatibility cannot be resolved by
unit tests or a code refactor.
