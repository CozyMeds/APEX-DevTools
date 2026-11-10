# Credits, licensing and distribution review

The compiler project includes substantial code derived from:

- https://github.com/someguythatmods/ps4-bo3-gsc-injector
- https://github.com/shiversoftdev/t7-compiler

Those repositories credit additional original contributors; review their README files and commit histories.

**Do not assume that public GitHub source is automatically licensed for redistribution.**
At the time this source was reviewed, no explicit redistribution license was found for the compiler source in the supplied archive or the two repository roots. Before publishing source, modified versions, or downloadable binaries, verify all applicable licenses or obtain permission from the relevant rights holders. Review rights for the PS4 opcode database as well.

The APEX GUI code in this project is user-provided. These notices do not grant a license to third-party compiler code.

Publishing clean source does not establish that a compiled executable is malware-free. Build release artifacts from inspected source, scan the artifacts, and avoid disabling security software.

## Component inventory

| Component | Recorded origin | Status |
| --- | --- | --- |
| Compiler and bytecode model (`src/APEX.Compiler.Core/Compiler*`) | Community repositories above | Exact revisions and file-level permissions unverified |
| `apex_ps4_ops.bin` | Supplied PS4 BO3 1.33 opcode map | Origin and redistribution permission unverified |
| GUI and current integration/tests | APEX DevTools project | No project-wide license granted by these notices |
| Irony 1.1.0 | IronyProject NuGet package and upstream v1.1.0 LICENSE | MIT; full upstream notice in THIRD_PARTY_NOTICES.md |

The extraction into APEX.Compiler.Core and the 1.2.0 corrections do not change or
resolve inherited ownership. No upstream permission, signing certificate, malware
scan or PS4 runtime validation is asserted by this update. Runtime packages retain
this inventory and THIRD_PARTY_NOTICES.md.
