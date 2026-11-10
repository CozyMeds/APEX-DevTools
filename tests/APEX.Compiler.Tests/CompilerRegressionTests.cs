using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using ApexCompiler.Core;
using T7CompilerLib;
using T7CompilerLib.OpCodes;
using TreyarchCompiler;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace ApexCompiler.Tests
{
    public sealed class TemporaryDirectory : IDisposable
    {
        public string Path { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "apex-tests-" + Guid.NewGuid().ToString("N"));
        public TemporaryDirectory() { Directory.CreateDirectory(Path); }
        public void Dispose() { Directory.Delete(Path, true); }
    }

    public class CompilerRegressionTests
    {
        private static byte[] Compile(string source)
        {
            var result = Compiler.Compile(false, source);
            Assert.True(string.IsNullOrEmpty(result.Error), result.Error);
            GsccValidator.Validate(result.CompiledScript);
            return result.CompiledScript;
        }

        [Theory]
        [InlineData(254, true)]
        [InlineData(255, true)]
        [InlineData(256, false)]
        [InlineData(257, false)]
        public void LocalCountsCannotWrap(int count, bool succeeds)
        {
            var source = "main(){" + string.Concat(Enumerable.Range(0, count).Select(i => "v" + i + "=1;")) + "}";
            var result = Compiler.Compile(false, source);
            Assert.Equal(succeeds, string.IsNullOrEmpty(result.Error));
            if (succeeds)
            {
                int start = FunctionStart(result.CompiledScript);
                Assert.Equal(count, result.CompiledScript[start + 2]);
            }
            else Assert.Contains("255 locals", result.Error);
        }

        [Theory]
        [InlineData(255, true)]
        [InlineData(256, false)]
        public void IncludeCountsCannotWrap(int count, bool succeeds)
        {
            var result = Compiler.Compile(false, string.Concat(Enumerable.Range(0, count).Select(i => "#include scripts/x" + i + ";\n")) + "main(){return;}");
            Assert.Equal(succeeds, string.IsNullOrEmpty(result.Error));
            if (succeeds)
            {
                GsccValidator.Validate(result.CompiledScript);
                Assert.Equal(count, result.CompiledScript[0x44]);
            }
        }

        [Fact]
        public void OversizedJumpsFailBeforePublishingBytecode()
        {
            var result = Compiler.Compile(false, "main(x){if(x){" + string.Concat(Enumerable.Repeat("wait 0;", 9000)) + "}}");
            Assert.Contains("signed 16-bit", result.Error);
            Assert.Empty(result.CompiledScript);
        }

        [Fact]
        public void NearbyJumpsRemainSupported() => Compile("main(x){if(x){" + string.Concat(Enumerable.Repeat("wait 0;", 100)) + "}}");

        [Fact]
        public void ImportIdentityPreservesNamespaceAndArgumentCount()
        {
            var data = Compile("main(){ namespace_00000001::foo(); namespace_00000000::foo(1); }");
            Assert.Equal(2, BitConverter.ToUInt16(data, 0x3c));
            int first = BitConverter.ToInt32(data, 0x24);
            Assert.Equal(1u, BitConverter.ToUInt32(data, first + 4));
            Assert.Equal(0, data[first + 10]);
            int second = first + 12 + BitConverter.ToUInt16(data, first + 8) * 4;
            Assert.Equal(0u, BitConverter.ToUInt32(data, second + 4));
            Assert.Equal(1, data[second + 10]);
        }

        [Theory]
        [InlineData("#define A = A; main(){x=A;}")]
        [InlineData("#define A = B; #define B = A; main(){x=A;}")]
        public void RecursiveMacrosProduceDiagnostics(string source)
        {
            var result = Compiler.Compile(false, source);
            Assert.Contains("Recursive macro", result.Error);
        }

        [Theory]
        [InlineData("#define A = self.A; main(){x=A;}")]
        [InlineData("#define A = A(); main(){x=A;}")]
        public void FieldAndFunctionLabelsAreNotRecursiveMacroReferences(string source) => Compile(source);

        [Fact]
        public void ReusingAnAcyclicMacroWorks() => Compile("#define A = 1; #define B = A + A; main(){x=B; y=B;}");

        [Theory]
        [InlineData("isdefined()")]
        [InlineData("isdefined(1,2)")]
        [InlineData("gettime(1)")]
        [InlineData("vectorscale(1)")]
        [InlineData("self endon()")]
        [InlineData("self notify()")]
        [InlineData("notify(1)")]
        [InlineData("self isdefined(1)")]
        public void InvalidBuiltinSignaturesFail(string call)
        {
            var result = Compiler.Compile(false, "main(){" + call + ";}");
            Assert.Contains("Builtin", result.Error);
        }

        [Fact]
        public void UnusedBuiltinResultsArePopped()
        {
            var data = Compile("main(){gettime();}");
            int start = FunctionStart(data);
            var metadata = new T7ScriptObject(true).ScriptMetadata;
            Assert.Equal(metadata.GetOpcodeValue(ScriptOpCode.DecTop), BitConverter.ToUInt16(data, start + 4));
        }

        [Fact]
        public void BuiltinReturnExpressionsKeepTheirResult()
        {
            var data = Compile("main(){return gettime();}");
            var metadata = new T7ScriptObject(true).ScriptMetadata;
            Assert.Equal(metadata.GetOpcodeValue(ScriptOpCode.Return), BitConverter.ToUInt16(data, FunctionStart(data) + 4));
        }

        [Theory]
        [InlineData("main(x){switch(x){case 1:}}")]
        [InlineData("main(x){switch(x){case 1: case 2:}}")]
        [InlineData("main(x){switch(x){case 1: case 2: return; default:}}")]
        [InlineData("main(x){switch(x){default: return; case 1:}}")]
        public void EmptyAndGroupedSwitchLabelsCompile(string source) => Compile(source);

        [Fact]
        public void SampleBuildsAreByteForByteReproducible()
        {
            string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Samples", "main.gsc");
            Assert.Equal(Compile(File.ReadAllText(path)), Compile(File.ReadAllText(path)));
        }

        [Fact]
        public void IdentifierHashesDoNotDependOnLocale()
        {
            var previous = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
                var english = Compile("main(){IPrintLnBold(\"hello\");}");
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");
                Assert.Equal(english, Compile("main(){IPrintLnBold(\"hello\");}"));
            }
            finally { CultureInfo.CurrentCulture = previous; }
        }

        [Fact]
        public void SourceDiagnosticsIdentifyTheOriginalFileAndLine()
        {
            using (var directory = new TemporaryDirectory())
            {
                File.WriteAllText(System.IO.Path.Combine(directory.Path, "a.gsc"), "main(){return;}\n");
                string broken = System.IO.Path.Combine(directory.Path, "b.gsc");
                File.WriteAllText(broken, "broken(){\n  x = ;\n}\n");
                var result = CompilationService.CompilePath(directory.Path);
                Assert.False(result.Success);
                var diagnostic = Assert.Single(result.Diagnostics);
                Assert.Equal(broken, diagnostic.File);
                Assert.Equal(2, diagnostic.Line);
                Assert.True(diagnostic.Column > 0);
            }
        }

        [Fact]
        public void UnsupportedOpcodesCannotBeSerializedAsFFFF()
        {
            var script = new T7ScriptObject(true);
            Assert.Throws<InvalidOperationException>(() => script.ScriptMetadata.GetOpcodeValue(ScriptOpCode.VoidCodePos));
        }

        internal static int FunctionStart(byte[] data) => BitConverter.ToInt32(data, BitConverter.ToInt32(data, 0x20) + 4);
    }

    public class PreprocessorTests
    {
        [Theory]
        [InlineData("#ifndef NOT_DEFINED\nmain(){return;}\n#endif")]
        [InlineData("#ifdef BO3\n#ifndef MISSING_TOKEN\nmain(){return;}\n#endif\n#endif")]
        [InlineData("main(){x=\"path\\\\\";}\n#ifdef BO3\nextra(){return;}\n#endif")]
        [InlineData("/* #ifdef NEVER */\nmain(){return;}// #endif\n")]
        [InlineData("/# #ifdef NEVER #/\nmain(){return;}\n")]
        public void ValidConditionalsAndEscapesCompile(string source)
        {
            using (var directory = new TemporaryDirectory())
            {
                string path = System.IO.Path.Combine(directory.Path, "main.gsc");
                File.WriteAllText(path, source);
                var result = CompilationService.CompilePath(path);
                Assert.True(result.Success, string.Join("\n", result.Diagnostics));
            }
        }

        [Theory]
        [InlineData("#ifdef BO3\n#else\n#else\n#endif")]
        [InlineData("#ifdef BO3\n")]
        [InlineData("#endif\n")]
        [InlineData("#ifdef\nmain(){}\n#endif")]
        public void InvalidConditionalsFailWithLocations(string source)
        {
            using (var directory = new TemporaryDirectory())
            {
                string path = System.IO.Path.Combine(directory.Path, "main.gsc");
                File.WriteAllText(path, source);
                var result = CompilationService.CompilePath(path);
                Assert.False(result.Success);
                var error = Assert.Single(result.Diagnostics);
                Assert.Equal("APEX002", error.Code);
                Assert.Equal(path, error.File);
                Assert.True(error.Line > 0);
            }
        }

        [Fact]
        public void ParserStateDoesNotLeakBetweenInvocations()
        {
            var blocks = new ConditionalBlocks();
            blocks.LoadConditionalTokens(new List<string> { "BO3" });
            blocks.ParseSource("// comment");
            string next = blocks.ParseSource("#ifdef BO3\nhello\n#endif");
            Assert.Contains("hello", next);
            Assert.DoesNotContain("#ifdef", next);
            Assert.Equal("/", blocks.ParseSource("/"));
        }
    }

    public class OutputTests
    {
        private static byte[] ValidScript() => Compiler.Compile(false, "main(){return;}").CompiledScript;

        [Fact]
        public void FailedWritesPreserveThePreviousBuildAndCleanTemporaryFiles()
        {
            using (var directory = new TemporaryDirectory())
            {
                string output = System.IO.Path.Combine(directory.Path, "build.gscc");
                byte[] previous = { 1, 2, 3, 4 };
                File.WriteAllBytes(output, previous);
                Assert.Throws<IOException>(() => AtomicOutput.WriteAllBytes(output, ValidScript(), (stream, bytes) =>
                {
                    stream.WriteByte(bytes[0]);
                    throw new IOException("Injected write failure.");
                }));
                Assert.Equal(previous, File.ReadAllBytes(output));
                Assert.Single(Directory.GetFiles(directory.Path));
            }
        }

        [Fact]
        public void SuccessfulWritesReplaceExistingBuilds()
        {
            using (var directory = new TemporaryDirectory())
            {
                string path = System.IO.Path.Combine(directory.Path, "build.gscc");
                File.WriteAllText(path, "old");
                var expected = ValidScript();
                AtomicOutput.WriteAllBytes(path, expected);
                Assert.Equal(expected, File.ReadAllBytes(path));
                Assert.Single(Directory.GetFiles(directory.Path));
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(0x14)]
        [InlineData(0x20)]
        [InlineData(0x24)]
        [InlineData(0x34)]
        [InlineData(0x3a)]
        public void InvalidHeadersAndSectionRangesAreRejected(int offset)
        {
            byte[] data = ValidScript();
            data[offset] = 0xff;
            data[offset + 1] = 0xff;
            data[offset + 2] = 0xff;
            data[offset + 3] = 0xff;
            Assert.Throws<InvalidDataException>(() => GsccValidator.Validate(data));
        }

        [Fact]
        public void InvalidFunctionOffsetsAreRejected()
        {
            byte[] data = ValidScript();
            int export = BitConverter.ToInt32(data, 0x20);
            Array.Copy(BitConverter.GetBytes(uint.MaxValue), 0, data, export + 4, 4);
            Assert.Throws<InvalidDataException>(() => GsccValidator.Validate(data));
        }

        [Theory]
        [InlineData("C:\\", "\"C:\\\\\"")]
        [InlineData("C:\\my source\\", "\"C:\\my source\\\\\"")]
        [InlineData("", "\"\"")]
        public void WindowsArgumentsPreserveTrailingBackslashes(string value, string expected) => Assert.Equal(expected, WindowsArguments.Quote(value));
    }
}
