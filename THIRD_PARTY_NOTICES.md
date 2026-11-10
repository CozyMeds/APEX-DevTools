# Third-party components

## Irony 1.1.0

- Package: https://www.nuget.org/packages/Irony/1.1.0
- License source: https://github.com/IronyProject/Irony/blob/v1.1.0/LICENSE
- Used by the compiler core; Irony.dll is included in runtime packages.

```text
MIT License

Copyright (c) 2019 Irony Project (https://github.com/IronyProject)

Permission is hereby granted, free of charge, to any person obtaining a copy of this software
and associated documentation files (the "Software"), to deal in the Software without restriction,
including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so,
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or
substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE
AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.```

## Inherited compiler source and opcode database

The compiler and script serialization code are derived from the community projects
listed in CREDITS_AND_RIGHTS.md. Exact upstream revisions, file-level ownership and
redistribution permissions remain unverified. The origin and distribution rights
of apex_ps4_ops.bin also require verification. These entries do not grant rights
to those components or apply Irony's MIT license to them.

The .NET targeting assemblies and test packages are build dependencies and are not
included in the CLI, GUI or bundle runtime ZIPs. Their versions are recorded in
packages.lock.json files.
